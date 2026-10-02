using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeProjectValidation(string Template,string PlanKey,int OptionIndex,BuildingPlanOption Option,
    bool BuildingValid,bool[] RoadValid,int[] RoadStepLostConnections,bool PreviewEntranceConnected,
    NativeRoadProtection RoadProtection,bool NoPersistentChangeObserved,bool SessionLocked,int AttemptsRemaining,
    bool Executable,string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeProjectValidation>> ValidateProject(BuildingProjectValidationRequest r,CancellationToken ct)
    {
        var p=r.Plan;
        var e=await Get<NativeProjectValidation>($"building-project-validation?template={Uri.EscapeDataString(p.Template)}&districtId={p.DistrictId}&session={p.Session}&x={p.X}&y={p.Y}&z={p.Z}&width={p.Width}&height={p.Height}&rotation={p.Rotation}&optionIndex={r.OptionIndex}&planKey={r.PlanKey}",ct,HttpMethod.Post);
        ValidateProjectEvidence(e,r);return e;
    }
    public static void ValidateProjectEvidence(BridgeEnvelope<NativeProjectValidation> e,BuildingProjectValidationRequest r)
    {
        var d=e.Data;var p=r.Plan;
        if(e.BridgeVersion is not ("0.24.1" or "0.25.0")||e.SessionId!=p.Session||d is null||d.Template!=p.Template||d.PlanKey!=r.PlanKey||
            d.OptionIndex!=r.OptionIndex||d.Option is null||d.Option.PlanKey!=r.PlanKey||d.Executable||
            d.RoadValid is null||d.RoadStepLostConnections is null||d.RoadValid.Length>8||
            d.RoadValid.Length!=d.Option.NewRoadCells?.Length||d.RoadStepLostConnections.Length!=d.RoadValid.Length||
            d.RoadStepLostConnections.Any(n=>n<0||n>16384)||d.AttemptsRemaining is <0 or >15||
            d.NoPersistentChangeObserved==d.SessionLocked||d.Limitations is null||d.Limitations.Length>16||
            d.Limitations.Any(s=>string.IsNullOrEmpty(s)||s.Length>160))throw new InvalidDataException("Invalid project preview");
        // Reuse the candidate's complete geometry contract; this is only a local
        // validation adapter, never a synthetic observation returned to the caller.
        int count=p.Width*p.Height;
        ValidateBuildingPlan(new(e.SchemaVersion,e.SessionId,e.ObservedAtUtc,e.BridgeVersion,
            new(p.Template,p.DistrictId,new(p.X,p.Y,p.Z),p.Width,p.Height,p.Rotation,count,count-1,count,true,"area_exhausted",[d.Option],[])),p);
        RoadProtectionContract.Validate(d.RoadProtection,false,true,true);
        if(d.NoPersistentChangeObserved&&!d.RoadProtection.Restored)throw new InvalidDataException("Unrestored project preview");
    }
}
