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
    [InlineData(0, true)][InlineData(7, true)][InlineData(2, false)]
    public async Task QueuedSpeedReceiptRetainsOwnerAndValidatesPendingCommand(int pending, bool valid)
    {
        const string session = "11111111-1111-1111-1111-111111111111";
        const string run = "22222222-2222-2222-2222-222222222222";
        var payload = new BridgeEnvelope<NativeSpeedResult>(1, session, DateTimeOffset.UnixEpoch,
            "0.64.0", new(0, 1, false, new(1, 1, .5f, 12, run, pending), ["synthetic"]));
        using var client = new NativeClient(new(8081, new string('a', 64)), new Handler(JsonSerializer.Serialize(payload, NativeJson.Options)));
        var request = BridgeRequest.Parse("/agent-api/v1/simulation-speed", new() {
            ["session"] = session, ["speed"] = "0", ["expectedSpeed"] = "1" });
        if (valid) {
            var result = await client.SetSpeed(request, TestContext.Current.CancellationToken);
            Assert.Equal(run, result.Data.Observation.ActiveRunId);
            Assert.Equal(pending, result.Data.Observation.PendingSpeed);
        } else await Assert.ThrowsAsync<InvalidDataException>(() => client.SetSpeed(request, TestContext.Current.CancellationToken));
    }
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(6, false)]
    [InlineData(7, true)]
    [InlineData(8, false)]
    [InlineData(-1, false)]
    public void OnlyStandardSpeedsAcceptedForTargetAndExpectation(int value, bool valid)
    {
        foreach (var key in new[] { "speed", "expectedSpeed" })
        {
            var query = new System.Collections.Specialized.NameValueCollection {
                ["speed"] = "1", ["expectedSpeed"] = "1", ["session"] = Guid.NewGuid().ToString("D") };
            query[key] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (valid) {
                var request = BridgeRequest.Parse("/agent-api/v1/simulation-speed", query);
                Assert.Equal(value, key == "speed" ? request.Speed : request.ExpectedSpeed);
            } else Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/simulation-speed", query));
        }
    }
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
    [Fact]
    public async Task FreshLockEvidenceSurvivesSpeedReceipt()
    {
        var session=Guid.NewGuid().ToString("D");
        var observation=new NativeSimulation(0,1,0.5f,12,SpeedLocked:false,
            SpeedLockSource:"speed_lock_changed_event",SpeedLockReason:"not_exposed_by_public_api",PauseSettled:true);
        var envelope=new BridgeEnvelope<NativeSpeedResult>(1,session,DateTimeOffset.UtcNow,"0.72.0",new(0,1,true,observation,["synthetic"]));
        var handler=new Handler(JsonSerializer.Serialize(envelope,NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableSpeedControl:true);
        var result=await tools.Invoke("set_simulation_speed",Args(0,1,session),TestContext.Current.CancellationToken);
        Assert.Equal("ok",result["status"]!.GetValue<string>());
        Assert.False(result["data"]!["observation"]!["speedLocked"]!.GetValue<bool>());
        Assert.True(result["data"]!["observation"]!["pauseSettled"]!.GetValue<bool>());
        Assert.Equal(1,handler.Calls);
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
