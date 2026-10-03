using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;
using System.Net;
using System.Text.Json;
namespace Timberborn.Tests;

public sealed class BuildingProjectTests
{
    private static NameValueCollection Query()=>new() { ["template"]="Lodge.Folktails",["session"]="11111111-1111-1111-1111-111111111111",
        ["districtId"]="22222222-2222-2222-2222-222222222222",["x"]="0",["y"]="0",["z"]="1",["width"]="3",["height"]="2",["rotation"]="0" };
    [Theory]
    [InlineData("width","0")] [InlineData("width","9")] [InlineData("height","-1")]
    [InlineData("rotation","4")] [InlineData("session","bad")] [InlineData("districtId","")]
    [InlineData("template","../bad")] [InlineData("x","4095")]
    public void RejectsUnboundedOrInvalidRequest(string key,string value)
    { var q=Query();q[key]=value;Assert.Throws<ArgumentException>(()=>BuildingPlanRequest.Parse(q)); }
    [Fact]
    public void DuplicateAndUnknownFieldsRejected()
    {
        var q=Query();q.Add("width","3");Assert.Throws<ArgumentException>(()=>BuildingPlanRequest.Parse(q));
        q=Query();q.Add("execute","true");Assert.Throws<ArgumentException>(()=>BuildingPlanRequest.Parse(q));
    }
    [Fact]
    public void BridgeBindsPlanToSession()
    { var r=BridgeRequest.Parse("/agent-api/v1/building-plan",Query());Assert.Equal(r.Session,r.BuildingPlan!.Session); }
    [Fact]
    public void RouteRunsFromConnectedGoalToEntranceAroundObstacle()
    {
        var path=FlatRoadSearch.Find(3,2,0,[true,false,true,true,true,true],[false,false,true,false,false,false]);
        Assert.Equal(new[]{2,5,4,3,0},path);
    }
    [Fact]
    public void NoDiagonalOrRowWrappingShortcut()
    {
        Assert.Null(FlatRoadSearch.Find(2,2,0,[true,false,false,true],[false,false,false,true]));
        Assert.Null(FlatRoadSearch.Find(3,2,2,[false,false,true,true,false,false],[false,false,false,true,false,false]));
    }
    [Fact]
    public void ExistingConnectionNeedsNoNewRouteStep() => Assert.Equal(new[]{0},FlatRoadSearch.Find(1,1,0,[true],[true]));
    [Fact]
    public void CandidateNeverAvailableWhenEntranceIsBlocked()=>Assert.Null(FlatRoadSearch.Find(1,1,0,[false],[true]));
    [Fact]
    public void ToolIsReadOnlyAndAvailableWithoutWrites()
    {
        var tool=BuildingTools.Catalog(false).Single(t=>t.Name=="plan_building_project");
        Assert.True(tool.Annotations!.ReadOnlyHint);Assert.False(BuildingTools.Writes(tool.Name));
    }
    private static BridgeEnvelope<NativeBuildingPlan> Envelope()
    {
        var option=new BuildingPlanOption(new(0,0,1),0,new(1,0,1),new(2,0,1),[new(1,0,1)],[new(2,0,1),new(1,0,1)],false,
            ["joint_game_validation_pending","construction_reachability_unproven","road_protection_incomplete"]);
        return new(1,Query()["session"]!,DateTimeOffset.UtcNow,"0.24.0",new("Lodge.Folktails",Query()["districtId"]!,new(0,0,1),3,2,0,6,5,6,true,"area_exhausted",[option],[]));
    }
    [Fact]
    public void CandidateContractAcceptsOnlyNonExecutableBoundedRoute()=>NativeClient.ValidateBuildingPlan(Envelope(),BuildingPlanRequest.Parse(Query()));
    [Fact]
    public void LodgeCandidateContractRemainsCompatibleOnNewBridge()
    {
        var e = Envelope();
        var option = e.Data.Options[0] with { PlanKey = new string('a', 64) };
        NativeClient.ValidateBuildingPlan(e with { BridgeVersion = "0.34.0", Data = e.Data with { Options = [option] } },
            BuildingPlanRequest.Parse(Query()));
    }
    [Fact]
    public async Task PlanToolUsesReadOnlyRouteAndPreservesNonExecutableEvidence()
    {
        using var client=new NativeClient(new(8081,new string('a',64)),new PlanHandler());
        var q=Query();var args=q.AllKeys.ToDictionary(k=>k!,k=>(object)(k is "x" or "y" or "z" or "width" or "height" or "rotation"?int.Parse(q[k]!):q[k]!));
        var result=await BuildingTools.Invoke(client,"plan_building_project",JsonSerializer.SerializeToElement(args),false,TestContext.Current.CancellationToken);
        Assert.Equal("ok",result["status"]!.GetValue<string>());
        Assert.False(result["data"]!["options"]![0]!["executable"]!.GetValue<bool>());
    }
    private sealed class PlanHandler:HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Assert.Equal(HttpMethod.Get,request.Method);
            Assert.Equal("/agent-api/v1/building-plan",request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(Envelope(),NativeJson.Options))});
        }
    }
    [Theory]
    [InlineData("executable")] [InlineData("jump")] [InlineData("outside")] [InlineData("counts")] [InlineData("session")]
    public void InvalidPlanEvidenceRejected(string defect)
    {
        var e=Envelope();var o=e.Data.Options[0];
        e=defect switch {
            "executable"=>e with {Data=e.Data with{Options=[o with{Executable=true}]}},
            "jump"=>e with{Data=e.Data with{Options=[o with{RouteCells=[new(2,0,1),new(0,0,1),new(1,0,1)]}]}},
            "outside"=>e with{Data=e.Data with{Options=[o with{NewRoadCells=[new(9,9,1)]}]}},
            "counts"=>e with{Data=e.Data with{CheckedCandidates=0}},
            _=>e with{SessionId=Guid.NewGuid().ToString("D")}
        };
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateBuildingPlan(e,BuildingPlanRequest.Parse(Query())));
    }
}
