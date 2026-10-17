using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record SearchGeometry(string Orientation,Position? Entrance,Position[] OccupiedCells,bool CellsTruncated);
public sealed record BuildingMatch(Guid Id,string Template,Position Position,bool Finished,bool IsPath,bool Production,SearchGeometry? Geometry);
public sealed record BuildingSearch(int Offset,int Limit,int Total,BuildingMatch[] Items,bool HasMore,int NativeReads,string[] Limitations);
public sealed record OperationBatchItem(string Id,string State,NativeOperation? Operation);
public sealed record OperationBatch(OperationBatchItem[] Items,int NativeReads,string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<BuildingSearch>> SearchBuildings(BuildingQueryRequest r,CancellationToken ct)
    {
        string q=$"offset={r.Offset}&limit={r.Limit}&includePaths={r.IncludePaths.ToString().ToLowerInvariant()}&productionOnly={r.ProductionOnly.ToString().ToLowerInvariant()}&detail={(r.Full?"full":"compact")}";
        if(r.Template.Length>0)q+="&template="+Uri.EscapeDataString(r.Template);
        if(r.Finished is {} finished)q+="&finished="+finished.ToString().ToLowerInvariant();
        if(r.HasBounds)q+=$"&x={r.X}&y={r.Y}&z={r.Z}&width={r.Width}&height={r.Height}&depth={r.Depth}";
        var e=await Get<BuildingSearch>("building-search?"+q,ct,maxResponseBytes:1024*1024);
        var d=e.Data;
        if(e.BridgeVersion is not ("0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0") || d.Items is null || d.Limitations is null || d.NativeReads!=1 ||
            d.Offset!=r.Offset||d.Limit!=r.Limit||d.Total<0||d.Items.Length!=Math.Min(r.Limit,Math.Max(0,d.Total-r.Offset))||
            d.HasMore!=((long)r.Offset+d.Items.Length<d.Total))throw new InvalidDataException("Invalid building search page");
        foreach(var item in d.Items) {
            if(item is null||item.Id==Guid.Empty||!BuildingPolicy.ValidTemplate(item.Template)||item.Position is not {} p||
                p.X is <0 or >4095 || p.Y is <0 or >4095 || p.Z is <0 or >4095 ||
                !r.Matches(item.Template,item.Finished,item.IsPath,item.Production,p.X,p.Y,p.Z)||
                r.Full!=(item.Geometry is not null))throw new InvalidDataException("Search filter or detail mismatch");
            if(item.Geometry is {} g && (string.IsNullOrEmpty(g.Orientation)||g.Orientation.Length>32||g.OccupiedCells is null||
                g.OccupiedCells.Length>BuildingPolicy.MaxGeometryCells||g.OccupiedCells.Any(c=>c is null)||
                g.CellsTruncated&&g.OccupiedCells.Length!=BuildingPolicy.MaxGeometryCells))throw new InvalidDataException("Invalid search geometry");
        }
        var ids=d.Items.Select(i=>i.Id).ToArray();
        if(!ids.SequenceEqual(ids.Distinct().Order()))throw new InvalidDataException("Invalid search order");
        return e;
    }

    public async Task<BridgeEnvelope<OperationBatch>> BuildingOperations(BuildingQueryRequest r,CancellationToken ct)
    {
        var e=await Get<OperationBatch>($"building-operations?session={r.Session}&ids={Uri.EscapeDataString(string.Join(",",r.Ids))}{(r.Diagnose?"&diagnose=true":"")}",ct,maxResponseBytes:2*1024*1024);
        var d=e.Data;
        if(e.BridgeVersion is not ("0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0")||e.SessionId!=r.Session||d.Items is null||d.Limitations is null||d.NativeReads!=1||d.Items.Length!=r.Ids.Length)
            throw new InvalidDataException("Invalid operation batch");
        var items=new List<OperationBatchItem>();
        for(int i=0;i<r.Ids.Length;i++) {
            var item=d.Items[i];
            if(item is null||item.Id!=r.Ids[i]||item.State is not ("observed" or "not_found")||
                (item.State=="observed")!=(item.Operation is not null))throw new InvalidDataException("Invalid batch item correlation");
            if(item.Operation is {} operation) {
                ValidateOperation(operation,item.Id,e.BridgeVersion);
                if(r.Diagnose&&operation.Bottleneck is null)throw new InvalidDataException("Missing bottleneck evidence");
                if(operation.Bottleneck is {} evidence)BottleneckContract.Validate(evidence);
                items.Add(item with{Operation=operation with{Assessment=OperationAssessmentBuilder.Read(operation)}});
            } else items.Add(item);
        }
        return e with{Data=d with{Items=items.ToArray()}};
    }
}
