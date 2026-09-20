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
        return new Tool { Name="inspect_agent_log", Description="Liest die neuesten bis zu 32 von maximal 128 Ingame-MCP-Logeinträgen, neueste zuerst. Begründungen sind vom Agenten mitgegebene kurze Absichten, keine internen Gedankengänge. Nur aktueller Spielstand im Arbeitsspeicher; Log ist Best-Effort-Telemetrie, keine Autorisierung oder garantierte Aktionshistorie. Der eigene laufende Aufruf kann enthalten sein.",
            InputSchema=JsonSerializer.SerializeToElement(new {type="object",properties=new {},additionalProperties=false}),
            OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeActivityLog>))),
            Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false} };
    }
    public static Tool WithReasoning(Tool tool)
    {
        var schema=JsonNode.Parse(tool.InputSchema.GetRawText())!.AsObject();
        schema["properties"]!.AsObject()["reasoning"]=new JsonObject { ["type"]="string",["maxLength"]=600,
            ["description"]="Optional: kurze, für den Spieler lesbare Begründung dieser konkreten Aktion/Abfrage. Ziel und relevanter Zustand, keine internen Gedankengänge oder Zugangsdaten. Erscheint im Ingame-MCP-Log." };
        tool.InputSchema=JsonSerializer.SerializeToElement(schema);return tool;
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
        if(args.EnumerateObject().Any())throw new ArgumentException();
        var e=await client.Activity(ct);var d=e.Data;
        if(e.BridgeVersion is not ("0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0" or "0.19.0" or "0.19.1" or "0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or "0.21.1") || d.Capacity!=128 || d.Items is null || d.Items.Length>32 || d.Revision<0 ||
            d.Items.Any(i=>i is null || !ActivityRequest.IsState(i.State) || i.Reasoning is null || i.Summary is null || i.Reasoning.Length>600 || i.Summary.Length>400))throw new InvalidDataException("Invalid activity log");
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<NativeActivityLog>(1,"ok",d,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
    }
}
