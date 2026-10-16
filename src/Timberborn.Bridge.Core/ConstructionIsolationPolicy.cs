namespace Timberborn.Bridge.Core;

// Exclusion, not hypothetical builder-path prediction. Owned sites may only be
// observed while waiting/confirming. Batch mode also permits its own recorded
// initialization window; its dispatcher waits for confirmation before another wave.
public static class ConstructionIsolationPolicy
{
    public static bool Allows(bool complete,
        IEnumerable<(bool Initialized, bool Finished, bool Unfinished, bool Owned)> objects,
        bool allowOwnedInitialization = false) => Blocker(complete, objects, allowOwnedInitialization) is null;

    public static string? Blocker(bool complete,
        IEnumerable<(bool Initialized, bool Finished, bool Unfinished, bool Owned)> objects,
        bool allowOwnedInitialization = false)
    {
        if (!complete) return "construction_inventory_incomplete";
        foreach (var o in objects) {
            if (allowOwnedInitialization && o.Owned && !o.Initialized) continue;
            if (!o.Initialized) return "construction_initialization_pending";
            if (o.Finished == o.Unfinished) return "construction_state_unknown";
            if (o.Unfinished && !o.Owned) return "foreign_construction";
        }
        return null;
    }
}
