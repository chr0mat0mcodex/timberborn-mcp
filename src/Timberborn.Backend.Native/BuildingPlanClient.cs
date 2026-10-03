using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record BuildingPlanOption(Position Origin,int Rotation,Position Entrance,Position Connection,
    Position[] NewRoadCells,Position[] RouteCells,bool Executable,string[] Reasons,string? PlanKey=null);
public sealed record NativeBuildingPlan(string Template,string DistrictId,Position Origin,int Width,int Height,int Rotation,
    int CheckedCandidates,int RejectedCandidates,int TotalCandidates,bool SearchComplete,string StopReason,
    BuildingPlanOption[] Options,string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeBuildingPlan>> BuildingPlan(BuildingPlanRequest r,CancellationToken ct)
    {
        var e=await Get<NativeBuildingPlan>($"building-plan?template={Uri.EscapeDataString(r.Template)}&districtId={r.DistrictId}&session={r.Session}&x={r.X}&y={r.Y}&z={r.Z}&width={r.Width}&height={r.Height}&rotation={r.Rotation}",ct);
        ValidateBuildingPlan(e,r); return e;
    }
    public static void ValidateBuildingPlan(BridgeEnvelope<NativeBuildingPlan> e,BuildingPlanRequest r)
    {
        var d=e.Data;
        bool Inside(Position? p)=>p is not null&&p.X>=r.X&&p.X<r.X+r.Width&&p.Y>=r.Y&&p.Y<r.Y+r.Height&&p.Z==r.Z;
        bool Route(BuildingPlanOption o)=>o.RouteCells is {Length:>0 and <=64}&&o.NewRoadCells is {Length:<=64}&&
            o.RouteCells.All(Inside)&&o.NewRoadCells.All(Inside)&&o.RouteCells.Distinct().Count()==o.RouteCells.Length&&
            o.NewRoadCells.Distinct().Count()==o.NewRoadCells.Length&&o.NewRoadCells.All(o.RouteCells.Contains)&&
            o.RouteCells[0]==o.Connection&&o.RouteCells[^1]==o.Entrance&&
            o.RouteCells.Zip(o.RouteCells.Skip(1),(a,b)=>Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y)==1).All(v=>v);
        if(e.BridgeVersion is not ("0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0")||e.SessionId!=r.Session||d is null||d.Template!=r.Template||d.DistrictId!=r.DistrictId||
            d.Origin!=new Position(r.X,r.Y,r.Z)||d.Width!=r.Width||d.Height!=r.Height||d.Rotation!=r.Rotation||
            d.TotalCandidates!=r.Width*r.Height||d.CheckedCandidates<1||d.CheckedCandidates>d.TotalCandidates||
            d.Options is null||d.Options.Length>4||d.RejectedCandidates!=d.CheckedCandidates-d.Options.Length||
            d.SearchComplete!=(d.CheckedCandidates==d.TotalCandidates)||d.StopReason!=(d.SearchComplete?"area_exhausted":"option_limit")||
            !d.SearchComplete&&d.Options.Length!=4||d.Limitations is null||d.Limitations.Length>16||d.Limitations.Any(s=>string.IsNullOrEmpty(s)||s.Length>160)||
            d.Options.Any(o=>o is null||o.Executable||o.Rotation!=r.Rotation||!Inside(o.Origin)||!Inside(o.Entrance)||!Inside(o.Connection)||
                e.BridgeVersion is ("0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0")&&!ValidPlanKey(o.PlanKey)||
                o.Reasons is null||!o.Reasons.SequenceEqual(new[]{"joint_game_validation_pending","construction_reachability_unproven","road_protection_incomplete"})||!Route(o)))
            throw new InvalidDataException("Invalid building plan");
    }
    private static bool ValidPlanKey(string? key)=>key is {Length:64}&&key.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
}
