using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class RunReportPresentationTests
{
    private static JsonObject Report()=>JsonNode.Parse("""
    {"schemaVersion":1,"status":"ok","meta":{"backend":"native","simulated":true},"error":null,
     "data":{"runId":"synthetic","state":"completed","reason":"production_measurement_complete","terminal":true,
       "pauseConfirmed":true,"observedSpeed":0,"elapsedGameHours":3,"elapsedRealSeconds":20,
       "startGameHours":24,"observedGameHours":27,"targetGameHours":27,"maxRealSeconds":60,"targetReached":true,
       "buildingIds":["synthetic-building"],"buildingJournalCursor":7,"buildingProductionReached":true,"measureProduction":true,
       "goodTargets":[],"stopAtScience":null,"observedScience":null,"productionTargetsReached":false,
       "productionDiagnostics":null,"diagnosticsObservedGameHours":null,"stopAtAvailableLogs":null,"observedAvailableLogs":null,"stockTargetReached":false,
       "buildingProduction":[{"id":"synthetic-building","template":"PaperMill.Folktails","recipe":"Paper","completedCycles":1,
         "provenGameHours":25,"observedGameHours":27,"blockers":[],"seenBlockers":["output_capacity_unavailable"],
         "measurement":{"completedCycles":2,"measuredHours":3,"cyclesPerHour":0.6666666666666666,
           "products":[{"good":"Paper","amountPerCycle":2}],"sciencePerCycle":0,"recipeDerivedScience":0,"recipeDerivedSciencePerHour":0,
           "recipeDerivedProducts":[{"good":"Paper","amount":4,"perGameHour":1.3333333333333333}],
           "times":[{"state":"producing","hours":2},{"state":"storage","hours":0.65625},{"state":"unknown","hours":0.34375},{"state":"staffing","hours":0}],
           "largestObservedStall":"storage"}}],
       "bottleneckObservedGameHours":29,"bottleneckState":"observed","currentBuildingOperations":{"nativeReads":1,"limitations":["fresh_observation"],
         "items":[{"id":"synthetic-building","state":"observed","operation":{"id":"synthetic-building","template":"PaperMill.Folktails","finished":true,"paused":false,
           "assessment":{"blockers":[],"warnings":[],"unknown":["transport"],"productionProven":false,"findings":[{"code":"no_observed_bottleneck"}]},
           "workplace":{"assigned":1},"manufacturing":{"ready":true},"statuses":[],"limitations":["synthetic"],"inventories":[{"goods":[]}],"power":{"powered":true},"bottleneck":{"complete":false}}}]}}}
    """)!.AsObject();

    [Fact]
    public void CompactRetainsExactValuesPauseAndIndependentFreshDiagnosisWithoutMutatingReceipt()
    {
        var full=Report();string original=full.ToJsonString();var compact=ResponsePresentation.Present("inspect_simulation_run",full,false);
        var d=compact["data"]!;var b=d["buildingProduction"]![0]!;var m=b["measurement"]!;
        Assert.Equal(2,m["completedCycles"]!.GetValue<int>());Assert.Equal(1,b["completedCycles"]!.GetValue<int>());
        Assert.Equal(full["data"]!["buildingProduction"]![0]!["measurement"]!["recipeDerivedProducts"]!.ToJsonString(),m["recipeDerivedProducts"]!.ToJsonString());
        Assert.Equal(3,m["times"]!.AsArray().Count);Assert.Equal(3,m["times"]!.AsArray().Sum(t=>t!["hours"]!.GetValue<double>()));
        Assert.Equal(0.6666666666666666,m["cyclesPerHour"]!.GetValue<double>());
        Assert.True(d["pauseConfirmed"]!.GetValue<bool>());Assert.True(d["terminal"]!.GetValue<bool>());
        Assert.Equal(27,b["observedGameHours"]!.GetValue<int>());Assert.Equal(29,d["bottleneckObservedGameHours"]!.GetValue<int>());
        Assert.Equal("transport",d["currentBuildingOperations"]!["items"]![0]!["operation"]!["assessment"]!["unknown"]![0]!.GetValue<string>());
        Assert.Null(m["products"]);Assert.Null(d["buildingIds"]);Assert.Null(d["currentBuildingOperations"]!["items"]![0]!["operation"]!["inventories"]);
        Assert.Equal(original,full.ToJsonString());Assert.Equal(original,ResponsePresentation.Present("inspect_simulation_run",full,true).ToJsonString());
        Assert.True(compact.ToJsonString().Length<original.Length);
    }
    [Theory]
    [InlineData("failed")]
    [InlineData("interrupted")]
    [InlineData("pausing")]
    public void FailurePauseAndHistoricalBlockersCannotDisappear(string state)
    {
        var r=Report();r["data"]!["state"]=state;r["data"]!["pauseConfirmed"]=false;
        r["data"]!["bottleneckState"]="unavailable";r["data"]!["currentBuildingOperations"]=null;
        var compact=ResponsePresentation.Present("inspect_simulation_run",r,false)["data"]!;
        Assert.Equal(state,compact["state"]!.GetValue<string>());Assert.False(compact["pauseConfirmed"]!.GetValue<bool>());
        Assert.Equal("unavailable",compact["bottleneckState"]!.GetValue<string>());Assert.NotNull(compact["bottleneckObservedGameHours"]);
        Assert.Single(compact["buildingProduction"]![0]!["seenBlockers"]!.AsArray());
    }
    [Fact]
    public void CurrentFloodedStateSurvivesCompactEvenAfterHistoricalProduction()
    {
        var full=Report();
        var operation=full["data"]!["currentBuildingOperations"]!["items"]![0]!["operation"]!;
        operation["water"]=new JsonObject { ["floodComponentPresent"]=true,["flooded"]=true };
        operation["assessment"]!["blockers"]=new JsonArray("building_flooded");
        operation["assessment"]!["findings"]=new JsonArray(new JsonObject { ["code"]="building_flooded" });
        var compact=ResponsePresentation.Present("inspect_simulation_run",full,false)["data"]!;
        Assert.True(compact["currentBuildingOperations"]!["items"]![0]!["operation"]!["water"]!["flooded"]!.GetValue<bool>());
        Assert.Equal(1,compact["buildingProduction"]![0]!["completedCycles"]!.GetValue<int>());
        Assert.False(compact["currentBuildingOperations"]!["items"]![0]!["operation"]!["assessment"]!["productionProven"]!.GetValue<bool>());
    }
    [Fact]
    public void ScienceAndStockTargetsAndNullRatesRemainVisible()
    {
        var r=Report();var data=r["data"]!;var m=data["buildingProduction"]![0]!["measurement"]!;
        m["sciencePerCycle"]=1.0;m["recipeDerivedScience"]=2.0;m["recipeDerivedSciencePerHour"]=null;m["cyclesPerHour"]=null;
        var compact=ResponsePresentation.Present("inspect_simulation_run",r,false);
        Assert.Equal(2.0,compact["data"]!["buildingProduction"]![0]!["measurement"]!["recipeDerivedScience"]!.GetValue<double>());
        Assert.True(compact["data"]!["buildingProduction"]![0]!["measurement"]!.AsObject().ContainsKey("cyclesPerHour"));
        data["buildingProduction"]=new JsonArray();data["stopAtScience"]=50;data["observedScience"]=null;
        data["goodTargets"]=JsonNode.Parse("[{\"good\":\"Paper\",\"amount\":20,\"available\":10}]");
        compact=ResponsePresentation.Present("inspect_simulation_run",r,false);
        Assert.NotNull(compact["data"]!["goodTargets"]);Assert.Equal(50,compact["data"]!["stopAtScience"]!.GetValue<int>());
        Assert.True(compact["data"]!.AsObject().ContainsKey("observedScience"));Assert.Null(compact["data"]!["buildingProduction"]);
    }
    [Fact]
    public void AllFourRunToolsOfferDetailWithoutForwardingItOrRepeatingAction()
    {
        foreach(var tool in SimulationRunTools.Catalog(true)) {
            var described=ResponsePresentation.Describe(tool);
            Assert.Equal("compact",described.InputSchema.GetProperty("properties").GetProperty("detail").GetProperty("default").GetString());
            Assert.DoesNotContain(described.InputSchema.GetProperty("required").EnumerateArray(),v=>v.GetString()=="detail");
            CheckOptional(described.OutputSchema!.Value);
            var parsed=ResponsePresentation.ReadRequest(tool.Name,JsonSerializer.SerializeToElement(new {detail="full",runId="synthetic"}));
            Assert.True(parsed.Full);Assert.False(parsed.Arguments.TryGetProperty("detail",out _));
            Assert.Equal(Report().ToJsonString(),ResponsePresentation.Present(tool.Name,Report(),true).ToJsonString());
        }
        using var bad=JsonDocument.Parse("{\"detail\":\"full\",\"detail\":\"compact\"}");
        Assert.Throws<ArgumentException>(()=>ResponsePresentation.ReadRequest("run_simulation_for",bad.RootElement));
        var error=JsonNode.Parse("{\"status\":\"error\",\"error\":{\"execution\":\"unconfirmed\"}}")!.AsObject();
        Assert.Equal(error.ToJsonString(),ResponsePresentation.Present("run_simulation_for",error,false).ToJsonString());
        var result=ResponsePresentation.ToToolResult(ResponsePresentation.Present("inspect_simulation_run",Report(),false));
        Assert.Equal(result.StructuredContent!.Value.GetRawText(),Assert.IsType<ModelContextProtocol.Protocol.TextContentBlock>(Assert.Single(result.Content)).Text);
    }
    private static void CheckOptional(JsonElement schema)
    {
        if(schema.ValueKind==JsonValueKind.Object) {
            if(schema.TryGetProperty("required",out var required))
                Assert.DoesNotContain(required.EnumerateArray(),v=>RunReportPresentation.OptionalFields.Contains(v.GetString()));
            foreach(var property in schema.EnumerateObject())CheckOptional(property.Value);
        } else if(schema.ValueKind==JsonValueKind.Array)foreach(var item in schema.EnumerateArray())CheckOptional(item);
    }
}
