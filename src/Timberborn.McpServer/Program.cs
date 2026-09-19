using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Timberborn.Application;
using Timberborn.Backend.Abstractions;
using Timberborn.Backend.Fake;
using Timberborn.McpServer;

var backendName = Environment.GetEnvironmentVariable("TIMBERBORN_BACKEND") ?? "more-http-api";
var scenario = Environment.GetEnvironmentVariable("TIMBERBORN_FAKE_SCENARIO") ?? "healthy";
if (scenario is not ("healthy" or "offline" or "partial"))
    throw new InvalidOperationException("Unbekanntes Fake-Szenario.");
ITimberbornReadBackend backend = backendName == "fake" ? new FakeTimberbornBackend(scenario)
    : throw new InvalidOperationException("Echtes Backend wird in Paket B ergänzt.");
var service = new ObservationService(backend);
var tools = ToolCatalog.Create();
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = [], DisableDefaults = true });
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Services.AddMcpServer().WithStdioServerTransport()
    .WithListToolsHandler((_, _) => ValueTask.FromResult(new ListToolsResult { Tools = tools.ToList() }))
    .WithCallToolHandler(async (context, ct) =>
    {
        var request = context.Params ?? throw new McpProtocolException("Missing parameters", McpErrorCode.InvalidParams);
        if (!ObservationService.ResultTypes.ContainsKey(request.Name))
            throw new McpProtocolException("Unknown tool", McpErrorCode.InvalidParams);
        var arguments = JsonSerializer.SerializeToElement(request.Arguments ?? new Dictionary<string, JsonElement>());
        var result = await service.InvokeAsync(request.Name, arguments, ct);
        return new CallToolResult
        {
            StructuredContent = JsonSerializer.SerializeToElement(result),
            Content = [new TextContentBlock { Text = result.ToJsonString() }],
            IsError = result["status"]!.GetValue<string>() == "error"
        };
    });
await builder.Build().RunAsync();
