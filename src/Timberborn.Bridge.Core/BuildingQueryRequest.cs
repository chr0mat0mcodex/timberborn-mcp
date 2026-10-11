using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class BuildingQueryRequest
{
    public string Session { get; private set; } = "";
    public string[] Ids { get; private set; } = Array.Empty<string>();
    public string Template { get; private set; } = "";
    public bool? Finished { get; private set; }
    public bool IncludePaths { get; private set; }
    public bool ProductionOnly { get; private set; }
    public bool Diagnose { get; private set; }
    public bool Full { get; private set; }
    public bool HasBounds { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Depth { get; private set; }
    public int Offset { get; private set; }
    public int Limit { get; private set; }
    public static bool Handles(string path)=>path is "/agent-api/v1/building-search" or "/agent-api/v1/building-operations";
    public static BuildingQueryRequest Parse(string path,NameValueCollection q)
    {
        if(!Handles(path))throw new ArgumentException();
        bool batch=path.EndsWith("building-operations",StringComparison.Ordinal);
        var allowed=batch?new[]{"ids","session","detail","diagnose"}:new[]{"offset","limit","template","finished","includePaths","productionOnly","detail","x","y","z","width","height","depth"};
        foreach(string? key in q.AllKeys)
            if(key is null || !allowed.Contains(key) || q.GetValues(key) is not {Length:1})throw new ArgumentException();
        string Read(string key)=>q[key]??throw new ArgumentException();
        int Number(string key,int min,int max) {
            if(!int.TryParse(Read(key),NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<min||n>max)throw new ArgumentException();return n;
        }
        bool Flag(string key)=>Read(key) switch {"true"=>true,"false"=>false,_=>throw new ArgumentException()};
        var r=new BuildingQueryRequest();
        if(q["detail"] is {} detail) {if(detail is not ("compact" or "full"))throw new ArgumentException();r.Full=detail=="full";}
        if(batch) {
            string Id(string value) {if(!Guid.TryParseExact(value,"D",out var id)||id==Guid.Empty)throw new ArgumentException();return id.ToString("D");}
            r.Session=Id(Read("session"));
            if(q["diagnose"] is not null)r.Diagnose=Flag("diagnose");
            var raw=Read("ids");if(raw.Length>32*37)throw new ArgumentException();
            r.Ids=raw.Split(',').Select(Id).ToArray();
            if(r.Ids.Length is <1 or >32 || r.Ids.Distinct().Count()!=r.Ids.Length)throw new ArgumentException();
            if(r.Diagnose&&r.Ids.Length>8)throw new ArgumentException("diagnosis_maximum_eight_buildings");
            return r;
        }
        r.Offset=Number("offset",0,65535);r.Limit=Number("limit",1,32);
        if(q["template"] is {} template) {if(!BuildingPolicy.ValidTemplate(template))throw new ArgumentException();r.Template=template;}
        if(q["finished"] is not null)r.Finished=Flag("finished");
        if(q["includePaths"] is not null)r.IncludePaths=Flag("includePaths");
        if(q["productionOnly"] is not null)r.ProductionOnly=Flag("productionOnly");
        var bounds=new[]{"x","y","z","width","height","depth"};
        r.HasBounds=bounds.Any(k=>q[k] is not null);
        if(r.HasBounds) {
            r.X=Number("x",0,4095);r.Y=Number("y",0,4095);r.Z=Number("z",0,4095);
            r.Width=Number("width",1,4096);r.Height=Number("height",1,4096);r.Depth=Number("depth",1,4096);
            if(r.X+r.Width>4096||r.Y+r.Height>4096||r.Z+r.Depth>4096)throw new ArgumentException();
        }
        return r;
    }
    // Bounds use the object's anchor, not any overlapping footprint cell.
    public bool Matches(string template,bool finished,bool path,bool production,int x,int y,int z)=>
        (IncludePaths||!path) && (!ProductionOnly||production) && (Template==""||Template==template) &&
        (Finished is null||Finished==finished) && (!HasBounds||x>=X&&x<X+Width&&y>=Y&&y<Y+Height&&z>=Z&&z<Z+Depth);
}
