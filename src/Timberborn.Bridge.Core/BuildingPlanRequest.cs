using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class BuildingPlanRequest
{
    public string Template { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string DistrictId { get; private set; } = "";
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Rotation { get; private set; }
    public static BuildingPlanRequest Parse(NameValueCollection q)
    {
        if(q.Count != 9) throw new ArgumentException();
        string Read(string k) { var v=q.GetValues(k); if(v is null || v.Length!=1)throw new ArgumentException(); return v[0]; }
        int Number(string k,int max) { if(!int.TryParse(Read(k),NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<0||n>max)throw new ArgumentException(); return n; }
        string Id(string k) { if(!Guid.TryParseExact(Read(k),"D",out var id)||id==Guid.Empty)throw new ArgumentException(); return id.ToString("D"); }
        var r=new BuildingPlanRequest { Template=Read("template"),Session=Id("session"),DistrictId=Id("districtId"),
            X=Number("x",4095),Y=Number("y",4095),Z=Number("z",4095),Width=Number("width",8),Height=Number("height",8),Rotation=Number("rotation",3) };
        if(!BuildingPolicy.ValidTemplate(r.Template)||r.Width<1||r.Height<1||r.X+r.Width>4096||r.Y+r.Height>4096)throw new ArgumentException();
        return r;
    }
}
