using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class RecentActionsRequest
{
    public string Session { get; private set; } = "";
    public long? After { get; private set; }
    public int Limit { get; private set; } = 32;
    public string Kind { get; private set; } = "all";
    public string Source { get; private set; } = "all";
    public static readonly string[] LegacyKinds = { "object_added", "object_removed", "finished", "paused", "recipe", "storage_good", "storage_mode", "speed", "demolition_marked", "workers" };
    public static readonly string[] SpatialKinds = { "plant_added", "plant_removed", "plant_state", "planting_designation", "cutting_designation" };
    public static readonly string[] Kinds = LegacyKinds.Concat(SpatialKinds).ToArray();
    public static bool ValidSource(string source) => source is "unknown" or "mcp";
    public static RecentActionsRequest Parse(NameValueCollection q)
    {
        foreach (string? key in q.AllKeys)
            if(key is null || !new[]{"session","after","limit","kind","source"}.Contains(key) || q.GetValues(key) is not {Length:1})throw new ArgumentException();
        if(!Guid.TryParseExact(q["session"],"D",out var session)||session==Guid.Empty)throw new ArgumentException();
        var r=new RecentActionsRequest { Session=session.ToString("D") };
        if(q["after"] is {} after) { if(!long.TryParse(after,NumberStyles.None,CultureInfo.InvariantCulture,out long n)||n<0||n>9007199254740991L)throw new ArgumentException();r.After=n; }
        if(q["limit"] is {} limit) { if(!int.TryParse(limit,NumberStyles.None,CultureInfo.InvariantCulture,out int n)||n<1||n>64)throw new ArgumentException();r.Limit=n; }
        if(q["kind"] is {} kind) { if(kind!="all"&&!Kinds.Contains(kind))throw new ArgumentException();r.Kind=kind; }
        if(q["source"] is {} source) { if(source!="all"&&!ValidSource(source))throw new ArgumentException();r.Source=source; }
        return r;
    }
}
public sealed class RecentAction(long sequence, DateTimeOffset observedAtUtc, double gameHours, string kind, string source,
    string evidence, string? id, string? template, int? x, int? y, int? z, string? before, string? after)
{
    public long Sequence { get; }=sequence;
    public DateTimeOffset ObservedAtUtc { get; }=observedAtUtc;
    public double GameHours { get; }=gameHours;
    public string Kind { get; }=kind;
    public string Source { get; }=source;
    public string Evidence { get; }=evidence;
    public string? Id { get; }=id;
    public string? Template { get; }=template;
    public int? X { get; }=x;
    public int? Y { get; }=y;
    public int? Z { get; }=z;
    public string? Before { get; }=before;
    public string? After { get; }=after;
}
public sealed class RecentActionPage(RecentAction[] items,long oldestAvailable,long latest,long nextCursor,bool hasMore,bool gap,long dropped)
{
    public RecentAction[] Items { get; }=items;
    public long OldestAvailable { get; }=oldestAvailable;
    public long Latest { get; }=latest;
    public long NextCursor { get; }=nextCursor;
    public bool HasMore { get; }=hasMore;
    public bool Gap { get; }=gap;
    public long Dropped { get; }=dropped;
}
// Game-thread owned, memory only. Cursor advances past filtered-out entries too.
public sealed class RecentActionBuffer
{
    public const int Capacity=512;
    private readonly Queue<RecentAction> entries=new();
    private long sequence;
    public void Add(DateTimeOffset at,double hours,string kind,string source,string evidence,string? id=null,string? template=null,
        int? x=null,int? y=null,int? z=null,string? before=null,string? after=null)
    {
        if(!RecentActionsRequest.Kinds.Contains(kind)||!RecentActionsRequest.ValidSource(source)||
            evidence is not ("event" or "state_difference") || double.IsNaN(hours)||double.IsInfinity(hours)||hours<0 ||
            new[]{template,before,after}.Any(s=>s is not null&&(s.Length>160||s.Any(char.IsControl))))throw new ArgumentException();
        entries.Enqueue(new RecentAction(++sequence,at,hours,kind,source,evidence,id,template,x,y,z,before,after));
        if(entries.Count>Capacity)entries.Dequeue();
    }
    public long Latest => sequence;
    public RecentActionPage Window(long after) => ReadPage(after,Capacity,"all","all");
    public RecentActionPage Read(RecentActionsRequest r) => ReadPage(r.After,r.Limit,r.Kind,r.Source);
    private RecentActionPage ReadPage(long? after,int limit,string kind,string source)
    {
        if(after>sequence)throw new BridgeRejectionException("cursor_ahead");
        long oldest=entries.Count==0?sequence+1:entries.Peek().Sequence;
        var eligible=entries.Where(e=>(after is null||e.Sequence>after.Value)&&
            (kind=="all"||e.Kind==kind)&&(source=="all"||e.Source==source)).ToArray();
        var items=after is null?eligible.Skip(Math.Max(0,eligible.Length-limit)).ToArray():eligible.Take(limit).ToArray();
        bool more=after is not null&&eligible.Length>items.Length;
        long next=more?items[items.Length-1].Sequence:sequence;
        return new RecentActionPage(items,oldest,sequence,next,more,after is {} cursor&&cursor<oldest-1,Math.Max(0,sequence-Capacity));
    }
}
