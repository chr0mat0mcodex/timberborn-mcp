using ModelContextProtocol.Client;
using System.Text.Json;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class StdioServerTests
{
    public static string Root
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TimberbornMcp.slnx"))) dir = dir.Parent;
            return dir?.FullName ?? throw new InvalidOperationException("Repository not found");
        }
    }
    [Fact]
    public async Task RealStdioHandshakeListsFiveToolsAndReturnsFakeCounts()
    {
        var dll = Path.Combine(Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll");
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Command = "dotnet", Arguments = [dll], WorkingDirectory = Root,
            EnvironmentVariables = new Dictionary<string, string?> { ["TIMBERBORN_BACKEND"] = "fake" }
        }), cancellationToken: TestContext.Current.CancellationToken);
        var tools = await client.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(5, tools.Count);
        var result = await client.CallToolAsync("inspect_population", cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(result.IsError);
        var json = JsonSerializer.SerializeToElement(result.StructuredContent);
        Assert.Equal(17, json.GetProperty("data").GetProperty("counts").GetProperty("totalEntities").GetInt32());
        Assert.True(json.GetProperty("meta").GetProperty("simulated").GetBoolean());
    }
}
