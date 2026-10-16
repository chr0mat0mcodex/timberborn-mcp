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
                Description = "Standardantwort kompakt mit aktuellem Fortschritt; details=true liefert Such-/Verlaufdetails. " + (name switch {
                    Start => "0.63: Plant 1–8 Gebäude gemeinsam in der autorisierten Region samt Wegen und startet genau einen Strukturauftrag. Alle Bauteile werden vor der ersten Baufortschrittsprüfung platziert. Kein Materialbestand als Startsperre. Zusätzliche Regionen sind alternative Gesamtstandorte. Optionale Vegetationsräumung bleibt vorgeschaltet. constructionHours ist ein gemeinsames Baubudget, kein Budget je Gebäude. Ein actionId verfolgt alle Teile bis Fertigbau; bei Fehlern konkrete Einzelzustände, kein Replay. details=true für vollständigen Plan/Beleg.",
                    Advance => "Liest und aktualisiert denselben Strukturauftrag, ohne einzelne Gebäude nachzuschieben. Die native Mod verfolgt den Bau autonom. Nach Gesamtfertigstellung optionale Anfangskonfiguration abschließen. Alte Journale (Schema 1) behalten ihren ursprünglichen Fortsetzungsvertrag. waitSeconds 0–45.",
                    Inspect => "Liest ausschließlich den lokalen gespeicherten Chargenstatus, keinen aktuellen Spielzustand. checkpointOnly=true. Zur Fortsetzung advance verwenden.",
                    _ => "Stoppt die Charge dauerhaft. Schema 2 fordert beim nativen Strukturauftrag Pause an; Beleg anschließend über inspect_large_project lesen. Bei alten Schema-1-Chargen läuft ein vorhandenes Zeitfenster bis zum festen Budget weiter. Keine Wiederholung verlorener Stop-Antworten."
                }),
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
            new BatchHandle(request.Batch.Session, request.Batch.BatchId, request.WaitSeconds, request.Details);
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
                job = new() { Spec = request.Batch, Fingerprint = BuildBatchState.Fingerprint(request.Batch), SchemaVersion = port is IStructureBatchPort ? 2 : 1 };
                store.Save(job); created = true;
            }
        }
        if (job is null) throw new ArgumentException("batch_not_found");
        if (name == Stop && !job.Terminal) {
            job.State = "cancelled"; job.DispatchStopped = true; job.Reason = "dispatch_stopped_existing_run_keeps_its_budget"; store.Save(job);
            if(job.SchemaVersion==2 && job.StructureReceipt is not null && port is IStructureBatchPort structure)
                job.StructureReceipt=await structure.Structure(job,"stop",ct);
            store.Save(job);
        }
        NativeFault? fault = null;
        if ((created || name == Advance) && !job.Terminal) {
            BuildBatchState.Validate(job.Spec, settingsEnabled);
            if (job.Spec.Clearing != "none" && !removalEnabled) throw new ArgumentException("removal_disabled");
            var engine = new BuildBatchEngine(port, store.Save); var watch = Stopwatch.StartNew();
            try {
                for (int i = 0; i < 64 && !job.Terminal; i++) {
                    bool waiting = await engine.Step(job, ct);
                    // waitSeconds limits waiting, not already available state transitions.
                    // Finish/access -> planning -> next order proceeds in the same call;
                    // the 64-transition cap still bounds immediate work, including waitSeconds=0.
                    if (waiting) {
                        if (watch.Elapsed.TotalSeconds >= handle.WaitSeconds) break;
                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, Math.Min(2, handle.WaitSeconds - watch.Elapsed.TotalSeconds))), ct);
                    }
                }
            } catch (Exception ex) when (ex is IOException or InvalidDataException or HttpRequestException or ArgumentException or JsonException or OperationCanceledException) {
                // The durable write-ahead checkpoint remains authoritative, never automatically retry a write.
                job = store.Load(handle.Session, handle.BatchId) ?? job;
                fault = new("batch_interrupted", "Gespeicherten Status prüfen; advance klärt ausstehende Eingriffe lesend. Keine neue batchId als Retry.", false, "ledger_required");
            }
        }
        object report = new { batchId = job.Spec.BatchId, job.State, job.Reason, checkpointOnly = true,
            job.UpdatedAtUtc, finishedCount = job.Index, total = job.Spec.Items.Length, job.Finished,
            currentActionId = job.Index < job.Spec.Items.Length ? job.ActionId : null, job.Selection,
            regionIndex = job.RegionIndex, regionCount = BuildBatchState.Regions(job.Spec).Length,
            region = job.Region, previousRegions = job.PreviousRegions,
            clearance = new { used = job.ClearanceUsed, targets = job.Targets.Length, processed = job.RemovalCursor,
                totalTargets = job.PreviousClearTargets + job.Targets.Length,
                runId = job.ClearanceUsed ? job.ClearanceRunId : null, state = job.ClearanceRun?.State },
            completion = job.Completion is null ? null : new { job.Completion.Outcome, job.Completion.RunId },
            job.Search };
        if (!handle.Details) {
            var current = job.Index < job.Spec.Items.Length && job.Completion?.ActionId == job.ActionId ? job.Completion : null;
            report = new { batchId = job.Spec.BatchId, job.State, job.Reason, checkpointOnly = true, job.UpdatedAtUtc,
                finishedCount = job.Index, total = job.Spec.Items.Length, regionIndex = job.RegionIndex,
                currentActionId = job.Index < job.Spec.Items.Length ? job.ActionId : null,
                progress = current is null ? null : new { current.Outcome, current.CurrentSpeed,
                    finishedObjects = current.Steps.Count(s => s.Finished == true), totalObjects = current.Steps.Length,
                    materialProgress = current.Steps.LastOrDefault()?.MaterialProgress,
                    buildProgress = current.Steps.LastOrDefault()?.BuildProgress,
                    elapsedGameHours = current.Run?.ElapsedGameHours,
                    remainingGameHours = current.Run is null ? (double?)null : Math.Max(0, current.Run.TargetGameHours - current.Run.ObservedGameHours) },
                unconfirmed = job.State is "pending_build" or "pending_build_run" or "pending_build_pause" or "pending_mark" or "pending_clearance_run",
                nextAction = job.Terminal ? "done_or_diagnose_reason" : "advance_existing_batch",
                suggestedPollSeconds = job.Terminal ? 0 : 10,
                clearanceRunId = job.ClearanceRun?.RunId,
                detailsAvailable = true };
        }
        if(job.SchemaVersion==2) {
            var receipt=job.StructureReceipt?.DeepClone().AsObject();
            if(receipt is not null && !handle.Details) ProjectReportPresentation.Compact("inspect_large_project",receipt);
            report=new { batchId=job.Spec.BatchId, actionId=job.StructureActionId, job.State, job.Reason,
                job.UpdatedAtUtc, checkpointOnly=true, total=job.Spec.Items.Length, construction=receipt,
                region=job.Region, job.PreviousRegions,
                plan=handle.Details?job.StructurePlan:null,
                unplaced=job.StructurePlan?["unplaced"],
                nextAction=job.Terminal?"done_or_diagnose_structure":"inspect_large_project_for_live_progress",
                configurationPending=job.State!="completed" && job.Spec.Items.Any(i=>i.InitialStorageGood is not null || i.InitialStorageMode is not null),
                detailsAvailable=true };
        }
        return JsonSerializer.SerializeToNode(new NativeResult<object>(1, fault is null ? "ok" : "error", report,
            new("native", false, job.Spec.Session, null), fault), NativeJson.Options)!.AsObject();
    }
}
