using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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

// Fixed vertical projects; the warehouse mode adds a third support and an inward-facing entrance.
public sealed class VerticalStairExecution(SiteValidation validation, BuildingCatalog catalog, BlockObjectPlacerService placers,
    EntityRegistry entities, IBlockService blocks, SpeedManager speed, RoadProtection roads)
{
    private readonly BuildingProjectController controller = new(capacity: VerticalStairRequest.MaxProjectsPerSession);
    private readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    private static object Payload(BuildingProjectController.Receipt receipt) => JObject.FromObject(receipt,
        JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    public void Update() => controller.Tick(clock.Elapsed.TotalSeconds);
    public object Handle(VerticalStairRequest request)
    {
        if (!request.Start) return Payload(controller.Inspect(request.ActionId));
        string fingerprint = string.Join("|", request.Session, request.DistrictId, request.X, request.Y, request.Z, request.Rotation, request.UpperPathCount, request.WithPlatform, request.WithWarehouse);
        var existing = controller.Existing(request.ActionId, fingerprint); if (existing is not null) return Payload(existing);
        if (speed.CurrentSpeed != 0 || !ConstructionIsolation.Allows(entities) || entities.Entities.Any(e => e.EntityId == request.ActionId)) throw new BridgeRejectionException("state_conflict");
        var districtEntity = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId.ToString("D") == request.DistrictId)
            ?? throw new BridgeRejectionException("state_conflict");
        var district = districtEntity.GetComponent<DistrictCenter>() ?? throw new BridgeRejectionException("state_conflict");
        var steps = new List<BuildingProjectController.Step> { new() { EntityId = request.ActionId.ToString("D"), Template = "Stairs.Folktails", X = request.X, Y = request.Y, Z = request.Z, Rotation = request.Rotation } };
        var upper = UpperConnection(request);
        for (int i = 0; i < request.UpperPathCount; i++)
        {
            if (request.WithPlatform)
                steps.Add(new BuildingProjectController.Step { EntityId = DerivedId(request.ActionId, 3 + i).ToString("D"), Template = "Platform.Folktails", X = upper.x + upper.dx * i, Y = upper.y + upper.dy * i, Z = upper.z - 1, Rotation = 0 });
            steps.Add(new BuildingProjectController.Step { EntityId = DerivedId(request.ActionId, i + 1).ToString("D"), Template = "Path", X = upper.x + upper.dx * i, Y = upper.y + upper.dy * i, Z = upper.z, Rotation = 0 });
        }
        if (request.WithWarehouse)
        {
            steps.Add(new BuildingProjectController.Step { EntityId = DerivedId(request.ActionId, 5).ToString("D"), Template = "Platform.Folktails", X = upper.x + upper.dx * 2, Y = upper.y + upper.dy * 2, Z = upper.z - 1, Rotation = 0 });
            steps.Add(new BuildingProjectController.Step { EntityId = DerivedId(request.ActionId, 6).ToString("D"), Template = "SmallWarehouse.Folktails", X = upper.x + upper.dx * 2, Y = upper.y + upper.dy * 2, Z = upper.z, Rotation = (request.Rotation + 2) % 4 });
        }
        var baseline = request.WithWarehouse ? roads.Capture() : null;
        if (baseline is not null && !baseline.Complete) throw new BridgeRejectionException("state_conflict");
        if (steps.Any(s => s.X is < 0 or > 4095 || s.Y is < 0 or > 4095 || s.Z is < 0 or > 4095 || entities.Entities.Any(e => e.EntityId.ToString("D") == s.EntityId)))
            throw new BridgeRejectionException("state_conflict");
        // Check every target against current real objects before consuming the pilot.
        // Support can depend on preceding steps, so full game validation stays per-step.
        foreach (var step in steps)
        {
            var spec = catalog.Resolve(step.Template).GetSpec<BlockObjectSpec>();
            var footprint = spec.GetBlocks(new Placement(new(step.X, step.Y, step.Z), Rotation(step), FlipMode.Unflipped)).Take(65).ToArray();
            int expectedBlocks = step.Template == "Stairs.Folktails" ? 2 : 1;
            if (footprint.Length != expectedBlocks || footprint.Any(c => !blocks.Contains(c.Coordinates) ||
                blocks.GetObjectsAt(c.Coordinates).Any(b => !b.IsPreview && b.IsIntersecting(c))))
                throw new BridgeRejectionException("state_conflict");
        }
        void Place(BuildingProjectController.Step s)
        {
            if (!ConstructionIsolation.Allows(entities)) throw new InvalidOperationException();
            var query = new System.Collections.Specialized.NameValueCollection { ["template"] = s.Template, ["x"] = s.X.ToString(), ["y"] = s.Y.ToString(), ["z"] = s.Z.ToString(), ["rotation"] = s.Rotation.ToString(), ["session"] = request.Session };
            var evidence = JObject.FromObject(validation.Validate(BridgeRequest.Parse("/agent-api/v1/building-validation", query), out _, out var safety));
            if ((bool?)evidence["valid"] != true || safety.status != "unknown" || !safety.restored || safety.lostConnections != 0 || safety.constructionAccessPreview.HasKnownFailure() || !safety.reasons.SequenceEqual(new[] { "construction_and_road_node_coverage_unproven" })) throw new InvalidOperationException();
            var spec = catalog.Resolve(s.Template).GetSpec<BlockObjectSpec>(); var placement = new Placement(new(s.X, s.Y, s.Z), Rotation(s), FlipMode.Unflipped);
            var footprint = spec.GetBlocks(placement).Take(65).ToArray();
            int expectedBlocks = s.Template == "Stairs.Folktails" ? 2 : 1;
            if (footprint.Length != expectedBlocks || footprint.Any(c => !blocks.Contains(c.Coordinates) || blocks.GetObjectsAt(c.Coordinates).Any(b => !b.IsPreview && b.IsIntersecting(c)))) throw new InvalidOperationException();
            placers.GetMatchingPlacer(spec).Place(new EntitySetup.Builder(catalog.Resolve(s.Template).Blueprint).SetId(Guid.Parse(s.EntityId)), placement);
        }
        string Confirm(BuildingProjectController.Step s)
        {
            var entity = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId.ToString("D") == s.EntityId);
            if (entity is null || !entity.Initialized) return "pending";
            if (!entity.TryGetComponent<BlockObject>(out var block) || block.IsPreview || block.Coordinates != new Vector3Int(s.X,s.Y,s.Z) || block.Orientation != Rotation(s) || !entity.TryGetComponent<TemplateSpec>(out var t) || t.TemplateName != s.Template) return "mismatch";
            if (s.Template == "SmallWarehouse.Folktails" && (!block.HasEntrance ||
                !district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(block.PositionedEntrance.Coordinates)))) return "pending";
            if (block.IsFinished) return "confirmed";
            return block.IsUnfinished && entity.TryGetComponent<ReachableConstructionSite>(out var site) && site.IsReachableByBuilders() ? "confirmed" : "pending";
        }
        Vector3Int lowerConnection = request.Rotation switch {
            0 => new(request.X, request.Y + 1, request.Z), 1 => new(request.X + 1, request.Y, request.Z),
            2 => new(request.X, request.Y - 1, request.Z), _ => new(request.X - 1, request.Y, request.Z) };
        string Ready(BuildingProjectController.Step current)
        {
            bool unfinished = false;
            foreach (var previous in steps.TakeWhile(s => s != current))
            {
                var entity = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId.ToString("D") == previous.EntityId);
                if (entity is null || !entity.Initialized || !entity.TryGetComponent<BlockObject>(out var block) ||
                    block.IsPreview || block.Coordinates != new Vector3Int(previous.X, previous.Y, previous.Z) ||
                    block.Orientation != Rotation(previous) || !entity.TryGetComponent<TemplateSpec>(out var template) ||
                    template.TemplateName != previous.Template) return "mismatch";
                if (!block.IsFinished)
                {
                    if (!block.IsUnfinished) return "mismatch";
                    unfinished = true;
                }
            }
            if (unfinished) return "construction";
            if (current.Template == "SmallWarehouse.Folktails" && steps.Where(s => s.Template == "Path").Any(s =>
                !district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(new Vector3Int(s.X, s.Y, s.Z))))) return "mismatch";
            return speed.CurrentSpeed == 0 ? "ready" : "pause";
        }
        return Payload(controller.Start(request.ActionId, fingerprint, Hash(fingerprint), steps.ToArray(), () =>
            ConstructionIsolation.Allows(entities, steps.Where(s => s.State != "pending").Select(s => s.EntityId)) && !districtEntity.Deleted &&
            (baseline is null || RoadProtectionPolicy.LostConnections(baseline.Before, roads.Read(baseline, false)).Length == 0) &&
            district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(lowerConnection)), Place, Confirm,
            Ready));
    }
    private static string Hash(string text)
    {
        using var hash = System.Security.Cryptography.SHA256.Create();
        return BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
    }
    private static Orientation Rotation(BuildingProjectController.Step s) => new[] { Orientation.Cw0, Orientation.Cw90, Orientation.Cw180, Orientation.Cw270 }[s.Rotation];
    private static (int x, int y, int z, int dx, int dy) UpperConnection(VerticalStairRequest r) => r.Rotation switch {
        0 => (r.X, r.Y - 1, r.Z + 1, 0, -1), 1 => (r.X - 1, r.Y, r.Z + 1, -1, 0),
        2 => (r.X, r.Y + 1, r.Z + 1, 0, 1), _ => (r.X + 1, r.Y, r.Z + 1, 1, 0) };
    private static Guid DerivedId(Guid actionId, int ordinal)
    {
        using var hash = System.Security.Cryptography.SHA256.Create();
        return new Guid(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(actionId.ToString("D") + "|upper-path|" + ordinal)).Take(16).ToArray());
    }
}
