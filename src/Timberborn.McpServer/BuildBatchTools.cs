using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;

namespace Timberborn.McpServer;

public static class BuildBatchTools
{
    public const string Start = "start_building_batch", Advance = "advance_building_batch",
        Inspect = "inspect_building_batch", Stop = "stop_building_batch";
    public static bool Handles(string name) => name is Start or Advance or Inspect or Stop;
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var json = new JsonSerializerOptions(BuildBatchState.Json) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        foreach (var name in new[] { Start, Advance, Inspect, Stop }) {
            if (!enabled && name is Start or Advance) continue;
            var input = json.GetJsonSchemaAsNode(name == Start ? typeof(BatchStartRequest) : typeof(BatchHandle)).AsObject();
            // The exporter permits null for a reference-type root. MCP requires type="object".
            // Runtime parsing already rejects null; keep the schema equally strict.
            input["type"] = "object";
            yield return new Tool {
                Name = name,
                Description = name switch {
                    Start => "Charge von 1–8 Gebäuden: Basisfläche plus optional bis drei additionalRegions, jede höchstens 8×8. Bei fehlendem Standort nächste Fläche in Listenreihenfolge, kein Rücksprung. Frische Bau-/Zugangsprüfung je Gebäude bis finished_accessible. clearing räumt bei Bedarf einmal je Fläche passende natürliche Vegetation; maxClearTargets gilt insgesamt (höchstens 64). Räum-/Baubudgets zusammen höchstens 672 Stunden. Unbestätigter Eingriff, fehlendes Material oder erschöpftes Räumbudget stoppt die ganze Charge. Gleiche batchId liest nur Status, geänderte Parameter abgelehnt. Danach advance, waitSeconds 0–45.",
                    Advance => "Setzt gespeicherte Baucharge fort: Standortwahl, Räumung, festes Räumzeitfenster und sequenzielle Fertigstellung. waitSeconds 0–45 begrenzt Statuswarten, höchstens 64 Zustandsübergänge. Kein Hintergrunddispatcher; zwischen Aufrufen läuft nur ein bereits gestartetes, begrenztes Spielzeitfenster weiter. Unbestätigte Eingriffe werden ausschließlich lesend geklärt.",
                    Inspect => "Liest ausschließlich den lokalen gespeicherten Chargenstatus, keinen aktuellen Spielzustand. checkpointOnly=true. Zur Fortsetzung advance verwenden.",
                    _ => "Stoppt weitere Aufträge dieser Charge dauerhaft. Ein bereits gestarteter Spielzeitlauf läuft bis zu seinem festen Budget und pausiert dann; kein sofortiger Spielstopp."
                },
                InputSchema = JsonSerializer.SerializeToElement(input),
                OutputSchema = JsonSerializer.SerializeToElement(json.GetJsonSchemaAsNode(typeof(NativeResult<object>))),
                Annotations = new() { ReadOnlyHint = name == Inspect, DestructiveHint = name is Start or Advance,
                    IdempotentHint = name is Inspect or Stop, OpenWorldHint = false }
            };
        }
    }
    public static Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled,
        bool removalEnabled, bool settingsEnabled, CancellationToken ct) => Execute(name, args, enabled, removalEnabled,
            settingsEnabled, new BuildBatchStore(Path.Combine(AppContext.BaseDirectory, ".local", "build-batches")), new NativeBuildBatchPort(client), ct);

    public static async Task<JsonObject> Execute(string name, JsonElement args, bool enabled, bool removalEnabled,
        bool settingsEnabled, BuildBatchStore store, IBuildBatchPort port, CancellationToken ct)
    {
        if (!Handles(name) || !enabled && name is Start or Advance) throw new ArgumentException();
        BatchStartRequest? request = name == Start ? BuildBatchState.Parse<BatchStartRequest>(args) : null;
        if (request is not null && request.Batch is null) throw new ArgumentException("batch_required");
        var handle = request is null ? BuildBatchState.Parse<BatchHandle>(args) :
            new BatchHandle(request.Batch.Session, request.Batch.BatchId, request.WaitSeconds);
        BuildBatchState.Id(handle.Session); BuildBatchState.Id(handle.BatchId);
        if (handle.WaitSeconds is < 0 or > 45 || name is Inspect or Stop && handle.WaitSeconds != 0) throw new ArgumentException();
        if (request is not null) BuildBatchState.Validate(request.Batch, settingsEnabled);
        using var lease = store.Acquire();
        var job = store.Load(handle.Session, handle.BatchId);
        bool created = false;
        if (request is not null) {
            if (job is not null && job.Fingerprint != BuildBatchState.Fingerprint(request.Batch)) throw new ArgumentException("batch_parameters_changed");
            if (job is null) {
                if (request.Batch.Clearing != "none" && !removalEnabled) throw new ArgumentException("removal_disabled");
                if (store.HasActive(handle.Session)) throw new ArgumentException("another_batch_active");
                job = new() { Spec = request.Batch, Fingerprint = BuildBatchState.Fingerprint(request.Batch) };
                store.Save(job); created = true;
            }
        }
        if (job is null) throw new ArgumentException("batch_not_found");
        if (name == Stop && !job.Terminal) {
            job.State = "cancelled"; job.DispatchStopped = true; job.Reason = "dispatch_stopped_existing_run_keeps_its_budget"; store.Save(job);
        }
        NativeFault? fault = null;
        if ((created || name == Advance) && !job.Terminal) {
            BuildBatchState.Validate(job.Spec, settingsEnabled);
            if (job.Spec.Clearing != "none" && !removalEnabled) throw new ArgumentException("removal_disabled");
            var engine = new BuildBatchEngine(port, store.Save); var watch = Stopwatch.StartNew();
            try {
                for (int i = 0; i < 64 && !job.Terminal; i++) {
                    bool waiting = await engine.Step(job, ct);
                    if (watch.Elapsed.TotalSeconds >= handle.WaitSeconds) break;
                    if (waiting) await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, Math.Min(2, handle.WaitSeconds - watch.Elapsed.TotalSeconds))), ct);
                }
            } catch (Exception ex) when (ex is IOException or InvalidDataException or HttpRequestException or ArgumentException or JsonException or OperationCanceledException) {
                // The durable write-ahead checkpoint remains authoritative, never automatically retry a write.
                job = store.Load(handle.Session, handle.BatchId) ?? job;
                fault = new("batch_interrupted", "Gespeicherten Status prüfen; advance klärt ausstehende Eingriffe lesend. Keine neue batchId als Retry.", false);
            }
        }
        var report = new { batchId = job.Spec.BatchId, job.State, job.Reason, checkpointOnly = true,
            job.UpdatedAtUtc, finishedCount = job.Index, total = job.Spec.Items.Length, job.Finished,
            currentActionId = job.Index < job.Spec.Items.Length ? job.ActionId : null, job.Selection,
            regionIndex = job.RegionIndex, regionCount = BuildBatchState.Regions(job.Spec).Length,
            region = job.Region, previousRegions = job.PreviousRegions,
            clearance = new { used = job.ClearanceUsed, targets = job.Targets.Length, processed = job.RemovalCursor,
                totalTargets = job.PreviousClearTargets + job.Targets.Length,
                runId = job.ClearanceUsed ? job.ClearanceRunId : null, state = job.ClearanceRun?.State },
            completion = job.Completion is null ? null : new { job.Completion.Outcome, job.Completion.RunId },
            job.Search };
        return JsonSerializer.SerializeToNode(new NativeResult<object>(1, fault is null ? "ok" : "error", report,
            new("native", false, job.Spec.Session, null), fault), NativeJson.Options)!.AsObject();
    }
}
