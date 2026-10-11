namespace Timberborn.Bridge.Core;

// Geometric candidates, never a substitute for ReachableConstructionSite.
public static class ConstructionWorkCells
{
    public static AccessRouteSearch.Cell[] Around(IEnumerable<AccessRouteSearch.Cell> foundations,
        Func<AccessRouteSearch.Cell, bool> usable)
    {
        var result = new HashSet<AccessRouteSearch.Cell>();
        foreach (var cell in foundations)
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var candidate = new AccessRouteSearch.Cell(cell.X + dx, cell.Y + dy, cell.Z);
                    if (usable(candidate)) result.Add(candidate);
                }
        return result.OrderBy(c => c.Z).ThenBy(c => c.Y).ThenBy(c => c.X).ToArray();
    }

    public static bool Accepts(bool required, string state) =>
        required ? state == "road_reachable" : state == "helper_runtime_check";
}
