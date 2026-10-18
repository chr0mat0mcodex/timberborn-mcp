using System.Security.Cryptography;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public static class McpRuntimeIdentity
{
    // MVIDs identify loaded modules, not possibly replaced files at their old paths.
    // No paths, Git metadata, configuration, timestamps or session data enter the payload.
    public static NativeMcpIdentity Create(IEnumerable<Tool> effectiveCatalog) => new(
        BuildFingerprint([
            ("Timberborn.McpServer", typeof(McpRuntimeIdentity).Module.ModuleVersionId),
            ("Timberborn.Backend.Native", typeof(NativeClient).Module.ModuleVersionId),
            ("Timberborn.Bridge.Core", typeof(BridgeRequest).Module.ModuleVersionId),
        ]),
        CatalogFingerprint(effectiveCatalog),
        "loaded-mvid-v1:McpServer,Backend.Native,Bridge.Core",
        "native-backend-tools-v1:name,description,inputSchema,outputSchema,annotations",
        "not_observed");

    public static string BuildFingerprint(IEnumerable<(string Name, Guid ModuleId)> modules) =>
        Hash(JsonSerializer.SerializeToElement(modules.OrderBy(m => m.Name, StringComparer.Ordinal)
            .Select(m => new { name = m.Name, moduleId = m.ModuleId.ToString("D") })));

    public static string CatalogFingerprint(IEnumerable<Tool> catalog) =>
        Hash(JsonSerializer.SerializeToElement(catalog.OrderBy(t => t.Name, StringComparer.Ordinal)
            .Select(t => new { t.Name, t.Description, t.InputSchema, t.OutputSchema, t.Annotations }), NativeJson.Options));

    private static string Hash(JsonElement value)
    {
        using var bytes = new MemoryStream();
        using (var writer = new Utf8JsonWriter(bytes)) WriteCanonical(writer, value);
        return "sha256:" + Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant();
    }

    private static void WriteCanonical(Utf8JsonWriter writer, JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var property in value.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                writer.WritePropertyName(property.Name);
                WriteCanonical(writer, property.Value);
            }
            writer.WriteEndObject();
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (var item in value.EnumerateArray()) WriteCanonical(writer, item);
            writer.WriteEndArray();
        }
        else value.WriteTo(writer);
    }
}
