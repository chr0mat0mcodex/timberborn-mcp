using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public static class BuildingTools
{
    private static readonly Dictionary<string,string> Routes = new() {
        ["execute_building_project_pilot"]="building-project-execute", ["inspect_building_project"]="building-project-status",
        ["validate_building_project"]="building-project-validation", ["plan_building_project"]="building-plan", ["inspect_build_options"]="building-catalog", ["precheck_building"]="building-precheck",
        ["validate_building"]="building-validation", ["place_building"]="building-placement" };
    public static bool Handles(string name) => Routes.ContainsKey(name);
    public static bool Writes(string name) => name is "execute_building_project_pilot" or "validate_building_project" or "validate_building" or "place_building";
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        foreach (var name in Routes.Keys.Where(n => enabled || !Writes(n)))
        {
            bool execute = name == "execute_building_project_pilot", status = name == "inspect_building_project", projectValidation = name is "validate_building_project" or "execute_building_project_pilot", plan = name is "plan_building_project" or "validate_building_project" or "execute_building_project_pilot", catalog = name == "inspect_build_options", write = Writes(name), place = name == "place_building";
            var props = new JsonObject();
            void Num(string key, int min, int max) => props[key] = new JsonObject { ["type"]="integer", ["minimum"]=min, ["maximum"]=max };
            if (status) { foreach(var key in new[]{"session","actionId"}) props[key]=new JsonObject {["type"]="string",["format"]="uuid"}; }
            else if (catalog) { Num("offset",0,65535); Num("limit",1,32); }
            else {
                props["template"] = new JsonObject { ["type"]="string", ["minLength"]=1, ["maxLength"]=160, ["pattern"]="^[A-Za-z0-9._-]+$" };
                foreach (var k in new[] { "x","y","z" }) Num(k,0,4095);
                Num("rotation",0,3);
                if(plan) { Num("width",1,8);Num("height",1,8);props["districtId"]=new JsonObject {["type"]="string",["format"]="uuid"}; }
                if (write || plan) props["session"] = new JsonObject { ["type"]="string", ["format"]="uuid" };
                if(projectValidation) { Num("optionIndex",0,3);props["planKey"]=new JsonObject {["type"]="string",["pattern"]="^[a-f0-9]{64}$"}; }
                if (execute) props["mode"]=new JsonObject {["type"]="string",["enum"]=new JsonArray("development_pilot")};
                if (place || execute) props["actionId"] = new JsonObject { ["type"]="string", ["format"]="uuid" };
            }
            string description = catalog ? "Vollständiger seitenweiser Gebäudekatalog der aktiven Spiel-Template-Sammlungen: IDs, Baukosten, Freischaltung, Geometrie, supported und unsupportedReasons. Alle Seiten lesen. supported beschreibt den Baupfad, keinen Live-Nachweis oder funktionierenden Betrieb. Gebäudekonfiguration nach Bau separat."
                : place ? "Erteilt einen regulären Bauauftrag für eine unterstützte Katalogvorlage. Frische Spielvalidierung inklusive. x/y/z ist BlockObject-Ursprung, rotation 0/1/2/3=Cw0/90/180/270, ungespiegelt. Frische session und NEUE actionId (UUID) nötig; diese wird Entity-ID. Jede ID ist pro Sitzung nur einmal ausführbar, auch bei Fehlern. Maximal 256 Versuche/Sitzung. Keine automatische Wiederholung; bei Unsicherheit inspect_building mit actionId lesen. applied bedeutet platziert, finished bedeutet fertig; Materialien, Bauzeit und Betrieb bleiben Spielaufgabe. Keine Überschreibung vorhandener Objekte."
                : write ? "Prüft eine unterstützte Katalogvorlage über eigene Spielvorschau und beide Spielvalidatoren, ohne Bauauftrag. Frische session erforderlich. Maximal 256 generische Prüfungen (inklusive Platzierungen) und 64 gecachte Vorlagen je Sitzung. Keine Erreichbarkeits-/Liefergarantie. Fehler nicht automatisch wiederholen."
                : "Liest Hindernisse, Gelände, Eingang und Kosten für eine unterstützte Katalogvorlage. x/y/z ist BlockObject-Ursprung; rotation 0/1/2/3=Cw0/90/180/270, ungespiegelt. Keine vollständige Spielvalidierung oder Erreichbarkeitsgarantie. Sonderlayouts laut Katalog nicht unterstützt.";
            if(plan) description="Liest bis zu vier NICHT AUSFÜHRBARE Kandidaten für Gebäude und ebenen Anschlussweg. x/y/z plus width/height (1..8) begrenzen Grundriss UND komplette Route, eine rotation (0..3). districtId muss fertiges Distriktzentrum sein; aktuelle session erforderlich. Maximal 64 Ursprünge, deterministische Reihenfolge, keine globale Optimierung. Nur Bodenwege, keine Treppen/Rodung/Plattformen. routeCells und newRoadCells laufen vom bestehenden Distriktweg zum Eingang. Vorprüfungen und Rasterroute sind keine gemeinsame Spielvalidierung, Baustellen-Erreichbarkeit oder Wegschutzgarantie. executable ist immer false. Kein Bauauftrag, keine Vorschau, kein gespeicherter Ausführungsplan; keine automatische Umsetzung über Einzelplatzierungen. searchComplete/stopReason lesen. Ab Bridge 0.24.0.";
            if(projectValidation) description="Gemeinsame temporäre Vorschau für einen frisch neu berechneten Bauplan. Gleiche Suchparameter plus optionIndex und planKey aus plan_building_project, aktuelle session. Höchstens acht neue Wege und ein Gebäude; 16 Versuche/Sitzung. Separates Bau-Opt-in. Kein Bauauftrag, executable bleibt false. Prüft Wegpräfixe, Gebäude und Vorschau-Eingangsverbindung sowie Wiederherstellung; kein Bauarbeiter-/Bauphasennachweis. Bei Fehler oder sessionLocked nicht wiederholen.";
            if(execute) description="Expliziter Entwicklungspilot: ein SmallWarehouse.Folktails mit höchstens vier ebenen neuen Wegen, einmal je Sitzung. mode=development_pilot bestätigt den noch unbelegten Bauphasen-Vorabnachweis. Aktuelle session, Planparameter/planKey und neue actionId erforderlich. Spiel muss pausiert bleiben. Prüft gemeinsame Vorschau, setzt regulär schrittweise und kontrolliert reale Verbindungen/Bauarbeiterzugang. Laufender Auftrag: inspect_building_project abfragen. Teilstände bleiben bei Abbruch erhalten; kein Rollback/automatischer Retry. Identische ID+Parameter liefert nur bisherigen Status. completed bedeutet Bauauftrag und Zugang bestätigt, nicht fertig gebaut oder vollständigen Wegschutz. Ab Bridge 0.26.0.";
            if(status) description="Liest den sitzungslokalen Baupilot-Auftrag mit Weg-/Gebäude-IDs und bestätigtem oder unbestätigtem Teilstand. session und actionId erforderlich; keine Spieländerung. Nach Neustart reale Objekte abgleichen. Ab Bridge 0.26.0.";
            Type result = execute || status ? typeof(NativeResult<NativeProjectExecution>) : projectValidation ? typeof(NativeResult<NativeProjectValidation>) : plan ? typeof(NativeResult<NativeBuildingPlan>) : catalog ? typeof(NativeResult<NativeBuildingCatalog>) : place ? typeof(NativeResult<NativePlacement>)
                : write ? typeof(NativeResult<NativeValidation>) : typeof(NativeResult<NativeSite>);
            if (write && !execute) description += " Ab Bridge 0.23.0 experimentelle Wegschutz-Diagnose: roadProtection getrennt prüfen; valid ist nur geometrische Spielvalidierung. Nur roadProtection.status=safe erlaubt Platzierung. Diese Diagnoseversion liefert mangels belegter Baustellen-/Wegknotenabdeckung noch keine safe-Freigaben und verweigert deshalb sämtliche Bauaufträge. blocked benennt verlorene Distriktzugänge, unknown eine Nachweislücke. Keine ungeprüfte Umgehung über Pilotplatzierungen.";
            yield return new Tool { Name=name, Description=description,
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject { ["type"]="object", ["properties"]=props,
                    ["required"]=new JsonArray(props.Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()), ["additionalProperties"]=false }),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(result)),
                Annotations=new() { ReadOnlyHint=!write, DestructiveHint=write, IdempotentHint=!write, OpenWorldHint=false } };
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled, CancellationToken ct)
    {
        if (Writes(name) && !enabled) throw new ArgumentException();
        var q = new NameValueCollection();
        foreach (var p in args.EnumerateObject()) {
            if (p.Name is "template" or "session" or "actionId" or "districtId" or "planKey" or "mode") {
                if (p.Value.ValueKind != JsonValueKind.String) throw new ArgumentException();
                q.Add(p.Name,p.Value.GetString());
            } else {
                if (p.Value.ValueKind != JsonValueKind.Number || !p.Value.TryGetInt32(out int n)) throw new ArgumentException();
                q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        JsonObject Wrap<T>(BridgeEnvelope<T> e) => (JsonObject)JsonSerializer.SerializeToNode(
            new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        if(name is "execute_building_project_pilot" or "inspect_building_project") return Wrap(await client.BuildingProjectExecution(BuildingProjectExecutionRequest.Parse(name=="execute_building_project_pilot",q),ct));
        if(name=="validate_building_project")return Wrap(await client.ValidateProject(BuildingProjectValidationRequest.Parse(q),ct));
        if(name=="plan_building_project")return Wrap(await client.BuildingPlan(BuildingPlanRequest.Parse(q),ct));
        var r = BridgeRequest.Parse("/agent-api/v1/"+Routes[name],q);
        return name switch {
            "inspect_build_options" => Wrap(await client.BuildingCatalog(r,ct)),
            "precheck_building" => Wrap(await client.Precheck(r,ct)),
            "validate_building" => Wrap(await client.Validate(r,ct)),
            "place_building" => Wrap(await client.PlaceBuilding(r,ct)),
            _ => throw new ArgumentException() };
    }
}
