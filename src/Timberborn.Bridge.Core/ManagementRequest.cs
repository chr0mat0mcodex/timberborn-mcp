using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class ManagementRequest
{
    public string Route { get; private set; } = "";
    public string Kind { get; private set; } = "";
    public string Id { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string Priority { get; private set; } = "";
    public string ExpectedPriority { get; private set; } = "";
    public string Operation { get; private set; } = "";
    public string Resource { get; private set; } = "";
    public string ExpectedResource { get; private set; } = "";
    public int X { get; private set; } public int Y { get; private set; } public int Z { get; private set; }
    public int Width { get; private set; } public int Height { get; private set; }
    public int Offset { get; private set; } public int Limit { get; private set; }
    public static bool IsPriority(string value) => value is "VeryLow" or "Low" or "Normal" or "High" or "VeryHigh";
    public static bool IsKind(string value) => value is "tree_cutting" or "crops" or "tree_planting" or "tapping";
    public static bool Handles(string path) => path is "/agent-api/v1/priority" or "/agent-api/v1/set-priority" or "/agent-api/v1/construction" or "/agent-api/v1/area-types" or "/agent-api/v1/areas" or "/agent-api/v1/set-area";
    public static ManagementRequest Parse(string path, NameValueCollection q)
    {
        string Text(string key, int max = 160) { var v=q.GetValues(key); if(v is null || v.Length!=1 || v[0] is null || v[0]!.Length>max)throw new ArgumentException("invalid_parameter"); return v[0]!; }
        int Number(string key,int min,int max) { if(!int.TryParse(Text(key),NumberStyles.None,CultureInfo.InvariantCulture,out var n)||n<min||n>max)throw new ArgumentException("invalid_number");return n; }
        string Id(string key) { if(!Guid.TryParseExact(Text(key),"D",out var id)||id==Guid.Empty)throw new ArgumentException("invalid_id");return id.ToString("D"); }
        var r=new ManagementRequest { Route=path.Substring("/agent-api/v1/".Length) };
        if(r.Route=="area-types" && q.Count==0)return r;
        if(r.Route is "priority" or "set-priority") {
            if(q.Count!=(r.Route=="priority"?3:5))throw new ArgumentException();
            r.Kind=Text("kind"); if(r.Kind is not ("workplace" or "construction"))throw new ArgumentException();
            r.Id=Id("id");r.Session=Id("session");
            if(r.Route=="set-priority"){r.Priority=Text("priority");r.ExpectedPriority=Text("expectedPriority");if(!IsPriority(r.Priority)||!IsPriority(r.ExpectedPriority))throw new ArgumentException();}
            return r;
        }
        if(r.Route is "construction" or "areas") {
            if(q.Count!=(r.Route=="construction"?2:3))throw new ArgumentException();
            r.Offset=Number("offset",0,65535);r.Limit=Number("limit",1,32);
            if(r.Route=="areas"){r.Kind=Text("kind");if(!IsKind(r.Kind))throw new ArgumentException();}return r;
        }
        if(r.Route=="set-area" && q.Count==10){
            r.Kind=Text("kind");if(!IsKind(r.Kind)||r.Kind=="tapping")throw new ArgumentException("unsupported_area_kind");
            r.Operation=Text("operation");if(r.Operation is not ("mark" or "remove"))throw new ArgumentException();
            r.Resource=Text("resource");r.ExpectedResource=Text("expectedResource");r.Session=Id("session");
            r.X=Number("x",0,4095);r.Y=Number("y",0,4095);r.Z=Number("z",0,4095);r.Width=Number("width",1,4);r.Height=Number("height",1,4);
            if(r.ExpectedResource.Length==0 || (r.Kind=="tree_cutting" && (r.Resource!="" || r.ExpectedResource is not ("unmarked" or "marked"))) ||
                (r.Operation=="remove" && r.Resource!="") || (r.Kind!="tree_cutting" && r.Operation=="mark" && r.Resource.Length==0))throw new ArgumentException();
            return r;
        }
        throw new ArgumentException("invalid_management_route");
    }
}

