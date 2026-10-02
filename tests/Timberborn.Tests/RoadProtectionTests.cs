using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;
using System.Net;
using System.Text.Json;
using Timberborn.McpServer;
namespace Timberborn.Tests;

public sealed class RoadProtectionTests
{
    [Fact]
    public void AlternativeNativeConnectionPreservesAccess()
    {
        Assert.Empty(RoadProtectionPolicy.LostConnections([true,true,false], [true,true,true]));
        Assert.Equal([1],RoadProtectionPolicy.LostConnections([true,true,false], [true,false,false]));
    }
    [Fact]
    public void IncompleteComparisonCannotClaimSafety() =>
        Assert.Throws<ArgumentException>(()=>RoadProtectionPolicy.LostConnections([true], []));

    [Theory]
    [InlineData(true,true,true,0,"safe")]
    [InlineData(true,true,false,0,"unknown")]
    [InlineData(true,true,false,1,"blocked")]
    [InlineData(true,true,true,1,"blocked")]
    [InlineData(false,true,true,0,"unknown")]
    [InlineData(true,false,true,0,"unknown")]
    [InlineData(false,true,true,1,"unknown")]
    [InlineData(true,false,true,1,"unknown")]
    public void IncompleteOrContaminatedEvidenceFailsClosed(bool baseline,bool restored,bool covered,int lost,string expected) =>
        Assert.Equal(expected,RoadProtectionPolicy.Decision(baseline,restored,covered,lost));

    private static NativeRoadProtection Evidence() => new("safe",[],4,0,true,true,[],false,[new(1,2,3)],[]);

    [Theory]
    [InlineData("road_cell")] [InlineData("construction_access")] [InlineData("building_access")]
    public void TypedLossRemainsBlocked(string kind)
    {
        var d = Evidence() with { Status = "blocked", ConnectedBefore = 2, RoadProbeCount = 1,
            ConstructionProbeCount = 1, LostConnections = 1,
            Affected = [new(Guid.NewGuid(), new(1,2,3), new(4,5,6), kind)] };
        RoadProtectionContract.Validate(d, false, true, true);
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(d, true, true, true));
    }

    [Theory]
    [InlineData("missing_counts")] [InlineData("negative")] [InlineData("overflow")]
    [InlineData("missing_kind")] [InlineData("unknown_kind")]
    public void TypedReceiptRequiresBoundedCountsAndKnownTargets(string defect)
    {
        var d = Evidence() with { Status = "blocked", ConnectedBefore = 2, RoadProbeCount = 1,
            ConstructionProbeCount = 1, LostConnections = 1,
            Affected = [new(Guid.NewGuid(), new(1,2,3), new(4,5,6), "road_cell")] };
        d = defect switch {
            "missing_counts" => d with { RoadProbeCount = null },
            "negative" => d with { ConstructionProbeCount = -1 },
            "overflow" => d with { RoadProbeCount = 4096 },
            "missing_kind" => d with { Affected = [d.Affected[0] with { Kind = null }] },
            _ => d with { Affected = [d.Affected[0] with { Kind = "invented" }] }
        };
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(d, false, true, true));
    }

    [Theory]
    [InlineData(null)] [InlineData(-1)] [InlineData(0)] [InlineData(5)]
    public void NewReceiptCannotClaimSafetyWithoutConnectedBaseline(int? count) =>
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(Evidence() with { ConnectedBefore = count }, true, true));

    [Fact]
    public void EmptyBaselineIsDiagnosticOnly()
    {
        var d = Evidence() with { Status = "unknown", ConstructionCovered = false, ConnectedBefore = 0 };
        RoadProtectionContract.Validate(d, false, true);
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(d, true, true));
        RoadProtectionContract.Validate(Evidence() with { ConnectedBefore = 2 }, true, true);
    }

    [Fact]
    public void LossCannotExceedPreviouslyConnectedCount()
    {
        var d = Evidence() with { Status = "blocked", LostConnections = 1, ConnectedBefore = 0,
            Affected = [new(Guid.NewGuid(), new(1,2,3), new(4,5,6))] };
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(d, false, true));
    }

    [Fact]
    public void CompletedCoverageIsRequiredForPlacement()
    {
        RoadProtectionContract.Validate(Evidence(),true);
        var unknown=Evidence() with{Status="unknown",ConstructionCovered=false,Reasons=["construction_and_road_node_coverage_unproven"]};
        RoadProtectionContract.Validate(unknown,false);
        Assert.Throws<InvalidDataException>(()=>RoadProtectionContract.Validate(unknown,true));
    }
    [Theory]
    [InlineData("missing")] [InlineData("restoration")] [InlineData("construction")]
    [InlineData("lost")] [InlineData("truncated")] [InlineData("empty_block")]
    [InlineData("null_cells")] [InlineData("count")]
    public void MalformedEvidenceRejected(string defect)
    {
        var d=defect switch {
            "missing"=>null,
            "restoration"=>Evidence() with{Restored=false},
            "construction"=>Evidence() with{ConstructionCovered=false},
            "lost"=>Evidence() with{LostConnections=1},
            "truncated"=>Evidence() with{AffectedTruncated=true},
            "empty_block"=>Evidence() with{Status="blocked"},
            "null_cells"=>Evidence() with{CandidateCells=null!},
            _=>Evidence() with{CheckedConnections=16385}
        };
        Assert.Throws<InvalidDataException>(()=>RoadProtectionContract.Validate(d,false));
    }
    [Fact]
    public void ConfirmedLossIncludesAffectedEntranceAndCannotBeApplied()
    {
        var d=Evidence() with{Status="blocked",LostConnections=1,ConstructionCovered=false,
            Affected=[new(Guid.NewGuid(),new(1,2,3),new(4,5,6))],Reasons=["existing_district_access_lost"]};
        RoadProtectionContract.Validate(d,false);
        Assert.Throws<InvalidDataException>(()=>RoadProtectionContract.Validate(d,true));
    }

    [Theory]
    [InlineData("place_building",false,false,"0.23.0")]
    [InlineData("place_building",false,false,"0.23.1")]
    [InlineData("place_building",false,false,"0.23.2")]
    [InlineData("place_building",true,true,"0.23.0")]
    [InlineData("place_building",true,true,"0.23.1")]
    [InlineData("place_building",true,true,"0.23.2")]
    [InlineData("place_building",true,false,"0.23.0")]
    [InlineData("place_building",true,false,"0.23.1")]
    [InlineData("place_building",true,false,"0.23.2")]
    [InlineData("validate_building",false,false,"0.23.0")]
    [InlineData("validate_building",false,false,"0.23.1")]
    [InlineData("validate_building",false,false,"0.23.2")]
    [InlineData("validate_building",true,false,"0.23.0")]
    [InlineData("validate_building",true,false,"0.23.1")]
    [InlineData("validate_building",true,false,"0.23.2")]
    public async Task NewBridgeRequiresGuardEvidenceAndRejectsUnsafeSuccess(string tool,bool evidence,bool applied,string version)
    {
        var session=Guid.NewGuid().ToString("D");var id=Guid.NewGuid();
        var guard=evidence?Evidence() with{Status="unknown",ConstructionCovered=false,ConnectedBefore=2,RoadProbeCount=1,ConstructionProbeCount=0,Reasons=["construction_and_road_node_coverage_unproven"]}:null;
        object data=tool=="place_building" ? new NativePlacement("Path",new(1,2,3),0,id,applied?"applied":"rejected",null,false,[],guard)
            : new NativeValidation("Path",new(1,2,3),0,true,true,true,false,255,[],guard);
        var payload=JsonSerializer.Serialize(new BridgeEnvelope<object>(1,session,DateTimeOffset.UtcNow,version,data),NativeJson.Options);
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),new Handler(payload)),enableBuildingPlacement:true);
        var args=new Dictionary<string,object?> { ["session"]=session,["template"]="Path",["x"]=1,["y"]=2,["z"]=3,["rotation"]=0 };
        if(tool=="place_building")args["actionId"]=id.ToString("D");
        var result=await tools.Invoke(tool,JsonSerializer.SerializeToElement(args),TestContext.Current.CancellationToken);
        Assert.Equal(evidence&&!applied?"ok":"error",result["status"]!.GetValue<string>());
        if(evidence&&!applied)Assert.Equal("unknown",result["data"]!["roadProtection"]!["status"]!.GetValue<string>());
        else Assert.Equal("backend_incompatible",result["error"]!["code"]!.GetValue<string>());
    }
    private sealed class Handler(string json):HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});
    }
}
