using System.Collections.Specialized;
using System.Globalization;

namespace Timberborn.Bridge.Core;

public sealed class StrategicMapRequest
{
    public const int TileSize = 32;
    public const int MaxResponseBytes = 4 * 1024 * 1024;
    public string Session { get; }
    public int Tile { get; }
    private StrategicMapRequest(string session, int tile) { Session = session; Tile = tile; }
    public static StrategicMapRequest Parse(NameValueCollection query)
    {
        var sessions = query.GetValues("session"); var tiles = query.GetValues("tile");
        if (query.Count != 2 || sessions?.Length != 1 || tiles?.Length != 1 ||
            !Guid.TryParseExact(sessions[0], "D", out var id) || id == Guid.Empty ||
            !int.TryParse(tiles[0], NumberStyles.None, CultureInfo.InvariantCulture, out int tile) || tile < 0 || tile >= 256)
            throw new ArgumentException("invalid_request");
        return new(id.ToString("D"), tile);
    }
}
