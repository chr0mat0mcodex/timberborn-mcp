using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class RegionGroupSearchTests
{
    private static SurveyRequest Request=>new("11111111-1111-4111-8111-111111111111",
        "22222222-2222-4222-8222-222222222222","A",0,0,3,8,8);
    private static SurveyCandidate C(int x,int z=3)=>new(new(0,0,3,8,8),0,0,new string('a',64),new(x,2,z),0,
        OccupiedCells:[new(x,2,z)],SolidCells:[new(x,2,z)]);
    private static BridgeEnvelope<JsonElement> Reply(LargeProjectStep[] targets,int paths=1,int extraHelpers=0,bool ok=true,ReservedArea[]? reserved=null)
    {
        var steps=targets.Concat(Enumerable.Range(0,paths).Select(i=>new LargeProjectStep{Template="Path",X=i,Y=0,Z=3}))
            .Concat(Enumerable.Range(0,extraHelpers).Select(i=>new LargeProjectStep{Template="Platform.Folktails",X=i,Y=1,Z=3})).ToArray();
        return new(1,Request.Session,DateTimeOffset.UnixEpoch,"0.51.0",JsonSerializer.SerializeToElement(new{
            candidateFound=ok,reason=ok?"candidate":"blocked",steps,cleanupCandidateIndices=Array.Empty<int>(),
            reservedAreasKey=ReservedArea.Encode(reserved??[]),reservedAreasClear=true,
            plan=new{pilotEligible=ok,restored=true,planKey=new string('b',64)}},NativeJson.Options));
    }
    private static Task<JsonObject> Search(string[] templates,Dictionary<string,SurveyCandidate[]> choices,
        Func<LargeProjectStep[],CancellationToken,Task<BridgeEnvelope<JsonElement>>> check,int budget=32,ReservedArea[]? reserved=null,
        CancellationToken? token=null)=>RegionGroupSearch.Plan(Request,templates,4,budget,reserved??[],choices,check,token??TestContext.Current.CancellationToken);
    [Fact]
    public async Task BacktrackingMovesEarlierBuildingToFitWholeGroup()
    {
        var choices=new Dictionary<string,SurveyCandidate[]>{{"A",[C(0),C(1)]},{"B",[C(0)]}};
        var old=await RegionGroupGreedyBaseline.Plan(Request,["A","B"],4,choices,(s,_)=>Task.FromResult(Reply(s)),TestContext.Current.CancellationToken);
        Assert.Equal("partial",old["state"]!.GetValue<string>());
        var r=await Search(["A","B"],choices,(s,_)=>Task.FromResult(Reply(s)));
        Assert.Equal("complete",r["state"]!.GetValue<string>());
        Assert.Equal(1,r["steps"]![0]!["x"]!.GetValue<int>());
        Assert.Equal(0,r["steps"]![1]!["x"]!.GetValue<int>());
        Assert.True(r["search"]!["backtracks"]!.GetValue<int>()>0);
    }
    [Fact]
    public async Task MoreBuildingsWinBeforePathsThenHelpersBreakTies()
    {
        var r=await Search(["A","B"],new(){{"A",[C(0),C(1),C(2)]},{"B",[C(3)]}},
            (s,_)=>Task.FromResult(Reply(s,paths:s[0].X==0?3:1,extraHelpers:s[0].X==1?2:0)));
        Assert.Equal(2,r["planned"]!.GetValue<int>());Assert.Equal(2,r["steps"]![0]!["x"]!.GetValue<int>());
        Assert.Equal(1,r["score"]!["newPaths"]!.GetValue<int>());Assert.Equal(1,r["score"]!["helpers"]!.GetValue<int>());
    }
    [Fact]
    public async Task SkippingEarlyTargetCanProduceBetterPartialPlan()
    {
        var r=await Search(["A","B","C"],new(){{"A",[C(0) with{SolidCells=[new(0,2,3),new(1,2,3)]}]},{"B",[C(0)]},{"C",[C(1)]}},
            (s,_)=>Task.FromResult(Reply(s)));
        Assert.Equal(2,r["planned"]!.GetValue<int>());Assert.Equal("partial",r["state"]!.GetValue<string>());
        Assert.Equal(0,r["unplaced"]![0]!["inputIndex"]!.GetValue<int>());
        Assert.Equal(1,r["selected"]![0]!["inputIndex"]!.GetValue<int>());
    }
    [Fact]
    public async Task HardNativeBudgetReturnsOnlyBestConfirmedPrefix()
    {
        int calls=0;
        var r=await Search(["A","B"],new(){{"A",[C(0),C(1)]},{"B",[C(2)]}},
            (s,_)=>{calls++;return Task.FromResult(Reply(s));},budget:1);
        Assert.Equal(1,calls);Assert.Equal(1,r["planned"]!.GetValue<int>());
        Assert.Equal("native_budget",r["search"]!["stopReason"]!.GetValue<string>());
        Assert.Equal(2,r["steps"]!.AsArray().Count);
    }
    [Fact]
    public async Task ReservationUsesWholeGeometryAndHeightAndRequiresNativeAcknowledgment()
    {
        ReservedArea[] areas=[new(){X=1,Y=2,Z=3,Width=1,Height=1,Depth=1}];
        var choices=new Dictionary<string,SurveyCandidate[]>{{"A",[C(0) with{OccupiedCells=[new(0,2,3),new(1,2,3)]},C(1,4)]}};
        var r=await Search(["A"],choices,(s,_)=>Task.FromResult(Reply(s,reserved:areas)),reserved:areas);
        Assert.Equal(4,r["steps"]![0]!["z"]!.GetValue<int>());
        Assert.Equal(1,r["filteredCandidates"]!.GetValue<int>());
        await Assert.ThrowsAsync<InvalidDataException>(()=>Search(["A"],choices,(s,_)=>Task.FromResult(Reply(s)),reserved:areas));
        // Missing geometric metadata never grants a reservation exception: native preview must decide.
        var missing=new Dictionary<string,SurveyCandidate[]>{{"A",[C(0) with{OccupiedCells=null}]}};
        var rejected=await Search(["A"],missing,(s,_)=>Task.FromResult(Reply(s,ok:false,reserved:areas)),reserved:areas);
        Assert.Equal("none",rejected["state"]!.GetValue<string>());Assert.Null(rejected["planKey"]);
    }
    [Fact]
    public async Task CompactAndFullKeepSameWinnerAndSearchStatus()
    {
        var r=await Search(["A","B"],new(){{"A",[C(0),C(1)]},{"B",[C(0)]}},(s,_)=>Task.FromResult(Reply(s)));
        var before=r.ToJsonString();var compact=(JsonObject)r.DeepClone();RegionGroupPlanning.Compact(compact);
        foreach(string key in new[]{"steps","planKey","selected","unplaced","search","score","reservedAreas"})
            Assert.True(JsonNode.DeepEquals(r[key],compact[key]));
        Assert.Equal(before,r.ToJsonString());Assert.Empty(compact["attempts"]!.AsArray());
        Assert.True(compact.ToJsonString().Length<before.Length);
    }
    [Fact]
    public async Task FailureAndCancellationAbortWithoutRetry()
    {
        int calls=0;var choices=new Dictionary<string,SurveyCandidate[]>{{"A",[C(0),C(1)]}};
        await Assert.ThrowsAsync<HttpRequestException>(()=>Search(["A"],choices,(_,_)=>{calls++;throw new HttpRequestException();}));
        Assert.Equal(1,calls);
        await Assert.ThrowsAsync<BridgeRejectionException>(()=>Search(["A"],choices,(s,_)=>Task.FromResult(Reply(s) with{SessionId="other"})));
        using var stop=new CancellationTokenSource();stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>Search(["A"],choices,(_,_)=>throw new Exception(),token:stop.Token));
    }
    [Fact]
    public async Task EightTargetsStateAndNativeBudgetsAreBounded()
    {
        var r=await Search(Enumerable.Repeat("A",8).ToArray(),new(){{"A",Enumerable.Range(0,32).Select(i=>C(i)).ToArray()}},
            (s,_)=>Task.FromResult(Reply(s)));
        Assert.Equal(8,r["planned"]!.GetValue<int>());
        Assert.InRange(r["nativeValidationCalls"]!.GetValue<int>(),1,32);
        Assert.InRange(r["search"]!["visitedStates"]!.GetValue<int>(),1,RegionGroupSearch.MaxStates);
    }
    [Fact]
    public void ReservationTransportRejectsInvalidBoundsAndWrongOperation()
    {
        var area=new ReservedArea{X=1,Y=2,Z=3,Width=2,Height=3,Depth=4};
        Assert.True(area.Contains(2,4,6));Assert.False(area.Contains(3,4,6));
        string encoded=ReservedArea.Encode([area]);Assert.Equal(encoded,ReservedArea.Encode(ReservedArea.Decode(encoded)));
        Assert.Throws<ArgumentException>(()=>ReservedArea.Validate([new(){X=int.MaxValue,Width=1,Height=1,Depth=1}]));
        var q=new NameValueCollection{{"session",Request.Session},{"districtId",Request.DistrictId},
            {"steps",LargeProjectRequest.Encode([new(){Template="A",X=0,Y=0,Z=3}])},{"reservedAreas",encoded}};
        Assert.Single(LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-access",q).ReservedAreas);
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-plan",q));
    }
    [Theory]
    [InlineData("{\"searchBudget\":0}")]
    [InlineData("{\"searchBudget\":33}")]
    [InlineData("{\"reservedAreas\":[{\"width\":1,\"height\":1,\"depth\":1}]}")]
    [InlineData("{\"reservedAreas\":[{\"x\":0,\"y\":0,\"z\":0,\"width\":1,\"height\":1,\"depth\":9}]}")]
    public void InvalidSearchInputsFailBeforeNativeCalls(string extra)
    {
        var node=JsonSerializer.SerializeToNode(Request,NativeJson.Options)!.AsObject();node.Remove("template");node["templates"]=new JsonArray("A");
        foreach(var p in JsonNode.Parse(extra)!.AsObject())node[p.Key]=p.Value!.DeepClone();
        Assert.Throws<ArgumentException>(()=>RegionGroupPlanning.Parse(JsonSerializer.SerializeToElement(node)));
    }
}
