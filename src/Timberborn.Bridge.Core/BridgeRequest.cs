using System.Collections.Specialized;
using System.Globalization;

namespace Timberborn.Bridge.Core;

public sealed class BridgeRequest
{
    public string Route { get; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }
    private BridgeRequest(string route, int x = 0, int y = 0, int z = 0, int width = 0, int height = 0, int depth = 0)
    { Route = route; X = x; Y = y; Z = z; Width = width; Height = height; Depth = depth; }

    public static BridgeRequest Parse(string path, NameValueCollection query)
    {
        if (path == "/agent-api/v1/snapshot" && query.Count == 0) return new("snapshot");
        if (path != "/agent-api/v1/map" || query.Count != 6) throw new ArgumentException("invalid_request");
        int Read(string key, int min, int max)
        {
            var values = query.GetValues(key);
            if (values is null || values.Length != 1 || !int.TryParse(values[0], NumberStyles.None,
                    CultureInfo.InvariantCulture, out var value) || value < min || value > max)
                throw new ArgumentException("invalid_request");
            return value;
        }
        return new("map", Read("x", 0, 4095), Read("y", 0, 4095), Read("z", 0, 4095),
            Read("width", 1, 8), Read("height", 1, 8), Read("depth", 1, 4));
    }
}
