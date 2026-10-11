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
                props["goodTargets"] = new JsonObject { ["type"]="array", ["minItems"]=1, ["maxItems"]=8,
                    ["items"]=new JsonObject { ["type"]="object", ["properties"]=new JsonObject {
                        ["good"]=new JsonObject { ["type"]="string", ["pattern"]="^[A-Za-z0-9._-]{1,160}$" },
                        ["amount"]=new JsonObject { ["type"]="integer", ["minimum"]=1, ["maximum"]=1000000 } },
                        ["required"]=new JsonArray("good","amount"), ["additionalProperties"]=false } };
                props["stopAtScience"] = new JsonObject { ["type"]="integer", ["minimum"]=1, ["maximum"]=1000000 };
                props["stopAtAvailableLogs"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 1000000 };
            }
            if (name == "run_simulation_until") {
                props["dayNumber"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 1000000 };
                props["hour"] = new JsonObject { ["type"] = "number", ["minimum"] = 0, ["exclusiveMaximum"] = 24 };
            }
            yield return new Tool { Name = name,
                Description = "Begrenzter autonomer Simulationslauf; neue UUID pro Start, aktuelle session/expectedSpeed. for: duration als Obergrenze, optional goodTargets (1–8 eindeutige good/amount) und stopAtScience; ALLE Ziele gleichzeitig aus AvailableStock/SciencePoints. Exklusiv zum kompatiblen stopAtAvailableLogs. Bereits erfüllt: keine Beschleunigung. until: monotoner dayNumber und hour, kein Zyklustag. Maximal 28 Spieltage; maxRealSeconds 30..86400. Mod pausiert selbst. productionTargetsReached mit production_targets_reached belegt das Bestandsziel; production_time_limit nur Zeitablauf. targetReached bleibt Zeitindikator. Fehlende Bestandsbeobachtung stoppt. Diagnose frisch, begrenzt auf aktuelle Manufakturen; eingefrorene terminale Zielbestände sind historische Belege. Keine Reservierung, Ertrags-, Windursachen- oder Liefergarantie. Status/Abbruch mit derselben ID; nach unbestätigtem Start nur lesen, nie wiederholen. Manuelles Tempo unterbricht; kein Entsperren des Spiels. Ein aktiver Lauf, 128 IDs/Sitzung; Abschluss erst mit beobachteter Pause.",
                InputSchema = JsonSerializer.SerializeToElement(new JsonObject { ["type"] = "object", ["properties"] = props, ["required"] = new JsonArray(props.Where(p => p.Key is not ("stopAtAvailableLogs" or "goodTargets" or "stopAtScience")).Select(p => (JsonNode?)JsonValue.Create(p.Key)).ToArray()), ["additionalProperties"] = false }),
                OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeSimulationRun>))),
                Annotations = new() { ReadOnlyHint = !write, DestructiveHint = write, IdempotentHint = !write, OpenWorldHint = false } };
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled, CancellationToken ct)
    {
        if (Writes(name) && !enabled) throw new ArgumentException();
        var q = new NameValueCollection();
        foreach (var p in args.EnumerateObject()) {
            if (p.Name == "goodTargets") {
                if (p.Value.ValueKind != JsonValueKind.Array || p.Value.GetArrayLength() is <1 or >8) throw new ArgumentException();
                var entries = p.Value.EnumerateArray().Select(v => {
                    if (v.ValueKind != JsonValueKind.Object) throw new ArgumentException();
                    var keys=v.EnumerateObject().Select(x=>x.Name).ToArray();
                    if (keys.Length!=2 || keys.Distinct().Count()!=2 || !keys.Contains("good") || !keys.Contains("amount")) throw new ArgumentException();
                    if (v.GetProperty("good").ValueKind!=JsonValueKind.String || v.GetProperty("amount").ValueKind!=JsonValueKind.Number || !v.GetProperty("amount").TryGetInt32(out int amount)) throw new ArgumentException();
                    string good=v.GetProperty("good").GetString()??"";
                    if (!BuildingPolicy.ValidTemplate(good) || amount is <1 or >1000000) throw new ArgumentException();
                    return good+":"+amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                });
                q.Add(p.Name,string.Join(";",entries));
            }
            else if (p.Name is "session" or "runId" or "unit") {
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
