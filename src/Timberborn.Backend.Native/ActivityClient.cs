using System.Text;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;
public sealed record NativeActivityAck(bool Accepted);
public sealed record NativeActivityLog(int Capacity, ActivityEntry[] Items, long Revision, bool WindowVisible, bool UiAttached);
public sealed partial class NativeClient
{
    public Task<BridgeEnvelope<NativeActivityLog>> Activity(CancellationToken ct)=>Get<NativeActivityLog>("activity-log",ct);
    public async Task<string?> TryRecordActivity(string id,string tool,string state,string session,string reasoning,string summary)
    {
        // Telemetry must not retry actions or turn a successful action into a failure.
        using var timeout=new CancellationTokenSource(TimeSpan.FromMilliseconds(1500));
        try {
            var headers=new Dictionary<string,string> { ["X-Agent-Reasoning"]=Convert.ToBase64String(Encoding.UTF8.GetBytes(reasoning)),
                ["X-Agent-Summary"]=Convert.ToBase64String(Encoding.UTF8.GetBytes(summary)) };
            var result=await Get<NativeActivityAck>($"activity?id={id}&tool={Uri.EscapeDataString(tool)}&state={state}&session={session}",timeout.Token,HttpMethod.Post,headers);
            return result.Data.Accepted ? result.SessionId : null;
        } catch(Exception ex) when(ex is ArgumentException or HttpRequestException or IOException or OperationCanceledException or System.Text.Json.JsonException or UnauthorizedAccessException) { return null; }
    }
}
