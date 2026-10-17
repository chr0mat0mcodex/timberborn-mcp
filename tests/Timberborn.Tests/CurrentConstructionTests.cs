using System.Text.Json.Nodes;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class CurrentConstructionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Fresh_completion_does_not_rewrite_historical_timeout(bool full)
    {
        var original = JsonNode.Parse("""
        {"status":"ok","data":{"state":"blocked","reason":"game_time_limit","released":false,
        "closeEligible":true,"lostConnections":0,"parts":[{"index":0,"recorded":"construction","observed":"finished","reason":"finished"}]}}
        """)!.AsObject();
        var result = ResponsePresentation.Present("diagnose_large_project",original,full)["data"]!;
        Assert.Equal("blocked",result["state"]!.GetValue<string>());
        Assert.Equal("game_time_limit",result["reason"]!.GetValue<string>());
        Assert.Equal("finished_accessible",result["currentConstruction"]!["state"]!.GetValue<string>());
        Assert.Null(original["data"]!["currentConstruction"]);
        if (!full) Assert.Equal("review_close_finished_project",result["summary"]!["nextStep"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("construction")]
    [InlineData("finished_access_missing")]
    [InlineData("missing")]
    [InlineData("identity_mismatch")]
    public void Unresolved_parts_never_become_finished(string observed)
    {
        var d = new JsonObject { ["state"]="blocked", ["closeEligible"]=true, ["lostConnections"]=0,
            ["parts"]=new JsonArray(new JsonObject { ["observed"]=observed }) };
        ProjectReportPresentation.ObserveCurrent(d);
        Assert.Equal("not_fully_finished_accessible",d["currentConstruction"]!["state"]!.GetValue<string>());
    }

    [Fact]
    public void Empty_diagnosis_and_cached_ledger_do_not_claim_fresh_completion()
    {
        var empty = JsonNode.Parse("""{"closeEligible":true,"parts":[],"lostConnections":0}""")!.AsObject();
        ProjectReportPresentation.ObserveCurrent(empty);
        Assert.Equal("not_fully_finished_accessible",empty["currentConstruction"]!["state"]!.GetValue<string>());
        var cached = JsonNode.Parse("""{"state":"blocked","parts":[{"state":"finished"}]}""")!.AsObject();
        ProjectReportPresentation.ObserveCurrent(cached);
        Assert.Null(cached["currentConstruction"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Explicit_watch_diagnosis_exposes_fresh_state_without_rewriting_ledger(bool full)
    {
        var original=JsonNode.Parse("""
        {"status":"ok","data":{"state":"blocked","reason":"game_time_limit"},
         "watch":{"diagnosis":{"status":"ok","data":{"state":"blocked","closeEligible":true,"lostConnections":0,
         "parts":[{"index":0,"recorded":"construction","observed":"finished","reason":"finished"}]}}}}
        """)!.AsObject();
        var result=ResponsePresentation.Present("inspect_large_project",original,full);
        Assert.Null(result["data"]!["currentConstruction"]);
        Assert.Equal("game_time_limit",result["data"]!["reason"]!.GetValue<string>());
        Assert.Equal("finished_accessible",result["watch"]!["diagnosis"]!["data"]!["currentConstruction"]!["state"]!.GetValue<string>());
    }

    [Fact]
    public void Finished_parts_do_not_hide_existing_access_loss()
    {
        var d = JsonNode.Parse("""
        {"state":"blocked","closeEligible":false,"lostConnections":1,
        "parts":[{"index":0,"recorded":"finished","observed":"finished","reason":"finished"}]}
        """)!.AsObject();
        ProjectReportPresentation.Compact("diagnose_large_project",d);
        Assert.Equal("lost_or_unknown",d["currentConstruction"]!["existingAccess"]!.GetValue<string>());
        Assert.Equal("inspect_access_repair_conditions",d["summary"]!["nextStep"]!.GetValue<string>());
    }
}
