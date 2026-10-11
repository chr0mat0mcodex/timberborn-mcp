using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public static class ProductionRunContract
{
    public static void Validate(NativeSimulationRun d, SimulationRunRequest request)
    {
        var targets=d.GoodTargets??Array.Empty<ProductionTargetState>();
        bool active=targets.Length>0||d.StopAtScience is not null;
        if (targets.Length>8 || targets.Any(t=>t is null || !BuildingPolicy.ValidTemplate(t.Good) || t.Amount is <1 or >1000000 || t.Available is <0) ||
            targets.Select(t=>t.Good).Distinct(StringComparer.Ordinal).Count()!=targets.Length ||
            d.StopAtScience is <1 or >1000000 || d.ObservedScience is <0 ||
            d.StopAtScience is null && d.ObservedScience is not null || active && d.StopAtAvailableLogs is not null)
            throw new InvalidDataException("invalid_production_targets");
        bool reached=active && targets.All(t=>t.Available>=t.Amount) && (d.StopAtScience is null||d.ObservedScience>=d.StopAtScience);
        if (reached!=d.ProductionTargetsReached || d.Reason=="production_targets_reached" && !reached ||
            d.Reason=="production_time_limit" && (!active||!d.TargetReached||reached) ||
            d.Reason=="production_observation_unavailable" && !active)
            throw new InvalidDataException("invalid_production_outcome");
        if (request.Starts && (request.StopAtScience!=d.StopAtScience ||
            !request.GoodTargets.Select(t=>(t.Good,t.Amount)).SequenceEqual(targets.Select(t=>(t.Good,t.Amount)))))
            throw new InvalidDataException("production_target_mismatch");
        if (!active && (d.ProductionDiagnostics is not null || d.DiagnosticsObservedGameHours is not null))
            throw new InvalidDataException("unexpected_production_diagnostics");
        if (d.DiagnosticsObservedGameHours is { } time && (!double.IsFinite(time)||time<0)) throw new InvalidDataException();
        if (d.ProductionDiagnostics is { } diagnostics)
        {
            var expected=targets.Select(t=>t.Good).Concat(d.StopAtScience is null?Array.Empty<string>():new[]{"$science"}).ToArray();
            if(d.DiagnosticsObservedGameHours is null || diagnostics.Any(x=>x is null) || !diagnostics.Select(x=>x.Good).SequenceEqual(expected)) throw new InvalidDataException("invalid_production_diagnostics");
            foreach(var item in diagnostics)
                if(item.Producers is <0 or >32 || item.Evidence is null || item.Evidence.Length>16 ||
                    item.Evidence.Any(e=>e is not ("no_current_manufactory_source" or "producer_paused" or "no_assigned_workers" or "understaffed" or "outside_working_hours" or "ingredients_not_ready" or "fuel_not_ready" or "output_capacity_unavailable" or "producer_ready" or "network_power_deficit" or "ingredient_global_shortage")) ||
                    item.MissingIngredients is null || item.MissingIngredients.Length>32 || item.MissingIngredients.Any(g=>!BuildingPolicy.ValidTemplate(g)))
                    throw new InvalidDataException("invalid_production_evidence");
        }
    }
}
