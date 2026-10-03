using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeProjectExecution>> VerticalStair(VerticalStairRequest r, CancellationToken ct)
    {
        string q = $"session={r.Session}&actionId={r.ActionId:D}";
        if (r.Start) q += $"&mode={(r.UpperPathCount == 0 ? "single_stair_pilot" : "stair_with_upper_paths_pilot")}&districtId={r.DistrictId}&x={r.X}&y={r.Y}&z={r.Z}&rotation={r.Rotation}&upperPathCount={r.UpperPathCount}";
        var e = await Get<NativeProjectExecution>((r.Start ? "vertical-stair-execute" : "vertical-stair-status") + "?" + q, ct, r.Start ? HttpMethod.Post : HttpMethod.Get);
        var d=e.Data;
        if(e.BridgeVersion!="0.28.1"||e.SessionId!=r.Session||d is null||d.ActionId!=r.ActionId.ToString("D")||d.Steps is null||d.Steps.Length is <1 or >3||r.Start&&d.Steps.Length!=1+r.UpperPathCount||d.Steps[0].Template!="Stairs.Folktails"||d.Steps[0].EntityId!=d.ActionId||d.Steps.Skip(1).Any(s=>s.Template!="Path"||s.Rotation!=0)||r.Start&&(d.Steps[0].X!=r.X||d.Steps[0].Y!=r.Y||d.Steps[0].Z!=r.Z||d.Steps[0].Rotation!=r.Rotation)||d.State is not ("running" or "completed" or "stopped" or "unconfirmed")) throw new InvalidDataException("Invalid vertical stair receipt");
        return e;
    }
}
