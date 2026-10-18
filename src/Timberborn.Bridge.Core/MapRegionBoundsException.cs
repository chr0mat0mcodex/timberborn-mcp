using System.Globalization;

namespace Timberborn.Bridge.Core;

// Only a known axis and numeric map size cross the boundary, never exception text.
public sealed class MapRegionBoundsException : ArgumentException
{
    public string Axis { get; }
    public int Size { get; }

    public MapRegionBoundsException(string axis, int size) : base("map_region_out_of_bounds")
    {
        if (axis is not ("x" or "y" or "z") || size <= 0)
            throw new ArgumentException("invalid_map_bounds");
        Axis = axis;
        Size = size;
    }

    public string ToResponseJson() => "{\"error\":\"map_region_out_of_bounds\",\"axis\":\"" +
        Axis + "\",\"size\":" + Size.ToString(CultureInfo.InvariantCulture) + "}";

    public static void Validate(BridgeRequest request, int sizeX, int sizeY, int sizeZ)
    {
        if (request.Route != "map" || sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
            throw new ArgumentException("invalid_map_bounds");
        Check("x", request.X, request.Width, sizeX);
        Check("y", request.Y, request.Height, sizeY);
        Check("z", request.Z, request.Depth, sizeZ);
    }

    private static void Check(string axis, int start, int length, int size)
    {
        if (start < 0 || (long)start + length > size)
            throw new MapRegionBoundsException(axis, size);
    }
}
