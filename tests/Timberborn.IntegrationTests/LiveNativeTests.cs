using ModelContextProtocol.Client;
using System.Text.Json;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class LiveNativeTests(ITestOutputHelper output)
{
    public static bool Enabled => Environment.GetEnvironmentVariable("TIMBERBORN_NATIVE_LIVE_TEST") == "1";

    [Fact(Skip = "Native-Livetest nur nach explizitem Opt-in.", SkipUnless = nameof(Enabled))]
    public async Task TwentyNineReadToolsAgainstInstalledNativeBridge()
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(40));
        var ct = budget.Token;
        var config = Environment.GetEnvironmentVariable("TIMBERBORN_NATIVE_CONFIG");
        Assert.False(string.IsNullOrWhiteSpace(config));
        await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Command = "dotnet", WorkingDirectory = StdioServerTests.Root,
            Arguments = [Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")],
            EnvironmentVariables = new Dictionary<string, string?>
            {
                ["TIMBERBORN_BACKEND"] = "native", ["TIMBERBORN_ENABLE_RESEARCH"] = "0", ["TIMBERBORN_NATIVE_CONFIG"] = config, ["TIMBERBORN_ENABLE_BUILDING_SETTINGS"] = "0", ["TIMBERBORN_ENABLE_BUILDING_PLACEMENT"] = "0",
                ["TIMBERBORN_ENABLE_WRITES"] = "0", ["TIMBERBORN_ENABLE_VALIDATION"] = "0", ["TIMBERBORN_ENABLE_PLACEMENT"] = "0", ["TIMBERBORN_ENABLE_LODGE_PLACEMENT"] = "0", ["TIMBERBORN_ENABLE_SPEED_CONTROL"] = "0", ["TIMBERBORN_ENABLE_STAFFING"] = "0", ["TIMBERBORN_ENABLE_PRIORITIES"] = "0", ["TIMBERBORN_ENABLE_AREAS"] = "0", ["TIMBERBORN_ENABLE_REMOVAL"] = "0"
            }
        }), cancellationToken: ct);
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        Assert.Equal(new[] { "inspect_needs", "inspect_beaver_needs", "inspect_building_operation", "inspect_building_access", "inspect_road_connection", "inspect_work_range", "inspect_good_history", "find_buildings", "inspect_alert_targets", "inspect_alerts", "inspect_goods", "inspect_agent_log", "inspect_area_types", "inspect_areas", "inspect_build_catalog", "inspect_build_options", "inspect_building", "inspect_building_priority", "inspect_building_settings", "inspect_colony", "inspect_construction", "inspect_map_region", "inspect_removal_targets", "inspect_research", "inspect_simulation", "inspect_workforce", "precheck_build_site", "precheck_building", "timberborn_status" }.Order(), tools.Select(t => t.Name).Order());
        string? session = null;
        async Task<JsonElement> Call(string name, Dictionary<string, object?>? args = null)
        {
            var result = await client.CallToolAsync(name, args, cancellationToken: ct);
            Assert.False(result.IsError);
            var root = result.StructuredContent!.Value;
            Assert.Equal("ok", root.GetProperty("status").GetString());
            var meta = root.GetProperty("meta");
            Assert.Equal("native", meta.GetProperty("backend").GetString());
            Assert.False(meta.GetProperty("simulated").GetBoolean());
            var current = meta.GetProperty("sessionId").GetString();
            Assert.True(Guid.TryParse(current, out _));
            session ??= current;
            Assert.Equal(session, current);
            return root.GetProperty("data");
        }
        var status = await Call("timberborn_status");
        Assert.Equal("reachable", status.GetProperty("connection").GetString());
        Assert.False(status.GetProperty("writesEnabled").GetBoolean());
        await Call("inspect_agent_log");
        await Call("inspect_research", new Dictionary<string,object?> { ["offset"]=0,["limit"]=32 });
        await Call("inspect_goods", new() { ["offset"]=0,["limit"]=32 });
        var alerts=await Call("inspect_alerts",new() { ["offset"]=0,["limit"]=32 });
        string alertId=alerts.GetProperty("items").GetArrayLength()>0 ? alerts.GetProperty("items")[0].GetProperty("alertId").GetString()! : new string('0',64);
        await Call("inspect_alert_targets",new() { ["offset"]=0,["limit"]=32,["session"]=session,["alertId"]=alertId });
        var simulation = await Call("inspect_simulation");
        Assert.True(simulation.GetProperty("currentSpeed").GetSingle() >= 0);
        await Call("inspect_removal_targets", new() { ["kind"]="all", ["x"]=0, ["y"]=0, ["z"]=0, ["width"]=1, ["height"]=1, ["depth"]=1, ["offset"]=0, ["limit"]=32 });
        var workforce = await Call("inspect_workforce", new() { ["offset"] = 0, ["limit"] = 32 });
        Assert.Equal("entities_with_worker_component", workforce.GetProperty("scope").GetString());
        await Call("inspect_needs",new(){["offset"]=0,["limit"]=32});
        var beaver=workforce.GetProperty("items").EnumerateArray().First(w=>w.GetProperty("workerType").GetString()=="Beaver");
        await Call("inspect_beaver_needs",new(){["id"]=beaver.GetProperty("id").GetString(),["session"]=session,["offset"]=0,["limit"]=32});
        await Call("inspect_area_types");
        await Call("inspect_areas", new() { ["kind"] = "tree_cutting", ["offset"] = 0, ["limit"] = 32 });
        await Call("inspect_construction", new() { ["offset"] = 0, ["limit"] = 32 });
        var colony = await Call("inspect_colony");
        var size = colony.GetProperty("mapSize");
        var objects = await Call("find_buildings", new() { ["offset"] = 0, ["limit"] = 32 });
        var sample = objects.GetProperty("items");
        Assert.NotEqual(0, sample.GetArrayLength());
        var center = sample.EnumerateArray().Single(b => b.GetProperty("template").GetString() == "DistrictCenter.Folktails");
        await Call("inspect_building_priority", new() { ["id"] = center.GetProperty("id").GetString(), ["kind"] = "workplace", ["session"] = session });
        await Call("inspect_building_settings", new() { ["id"] = center.GetProperty("id").GetString(), ["session"] = session });
        await Call("inspect_building_operation",new(){["id"]=center.GetProperty("id").GetString(),["session"]=session});
        await Call("inspect_building_access",new(){["id"]=center.GetProperty("id").GetString(),["session"]=session});
        await Call("inspect_road_connection",new(){["id"]=center.GetProperty("id").GetString(),["toId"]=center.GetProperty("id").GetString(),["session"]=session});
        await Call("inspect_work_range",new(){["id"]=center.GetProperty("id").GetString(),["session"]=session,["offset"]=0,["limit"]=32});
        await Call("inspect_good_history",new(){["good"]="Water",["offset"]=0,["limit"]=32});
        var position = sample[0].GetProperty("position");
        int x = position.GetProperty("x").GetInt32();
        int y = position.GetProperty("y").GetInt32();
        int z = position.GetProperty("z").GetInt32();
        Assert.InRange(x, 0, size.GetProperty("x").GetInt32() - 1);
        Assert.InRange(y, 0, size.GetProperty("y").GetInt32() - 1);
        Assert.InRange(z, 0, size.GetProperty("z").GetInt32() - 1);
        var map = await Call("inspect_map_region", new()
        { ["x"] = x, ["y"] = y, ["z"] = z, ["width"] = 1, ["height"] = 1, ["depth"] = 1 });
        Assert.Equal(1, map.GetProperty("cells").GetArrayLength());
        var catalog = await Call("inspect_build_catalog");
        Assert.Equal(2, catalog.GetProperty("items").GetArrayLength());
        await Call("inspect_build_options", new() { ["offset"] = 0, ["limit"] = 32 });
        await Call("precheck_building", new() { ["template"] = "Lodge.Folktails", ["x"] = x, ["y"] = y, ["z"] = z, ["rotation"] = 0 });
        var site = await Call("precheck_build_site", new()
        { ["template"] = "Lodge.Folktails", ["x"] = x, ["y"] = y, ["z"] = z, ["rotation"] = 0 });
        Assert.False(site.GetProperty("gameValidated").GetBoolean());
        var building = await Call("inspect_building", new()
        { ["id"] = sample[0].GetProperty("id").GetString(), ["session"] = session });
        Assert.True(building.GetProperty("found").GetBoolean());
        Assert.Equal(sample[0].GetProperty("template").GetString(), building.GetProperty("details").GetProperty("template").GetString());
        output.WriteLine("Native MCP: twenty-nine read tools succeeded in one game session, including generic catalog/precheck without placement or preview creation.");
        // Only aggregate diagnostics; no names, entity/session IDs, paths or tokens in test output.
        output.WriteLine("Population: " + colony.GetProperty("population"));
        output.WriteLine("Housing: " + colony.GetProperty("housing"));
        output.WriteLine("Map size: " + size);
    }
}
