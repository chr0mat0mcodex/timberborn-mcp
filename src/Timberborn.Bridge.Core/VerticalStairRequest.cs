using System.Collections.Specialized;
using System.Globalization;

namespace Timberborn.Bridge.Core;

public sealed class VerticalStairRequest
{
    public string Session { get; private set; } = "";
    public string DistrictId { get; private set; } = "";
    public Guid ActionId { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public int Rotation { get; private set; }
    public int UpperPathCount { get; private set; }
    public bool Start { get; private set; }

    public static VerticalStairRequest Parse(bool start, NameValueCollection q)
    {
        if (q.Count != (start ? 8 : 2) && q.Count != (start ? 9 : 2)) throw new ArgumentException();
        string One(string key) => q.GetValues(key) is { Length: 1 } values ? values[0] : throw new ArgumentException();
        string Id(string key) => Guid.TryParseExact(One(key), "D", out var id) && id != Guid.Empty ? id.ToString("D") : throw new ArgumentException();
        int Number(string key) => int.TryParse(One(key), NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value is >= 0 and <= 4095 ? value : throw new ArgumentException();
        var r = new VerticalStairRequest { Session = Id("session"), ActionId = Guid.Parse(Id("actionId")), Start = start };
        if (!start) return r;
        string mode = One("mode");
        if (mode is not ("single_stair_pilot" or "stair_with_upper_paths_pilot")) throw new ArgumentException();
        r.DistrictId = Id("districtId"); r.X = Number("x"); r.Y = Number("y"); r.Z = Number("z");
        r.Rotation = Number("rotation"); if (r.Rotation > 3) throw new ArgumentException();
        if (mode == "single_stair_pilot")
        {
            if (q.Count == 9 && Number("upperPathCount") != 0) throw new ArgumentException();
            if (q.Count is not (8 or 9)) throw new ArgumentException();
        }
        else { r.UpperPathCount = Number("upperPathCount"); if (r.UpperPathCount is < 1 or > 2 || q.Count != 9) throw new ArgumentException(); }
        return r;
    }
}
