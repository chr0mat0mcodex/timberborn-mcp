using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.BuildingsReachability;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Navigation;
using Timberborn.TemplateSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

public sealed class BuildingProjectExecution(SiteValidation validation, RoadProtection roads,
    BuildingCatalog catalog, BlockObjectPlacerService placers, EntityRegistry entities,
    IBlockService blocks, SpeedManager speed)
{
    private readonly BuildingProjectController controller = new(capacity: 4);
    private readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    public void Update() => controller.Tick(clock.Elapsed.TotalSeconds);
    private static JObject Payload(BuildingProjectController.Receipt receipt) => JObject.FromObject(receipt,
        JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    public object Handle(BuildingProjectExecutionRequest request)
    {
        if (request.Validation is null) return Payload(controller.Inspect(request.ActionId));
        var r = request.Validation; var p = r.Plan;
        string fingerprint = string.Join("|", p.Session, p.Template, p.DistrictId, p.X, p.Y, p.Z,
            p.Width, p.Height, p.Rotation, r.OptionIndex, r.PlanKey);
        var existing = controller.Existing(request.ActionId, fingerprint);
        if (existing is not null) return Payload(existing);
        if (speed.CurrentSpeed != 0 || !ConstructionIsolation.Allows(entities) || entities.Entities.Any(e => e.EntityId == request.ActionId))
            throw new BridgeRejectionException("state_conflict");
        // Fresh joint game preview is mandatory. Only this one explicitly authorized
        // construction-phase evidence gap is waived, never geometry or known losses.
        var evidence = JObject.FromObject(validation.ValidateProject(r));
        var protection = evidence["roadProtection"]!;
        var construction = protection["constructionAccessPreview"]!.ToObject<ConstructionAccessPreview.Report>()!;
        if (construction.HasKnownFailure()) throw new BridgeRejectionException("state_conflict");
        var option = evidence["option"]!;
        var cells = (JArray)option["newRoadCells"]!;
        if (!BuildingProjectPilotPolicy.Allows(cells.Count, (bool)evidence["buildingValid"]!,
            (bool)evidence["previewEntranceConnected"]!, (bool)evidence["noPersistentChangeObserved"]!, (bool)evidence["sessionLocked"]!,
            ((JArray)evidence["roadValid"]!).Values<bool>().ToArray(), ((JArray)evidence["roadStepLostConnections"]!).Values<int>().ToArray(),
            (string)protection["status"]!, (bool)protection["restored"]!, (int)protection["lostConnections"]!,
            ((JArray)protection["reasons"]!).Values<string>().Select(s => s!).ToArray()))
            throw new BridgeRejectionException("state_conflict");
        var baseline = roads.Capture();
        if (!baseline.Complete) throw new BridgeRejectionException("state_conflict");
        var district = entities.Entities.Single(e => !e.Deleted && e.EntityId.ToString("D") == p.DistrictId).GetComponent<DistrictCenter>();
        BuildingProjectController.Step Step(JToken cell, string template, Guid id, int rotation) => new() {
            EntityId = id.ToString("D"), Template = template, X = (int)cell["x"]!, Y = (int)cell["y"]!, Z = (int)cell["z"]!, Rotation = rotation };
        var steps = cells.Select(c => Step(c, "Path", Guid.NewGuid(), 0)).Concat(new[] {
            Step(option["origin"]!, p.Template, request.ActionId, p.Rotation) }).ToArray();
        bool Guard() => speed.CurrentSpeed == 0 &&
            ConstructionIsolation.Allows(entities, steps.Where(s => s.State != "pending").Select(s => s.EntityId)) &&
            RoadProtectionPolicy.LostConnections(baseline.Before, roads.Read(baseline, false)).Length == 0 &&
            steps.Where(s => s.State == "confirmed").All(s => Confirm(s, district) == "confirmed");
        return Payload(controller.Start(request.ActionId, fingerprint, r.PlanKey, steps, Guard, step => Place(step, request.Session),
            step => Confirm(step, district)));
    }
    private void Place(BuildingProjectController.Step step, string session)
    {
        // Confirmed paths are already finished; no independent or prior open site
        // may exist at the next mutation boundary, even when preview says unknown.
        if (!ConstructionIsolation.Allows(entities)) throw new InvalidOperationException();
        // Recheck the current game geometry and known road losses immediately before
        // each mutation; user edits between frames must not bypass validation.
        var query = new System.Collections.Specialized.NameValueCollection {
            ["template"] = step.Template, ["x"] = step.X.ToString(), ["y"] = step.Y.ToString(), ["z"] = step.Z.ToString(),
            ["rotation"] = step.Rotation.ToString(), ["session"] = session };
        var check = JObject.FromObject(validation.Validate(BridgeRequest.Parse("/agent-api/v1/building-validation", query), out _, out var protection));
        if ((bool?)check["valid"] != true || protection.status != "unknown" || !protection.restored ||
            protection.lostConnections != 0 || protection.constructionAccessPreview.HasKnownFailure() ||
            !protection.reasons.SequenceEqual(new[] { "construction_and_road_node_coverage_unproven" }))
            throw new InvalidOperationException();
        var template = catalog.Resolve(step.Template);
        var spec = template.GetSpec<BlockObjectSpec>();
        var placement = new Placement(Position(step), Rotation(step), FlipMode.Unflipped);
        var footprint = spec.GetBlocks(placement).Take(65).ToArray();
        if (footprint.Length is < 1 or > 64 || entities.Entities.Any(e => e.EntityId.ToString("D") == step.EntityId) ||
            footprint.Any(c => !blocks.Contains(c.Coordinates) || blocks.GetObjectsAt(c.Coordinates).Any(b => !b.IsPreview && b.IsIntersecting(c))))
            throw new InvalidOperationException();
        placers.GetMatchingPlacer(spec).Place(new EntitySetup.Builder(template.Blueprint).SetId(Guid.Parse(step.EntityId)), placement);
    }
    private string Confirm(BuildingProjectController.Step step, DistrictCenter district)
    {
        var entity = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId.ToString("D") == step.EntityId);
        if (entity is null || !entity.Initialized) return "pending";
        if (!entity.TryGetComponent<BlockObject>(out var block) || block.IsPreview || block.Coordinates != Position(step) ||
            block.Orientation != Rotation(step) || !entity.TryGetComponent<TemplateSpec>(out var actual) || actual.TemplateName != step.Template)
            return "mismatch";
        if (step.Template == "Path") return block.IsFinished && district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(Position(step))) ? "confirmed" : "pending";
        if (!block.IsUnfinished) return "mismatch";
        return block.HasEntrance && district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(block.PositionedEntrance.Coordinates)) &&
            entity.TryGetComponent<ReachableConstructionSite>(out var site) && site.IsReachableByBuilders() ? "confirmed" : "pending";
    }
    private static Vector3Int Position(BuildingProjectController.Step s) => new(s.X, s.Y, s.Z);
    private static Orientation Rotation(BuildingProjectController.Step s) => new[] { Orientation.Cw0, Orientation.Cw90, Orientation.Cw180, Orientation.Cw270 }[s.Rotation];
}
