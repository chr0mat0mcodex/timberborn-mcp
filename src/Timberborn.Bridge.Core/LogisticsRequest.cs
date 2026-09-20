using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class LogisticsRequest
{
    public string Route { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string Id { get; private set; } = "";
    public string ToId { get; private set; } = "";
    public string Good { get; private set; } = "";
    public int Offset { get; private set; }
    public int Limit { get; private set; }
    public static bool Handles(string path)=>path is "/agent-api/v1/building-access" or "/agent-api/v1/road-connection" or "/agent-api/v1/work-range" or "/agent-api/v1/good-history";
    public static LogisticsRequest Parse(string path,NameValueCollection q)
    {
        if(!Handles(path))throw new ArgumentException();
        var r=new LogisticsRequest {Route=path.Substring("/agent-api/v1/".Length)};
        int count=r.Route=="building-access"?2:r.Route=="work-range"?4:3;
        if(q.Count!=count)throw new ArgumentException();
        string Read(string key){var v=q.GetValues(key);if(v is null||v.Length!=1)throw new ArgumentException();return v[0];}
        string Id(string key){if(!Guid.TryParseExact(Read(key),"D",out var id)||id==Guid.Empty)throw new ArgumentException();return id.ToString("D");}
        int Number(string key,int min,int max){if(!int.TryParse(Read(key),NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<min||n>max)throw new ArgumentException();return n;}
        if(r.Route=="good-history"){r.Good=Read("good");if(!BuildingPolicy.ValidTemplate(r.Good))throw new ArgumentException();}
        else {r.Id=Id("id");r.Session=Id("session");}
        if(r.Route=="road-connection")r.ToId=Id("toId");
        if(r.Route is "good-history" or "work-range"){r.Offset=Number("offset",0,65535);r.Limit=Number("limit",1,32);}
        return r;
    }
}
