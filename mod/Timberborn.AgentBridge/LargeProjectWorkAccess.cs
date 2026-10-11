using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Coordinates;
using Timberborn.GameDistricts;
using Timberborn.Navigation;
using Timberborn.PathSystem;
using Timberborn.TemplateSystem;
using UnityEngine;
using Cell = Timberborn.Bridge.Core.AccessRouteSearch.Cell;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    private sealed class WorkAccessReport
    {
        public bool required;
        public string state = "unknown";
        public string source = "geometric_foundation_perimeter_and_native_district_road";
        public int candidateCount;
        public object[] reachableCells = Array.Empty<object>();
        public bool nativeConstructionProven = false;
        [Newtonsoft.Json.JsonIgnore]
        public bool Allowed => ConstructionWorkCells.Accepts(required, state);
    }

    private static bool RequiresWorkAccess(TemplateSpec template, string name) =>
        !template.HasSpec<PathSpec>() && !LargeProjectRecoveryPolicy.AccessHelper(name);

    private Cell[] WorkCells(Block[] shape, IEnumerable<Block> planned)
    {
        var obstacles = planned.ToArray();
        return ConstructionWorkCells.Around(shape.Where(b => b.IsFoundationBlock)
            .Select(b => new Cell(b.Coordinates.x, b.Coordinates.y, b.Coordinates.z)), cell => {
                var p = new Vector3Int(cell.X, cell.Y, cell.Z);
                if (!blocks.Contains(p) || terrain.Underground(p)) return false;
                var probe = Block.FullFrom(p);
                return !obstacles.Any(b => b.IsIntersecting(probe)) &&
                    !blocks.GetObjectsAt(p).Any(b => !b.IsPreview && b.IsIntersecting(probe) &&
                        !b.TryGetComponent<PathSpec>(out _));
            });
    }

    private WorkAccessReport WorkAccess(LargeProjectStep step, DistrictCenter district,
        IEnumerable<Block> planned, bool preview)
    {
        var template = Resolve(step);
        if (!RequiresWorkAccess(template, step.Template))
            return new() { state = "helper_runtime_check" };
        var shape = template.GetSpec<BlockObjectSpec>().GetBlocks(Placement(step)).Take(65).ToArray();
        var candidates = WorkCells(shape, planned);
        var report = new WorkAccessReport { required = true, candidateCount = candidates.Length };
        if (candidates.Length == 0) { report.state = "no_work_cell"; return report; }
        var reachable = candidates.Where(c => preview
            ? district.IsOnPreviewDistrictRoad(NavigationCoordinateSystem.GridToWorld(new Vector3Int(c.X,c.Y,c.Z)))
            : district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(new Vector3Int(c.X,c.Y,c.Z))))
            .Take(16).ToArray();
        report.state = reachable.Length > 0 ? "road_reachable" : "no_connected_work_cell";
        report.reachableCells = reachable.Select(c => (object)new { x=c.X,y=c.Y,z=c.Z }).ToArray();
        return report;
    }
}
