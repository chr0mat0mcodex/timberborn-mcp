using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.TemplateSystem;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    private readonly Dictionary<string, LargeProjectBatch> batches = new();
    private string? activeBatch;
    private double nextBatchObservation;
    private bool BatchesBlockNewProject => batches.Values.Any(b => !releasedProjects.Contains(b.Inspect().ActionId) && b.Inspect().State != "completed" && !(b.Inspect().State == "stopped" && b.Inspect().Ordered == 0));
    private static string BatchFingerprint(LargeProjectRequest r) => Fingerprint(r) + "|" + r.Operation + "|" + r.ParentActionId + "|" + r.PlanKey + "|" + r.Speed + "|" + r.MaxGameHours + "|" + r.MaxRealSeconds;
    private object StartBatch(LargeProjectRequest r)
    {
        if (batches.TryGetValue(r.ActionId, out var existing))
        {
            if (existing.Fingerprint != BatchFingerprint(r)) throw new BridgeRejectionException("state_conflict");
            return BatchReceipt(r.ActionId); // Same identity never places, restarts or enlarges budgets.
        }
        if (jobs.ContainsKey(r.ActionId)) throw new BridgeRejectionException("project_start_identity_conflict");
        if (activeBatch is not null) throw new BridgeRejectionException("project_start_busy");
        if (jobs.Count >= 128) throw new BridgeRejectionException("project_start_capacity");
        if (r.Operation != "run-repair" && (ledger.BlocksNewProject || BatchesBlockNewProject))
            throw new BridgeRejectionException("project_start_unresolved");
        try { simulation.RequireBatchAvailable(); }
        catch (BridgeRejectionException ex) when (ex.Code == "state_conflict") {
            throw new BridgeRejectionException("project_start_simulation_unavailable");
        }
        JObject plan;
        try { plan = r.Operation == "run-repair" ? PlanRepair(r) : Plan(r); }
        catch (BridgeRejectionException ex) when (ex.Code == "state_conflict") {
            throw new BridgeRejectionException("project_start_preflight_conflict");
        }
        LargeProjectStartPolicy.RequirePlan((string?)plan["planKey"], r.PlanKey, (bool?)plan["pilotEligible"] == true);
        var baseline = roads.Capture();
        if (!baseline.Complete) throw new BridgeRejectionException("project_start_baseline_unavailable");
        var ids = Enumerable.Range(0, r.Steps.Length).Select(i => EntityId(r.ActionId, i)).ToArray();
        if (entities.Entities.Any(e => ids.Contains(e.EntityId.ToString("D")))) throw new BridgeRejectionException("project_start_entity_conflict");
        var batch = new LargeProjectBatch(r, BatchFingerprint(r), ids);
        // Start owns the one simulation controller; no second runner can compete for speed.
        var run = simulation.StartBatch(r);
        batch.Inspect().Simulation = run;
        if (r.Operation == "run-repair") repairParents.Add(r.ActionId, r.ParentActionId);
        batches.Add(r.ActionId, batch); jobs.Add(r.ActionId, (r, baseline)); activeBatch = r.ActionId;
        nextBatchObservation = 0;
        UpdateBatch(r.ActionId); // Issue the complete ready set while paused, then start simulation.
        return BatchReceipt(r.ActionId);
    }
    public void Update()
    {
        if (activeBatch is not { } id || clock.Elapsed.TotalSeconds < nextBatchObservation) return;
        nextBatchObservation = clock.Elapsed.TotalSeconds + 0.25;
        UpdateBatch(id);
    }
    private void UpdateBatch(string id)
    {
        var batch = batches[id]; var receipt = batch.Inspect(); var job = jobs[id];
        // All delegates run synchronously on Unity's main thread, between simulation ticks.
        // Native previews are removed before any placement, even while the simulation is running.
        batch.Tick(clock.Elapsed.TotalSeconds, simulation.InspectBatch(id), simulation.SpeedLocked,
            () => GuardWithEvidence(id, () => !faulted && ConstructionIsolation.Allows(entities, receipt.EntityIds.Where((_, i) => receipt.PartStates[i] != "pending").Concat(RepairOwned(id)), allowOwnedInitialization: true) &&
                RoadProtectionPolicy.LostConnections(job.Baseline.Before, roads.Read(job.Baseline, false)).Length == 0 && RepairIdentitiesValid(id)),
            i => Observe(job.Request.Steps[i], receipt.EntityIds[i], District(job.Request.DistrictId)),
            i => Preflight(job.Request.Steps[i], job.Request.Session, District(job.Request.DistrictId), i, id),
            i => {
                var step = job.Request.Steps[i]; var template = Resolve(step); var spec = template.GetSpec<BlockObjectSpec>();
                placers.GetMatchingPlacer(spec).Place(new EntitySetup.Builder(template.Blueprint).SetId(Guid.Parse(receipt.EntityIds[i])), Placement(step));
            }, () => simulation.CancelBatch(id), () => simulation.BeginBatch(id), () => RepairFinished(id));
        if (batch.Terminal) { simulation.ReleaseBatch(id); if (activeBatch == id) activeBatch = null; }
    }
    private object BatchReceipt(string id)
    {
        var result = JObject.FromObject(batches[id].Inspect(), JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
        result["released"] = releasedProjects.Contains(id);
        result["parentActionId"] = repairParents.TryGetValue(id, out var parent) ? parent : null;
        result["limitations"] = JArray.FromObject(new[] { "development_pilot_not_complete_construction_preflight", "all_ready_orders_then_autonomous_simulation", "explicit_dependencies_wait_for_real_finish", "inspect_never_dispatches", "bounded_time_not_material_delivery_guarantee", "manual_speed_change_interrupts_without_override", "no_retry_rollback_or_demolition", "ledger_lost_on_game_reload", "power_and_production_read_separately" });
        return result;
    }
}
