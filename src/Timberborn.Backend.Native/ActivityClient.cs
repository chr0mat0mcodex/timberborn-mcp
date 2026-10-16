using System.Text;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;
public sealed record NativeActivityAck(bool Accepted);
public sealed record NativeActivityLog(int Capacity, ActivityEntry[] Items, long Revision, bool WindowVisible, bool UiAttached,
    int? PaginationVersion=null, string? LogEpoch=null, int? RetainedCount=null, long? EvictedCount=null,
    long? OldestSequence=null, long? NewestSequence=null, bool? HasMore=null, string? NextCursor=null,
    bool? GapDetected=null, long? UnavailableBeforeCursor=null, string? Tool=null, string? State=null,
    int? CorrelationVersion=null,string? ProjectId=null,string? RunId=null);
public sealed partial class NativeClient
{
    public Task<BridgeEnvelope<NativeActivityLog>> Activity(CancellationToken ct)=>Get<NativeActivityLog>("activity-log",ct);
    public Task<BridgeEnvelope<NativeActivityLog>> Activity(ActivityLogQuery query,CancellationToken ct)
    {
        var args=new List<string> { "limit="+query.Limit.ToString(System.Globalization.CultureInfo.InvariantCulture) };
        if(query.Tool.Length>0)args.Add("tool="+Uri.EscapeDataString(query.Tool));
        if(query.State.Length>0)args.Add("state="+Uri.EscapeDataString(query.State));
        if(query.Cursor.Length>0)args.Add("cursor="+Uri.EscapeDataString(query.Cursor));
        if(query.ProjectId.Length>0)args.Add("projectId="+query.ProjectId);
        if(query.RunId.Length>0)args.Add("runId="+query.RunId);
        return Get<NativeActivityLog>("activity-log?"+string.Join("&",args),ct);
    }
    public async Task<string?> TryRecordActivity(string id,string tool,string state,string session,string reasoning,string summary,ActivityCorrelation[]? correlations=null)
    {
        // Telemetry must not retry actions or turn a successful action into a failure.
        using var timeout=new CancellationTokenSource(TimeSpan.FromMilliseconds(1500));
        try {
            var headers=new Dictionary<string,string> { ["X-Agent-Reasoning"]=Convert.ToBase64String(Encoding.UTF8.GetBytes(reasoning)),
                ["X-Agent-Summary"]=Convert.ToBase64String(Encoding.UTF8.GetBytes(summary)) };
            if(correlations is {Length:>0})headers["X-Agent-Correlation"]=Convert.ToBase64String(Encoding.UTF8.GetBytes(ActivityCorrelation.Encode(correlations)));
            var result=await Get<NativeActivityAck>($"activity?id={id}&tool={Uri.EscapeDataString(tool)}&state={state}&session={session}",timeout.Token,HttpMethod.Post,headers);
            return result.Data.Accepted ? result.SessionId : null;
        } catch(Exception ex) when(ex is ArgumentException or HttpRequestException or IOException or OperationCanceledException or System.Text.Json.JsonException or UnauthorizedAccessException) { return null; }
    }
}
