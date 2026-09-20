using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class ManagementTools
{
    private static readonly Dictionary<string,string> Routes=new(){["inspect_building_priority"]="priority",["set_building_priority"]="set-priority",["inspect_construction"]="construction",["inspect_area_types"]="area-types",["inspect_areas"]="areas",["set_area"]="set-area"};
    public static bool Handles(string name)=>Routes.ContainsKey(name);
    public static bool Writes(string name)=>name is "set_building_priority" or "set_area";
    public static IEnumerable<Tool> Catalog(bool priorities,bool areas)
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var (name,route) in Routes){
            if(name=="set_building_priority"&&!priorities||name=="set_area"&&!areas)continue;
            var props=new JsonObject();
            void Str(string key,params string[] values){var p=new JsonObject{["type"]="string"};if(values.Length>0)p["enum"]=new JsonArray(values.Select(v=>(JsonNode?)JsonValue.Create(v)).ToArray());props[key]=p;}
            void Num(string key,int min,int max)=>props[key]=new JsonObject{["type"]="integer",["minimum"]=min,["maximum"]=max};
            if(route is "priority" or "set-priority"){Str("id");Str("session");Str("kind","workplace","construction");}
            if(route=="set-priority"){Str("priority","VeryLow","Low","Normal","High","VeryHigh");Str("expectedPriority","VeryLow","Low","Normal","High","VeryHigh");}
            if(route is "construction" or "areas"){Num("offset",0,65535);Num("limit",1,32);}
            if(route=="areas")Str("kind","tree_cutting","crops","tree_planting","tapping");
            if(route=="set-area"){
                Str("kind","tree_cutting","crops","tree_planting","tapping");Str("operation","mark","remove");Str("resource");Str("expectedResource");Str("session");
                foreach(var key in new[]{"x","y","z"})Num(key,0,4095);Num("width",1,4);Num("height",1,4);
            }
            Type output=route switch{"priority" or "set-priority"=>typeof(NativeResult<NativePriority>),"construction"=>typeof(NativeResult<NativeConstructionList>),"area-types"=>typeof(NativeResult<NativeAreaTypes>),"areas"=>typeof(NativeResult<NativeAreas>),_=>typeof(NativeResult<NativeAreaChange>)};
            string description=route switch{
                "priority"=>"Liest separate Arbeitsplatz- oder Baupriorität einer Entity, gebunden an aktuelle Session. Kein Arbeiter-/Liefernachweis.",
                "set-priority"=>"Setzt workplace-Priorität eines fertigen Arbeitsplatzes oder construction-Priorität einer offenen Baustelle. Session und erwartete bisherige Priorität erforderlich. Separates Opt-in. Danach separat lesen; unconfirmed/Fehler niemals automatisch wiederholen.",
                "construction"=>"Übersicht ALLER offenen Baustellen, seitenweise maximal 32 mit Gebäude-ID, Position, Material/Baufortschritt und Baupriorität. Keine fertiggestellten Gebäude. Kosten und Baustellenbestand sind kein berechneter Restbedarf. Seiten nicht atomar.",
                "area-types"=>"Liest unterstützte Flächenarten und Pflanzvorlagen mit regulärem Werkzeug-Lockstatus. Baumfällen und Pflanzmarkierungen; tapping ist abgeleiteter Kiefernschutz durch Entfernen von Fällmarkierungen. Kein Unlock.",
                "areas"=>"Liest markierte Zellen kolonieweit, gefiltert nach Flächenart, maximal 32 pro Seite. crops/tree_planting sind Pflanzaufträge, keine frei definierten Erntezonen. tapping listet alle aktuell nicht zum Fällen markierten Kiefern, auch natürlich unmarkierte; keine gespeicherte Zone, kein Ernte-/Reichweitennachweis. Jede Seite frisch.",
                _=>"Markiert/entfernt Markierungen in maximal 4x4 Zellen auf einer Z-Ebene. Vorher inspect_area_types/inspect_areas lesen. Alle Zellen müssen expectedResource entsprechen: unmarked, für Baumfällen marked, sonst Vorlagenname. resource bei Baumfällen/Entfernen leer, sonst freigeschaltete Pflanzvorlage. Geometrie und Pflanzenvalidierung vor Mutation. Entfernt nur Markierung, niemals Pflanzen. Teiländerung möglich; kein Retry/Rollback; Ergebnis nachlesen. tapping erlaubt nur mark: entfernt Fällmarkierungen im Rechteck, resource leer und expectedResource marked/unmarked. Erneutes Fällen nur ausdrücklich über tree_cutting. Kein eigener Flächenspeicher."};
            yield return new Tool{Name=name,Description=description,InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=props,["required"]=new JsonArray(props.Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()),["additionalProperties"]=false}),OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(output)),Annotations=new(){ReadOnlyHint=!Writes(name),DestructiveHint=Writes(name),IdempotentHint=!Writes(name),OpenWorldHint=false}};
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool priorities,bool areas,CancellationToken ct)
    {
        if(name=="set_building_priority"&&!priorities||name=="set_area"&&!areas)throw new ArgumentException();
        var query=new NameValueCollection();
        foreach(var p in args.EnumerateObject()){
            bool numeric=p.Name is "offset" or "limit" or "x" or "y" or "z" or "width" or "height";
            if(numeric&&p.Value.ValueKind==JsonValueKind.Number&&p.Value.TryGetInt32(out var n))query.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture));
            else if(!numeric&&p.Value.ValueKind==JsonValueKind.String)query.Add(p.Name,p.Value.GetString());else throw new ArgumentException();
        }
        var r=ManagementRequest.Parse("/agent-api/v1/"+Routes[name],query);
        JsonObject Wrap<T>(BridgeEnvelope<T> e)=>(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        return r.Route switch{"priority" or "set-priority"=>Wrap(await client.Priority(r,ct)),"construction"=>Wrap(await client.Construction(r,ct)),"area-types"=>Wrap(await client.AreaTypes(ct)),"areas"=>Wrap(await client.Areas(r,ct)),_=>Wrap(await client.SetArea(r,ct))};
    }
}
