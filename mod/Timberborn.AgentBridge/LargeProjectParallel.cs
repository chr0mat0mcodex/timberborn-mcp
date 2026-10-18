using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.BuildingsReachability;
using Timberborn.EntitySystem;
using Timberborn.Navigation;
using Timberborn.PathSystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    private sealed class ProtectedSite
    {
        public EntityComponent Entity = null!;
        public string Template = "";
        public Vector3Int Position;
        public string Orientation = "";
        public Vector3Int[] Shape = Array.Empty<Vector3Int>();
        public bool Reachable;
    }
    private readonly Dictionary<string, ProtectedSite[]> protectedSites = new();

    private void RequireKnownProjects(string except = "", bool repair = false)
    {
        foreach (var id in jobs.Keys.Where(id => id != except && !releasedProjects.Contains(id)))
            for (int i = 0; i < Ids(id).Length; i++)
            {
                var recorded = Recorded(id)[i]; var observed = IdentityObservation(id, i);
                if (repair ? !ParallelConstructionPolicy.KnownRepairPart(recorded, observed) :
                    !ParallelConstructionPolicy.KnownPart(recorded, observed))
                    throw new BridgeRejectionException(ParallelConstructionPolicy.KnownRepairPart(recorded, observed)
                        ? "project_plan_existing_access_lost" : "project_plan_unresolved_identity");
            }
    }

    private void RequireParallelInventory()
    {
        // Exempt only unfinished identity here, never initialization/unknown state.
        // Scan through EVERY foreign site, so an early one cannot hide a later invalid object.
        var unfinished = entities.Entities.Where(e=>!e.Deleted && e.TryGetComponent<BlockObject>(out var b) && !b.IsPreview && b.IsUnfinished)
            .Select(e=>e.EntityId.ToString("D")).ToArray();
        LargeProjectPlanningPolicy.RequireIsolation(ConstructionIsolation.Blocker(entities,unfinished));
    }

    private ProtectedSite[] CaptureProtectedSites(IEnumerable<Block> proposed, RoadProtection.Sample baseline, string[]? owned = null, bool repair = false)
    {
        var own = new HashSet<string>(owned ?? Array.Empty<string>());
        var sites = entities.Entities.Where(e => !e.Deleted && e.TryGetComponent<BlockObject>(out var b) &&
            !b.IsPreview && b.IsUnfinished && !own.Contains(e.EntityId.ToString("D"))).Take(33).ToArray();
        if (sites.Length == 0) return Array.Empty<ProtectedSite>();
        if (sites.Length > 32 || !roads.PreservesEveryConstructionAccess(baseline, false, repair))
            throw new BridgeRejectionException("project_plan_foreign_access_unknown");
        var cells = proposed.Select(b => new ParallelConstructionPolicy.Cell(b.Coordinates.x, b.Coordinates.y)).Distinct().ToArray();
        var result = new List<ProtectedSite>();
        foreach (var e in sites)
        {
            var b = e.GetComponent<BlockObject>();
            if (!e.Initialized || !e.TryGetComponent<TemplateSpec>(out var template))
                throw new BridgeRejectionException("project_plan_construction_state_unknown");
            var shape = b.PositionedBlocks.GetAllBlocks().Take(BuildingPolicy.MaxGeometryCells + 1).Select(c => c.Coordinates).ToArray();
            int index = Array.IndexOf(baseline.Construction.Sites, e);
            if (shape.Length is < 1 or > BuildingPolicy.MaxGeometryCells || index < 0)
                throw new BridgeRejectionException("project_plan_foreign_access_unknown");
            // A spill-only comparison is not sufficient: retain a native district-road
            // witness at an actual work access, also covered by every prefix road probe.
            var district=baseline.Construction.District!;
            if (!repair && !baseline.Construction.Accesses[index].Any(c=>district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(c))))
                throw new BridgeRejectionException("project_plan_foreign_access_unknown");
            var exclusion = shape.Concat(baseline.Construction.Accesses[index])
                .Select(c => new ParallelConstructionPolicy.Cell(c.x, c.y)).Distinct().ToArray();
            if (!repair && !ParallelConstructionPolicy.Separated(cells, exclusion))
                throw new BridgeRejectionException("project_plan_foreign_site_nearby");
            result.Add(new() { Entity=e, Template=template.TemplateName, Position=b.Coordinates,
                Orientation=b.Orientation.ToString(), Shape=shape, Reachable=baseline.Construction.Observed[index] });
        }
        return result.ToArray();
    }

    private IEnumerable<string> ManagedSites() => jobs.Keys.SelectMany(id => Ids(id).Where((_, i) => Recorded(id)[i] != "pending"));

    private bool ParallelGuard(string id)
    {
        if (jobs.Keys.Any(other=>other!=id && !releasedProjects.Contains(other) &&
            (State(other)=="unconfirmed" || TerminalProject(other) && Recorded(other).Any(s=>s=="unconfirmed")))) return false;
        var saved = protectedSites.TryGetValue(id, out var value) ? value : Array.Empty<ProtectedSite>();
        var admitted = ManagedSites().Concat(saved.Select(s => s.Entity.EntityId.ToString("D")));
        if (!ConstructionIsolation.Allows(entities, admitted, allowOwnedInitialization:true)) return false;
        foreach (var site in saved)
        {
            var e = site.Entity;
            if (e.Deleted || !e.Initialized || !e.TryGetComponent<BlockObject>(out var b) || b.IsPreview ||
                !e.TryGetComponent<TemplateSpec>(out var template) || template.TemplateName != site.Template ||
                b.Coordinates != site.Position || b.Orientation.ToString() != site.Orientation ||
                !b.PositionedBlocks.GetAllBlocks().Select(c => c.Coordinates).SequenceEqual(site.Shape)) return false;
            if (b.IsUnfinished)
            {
                if (!e.TryGetComponent<ReachableConstructionSite>(out var reachable)) return false;
                bool now = reachable.IsReachableByBuilders();
                if (!ParallelConstructionPolicy.RetainsReachability(repairParents.ContainsKey(id),site.Reachable,now)) return false;
                site.Reachable |= now; // A restored connection may not be lost again.
            }
            else if (b.IsFinished)
            {
                var district = jobs[id].Baseline.Construction.District;
                if (district is null || b.HasEntrance && !district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(b.PositionedEntrance.Coordinates))) return false;
                if (b.TryGetComponent<PathSpec>(out var path) && !district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(b.TransformCoordinates(path.MainPathCoordinates)))) return false;
            }
            else return false;
        }
        return true;
    }
}
