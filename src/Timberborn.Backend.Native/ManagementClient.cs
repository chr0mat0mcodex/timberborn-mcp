using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;
public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativePriority>> Priority(ManagementRequest r, CancellationToken ct)
    {
        var route=$"{r.Route}?id={r.Id}&kind={r.Kind}&session={r.Session}";
        if(r.Route=="set-priority")route+=$"&priority={r.Priority}&expectedPriority={r.ExpectedPriority}";
        var result=await Get<NativePriority>(route,ct,r.Route=="set-priority"?HttpMethod.Post:HttpMethod.Get);var d=result.Data;
        if(result.SessionId!=r.Session||d.Id.ToString("D")!=r.Id||d.Kind!=r.Kind||!ManagementRequest.IsPriority(d.Priority)||!ManagementRequest.IsPriority(d.PreviousPriority)||d.Limitations is null||
            (r.Route=="priority" ? d.Outcome!="observed"||d.Priority!=d.PreviousPriority : d.Outcome is not ("applied" or "unconfirmed")||d.PreviousPriority!=r.ExpectedPriority||(d.Outcome=="applied"&&d.Priority!=r.Priority)))throw new InvalidDataException("Invalid priority");return result;
    }
    public async Task<BridgeEnvelope<NativeConstructionList>> Construction(ManagementRequest r,CancellationToken ct)
    {
        var result=await Get<NativeConstructionList>($"construction?offset={r.Offset}&limit={r.Limit}",ct);var d=result.Data;
        if(d.Items is null||d.Total<0||d.Offset!=r.Offset||d.Limit!=r.Limit||d.Items.Length!=Math.Min(r.Limit,Math.Max(0,d.Total-r.Offset))||d.HasMore!=((long)r.Offset+d.Items.Length<d.Total)||d.Limitations is null||
            d.Items.Any(i=>i?.Building is null||!i.Building.Found||i.Building.Id==Guid.Empty||i.Building.Details is not {Finished:false,Unfinished:true,ConstructionComponentPresent:true,Construction:not null}||i.Building.Details.Position is null||string.IsNullOrEmpty(i.Building.Details.Template)||i.Priority is not null&&!ManagementRequest.IsPriority(i.Priority))||d.Items.Select(i=>i.Building.Id).Distinct().Count()!=d.Items.Length)throw new InvalidDataException("Invalid construction list");
        foreach(var i in d.Items){var c=i.Building.Details!.Construction!;if(!float.IsFinite(c.MaterialProgress)||c.MaterialProgress<0||!float.IsFinite(c.BuildTimeProgress)||c.BuildTimeProgress<0||c.Materials is null||!ValidGoods(c.Materials.BuildingCosts)||c.Materials.InventoryAvailable!=(c.Materials.SiteStock is not null)||c.Materials.SiteStock is not null&&!ValidGoods(c.Materials.SiteStock))throw new InvalidDataException("Invalid construction materials");}
        return result;
    }
    public async Task<BridgeEnvelope<NativeAreaTypes>> AreaTypes(CancellationToken ct)
    {
        var result=await Get<NativeAreaTypes>("area-types",ct);var d=result.Data;
        if(d.Kinds is null||d.Kinds.Length!=4||d.Kinds.Any(k=>k is null||!ManagementRequest.IsKind(k.Kind)||string.IsNullOrEmpty(k.Reason))||d.Kinds.Select(k=>k.Kind).Distinct().Count()!=4||d.Plants is null||d.Plants.Length>64||d.Plants.Any(p=>p is null||string.IsNullOrEmpty(p.Resource)||p.Resource.Length>160||p.Kind is not ("crops" or "tree_planting")||string.IsNullOrEmpty(p.ResourceGroup))||d.Plants.Select(p=>p.Resource).Distinct().Count()!=d.Plants.Length||d.Limitations is null)throw new InvalidDataException("Invalid area catalog");return result;
    }
    public async Task<BridgeEnvelope<NativeAreas>> Areas(ManagementRequest r,CancellationToken ct)
    {
        var result=await Get<NativeAreas>($"areas?kind={r.Kind}&offset={r.Offset}&limit={r.Limit}",ct);var d=result.Data;
        if(d.Kind!=r.Kind||d.Offset!=r.Offset||d.Limit!=r.Limit||d.Items is null||!ValidCells(d.Items)||d.Limitations is null||
            (d.Supported?d.Total is null||d.Total<0||d.Items.Length!=Math.Min(r.Limit,Math.Max(0,d.Total.Value-r.Offset))||d.HasMore!=((long)r.Offset+d.Items.Length<d.Total):d.Total is not null||d.Items.Length!=0||d.HasMore))throw new InvalidDataException("Invalid areas");
        if(r.Kind=="tapping"&&(result.BridgeVersion is not ("0.13.2" or "0.14.0" or "0.14.1" or "0.15.0")||d.Items.Any(c=>c.Vegetation?.IsTappingCandidate()!=true)))throw new InvalidDataException("Tapping health unavailable or ineligible");
        return result;
    }
    public async Task<BridgeEnvelope<NativeAreaChange>> SetArea(ManagementRequest r,CancellationToken ct)
    {
        string E(string s)=>Uri.EscapeDataString(s);
        var result=await Get<NativeAreaChange>($"set-area?kind={r.Kind}&operation={r.Operation}&resource={E(r.Resource)}&expectedResource={E(r.ExpectedResource)}&x={r.X}&y={r.Y}&z={r.Z}&width={r.Width}&height={r.Height}&session={r.Session}",ct,HttpMethod.Post);var d=result.Data;
        string desired=r.Operation=="remove"||r.Kind=="tapping"?"unmarked":r.Kind=="tree_cutting"?"marked":r.Resource;
        if(result.SessionId!=r.Session||d.Kind!=r.Kind||d.Operation!=r.Operation||d.Outcome is not ("applied" or "unconfirmed")||d.Items is null||d.Items.Length!=r.Width*r.Height||!ValidCells(d.Items)||d.Items.Any(i=>i.Position.X<r.X||i.Position.X>=r.X+r.Width||i.Position.Y<r.Y||i.Position.Y>=r.Y+r.Height||i.Position.Z!=r.Z||d.Outcome=="applied"&&i.Resource!=desired)||d.Limitations is null)throw new InvalidDataException("Invalid area change");return result;
    }
    private static bool ValidCells(NativeAreaCell[] cells)=>cells.All(c=>c is not null&&c.Position is not null&&c.Position.X>=0&&c.Position.Y>=0&&c.Position.Z>=0&&!string.IsNullOrEmpty(c.Resource)&&c.Resource.Length<=160&&(c.Vegetation is null||c.Vegetation.IsValid()))&&cells.Select(c=>c.Position).Distinct().Count()==cells.Length;
}
