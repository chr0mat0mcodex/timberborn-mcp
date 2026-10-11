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
    private static readonly Dictionary<string,string> Operations=new(){["plan_large_project"]="plan",["start_large_project"]="start",["run_large_project"]="run",["advance_large_project"]="advance",["inspect_large_project"]="inspect",["stop_large_project"]="stop",["inspect_power_network"]="power-network",["diagnose_large_project"]="diagnose",["close_large_project"]="close",["plan_large_project_repair"]="plan-repair",["run_large_project_repair"]="run-repair"};
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
            else if(pair.Value is "plan" or "start" or "run" or "plan-repair" or "run-repair")
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
            if(pair.Value is "plan-repair" or "run-repair") props["parentActionId"]=Id();
            string description=pair.Value switch{
                "diagnose"=>"Frischer lesender Projektabgleich: erwartete Objekte, Bauzugang, Materialbestand, verlorene Bestandsanschlüsse und fremde Baustellen. Auch terminale Projekte; keine Platzierung, kein Fortschalten. closeEligible/repairEligible gelten nur für diese Beobachtung. Kosten sind kein Restbedarf.",
                "close"=>"Terminales Projekt bei Pause frisch abgleichen und Ledger freigeben, ohne Objekte zu bauen oder abzureißen. Alle versuchten Teile müssen fertig und zugänglich oder nach bestätigter Beobachtung entfernt sein. Fehlende unbestätigte Aufträge blockieren. Unbeauftragte Teile werden aufgegeben; alte actionId bleibt terminal. Wiederholter Abschluss idempotent.",
                "plan-repair"=>"Pausierte Reparaturvorschau für ein terminales parentActionId: ausschließlich zusätzliche Wege, Treppen und Plattformen, maximal 32 Teile/acht Höhen. Bekannte Baustellen dieses Projekts dürfen bestehen, fremde Baustellen nicht. Ursprüngliche und heutige Bestandszugänge prüfen. Originalaufträge werden nie wiederholt. Neue Schritte lokal mit dependsOn verknüpfen; vorhandene Träger als Bestand nutzen.",
                "run-repair"=>"Frischen Reparaturplan einmal als eigene begrenzte Charge starten; parentActionId und neue actionId, planKey und Budget angeben. Bau- und Tempo-Schalter erforderlich. Bauten des Ursprungsprojekts bleiben erhalten. Charge wartet nach Bauhilfen auch auf Fertigstellung und Zugang der bereits beauftragten Originalteile. Unbeauftragte Originalteile werden nicht nachgeholt. Danach Originalprojekt ausdrücklich schließen und Rest neu planen. Keine automatische Entfernung der Bauhilfen.",
                "run"=>"Gesamten 3D-Bauplan einmal starten: alle unabhängigen Aufträge gesammelt, explizite dependsOn-Teile automatisch nach deren Fertigstellung. Mod führt die begrenzte Simulation bis alle Teile fertig und Zugänge beobachtet sind; kein advance pro Teil. Frischer planKey, pausiert starten, mode=development_pilot; Bau- und Tempo-Schalter erforderlich. speed 1/3/7, maxGameHours und maxRealSeconds sind feste Obergrenzen, keine Warteziele. inspect liest Fortschritt; completed erst mit bestätigter Pause. Blockade/Unklarheit stoppt weitere Aufträge. Gleiche actionId nur lesen, nie neu starten; kein Replay nach Verbindungsabbruch. Manueller Tempoeingriff unterbricht. Session endet beim Neuladen. Alle Wege/Stützen vorab einplanen; echte Zugangs-/Stützabhängigkeiten in dependsOn, unabhängige Teile nicht künstlich verketten.",
                "plan"=>"0.37: Freier expliziter 3D-Bauplan, maximal 32 Bauteile, 32x32 und acht belegte Höhen. steps enthalten Vorlagen, Koordinaten, Drehungen und nullbasierte dependsOn-Indizes; zyklische Abhängigkeiten abgelehnt. Native gemeinsame Vorschau in topologischer Reihenfolge mit Stützen, Wegen, Eingang und Bestandsschutz; kein Bauauftrag. Kraftports und geometrisch passende Verbindungen sind keine Netz-/Leistungsgarantie. Nur pausiert, keine unabhängigen Baustellen. pilotEligible ist keine reguläre Ausführungsfreigabe. placementState=deferred_native_validation bleibt ungeprüft bis direkt abhängige stapelbare Träger fertig sind; vor dem Auftrag zwingend erneute vollständige native Prüfung. Fehlende Wege/Stützen selbst als Bauteile angeben; keine automatische Geländeroutensuche.",
                "start"=>"0.37: Registriert den frisch nachgeprüften 3D-Plan als Entwicklungspilot. Gleiche steps/districtId/planKey, frische Session und neue actionId. Noch kein Bauauftrag; advance setzt fort. Gleiche ID und identischer Plan liefern vorhandenen Stand, Änderungen abgelehnt. Maximal 128 Projekte/Sitzung; offene oder fehlgeschlagene Projekte blockieren weitere. Freigabe über close_large_project nach frischem Abgleich; Stopp ohne Bauversuch bleibt direkt freigebend. Alte ID bleibt terminal. Keine Umgehung von unbekannten regulären Baufreigaben.",
                "advance"=>"0.37: Nur pausiert, höchstens EIN neuer Bauauftrag. Liest aktuellen Bauzugang/Fertigstatus, prüft alle Vorgänger und Bestandszugänge, validiert nächsten Schritt nativ. waiting braucht getrennten begrenzten Simulationslauf; vor erneutem advance Pause nachweisen. Keine Hintergrundaufträge, kein Zeitbudgetwachstum. completed bedeutet alle Bauteile fertig plus anwendbare Zugänge beobachtet, nicht Kraftversorgung/Produktion. stopped/unconfirmed nur diagnostizieren, keine Wiederholung.",
                "inspect"=>"3D-Projektstand lesen, niemals Aufträge auslösen. executionMode=batch wird von der Mod automatisch aktualisiert: ordered/finished, partStates, blockedPart, reason und begrenzte simulation. Manueller Altmodus bleibt Ledger-Lesung. Für frische Blockadediagnose diagnose_large_project; released zeigt Ledgerabschluss ohne Wiederaufnahme. completed beweist keine Produktion. Nach MCP-Neuverbindung gleiche actionId lesen; nach Spielneuladen ist das Ledger verloren.",
                "stop"=>"Weitere Aufträge dauerhaft stoppen, beauftragte Objekte bleiben. Autonome Charge fordert zusätzlich eine Pause an: pausing bis Bestätigung, bei manuellem Tempoeingriff kein Überschreiben. Manueller Altmodus ändert die Simulation nicht. Terminale Projekte werden nie wiederaufgenommen. Danach diagnose_large_project und bei geklärtem Zustand close_large_project; Bauzugang separat mit Reparaturcharge ergänzen.",
                _=>"0.37: Native Kraftanschlüsse eines Gebäudes mit Rasterposition, Ziel und Richtung, echten Verbindungen, Netzmitgliedern (max512), aktueller Erzeugung/Nachfrage und Batterie. Kein grafisches Raten, keine Zustandsänderung. supported=false oder network=null bleibt unbekannt/ohne Netz; Leistung ist Momentaufnahme, kein Produktionsnachweis."};
            yield return new Tool{Name=pair.Key,Description=description,InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=props,["required"]=new JsonArray(props.Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()),["additionalProperties"]=false}),
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
            else if(p.Name is "speed" or "maxGameHours" or "maxRealSeconds") { if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt32(out int n))throw new ArgumentException();q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture)); }
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
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<JsonElement>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
    }
}
