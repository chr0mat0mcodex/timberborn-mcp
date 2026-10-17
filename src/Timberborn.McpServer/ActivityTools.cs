using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class ActivityTools
{
    public static Tool Reader()
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        return new Tool { Name="inspect_agent_log", Description="Liest das Ingame-MCP-Log: limit 1–32 (Standard 32), bis 128 gespeicherte Aufrufe, neueste Startsequenz zuerst. Projekt-/Laufbezüge mit Herkunft request/result; kein Existenz- oder Wirkungsnachweis. projectId (auch Elternprojekt) und runId als UND-Filter. nextCursor mit denselben tool/state/projectId/runId-Filtern weitergeben; neue Aufrufe verschieben Folgeseiten nicht. Status wird beim Lesen ausgewertet, kein eingefrorener Snapshot. gapDetected/unavailableBeforeCursor zeigen verdrängte ältere Aufrufe unabhängig vom Filter; Telemetrieausfälle bleiben unbekannt. Cursor nach Löschen/Spielneuladen ungültig. Begründungen sind kurze Absichten, keine internen Gedankengänge. Best-Effort-Telemetrie, kein Wirkungsnachweis. Der eigene laufende Aufruf kann enthalten sein. Paginierung benötigt die aktualisierte Bridge.",
            InputSchema=JsonSerializer.SerializeToElement(new {type="object",properties=new {
                limit=new {type="integer",minimum=1,maximum=32,@default=32},
                cursor=new {type="string",minLength=1,maxLength=260},
                projectId=new {type="string",format="uuid",description="Projekt oder Elternprojekt; Anfrage/Ergebnis sind getrennte Herkunft, kein Existenznachweis."},
                runId=new {type="string",format="uuid",description="Simulationslauf; zusammen mit projectId als UND-Filter."},
                tool=new {type="string",minLength=1,maxLength=80,pattern="^[a-z0-9_]+$"},
                state=new {type="string",@enum=new[]{"running","ok","applied","rejected","unconfirmed","error","cancelled"}}
            },additionalProperties=false}),
            OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeActivityLog>))),
            Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false} };
    }
    public static Tool WithReasoning(Tool tool)
    {
        var schema=JsonNode.Parse(tool.InputSchema.GetRawText())!.AsObject();
        schema["properties"]!.AsObject()["reasoning"]=new JsonObject { ["type"]="string",["minLength"]=1,["maxLength"]=600,["pattern"]="\\S",
            ["description"]="Pflicht: kurze, für den Spieler lesbare Begründung dieser konkreten Aktion/Abfrage. Nicht leer oder nur Leerraum. Ziel und relevanter Zustand, keine internen Gedankengänge oder Zugangsdaten. Erscheint beim nativen Backend im Ingame-MCP-Log." };
        var required = schema["required"] as JsonArray;
        if (required is null) schema["required"] = required = new JsonArray();
        if (!required.Any(n => n?.GetValue<string>() == "reasoning")) required.Add("reasoning");
        tool.InputSchema=JsonSerializer.SerializeToElement(schema);return tool;
    }
    public static JsonElement RequireReasoning(JsonElement args)
    {
        // Before any backend access, including best-effort activity telemetry.
        var stripped = WithoutReasoning(args);
        if (!args.TryGetProperty("reasoning", out var reason) || string.IsNullOrWhiteSpace(reason.GetString()))
            throw new ArgumentException("reasoning_required");
        return stripped;
    }
    public static JsonElement WithoutReasoning(JsonElement args)
    {
        if(args.ValueKind!=JsonValueKind.Object)throw new ArgumentException();
        using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream)) {
            writer.WriteStartObject();bool seen=false;
            foreach(var p in args.EnumerateObject()) {
                if(p.Name=="reasoning") { if(seen || p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();seen=true;ActivityRequest.Clean(p.Value.GetString()!,600); }
                else p.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
        using var doc=JsonDocument.Parse(stream.ToArray());return doc.RootElement.Clone();
    }
    public static (string Reasoning,string Summary) Describe(JsonElement args)
    {
        if(args.ValueKind!=JsonValueKind.Object)return ("","Ungültige Argumente");
        string reason="";var summary=new List<string>();
        foreach(var p in args.EnumerateObject()) {
            if(p.Name=="reasoning") {try {reason=p.Value.ValueKind==JsonValueKind.String?ActivityRequest.Clean(p.Value.GetString()!,600):"Ungültige Begründung";}catch(ArgumentException){reason="Ungültige Begründung";}continue;}
            if(p.Name is not ("id" or "template" or "x" or "y" or "z" or "width" or "height" or "depth" or "rotation" or "speed" or "paused" or "good" or "mode" or "priority" or "resource" or "kind" or "operation" or "desiredWorkers" or "offset" or "limit"))continue;
            string value=p.Value.ValueKind==JsonValueKind.String?p.Value.GetString()!:p.Value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False?p.Value.GetRawText():"";
            if(value.Length<=160 && (value==""||BuildingPolicy.ValidTemplate(value)))summary.Add(p.Name+"="+ActivityRequest.Clean(value,160));
        }
        var text=string.Join(" · ",summary);return(reason,text.Length>400?text[..397]+"...":text);
    }
    public static async Task<JsonObject> Read(NativeClient client,JsonElement args,CancellationToken ct)
    {
        var values=new System.Collections.Specialized.NameValueCollection(StringComparer.Ordinal);
        if(args.ValueKind!=JsonValueKind.Object)throw new ArgumentException();
        foreach(var property in args.EnumerateObject()) {
            if(property.Name=="limit") {
                if(property.Value.ValueKind!=JsonValueKind.Number || !property.Value.TryGetInt32(out int limit))throw new ArgumentException();
                values.Add(property.Name,limit.ToString(System.Globalization.CultureInfo.InvariantCulture));
            } else {
                if(property.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();
                values.Add(property.Name,property.Value.GetString());
            }
        }
        var query=ActivityLogQuery.Parse(values);
        // Preserve the original no-argument route for older installed bridges.
        var e=values.Count==0?await client.Activity(ct):await client.Activity(query,ct);var d=e.Data;
        if(e.BridgeVersion is not ("0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0" or "0.19.0" or "0.19.1" or "0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or "0.21.1" or "0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0") || d.Capacity!=128 || d.Items is null || d.Items.Length>32 || d.Revision<0 ||
            d.Items.Any(i=>i is null || !ActivityRequest.IsState(i.State) || i.Reasoning is null || i.Summary is null || i.Reasoning.Length>600 || i.Summary.Length>400 || i.OutcomeSummary is null || i.OutcomeSummary.Length>400))throw new InvalidDataException("Invalid activity log");
        if(e.BridgeVersion is ("0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0") && d.PaginationVersion is null)throw new InvalidDataException("missing_activity_pagination");
        if(d.PaginationVersion is not null)ValidatePage(d,query);
        else if(values.Count>0)throw new InvalidDataException("activity_pagination_not_supported");
        if(e.BridgeVersion is ("0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0") && d.CorrelationVersion!=1)throw new InvalidDataException("missing_activity_correlation");
        if(d.CorrelationVersion is null && (query.ProjectId.Length>0||query.RunId.Length>0))throw new InvalidDataException("activity_correlation_not_supported");
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<NativeActivityLog>(1,"ok",d,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
    }

    public static void ValidatePage(NativeActivityLog d,ActivityLogQuery query)
    {
        if(d.CorrelationVersion is not null) {
            if(d.CorrelationVersion!=1||d.ProjectId!=query.ProjectId||d.RunId!=query.RunId||
                d.Items.Any(i=>i.Correlations is null || i.Correlations.Length>8 || i.Correlations.Any(c=>!ActivityCorrelation.Valid(c)) ||
                    ActivityCorrelation.Encode(i.Correlations)!=ActivityCorrelation.Encode(ActivityCorrelation.Merge(i.Correlations,Array.Empty<ActivityCorrelation>())) || !query.Matches(i)))
                throw new InvalidDataException("invalid_activity_correlation_page");
        }
        if(d.PaginationVersion!=1 || !Guid.TryParseExact(d.LogEpoch,"N",out _) ||
            d.RetainedCount is null or <0 or >128 || d.EvictedCount is null or <0 or >9007199254740863L ||
            d.HasMore is null || d.GapDetected is null || d.UnavailableBeforeCursor is null or <0 ||
            d.Tool!=query.Tool || d.State!=query.State || d.Items.Length>query.Limit || d.Items.Length>d.RetainedCount ||
            d.GapDetected!=(d.UnavailableBeforeCursor>0) || d.UnavailableBeforeCursor>d.EvictedCount ||
            (d.EvictedCount>0 && d.RetainedCount!=128) ||
            (d.RetainedCount==0 ? d.OldestSequence is not null || d.NewestSequence is not null :
                d.OldestSequence!=d.EvictedCount+1 || d.NewestSequence!=d.EvictedCount+d.RetainedCount))
            throw new InvalidDataException("invalid_activity_page");
        long before=(d.NewestSequence??0)+1;
        if(query.Cursor.Length>0) {
            var cursor=query.DecodeCursor();
            if(cursor.Epoch!=d.LogEpoch || cursor.Before>before)throw new InvalidDataException("activity_cursor_mismatch");
            before=cursor.Before;
        }
        if(d.UnavailableBeforeCursor!=Math.Min(before-1,d.EvictedCount!.Value))throw new InvalidDataException("activity_gap_mismatch");
        foreach(var item in d.Items) {
            if(item.Sequence<d.OldestSequence || item.Sequence>=before || item.Sequence<=0 ||
                (query.Tool.Length>0&&item.Tool!=query.Tool) || (query.State.Length>0&&item.State!=query.State))
                throw new InvalidDataException("invalid_activity_page_order_or_filter");
            before=item.Sequence;
        }
        if(d.HasMore==true ? d.Items.Length!=query.Limit || d.NextCursor!=query.EncodeCursor(d.LogEpoch!,before) : d.NextCursor is not null)
            throw new InvalidDataException("invalid_activity_next_cursor");
    }
}
