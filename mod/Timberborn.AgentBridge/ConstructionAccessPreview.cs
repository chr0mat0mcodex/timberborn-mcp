using Timberborn.BlockSystem;
using Timberborn.BuildingsNavigation;
using Timberborn.BuildingsReachability;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Navigation;
using UnityEngine;
namespace Timberborn.AgentBridge;

// Public range comparison experiment. Not a hypothetical construction-state model.
public sealed class ConstructionAccessPreview(INavigationRangeService ranges, INavMeshService navMesh, IBlockService blocks,
    INavigationService navigation, IDistrictService districtService)
{
    public sealed class AccessState
    {
        public bool rangeContains, connected, roadConnected, existingOccupied;
        public bool candidateCoversCell, candidateOccupied;
        public string candidateOccupation = "None";
        public bool originOnActualNavMesh, targetOnActualNavMesh;
        public int originNeighbourRoadEdges;
        public bool actualRoadPathFound, actualDestinationReachable, actualDistrictRoadSpill;
        public bool Same(AccessState other) => rangeContains == other.rangeContains && connected == other.connected &&
            roadConnected == other.roadConnected && existingOccupied == other.existingOccupied &&
            candidateCoversCell == other.candidateCoversCell && candidateOccupied == other.candidateOccupied &&
            candidateOccupation == other.candidateOccupation && originOnActualNavMesh == other.originOnActualNavMesh &&
            targetOnActualNavMesh == other.targetOnActualNavMesh && originNeighbourRoadEdges == other.originNeighbourRoadEdges &&
            actualRoadPathFound == other.actualRoadPathFound && actualDestinationReachable == other.actualDestinationReachable &&
            actualDistrictRoadSpill == other.actualDistrictRoadSpill;
    }
    public sealed class AccessDetail
    {
        public object cell = null!;
        public AccessState before = null!, previewBefore = null!, during = null!, after = null!, previewAfter = null!;
    }
    public sealed class DetailsReport
    {
        public string status = "unknown", reason = "no_existing_construction_sites";
        public bool? baselineMatches, restored;
        public object? origin;
        public object? rangeOrigin;
        public string? originSource;
        public bool? originOnDistrictRoad;
        public AccessDetail[] accesses = Array.Empty<AccessDetail>();
        public string[] limitations = new[] { "one_site_maximum_eight_accesses", "cached_actual_access_positions_not_recomputed_in_preview",
            "connectivity_not_builder_range_or_trip_proof", "occupation_not_navigation_blockage", "connection_origin_is_district_entrance",
            "range_origin_is_district_center", "node_validity_is_actual_not_preview",
            "actual_queries_not_preview_reachability", "neighbour_control_12_cardinal_offsets_height_minus1_to_plus1",
            "point_queries_use_grid_cell_centers", "instant_spill_can_belong_to_any_district" };
    }
    public sealed class Sample
    {
        public DistrictCenter? District;
        public EntityComponent[] Sites = Array.Empty<EntityComponent>();
        public Vector3Int[][] Accesses = Array.Empty<Vector3Int[]>();
        public bool[] Observed = Array.Empty<bool>();
        public bool[] Before = Array.Empty<bool>();
        public bool[] PreviewBefore = Array.Empty<bool>();
        public bool[]? During;
        public string Reason = "no_existing_construction_sites";
        public string DetailReason = "no_existing_construction_sites";
        public Vector3Int ConnectionOrigin;
        public bool OriginOnDistrictRoad;
        public AccessState[] DetailBefore = Array.Empty<AccessState>(), DetailPreviewBefore = Array.Empty<AccessState>();
        public AccessState[]? DetailDuring;
    }
    public sealed class Report
    {
        public string status = "unknown";
        public string reason = "no_existing_construction_sites";
        public bool? baselineMatches;
        public bool? restored;
        public int lostSites;
        public object[] items = Array.Empty<object>();
        public DetailsReport details = new();
        public bool HasKnownFailure() => restored == false || status == "observed" && lostSites > 0;
        public string[] limitations = new[] { "diagnostic_range_comparison_not_build_permission",
            "existing_sites_under_finished_preview_only", "single_finished_district_only",
            "new_construction_state_not_proven", "maximum_32_sites_64_accesses_32768_range_nodes" };
    }
    public Sample Capture(EntityComponent[] entities, DistrictCenter[] districts)
    {
        var s = new Sample { Sites = entities.Where(e => e.GetComponent<BlockObject>().IsUnfinished).Take(33).ToArray() };
        if (s.Sites.Length == 0) return s;
        s.DetailReason = "detail_not_sampled";
        if (s.Sites.Length > 32) { s.Reason = "construction_site_limit"; return s; }
        if (districts.Length != 1) { s.Reason = "requires_single_finished_district"; return s; }
        s.District = districts[0];
        var accesses = new List<Vector3Int[]>(); var observed = new List<bool>();
        foreach (var site in s.Sites)
        {
            if (site.AllComponents.OfType<IExpandedConstructionSiteReachability>().Any() ||
                !site.TryGetComponent<ConstructionSiteAccessible>(out var access) ||
                !site.TryGetComponent<ReachableConstructionSite>(out var reachable) ||
                access.Accessible is null || !access.Accessible.ValidAccessible)
            { s.Reason = "unsupported_construction_access"; return s; }
            var cells = access.Accessible.Accesses.Select(p => NavigationCoordinateSystem.WorldToGridInt(p)).Distinct().Take(65).ToArray();
            if (cells.Length is < 1 or > 64) { s.Reason = "construction_access_limit"; return s; }
            accesses.Add(cells); observed.Add(reachable.IsReachableByBuilders());
        }
        s.Accesses = accesses.ToArray(); s.Observed = observed.ToArray();
        s.DetailReason = s.Sites.Length != 1 ? "detail_site_limit" : s.Accesses[0].Length > 8 ? "detail_access_limit" : "captured";
        if (s.DetailReason == "captured")
        {
            var district = s.District!;
            var districtBlock = district.GetComponent<BlockObject>();
            if (!districtBlock.HasEntrance) s.DetailReason = "detail_origin_unavailable";
            else { s.ConnectionOrigin = districtBlock.PositionedEntrance.Coordinates;
                s.OriginOnDistrictRoad = district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(s.ConnectionOrigin)); }
        }
        try { s.Before = Read(s, false, Array.Empty<Block>(), out s.DetailBefore);
            s.PreviewBefore = Read(s, true, Array.Empty<Block>(), out s.DetailPreviewBefore); s.Reason = "captured"; }
        catch { s.Reason = "construction_range_unavailable"; }
        return s;
    }
    private bool[] Read(Sample s, bool preview, Block[] candidate, out AccessState[] states)
    {
        var origin = NavigationCoordinateSystem.GridToWorld(s.District!.CenterCoordinates);
        var nodes = (preview ? ranges.GetRoadSpillPreviewNodesInRange(origin) : ranges.GetRoadSpillNodesInRange(origin)).Take(32769).ToArray();
        if (nodes.Length > 32768) throw new InvalidOperationException("construction_range_limit");
        var set = new HashSet<Vector3Int>(nodes);
        states = Array.Empty<AccessState>();
        if (s.DetailReason == "captured")
        {
            try { var originValid = navMesh.IsOnNavMesh(s.ConnectionOrigin);
                var connectionWorld = NavigationCoordinateSystem.GridToWorld(s.ConnectionOrigin);
                // Bounded edge control, not a home-grown route search. Four horizontal
                // cardinal offsets at three heights; out-of-map positions are excluded.
                var neighbourEdges = new[] { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down }
                    .SelectMany(offset => new[] { -1, 0, 1 }.Select(dz => s.ConnectionOrigin + offset + new Vector3Int(0, 0, dz)))
                    .Where(cell => blocks.Contains(cell)).Count(cell => preview ? navMesh.AreConnectedRoadPreview(s.ConnectionOrigin, cell) :
                        navMesh.AreConnectedRoadInstant(s.ConnectionOrigin, cell));
                states = s.Accesses[0].Select(cell => {
                if (!blocks.Contains(cell)) throw new InvalidOperationException("detail_access_outside_map");
                var covered = candidate.Where(b => b.Coordinates == cell).ToArray();
                var occupation = covered.Aggregate(BlockOccupations.None, (value, b) => value | b.Occupation);
                var targetWorld = NavigationCoordinateSystem.GridToWorld(cell);
                return new AccessState { rangeContains = set.Contains(cell),
                    connected = preview ? navMesh.AreConnectedPreview(s.ConnectionOrigin, cell) : navMesh.AreConnectedInstant(s.ConnectionOrigin, cell),
                    roadConnected = preview ? navMesh.AreConnectedRoadPreview(s.ConnectionOrigin, cell) : navMesh.AreConnectedRoadInstant(s.ConnectionOrigin, cell),
                    originOnActualNavMesh = originValid, targetOnActualNavMesh = navMesh.IsOnNavMesh(cell),
                    originNeighbourRoadEdges = neighbourEdges,
                    // These APIs read the actual graph, including during preview. They
                    // are independent controls and never upgrade preview access safety.
                    actualRoadPathFound = navigation.FindInstantRoadPath(connectionWorld, targetWorld, out _),
                    actualDestinationReachable = navigation.DestinationIsReachable(connectionWorld, targetWorld),
                    actualDistrictRoadSpill = districtService.IsOnInstantDistrictRoadSpill(targetWorld),
                    existingOccupied = blocks.GetObjectsAt(cell).Any(b => !b.IsPreview && b.IsIntersecting(Block.FullFrom(cell))),
                    candidateCoversCell = covered.Length > 0, candidateOccupied = covered.Any(b => b.IsOccupied),
                    candidateOccupation = occupation.ToString() };
            }).ToArray(); }
            catch { states = Array.Empty<AccessState>(); s.DetailReason = "detail_unavailable"; }
        }
        return s.Accesses.Select(cells => cells.Any(set.Contains)).ToArray();
    }
    public void ObservePreview(Sample s, IEnumerable<Block> candidate)
    {
        if (s.Reason != "captured") return;
        try { var cells = candidate.Take(577).ToArray();
            if (cells.Length > 576) throw new InvalidOperationException("candidate_detail_limit");
            s.During = Read(s, true, cells, out var states); s.DetailDuring = states; }
        catch { s.Reason = "construction_range_unavailable"; }
    }
    public Report Finish(Sample s)
    {
        var report = new Report { reason = s.Reason };
        var during = s.During;
        report.details.reason = s.DetailReason == "captured" ? s.Reason : s.DetailReason;
        if (s.Reason != "captured" || during is null) return report;
        bool[] after, previewAfter, observedAfter;
        AccessState[] detailAfter, detailPreviewAfter;
        try { after = Read(s, false, Array.Empty<Block>(), out detailAfter);
            previewAfter = Read(s, true, Array.Empty<Block>(), out detailPreviewAfter);
            observedAfter = s.Sites.Select(e => e.GetComponent<ReachableConstructionSite>().IsReachableByBuilders()).ToArray(); }
        catch { report.reason = "construction_range_unavailable"; return report; }
        report.baselineMatches = s.Before.SequenceEqual(s.PreviewBefore) && s.Before.SequenceEqual(s.Observed);
        report.restored = s.Before.SequenceEqual(after) && s.PreviewBefore.SequenceEqual(previewAfter) && s.Observed.SequenceEqual(observedAfter);
        var detailDuring = s.DetailDuring;
        report.details.reason = s.DetailReason;
        if (s.DetailReason == "captured" && detailDuring is not null)
        {
            bool Same(AccessState[] a, AccessState[] b) => a.Length == b.Length && a.Zip(b, (x,y) => x.Same(y)).All(v => v);
            var detail = report.details;
            detail.origin = Vec(s.ConnectionOrigin);
            detail.rangeOrigin = Vec(s.District!.CenterCoordinates);
            detail.originSource = "district_entrance";
            detail.originOnDistrictRoad = s.OriginOnDistrictRoad;
            detail.baselineMatches = Same(s.DetailBefore, s.DetailPreviewBefore);
            detail.restored = Same(s.DetailBefore, detailAfter) && Same(s.DetailPreviewBefore, detailPreviewAfter);
            bool originValid = s.OriginOnDistrictRoad && s.DetailBefore.All(v => v.originOnActualNavMesh);
            detail.status = detail.baselineMatches == true && detail.restored == true && originValid ? "observed" : "unknown";
            detail.reason = detail.restored == false ? "detail_not_restored" : detail.baselineMatches == false ? "detail_baseline_mismatch" :
                !originValid ? "detail_origin_invalid" : "per_access_comparison_only";
            detail.accesses = s.Accesses[0].Select((cell,i) => new AccessDetail { cell = Vec(cell),
                before = s.DetailBefore[i], previewBefore = s.DetailPreviewBefore[i], during = detailDuring[i],
                after = detailAfter[i], previewAfter = detailPreviewAfter[i] }).ToArray();
            if (detail.restored == false) report.restored = false;
        }
        report.status = report.baselineMatches == true && report.restored == true ? "observed" : "unknown";
        report.reason = report.restored != true ? "construction_range_not_restored" : report.baselineMatches != true ? "construction_range_baseline_mismatch" : "range_comparison_only";
        report.lostSites = Enumerable.Range(0,s.Before.Length).Count(i => s.Before[i] && !during[i]);
        report.items = s.Sites.Select((e,i) => (object)new { id=e.EntityId, accessCells=s.Accesses[i].Select(p=>new{x=p.x,y=p.y,z=p.z}).ToArray(),
            observedBuildersReachable=s.Observed[i], before=s.Before[i], previewBefore=s.PreviewBefore[i],
            during=during[i], after=after[i], previewAfter=previewAfter[i], observedBuildersReachableAfter=observedAfter[i] }).ToArray();
        return report;
    }
    private static object Vec(Vector3Int cell) => new { x = cell.x, y = cell.y, z = cell.z };
}
