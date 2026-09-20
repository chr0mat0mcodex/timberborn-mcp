using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class PlacementTests
{
    [Theory]
    [InlineData("GET", true, false, false)]
    [InlineData("POST", false, false, false)]
    [InlineData("POST", true, true, false)]
    [InlineData("POST", true, false, true)]
    public void LodgeRequiresSeparateBodylessPost(string method, bool enabled, bool body, bool allowed) =>
        Assert.Equal(allowed, BridgeHttpServer.MethodAllowed(method, "/agent-api/v1/lodge-placement", body, true, true, enabled));

    [Theory]
    [InlineData(false, "Lodge.Folktails", true)]
    [InlineData(true, "Path", true)]
    [InlineData(true, "Lodge.Folktails", false)]
    public async Task LodgeRejectsDisabledWrongTemplateOrStaleFormat(bool enabled, string template, bool validSession)
    {
        var handler = new Handler("{}");
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), true, true, enabled);
        var result = await tools.Invoke("place_lodge", Args(validSession ? Guid.NewGuid().ToString("D") : "bad", template), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0, handler.Calls);
        Assert.DoesNotContain(NativeTools.Catalog(true, true), t => t.Name == "place_lodge");
    }

    [Theory]
    [InlineData("Path", false)]
    [InlineData("Lodge.Folktails", true)]
    public async Task LodgeReceiptMustMatchAndCanRemainUnfinished(string template, bool valid)
    {
        var session = Guid.NewGuid().ToString("D");
        var response = new BridgeEnvelope<NativePlacement>(1, session, DateTimeOffset.UtcNow, "0.7.0",
            new(template, new(1, 2, 3), 0, Guid.NewGuid(), "applied", false, true, ["synthetic_test"]));
        var handler = new Handler(JsonSerializer.Serialize(response, NativeJson.Options));
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), false, false, true);
        var result = await tools.Invoke("place_lodge", Args(session, "Lodge.Folktails"), TestContext.Current.CancellationToken);
        Assert.Equal(valid ? "ok" : "error", result["status"]!.GetValue<string>());
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("GET", true, false, false)]
    [InlineData("POST", false, false, false)]
    [InlineData("POST", true, true, false)]
    [InlineData("POST", true, false, true)]
    public void PlacementRequiresOwnOptInAndBodylessPost(string method, bool enabled, bool body, bool allowed) =>
        Assert.Equal(allowed, BridgeHttpServer.MethodAllowed(method, "/agent-api/v1/path-placement", body, true, enabled));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GateNeverRetriesAfterSuccessOrException(bool fail)
    {
        var gate = new SinglePlacementGate(); int calls = 0;
        int Place() { calls++; if (fail) throw new IOException(); return 1; }
        if (fail) Assert.Throws<IOException>(() => gate.Execute(Place));
        else Assert.Equal(1, gate.Execute(Place));
        Assert.Throws<InvalidOperationException>(() => gate.Execute(Place));
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(false, "Path", true)]
    [InlineData(true, "Lodge.Folktails", true)]
    [InlineData(true, "Path", false)]
    public async Task DisabledOrInvalidPlacementNeverCallsTransport(bool enabled, string template, bool validSession)
    {
        var handler = new Handler("{}");
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), false, enabled);
        var result = await tools.Invoke("place_path", Args(validSession ? Guid.NewGuid().ToString("D") : "bad", template), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0, handler.Calls);
        Assert.DoesNotContain(NativeTools.Catalog(), t => t.Name == "place_path");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResponseMustMatchSessionAndCannotTriggerRetry(bool mismatch)
    {
        var session = Guid.NewGuid().ToString("D");
        var response = new BridgeEnvelope<NativePlacement>(1, mismatch ? Guid.NewGuid().ToString("D") : session,
            DateTimeOffset.UtcNow, "0.5.0", new("Path", new(1, 2, 3), 0, Guid.NewGuid(), "applied", true, true, ["test"]));
        var handler = new Handler(JsonSerializer.Serialize(response, NativeJson.Options));
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), false, true);
        var result = await tools.Invoke("place_path", Args(session), TestContext.Current.CancellationToken);
        Assert.Equal(mismatch ? "error" : "ok", result["status"]!.GetValue<string>());
        if (mismatch) Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task TransportFailureIsNeverRetryable()
    {
        var handler = new Handler(null);
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), false, true);
        var result = await tools.Invoke("place_path", Args(Guid.NewGuid().ToString("D")), TestContext.Current.CancellationToken);
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1, handler.Calls);
    }
    private static JsonElement Args(string session, string template = "Path") =>
        JsonSerializer.SerializeToElement(new { template, x = 1, y = 2, z = 3, rotation = 0, session });
    private sealed class Handler(string? json) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++; Assert.Equal(HttpMethod.Post, request.Method); Assert.Null(request.Content);
            if (json is null) throw new HttpRequestException();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        }
    }
}
