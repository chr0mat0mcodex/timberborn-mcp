using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class DiagnosticsRequest
{
    public string Route { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string Id { get; private set; } = "";
    public int Offset { get; private set; }
    public int Limit { get; private set; }
    public static bool Handles(string path)=>path is "/agent-api/v1/needs" or "/agent-api/v1/beaver-needs" or "/agent-api/v1/building-operation";
    public static DiagnosticsRequest Parse(string path,NameValueCollection q)
    {
        if(!Handles(path))throw new ArgumentException();
        var r=new DiagnosticsRequest {Route=path.Substring("/agent-api/v1/".Length)};
        if(q.Count!=(r.Route=="beaver-needs"?4:2))throw new ArgumentException();
        string Read(string key){var a=q.GetValues(key);if(a is null||a.Length!=1)throw new ArgumentException();return a[0];}
        string Id(string key){if(!Guid.TryParseExact(Read(key),"D",out var id)||id==Guid.Empty)throw new ArgumentException();return id.ToString("D");}
        int Number(string key,int min,int max){if(!int.TryParse(Read(key),NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<min||n>max)throw new ArgumentException();return n;}
        if(r.Route!="needs"){r.Id=Id("id");r.Session=Id("session");}
        if(r.Route!="building-operation"){r.Offset=Number("offset",0,65535);r.Limit=Number("limit",1,32);}
        return r;
    }
}
