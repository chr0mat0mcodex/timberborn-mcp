using ModelContextProtocol.Client;
using System.Text.Json;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class LiveSmokeTests(ITestOutputHelper output)
{
    public static bool LiveEnabled => Environment.GetEnvironmentVariable("TIMBERBORN_LIVE_TEST") == "1";

    [Fact(Skip = "Live-Test nur nach explizitem Opt-in.", SkipUnless = nameof(LiveEnabled))]
    public async Task FiveReadToolsAgainstUserPreparedColony()
    {
        var dll = Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll");
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Command = "dotnet", Arguments = [dll], WorkingDirectory = StdioServerTests.Root,
            EnvironmentVariables = new Dictionary<string, string?> { ["TIMBERBORN_BACKEND"] = "more-http-api" }
        }), cancellationToken: TestContext.Current.CancellationToken);
        async Task<JsonElement> Call(string name, Dictionary<string, object?>? args = null)
        {
            var result = await client.CallToolAsync(name, args, cancellationToken: TestContext.Current.CancellationToken);
            Assert.False(result.IsError);
            var root = result.StructuredContent!.Value;
            Assert.Equal("ok", root.GetProperty("status").GetString());
            Assert.False(root.GetProperty("meta").GetProperty("simulated").GetBoolean());
            return root.GetProperty("data");
        }
        var status = await Call("timberborn_status");
        Assert.Equal("reachable", status.GetProperty("connection").GetString());
        var colony = await Call("inspect_colony");
        var population = await Call("inspect_population");
        var buildings = await Call("find_buildings", new() { ["limit"] = 1 });
        Assert.True(buildings.GetProperty("page").GetProperty("matchedTotal").GetInt32() > 0);
        var id = buildings.GetProperty("items")[0].GetProperty("id").GetString()!;
        var single = await Call("inspect_building", new() { ["id"] = id });
        Assert.Equal(id, single.GetProperty("id").GetString());
        output.WriteLine("Live MCP: all five read tools succeeded. Backend version: " + status.GetProperty("backendVersion"));
        output.WriteLine("Time: " + colony.GetProperty("time"));
        output.WriteLine("Population: " + population.GetProperty("counts"));
        output.WriteLine("Buildings: " + buildings.GetProperty("page").GetProperty("matchedTotal"));
    }
}
