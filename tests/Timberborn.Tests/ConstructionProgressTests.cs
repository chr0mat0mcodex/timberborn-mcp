using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ConstructionProgressTests
{
    private static ConstructionProgressSample Sample(double hours=10,double? build=0,double? material=1,
        string reason="awaiting_builders_or_work",bool? workers=true,string observed="construction",int stock=12)
        => new() { Id="test-part",GameHours=hours,BuildProgress=build,MaterialProgress=material,
            Reason=reason,BuildersAvailable=workers,Observed=observed,SiteStock=new() { ["Log"]=stock } };

    [Fact]
    public void Baseline_and_paused_reads_never_claim_stall()
    {
        var window=new ConstructionProgressWindow();
        Assert.Equal("baseline_recorded",window.Observe(Sample()).State);
        Assert.Equal("no_game_time_elapsed",window.Observe(Sample()).State);
    }

    [Fact]
    public void Repeated_paused_reads_preserve_elapsed_window()
    {
        var window=new ConstructionProgressWindow();window.Observe(Sample());
        var first=window.Observe(Sample(12,build:0.5));
        var repeat=window.Observe(Sample(12,build:0.5));
        Assert.Equal("construction_progress",first.State);
        Assert.Equal(first.State,repeat.State);Assert.Equal(2,repeat.ElapsedGameHours);
        Assert.Equal(0.5,repeat.BuildProgressDelta);
        Assert.Equal(12,window.Observe(Sample(13,build:0.6)).Before!.GameHours);
    }

    [Fact]
    public void Completion_survives_missing_construction_component()
        => Assert.Equal("finished_since_baseline",ConstructionProgressPolicy.Compare(Sample(),
            Sample(12,build:null,material:null,observed:"finished")).State);

    [Fact]
    public void Net_stock_increase_is_material_evidence_not_gross_delivery()
    {
        var result=ConstructionProgressPolicy.Compare(Sample(material:0,stock:0),Sample(12,material:0,stock:3));
        Assert.Equal("material_progress",result.State);Assert.Equal(3,result.SiteStockDelta!["Log"]);
        Assert.Contains("not_continuous",result.Scope);
    }

    [Theory]
    [InlineData("builders_outside_working_hours",false,"currently_outside_working_hours")]
    [InlineData("materials_not_ready",true,"waiting_for_materials")]
    [InlineData("construction_paused",true,"current_blocker")]
    [InlineData("builders_unreachable",true,"current_blocker")]
    [InlineData("waiting_for_support_completion",true,"current_blocker")]
    [InlineData("awaiting_builders_or_work",true,"no_progress_builders_available_at_endpoints")]
    [InlineData("awaiting_builders_or_work",null,"no_progress_observed")]
    public void Unchanged_endpoints_distinguish_current_causes(string reason,bool? workers,string state)
        => Assert.Equal(state,ConstructionProgressPolicy.Compare(Sample(),Sample(12,reason:reason,workers:workers)).State);

    [Fact]
    public void Unknown_removed_and_regressed_are_not_stalls()
    {
        Assert.Equal("observation_unknown",ConstructionProgressPolicy.Compare(Sample(),Sample(12,build:null)).State);
        Assert.Equal("observation_unknown",ConstructionProgressPolicy.Compare(Sample(),Sample(12,observed:"missing")).State);
        Assert.Equal("progress_regressed",ConstructionProgressPolicy.Compare(Sample(build:0.5),Sample(12)).State);
        Assert.Equal("not_comparable",ConstructionProgressPolicy.Compare(Sample(),Sample(9)).State);
        var changed=Sample(12);changed.Id="another-part";
        Assert.Equal("not_comparable",ConstructionProgressPolicy.Compare(Sample(),changed).State);
    }

    [Fact]
    public void Endpoint_work_does_not_imply_full_window_work()
    {
        var result=ConstructionProgressPolicy.Compare(Sample(workers:false),Sample(12));
        Assert.Equal("no_progress_observed",result.State);
    }

    [Fact]
    public void Backwards_time_resets_window_without_reusing_old_future_sample()
    {
        var window=new ConstructionProgressWindow();window.Observe(Sample());
        Assert.Equal("not_comparable",window.Observe(Sample(9)).State);
        Assert.Equal(9,window.Observe(Sample(11)).Before!.GameHours);
    }

    [Theory]
    [InlineData("awaiting_builders_or_work","check_workers_access_and_competing_sites")]
    [InlineData("builders_outside_working_hours","wait_for_construction_working_hours")]
    public void Progress_advice_is_actionable_but_does_not_override_current_off_hours(string reason,string next)
    {
        var comparison=JsonSerializer.SerializeToNode(ConstructionProgressPolicy.Compare(Sample(),Sample(12)),
            new JsonSerializerOptions { PropertyNamingPolicy=JsonNamingPolicy.CamelCase });
        var parts=new JsonArray(new JsonObject { ["id"]="test-part",["index"]=0,
            ["observed"]="construction",["reason"]=reason });
        var data=new JsonObject { ["parts"]=parts,["lostConnections"]=0,["currentSpeed"]=0,
            ["progressComparison"]=new JsonObject { ["parts"]=new JsonArray(comparison) } };
        ProjectRecoveryAdvice.Apply(data,parts);
        Assert.Equal(next,parts[0]!["nextStep"]!.GetValue<string>());
        Assert.Equal(next,data["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
        data["lostConnections"]=1;
        ProjectRecoveryAdvice.Apply(data,parts);
        Assert.Equal("inspect_access_repair_conditions",data["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Comparison_and_finished_parts_survive_presentation(bool full)
    {
        var comparison=JsonSerializer.SerializeToNode(ConstructionProgressPolicy.Compare(Sample(),Sample(12,observed:"finished")),
            new JsonSerializerOptions { PropertyNamingPolicy=JsonNamingPolicy.CamelCase });
        var report=new JsonObject { ["status"]="ok",["data"]=new JsonObject {
            ["state"]="stopped",["released"]=false,["closeEligible"]=true,["lostConnections"]=0,["currentSpeed"]=0,
            ["parts"]=new JsonArray(new JsonObject { ["index"]=0,["observed"]="finished",["reason"]="finished" }),
            ["progressComparison"]=new JsonObject { ["parts"]=new JsonArray(comparison) } } };
        var result=ResponsePresentation.Present("diagnose_large_project",report,full);
        Assert.Equal("finished_since_baseline",result["data"]!["progressComparison"]!["parts"]![0]!["state"]!.GetValue<string>());
    }
}
