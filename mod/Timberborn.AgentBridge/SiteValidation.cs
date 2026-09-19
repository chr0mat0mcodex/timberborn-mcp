using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.GameFactionSystem;
using Timberborn.Goods;
using Timberborn.ResourceCountingSystem;
using Timberborn.ScienceSystem;
using Timberborn.TemplateSystem;
using Timberborn.TerrainSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

// PreviewFactory uses the game's preview instantiation path, not EntityService placement.
// Keep at most two hidden previews owned by the game scene, as established mod patterns do.
public sealed class SiteValidation(PreviewFactory factory, BlockObjectValidationService validators,
    TemplateNameMapper templates, FactionService faction, BuildingUnlockingService unlocks,
    EntityRegistry entities, ResourceCountingService resources, IGoodService goods, ITerrainService terrain)
{
    private readonly Dictionary<string, Preview> previews = new();
    private bool faulted;
    private int attempts;

    public object Validate(BridgeRequest request)
    {
        if (faulted || attempts >= 8) throw new InvalidOperationException("validation_session_locked");
        if (request.Template is not ("Lodge.Folktails" or "Path") ||
            (request.Template == "Lodge.Folktails" && faction.Current.Id != "Folktails")) throw new ArgumentException("invalid_template");
        var template = templates.GetTemplate(request.Template);
        var building = template.GetSpec<BuildingSpec>();
        var placeable = template.GetSpec<PlaceableBlockObjectSpec>();
        var spec = template.GetSpec<BlockObjectSpec>();
        if (!template.UsableWithCurrentFeatureToggles || !placeable.UsableWithCurrentFeatureToggles || !unlocks.Unlocked(building))
            throw new ArgumentException("template_unavailable");
        var rotation = new[] { Orientation.Cw0, Orientation.Cw90, Orientation.Cw180, Orientation.Cw270 }[request.Rotation];
        var placement = new Placement(new Vector3Int(request.X, request.Y, request.Z), rotation, FlipMode.Unflipped);
        var cells = spec.GetBlocks(placement).Take(65).ToArray();
        if (cells.Length == 0 || cells.Length > 64 || cells.Any(c => !terrain.Contains(c.Coordinates)))
            throw new ArgumentException("invalid_region");
        var beforeIds = RegisteredIds();
        var beforeStock = Stocks();
        Preview? preview = null;
        bool valid = false;
        attempts++;
        try
        {
            if (!previews.TryGetValue(request.Template, out preview))
            {
                preview = factory.Create(placeable);
                previews.Add(request.Template, preview);
            }
            preview.Hide();
            preview.Reposition(placement);
            if (!preview.BlockObject.IsPreview || preview.BlockObject.AddedToService)
                throw new InvalidOperationException("unexpected_preview_state");
            valid = validators.AreValid(new BaseComponent[] { preview.BlockObject }, out _);
        }
        catch { faulted = true; throw; }
        finally
        {
            if (preview is not null)
            {
                try { preview.Hide(); preview.RemoveFromPreviewServices(); }
                catch { faulted = true; throw; }
            }
        }
        bool unchanged = beforeIds.SetEquals(RegisteredIds()) && beforeStock.SequenceEqual(Stocks());
        if (!unchanged) faulted = true;
        return new { template = request.Template, origin = new { x = request.X, y = request.Y, z = request.Z },
            rotation = request.Rotation, gameValidated = true, valid = unchanged ? (bool?)valid : null,
            noPersistentChangeObserved = unchanged, sessionLocked = faulted, attemptsRemaining = 8 - attempts,
            limitations = new[] { "preview_validation_not_placement", "no_build_order_created", "no_material_delivery_or_completion_guarantee",
                "registered_entity_ids_and_global_stock_checked_not_all_game_state", "hidden_preview_cached_until_scene_unload" } };
    }

    private HashSet<Guid> RegisteredIds() => new(entities.Entities.Where(e => !e.Deleted).Select(e => e.EntityId));
    private int[] Stocks() => goods.Goods.OrderBy(id => id, StringComparer.Ordinal)
        .Select(id => resources.GetGlobalResourceCount(id).AllStock).ToArray();
}
