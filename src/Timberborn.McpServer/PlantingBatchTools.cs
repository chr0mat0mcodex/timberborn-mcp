using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public sealed record PlantingPreview(string Session,string JobId,PlantingRectangle[] Rectangles,string Detail="compact");
public sealed record PlantingApply(string Session,string JobId,PlantingRectangle[] Rectangles,string Key,string Detail="compact");
public sealed record PlantingStatus(string Session,string JobId,string Detail="compact");
public static class PlantingBatchTools
{
    public const string Preview="preview_planting_batch",Apply="apply_planting_batch",Status="inspect_planting_batch";
    public static bool Handles(string name)=>name is Preview or Apply or Status;
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options=new JsonSerializerOptions(BuildBatchState.Json){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var name in new[]{Preview,Apply,Status}) {
            if(name==Apply&&!enabled)continue;
            var schema=options.GetJsonSchemaAsNode(name==Preview?typeof(PlantingPreview):name==Apply?typeof(PlantingApply):typeof(PlantingStatus)).AsObject();
            // The exporter permits a null root for reference types; MCP requires an object root.
            schema["type"]="object";
            schema["properties"]!["detail"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("compact","full"),["default"]="compact"};
            if(name!=Status) {
                var rectangles=schema["properties"]!["rectangles"]!.AsObject();rectangles["minItems"]=1;rectangles["maxItems"]=8;
                var item=rectangles["items"]!.AsObject();item["required"]=new JsonArray("x","y","z","width","height","resource","workBuildingId");
                foreach(var key in new[]{"x","y","z","width","height"}) {
                    var prop=item["properties"]![key]!.AsObject();prop["minimum"]=key is "width" or "height"?1:0;prop["maximum"]=key is "width" or "height"?8:4095;
                }
            }
            yield return new Tool{Name=name,InputSchema=JsonSerializer.SerializeToElement(schema),OutputSchema=JsonSerializer.SerializeToElement(new{type="object"}),
                Description=name==Preview?"0.53: Pausierte native Pflanzvorpruefung fuer 1-8 nicht ueberlappende Rechtecke je 1-8x1-8, maximal 512 Zellen. Je Rechteck resource, workBuildingId und requireMoist (Standard true). Prueft Pflanzvalidator, passende PlanterBuilding-Pflanzenliste, echte Gelaendearbeitsreichweite, Feuchte und Belegungsart. Schluessel fuer genau diese Vorschau; ausgeschlossene Zellen werden nicht veraendert. session/jobId als UUID. Keine Pflanz-/Wachstums-/Ertragsgarantie. detail=full Einzelzellen.":
                name==Apply?"0.53: Genau dieselben Rechtecke und Vorschau-key einmal ausfuehren. Frische gesamte Vorpruefung auf dem Spielthread; bei Aenderung kein Auftrag. Markiert nur eligible-Zellen, ueberschreibt keine fremden Pflanzmarkierungen, loescht keine Pflanzen oder Stapel. Gleiche jobId+Parameter liefert alten Beleg, kein Replay. unconfirmed nur lesend klaeren. Ergebnis ist Pflanzauftrag, keine gewachsene Pflanze. Keine Simulation. session/jobId UUID; detail=full.":
                "Liest nativen Pflanzchargen-Beleg fuer session/jobId ohne erneute Ausfuehrung. Sitzungslokal, nach Spielneuladen kein Replay. Compact mit Summen/Gruenden und begrenzten Zellbeispielen; full alle Zellen. Beleg ist historisch, keine frische Wachstumsbeobachtung.",
                Annotations=new(){ReadOnlyHint=name!=Apply,DestructiveHint=name==Apply,IdempotentHint=true,OpenWorldHint=false}};
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool enabled,CancellationToken ct)
    {
        if(args.ValueKind!=JsonValueKind.Object)throw new ArgumentException("invalid_argument_structure");
        if(name==Apply&&!enabled)throw new ArgumentException("planting_actions_disabled");
        if(name!=Status&&args.TryGetProperty("rectangles",out var rectangles)&&rectangles.ValueKind==JsonValueKind.Array)
            foreach(var rect in rectangles.EnumerateArray())
                if(rect.ValueKind!=JsonValueKind.Object||new[]{"x","y","z","width","height","resource","workBuildingId"}.Any(k=>!rect.TryGetProperty(k,out _)))
                    throw new ArgumentException("planting_missing_rectangle_field");
        PlantingBatchRequest request;string detail;
        if(name==Preview){var r=ArgumentDiagnostics.Parse<PlantingPreview>(args);request=new(){Session=r.Session,JobId=r.JobId,Rectangles=r.Rectangles};detail=r.Detail;}
        else if(name==Apply){var r=ArgumentDiagnostics.Parse<PlantingApply>(args);request=new(){Session=r.Session,JobId=r.JobId,Rectangles=r.Rectangles,Key=r.Key,Operation="apply"};detail=r.Detail;}
        else {var r=ArgumentDiagnostics.Parse<PlantingStatus>(args);request=new(){Session=r.Session,JobId=r.JobId,Operation="status"};detail=r.Detail;}
        if(detail is not ("compact" or "full"))throw new ArgumentException("invalid_detail");
        var envelope=await client.PlantingBatch(request,ct);
        return Present(envelope,detail);
    }
    public static JsonObject Present(BridgeEnvelope<PlantingBatchReport> e,string detail)
    {
        var d=e.Data;var node=JsonSerializer.SerializeToNode(d,NativeJson.Options)!.AsObject();
        node["summary"]=JsonSerializer.SerializeToNode(new{total=d.Cells.Length,eligible=d.Cells.Count(c=>c.Eligible),marked=d.Cells.Count(c=>c.Result=="marked"),
            alreadyMarked=d.Cells.Count(c=>c.Result=="already_marked"),excluded=d.Cells.Count(c=>c.Result=="excluded"),
            unconfirmed=d.Cells.Count(c=>c.Result=="unconfirmed"),notAttempted=d.Cells.Count(c=>c.Result=="not_attempted"),
            reasons=d.Cells.SelectMany(c=>c.Reasons).GroupBy(s=>s).Select(g=>new{reason=g.Key,cells=g.Count()}).ToArray()},NativeJson.Options);
        node["evidence"]=d.Outcome=="preview"?"native_preflight_not_planting":"designation_receipt_not_growth";
        node["detailAvailable"]="full";
        if(detail!="full") {node.Remove("cells");node["excludedExamples"]=JsonSerializer.SerializeToNode(d.Cells.Where(c=>!c.Eligible).Take(8),NativeJson.Options);node["excludedExamplesTruncated"]=d.Cells.Count(c=>!c.Eligible)>8;}
        return JsonSerializer.SerializeToNode(new NativeResult<JsonObject>(1,"ok",node,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!.AsObject();
    }
}
