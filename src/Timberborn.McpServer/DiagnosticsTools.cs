using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class DiagnosticsTools
{
    private static readonly Dictionary<string,string> Routes=new(){["inspect_needs"]="needs",["inspect_beaver_needs"]="beaver-needs",["inspect_building_operation"]="building-operation"};
    public static bool Handles(string name)=>Routes.ContainsKey(name);
    public static IEnumerable<Tool> Catalog()
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var (name,route) in Routes){
            var p=new JsonObject();
            if(route!="needs")foreach(var key in new[]{"id","session"})p[key]=new JsonObject{["type"]="string",["format"]="uuid"};
            if(route!="building-operation"){
                p["offset"]=new JsonObject{["type"]="integer",["minimum"]=0,["maximum"]=65535};
                p["limit"]=new JsonObject{["type"]="integer",["minimum"]=1,["maximum"]=32};
            }
            yield return new Tool{Name=name,Description=(route switch{
                "needs"=>"Liest pro Bedürfnis die beobachteten Biber, aktivierten Bedürfnisse sowie native Warn-/Kritisch-Flags und Punkteverteilung. Zähler enthalten nur aktivierte Bedürfnisse und können sich überschneiden. Fehlende NeedManager werden separat gezählt. Keine Namen, keine aus Meldungsabwesenheit geratene Gesundheit. Maximal 32 Bedürfnisse pro Seite, nach ID sortiert.",
                "beaver-needs"=>"Liest native Bedürfniswerte und Warn-/Kritisch-Flags eines Bibers mit Session-Prüfung; ID aus Arbeiterliste oder Warnungszielen. Auch deaktivierte Bedürfnisse mit enabled=false. Maximal 32 Bedürfnisse pro Seite. worldPosition verwendet Unity-Weltachsen. supported=false bedeutet fehlende Beobachtung, nicht gesund.",
                _=>"Liest Betriebsbelege eines Gebäudes: Pause, zugewiesene/Soll-Arbeiter, Arbeitszeit, laufende Jobs; bei nativer Manufactory Rezept, Zutaten, Brennstoff und Produktplatz. Fehlende Komponenten bleiben null. Kein laufender Job ist kein Beweis einer Blockade (Nacht, Wege). ready garantiert keine Produktion. Aktive Statustexte sind untrusted Spieldaten. Keine vollständige Energie-/Wasser-/Lieferdiagnose."})+" Ab Bridge 0.20.0. Rein lesend; getrennte Aufrufe sind keine atomare Gesamtsicht.",
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=p,["required"]=new JsonArray(p.Select(x=>(JsonNode?)JsonValue.Create(x.Key)).ToArray()),["additionalProperties"]=false}),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(route switch{"needs"=>typeof(NativeResult<NativeNeeds>),"beaver-needs"=>typeof(NativeResult<NativeBeaverNeeds>),_=>typeof(NativeResult<NativeOperation>)})),Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false}};
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,CancellationToken ct)
    {
        if(!Routes.TryGetValue(name,out var route))throw new ArgumentException();var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()){
            if(p.Name is "offset" or "limit"){if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt32(out int n))throw new ArgumentException();q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture));}
            else {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
        }
        var r=DiagnosticsRequest.Parse("/agent-api/v1/"+route,q);
        JsonObject Wrap<T>(BridgeEnvelope<T> e)=>(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        return route switch{"needs"=>Wrap(await client.Needs(r,ct)),"beaver-needs"=>Wrap(await client.BeaverNeeds(r,ct)),_=>Wrap(await client.BuildingOperation(r,ct))};
    }
}
