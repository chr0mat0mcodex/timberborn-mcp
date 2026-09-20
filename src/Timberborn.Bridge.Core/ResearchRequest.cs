using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class ResearchRequest
{
    public bool Write { get; private set; }
    public string Template { get; private set; } = "";
    public string Session { get; private set; } = "";
    public int ExpectedCost { get; private set; }
    public int Offset { get; private set; }
    public int Limit { get; private set; }
    public static ResearchRequest Parse(bool write, NameValueCollection q)
    {
        string Read(string key) { var v=q.GetValues(key); if(v is null || v.Length!=1)throw new ArgumentException("invalid_research_request"); return v[0]; }
        int Number(string key,int max) { if(!int.TryParse(Read(key),NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<0||n>max)throw new ArgumentException("invalid_research_number");return n; }
        if(q.Count!=(write?3:2))throw new ArgumentException("invalid_research_request");
        var r=new ResearchRequest { Write=write };
        if(write) {
            r.Template=Read("template");r.Session=Read("session");r.ExpectedCost=Number("expectedCost",int.MaxValue);
            if(!BuildingPolicy.ValidTemplate(r.Template)||!Guid.TryParseExact(r.Session,"D",out var id)||id==Guid.Empty)throw new ArgumentException("invalid_research_target");
            r.Session=id.ToString("D");
        } else { r.Offset=Number("offset",65535);r.Limit=Number("limit",32);if(r.Limit<1)throw new ArgumentException("invalid_limit"); }
        return r;
    }
}

// Shared transaction policy: the game adapter supplies only regular public operations.
public static class ResearchTransaction
{
    public static string Execute(int expectedCost,int cost,bool available,Func<int> points,Func<bool> unlocked,Func<bool> unlockable,Action unlock)
    {
        if(cost<0||expectedCost!=cost)return "cost_changed";
        if(!available)return "unavailable";
        if(unlocked())return "already_unlocked";
        int before=points();
        if(before<cost)return "insufficient_points";
        if(!unlockable())return "not_unlockable";
        unlock();
        return unlocked() && points()==before-cost ? "applied" : "unconfirmed";
    }
}
