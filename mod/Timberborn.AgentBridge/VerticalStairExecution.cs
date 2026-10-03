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

// A deliberately narrow vertical project: one stair, then at most two upper path cells.
public sealed class VerticalStairExecution(SiteValidation validation, BuildingCatalog catalog, BlockObjectPlacerService placers,
    EntityRegistry entities, IBlockService blocks, SpeedManager speed)
{
    private readonly BuildingProjectController controller = new();
    private readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    private static object Payload(BuildingProjectController.Receipt receipt) => JObject.FromObject(receipt,
        JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    public void Update() => controller.Tick(clock.Elapsed.TotalSeconds);
    public object Handle(VerticalStairRequest request)
    {
        if (!request.Start) return Payload(controller.Inspect(request.ActionId));
        string fingerprint = string.Join("|", request.Session, request.DistrictId, request.X, request.Y, request.Z, request.Rotation, request.UpperPathCount);
        var existing = controller.Existing(request.ActionId, fingerprint); if (existing is not null) return Payload(existing);
        if (speed.CurrentSpeed != 0 || entities.Entities.Any(e => e.EntityId == request.ActionId)) throw new BridgeRejectionException("state_conflict");
        var district = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId.ToString("D") == request.DistrictId)?.GetComponent<DistrictCenter>()
            ?? throw new BridgeRejectionException("state_conflict");
        var steps = new List<BuildingProjectController.Step> { new() { EntityId = request.ActionId.ToString("D"), Template = "Stairs.Folktails", X = request.X, Y = request.Y, Z = request.Z, Rotation = request.Rotation } };
        var upper = UpperConnection(request);
        for (int i = 0; i < request.UpperPathCount; i++)
            steps.Add(new BuildingProjectController.Step { EntityId = DerivedId(request.ActionId, i + 1).ToString("D"), Template = "Path", X = upper.x + upper.dx * i, Y = upper.y + upper.dy * i, Z = upper.z, Rotation = 0 });
        void Place(BuildingProjectController.Step s)
        {
            var query = new System.Collections.Specialized.NameValueCollection { ["template"] = s.Template, ["x"] = s.X.ToString(), ["y"] = s.Y.ToString(), ["z"] = s.Z.ToString(), ["rotation"] = s.Rotation.ToString(), ["session"] = request.Session };
            var evidence = JObject.FromObject(validation.Validate(BridgeRequest.Parse("/agent-api/v1/building-validation", query), out _, out var safety));
            if ((bool?)evidence["valid"] != true || safety.status != "unknown" || !safety.restored || safety.lostConnections != 0 || !safety.reasons.SequenceEqual(new[] { "construction_and_road_node_coverage_unproven" })) throw new InvalidOperationException();
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
            if (s.Template == "Path" && block.IsFinished) return "confirmed";
            return block.IsUnfinished && entity.TryGetComponent<ReachableConstructionSite>(out var site) && site.IsReachableByBuilders() ? "confirmed" : "pending";
        }
        Vector3Int lowerConnection = request.Rotation switch {
            0 => new(request.X, request.Y + 1, request.Z), 1 => new(request.X + 1, request.Y, request.Z),
            2 => new(request.X, request.Y - 1, request.Z), _ => new(request.X - 1, request.Y, request.Z) };
        return Payload(controller.Start(request.ActionId, fingerprint, Hash(fingerprint), steps.ToArray(), () => speed.CurrentSpeed == 0 &&
            district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(lowerConnection)), Place, Confirm));
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
