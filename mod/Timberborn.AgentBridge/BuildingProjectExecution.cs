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
    SpeedManager speed, BuildingSettings settings)
{
    private readonly BuildingProjectController controller = new(capacity: BuildingProjectController.MaxProjectsPerSession);
    private readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    private readonly List<(BuildingProjectController.Receipt Receipt, string Session)> configurations = new();
    public void Update()
    {
        controller.Tick(clock.Elapsed.TotalSeconds);
        foreach (var item in configurations.ToArray())
        {
            var configuration = item.Receipt.InitialConfiguration!;
            var query = new System.Collections.Specialized.NameValueCollection {
                ["id"] = item.Receipt.ActionId, ["session"] = item.Session };
            configuration.Tick(item.Receipt.State == "ordered" ? "completed" : item.Receipt.State,
                () => settings.ReadInitialStorage(BuildingSettingsRequest.Parse("/agent-api/v1/building-settings", query)),
                (key, expected, value) => {
                    var q = new System.Collections.Specialized.NameValueCollection(query);
                    q.Add(key, value); q.Add(key == "good" ? "expectedGood" : "expectedMode", expected);
                    var result = JObject.FromObject(settings.Set(BuildingSettingsRequest.Parse("/agent-api/v1/set-storage-" + key, q), true));
                    if ((string?)result["outcome"] != "applied") throw new InvalidOperationException();
                });
            if (configuration.Terminal) configurations.Remove(item);
        }
    }
    private static JObject Payload(BuildingProjectController.Receipt receipt) => JObject.FromObject(receipt,
        JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    public object Handle(BuildingProjectExecutionRequest request)
    {
        if (request.Validation is null) return Payload(controller.Inspect(request.ActionId));
        var r = request.Validation; var p = r.Plan;
        string fingerprint = string.Join("|", p.Session, p.Template, p.DistrictId, p.X, p.Y, p.Z,
            p.Width, p.Height, p.Rotation, r.OptionIndex, r.PlanKey,
            request.InitialStorageGood ?? "<absent>", request.InitialStorageMode ?? "<absent>");
        var existing = controller.Existing(request.ActionId, fingerprint);
        if (existing is not null) return Payload(existing);
        if (request.HasInitialConfiguration)
            settings.ValidateInitialStorage(catalog.Resolve(p.Template), request.InitialStorageGood, request.InitialStorageMode);
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
        bool Guard() =>
            ConstructionIsolation.Allows(entities, steps.Where(s => s.State != "pending").Select(s => s.EntityId), allowOwnedInitialization: true) &&
            RoadProtectionPolicy.LostConnections(baseline.Before, roads.Read(baseline, false)).Length == 0;
        var receipt = controller.Start(request.ActionId, fingerprint, r.PlanKey, steps, Guard, step => Place(step),
            step => Confirm(step, district));
        if (request.HasInitialConfiguration) {
            receipt.InitialConfiguration = new InitialStorageConfiguration { Good = request.InitialStorageGood, Mode = request.InitialStorageMode };
            configurations.Add((receipt, request.Session));
        }
        controller.Tick(clock.Elapsed.TotalSeconds);
        return Payload(receipt);
    }
    private void Place(BuildingProjectController.Step step)
    {
        var template = catalog.Resolve(step.Template);
        var spec = template.GetSpec<BlockObjectSpec>();
        var placement = new Placement(Position(step), Rotation(step), FlipMode.Unflipped);
        placers.GetMatchingPlacer(spec).Place(new EntitySetup.Builder(template.Blueprint).SetId(Guid.Parse(step.EntityId)), placement);
    }
    private string Confirm(BuildingProjectController.Step step, DistrictCenter district)
    {
        var entity = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId.ToString("D") == step.EntityId);
        if (entity is null || !entity.Initialized) return "pending";
        if (!entity.TryGetComponent<BlockObject>(out var block) || block.IsPreview || block.Coordinates != Position(step) ||
            block.Orientation != Rotation(step) || !entity.TryGetComponent<TemplateSpec>(out var actual) || actual.TemplateName != step.Template)
            return "mismatch";
        if (step.Template == "Path") return block.IsFinished && district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(Position(step))) ? "finished" : "pending";
        if (!block.HasEntrance || !district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(block.PositionedEntrance.Coordinates)))
            return "pending";
        // Some catalogue objects are legitimately placed finished. Never demand
        // a construction-site component for those, or mark construction finished.
        if (block.IsFinished) return "finished";
        return block.IsUnfinished && entity.TryGetComponent<ReachableConstructionSite>(out var site) &&
            site.IsReachableByBuilders() ? "confirmed" : "pending";
    }
    private static Vector3Int Position(BuildingProjectController.Step s) => new(s.X, s.Y, s.Z);
    private static Orientation Rotation(BuildingProjectController.Step s) => new[] { Orientation.Cw0, Orientation.Cw90, Orientation.Cw180, Orientation.Cw270 }[s.Rotation];
}
