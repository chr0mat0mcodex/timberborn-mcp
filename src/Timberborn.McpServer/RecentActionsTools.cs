using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
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
            ["source"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("all","mcp","unknown")},
            ["detail"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("compact","full"),["default"]="compact"}
        };
        return new Tool{Name=Name,
            Description="Ab 0.40: sitzungsgebundenes Änderungsjournal, 512 Einträge im Speicher, maximal 64 je Abruf. Ohne after letzte X passende Einträge chronologisch; mit after Änderungen seit Cursor vorwärts. nextCursor immer übernehmen, auch bei leerer Filterseite. Filterwechsel braucht eigenen Cursor. gap/dropped melden Pufferverlust, trackingComplete und observationFailures Erfassungslücken. Gebäude/Wege: hinzugefügt, entfernt, fertig, Pause, Rezept, Lagergut/-modus, Sollpersonal, Abrissmarkierung; außerdem Tempo. Ereignisse plus periodische Zustandsvergleiche und frischer Abgleich beim Abruf, Zwischenänderungen können fehlen. Herkunft mcp nur im direkten Ausführungskontext, sonst unknown: kein Beweis menschlicher Eingriffe oder vollständiger Auftragshistorie. Ab 0.54 auch planting_designation/cutting_designation sowie plant_added/plant_removed/plant_state (tree, stump, tree_unknown, plant). Pflanzen und Flaechen werden begrenzt abgetastet; keine Forschung. Compact buendelt gleiche Aenderungen je 8x8-Rasterkachel/Hoehe innerhalb dieser Seite unter page.groups; page.items behaelt andere Ereignisse. count zaehlt Ereignisse, Grenzen sind keine gefuellte Flaeche oder exakte Reihenfolge. Cursor bezieht sich auf rohe Ereignisse, nicht Gruppen. detail=full liefert alle Einzelzellen/Objekte; fuer denselben Ausschnitt denselben after/limit/Filter verwenden, bei weiterlaufender Historie koennen Seiten abweichen. Für 'Überprüf mal meine letzten Aktionen'; Änderungen erklären, nicht automatisch zurücknehmen. Keine Historie vor Erfassungsbeginn oder vor Sitzungswechsel.",
            InputSchema=JsonSerializer.SerializeToElement(new JsonObject{["type"]="object",["properties"]=p,["required"]=new JsonArray("session"),["additionalProperties"]=false}),
            OutputSchema=JsonSerializer.SerializeToElement(new{type="object"}),
            Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false}};
    }
    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct)
    {
        var q=new NameValueCollection();string detail="compact";bool seenDetail=false;
        foreach(var p in args.EnumerateObject()) {
            if(p.Name=="detail") {
                if(seenDetail||p.Value.ValueKind!=JsonValueKind.String||p.Value.GetString() is not ("compact" or "full"))throw new ArgumentException("invalid_detail");
                seenDetail=true;detail=p.Value.GetString()!;continue;
            }
            if(p.Name is "limit" or "after") {
                if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt64(out long n))throw new ArgumentException();
                q.Add(p.Name,n.ToString(CultureInfo.InvariantCulture));
            } else {if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString());}
        }
        var r=RecentActionsRequest.Parse(q);var e=await client.RecentActions(r,ct);
        return Present(e,detail);
    }
    public static JsonObject Present(BridgeEnvelope<NativeRecentActions> e,string detail) {
        var result=(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<NativeRecentActions>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        if(detail=="full")return result;
        var page=result["data"]!["page"]!.AsObject();var items=e.Data.Page.Items;
        var spatial=items.Where(i=>RecentActionsRequest.SpatialKinds.Contains(i.Kind)).ToArray();
        page["items"]=JsonSerializer.SerializeToNode(items.Where(i=>!RecentActionsRequest.SpatialKinds.Contains(i.Kind)),NativeJson.Options);
        page["groups"]=JsonSerializer.SerializeToNode(spatial.GroupBy(i=>new{i.Kind,i.Source,i.Evidence,i.Template,i.Before,i.After,tileX=i.X!.Value/8,tileY=i.Y!.Value/8,i.Z})
            .Select(g=>new{g.Key.Kind,g.Key.Source,g.Key.Evidence,g.Key.Template,g.Key.Before,g.Key.After,
                count=g.Count(),firstSequence=g.Min(i=>i.Sequence),lastSequence=g.Max(i=>i.Sequence),
                bounds=new{x=g.Min(i=>i.X),y=g.Min(i=>i.Y),z=g.Key.Z,width=g.Max(i=>i.X)-g.Min(i=>i.X)+1,height=g.Max(i=>i.Y)-g.Min(i=>i.Y)+1},
                firstObservedAtUtc=g.Min(i=>i.ObservedAtUtc),lastObservedAtUtc=g.Max(i=>i.ObservedAtUtc)})
            .OrderBy(g=>g.firstSequence),NativeJson.Options);
        page["eventCount"]=items.Length;
        page["grouping"]="same_change_per_8x8_tile_and_height_page_only_bounds_not_filled_area";
        return result;
    }
}
