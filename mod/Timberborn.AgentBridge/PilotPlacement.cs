using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

public sealed class PilotPlacement(SiteValidation validation, TemplateNameMapper templates,
    BlockObjectPlacerService placers, EntityRegistry entities, IBlockService blocks, BuildingCatalog catalog)
{
    private readonly SinglePlacementGate gate = new();
    private readonly BuildingActionGate buildingGate = new();

    public object Place(BridgeRequest request) => request.GenericBuilding
        ? buildingGate.Execute(Guid.Parse(request.EntityId), () => PlaceOnce(request))
        : gate.Execute(() => PlaceOnce(request));

    private object PlaceOnce(BridgeRequest request)
    {
        if (!request.GenericBuilding && request.Template is not ("Path" or "Lodge.Folktails")) throw new ArgumentException("invalid_template");
        var position = new Vector3Int(request.X, request.Y, request.Z);
        var id = request.GenericBuilding ? Guid.Parse(request.EntityId) : Guid.NewGuid();
        if (entities.Entities.Any(e => e.EntityId == id)) throw new ArgumentException("entity_id_exists");
        object Result(string outcome, bool? finished = null) => new
        {
            template = request.Template, origin = new { x = request.X, y = request.Y, z = request.Z },
            rotation = request.Rotation, entityId = id, outcome, finished, sessionLocked = !request.GenericBuilding,
            limitations = new[] { request.GenericBuilding ? "action_id_consumed_256_attempts_per_session" : "one_attempt_per_session", "no_automatic_retry", "no_district_reachability_guarantee",
                "entity_id_correlation_requires_live_verification", "read_find_buildings_to_reconcile" }
        };
        // Reject replacement of existing paths or structures, including overridable ones.
        var template = request.GenericBuilding ? catalog.Resolve(request.Template) : templates.GetTemplate(request.Template);
        if (!request.GenericBuilding && request.Template == "Lodge.Folktails" && template.GetSpec<BuildingSpec>().PlaceFinished)
            return Result("rejected");
        var spec = template.GetSpec<BlockObjectSpec>();
        var rotation = new[] { Orientation.Cw0, Orientation.Cw90, Orientation.Cw180, Orientation.Cw270 }[request.Rotation];
        var placement = new Placement(position, rotation, FlipMode.Unflipped);
        var footprint = spec.GetBlocks(placement).Take(65).ToArray();
        if ((request.GenericBuilding ? footprint.Length is < 1 or > 64 : footprint.Length != (request.Template == "Path" ? 1 : 4)) ||
            footprint.Any(cell => !blocks.Contains(cell.Coordinates) || blocks.GetObjectsAt(cell.Coordinates).Any(b => !b.IsPreview && (!request.GenericBuilding || b.IsIntersecting(cell)))))
            return Result("rejected");
        validation.Validate(request, out var allowed);
        if (!allowed) return Result("rejected");
        var placer = placers.GetMatchingPlacer(spec);
        var setup = new EntitySetup.Builder(template.Blueprint).SetId(id);
        try
        {
            // Exactly one normal game placement. No forced completion, entity deletion or rollback.
            placer.Place(setup, placement);
            var entity = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId == id);
            if (entity is null || !entity.Initialized || !entity.TryGetComponent<BlockObject>(out var block) ||
                block.IsPreview || block.Coordinates != position || block.Orientation != rotation ||
                !entity.TryGetComponent<TemplateSpec>(out var actual) || actual.TemplateName != request.Template)
                return Result("unconfirmed");
            return Result("applied", block.IsFinished);
        }
        catch { return Result("unconfirmed"); }
    }
}
