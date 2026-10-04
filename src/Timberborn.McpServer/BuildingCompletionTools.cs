using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record CompletionRequest(string Session, string ActionId, int DurationHours, int Speed,
    int MaxRealSeconds, int WaitSeconds, bool Advance)
{
    // One immutable simulation budget per project. Survives MCP restarts through the native run ledger.
    public string RunId => new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(
        "timberborn-building-completion-v1:" + Session + ":" + ActionId)).AsSpan(0, 16)).ToString("D");
}
public sealed record CompletionStep(string Id, string Template, bool Found, bool? Finished,
    float? MaterialProgress, float? BuildProgress);
public sealed record CompletionReport(string ActionId, string RunId, string Outcome, string NextAction,
    string ProjectState, string ProjectReason, CompletionStep[] Steps, NativeAccess? Access, NativeSimulationRun? Run,
    float? CurrentSpeed, bool? ConfigurationRetained, string[] Limitations);
public sealed record DevelopmentStart(string ActionId, string Outcome, bool RequestSubmitted, int? CheckedCandidates,
    string? SearchStopReason, Position? Position, int? Rotation, string? ProjectState, string? ProjectReason,
    InitialStorageConfiguration? InitialConfiguration, OriginDiagnosis? OriginDiagnosis);
public sealed record DevelopmentReport(DevelopmentStart Start, CompletionReport? Completion);

public static class BuildingCompletionTools
{
    public const string Advance = "advance_building_project";
    public const string Inspect = "inspect_building_completion";
    public const string Develop = "develop_building_project";
    public static bool Handles(string name) => name is Advance or Inspect or Develop;

    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var json = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        foreach (var name in new[] { Advance, Inspect }) {
            if (name == Advance && !enabled) continue;
            var p = new JsonObject {
                ["session"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" },
                ["actionId"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" } };
            if (name == Advance) {
                p["durationHours"] = Integer(1, 168);
                p["speed"] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(1, 3, 7) };
                p["maxRealSeconds"] = Integer(30, 3600);
                p["waitSeconds"] = Integer(0, 20);
            }
            yield return new Tool {
                Name = name,
                Description = name == Advance
                    ? "Führt EIN bereits gestartetes ebenes Bauprojekt durch ein begrenztes Bauzeitfenster. Bündelt Auftragsstatus, alle Bauobjekte, Zugang, Spielzeitlauf und Abschlussprüfung. Pro actionId genau ein dauerhaft im Spiel registriertes Zeitbudget, niemals automatisch verlängert. Gleiche Parameter setzen die Beobachtung nach MCP-Neustart fort; geändertes Budget wird abgelehnt. Erst nach bestätigtem Auftrag, Bauzugang und Pause starten. waitSeconds begrenzt internes Statuswarten; laufender Modlauf überlebt Disconnect. Bei Unsicherheit inspect_building_completion lesen. finished_accessible bestätigt alle Bauobjekte fertig, Gebäudezugang, gegebenenfalls Lagerkonfiguration und aktuelle Pause; kein Produktions-/Zufriedenheitsbeleg. budget_exhausted verlangt eine neue Entscheidung, keinen erneuten Start."
                    : "Liest Bauauftrag, alle Bauobjekte, Gebäudezugang, zugehörigen begrenzten Simulationslauf und aktuellen Pausenstatus gebündelt. Keine Mutation oder neue Simulation. Mit session/actionId nach Disconnect oder MCP-Neustart wiederaufnehmbar; Spielsession muss fortbestehen. Auftrag completed allein bedeutet nicht fertig gebaut.",
                InputSchema = JsonSerializer.SerializeToElement(new JsonObject {
                    ["type"] = "object", ["additionalProperties"] = false, ["properties"] = p,
                    ["required"] = new JsonArray(p.Select(k => (JsonNode?)JsonValue.Create(k.Key)).ToArray()) }),
                OutputSchema = JsonSerializer.SerializeToElement(json.GetJsonSchemaAsNode(typeof(NativeResult<CompletionReport>))),
                Annotations = new() { ReadOnlyHint = name == Inspect, DestructiveHint = name == Advance,
                    IdempotentHint = name == Inspect, OpenWorldHint = false }
            };
        }
        static JsonObject Integer(int min, int max) => new() { ["type"] = "integer", ["minimum"] = min, ["maximum"] = max };
        if (enabled) {
            var input = JsonNode.Parse(WorkflowTools.Catalog(false, true).Single().InputSchema.GetRawText())!.AsObject();
            var p = input["properties"]!.AsObject();
            p["durationHours"] = Integer(1, 168);
            p["speed"] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(1, 3, 7) };
            p["maxRealSeconds"] = Integer(30, 3600); p["waitSeconds"] = Integer(0, 20);
            foreach (var key in new[] { "durationHours", "speed", "maxRealSeconds", "waitSeconds" }) input["required"]!.AsArray().Add(key);
            yield return new Tool {
                Name = Develop,
                Description = "Plant, startet und begleitet EIN neues ebenes Gebäudeprojekt mit festem Bauzeitbudget. Bestehende native Zugangsprüfungen bleiben. Kein Kandidat: Diagnose am Suchursprung, keine Mutation. Nach bestätigt abgeschlossenem Bauauftrag automatisch advance_building_project; Ergebnis kann noch running/awaiting_order sein. Nur einmal mit neuer actionId senden. Danach ausschließlich inspect_building_completion oder advance_building_project mit derselben session/actionId und unverändertem Zeitbudget; niemals develop erneut senden. Anfangskonfiguration direkt an Baustelle. Keine automatische Räumung, Forschung oder Mehrgebäude-Warteschlange.",
                InputSchema = JsonSerializer.SerializeToElement(input),
                OutputSchema = JsonSerializer.SerializeToElement(json.GetJsonSchemaAsNode(typeof(NativeResult<DevelopmentReport>))),
                Annotations = new() { ReadOnlyHint = false, DestructiveHint = true, IdempotentHint = false, OpenWorldHint = false }
            };
        }
    }

    public static CompletionRequest Parse(string name, JsonElement args)
    {
        if (name is not (Advance or Inspect) || args.ValueKind != JsonValueKind.Object) throw new ArgumentException();
        var allowed = name == Inspect ? new[] { "session", "actionId" }
            : new[] { "session", "actionId", "durationHours", "speed", "maxRealSeconds", "waitSeconds" };
        var keys = args.EnumerateObject().Select(p => p.Name).ToArray();
        if (keys.Length != allowed.Length || keys.Distinct().Count() != keys.Length || keys.Except(allowed).Any()) throw new ArgumentException();
        string Id(string key) => args.GetProperty(key).ValueKind == JsonValueKind.String &&
            Guid.TryParseExact(args.GetProperty(key).GetString(), "D", out var id) && id != Guid.Empty ? id.ToString("D") : throw new ArgumentException();
        int Number(string key, int min, int max) => args.GetProperty(key).ValueKind == JsonValueKind.Number &&
            args.GetProperty(key).TryGetInt32(out int n) && n >= min && n <= max ? n : throw new ArgumentException();
        var r = new CompletionRequest(Id("session"), Id("actionId"), name == Inspect ? 0 : Number("durationHours", 1, 168),
            name == Inspect ? 0 : Number("speed", 1, 7), name == Inspect ? 0 : Number("maxRealSeconds", 30, 3600),
            name == Inspect ? 0 : Number("waitSeconds", 0, 20), name == Advance);
        if (r.Advance && r.Speed is not (1 or 3 or 7)) throw new ArgumentException();
        return r;
    }

    public static Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled, CancellationToken ct, bool settingsEnabled = false)
    {
        if ((name is Advance or Develop) && !enabled) throw new ArgumentException();
        if (name == Develop) return DevelopProject(client, args, settingsEnabled, ct);
        return Execute(Parse(name, args), client.BuildingProjectExecution, client.Building, client.BuildingAccess,
            client.BuildingSettings, client.SimulationRun, client.Simulation, Task.Delay, ct);
    }

    public static (NameValueCollection Build, CompletionRequest Completion) ParseDevelopment(JsonElement args, bool settingsEnabled)
    {
        if (args.ValueKind != JsonValueKind.Object || args.EnumerateObject().Select(p => p.Name).Distinct().Count() != args.EnumerateObject().Count()) throw new ArgumentException();
        var build = JsonNode.Parse(args.GetRawText())!.AsObject();
        var finish = new JsonObject();
        foreach (var key in new[] { "session", "actionId", "durationHours", "speed", "maxRealSeconds", "waitSeconds" }) {
            finish[key] = build[key]?.DeepClone();
            if (key is not ("session" or "actionId")) build.Remove(key);
        }
        // Validate both phases before any native operation.
        return (WorkflowTools.ParseBuild(JsonSerializer.SerializeToElement(build), settingsEnabled),
            Parse(Advance, JsonSerializer.SerializeToElement(finish)));
    }

    private static async Task<JsonObject> DevelopProject(NativeClient client, JsonElement args, bool settingsEnabled, CancellationToken ct)
    {
        var parsed = ParseDevelopment(args, settingsEnabled);
        var startJson = await WorkflowTools.Start(parsed.Build, client.BuildingPlan, client.BuildingProjectExecution, ct, client.Precheck);
        var start = startJson.Deserialize<NativeResult<BuildingStartResult>>(NativeJson.Options)!;
        NativeResult<CompletionReport>? finish = null;
        NativeFault? followupFault = null;
        if (start.Status == "ok" && start.Data is { RequestSubmitted: true, Execution.State: "completed" or "running" }) {
            try {
                var result = await Execute(parsed.Completion, client.BuildingProjectExecution, client.Building, client.BuildingAccess,
                    client.BuildingSettings, client.SimulationRun, client.Simulation, Task.Delay, ct);
                finish = result.Deserialize<NativeResult<CompletionReport>>(NativeJson.Options)!;
            } catch (Exception ex) when (ex is ArgumentException or HttpRequestException or IOException or InvalidDataException or JsonException or UnauthorizedAccessException or OperationCanceledException) {
                followupFault = new(ex is BridgeRejectionException rejected ? rejected.Code : "unconfirmed",
                    "Baustartbeleg bleibt erhalten. Fortsetzung nur mit inspect_building_completion klären, develop nicht wiederholen.", false);
            }
        }
        var d = start.Data!;
        var compact = new DevelopmentStart(d.ActionId, d.Outcome, d.RequestSubmitted, d.CheckedCandidates,
            d.SearchStopReason, d.SelectedOption?.Origin, d.SelectedOption?.Rotation, d.Execution?.State,
            d.Execution?.Reason, d.Execution?.InitialConfiguration, d.OriginDiagnosis);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<DevelopmentReport>(1, followupFault is not null ? "error" : finish?.Status ?? start.Status,
            new(compact, finish?.Data), finish?.Meta ?? start.Meta, followupFault ?? finish?.Error ?? start.Error), NativeJson.Options)!;
    }

    public static async Task<JsonObject> Execute(CompletionRequest r,
        Func<BuildingProjectExecutionRequest, CancellationToken, Task<BridgeEnvelope<NativeProjectExecution>>> project,
        Func<BridgeRequest, CancellationToken, Task<BridgeEnvelope<NativeBuilding>>> building,
        Func<LogisticsRequest, CancellationToken, Task<BridgeEnvelope<NativeAccess>>> access,
        Func<BuildingSettingsRequest, CancellationToken, Task<BridgeEnvelope<NativeBuildingSettings>>> settings,
        Func<SimulationRunRequest, NameValueCollection, CancellationToken, Task<BridgeEnvelope<NativeSimulationRun>>> simulationRun,
        Func<CancellationToken, Task<BridgeEnvelope<NativeSimulation>>> simulation,
        Func<TimeSpan, CancellationToken, Task> delay, CancellationToken ct)
    {
        string version = ""; DateTimeOffset? observed = null;
        T Check<T>(BridgeEnvelope<T> e) {
            if (e.SessionId != r.Session || version.Length > 0 && e.BridgeVersion != version) throw new InvalidDataException("Observation changed session/version");
            version = e.BridgeVersion; observed = e.ObservedAtUtc; return e.Data;
        }
        NameValueCollection Identity(string key, string id) => new() { ["session"] = r.Session, [key] = id };
        var p = Check(await project(BuildingProjectExecutionRequest.Parse(false, Identity("actionId", r.ActionId)), ct));
        NativeSimulationRun? run = null; NativeAccess? a = null; float? speed = null; bool? retained = null;
        var steps = new List<CompletionStep>();
        JsonObject Result(string outcome, string next, NativeFault? fault = null) => (JsonObject)JsonSerializer.SerializeToNode(
            new NativeResult<CompletionReport>(1, fault is null ? "ok" : "error",
                new(r.ActionId, r.RunId, outcome, next, p.State, p.Reason, steps.ToArray(), a, run, speed, retained,
                    ["one_simulation_budget_per_project", "non_atomic_reads", "no_production_or_wellbeing_proof", "native_construction_preflight_gap_unchanged"]),
                new("native", false, r.Session, observed), fault), NativeJson.Options)!;
        async Task<NativeSimulationRun> Run(string route, NameValueCollection q) => Check(await simulationRun(
            SimulationRunRequest.Parse("/agent-api/v1/" + route, q), q, ct));
        var runQuery = Identity("runId", r.RunId);
        try { run = await Run("simulation-run", runQuery); }
        catch (BridgeRejectionException ex) when (ex.Code == "run_not_found") { /* Definitive absence, not transport failure. */ }
        if (run is not null && r.Advance && (run.RequestedSpeed != r.Speed || run.MaxRealSeconds != r.MaxRealSeconds ||
            Math.Abs(run.TargetGameHours - run.StartGameHours - r.DurationHours) > 0.0000001))
            return Result("budget_conflict", "inspect", new("state_conflict", "Vorhandenes Zeitbudget stimmt nicht überein; keine Änderung.", false));
        if (p.State != "completed" || p.Steps.Any(s => s.State != "confirmed"))
            return Result(p.State == "running" ? "awaiting_order" : "order_stopped", "inspect");

        async Task Observe()
        {
            steps.Clear(); retained = null;
            foreach (var step in p.Steps) {
                var b = Check(await building(BridgeRequest.Parse("/agent-api/v1/building", Identity("id", step.EntityId)), ct));
                if (b.Found && (b.Details!.Template != step.Template || b.Details.Position != new Position(step.X, step.Y, step.Z)))
                    throw new InvalidDataException("Project entity changed");
                steps.Add(new(step.EntityId, step.Template, b.Found, b.Details?.Finished,
                    b.Details?.Construction?.MaterialProgress, b.Details?.Construction?.BuildTimeProgress));
            }
            a = null;
            if (steps[^1].Found) a = Check(await access(LogisticsRequest.Parse("/agent-api/v1/building-access", Identity("id", r.ActionId)), ct));
            if (a is not null && (a.Position != new Position(p.Steps[^1].X, p.Steps[^1].Y, p.Steps[^1].Z) || a.Finished != steps[^1].Finished))
                throw new InvalidDataException("Non-atomic construction transition; inspect again");
            if (p.InitialConfiguration is { } config && steps[^1].Finished == true) {
                var s = Check(await settings(BuildingSettingsRequest.Parse("/agent-api/v1/building-settings", Identity("id", r.ActionId)), ct));
                if (s.Template != p.Steps[^1].Template || s.Position != a?.Position) throw new InvalidDataException("Settings target changed");
                retained = s.Finished && s.Storage is { } storage &&
                    (config.Good is null || config.Good == storage.SelectedGood) && (config.Mode is null || config.Mode == storage.Mode);
            }
            speed = Check(await simulation(ct)).CurrentSpeed;
        }
        bool Finished() => steps.All(s => s.Finished == true) && a is { Finished: true, EntranceBlocked: false,
            EntranceInaccessible: false } && (a.UnconnectedBlocked == false ||
                a is { UnconnectedBlocked: null, UnconnectedBlockerCount: 0 }) && (p.InitialConfiguration is null || retained == true);

        await Observe();
        bool refreshAfterRun = false;
        if (steps.Any(s => !s.Found)) return Result("entity_missing", "diagnose");
        if (retained == false) return Result("configuration_changed", "diagnose");
        if (run is null && Finished() && speed == 0) return Result("finished_accessible", "done");
        if (run is null && r.Advance) {
            if (steps[^1].Finished == true || a?.BuildersReachable != true || steps.Take(steps.Count - 1).Any(s => s.Finished != true))
                return Result("access_unproven", "diagnose");
            if (speed != 0) return Result("not_paused", "diagnose");
            var start = new NameValueCollection(runQuery) { ["duration"] = r.DurationHours.ToString(CultureInfo.InvariantCulture),
                ["unit"] = "hours", ["speed"] = r.Speed.ToString(CultureInfo.InvariantCulture), ["expectedSpeed"] = "0",
                ["maxRealSeconds"] = r.MaxRealSeconds.ToString(CultureInfo.InvariantCulture) };
            try { run = await Run("simulation-run-for", start); refreshAfterRun = true; }
            catch (Exception ex) when (ex is ArgumentException or HttpRequestException or IOException or InvalidDataException or JsonException or OperationCanceledException) {
                return Result("simulation_unconfirmed", "inspect", new(ex is BridgeRejectionException rejection ? rejection.Code : "unconfirmed",
                    "Kein erneuter Start. Zugehörigen Lauf mit inspect_building_completion lesen.", false));
            }
        }
        // Bounded polling of the SAME native handle, never a second time window or detached task.
        var clock = System.Diagnostics.Stopwatch.StartNew();
        for (int seconds = 0; run is { Terminal: false } && seconds < r.WaitSeconds && clock.Elapsed.TotalSeconds < r.WaitSeconds; seconds += 2) {
            await delay(TimeSpan.FromSeconds(Math.Max(0, Math.Min(2, Math.Min(r.WaitSeconds - seconds, r.WaitSeconds - clock.Elapsed.TotalSeconds)))), ct);
            run = await Run("simulation-run", runQuery);
            refreshAfterRun = true;
        }
        if (run is { Terminal: false }) { speed = null; return Result("running", "inspect"); }
        if (refreshAfterRun) await Observe();
        if (steps.Any(s => !s.Found)) return Result("entity_missing", "diagnose");
        if (retained == false) return Result("configuration_changed", "diagnose");
        if (run is { State: not "completed" }) return Result("simulation_stopped", "diagnose");
        if (speed != 0) return Result("not_paused", "diagnose");
        if (Finished()) return Result("finished_accessible", "done");
        if (steps.All(s => s.Finished == true)) return Result("access_unproven", "diagnose");
        return Result(run is null ? "awaiting_simulation" : "budget_exhausted", run is null ? "advance" : "diagnose");
    }
}
