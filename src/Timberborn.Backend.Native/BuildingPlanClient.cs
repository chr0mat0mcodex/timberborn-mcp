using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record BuildingPlanOption(Position Origin,int Rotation,Position Entrance,Position Connection,
    Position[] NewRoadCells,Position[] RouteCells,bool Executable,string[] Reasons,string? PlanKey=null,Position[]? OccupiedCells=null,Position[]? SolidCells=null,CandidateOccupation[]? CellOccupations=null,OccupationProfile[]? OccupationProfiles=null);
public sealed record CandidateOccupation(Position Position,string Occupation,int? Mask=null);
public sealed record OccupationProfile(int Mask,int[] ConflictsWith,bool Known);
public sealed record BuildingPlanRejection(Position Origin,int Rotation,string[] Reasons);
public sealed record NativeBuildingPlan(string Template,string DistrictId,Position Origin,int Width,int Height,int Rotation,
    int CheckedCandidates,int RejectedCandidates,int TotalCandidates,bool SearchComplete,string StopReason,
    BuildingPlanOption[] Options,string[] Limitations,Dictionary<string,int>? RejectionReasons=null,BuildingPlanRejection[]? RejectedSites=null);

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
        if(d is null)throw new InvalidDataException("Missing building plan");
        if(d.RejectionReasons is {} reasons && (reasons.Count>32 || reasons.Any(p=>
            !BuildingPolicy.ValidTemplate(p.Key) || p.Value<1 || p.Value>d.RejectedCandidates)))
            throw new InvalidDataException("Invalid building rejection coverage");
        bool Inside(Position? p)=>p is not null&&p.X>=r.X&&p.X<r.X+r.Width&&p.Y>=r.Y&&p.Y<r.Y+r.Height&&p.Z==r.Z;
        if(d.RejectedSites is {} sites && (sites.Length>4 || sites.Length>d.RejectedCandidates || sites.Any(s=>
            s is null || !Inside(s.Origin) || s.Rotation!=r.Rotation || s.Reasons is not {Length:>0 and <=32} ||
            s.Reasons.Any(reason=>!BuildingPolicy.ValidTemplate(reason))))) throw new InvalidDataException("Invalid rejected sites");
        if(d.Options is not null && d.Options.Any(o=>o is not null && (
            e.BridgeVersion is ("0.50.0" or "0.51.0" or "0.52.0" or "0.53.0") && o.OccupiedCells is null ||
            o.OccupiedCells is {} cells && (cells.Length is <1 or >512 || cells.Distinct().Count()!=cells.Length ||
                cells.Any(p=>p is null || p.X<r.X || p.X>=r.X+r.Width || p.Y<r.Y || p.Y>=r.Y+r.Height || p.Z is <0 or >4095)))))
            throw new InvalidDataException("Invalid candidate geometry");
        if(d.Options is not null && d.Options.Any(o=>o is not null && (
            e.BridgeVersion is ("0.50.0" or "0.51.0" or "0.52.0" or "0.53.0") && (o.SolidCells is null || o.CellOccupations is null) ||
            o.SolidCells is {} solid && (solid.Length>512 || solid.Distinct().Count()!=solid.Length ||
                solid.Any(c=>c is null || o.OccupiedCells is null || !o.OccupiedCells.Contains(c))) ||
            o.CellOccupations is {} occupations && (occupations.Length is <1 or >512 ||
                occupations.Any(c=>c is null || c.Position is null || c.Occupation is not {Length:>0 and <=160} ||
                    o.OccupiedCells is null || !o.OccupiedCells.Contains(c.Position))))))
            throw new InvalidDataException("Invalid candidate occupation");
        if(d.Options is not null && d.Options.Any(o=>o is not null && o.OccupationProfiles is {} profiles && (
            profiles.Length>512 || profiles.Any(p=>p is null) || profiles.Select(p=>p.Mask).Distinct().Count()!=profiles.Length ||
            profiles.Any(p=>p.ConflictsWith is null || p.ConflictsWith.Length>257 || p.ConflictsWith.Distinct().Count()!=p.ConflictsWith.Length ||
                !p.Known && p.ConflictsWith.Length!=0))))
            throw new InvalidDataException("Invalid occupation profile");
        bool Route(BuildingPlanOption o)=>o.RouteCells is {Length:>0 and <=64}&&o.NewRoadCells is {Length:<=64}&&
            o.RouteCells.All(Inside)&&o.NewRoadCells.All(Inside)&&o.RouteCells.Distinct().Count()==o.RouteCells.Length&&
            o.NewRoadCells.Distinct().Count()==o.NewRoadCells.Length&&o.NewRoadCells.All(o.RouteCells.Contains)&&
            o.RouteCells[0]==o.Connection&&o.RouteCells[^1]==o.Entrance&&
            o.RouteCells.Zip(o.RouteCells.Skip(1),(a,b)=>Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y)==1).All(v=>v);
        if(e.BridgeVersion is not ("0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0")||e.SessionId!=r.Session||d is null||d.Template!=r.Template||d.DistrictId!=r.DistrictId||
            d.Origin!=new Position(r.X,r.Y,r.Z)||d.Width!=r.Width||d.Height!=r.Height||d.Rotation!=r.Rotation||
            d.TotalCandidates!=r.Width*r.Height||d.CheckedCandidates<1||d.CheckedCandidates>d.TotalCandidates||
            d.Options is null||d.Options.Length>4||d.RejectedCandidates!=d.CheckedCandidates-d.Options.Length||
            d.SearchComplete!=(d.CheckedCandidates==d.TotalCandidates)||d.StopReason!=(d.SearchComplete?"area_exhausted":"option_limit")||
            !d.SearchComplete&&d.Options.Length!=4||d.Limitations is null||d.Limitations.Length>16||d.Limitations.Any(s=>string.IsNullOrEmpty(s)||s.Length>160)||
            d.Options.Any(o=>o is null||o.Executable||o.Rotation!=r.Rotation||!Inside(o.Origin)||!Inside(o.Entrance)||!Inside(o.Connection)||
                e.BridgeVersion is ("0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0") && o.NewRoadCells is {Length: > BuildingProjectPilotPolicy.MaxNewRoads} ||
                e.BridgeVersion is ("0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0")&&!ValidPlanKey(o.PlanKey)||
                o.Reasons is null||!o.Reasons.SequenceEqual(new[]{"joint_game_validation_pending","construction_reachability_unproven","road_protection_incomplete"})||!Route(o)))
            throw new InvalidDataException("Invalid building plan");
    }
    private static bool ValidPlanKey(string? key)=>key is {Length:64}&&key.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
}
