using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class LargeProjectTools
{
    private static readonly Dictionary<string,string> Operations=new(){["inspect_large_project_priority"]="priority",["set_large_project_priority"]="set-priority",["set_large_project_identity_pilot"]="identity-pilot",["plan_large_project_access"]="plan-access",["plan_large_project"]="plan",["start_large_project"]="start",["run_large_project"]="run",["advance_large_project"]="advance",["inspect_large_project"]="inspect",["stop_large_project"]="stop",["inspect_power_network"]="power-network",["diagnose_large_project"]="diagnose",["close_large_project"]="close",["plan_large_project_repair"]="plan-repair",["run_large_project_repair"]="run-repair"};
    public static bool Handles(string name)=>Operations.ContainsKey(name);
    public static bool Writes(string name)=>Operations.TryGetValue(name,out var operation)&&LargeProjectRequest.Writes(operation);
    public static IEnumerable<Tool> Catalog(bool enabled, bool speedEnabled = false)
    {
        JsonObject Id()=>new(){["type"]="string",["format"]="uuid"};
        JsonObject Number(int max)=>new(){["type"]="integer",["minimum"]=0,["maximum"]=max};
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var pair in Operations)
        {
            bool write=Writes(pair.Key);if(write&&!enabled)continue;
            if(pair.Value is ("run" or "run-repair")&&!speedEnabled)continue;
            var props=new JsonObject{["session"]=Id()};
            if(pair.Value=="power-network")props["id"]=Id();
            else if(pair.Value is "plan-access" or "plan" or "start" or "run" or "plan-repair" or "run-repair")
            {
                props["districtId"]=Id();
                props["steps"]=new JsonObject{["type"]="array",["minItems"]=1,["maxItems"]=32,["items"]=new JsonObject{
                    ["type"]="object",["additionalProperties"]=false,["properties"]=new JsonObject{
                        ["template"]=new JsonObject{["type"]="string",["minLength"]=1,["maxLength"]=160},["x"]=Number(4095),["y"]=Number(4095),["z"]=Number(4095),["rotation"]=Number(3),
                        ["dependsOn"]=new JsonObject{["type"]="array",["maxItems"]=31,["uniqueItems"]=true,["items"]=Number(31)}},
                    ["required"]=new JsonArray("template","x","y","z","rotation","dependsOn")}};
                if(pair.Value is "start" or "run" or "run-repair") {props["actionId"]=Id();props["planKey"]=new JsonObject{["type"]="string",["pattern"]="^[0-9a-f]{64}$"};props["mode"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("development_pilot")};}
                if(pair.Value is "run" or "run-repair") {
                    props["speed"]=new JsonObject{["type"]="integer",["enum"]=new JsonArray(1,3,7)};
                    props["maxGameHours"]=new JsonObject{["type"]="integer",["minimum"]=1,["maximum"]=672};
                    props["maxRealSeconds"]=new JsonObject{["type"]="integer",["minimum"]=30,["maximum"]=86400};
                }
            }
            else props["actionId"]=Id();
            if(pair.Value == "identity-pilot") {
                props["pilotAction"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("enable","disable")};
                props["partIndex"]=Number(31);
                props["mode"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("development_pilot")};
            }
            if(pair.Value is "plan-repair" or "run-repair") props["parentActionId"]=Id();
            if(pair.Value=="set-priority") {
                props["priority"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("VeryLow","Low","Normal","High","VeryHigh")};
                props["expectedRevision"]=new JsonObject{["type"]="string",["pattern"]="^[0-9a-f]{64}$"};
            }
            string description=pair.Value switch{
                "priority"=>"Liest Bauprioritäten sämtlicher Teile eines bekannten Großprojekts samt revision. Fertige und unbeauftragte Teile werden ausgewiesen. Keine Änderung.",
                "set-priority"=>"Setzt Bauprioritäten aller offenen, eindeutig bestätigten Baustellen eines Projekts. Zuerst inspect_large_project_priority lesen und dessen revision als expectedRevision senden. Alle Identitäten und Revision vor dem ersten Setter prüfen. Teilfehler stoppen weitere Setter; Einzelbelege lesen, keine automatische Wiederholung. Keine Arbeitsplatzpriorität oder garantierte Lieferreihenfolge.",
                "identity-pilot"=>"N03-Entwicklungsinjektion, standardmäßig inaktiv: genau eine bestätigte Objektidentität eines terminalen, nicht freigegebenen Projekts als identity_mismatch beobachten. Nur pausiert ohne aktive Projekte/Simulation aktivierbar. Session/actionId/partIndex gebunden, mode=development_pilot ausdrücklich erforderlich. Keine Änderung von Spielobjekt oder Journal; identityInjection kennzeichnet jeden betroffenen Recovery-Befund mit realObserved. disable entfernt die Injektion, auch wenn der reale Befund inzwischen fehlt. Nicht als natürlicher Identitätsfehler oder allgemeine Freigabe ausweisen. Nach dem Pilot immer deaktivieren; Spielneuladen verwirft die Injektion.",
                "plan-access"=>"Pausierte automatische Zugangsergänzung für explizite Gebäudeziele: Wege, Folktails-Treppen und Plattformstützen im begrenzten Umfeld suchen, Abhängigkeiten ergänzen und gemeinsam nativ prüfen. Ab 0.70: höchstens acht native Zielweg-Vorschauen an gedrehten Folktails-Bestandstreppen; searchDiagnostics Version 3 trennt Instant-Kante von previewLandingConnected. Ungeprüft bleibt null, volle Gesamtvorschau weiterhin Pflicht. Ab 0.69: Höhenanschlüsse über nativen Weggraph; searchDiagnostics Version 2 mit connectionGraph=instant_road, nativeConnected für Wegverbindung und generalConnected als getrennte Kontrolle. Ab 0.68: A*-Priorisierung nach Teilekosten und Zielentfernung; searchDiagnostics zeigt Bestandsquellen, geprüfte Höhenanschlüsse, Ablehnungszähler und Suchkosten auch bei Budgetende. Stichproben sind begrenzt und als gekürzt markiert; ungeprüft ist nicht unerreichbar. Ab 0.67: Routengeometrie verhindert Eigenkollisionen; nativ bestätigte Höhenanschlüsse vorhandener Treppen mit regulärem Zielweg nutzbar. Maximal 32 Teile/acht Höhen, 4096 Suchzustände einschließlich Geometrievarianten; bestehender Distriktweg innerhalb zwölf Zellen Rand erforderlich. Budgetgrenze beweist keine Unerreichbarkeit. Keine Platzierung. candidateFound nur mit erfolgreicher Gesamtvorschau; steps plus plan.planKey unverändert an run_large_project übergeben. Policy 1 prüft freie Arbeitszellen und nativen Distriktweg, auch vor Auftrag. blockedPart/phase erklären Ablehnungen. Tatsächlicher Bauarbeiterzugang bleibt erst an realer Baustelle nachweisbar. cleanupCandidateIndices nur nach frischer Zugangsprüfung rückbauen; Rückgewinnungsrohstoffe niemals löschen. Keine automatische Standortverschiebung, Rodung oder optimale Gesamtlösungsgarantie.",
                "diagnose"=>"progressComparison vergleicht aufeinanderfolgende frische Diagnosen desselben Projekts: erste Diagnose vor einem begrenzten Simulationsfenster erfasst den Ausgangswert, nächste liefert Material-/Baufortschritt und konkrete nächste Schritte. Endpunktbelege sind keine durchgehende Arbeit oder Bruttoliefermenge. Keine automatische Budgetverlängerung oder Materialsperre. Frische Baudiagnose berücksichtigt Bezirkszentrum und fertige Bauhütten im zuständigen Bezirk. builderWorkforce enthält Einzelbelege zu Zuordnung, Besetzung, Pause, Arbeitszeit und laufenden Jobs (maximal 64). builders_outside_working_hours nur bei vollständigen eindeutigen Belegen für Feierabend aller besetzten unpausierten Bauarbeitsplätze. Eine aktive Schicht oder ein laufender Job verhindert den Befund; fehlende Werte bleiben unbekannt. constructionCenterWorkingHours ist nur ein Einzelbeleg, keine Bezirksbaupause. Ab 0.74 trennt recoveryAdvice den beobachteten Zugangsbedarf von repairEligible (nur technische Möglichkeit); Material-/Stützwarten empfiehlt keine Zugangsreparatur. Unbekannter Zugang bleibt unknown. Ab 0.73 zeigt currentConstruction in compact/full den frisch beobachteten Fertigbau und existingAccess getrennt vom historischen Projektstatus; alter Zeitabbruch bleibt Historie. Kein Betriebsnachweis. Ab 0.72 trennt reason manuelle construction_paused, waiting_for_support_completion und construction_inactive_unknown; manuallyPaused und unfinishedSupportIndices bleiben auch kompakt sichtbar. Frischer lesender Projektabgleich: erwartete Objekte, Bauzugang, Materialbestand, verlorene Bestandsanschlüsse und fremde Baustellen. Compact trennt confirmedPresent und finishedAccessible; Kosten/Bestände mit detail=full. Auch terminale Projekte; keine Platzierung, kein Fortschalten. closeEligible/repairEligible gelten nur für diese Beobachtung. Kosten sind kein Restbedarf.",
                "close"=>"Terminales Projekt bei Pause frisch abgleichen und Ledger freigeben, ohne Objekte zu bauen oder abzureißen. Alle versuchten Teile müssen fertig und zugänglich oder nach bestätigter Beobachtung entfernt sein. Fehlende unbestätigte Aufträge blockieren. Unbeauftragte Teile werden aufgegeben; alte actionId bleibt terminal. Wiederholter Abschluss idempotent.",
                "plan-repair"=>"Pausierte Mehrprojekt-Zugangsreparatur: nur Wege, Treppen und Plattformen, maximal 32 Teile/acht Höhen. parentActionId optional; nach Reload weglassen, kein Ledger wird rekonstruiert. Angegebener Elternauftrag muss terminal sein. Alle aktuellen Baustellen frisch erfassen (maximal 32, ein Distrikt), bekannte Unerreichbarkeit zulässig, unbekannte Identität/Zugänge nicht. Alle noch verbundenen Zugänge erhalten; keine Originalaufträge wiederholen.",
                "run-repair"=>"Frischen Mehrprojekt-Reparaturplan einmal mit neuer actionId, planKey und Budget starten; optionales parentActionId wie in der Vorschau. Nach fertigen Hilfen müssen alle erfassten Baustellen builder-erreichbar bzw. fertig zugänglich sein; kein Warten auf deren Fertigbau. Wiedergewonnene Zugänge bleiben geschützt. Alte Projekte bleiben terminal, kein Wiederholen/Wiederaufnehmen. Bau- und Tempo-Schalter erforderlich.",
                "run"=>"0.63: Ein Go platziert die gesamte Struktur in einem Main-Thread-Durchgang. Keine Material-, Zugangs-, Bestätigungs- oder Fertigstellungsprüfung zwischen Teilen. dependsOn ordnet nur die Platzierung. Danach automatische Beobachtung aller Teile bis Fertigbau oder festes Zeitbudget; Teilfehler bleiben sichtbar, kein Replay. Start pausiert mit frischem planKey und mode=development_pilot. speed 1/3/7, maxGameHours/maxRealSeconds feste Obergrenzen. inspect liest; completed bedeutet alle Teile fertig und Zugang beobachtet sowie Pause bestätigt. Native Ablehnungen und unbestätigte Wirkungen sind keine Fertigstellung. Kraft und Betrieb separat prüfen.",
                "plan"=>"0.37: Freier expliziter 3D-Bauplan, maximal 32 Bauteile, 32x32 und acht belegte Höhen. Pro Objekt bis 512 native Blockzellen, Single/Hex eingeschlossen; installiertes maxGeometryCells im Katalog pruefen. steps enthalten Vorlagen, Koordinaten, Drehungen und nullbasierte dependsOn-Indizes; zyklische Abhängigkeiten abgelehnt. Native gemeinsame Vorschau in topologischer Reihenfolge mit Stützen, Wegen, Eingang und Bestandsschutz; kein Bauauftrag. constructionWorkAccess muss bei required=true road_reachable sein; kein hypothetischer Bauarbeiternachweis. Spätere Blockaden werden mitgeprüft. Kraftports und geometrisch passende Verbindungen sind keine Netz-/Leistungsgarantie. Nur pausiert. Räumlich getrennte Baustellen sind bei vollständiger Identität, einem Feld XY-Abstand einschließlich Arbeitszugängen und nativ erhaltener Zugänglichkeit zulässig; unbekannte Nachweise blockieren. Mehrere start_large_project-Aufträge können gleichzeitig gebaut werden; ein gemeinsamer Simulationslauf. Prioritäten über inspect/set_large_project_priority. pilotEligible ist keine reguläre Ausführungsfreigabe. placementState=deferred_native_validation bleibt ungeprüft bis direkt abhängige stapelbare Träger fertig sind; als ungeprüfte native Einschränkung ausweisen; nach gemeinsamem Platzierungsversuch tatsächliche Wirkung lesen. Fehlende Wege/Stützen selbst als Bauteile angeben; keine automatische Geländeroutensuche.",
                "start"=>"0.63: Platziert den gesamten frisch geplanten 3D-Auftrag sofort in einem Durchgang und beobachtet danach automatisch. Kein advance pro Teil. Ohne eigenes Simulationszeitfenster; für selbstständig laufende Bauzeit run_large_project verwenden. Gleiche actionId liest nur vorhandenen Stand. Keine Wiederholung unbestätigter Aufträge.",
                "advance"=>"0.63: Kompatibilitätsaufruf zur Beobachtung eines gestarteten Projekts. start hat bereits alle Bauaufträge in einem Durchgang erteilt; advance erteilt keine Folgeaufträge. Die Mod beobachtet auch ohne advance weiter. Simulation separat oder run_large_project verwenden.",
                "inspect"=>"3D-Projektstand lesen, niemals Aufträge auslösen. executionMode=batch wird von der Mod automatisch aktualisiert. Compact: summary trennt attempted, confirmedPlaced und finishedAccessRecorded; attentionParts enthält offene/blockierte IDs mit Originalindex. Full: sämtliche entityIds/partStates. blockedPart, reason und begrenzte simulation bleiben sichtbar. Manueller Altmodus bleibt Ledger-Lesung. Für frische Blockadediagnose diagnose_large_project; released zeigt Ledgerabschluss ohne Wiederaufnahme. completed beweist keine Produktion. Nach MCP-Neuverbindung gleiche actionId lesen; nach Spielneuladen ist das Ledger verloren.",
                "stop"=>"Weitere Aufträge dauerhaft stoppen, beauftragte Objekte bleiben. Autonome Charge fordert zusätzlich eine Pause an: pausing bis Bestätigung, bei manuellem Tempoeingriff kein Überschreiben. Manueller Altmodus ändert die Simulation nicht. Terminale Projekte werden nie wiederaufgenommen. Danach diagnose_large_project und bei geklärtem Zustand close_large_project; Bauzugang separat mit Reparaturcharge ergänzen.",
                _=>"0.37: Native Kraftanschlüsse eines Gebäudes mit Rasterposition, Ziel und Richtung, echten Verbindungen, Netzmitgliedern (max512), aktueller Erzeugung/Nachfrage und Batterie. Kein grafisches Raten, keine Zustandsänderung. supported=false oder network=null bleibt unbekannt/ohne Netz; Leistung ist Momentaufnahme, kein Produktionsnachweis."};
            yield return new Tool{Name=pair.Key,Description=description + (pair.Value is "start" or "run" or "run-repair" ? " MCP führt vorher eine automatische Betriebsdiagnose mit vollen Schrittkosten einschließlich Hilfsbauten aus; operatingPreflight im Ergebnis, bei Bretterkosten zusätzlich plankChain mit Engpässen und nächsten Schritten im selben Vorprüfungsbudget. Keine Material-Startsperre, kein Ausbau und keine Budgetverlängerung." : ""),InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=props,["required"]=new JsonArray(props.Where(p=>p.Key!="parentActionId").Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()),["additionalProperties"]=false}),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<JsonElement>))),Annotations=new(){ReadOnlyHint=!write,DestructiveHint=write,IdempotentHint=!write,OpenWorldHint=false}};
        }
    }
    public static LargeProjectRequest Parse(string name,JsonElement args)
    {
        if(!Operations.TryGetValue(name,out var op))throw new ArgumentException();
        var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject())
        {
            if(q.GetValues(p.Name)is not null)throw new ArgumentException("duplicate_argument");
            if(p.Name=="steps")
            {
                if(p.Value.ValueKind!=JsonValueKind.Array||p.Value.GetArrayLength()is <1 or >32)throw new ArgumentException("invalid_steps");
                var steps=p.Value.EnumerateArray().Select(v=>{
                    var keys=v.EnumerateObject().Select(a=>a.Name).ToArray();
                    if(keys.Length!=6||keys.Distinct().Count()!=6||keys.Any(k=>k is not ("template" or "x" or "y" or "z" or "rotation" or "dependsOn")))throw new ArgumentException("invalid_step_fields");
                    return new LargeProjectStep{Template=v.GetProperty("template").GetString()??"",X=v.GetProperty("x").GetInt32(),Y=v.GetProperty("y").GetInt32(),Z=v.GetProperty("z").GetInt32(),Rotation=v.GetProperty("rotation").GetInt32(),DependsOn=v.GetProperty("dependsOn").EnumerateArray().Select(d=>d.GetInt32()).ToArray()};
                }).ToArray();q.Add("steps",LargeProjectRequest.Encode(steps));
            }
            else if(p.Name is "partIndex" or "speed" or "maxGameHours" or "maxRealSeconds") { if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt32(out int n))throw new ArgumentException();q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture)); }
            else {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
        }
        return LargeProjectRequest.Parse("/agent-api/v1/"+(op=="power-network"?op:"large-project-"+op),q);
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool enabled,CancellationToken ct,bool speedEnabled = false)
    {
        if(Writes(name)&&!enabled)throw new ArgumentException("building_actions_disabled");
        if(name is ("run_large_project" or "run_large_project_repair")&&!speedEnabled)throw new ArgumentException("speed_control_disabled");
        LargeProjectRequest r;
        try {r=Parse(name,args);}catch(Exception ex)when(ex is KeyNotFoundException or InvalidOperationException or FormatException or OverflowException){throw new ArgumentException("invalid_large_project_argument",ex);}
        var e=await client.LargeProject(r,ct);
        if(e.Data.TryGetProperty("rejectionCode",out var rejection))
            return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<JsonElement>(1,"error",e.Data,
                new("native",false,e.SessionId,e.ObservedAtUtc),
                new(rejection.GetString()!,"Native Reparatursperre bei ausdrücklich injiziertem Identitätsbefund; kein Auftrag ausgeführt.",false,"not_executed")),NativeJson.Options)!;
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<JsonElement>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
    }
}
