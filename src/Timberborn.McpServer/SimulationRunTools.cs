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
    // Only this family is migrated in A04. Order is the existing catalog order.
    public static IReadOnlyList<NativeToolDescriptor> Descriptors { get; } = Array.AsReadOnly(new[]
    {
        new NativeToolDescriptor("run_simulation_for", "simulation-run-for", NativeToolKind.Mutation),
        new NativeToolDescriptor("run_simulation_until", "simulation-run-until", NativeToolKind.Mutation),
        new NativeToolDescriptor("inspect_simulation_run", "simulation-run", NativeToolKind.Status),
        new NativeToolDescriptor("cancel_simulation_run", "simulation-run-cancel", NativeToolKind.Cancel),
    });
    public static NativeToolDescriptor? Find(string name) => Descriptors.FirstOrDefault(d => d.Name == name);
    public static bool Handles(string name) => Find(name) is not null;
    public static bool Writes(string name) => Find(name)?.Writes == true;
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        foreach (var descriptor in Descriptors) {
            string name = descriptor.Name;
            bool write = descriptor.Writes, start = descriptor.Kind == NativeToolKind.Mutation;
            if (!descriptor.IsEnabled(enabled)) continue;
            var props = new JsonObject();
            foreach (string key in new[] { "session", "runId" }) props[key] = new JsonObject { ["type"] = "string", ["format"] = "uuid" };
            if (start) {
                props["speed"] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(1, 3, 7) };
                props["expectedSpeed"] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(0, 1, 3, 7) };
                props["maxRealSeconds"] = new JsonObject { ["type"] = "integer", ["minimum"] = 30, ["maximum"] = 86400 };
            }
            if (name == "run_simulation_for") {
                props["completionSpeed"] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(0, 1), ["default"] = 0,
                    ["description"] = "Nur reine Zeitläufe ohne Ziel-/Messparameter: 0 pausiert (Standard), 1 übergibt nach bestätigter Pause und Tempobestätigung an unüberwachtes Weiterspielen. Cancel/Fehler/Spielsperre setzen diese Wahl außer Kraft." };
                props["duration"] = new JsonObject { ["type"] = "number", ["minimum"] = 0.000001, ["maximum"] = 672 };
                props["unit"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("hours", "days", "weeks") };
                props["measureProduction"]=new JsonObject{["type"]="boolean",["default"]=false,["description"]="0.44: Mit buildingIds bis zur Zeitgrenze messen statt beim Erstzyklus stoppen. Native Zyklen, rezeptabgeleitete Mengen und beobachtete Stillstandszeiten in Spielstunden."};
                props["buildingIds"]=new JsonObject{["type"]="array",["minItems"]=1,["maxItems"]=8,["uniqueItems"]=true,["items"]=new JsonObject{["type"]="string",["format"]="uuid"}};
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
                Description = "MCP-Vorprüfung: vor for ab sechs Spielstunden und vor jedem until automatische Betriebsdiagnose; operatingPreflight im Ergebnis. Bei Bretterbezug zusätzlich plankChain mit Engpässen und nächsten Schritten im selben Vorprüfungsbudget; unklarer Bezirk bleibt district_unknown. Befunde/Materialmangel sperren den Start nicht, kein Ausbau oder verlängertes Laufbudget. Status und Cancel ohne Vorprüfung. " + "Ab 0.66: Explizite run_start_*-Ablehnungen vor Laufregistrierung melden not_executed mit konkretem Tempo-/Besitzer-/Journalgrund. Aktuellen Zustand lesen und neu entscheiden, kein automatisches Retry. Generische Konflikte und Transportverlust bleiben unconfirmed. Ab 0.44 optional measureProduction=true mit buildingIds: Messung bis duration, nicht bis Erstzyklus; production_measurement_complete bedeutet Zeitfenster beendet, kein Produktionsziel. measurement je Gebäude enthält native Zykluszahl, rezeptabgeleitete Mengen/Raten und exklusiv klassifizierte beobachtete Zeitanteile einschließlich unknown/multiple_blockers. Kein Beweis exakter Stillstandsursachen oder ausgelieferter Mengen. Ab 0.47 nur mit diagnose=true: inspect_simulation_run liefert für buildingIds zusätzlich currentBuildingOperations mit frischen Engpassbelegen und nächsten Schritten. bottleneckObservedGameHours separat vom eingefrorenen Produktionsnachweis; unavailable ist unbekannt. Ab 0.41 optional buildingIds (1–8 eindeutige fertige Manufactory-Gebäude mit gewähltem Rezept), exklusiv zu Güter-/Forschungs-/Holzzielen. Ohne Messmodus Stopp nach mindestens einem nativen ProductionFinished pro Gebäude; buildingProductionReached und building_production_proven sind der Nachweis, niemals bloßer Bestand/Transport. Bau-/Rezeptvoraussetzungen vor Start geprüft. Rezept-/Pause-/Lager-/Sollpersonaländerungen, Objektverlust oder Journallücke stoppen mit Fehler und Pause. buildingProduction enthält aktuelle und während des Laufs beobachtete Blockaden, terminal eingefroren. Kein Wiederholen unbestätigter Starts. Begrenzter autonomer Simulationslauf; neue UUID pro Start, aktuelle session/expectedSpeed. for: duration als Obergrenze, optional goodTargets (1–8 eindeutige good/amount) und stopAtScience; ALLE Ziele gleichzeitig aus AvailableStock/SciencePoints. Exklusiv zum kompatiblen stopAtAvailableLogs. Bereits erfüllt: keine Beschleunigung. until: monotoner dayNumber und hour, kein Zyklustag. Maximal 28 Spieltage; maxRealSeconds 30..86400. Mod pausiert selbst; nur reine for-Zeitläufe dürfen mit completionSpeed=1 nach bestätigter Pause an Tempo 1 übergeben. Das Spiel läuft danach ohne Laufbudget und Überwachung weiter. productionTargetsReached mit production_targets_reached belegt das Bestandsziel; production_time_limit nur Zeitablauf. targetReached bleibt Zeitindikator. Fehlende Bestandsbeobachtung stoppt. Diagnose frisch, begrenzt auf aktuelle Manufakturen; eingefrorene terminale Zielbestände sind historische Belege. Keine Reservierung, Ertrags-, Windursachen- oder Liefergarantie. Status/Abbruch mit derselben ID; nach unbestätigtem Start nur lesen, nie wiederholen. Manuelles Tempo unterbricht; kein Entsperren des Spiels. Ein aktiver Lauf, 128 IDs/Sitzung; Abschluss mit beobachteter Pause oder ausdrücklich bestätigtem completionSpeed=1; completing hält den Besitzer. Cancel terminaler IDs ändert kein Tempo.",
                InputSchema = JsonSerializer.SerializeToElement(new JsonObject { ["type"] = "object", ["properties"] = props, ["required"] = new JsonArray(props.Where(p => p.Key is not ("completionSpeed" or "stopAtAvailableLogs" or "goodTargets" or "stopAtScience" or "buildingIds" or "measureProduction")).Select(p => (JsonNode?)JsonValue.Create(p.Key)).ToArray()), ["additionalProperties"] = false }),
                OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeSimulationRun>))),
                Annotations = new() { ReadOnlyHint = !write, DestructiveHint = write, IdempotentHint = !write, OpenWorldHint = false } };
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled, CancellationToken ct)
    {
        var descriptor = Find(name) ?? throw new ArgumentException();
        if (!descriptor.IsEnabled(enabled)) throw new ArgumentException();
        var q = new NameValueCollection();
        foreach (var p in args.EnumerateObject()) {
            if(p.Name=="buildingIds") {
                if(p.Value.ValueKind!=JsonValueKind.Array||p.Value.GetArrayLength() is <1 or >8)throw new ArgumentException();
                var ids=p.Value.EnumerateArray().Select(v=>{
                    if(v.ValueKind!=JsonValueKind.String||!Guid.TryParseExact(v.GetString(),"D",out var id)||id==Guid.Empty)throw new ArgumentException();return id.ToString("D");
                }).ToArray();
                q.Add(p.Name,string.Join(",",ids));
            }
            else if(p.Name is "measureProduction" or "diagnose") {
                if(p.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))throw new ArgumentException();
                q.Add(p.Name,p.Value.GetBoolean()?"true":"false");
            }
            else if (p.Name == "goodTargets") {
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
        var r = SimulationRunRequest.Parse("/agent-api/v1/" + descriptor.Route, q);
        var e = await client.SimulationRun(r, q, ct);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<NativeSimulationRun>(1, "ok", e.Data, new("native", false, e.SessionId, e.ObservedAtUtc), null), NativeJson.Options)!;
    }
}
