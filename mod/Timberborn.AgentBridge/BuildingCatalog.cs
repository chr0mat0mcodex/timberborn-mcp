using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.GameFactionSystem;
using Timberborn.ResourceCountingSystem;
using Timberborn.ScienceSystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

public sealed class BuildingCatalog(TemplateService service, FactionService faction,
    BuildingUnlockingService unlocks, ResourceCountingService resources)
{
    // TemplateService uses the scene's template collections, including the active faction.
    private TemplateSpec[] Templates() => service.GetAll<TemplateSpec>()
        .Where(t => t.HasSpec<PlaceableBlockObjectSpec>() && t.HasSpec<BuildingSpec>() && t.HasSpec<BlockObjectSpec>())
        .GroupBy(t => t.TemplateName).Select(g => g.First()).OrderBy(t => t.TemplateName, StringComparer.Ordinal).ToArray();

    public TemplateSpec Resolve(string name)
    {
        var template = Templates().SingleOrDefault(t => t.TemplateName == name);
        if (template is null || Reasons(template).Length != 0) throw new ArgumentException("unsupported_building");
        return template;
    }

    private static string[] Reasons(TemplateSpec template)
    {
        var p = template.GetSpec<PlaceableBlockObjectSpec>();
        return BuildingPolicy.UnsupportedReasons(p.Layout.ToString(), p.ToolShape.ToString(), p.CanBeAttachedToTerrainSide,
            p.DevModeTool, template.GetSpec<BlockObjectSpec>().Blocks.Length);
    }

    public object Observe(BridgeRequest r)
    {
        var all = Templates();
        var items = all.Skip(r.Offset).Take(r.Limit).Select(t => {
            var p = t.GetSpec<PlaceableBlockObjectSpec>(); var b = t.GetSpec<BlockObjectSpec>(); var building = t.GetSpec<BuildingSpec>();
            var reasons = Reasons(t);
            return new { template = t.TemplateName, available = t.UsableWithCurrentFeatureToggles && p.UsableWithCurrentFeatureToggles,
                unlocked = unlocks.Unlocked(building), supported = reasons.Length == 0, unsupportedReasons = reasons,
                layout = p.Layout.ToString(), toolShape = p.ToolShape.ToString(), toolGroup = p.ToolGroupId,
                size = Vec(b.Size), entrance = b.Entrance.HasEntrance ? Vec(b.Entrance.Coordinates) : null,
                placeFinished = building.PlaceFinished,
                costs = building.BuildingCost.Select(c => new { id = c.Id, required = c.Amount,
                    availableGlobally = resources.GetGlobalResourceCount(c.Id).AvailableStock }).ToArray() };
        }).ToArray();
        return new { faction = faction.Current.Id, offset = r.Offset, limit = r.Limit, total = all.Length, items,
            hasMore = r.Offset + items.Length < all.Length,
            limitations = new[] { "active_scene_template_collections", "supported_is_not_live_tested", "single_unflipped_placement_only",
                "configuration_after_construction_separate", "global_stock_not_local_delivery" } };
    }
    private static object Vec(Vector3Int p) => new { x = p.x, y = p.y, z = p.z };
}
