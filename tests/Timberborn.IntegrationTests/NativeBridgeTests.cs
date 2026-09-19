using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using ModelContextProtocol.Client;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class NativeBridgeTests
{
    [Fact]
    public async Task AuthenticatedBridgeThroughRealStdioReturnsSnapshotAndMapWithoutWriteTools()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var ct = timeout.Token;
        using var reservation = new TcpListener(IPAddress.Loopback, 0);
        reservation.Start(); int port = ((IPEndPoint)reservation.LocalEndpoint).Port; reservation.Stop();
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        using var queue = new MainThreadQueue();
        using var bridge = new BridgeHttpServer(port, token, queue);
        bridge.Start();
        int observations = 0;
        var session = Guid.NewGuid().ToString("D");
        using var pumpStop = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var pump = Task.Run(async () =>
        {
            try
            {
                while (!pumpStop.IsCancellationRequested)
                {
                    queue.Pump(r =>
                    {
                        Interlocked.Increment(ref observations);
                        object data = r.Route == "snapshot" ? Snapshot() : new NativeMap(new(r.X, r.Y, r.Z), 1, 1, 1,
                            [new(r.X, r.Y, r.Z, false, true, 2, 0.5f, 0, true)], ["synthetic_test"]);
                        return JsonSerializer.Serialize(new BridgeEnvelope<object>(1, session, DateTimeOffset.UtcNow, "0.2.0", data), NativeJson.Options);
                    });
                    await Task.Delay(5, pumpStop.Token);
                }
            }
            catch (OperationCanceledException) when (pumpStop.IsCancellationRequested) { }
        }, ct);
        var configPath = Path.Combine(StdioServerTests.Root, ".local", $"native-test-{Guid.NewGuid():N}.local.json");
        try
        {
            using var http = new HttpClient(new SocketsHttpHandler { UseProxy = false });
            var url = $"http://localhost:{port}/agent-api/v1/snapshot";
            using var unauth = await http.GetAsync(url, ct);
            Assert.Equal(HttpStatusCode.Unauthorized, unauth.StatusCode);
            http.DefaultRequestHeaders.Authorization = new("Bearer", token);
            using var post = await http.PostAsync(url, new StringContent("{}"), ct);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, post.StatusCode);
            using var originRequest = new HttpRequestMessage(HttpMethod.Get, url);
            originRequest.Headers.Add("Origin", "https://example.invalid");
            using var origin = await http.SendAsync(originRequest, ct);
            Assert.Equal(HttpStatusCode.Forbidden, origin.StatusCode);
            Assert.Equal(0, Volatile.Read(ref observations));

            await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(new NativeConfiguration(port, token), NativeJson.Options), ct);
            await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
            {
                Command = "dotnet", WorkingDirectory = StdioServerTests.Root,
                Arguments = [Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")],
                EnvironmentVariables = new Dictionary<string, string?>
                {
                    ["TIMBERBORN_BACKEND"] = "native", ["TIMBERBORN_NATIVE_CONFIG"] = configPath,
                    ["TIMBERBORN_ENABLE_WRITES"] = "1"
                }
            }), cancellationToken: ct);
            var tools = await client.ListToolsAsync(cancellationToken: ct);
            Assert.Equal(new[] { "inspect_colony", "inspect_map_region", "timberborn_status" }, tools.Select(t => t.Name).Order());
            Assert.All(tools, t => { Assert.True(t.ProtocolTool.Annotations!.ReadOnlyHint); Assert.NotNull(t.ProtocolTool.OutputSchema); });
            var colony = await client.CallToolAsync("inspect_colony", cancellationToken: ct);
            Assert.False(colony.IsError);
            var json = colony.StructuredContent!.Value;
            Assert.Equal(6, json.GetProperty("data").GetProperty("housing").GetProperty("totalBeds").GetInt32());
            Assert.Equal(session, json.GetProperty("meta").GetProperty("sessionId").GetString());
            Assert.Equal("native", json.GetProperty("meta").GetProperty("backend").GetString());
            var map = await client.CallToolAsync("inspect_map_region", new Dictionary<string, object?>
                { ["x"] = 1, ["y"] = 2, ["z"] = 3, ["width"] = 1, ["height"] = 1, ["depth"] = 1 }, cancellationToken: ct);
            Assert.False(map.IsError);
            Assert.Equal(0.5f, map.StructuredContent!.Value.GetProperty("data").GetProperty("cells")[0].GetProperty("waterDepth").GetSingle());
            var status = await client.CallToolAsync("timberborn_status", cancellationToken: ct);
            Assert.False(status.StructuredContent!.Value.GetProperty("data").GetProperty("writesEnabled").GetBoolean());
            Assert.Equal(3, Volatile.Read(ref observations));
        }
        finally
        {
            pumpStop.Cancel(); await pump;
            if (File.Exists(configPath)) File.Delete(configPath);
        }
    }

    private static NativeSnapshot Snapshot() => new("global",
        [new("Water", true, 10, 10, 10, 0, 20, 20), new("Berries", true, 4, 4, 4, 0, 20, 20), new("Log", true, 2, 2, 2, 0, 20, 20)],
        new(4, 2, 0, 6), new(4, 0, 0), new(new(4, 0, 1, 3, 0), new(0, 0, 0, 0, 0)),
        new(32, 32, 16), 0, [], false, ["synthetic_test"]);
}
