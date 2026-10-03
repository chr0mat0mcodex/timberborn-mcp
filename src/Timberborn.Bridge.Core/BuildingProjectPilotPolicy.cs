namespace Timberborn.Bridge.Core;

public static class BuildingProjectPilotPolicy
{
    public const int MaxNewRoads = 4;
    // Syntax only: the game catalogue and fresh native geometry decide support.
    // Path is infrastructure, not the terminal building of a flat project.
    public static bool SupportsTemplate(string? template) =>
        BuildingPolicy.ValidTemplate(template) && template != "Path";

    public static bool Allows(int roadCount, bool buildingValid, bool entranceConnected, bool unchanged,
        bool locked, bool[] validRoads, int[] prefixLosses, string status, bool restored, int lost, string[] reasons) =>
        roadCount is >= 0 and <= MaxNewRoads && buildingValid && entranceConnected && unchanged && !locked &&
        validRoads.Length == roadCount && prefixLosses.Length == roadCount && validRoads.All(v => v) &&
        prefixLosses.All(v => v == 0) && status == "unknown" && restored && lost == 0 &&
        reasons.SequenceEqual(new[] { "construction_and_road_node_coverage_unproven" });
}
