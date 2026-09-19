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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AuthenticatedBridgeThroughRealStdioReturnsSnapshotAndMapWithoutWriteTools(bool enableValidation)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var ct = timeout.Token;
        using var reservation = new TcpListener(IPAddress.Loopback, 0);
        reservation.Start(); int port = ((IPEndPoint)reservation.LocalEndpoint).Port; reservation.Stop();
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        using var queue = new MainThreadQueue();
        using var bridge = new BridgeHttpServer(port, token, queue, enableValidation);
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
                        object data = r.Route switch
                        {
                            "snapshot" => Snapshot(),
                            "map" => new NativeMap(new(r.X, r.Y, r.Z), 1, 1, 1,
                                [new(r.X, r.Y, r.Z, false, true, 2, 0.5f, 0, true)], ["synthetic_test"]),
                            "objects" => new NativeObjects("buildings_and_paths", r.Offset, r.Limit, 0, [], false, ["synthetic_test"]),
                            "catalog" => new NativeCatalog("Folktails",
                                [new("Lodge.Folktails", true, true, true, new(2, 2, 1), new(1, -1, 0), [new("Log", 12, 0)]),
                                 new("Path", true, true, true, new(1, 1, 1), null, [])], ["synthetic_test"]),
                            "site-precheck" => new NativeSite(r.Template, new(r.X, r.Y, r.Z), r.Rotation,
                                "requires_game_validation", false, [], [new(new(r.X, r.Y, r.Z), true, false, true, false, "Ground")],
                                null, null, [], ["not_full_game_validator"]),
                            "site-validation" => new NativeValidation(r.Template, new(r.X, r.Y, r.Z), r.Rotation,
                                true, true, true, false, 7, ["synthetic_test"]),
                            _ => throw new ArgumentException()
                        };
                        return JsonSerializer.Serialize(new BridgeEnvelope<object>(1, session, DateTimeOffset.UtcNow, "0.4.0", data), NativeJson.Options);
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
            using var forbiddenGet = await http.GetAsync($"http://localhost:{port}/agent-api/v1/site-validation", ct);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, forbiddenGet.StatusCode);
            if (!enableValidation)
            {
                using var denied = await http.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{port}/agent-api/v1/site-validation"), ct);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, denied.StatusCode);
            }
            Assert.Equal(0, Volatile.Read(ref observations));

            await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(new NativeConfiguration(port, token), NativeJson.Options), ct);
            await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
            {
                Command = "dotnet", WorkingDirectory = StdioServerTests.Root,
                Arguments = [Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")],
                EnvironmentVariables = new Dictionary<string, string?>
                {
                    ["TIMBERBORN_BACKEND"] = "native", ["TIMBERBORN_NATIVE_CONFIG"] = configPath,
                    ["TIMBERBORN_ENABLE_WRITES"] = "1",
                    ["TIMBERBORN_ENABLE_VALIDATION"] = enableValidation ? "1" : "0"
                }
            }), cancellationToken: ct);
            var tools = await client.ListToolsAsync(cancellationToken: ct);
            var expected = new List<string> { "find_buildings", "inspect_build_catalog", "inspect_colony", "inspect_map_region", "precheck_build_site", "timberborn_status" };
            if (enableValidation) expected.Add("validate_build_site");
            Assert.Equal(expected, tools.Select(t => t.Name).Order());
            Assert.All(tools, t => { Assert.Equal(t.Name != "validate_build_site", t.ProtocolTool.Annotations!.ReadOnlyHint); Assert.NotNull(t.ProtocolTool.OutputSchema); });
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
            var objects = await client.CallToolAsync("find_buildings", new Dictionary<string, object?> { ["offset"] = 0, ["limit"] = 32 }, cancellationToken: ct);
            Assert.False(objects.IsError);
            Assert.Equal(0, objects.StructuredContent!.Value.GetProperty("data").GetProperty("total").GetInt32());
            var catalog = await client.CallToolAsync("inspect_build_catalog", cancellationToken: ct);
            Assert.False(catalog.IsError);
            Assert.Equal(12, catalog.StructuredContent!.Value.GetProperty("data").GetProperty("items")[0].GetProperty("costs")[0].GetProperty("required").GetInt32());
            var site = await client.CallToolAsync("precheck_build_site", new Dictionary<string, object?>
                { ["template"] = "Path", ["x"] = 1, ["y"] = 2, ["z"] = 3, ["rotation"] = 0 }, cancellationToken: ct);
            Assert.False(site.IsError);
            Assert.False(site.StructuredContent!.Value.GetProperty("data").GetProperty("gameValidated").GetBoolean());
            Assert.Equal("requires_game_validation", site.StructuredContent!.Value.GetProperty("data").GetProperty("assessment").GetString());
            Assert.Equal(6, Volatile.Read(ref observations));
            if (enableValidation)
            {
                var validation = await client.CallToolAsync("validate_build_site", new Dictionary<string, object?>
                    { ["template"] = "Path", ["x"] = 1, ["y"] = 2, ["z"] = 3, ["rotation"] = 0, ["session"] = session }, cancellationToken: ct);
                Assert.False(validation.IsError);
                Assert.True(validation.StructuredContent!.Value.GetProperty("data").GetProperty("noPersistentChangeObserved").GetBoolean());
                Assert.Equal(7, Volatile.Read(ref observations));
            }
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
