using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;

namespace Timberborn.McpServer;

// Presentation only, after native validation. Never changes execution or retries actions.
public static class ResponsePresentation
{
    public static bool Supports(string name) => name is "timberborn_status" or
        "inspect_building_settings" or "set_storage_good" or "set_storage_mode" or "validate_building";

    private static string[] OptionalFields(string name) => name switch {
        "inspect_building_settings" => ["limitations", "allowedGoods", "allowedPlants"],
        "set_storage_good" or "set_storage_mode" => ["limitations", "observation"],
        "validate_building" => ["limitations", "candidateCells"],
        _ => []
    };

    public static Tool Describe(Tool tool)
    {
        if (!Supports(tool.Name)) return tool;
        var input = JsonNode.Parse(tool.InputSchema.GetRawText())!.AsObject();
        input["properties"]!.AsObject()["detail"] = new JsonObject {
            ["type"] = "string", ["enum"] = new JsonArray("compact", "full"), ["default"] = "compact",
            ["description"] = "compact: knapper Ergebnisbeleg. full: zusätzlich Auswahllisten, vollständiger Zustand und Geometriedetails. Bei Schreibaufrufen keinen zweiten Auftrag nur für Details senden; Zustand separat lesen." };
        tool.InputSchema = JsonSerializer.SerializeToElement(input);
        if (tool.OutputSchema is { } output) {
            var schema = JsonNode.Parse(output.GetRawText())!;
            MakeOptional(schema, OptionalFields(tool.Name));
            tool.OutputSchema = JsonSerializer.SerializeToElement(schema);
        }
        tool.Description += " Standard kompakt; detail=full für zusätzliche Details. Fehlende Detailfelder sind keine Null-/Leerbeobachtung.";
        return tool;
    }

    private static void MakeOptional(JsonNode node, string[] fields)
    {
        if (node is JsonObject obj) {
            if (obj["required"] is JsonArray required)
                for (int i = required.Count - 1; i >= 0; i--)
                    if (required[i]?.GetValue<string>() is { } key && fields.Contains(key)) required.RemoveAt(i);
            foreach (var entry in obj.ToArray())
                if (entry.Value is not null) MakeOptional(entry.Value, fields);
        } else if (node is JsonArray array)
            foreach (var item in array) if (item is not null) MakeOptional(item, fields);
    }

    // Strip presentation arguments before logging/backend dispatch; reject malformed values
    // before any game call. Preserve duplicate backend arguments for their strict validators.
    public static (JsonElement Arguments, bool Full) ReadRequest(string name, JsonElement args)
    {
        if (args.ValueKind != JsonValueKind.Object) throw new ArgumentException("Expected object");
        bool seen = false, full = false;
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) {
            writer.WriteStartObject();
            foreach (var property in args.EnumerateObject()) {
                if (property.Name != "detail") { property.WriteTo(writer); continue; }
                if (!Supports(name) || seen || property.Value.ValueKind != JsonValueKind.String ||
                    property.Value.GetString() is not ("compact" or "full"))
                    throw new ArgumentException("detail must be compact or full on a supported tool");
                seen = true; full = property.Value.GetString() == "full";
            }
            writer.WriteEndObject();
        }
        using var document = JsonDocument.Parse(stream.ToArray());
        return (document.RootElement.Clone(), full);
    }

    public static JsonObject Present(string name, JsonObject result, bool full)
    {
        var copy = (JsonObject)result.DeepClone();
        if (full || !Supports(name) || copy["status"]?.GetValue<string>() != "ok" ||
            copy["data"] is not JsonObject data) return copy;
        switch (name) {
            case "inspect_building_settings":
                data.Remove("limitations");
                (data["storage"] as JsonObject)?.Remove("allowedGoods");
                (data["farm"] as JsonObject)?.Remove("allowedPlants");
                break;
            case "set_storage_good":
            case "set_storage_mode":
                // Keep the complete evidence for an uncertain/rejected mutation.
                if (data["outcome"]?.GetValue<string>() == "applied") {
                    data.Remove("observation"); data.Remove("limitations");
                }
                break;
            case "validate_building":
                // Keep assessment, unknowns, reasons, affected objects and restoration.
                // Nested limitations describe actual proof boundaries and remain intact.
                data.Remove("limitations");
                (data["roadProtection"] as JsonObject)?.Remove("candidateCells");
                break;
        }
        return copy;
    }

    public static CallToolResult ToToolResult(JsonObject result) => new() {
        StructuredContent = JsonSerializer.SerializeToElement(result),
        // Retain equivalent text for clients without structured-content support.
        Content = [new TextContentBlock { Text = result.ToJsonString() }],
        IsError = result["status"]!.GetValue<string>() == "error"
    };
}
