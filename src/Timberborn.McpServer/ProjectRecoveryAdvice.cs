using System.Text.Json;
using System.Text.Json.Nodes;

namespace Timberborn.McpServer;

// Advice from validated fresh observations, never a change to native repair eligibility.
public static class ProjectRecoveryAdvice
{
    public static void Apply(JsonObject data, JsonArray parts)
    {
        var missing = new JsonArray(); var unknown = new JsonArray();
        var nextSteps = new HashSet<string>();
        foreach (var part in parts.OfType<JsonObject>()) {
            string? observed=part["observed"]?.GetValue<string>();
            string? reason=part["reason"]?.GetValue<string>();
            bool support=part["unfinishedSupportIndices"] is JsonArray { Count: > 0 };
            string next=observed switch {
                "finished" => "none_part_finished",
                "finished_access_missing" => "inspect_missing_access",
                "construction_unreachable" when support => "wait_for_support_completion",
                "construction_unreachable" => "inspect_missing_access",
                "construction" => reason switch {
                    "materials_not_ready" => "check_material_supply",
                    "waiting_for_support_completion" => "wait_for_support_completion",
                    "construction_paused" => "review_construction_pause",
                    "awaiting_builders_or_work" => "check_workers_and_work_progress",
                    _ => "inspect_construction_inactivity"
                },
                "missing" when reason is "not_ordered" or "previously_observed_object_removed" => "review_unissued_or_removed_parts",
                _ => "inspect_unknown_access_or_identity"
            };
            part["nextStep"]=next;
            nextSteps.Add(next);
            if(next=="inspect_missing_access")missing.Add(part["index"]?.DeepClone());
            if(next=="inspect_unknown_access_or_identity")unknown.Add(part["index"]?.DeepClone());
        }
        int? losses=data["lostConnections"]?.GetValue<int>();
        bool needed=losses>0 || missing.Count>0;
        bool uncertain=losses is null || unknown.Count>0 || parts.Count==0;
        string nextStep=needed
            ? data["repairEligible"]?.GetValue<bool>()==true ? "review_access_repair_plan" : "inspect_access_repair_conditions"
            : uncertain ? "inspect_unknown_access_or_identity"
            : new[] { "review_construction_pause", "wait_for_support_completion", "check_material_supply",
                "check_workers_and_work_progress", "inspect_construction_inactivity", "review_unissued_or_removed_parts" }
                .FirstOrDefault(nextSteps.Contains) ?? "inspect_release_conditions";
        if (data["released"]?.GetValue<bool>()==true) nextStep="none_project_released";
        else if (data["currentSpeed"] is { } speed && JsonSerializer.SerializeToElement(speed).GetDouble() > 0) nextStep="inspect_pause_confirmation";
        else if (!needed && !uncertain && nextSteps.SetEquals(["none_part_finished"]))
            nextStep=data["closeEligible"]?.GetValue<bool>()==true ? "review_close_finished_project" : "inspect_release_conditions";
        else if (data["closeEligible"]?.GetValue<bool>()==true && !needed && !uncertain)
            nextStep="review_close_abandons_unissued_parts";
        data["recoveryAdvice"]=new JsonObject {
            ["evidence"]="fresh_recovery_observation",
            ["accessRepairState"]=needed ? "observed_access_problem" : uncertain ? "unknown" :
                nextSteps.Contains("wait_for_support_completion") ? "deferred_until_support_finished" : "no_observed_access_problem",
            ["accessProblemPartIndices"]=missing, ["unknownPartIndices"]=unknown,
            ["lostConnections"]=data["lostConnections"]?.DeepClone(),
            ["nextStep"]=nextStep,
            ["scope"]="recommendation_not_permission_or_worker_supply_proof"
        };
    }
}
