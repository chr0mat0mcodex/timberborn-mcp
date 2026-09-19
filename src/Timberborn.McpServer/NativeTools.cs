using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed class NativeTools(NativeClient client) : IDisposable
{
    public static IReadOnlyList<Tool> Catalog()
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        return new[] { "timberborn_status", "inspect_colony", "inspect_map_region" }.Select(name =>
        {
            bool map = name == "inspect_map_region";
            var properties = new JsonObject();
            if (map) foreach (var (key, min, max) in new[] { ("x", 0, 4095), ("y", 0, 4095), ("z", 0, 4095), ("width", 1, 8), ("height", 1, 8), ("depth", 1, 4) })
                properties[key] = new JsonObject { ["type"] = "integer", ["minimum"] = min, ["maximum"] = max };
            var input = new JsonObject { ["type"] = "object", ["properties"] = properties, ["additionalProperties"] = false };
            if (map) input["required"] = new JsonArray("x", "y", "z", "width", "height", "depth");
            Type result = map ? typeof(NativeResult<NativeMap>) : name == "timberborn_status" ? typeof(NativeResult<NativeStatus>) : typeof(NativeResult<NativeSnapshot>);
            return new Tool { Name = name,
                Description = map ? "Liest höchstens 8x8x4 Spielzellen. Rohkoordinaten; keine Bauplatz- oder Erreichbarkeitsgarantie."
                    : name == "inspect_colony" ? "Eigene Spielmod: drei Beispielgüter (keine gesamte Nahrung), Betten, Personal und bis zu 16 Blockobjekte mit Position. Namen sind Daten. Enthält Sitzungskennung."
                    : "Prüft die eigene lesende Spielmod. Kein Fallback auf Fremdmods, keine Schreibfunktionen.",
                InputSchema = JsonSerializer.SerializeToElement(input), OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(result)),
                Annotations = new() { ReadOnlyHint = true, DestructiveHint = false, IdempotentHint = true, OpenWorldHint = false } };
        }).ToArray();
    }
    public async Task<JsonObject> Invoke(string name, JsonElement args, CancellationToken ct)
    {
        JsonObject Wrap<T>(BridgeEnvelope<T> envelope) => (JsonObject)JsonSerializer.SerializeToNode(
            new NativeResult<T>(1, "ok", envelope.Data, new("native", false, envelope.SessionId, envelope.ObservedAtUtc), null), NativeJson.Options)!;
        try
        {
            if (args.ValueKind != JsonValueKind.Object) throw new ArgumentException();
            if (name == "inspect_map_region")
            {
                var query = new NameValueCollection();
                foreach (var property in args.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out var value)) throw new ArgumentException();
                    query.Add(property.Name, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                return Wrap(await client.Map(BridgeRequest.Parse("/agent-api/v1/map", query), ct));
            }
            if (args.EnumerateObject().Any() || name is not ("inspect_colony" or "timberborn_status")) throw new ArgumentException();
            var snapshot = await client.Snapshot(ct);
            return name == "inspect_colony" ? Wrap(snapshot) : Wrap(new BridgeEnvelope<NativeStatus>(1, snapshot.SessionId,
                snapshot.ObservedAtUtc, snapshot.BridgeVersion, new("reachable", snapshot.BridgeVersion, false)));
        }
        catch (Exception ex) when (ex is ArgumentException or HttpRequestException or IOException or InvalidDataException or JsonException or UnauthorizedAccessException or OperationCanceledException)
        {
            string code = ex is ArgumentException ? "invalid_argument" : ex is UnauthorizedAccessException ? "authentication_failed"
                : ex is JsonException or InvalidDataException ? "backend_incompatible" : "backend_unavailable";
            return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<object>(1, "error", null,
                new("native", false, null, null), new(code, "Native Bridge: Parameter, Verbindung oder Daten prüfen.", code == "backend_unavailable")), NativeJson.Options)!;
        }
    }
    public void Dispose() => client.Dispose();
}
