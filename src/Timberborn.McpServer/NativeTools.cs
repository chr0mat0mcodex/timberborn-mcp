using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed class NativeTools(NativeClient client, bool enableValidation = false, bool enablePlacement = false, bool enableLodgePlacement = false, bool enableSpeedControl = false, bool enableStaffing = false, bool enablePriorities = false, bool enableAreas = false, bool enableRemoval = false, bool enableBuildingPlacement = false, bool enableBuildingSettings = false) : IDisposable
{
    public static IReadOnlyList<Tool> Catalog(bool enableValidation = false, bool enablePlacement = false, bool enableLodgePlacement = false, bool enableSpeedControl = false, bool enableStaffing = false, bool enablePriorities = false, bool enableAreas = false, bool enableRemoval = false, bool enableBuildingPlacement = false, bool enableBuildingSettings = false)
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var names = new List<string> { "timberborn_status", "inspect_colony", "inspect_map_region", "find_buildings", "inspect_build_catalog", "precheck_build_site", "inspect_building", "inspect_simulation", "inspect_workforce" };
        if (enableStaffing) names.Add("set_workplace_staffing");
        if (enableSpeedControl) names.Add("set_simulation_speed");
        if (enableValidation) names.Add("validate_build_site");
        if (enablePlacement) names.Add("place_path"); if (enableLodgePlacement) names.Add("place_lodge");
        return names.Select(name =>
        {
            bool map = name == "inspect_map_region";
            bool validation = name == "validate_build_site";
            bool placement = name is "place_path" or "place_lodge";
            bool speed = name == "set_simulation_speed";
            bool staffing = name == "set_workplace_staffing";
            bool action = validation || placement || speed || staffing;
            bool site = name == "precheck_build_site" || validation || placement;
            var properties = new JsonObject();
            if (staffing) {
                properties["id"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" };
                foreach (var key in new[] { "desiredWorkers", "expectedDesiredWorkers" }) properties[key] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 64 };
            }
            if (speed) foreach (var key in new[] { "speed", "expectedSpeed" }) properties[key] = new JsonObject { ["type"] = "integer", ["enum"] = new JsonArray(0, 1, 3, 7) };
            if (name == "inspect_building")
                foreach (var key in new[] { "id", "session" }) properties[key] = new JsonObject { ["type"] = "string", ["format"] = "uuid" };
            if (map) foreach (var (key, min, max) in new[] { ("x", 0, 4095), ("y", 0, 4095), ("z", 0, 4095), ("width", 1, 8), ("height", 1, 8), ("depth", 1, 4) })
                properties[key] = new JsonObject { ["type"] = "integer", ["minimum"] = min, ["maximum"] = max };
            if (name is "find_buildings" or "inspect_workforce")
            {
                properties["offset"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 65535 };
                properties["limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 32 };
            }
            if (site)
            {
                properties["template"] = new JsonObject { ["type"] = "string", ["enum"] = placement ? new JsonArray(name == "place_lodge" ? "Lodge.Folktails" : "Path") : new JsonArray("Lodge.Folktails", "Path") };
                foreach (var key in new[] { "x", "y", "z", "rotation" })
                    properties[key] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = key == "rotation" ? 3 : 4095 };
            }
            if (action) properties["session"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" };
            var input = new JsonObject { ["type"] = "object", ["properties"] = properties, ["additionalProperties"] = false };
            if (properties.Count > 0) input["required"] = new JsonArray(properties.Select(p => (JsonNode?)JsonValue.Create(p.Key)).ToArray());
            Type result = name switch { "inspect_map_region" => typeof(NativeResult<NativeMap>),
                "find_buildings" => typeof(NativeResult<NativeObjects>), "inspect_build_catalog" => typeof(NativeResult<NativeCatalog>),
                "precheck_build_site" => typeof(NativeResult<NativeSite>), "timberborn_status" => typeof(NativeResult<NativeStatus>),
                "validate_build_site" => typeof(NativeResult<NativeValidation>),
                "place_path" or "place_lodge" => typeof(NativeResult<NativePlacement>),
                "inspect_building" => typeof(NativeResult<NativeBuilding>),
                "inspect_simulation" => typeof(NativeResult<NativeSimulation>),
                "inspect_workforce" => typeof(NativeResult<NativeWorkforce>),
                "set_simulation_speed" => typeof(NativeResult<NativeSpeedResult>),
                "set_workplace_staffing" => typeof(NativeResult<NativeStaffingResult>),
                _ => typeof(NativeResult<NativeSnapshot>) };
            return new Tool { Name = name,
                Description = staffing ? "Ändert reguläre Sollbesetzung eines fertigen Arbeitsplatzes. ID, aktuelle Session, gewünschte und zuletzt gelesene Sollbesetzung erforderlich. Eigenes Mod-/MCP-Opt-in; Bereich 0..64 und höchstens Spiel-Maximum. Nutzt reguläre +/-Methoden, keine direkte Arbeiterzuweisung. applied bestätigt nur Sollwert. Teiländerung bei unconfirmed/Fehler möglich: niemals automatisch wiederholen oder zurücksetzen; inspect_building und inspect_workforce nachlesen."
                    : speed ? "Setzt Pause (0) oder eine der drei regulären Spielstufen: 1× (1), 3× (3), 7× (7). Ab Bridge 0.11.0. Eigene Freigabe in Mod und MCP, frische Session und expectedSpeed nötig. Vergleicht aktuellen Wert auf dem Spielthread; überschreibt keine abweichende Nutzereinstellung. Entsperrt keine Spielsperren. Kein automatisches Retry; inspect_simulation danach zwingend lesen. matchedImmediately ist nur eine unmittelbare Beobachtung, keine Garantie für tickende Simulation."
                    : name == "inspect_workforce" ? "Kolonieweite Zuordnung von Worker-Entities zu Arbeitsgebäuden, maximal 32 je Seite: Arbeiter-ID, WorkerType, Employed, JobRunning, Arbeitsplatz-ID/Vorlage/Position. Totals gelten für Worker-Komponenten, nicht für Bevölkerung oder Arbeitsfähigkeit. assigned/unassigned/unresolved beschreibt die Arbeitsplatzreferenz; Employed bleibt getrennt. Kein exakter Tätigkeitstyp und kein Produktionsbeweis. Seiten sind frische Beobachtungen; Session beachten. Benötigt Bridge 0.9.0."
                    : name == "inspect_simulation" ? "Liest SpeedManager.CurrentSpeed, Spieltag, Tagesfortschritt und vergangene Stunden. Keine gemessene Tickrate; eine Spielsperre kann Fortschritt verhindern. Benötigt Bridge 0.8.0."
                    : placement ? "Pilot: erteilt genau einen Auftrag für die angegebene Pilotvorlage (Path oder Lodge.Folktails) über den regulären Spielplatzierer, nach frischer Vorschauvalidierung. Benötigt aktuelle Session-ID und separates Mod-Opt-in. Weg und Lodge teilen sich einen Versuch je Sitzung, auch bei Ablehnung/Fehler. applied bestätigt Entity-ID, Vorlage und Position; finished separat. Bei Fehler/unconfirmed niemals automatisch wiederholen oder zurücksetzen; find_buildings zur Klärung lesen. Keine Erreichbarkeitsgarantie."
                    : validation ? "Geschützter Pilot: erzeugt/verwendet eine eigene temporäre Vorschau und ruft Spielvalidatoren auf. Kein Bauauftrag. Frische Session-ID erforderlich, maximal 8 Versuche pro Sitzung. Bei Fehler/Zustandsabweichung gesperrt; niemals automatisch wiederholen. Valid bedeutet geometrische Spielprüfung, keine Material-/Liefer-/Fertigstellungszusage."
                    : name == "inspect_building" ? "Liest ein Gebäude/Path anhand Entity-ID und aktueller Session. Meldet Fertigstatus, Baustellenfortschritt, gesamte Vorlagen-Baukosten und aktuellen Baustellenbestand getrennt. Kein berechneter Restbedarf: schon verbaute Materialien und Lieferungen unterwegs sind unbekannt. Dazu Betriebs-, Instant- und Baudistrikt-IDs. Fehlend/kein Gebäude: found=false. Fehlende Komponente ist unbekannt, keine bestätigte Nichtanbindung. Keine Arbeiter-/Liefergarantie. Baustellen-Materialvertrag benötigt Bridge 0.6.1. Ab 0.9.0 operations: Pausefähigkeit/-status und bei fertigen Arbeitsgebäuden Soll-/Ist-/Maximalbesetzung, Unter-/Überbesetzung und laufender Arbeitsauftrag. Fehlende Komponente oder operations=null bedeutet unbekannt/nicht verfügbar, nicht null Arbeiter. Besetzung und laufender Auftrag garantieren keine Produktion; Blockierungsgründe noch unbekannt."
                    : name == "find_buildings" ? "Liest Gebäude und Wege mit stabilen Vorlagen-IDs, Position, Eingang und bis zu 64 belegten Zellen je Objekt. Maximal 32 Einträge; Seiten sind frische Beobachtungen."
                    : name == "inspect_build_catalog" ? "Liest die Pilotvorlagen Lodge.Folktails und Path, aktive Fraktion, Freischaltung, Geometrie und Kosten. Keine gesamte Bauliste; globale Vorräte garantieren keine lokale Lieferung."
                    : site ? "Rein lesende räumliche Vorprüfung für Lodge.Folktails oder Path. Rotation 0/1/2/3 entspricht Cw0/Cw90/Cw180/Cw270; ungespiegelt. Meldet Hindernisse, Terrain, Eingang und Kosten. gameValidated bleibt false: KEINE vollständige Spiel-Bauprüfung, Freigabe oder Platzierung."
                    : map ? "Liest höchstens 8x8x4 Spielzellen. Rohkoordinaten; keine Bauplatz- oder Erreichbarkeitsgarantie."
                    : name == "inspect_colony" ? "Eigene Spielmod: drei Beispielgüter (keine gesamte Nahrung), Betten, Personal und bis zu 16 Blockobjekte mit Position. Namen sind Daten. Enthält Sitzungskennung."
                    : "Prüft die eigene lesende Spielmod. Kein Fallback auf Fremdmods, keine Schreibfunktionen.",
                InputSchema = JsonSerializer.SerializeToElement(input), OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(result)),
                Annotations = new() { ReadOnlyHint = !action, DestructiveHint = action, IdempotentHint = !action, OpenWorldHint = false } };
        }).Concat(ManagementTools.Catalog(enablePriorities, enableAreas)).Concat(RemovalTools.Catalog(enableRemoval)).Concat(BuildingTools.Catalog(enableBuildingPlacement)).Concat(BuildingSettingsTools.Catalog(enableBuildingSettings)).Append(ActivityTools.Reader()).Select(ActivityTools.WithReasoning).ToArray();
    }
    public async Task<JsonObject> Invoke(string name, JsonElement args, CancellationToken ct)
    {
        JsonObject Wrap<T>(BridgeEnvelope<T> envelope) => (JsonObject)JsonSerializer.SerializeToNode(
            new NativeResult<T>(1, "ok", envelope.Data, new("native", false, envelope.SessionId, envelope.ObservedAtUtc), null), NativeJson.Options)!;
        try
        {
            args=ActivityTools.WithoutReasoning(args);
            if(name=="inspect_agent_log") return await ActivityTools.Read(client,args,ct);
            if (BuildingSettingsTools.Handles(name)) return await BuildingSettingsTools.Invoke(client,name,args,enableBuildingSettings,ct);
            if (BuildingTools.Handles(name)) return await BuildingTools.Invoke(client,name,args,enableBuildingPlacement,ct);
            if (RemovalTools.Handles(name)) return await RemovalTools.Invoke(client,name,args,enableRemoval,ct);
            if (ManagementTools.Handles(name)) return await ManagementTools.Invoke(client,name,args,enablePriorities,enableAreas,ct);
            if (name == "inspect_building")
            {
                var query = new NameValueCollection();
                foreach (var p in args.EnumerateObject())
                {
                    if (p.Value.ValueKind != JsonValueKind.String) throw new ArgumentException();
                    query.Add(p.Name, p.Value.GetString());
                }
                return Wrap(await client.Building(BridgeRequest.Parse("/agent-api/v1/building", query), ct));
            }
            if (name == "validate_build_site" && !enableValidation) throw new ArgumentException();
            if (name == "place_path" && !enablePlacement) throw new ArgumentException();
            if (name == "place_lodge" && !enableLodgePlacement) throw new ArgumentException();
            if (name == "inspect_simulation" && !args.EnumerateObject().Any()) return Wrap(await client.Simulation(ct));
            if (name == "set_workplace_staffing")
            {
                if (!enableStaffing) throw new ArgumentException();
                var query = new NameValueCollection();
                foreach (var p in args.EnumerateObject()) {
                    if ((p.Name is "id" or "session") && p.Value.ValueKind == JsonValueKind.String) query.Add(p.Name, p.Value.GetString());
                    else if (p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetInt32(out var v)) query.Add(p.Name, v.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    else throw new ArgumentException();
                }
                return Wrap(await client.SetStaffing(BridgeRequest.Parse("/agent-api/v1/workplace-staffing", query), ct));
            }
            if (name == "set_simulation_speed")
            {
                if (!enableSpeedControl) throw new ArgumentException();
                var query = new NameValueCollection();
                foreach (var p in args.EnumerateObject())
                {
                    if (p.Name == "session" && p.Value.ValueKind == JsonValueKind.String) query.Add(p.Name, p.Value.GetString());
                    else if (p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetInt32(out var value)) query.Add(p.Name, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    else throw new ArgumentException();
                }
                return Wrap(await client.SetSpeed(BridgeRequest.Parse("/agent-api/v1/simulation-speed", query), ct));
            }
            if (name is "inspect_map_region" or "find_buildings" or "inspect_workforce" or "precheck_build_site" or "validate_build_site" or "place_path" or "place_lodge")
            {
                var query = new NameValueCollection();
                foreach (var property in args.EnumerateObject())
                {
                    if ((name is "precheck_build_site" or "validate_build_site" or "place_path" or "place_lodge") &&
                        (property.Name == "template" || ((name is "validate_build_site" or "place_path" or "place_lodge") && property.Name == "session")) && property.Value.ValueKind == JsonValueKind.String)
                    { query.Add(property.Name, property.Value.GetString()); continue; }
                    if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out var value)) throw new ArgumentException();
                    query.Add(property.Name, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (name == "inspect_workforce") return Wrap(await client.Workforce(BridgeRequest.Parse("/agent-api/v1/workforce", query), ct));
                if (name == "find_buildings") return Wrap(await client.Objects(BridgeRequest.Parse("/agent-api/v1/objects", query), ct));
                if (name == "validate_build_site") return Wrap(await client.Validate(BridgeRequest.Parse("/agent-api/v1/site-validation", query), ct));
                if (name == "place_lodge") return Wrap(await client.PlaceLodge(BridgeRequest.Parse("/agent-api/v1/lodge-placement", query), ct));
                if (name == "place_path") return Wrap(await client.PlacePath(BridgeRequest.Parse("/agent-api/v1/path-placement", query), ct));
                if (name == "precheck_build_site") return Wrap(await client.Precheck(BridgeRequest.Parse("/agent-api/v1/site-precheck", query), ct));
                return Wrap(await client.Map(BridgeRequest.Parse("/agent-api/v1/map", query), ct));
            }
            if (name == "inspect_build_catalog" && !args.EnumerateObject().Any()) return Wrap(await client.Catalog(ct));
            if (args.EnumerateObject().Any() || name is not ("inspect_colony" or "timberborn_status")) throw new ArgumentException();
            var snapshot = await client.Snapshot(ct);
            return name == "inspect_colony" ? Wrap(snapshot) : Wrap(new BridgeEnvelope<NativeStatus>(1, snapshot.SessionId,
                snapshot.ObservedAtUtc, snapshot.BridgeVersion, new("reachable", snapshot.BridgeVersion, enablePlacement || enableLodgePlacement || enableSpeedControl || enableStaffing || enablePriorities || enableAreas || enableRemoval || enableBuildingPlacement || enableBuildingSettings)));
        }
        catch (Exception ex) when (ex is ArgumentException or HttpRequestException or IOException or InvalidDataException or JsonException or UnauthorizedAccessException or OperationCanceledException)
        {
            string code = ex is ArgumentException ? "invalid_argument" : ex is UnauthorizedAccessException ? "authentication_failed"
                : ex is JsonException or InvalidDataException ? "backend_incompatible" : "backend_unavailable";
            return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<object>(1, "error", null,
                new("native", false, null, null), new(code, BuildingSettingsTools.Writes(name) || name is "place_building" or "place_path" or "place_lodge" or "set_simulation_speed" or "set_workplace_staffing" or "set_building_priority" or "set_area" or "demolish_building" or "remove_planted" or "remove_vegetation" or "remove_debris" ? "Aktionsergebnis möglicherweise unbestätigt. Nur lesend klären; niemals automatisch erneut ausführen." : "Native Bridge: Parameter, Verbindung oder Daten prüfen.", code == "backend_unavailable" && !BuildingSettingsTools.Writes(name) && name is not ("validate_building" or "place_building" or "validate_build_site" or "place_path" or "place_lodge" or "set_simulation_speed" or "set_workplace_staffing" or "set_building_priority" or "set_area" or "demolish_building" or "remove_planted" or "remove_vegetation" or "remove_debris"))), NativeJson.Options)!;
        }
    }
    public async Task<JsonObject> InvokeLogged(string name,JsonElement args,CancellationToken ct)
    {
        var id=Guid.NewGuid().ToString("D");var detail=ActivityTools.Describe(args);
        // Unknown names are never allowed to inject arbitrary text into the game log.
        var displayName=name.Length is >0 and <=80 && name.All(c=>c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_')?name:"unknown_tool";
        var logSession=await client.TryRecordActivity(id,displayName,"running","",detail.Reasoning,detail.Summary);
        string state="error";
        try {
            var result=await Invoke(name,args,ct);
            state=result["status"]?.GetValue<string>()=="error"?"error":result["data"]?["outcome"]?.GetValue<string>() is "applied" or "rejected" or "unconfirmed" ? result["data"]!["outcome"]!.GetValue<string>() : "ok";
            return result;
        } catch(OperationCanceledException){state="cancelled";throw;}
        finally {
            if(logSession is not null) {
                var logged=await client.TryRecordActivity(id,displayName,state,logSession,"","");
                if(logged is null)Console.Error.WriteLine("MCP activity completion could not be recorded; do not retry the action.");
            } else Console.Error.WriteLine("Ingame MCP activity log unavailable for this call.");
        }
    }
    public void Dispose() => client.Dispose();
}
