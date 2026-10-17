namespace Timberborn.Bridge.Core;

// IsOccupied comes from the public native Block API, not from a template name
// or bounding box. Empty specification cells may extend into a river bank.
public static class TerrainOccupancyPolicy
{
    public static bool Intersects(bool isOccupied, bool terrainUnderground,
        bool undergroundAllowed, bool optionallyUnderground) =>
        isOccupied && terrainUnderground && !undergroundAllowed && !optionallyUnderground;
}
