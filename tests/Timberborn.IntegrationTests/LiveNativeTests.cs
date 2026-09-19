using ModelContextProtocol.Client;
using System.Text.Json;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class LiveNativeTests(ITestOutputHelper output)
{
    public static bool Enabled => Environment.GetEnvironmentVariable("TIMBERBORN_NATIVE_LIVE_TEST") == "1";

    [Fact(Skip = "Native-Livetest nur nach explizitem Opt-in.", SkipUnless = nameof(Enabled))]
    public async Task SixReadToolsAgainstInstalledNativeBridge()
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(40));
        var ct = budget.Token;
        var config = Environment.GetEnvironmentVariable("TIMBERBORN_NATIVE_CONFIG");
        Assert.False(string.IsNullOrWhiteSpace(config));
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Command = "dotnet", WorkingDirectory = StdioServerTests.Root,
            Arguments = [Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")],
            EnvironmentVariables = new Dictionary<string, string?>
            {
                ["TIMBERBORN_BACKEND"] = "native", ["TIMBERBORN_NATIVE_CONFIG"] = config,
                ["TIMBERBORN_ENABLE_WRITES"] = "0", ["TIMBERBORN_ENABLE_VALIDATION"] = "0"
            }
        }), cancellationToken: ct);
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        Assert.Equal(new[] { "find_buildings", "inspect_build_catalog", "inspect_colony", "inspect_map_region", "precheck_build_site", "timberborn_status" }, tools.Select(t => t.Name).Order());
        string? session = null;
        async Task<JsonElement> Call(string name, Dictionary<string, object?>? args = null)
        {
            var result = await client.CallToolAsync(name, args, cancellationToken: ct);
            Assert.False(result.IsError);
            var root = result.StructuredContent!.Value;
            Assert.Equal("ok", root.GetProperty("status").GetString());
            var meta = root.GetProperty("meta");
            Assert.Equal("native", meta.GetProperty("backend").GetString());
            Assert.False(meta.GetProperty("simulated").GetBoolean());
            var current = meta.GetProperty("sessionId").GetString();
            Assert.True(Guid.TryParse(current, out _));
            session ??= current;
            Assert.Equal(session, current);
            return root.GetProperty("data");
        }
        var status = await Call("timberborn_status");
        Assert.Equal("reachable", status.GetProperty("connection").GetString());
        Assert.False(status.GetProperty("writesEnabled").GetBoolean());
        var colony = await Call("inspect_colony");
        var size = colony.GetProperty("mapSize");
        var objects = await Call("find_buildings", new() { ["offset"] = 0, ["limit"] = 32 });
        var sample = objects.GetProperty("items");
        Assert.NotEqual(0, sample.GetArrayLength());
        var position = sample[0].GetProperty("position");
        int x = position.GetProperty("x").GetInt32();
        int y = position.GetProperty("y").GetInt32();
        int z = position.GetProperty("z").GetInt32();
        Assert.InRange(x, 0, size.GetProperty("x").GetInt32() - 1);
        Assert.InRange(y, 0, size.GetProperty("y").GetInt32() - 1);
        Assert.InRange(z, 0, size.GetProperty("z").GetInt32() - 1);
        var map = await Call("inspect_map_region", new()
        { ["x"] = x, ["y"] = y, ["z"] = z, ["width"] = 1, ["height"] = 1, ["depth"] = 1 });
        Assert.Equal(1, map.GetProperty("cells").GetArrayLength());
        var catalog = await Call("inspect_build_catalog");
        Assert.Equal(2, catalog.GetProperty("items").GetArrayLength());
        var site = await Call("precheck_build_site", new()
        { ["template"] = "Lodge.Folktails", ["x"] = x, ["y"] = y, ["z"] = z, ["rotation"] = 0 });
        Assert.False(site.GetProperty("gameValidated").GetBoolean());
        output.WriteLine("Native MCP: six read tools succeeded in one game session, including spatial precheck without placement or preview creation.");
        // Only aggregate diagnostics; no names, entity/session IDs, paths or tokens in test output.
        output.WriteLine("Population: " + colony.GetProperty("population"));
        output.WriteLine("Housing: " + colony.GetProperty("housing"));
        output.WriteLine("Map size: " + size);
    }
}
