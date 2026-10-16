namespace Timberborn.Bridge.Core;

// These codes are emitted only before reserving a run, recording a job or placing a part.
// Generic conflicts and failures after dispatch must never imply non-execution.
public static class LargeProjectStartPolicy
{
    public static bool IsPreDispatchCode(string? code) => code is
        "project_start_busy" or "project_start_capacity" or "project_start_identity_conflict" or
        "project_start_ledger_conflict" or "project_start_footprint_changed" or
        "project_start_outside_map" or "project_start_footprint_occupied" or
        "project_start_unresolved" or "project_start_simulation_unavailable" or
        "project_start_plan_changed" or "project_start_ineligible" or
        "project_start_baseline_unavailable" or "project_start_entity_conflict" or
        "project_start_preflight_conflict" or "repair_parent_ineligible" or
        "repair_district_mismatch" or "repair_helpers_only" or "repair_nested" or
        "repair_other_project_unresolved";

    public static void RequirePlan(string? actualKey, string requestedKey, bool eligible)
    {
        if (actualKey != requestedKey) throw new BridgeRejectionException("project_start_plan_changed");
        if (!eligible) throw new BridgeRejectionException("project_start_ineligible");
    }
}
