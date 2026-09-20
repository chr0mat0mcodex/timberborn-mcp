using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using ModelContextProtocol.Client;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class NativeBridgeTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    public async Task AuthenticatedBridgeThroughRealStdioHonorsIndependentActionGates(bool enableValidation, bool enablePlacement, bool enableLodgePlacement)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var ct = timeout.Token;
        using var reservation = new TcpListener(IPAddress.Loopback, 0);
        reservation.Start(); int port = ((IPEndPoint)reservation.LocalEndpoint).Port; reservation.Stop();
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        using var queue = new MainThreadQueue();
        using var bridge = new BridgeHttpServer(port, token, queue, enableValidation, enablePlacement, enableLodgePlacement, enableSpeedControl: enableLodgePlacement, enableStaffing: enableValidation, enablePriorities: enablePlacement, enableAreas: enableLodgePlacement, enableRemoval: enableValidation, enableBuildingPlacement: enableLodgePlacement, enableBuildingSettings: enableValidation, enableResearch: enablePlacement);
        bridge.Start();
        int sciencePoints=20;bool researchUnlocked=false;
        int observations = 0; var activityLog=new ActivityLog();
        float currentSpeed = 1; int desiredWorkers = 2; string priority = "Normal"; string areaState = "unmarked";
        var session = Guid.NewGuid().ToString("D");
        var settingValues=new Dictionary<string,string>{["paused"]="false",["good"]="",["mode"]="accept",["priority"]="harvesting",["resource"]=""};
        NativeBuildingSettings Settings(Guid id)=>new(id,"SyntheticSettingsBuilding",new(1,2,3),true,new(settingValues["paused"]=="true",true),true,
            new(settingValues["good"],true,["Berries","Carrot"],settingValues["mode"],true,30,[],false),true,
            new(settingValues["priority"],settingValues["resource"],true,false,[new("Carrot",true)]),["synthetic_hybrid_fixture"]);
        object Change(BuildingSettingsRequest r){
            if(r.Session!=session)throw new BridgeRejectionException("stale_session");
            var key=BuildingSettingsRequest.ValueKey(r.Route);
            var changed=SettingChange.Execute(r.ExpectedValue,r.Value,()=>settingValues[key],()=>settingValues[key]=r.Value);
            var observation=Settings(Guid.Parse(r.Id));
            return new NativeSettingChange(observation.Id,observation.Template,key,changed.Previous,changed.Requested,changed.Observed,changed.Outcome,observation,["synthetic_test"]);
        }
        object Unlock(ResearchRequest r) {
            if(r.Session!=session)throw new BridgeRejectionException("stale_session");
            int before=sciencePoints;bool previous=researchUnlocked;
            var outcome=ResearchTransaction.Execute(r.ExpectedCost,10,true,()=>sciencePoints,()=>researchUnlocked,()=>true,()=>{sciencePoints-=10;researchUnlocked=true;});
            return new NativeUnlock(r.Template,r.ExpectedCost,10,before,sciencePoints,previous,researchUnlocked,outcome);
        }
        var placementGate = new SinglePlacementGate(); var buildingGate = new BuildingActionGate();
        using var pumpStop = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var pump = Task.Run(async () =>
        {
            try
            {
                while (!pumpStop.IsCancellationRequested)
                {
                    queue.Pump(r =>
                    {
                        if(r.Route=="activity") {
                            var a=r.Activity!;
                            if(a.State!="running" && a.Session!=session)throw new ArgumentException();
                            return JsonSerializer.Serialize(new BridgeEnvelope<object>(1,session,DateTimeOffset.UtcNow,"0.19.1",new NativeActivityAck(activityLog.Record(a,DateTimeOffset.UtcNow))),NativeJson.Options);
                        }
                        if(r.Route=="activity-log")return JsonSerializer.Serialize(new BridgeEnvelope<object>(1,session,DateTimeOffset.UtcNow,"0.19.1",new NativeActivityLog(128,activityLog.Snapshot().Reverse().Take(32).ToArray(),activityLog.Revision,false,false)),NativeJson.Options);
                        Interlocked.Increment(ref observations);
                        if(r.Route=="alert-targets" && r.Session!=session)throw new BridgeRejectionException("stale_session");
                        object data = r.Route switch
                        {
                            "building-access" => new NativeAccess(r.Logistics!.Id,true,new(1,2,3),false,false,false,null,3,1,1,[]),
                            "road-connection" => new NativeRoad(r.Logistics!.Id,r.Logistics.ToId,true,1,1,true,3,[]),
                            "work-range" => new NativeRange(r.Logistics!.Id,false,[],r.Logistics.Offset,r.Logistics.Limit,0,[],false,[]),
                            "good-history" => new NativeGoodHistory(r.Logistics!.Good,true,r.Logistics.Offset,r.Logistics.Limit,0,[],false,0,0,null,[]),
                            "goods" => new NativeGoods("global_registered_goods",r.Economy!.Offset,r.Economy.Limit,0,[],false,["synthetic_test"]),
                            "alerts" => new NativeAlerts("visible_active_entity_statuses",r.Economy!.Offset,r.Economy.Limit,0,[],false,["synthetic_test"]),
                            "alert-targets" => new NativeAlertTargets(r.Economy!.AlertId,false,r.Economy.Offset,r.Economy.Limit,0,[],false,["synthetic_test"]),
                            "research" => new NativeResearch(sciencePoints,r.Research!.Offset,r.Research.Limit,1,r.Research.Offset==0?[new("SyntheticResearch",10,true,researchUnlocked,!researchUnlocked)]:[],false,["synthetic_test"]),
                            "unlock-building" => Unlock(r.Research!),
                            "building-settings" when r.Session==session => Settings(Guid.Parse(r.Settings!.Id)),
                            "set-building-paused" or "set-storage-good" or "set-storage-mode" or "set-farm-priority" or "set-farm-crop" => Change(r.Settings!),
                            "removal-targets" => new NativeRemovalTargets(r.Removal!.Kind,r.Removal.Offset,r.Removal.Limit,0,[],false,["synthetic_test"]),
                            "remove-object" when r.Session == session => new NativeRemoval(Guid.Parse(r.Removal!.Id),r.Removal.Kind,r.Removal.Template,new(r.Removal.X,r.Removal.Y,r.Removal.Z),r.Removal.Operation,"applied",r.Removal.Operation=="delete",r.Removal.Operation=="delete"?null:r.Removal.Operation=="mark",["synthetic_test"]),
                            "workplace-staffing" when r.Session == session && r.ExpectedDesiredWorkers == desiredWorkers && r.DesiredWorkers <= 4 => new NativeStaffingResult(Guid.Parse(r.EntityId), "DistrictCenter.Folktails", desiredWorkers, r.DesiredWorkers, desiredWorkers = r.DesiredWorkers, 2, 4, "applied", ["synthetic_test"]),
                            "priority" => new NativePriority(Guid.Parse(r.Management!.Id),r.Management.Kind,priority,priority,"observed",["synthetic_test"]),
                            "set-priority" when r.Session == session && r.Management!.ExpectedPriority == priority => new NativePriority(Guid.Parse(r.Management.Id),r.Management.Kind,priority,priority=r.Management.Priority,"applied",["synthetic_test"]),
                            "construction" => new NativeConstructionList(r.Management!.Offset,r.Management.Limit,0,[],false,["synthetic_test"]),
                            "areas" => new NativeAreas(r.Management!.Kind,true,r.Management.Offset,r.Management.Limit,0,[],false,["synthetic_test"]),
                            "area-types" => new NativeAreaTypes([new("tree_cutting",true,true,true,"test"),new("crops",true,true,true,"test"),new("tree_planting",true,true,true,"test"),new("tapping",true,true,false,"cutting_protection")],[],["synthetic_test"]),
                            "set-area" when r.Session == session && r.Management!.ExpectedResource == areaState => new NativeAreaChange(r.Management.Kind,r.Management.Operation,"applied",[new(new(r.Management.X,r.Management.Y,r.Management.Z),areaState=r.Management.Operation=="mark"?"marked":"unmarked")],["synthetic_test"]),
                            "simulation" => new NativeSimulation(currentSpeed, 1, 0.5f, 12),
                            "simulation-speed" when r.Session == session && r.ExpectedSpeed == currentSpeed => new NativeSpeedResult(r.Speed, currentSpeed, true, new NativeSimulation(currentSpeed = r.Speed, 1, 0.5f, 12), ["synthetic_test"]),
                            "snapshot" => Snapshot(),
                            "map" => new NativeMap(new(r.X, r.Y, r.Z), 1, 1, 1,
                                [new(r.X, r.Y, r.Z, false, true, 2, 0.5f, 0, true)], ["synthetic_test"]),
                            "workforce" => new NativeWorkforce("entities_with_worker_component", r.Offset, r.Limit, 0, 0, 0, [], false, ["synthetic_test"]),
                            "objects" => new NativeObjects("buildings_and_paths", r.Offset, r.Limit, 0, [], false, ["synthetic_test"]),
                            "building" => new NativeBuilding(Guid.Parse(r.EntityId), false, null, ["synthetic_test"]),
                            "building-catalog" => new NativeBuildingCatalog("Folktails",r.Offset,r.Limit,1,
                                r.Offset==0?[new("WaterPump.Folktails",true,true,true,[],"Single","Square","Water",new(1,2,3),null,false,[new("Log",12,20)])]:[],r.Offset==0&&r.Limit<1,["synthetic_test"]),
                            "building-validation" when r.Session==session => new NativeValidation(r.Template,new(r.X,r.Y,r.Z),r.Rotation,true,true,true,false,255,["synthetic_test"]),
                            "building-placement" when r.Session==session => buildingGate.Execute(Guid.Parse(r.EntityId),()=>new NativePlacement(r.Template,new(r.X,r.Y,r.Z),r.Rotation,Guid.Parse(r.EntityId),"applied",false,false,["synthetic_test"])),
                            "catalog" => new NativeCatalog("Folktails",
                                [new("Lodge.Folktails", true, true, true, new(2, 2, 1), new(1, -1, 0), [new("Log", 12, 0)]),
                                 new("Path", true, true, true, new(1, 1, 1), null, [])], ["synthetic_test"]),
                            "site-precheck" or "building-precheck" => new NativeSite(r.Template, new(r.X, r.Y, r.Z), r.Rotation,
                                "requires_game_validation", false, [], [new(new(r.X, r.Y, r.Z), true, false, true, false, "Ground")],
                                null, null, [], ["not_full_game_validator"]),
                            "site-validation" => new NativeValidation(r.Template, new(r.X, r.Y, r.Z), r.Rotation,
                                true, true, true, false, 7, ["synthetic_test"]),
                            "path-placement" or "lodge-placement" => placementGate.Execute(() => new NativePlacement(r.Template, new(r.X, r.Y, r.Z), r.Rotation,
                                Guid.NewGuid(), "applied", r.Template == "Path", true, ["synthetic_test"])),
                            _ => throw new ArgumentException()
                        };
                        return JsonSerializer.Serialize(new BridgeEnvelope<object>(1, session, DateTimeOffset.UtcNow, "0.19.1", data), NativeJson.Options);
                    });
                    await Task.Delay(5, pumpStop.Token);
                }
            }
            catch (OperationCanceledException) when (pumpStop.IsCancellationRequested) { }
        }, ct);
        var configPath = Path.Combine(StdioServerTests.Root, ".local", $"native-test-{Guid.NewGuid():N}.local.json");
        try
        {
            using var http = new HttpClient(new SocketsHttpHandler { UseProxy = false });
            var url = $"http://localhost:{port}/agent-api/v1/snapshot";
            using var unauth = await http.GetAsync(url, ct);
            Assert.Equal(HttpStatusCode.Unauthorized, unauth.StatusCode);
            http.DefaultRequestHeaders.Authorization = new("Bearer", token);
            using var post = await http.PostAsync(url, new StringContent("{}"), ct);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, post.StatusCode);
            using var originRequest = new HttpRequestMessage(HttpMethod.Get, url);
            originRequest.Headers.Add("Origin", "https://example.invalid");
            using var origin = await http.SendAsync(originRequest, ct);
            Assert.Equal(HttpStatusCode.Forbidden, origin.StatusCode);
            using var forbiddenGet = await http.GetAsync($"http://localhost:{port}/agent-api/v1/site-validation", ct);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, forbiddenGet.StatusCode);
            if (!enableValidation)
            {
                using var denied = await http.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{port}/agent-api/v1/site-validation"), ct);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, denied.StatusCode);
            }
            Assert.Equal(0, Volatile.Read(ref observations));
            using var placementGet = await http.GetAsync($"http://localhost:{port}/agent-api/v1/path-placement", ct);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, placementGet.StatusCode);
            if (!enablePlacement)
            {
                using var placementPost = await http.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{port}/agent-api/v1/path-placement"), ct);
                Assert.Equal(HttpStatusCode.MethodNotAllowed, placementPost.StatusCode);
            }

            await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(new NativeConfiguration(port, token), NativeJson.Options), ct);
            await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
            {
                Command = "dotnet", WorkingDirectory = StdioServerTests.Root,
                Arguments = [Path.Combine(StdioServerTests.Root, "src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll")],
                EnvironmentVariables = new Dictionary<string, string?>
                {
                    ["TIMBERBORN_ENABLE_BUILDING_SETTINGS"] = enableValidation ? "1" : "0", ["TIMBERBORN_ENABLE_BUILDING_PLACEMENT"] = enableLodgePlacement ? "1" : "0", ["TIMBERBORN_BACKEND"] = "native", ["TIMBERBORN_NATIVE_CONFIG"] = configPath,
                    ["TIMBERBORN_ENABLE_RESEARCH"] = enablePlacement ? "1" : "0", ["TIMBERBORN_ENABLE_WRITES"] = "1", ["TIMBERBORN_ENABLE_REMOVAL"] = enableValidation ? "1" : "0",
                    ["TIMBERBORN_ENABLE_VALIDATION"] = enableValidation ? "1" : "0",
                    ["TIMBERBORN_ENABLE_PLACEMENT"] = enablePlacement ? "1" : "0",
                    ["TIMBERBORN_ENABLE_LODGE_PLACEMENT"] = enableLodgePlacement ? "1" : "0",
                    ["TIMBERBORN_ENABLE_SPEED_CONTROL"] = enableLodgePlacement ? "1" : "0",
                    ["TIMBERBORN_ENABLE_STAFFING"] = enableValidation ? "1" : "0",
                    ["TIMBERBORN_ENABLE_PRIORITIES"] = enablePlacement ? "1" : "0", ["TIMBERBORN_ENABLE_AREAS"] = enableLodgePlacement ? "1" : "0"
                }
            }), cancellationToken: ct);
            var tools = await client.ListToolsAsync(cancellationToken: ct);
            var expected = new List<string> { "inspect_building_access", "inspect_road_connection", "inspect_work_range", "inspect_good_history", "find_buildings", "inspect_alert_targets", "inspect_alerts", "inspect_goods", "inspect_research", "inspect_agent_log", "inspect_area_types", "inspect_areas", "inspect_build_catalog", "inspect_build_options", "precheck_building", "inspect_building", "inspect_building_priority", "inspect_building_settings", "inspect_colony", "inspect_construction", "inspect_map_region", "inspect_simulation", "inspect_workforce", "inspect_removal_targets", "precheck_build_site", "timberborn_status" };
            if (enableValidation) { expected.AddRange(["set_building_paused","set_storage_good","set_storage_mode","set_farm_priority","set_farm_crop"]); expected.Add("validate_build_site"); expected.Add("set_workplace_staffing"); expected.AddRange(["demolish_building","remove_planted","remove_vegetation","remove_debris"]); }
            if (enablePlacement) { expected.Add("unlock_building"); expected.Add("place_path"); expected.Add("set_building_priority"); } if (enableLodgePlacement) { expected.Add("place_building"); expected.Add("validate_building"); expected.Add("set_area"); expected.Add("place_lodge"); expected.Add("set_simulation_speed"); }
            Assert.Equal(expected.Order(), tools.Select(t => t.Name).Order());
            Assert.All(tools, t => { Assert.Equal(t.Name is not ("unlock_building" or "set_building_paused" or "set_storage_good" or "set_storage_mode" or "set_farm_priority" or "set_farm_crop" or "place_building" or "validate_building" or "validate_build_site" or "place_path" or "place_lodge" or "set_simulation_speed" or "set_workplace_staffing" or "set_building_priority" or "set_area" or "demolish_building" or "remove_planted" or "remove_vegetation" or "remove_debris"), t.ProtocolTool.Annotations!.ReadOnlyHint); Assert.NotNull(t.ProtocolTool.OutputSchema); });
            foreach(var name in new[]{"inspect_goods","inspect_alerts","inspect_alert_targets"}) {
                var args=new Dictionary<string,object?>{["offset"]=0,["limit"]=32,["reasoning"]="Versorgung lesend prüfen."};
                if(name=="inspect_alert_targets") {args["session"]=session;args["alertId"]=new string('0',64);}
                var result=await client.CallToolAsync(name,args,cancellationToken:ct);
                Assert.False(result.IsError);Assert.Equal(0,result.StructuredContent!.Value.GetProperty("data").GetProperty("total").GetInt32());
                Assert.Contains(activityLog.Snapshot(),e=>e.Tool==name&&e.State=="ok"&&e.Reasoning=="Versorgung lesend prüfen.");
                if(name=="inspect_alert_targets") {
                    args["session"]=Guid.NewGuid().ToString();
                    var rejected=await client.CallToolAsync(name,args,cancellationToken:ct);
                    Assert.True(rejected.IsError);Assert.Equal("stale_session",rejected.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString());
                }
            }
            foreach(var name in new[]{"inspect_building_access","inspect_road_connection","inspect_work_range","inspect_good_history"}){
                var a=new Dictionary<string,object?>();
                if(name=="inspect_good_history")a["good"]="Water";else{a["id"]=Guid.NewGuid().ToString();a["session"]=session;}
                if(name=="inspect_road_connection")a["toId"]=Guid.NewGuid().ToString();
                if(name is "inspect_work_range" or "inspect_good_history"){a["offset"]=0;a["limit"]=32;}
                Assert.False((await client.CallToolAsync(name,a,cancellationToken:ct)).IsError);
                Assert.Contains(activityLog.Snapshot(),e=>e.Tool==name&&e.State=="ok");
            }
            var researchResult=await client.CallToolAsync("inspect_research",new Dictionary<string,object?>{["offset"]=0,["limit"]=32},cancellationToken:ct);
            Assert.False(researchResult.IsError);
            Assert.Equal(20,researchResult.StructuredContent!.Value.GetProperty("data").GetProperty("sciencePoints").GetInt32());
            var unlockArgs=new Dictionary<string,object?>{["template"]="SyntheticResearch",["session"]=session,["expectedCost"]=10};
            if(enablePlacement) {
                unlockArgs["session"]=Guid.NewGuid().ToString("D");
                var rejected=await client.CallToolAsync("unlock_building",unlockArgs,cancellationToken:ct);
                Assert.True(rejected.IsError);
                Assert.Equal("stale_session",rejected.StructuredContent!.Value.GetProperty("error").GetProperty("code").GetString());
                Assert.False(rejected.StructuredContent.Value.GetProperty("error").GetProperty("retryable").GetBoolean());
                Assert.Contains(activityLog.Snapshot(),e=>e.Tool=="unlock_building"&&e.State=="rejected");
                Assert.Equal(20,sciencePoints);
                unlockArgs["session"]=session;
                var unlocked=await client.CallToolAsync("unlock_building",unlockArgs,cancellationToken:ct);
                Assert.False(unlocked.IsError);Assert.Equal("applied",unlocked.StructuredContent!.Value.GetProperty("data").GetProperty("outcome").GetString());
                var repeated=await client.CallToolAsync("unlock_building",unlockArgs,cancellationToken:ct);
                Assert.Equal("already_unlocked",repeated.StructuredContent!.Value.GetProperty("data").GetProperty("outcome").GetString());Assert.Equal(10,sciencePoints);
            } else { Assert.True((await client.CallToolAsync("unlock_building",unlockArgs,cancellationToken:ct)).IsError);Assert.Equal(20,sciencePoints); }
            var optionsResult=await client.CallToolAsync("inspect_build_options",new Dictionary<string,object?>{["offset"]=0,["limit"]=32,["reasoning"]="Bauoptionen für den nächsten regulären Auftrag prüfen."},cancellationToken:ct);
            Assert.False(optionsResult.IsError);
            var logResult=await client.CallToolAsync("inspect_agent_log",cancellationToken:ct);
            Assert.False(logResult.IsError);
            var entry=logResult.StructuredContent!.Value.GetProperty("data").GetProperty("items").EnumerateArray().Single(e=>e.GetProperty("tool").GetString()=="inspect_build_options");
            Assert.Equal("ok",entry.GetProperty("state").GetString());
            Assert.Equal("Bauoptionen für den nächsten regulären Auftrag prüfen.",entry.GetProperty("reasoning").GetString());
            Assert.DoesNotContain(token,logResult.StructuredContent.Value.GetRawText());
            Assert.Equal("WaterPump.Folktails",optionsResult.StructuredContent!.Value.GetProperty("data").GetProperty("items")[0].GetProperty("template").GetString());
            var buildArgs=new Dictionary<string,object?>{["template"]="WaterPump.Folktails",["x"]=1,["y"]=2,["z"]=3,["rotation"]=0};
            Assert.False((await client.CallToolAsync("precheck_building",buildArgs,cancellationToken:ct)).IsError);
            if(enableLodgePlacement){
                buildArgs["session"]=session;
                Assert.False((await client.CallToolAsync("validate_building",buildArgs,cancellationToken:ct)).IsError);
                buildArgs["actionId"]=Guid.NewGuid().ToString("D");
                Assert.False((await client.CallToolAsync("place_building",buildArgs,cancellationToken:ct)).IsError);
                Assert.True((await client.CallToolAsync("place_building",buildArgs,cancellationToken:ct)).IsError);
                buildArgs["actionId"]=Guid.NewGuid().ToString("D");buildArgs["x"]=4;
                Assert.False((await client.CallToolAsync("place_building",buildArgs,cancellationToken:ct)).IsError);
            }
            var colony = await client.CallToolAsync("inspect_colony", cancellationToken: ct);
            Assert.False(colony.IsError);
            var json = colony.StructuredContent!.Value;
            Assert.Equal(6, json.GetProperty("data").GetProperty("housing").GetProperty("totalBeds").GetInt32());
            Assert.Equal(session, json.GetProperty("meta").GetProperty("sessionId").GetString());
            Assert.Equal("native", json.GetProperty("meta").GetProperty("backend").GetString());
            var map = await client.CallToolAsync("inspect_map_region", new Dictionary<string, object?>
                { ["x"] = 1, ["y"] = 2, ["z"] = 3, ["width"] = 1, ["height"] = 1, ["depth"] = 1 }, cancellationToken: ct);
            Assert.False(map.IsError);
            Assert.Equal(0.5f, map.StructuredContent!.Value.GetProperty("data").GetProperty("cells")[0].GetProperty("waterDepth").GetSingle());
            var status = await client.CallToolAsync("timberborn_status", cancellationToken: ct);
            Assert.Equal(enablePlacement || enableLodgePlacement || enableValidation, status.StructuredContent!.Value.GetProperty("data").GetProperty("writesEnabled").GetBoolean());
            var objects = await client.CallToolAsync("find_buildings", new Dictionary<string, object?> { ["offset"] = 0, ["limit"] = 32 }, cancellationToken: ct);
            Assert.False(objects.IsError);
            Assert.Equal(0, objects.StructuredContent!.Value.GetProperty("data").GetProperty("total").GetInt32());
            var catalog = await client.CallToolAsync("inspect_build_catalog", cancellationToken: ct);
            Assert.False(catalog.IsError);
            Assert.Equal(12, catalog.StructuredContent!.Value.GetProperty("data").GetProperty("items")[0].GetProperty("costs")[0].GetProperty("required").GetInt32());
            var site = await client.CallToolAsync("precheck_build_site", new Dictionary<string, object?>
                { ["template"] = "Path", ["x"] = 1, ["y"] = 2, ["z"] = 3, ["rotation"] = 0 }, cancellationToken: ct);
            Assert.False(site.IsError);
            Assert.False(site.StructuredContent!.Value.GetProperty("data").GetProperty("gameValidated").GetBoolean());
            Assert.Equal("requires_game_validation", site.StructuredContent!.Value.GetProperty("data").GetProperty("assessment").GetString());
            Assert.Equal((enableLodgePlacement ? 12 : 8) + (enablePlacement ? 4 : 1) + 8, Volatile.Read(ref observations));
            if (enableValidation)
            {
                var validation = await client.CallToolAsync("validate_build_site", new Dictionary<string, object?>
                    { ["template"] = "Path", ["x"] = 1, ["y"] = 2, ["z"] = 3, ["rotation"] = 0, ["session"] = session }, cancellationToken: ct);
                Assert.False(validation.IsError);
                Assert.True(validation.StructuredContent!.Value.GetProperty("data").GetProperty("noPersistentChangeObserved").GetBoolean());
                Assert.Equal((enableLodgePlacement ? 13 : 9) + (enablePlacement ? 4 : 1) + 8, Volatile.Read(ref observations));
            }
            if (enablePlacement)
            {
                var args = new Dictionary<string, object?> { ["template"] = "Path", ["x"] = 1, ["y"] = 2, ["z"] = 3, ["rotation"] = 0, ["session"] = session };
                var placed = await client.CallToolAsync("place_path", args, cancellationToken: ct);
                Assert.False(placed.IsError);
                Assert.Equal("applied", placed.StructuredContent!.Value.GetProperty("data").GetProperty("outcome").GetString());
                // Synthetic transport retry cannot cause a second mutation through the one-shot gate.
                var repeated = await client.CallToolAsync("place_path", args, cancellationToken: ct);
                Assert.True(repeated.IsError);
                Assert.False(repeated.StructuredContent!.Value.GetProperty("error").GetProperty("retryable").GetBoolean());
            }
            if (enableLodgePlacement)
            {
                var args = new Dictionary<string, object?> { ["template"] = "Lodge.Folktails", ["x"] = 1, ["y"] = 2, ["z"] = 3, ["rotation"] = 0, ["session"] = session };
                var lodge = await client.CallToolAsync("place_lodge", args, cancellationToken: ct);
                Assert.Equal(enablePlacement, lodge.IsError == true);
                if (!enablePlacement) {
                    Assert.Equal("Lodge.Folktails", lodge.StructuredContent!.Value.GetProperty("data").GetProperty("template").GetString());
                    Assert.False(lodge.StructuredContent!.Value.GetProperty("data").GetProperty("finished").GetBoolean());
                }
                var repeated = await client.CallToolAsync("place_lodge", args, cancellationToken: ct);
                Assert.True(repeated.IsError);
                Assert.False(repeated.StructuredContent!.Value.GetProperty("error").GetProperty("retryable").GetBoolean());
            }
            if (enableValidation) {
                var id = Guid.NewGuid().ToString("D");
                foreach (var target in new[] { 3, 2 }) {
                    var staffed = await client.CallToolAsync("set_workplace_staffing", new Dictionary<string, object?> { ["id"] = id, ["session"] = session, ["desiredWorkers"] = target, ["expectedDesiredWorkers"] = desiredWorkers }, cancellationToken: ct);
                    Assert.False(staffed.IsError);
                    Assert.Equal(target, staffed.StructuredContent!.Value.GetProperty("data").GetProperty("observedDesiredWorkers").GetInt32());
                }
                var stale = await client.CallToolAsync("set_workplace_staffing", new Dictionary<string, object?> { ["id"] = id, ["session"] = session, ["desiredWorkers"] = 0, ["expectedDesiredWorkers"] = 3 }, cancellationToken: ct);
                Assert.True(stale.IsError); Assert.Equal(2, desiredWorkers);
                Assert.False(stale.StructuredContent!.Value.GetProperty("error").GetProperty("retryable").GetBoolean());
            }
            var removalRead=await client.CallToolAsync("inspect_removal_targets",new Dictionary<string,object?>{["kind"]="all",["x"]=0,["y"]=0,["z"]=0,["width"]=1,["height"]=1,["depth"]=1,["offset"]=0,["limit"]=32},cancellationToken:ct);
            Assert.False(removalRead.IsError);
            if(enableValidation) foreach(var name in new[]{"demolish_building","remove_planted","remove_vegetation","remove_debris"}) {
                bool delete=name is "demolish_building" or "remove_debris";
                var a=new Dictionary<string,object?>{["id"]=Guid.NewGuid().ToString(),["session"]=session,["template"]="Synthetic",["x"]=1,["y"]=2,["z"]=3,["operation"]=delete?"delete":"mark",["expectedMarked"]=false};
                Assert.False((await client.CallToolAsync(name,a,cancellationToken:ct)).IsError);
            }
            var readPriority = await client.CallToolAsync("inspect_building_priority", new Dictionary<string, object?> { ["id"] = Guid.NewGuid().ToString("D"), ["session"] = session, ["kind"] = "workplace" }, cancellationToken: ct);
            Assert.False(readPriority.IsError);
            foreach(var read in new[]{"inspect_construction","inspect_areas"}) {
                var a=new Dictionary<string,object?>{["offset"]=0,["limit"]=32}; if(read=="inspect_areas")a["kind"]="tree_cutting";
                Assert.False((await client.CallToolAsync(read,a,cancellationToken:ct)).IsError);
            }
            Assert.False((await client.CallToolAsync("inspect_area_types",cancellationToken:ct)).IsError);
            if(enablePlacement) foreach(var kind in new[]{"workplace","construction"}) foreach(var p in new[]{"High","Normal"}) {
                var a=new Dictionary<string,object?>{["id"]=Guid.NewGuid().ToString("D"),["session"]=session,["kind"]=kind,["priority"]=p,["expectedPriority"]=priority};
                Assert.False((await client.CallToolAsync("set_building_priority",a,cancellationToken:ct)).IsError);
            }
            if(enableLodgePlacement) foreach(var op in new[]{"mark","remove"}) {
                var a=new Dictionary<string,object?>{["kind"]="tree_cutting",["operation"]=op,["resource"]="",["expectedResource"]=areaState,["x"]=1,["y"]=2,["z"]=3,["width"]=1,["height"]=1,["session"]=session};
                Assert.False((await client.CallToolAsync("set_area",a,cancellationToken:ct)).IsError);
            }
            var workforce = await client.CallToolAsync("inspect_workforce", new Dictionary<string, object?> { ["offset"] = 0, ["limit"] = 32 }, cancellationToken: ct);
            Assert.False(workforce.IsError);
            Assert.Equal(0, workforce.StructuredContent!.Value.GetProperty("data").GetProperty("total").GetInt32());
            var simulation = await client.CallToolAsync("inspect_simulation", cancellationToken: ct);
            Assert.Equal(1, simulation.StructuredContent!.Value.GetProperty("data").GetProperty("currentSpeed").GetSingle());
            if (enableLodgePlacement)
            {
                int expectedSpeed = 1;
                foreach (int speed in new[] { 0, 1, 3, 7, 1 }) {
                    var result = await client.CallToolAsync("set_simulation_speed", new Dictionary<string, object?> { ["speed"] = speed, ["expectedSpeed"] = expectedSpeed, ["session"] = session }, cancellationToken: ct);
                    Assert.False(result.IsError);
                    var readback = await client.CallToolAsync("inspect_simulation", cancellationToken: ct);
                    Assert.Equal(speed, readback.StructuredContent!.Value.GetProperty("data").GetProperty("currentSpeed").GetSingle());
                    expectedSpeed = speed;
                }
                var stale = await client.CallToolAsync("set_simulation_speed", new Dictionary<string, object?> { ["speed"] = 0, ["expectedSpeed"] = 0, ["session"] = session }, cancellationToken: ct);
                Assert.True(stale.IsError);
                Assert.Equal(1, currentSpeed);
            }
            var settingsId=Guid.NewGuid().ToString("D");
            var settingsRead=await client.CallToolAsync("inspect_building_settings",new Dictionary<string,object?>{["id"]=settingsId,["session"]=session},cancellationToken:ct);
            Assert.False(settingsRead.IsError);
            if(enableValidation){
                foreach(var (tool,key,before,after) in new[]{("set_building_paused","paused","false","true"),("set_storage_good","good","","Berries"),
                    ("set_storage_mode","mode","accept","obtain"),("set_storage_mode","mode","obtain","supply"),("set_storage_mode","mode","supply","empty"),("set_storage_mode","mode","empty","accept"),
                    ("set_farm_priority","priority","harvesting","planting"),("set_farm_crop","resource","","Carrot"),("set_storage_good","good","Berries","")}){
                    var settingArgs=new Dictionary<string,object?>{["id"]=settingsId,["session"]=session,[key]=key=="paused"?true:after,["expected"+char.ToUpperInvariant(key[0])+key[1..]]=key=="paused"?false:before};
                    var change=await client.CallToolAsync(tool,settingArgs,cancellationToken:ct);Assert.False(change.IsError);
                    Assert.Equal(after,change.StructuredContent!.Value.GetProperty("data").GetProperty("observedValue").GetString());
                    Assert.False((await client.CallToolAsync("inspect_building_settings",new Dictionary<string,object?>{["id"]=settingsId,["session"]=session},cancellationToken:ct)).IsError);
                }
                var stale=await client.CallToolAsync("set_storage_good",new Dictionary<string,object?>{["id"]=settingsId,["session"]=session,["good"]="Carrot",["expectedGood"]="Berries"},cancellationToken:ct);
                Assert.True(stale.IsError);Assert.Equal("",settingValues["good"]);
                var resumed=await client.CallToolAsync("set_building_paused",new Dictionary<string,object?>{["id"]=settingsId,["session"]=session,["paused"]=false,["expectedPaused"]=true},cancellationToken:ct);
                Assert.False(resumed.IsError);Assert.Equal("false",settingValues["paused"]);
            }
            var building = await client.CallToolAsync("inspect_building", new Dictionary<string, object?>
                { ["id"] = Guid.NewGuid().ToString("D"), ["session"] = session }, cancellationToken: ct);
            Assert.False(building.IsError);
            Assert.False(building.StructuredContent!.Value.GetProperty("data").GetProperty("found").GetBoolean());
        }
        finally
        {
            pumpStop.Cancel(); await pump;
            if (File.Exists(configPath)) File.Delete(configPath);
        }
    }

    private static NativeSnapshot Snapshot() => new("global",
        [new("Water", true, 10, 10, 10, 0, 20, 20), new("Berries", true, 4, 4, 4, 0, 20, 20), new("Log", true, 2, 2, 2, 0, 20, 20)],
        new(4, 2, 0, 6), new(4, 0, 0), new(new(4, 0, 1, 3, 0), new(0, 0, 0, 0, 0)),
        new(32, 32, 16), 0, [], false, ["synthetic_test"]);
}
