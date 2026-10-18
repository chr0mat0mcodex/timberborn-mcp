using System.Diagnostics;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class McpIdentityReportTests
{
    [Fact]
    public async Task OfflineReportMatchesLoadedModulesAndSupervisorCatalogWithoutConfigOrGame()
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false, CreateNoWindow = true,
        };
        start.ArgumentList.Add(Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll"));
        start.ArgumentList.Add("--print-native-identity");
        start.Environment["TIMBERBORN_BACKEND"] = "invalid_backend_must_not_be_read";
        start.Environment.Remove("TIMBERBORN_NATIVE_CONFIG");
        using var process = Process.Start(start)!;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try { await process.WaitForExitAsync(timeout.Token); }
        finally { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        Assert.Equal(0, process.ExitCode);
        Assert.Equal("", await stderr);
        var report = JsonSerializer.Deserialize<NativeMcpIdentity>(await stdout, NativeJson.Options);
        var expected = McpRuntimeIdentity.Create(NativeTools.Catalog(NativeToolOptions.AllEnabled).Select(ResponsePresentation.Describe));
        Assert.Equal(expected, report);
    }
}
