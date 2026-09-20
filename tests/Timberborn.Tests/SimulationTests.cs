using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class SimulationTests
{
    [Theory]
    [InlineData("POST", false, false, false)]
    [InlineData("GET", true, false, false)]
    [InlineData("POST", true, true, false)]
    [InlineData("POST", true, false, true)]
    public void SeparateOptIn(string method, bool enabled, bool body, bool allowed) =>
        Assert.Equal(allowed, BridgeHttpServer.MethodAllowed(method, "/agent-api/v1/simulation-speed", body, true, true, true, enabled));

    [Theory]
    [InlineData(false, 0, 1, true)]
    [InlineData(true, 2, 1, true)]
    [InlineData(true, 0, 2, true)]
    [InlineData(true, 0, 1, false)]
    public async Task InvalidCommandNeverReachesTransport(bool enabled, int speed, int expected, bool validSession)
    {
        var handler = new Handler(null);
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), true, true, true, enabled);
        var result = await tools.Invoke("set_simulation_speed", Args(speed, expected, validSession ? Guid.NewGuid().ToString("D") : "bad"), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0, handler.Calls);
        Assert.DoesNotContain(NativeTools.Catalog(true, true, true), t => t.Name == "set_simulation_speed");
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReceiptMustMatchSession(bool mismatch)
    {
        var session = Guid.NewGuid().ToString("D");
        var envelope = new BridgeEnvelope<NativeSpeedResult>(1, mismatch ? Guid.NewGuid().ToString("D") : session, DateTimeOffset.UtcNow,
            "0.8.0", new(0, 1, true, new(0, 1, 0.5f, 12), ["synthetic_test"]));
        var handler = new Handler(JsonSerializer.Serialize(envelope, NativeJson.Options));
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), enableSpeedControl: true);
        var result = await tools.Invoke("set_simulation_speed", Args(0, 1, session), TestContext.Current.CancellationToken);
        Assert.Equal(mismatch ? "error" : "ok", result["status"]!.GetValue<string>());
        if (mismatch) Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1, handler.Calls);
    }
    [Fact]
    public async Task TransportFailureNeverRetries()
    {
        var handler = new Handler(null);
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), enableSpeedControl: true);
        var result = await tools.Invoke("set_simulation_speed", Args(0, 1, Guid.NewGuid().ToString("D")), TestContext.Current.CancellationToken);
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1, handler.Calls);
    }
    private static JsonElement Args(int speed, int expectedSpeed, string session) => JsonSerializer.SerializeToElement(new { speed, expectedSpeed, session });
    private sealed class Handler(string? json) : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++; Assert.Equal(HttpMethod.Post, request.Method); Assert.Null(request.Content);
            Assert.Equal("/agent-api/v1/simulation-speed", request.RequestUri!.AbsolutePath);
            if (json is null) throw new HttpRequestException();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        }
    }
}
