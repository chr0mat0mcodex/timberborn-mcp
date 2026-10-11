using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ProjectReportPresentationTests
{
    private static JsonObject Wrap(string data) => JsonNode.Parse("{\"status\":\"ok\",\"data\":" + data + "}")!.AsObject();
    private static JsonObject Ledger() => Wrap("""
    {"actionId":"synthetic-project","planKey":"synthetic-key","state":"blocked","reason":"builders_unreachable",
     "executionMode":"batch","entityIds":["a","b","c","d"],"partStates":["finished","construction","unconfirmed","pending"],
     "order":[0,1,2,3],"ordered":3,"finished":1,"blockedPart":1,"deferredParts":[3],"regularExecutionAllowed":false,
     "simulation":{"state":"cancelled","terminal":true,"pauseConfirmed":true,"observedSpeed":0,"elapsedGameHours":1.125,
       "reason":"cancelled","goodTargets":[],"stopAtScience":null,"productionTargetsReached":false,"buildingProduction":[],"buildingIds":[]},
     "limitations":["inspect_never_dispatches"]}
    """);

    [Fact]
    public void LedgerSeparatesAttemptConfirmationAndRecordedAccessWithoutChangingFull()
    {
        var full = Ledger(); string original = full.ToJsonString();
        var result = ResponsePresentation.Present("inspect_large_project", full, false);
        var d = result["data"]!; var s = d["summary"]!;
        Assert.Equal(4,s["planned"]!.GetValue<int>()); Assert.Equal(3,s["attempted"]!.GetValue<int>());
        Assert.Equal(2,s["confirmedPlaced"]!.GetValue<int>()); Assert.Equal(1,s["finishedAccessRecorded"]!.GetValue<int>());
        Assert.Equal(1,s["unconfirmed"]!.GetValue<int>()); Assert.Equal(1,s["pending"]!.GetValue<int>());
        Assert.Equal("b",s["knownBlockedParts"]![0]!["id"]!.GetValue<string>());
        Assert.Equal("builders_unreachable",s["knownBlockedParts"]![0]!["reason"]!.GetValue<string>());
        Assert.Equal("recorded_project_state_not_fresh_access",s["evidence"]!.GetValue<string>());
        Assert.Equal("diagnose_existing_project",s["nextStep"]!.GetValue<string>());
        Assert.Equal(original,full.ToJsonString()); Assert.Equal(original,ResponsePresentation.Present("inspect_large_project",full,true).ToJsonString());
        Assert.Null(d["entityIds"]); Assert.Equal(3,d["attentionParts"]!.AsArray().Count);
        Assert.Equal("c",d["attentionParts"]![1]!["id"]!.GetValue<string>());
        Assert.Equal(2,d["attentionParts"]![1]!["index"]!.GetValue<int>());
        Assert.Equal(1.125,d["simulation"]!["elapsedGameHours"]!.GetValue<double>());
        Assert.False(d["regularExecutionAllowed"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("unconfirmed")]
    [InlineData("stopped")]
    [InlineData("interrupted")]
    [InlineData("pausing")]
    public void UnconfirmedPauseNeverRecommendsNewOrder(string state)
    {
        var r=Ledger();r["data"]!["state"]=state;r["data"]!["simulation"]!["pauseConfirmed"]=false;
        var d=ResponsePresentation.Present("stop_large_project",r,false)["data"]!;
        Assert.Equal(state,d["state"]!.GetValue<string>());
        Assert.Equal("inspect_pause_confirmation",d["summary"]!["nextStep"]!.GetValue<string>());
        Assert.False(d["simulation"]!["pauseConfirmed"]!.GetValue<bool>());
    }

    [Fact]
    public void CompletedLargeReceiptOmitsFinishedIdsAndIsSmaller()
    {
        var r=Ledger();var d=r["data"]!;
        d["state"]="completed";d["blockedPart"]=null;
        d["entityIds"]=new JsonArray(Enumerable.Range(0,32).Select(i=>(JsonNode?)JsonValue.Create($"11111111-1111-4111-8111-{i+1:000000000000}")).ToArray());
        d["partStates"]=new JsonArray(Enumerable.Range(0,32).Select(_=>(JsonNode?)JsonValue.Create("finished")).ToArray());
        d["ordered"]=32;d["finished"]=32;
        var compact=ResponsePresentation.Present("inspect_large_project",r,false);
        Assert.Equal(32,compact["data"]!["summary"]!["finishedAccessRecorded"]!.GetValue<int>());
        Assert.Empty(compact["data"]!["attentionParts"]!.AsArray());
        Assert.True(compact.ToJsonString().Length<r.ToJsonString().Length);
        d["released"]=true;
        Assert.Equal("none_project_released",ResponsePresentation.Present("inspect_large_project",r,false)["data"]!["summary"]!["nextStep"]!.GetValue<string>());
    }

    [Fact]
    public void ManualLedgerDoesNotInventPauseOrFreshAccess()
    {
        var r=Ledger();var d=r["data"]!.AsObject();d.Remove("simulation");d.Remove("executionMode");d.Remove("blockedPart");d["state"]="waiting";
        var compact=ResponsePresentation.Present("advance_large_project",r,false)["data"]!;
        Assert.Null(compact["simulation"]);
        Assert.Equal("diagnose_existing_project",compact["summary"]!["nextStep"]!.GetValue<string>());
        Assert.Equal("ledger_only",compact["summary"]!["blockerCoverage"]!.GetValue<string>());
    }

    [Fact]
    public void TwoFinishedDiagnosisPartsAreCountedWithoutRepeatingTheirDetails()
    {
        var r=Wrap("""
        {"actionId":"synthetic","state":"completed","released":false,"closeEligible":true,"repairEligible":false,
         "parts":[{"index":0,"id":"11111111-1111-4111-8111-111111111111","template":"Path","position":{"x":1,"y":2,"z":3},
          "recorded":"finished","observed":"finished","reason":"finished","construction":null,"availableGoods":null},
          {"index":1,"id":"22222222-2222-4222-8222-222222222222","template":"Path","position":{"x":1,"y":3,"z":3},
          "recorded":"finished","observed":"finished","reason":"finished","construction":null,"availableGoods":null}]}
        """);
        var compact=ResponsePresentation.Present("diagnose_large_project",r,false);
        Assert.Equal(2,compact["data"]!["summary"]!["finishedAccessible"]!.GetValue<int>());
        Assert.Empty(compact["data"]!["parts"]!.AsArray());
        Assert.True(compact.ToJsonString().Length<r.ToJsonString().Length);
        Assert.Equal(2,ResponsePresentation.Present("diagnose_large_project",r,true)["data"]!["parts"]!.AsArray().Count);
    }

    [Fact]
    public void DiagnosisKeepsConcreteAccessFailuresAndDoesNotCountInaccessibleFinishAsAccessible()
    {
        var r=Wrap("""
        {"actionId":"synthetic","state":"stopped","closeEligible":false,"repairEligible":true,"released":false,
         "lostConnections":1,"affected":[{"id":"old-road"}],"affectedTruncated":true,"foreignSites":[],"foreignSitesTruncated":false,
         "parts":[{"index":0,"id":"a","recorded":"construction","observed":"construction_unreachable","reason":"builders_unreachable",
           "construction":{"hasMaterialsToResumeBuilding":null,"buildTimeProgress":0.125,"materials":{"siteStock":[]}},"availableGoods":[]},
          {"index":1,"id":"b","recorded":"finished","observed":"finished_access_missing","reason":"finished_access_missing","construction":null},
          {"index":2,"id":"c","recorded":"pending","observed":"missing","reason":"not_ordered","construction":null}],
         "lastValidationFailure":{"reason":"preview_restore_failed","constructionAccess":{"state":"unknown","items":[{"id":"old-road","state":"unknown","accessCells":[1,2]}]}}}
        """);
        var d=ResponsePresentation.Present("diagnose_large_project",r,false)["data"]!;
        Assert.Equal(0,d["summary"]!["finishedAccessible"]!.GetValue<int>());
        Assert.Equal(2,d["summary"]!["confirmedPresent"]!.GetValue<int>());
        Assert.Equal(2,d["summary"]!["attentionPartIndices"]!.AsArray().Count);
        Assert.Equal("review_access_repair_plan",d["summary"]!["nextStep"]!.GetValue<string>());
        Assert.True(d["affectedTruncated"]!.GetValue<bool>()); Assert.Equal("old-road",d["affected"]![0]!["id"]!.GetValue<string>());
        Assert.True(d["parts"]![0]!["construction"]!.AsObject().ContainsKey("hasMaterialsToResumeBuilding"));
        Assert.Equal(0.125,d["parts"]![0]!["construction"]!["buildTimeProgress"]!.GetValue<double>());
        Assert.Equal("unknown",d["lastValidationFailure"]!["constructionAccess"]!["items"]![0]!["state"]!.GetValue<string>());
        Assert.Null(d["parts"]![0]!["construction"]!["materials"]);
        Assert.NotNull(r["data"]!["parts"]![0]!["construction"]!["materials"]);
    }

    [Fact]
    public void AccessPlanRetainsExecutableStepsAndSafetyWhileOmittingCellEvidence()
    {
        var r=Wrap("""
        {"candidateFound":false,"blockedPart":0,"phase":"construction_work_cell","reason":"blocked","steps":[{"template":"Synthetic","dependsOn":[]}],
         "cleanupCandidateIndices":[1],"plan":{"planKey":"key","pilotEligible":false,"regularExecutionAllowed":false,"restored":false,
          "parts":[{"index":0,"placementState":"deferred_native_validation","constructionAccess":"unknown","powerPorts":[{}],
            "constructionWorkAccess":{"required":true,"state":"no_connected_work_cell","nativeConstructionProven":false,"reachableCells":[]}}],
          "roadProtection":{"status":"unknown","reasons":["coverage_unproven"],"candidateCells":[1,2,3],"affected":[{"id":"old"}],"affectedTruncated":true},
          "limitations":["no_reservation_or_execution_permission"]}}
        """);
        var d=ResponsePresentation.Present("plan_large_project_access",r,false)["data"]!;
        Assert.Equal(r["data"]!["steps"]!.ToJsonString(),d["steps"]!.ToJsonString());
        Assert.Equal("key",d["plan"]!["planKey"]!.GetValue<string>());
        Assert.Equal(0,d["plan"]!["summary"]!["knownBlockedParts"]![0]!.GetValue<int>());
        Assert.Single(d["plan"]!["summary"]!["deferredParts"]!.AsArray());
        Assert.Equal("unknown",d["plan"]!["parts"]![0]!["constructionAccess"]!.GetValue<string>());
        Assert.False(d["plan"]!["restored"]!.GetValue<bool>()); Assert.True(d["plan"]!["roadProtection"]!["affectedTruncated"]!.GetValue<bool>());
        Assert.Equal("resolve_access_plan_blockers",d["nextStep"]!.GetValue<string>());
        Assert.Null(d["plan"]!["roadProtection"]!["candidateCells"]);
    }

    [Fact]
    public void CatalogAndParserExposeOptionalDetailOnlyForProjectTools()
    {
        var catalog=LargeProjectTools.Catalog(true,true).ToArray();
        foreach(var tool in catalog.Where(t=>t.Name!="inspect_power_network")) {
            var described=ResponsePresentation.Describe(tool);
            Assert.Equal("compact",described.InputSchema.GetProperty("properties").GetProperty("detail").GetProperty("default").GetString());
            Assert.DoesNotContain(described.InputSchema.GetProperty("required").EnumerateArray(),p=>p.GetString()=="detail");
            var parsed=ResponsePresentation.ReadRequest(tool.Name,JsonSerializer.SerializeToElement(new{detail="full",actionId="synthetic"}));
            Assert.True(parsed.Full); Assert.False(parsed.Arguments.TryGetProperty("detail",out _));
        }
        Assert.False(ResponsePresentation.Supports("inspect_power_network"));
        using var duplicate=JsonDocument.Parse("{\"detail\":\"compact\",\"detail\":\"full\"}");
        Assert.Throws<ArgumentException>(()=>ResponsePresentation.ReadRequest("run_large_project",duplicate.RootElement));
        var error=JsonNode.Parse("{\"status\":\"error\",\"error\":{\"execution\":\"unconfirmed\"}}")!.AsObject();
        Assert.Equal(error.ToJsonString(),ResponsePresentation.Present("run_large_project",error,false).ToJsonString());
        var result=ResponsePresentation.ToToolResult(ResponsePresentation.Present("inspect_large_project",Ledger(),false));
        Assert.Equal(result.StructuredContent!.Value.GetRawText(),Assert.IsType<ModelContextProtocol.Protocol.TextContentBlock>(Assert.Single(result.Content)).Text);
    }
}
