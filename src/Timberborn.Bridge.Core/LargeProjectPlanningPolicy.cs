namespace Timberborn.Bridge.Core;

// Only before the first order: before preview mutation or after confirmed preview cleanup.
// Never reclassify cleanup failures or failures after job creation/placement as preflight failures.
public static class LargeProjectPlanningPolicy
{
    public static bool IsPreDispatchCode(string? code) => code is
        "project_plan_requires_pause" or "project_plan_faulted" or "project_plan_busy" or
        "project_plan_foreign_construction" or "project_plan_district_unavailable" or
        "project_plan_construction_inventory_incomplete" or "project_plan_construction_initialization_pending" or
        "project_plan_construction_state_unknown" or
        "project_plan_navigation_access_limit" or "project_plan_navigation_comparison_limit" or "project_plan_navigation_no_probes" or
        "project_plan_navigation_district_coverage_unknown" or "project_plan_navigation_preview_baseline_mismatch" or
        "project_plan_navigation_no_connected_baseline" or "project_plan_navigation_coverage_unknown";

    public static void RequireAvailable(bool faulted, bool paused, bool busy, bool isolated)
    {
        if (faulted) throw new BridgeRejectionException("project_plan_faulted");
        if (!paused) throw new BridgeRejectionException("project_plan_requires_pause");
        if (busy) throw new BridgeRejectionException("project_plan_busy");
        if (!isolated) throw new BridgeRejectionException("project_plan_foreign_construction");
    }

    public static void RequireBaseline(bool complete, string reason)
    {
        if (complete) return;
        string code = "project_plan_" + reason;
        throw new BridgeRejectionException(IsPreDispatchCode(code) && reason.StartsWith("navigation_", StringComparison.Ordinal)
            ? code : "project_plan_navigation_coverage_unknown");
    }

    public static void RequireIsolation(string? reason)
    {
        if (reason is null) return;
        string code = "project_plan_" + reason;
        throw new BridgeRejectionException(reason is "foreign_construction" or "construction_inventory_incomplete" or
            "construction_initialization_pending" or "construction_state_unknown" ? code : "project_plan_construction_state_unknown");
    }
}
