using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record GoodStock(string Id,string DisplayName,string GoodType,string GroupId,int AvailableStock,int AllStock,
    int StockpiledStock,int BufferedOutputStock,int BufferedInput,int StockUnderProcessing,int CarriedToStockpilesStock,
    int CarriedToProcessors,int InputOutputCapacity,int TotalCapacity);
public sealed record NativeGoods(string Scope,int Offset,int Limit,int Total,GoodStock[] Items,bool HasMore,string[] Limitations);
public sealed record AlertGroup(string AlertId,string StatusDescription,string AlertDescription,bool ShowAlert,bool Priority,bool Notifying,int AffectedCount,int InstanceCount);
public sealed record NativeAlerts(string Scope,int Offset,int Limit,int Total,AlertGroup[] Items,bool HasMore,string[] Limitations);
public sealed record WorldPosition(float X,float Y,float Z);
public sealed record AlertTarget(Guid Id,string? Template,string Kind,Position? GridPosition,WorldPosition WorldPosition);
public sealed record NativeAlertTargets(string AlertId,bool Found,int Offset,int Limit,int Total,AlertTarget[] Items,bool HasMore,string[] Limitations);

public sealed partial class NativeClient
{
    private static void CheckPage<T>(BridgeEnvelope<T> e,EconomyRequest r,int offset,int limit,int total,int count,bool more,string[]? limitations)
    {
        if(e.BridgeVersion is not ("0.18.0" or "0.19.0" or "0.19.1")||offset!=r.Offset||limit!=r.Limit||total<0||count!=Math.Min(limit,Math.Max(0,total-offset))||
            more!=((long)offset+count<total)||limitations is null||limitations.Any(s=>s is null))throw new InvalidDataException("Invalid economy page");
    }
    private static bool Text(string? value,int max)=>value is not null && value.Length<=max;
    public async Task<BridgeEnvelope<NativeGoods>> Goods(EconomyRequest r,CancellationToken ct)
    {
        if(r.Route!="goods")throw new ArgumentException();
        var e=await Get<NativeGoods>($"goods?offset={r.Offset}&limit={r.Limit}",ct);var d=e.Data;
        CheckPage(e,r,d.Offset,d.Limit,d.Total,d.Items?.Length??-1,d.HasMore,d.Limitations);
        if(d.Items is null||d.Scope!="global_registered_goods"||d.Items!.Any(i=>i is null||!BuildingPolicy.ValidTemplate(i.Id)||!Text(i.DisplayName,512)||!Text(i.GoodType,160)||!Text(i.GroupId,160)||
            new[]{i.AvailableStock,i.AllStock,i.StockpiledStock,i.BufferedOutputStock,i.BufferedInput,i.StockUnderProcessing,i.CarriedToStockpilesStock,i.CarriedToProcessors,i.InputOutputCapacity,i.TotalCapacity}.Any(n=>n<0))||
            !d.Items.Select(i=>i.Id).SequenceEqual(d.Items.Select(i=>i.Id).Distinct().Order(StringComparer.Ordinal)))throw new InvalidDataException("Invalid goods");
        return e;
    }
    public async Task<BridgeEnvelope<NativeAlerts>> Alerts(EconomyRequest r,CancellationToken ct)
    {
        if(r.Route!="alerts")throw new ArgumentException();
        var e=await Get<NativeAlerts>($"alerts?offset={r.Offset}&limit={r.Limit}",ct);var d=e.Data;
        CheckPage(e,r,d.Offset,d.Limit,d.Total,d.Items?.Length??-1,d.HasMore,d.Limitations);
        if(d.Items is null||d.Scope!="visible_active_entity_statuses"||d.Items!.Any(i=>i is null||!EconomyRequest.ValidAlertId(i.AlertId)||!Text(i.StatusDescription,512)||!Text(i.AlertDescription,512)||
            i.AffectedCount<1||i.InstanceCount<i.AffectedCount)||!d.Items.Select(i=>i.AlertId).SequenceEqual(d.Items.Select(i=>i.AlertId).Distinct().Order(StringComparer.Ordinal)))throw new InvalidDataException("Invalid alerts");
        return e;
    }
    public async Task<BridgeEnvelope<NativeAlertTargets>> AlertTargets(EconomyRequest r,CancellationToken ct)
    {
        if(r.Route!="alert-targets")throw new ArgumentException();
        var e=await Get<NativeAlertTargets>($"alert-targets?offset={r.Offset}&limit={r.Limit}&session={r.Session}&alertId={r.AlertId}",ct);var d=e.Data;
        CheckPage(e,r,d.Offset,d.Limit,d.Total,d.Items?.Length??-1,d.HasMore,d.Limitations);
        if(d.Items is null||e.SessionId!=r.Session||d.AlertId!=r.AlertId||d.Found!=(d.Total>0)||d.Items!.Any(i=>i is null||i.Id==Guid.Empty||
            i.Template is not null&&!BuildingPolicy.ValidTemplate(i.Template)||i.Kind is not ("entity" or "block_object")||
            (i.Kind=="block_object")!=(i.GridPosition is not null)||i.WorldPosition is null||!float.IsFinite(i.WorldPosition.X)||!float.IsFinite(i.WorldPosition.Y)||!float.IsFinite(i.WorldPosition.Z))||
            !d.Items.Select(i=>i.Id).SequenceEqual(d.Items.Select(i=>i.Id).Distinct().Order()))throw new InvalidDataException("Invalid alert targets");
        return e;
    }
}
