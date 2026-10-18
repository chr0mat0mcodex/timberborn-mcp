using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class PlankChainTests
{
    private const string Session="11111111-1111-4111-8111-111111111111";
    private const string District="22222222-2222-4222-8222-222222222222";
    private const string Mill="33333333-3333-4333-8333-333333333333";
    private static GraphRecipe Recipe=>new("PlankRecipe",[new("Log",1)],[new("Plank",1)],1,null,0,0,["LumberMill.Folktails"]);
    private static NativeOperation Operation(bool flooded=false,bool output=false,bool powered=false,string recipe="PlankRecipe")=>new(
        Mill,"LumberMill.Folktails",true,false,new(1,1,1,false,true,true),
        new(true,recipe,false,true,true,false,output,0),[],[],[],new(true,true,powered,true,0,50,true),null,
        new BottleneckEvidence { State="observed",Complete=true,DistrictConnected=true,EntranceBlocked=false,
            Goods=[new() { Good="Plank",Role="product",Amount=1,ReachableEndpoints=1,MatchingEndpoints=1,ReachableExamples=[District] }] },
        new(true,flooded,false,null,null,false,null));
    private static AutoOperatingReport Supply(bool complete=true,bool include=true,int logs=10)=>new("observed","plank_chain_supply",
        new("selected",District,District,include?[Mill]:[],complete,complete?null:128,1,false,[],[],[]),
        [new("Log",0),new("Plank",0)],new(new(new(4,0,0,4,0),new(0,0,0,0,0)),
            include?[new(Mill,"material_producer","observed",new("LumberMill.Folktails",true,false,new(1,1,1,false,true,true),null,null,null),null)]:[],
            [new("Log",0,logs,null,[],[],"not_observed_in_selection",false)],[],true,null,false,false,[],[]),[],[]);
    private static BridgeEnvelope<T> E<T>(T data)=>new(1,Session,DateTimeOffset.UnixEpoch,"synthetic",data);
    private static NativeProductionGraph Graph=>new("definitions","test","test","test",true,new([],[Recipe],[],[]),new([],[],[]),[]);

    [Fact]
    public void Catalog_is_read_only_and_parser_requires_district()
    {
        var tool=Assert.Single(NativeTools.Catalog(),t=>t.Name==PlankChainTools.Name);
        Assert.True(tool.Annotations!.ReadOnlyHint);Assert.False(tool.Annotations.DestructiveHint);
        var request=PlankChainTools.Parse(JsonSerializer.SerializeToElement(new { session=Session,districtId=District }));
        Assert.Equal(District,request.District);
        Assert.Throws<ArgumentException>(()=>PlankChainTools.Parse(JsonSerializer.SerializeToElement(new { session=Session })));
        Assert.Throws<ArgumentException>(()=>PlankChainTools.Parse(JsonSerializer.SerializeToElement(new { session=Session,districtId=District,build=true })));
    }

    [Fact]
    public void Power_and_full_output_keep_concrete_existing_destination_advice()
    {
        var report=PlankChainTools.Build(District,[Recipe],Supply(),new([new(Mill,"observed",Operation())],1,[]),[]);
        Assert.Contains(report.Findings,f=>f.Code=="consumer_not_powered");
        Assert.Contains(report.Findings,f=>f.Code=="output_full_destination_available");
        Assert.Contains(report.Findings,f=>f.Code=="verify_bounded_production");
        Assert.False(report.ProductionProven);
        Assert.Contains("no_material_start_gate",report.Limitations);
        Assert.Contains("read_only_no_automatic_expansion",report.Limitations);
    }

    [Fact]
    public void Flooding_precedes_staff_or_power_recommendations()
    {
        var report=PlankChainTools.Build(District,[Recipe],Supply(),new([new(Mill,"observed",Operation(flooded:true))],1,[]),[]);
        Assert.Equal("building_flooded",report.Findings[0].Code);
    }

    [Fact]
    public void Incomplete_selection_and_no_mill_remain_unknown()
    {
        var report=PlankChainTools.Build(District,[Recipe],Supply(false,false),null,[]);
        Assert.Contains(report.Findings,f=>f.Code=="selection_incomplete"&&f.Severity=="unknown");
        Assert.Contains(report.Findings,f=>f.Code=="sawmill_not_observed"&&f.Severity=="unknown");
        Assert.False(report.ProductionProven);
    }

    [Fact]
    public void Wrong_recipe_is_not_plank_production_and_zero_logs_are_advisory()
    {
        var report=PlankChainTools.Build(District,[Recipe],Supply(logs:0),new([new(Mill,"observed",Operation(recipe:"OtherRecipe"))],1,[]),[]);
        Assert.Contains(report.Findings,f=>f.Code=="plank_recipe_not_selected");
        Assert.Contains(report.Findings,f=>f.Code=="no_global_available_logs"&&f.Severity=="warning");
    }

    [Fact]
    public async Task Composition_requests_only_selected_mills_and_preserves_evidence()
    {
        var calls=0;
        var result=await PlankChainTools.Observe(Session,District,ct=>Task.FromResult(E(Graph)),ct=>Task.FromResult(E(Supply())),
            (ids,ct)=> { calls++;Assert.Equal(new[]{Mill},ids);return Task.FromResult(E(new OperationBatch([new(Mill,"observed",Operation())],1,[]))); },TestContext.Current.CancellationToken);
        Assert.Equal(1,calls);Assert.Equal(3,result.Data.Observations.Length);
        Assert.NotNull(result.Data.Sawmills!.Items[0].Operation!.Bottleneck);
        Assert.Equal(1,result.Data.Recipes[0].Inputs[0].Amount);
    }

    [Fact]
    public async Task Session_change_is_rejected_before_extra_reads()
    {
        await Assert.ThrowsAsync<BridgeRejectionException>(()=>PlankChainTools.Observe(Session,District,
            ct=>Task.FromResult(E(Graph)),ct=>Task.FromResult(E(Supply()) with { SessionId=District }),
            (ids,ct)=>throw new InvalidOperationException("must_not_read"),TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Inactive_power_and_missing_flow_evidence_do_not_invent_failure_causes()
    {
        var inactive=Operation() with { Power=new(true,true,false,true,0,0,false) };
        var report=PlankChainTools.Build(District,[Recipe],Supply(),new([new(Mill,"observed",inactive)],1,[]),[]);
        Assert.DoesNotContain(report.Findings,f=>f.Code=="consumer_not_powered");
        Assert.Contains(report.Findings,f=>f.Code=="consumer_inactive");
        var unknown=PlankChainTools.Build(District,[Recipe],Supply(),new([new(Mill,"observed",inactive with { Bottleneck=null })],1,[]),[]);
        Assert.Contains(unknown.Findings,f=>f.Stage=="observation"&&f.Severity=="unknown");
    }

    [Fact]
    public async Task Wrong_batch_identity_is_rejected()
    {
        await Assert.ThrowsAsync<BridgeRejectionException>(()=>PlankChainTools.Observe(Session,District,
            ct=>Task.FromResult(E(Graph)),ct=>Task.FromResult(E(Supply())),
            (ids,ct)=>Task.FromResult(E(new OperationBatch([new(District,"not_found",null)],1,[]))),TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Cancelled_request_never_starts_observation()
    {
        using var cancellation=new CancellationTokenSource();cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>PlankChainTools.Observe(Session,District,
            ct=>throw new InvalidOperationException("must_not_read"),ct=>throw new InvalidOperationException("must_not_read"),
            (ids,ct)=>throw new InvalidOperationException("must_not_read"),cancellation.Token));
    }

    [Fact]
    public async Task No_mill_skips_flow_reads()
    {
        var result=await PlankChainTools.Observe(Session,District,ct=>Task.FromResult(E(Graph)),ct=>Task.FromResult(E(Supply(include:false))),
            (ids,ct)=>throw new InvalidOperationException("must_not_read"),TestContext.Current.CancellationToken);
        Assert.Null(result.Data.Sawmills);
    }
}
