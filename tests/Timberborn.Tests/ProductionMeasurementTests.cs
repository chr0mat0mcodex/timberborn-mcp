using System.Collections.Specialized;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ProductionMeasurementTests
{
    private static string Id(int n)=>new Guid(n,0,0,new byte[8]).ToString("D");
    private static NameValueCollection Query(int count=1)=>new() {
        {"session",Id(100)},{"runId",Id(101)},{"duration","1"},{"unit","hours"},
        {"speed","3"},{"expectedSpeed","0"},{"maxRealSeconds","30"},
        {"buildingIds",string.Join(",",Enumerable.Range(1,count).Select(Id))},{"measureProduction","true"} };
    private static SimulationRunRequest Parse(NameValueCollection q)=>SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for",q);
    private static ProductionMeasurementTracker Tracker()=>new(new[]{new MeasuredProduct{Good="Bread",AmountPerCycle=5}},2);
    private static double Time(ProductionMeasurement m,string state)=>m.Times.Single(t=>t.State==state).Hours;
    private static BuildingProductionSample Sample(double hours,long cycles=0,int count=1,string? failure=null)=>new() {
        JournalCursor=1,Failure=failure,
        Buildings=Enumerable.Range(1,count).Select(n=>new BuildingProductionState {
            Id=Id(n),Template="Bakery.Folktails",Recipe="Bread",ObservedGameHours=hours,
            CompletedCycles=cycles>0?1:0,ProvenGameHours=cycles>0?24.25:null,
            Measurement=new ProductionMeasurement {CompletedCycles=cycles,MeasuredHours=hours-24,
                Products=new[]{new MeasuredProduct{Good="Bread",AmountPerCycle=5}},
                Times=ProductionMeasurementTracker.States.Select(s=>new MeasuredTime{State=s,Hours=s=="unknown"?hours-24:0}).ToArray()}
        }).ToArray() };
    private static NativeSimulationRun Wire(SimulationRunStatus s)=>JsonSerializer.Deserialize<NativeSimulationRun>(JsonSerializer.Serialize(s,NativeJson.Options),NativeJson.Options)!;

    [Fact]
    public void CountsMultipleNativeCyclesAndDerivesOutputsWithoutInventories()
    {
        var t=Tracker();var initial=t.Observe(24,0.8,0,[]);
        Assert.Null(initial.CyclesPerHour);
        t.Observe(24.25,0.2,1,[]); // completion resets progress
        var m=t.Observe(24.5,0.1,3,[]); // multiple events between samples
        Assert.Equal(3,m.CompletedCycles);Assert.Equal(6d,m.CyclesPerHour);
        Assert.Equal(15d,m.RecipeDerivedProducts.Single().Amount);
        Assert.Equal(30d,m.RecipeDerivedProducts.Single().PerGameHour);
        Assert.Equal(6d,m.RecipeDerivedScience);Assert.Equal(0.5,Time(m,"producing"));
        Assert.True(ProductionMeasurementTracker.Valid(m,0.5));
        Assert.Equal(0,initial.CompletedCycles);Assert.Equal(0,initial.MeasuredHours);
    }

    [Theory]
    [InlineData("ingredients_not_ready","ingredients")]
    [InlineData("fuel_not_ready","ingredients")]
    [InlineData("output_capacity_unavailable","storage")]
    [InlineData("no_assigned_workers","staffing")]
    [InlineData("building_paused","paused")]
    [InlineData("outside_working_hours","outside_working_hours")]
    [InlineData("consumer_not_powered","power")]
    [InlineData("power_network_unknown","unknown")]
    public void StableObservedBlockerAccruesGameTime(string blocker,string expected)
    {
        var t=Tracker();t.Observe(24,0,0,[blocker]);
        t.Observe(24,0,0,[blocker]); // real wait/pause cannot add game time
        var m=t.Observe(24.25,0,0,[blocker]);
        Assert.Equal(0.25,Time(m,expected));Assert.Equal(expected,m.LargestObservedStall);
        Assert.Equal(0d,m.CyclesPerHour);Assert.Equal(m.MeasuredHours,m.Times.Sum(x=>x.Hours));
    }

    [Fact]
    public void TransitionsLongGapsAndUnexplainedWaitAreUnknownNotInventedCauses()
    {
        var t=Tracker();t.Observe(24,0,0,["ingredients_not_ready"]);
        t.Observe(24.25,0,0,["output_capacity_unavailable"]);
        t.Observe(25,0,0,["output_capacity_unavailable"]);
        var m=t.Observe(25.25,0,0,[]);
        Assert.Equal(1.25,Time(m,"unknown"));Assert.Equal(0,Time(m,"storage"));
        var simultaneous=Tracker();string[] blockers=["no_assigned_workers","ingredients_not_ready"];
        simultaneous.Observe(24,0,0,blockers);m=simultaneous.Observe(24.25,0,0,blockers);
        Assert.Equal(0.25,Time(m,"multiple_blockers"));Assert.Equal(0,Time(m,"staffing"));
    }

    [Fact]
    public void ProgressOverridesReadinessButDoesNotInventCycleOrProduct()
    {
        var t=Tracker();t.Observe(24,0.1,0,["ingredients_not_ready"]);
        var m=t.Observe(24.25,0.3,0,["ingredients_not_ready"]);
        Assert.Equal(0.25,Time(m,"producing"));Assert.Equal(0,m.CompletedCycles);
        Assert.Equal(0,m.RecipeDerivedProducts[0].Amount);
        Assert.Throws<ArgumentException>(()=>t.Observe(23,0,0,[]));
    }

    [Fact]
    public void SameGameTimeRenderUpdatesDoNotEraseProgressOrCompletionBaseline()
    {
        var t=Tracker();t.Observe(24,0.1,0,[]);
        t.Observe(24,0.3,0,[]);
        var progress=t.Observe(24.125,0.3,0,[]);
        Assert.Equal(0.125,Time(progress,"producing"));
        var completion=t.Observe(24.125,0,1,[]);
        Assert.Equal(1,completion.CompletedCycles);Assert.Equal(0.125,completion.MeasuredHours);
        t.Observe(24.125,0,1,[]);
        Assert.Throws<ArgumentException>(()=>t.Observe(24.125,0,0,[]));
        var next=t.Observe(24.25,0,1,[]);
        Assert.Equal(0.25,Time(next,"producing"));Assert.Equal(0,Time(next,"unknown"));
        Assert.Equal(1,next.CompletedCycles);
        Assert.Equal(0.125,completion.MeasuredHours);
    }

    [Fact]
    public void EightBuildingsContinueAfterFirstProofThenFreezeAtWindowEnd()
    {
        var q=Query(8);var r=Parse(q);var c=new SimulationRunController();var commands=new List<int>();
        var run=c.Start(r,24,0,0,commands.Add,buildingProduction:Sample(24,count:8));
        c.Tick(24.25,3,1,commands.Add,buildingProduction:Sample(24.25,1,8));
        Assert.Equal("running",run.State);Assert.True(run.BuildingProductionReached);
        c.Tick(24.5,3,2,commands.Add,buildingProduction:Sample(24.5,3,8));
        var final=Sample(25,5,8);c.Tick(25,3,3,commands.Add,buildingProduction:final);
        Assert.Equal("production_measurement_complete",run.Reason);Assert.Equal("pausing",run.State);
        final.Buildings[0].Measurement!.Products[0].AmountPerCycle=99;
        c.Tick(25,0,4,commands.Add);
        var snapshot=JsonSerializer.Serialize(run.BuildingProduction);
        c.Tick(26,3,5,commands.Add,buildingProduction:Sample(26,8,8));
        Assert.Equal(snapshot,JsonSerializer.Serialize(run.BuildingProduction));
        Assert.True(run.Terminal);Assert.True(run.PauseConfirmed);Assert.Equal(new[]{3,0},commands);
        Assert.All(run.BuildingProduction,b=>{Assert.Equal(1,b.CompletedCycles);Assert.Equal(5,b.Measurement!.CompletedCycles);});
        Assert.Equal(5,run.BuildingProduction[0].Measurement!.Products[0].AmountPerCycle);
        BuildingProductionRunContract.Validate(Wire(run),r,"0.44.0");
    }

    [Fact]
    public void ZeroProductionCompletesMeasurementWithoutClaimingProductionSuccess()
    {
        var c=new SimulationRunController();var r=Parse(Query());
        var run=c.Start(r,24,0,0,_=>{},buildingProduction:Sample(24));
        c.Tick(25,3,1,_=>{},buildingProduction:Sample(25));c.Tick(25,0,2,_=>{});
        Assert.Equal("completed",run.State);Assert.False(run.BuildingProductionReached);
        Assert.Equal("production_measurement_complete",run.Reason);
        BuildingProductionRunContract.Validate(Wire(run),r,"0.44.0");
    }

    [Theory]
    [InlineData("building_configuration_changed")]
    [InlineData("building_unavailable")]
    [InlineData("building_proof_journal_gap")]
    public void ExistingInvalidationStillStopsMeasurementAndPreservesPartialCounts(string failure)
    {
        var c=new SimulationRunController();var run=c.Start(Parse(Query()),24,0,0,_=>{},buildingProduction:Sample(24));
        c.Tick(24.25,3,1,_=>{},buildingProduction:Sample(24.25,2,failure:failure));
        c.Tick(24.25,0,2,_=>{});
        Assert.Equal("failed",run.State);Assert.Equal(failure,run.Reason);Assert.True(run.PauseConfirmed);
        Assert.Equal(2,run.BuildingProduction[0].Measurement!.CompletedCycles);
    }

    [Theory]
    [InlineData("cancel")]
    [InlineData("speed")]
    [InlineData("real_limit")]
    [InlineData("locked")]
    public void LifecycleBoundariesRetainLastMeasuredCoverage(string action)
    {
        var c=new SimulationRunController();var request=Parse(Query());
        var run=c.Start(request,24,0,0,_=>{},buildingProduction:Sample(24));
        c.Tick(24.25,3,1,_=>{},buildingProduction:Sample(24.25,1));
        if(action=="cancel")c.Cancel(request.RunId,24.5,3,2,_=>{});
        else if(action=="speed")c.Tick(24.5,1,2,_=>{},buildingProduction:Sample(24.5,2));
        else if(action=="real_limit")c.Tick(24.5,3,31,_=>{},buildingProduction:Sample(24.5,2));
        else {c.Tick(25,0,2,_=>{},speedLocked:true);c.Tick(25,0,3,_=>{});}
        c.Tick(action=="locked"?25:24.5,0,32,_=>{});
        Assert.True(run.Terminal);
        Assert.Equal(action=="real_limit"?2:1,run.BuildingProduction[0].Measurement!.CompletedCycles);
        Assert.True(run.BuildingProduction[0].Measurement!.MeasuredHours<=run.ElapsedGameHours);
        BuildingProductionRunContract.Validate(Wire(run),request,"0.44.0");
    }

    [Fact]
    public void RejectsInvalidModesAndMissingMeasurementBeforeSpeedChange()
    {
        var q=Query();q.Remove("buildingIds");Assert.Throws<ArgumentException>(()=>Parse(q));
        q=Query();q["measureProduction"]="yes";Assert.Throws<ArgumentException>(()=>Parse(q));
        Assert.Throws<ArgumentException>(()=>Parse(Query(9)));
        q=Query();q["goodTargets"]="Bread:10";Assert.Throws<ArgumentException>(()=>Parse(q));
        var c=new SimulationRunController();var commands=new List<int>();var sample=Sample(24);sample.Buildings[0].Measurement=null;
        Assert.Throws<BridgeRejectionException>(()=>c.Start(Parse(Query()),24,0,0,commands.Add,buildingProduction:sample));Assert.Empty(commands);
    }

    [Fact]
    public void ExplicitFalseRetainsPlainTimeRunAndFinalReconciliationIsBounded()
    {
        var q=Query();q.Remove("buildingIds");q["measureProduction"]="false";
        Assert.False(Parse(q).MeasureProduction);
        var c=new SimulationRunController();c.Start(Parse(Query()),24,0,0,_=>{},buildingProduction:Sample(24));
        Assert.False(c.BuildingObservationNeedsReconcile(24.5,10));
        Assert.True(c.BuildingObservationNeedsReconcile(25,10));
        Assert.True(c.BuildingObservationNeedsReconcile(24.5,30));
        c.Tick(25,3,10,_=>{},buildingProduction:Sample(25));
        Assert.False(c.BuildingObservationNeedsReconcile(25,31));
    }

    [Theory]
    [InlineData("string")]
    [InlineData("number")]
    public async Task WrongBooleanTypeNeverReachesTransport(string kind)
    {
        var q=Query();var r=Parse(q);
        using var handler=new Handler("{}");
        using var client=new NativeClient(new NativeConfiguration(8081,new string('a',64)),handler);
        object bad=kind=="string"?"true":1;
        var args=JsonSerializer.SerializeToElement(new {session=r.Session,runId=r.RunId,duration=1,unit="hours",speed=3,expectedSpeed=0,maxRealSeconds=30,buildingIds=r.BuildingIds,measureProduction=bad});
        await Assert.ThrowsAsync<ArgumentException>(()=>SimulationRunTools.Invoke(client,"run_simulation_for",args,true,TestContext.Current.CancellationToken));
        Assert.Empty(handler.Query);
    }

    [Fact]
    public void RejectsCorruptOrRegressingAggregates()
    {
        var r=Parse(Query());var c=new SimulationRunController();var run=c.Start(r,24,0,0,_=>{},buildingProduction:Sample(24));
        c.Tick(24.25,3,1,_=>{},buildingProduction:Sample(24.25,3));
        var bad=Sample(24.5,2);c.Tick(24.5,3,2,_=>{},buildingProduction:bad);c.Tick(24.5,0,3,_=>{});
        Assert.Equal("building_proof_observation_unavailable",run.Reason);Assert.Equal(3,run.BuildingProduction[0].Measurement!.CompletedCycles);
        var wire=Wire(run);wire.BuildingProduction![0].Measurement!.Times[0].Hours=100;
        Assert.Throws<InvalidDataException>(()=>BuildingProductionRunContract.Validate(wire,r,"0.44.0"));
        Assert.Throws<InvalidDataException>(()=>BuildingProductionRunContract.Validate(wire with {MeasureProduction=false},r,"0.44.0"));
        Assert.Throws<InvalidDataException>(()=>BuildingProductionRunContract.Validate(wire,r,"0.43.0"));
    }

    [Fact]
    public void SchemaAndTransportExposeOptionalBooleanWithoutNewTool()
    {
        var catalog=SimulationRunTools.Catalog(true).ToArray();
        var schema=catalog.Single(t=>t.Name=="run_simulation_for").InputSchema;
        Assert.Equal("boolean",schema.GetProperty("properties").GetProperty("measureProduction").GetProperty("type").GetString());
        Assert.DoesNotContain(schema.GetProperty("required").EnumerateArray(),v=>v.GetString()=="measureProduction");
        Assert.False(catalog.Single(t=>t.Name=="run_simulation_until").InputSchema.GetProperty("properties").TryGetProperty("measureProduction",out _));
        Assert.DoesNotContain(catalog,t=>t.Description!.Contains("Ã"));
    }

    [Fact]
    public async Task MeasurementFlagAndTypedReportRoundTripThroughExistingTool()
    {
        var q=Query();var r=Parse(q);var c=new SimulationRunController();var state=c.Start(r,24,0,0,_=>{},buildingProduction:Sample(24));
        using var handler=new Handler(JsonSerializer.Serialize(new BridgeEnvelope<NativeSimulationRun>(1,r.Session,DateTimeOffset.UnixEpoch,"0.44.0",Wire(state)),NativeJson.Options));
        using var client=new NativeClient(new NativeConfiguration(8081,new string('a',64)),handler);
        var args=JsonSerializer.SerializeToElement(new {session=r.Session,runId=r.RunId,duration=1,unit="hours",speed=3,expectedSpeed=0,maxRealSeconds=30,buildingIds=r.BuildingIds,measureProduction=true});
        var result=await SimulationRunTools.Invoke(client,"run_simulation_for",args,true,TestContext.Current.CancellationToken);
        Assert.Contains("measureProduction=true",handler.Query);Assert.True(result["data"]!["measureProduction"]!.GetValue<bool>());
        Assert.Equal(0,result["data"]!["buildingProduction"]![0]!["measurement"]!["completedCycles"]!.GetValue<long>());
    }
    private sealed class Handler(string payload):HttpMessageHandler
    {
        public string Query="";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Query=request.RequestUri!.Query;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(payload)});
        }
    }
}
