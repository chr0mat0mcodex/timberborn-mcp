using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Timberborn.Application;
using Timberborn.Backend.Abstractions;
using Timberborn.Backend.Fake;
using Timberborn.Backend.MoreHttpApi;
using Timberborn.Backend.Native;
using Timberborn.McpServer;

var backendName = Environment.GetEnvironmentVariable("TIMBERBORN_BACKEND") ?? "more-http-api";
var scenario = Environment.GetEnvironmentVariable("TIMBERBORN_FAKE_SCENARIO") ?? "healthy";
var writesEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_WRITES") == "1";
var validationEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_VALIDATION") == "1";
var prioritiesEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_PRIORITIES") == "1";
var researchEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_RESEARCH") == "1";
var buildingSettingsEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_BUILDING_SETTINGS") == "1";
var buildingPlacementEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_BUILDING_PLACEMENT") == "1";
var removalEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_REMOVAL") == "1";
var areasEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_AREAS") == "1";
var staffingEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_STAFFING") == "1";
var speedControlEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_SPEED_CONTROL") == "1";
var lodgePlacementEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_LODGE_PLACEMENT") == "1";
var placementEnabled = Environment.GetEnvironmentVariable("TIMBERBORN_ENABLE_PLACEMENT") == "1";
if (scenario is not ("healthy" or "offline" or "partial"))
    throw new InvalidOperationException("Unbekanntes Fake-Szenario.");
ITimberbornReadBackend? backend = backendName switch
{
    "native" => null,
    "fake" => new FakeTimberbornBackend(scenario, writesEnabled),
    "more-http-api" => new MoreHttpApiBackend(new()
    {
        EnableWrites = writesEnabled,
        BaseUrl = Environment.GetEnvironmentVariable("TIMBERBORN_BASE_URL") ?? "http://localhost:8080/",
        Authorization = Environment.GetEnvironmentVariable("TIMBERBORN_AUTHORIZATION")
    }),
    _ => throw new InvalidOperationException("Unbekanntes Backend.")
};
using var native = backendName == "native" ? new NativeTools(new NativeClient(NativeConfiguration.Load(
    Environment.GetEnvironmentVariable("TIMBERBORN_NATIVE_CONFIG") ?? throw new InvalidOperationException("TIMBERBORN_NATIVE_CONFIG fehlt."))), validationEnabled, placementEnabled, lodgePlacementEnabled, speedControlEnabled, staffingEnabled, prioritiesEnabled, areasEnabled, removalEnabled, buildingPlacementEnabled, buildingSettingsEnabled, researchEnabled) : null;
var service = backend is null ? null : new ObservationService(backend);
var actions = backend is null ? null : new BuildingActionService(backend, (ITimberbornWriteBackend)backend);
var tools = native is null ? ToolCatalog.Create(writesEnabled) : NativeTools.Catalog(validationEnabled, placementEnabled, lodgePlacementEnabled, speedControlEnabled, staffingEnabled, prioritiesEnabled, areasEnabled, removalEnabled, buildingPlacementEnabled, buildingSettingsEnabled, researchEnabled);
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = [], DisableDefaults = true });
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Services.AddMcpServer().WithStdioServerTransport()
    .WithListToolsHandler((_, _) => ValueTask.FromResult(new ListToolsResult { Tools = tools.ToList() }))
    .WithCallToolHandler(async (context, ct) =>
    {
        var request = context.Params ?? throw new McpProtocolException("Missing parameters", McpErrorCode.InvalidParams);
        if (native is null && !tools.Any(t => t.Name == request.Name))
            throw new McpProtocolException("Unknown tool", McpErrorCode.InvalidParams);
        var arguments = JsonSerializer.SerializeToElement(request.Arguments ?? new Dictionary<string, JsonElement>());
        var result = native is not null ? await native.InvokeLogged(request.Name, arguments, ct)
            : request.Name == "set_building_paused"
                ? await actions!.InvokeAsync(arguments, ct)
                : await service!.InvokeAsync(request.Name, arguments, ct);
        return new CallToolResult
        {
            StructuredContent = JsonSerializer.SerializeToElement(result),
            Content = [new TextContentBlock { Text = result.ToJsonString() }],
            IsError = result["status"]!.GetValue<string>() == "error"
        };
    });
try { await builder.Build().RunAsync(); }
finally { (backend as IDisposable)?.Dispose(); }
