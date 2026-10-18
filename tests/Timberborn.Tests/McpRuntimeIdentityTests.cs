using System.Net;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class McpRuntimeIdentityTests
{
    private static Tool Tool(string name = "reader", string schema = "{\"type\":\"object\",\"properties\":{}}") => new()
    {
        Name = name, Description = "Synthetic reader",
        InputSchema = JsonSerializer.Deserialize<JsonElement>(schema),
        OutputSchema = JsonSerializer.SerializeToElement(new { type = "object" }),
        Annotations = new() { ReadOnlyHint = true, DestructiveHint = false },
    };

    [Theory]
    [InlineData("server")]
    [InlineData("backend")]
    [InlineData("core")]
    public void ChangedLoadedModuleChangesBuildIdentityWithoutBridgeVersionChange(string changed)
    {
        var first = new[] { ("server", Guid.Parse("11111111-1111-4111-8111-111111111111")),
            ("backend", Guid.Parse("22222222-2222-4222-8222-222222222222")),
            ("core", Guid.Parse("33333333-3333-4333-8333-333333333333")) };
        var second = first.Select(m => (m.Item1, m.Item1 == changed ? Guid.Parse("44444444-4444-4444-8444-444444444444") : m.Item2));
        Assert.NotEqual(McpRuntimeIdentity.BuildFingerprint(first), McpRuntimeIdentity.BuildFingerprint(second));
        Assert.Equal(McpRuntimeIdentity.BuildFingerprint(first), McpRuntimeIdentity.BuildFingerprint(first.Reverse()));
    }

    [Theory]
    [InlineData("name")]
    [InlineData("description")]
    [InlineData("input")]
    [InlineData("output")]
    [InlineData("annotations")]
    public void PublicCatalogChangesAreDetectable(string field)
    {
        var changed = Tool();
        switch (field)
        {
            case "name": changed.Name = "other"; break;
            case "description": changed.Description = "Changed meaning"; break;
            case "input":
                // MCP requires an object input schema; change a valid property, not the root type.
                changed.InputSchema = JsonSerializer.SerializeToElement(new
                {
                    type = "object", properties = new { limit = new { type = "integer" } },
                });
                break;
            case "output": changed.OutputSchema = null; break;
            case "annotations": changed.Annotations = new() { ReadOnlyHint = false, DestructiveHint = true }; break;
        }
        Assert.NotEqual(McpRuntimeIdentity.CatalogFingerprint([Tool()]), McpRuntimeIdentity.CatalogFingerprint([changed]));
    }

    [Fact]
    public void CatalogOrderingAndObjectPropertyOrderingDoNotChangeIdentity()
    {
        var first = new[] { Tool("a"), Tool("b") };
        var second = new[] { Tool("b", "{\"properties\":{},\"type\":\"object\"}"), Tool("a") };
        Assert.Equal(McpRuntimeIdentity.CatalogFingerprint(first), McpRuntimeIdentity.CatalogFingerprint(second));
    }

    [Fact]
    public void OptionsAndPresentationAffectCatalogButNotLoadedBuild()
    {
        var raw = NativeTools.Catalog();
        var readOnly = McpRuntimeIdentity.Create(raw.Select(ResponsePresentation.Describe));
        var enabled = McpRuntimeIdentity.Create(NativeTools.Catalog(NativeToolOptions.AllEnabled).Select(ResponsePresentation.Describe));
        Assert.Equal(readOnly.BuildId, enabled.BuildId);
        Assert.NotEqual(readOnly.CatalogId, enabled.CatalogId);
        Assert.NotEqual(readOnly.CatalogId, McpRuntimeIdentity.CatalogFingerprint(NativeTools.Catalog()));
        Assert.Equal("not_observed", enabled.ClientCatalogState);
        Assert.Matches("^sha256:[0-9a-f]{64}$", enabled.BuildId);
        Assert.Matches("^sha256:[0-9a-f]{64}$", enabled.CatalogId);
    }

    [Fact]
    public async Task StatusReportsSameIdentityAsEffectiveCatalogAndKeepsItInBothPresentations()
    {
        using var client = new NativeClient(new(8081, new string('a', 64)), new SnapshotHandler());
        using var native = new NativeTools(client, NativeToolOptions.AllEnabled);
        var expected = McpRuntimeIdentity.Create(NativeTools.Catalog(NativeToolOptions.AllEnabled).Select(ResponsePresentation.Describe));
        var result = await native.Invoke("timberborn_status", JsonSerializer.SerializeToElement(new { }), TestContext.Current.CancellationToken);
        Assert.Equal("ok", result["status"]!.GetValue<string>());
        Assert.Equal("0.74.0", result["data"]!["bridgeVersion"]!.GetValue<string>());
        foreach (bool full in new[] { false, true })
        {
            var shown = ResponsePresentation.Present("timberborn_status", result, full);
            Assert.Equal(expected, shown["data"]!["mcp"]!.Deserialize<NativeMcpIdentity>(NativeJson.Options));
        }
        var again = await native.Invoke("timberborn_status", JsonSerializer.SerializeToElement(new { }), TestContext.Current.CancellationToken);
        Assert.Equal(result["data"]!["mcp"]!.ToJsonString(), again["data"]!["mcp"]!.ToJsonString());
        Assert.DoesNotContain("session", JsonSerializer.Serialize(expected, NativeJson.Options), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("8081", JsonSerializer.Serialize(expected, NativeJson.Options));
    }

    private sealed class SnapshotHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/agent-api/v1/snapshot", request.RequestUri!.AbsolutePath);
            var snapshot = new NativeSnapshot("global", new[] { "Berries", "Log", "Water" }
                .Select(id => new NativeResource(id, true, 0, 0, 0, 0, 0, 0)).ToArray(),
                new(1, 0, 0, 1), new(1, 0, 0), new(new(1, 0, 0, 0, 1), new(0, 0, 0, 0, 0)),
                new(64, 64, 16), 0, [], false, []);
            var payload = new BridgeEnvelope<NativeSnapshot>(1, "11111111-1111-4111-8111-111111111111",
                DateTimeOffset.UnixEpoch, "0.74.0", snapshot);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(JsonSerializer.Serialize(payload, NativeJson.Options)) });
        }
    }
}
