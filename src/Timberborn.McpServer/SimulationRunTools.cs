using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class SimulationRunTools
{
    public static bool Handles(string name) => name is "run_simulation_for" or "run_simulation_until" or "inspect_simulation_run" or "cancel_simulation_run";
    public static bool Writes(string name) => Handles(name) && name != "inspect_simulation_run";
    private static string Route(string name) => name switch { "run_simulation_for" => "simulation-run-for", "run_simulation_until" => "simulation-run-until", "inspect_simulation_run" => "simulation-run", "cancel_simulation_run" => "simulation-run-cancel", _ => throw new ArgumentException() };
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        foreach (string name in new[] { "run_simulation_for", "run_simulation_until", "inspect_simulation_run", "cancel_simulation_run" }) {
            bool write = Writes(name), start = name.StartsWith("run_");
            if (write && !enabled) continue;
            var props = new JsonObject();
            foreach (string key in new[] { "session", "runId" }) props[key] = new JsonObject { ["type"] = "string", ["format"] = "uuid" };
            if (start) {
                props["speed"] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(1, 3, 7) };
                props["expectedSpeed"] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(0, 1, 3, 7) };
                props["maxRealSeconds"] = new JsonObject { ["type"] = "integer", ["minimum"] = 30, ["maximum"] = 86400 };
            }
            if (name == "run_simulation_for") {
                props["duration"] = new JsonObject { ["type"] = "number", ["minimum"] = 0.000001, ["maximum"] = 672 };
                props["unit"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("hours", "days", "weeks") };
            }
            if (name == "run_simulation_until") {
                props["dayNumber"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 1000000 };
                props["hour"] = new JsonObject { ["type"] = "number", ["minimum"] = 0, ["exclusiveMaximum"] = 24 };
            }
            yield return new Tool { Name = name,
                Description = "Bridge 0.22.0: begrenzter asynchroner Spielzeitlauf. Start: eigene neue runId (UUID), aktuelle session und expectedSpeed; maximal 28 Spieltage, Woche=7 Tage. until: monotoner dayNumber aus inspect_simulation, KEIN Zyklustag; hour 0..<24. maxRealSeconds 30..86400, z.B. 7200. Mod überwacht ohne MCP-Polling und pausiert am Ziel; completed erst nach beobachteter Pause, overshootHours beachten. Status/Abbruch mit derselben runId/session. Ein aktiver Lauf, 128 IDs pro Sitzung; IDs nie wiederverwenden. MCP-Abbruch/Disconnect beendet den Modlauf NICHT. Bei unbestätigter Antwort nur Status lesen, Start niemals automatisch wiederholen. Manuelle Speedänderung unterbricht; Stillstand 120s, Speed/Pause-Bestätigung 5s. Zeitlimits erst beim nächsten Spiel-Update auswertbar. Endstatus ist eingefrorener Abschlussbeleg; aktuellen Zustand mit inspect_simulation lesen. Keine Spielsperren entsperrt.",
                InputSchema = JsonSerializer.SerializeToElement(new JsonObject { ["type"] = "object", ["properties"] = props, ["required"] = new JsonArray(props.Select(p => (JsonNode?)JsonValue.Create(p.Key)).ToArray()), ["additionalProperties"] = false }),
                OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeSimulationRun>))),
                Annotations = new() { ReadOnlyHint = !write, DestructiveHint = write, IdempotentHint = !write, OpenWorldHint = false } };
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled, CancellationToken ct)
    {
        if (Writes(name) && !enabled) throw new ArgumentException();
        var q = new NameValueCollection();
        foreach (var p in args.EnumerateObject()) {
            if (p.Name is "session" or "runId" or "unit") {
                if (p.Value.ValueKind != JsonValueKind.String) throw new ArgumentException();
                q.Add(p.Name, p.Value.GetString());
            }
            else if (p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetDecimal(out var number))
                q.Add(p.Name, number.ToString("0.############################", System.Globalization.CultureInfo.InvariantCulture));
            else throw new ArgumentException();
        }
        var r = SimulationRunRequest.Parse("/agent-api/v1/" + Route(name), q);
        var e = await client.SimulationRun(r, q, ct);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<NativeSimulationRun>(1, "ok", e.Data, new("native", false, e.SessionId, e.ObservedAtUtc), null), NativeJson.Options)!;
    }
}
