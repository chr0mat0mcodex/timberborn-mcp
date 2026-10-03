using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeConstructionAccess(string State,Position[] Cells,bool Expanded);
public sealed record NativeAccess(string Id,bool Finished,Position Position,bool? EntranceBlocked,bool? EntranceInaccessible,bool? UnconnectedBlocked,bool? BuildersReachable,int? DistanceToDistrict,int AccessibleCount,int ValidAccessibleCount,string[] Limitations,NativeConstructionAccess? ConstructionAccess=null);
public sealed record NativeRoad(string Id,string ToId,bool Supported,int SourceAccessCount,int TargetAccessCount,bool? Connected,float? Distance,string[] Limitations);
public sealed record NativeRange(string Id,bool Supported,string[] RangeNames,int Offset,int Limit,int Total,Position[] Items,bool HasMore,string[] Limitations,string Source);
public sealed record GoodHistorySample(int Index,int Cycle,int Day,int Stock,int Capacity,int Production,int Consumption,long NetProduction);
public sealed record NativeGoodHistory(string Good,bool Available,int Offset,int Limit,int Total,GoodHistorySample[] Items,bool HasMore,long PageProduction,long PageConsumption,long? StockChange,string[] Limitations);

public sealed partial class NativeClient
{
    private static void LogisticsEnvelope<T>(BridgeEnvelope<T> e,LogisticsRequest r,string[]? limits)
    {
        if(e.BridgeVersion is not ("0.19.0" or "0.19.1" or "0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or "0.21.1" or "0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1")||r.Session.Length>0&&e.SessionId!=r.Session||limits is null||limits.Any(s=>s is null))throw new InvalidDataException("Invalid logistics envelope");
    }
    private static bool Page(LogisticsRequest r,int offset,int limit,int total,int count,bool more)=>offset==r.Offset&&limit==r.Limit&&total>=0&&count==Math.Min(limit,Math.Max(0,total-offset))&&more==((long)offset+count<total);
    public async Task<BridgeEnvelope<NativeAccess>> BuildingAccess(LogisticsRequest r,CancellationToken ct)
    {
        if(r.Route!="building-access")throw new ArgumentException();
        var e=await Get<NativeAccess>($"building-access?id={r.Id}&session={r.Session}",ct);
        ValidateBuildingAccess(e,r);return e;
    }
    public static void ValidateBuildingAccess(BridgeEnvelope<NativeAccess> e,LogisticsRequest r)
    {
        var d=e.Data;
        LogisticsEnvelope(e,r,d.Limitations);
        if(d.Id!=r.Id||d.Position is null||d.DistanceToDistrict<0||d.AccessibleCount<0||d.ValidAccessibleCount<0||d.ValidAccessibleCount>d.AccessibleCount||d.Finished&&d.BuildersReachable is not null)throw new InvalidDataException("Invalid building access");
        var a=d.ConstructionAccess;
        if(e.BridgeVersion is ("0.31.4" or "0.32.0" or "0.32.1") && a is null)throw new InvalidDataException("Missing construction accesses");
        if(a is not null && (a.State is not ("not_construction" or "observed" or "unavailable") ||
            a.Cells is null || a.Cells.Length>64 || a.Cells.Any(c=>c is null) ||
            a.Cells.Distinct().Count()!=a.Cells.Length ||
            d.Finished!=(a.State=="not_construction") || d.Finished&&a.Expanded ||
            (a.State=="observed")!=(a.Cells.Length>0) || a.Expanded&&a.State!="unavailable"))
            throw new InvalidDataException("Invalid construction accesses");
    }
    public async Task<BridgeEnvelope<NativeRoad>> RoadConnection(LogisticsRequest r,CancellationToken ct)
    {
        if(r.Route!="road-connection")throw new ArgumentException();
        var e=await Get<NativeRoad>($"road-connection?id={r.Id}&toId={r.ToId}&session={r.Session}",ct);var d=e.Data;LogisticsEnvelope(e,r,d.Limitations);
        if(e.BridgeVersion is not ("0.19.1" or "0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or "0.21.1" or "0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1")||d.Id!=r.Id||d.ToId!=r.ToId||d.SourceAccessCount<0||d.TargetAccessCount<0||d.Supported!=(d.Connected is not null)||
            d.Supported&&(d.SourceAccessCount!=1||d.TargetAccessCount!=1)||(d.Connected==true)!=(d.Distance is not null)||d.Distance is not null&&(!float.IsFinite(d.Distance.Value)||d.Distance<0))throw new InvalidDataException("Invalid road observation");return e;
    }
    public async Task<BridgeEnvelope<NativeRange>> WorkRange(LogisticsRequest r,CancellationToken ct)
    {
        if(r.Route!="work-range")throw new ArgumentException();
        var e=await Get<NativeRange>($"work-range?id={r.Id}&session={r.Session}&offset={r.Offset}&limit={r.Limit}",ct);var d=e.Data;LogisticsEnvelope(e,r,d.Limitations);
        if(e.BridgeVersion is not ("0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or "0.21.1" or "0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1")||d.Source is not ("building_terrain_range" or "range_providers" or "unavailable")||d.Supported!=(d.Source!="unavailable")||d.Id!=r.Id||d.Items is null||!Page(r,d.Offset,d.Limit,d.Total,d.Items.Length,d.HasMore)||d.Total>65536||d.RangeNames is null||d.RangeNames.Length>16||d.RangeNames.Any(n=>n is null||n.Length>160)||
            !d.Supported&&d.Total!=0||d.Items.Any(i=>i is null)||!d.Items.SequenceEqual(d.Items.Distinct().OrderBy(p=>p.Z).ThenBy(p=>p.Y).ThenBy(p=>p.X)))throw new InvalidDataException("Invalid range observation");return e;
    }
    public async Task<BridgeEnvelope<NativeGoodHistory>> GoodHistory(LogisticsRequest r,CancellationToken ct)
    {
        if(r.Route!="good-history")throw new ArgumentException();
        var e=await Get<NativeGoodHistory>($"good-history?good={Uri.EscapeDataString(r.Good)}&offset={r.Offset}&limit={r.Limit}",ct);var d=e.Data;LogisticsEnvelope(e,r,d.Limitations);
        if(d.Good!=r.Good||d.Items is null||!Page(r,d.Offset,d.Limit,d.Total,d.Items.Length,d.HasMore)||!d.Available&&d.Total!=0||
            d.Items.Where((i,n)=>i is null||i.Index!=r.Offset+n||i.Cycle<0||i.Day<0||i.Stock<0||i.Capacity<0||i.Production<0||i.Consumption<0||i.NetProduction!=(long)i.Production-i.Consumption).Any()||
            d.PageProduction!=d.Items.Sum(i=>(long)i.Production)||d.PageConsumption!=d.Items.Sum(i=>(long)i.Consumption)||
            d.StockChange!=(d.Items.Length>1?(long?)d.Items[^1].Stock-d.Items[0].Stock:null))throw new InvalidDataException("Invalid goods history");return e;
    }
}
