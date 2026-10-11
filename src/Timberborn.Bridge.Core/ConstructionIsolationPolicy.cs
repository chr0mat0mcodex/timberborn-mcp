namespace Timberborn.Bridge.Core;

// Exclusion, not hypothetical builder-path prediction. Owned sites may only be
// observed while waiting/confirming. Batch mode also permits its own recorded
// initialization window; its dispatcher waits for confirmation before another wave.
public static class ConstructionIsolationPolicy
{
    public static bool Allows(bool complete,
        IEnumerable<(bool Initialized, bool Finished, bool Unfinished, bool Owned)> objects,
        bool allowOwnedInitialization = false) =>
        complete && objects.All(o => allowOwnedInitialization && o.Owned && !o.Initialized ||
            o.Initialized && o.Finished != o.Unfinished && (o.Finished || o.Owned));
}
