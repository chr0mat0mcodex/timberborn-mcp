using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
namespace Timberborn.Bridge.Core;

public sealed class ActivityRequest
{
    public string Id { get; private set; } = "";
    public string Tool { get; private set; } = "";
    public string State { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string Reasoning { get; private set; } = "";
    public string Summary { get; private set; } = "";
    public static bool IsState(string state) => state is "running" or "ok" or "applied" or "rejected" or "unconfirmed" or "error" or "cancelled";
    public static string Decode(string? encoded, int limit)
    {
        if (encoded is null) return "";
        if (encoded.Length > (limit * 4 + 8) * 2) throw new ArgumentException("activity_text_too_long");
        try { return Clean(new UTF8Encoding(false, true).GetString(Convert.FromBase64String(encoded)), limit); }
        catch (Exception ex) when (ex is FormatException or DecoderFallbackException) { throw new ArgumentException("invalid_activity_text"); }
    }
    public static string Clean(string text, int limit)
    {
        if (text.Length > limit || text.Any(c => char.IsControl(c) && c is not ('\n' or '\r' or '\t')))
            throw new ArgumentException("invalid_activity_text");
        return Regex.Replace(text.Replace('\r',' ').Replace('\n',' ').Replace('\t',' '), @"(?i)\bBearer\s+\S+|\b[a-f0-9]{64}\b", "[redacted]");
    }
    public static ActivityRequest Parse(NameValueCollection q)
    {
        string Read(string key) { var values=q.GetValues(key); if(values is null || values.Length!=1)throw new ArgumentException();return values[0]!; }
        if(q.Count!=6)throw new ArgumentException();
        var r=new ActivityRequest { Id=Read("id"), Tool=Read("tool"), State=Read("state"), Session=Read("session"),
            Reasoning=Clean(Read("reasoning"),600), Summary=Clean(Read("summary"),400) };
        if(!Guid.TryParseExact(r.Id,"D",out var id)||id==Guid.Empty || r.Tool.Length is <1 or >80 ||
            r.Tool.Any(c=>!(c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_')) || !IsState(r.State) ||
            (r.State=="running" ? r.Session!="" : !Guid.TryParseExact(r.Session,"D",out var session)||session==Guid.Empty))throw new ArgumentException();
        return r;
    }
}

public sealed class ActivityEntry(string id, string tool, string state, string reasoning, string summary, DateTimeOffset startedAtUtc, DateTimeOffset updatedAtUtc, long sequence = 0)
{
    public long Sequence { get; } = sequence;
    public string Id { get; } = id;
    public string Tool { get; } = tool;
    public string State { get; } = state;
    public string Reasoning { get; } = reasoning;
    public string Summary { get; } = summary;
    public DateTimeOffset StartedAtUtc { get; } = startedAtUtc;
    public DateTimeOffset UpdatedAtUtc { get; } = updatedAtUtc;
}

// Game-thread only. No files, save data, credentials or full result payloads.
public sealed class ActivityLog
{
    public const int Capacity=128;
    private readonly List<ActivityEntry> entries=new();
    private string epoch=Guid.NewGuid().ToString("N");
    private long sequence;
    private long evictedThrough;
    public long Revision { get; private set; }
    public ActivityEntry[] Snapshot()=>entries.ToArray();
    public static object Payload(ActivityEntry e)=>new { id=e.Id, tool=e.Tool, state=e.State, reasoning=e.Reasoning, summary=e.Summary, startedAtUtc=e.StartedAtUtc, updatedAtUtc=e.UpdatedAtUtc, sequence=e.Sequence };
    public void Clear(){entries.Clear();epoch=Guid.NewGuid().ToString("N");sequence=0;evictedThrough=0;Revision++;}
    public object Read(ActivityLogQuery query, bool windowVisible, bool uiAttached)
    {
        long before=sequence+1;
        if(query.Cursor.Length>0) {
            var cursor=query.DecodeCursor();
            if(cursor.Epoch!=epoch)throw new BridgeRejectionException("stale_activity_cursor");
            if(cursor.Before>before)throw new ArgumentException("invalid_activity_cursor");
            before=cursor.Before;
        }
        var matches=entries.AsEnumerable().Reverse().Where(e=>e.Sequence<before &&
            (query.Tool.Length==0||e.Tool==query.Tool) && (query.State.Length==0||e.State==query.State)).ToArray();
        var page=matches.Take(query.Limit).ToArray();
        bool hasMore=matches.Length>page.Length;
        long unavailableBeforeCursor=Math.Min(before-1,evictedThrough);
        return new { capacity=Capacity, items=page.Select(Payload).ToArray(), revision=Revision, windowVisible, uiAttached,
            paginationVersion=1, logEpoch=epoch, retainedCount=entries.Count, evictedCount=evictedThrough,
            oldestSequence=entries.Count==0 ? (long?)null : entries[0].Sequence,
            newestSequence=entries.Count==0 ? (long?)null : entries[entries.Count-1].Sequence,
            hasMore, nextCursor=hasMore?query.EncodeCursor(epoch,page[page.Length-1].Sequence):null,
            gapDetected=unavailableBeforeCursor>0, unavailableBeforeCursor,
            tool=query.Tool, state=query.State };
    }
    public bool Record(ActivityRequest r, DateTimeOffset now)
    {
        int index=entries.FindIndex(e=>e.Id==r.Id);
        if(r.State=="running") {
            if(index>=0)return false;
            if(entries.Count==Capacity){evictedThrough=entries[0].Sequence;entries.RemoveAt(0);}
            entries.Add(new(r.Id,r.Tool,r.State,r.Reasoning,r.Summary,now,now,++sequence));
        } else {
            if(index<0)return false; // Cleared/evicted calls must not reappear.
            var old=entries[index];
            if(old.State!="running" || old.Tool!=r.Tool)return false;
            entries[index]=new(old.Id,old.Tool,r.State,old.Reasoning,old.Summary,old.StartedAtUtc,now,old.Sequence);
        }
        Revision++;return true;
    }
}
