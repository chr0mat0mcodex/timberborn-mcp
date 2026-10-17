using System.Text.Json.Nodes;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class ProjectRecoveryAdviceTests
{
    [Theory]
    [InlineData("construction","materials_not_ready",true,"no_observed_access_problem","check_material_supply")]
    [InlineData("construction","waiting_for_support_completion",true,"deferred_until_support_finished","wait_for_support_completion")]
    [InlineData("construction","construction_paused",true,"no_observed_access_problem","review_construction_pause")]
    [InlineData("construction","awaiting_builders_or_work",true,"no_observed_access_problem","check_workers_and_work_progress")]
    [InlineData("construction","construction_inactive_unknown",true,"no_observed_access_problem","inspect_construction_inactivity")]
    [InlineData("construction_unreachable","builders_unreachable",true,"observed_access_problem","review_access_repair_plan")]
    [InlineData("finished_access_missing","finished_access_missing",false,"observed_access_problem","inspect_access_repair_conditions")]
    [InlineData("construction_access_unknown","construction_access_unknown",true,"unknown","inspect_unknown_access_or_identity")]
    [InlineData("missing","placement_effect_unconfirmed",true,"unknown","inspect_unknown_access_or_identity")]
    [InlineData("finished","finished",true,"no_observed_access_problem","review_close_finished_project")]
    public void Advice_uses_fresh_evidence_in_both_formats(string observed,string reason,bool eligible,string expectedState,string expectedNext)
    {
        var d=Data(observed,reason,eligible);
        d["closeEligible"]=observed=="finished";
        var result=new JsonObject { ["status"]="ok", ["data"]=d };
        foreach(bool full in new[]{false,true}) {
            var copy=ResponsePresentation.Present("diagnose_large_project",result,full)["data"]!;
            Assert.Equal(eligible,copy["repairEligible"]!.GetValue<bool>());
            Assert.Equal(expectedState,copy["recoveryAdvice"]!["accessRepairState"]!.GetValue<string>());
            Assert.Equal(expectedNext,copy["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
            Assert.Equal("blocked",copy["state"]!.GetValue<string>());
            if(!full)Assert.Equal(expectedNext,copy["summary"]!["nextStep"]!.GetValue<string>());
        }
        Assert.Null(d["recoveryAdvice"]);
    }

    [Fact]
    public void Lost_existing_connection_outweighs_material_wait_and_preserves_both_findings()
    {
        var d=Data("construction","materials_not_ready",true);d["lostConnections"]=1;
        ProjectReportPresentation.Compact("diagnose_large_project",d);
        Assert.Equal("review_access_repair_plan",d["summary"]!["nextStep"]!.GetValue<string>());
        Assert.Equal("check_material_supply",d["parts"]![0]!["nextStep"]!.GetValue<string>());
    }

    [Fact]
    public void Unreachable_part_on_unfinished_support_waits_without_inventing_a_missing_route()
    {
        var d=Data("construction_unreachable","builders_unreachable",true);
        d["parts"]![0]!["unfinishedSupportIndices"]=new JsonArray(1);
        ProjectReportPresentation.ObserveCurrent(d);
        Assert.Equal("wait_for_support_completion",d["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
        Assert.Empty(d["recoveryAdvice"]!["accessProblemPartIndices"]!.AsArray());
    }

    [Fact]
    public void Unknown_baseline_and_released_projects_do_not_recommend_new_repairs()
    {
        var d=Data("construction","materials_not_ready",true);d.Remove("lostConnections");
        ProjectReportPresentation.ObserveCurrent(d);
        Assert.Equal("unknown",d["recoveryAdvice"]!["accessRepairState"]!.GetValue<string>());
        d["released"]=true;
        ProjectReportPresentation.ObserveCurrent(d);
        Assert.Equal("none_project_released",d["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
    }

    [Fact]
    public void Mixed_unknown_access_remains_visible_beside_a_proven_loss()
    {
        var d=Data("construction_access_unknown","construction_access_unknown",false);
        d["lostConnections"]=1;
        ProjectReportPresentation.ObserveCurrent(d);
        Assert.Equal("observed_access_problem",d["recoveryAdvice"]!["accessRepairState"]!.GetValue<string>());
        Assert.Single(d["recoveryAdvice"]!["unknownPartIndices"]!.AsArray());
        Assert.Equal("inspect_access_repair_conditions",d["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
    }

    [Fact]
    public void Explicit_watch_diagnosis_has_advice_but_plain_ledger_does_not()
    {
        var r=new JsonObject { ["status"]="ok",["data"]=new JsonObject { ["state"]="blocked" },
            ["watch"]=new JsonObject { ["diagnosis"]=new JsonObject { ["status"]="ok",
                ["data"]=Data("construction","materials_not_ready",true) } } };
        foreach(bool full in new[]{false,true}) {
            var result=ResponsePresentation.Present("inspect_large_project",r,full);
            Assert.Null(result["data"]!["recoveryAdvice"]);
            Assert.Equal("check_material_supply",result["watch"]!["diagnosis"]!["data"]!["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
        }
    }

    [Fact]
    public void Running_simulation_requires_pause_clarification_before_repair()
    {
        var d=Data("finished_access_missing","finished_access_missing",true);d["currentSpeed"]=7;
        ProjectReportPresentation.Compact("diagnose_large_project",d);
        Assert.Equal("inspect_pause_confirmation",d["summary"]!["nextStep"]!.GetValue<string>());
        Assert.Equal("observed_access_problem",d["recoveryAdvice"]!["accessRepairState"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("0.0","check_material_supply")]
    [InlineData("7.0","inspect_pause_confirmation")]
    [InlineData("0","check_material_supply")]
    [InlineData("1.5","inspect_pause_confirmation")]
    public void Native_numeric_speed_representation_is_supported_in_both_formats(string speed,string expected)
    {
        var original=new JsonObject { ["status"]="ok", ["data"]=Data("construction","materials_not_ready",true) };
        original["data"]!["currentSpeed"]=JsonNode.Parse(speed);
        // Reproduce the wire-backed JsonElement values, not only JsonValue<int> fixtures.
        var wire=JsonNode.Parse(original.ToJsonString())!.AsObject();
        foreach(bool full in new[]{false,true}) {
            var result=ResponsePresentation.Present("diagnose_large_project",wire,full);
            Assert.Equal(expected,result["data"]!["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
            Assert.True(result["data"]!["repairEligible"]!.GetValue<bool>());
        }
    }

    private static JsonObject Data(string observed,string reason,bool eligible)=>new() {
        ["state"]="blocked", ["reason"]="game_time_limit", ["currentSpeed"]=0,
        ["repairEligible"]=eligible,["closeEligible"]=false,["released"]=false,["lostConnections"]=0,
        ["parts"]=new JsonArray(new JsonObject { ["index"]=0,["recorded"]="construction",
            ["observed"]=observed,["reason"]=reason,["unfinishedSupportIndices"]=new JsonArray() })
    };
}
