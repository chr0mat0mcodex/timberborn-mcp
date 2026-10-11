using System.Text.Json.Nodes;

namespace Timberborn.McpServer;

// Projection of validated native reports only. No reads, dispatch or ledger mutation.
public static class ProjectReportPresentation
{
    public static bool Supports(string name) => LargeProjectTools.Handles(name) && name != "inspect_power_network";

    public static void Compact(string name, JsonObject data)
    {
        if (name == "plan_large_project_access") {
            // Keep the generated executable steps, nested plan key and cleanup indices intact.
            if (data["plan"] is JsonObject plan) Plan(plan);
            data["nextStep"] = data["candidateFound"]?.GetValue<bool>() == true
                ? "review_generated_steps_and_plan" : "resolve_access_plan_blockers";
        } else if (name is "plan_large_project" or "plan_large_project_repair") Plan(data);
        else if (name is "diagnose_large_project" or "close_large_project") Diagnose(data);
        else Ledger(data);
    }

    private static void Plan(JsonObject data)
    {
        if (data["parts"] is not JsonArray parts) return;
        var invalid = new JsonArray();
        var deferred = new JsonArray();
        foreach (var part in parts.OfType<JsonObject>()) {
            if (part["placementState"]?.GetValue<string>() == "deferred_native_validation")
                deferred.Add(part["index"]?.DeepClone());
            if (part["placementState"]?.GetValue<string>() == "native_invalid" ||
                part["previewEntranceConnected"]?.GetValue<bool>() == false ||
                part["lostConnections"]?.GetValue<int>() > 0 ||
                part["constructionWorkAccess"] is JsonObject work && work["required"]?.GetValue<bool>() == true &&
                work["state"]?.GetValue<string>() != "road_reachable") invalid.Add(part["index"]?.DeepClone());
            if (part["powerPorts"] is JsonArray ports) {
                part["powerPortCount"] = ports.Count;
                part.Remove("powerPorts");
            }
            if (part["constructionWorkAccess"] is JsonObject access) access.Remove("reachableCells");
        }
        data["summary"] = new JsonObject {
            ["evidence"] = "native_preview_not_construction", ["planned"] = parts.Count,
            ["knownBlockedParts"] = invalid, ["deferredParts"] = deferred,
            ["nextStep"] = data["pilotEligible"]?.GetValue<bool>() == true
                ? "review_development_pilot_plan" : "resolve_plan_blockers"
        };
        CompactRoadEvidence(data["roadProtection"] as JsonObject);
        // Limitations, per-part unknowns, guard results and power-link semantics are retained.
    }

    private static void Ledger(JsonObject data)
    {
        if (data["partStates"] is not JsonArray states || data["entityIds"] is not JsonArray ids) return;
        var pending = 0; var construction = 0; var finished = 0; var unconfirmed = 0;
        foreach (var state in states) switch (state?.GetValue<string>()) {
            case "pending": pending++; break;
            case "construction": construction++; break;
            case "finished": finished++; break;
            case "unconfirmed": unconfirmed++; break;
        }
        var blocked = new JsonArray();
        if (data["blockedPart"] is JsonNode blockedIndex) {
            int index = blockedIndex.GetValue<int>();
            blocked.Add(new JsonObject { ["index"] = index, ["id"] = ids[index]?.DeepClone(), ["reason"] = data["reason"]?.DeepClone() });
        }
        data["summary"] = new JsonObject {
            ["evidence"] = "recorded_project_state_not_fresh_access", ["planned"] = states.Count,
            ["attempted"] = states.Count - pending, ["confirmedPlaced"] = construction + finished,
            ["finishedAccessRecorded"] = finished, ["construction"] = construction,
            ["unconfirmed"] = unconfirmed, ["pending"] = pending,
            // Empty means no indexed blocker supplied, not that the project is unblocked.
            ["knownBlockedParts"] = blocked, ["blockerCoverage"] = "ledger_only",
            ["nextStep"] = Next(data)
        };
        var activeParts = new JsonArray();
        for (int i = 0; i < states.Count; i++)
            if (states[i]?.GetValue<string>() != "finished" || data["blockedPart"]?.GetValue<int>() == i)
                activeParts.Add(new JsonObject { ["index"] = i, ["id"] = ids[i]?.DeepClone(), ["state"] = states[i]?.DeepClone() });
        data["attentionParts"] = activeParts;
        data.Remove("entityIds"); data.Remove("partStates");
        // Plan identity/duplicate counters remain available in Full; actionId identifies this ledger.
        data.Remove("planKey"); data.Remove("ordered"); data.Remove("finished");
        if (data["executionMode"]?.GetValue<string>() == "batch") data.Remove("order");
        if (data["simulation"] is JsonObject simulation) RunReportPresentation.Compact(simulation);
        // Original indices remain stable. Finished IDs remain available through full inspection.
        data["proofLimits"] = new JsonArray("power_and_production_not_proven",
            "development_pilot_not_complete_construction_preflight", "no_replay_of_unconfirmed_orders", "ledger_lost_on_game_reload");
        data.Remove("limitations");
    }

    private static void Diagnose(JsonObject data)
    {
        if (data["parts"] is not JsonArray parts) return;
        var attentionIndices = new JsonArray(); var observedFinished = 0; var pending = 0; var attempted = 0; var placed = 0;
        foreach (var part in parts.OfType<JsonObject>()) {
            string? observed = part["observed"]?.GetValue<string>();
            string? recorded = part["recorded"]?.GetValue<string>();
            string? reason = part["reason"]?.GetValue<string>();
            if (recorded == "pending") pending++; else attempted++;
            if (observed == "finished") observedFinished++;
            if (observed is "finished" or "finished_access_missing" or "construction" or "construction_unreachable" or "construction_access_unknown") placed++;
            if (reason is not ("finished" or "not_ordered" or "previously_observed_object_removed"))
                attentionIndices.Add(part["index"]?.DeepClone());
            // Keep readiness/progress, including nulls. Detailed costs/stocks are not remaining need.
            if (part["construction"] is JsonObject construction) construction.Remove("materials");
            part.Remove("availableGoods");
        }
        data["summary"] = new JsonObject {
            ["evidence"] = "fresh_recovery_observation", ["planned"] = parts.Count, ["attempted"] = attempted,
            ["confirmedPresent"] = placed, ["finishedAccessible"] = observedFinished, ["pending"] = pending,
            ["attentionPartIndices"] = attentionIndices, ["nextStep"] = Next(data)
        };
        // Successful individual observations are represented by the exact aggregate above.
        // Missing/unknown/inaccessible objects retain their original IDs and reasons.
        for (int i = parts.Count - 1; i >= 0; i--)
            if (parts[i]?["observed"]?.GetValue<string>() == "finished") parts.RemoveAt(i);
        // Keep all affected objects, truncation flags and historical failure reasons.
        if (data["lastValidationFailure"] is JsonObject failure) {
            CompactConstruction(failure["constructionAccess"] as JsonObject);
            if (failure["constructionWorkAccess"] is JsonObject access) access.Remove("reachableCells");
        }
    }

    private static string Next(JsonObject data)
    {
        if (data["released"]?.GetValue<bool>() == true) return "none_project_released";
        if (data["simulation"] is JsonObject sim && sim["pauseConfirmed"]?.GetValue<bool>() != true &&
            data["state"]?.GetValue<string>() != "running") return "inspect_pause_confirmation";
        if (data["closeEligible"]?.GetValue<bool>() == true) return "review_close_abandons_unissued_parts";
        if (data["repairEligible"]?.GetValue<bool>() == true) return "review_access_repair_plan";
        return data["state"]?.GetValue<string>() switch {
            "running" or "pausing" => "inspect_existing_project",
            "ready" => "review_manual_advance",
            _ => data.ContainsKey("closeEligible") ? "resolve_reported_blockers" : "diagnose_existing_project"
        };
    }

    private static void CompactRoadEvidence(JsonObject? road)
    {
        if (road is null) return;
        road.Remove("candidateCells");
        CompactConstruction(road["constructionAccessPreview"] as JsonObject);
    }
    private static void CompactConstruction(JsonObject? construction)
    {
        if (construction is null) return;
        if (construction["details"] is JsonObject details)
            foreach (string field in new[] { "origin", "accesses", "rangeOrigin", "originSource" }) details.Remove(field);
        if (construction["items"] is JsonArray sites)
            foreach (var site in sites.OfType<JsonObject>()) site.Remove("accessCells");
    }
}
