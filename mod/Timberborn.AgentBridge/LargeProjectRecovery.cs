using Newtonsoft.Json.Linq;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.BuildingsReachability;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    private readonly Dictionary<string, object> guardFailures = new();
    private readonly HashSet<string> releasedProjects = new();
    private readonly Dictionary<string, string> repairParents = new();
    private string[] Recorded(string id) => batches.TryGetValue(id, out var batch) ? batch.Inspect().PartStates : ledger.Inspect(id).PartStates;
    private string[] Ids(string id) => batches.TryGetValue(id, out var batch) ? batch.Inspect().EntityIds : ledger.Inspect(id).EntityIds;
    private string State(string id) => batches.TryGetValue(id, out var batch) ? batch.Inspect().State : ledger.Inspect(id).State;
    private bool TerminalProject(string id) => State(id) is "completed" or "blocked" or "stopped" or "unconfirmed" or "interrupted";

    // Identity and access are separate: an unreachable site is still a confirmed placement.
    private string IdentityObservation(string id, int index)
    {
        var step = jobs[id].Request.Steps[index];
        var entityId = Ids(id)[index];
        var e = entities.Entities.SingleOrDefault(e => !e.Deleted && e.EntityId.ToString("D") == entityId);
        if (e is null) return "missing";
        if (!e.Initialized) return "initializing";
        if (!e.TryGetComponent<BlockObject>(out var b) || b.IsPreview ||
            b.Coordinates != new Vector3Int(step.X, step.Y, step.Z) || b.Orientation != Rotation(step.Rotation) ||
            !e.TryGetComponent<TemplateSpec>(out var t) || t.TemplateName != step.Template) return "identity_mismatch";
        if (b.IsUnfinished)
            return e.TryGetComponent<ReachableConstructionSite>(out var site) ?
                site.IsReachableByBuilders() ? "construction" : "construction_unreachable" : "construction_access_unknown";
        if (!b.IsFinished) return "identity_mismatch";
        try { return Observe(step, entityId, District(jobs[id].Request.DistrictId)) == "finished" ? "finished" : "finished_access_missing"; }
        catch (BridgeRejectionException) { return "district_unavailable"; }
    }

    private bool GuardWithEvidence(string id, Func<bool> guard)
    {
        if (guard()) return true;
        var report = Diagnose(id);
        guardFailures[id] = new { reason="guard_changed", faulted, lostConnections=(int)report["lostConnections"]!,
            affected=report["affected"]!.DeepClone(), affectedTruncated=(bool)report["affectedTruncated"]!,
            foreignSites=report["foreignSites"]!.DeepClone(), foreignSitesTruncated=(bool)report["foreignSitesTruncated"]!,
            parts=report["parts"]!.Select(p=>new {index=(int)p["index"]!, observed=p.Value<string>("observed")??"unknown", reason=p.Value<string>("reason")??"unknown"}).ToArray() };
        return false;
    }

    private JObject Diagnose(string id)
    {
        if (!jobs.TryGetValue(id, out var job)) throw new BridgeRejectionException("project_not_found");
        var recorded = Recorded(id); var ids = Ids(id);
        var observations = Enumerable.Range(0, ids.Length).Select(i => IdentityObservation(id, i)).ToArray();
        // Preserve matching identity evidence independently of access and terminal
        // job state. Later absence can then mean removal, never permission to replay.
        for (int i = 0; i < recorded.Length; i++)
            recorded[i] = LargeProjectRecoveryPolicy.ConfirmIdentity(recorded[i], observations[i]);
        var lost = RoadProtectionPolicy.LostConnections(job.Baseline.Before, roads.Read(job.Baseline, false));
        var affected = lost.Take(32).Select(i => {
            var probe = job.Baseline.Probes[i % job.Baseline.Probes.Length];
            return new { id = probe.Id.ToString("D"), position = PowerObservations.Vec(probe.Cell), kind = probe.Kind,
                baselineConnected = true, currentConnected = false };
        }).ToArray();
        var owned = ids.Where((_, i) => recorded[i] != "pending").ToHashSet();
        var others = entities.Entities.Where(e => !e.Deleted && e.TryGetComponent<BlockObject>(out var b) && !b.IsPreview &&
            (!e.Initialized || !b.IsFinished) && !owned.Contains(e.EntityId.ToString("D"))).Take(33).Select(e => e.EntityId.ToString("D")).ToArray();
        bool busy = activeBatch is not null || repairParents.Any(p => p.Value == id && !TerminalProject(p.Key));
        bool ready = TerminalProject(id) && !busy && !faulted && simulation.PauseSettled;
        bool resolved = observations.Select((o,i) => LargeProjectRecoveryPolicy.Resolved(recorded[i], o)).All(v => v);
        bool repairable = observations.Select((o,i) => LargeProjectRecoveryPolicy.Repairable(recorded[i], o)).All(v => v);
        var reader = new BuildingObservations(entities);
        var parts = observations.Select((observed,i) => {
            var step = job.Request.Steps[i];
            var details = JObject.FromObject(reader.Observe(Guid.Parse(ids[i])))["details"] as JObject;
            var construction = details?["construction"] as JObject;
            var costs = construction?["materials"]?["buildingCosts"] as JArray;
            var available = costs?.Select(c => { var good = c.Value<string>("id") ?? throw new InvalidOperationException("missing_good_id"); var count = resources.GetGlobalResourceCount(good); return new { id=good, availableStock=count.AvailableStock, allStock=count.AllStock }; }).ToArray();
            string reason = observed switch {
                "construction_unreachable" => "builders_unreachable",
                "construction" when (bool?)construction?["isOn"] == false => "construction_paused",
                "construction" when (bool?)construction?["hasMaterialsToResumeBuilding"] == false => "materials_not_ready",
                "construction" => "awaiting_builders_or_work",
                "missing" when recorded[i] is "pending" or "rejected" => "not_ordered",
                "missing" when recorded[i] == "unconfirmed" => "placement_effect_unconfirmed",
                "missing" => "previously_observed_object_removed",
                _ => observed
            };
            return new { index = i, id = ids[i], template = step.Template, position = new { x=step.X,y=step.Y,z=step.Z },
                recorded = recorded[i], observed, reason, construction, availableGoods = available };
        }).ToArray();
        return JObject.FromObject(new { actionId=id, state=State(id), released=releasedProjects.Contains(id), currentSpeed=speed.CurrentSpeed,
            closeEligible=ready && resolved, repairEligible=ready && !releasedProjects.Contains(id) && State(id)!="completed" && repairable && others.Length==0,
            lastGuardFailure=guardFailures.TryGetValue(id, out var guardFailure) ? guardFailure : null,
            lastValidationFailure=validationFailures.TryGetValue(id, out var failure) ? failure : null,
            faulted, busy, lostConnections=lost.Length, affected, affectedTruncated=lost.Length>32,
            foreignSites=others.Take(32).ToArray(), foreignSitesTruncated=others.Length>32,
            unissuedParts=recorded.Count(s=>s is "pending" or "rejected"), parts,
            limitations=new[]{"fresh_observation_not_placement","materials_are_total_cost_and_current_stock_not_remaining_delivery","access_and_materials_not_worker_guarantee","close_abandons_unissued_parts_without_demolition","unknown_placement_effects_block_release","ledger_lost_on_game_reload"} });
    }

    private object Recover(string id, bool close)
    {
        var report = Diagnose(id);
        if (!close) return report;
        report["closeOutcome"] = releasedProjects.Contains(id) ? "already_released" : "rejected";
        if (releasedProjects.Contains(id)) return report;
        if ((bool?)report["closeEligible"] != true) return report; // Explicit rejection evidence, no mutation.
        if (!batches.ContainsKey(id)) ledger.Release(id, ((JArray)report["parts"]!).Select(p=>p.Value<string>("observed")??"unknown").ToArray());
        releasedProjects.Add(id);
        report["released"] = true;
        report["repairEligible"] = false;
        report["closeOutcome"] = "released";
        return report;
    }

    private string[] RepairOwned(string id) => repairParents.TryGetValue(id, out var parent) ?
        Ids(parent).Where((_,i)=>Recorded(parent)[i]!="pending").ToArray() : Array.Empty<string>();
    private bool RepairIdentitiesValid(string id) => !repairParents.TryGetValue(id, out var parent) ||
        Enumerable.Range(0,Ids(parent).Length).All(i=>LargeProjectRecoveryPolicy.Repairable(Recorded(parent)[i],IdentityObservation(parent,i)));
    private bool RepairFinished(string id) => !repairParents.TryGetValue(id, out var parent) ||
        Enumerable.Range(0,Ids(parent).Length).All(i=>LargeProjectRecoveryPolicy.Resolved(Recorded(parent)[i],IdentityObservation(parent,i)));

    private JObject PlanRepair(LargeProjectRequest r)
    {
        if (!jobs.ContainsKey(r.ParentActionId)) throw new BridgeRejectionException("repair_parent_ineligible");
        var report = Diagnose(r.ParentActionId);
        if ((bool?)report["repairEligible"] != true) throw new BridgeRejectionException("repair_parent_ineligible");
        if (jobs[r.ParentActionId].Request.DistrictId != r.DistrictId) throw new BridgeRejectionException("repair_district_mismatch");
        if (r.Steps.Any(s=>!LargeProjectRecoveryPolicy.AccessHelper(s.Template))) throw new BridgeRejectionException("repair_helpers_only");
        // Only the chosen parent may be unresolved. No unrelated ledger or nested repair bypass.
        if (repairParents.ContainsKey(r.ParentActionId)) throw new BridgeRejectionException("repair_nested");
        if (jobs.Keys.Any(id=>id!=r.ParentActionId && !releasedProjects.Contains(id) &&
            State(id)!="completed" && !(State(id)=="stopped" && Recorded(id).All(s=>s=="pending")))) throw new BridgeRejectionException("repair_other_project_unresolved");
        var owned = Ids(r.ParentActionId).Where((_,i)=>Recorded(r.ParentActionId)[i]!="pending").ToArray();
        var plan = Plan(r, owned);
        plan["parentActionId"] = r.ParentActionId;
        return plan;
    }
}
