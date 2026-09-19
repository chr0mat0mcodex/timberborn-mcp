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
    public int Offset { get; }
    public int Limit { get; }
    public string Template { get; }
    public int Rotation { get; }
    private BridgeRequest(string route, int x = 0, int y = 0, int z = 0, int width = 0, int height = 0, int depth = 0,
        int offset = 0, int limit = 0, string template = "", int rotation = 0)
    { Route = route; X = x; Y = y; Z = z; Width = width; Height = height; Depth = depth;
        Offset = offset; Limit = limit; Template = template; Rotation = rotation; }

    public static BridgeRequest Parse(string path, NameValueCollection query)
    {
        if (path == "/agent-api/v1/snapshot" && query.Count == 0) return new("snapshot");
        if (path == "/agent-api/v1/catalog" && query.Count == 0) return new("catalog");
        int Read(string key, int min, int max)
        {
            var values = query.GetValues(key);
            if (values is null || values.Length != 1 || !int.TryParse(values[0], NumberStyles.None,
                    CultureInfo.InvariantCulture, out var value) || value < min || value > max)
                throw new ArgumentException("invalid_request");
            return value;
        }
        if (path == "/agent-api/v1/objects" && query.Count == 2)
            return new("objects", offset: Read("offset", 0, 65535), limit: Read("limit", 1, 32));
        if (path == "/agent-api/v1/site-precheck" && query.Count == 5)
        {
            var values = query.GetValues("template");
            if (values is null || values.Length != 1 || values[0] is not ("Lodge.Folktails" or "Path"))
                throw new ArgumentException("invalid_request");
            return new("site-precheck", Read("x", 0, 4095), Read("y", 0, 4095), Read("z", 0, 4095),
                template: values[0], rotation: Read("rotation", 0, 3));
        }
        if (path != "/agent-api/v1/map" || query.Count != 6) throw new ArgumentException("invalid_request");
        return new("map", Read("x", 0, 4095), Read("y", 0, 4095), Read("z", 0, 4095),
            Read("width", 1, 8), Read("height", 1, 8), Read("depth", 1, 4));
    }
}
