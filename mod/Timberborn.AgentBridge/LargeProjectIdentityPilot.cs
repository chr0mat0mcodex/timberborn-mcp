using Newtonsoft.Json.Linq;
using Timberborn.Bridge.Core;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    private readonly ProjectIdentityPilot identityPilot = new();
    public void ClearIdentityPilot() => identityPilot.Clear();
    private bool IdentityPilotApplies(string id) => identityPilot.Active &&
        jobs.TryGetValue(id, out var job) && identityPilot.Session == job.Request.Session && identityPilot.ActionId == id;

    private JObject IdentityPilotEvidence(string id, int index, bool active) => JObject.FromObject(new {
        source = "development_identity_injection", injected = active, active,
        session = jobs[id].Request.Session, actionId = id, partIndex = index,
        injectedObservation = "identity_mismatch", realObserved = RealIdentityObservation(id, index),
        scope = "recovery_observation_only_no_entity_or_ledger_mutation"
    });

    private object SetIdentityPilot(LargeProjectRequest r)
    {
        if (!jobs.TryGetValue(r.ActionId, out var job)) throw new BridgeRejectionException("project_not_found");
        if (job.Request.Session != r.Session) throw new BridgeRejectionException("stale_session");
        if (r.PartIndex >= job.Request.Steps.Length) throw new ArgumentException("invalid_part_index");
        if (r.PilotAction == "disable") identityPilot.Disable(r.Session, r.ActionId, r.PartIndex);
        else
        {
            simulation.RequireBatchAvailable();
            bool idle = activeBatch is null && jobs.Keys.All(TerminalProject);
            identityPilot.Enable(r.Session, r.ActionId, r.PartIndex, Recorded(r.ActionId)[r.PartIndex],
                RealIdentityObservation(r.ActionId, r.PartIndex), TerminalProject(r.ActionId),
                releasedProjects.Contains(r.ActionId), idle);
        }
        return new JObject {
            ["actionId"] = r.ActionId, ["pilotAction"] = r.PilotAction,
            ["identityInjection"] = IdentityPilotEvidence(r.ActionId, r.PartIndex, identityPilot.Active),
            ["limitations"] = new JArray("injected_recovery_guard_evidence_not_natural_identity_failure")
        };
    }

    // Keep the existing native rejection; only its transport gains explicit pilot evidence.
    public object Handle(LargeProjectRequest r)
    {
        try { return HandleCore(r); }
        catch (BridgeRejectionException ex) when (ex.Code == "repair_parent_ineligible" &&
            r.Operation is ("plan-repair" or "run-repair") && IdentityPilotApplies(r.ParentActionId))
        {
            return new JObject {
                ["parentActionId"] = r.ParentActionId, ["rejectionCode"] = ex.Code,
                ["outcome"] = "rejected", ["execution"] = "not_executed",
                ["identityInjection"] = IdentityPilotEvidence(r.ParentActionId, identityPilot.PartIndex, true),
                ["limitations"] = new JArray("injected_recovery_guard_evidence_not_natural_identity_failure")
            };
        }
    }
}
