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
        LargeProjectPlanningPolicy.RequireBaseline(baseline.Complete, baseline.Reason);
        var protection=CaptureProtectedSites(r.Steps.SelectMany(s=>Resolve(s).GetSpec<BlockObjectSpec>().GetBlocks(Placement(s))),baseline,
            repair: r.Operation=="run-repair");
        var ids = Enumerable.Range(0, r.Steps.Length).Select(i => EntityId(r.ActionId, i)).ToArray();
        if (entities.Entities.Any(e => ids.Contains(e.EntityId.ToString("D")))) throw new BridgeRejectionException("project_start_entity_conflict");
        var batch = new LargeProjectBatch(r, BatchFingerprint(r), ids);
        // Start owns the one simulation controller; no second runner can compete for speed.
        var run = simulation.StartBatch(r);
        batch.Inspect().Simulation = run;
        if (r.Operation == "run-repair") repairParents.Add(r.ActionId, r.ParentActionId);
        batches.Add(r.ActionId, batch); jobs.Add(r.ActionId, (r, baseline)); activeBatch = r.ActionId;
        protectedSites.Add(r.ActionId,protection);
        nextBatchObservation = 0;
        UpdateBatch(r.ActionId); // Issue the complete ready set while paused, then start simulation.
        return BatchReceipt(r.ActionId);
    }
    public void Update()
    {
        // Legacy start handles now submit once and monitor automatically too.
        foreach (var job in jobs.ToArray()) {
            if(batches.ContainsKey(job.Key) || ledger.Inspect(job.Key).State is "completed" or "stopped" or "unconfirmed") continue;
            Handle(LargeProjectRequest.Parse("/agent-api/v1/large-project-advance", new System.Collections.Specialized.NameValueCollection {
                ["session"]=job.Value.Request.Session, ["actionId"]=job.Key }));
        }
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
            () => GuardWithEvidence(id, () => !faulted && ParallelGuard(id) &&
                RoadProtectionPolicy.LostConnections(job.Baseline.Before, roads.Read(job.Baseline, false)).Length == 0 && RepairIdentitiesValid(id)),
            i => Observe(job.Request.Steps[i], receipt.EntityIds[i], District(job.Request.DistrictId)),
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
        var receipt = batches[id].Inspect();
        var steps = jobs[id].Request.Steps;
        result["parts"] = JArray.FromObject(steps.Select((s, i) => new {
            index = i, id = receipt.EntityIds[i], template = s.Template,
            position = new { x = s.X, y = s.Y, z = s.Z }, state = receipt.PartStates[i],
            reason = receipt.PartReasons[i], exceptionType = receipt.PartExceptionTypes[i], dependsOn = s.DependsOn,
            nextAction = receipt.PartStates[i] switch {
                "rejected" => "correct_part_and_plan_new_job_after_diagnosis",
                "unconfirmed" => "inspect_identity_do_not_repeat",
                "pending" => "await_required_dependency_within_same_job",
                "construction" => "check_materials_workers_and_access",
                "awaiting_access" => "await_structure_access_or_repair_after_stop",
                _ => "none"
            }
        }));
        result["lastValidationFailure"] = validationFailures.TryGetValue(id, out var failure) ? JToken.FromObject(failure) : null;
        result["limitations"] = JArray.FromObject(new[] { "development_pilot_not_complete_construction_preflight", "all_orders_before_observation", "dependencies_are_submission_order_not_finish_barriers", "inspect_never_dispatches", "bounded_time_not_material_delivery_guarantee", "manual_speed_change_interrupts_without_override", "no_retry_rollback_or_demolition", "ledger_lost_on_game_reload", "power_and_production_read_separately" });
        return result;
    }
}
