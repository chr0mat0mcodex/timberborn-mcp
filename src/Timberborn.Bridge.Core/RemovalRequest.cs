using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class RemovalRequest
{
    public string Route { get; private set; } = "";
    public string Kind { get; private set; } = "";
    public string Id { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string Template { get; private set; } = "";
    public string Operation { get; private set; } = "";
    public bool ExpectedMarked { get; private set; }
    public int X { get; private set; } public int Y { get; private set; } public int Z { get; private set; }
    public int Width { get; private set; } public int Height { get; private set; } public int Depth { get; private set; }
    public int Offset { get; private set; } public int Limit { get; private set; }
    public static bool IsKind(string kind) => kind is "buildings" or "planted" or "vegetation" or "debris";
    public static bool Handles(string path) => path is "/agent-api/v1/removal-targets" or "/agent-api/v1/remove-object";
    public static RemovalRequest Parse(string path, NameValueCollection q)
    {
        string Text(string k) { var v=q.GetValues(k); if(v is null || v.Length!=1 || string.IsNullOrEmpty(v[0]) || v[0]!.Length>160) throw new ArgumentException(); return v[0]!; }
        int Num(string k,int min,int max) { if(!int.TryParse(Text(k),NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<min||n>max)throw new ArgumentException();return n; }
        string Id(string k) {if(!Guid.TryParseExact(Text(k),"D",out var id)||id==Guid.Empty)throw new ArgumentException();return id.ToString("D");}
        if(!Handles(path))throw new ArgumentException();
        var r=new RemovalRequest{Route=path.Substring("/agent-api/v1/".Length),Kind=Text("kind"),X=Num("x",0,4095),Y=Num("y",0,4095),Z=Num("z",0,4095)};
        if(r.Route=="removal-targets") {
            if(q.Count!=9 || (r.Kind!="all"&&!IsKind(r.Kind)))throw new ArgumentException();
            r.Width=Num("width",1,8);r.Height=Num("height",1,8);r.Depth=Num("depth",1,4);r.Offset=Num("offset",0,65535);r.Limit=Num("limit",1,32);
        } else {
            if(q.Count!=9||!IsKind(r.Kind))throw new ArgumentException();
            r.Id=Id("id");r.Session=Id("session");r.Template=Text("template");r.Operation=Text("operation");
            var expected=Text("expectedMarked");if(expected is not ("true" or "false"))throw new ArgumentException();r.ExpectedMarked=expected=="true";
            if(r.Kind is "buildings" or "debris") {if(r.Operation!="delete"||r.ExpectedMarked)throw new ArgumentException();}
            else if(r.Operation is not ("mark" or "unmark"))throw new ArgumentException();
        }
        return r;
    }
}
