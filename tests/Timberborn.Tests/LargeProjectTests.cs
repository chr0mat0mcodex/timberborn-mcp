using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class LargeProjectTests
{
    private const string Session="11111111-1111-1111-1111-111111111111";
    private const string District="22222222-2222-2222-2222-222222222222";
    private const string Action="33333333-3333-3333-3333-333333333333";
    private static LargeProjectStep Step(int x,int z=0,params int[] deps)=>new(){Template="Path",X=x,Y=1,Z=z,DependsOn=deps};
    private static LargeProjectRequest Request(string operation,LargeProjectStep[]? steps=null)
    {
        var q=new NameValueCollection{{"session",Session}};
        if(operation is "plan" or "start") {q.Add("districtId",District);q.Add("steps",LargeProjectRequest.Encode(steps??[Step(0)]));}
        if(operation!="plan")q.Add("actionId",Action);
        if(operation=="start") {q.Add("planKey",new string('a',64));q.Add("mode","development_pilot");}
        return LargeProjectRequest.Parse("/agent-api/v1/large-project-"+operation,q);
    }
    [Fact]
    public void MaximumBlueprintRoundTripsWithEightLevelsAndStableDependencyOrder()
    {
        var steps=Enumerable.Range(0,32).Select(i=>Step(i,i%8,i==0?[]:[i-1])).ToArray();
        var decoded=LargeProjectRequest.Decode(LargeProjectRequest.Encode(steps));
        Assert.Equal(32,decoded.Length);
        Assert.Equal(Enumerable.Range(0,32),LargeProjectRequest.Order(decoded));
        Assert.Equal(7,decoded.Max(s=>s.Z));
        Assert.Equal("large-project",BridgeRequest.Parse("/agent-api/v1/large-project-plan",new(){{"session",Session},{"districtId",District},{"steps",LargeProjectRequest.Encode(steps)}}).Route);
    }
    [Fact]
    public void LimitsAndInvalidDependenciesAreRejectedBeforeGameAccess()
    {
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate(Enumerable.Range(0,33).Select(i=>Step(i)).ToArray()));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate([Step(0),Step(1,8)]));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate([Step(0),Step(32)]));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate([Step(0),Step(0)]));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate([Step(0,0,1),Step(1,0,0)]));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate([Step(0,0,2),Step(1)]));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate([Step(0,0,0)]));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Validate([Step(0),Step(1,0,0,0)]));
        Assert.Equal(new[]{1,0},LargeProjectRequest.Order([Step(0,0,1),Step(1)]));
    }
    [Fact]
    public void ParserDoesNotAcceptExtraDuplicateOrImplicitSteps()
    {
        var args=JsonSerializer.SerializeToElement(new{session=Session,districtId=District,steps=new[]{new{template="Path",x=0,y=1,z=0,rotation=0,dependsOn=Array.Empty<int>()}}});
        Assert.Single(LargeProjectTools.Parse("plan_large_project",args).Steps);
        Assert.Throws<ArgumentException>(()=>LargeProjectTools.Parse("plan_large_project",JsonSerializer.Deserialize<JsonElement>(args.GetRawText().Replace("\"rotation\":0","\"rotation\":0,\"rotation\":1"))));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-inspect",new(){{"session",Session},{"session",Session},{"actionId",Action}}));
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-inspect",new(){{"session",Session},{"actionId",Action},{"execute","true"}}));
    }
    private static LargeProjectLedger Ledger(int count=2)
    {
        var ledger=new LargeProjectLedger();
        ledger.Start(Action,new string('a',64),"plan",Enumerable.Range(0,count).ToArray(),Enumerable.Range(1,count).Select(i=>new Guid(i,0,0,new byte[8]).ToString("D")).ToArray());
        return ledger;
    }
    [Fact]
    public void SuccessorWaitsForActualFinishAndEachAdvancePlacesAtMostOne()
    {
        var ledger=Ledger();var placed=new List<int>();string observation="construction";
        LargeProjectLedger.Receipt Advance(double time)=>ledger.Advance(Action,time,()=>true,_=>observation,_=>true,placed.Add);
        Assert.Equal("ready",ledger.Inspect(Action).State);
        Advance(0);Assert.Equal(new[]{0},placed);
        Advance(1);Advance(100);Assert.Equal(new[]{0},placed);
        Assert.Equal("construction",ledger.Inspect(Action).PartStates[0]);
        observation="finished";Advance(101);Assert.Equal(new[]{0,1},placed);
        Assert.NotEqual("completed",ledger.Inspect(Action).State);
        Assert.Equal("completed",Advance(102).State);
        Advance(103);Assert.Equal(2,placed.Count);
    }
    [Fact]
    public void PlacementExceptionIsNeverReplayedAndBlocksNewProjects()
    {
        var ledger=Ledger();int calls=0;
        void Place(int _) {calls++;throw new InvalidOperationException();}
        Assert.Equal("unconfirmed",ledger.Advance(Action,0,()=>true,_=>"missing",_=>true,Place).State);
        ledger.Advance(Action,10,()=>true,_=>"finished",_=>true,Place);
        Assert.Equal(1,calls);
        Assert.Throws<BridgeRejectionException>(()=>ledger.Start(District,new string('a',64),"other",[0],[Session]));
    }
    [Theory]
    [InlineData("missing","unconfirmed")]
    [InlineData("pending","unconfirmed")]
    [InlineData("mismatch","stopped")]
    public void UnconfirmedOrChangedObjectCannotAdvance(string observed,string expected)
    {
        var ledger=Ledger();int count=0;
        ledger.Advance(Action,0,()=>true,_=>observed,_=>true,_=>count++);
        Assert.Equal(expected,ledger.Advance(Action,6,()=>true,_=>observed,_=>true,_=>count++).State);
        Assert.Equal(1,count);
    }
    [Theory]
    [InlineData(false,true)] [InlineData(true,false)]
    public void GuardAndFreshValidationStopBeforePlacement(bool guard,bool valid)
    {
        var ledger=Ledger();int count=0;
        Assert.Equal("stopped",ledger.Advance(Action,0,()=>guard,_=>"finished",_=>valid,_=>count++).State);
        Assert.Equal(0,count);
    }
    [Fact]
    public void StopAndDuplicateIdentityNeverCreateOrders()
    {
        var ledger=Ledger();
        Assert.NotNull(ledger.Existing(Action,"plan"));
        Assert.Throws<BridgeRejectionException>(()=>ledger.Existing(Action,"changed"));
        ledger.Stop(Action);
        int count=0;ledger.Advance(Action,0,()=>true,_=>"finished",_=>true,_=>count++);
        Assert.Equal(0,count);Assert.Equal("stopped",ledger.Inspect(Action).State);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void NewToolsUseIndependentBuildingGateAndCorrectMethods(bool enabled)
    {
        var tools=LargeProjectTools.Catalog(enabled).ToArray();
        Assert.Equal(enabled?6:2,tools.Length);
        foreach(var name in new[]{"plan","start","advance","stop"}) {
            string path="/agent-api/v1/large-project-"+name;
            Assert.Equal(enabled,BridgeHttpServer.MethodAllowed("POST",path,false,false,enableBuildingPlacement:enabled));
            Assert.False(BridgeHttpServer.MethodAllowed("GET",path,false,true,true,true,enableBuildingPlacement:enabled));
            Assert.False(BridgeHttpServer.MethodAllowed("POST",path,true,false,enableBuildingPlacement:enabled));
        }
        Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/large-project-inspect",false,false));
        Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/power-network",false,false));
        Assert.All(tools,t=>Assert.Equal(!LargeProjectTools.Writes(t.Name),t.Annotations!.ReadOnlyHint));
    }
    [Fact]
    public void CapabilityProfileDoesNotInventLiveEvidence()
    {
        var mode=Assert.Single(BuildingCapabilityTools.Describe("0.37.0","Folktails",true).Modes,m=>m.Tool=="start_large_project");
        Assert.Equal(32,mode.MaxSteps);Assert.Equal(8,mode.MaxOccupiedHeightSpan);Assert.Empty(mode.LiveRotations);
        Assert.Equal("not_live_proven",mode.EvidenceSource);
        Assert.DoesNotContain(BuildingCapabilityTools.Describe("0.36.0","Folktails",true).Modes,m=>m.Tool=="start_large_project");
        Assert.Equal("unknown",BuildingCapabilityTools.Describe("0.38.0","Folktails",true).ProfileState);
    }
    [Fact]
    public void ResponseContractRejectsPrematureCompletionWrongSessionAndDuplicateEntities()
    {
        var request=Request("inspect");var receipt=Ledger().Inspect(Action);
        BridgeEnvelope<JsonElement> Envelope()=>new(1,Session,DateTimeOffset.UtcNow,"0.37.0",JsonSerializer.SerializeToElement(new{receipt.ActionId,receipt.PlanKey,receipt.State,receipt.Reason,receipt.Current,receipt.Order,receipt.EntityIds,receipt.PartStates,receipt.RegularExecutionAllowed,limitations=new[]{"synthetic"}},NativeJson.Options));
        NativeClient.ValidateLargeProject(Envelope(),request);
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope() with{SessionId=District},request));
        receipt.State="completed";Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),request));
        receipt.State="ready";receipt.EntityIds[1]=receipt.EntityIds[0];
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),request));
    }
    [Fact]
    public async Task PowerReaderUsesAuthenticatedReadOnlyRouteAndValidatesRealMembership()
    {
        var handler=new PowerHandler();
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var request=LargeProjectRequest.Parse("/agent-api/v1/power-network",new(){{"session",Session},{"id",District}});
        var result=await client.LargeProject(request,TestContext.Current.CancellationToken);
        Assert.True(result.Data.GetProperty("supported").GetBoolean());
        handler.ForeignMembership=true;
        await Assert.ThrowsAsync<InvalidDataException>(()=>client.LargeProject(request,TestContext.Current.CancellationToken));
    }
    private sealed class PowerHandler:HttpMessageHandler
    {
        public bool ForeignMembership;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Assert.Equal(HttpMethod.Get,request.Method);
            Assert.Equal("/agent-api/v1/power-network",request.RequestUri!.AbsolutePath);
            Assert.Equal("?session="+Session+"&id="+District,request.RequestUri.Query);
            Assert.Equal("Bearer",request.Headers.Authorization!.Scheme);
            var data=new{id=District,template="Synthetic",position=new{x=1,y=2,z=3},finished=true,supported=true,ports=Array.Empty<object>(),active=true,powered=true,actualInput=10,actualOutput=0,
                network=new{anchorId=District,members=new[]{ForeignMembership?Action:District},powerSupply=20,powerDemand=10,powerSurplus=10,powered=true,batteryCharge=0,batteryCapacity=0},limitations=new[]{"synthetic"}};
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new BridgeEnvelope<object>(1,Session,DateTimeOffset.UtcNow,"0.37.0",data),NativeJson.Options))});
        }
    }
}
