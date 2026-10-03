namespace Timberborn.Bridge.Core;

// Exclusion, not hypothetical builder-path prediction. Owned sites may only be
// observed while waiting/confirming; every new placement requires no open sites.
public static class ConstructionIsolationPolicy
{
    public static bool Allows(bool complete,
        IEnumerable<(bool Initialized, bool Finished, bool Unfinished, bool Owned)> objects) =>
        complete && objects.All(o => o.Initialized && o.Finished != o.Unfinished && (o.Finished || o.Owned));
}
