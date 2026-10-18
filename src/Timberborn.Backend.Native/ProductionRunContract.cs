using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ProductionRunContract
{
    public static void Validate(NativeSimulationRun run, SimulationRunRequest request)
    {
        var targets = run.GoodTargets ?? Array.Empty<ProductionTargetState>();
        bool active = targets.Length > 0 || run.StopAtScience is not null;

        // Keep rejection order: malformed targets must never reach outcome/diagnostic checks.
        ValidateTargets(run, targets, active);
        ValidateOutcome(run, targets, active);
        ValidateRequestedTargets(run, request, targets);
        ValidateDiagnostics(run, targets, active);
    }

    private static void ValidateTargets(NativeSimulationRun run, ProductionTargetState[] targets, bool active)
    {
        if (targets.Length > 8 ||
            targets.Any(target => target is null ||
                !BuildingPolicy.ValidTemplate(target.Good) ||
                target.Amount is < 1 or > 1000000 ||
                target.Available is < 0) ||
            targets.Select(target => target.Good).Distinct(StringComparer.Ordinal).Count() != targets.Length ||
            run.StopAtScience is < 1 or > 1000000 ||
            run.ObservedScience is < 0 ||
            run.StopAtScience is null && run.ObservedScience is not null ||
            active && run.StopAtAvailableLogs is not null)
        {
            throw new InvalidDataException("invalid_production_targets");
        }
    }

    private static void ValidateOutcome(NativeSimulationRun run, ProductionTargetState[] targets, bool active)
    {
        // A missing observation is not a reached target. All goals must coincide.
        bool reached = active &&
            targets.All(target => target.Available >= target.Amount) &&
            (run.StopAtScience is null || run.ObservedScience >= run.StopAtScience);

        if (reached != run.ProductionTargetsReached ||
            run.Reason == "production_targets_reached" && !reached ||
            run.Reason == "production_time_limit" && (!active || !run.TargetReached || reached) ||
            run.Reason == "production_observation_unavailable" && !active)
        {
            throw new InvalidDataException("invalid_production_outcome");
        }
    }

    private static void ValidateRequestedTargets(
        NativeSimulationRun run, SimulationRunRequest request, ProductionTargetState[] targets)
    {
        // Only start receipts are bound to requested targets; status/cancel requests carry no goals.
        if (request.Starts &&
            (request.StopAtScience != run.StopAtScience ||
             !request.GoodTargets.Select(target => (target.Good, target.Amount))
                 .SequenceEqual(targets.Select(target => (target.Good, target.Amount)))))
        {
            throw new InvalidDataException("production_target_mismatch");
        }
    }

    private static void ValidateDiagnostics(NativeSimulationRun run, ProductionTargetState[] targets, bool active)
    {
        if (!active && (run.ProductionDiagnostics is not null || run.DiagnosticsObservedGameHours is not null))
            throw new InvalidDataException("unexpected_production_diagnostics");

        if (run.DiagnosticsObservedGameHours is { } time && (!double.IsFinite(time) || time < 0))
            throw new InvalidDataException();

        if (run.ProductionDiagnostics is not { } diagnostics) return;

        var expected = targets.Select(target => target.Good)
            .Concat(run.StopAtScience is null ? Array.Empty<string>() : new[] { "$science" })
            .ToArray();
        if (run.DiagnosticsObservedGameHours is null ||
            diagnostics.Any(item => item is null) ||
            !diagnostics.Select(item => item.Good).SequenceEqual(expected))
        {
            throw new InvalidDataException("invalid_production_diagnostics");
        }

        foreach (var item in diagnostics)
        {
            if (item.Producers is < 0 or > 32 ||
                item.Evidence is null || item.Evidence.Length > 16 ||
                item.Evidence.Any(evidence => evidence is not (
                    "no_current_manufactory_source" or "producer_paused" or "no_assigned_workers" or
                    "understaffed" or "outside_working_hours" or "ingredients_not_ready" or
                    "fuel_not_ready" or "output_capacity_unavailable" or "producer_ready" or
                    "network_power_deficit" or "ingredient_global_shortage")) ||
                item.MissingIngredients is null || item.MissingIngredients.Length > 32 ||
                item.MissingIngredients.Any(good => !BuildingPolicy.ValidTemplate(good)))
            {
                throw new InvalidDataException("invalid_production_evidence");
            }
        }
    }
}
