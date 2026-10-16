using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<PlantingBatchReport>> PlantingBatch(PlantingBatchRequest r,CancellationToken ct)
    {
        r.Validate();string E(string s)=>Uri.EscapeDataString(s);
        string route=$"planting-{r.Operation}?session={E(r.Session)}&jobId={E(r.JobId)}";
        if(r.Operation!="status")route+="&rectangles="+E(r.Rows());
        if(r.Operation=="apply")route+="&key="+E(r.Key);
        var e=await Get<PlantingBatchReport>(route,ct,r.Operation=="apply"?HttpMethod.Post:HttpMethod.Get,maxResponseBytes:PlantingBatchRequest.MaxResponseBytes);
        if(e.BridgeVersion is not ("0.53.0" or "0.54.0" or "0.55.0")||e.SessionId!=r.Session)throw new InvalidDataException("planting_version_or_session");
        PlantingBatchContract.Validate(e.Data,r);return e;
    }
}
public static class PlantingBatchContract
{
    public static void Validate(PlantingBatchReport d,PlantingBatchRequest r)
    {
        if(d.JobId!=r.JobId||d.Key is null||d.Key.Length!=64||d.Key.Any(c=>!Uri.IsHexDigit(c))||
            d.Outcome is not ("preview" or "applied" or "unconfirmed")||d.Cells is null||d.Cells.Length is <1 or >512||
            (d.Outcome=="unconfirmed"?d.Code!="planting_partial_change_no_retry":d.Code!="")||
            r.Operation=="preview"&&d.Outcome!="preview"||r.Operation!="preview"&&d.Outcome=="preview"||
            r.Operation=="apply"&&d.Key!=r.Key)throw new InvalidDataException("invalid_planting_report");
        var seen=new HashSet<(int,int,int)>();
        foreach(var c in d.Cells) {
            if(c is null||c.Rectangle is <0 or >7||c.X is <0 or >4095||c.Y is <0 or >4095||c.Z is <0 or >4095||
                !BuildingPolicy.ValidTemplate(c.Resource)||!BuildingPolicy.ValidTemplate(c.Existing)||!seen.Add((c.X,c.Y,c.Z))||
                c.Reasons is null||c.Reasons.Length>16||c.Reasons.Any(s=>!BuildingPolicy.ValidTemplate(s))||
                c.Occupants is null||c.Occupants.Length>8||c.Occupants.Any(o=>o is null||!Guid.TryParseExact(o.Id,"D",out var id)||id==Guid.Empty||!BuildingPolicy.ValidTemplate(o.Template)||o.Kind is not ("resource_pile" or "path" or "building" or "vegetation" or "other"))||
                c.Result is not ("excluded" or "already_marked" or "not_attempted" or "marked" or "unconfirmed")||
                c.Eligible&&(c.NativeValid!=true||c.Reachable!=true||c.Reasons.Length!=0)||
                !c.Eligible&&c.Result is not ("excluded" or "already_marked")||
                c.Result=="marked"&&c.Existing!=c.Resource||d.Outcome=="applied"&&c.Eligible&&c.Result!="marked"||
                d.Outcome=="preview"&&c.Eligible&&(c.Existing!="unmarked"||c.Result!="not_attempted")||
                c.Result=="already_marked"&&c.Existing!=c.Resource)throw new InvalidDataException("invalid_planting_cell");
            if(r.Operation!="status") {
                if(c.Rectangle>=r.Rectangles.Length)throw new InvalidDataException("invalid_planting_rectangle");
                var rect=r.Rectangles[c.Rectangle];
                if(c.X<rect.X||c.X>=rect.X+rect.Width||c.Y<rect.Y||c.Y>=rect.Y+rect.Height||c.Z!=rect.Z||c.Resource!=rect.Resource||
                    c.Eligible&&rect.RequireMoist&&c.SoilIsMoist!=true)throw new InvalidDataException("invalid_planting_bounds");
            }
        }
        if(r.Operation!="status"&&d.Cells.Length!=r.Rectangles.Sum(x=>x.Width*x.Height))throw new InvalidDataException("planting_incomplete_cells");
        if(d.Outcome=="unconfirmed"&&d.Cells.Count(c=>c.Result=="unconfirmed")!=1)throw new InvalidDataException("planting_missing_uncertain_cell");
        if(r.Operation=="preview"&&d.Key!=PlantingBatchController.Key(r,d.Cells))throw new InvalidDataException("planting_key_mismatch");
    }
}
