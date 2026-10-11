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

public static class RecentActionsTools
{
    public const string Name="inspect_recent_actions";
    public static Tool Reader()
    {
        var p=new JsonObject {
            ["session"]=new JsonObject{["type"]="string",["format"]="uuid"},
            ["limit"]=new JsonObject{["type"]="integer",["minimum"]=1,["maximum"]=64,["default"]=32},
            ["after"]=new JsonObject{["type"]="integer",["minimum"]=0,["maximum"]=9007199254740991L},
            ["kind"]=new JsonObject{["type"]="string",["enum"]=JsonSerializer.SerializeToNode(new[]{"all"}.Concat(RecentActionsRequest.Kinds))},
            ["source"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("all","mcp","unknown")}
        };
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        return new Tool{Name=Name,
            Description="Ab 0.40: sitzungsgebundenes Änderungsjournal, 512 Einträge im Speicher, maximal 64 je Abruf. Ohne after letzte X passende Einträge chronologisch; mit after Änderungen seit Cursor vorwärts. nextCursor immer übernehmen, auch bei leerer Filterseite. Filterwechsel braucht eigenen Cursor. gap/dropped melden Pufferverlust, trackingComplete und observationFailures Erfassungslücken. Gebäude/Wege: hinzugefügt, entfernt, fertig, Pause, Rezept, Lagergut/-modus, Sollpersonal, Abrissmarkierung; außerdem Tempo. Ereignisse plus periodische Zustandsvergleiche und frischer Abgleich beim Abruf, Zwischenänderungen können fehlen. Herkunft mcp nur im direkten Ausführungskontext, sonst unknown: kein Beweis menschlicher Eingriffe oder vollständiger Auftragshistorie. Keine Pflanzen/Flächen/Forschung. Für 'Überprüf mal meine letzten Aktionen'; Änderungen erklären, nicht automatisch zurücknehmen. Keine Historie vor Erfassungsbeginn oder vor Sitzungswechsel.",
            InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=p,["required"]=new JsonArray("session"),["additionalProperties"]=false}),
            OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeRecentActions>))),
            Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false}};
    }
    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct)
    {
        var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()) {
            if(p.Name is "limit" or "after") {
                if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt64(out long n))throw new ArgumentException();
                q.Add(p.Name,n.ToString(CultureInfo.InvariantCulture));
            } else {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
        }
        var r=RecentActionsRequest.Parse(q);var e=await client.RecentActions(r,ct);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<NativeRecentActions>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
    }
}
