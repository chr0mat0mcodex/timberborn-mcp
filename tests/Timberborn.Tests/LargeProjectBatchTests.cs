using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class LargeProjectBatchTests
{
    private const string Session = "11111111-1111-1111-1111-111111111111";
    private const string ActionId = "22222222-2222-2222-2222-222222222222";
    private static NameValueCollection Arguments(int count = 3) => new() {
        {"session",Session},{"districtId","33333333-3333-3333-3333-333333333333"},{"actionId",ActionId},
        {"planKey",new string('a',64)},{"mode","development_pilot"},{"speed","7"},{"maxGameHours","24"},{"maxRealSeconds","30"},
        {"steps",LargeProjectRequest.Encode(Enumerable.Range(0,count).Select(i=>new LargeProjectStep {Template="Path",X=i,Y=1}).ToArray())} };
    private sealed class Pilot
    {
        public readonly LargeProjectBatch Batch;
        public readonly SimulationRunController Runs = new();
        public readonly List<int> Orders = new();
        public readonly List<int> SpeedCommands = new();
        public readonly List<float> SpeedsAtOrder = new();
        public readonly string[] Observations;
        public readonly LargeProjectRequest Request;
        public bool Guard = true;
        public int InvalidPart = -1, ThrowPart = -1;
        public float Speed;
        private double now, hours;
        private bool locked;
        public LargeProjectBatch.Receipt Receipt => Batch.Inspect();
        public Pilot(int count = 3, bool dependency = false)
        {
            var args=Arguments(count);
            if(dependency) {var steps=LargeProjectRequest.Decode(args["steps"]!);steps[1].DependsOn=[0];args["steps"]=LargeProjectRequest.Encode(steps);}
            Request=LargeProjectRequest.Parse("/agent-api/v1/large-project-run",args);
            Batch=new(Request,"synthetic",Enumerable.Range(1,count).Select(i=>new Guid(i,0,0,new byte[8]).ToString("D")).ToArray());
            Observations=Enumerable.Repeat("construction",count).ToArray();
            Runs.Start(SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for",new() {
                {"session",Session},{"runId",ActionId},{"duration","24"},{"unit","hours"},{"speed","7"},{"expectedSpeed","0"},{"maxRealSeconds","30"} }),0,0,0,Change,deferSpeedStart:true);
        }
        private void Change(int speed) {SpeedCommands.Add(speed);Speed=speed;}
        public void Tick(double time, double gameHours=0, bool isLocked=false)
        {
            now=time;hours=gameHours;locked=isLocked;
            Runs.Tick(hours,Speed,now,Change,locked);
            Batch.Tick(now,Runs.Inspect(ActionId),locked,()=>Guard,i=>Observations[i],i=>i!=InvalidPart,
                i=> {Orders.Add(i);SpeedsAtOrder.Add(Speed);if(i==ThrowPart)throw new InvalidOperationException();},
                ()=>Runs.Cancel(ActionId,hours,Speed,now,Change,locked),()=>Runs.BeginDeferred(ActionId,now,Change));
        }
    }
    [Fact]
    public void ThirtyTwoOrdersThenOneCompletionWithoutPerPartAdvance()
    {
        var p=new Pilot(32);p.Tick(0.25);
        Assert.Equal(Enumerable.Range(0,32),p.Orders);
        Assert.All(p.SpeedsAtOrder,s=>Assert.Equal(0,s));
        Assert.Equal(32,p.Receipt.Ordered);Assert.Equal(0,p.Receipt.Finished);
        p.Tick(1,1);Assert.Equal(32,p.Orders.Count);
        Array.Fill(p.Observations,"finished");p.Tick(2,2);
        Assert.Equal("pausing",p.Receipt.State);
        p.Tick(2.25,2);Assert.Equal("completed",p.Receipt.State);
        Assert.True(p.Receipt.Simulation.PauseConfirmed);Assert.Equal(new[]{7,0},p.SpeedCommands);
        p.Tick(3,2);Assert.Equal(32,p.Orders.Count);
    }
    [Fact]
    public void IndependentPartsStartTogetherAndSupportSuccessorDispatchesOnFinish()
    {
        var p=new Pilot(dependency:true);p.Tick(0.25);
        Assert.Equal(new[]{0,2},p.Orders);
        Assert.Equal(new[]{1},p.Receipt.DeferredParts);
        p.Tick(1);Assert.Equal(new[]{0,2},p.Orders);
        p.Observations[0]="finished";p.Tick(1.25);
        Assert.Equal(new[]{0,2,1},p.Orders);
        Assert.Empty(p.Receipt.DeferredParts);
        Assert.NotEqual("completed",p.Receipt.State);
    }
    [Fact]
    public void ReadOnlyInspectionCannotDispatchAndPartFailureCannotReplay()
    {
        var p=new Pilot {ThrowPart=1};
        for(int i=0;i<10;i++)Assert.Equal(0,p.Batch.Inspect().Ordered);
        p.Tick(0.25);Assert.Equal(new[]{0,1},p.Orders);
        p.Tick(1);Assert.Equal("unconfirmed",p.Receipt.State);Assert.Equal(1,p.Receipt.BlockedPart);
        Array.Fill(p.Observations,"finished");p.Tick(2);
        Assert.Equal(new[]{0,1},p.Orders);Assert.Equal("pending",p.Receipt.PartStates[2]);
    }
    [Theory]
    [InlineData(false,-1,"guard_changed",0)]
    [InlineData(true,1,"native_step_validation_failed",1)]
    public void FailedGuardOrPreflightStopsRemainingOrders(bool guard,int invalid,string reason,int count)
    {
        var p=new Pilot {Guard=guard,InvalidPart=invalid};p.Tick(0.25);p.Tick(1);
        Assert.Equal("blocked",p.Receipt.State);Assert.Equal(reason,p.Receipt.Reason);Assert.Equal(count,p.Orders.Count);
        Assert.True(p.Receipt.Simulation.PauseConfirmed);
    }
    [Theory]
    [InlineData("missing","unconfirmed")]
    [InlineData("pending","unconfirmed")]
    [InlineData("mismatch","blocked")]
    public void ChangedObjectStopsBeforeDependentPlacement(string observed,string state)
    {
        var p=new Pilot(dependency:true);p.Tick(0.25);p.Observations[0]=observed;p.Tick(6);p.Tick(6.25);
        Assert.Equal(state,p.Receipt.State);Assert.Equal(0,p.Receipt.BlockedPart);Assert.DoesNotContain(1,p.Orders);
    }
    [Fact]
    public void StopDuringConstructionPausesOnceAndDoesNotFinishDependentPart()
    {
        var p=new Pilot(dependency:true);p.Tick(0.25);p.Batch.Stop();p.Tick(1);p.Tick(1.25);
        Assert.Equal("stopped",p.Receipt.State);Assert.Equal(new[]{7,0},p.SpeedCommands);
        p.Batch.Stop();p.Tick(2);Assert.Equal(2,p.Orders.Count);
    }
    [Fact]
    public void ManualSpeedChangeInterruptsWithoutOverwritingHumanChoice()
    {
        var p=new Pilot(dependency:true);p.Tick(0.25);p.Speed=3;p.Tick(1);
        Assert.Equal("interrupted",p.Receipt.State);Assert.Equal(new[]{7},p.SpeedCommands);Assert.Equal(3,p.Speed);
        Assert.DoesNotContain(1,p.Orders);
    }
    [Fact]
    public void HumanInterruptionDuringFinalPauseCannotProduceCompleted()
    {
        var p=new Pilot();p.Tick(0.25);Array.Fill(p.Observations,"finished");p.Tick(1);
        Assert.Equal("pausing",p.Receipt.State);
        p.Runs.Interrupt();p.Speed=3;p.Tick(1.25);
        Assert.Equal("interrupted",p.Receipt.State);
    }
    [Fact]
    public void InitializationGraceSurvivesBriefGameLock()
    {
        var p=new Pilot(dependency:true);p.Tick(0.25);p.Observations[0]="pending";
        p.Tick(1,0,true);p.Tick(10,0,true);p.Tick(10.25);p.Tick(10.5);
        Assert.Equal("running",p.Receipt.State);Assert.DoesNotContain(1,p.Orders);
        p.Observations[0]="finished";p.Tick(10.75);
        Assert.Contains(1,p.Orders);
    }
    [Fact]
    public void NewWaveWaitsForAllPreviousOrderConfirmationsButNotTheirCompletion()
    {
        var p=new Pilot(dependency:true);p.Tick(0.25);p.Observations[0]="finished";p.Observations[2]="pending";
        p.Tick(1);Assert.DoesNotContain(1,p.Orders);
        p.Observations[2]="construction";p.Tick(1.25);Assert.Contains(1,p.Orders);
    }
    [Fact]
    public void OnlyRecordedBatchObjectsMayHaveUnconfirmedInitialization()
    {
        Assert.True(ConstructionIsolationPolicy.Allows(true,[(false,false,false,true)],allowOwnedInitialization:true));
        Assert.False(ConstructionIsolationPolicy.Allows(true,[(false,false,false,true)]));
        Assert.False(ConstructionIsolationPolicy.Allows(true,[(false,false,false,false)],allowOwnedInitialization:true));
        Assert.False(ConstructionIsolationPolicy.Allows(false,[(false,false,false,true)],allowOwnedInitialization:true));
        Assert.False(ConstructionIsolationPolicy.Allows(true,[(true,true,true,true)],allowOwnedInitialization:true));
    }
    [Fact]
    public void McpParserRequiresNumericFixedBudgetsAndCompletePlan()
    {
        var args=JsonSerializer.SerializeToElement(new {session=Session,districtId="33333333-3333-3333-3333-333333333333",actionId=ActionId,
            planKey=new string('a',64),mode="development_pilot",speed=7,maxGameHours=24,maxRealSeconds=30,
            steps=new[]{new{template="Path",x=0,y=1,z=0,rotation=0,dependsOn=Array.Empty<int>()}}});
        var request=LargeProjectTools.Parse("run_large_project",args);Assert.Equal(24,request.MaxGameHours);
        Assert.Throws<ArgumentException>(()=>LargeProjectTools.Parse("run_large_project",JsonSerializer.Deserialize<JsonElement>(args.GetRawText().Replace("\"speed\":7","\"speed\":\"7\""))));
        var missing=Arguments();missing.Remove("maxRealSeconds");
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-run",missing));
    }
    [Theory]
    [InlineData(31,1,"simulation_real_time_limit")]
    [InlineData(2,24,"game_time_limit")]
    public void TimeBudgetIsBlockedNotCompleted(double time,double hours,string reason)
    {
        var p=new Pilot();p.Tick(0.25);p.Tick(time,hours);p.Tick(time+0.25,hours);
        Assert.Equal("blocked",p.Receipt.State);Assert.Equal(reason,p.Receipt.Reason);Assert.Equal(0,p.Receipt.Finished);
    }
    [Fact]
    public void GameLockDefersOrdersAndPausingButNeverExtendsBudget()
    {
        var p=new Pilot(dependency:true);p.Tick(0.25);p.Observations[0]="finished";
        p.Tick(1,0,true);Assert.DoesNotContain(1,p.Orders);
        p.Tick(31,0,true);Assert.Equal(new[]{7},p.SpeedCommands);
        p.Tick(32);p.Tick(32.25);
        Assert.Equal("blocked",p.Receipt.State);Assert.Equal(new[]{7,0},p.SpeedCommands);
    }
    [Theory]
    [InlineData("speed","2")][InlineData("maxGameHours","0")][InlineData("maxGameHours","673")]
    [InlineData("maxRealSeconds","29")][InlineData("maxRealSeconds","86401")]
    public void InvalidBudgetsRejected(string name,string value)
    {
        var args=Arguments();args[name]=value;
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-run",args));
    }
    [Theory]
    [InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public void BothGatesRequiredAndReadOnlyHintsAreAccurate(bool building,bool speed)
    {
        var tools=LargeProjectTools.Catalog(building,speed).ToArray();
        Assert.Equal(building&&speed,tools.Any(t=>t.Name=="run_large_project"));
        Assert.Equal(building&&speed,BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/large-project-run",false,false,enableSpeedControl:speed,enableBuildingPlacement:building));
        Assert.False(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/large-project-run",false,false,enableSpeedControl:speed,enableBuildingPlacement:building));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/large-project-run",true,false,enableSpeedControl:speed,enableBuildingPlacement:building));
        if(building&&speed)Assert.False(Assert.Single(tools,t=>t.Name=="run_large_project").Annotations!.ReadOnlyHint);
    }
    [Fact]
    public void CapabilityProfileLabelsBatchAsUnproven()
    {
        var mode=Assert.Single(BuildingCapabilityTools.Describe("0.37.0","Folktails",true).Modes,m=>m.Tool=="run_large_project");
        Assert.Equal(32,mode.MaxSteps);Assert.Equal(8,mode.MaxOccupiedHeightSpan);Assert.Equal("not_live_proven",mode.EvidenceSource);
        Assert.DoesNotContain(BuildingCapabilityTools.Describe("0.36.0","Folktails",true).Modes,m=>m.Tool=="run_large_project");
    }
    private static JsonObject Data(Pilot p)
    {
        var d=JsonSerializer.SerializeToNode(p.Receipt,NativeJson.Options)!.AsObject();d["limitations"]=new JsonArray("synthetic");return d;
    }
    private static void Validate(JsonObject d,LargeProjectRequest request) => NativeClient.ValidateLargeProject(new(1,Session,DateTimeOffset.UtcNow,"0.37.0",JsonSerializer.SerializeToElement(d)),request);
    [Fact]
    public void ContractAcceptsConcurrentOrdersAndRejectsFalseCompletionAndCounts()
    {
        var p=new Pilot();p.Tick(0.25);Validate(Data(p),p.Request);
        var d=Data(p);d["state"]="completed";Assert.Throws<InvalidDataException>(()=>Validate(d,p.Request));
        d=Data(p);d["ordered"]=0;Assert.Throws<InvalidDataException>(()=>Validate(d,p.Request));
        d=Data(p);d["entityIds"]![1]=d["entityIds"]![0]!.DeepClone();Assert.Throws<InvalidDataException>(()=>Validate(d,p.Request));
        Array.Fill(p.Observations,"finished");p.Tick(1);p.Tick(1.25);Validate(Data(p),p.Request);
    }
    [Fact]
    public async Task AuthenticatedBatchRouteCarriesBudgetAndDoesNotRetryTransportFailure()
    {
        var p=new Pilot();p.Tick(0.25);var handler=new BatchHandler(Data(p));
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        await client.LargeProject(p.Request,TestContext.Current.CancellationToken);
        Assert.Equal(1,handler.Calls);
        handler.Fail=true;
        await Assert.ThrowsAsync<HttpRequestException>(()=>client.LargeProject(p.Request,TestContext.Current.CancellationToken));
        Assert.Equal(2,handler.Calls);
    }
    private sealed class BatchHandler(JsonObject data):HttpMessageHandler
    {
        public int Calls;public bool Fail;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;
            Assert.Equal(HttpMethod.Post,request.Method);
            Assert.Equal("/agent-api/v1/large-project-run",request.RequestUri!.AbsolutePath);
            Assert.Contains("&speed=7&maxGameHours=24&maxRealSeconds=30",request.RequestUri.Query);
            Assert.Contains("steps=",request.RequestUri.Query);
            Assert.Equal("Bearer",request.Headers.Authorization!.Scheme);
            if(Fail)throw new HttpRequestException("synthetic lost response");
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) {Content=new StringContent(JsonSerializer.Serialize(new BridgeEnvelope<JsonObject>(1,Session,DateTimeOffset.UtcNow,"0.37.0",data),NativeJson.Options))});
        }
    }
}
