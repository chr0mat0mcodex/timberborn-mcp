namespace Timberborn.Bridge.Core;

public sealed class BuildingProductionState
{
    public string Id { get; set; } = "";
    public string Template { get; set; } = "";
    public string Recipe { get; set; } = "";
    public int CompletedCycles { get; set; }
    public double? ProvenGameHours { get; set; }
    public double ObservedGameHours { get; set; }
    public string[] Blockers { get; set; } = Array.Empty<string>();
    public string[] SeenBlockers { get; set; } = Array.Empty<string>();
}
public sealed class BuildingProductionSample
{
    public BuildingProductionState[] Buildings { get; set; } = Array.Empty<BuildingProductionState>();
    public string? Failure { get; set; }
    public long JournalCursor { get; set; }
}
public static class BuildingProductionProof
{
    public static readonly string[] Blockers = { "building_paused","no_assigned_workers","outside_working_hours","ingredients_not_ready","fuel_not_ready","output_capacity_unavailable","consumer_inactive","consumer_not_powered","power_network_unknown","observation_unavailable" };
    public static readonly string[] Failures = { "building_configuration_changed","building_unavailable","building_proof_observation_unavailable","building_proof_journal_gap" };
    public static string[] ParseIds(string raw)
    {
        if(raw.Length>8*37)throw new ArgumentException();
        var ids=raw.Split(',').Select(value=>{
            if(!Guid.TryParseExact(value,"D",out var id)||id==Guid.Empty)throw new ArgumentException();return id.ToString("D");
        }).ToArray();
        if(ids.Length is <1 or >8||ids.Distinct().Count()!=ids.Length)throw new ArgumentException();return ids;
    }
    public static string? Observe(SimulationRunStatus status,BuildingProductionSample? sample,bool initial=false)
    {
        if(sample is null || sample.Buildings is null || sample.Buildings.Any(b=>b is null) ||
            !sample.Buildings.Select(b=>b.Id).SequenceEqual(status.BuildingIds) ||
            sample.Failure is not null && !Failures.Contains(sample.Failure) ||
            sample.JournalCursor < status.BuildingJournalCursor ||
            sample.Buildings.Any(b=>!BuildingPolicy.ValidTemplate(b.Template)||!BuildingPolicy.ValidTemplate(b.Recipe)||b.CompletedCycles is <0 or >1||
                initial&&b.CompletedCycles!=0||b.CompletedCycles!=(b.ProvenGameHours is null?0:1)||
                !Finite(b.ObservedGameHours)||b.ObservedGameHours<status.StartGameHours||b.ObservedGameHours>status.ObservedGameHours||
                b.ProvenGameHours is {} h&&(!Finite(h)||h<status.StartGameHours||h>b.ObservedGameHours)||
                b.Blockers is null||b.SeenBlockers is null||b.Blockers.Any(x=>!Blockers.Contains(x))||
                b.SeenBlockers.Any(x=>!Blockers.Contains(x))||b.Blockers.Except(b.SeenBlockers).Any()))
            return "building_proof_observation_unavailable";
        if(!initial && status.BuildingProduction.Where((old,i)=> {
            var next=sample.Buildings[i];
            return old.Template!=next.Template||old.Recipe!=next.Recipe||old.CompletedCycles>next.CompletedCycles||
                old.ProvenGameHours is not null&&old.ProvenGameHours!=next.ProvenGameHours||
                old.ObservedGameHours>next.ObservedGameHours||old.SeenBlockers.Except(next.SeenBlockers).Any();
        }).Any())return "building_proof_observation_unavailable";
        // Own the receipt: later observations or caller mutations cannot rewrite history.
        status.BuildingProduction=sample.Buildings.Select(b=>new BuildingProductionState {
            Id=b.Id,Template=b.Template,Recipe=b.Recipe,CompletedCycles=b.CompletedCycles,
            ProvenGameHours=b.ProvenGameHours,ObservedGameHours=b.ObservedGameHours,
            Blockers=b.Blockers.ToArray(),SeenBlockers=b.SeenBlockers.ToArray()
        }).ToArray();
        status.BuildingJournalCursor=sample.JournalCursor;
        status.BuildingProductionReached=sample.Failure is null&&sample.Buildings.All(b=>b.CompletedCycles==1);
        return sample.Failure;
    }
    private static bool Finite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
}
