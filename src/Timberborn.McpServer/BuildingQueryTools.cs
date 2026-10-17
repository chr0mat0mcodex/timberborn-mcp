using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public sealed record OperationSummary(string Template,bool Finished,bool? Paused,OperationWorkplace? Workplace,
    OperationManufacturing? Manufacturing,OperationPower? Power,OperationAssessment? Assessment, BottleneckEvidence? Bottleneck=null, OperationWater? Water=null);
public sealed record OperationViewItem(string Id,string State,OperationSummary? Summary,NativeOperation? Details);
public sealed record OperationView(OperationViewItem[] Items,int NativeReads,string[] Limitations);

public static class BuildingQueryTools
{
    public static bool Handles(string name)=>name is "find_buildings" or "inspect_building_operations";
    public static IEnumerable<Tool> Catalog()
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(bool batch in new[]{false,true}) {
            var p=new JsonObject { ["detail"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("compact","full"),["default"]="compact"} };
            if(batch) {
                p["diagnose"]=new JsonObject{["type"]="boolean",["default"]=false,["description"]="0.43: Engpassbelege und nächste Schritte für maximal acht IDs; begrenzte Lager-/Wegprüfung."};
                p["session"]=new JsonObject{["type"]="string",["format"]="uuid"};
                p["ids"]=new JsonObject{["type"]="array",["minItems"]=1,["maxItems"]=32,["uniqueItems"]=true,
                    ["items"]=new JsonObject{["type"]="string",["format"]="uuid"}};
            } else {
                foreach(var (key,min,max) in new[]{("offset",0,65535),("limit",1,32),("x",0,4095),("y",0,4095),("z",0,4095),("width",1,4096),("height",1,4096),("depth",1,4096)})
                    p[key]=new JsonObject{["type"]="integer",["minimum"]=min,["maximum"]=max};
                p["template"]=new JsonObject{["type"]="string",["minLength"]=1,["maxLength"]=160};
                foreach(var key in new[]{"finished","includePaths","productionOnly"})p[key]=new JsonObject{["type"]="boolean"};
            }
            yield return new Tool{Name=batch?"inspect_building_operations":"find_buildings",
                Description=batch?"Ab 0.39: liest 1–32 eindeutige Gebäude-IDs in einem nativen Request und Spielthread-Durchlauf. Je ID observed oder not_found; fehlend ist keine leere Betriebsbeobachtung. Ab 0.65 enthält water native Überflutung und Ansaugbelege auch kompakt; Überflutung vor Personalmaßnahmen prüfen. Kompakte Summary enthält Rezept, Personal, Zutatenbereitschaft, Produktplatz, Kraft und Diagnose. detail=full ergänzt Inventare/Statustexte. Keine Rezeptlisten und kein Produktionsbeweis aus Bereitschaft. Aktuelle session nötig.":
                    "Ab 0.39: native Gebäudesuche, Filter vor Pagination. Optional exakte template, finished, productionOnly (Manufactory-Komponente), includePaths (standardmäßig false, PathSpec einschließlich Treppen). Optionaler Bereich benötigt x/y/z/width/height/depth gemeinsam und filtert den Objektanker, nicht den Grundriss. GUID-sortiert, maximal 32 Treffer; total/hasMore beziehen sich auf gefilterte Treffer. Standard kompakt; detail=full für geometry mit Eingang/Orientierung/bis 512 Zellen. Kein Zugangs- oder Betriebsnachweis.",
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=p,
                    ["required"]=batch?new JsonArray("ids","session"):new JsonArray("offset","limit"),["additionalProperties"]=false}),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(batch?typeof(NativeResult<OperationView>):typeof(NativeResult<BuildingSearch>))),
                Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false}};
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,CancellationToken ct)
    {
        if(!Handles(name))throw new ArgumentException();
        var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()) {
            if(p.Name=="ids") {
                if(p.Value.ValueKind!=JsonValueKind.Array||p.Value.GetArrayLength() is <1 or >32||p.Value.EnumerateArray().Any(v=>v.ValueKind!=JsonValueKind.String))throw new ArgumentException();
                if(p.Value.EnumerateArray().Any(v=>!Guid.TryParseExact(v.GetString(),"D",out var id)||id==Guid.Empty))throw new ArgumentException();
                q.Add(p.Name,string.Join(",",p.Value.EnumerateArray().Select(v=>v.GetString())));
            } else if(p.Name is "finished" or "includePaths" or "productionOnly" or "diagnose") {
                if(p.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))throw new ArgumentException();q.Add(p.Name,p.Value.GetBoolean()?"true":"false");
            } else if(p.Name is "detail" or "session" or "template") {
                if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());
            } else {
                if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt32(out int n))throw new ArgumentException();q.Add(p.Name,n.ToString(CultureInfo.InvariantCulture));
            }
        }
        bool batch=name=="inspect_building_operations";
        var r=BuildingQueryRequest.Parse("/agent-api/v1/"+(batch?"building-operations":"building-search"),q);
        JsonObject Wrap<T>(BridgeEnvelope<T> e)=>(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        if(!batch)return Wrap(await client.SearchBuildings(r,ct));
        var result=await client.BuildingOperations(r,ct);
        var view=new OperationView(result.Data.Items.Select(i=>new OperationViewItem(i.Id,i.State,
            i.Operation is {} o?new(o.Template,o.Finished,o.Paused,o.Workplace,o.Manufacturing,o.Power,o.Assessment,o.Bottleneck,o.Water):null,
            r.Full?i.Operation:null)).ToArray(),result.Data.NativeReads,result.Data.Limitations);
        return Wrap(new BridgeEnvelope<OperationView>(result.SchemaVersion,result.SessionId,result.ObservedAtUtc,result.BridgeVersion,view));
    }
}
