using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class LogisticsTools
{
    private static readonly Dictionary<string,string> Routes=new(){["inspect_building_access"]="building-access",["inspect_road_connection"]="road-connection",["inspect_work_range"]="work-range",["inspect_good_history"]="good-history"};
    public static bool Handles(string name)=>Routes.ContainsKey(name);
    public static IEnumerable<Tool> Catalog()
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var (name,route) in Routes){
            var p=new JsonObject();
            if(route=="good-history")p["good"]=new JsonObject{["type"]="string",["pattern"]="^[A-Za-z0-9._-]+$",["minLength"]=1,["maxLength"]=160};
            else foreach(var key in route=="road-connection"?new[]{"id","toId","session"}:new[]{"id","session"})p[key]=new JsonObject{["type"]="string",["format"]="uuid"};
            if(route is "good-history" or "work-range"){
                p["offset"]=new JsonObject{["type"]="integer",["minimum"]=0,["maximum"]=65535};
                p["limit"]=new JsonObject{["type"]="integer",["minimum"]=1,["maximum"]=32};
            }
            yield return new Tool{Name=name,Description=(route switch{
                "building-access"=>"Liest native Eingangsblockade, Unzugänglichkeit, Anschlussblockade, Baudienst-Erreichbarkeit und Distriktdistanz eines Blockobjekts. null bedeutet fehlende Komponente/Beobachtung, niemals false. Kein Raten anhand benachbarter Wege.",
                "road-connection"=>"Prüft eine echte reguläre Spiel-Wegverbindung vom Gebäude id nach toId. Nur fertige Objekte mit jeweils genau einem gültigen Accessible werden unterstützt. supported=false ist unbekannt, nicht unerreichbar. connected=false ist eine abgelehnte aktuelle Wegsuche. Distanz ist Spielmetrik, keine Reisezeit. Kein Terrain-Abkürzungsweg, keine Bauvorschau.",
                "work-range"=>"Liest Rasterzellen aus öffentlichen Reichweitenprovidern eines fertigen Gebäudes, als sortierte Vereinigungsmenge. Maximal 32 pro Seite, Z/Y/X-Sortierung. supported=false bedeutet keine belegte Reichweite; leere Liste nicht als Reichweite null auslegen. Reichweite beweist weder passenden Rohstoff noch aktuelle Arbeitszuweisung. Alle Seiten sind getrennte Beobachtungen.",
                _=>"Liest gespeicherte globale Güterstatistik des Spiels in dessen Reihenfolge: Cycle, Day, Stock, Capacity, Production und Consumption. Produktions-/Verbrauchszähler stammen aus dem Spiel, nicht aus Vorratsdifferenzen. netProduction=Production-Consumption; stockChange separat. Summen gelten nur für die gelesene Seite. Keine angenommene Abtastdauer oder momentane Rate; leere Historie bedeutet fehlende Daten. Maximal 32 Einträge pro Seite."})+" Ab Bridge 0.19.0. Rein lesend; Navigationsdaten können nach Spieländerungen verzögert aktualisiert werden.",
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=p,["required"]=new JsonArray(p.Select(x=>(JsonNode?)JsonValue.Create(x.Key)).ToArray()),["additionalProperties"]=false}),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(route switch{"building-access"=>typeof(NativeResult<NativeAccess>),"road-connection"=>typeof(NativeResult<NativeRoad>),"work-range"=>typeof(NativeResult<NativeRange>),_=>typeof(NativeResult<NativeGoodHistory>)})),Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false}};
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,CancellationToken ct)
    {
        if(!Routes.TryGetValue(name,out var route))throw new ArgumentException();var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()){
            if(p.Name is "offset" or "limit"){if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt32(out int n))throw new ArgumentException();q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture));}
            else {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
        }
        var r=LogisticsRequest.Parse("/agent-api/v1/"+route,q);
        JsonObject Wrap<T>(BridgeEnvelope<T> e)=>(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        return route switch{"building-access"=>Wrap(await client.BuildingAccess(r,ct)),"road-connection"=>Wrap(await client.RoadConnection(r,ct)),"work-range"=>Wrap(await client.WorkRange(r,ct)),_=>Wrap(await client.GoodHistory(r,ct))};
    }
}
