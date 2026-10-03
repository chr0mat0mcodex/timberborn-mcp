using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;

namespace Timberborn.AgentBridge;

public static class ConstructionIsolation
{
    public static bool Allows(EntityRegistry entities, IEnumerable<string>? owned = null)
    {
        var ids = new HashSet<string>(owned ?? Enumerable.Empty<string>());
        var objects = entities.Entities.Where(e => !e.Deleted && e.TryGetComponent<BlockObject>(out var b) && !b.IsPreview)
            .Take(4097).ToArray();
        return ConstructionIsolationPolicy.Allows(objects.Length <= 4096, objects.Select(e => {
            var b = e.GetComponent<BlockObject>();
            return (e.Initialized, b.IsFinished, b.IsUnfinished, ids.Contains(e.EntityId.ToString("D")));
        }));
    }
}
