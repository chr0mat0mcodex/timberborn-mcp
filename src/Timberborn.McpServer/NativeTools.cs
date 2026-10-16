using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed class NativeTools(NativeClient client, bool enableValidation = false, bool enablePlacement = false, bool enableLodgePlacement = false, bool enableSpeedControl = false, bool enableStaffing = false, bool enablePriorities = false, bool enableAreas = false, bool enableRemoval = false, bool enableBuildingPlacement = false, bool enableBuildingSettings = false, bool enableResearch = false, DiagnosticFileLog? fileLog=null) : IDisposable
{
    private readonly ClearedGroupTools clearedGroups = new(new NativeClearedGroupPort(client), Path.Combine(AppContext.BaseDirectory,".local","cleared-groups"));
    private readonly SemaphoreSlim callGate = new(1,1);
    public static IReadOnlyList<Tool> Catalog(bool enableValidation = false, bool enablePlacement = false, bool enableLodgePlacement = false, bool enableSpeedControl = false, bool enableStaffing = false, bool enablePriorities = false, bool enableAreas = false, bool enableRemoval = false, bool enableBuildingPlacement = false, bool enableBuildingSettings = false, bool enableResearch = false)
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var names = new List<string> { "timberborn_status", "inspect_colony", "inspect_map_region", "inspect_build_catalog", "precheck_build_site", "inspect_building", "inspect_simulation", "inspect_workforce" };
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
                    : placement ? "Ab Bridge 0.23.0 Diagnoseversion: Bauaufträge bis zum belegten Wegschutz gesperrt; roadProtection.status und reasons lesen. Pilot: erteilt genau einen Auftrag für die angegebene Pilotvorlage (Path oder Lodge.Folktails) über den regulären Spielplatzierer, nach frischer Vorschauvalidierung. Benötigt aktuelle Session-ID und separates Mod-Opt-in. Weg und Lodge teilen sich einen Versuch je Sitzung, auch bei Ablehnung/Fehler. applied bestätigt Entity-ID, Vorlage und Position; finished separat. Bei Fehler/unconfirmed niemals automatisch wiederholen oder zurücksetzen; find_buildings zur Klärung lesen. Keine Erreichbarkeitsgarantie."
                    : validation ? "Ab Bridge 0.23.0 roadProtection getrennt von geometrischem valid lesen; keine safe-Freigabe ohne vollständigen Wegschutz. Geschützter Pilot: erzeugt/verwendet eine eigene temporäre Vorschau und ruft Spielvalidatoren auf. Kein Bauauftrag. Frische Session-ID erforderlich, maximal 8 Versuche pro Sitzung. Bei Fehler/Zustandsabweichung gesperrt; niemals automatisch wiederholen. Valid bedeutet geometrische Spielprüfung, keine Material-/Liefer-/Fertigstellungszusage."
                    : name == "inspect_building" ? "Liest ein Gebäude/Path anhand Entity-ID und aktueller Session. Meldet Fertigstatus, Baustellenfortschritt, gesamte Vorlagen-Baukosten und aktuellen Baustellenbestand getrennt. Kein berechneter Restbedarf: schon verbaute Materialien und Lieferungen unterwegs sind unbekannt. Dazu Betriebs-, Instant- und Baudistrikt-IDs. Fehlend/kein Gebäude: found=false. Fehlende Komponente ist unbekannt, keine bestätigte Nichtanbindung. Keine Arbeiter-/Liefergarantie. Baustellen-Materialvertrag benötigt Bridge 0.6.1. Ab 0.9.0 operations: Pausefähigkeit/-status und bei fertigen Arbeitsgebäuden Soll-/Ist-/Maximalbesetzung, Unter-/Überbesetzung und laufender Arbeitsauftrag. Fehlende Komponente oder operations=null bedeutet unbekannt/nicht verfügbar, nicht null Arbeiter. Besetzung und laufender Auftrag garantieren keine Produktion; Blockierungsgründe noch unbekannt."
                    : name == "find_buildings" ? "Liest Gebäude und Wege mit stabilen Vorlagen-IDs, Position, Eingang und bis zu 512 belegten Zellen je Objekt. Maximal 32 Einträge; Seiten sind frische Beobachtungen."
                    : name == "inspect_build_catalog" ? "Liest die Pilotvorlagen Lodge.Folktails und Path, aktive Fraktion, Freischaltung, Geometrie und Kosten. Keine gesamte Bauliste; globale Vorräte garantieren keine lokale Lieferung."
                    : site ? "Rein lesende räumliche Vorprüfung für Lodge.Folktails oder Path. Rotation 0/1/2/3 entspricht Cw0/Cw90/Cw180/Cw270; ungespiegelt. Meldet Hindernisse, Terrain, Eingang und Kosten. gameValidated bleibt false: KEINE vollständige Spiel-Bauprüfung, Freigabe oder Platzierung."
                    : map ? "Liest bis 8x8x4 Zellen: Gelände, Wasser, Verschmutzung und soilIsMoist (ab Bridge 0.35.4, nur Boden; null=unbekannt/nicht anwendbar). Aktuelle Feuchte ist keine Pflanz-, Ertrags- oder Erreichbarkeitsgarantie."
                    : name == "inspect_colony" ? "Eigene Spielmod: drei Beispielgüter (keine gesamte Nahrung), Betten, Personal und bis zu 16 Blockobjekte mit Position. Namen sind Daten. Enthält Sitzungskennung."
                    : "Liest Erreichbarkeit, Bridge-Version und aktuelle Spielsitzung der eigenen Mod. Dieser Aufruf ist lesend; der Teststarter aktiviert alle vorhandenen Aktionsgruppen. Die bestehende MCP-Verbindung des Clients verwenden; für Spielaufgaben keine zusätzlichen Startskripte oder dotnet-Prozesse starten. stdio wird vom Client verwaltet und erlaubt keinen nachträglichen Anschluss an einen separat gestarteten Prozess. backend_unavailable ist kein Beleg für einen defekten MCP-Prozess; Spiel, Mod und Ladezustand prüfen. Kein Fallback auf Fremdmods.",
                InputSchema = JsonSerializer.SerializeToElement(input), OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(result)),
                Annotations = new() { ReadOnlyHint = !action, DestructiveHint = action, IdempotentHint = !action, OpenWorldHint = false } };
        }).Concat(BuildingQueryTools.Catalog()).Concat(SimulationRunTools.Catalog(enableSpeedControl)).Concat(BuildingConfigurationTools.Catalog(enableBuildingSettings && enableStaffing && enableSpeedControl)).Concat(ManagementTools.Catalog(enablePriorities, enableAreas)).Concat(PlantingBatchTools.Catalog(enableAreas)).Concat(RemovalTools.Catalog(enableRemoval)).Concat(BuildingTools.Catalog(enableBuildingPlacement)).Concat(BuildingSettingsTools.Catalog(enableBuildingSettings)).Concat(ResearchTools.Catalog(enableResearch)).Concat(EconomyTools.Catalog()).Concat(LogisticsTools.Catalog()).Concat(DiagnosticsTools.Catalog()).Append(RecentActionsTools.Reader()).Append(ActivityTools.Reader()).Append(ProductionGraphTools.Reader()).Append(SelectionTools.Reader()).Append(ScreenshotTools.Reader()).Append(BuildingCapabilityTools.Reader()).Append(ColonyOverviewTools.Reader()).Append(RegionSurveyTools.Reader()).Append(StrategicMapTools.Reader()).Concat(RegionBuildPlanning.Catalog(enableBuildingPlacement)).Concat(ForestryTools.Catalog(enableAreas)).Concat(WorkflowTools.Catalog(enableRemoval, enableBuildingPlacement)).Concat(BuildingCompletionTools.Catalog(enableBuildingPlacement && enableSpeedControl)).Concat(BuildBatchTools.Catalog(enableBuildingPlacement && enableSpeedControl)).Concat(LargeProjectTools.Catalog(enableBuildingPlacement, enableSpeedControl)).Concat(ClearedGroupTools.Catalog(enableBuildingPlacement && enableSpeedControl && enableRemoval && enableAreas)).Select(ActivityTools.WithReasoning).ToArray();
    }
    public async Task<JsonObject> Invoke(string name, JsonElement args, CancellationToken ct)
    {
        using var dispatch=new NativeDispatchTrace();
        bool ownsGate=false;
        JsonObject Wrap<T>(BridgeEnvelope<T> envelope) => (JsonObject)JsonSerializer.SerializeToNode(
            new NativeResult<T>(1, "ok", envelope.Data, new("native", false, envelope.SessionId, envelope.ObservedAtUtc), null), NativeJson.Options)!;
        try
        {
            // Admission belongs to the same result boundary as native dispatch.
            // Reject contention instead of queuing an action behind an unknown result.
            ownsGate=await callGate.WaitAsync(0,ct);
            if(!ownsGate)throw new ArgumentException("another_call_in_progress_not_queued");
            args=ActivityTools.WithoutReasoning(args);
            if(ClearedGroupTools.Handles(name))return await clearedGroups.Invoke(name,args,enableBuildingPlacement && enableSpeedControl && enableRemoval && enableAreas,ct);
            if(clearedGroups.Active && Catalog(true,true,true,true,true,true,true,true,true,true,true).Any(t=>t.Name==name && t.Annotations?.ReadOnlyHint!=true))
                throw new ArgumentException("cleared_group_owns_dispatch_stop_or_wait");
            if(PlantingBatchTools.Handles(name))return await PlantingBatchTools.Invoke(client,name,args,enableAreas,ct);
            if (StatusWaitTools.Handles(name)) return await StatusWaitTools.Invoke(client,name,args,ct);
            if(LargeProjectTools.Handles(name)) return await LargeProjectTools.Invoke(client,name,args,enableBuildingPlacement,ct,enableSpeedControl);
            if (name==RecentActionsTools.Name) return await RecentActionsTools.Invoke(client,args,ct);
            if (BuildingQueryTools.Handles(name)) return await BuildingQueryTools.Invoke(client,name,args,ct);
            if (BuildBatchTools.Handles(name)) return await BuildBatchTools.Invoke(client, name, args, enableBuildingPlacement && enableSpeedControl, enableRemoval, enableBuildingSettings, ct);
            if (BuildingCompletionTools.Handles(name)) return await BuildingCompletionTools.Invoke(client, name, args, enableBuildingPlacement && enableSpeedControl, ct, enableBuildingSettings);
            if (WorkflowTools.Handles(name)) return await WorkflowTools.Invoke(client, name, args, enableRemoval, enableBuildingPlacement, enableBuildingSettings, ct);
            if (ForestryTools.Handles(name)) return await ForestryTools.Invoke(client, name, args, enableAreas, ct);
            if (name == ScreenshotTools.Name) return await ScreenshotTools.Invoke(client, args, ct);
            if (name == RegionBuildPlanning.Name) return await RegionBuildPlanning.Invoke(client,args,enableBuildingPlacement,ct);
            if (name == RegionSurveyTools.Name) return await RegionSurveyTools.Invoke(client, args, ct);
            if (name == StrategicMapTools.Name) return await StrategicMapTools.Invoke(client, args, ct);
            if (name == ColonyOverviewTools.Name) return await ColonyOverviewTools.Invoke(client, args, ct);
            if (BuildingConfigurationTools.Handles(name)) return await BuildingConfigurationTools.Invoke(client,name,args,enableBuildingSettings && enableStaffing && enableSpeedControl,ct);
            if (SimulationRunTools.Handles(name)) return await SimulationRunTools.Invoke(client,name,args,enableSpeedControl,ct);
            if(name=="inspect_production_graph") return await ProductionGraphTools.Invoke(client,args,ct);
            if(name=="inspect_agent_log") return await ActivityTools.Read(client,args,ct);
            if (DiagnosticsTools.Handles(name)) return await DiagnosticsTools.Invoke(client,name,args,ct);
            if (name == "inspect_selection") return await SelectionTools.Invoke(client,args,ct);
            if (name == "inspect_building_capabilities") return await BuildingCapabilityTools.Invoke(client,args,enableBuildingPlacement,ct);
            if (LogisticsTools.Handles(name)) return await LogisticsTools.Invoke(client,name,args,ct);
            if (EconomyTools.Handles(name)) return await EconomyTools.Invoke(client,name,args,ct);
            if (ResearchTools.Handles(name)) return await ResearchTools.Invoke(client,name,args,enableResearch,ct);
            if (BuildingSettingsTools.Handles(name)) return await BuildingSettingsTools.Invoke(client,name,args,enableBuildingSettings,ct);
            if (BuildingTools.Handles(name)) return await BuildingTools.Invoke(client,name,args,enableBuildingPlacement,ct,enableBuildingSettings);
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
                snapshot.ObservedAtUtc, snapshot.BridgeVersion, new("reachable", snapshot.BridgeVersion, enablePlacement || enableLodgePlacement || enableSpeedControl || enableStaffing || enablePriorities || enableAreas || enableRemoval || enableBuildingPlacement || enableBuildingSettings || enableResearch)));
        }
        catch (Exception ex) when (ex is ArgumentException or HttpRequestException or IOException or InvalidDataException or JsonException or UnauthorizedAccessException or OperationCanceledException)
        {
            string code = ex is BridgeRejectionException rejection ? rejection.Code : ex is ArgumentException ? "invalid_argument" : ex is UnauthorizedAccessException ? "authentication_failed"
                : ex is JsonException or InvalidDataException ? "backend_incompatible" : "backend_unavailable";
            bool action = name==PlantingBatchTools.Apply || ClearedGroupTools.Writes(name) || name == RegionBuildPlanning.Name || LargeProjectTools.Writes(name) || ForestryTools.Writes(name) || BuildBatchTools.Handles(name) || BuildingCompletionTools.Handles(name) || WorkflowTools.Handles(name) || BuildingConfigurationTools.Writes(name) || SimulationRunTools.Writes(name) || BuildingSettingsTools.Writes(name) || name is "unlock_building" or "place_building" or "place_path" or "place_lodge" or "set_simulation_speed" or "set_workplace_staffing" or "set_building_priority" or "set_area" or "demolish_building" or "remove_planted" or "remove_vegetation" or "remove_debris";
            // This exact native rejection occurs before the planting ledger or first mutation.
            // Never infer non-execution from arbitrary HTTP conflicts or transport failures.
            bool plantingRejected=name==PlantingBatchTools.Apply && dispatch.WriteAttempts==1 &&
                ex is BridgeRejectionException { Code: "planting_preview_changed" };
            bool projectStartRejected = name is ("run_large_project" or "run_large_project_repair" or "execute_vertical_stair_pilot") &&
                dispatch.WriteAttempts == 1 && ex is BridgeRejectionException && LargeProjectStartPolicy.IsPreDispatchCode(code);
            bool projectPlanningRejected = ProjectPlanningOutcome.Handles(name) && dispatch.WriteAttempts == 1 &&
                ex is BridgeRejectionException && LargeProjectPlanningPolicy.IsPreDispatchCode(code);
            string execution=plantingRejected||projectStartRejected||projectPlanningRejected?"not_executed":dispatch.WriteAttempts>0?"unconfirmed":action||code=="invalid_argument"?"not_executed":"not_applicable";
            string message=execution switch {
                "not_executed"=>"Nicht ausgeführt: kein schreibender Spielauftrag an den Transport übergeben. Ursache korrigieren; kein automatisches Retry.",
                "unconfirmed"=>"Schreibender Spielauftrag an den Transport übergeben; Ausgang unbestätigt. Nur lesend klären, niemals automatisch wiederholen.",
                _=>"Leseabfrage fehlgeschlagen. Parameter, Spielzustand, Verbindung oder Antwortdaten prüfen."
            };
            if(plantingRejected)message="Nicht ausgeführt: native Vorprüfung hat die veraltete Pflanzvorschau vor jeder Änderung abgewiesen. Frische Vorschau lesen und neu beurteilen; kein automatisches Retry.";
            if(projectStartRejected)message=ProjectStartOutcome.Describe(code)+" Kein automatisches Retry.";
            if(projectPlanningRejected)message=ProjectPlanningOutcome.Describe(code)+" Kein automatisches Retry.";
            if(code=="project_preview_failed")message="Native Bauvorschau fehlgeschlagen; Wiederherstellung unbekannt, Planer gesperrt. Spiel neu laden und Zustand mit neuer Session prüfen; kein automatisches Retry.";
            if(code=="project_not_found")message="Unter dieser Kennung existiert kein natives Großprojekt in der aktuellen Spielsitzung. Chargenstatus und Planungsfehler prüfen; keine automatische Wiederholung.";
            if(ArgumentDiagnostics.Hint(ex) is { } hint)message+=" "+hint;
            return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<object>(1, "error", null,
                new("native", false, null, null), new(code,message,code=="backend_unavailable"&&!action&&dispatch.WriteAttempts==0,execution)), NativeJson.Options)!;
        }
        finally { if(ownsGate)callGate.Release(); }
    }
    public async Task<JsonObject> InvokeLogged(string name,JsonElement args,CancellationToken ct)
    {
        ActivityTools.RequireReasoning(args);
        var id=Guid.NewGuid().ToString("D");var detail=ActivityTools.Describe(args);
        // Unknown names are never allowed to inject arbitrary text into the game log.
        var displayName=name.Length is >0 and <=80 && name.All(c=>c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_')?name:"unknown_tool";
        var references=ActivityCorrelationTools.Request(name,args);
        fileLog?.Write("call_started",displayName,"running",callId:id,correlations:references,reasoning:detail.Reasoning,summary:detail.Summary);
        var logSession=await client.TryRecordActivity(id,displayName,"running","",detail.Reasoning,detail.Summary,references,
            (session,saveName)=>fileLog?.Write("activity_started",displayName,"running",callId:id,session:session,correlations:references,reasoning:detail.Reasoning,summary:detail.Summary,saveName:saveName));
        string state="error";
        string? errorCode=null;
        string outcomeSummary=ActivityOutcome.Handles(name)?"Kein bestätigtes Ergebnis verfügbar.":"";
        try {
            var result=await Invoke(name,args,ct);
            outcomeSummary=ActivityOutcome.Describe(name,result);
            errorCode=result["error"]?["code"]?.GetValue<string>();
            references=ActivityCorrelation.Merge(references,ActivityCorrelationTools.Result(name,result));
            state=result["status"]?.GetValue<string>()=="error"?(BridgeRejectionException.IsCode(result["error"]?["code"]?.GetValue<string>())?"rejected":"error"):result["data"]?["outcome"]?.GetValue<string>() is "applied" or "rejected" or "unconfirmed" ? result["data"]!["outcome"]!.GetValue<string>() : "ok";
            if(result["error"]?["execution"]?.GetValue<string>()=="unconfirmed")state="unconfirmed";
            if(BuildingConfigurationTools.Handles(name) && (result["data"]?["outcome"]?.GetValue<string>()=="partial" || result["data"]?["proofState"]?.GetValue<string>()=="unconfirmed"))state="unconfirmed";
            if(name=="unlock_building" && result["data"]?["outcome"]?.GetValue<string>() is "cost_changed" or "unavailable" or "insufficient_points" or "not_unlockable")state="rejected";
            if((LargeProjectTools.Handles(name) || ClearedGroupTools.Handles(name)) && result["data"]?["state"]?.GetValue<string>()=="unconfirmed")state="unconfirmed";
            if(name==PlantingBatchTools.Apply && result["data"]?["outcome"]?.GetValue<string>()=="unconfirmed")state="unconfirmed";
            return result;
        } catch(OperationCanceledException){state="cancelled";throw;}
        finally {
            fileLog?.Write("call_finished",displayName,state,errorCode,id,logSession,references,reasoning:detail.Reasoning,summary:detail.Summary,outcomeSummary:outcomeSummary);
            if(logSession is not null) {
                var logged=await client.TryRecordActivity(id,displayName,state,logSession,"","",references,outcomeSummary:outcomeSummary);
                if(logged is null) {
                    fileLog?.Write("telemetry_failed",displayName,code:"completion_unavailable",callId:id);
                    Console.Error.WriteLine("MCP activity completion could not be recorded; do not retry the action.");
                }
            } else {
                fileLog?.Write("telemetry_failed",displayName,code:"start_unavailable",callId:id);
                Console.Error.WriteLine("Ingame MCP activity log unavailable for this call.");
            }
        }
    }
    public void Dispose() {clearedGroups.Dispose();client.Dispose();}
}
