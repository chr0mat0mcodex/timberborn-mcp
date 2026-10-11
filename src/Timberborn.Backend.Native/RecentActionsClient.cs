using System.Globalization;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeRecentAction(long Sequence,DateTimeOffset ObservedAtUtc,double GameHours,string Kind,string Source,
    string Evidence,string? Id,string? Template,int? X,int? Y,int? Z,string? Before,string? After);
public sealed record NativeRecentPage(NativeRecentAction[] Items,long OldestAvailable,long Latest,long NextCursor,bool HasMore,bool Gap,long Dropped);
public sealed record NativeRecentActions(int Capacity,DateTimeOffset? StartedAtUtc,DateTimeOffset? LastScanAtUtc,bool TrackingComplete,
    long ObservationFailures,int TrackedObjects,int MaxObjects,int ScanIntervalSeconds,NativeRecentPage Page,string[] SupportedKinds,string[] Limitations);
public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeRecentActions>> RecentActions(RecentActionsRequest r,CancellationToken ct)
    {
        string q=$"recent-actions?session={r.Session}&limit={r.Limit}&kind={r.Kind}&source={r.Source}";
        if(r.After is {} after)q+="&after="+after.ToString(CultureInfo.InvariantCulture);
        var e=await Get<NativeRecentActions>(q,ct,maxResponseBytes:256*1024);
        var d=e.Data;var p=d.Page;
        if(e.BridgeVersion!="0.40.0"||e.SessionId!=r.Session||d.Capacity!=512||d.MaxObjects!=4096||d.ScanIntervalSeconds!=1||
            d.ObservationFailures<0||d.TrackedObjects<0||d.TrackedObjects>d.MaxObjects||d.SupportedKinds is null||
            !d.SupportedKinds.SequenceEqual(RecentActionsRequest.Kinds)||d.Limitations is null||d.Limitations.Any(s=>s is null)||
            p is null||p.Items is null||p.Items.Length>r.Limit||p.Latest<0||p.Latest>9007199254740991L||
            p.OldestAvailable!=Math.Max(1,p.Latest-511)||p.Dropped!=Math.Max(0,p.Latest-512)||
            p.NextCursor<0||p.NextCursor>p.Latest||r.After>p.Latest||
            p.Gap!=(r.After is {} a&&a<p.OldestAvailable-1)||
            (p.HasMore&&(r.After is null||p.Items.Length!=r.Limit))||
            (!p.HasMore&&p.NextCursor!=p.Latest)||
            (d.TrackingComplete&&(d.StartedAtUtc is null||d.LastScanAtUtc is null))||
            (d.StartedAtUtc is null&&(p.Latest!=0||d.TrackedObjects!=0))||d.StartedAtUtc>e.ObservedAtUtc||d.LastScanAtUtc>e.ObservedAtUtc||d.LastScanAtUtc<d.StartedAtUtc)throw new InvalidDataException("Invalid recent actions page");
        long previous=r.After??0;
        foreach(var item in p.Items) {
            if(item is null||item.Sequence<=previous||item.Sequence<p.OldestAvailable||item.Sequence>p.Latest||item.ObservedAtUtc<d.StartedAtUtc||item.ObservedAtUtc>e.ObservedAtUtc||
                !RecentActionsRequest.Kinds.Contains(item.Kind)||!RecentActionsRequest.ValidSource(item.Source)||
                item.Evidence is not ("event" or "state_difference")||r.Kind!="all"&&item.Kind!=r.Kind||r.Source!="all"&&item.Source!=r.Source||
                !double.IsFinite(item.GameHours)||item.GameHours<0||
                new[]{item.Template,item.Before,item.After}.Any(s=>s is not null&&(s.Length>160||s.Any(char.IsControl)))||
                (item.Kind=="speed"?(item.Id is not null||item.Template is not null||item.X is not null||item.Y is not null||item.Z is not null):
                    !Guid.TryParseExact(item.Id,"D",out var id)||id==Guid.Empty||!BuildingPolicy.ValidTemplate(item.Template)||item.X is null or <0 or >4095||item.Y is null or <0 or >4095||item.Z is null or <0 or >4095))
                throw new InvalidDataException("Invalid recent action entry");
            previous=item.Sequence;
        }
        if(p.HasMore&&p.NextCursor!=previous)throw new InvalidDataException("Invalid journal cursor");
        return e;
    }
}
