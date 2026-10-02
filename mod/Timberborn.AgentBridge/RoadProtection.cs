using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Navigation;
using Timberborn.PathSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

// Conservative pilot: native preview is evidence, not proof of construction-state parity.
// Never modify the real navmesh, remove entities or override the game's locks.
public sealed class RoadProtection(EntityRegistry entities)
{
    public sealed class Report
    {
        public string status = "unknown";
        public string[] reasons = Array.Empty<string>();
        public int checkedConnections;
        public int connectedBefore;
        public int roadProbeCount;
        public int constructionProbeCount;
        public int lostConnections;
        public bool restored;
        public bool constructionCovered;
        public object[] affected = Array.Empty<object>();
        public bool affectedTruncated;
        public object[] candidateCells = Array.Empty<object>();
        public string[] limitations = new[] { "native_district_preview_not_worker_or_delivery_guarantee", "construction_state_requires_separate_evidence", "no_autonomous_terrain_access_guarantee" };
    }
    public sealed class Sample
    {
        public DistrictCenter[] Districts = Array.Empty<DistrictCenter>();
        public (Guid Id, Vector3Int Cell, Vector3 Access, string Kind)[] Probes = Array.Empty<(Guid, Vector3Int, Vector3, string)>();
        public bool[] Before = Array.Empty<bool>();
        public bool[] PreviewBefore = Array.Empty<bool>();
        public bool Complete;
        public string Reason = "navigation_coverage_unknown";
    }

    public Sample Capture()
    {
        var sample = new Sample();
        var all = entities.Entities.Where(e => e.Initialized && !e.Deleted && e.TryGetComponent<BlockObject>(out var b) && !b.IsPreview).Take(4097).ToArray();
        if (all.Length > 4096) { sample.Reason = "navigation_object_limit"; return sample; }
        sample.Districts = all.Where(e => e.GetComponent<BlockObject>().IsFinished && e.HasComponent<DistrictCenter>()).Select(e => e.GetComponent<DistrictCenter>()).ToArray();
        if (sample.Districts.Length is < 1 or > 16) { sample.Reason = "navigation_district_coverage_unknown"; return sample; }
        var probes = new List<(Guid, Vector3Int, Vector3, string)>();
        foreach (var e in all)
        {
            var b = e.GetComponent<BlockObject>();
            // Existing sites are targets only, never assumed to be finished detours.
            if (b.IsFinished && e.TryGetComponent<PathSpec>(out var path))
            {
                if (probes.Count >= 4096) { sample.Reason = "navigation_access_limit"; return sample; }
                var cell = b.TransformCoordinates(path.MainPathCoordinates);
                probes.Add((e.EntityId, cell, NavigationCoordinateSystem.GridToWorld(cell), "road_cell"));
            }
            foreach (var access in e.GetComponentsAllocating<Accessible>())
            {
                if (!access.ValidAccessible) continue;
                foreach (var position in access.Accesses)
                {
                    if (probes.Count >= 4096) { sample.Reason = "navigation_access_limit"; return sample; }
                    probes.Add((e.EntityId, b.HasEntrance ? b.PositionedEntrance.Coordinates : b.Coordinates, position,
                        b.IsFinished ? "building_access" : "construction_access"));
                }
            }
        }
        sample.Probes = probes.ToArray();
        if (probes.Count == 0 || probes.Count * sample.Districts.Length > 16384) { sample.Reason = "navigation_comparison_limit"; return sample; }
        sample.Before = Read(sample, false);
        sample.PreviewBefore = Read(sample, true);
        bool matches = sample.Before.SequenceEqual(sample.PreviewBefore);
        sample.Complete = matches && sample.Before.Any(connected => connected);
        sample.Reason = !matches ? "navigation_preview_baseline_mismatch" : sample.Complete ? "" : "navigation_no_connected_baseline";
        return sample;
    }

    public bool[] Read(Sample sample, bool preview) => sample.Districts.SelectMany(d => sample.Probes.Select(p =>
        preview ? d.IsOnPreviewDistrictRoad(p.Access) : d.IsOnInstantDistrictRoad(p.Access))).ToArray();

    public Report Finish(Sample sample, bool[]? withPreview, IEnumerable<Block> candidate)
    {
        var report = new Report { candidateCells = candidate.Select(c => Vec(c.Coordinates)).ToArray(),
            checkedConnections = sample.Before.Length, connectedBefore = sample.Before.Count(connected => connected),
            roadProbeCount = sample.Probes.Count(p => p.Kind == "road_cell"),
            constructionProbeCount = sample.Probes.Count(p => p.Kind == "construction_access") };
        if (!sample.Complete || withPreview is null) { report.reasons = new[] { sample.Reason.Length > 0 ? sample.Reason : "preview_navigation_unavailable" }; return report; }
        report.checkedConnections = sample.Before.Length;
        report.restored = sample.Before.SequenceEqual(Read(sample, false)) && sample.PreviewBefore.SequenceEqual(Read(sample, true));
        int[] lost = RoadProtectionPolicy.LostConnections(sample.Before, withPreview);
        report.lostConnections = lost.Length;
        // Finished preview, construction occupancy, unconnected roads and arbitrary nav adders
        // are not interchangeable. Keep the gate closed until these domains are live-proven.
        report.constructionCovered = false;
        report.status = RoadProtectionPolicy.Decision(true, report.restored, report.constructionCovered, lost.Length);
        report.reasons = !report.restored ? new[] { "navigation_not_restored" } : lost.Length > 0
            ? new[] { "existing_district_access_lost" } : new[] { "construction_and_road_node_coverage_unproven" };
        report.affected = lost.Take(32).Select(index => {
            var p = sample.Probes[index % sample.Probes.Length];
            var d = sample.Districts[index / sample.Probes.Length];
            return (object)new { id = p.Id, entrance = Vec(p.Cell), districtCenter = Vec(d.CenterCoordinates), kind = p.Kind };
        }).ToArray();
        report.affectedTruncated = lost.Length > report.affected.Length;
        return report;
    }
    private static object Vec(Vector3Int p) => new { x = p.x, y = p.y, z = p.z };
}
