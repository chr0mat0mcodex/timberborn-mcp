using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeProjectStep(string EntityId,string Template,int X,int Y,int Z,int Rotation,string State);
public sealed record NativeProjectExecution(string ActionId,string PlanKey,string State,string Reason,
    NativeProjectStep[] Steps,bool ConstructionPreflightProven,string[] Limitations);
public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeProjectExecution>> BuildingProjectExecution(BuildingProjectExecutionRequest r,CancellationToken ct)
    {
        string query=$"session={r.Session}&actionId={r.ActionId:D}";
        if(r.Validation is { } v) { var p=v.Plan; query += $"&mode=development_pilot&template={Uri.EscapeDataString(p.Template)}&districtId={p.DistrictId}&x={p.X}&y={p.Y}&z={p.Z}&width={p.Width}&height={p.Height}&rotation={p.Rotation}&optionIndex={v.OptionIndex}&planKey={v.PlanKey}"; }
        var e=await Get<NativeProjectExecution>((r.Validation is null?"building-project-status":"building-project-execute")+"?"+query,ct,r.Validation is null?HttpMethod.Get:HttpMethod.Post);
        ValidateProjectExecution(e,r);return e;
    }
    public static void ValidateProjectExecution(BridgeEnvelope<NativeProjectExecution> e,BuildingProjectExecutionRequest r)
    {
        var d=e.Data;
        if(e.BridgeVersion is not ("0.26.0" or "0.27.0")||e.SessionId!=r.Session||d is null||d.ActionId!=r.ActionId.ToString("D")||
            !ValidPlanKey(d.PlanKey)||r.Validation is not null&&d.PlanKey!=r.Validation.PlanKey||d.ConstructionPreflightProven||
            d.State is not ("running" or "completed" or "stopped" or "unconfirmed")||d.Reason is null||d.Reason.Length>100||
            d.Steps is not {Length:>=1 and <=5}||d.Steps.Any(s=>s is null||!Guid.TryParse(s.EntityId,out var id)||id==Guid.Empty||
                s.X is <0 or >4095||s.Y is <0 or >4095||s.Z is <0 or >4095||s.Rotation is <0 or >3||s.State is not ("pending" or "confirmed" or "unconfirmed"))||
            d.Steps.Select(s=>s.EntityId).Distinct().Count()!=d.Steps.Length||d.Steps[^1].EntityId!=d.ActionId||
            d.Steps[^1].Template!="SmallWarehouse.Folktails"||d.Steps.Take(d.Steps.Length-1).Any(s=>s.Template!="Path"||s.Rotation!=0)||
            d.State=="completed"&&d.Steps.Any(s=>s.State!="confirmed")||
            d.Steps.SkipWhile(s=>s.State=="confirmed").Skip(1).Any(s=>s.State!="pending")||
            d.Limitations is null||d.Limitations.Length>16||d.Limitations.Any(s=>string.IsNullOrEmpty(s)||s.Length>160))
            throw new InvalidDataException("Invalid project execution receipt");
        if (r.Validation is { } validation)
        {
            var p = validation.Plan;
            if (d.Steps.Any(s => s.X < p.X || s.X >= p.X + p.Width || s.Y < p.Y || s.Y >= p.Y + p.Height || s.Z != p.Z) ||
                d.Steps[^1].Rotation != p.Rotation)
                throw new InvalidDataException("Project receipt outside selected region");
        }
    }
}
