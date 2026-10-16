using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;

namespace Timberborn.AgentBridge;

public static class ConstructionIsolation
{
    public static bool Allows(EntityRegistry entities, IEnumerable<string>? owned = null, bool allowOwnedInitialization = false)
        => Blocker(entities, owned, allowOwnedInitialization) is null;

    public static string? Blocker(EntityRegistry entities, IEnumerable<string>? owned = null, bool allowOwnedInitialization = false)
    {
        var ids = new HashSet<string>(owned ?? Enumerable.Empty<string>());
        // Stream the complete registry; finished vegetation must not hide a later foreign site.
        var objects = entities.Entities.Where(e => !e.Deleted && e.TryGetComponent<BlockObject>(out var b) && !b.IsPreview);
        return ConstructionIsolationPolicy.Blocker(true, objects.Select(e => {
            var b = e.GetComponent<BlockObject>();
            return (e.Initialized, b.IsFinished, b.IsUnfinished, ids.Contains(e.EntityId.ToString("D")));
        }), allowOwnedInitialization);
    }
}
