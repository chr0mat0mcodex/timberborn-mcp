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
    private static readonly Dictionary<string,string> Operations=new(){["plan_large_project"]="plan",["start_large_project"]="start",["advance_large_project"]="advance",["inspect_large_project"]="inspect",["stop_large_project"]="stop",["inspect_power_network"]="power-network"};
    public static bool Handles(string name)=>Operations.ContainsKey(name);
    public static bool Writes(string name)=>Operations.TryGetValue(name,out var operation)&&LargeProjectRequest.Writes(operation);
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        JsonObject Id()=>new(){["type"]="string",["format"]="uuid"};
        JsonObject Number(int max)=>new(){["type"]="integer",["minimum"]=0,["maximum"]=max};
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var pair in Operations)
        {
            bool write=Writes(pair.Key);if(write&&!enabled)continue;
            var props=new JsonObject{["session"]=Id()};
            if(pair.Value=="power-network")props["id"]=Id();
            else if(pair.Value is "plan" or "start")
            {
                props["districtId"]=Id();
                props["steps"]=new JsonObject{["type"]="array",["minItems"]=1,["maxItems"]=32,["items"]=new JsonObject{
                    ["type"]="object",["additionalProperties"]=false,["properties"]=new JsonObject{
                        ["template"]=new JsonObject{["type"]="string",["minLength"]=1,["maxLength"]=160},["x"]=Number(4095),["y"]=Number(4095),["z"]=Number(4095),["rotation"]=Number(3),
                        ["dependsOn"]=new JsonObject{["type"]="array",["maxItems"]=31,["uniqueItems"]=true,["items"]=Number(31)}},
                    ["required"]=new JsonArray("template","x","y","z","rotation","dependsOn")}};
                if(pair.Value=="start") {props["actionId"]=Id();props["planKey"]=new JsonObject{["type"]="string",["pattern"]="^[0-9a-f]{64}$"};props["mode"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("development_pilot")};}
            }
            else props["actionId"]=Id();
            string description=pair.Value switch{
                "plan"=>"0.37: Freier expliziter 3D-Bauplan, maximal 32 Bauteile, 32x32 und acht belegte Höhen. steps enthalten Vorlagen, Koordinaten, Drehungen und nullbasierte dependsOn-Indizes; zyklische Abhängigkeiten abgelehnt. Native gemeinsame Vorschau in topologischer Reihenfolge mit Stützen, Wegen, Eingang und Bestandsschutz; kein Bauauftrag. Kraftports und geometrisch passende Verbindungen sind keine Netz-/Leistungsgarantie. Nur pausiert, keine unabhängigen Baustellen. pilotEligible ist keine reguläre Ausführungsfreigabe. placementState=deferred_native_validation bleibt ungeprüft bis direkt abhängige stapelbare Träger fertig sind; vor dem Auftrag zwingend erneute vollständige native Prüfung. Fehlende Wege/Stützen selbst als Bauteile angeben; keine automatische Geländeroutensuche.",
                "start"=>"0.37: Registriert den frisch nachgeprüften 3D-Plan als Entwicklungspilot. Gleiche steps/districtId/planKey, frische Session und neue actionId. Noch kein Bauauftrag; advance setzt fort. Gleiche ID und identischer Plan liefern vorhandenen Stand, Änderungen abgelehnt. Maximal 128 Projekte/Sitzung; ein offenes oder fehlgeschlagenes Projekt blockiert weitere. Keine Umgehung von unbekannten regulären Baufreigaben.",
                "advance"=>"0.37: Nur pausiert, höchstens EIN neuer Bauauftrag. Liest aktuellen Bauzugang/Fertigstatus, prüft alle Vorgänger und Bestandszugänge, validiert nächsten Schritt nativ. waiting braucht getrennten begrenzten Simulationslauf; vor erneutem advance Pause nachweisen. Keine Hintergrundaufträge, kein Zeitbudgetwachstum. completed bedeutet alle Bauteile fertig plus anwendbare Zugänge beobachtet, nicht Kraftversorgung/Produktion. stopped/unconfirmed nur diagnostizieren, keine Wiederholung.",
                "inspect"=>"0.37: Gespeicherten 3D-Projektstand lesen; keine frische Fertigprüfung und kein Auftrag. Für aktuelle Objekte inspect_building/inspect_power_network. Nach Spielneuladen ist das sitzungsgebundene Ledger verloren.",
                "stop"=>"0.37: Weitere Aufträge des 3D-Projekts dauerhaft stoppen. Bereits beauftragte Objekte bleiben; kein Abriss und keine Änderung der Simulation. Unbestätigte Wirkungen separat lesen.",
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
            else {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
        }
        return LargeProjectRequest.Parse("/agent-api/v1/"+(op=="power-network"?op:"large-project-"+op),q);
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool enabled,CancellationToken ct)
    {
        if(Writes(name)&&!enabled)throw new ArgumentException("building_actions_disabled");
        LargeProjectRequest r;
        try {r=Parse(name,args);}catch(Exception ex)when(ex is KeyNotFoundException or InvalidOperationException or FormatException or OverflowException){throw new ArgumentException("invalid_large_project_argument",ex);}
        var e=await client.LargeProject(r,ct);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<JsonElement>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
    }
}
