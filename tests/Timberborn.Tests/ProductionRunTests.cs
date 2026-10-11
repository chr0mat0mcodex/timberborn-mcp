using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ProductionRunTests
{
    private static NameValueCollection Query() => new() {
        {"session","11111111-1111-1111-1111-111111111111"},{"runId","22222222-2222-2222-2222-222222222222"},
        {"duration","2"},{"unit","hours"},{"speed","3"},{"expectedSpeed","0"},{"maxRealSeconds","60"},
        {"goodTargets","TreatedPlank:8;Gear:4"},{"stopAtScience","10"} };
    private static SimulationRunRequest Parse(NameValueCollection q) => SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for",q);
    private static ProductionSample Sample(int? plank=8,int? gear=4,int? science=10) => new() {
        Goods=new() {{"TreatedPlank",plank},{"Gear",gear}}, Science=science };

    [Fact]
    public void AllTargetsMustCoincideAndPauseReceiptFreezesSuccessfulSample()
    {
        var c=new SimulationRunController();var commands=new List<int>();
        var r=c.Start(Parse(Query()),24,0,0,commands.Add,production:Sample(plank:0));
        c.Tick(24.1,3,1,commands.Add,production:Sample(gear:0));
        Assert.False(r.ProductionTargetsReached);Assert.Equal("running",r.State);
        c.Tick(24.2,3,2,commands.Add,production:Sample(science:9));
        Assert.False(r.ProductionTargetsReached);
        c.Tick(24.3,3,3,commands.Add,production:Sample());
        Assert.Equal("pausing",r.State);Assert.False(r.Terminal);
        c.Tick(24.3,0,4,commands.Add,production:Sample(0,0,0));
        Assert.True(r.Terminal);Assert.True(r.PauseConfirmed);Assert.True(r.ProductionTargetsReached);
        Assert.Equal("production_targets_reached",r.Reason);Assert.Equal(new[]{3,0},commands);
        Assert.Equal(8,r.GoodTargets.Single(t=>t.Good=="TreatedPlank").Available);
    }

    [Fact]
    public void AlreadyMetScienceOnlyTargetNeverAccelerates()
    {
        var q=Query();q.Remove("goodTargets");var commands=new List<int>();var c=new SimulationRunController();
        var r=c.Start(Parse(q),24,0,0,commands.Add,production:Sample());
        Assert.Equal(new[]{0},commands);Assert.True(r.ProductionTargetsReached);
        c.Tick(24,0,1,commands.Add);
        Assert.Equal("completed",r.State);
    }

    [Fact]
    public void MissingObservationStopsAndUnknownStartDoesNotConsumeId()
    {
        var c=new SimulationRunController();var commands=new List<int>();var request=Parse(Query());
        Assert.Throws<BridgeRejectionException>(()=>c.Start(request,24,0,0,commands.Add,production:Sample(plank:null)));
        Assert.Empty(commands);Assert.False(c.HasActiveRun);
        var r=c.Start(request,24,0,1,commands.Add,production:Sample(plank:0));
        c.Tick(24.1,3,2,commands.Add,production:Sample(gear:null));
        c.Tick(24.1,0,3,commands.Add);
        Assert.Equal("failed",r.State);Assert.Equal("production_observation_unavailable",r.Reason);
        Assert.False(r.ProductionTargetsReached);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BudgetsDoNotBecomeTargetSuccess(bool locked)
    {
        var c=new SimulationRunController();var r=c.Start(Parse(Query()),24,0,0,_=>{},production:Sample(plank:0));
        c.Tick(26,locked?0:3,2,_=>{},speedLocked:locked,production:Sample(plank:0));
        if(locked) c.Tick(26,0,3,_=>{},production:Sample(plank:0));
        c.Tick(26,0,4,_=>{},production:Sample(plank:0));
        Assert.True(r.Terminal);Assert.True(r.TargetReached);Assert.False(r.ProductionTargetsReached);
        Assert.Equal("production_time_limit",r.Reason);
    }

    [Fact]
    public void RealTimeLimitAndCancellationDoNotClaimGoodsWereProduced()
    {
        var c=new SimulationRunController();var r=c.Start(Parse(Query()),24,0,0,_=>{},production:Sample(plank:0));
        c.Tick(24.1,3,60,_=>{},production:Sample(plank:0));
        c.Tick(24.1,0,61,_=>{});
        Assert.Equal("failed",r.State);Assert.Equal("real_time_limit",r.Reason);Assert.False(r.ProductionTargetsReached);
        var second=Query();second["runId"]="33333333-3333-3333-3333-333333333333";
        var cancelled=c.Start(Parse(second),25,0,62,_=>{},production:Sample(plank:0));
        c.Cancel(cancelled.RunId,25,3,63,_=>{});c.Tick(25,0,64,_=>{});
        Assert.Equal("cancelled",cancelled.State);Assert.False(cancelled.ProductionTargetsReached);
        Assert.Throws<BridgeRejectionException>(()=>c.Start(Parse(second),25,0,65,_=>{},production:Sample()));
    }

    [Fact]
    public void ManualSpeedChangeInterruptsWithoutRestoringSpeed()
    {
        var c=new SimulationRunController();var commands=new List<int>();
        var r=c.Start(Parse(Query()),24,0,0,commands.Add,production:Sample(plank:0));
        c.Tick(24.1,3,1,commands.Add,production:Sample(plank:0));
        c.Tick(24.2,1,2,commands.Add,production:Sample());
        Assert.Equal("interrupted",r.State);Assert.Single(commands);Assert.False(r.ProductionTargetsReached);
    }

    [Theory]
    [InlineData("Gear:1;Gear:2")]
    [InlineData("Gear:0")]
    [InlineData("Gear:1000001")]
    [InlineData("Gear:1.5")]
    [InlineData("Gear:1;")]
    [InlineData("A:1;B:1;C:1;D:1;E:1;F:1;G:1;H:1;I:1")]
    public void RejectMalformedTargets(string encoded)
    { var q=Query();q["goodTargets"]=encoded;Assert.Throws<ArgumentException>(()=>Parse(q)); }

    [Fact]
    public void EightTargetsAreAcceptedAndDiagnosticClaimsStayBounded()
    {
        var q=Query();q["goodTargets"]="A:1;B:1;C:1;D:1;E:1;F:1;G:1;H:1";
        Assert.Equal(8,Parse(q).GoodTargets.Length);
        var request=Parse(Query());
        var state=new NativeSimulationRun(request.RunId,"running","advancing",24,26,24.5,.5,0,2,60,3,3,false,false,false,
            GoodTargets:[new(){Good="Gear",Amount=4,Available=4},new(){Good="TreatedPlank",Amount=8,Available=0}],
            StopAtScience:10,ObservedScience:10,DiagnosticsObservedGameHours:24.5,
            ProductionDiagnostics:[new(){Good="Gear",Producers=1,Complete=true,Evidence=["producer_ready"]},
                new(){Good="TreatedPlank",Producers=1,Complete=true,Evidence=["ingredients_not_ready","ingredient_global_shortage"],MissingIngredients=["PineResin"]},
                new(){Good="$science",Producers=0,Complete=true,Evidence=["no_current_manufactory_source"]}]);
        ProductionRunContract.Validate(state,request);
        state.ProductionDiagnostics![1].Evidence=["wind_is_certainly_the_cause"];
        Assert.Throws<InvalidDataException>(()=>ProductionRunContract.Validate(state,request));
    }

    [Fact]
    public void RejectLegacyMixAndKeepGoalsOptionalInCatalog()
    {
        var q=Query();q["stopAtAvailableLogs"]="4";Assert.Throws<ArgumentException>(()=>Parse(q));
        var tool=SimulationRunTools.Catalog(true).Single(t=>t.Name=="run_simulation_for");
        var required=tool.InputSchema.GetProperty("required").EnumerateArray().Select(x=>x.GetString()).ToArray();
        Assert.DoesNotContain("goodTargets",required);Assert.DoesNotContain("stopAtScience",required);
        Assert.DoesNotContain(SimulationRunTools.Catalog(false),t=>t.Name=="run_simulation_for");
    }

    [Fact]
    public async Task StructuredMcpTargetsReachTransportAndContradictoryReceiptIsRejected()
    {
        var q=Query();var request=Parse(q);
        var state=new NativeSimulationRun(request.RunId,"completed","production_targets_reached",24,26,24.5,.5,0,2,60,3,0,true,false,true,
            GoodTargets:[new(){Good="Gear",Amount=4,Available=4},new(){Good="TreatedPlank",Amount=8,Available=8}],
            StopAtScience:10,ObservedScience:10,ProductionTargetsReached:true);
        var payload=JsonSerializer.Serialize(new BridgeEnvelope<NativeSimulationRun>(1,request.Session,DateTimeOffset.UnixEpoch,"0.37.0",state),NativeJson.Options);
        using var handler=new Handler(payload);
        using var client=new NativeClient(new NativeConfiguration(8081,new string('a',64)),handler);
        var args=JsonSerializer.SerializeToElement(new { session=request.Session,runId=request.RunId,duration=2,unit="hours",speed=3,expectedSpeed=0,maxRealSeconds=60,
            goodTargets=new[]{new{good="TreatedPlank",amount=8},new{good="Gear",amount=4}},stopAtScience=10 });
        var result=await SimulationRunTools.Invoke(client,"run_simulation_for",args,true,TestContext.Current.CancellationToken);
        Assert.True(result["data"]!["productionTargetsReached"]!.GetValue<bool>());
        Assert.Contains("goodTargets=",handler.Uri!.Query);Assert.Equal(HttpMethod.Post,handler.Method);
        Assert.Throws<InvalidDataException>(()=>ProductionRunContract.Validate(state with {ProductionTargetsReached=false},request));
        Assert.Throws<InvalidDataException>(()=>ProductionRunContract.Validate(state with {StopAtScience=11},request));
    }

    private sealed class Handler(string payload):HttpMessageHandler
    {
        public Uri? Uri;public HttpMethod? Method;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {Uri=request.RequestUri;Method=request.Method;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(payload)});}
    }
}
