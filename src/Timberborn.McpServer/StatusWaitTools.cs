using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;

namespace Timberborn.McpServer;

public static class StatusWaitTools
{
    public sealed record Options(JsonElement Arguments, int WaitSeconds, string? After, bool Diagnose);
    public static bool Handles(string name) => name is "inspect_simulation_run" or "inspect_large_project";
    public static Options Parse(string name, JsonElement args)
    {
        if (!Handles(name) || args.ValueKind != JsonValueKind.Object) throw new ArgumentException();
        var clean = new JsonObject(); var seen = new HashSet<string>();
        int wait = 0; string? after = null; bool diagnose = false;
        foreach (var p in args.EnumerateObject()) {
            if (!seen.Add(p.Name)) throw new ArgumentException("duplicate_argument");
            switch (p.Name) {
                case "waitSeconds":
                    if (p.Value.ValueKind != JsonValueKind.Number || !p.Value.TryGetInt32(out wait) || wait is < 0 or > 30) throw new ArgumentException("wait_seconds_0_to_30");
                    break;
                case "after":
                    if (p.Value.ValueKind != JsonValueKind.String || p.Value.GetString() is not { Length: 64 } token ||
                        token.Any(c => !char.IsAsciiHexDigit(c)) || token != token.ToLowerInvariant()) throw new ArgumentException("invalid_change_token");
                    after = token; break;
                case "diagnose":
                    if (p.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new ArgumentException();
                    diagnose = p.Value.GetBoolean(); break;
                default:
                    if (p.Name != "session" && p.Name != (name == "inspect_large_project" ? "actionId" : "runId")) throw new ArgumentException();
                    if (p.Value.ValueKind != JsonValueKind.String || !Guid.TryParseExact(p.Value.GetString(),"D",out var id) || id == Guid.Empty) throw new ArgumentException();
                    clean[p.Name] = id.ToString("D"); break;
            }
        }
        if (clean.Count != 2) throw new ArgumentException();
        return new(JsonSerializer.SerializeToElement(clean),wait,after,diagnose);
    }

    public static async Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, CancellationToken ct)
    {
        var options = Parse(name,args);
        string? baseline = options.After;
        async Task<JsonObject> Read(bool diagnosis, CancellationToken token) {
            if (name == "inspect_large_project") return await LargeProjectTools.Invoke(client,diagnosis ? "diagnose_large_project" : name,options.Arguments,false,token);
            var request = JsonNode.Parse(options.Arguments.GetRawText())!.AsObject(); request["diagnose"] = diagnosis;
            var observation = await SimulationRunTools.Invoke(client,name,JsonSerializer.SerializeToElement(request),false,token);
            if (!diagnosis) baseline ??= ChangeToken(name,observation);
            return observation;
        }
        var result = await Wait(name,options,token=>Read(false,token),ct);
        if (options.Diagnose) {
            var diagnosis = await Read(true,ct); // Exactly once, after lightweight waiting has ended.
            var watch = result["watch"]!.AsObject();
            watch["diagnosticRequests"] = 1;
            watch["diagnosisState"] = diagnosis["status"]?.GetValue<string>() == "ok" ? "returned" : "unavailable";
            if (name == "inspect_large_project") watch["diagnosis"] = diagnosis;
            else {
                // Full validated run and fresh diagnosis from the same final read.
                // Retain the reason for ending the wait; changed is compared to the caller/baseline.
                string current = ChangeToken(name,diagnosis);
                watch["changeToken"] = current;
                watch["changed"] = options.After is null && watch["changed"] is null ? null :
                    JsonValue.Create(current != baseline);
                result = diagnosis; result["watch"] = watch.DeepClone();
            }
        }
        return result;
    }

    public interface IClock { double Seconds { get; } Task Delay(TimeSpan duration, CancellationToken ct); }
    private sealed class Clock : IClock {
        private readonly Stopwatch watch = Stopwatch.StartNew();
        public double Seconds => watch.Elapsed.TotalSeconds;
        public Task Delay(TimeSpan duration, CancellationToken ct) => Task.Delay(duration,ct);
    }
    public static async Task<JsonObject> Wait(string name, Options options, Func<CancellationToken,Task<JsonObject>> read,
        CancellationToken ct, IClock? clock = null)
    {
        clock ??= new Clock();
        ct.ThrowIfCancellationRequested();
        var result = await read(ct); int reads = 1;
        string first = ChangeToken(name,result), baseline = options.After ?? first;
        double start = clock.Seconds;
        string reason;
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (options.WaitSeconds > 0) budget.CancelAfter(TimeSpan.FromSeconds(options.WaitSeconds));
        while (true) {
            ct.ThrowIfCancellationRequested();
            if (Terminal(result)) { reason = "terminal"; break; }
            if (Blocked(result)) { reason = "blocked"; break; }
            if (ChangeToken(name,result) != baseline) { reason = "changed"; break; }
            if (options.WaitSeconds == 0) { reason = "snapshot"; break; }
            double remaining = options.WaitSeconds - (clock.Seconds - start);
            if (remaining <= 0) { reason = "wait_limit"; break; }
            try {
                await clock.Delay(TimeSpan.FromSeconds(Math.Min(1,remaining)),budget.Token);
                if (clock.Seconds-start >= options.WaitSeconds) { reason="wait_limit"; break; }
                reads++;
                result = await read(budget.Token);
            } catch (OperationCanceledException) when (!ct.IsCancellationRequested && budget.IsCancellationRequested) {
                reason = "wait_limit"; break; // Return last validated observation with its original timestamp.
            }
        }
        string current = ChangeToken(name,result);
        bool? changed = options.After is null && reason == "snapshot" ? null : current != baseline;
        result["watch"] = JsonSerializer.SerializeToNode(new StatusWatch(current,changed,reason,
            Math.Max(0,clock.Seconds-start),reads,0,"not_requested"),NativeJson.Options);
        return result;
    }

    public static string ChangeToken(string name, JsonObject result)
    {
        var d = result["data"] as JsonObject ?? throw new InvalidDataException("Missing status receipt");
        var core = new JsonObject { ["tool"] = name, ["session"] = result["meta"]?["sessionId"]?.DeepClone() };
        foreach (string key in new[] { "runId","actionId","state","reason","observedSpeed","pauseConfirmed","terminal",
            "completionSpeed","effectiveCompletionSpeed","completionSpeedConfirmed","completionPauseObserved","completionFallbackReason",
            "targetReached","stockTargetReached","productionTargetsReached","buildingProductionReached","observedAvailableLogs",
            "observedScience","goodTargets","partStates","blockedPart","deferredParts","released" })
            if (d.ContainsKey(key)) core[key] = d[key]?.DeepClone();
        if (d["simulation"] is JsonObject simulation) {
            var sim = new JsonObject();
            foreach (string key in new[] { "state","reason","observedSpeed","pauseConfirmed","terminal" }) sim[key] = simulation[key]?.DeepClone();
            core["simulation"] = sim;
        }
        if (d["buildingProduction"] is JsonArray buildings) {
            var items = new JsonArray();
            foreach (var b in buildings.OfType<JsonObject>()) {
                var item = new JsonObject();
                foreach (string key in new[] { "id","recipe","completedCycles","blockers","seenBlockers" }) item[key] = b[key]?.DeepClone();
                item["measuredCycles"] = b["measurement"]?["completedCycles"]?.DeepClone(); items.Add(item);
            }
            core["buildings"] = items;
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(core.ToJsonString()))).ToLowerInvariant();
    }
    private static bool Terminal(JsonObject r) => r["data"]?["terminal"]?.GetValue<bool>() == true ||
        r["data"]?["state"]?.GetValue<string>() is "completed" or "blocked" or "stopped" or "unconfirmed" or "interrupted";
    private static bool Blocked(JsonObject r) => r["data"]?["blockedPart"] is not null ||
        r["data"]?["buildingProduction"] is JsonArray b && b.OfType<JsonObject>().Any(x=>x["blockers"] is JsonArray { Count: > 0 });
}
