using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.MechanicalSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
using Timberborn.ResourceCountingSystem;

namespace Timberborn.AgentBridge;

// Queried on explicit status reads, not on every simulation tick. Evidence is current,
// whereas target stock in the terminal run receipt is frozen at its stop condition.
public static class ProductionRunObservations
{
    public static ProductionDiagnostic[] Read(EntityRegistry entities, ResourceCountingService resources, SimulationRunStatus run)
    {
        var producers = entities.Entities.Where(e => e.Initialized && !e.Deleted &&
            e.TryGetComponent<BlockObject>(out var b) && b.IsFinished && !b.IsPreview &&
            e.TryGetComponent<Manufactory>(out var m) && m.HasCurrentRecipe).ToArray();
        var keys = run.GoodTargets.Select(t => t.Good).Concat(run.StopAtScience is null ? Array.Empty<string>() : new[] { "$science" });
        return keys.Select(good => {
            var matching = producers.Where(e => {
                var recipe = e.GetComponent<Manufactory>().CurrentRecipe;
                return good == "$science" ? recipe.ProducedSciencePoints > 0 : recipe.Products.Any(p => p.Id == good);
            }).Take(33).ToArray();
            var evidence = new HashSet<string>(StringComparer.Ordinal);
            var missing = new HashSet<string>(StringComparer.Ordinal);
            if (matching.Length == 0) evidence.Add("no_current_manufactory_source");
            foreach (var e in matching.Take(32))
            {
                var m = e.GetComponent<Manufactory>();
                if (e.TryGetComponent<PausableBuilding>(out var pause) && pause.Paused) evidence.Add("producer_paused");
                if (e.TryGetComponent<Workplace>(out var w)) {
                    if (w.NumberOfAssignedWorkers == 0) evidence.Add("no_assigned_workers");
                    else if (w.Understaffed) evidence.Add("understaffed");
                }
                if (e.TryGetComponent<WorkplaceWorkingHours>(out var hours) && !hours.AreWorkingHours) evidence.Add("outside_working_hours");
                if (!m.HasAllIngredients) evidence.Add("ingredients_not_ready");
                if (m.CurrentRecipe.ConsumesFuel && !m.HasFuel) evidence.Add("fuel_not_ready");
                if (!m.HasUnreservedCapacityForCurrentProducts()) evidence.Add("output_capacity_unavailable");
                if (m.IsReadyToProduce) evidence.Add("producer_ready");
                foreach (var ingredient in m.CurrentRecipe.Ingredients)
                    if (resources.GetGlobalResourceCount(ingredient.Id).AvailableStock < ingredient.Amount) missing.Add(ingredient.Id);
                if (e.TryGetComponent<MechanicalNode>(out var node) && node.Graph is { } graph && graph.Valid && graph.PowerDemand > graph.PowerSupply)
                    evidence.Add("network_power_deficit");
            }
            if (missing.Count > 0) evidence.Add("ingredient_global_shortage");
            return new ProductionDiagnostic { Good=good, Producers=Math.Min(32,matching.Length),
                Complete=matching.Length<=32 && missing.Count<=32, Evidence=evidence.OrderBy(x=>x,StringComparer.Ordinal).ToArray(),
                MissingIngredients=missing.OrderBy(x=>x,StringComparer.Ordinal).Take(32).ToArray() };
        }).ToArray();
    }
}
