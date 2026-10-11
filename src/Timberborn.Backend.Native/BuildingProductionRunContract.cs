using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public static class BuildingProductionRunContract
{
    public static void Validate(NativeSimulationRun d,SimulationRunRequest r,string version)
    {
        var ids=d.BuildingIds??[];var states=d.BuildingProduction??[];
        if(version is ("0.41.0" or "0.42.0" or "0.43.0")&&(d.BuildingIds is null||d.BuildingProduction is null)||ids.Length>8||ids.Distinct().Count()!=ids.Length||
            ids.Any(id=>!Guid.TryParseExact(id,"D",out var guid)||guid==Guid.Empty)||states.Any(s=>s is null)||
            !states.Select(s=>s.Id).SequenceEqual(ids)||d.BuildingJournalCursor<0||
            r.Starts&&!r.BuildingIds.SequenceEqual(ids)||ids.Length>0&&(d.StopAtAvailableLogs is not null||(d.GoodTargets?.Length??0)>0||d.StopAtScience is not null))
            throw new InvalidDataException("invalid_building_production_targets");
        foreach(var s in states) {
            if(!BuildingPolicy.ValidTemplate(s.Template)||!BuildingPolicy.ValidTemplate(s.Recipe)||s.CompletedCycles is <0 or >1||
                s.CompletedCycles!=(s.ProvenGameHours is null?0:1)||!double.IsFinite(s.ObservedGameHours)||s.ObservedGameHours<d.StartGameHours||s.ObservedGameHours>d.ObservedGameHours||
                s.ProvenGameHours is {} h&&(!double.IsFinite(h)||h<d.StartGameHours||h>s.ObservedGameHours)||
                s.Blockers is null||s.SeenBlockers is null||s.Blockers.Length>BuildingProductionProof.Blockers.Length||s.SeenBlockers.Length>BuildingProductionProof.Blockers.Length||
                s.Blockers.Any(b=>!BuildingProductionProof.Blockers.Contains(b))||s.SeenBlockers.Any(b=>!BuildingProductionProof.Blockers.Contains(b))||
                s.Blockers.Except(s.SeenBlockers).Any())throw new InvalidDataException("invalid_building_production_evidence");
        }
        bool proven=ids.Length>0&&states.All(s=>s.CompletedCycles==1);
        if(d.BuildingProductionReached&&!proven||d.Reason=="building_production_proven"&&!d.BuildingProductionReached||
            d.Reason=="building_production_time_limit"&&(ids.Length==0||!d.TargetReached||d.BuildingProductionReached)||
            (BuildingProductionProof.Failures.Contains(d.Reason)&&ids.Length==0)||
            ids.Length==0&&(d.BuildingProductionReached||d.BuildingJournalCursor!=0))throw new InvalidDataException("invalid_building_production_outcome");
    }
}
