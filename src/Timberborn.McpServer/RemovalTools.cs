using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;
public static class RemovalTools
{
    private static readonly Dictionary<string,string> Kinds=new(){["demolish_building"]="buildings",["remove_planted"]="planted",["remove_vegetation"]="vegetation",["remove_debris"]="debris"};
    public static bool Writes(string name)=>Kinds.ContainsKey(name);
    public static bool Handles(string name)=>name=="inspect_removal_targets"||Writes(name);
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var name in new[]{"inspect_removal_targets"}.Concat(enabled?Kinds.Keys:Enumerable.Empty<string>())) {
            bool write=Writes(name);var props=new JsonObject();
            void Str(string key,params string[] values){var p=new JsonObject{["type"]="string"};if(values.Length>0)p["enum"]=new JsonArray(values.Select(v=>(JsonNode?)JsonValue.Create(v)).ToArray());props[key]=p;}
            void Num(string key,int min,int max)=>props[key]=new JsonObject{["type"]="integer",["minimum"]=min,["maximum"]=max};
            foreach(var k in new[]{"x","y","z"})Num(k,0,4095);
            if(!write){Str("kind","all","buildings","planted","vegetation","debris");Num("width",1,8);Num("height",1,8);Num("depth",1,4);Num("offset",0,65535);Num("limit",1,32);}
            else {Str("id");Str("session");Str("template");Str("operation",Kinds[name] is "buildings" or "debris"?["delete"]:["mark","unmark"]);props["expectedMarked"]=new JsonObject{["type"]="boolean"};}
            string description=!write?"Liest alle Blockobjekte, die einen Bereich bis 8x8x4 überlappen, samt ID, Kategorie, Position, Abrissmodus und CanDelete. Alle Seiten lesen. Keine vollständige Bauvalidierung; precheck_build_site nutzen. planted bedeutet passende AKTUELLE Pflanzmarkierung, historische Herkunft unbekannt. other enthält nicht unterstützte Objekte (z.B. Ruinen).":
                Kinds[name] is "buildings" or "debris"?"Entfernt genau ein zuvor gelesenes Ziel irreversibel über regulären Lebenszyklus nach CanDelete-Prüfung. Gebäude inklusive Wege bzw. Schuttstapel streng getrennt. Güterverlust möglich. Exakte ID, Session, Vorlage, Position und expectedMarked=false nötig. Keine Stapel-/Gelände-/Kaskadenlöschung; kein Retry. Danach nachlesen.":
                "Markiert genau eine Pflanze für reguläre Entfernung durch Biber oder nimmt den Auftrag zurück. remove_planted nur auf passender aktueller Pflanzmarkierung; remove_vegetation übriger Bewuchs. Historische Herkunft unbekannt. ID, Session, Vorlage, Position und expectedMarked erforderlich. Pflanzmarkierung bleibt erhalten (Nachwuchs möglich); separat mit set_area entfernen. Kein erzwungenes Sofortlöschen und kein Retry. Der reguläre Mark-Aufruf kann geeignete Ressourcen sofort entfernen: removed=true bedeutet abgeschlossen, sonst ist nur die Auftragsannahme bestätigt.";
            yield return new Tool{Name=name,Description=description,InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=props,["required"]=new JsonArray(props.Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()),["additionalProperties"]=false}),OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(write?typeof(NativeResult<NativeRemoval>):typeof(NativeResult<NativeRemovalTargets>))),Annotations=new(){ReadOnlyHint=!write,DestructiveHint=write,IdempotentHint=!write,OpenWorldHint=false}};
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool enabled,CancellationToken ct)
    {
        bool write=Writes(name);if(write&&!enabled)throw new ArgumentException();var q=new NameValueCollection();
        if(write)q.Add("kind",Kinds[name]);
        foreach(var p in args.EnumerateObject()) {
            if(write&&p.Name=="kind")throw new ArgumentException();
            if(p.Name is "x" or "y" or "z" or "width" or "height" or "depth" or "offset" or "limit") {if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt32(out int n))throw new ArgumentException();q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture));}
            else if(p.Name=="expectedMarked") {if(p.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))throw new ArgumentException();q.Add(p.Name,p.Value.GetBoolean()?"true":"false");}
            else {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
        }
        var r=RemovalRequest.Parse("/agent-api/v1/"+(write?"remove-object":"removal-targets"),q);
        JsonObject Wrap<T>(BridgeEnvelope<T> e)=>(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        return write?Wrap(await client.RemoveObject(r,ct)):Wrap(await client.RemovalTargets(r,ct));
    }
}
