using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;
public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeRemovalTargets>> RemovalTargets(RemovalRequest r,CancellationToken ct)
    {
        var result=await Get<NativeRemovalTargets>($"removal-targets?kind={r.Kind}&x={r.X}&y={r.Y}&z={r.Z}&width={r.Width}&height={r.Height}&depth={r.Depth}&offset={r.Offset}&limit={r.Limit}",ct);
        var d=result.Data;
        if(d.Kind!=r.Kind||d.Offset!=r.Offset||d.Limit!=r.Limit||d.Total<0||d.Items is null||d.Items.Length!=Math.Min(r.Limit,Math.Max(0,d.Total-r.Offset))||d.HasMore!=((long)r.Offset+d.Items.Length<d.Total)||d.Limitations is null||d.Items.Any(i=>i is null||i.Id==Guid.Empty||i.Position is null||i.Position.X<0||i.Position.Y<0||i.Position.Z<0||string.IsNullOrEmpty(i.Template)||(!RemovalRequest.IsKind(i.Kind)&&i.Kind!="other")||(r.Kind!="all"&&i.Kind!=r.Kind)||i.Mode is not ("delete" or "demolition_mark" or "unsupported")||i.PlantOrigin!="unknown"||string.IsNullOrEmpty(i.Classification))||d.Items.Select(i=>i.Id).Distinct().Count()!=d.Items.Length)throw new InvalidDataException("Invalid removal targets");
        if(d.Items.Any(i=>(i.Vegetation is not null&&!i.Vegetation.IsValid())||(result.BridgeVersion is "0.13.2" or "0.14.0" or "0.14.1" or "0.15.0" or "0.16.0"&&(i.Kind is "planted" or "vegetation")&&i.Vegetation is null)))throw new InvalidDataException("Missing or invalid vegetation state");
        return result;
    }
    public async Task<BridgeEnvelope<NativeRemoval>> RemoveObject(RemovalRequest r,CancellationToken ct)
    {
        var result=await Get<NativeRemoval>($"remove-object?kind={r.Kind}&id={r.Id}&session={r.Session}&template={Uri.EscapeDataString(r.Template)}&x={r.X}&y={r.Y}&z={r.Z}&operation={r.Operation}&expectedMarked={r.ExpectedMarked.ToString().ToLowerInvariant()}",ct,HttpMethod.Post);
        var d=result.Data;
        if(result.SessionId!=r.Session||d.Id.ToString("D")!=r.Id||d.Kind!=r.Kind||d.Template!=r.Template||d.Position!=new Position(r.X,r.Y,r.Z)||d.Operation!=r.Operation||d.Outcome is not ("applied" or "unconfirmed")||d.Limitations is null||(d.Removed?d.Marked is not null:d.Marked is null)||(d.Outcome=="applied"&&(r.Operation=="delete"?!d.Removed:r.Operation=="mark"?!d.Removed&&d.Marked!=true:d.Removed||d.Marked!=false)))throw new InvalidDataException("Invalid removal receipt");
        return result;
    }
}
