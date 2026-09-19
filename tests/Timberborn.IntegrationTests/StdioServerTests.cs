using ModelContextProtocol.Client;
using System.Text.Json;
using System.Diagnostics;
using ModelContextProtocol.Protocol;
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
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content));
        Assert.True(JsonElement.DeepEquals(json, JsonDocument.Parse(text.Text).RootElement));
        foreach (var tool in tools)
        {
            Assert.NotNull(tool.ProtocolTool.OutputSchema);
            Assert.False(tool.ProtocolTool.InputSchema.GetProperty("additionalProperties").GetBoolean());
            Assert.True(tool.ProtocolTool.Annotations!.ReadOnlyHint);
        }
        var invalid = await client.CallToolAsync("inspect_population", new Dictionary<string, object?> { ["limit"] = 3 }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(invalid.IsError);
        Assert.Equal("invalid_argument", invalid.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString());
    }
    [Fact]
    public async Task ServerExitsOnStdinEofWithoutProtocolNoise()
    {
        var psi = new ProcessStartInfo("dotnet")
        { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        psi.ArgumentList.Add(Path.Combine(Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll"));
        psi.Environment["TIMBERBORN_BACKEND"] = "fake";
        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.StandardInput.Close();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try { await process.WaitForExitAsync(timeout.Token); }
        finally { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        Assert.Equal(0, process.ExitCode);
        Assert.Equal("", await stdout);
        await stderr;
    }
}
