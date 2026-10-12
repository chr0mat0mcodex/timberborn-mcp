using System.Globalization;
namespace Timberborn.Bridge.Core;

// Hard exclusion volumes for newly proposed parts; existing terrain/roads are unchanged.
public sealed class ReservedArea
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Depth { get; set; }
    public bool Contains(int x,int y,int z) => x>=X && x<X+Width && y>=Y && y<Y+Height && z>=Z && z<Z+Depth;
    public static void Validate(ReservedArea[] areas)
    {
        if(areas is null || areas.Length>8 || areas.Any(a=>a is null ||
            a.X<0 || a.Y<0 || a.Z<0 || a.Width is <1 or >32 || a.Height is <1 or >32 || a.Depth is <1 or >8 ||
            a.X>4096-a.Width || a.Y>4096-a.Height || a.Z>4096-a.Depth))
            throw new ArgumentException("invalid_reserved_areas");
    }
    public static string Encode(ReservedArea[] areas)
    {
        Validate(areas);
        return string.Join(";",areas.Select(a=>string.Join(",",new[]{a.X,a.Y,a.Z,a.Width,a.Height,a.Depth}
            .Select(v=>v.ToString(CultureInfo.InvariantCulture)))));
    }
    public static ReservedArea[] Decode(string value)
    {
        if(value.Length is <1 or >256)throw new ArgumentException("invalid_reserved_areas");
        var result=value.Split(';').Select(item=>{
            var p=item.Split(',');
            if(p.Length!=6)throw new ArgumentException("invalid_reserved_areas");
            var n=p.Select(s=>int.TryParse(s,NumberStyles.None,CultureInfo.InvariantCulture,out int v)?v:
                throw new ArgumentException("invalid_reserved_areas")).ToArray();
            return new ReservedArea{X=n[0],Y=n[1],Z=n[2],Width=n[3],Height=n[4],Depth=n[5]};
        }).ToArray();
        Validate(result);return result;
    }
}
