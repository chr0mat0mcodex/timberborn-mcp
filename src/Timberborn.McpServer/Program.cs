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
using Timberborn.Bridge.Core;

// Explicit offline gate report: no host, game connection, config read or diagnostic file.
if (args is ["--print-native-identity"])
{
    var catalog = NativeTools.Catalog(NativeToolOptions.AllEnabled).Select(ResponsePresentation.Describe);
    Console.WriteLine(JsonSerializer.Serialize(McpRuntimeIdentity.Create(catalog), NativeJson.Options));
    return;
}

var backendName = Environment.GetEnvironmentVariable("TIMBERBORN_BACKEND");
if (string.IsNullOrWhiteSpace(backendName))
{
    Console.Error.WriteLine("TIMBERBORN_BACKEND fehlt: explizit native, more-http-api (Legacy) oder fake setzen. Kein automatischer Fallback.");
    Environment.ExitCode = 2;
    return;
}

var fileLog=new DiagnosticFileLog(DiagnosticFileLog.DefaultDirectory("mcp"),"mcp",value=>JsonSerializer.Serialize(value),
    ()=>Console.Error.WriteLine("MCP file diagnostics unavailable; records may be missing."));
fileLog.Write("server_starting");
try {

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
var nativeOptions = new NativeToolOptions
{
    EnableValidation = validationEnabled,
    EnablePlacement = placementEnabled,
    EnableLodgePlacement = lodgePlacementEnabled,
    EnableSpeedControl = speedControlEnabled,
    EnableStaffing = staffingEnabled,
    EnablePriorities = prioritiesEnabled,
    EnableAreas = areasEnabled,
    EnableRemoval = removalEnabled,
    EnableBuildingPlacement = buildingPlacementEnabled,
    EnableBuildingSettings = buildingSettingsEnabled,
    EnableResearch = researchEnabled,
};
using var native = backendName == "native" ? new NativeTools(new NativeClient(NativeConfiguration.Load(
    Environment.GetEnvironmentVariable("TIMBERBORN_NATIVE_CONFIG") ?? throw new InvalidOperationException("TIMBERBORN_NATIVE_CONFIG fehlt."))), nativeOptions,fileLog) : null;
var service = backend is null ? null : new ObservationService(backend);
var actions = backend is null ? null : new BuildingActionService(backend, (ITimberbornWriteBackend)backend);
var tools = native is null ? ToolCatalog.Create(writesEnabled) : NativeTools.Catalog(nativeOptions).Select(ResponsePresentation.Describe).ToArray();
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
        bool full = false;
        if (native is not null) {
            try { (arguments, full) = ResponsePresentation.ReadRequest(request.Name, arguments); }
            catch (ArgumentException) { fileLog.Write("request_rejected",request.Name,"error","invalid_detail"); throw new McpProtocolException("Nicht ausgeführt: detail muss compact oder full sein und vom Werkzeug unterstützt werden.", McpErrorCode.InvalidParams); }
        }
        JsonElement backendArguments;
        try { backendArguments = ActivityTools.RequireReasoning(arguments); }
        catch (ArgumentException) { fileLog.Write("request_rejected",request.Name,"error","invalid_reasoning"); throw new McpProtocolException("Nicht ausgeführt: reasoning ist erforderlich: nicht leere Begründung, maximal 600 Zeichen.", McpErrorCode.InvalidParams); }
        var result = native is not null ? await native.InvokeLogged(request.Name, arguments, ct)
            : request.Name == "set_building_paused"
                ? await actions!.InvokeAsync(backendArguments, ct)
                : await service!.InvokeAsync(request.Name, backendArguments, ct);
        if(native is not null && request.Name==ScreenshotTools.Name) return ScreenshotTools.ToToolResult(result);
        return ResponsePresentation.ToToolResult(native is null ? result : ResponsePresentation.Present(request.Name, result, full));
    });
try { await builder.Build().RunAsync(); }
finally { fileLog.Write("server_stopping"); (backend as IDisposable)?.Dispose(); }
} catch(Exception) {fileLog.Write("server_failed",code:"startup_or_host_failure");throw;}
