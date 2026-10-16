using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class StrategicScanPresentationTests
{
    private static JsonObject Report()
    {
        var result=JsonNode.Parse("""
        {"status":"ok","meta":{"simulated":true},"error":null,"data":{
          "scanId":"synthetic","state":"complete_non_atomic","nextStep":"inspect_priority_closures_and_plan_protection_before_expansion",
          "protectedZone":{"x":6,"y":2,"z":1,"width":1,"height":1},
          "coverage":{"terrainComplete":true,"atomic":false,"observationSpreadSeconds":3},
          "analysis":{"corridors":[],"limitations":["not_a_flood_forecast"],"hazards":{
            "potentialInflowNodes":20,"representedPotentialInflowNodes":2,"priorities":[{
              "origin":{"x":0,"y":2,"z":1},"priority":"high_contaminated_inflow_potential",
              "observedLevel":4,"minimumTerrainPassLevel":2,"contamination":1,
              "targetEntry":{"x":6,"y":2,"z":1},"closureAssessment":"blocked_by_cave_roof",
              "candidate":{"at":{"x":3,"y":2,"z":2},"assumedBarrierCrest":5,"crossSection":[],"nearbyTerrainBanks":[],"requiredChecks":[]},
              "measures":["resurvey_origin_cross_section_and_target_entry","find_alternative_barrier_or_bypass_closure_before_building"],
              "unresolved":["existing_waterworks_unknown","no_explosion_countdown_or_arrival_time"]
            }]}},"tiles":[],"specialObjects":[],"rivers":{"sources":[],"sections":[],"links":[]}}}
        """)!.AsObject();
        var data=result["data"]!.AsObject();
        var priorities=data["analysis"]!["hazards"]!["priorities"]!.AsArray();
        var second=priorities[0]!.DeepClone().AsObject();
        second["closureAssessment"]="terrain_route_cut_at_observed_level";
        second["measures"]![1]="plan_barrier_at_candidate_cross_section_with_native_build_validation";
        priorities.Add(second);
        for(int i=0;i<64;i++) data["tiles"]!.AsArray().Add(new JsonObject{["tile"]=i,["wetColumns"]=20,["caveColumns"]=10});
        return result;
    }

    [Fact]
    public void CompactRetainsThreatAndActionButMovesInventoryToFull()
    {
        var raw=Report();var original=raw.ToJsonString();
        var compact=ResponsePresentation.Present(StrategicMapTools.Name,raw,false);
        var data=compact["data"]!;var hazards=data["analysis"]!["hazards"]!;
        Assert.Null(data["tiles"]);Assert.Null(data["rivers"]);Assert.Null(data["analysis"]!["corridors"]);
        Assert.Equal(20,hazards["potentialInflowNodes"]!.GetValue<int>());
        Assert.Equal(raw["data"]!["coverage"]!.ToJsonString(),data["coverage"]!.ToJsonString());
        Assert.Equal(raw["data"]!["protectedZone"]!.ToJsonString(),data["protectedZone"]!.ToJsonString());
        var first=hazards["priorities"]![0]!;
        Assert.Equal(4,first["observedLevel"]!.GetValue<int>());
        Assert.Equal(2,first["minimumTerrainPassLevel"]!.GetValue<int>());
        Assert.Equal("blocked_by_cave_roof",first["closureAssessment"]!.GetValue<string>());
        Assert.Equal(3,first["candidate"]!["at"]!["x"]!.GetValue<int>());
        Assert.Equal(5,first["candidate"]!["assumedBarrierCrest"]!.GetValue<int>());
        Assert.Null(first["candidate"]!["crossSection"]);
        Assert.Contains(hazards["commonMeasures"]!.AsArray(),n=>n!.GetValue<string>()=="resurvey_origin_cross_section_and_target_entry");
        Assert.Contains(first["measures"]!.AsArray(),n=>n!.GetValue<string>()=="find_alternative_barrier_or_bypass_closure_before_building");
        Assert.Contains(hazards["priorities"]![1]!["measures"]!.AsArray(),n=>n!.GetValue<string>()=="plan_barrier_at_candidate_cross_section_with_native_build_validation");
        Assert.Equal(2,hazards["commonUnresolved"]!.AsArray().Count);
        Assert.True(compact.ToJsonString().Length < original.Length * 0.6);
        Assert.Equal(original,raw.ToJsonString());
        Assert.Equal(original,ResponsePresentation.Present(StrategicMapTools.Name,raw,true).ToJsonString());
    }

    [Fact]
    public void DefaultIsCompactAndDetailDoesNotReachScanIdentity()
    {
        var tool=ResponsePresentation.Describe(StrategicMapTools.Reader());
        Assert.Equal("compact",tool.InputSchema.GetProperty("properties").GetProperty("detail").GetProperty("default").GetString());
        var compact=ResponsePresentation.ReadRequest(StrategicMapTools.Name,JsonSerializer.SerializeToElement(new{scanId="synthetic"}));
        var full=ResponsePresentation.ReadRequest(StrategicMapTools.Name,JsonSerializer.SerializeToElement(new{scanId="synthetic",detail="full"}));
        Assert.False(compact.Full);Assert.True(full.Full);
        Assert.Equal(compact.Arguments.GetRawText(),full.Arguments.GetRawText());
        Assert.Throws<ArgumentException>(()=>ResponsePresentation.ReadRequest(StrategicMapTools.Name,JsonSerializer.SerializeToElement(new{detail="invalid"})));
    }

    [Fact]
    public void ErrorsStayComplete()
    {
        var error=JsonNode.Parse("""{"status":"error","data":null,"error":{"code":"strategic_scan_missing_or_expired_start_new_scan","execution":"not_applicable"}}""")!.AsObject();
        Assert.Equal(error.ToJsonString(),ResponsePresentation.Present(StrategicMapTools.Name,error,false).ToJsonString());
    }
}
