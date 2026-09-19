using ModelContextProtocol.Client;
using ModelContextProtocol;
using Timberborn.Backend.Fake;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class WriteStdioTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WriteToolRequiresOptInAndVerifiesBothTransitions(bool enabled)
    {
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Command = "dotnet", Arguments = [Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")],
            WorkingDirectory = StdioServerTests.Root,
            EnvironmentVariables = new Dictionary<string, string?> { ["TIMBERBORN_BACKEND"] = "fake", ["TIMBERBORN_ENABLE_WRITES"] = enabled ? "1" : "0" }
        }), cancellationToken: TestContext.Current.CancellationToken);
        var tools = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(enabled ? 6 : 5, tools.Count);
        var args = new Dictionary<string, object?> { ["id"] = FakeTimberbornBackend.BuildingId.ToString(), ["paused"] = true, ["expectedPaused"] = false };
        if (!enabled)
        {
            await Assert.ThrowsAsync<McpProtocolException>(() => client.CallToolAsync("set_building_paused", args, cancellationToken: TestContext.Current.CancellationToken).AsTask());
            return;
        }
        var tool = tools.Single(t => t.Name == "set_building_paused").ProtocolTool;
        Assert.False(tool.Annotations!.ReadOnlyHint); Assert.True(tool.Annotations.DestructiveHint);
        Assert.False(tool.Annotations.IdempotentHint);
        Assert.NotNull(tool.OutputSchema); Assert.Equal(3, tool.InputSchema.GetProperty("required").GetArrayLength());
        foreach (bool target in new[] { true, false })
        {
            args["paused"] = target; args["expectedPaused"] = !target;
            var result = await client.CallToolAsync("set_building_paused", args, cancellationToken: TestContext.Current.CancellationToken);
            Assert.False(result.IsError);
            Assert.Equal("applied", result.StructuredContent!.Value.GetProperty("data").GetProperty("outcome").GetString());
            Assert.True(result.StructuredContent.Value.GetProperty("meta").GetProperty("simulated").GetBoolean());
        }
    }
}
