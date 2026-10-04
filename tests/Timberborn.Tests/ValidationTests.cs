using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ValidationTests
{
    [Theory]
    [InlineData("GET", false, false)]
    [InlineData("GET", true, false)]
    [InlineData("POST", false, false)]
    [InlineData("POST", true, true)]
    [InlineData("DELETE", true, false)]
    public void ValidationRequiresEnabledPost(string method, bool enabled, bool allowed)
    {
        Assert.Equal(allowed, BridgeHttpServer.MethodAllowed(method, "/agent-api/v1/site-validation", false, enabled));
        Assert.False(BridgeHttpServer.MethodAllowed(method, "/agent-api/v1/site-validation", true, enabled));
    }

    [Fact]
    public async Task DisabledToolNeverCallsTransport()
    {
        var handler = new ValidationHandler("{}");
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler));
        var result = await tools.Invoke("validate_build_site", Args(Guid.NewGuid().ToString("D")), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0, handler.Calls);
        Assert.DoesNotContain(NativeTools.Catalog(), t => t.Name == "validate_build_site");
    }

    [Theory]
    [InlineData("not-a-session")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task InvalidSessionNeverCallsTransport(string session)
    {
        var handler = new ValidationHandler("{}");
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), true);
        var result = await tools.Invoke("validate_build_site", Args(session), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task MismatchedSessionRejectedWithoutRetry()
    {
        var response = new BridgeEnvelope<NativeValidation>(1, Guid.NewGuid().ToString("D"), DateTimeOffset.UtcNow, "0.4.0",
            new("Path", new(1, 2, 3), 0, true, true, true, false, 7, ["test"]));
        var handler = new ValidationHandler(JsonSerializer.Serialize(response, NativeJson.Options));
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), true);
        var result = await tools.Invoke("validate_build_site", Args(Guid.NewGuid().ToString("D")), TestContext.Current.CancellationToken);
        Assert.Equal("backend_incompatible", result["error"]!["code"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1, handler.Calls);
    }

    private static JsonElement Args(string session) => JsonSerializer.SerializeToElement(new { template = "Path", x = 1, y = 2, z = 3, rotation = 0, session });
    [Theory]
    [InlineData("validate_build_site", 999999, true)]
    [InlineData("validate_building", 999999, true)]
    [InlineData("validate_build_site", 0, true)]
    [InlineData("validate_building", 0, true)]
    [InlineData("validate_build_site", 1000000, false)]
    [InlineData("validate_building", 1000000, false)]
    [InlineData("validate_build_site", -1, false)]
    [InlineData("validate_building", -1, false)]
    public async Task ExpandedSingleBudgetDoesNotBecomeBuildPermission(string tool, int remaining, bool accepted)
    {
        const string session = "11111111-1111-1111-1111-111111111111";
        var construction = new NativeConstructionPreview("unknown", "no_existing_construction_sites",
            null, null, 0, [], [], new("unknown", "no_existing_construction_sites", null, null, null, [], []));
        var roads = new NativeRoadProtection("unknown", ["construction_and_road_node_coverage_unproven"],
            4, 0, true, false, [], false, [], [], 2, 1, 0, construction);
        var response = new BridgeEnvelope<NativeValidation>(1, session, DateTimeOffset.UtcNow, "0.35.1",
            new("Path", new(1, 2, 3), 0, true, true, true, false, remaining, [], roads));
        var handler = new ValidationHandler(JsonSerializer.Serialize(response, NativeJson.Options));
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler),
            enableValidation: true, enableBuildingPlacement: true);
        var result = await tools.Invoke(tool, Args(session), TestContext.Current.CancellationToken);
        Assert.Equal(accepted ? "ok" : "error", result["status"]!.GetValue<string>());
        Assert.Equal(1, handler.Calls);
        if (accepted) {
            Assert.Equal(remaining, result["data"]!["attemptsRemaining"]!.GetValue<int>());
            Assert.False(result["data"]!["assessment"]!["regularExecutionAllowed"]!.GetValue<bool>());
        } else Assert.Equal("backend_incompatible", result["error"]!["code"]!.GetValue<string>());
    }
    private sealed class ValidationHandler(string payload) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Null(request.Content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload) });
        }
    }
}
