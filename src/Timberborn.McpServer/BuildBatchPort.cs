using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public interface IBuildBatchPort
{
    Task<NativeSimulation> Current(BuildBatchSpec s, CancellationToken ct);
    Task<string?> Prerequisite(BuildBatchSpec s, int index, CancellationToken ct);
    Task<NativeBuildingPlan> Plan(BuildBatchSpec s, int index, int rotation, CancellationToken ct);
    Task<NativeRemovalTarget[]> Targets(BuildBatchSpec s, CancellationToken ct);
    Task<NativeRemoval> Mark(BuildBatchSpec s, NativeRemovalTarget target, CancellationToken ct);
    Task<NativeSimulationRun> ClearanceRun(BuildBatchJob job, bool start, CancellationToken ct);
    Task<NativeProjectExecution> Submit(BuildBatchJob job, CancellationToken ct);
    Task<CompletionReport> Completion(BuildBatchJob job, bool advance, CancellationToken ct);
}

public sealed class NativeBuildBatchPort(NativeClient client) : IBuildBatchPort
{
    private T Check<T>(BuildBatchSpec s, BridgeEnvelope<T> e) {
        if (e.SessionId != s.Session) throw new BridgeRejectionException("stale_session");
        return e.Data;
    }
    private static NameValueCollection Query(params (string Key, object Value)[] fields) {
        var q = new NameValueCollection();
        foreach (var (key, value) in fields) q[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
        return q;
    }
    private static NameValueCollection PlanQuery(BuildBatchSpec s, int index, int rotation) => Query(
        ("session", s.Session), ("districtId", s.DistrictId), ("template", s.Items[index].Template),
        ("x", s.X), ("y", s.Y), ("z", s.Z), ("width", s.Width), ("height", s.Height), ("rotation", rotation));
    public async Task<NativeSimulation> Current(BuildBatchSpec s, CancellationToken ct) => Check(s, await client.Simulation(ct));
    public async Task<string?> Prerequisite(BuildBatchSpec s, int index, CancellationToken ct)
    {
        // Fresh per building: unlock/material availability can change during earlier construction.
        for (int offset = 0; offset < 1024; offset += 32) {
            var page = Check(s, await client.BuildingCatalog(BridgeRequest.Parse("/agent-api/v1/building-catalog", Query(("offset", offset), ("limit", 32))), ct));
            var item = page.Items.SingleOrDefault(i => i.Template == s.Items[index].Template);
            if (item is not null) return !item.Available ? "template_disabled" : !item.Unlocked ? "template_locked" :
                !item.Supported ? "template_unsupported" : item.Costs.Any(c => c.AvailableGlobally < c.Required) ? "materials_missing" : null;
            if (!page.HasMore) return "template_missing";
        }
        return "catalog_bound_reached";
    }
    public async Task<NativeBuildingPlan> Plan(BuildBatchSpec s, int index, int rotation, CancellationToken ct) =>
        Check(s, await client.BuildingPlan(BuildingPlanRequest.Parse(PlanQuery(s, index, rotation)), ct));
    public async Task<NativeRemovalTarget[]> Targets(BuildBatchSpec s, CancellationToken ct)
    {
        var items = new List<NativeRemovalTarget>(); int? total = null;
        for (int offset = 0; offset < 128; offset += 32) {
            var q = Query(("kind", "all"), ("x", s.X), ("y", s.Y), ("z", s.Z), ("width", s.Width), ("height", s.Height),
                ("depth", 1), ("offset", offset), ("limit", 32));
            var p = Check(s, await client.RemovalTargets(RemovalRequest.Parse("/agent-api/v1/removal-targets", q), ct));
            if (total is not null && total != p.Total || p.Total > 128) throw new InvalidDataException("unstable_or_large_target_page");
            total = p.Total; items.AddRange(p.Items);
            if (!p.HasMore) {
                if (items.Count != p.Total || items.Select(i => i.Id).Distinct().Count() != items.Count) throw new InvalidDataException("incomplete_or_duplicate_targets");
                return items.ToArray();
            }
        }
        throw new InvalidDataException("target_scan_bound");
    }
    public async Task<NativeRemoval> Mark(BuildBatchSpec s, NativeRemovalTarget t, CancellationToken ct) => Check(s,
        await client.RemoveObject(RemovalRequest.Parse("/agent-api/v1/remove-object", Query(("kind", "vegetation"), ("id", t.Id.ToString("D")),
            ("session", s.Session), ("template", t.Template), ("x", t.Position.X), ("y", t.Position.Y), ("z", t.Position.Z),
            ("operation", "mark"), ("expectedMarked", t.Marked ? "true" : "false"))), ct));
    public async Task<NativeSimulationRun> ClearanceRun(BuildBatchJob job, bool start, CancellationToken ct)
    {
        var s = job.Spec;
        var q = Query(("session", s.Session), ("runId", job.ClearanceRunId));
        if (start) {
            q["duration"] = s.ClearanceHours.ToString(CultureInfo.InvariantCulture); q["unit"] = "hours";
            q["speed"] = s.Speed.ToString(CultureInfo.InvariantCulture); q["expectedSpeed"] = "0";
            q["maxRealSeconds"] = s.MaxRealSeconds.ToString(CultureInfo.InvariantCulture);
        }
        return Check(s, await client.SimulationRun(SimulationRunRequest.Parse("/agent-api/v1/" + (start ? "simulation-run-for" : "simulation-run"), q), q, ct));
    }
    public async Task<NativeProjectExecution> Submit(BuildBatchJob job, CancellationToken ct)
    {
        var selected = job.Selection ?? throw new InvalidDataException(); var s = job.Spec;
        var q = PlanQuery(s, job.Index, selected.Rotation);
        q["actionId"] = job.ActionId; q["mode"] = "development_pilot";
        q["optionIndex"] = selected.OptionIndex.ToString(CultureInfo.InvariantCulture); q["planKey"] = selected.PlanKey;
        if (s.Items[job.Index].InitialStorageGood is { } good) q["initialStorageGood"] = good;
        if (s.Items[job.Index].InitialStorageMode is { } mode) q["initialStorageMode"] = mode;
        return Check(s, await client.BuildingProjectExecution(BuildingProjectExecutionRequest.Parse(true, q), ct));
    }
    public async Task<CompletionReport> Completion(BuildBatchJob job, bool advance, CancellationToken ct)
    {
        var s = job.Spec;
        var result = await BuildingCompletionTools.Execute(new(s.Session, job.ActionId, s.ConstructionHours, s.Speed, s.MaxRealSeconds, 0, advance),
            client.BuildingProjectExecution, client.Building, client.BuildingAccess, client.BuildingSettings, client.SimulationRun,
            client.Simulation, Task.Delay, ct);
        var envelope = result.Deserialize<NativeResult<CompletionReport>>(NativeJson.Options)!;
        if (envelope.Meta.SessionId != s.Session || envelope.Data is null) throw new InvalidDataException("invalid_completion_envelope");
        return envelope.Data;
    }
}
