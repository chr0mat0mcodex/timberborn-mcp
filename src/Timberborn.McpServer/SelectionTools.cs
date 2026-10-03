using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class SelectionTools
{
    public static Tool Reader()
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        return new Tool { Name = "inspect_selection",
            Description = "Ab Bridge 0.30.0: Liest die aktuell angeklickte UI-Objektauswahl für Aussagen wie 'das markierte Objekt'. Frische session erforderlich. state=none bedeutet keine Auswahl; unsupported bedeutet ausgewählt, aber nicht als bestehendes Spielobjekt mit Vorlage auflösbar. selected liefert ID, Vorlage, kind, Weltposition und bei Blockobjekten Rasterposition. Kein Hover, keine Flächen-/Abriss-/Pflanzmarkierung, keine persönlichen Namen. Auswahl erteilt keinen Änderungsauftrag; vor späteren Aktionen frisch lesen und Ziel prüfen. Rein lesend; verändert Auswahl und Kamera nicht.",
            InputSchema = JsonSerializer.SerializeToElement(new JsonObject { ["type"] = "object",
                ["properties"] = new JsonObject { ["session"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" } },
                ["required"] = new JsonArray("session"), ["additionalProperties"] = false }),
            OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeSelection>))),
            Annotations = new() { ReadOnlyHint = true, DestructiveHint = false, IdempotentHint = true, OpenWorldHint = false } };
    }
    public static async Task<JsonObject> Invoke(NativeClient client, JsonElement args, CancellationToken ct)
    {
        var q = new NameValueCollection();
        foreach (var p in args.EnumerateObject()) {
            if (p.Value.ValueKind != JsonValueKind.String) throw new ArgumentException();
            q.Add(p.Name, p.Value.GetString());
        }
        var r = BridgeRequest.Parse("/agent-api/v1/selection", q);
        var e = await client.Selection(r, ct);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<NativeSelection>(1, "ok", e.Data,
            new("native", false, e.SessionId, e.ObservedAtUtc), null), NativeJson.Options)!;
    }
}
