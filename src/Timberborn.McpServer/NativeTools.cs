using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed class NativeTools(NativeClient client, bool enableValidation = false) : IDisposable
{
    public static IReadOnlyList<Tool> Catalog(bool enableValidation = false)
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var names = new List<string> { "timberborn_status", "inspect_colony", "inspect_map_region", "find_buildings", "inspect_build_catalog", "precheck_build_site" };
        if (enableValidation) names.Add("validate_build_site");
        return names.Select(name =>
        {
            bool map = name == "inspect_map_region";
            bool validation = name == "validate_build_site";
            bool site = name == "precheck_build_site" || validation;
            var properties = new JsonObject();
            if (map) foreach (var (key, min, max) in new[] { ("x", 0, 4095), ("y", 0, 4095), ("z", 0, 4095), ("width", 1, 8), ("height", 1, 8), ("depth", 1, 4) })
                properties[key] = new JsonObject { ["type"] = "integer", ["minimum"] = min, ["maximum"] = max };
            if (name == "find_buildings")
            {
                properties["offset"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 65535 };
                properties["limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 32 };
            }
            if (site)
            {
                properties["template"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("Lodge.Folktails", "Path") };
                foreach (var key in new[] { "x", "y", "z", "rotation" })
                    properties[key] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = key == "rotation" ? 3 : 4095 };
            }
            if (validation) properties["session"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" };
            var input = new JsonObject { ["type"] = "object", ["properties"] = properties, ["additionalProperties"] = false };
            if (properties.Count > 0) input["required"] = new JsonArray(properties.Select(p => (JsonNode?)JsonValue.Create(p.Key)).ToArray());
            Type result = name switch { "inspect_map_region" => typeof(NativeResult<NativeMap>),
                "find_buildings" => typeof(NativeResult<NativeObjects>), "inspect_build_catalog" => typeof(NativeResult<NativeCatalog>),
                "precheck_build_site" => typeof(NativeResult<NativeSite>), "timberborn_status" => typeof(NativeResult<NativeStatus>),
                "validate_build_site" => typeof(NativeResult<NativeValidation>),
                _ => typeof(NativeResult<NativeSnapshot>) };
            return new Tool { Name = name,
                Description = validation ? "Geschützter Pilot: erzeugt/verwendet eine eigene temporäre Vorschau und ruft Spielvalidatoren auf. Kein Bauauftrag. Frische Session-ID erforderlich, maximal 8 Versuche pro Sitzung. Bei Fehler/Zustandsabweichung gesperrt; niemals automatisch wiederholen. Valid bedeutet geometrische Spielprüfung, keine Material-/Liefer-/Fertigstellungszusage."
                    : name == "find_buildings" ? "Liest Gebäude und Wege mit stabilen Vorlagen-IDs, Position, Eingang und bis zu 64 belegten Zellen je Objekt. Maximal 32 Einträge; Seiten sind frische Beobachtungen."
                    : name == "inspect_build_catalog" ? "Liest die Pilotvorlagen Lodge.Folktails und Path, aktive Fraktion, Freischaltung, Geometrie und Kosten. Keine gesamte Bauliste; globale Vorräte garantieren keine lokale Lieferung."
                    : site ? "Rein lesende räumliche Vorprüfung für Lodge.Folktails oder Path. Rotation 0/1/2/3 entspricht Cw0/Cw90/Cw180/Cw270; ungespiegelt. Meldet Hindernisse, Terrain, Eingang und Kosten. gameValidated bleibt false: KEINE vollständige Spiel-Bauprüfung, Freigabe oder Platzierung."
                    : map ? "Liest höchstens 8x8x4 Spielzellen. Rohkoordinaten; keine Bauplatz- oder Erreichbarkeitsgarantie."
                    : name == "inspect_colony" ? "Eigene Spielmod: drei Beispielgüter (keine gesamte Nahrung), Betten, Personal und bis zu 16 Blockobjekte mit Position. Namen sind Daten. Enthält Sitzungskennung."
                    : "Prüft die eigene lesende Spielmod. Kein Fallback auf Fremdmods, keine Schreibfunktionen.",
                InputSchema = JsonSerializer.SerializeToElement(input), OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(result)),
                Annotations = new() { ReadOnlyHint = !validation, DestructiveHint = validation, IdempotentHint = !validation, OpenWorldHint = false } };
        }).ToArray();
    }
    public async Task<JsonObject> Invoke(string name, JsonElement args, CancellationToken ct)
    {
        JsonObject Wrap<T>(BridgeEnvelope<T> envelope) => (JsonObject)JsonSerializer.SerializeToNode(
            new NativeResult<T>(1, "ok", envelope.Data, new("native", false, envelope.SessionId, envelope.ObservedAtUtc), null), NativeJson.Options)!;
        try
        {
            if (args.ValueKind != JsonValueKind.Object) throw new ArgumentException();
            if (name == "validate_build_site" && !enableValidation) throw new ArgumentException();
            if (name is "inspect_map_region" or "find_buildings" or "precheck_build_site" or "validate_build_site")
            {
                var query = new NameValueCollection();
                foreach (var property in args.EnumerateObject())
                {
                    if ((name is "precheck_build_site" or "validate_build_site") &&
                        (property.Name == "template" || (name == "validate_build_site" && property.Name == "session")) && property.Value.ValueKind == JsonValueKind.String)
                    { query.Add(property.Name, property.Value.GetString()); continue; }
                    if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out var value)) throw new ArgumentException();
                    query.Add(property.Name, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (name == "find_buildings") return Wrap(await client.Objects(BridgeRequest.Parse("/agent-api/v1/objects", query), ct));
                if (name == "validate_build_site") return Wrap(await client.Validate(BridgeRequest.Parse("/agent-api/v1/site-validation", query), ct));
                if (name == "precheck_build_site") return Wrap(await client.Precheck(BridgeRequest.Parse("/agent-api/v1/site-precheck", query), ct));
                return Wrap(await client.Map(BridgeRequest.Parse("/agent-api/v1/map", query), ct));
            }
            if (name == "inspect_build_catalog" && !args.EnumerateObject().Any()) return Wrap(await client.Catalog(ct));
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
                new("native", false, null, null), new(code, "Native Bridge: Parameter, Verbindung oder Daten prüfen.", code == "backend_unavailable" && name != "validate_build_site")), NativeJson.Options)!;
        }
    }
    public void Dispose() => client.Dispose();
}
