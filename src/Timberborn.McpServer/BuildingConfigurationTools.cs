using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class BuildingConfigurationTools
{
    public static bool Handles(string name)=>name is "configure_buildings" or "inspect_building_configuration";
    public static bool Writes(string name)=>name=="configure_buildings";
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        static JsonObject Id()=>new(){["type"]="string",["format"]="uuid"};
        static JsonObject Recipe(bool empty)=>new(){["type"]="string",["maxLength"]=160,["pattern"]=empty?"^[A-Za-z0-9._-]{0,160}$":"^[A-Za-z0-9._-]{1,160}$"};
        static JsonObject Integer(int min,int max)=>new(){["type"]="integer",["minimum"]=min,["maximum"]=max};
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(string name in new[]{"configure_buildings","inspect_building_configuration"}) {
            bool write=Writes(name);if(write&&!enabled)continue;
            var props=new JsonObject{["session"]=Id(),["jobId"]=Id()};
            var required=new JsonArray("session","jobId");
            if(write) {
                props["items"]=new JsonObject{["type"]="array",["minItems"]=1,["maxItems"]=8,["items"]=new JsonObject{
                    ["type"]="object",["additionalProperties"]=false,["properties"]=new JsonObject{
                        ["id"]=Id(),["recipe"]=Recipe(false),["expectedRecipe"]=Recipe(true),
                        ["desiredWorkers"]=Integer(1,100),["expectedDesiredWorkers"]=Integer(0,100),
                        ["paused"]=new JsonObject{["type"]="boolean"},["expectedPaused"]=new JsonObject{["type"]="boolean"}},
                    ["required"]=new JsonArray("id","recipe","expectedRecipe","desiredWorkers","expectedDesiredWorkers","paused","expectedPaused")}};
                required.Add("items");
                props["proofHours"]=new JsonObject{["type"]="number",["minimum"]=0.000001,["maximum"]=672};
                props["proofSpeed"]=new JsonObject{["type"]="integer",["enum"]=new JsonArray(1,3,7)};
                props["proofMaxRealSeconds"]=Integer(30,86400);
            }
            yield return new Tool{Name=name,Description="0.42: 1–8 fertige Manufakturen gesammelt konfigurieren (native Rezepte, Sollpersonal 1 bis Gebäudemaximum, Pause). Simulation muss pausiert und ohne aktiven Lauf/Baucharge sein. Zum Deaktivieren paused=true statt Sollpersonal 0. Alle items vollständig mit erwarteten Ausgangswerten; gesamte Vorprüfung vor erster Änderung. Teilfehler stoppen weitere Änderungen, kein Rollback/Replay. Neue jobId pro Auftrag; nach unbestätigtem Aufruf nur inspect_building_configuration lesen. Optional proofHours/proofSpeed/proofMaxRealSeconds gemeinsam: nur aktive, besetzbar konfigurierte Ziele, Nachweislauf nach bestätigter Einrichtung. runId dann gleich jobId; Produktionsstatus separat über inspect_simulation_run. applied bestätigt Einstellungen, nicht Produktion. proofState=unconfirmed nur lesend klären. 128 Aufträge pro Sitzung, Historie bleibt eingefroren.",
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=props,["required"]=required,["additionalProperties"]=false}),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<BuildingConfigurationResult>))),
                Annotations=new(){ReadOnlyHint=!write,DestructiveHint=write,IdempotentHint=!write,OpenWorldHint=false}};
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool enabled,CancellationToken ct)
    {
        if(!Handles(name)||Writes(name)&&!enabled)throw new ArgumentException();
        var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()) {
            if(p.Name=="items") {
                if(p.Value.ValueKind!=JsonValueKind.Array||p.Value.GetArrayLength() is <1 or >8)throw new ArgumentException();
                var rows=p.Value.EnumerateArray().Select(i=>{
                    var keys=new[]{"id","recipe","expectedRecipe","desiredWorkers","expectedDesiredWorkers","paused","expectedPaused"};
                    if(i.ValueKind!=JsonValueKind.Object||!i.EnumerateObject().Select(x=>x.Name).Order().SequenceEqual(keys.Order()))throw new ArgumentException();
                    string Text(string key)=>i.GetProperty(key).ValueKind==JsonValueKind.String?i.GetProperty(key).GetString()!:throw new ArgumentException();
                    string Number(string key)=>i.GetProperty(key).ValueKind==JsonValueKind.Number&&i.GetProperty(key).TryGetInt32(out int n)?n.ToString(System.Globalization.CultureInfo.InvariantCulture):throw new ArgumentException();
                    string Bool(string key)=>i.GetProperty(key).ValueKind==JsonValueKind.True?"true":i.GetProperty(key).ValueKind==JsonValueKind.False?"false":throw new ArgumentException();
                    string id=Text("id"),recipe=Text("recipe"),expected=Text("expectedRecipe");
                    if(!Guid.TryParseExact(id,"D",out _)||!BuildingPolicy.ValidTemplate(recipe)||expected!=""&&!BuildingPolicy.ValidTemplate(expected))throw new ArgumentException();
                    return string.Join("|",id,recipe,expected,Number("desiredWorkers"),Number("expectedDesiredWorkers"),Bool("paused"),Bool("expectedPaused"));
                });q.Add("items",string.Join(";",rows));
            } else if(p.Name is "session" or "jobId") {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
            else if(p.Value.ValueKind==JsonValueKind.Number&&p.Value.TryGetDecimal(out decimal n))q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture));
            else throw new ArgumentException();
        }
        var r=BuildingConfigurationRequest.Parse("/agent-api/v1/"+(Writes(name)?"configure-buildings":"building-configuration"),q);
        var e=await client.ConfigureBuildings(r,q,ct);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<BuildingConfigurationResult>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
    }
}
