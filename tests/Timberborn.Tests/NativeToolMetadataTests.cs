using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class NativeToolMetadataTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string Run = "22222222-2222-4222-8222-222222222222";

    // Frozen public family contract, independent of descriptor implementation.
    [Theory]
    [InlineData("run_simulation_for", "simulation-run-for", NativeToolKind.Mutation, true)]
    [InlineData("run_simulation_until", "simulation-run-until", NativeToolKind.Mutation, true)]
    [InlineData("inspect_simulation_run", "simulation-run", NativeToolKind.Status, false)]
    [InlineData("cancel_simulation_run", "simulation-run-cancel", NativeToolKind.Cancel, true)]
    public void CatalogDispatchOwnershipAndIndependentHttpGateAgree(string name, string route, NativeToolKind kind, bool writes)
    {
        var descriptor = Assert.IsType<NativeToolDescriptor>(SimulationRunTools.Find(name));
        Assert.Equal(route, descriptor.Route);
        Assert.Equal(kind, descriptor.Kind);
        Assert.Equal(writes, SimulationRunTools.Writes(name));
        Assert.Equal(writes, NativeTools.BlockedByClearedGroup(name));
        foreach (bool enabled in new[] { false, true })
        {
            var tool = SimulationRunTools.Catalog(enabled).SingleOrDefault(t => t.Name == name);
            Assert.Equal(!writes || enabled, tool is not null);
            if (tool is not null)
            {
                Assert.Equal(!writes, tool.Annotations!.ReadOnlyHint);
                Assert.Equal(writes, tool.Annotations.DestructiveHint);
                Assert.Equal(!writes, tool.Annotations.IdempotentHint);
                Assert.False(tool.Annotations.OpenWorldHint);
            }
            foreach (string method in new[] { "GET", "POST", "DELETE" })
                foreach (bool body in new[] { false, true })
                    Assert.Equal(!body && method == (writes ? "POST" : "GET") && (!writes || enabled),
                        BridgeHttpServer.MethodAllowed(method, "/agent-api/v1/" + route, body, false, enableSpeedControl: enabled));
        }
    }

    [Theory]
    [InlineData("run_simulation_for", "session,runId,speed,expectedSpeed,maxRealSeconds,duration,unit")]
    [InlineData("run_simulation_until", "session,runId,speed,expectedSpeed,maxRealSeconds,dayNumber,hour")]
    [InlineData("inspect_simulation_run", "session,runId")]
    [InlineData("cancel_simulation_run", "session,runId")]
    public void RequiredArgumentsAndClosedSchemasRemainUnchanged(string name, string required)
    {
        var tool = SimulationRunTools.Catalog(true).Single(t => t.Name == name);
        Assert.Equal(required.Split(','), tool.InputSchema.GetProperty("required").EnumerateArray().Select(x => x.GetString()));
        Assert.False(tool.InputSchema.GetProperty("additionalProperties").GetBoolean());
        var props = tool.InputSchema.GetProperty("properties");
        Assert.Equal("uuid", props.GetProperty("session").GetProperty("format").GetString());
        Assert.Equal("uuid", props.GetProperty("runId").GetProperty("format").GetString());
        if (name == "run_simulation_for")
        {
            Assert.Equal(0.000001, props.GetProperty("duration").GetProperty("minimum").GetDouble());
            Assert.Equal(672, props.GetProperty("duration").GetProperty("maximum").GetInt32());
            Assert.DoesNotContain("measureProduction", required.Split(','));
        }
    }

    private static JsonElement Args(string name)
    {
        var args = new JsonObject { ["session"] = Session, ["runId"] = Run };
        if (name.StartsWith("run_", StringComparison.Ordinal))
        {
            args["speed"] = 3; args["expectedSpeed"] = 0; args["maxRealSeconds"] = 30;
            if (name == "run_simulation_for") { args["duration"] = 0.25; args["unit"] = "hours"; }
            else { args["dayNumber"] = 1; args["hour"] = 2; }
        }
        return JsonSerializer.SerializeToElement(args);
    }

    [Theory]
    [InlineData("run_simulation_for")]
    [InlineData("run_simulation_until")]
    [InlineData("cancel_simulation_run")]
    public async Task DisabledMutationNeverReachesHttpEvenWithAllOtherOptionsEnabled(string name)
    {
        using var handler = new RejectTransport();
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler),
            NativeToolOptions.AllEnabled with { EnableSpeedControl = false });
        var result = await tools.Invoke(name, Args(name), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal("not_executed", result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData("run_simulation_for", "simulation-run-for", "POST")]
    [InlineData("run_simulation_until", "simulation-run-until", "POST")]
    [InlineData("inspect_simulation_run", "simulation-run", "GET")]
    [InlineData("cancel_simulation_run", "simulation-run-cancel", "POST")]
    public async Task FamilyHandlerUsesExpectedRouteAndMethod(string name, string route, string method)
    {
        using var handler = new RejectTransport();
        using var client = new NativeClient(new(8081, new string('a', 64)), handler);
        await Assert.ThrowsAsync<HttpRequestException>(() => SimulationRunTools.Invoke(client, name, Args(name), true, TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.Calls);
        Assert.Equal("/agent-api/v1/" + route, handler.Path);
        Assert.Equal(method, handler.Method);
    }

    [Fact]
    public async Task UnknownToolAndInvalidArgumentsKeepErrorContractWithoutHttp()
    {
        Assert.Null(SimulationRunTools.Find("unknown_tool"));
        Assert.False(SimulationRunTools.Writes("unknown_tool"));
        Assert.False(NativeTools.BlockedByClearedGroup("unknown_tool"));
        using var handler = new RejectTransport();
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), new NativeToolOptions { EnableSpeedControl = true });
        foreach (var name in new[] { "unknown_tool", "run_simulation_for" })
        {
            var result = await tools.Invoke(name, JsonSerializer.SerializeToElement(new { }), TestContext.Current.CancellationToken);
            Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
            Assert.Equal(name == "unknown_tool" ? "not_applicable" : "not_executed", result["error"]!["execution"]!.GetValue<string>());
            if (name == "run_simulation_for") Assert.Contains("Pflichtparameter session fehlt.", result["error"]!["message"]!.GetValue<string>());
        }
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public void LegacyOwnershipIndexMatchesCatalogAndIsIsolatedFromReturnedToolMutations()
    {
        foreach (var tool in NativeTools.Catalog(NativeToolOptions.AllEnabled))
        {
            bool blocked = tool.Annotations?.ReadOnlyHint != true;
            Assert.Equal(blocked, NativeTools.BlockedByClearedGroup(tool.Name));
            if (tool.Annotations is not null) tool.Annotations = new() { ReadOnlyHint = blocked };
            Assert.Equal(blocked, NativeTools.BlockedByClearedGroup(tool.Name));
        }
    }

    [Theory]
    [InlineData("Validation", "validate_build_site")]
    [InlineData("Placement", "place_path")]
    [InlineData("LodgePlacement", "place_lodge")]
    [InlineData("SpeedControl", "run_simulation_for")]
    [InlineData("Staffing", "set_workplace_staffing")]
    [InlineData("Priorities", "set_building_priority")]
    [InlineData("Areas", "set_area")]
    [InlineData("Removal", "remove_vegetation")]
    [InlineData("BuildingPlacement", "place_building")]
    [InlineData("BuildingSettings", "set_building_recipe")]
    [InlineData("Research", "unlock_building")]
    public void NamedOptionsEnableOnlyTheirOwnRepresentativeActions(string option, string expected)
    {
        var options = option switch
        {
            "Validation" => new NativeToolOptions { EnableValidation = true },
            "Placement" => new NativeToolOptions { EnablePlacement = true },
            "LodgePlacement" => new NativeToolOptions { EnableLodgePlacement = true },
            "SpeedControl" => new NativeToolOptions { EnableSpeedControl = true },
            "Staffing" => new NativeToolOptions { EnableStaffing = true },
            "Priorities" => new NativeToolOptions { EnablePriorities = true },
            "Areas" => new NativeToolOptions { EnableAreas = true },
            "Removal" => new NativeToolOptions { EnableRemoval = true },
            "BuildingPlacement" => new NativeToolOptions { EnableBuildingPlacement = true },
            "BuildingSettings" => new NativeToolOptions { EnableBuildingSettings = true },
            "Research" => new NativeToolOptions { EnableResearch = true },
            _ => throw new ArgumentException(),
        };
        var catalog = NativeTools.Catalog(options);
        string[] representatives = ["validate_build_site", "place_path", "place_lodge", "run_simulation_for",
            "set_workplace_staffing", "set_building_priority", "set_area", "remove_vegetation", "place_building",
            "set_building_recipe", "unlock_building"];
        Assert.Equal(new[] { expected }, catalog.Where(t => representatives.Contains(t.Name)).Select(t => t.Name));
        Assert.Contains(catalog, t => t.Name == "inspect_simulation_run" && t.Annotations!.ReadOnlyHint == true);
        Assert.DoesNotContain(catalog, t => t.Name == ClearedGroupTools.Start);
    }

    private sealed class RejectTransport : HttpMessageHandler
    {
        public int Calls;
        public string? Path, Method;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++; Path = request.RequestUri!.AbsolutePath; Method = request.Method.Method;
            throw new HttpRequestException("Synthetic unavailable transport");
        }
    }
}
