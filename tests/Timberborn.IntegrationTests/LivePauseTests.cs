using ModelContextProtocol.Client;
using System.Text.Json;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class LivePauseTests(ITestOutputHelper output)
{
    public static bool Enabled => Environment.GetEnvironmentVariable("TIMBERBORN_LIVE_WRITE_TEST") == "1";
    [Fact(Skip = "Schreibpilot nur nach expliziter Freigabe der einzigen Holzfällerflagge und des Rückwegs.", SkipUnless = nameof(Enabled))]
    public async Task OnlyLumberjackFlagPauseAndRestore()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));
        var ct = timeout.Token;
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Command = "dotnet", Arguments = [Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")],
            WorkingDirectory = StdioServerTests.Root,
            EnvironmentVariables = new Dictionary<string, string?>
            { ["TIMBERBORN_BACKEND"] = "more-http-api", ["TIMBERBORN_ENABLE_WRITES"] = "1" }
        }), cancellationToken: ct);
        async Task<JsonElement> Read(string name, Dictionary<string, object?> args)
        {
            var result = await client.CallToolAsync(name, args, cancellationToken: ct);
            Assert.False(result.IsError);
            Assert.False(result.StructuredContent!.Value.GetProperty("meta").GetProperty("simulated").GetBoolean());
            return result.StructuredContent.Value.GetProperty("data");
        }
        var matches = await Read("find_buildings", new() { ["template"] = "LumberjackFlag.Folktails", ["limit"] = 2 });
        Assert.Equal(1, matches.GetProperty("page").GetProperty("matchedTotal").GetInt32());
        var id = matches.GetProperty("items")[0].GetProperty("id").GetString()!;
        var before = await Read("inspect_building", new() { ["id"] = id });
        Assert.True(before.GetProperty("pausable").GetBoolean());
        bool original = before.GetProperty("paused").GetBoolean();
        foreach (bool target in new[] { !original, original })
        {
            var response = await client.CallToolAsync("set_building_paused", new Dictionary<string, object?>
                { ["id"] = id, ["paused"] = target, ["expectedPaused"] = !target }, cancellationToken: ct);
            // A failed or uncertain first action must not trigger an automatic second write.
            Assert.False(response.IsError, "Schreibpilot gestoppt. Zustand nur lesend klären; nicht automatisch wiederholen.");
            var data = response.StructuredContent!.Value.GetProperty("data");
            Assert.Equal("applied", data.GetProperty("outcome").GetString());
            Assert.Equal(target, data.GetProperty("observedPaused").GetBoolean());
            output.WriteLine($"Holzfällerflagge: Paused={target} nach Schreibrequest bestätigt.");
        }
        var final = await Read("inspect_building", new() { ["id"] = id });
        Assert.Equal(original, final.GetProperty("paused").GetBoolean());
        output.WriteLine("Einziges Testgebäude erkannt; ursprünglicher Pausenstatus wiederhergestellt und separat nachgelesen.");
    }
}
