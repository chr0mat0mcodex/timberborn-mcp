namespace Timberborn.Bridge.Core;

public static class BuildingProjectPilotPolicy
{
    // Deliberately bounded flat projects. Vertical pilots keep their own fixed scope.
    public static bool SupportsTemplate(string? template) =>
        template is "SmallWarehouse.Folktails" or "MediumWarehouse.Folktails";

    public static bool Allows(int roadCount, bool buildingValid, bool entranceConnected, bool unchanged,
        bool locked, bool[] validRoads, int[] prefixLosses, string status, bool restored, int lost, string[] reasons) =>
        roadCount is >= 0 and <= 4 && buildingValid && entranceConnected && unchanged && !locked &&
        validRoads.Length == roadCount && prefixLosses.Length == roadCount && validRoads.All(v => v) &&
        prefixLosses.All(v => v == 0) && status == "unknown" && restored && lost == 0 &&
        reasons.SequenceEqual(new[] { "construction_and_road_node_coverage_unproven" });
}
