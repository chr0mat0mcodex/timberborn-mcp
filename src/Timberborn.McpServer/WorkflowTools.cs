using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record RemovalBatchItem(string Id, string Template, Position Position, string Operation,
    string Outcome, bool? Removed, bool? Marked, string? ErrorCode);
public sealed record RemovalBatchResult(string Outcome, int Requested, int Attempted, int Applied,
    int PendingWorkerOrders, RemovalBatchItem[] Items, string[] Limitations);
public sealed record BuildingStartResult(string ActionId, string Outcome, bool RequestSubmitted,
    int? CheckedCandidates, string? SearchStopReason, BuildingPlanOption? SelectedOption,
    NativeProjectExecution? Execution, string[] Limitations)
{
    public OriginDiagnosis? OriginDiagnosis { get; init; }
}
public sealed record OriginDiagnosis(Position Origin, string Assessment, string[] Reasons,
    SiteCell[] BlockedCells, Position? Entrance, bool? PathAtEntrance, string[]? EntranceOccupants,
    string Scope = "search_origin_only_not_whole_region");

// Bounded orchestration of the existing validated native clients, never a second placement path.
public static class WorkflowTools
{
    public const string Removal = "remove_vegetation_batch";
    public const string Build = "start_building_project";
    public const int MaxTargets = 16;
    public static bool Handles(string name) => name is Removal or Build;

    public static IEnumerable<Tool> Catalog(bool removalEnabled, bool buildingEnabled)
    {
        var json = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        if (removalEnabled) {
            var single = RemovalTools.Catalog(true).Single(t => t.Name == "remove_vegetation");
            var item = JsonNode.Parse(single.InputSchema.GetRawText())!.AsObject();
            item["properties"]!.AsObject().Remove("session");
            item["properties"]!.AsObject().Remove("operation");
            item["required"] = new JsonArray(item["properties"]!.AsObject().Select(p => (JsonNode?)JsonValue.Create(p.Key)).ToArray());
            var input = new JsonObject {
                ["type"] = "object", ["additionalProperties"] = false,
                ["properties"] = new JsonObject {
                    ["session"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" },
                    ["operation"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("mark", "unmark") },
                    ["targets"] = new JsonObject { ["type"] = "array", ["minItems"] = 1, ["maxItems"] = MaxTargets, ["items"] = item } },
                ["required"] = new JsonArray("session", "operation", "targets") };
            yield return Tool(Removal, "Bis 16 zuvor gelesene Vegetationsziele seriell markieren/entmarkieren. Alle Parameter vorab prüfen; jede native ID-/Positions-/Session-/Zustandsprüfung bleibt aktiv. Erstes abgelehntes oder unbestätigtes Ziel stoppt den Rest. Kein atomarer Auftrag, kein Rollback/Retry, keine automatische Fortsetzung. Einzelbelege: removed=true ist entfernt; marked=true nur angenommener Arbeiterauftrag. Bei Verbindungsverlust Ziele lesend prüfen, Stapel nicht erneut senden. Keine Gebäude, Schutt oder planted-Ziele.", input, typeof(NativeResult<RemovalBatchResult>));
        }
        if (buildingEnabled) {
            var single = BuildingTools.Catalog(true).Single(t => t.Name == "execute_building_project_pilot");
            var input = JsonNode.Parse(single.InputSchema.GetRawText())!.AsObject();
            var props = input["properties"]!.AsObject();
            props.Remove("optionIndex"); props.Remove("planKey");
            props["selection"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("first_candidate") };
            input["required"] = new JsonArray(props.Where(p => p.Key is not ("initialStorageGood" or "initialStorageMode"))
                .Select(p => (JsonNode?)JsonValue.Create(p.Key)).ToArray());
            yield return Tool(Build, "Plant und startet EIN Gebäudeprojekt in einem Aufruf. selection=first_candidate autorisiert ausschließlich den ersten deterministischen Kandidaten innerhalb der angegebenen Suchfläche/Drehung. Kein Kandidat: kein Start; abgelehnter Kandidat: Stopp, kein Weitersuchen. mode=development_pilot erforderlich; bestehende native gemeinsame Vorschau, Zugangsprüfungen, Pause, Baustellensperre und Konfigurationsprüfungen bleiben aktiv. Neue actionId; kein automatischer Retry. Bei Unsicherheit inspect_building_project mit actionId/session lesen. Ergebnis bestätigt höchstens den Start, nicht Fertigbau; Simulation und Bauabschluss separat. Optional initialStorageGood/Mode direkt an Baustelle wie im bisherigen Pilot.", input, typeof(NativeResult<BuildingStartResult>));
        }
        Tool Tool(string name, string description, JsonObject input, Type output) => new() {
            Name = name, Description = description, InputSchema = JsonSerializer.SerializeToElement(input),
            OutputSchema = JsonSerializer.SerializeToElement(json.GetJsonSchemaAsNode(output)),
            Annotations = new() { ReadOnlyHint = false, DestructiveHint = true, IdempotentHint = false, OpenWorldHint = false } };
    }

    private static NameValueCollection Query(JsonElement args)
    {
        if (args.ValueKind != JsonValueKind.Object) throw new ArgumentException();
        var q = new NameValueCollection();
        foreach (var p in args.EnumerateObject()) {
            if (q.GetValues(p.Name) is not null) throw new ArgumentException();
            q.Add(p.Name, p.Value.ValueKind switch {
                JsonValueKind.String => p.Value.GetString(),
                JsonValueKind.Number when p.Value.TryGetInt32(out int n) => n.ToString(CultureInfo.InvariantCulture),
                JsonValueKind.True => "true", JsonValueKind.False => "false", _ => throw new ArgumentException() });
        }
        return q;
    }

    public static RemovalRequest[] ParseRemoval(JsonElement args)
    {
        if (args.ValueKind != JsonValueKind.Object || args.EnumerateObject().Count() != 3 ||
            !args.TryGetProperty("session", out var session) || session.ValueKind != JsonValueKind.String ||
            !args.TryGetProperty("operation", out var operation) || operation.ValueKind != JsonValueKind.String ||
            !args.TryGetProperty("targets", out var targets) || targets.ValueKind != JsonValueKind.Array ||
            targets.GetArrayLength() is < 1 or > MaxTargets) throw new ArgumentException();
        var requests = targets.EnumerateArray().Select(target => {
            // Enforce JSON types as well as native query validation.
            if (target.ValueKind != JsonValueKind.Object || target.EnumerateObject().Count() != 6) throw new ArgumentException();
            foreach (var p in target.EnumerateObject()) {
                if (p.Name is "x" or "y" or "z") { if (p.Value.ValueKind != JsonValueKind.Number) throw new ArgumentException(); }
                else if (p.Name == "expectedMarked") { if (p.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new ArgumentException(); }
                else if (p.Name is not ("id" or "template") || p.Value.ValueKind != JsonValueKind.String) throw new ArgumentException();
            }
            var q = Query(target); q.Add("kind", "vegetation"); q.Add("session", session.GetString()); q.Add("operation", operation.GetString());
            return RemovalRequest.Parse("/agent-api/v1/remove-object", q);
        }).ToArray();
        if (requests.Select(r => r.Id).Distinct().Count() != requests.Length) throw new ArgumentException();
        return requests;
    }

    public static NameValueCollection ParseBuild(JsonElement args, bool settingsEnabled)
    {
        foreach (var p in args.EnumerateObject()) {
            if (p.Name is "x" or "y" or "z" or "width" or "height" or "rotation") {
                if (p.Value.ValueKind != JsonValueKind.Number) throw new ArgumentException();
            } else if (p.Value.ValueKind != JsonValueKind.String) throw new ArgumentException();
        }
        var q = Query(args);
        if (q["selection"] != "first_candidate" || q.GetValues("optionIndex") is not null || q.GetValues("planKey") is not null ||
            !settingsEnabled && (q.GetValues("initialStorageGood") is not null || q.GetValues("initialStorageMode") is not null)) throw new ArgumentException();
        q.Remove("selection"); q.Add("optionIndex", "0"); q.Add("planKey", new string('0', 64));
        _ = BuildingProjectExecutionRequest.Parse(true, q); // Complete validation before the first native read/write.
        return q;
    }

    public static Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args,
        bool removalEnabled, bool buildingEnabled, bool settingsEnabled, CancellationToken ct)
    {
        if (name == Removal && removalEnabled) return Remove(ParseRemoval(args), client.RemoveObject, ct);
        if (name == Build && buildingEnabled) return Start(ParseBuild(args, settingsEnabled), client.BuildingPlan, client.BuildingProjectExecution, ct, client.Precheck);
        throw new ArgumentException();
    }

    public static async Task<JsonObject> Remove(RemovalRequest[] requests,
        Func<RemovalRequest, CancellationToken, Task<BridgeEnvelope<NativeRemoval>>> remove, CancellationToken ct)
    {
        var items = new List<RemovalBatchItem>();
        NativeFault? fault = null; DateTimeOffset? observed = null; int attempted = 0;
        foreach (var request in requests) {
            if (fault is not null || ct.IsCancellationRequested) {
                fault ??= new("cancelled", "Rest nicht gesendet; bereits ausgeführte Schritte bleiben erhalten.", false);
                items.Add(Item(request, "not_attempted", null, null, null)); continue;
            }
            try {
                attempted++;
                var receipt = await remove(request, ct); observed = receipt.ObservedAtUtc;
                var d = receipt.Data;
                items.Add(Item(request, d.Outcome, d.Removed, d.Marked, null));
                if (d.Outcome != "applied") fault = new("unconfirmed", "Zielzustand unbestätigt; Rest nicht gesendet. Nur lesend klären.", false);
            } catch (Exception ex) when (Known(ex)) {
                bool rejected = ex is BridgeRejectionException;
                fault = Fault(ex);
                items.Add(Item(request, rejected ? "rejected" : "unconfirmed", null, null, fault.Code));
            }
        }
        var outcome = fault is null ? "applied" : items.Any(i => i.Outcome == "unconfirmed") ? "unconfirmed" : "rejected";
        return Wrap(new RemovalBatchResult(outcome, requests.Length, attempted, items.Count(i => i.Outcome == "applied"),
            items.Count(i => i.Outcome == "applied" && i.Removed == false && i.Marked == true), items.ToArray(),
            ["sequential_not_atomic", "stop_on_first_failure", "no_retry_or_rollback", "marked_is_not_removed", "planting_designation_retained"]),
            requests[0].Session, observed, fault);
    }

    public static async Task<JsonObject> Start(NameValueCollection q,
        Func<BuildingPlanRequest, CancellationToken, Task<BridgeEnvelope<NativeBuildingPlan>>> plan,
        Func<BuildingProjectExecutionRequest, CancellationToken, Task<BridgeEnvelope<NativeProjectExecution>>> execute, CancellationToken ct,
        Func<BridgeRequest, CancellationToken, Task<BridgeEnvelope<NativeSite>>>? precheck = null)
    {
        var validated = BuildingProjectExecutionRequest.Parse(true, q);
        var p = validated.Validation!.Plan;
        var planned = await plan(p, ct);
        var selected = planned.Data.Options.FirstOrDefault();
        var limits = new[] { "one_first_candidate_only", "native_execution_guards_unchanged", "construction_preflight_unproven",
            "not_construction_completion", "no_retry_use_inspect_building_project" };
        if (selected is null) {
            OriginDiagnosis? diagnosis = null;
            if (precheck is not null) {
                var siteQuery = new NameValueCollection();
                foreach (var key in new[] { "template", "x", "y", "z", "rotation" }) siteQuery[key] = q[key];
                var site = await precheck(BridgeRequest.Parse("/agent-api/v1/building-precheck", siteQuery), ct);
                if (site.SessionId != planned.SessionId || site.BridgeVersion != planned.BridgeVersion) throw new InvalidDataException("Site observation changed session/version");
                var d = site.Data;
                diagnosis = new(d.Origin, d.Assessment, d.Reasons, d.Cells.Where(c => !c.InsideMap || c.Underground || c.IntersectsObject).ToArray(),
                    d.Entrance, d.PathAtEntrance, d.EntranceOccupants);
            }
            return Wrap(new BuildingStartResult(validated.ActionId.ToString("D"), "not_started", false,
                planned.Data.CheckedCandidates, planned.Data.StopReason, null, null, limits) { OriginDiagnosis = diagnosis }, p.Session, planned.ObservedAtUtc, null);
        }
        var executionQuery = new NameValueCollection(q); executionQuery["planKey"] = selected.PlanKey;
        var request = BuildingProjectExecutionRequest.Parse(true, executionQuery);
        ct.ThrowIfCancellationRequested();
        try {
            var receipt = await execute(request, ct);
            var failed = receipt.Data.State is "stopped" or "unconfirmed";
            return Wrap(new BuildingStartResult(validated.ActionId.ToString("D"), failed ? receipt.Data.State : "submitted", true,
                planned.Data.CheckedCandidates, planned.Data.StopReason, selected, receipt.Data, limits), p.Session, receipt.ObservedAtUtc,
                failed ? new NativeFault(receipt.Data.State, "Projekt gestoppt oder unbestätigt; vorhandenen Auftrag nur lesend klären.", false) : null);
        } catch (Exception ex) when (Known(ex)) {
            return Wrap(new BuildingStartResult(validated.ActionId.ToString("D"), ex is BridgeRejectionException ? "rejected" : "unconfirmed", true,
                planned.Data.CheckedCandidates, planned.Data.StopReason, selected, null, limits), p.Session, planned.ObservedAtUtc, Fault(ex));
        }
    }

    private static RemovalBatchItem Item(RemovalRequest r, string outcome, bool? removed, bool? marked, string? error) =>
        new(r.Id, r.Template, new(r.X, r.Y, r.Z), r.Operation, outcome, removed, marked, error);
    private static bool Known(Exception ex) => ex is ArgumentException or HttpRequestException or IOException or InvalidDataException or JsonException or UnauthorizedAccessException or OperationCanceledException;
    private static NativeFault Fault(Exception ex) => new(ex is BridgeRejectionException rejected ? rejected.Code :
        ex is UnauthorizedAccessException ? "authentication_failed" : ex is JsonException or InvalidDataException ? "backend_incompatible" : "backend_unavailable",
        "Stopp; bestätigte Schritte bleiben erhalten. Unbestätigte Wirkung nur lesend klären, nicht automatisch wiederholen.", false);
    private static JsonObject Wrap<T>(T data, string session, DateTimeOffset? observed, NativeFault? error) =>
        (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1, error is null ? "ok" : "error", data,
            new("native", false, session, observed), error), NativeJson.Options)!;
}
