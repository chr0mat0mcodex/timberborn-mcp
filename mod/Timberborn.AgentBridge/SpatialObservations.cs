using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.GameFactionSystem;
using Timberborn.ResourceCountingSystem;
using Timberborn.ScienceSystem;
using Timberborn.TemplateSystem;
using Timberborn.TerrainSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

// Pure observations: no preview creation, placement, unlock, service registration or deletion.
public sealed class SpatialObservations(EntityRegistry entities, TemplateNameMapper templates,
    FactionService faction, BuildingUnlockingService unlocks, ResourceCountingService resources,
    ITerrainService terrain, IBlockService blocks, BuildingCatalog catalog)
{
    private static readonly string[] PilotTemplates = { "Lodge.Folktails", "Path" };

    public object Objects(BridgeRequest request)
    {
        var candidates = entities.Entities.Where(e => e.Initialized && !e.Deleted &&
            e.TryGetComponent<BlockObject>(out var block) && !block.IsPreview &&
            e.TryGetComponent<TemplateSpec>(out var template) &&
            (e.HasComponent<Building>() || template.TemplateName == "Path"))
            .OrderBy(e => e.EntityId).ToArray();
        var items = candidates.Skip(request.Offset).Take(request.Limit).Select(e =>
        {
            var block = e.GetComponent<BlockObject>();
            var template = e.GetComponent<TemplateSpec>();
            var occupied = block.PositionedBlocks.GetOccupiedCoordinates().Take(65).ToArray();
            return new { id = e.EntityId, template = template.TemplateName, position = Vec(block.Coordinates),
                orientation = block.Orientation.ToString(), finished = block.IsFinished,
                entrance = block.HasEntrance ? Vec(block.PositionedEntrance.Coordinates) : null,
                occupiedCells = occupied.Take(64).Select(Vec).ToArray(), cellsTruncated = occupied.Length > 64 };
        }).ToArray();
        return new { scope = "buildings_and_paths", offset = request.Offset, limit = request.Limit,
            total = candidates.Length, items, hasMore = request.Offset + items.Length < candidates.Length,
            limitations = new[] { "pages_are_fresh_observations", "no_district_reachability", "occupied_cells_capped_at_64" } };
    }

    public object Catalog() => new { faction = faction.Current.Id,
        items = PilotTemplates.Select(CatalogEntry).ToArray(),
        limitations = new[] { "two_template_pilot_not_full_catalog", "global_stock_not_local_delivery", "housing_capacity_not_exposed" } };

    private object CatalogEntry(string name)
    {
        if (!templates.TryGetTemplate(name, out var template) || !template.HasSpec<BlockObjectSpec>() ||
            !template.HasSpec<PlaceableBlockObjectSpec>() || !template.HasSpec<BuildingSpec>())
            return new { template = name, available = false, factionCompatible = Compatible(name),
                unlocked = (bool?)null, size = (object?)null, entrance = (object?)null, costs = Array.Empty<object>() };
        var block = template.GetSpec<BlockObjectSpec>();
        var building = template.GetSpec<BuildingSpec>();
        return new { template = name, available = template.UsableWithCurrentFeatureToggles &&
                template.GetSpec<PlaceableBlockObjectSpec>().UsableWithCurrentFeatureToggles,
            factionCompatible = Compatible(name), unlocked = (bool?)unlocks.Unlocked(building),
            size = Vec(block.Size), entrance = block.Entrance.HasEntrance ? Vec(block.Entrance.Coordinates) : null,
            costs = Costs(building) };
    }

    public object Precheck(BridgeRequest request)
    {
        if ((!request.GenericBuilding && !PilotTemplates.Contains(request.Template)) || !templates.TryGetTemplate(request.Template, out var template) ||
            !template.HasSpec<BlockObjectSpec>() || !template.HasSpec<BuildingSpec>() || !template.HasSpec<PlaceableBlockObjectSpec>())
            throw new ArgumentException("unsupported_template");
        if (request.GenericBuilding) template = catalog.Resolve(request.Template);
        var spec = template.GetSpec<BlockObjectSpec>();
        var building = template.GetSpec<BuildingSpec>();
        var rotation = new[] { Orientation.Cw0, Orientation.Cw90, Orientation.Cw180, Orientation.Cw270 }[request.Rotation];
        var placement = new Placement(new Vector3Int(request.X, request.Y, request.Z), rotation, FlipMode.Unflipped);
        var geometry = spec.GetBlocks();
        var positioned = PositionedBlocks.From(geometry, placement);
        var occupied = positioned.GetOccupiedAndUndergroundBlocks().Take(65).ToArray();
        if (occupied.Length == 0 || occupied.Length > 64) throw new ArgumentException("unsupported_geometry");
        var reasons = new HashSet<string>();
        if (!request.GenericBuilding && !Compatible(request.Template)) reasons.Add("wrong_faction");
        if (!template.UsableWithCurrentFeatureToggles || !template.GetSpec<PlaceableBlockObjectSpec>().UsableWithCurrentFeatureToggles)
            reasons.Add("template_disabled");
        if (!unlocks.Unlocked(building)) reasons.Add("template_locked");
        var cells = occupied.Select(block =>
        {
            var p = block.Coordinates;
            bool inside = terrain.Contains(p) && blocks.Contains(p);
            bool underground = inside && terrain.Underground(p);
            bool onGround = inside && terrain.OnGround(p);
            bool intersects = inside && blocks.GetObjectsAt(p).Any(other => !other.IsPreview && other.IsIntersecting(block));
            if (!inside) reasons.Add("outside_map");
            if (intersects) reasons.Add("object_intersection");
            if (underground && !block.Underground && !block.OptionallyUnderground) reasons.Add("terrain_intersection");
            if (inside && block.MatterBelow == MatterBelow.Ground && !onGround) reasons.Add("required_ground_missing");
            return new { position = Vec(p), insideMap = inside, underground, onGround, intersectsObject = intersects,
                supportRule = block.MatterBelow.ToString() };
        }).ToArray();
        Vector3Int? entrance = null;
        if (spec.Entrance.HasEntrance) entrance = PositionedEntrance.From(geometry, spec.Entrance, placement).Coordinates;
        // GetPathObjectAt describes an occupation layer, not an actual road template.
        var entranceObjects = entrance.HasValue && blocks.Contains(entrance.Value)
            ? blocks.GetObjectsAt(entrance.Value).Where(o => !o.IsPreview).ToArray() : Array.Empty<BlockObject>();
        bool? pathAtEntrance = entrance.HasValue ? entranceObjects.Any(o => o.IsFinished &&
            o.TryGetComponent<TemplateSpec>(out var t) && t.TemplateName == "Path") : (bool?)null;
        var entranceOccupants = entranceObjects.Select(o => o.TryGetComponent<TemplateSpec>(out var t)
            ? t.TemplateName : "unknown").Distinct().OrderBy(n => n, StringComparer.Ordinal).Take(32).ToArray();
        var costs = Costs(building);
        return new { template = request.Template, origin = Vec(placement.Coordinates), rotation = request.Rotation,
            assessment = reasons.Count > 0 ? "blocked" : "requires_game_validation", gameValidated = false,
            reasons = reasons.OrderBy(r => r).ToArray(), cells, entrance = entrance.HasValue ? Vec(entrance.Value) : null,
            pathAtEntrance, entranceOccupants, costs,
            limitations = new[] { "not_full_game_validator", "no_preview_or_entity_created", "no_district_or_builder_reachability",
                "path_at_entrance_means_finished_Path_template_only", "entrance_occupants_are_not_traversability", "stairs_and_special_paths_not_classified_as_Path",
                "stackable_support_not_validated", "no_water_or_hazard_safety_verdict", "global_stock_not_local_delivery" } };
    }

    private bool Compatible(string name) => name == "Path" || faction.Current.Id == "Folktails";
    private object[] Costs(BuildingSpec building) => building.BuildingCost.Select(c => (object)new
    { id = c.Id, required = c.Amount, availableGlobally = resources.GetGlobalResourceCount(c.Id).AvailableStock }).ToArray();
    private static object Vec(Vector3Int p) => new { x = p.x, y = p.y, z = p.z };
}
