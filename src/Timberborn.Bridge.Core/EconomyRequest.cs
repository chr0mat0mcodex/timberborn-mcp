using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
namespace Timberborn.Bridge.Core;

public sealed class EconomyRequest
{
    public string Route { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string AlertId { get; private set; } = "";
    public int Offset { get; private set; }
    public int Limit { get; private set; }
    public static bool Handles(string path) => path is "/agent-api/v1/goods" or "/agent-api/v1/alerts" or "/agent-api/v1/alert-targets";
    public static EconomyRequest Parse(string path, NameValueCollection q)
    {
        if (!Handles(path)) throw new ArgumentException();
        bool targets = path.EndsWith("alert-targets", StringComparison.Ordinal);
        if (q.Count != (targets ? 4 : 2)) throw new ArgumentException();
        string Read(string key) { var values=q.GetValues(key); if(values is null || values.Length!=1)throw new ArgumentException();return values[0]; }
        int Number(string key,int min,int max) { if(!int.TryParse(Read(key),NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<min||n>max)throw new ArgumentException();return n; }
        var r=new EconomyRequest { Route=path.Substring("/agent-api/v1/".Length), Offset=Number("offset",0,65535),Limit=Number("limit",1,32) };
        if(targets) {
            if(!Guid.TryParseExact(Read("session"),"D",out var id)||id==Guid.Empty)throw new ArgumentException();
            r.Session=id.ToString("D");r.AlertId=Read("alertId");
            if(!ValidAlertId(r.AlertId))throw new ArgumentException();
        }
        return r;
    }
    public static bool ValidAlertId(string id) => id is not null && id.Length==64 && id.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');
    // Public StatusInstance exposes no stable specification id. Opaque session-scoped grouping,
    // using complete length-prefixed descriptions rather than truncated display text.
    public static string GroupId(string session,string status,string alert,bool showAlert,bool priority,bool notifying)
    {
        using var hash=SHA256.Create();
        string input=$"{session.Length}:{session}{status.Length}:{status}{alert.Length}:{alert}:{showAlert}:{priority}:{notifying}";
        return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLowerInvariant();
    }
}
