using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class BridgeTests
{
    private static BridgeRequest Snapshot() => BridgeRequest.Parse("/agent-api/v1/snapshot", new());

    [Fact]
    public async Task QueueRunsOnlyWhenPumpedAndOnCallingThread()
    {
        using var queue = new MainThreadQueue();
        var result = queue.Enqueue(Snapshot(), TestContext.Current.CancellationToken);
        Assert.False(result.IsCompleted);
        int thread = Environment.CurrentManagedThreadId;
        queue.Pump(_ => { Assert.Equal(thread, Environment.CurrentManagedThreadId); return "observed"; });
        Assert.Equal("observed", await result);
    }

    [Fact]
    public async Task CancelledAndUnloadedRequestsNeverObserveGame()
    {
        using var queue = new MainThreadQueue();
        using var cancelled = new CancellationTokenSource();
        var first = queue.Enqueue(Snapshot(), cancelled.Token);
        cancelled.Cancel();
        queue.Pump(_ => throw new Xunit.Sdk.XunitException("Game accessed after cancellation"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
        var second = queue.Enqueue(Snapshot(), TestContext.Current.CancellationToken);
        queue.Dispose();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second);
        queue.Pump(_ => throw new Xunit.Sdk.XunitException("Game accessed after unload"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.Enqueue(Snapshot(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueueIsBoundedAndSanitizesGameErrors()
    {
        using var queue = new MainThreadQueue();
        var pending = Enumerable.Range(0, 8).Select(_ => queue.Enqueue(Snapshot(), TestContext.Current.CancellationToken)).ToArray();
        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.Enqueue(Snapshot(), TestContext.Current.CancellationToken));
        queue.Pump(_ => throw new Exception("private game data"));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => pending[0]);
        Assert.Equal("observation_failed", error.Message);
        queue.Dispose();
        foreach (var task in pending.Skip(1)) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Theory]
    [InlineData("width", "9")]
    [InlineData("depth", "5")]
    [InlineData("x", "-1")]
    [InlineData("height", "0")]
    [InlineData("x", "4096")]
    public void RejectsUnboundedRegions(string key, string value)
    {
        var query = MapQuery(); query[key] = value;
        Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/map", query));
    }

    [Fact]
    public void RejectsUnknownRoutesExtraAndDuplicateArguments()
    {
        Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/build", new()));
        Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/snapshot", MapQuery()));
        var query = MapQuery(); query.Add("x", "2");
        Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/map", query));
        Assert.Equal(256, BridgeRequest.Parse("/agent-api/v1/map", MapQuery()).Width * 8 * 4);
    }

    private static NameValueCollection MapQuery() => new() { ["x"] = "0", ["y"] = "0", ["z"] = "0", ["width"] = "8", ["height"] = "8", ["depth"] = "4" };

    [Theory]
    [InlineData("{\"schemaVersion\":1}", "backend_incompatible")]
    [InlineData("null", "backend_incompatible")]
    [InlineData("unauthorized", "authentication_failed")]
    [InlineData("oversized", "backend_incompatible")]
    public async Task NativeFailuresRemainStructuredAndDoNotLeakResponse(string payload, string code)
    {
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), new ResponseHandler(payload)));
        var result = await tools.Invoke("inspect_colony", JsonSerializer.SerializeToElement(new { }), TestContext.Current.CancellationToken);
        Assert.Equal("error", result["status"]!.GetValue<string>());
        Assert.Equal(code, result["error"]!["code"]!.GetValue<string>());
        Assert.DoesNotContain("private", result.ToJsonString());
        Assert.DoesNotContain(new string('a', 64), result.ToJsonString());
    }

    [Fact]
    public async Task InvalidMapArgumentsNeverReachHttp()
    {
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), new ResponseHandler("must_not_call")));
        var result = await tools.Invoke("inspect_map_region", JsonSerializer.SerializeToElement(new { x = 0 }), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
    }

    private sealed class ResponseHandler(string payload) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.NotEqual("must_not_call", payload);
            Assert.Equal("localhost", request.RequestUri!.Host);
            Assert.Equal(HttpMethod.Get, request.Method);
            return Task.FromResult(new HttpResponseMessage(payload == "unauthorized" ? HttpStatusCode.Unauthorized : HttpStatusCode.OK)
            { Content = new StringContent(payload == "oversized" ? new string('x', 131073) : payload == "unauthorized" ? "private" : payload) });
        }
    }
}
