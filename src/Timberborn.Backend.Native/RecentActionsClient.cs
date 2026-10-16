using System.Globalization;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeRecentAction(long Sequence,DateTimeOffset ObservedAtUtc,double GameHours,string Kind,string Source,
    string Evidence,string? Id,string? Template,int? X,int? Y,int? Z,string? Before,string? After);
public sealed record NativeRecentPage(NativeRecentAction[] Items,long OldestAvailable,long Latest,long NextCursor,bool HasMore,bool Gap,long Dropped);
public sealed record NativeRecentActions(int Capacity,DateTimeOffset? StartedAtUtc,DateTimeOffset? LastScanAtUtc,bool TrackingComplete,
    long ObservationFailures,int TrackedObjects,int MaxObjects,int ScanIntervalSeconds,NativeRecentPage Page,string[] SupportedKinds,string[] Limitations,
    bool? SpatialTrackingComplete=null,long? SpatialObservationFailures=null,int? TrackedSpatialEntries=null,int? MaxSpatialEntries=null,
    DateTimeOffset? SpatialStartedAtUtc=null,DateTimeOffset? SpatialLastScanAtUtc=null);
public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeRecentActions>> RecentActions(RecentActionsRequest r,CancellationToken ct)
    {
        string q=$"recent-actions?session={r.Session}&limit={r.Limit}&kind={r.Kind}&source={r.Source}";
        if(r.After is {} after)q+="&after="+after.ToString(CultureInfo.InvariantCulture);
        var e=await Get<NativeRecentActions>(q,ct,maxResponseBytes:256*1024);
        var d=e.Data;var p=d.Page;
        if(e.BridgeVersion is not ("0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0")||e.SessionId!=r.Session||d.Capacity!=512||d.MaxObjects!=4096||d.ScanIntervalSeconds!=1||
            d.ObservationFailures<0||d.TrackedObjects<0||d.TrackedObjects>d.MaxObjects||d.SupportedKinds is null||
            !d.SupportedKinds.SequenceEqual(e.BridgeVersion is ("0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0")?RecentActionsRequest.Kinds:RecentActionsRequest.LegacyKinds)||d.Limitations is null||d.Limitations.Any(s=>s is null)||
            p is null||p.Items is null||p.Items.Length>r.Limit||p.Latest<0||p.Latest>9007199254740991L||
            p.OldestAvailable!=Math.Max(1,p.Latest-511)||p.Dropped!=Math.Max(0,p.Latest-512)||
            p.NextCursor<0||p.NextCursor>p.Latest||r.After>p.Latest||
            p.Gap!=(r.After is {} a&&a<p.OldestAvailable-1)||
            (p.HasMore&&(r.After is null||p.Items.Length!=r.Limit))||
            (!p.HasMore&&p.NextCursor!=p.Latest)||
            (d.TrackingComplete&&(d.StartedAtUtc is null||d.LastScanAtUtc is null))||
            (d.StartedAtUtc is null&&(p.Latest!=0||d.TrackedObjects!=0))||d.StartedAtUtc>e.ObservedAtUtc||d.LastScanAtUtc>e.ObservedAtUtc||d.LastScanAtUtc<d.StartedAtUtc)throw new InvalidDataException("Invalid recent actions page");
        if(e.BridgeVersion is ("0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0") && (d.SpatialTrackingComplete is null||d.SpatialObservationFailures is null or <0||
            d.TrackedSpatialEntries is null or <0 or >SpatialJournal.MaxEntries||d.MaxSpatialEntries!=SpatialJournal.MaxEntries||
            d.ObservationFailures<d.SpatialObservationFailures||d.TrackingComplete&&d.SpatialTrackingComplete!=true||
            d.SpatialTrackingComplete==true&&(d.SpatialStartedAtUtc is null||d.SpatialLastScanAtUtc is null)||
            d.SpatialStartedAtUtc>e.ObservedAtUtc||d.SpatialLastScanAtUtc>e.ObservedAtUtc||d.SpatialLastScanAtUtc<d.SpatialStartedAtUtc))
            throw new InvalidDataException("Invalid spatial journal coverage");
        long previous=r.After??0;
        foreach(var item in p.Items) {
            if(item is null||item.Sequence<=previous||item.Sequence<p.OldestAvailable||item.Sequence>p.Latest||item.ObservedAtUtc<d.StartedAtUtc||item.ObservedAtUtc>e.ObservedAtUtc||
                !d.SupportedKinds.Contains(item.Kind)||!RecentActionsRequest.ValidSource(item.Source)||
                item.Evidence is not ("event" or "state_difference")||r.Kind!="all"&&item.Kind!=r.Kind||r.Source!="all"&&item.Source!=r.Source||
                !double.IsFinite(item.GameHours)||item.GameHours<0||
                new[]{item.Template,item.Before,item.After}.Any(s=>s is not null&&(s.Length>160||s.Any(char.IsControl)))||
                (item.Kind=="speed"?(item.Id is not null||item.Template is not null||item.X is not null||item.Y is not null||item.Z is not null):
                    (item.Kind is "planting_designation" or "cutting_designation" ? item.Id is not null||item.Template is not null :
                        !Guid.TryParseExact(item.Id,"D",out var id)||id==Guid.Empty||!BuildingPolicy.ValidTemplate(item.Template))||item.X is null or <0 or >4095||item.Y is null or <0 or >4095||item.Z is null or <0 or >4095))
                throw new InvalidDataException("Invalid recent action entry");
            if(RecentActionsRequest.SpatialKinds.Contains(item.Kind))ValidateSpatialAction(item);
            previous=item.Sequence;
        }
        if(p.HasMore&&p.NextCursor!=previous)throw new InvalidDataException("Invalid journal cursor");
        return e;
    }
    private static void ValidateSpatialAction(NativeRecentAction i) {
        bool Plant(string? v)=>v is "plant" or "tree" or "stump" or "tree_unknown";
        bool Value(string? v)=>i.Kind=="cutting_designation"?v=="marked":BuildingPolicy.ValidTemplate(v);
        bool valid=i.Kind switch {
            "plant_added"=>i.Before is null&&Plant(i.After),
            "plant_removed"=>Plant(i.Before)&&i.After is null,
            "plant_state"=>Plant(i.Before)&&Plant(i.After)&&i.Before!=i.After,
            _=>(i.Before is null||Value(i.Before))&&(i.After is null||Value(i.After))&&i.Before!=i.After
        };
        if(!valid||i.Evidence!="state_difference")throw new InvalidDataException("Invalid spatial journal change");
    }
}
