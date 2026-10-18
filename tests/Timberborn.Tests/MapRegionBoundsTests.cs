using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class MapRegionBoundsTests
{
    private static BridgeRequest Request(int x, int y, int z, int width = 1, int height = 1, int depth = 1) =>
        BridgeRequest.Parse("/agent-api/v1/map", new NameValueCollection {
            ["x"] = x.ToString(), ["y"] = y.ToString(), ["z"] = z.ToString(),
            ["width"] = width.ToString(), ["height"] = height.ToString(), ["depth"] = depth.ToString() });

    [Fact]
    public void ValidOriginAndFarCornerAndRegionEndingAtBoundaryAreAccepted()
    {
        MapRegionBoundsException.Validate(Request(0, 0, 0), 16, 24, 8);
        MapRegionBoundsException.Validate(Request(15, 23, 7), 16, 24, 8);
        MapRegionBoundsException.Validate(Request(8, 16, 4, 8, 8, 4), 16, 24, 8);
    }

    [Theory]
    [InlineData(16, 0, 0, 1, 1, 1, "x", 16)]
    [InlineData(0, 24, 0, 1, 1, 1, "y", 24)]
    [InlineData(0, 0, 8, 1, 1, 1, "z", 8)]
    [InlineData(15, 0, 0, 2, 1, 1, "x", 16)]
    [InlineData(0, 23, 0, 1, 2, 1, "y", 24)]
    [InlineData(0, 0, 7, 1, 1, 2, "z", 8)]
    public void OriginAndExtentFailuresIdentifyActualAxis(int x, int y, int z, int width, int height, int depth, string axis, int size)
    {
        var error = Assert.Throws<MapRegionBoundsException>(() =>
            MapRegionBoundsException.Validate(Request(x, y, z, width, height, depth), 16, 24, 8));
        Assert.Equal(axis, error.Axis);
        Assert.Equal(size, error.Size);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BothQueuePathsPreserveOnlyTypedBounds(bool asynchronous)
    {
        using var queue = new MainThreadQueue();
        var pending = queue.Enqueue(Request(16, 0, 0), TestContext.Current.CancellationToken);
        if (asynchronous) queue.PumpAsync(_ => Task.FromException<string>(new MapRegionBoundsException("x", 16)));
        else queue.Pump(_ => throw new MapRegionBoundsException("x", 16));
        var error = await Assert.ThrowsAsync<MapRegionBoundsException>(() => pending);
        Assert.Equal("x", error.Axis);
        Assert.Equal(16, error.Size);
    }

    [Theory]
    [InlineData("x", 16)]
    [InlineData("y", 24)]
    [InlineData("z", 8)]
    public async Task NativeDiagnosticSurvivesWireFormatWithoutWriteClaim(string axis, int size)
    {
        var handler = new Rejection(new MapRegionBoundsException(axis, size).ToResponseJson());
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler));
        var result = await tools.Invoke("inspect_map_region", JsonSerializer.SerializeToElement(
            new { x = 16, y = 24, z = 8, width = 1, height = 1, depth = 1 }), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal("not_applicable", result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        var message = result["error"]!["message"]!.GetValue<string>();
        Assert.Contains($"Achse {axis}", message);
        Assert.Contains($"0..{size - 1}", message);
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("map", "{\"error\":\"map_region_out_of_bounds\",\"axis\":\"private-value\",\"size\":16}")]
    [InlineData("map", "{\"error\":\"map_region_out_of_bounds\",\"axis\":\"x\",\"size\":0}")]
    [InlineData("map", "{\"error\":\"map_region_out_of_bounds\",\"axis\":\"x\",\"size\":\"16\"}")]
    [InlineData("map", "{\"error\":\"map_region_out_of_bounds\",\"axis\":\"x\",\"size\":16,\"extra\":\"private-value\"}")]
    [InlineData("snapshot", "{\"error\":\"map_region_out_of_bounds\",\"axis\":\"x\",\"size\":16}")]
    public async Task MalformedOrWrongRouteResponsesStayIncompatible(string route, string payload)
    {
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), new Rejection(payload)));
        var args = route == "map" ? JsonSerializer.SerializeToElement(new { x = 16, y = 0, z = 0, width = 1, height = 1, depth = 1 })
            : JsonSerializer.SerializeToElement(new { });
        var result = await tools.Invoke(route == "map" ? "inspect_map_region" : "inspect_colony", args, TestContext.Current.CancellationToken);
        Assert.Equal("backend_incompatible", result["error"]!["code"]!.GetValue<string>());
        Assert.DoesNotContain("private-value", result.ToJsonString());
    }

    private sealed class Rejection(string payload) : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++;
            Assert.Equal(HttpMethod.Get, request.Method);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(payload) });
        }
    }
}
