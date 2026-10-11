using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuildingProductionRunTests
{
    private static string Id(int n)=>new Guid(n,0,0,new byte[8]).ToString("D");
    private static NameValueCollection Query(int count=2)=>new() {
        {"session",Id(100)},{"runId",Id(101)},{"duration","2"},{"unit","hours"},
        {"speed","3"},{"expectedSpeed","0"},{"maxRealSeconds","60"},
        {"buildingIds",string.Join(",",Enumerable.Range(1,count).Select(Id))} };
    private static SimulationRunRequest Parse(NameValueCollection q)=>SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for",q);
    private static BuildingProductionSample Sample(int count=2,int proven=0,double hours=24,string? failure=null)=>new() {
        Failure=failure,JournalCursor=10,
        Buildings=Enumerable.Range(1,count).Select(n=>new BuildingProductionState {
            Id=Id(n),Template="Bakery.Folktails",Recipe="Bread",ObservedGameHours=hours,
            CompletedCycles=n<=proven?1:0,ProvenGameHours=n<=proven?24.1:null
        }).ToArray() };

    [Fact]
    public void EightBuildingsRequireEightCompletionsAndFreezeReceipt()
    {
        var controller=new SimulationRunController();var commands=new List<int>();
        var run=controller.Start(Parse(Query(8)),24,0,0,commands.Add,buildingProduction:Sample(8));
        controller.Tick(24.1,3,1,commands.Add,buildingProduction:Sample(8,7,24.1));
        Assert.Equal("running",run.State);Assert.False(run.BuildingProductionReached);
        var complete=Sample(8,8,24.2);
        controller.Tick(24.2,3,2,commands.Add,buildingProduction:complete);
        Assert.Equal("pausing",run.State);Assert.True(run.BuildingProductionReached);
        complete.Buildings[0].CompletedCycles=0;
        controller.Tick(24.2,0,3,commands.Add,buildingProduction:Sample(8));
        Assert.Equal("completed",run.State);Assert.True(run.PauseConfirmed);
        Assert.Equal("building_production_proven",run.Reason);
        Assert.All(run.BuildingProduction,b=>Assert.Equal(1,b.CompletedCycles));
        Assert.Equal(new[]{3,0},commands);
    }

    [Theory]
    [InlineData("goodTargets","Bread:1")]
    [InlineData("stopAtScience","10")]
    [InlineData("stopAtAvailableLogs","10")]
    public void BuildingProofCannotBeMixedWithInventoryGoals(string key,string value)
    {
        var q=Query();q[key]=value;Assert.Throws<ArgumentException>(()=>Parse(q));
    }

    [Fact]
    public void RejectInvalidSelectionBeforeStarting()
    {
        Assert.Throws<ArgumentException>(()=>Parse(Query(9)));
        foreach(var ids in new[]{"",Id(1)+","+Id(1),Guid.Empty.ToString("D"),"invalid"}) {
            var q=Query();q["buildingIds"]=ids;Assert.Throws<ArgumentException>(()=>Parse(q));
        }
        Assert.Equal(8,Parse(Query(8)).BuildingIds.Length);
    }

    [Fact]
    public void InitialProofAndMissingObservationNeverBecomeSuccess()
    {
        var c=new SimulationRunController();var commands=new List<int>();
        Assert.Throws<BridgeRejectionException>(()=>c.Start(Parse(Query()),24,0,0,commands.Add,buildingProduction:Sample(2,1)));
        Assert.Empty(commands);Assert.False(c.HasActiveRun);
        var r=c.Start(Parse(Query()),24,0,0,commands.Add,buildingProduction:Sample());
        c.Tick(24.1,3,1,commands.Add);
        c.Tick(24.1,0,2,commands.Add);
        Assert.Equal("failed",r.State);Assert.False(r.BuildingProductionReached);
        Assert.Equal("building_proof_observation_unavailable",r.Reason);
    }

    [Theory]
    [InlineData("building_configuration_changed")]
    [InlineData("building_unavailable")]
    [InlineData("building_proof_journal_gap")]
    public void InvalidatedRunPausesAndRetainsPartialEvidence(string failure)
    {
        var c=new SimulationRunController();var r=c.Start(Parse(Query()),24,0,0,_=>{},buildingProduction:Sample());
        c.Tick(24.1,3,1,_=>{},buildingProduction:Sample(2,1,24.1,failure));
        c.Tick(24.1,0,2,_=>{});
        Assert.Equal("failed",r.State);Assert.Equal(failure,r.Reason);Assert.True(r.PauseConfirmed);
        Assert.False(r.BuildingProductionReached);Assert.Equal(1,r.BuildingProduction[0].CompletedCycles);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TimeBudgetDoesNotProveProduction(bool locked)
    {
        var c=new SimulationRunController();var r=c.Start(Parse(Query()),24,0,0,_=>{},buildingProduction:Sample());
        c.Tick(26,locked?0:3,2,_=>{},speedLocked:locked,buildingProduction:Sample(2,0,26));
        if(locked)c.Tick(26,0,3,_=>{});
        c.Tick(26,0,4,_=>{});
        Assert.True(r.Terminal);Assert.True(r.TargetReached);Assert.False(r.BuildingProductionReached);
        Assert.Equal("building_production_time_limit",r.Reason);
    }

    [Fact]
    public void ProofCannotRegressAndBlockerHistoryIsRetained()
    {
        var state=new SimulationRunStatus{BuildingIds=new[]{Id(1),Id(2)},StartGameHours=24,ObservedGameHours=24.1};
        var first=Sample(2,1,24.1);first.Buildings[1].Blockers=new[]{"ingredients_not_ready"};
        first.Buildings[1].SeenBlockers=new[]{"ingredients_not_ready"};
        Assert.Null(BuildingProductionProof.Observe(state,first));
        Assert.Equal("building_proof_observation_unavailable",BuildingProductionProof.Observe(state,Sample(2,0,24.1)));
        Assert.Equal(1,state.BuildingProduction[0].CompletedCycles);
        var invalid=Sample();invalid.Buildings[0]=null!;
        Assert.Equal("building_proof_observation_unavailable",BuildingProductionProof.Observe(state,invalid));
    }

    [Fact]
    public void NativeContractRejectsForgedSuccessAndWrongSelection()
    {
        var request=Parse(Query());var c=new SimulationRunController();
        var run=c.Start(request,24,0,0,_=>{},buildingProduction:Sample());
        var json=System.Text.Json.JsonSerializer.Serialize(run,Timberborn.Backend.Native.NativeJson.Options);
        var receipt=System.Text.Json.JsonSerializer.Deserialize<Timberborn.Backend.Native.NativeSimulationRun>(json,Timberborn.Backend.Native.NativeJson.Options)!;
        Timberborn.Backend.Native.BuildingProductionRunContract.Validate(receipt,request,"0.41.0");
        Assert.Throws<InvalidDataException>(()=>Timberborn.Backend.Native.BuildingProductionRunContract.Validate(
            receipt with {BuildingProductionReached=true,Reason="building_production_proven"},request,"0.41.0"));
        Assert.Throws<InvalidDataException>(()=>Timberborn.Backend.Native.BuildingProductionRunContract.Validate(
            receipt with {BuildingIds=new[]{Id(3),Id(4)}},request,"0.41.0"));
        Assert.Throws<InvalidDataException>(()=>Timberborn.Backend.Native.BuildingProductionRunContract.Validate(
            receipt with {BuildingProduction=null},request,"0.41.0"));
    }

    [Fact]
    public void InternalJournalWindowCoversMoreThanPublicPageAndReportsLoss()
    {
        var buffer=new RecentActionBuffer();
        for(int i=0;i<100;i++)buffer.Add(DateTimeOffset.UnixEpoch,24,"paused","unknown","state_difference");
        var page=buffer.Window(0);
        Assert.Equal(100,page.Items.Length);Assert.False(page.HasMore);Assert.False(page.Gap);
        for(int i=0;i<513;i++)buffer.Add(DateTimeOffset.UnixEpoch,25,"paused","unknown","state_difference");
        Assert.True(buffer.Window(page.NextCursor).Gap);
    }

    [Fact]
    public void PublicSchemaExposesOptionalEightBuildingSelectionOnlyOnFor()
    {
        var catalog=Timberborn.McpServer.SimulationRunTools.Catalog(true).ToArray();
        var schema=catalog.Single(t=>t.Name=="run_simulation_for").InputSchema;
        Assert.Equal(8,schema.GetProperty("properties").GetProperty("buildingIds").GetProperty("maxItems").GetInt32());
        Assert.DoesNotContain(schema.GetProperty("required").EnumerateArray(),v=>v.GetString()=="buildingIds");
        Assert.False(catalog.Single(t=>t.Name=="run_simulation_until").InputSchema.GetProperty("properties").TryGetProperty("buildingIds",out _));
    }

    [Fact]
    public async Task StructuredBuildingSelectionReachesNativeTransport()
    {
        var request=Parse(Query());
        var c=new SimulationRunController();var run=c.Start(request,24,0,0,_=>{},buildingProduction:Sample());
        var options=Timberborn.Backend.Native.NativeJson.Options;
        var receipt=System.Text.Json.JsonSerializer.Deserialize<Timberborn.Backend.Native.NativeSimulationRun>(
            System.Text.Json.JsonSerializer.Serialize(run,options),options)!;
        var payload=System.Text.Json.JsonSerializer.Serialize(new Timberborn.Backend.Native.BridgeEnvelope<Timberborn.Backend.Native.NativeSimulationRun>(
            1,request.Session,DateTimeOffset.UnixEpoch,"0.41.0",receipt),options);
        using var handler=new Handler(payload);
        using var client=new Timberborn.Backend.Native.NativeClient(new Timberborn.Backend.Native.NativeConfiguration(8081,new string('a',64)),handler);
        var args=System.Text.Json.JsonSerializer.SerializeToElement(new {
            session=request.Session,runId=request.RunId,duration=2,unit="hours",speed=3,expectedSpeed=0,maxRealSeconds=60,buildingIds=request.BuildingIds });
        var result=await Timberborn.McpServer.SimulationRunTools.Invoke(client,"run_simulation_for",args,true,TestContext.Current.CancellationToken);
        Assert.False(result["data"]!["buildingProductionReached"]!.GetValue<bool>());
        Assert.Contains("buildingIds=",handler.Uri!.Query);Assert.Equal(HttpMethod.Post,handler.Method);
    }
    private sealed class Handler(string payload):HttpMessageHandler
    {
        public Uri? Uri;public HttpMethod? Method;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {Uri=request.RequestUri;Method=request.Method;return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(payload)});}
    }
}
