using System.Diagnostics;
using ModelContextProtocol.Client;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class BackendSelectionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("native")]
    public async Task MissingSelectionOrNativeConfigCannotFallBack(string? backend)
    {
        var start = new ProcessStartInfo("dotnet") {
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add(Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll"));
        if (backend is null) start.Environment.Remove("TIMBERBORN_BACKEND");
        else start.Environment["TIMBERBORN_BACKEND"] = backend;
        // Even otherwise available mode settings must not select a backend implicitly.
        start.Environment["TIMBERBORN_FAKE_SCENARIO"] = "healthy";
        start.Environment.Remove("TIMBERBORN_NATIVE_CONFIG");
        using var process = Process.Start(start)!;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        // Keep stdin open: exit must not depend on receiving EOF or an MCP request.
        try { await process.WaitForExitAsync(timeout.Token); }
        finally { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        Assert.Equal("", await stdout);
        if (backend == "native") {
            Assert.NotEqual(0, process.ExitCode);
            Assert.Contains("TIMBERBORN_NATIVE_CONFIG fehlt.", await stderr);
        } else {
            Assert.Equal(2, process.ExitCode);
            Assert.Equal("TIMBERBORN_BACKEND fehlt: explizit native, more-http-api (Legacy) oder fake setzen. Kein automatischer Fallback.", (await stderr).Trim());
        }
    }

    [Fact]
    public async Task ExplicitLegacyStillSupportsStdioDiscoveryWithoutGameAccess()
    {
        var dll = Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new() {
            Command = "dotnet", Arguments = [dll], WorkingDirectory = StdioServerTests.Root,
            EnvironmentVariables = new Dictionary<string, string?> {
                ["TIMBERBORN_BACKEND"] = "more-http-api", ["TIMBERBORN_ENABLE_WRITES"] = "0",
                ["TIMBERBORN_FAKE_SCENARIO"] = "healthy", ["TIMBERBORN_BASE_URL"] = "http://127.0.0.1:1/",
                ["TIMBERBORN_AUTHORIZATION"] = null }
        }), cancellationToken: timeout.Token);
        var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
        Assert.Equal(5, tools.Count);
        Assert.Contains(tools, t => t.Name == "inspect_population");
        // Discovery is local: no legacy game-service request is made.
    }
}
