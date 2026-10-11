using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class RegionBuildPlanningTests
{
    private const string Session="11111111-1111-4111-8111-111111111111";
    private static SurveyRequest Request => new(Session,"22222222-2222-4222-8222-222222222222","Synthetic",0,0,3,8,8);
    private static SurveyCandidate Candidate(int x,int rotation=0) => new(new(0,0,3,8,8),rotation,0,new string('a',64),new(x,2,3),x);
    private static BridgeEnvelope<JsonElement> Reply(LargeProjectStep s,bool ok) => new(1,Session,DateTimeOffset.UnixEpoch,"0.48.0",
        JsonSerializer.SerializeToElement(new {candidateFound=ok,reason=ok?"candidate":"native_joint_preview_rejected",
            phase="native_joint_preview",steps=new[]{s},cleanupCandidateIndices=Array.Empty<int>(),
            plan=new{pilotEligible=ok,restored=true,planKey=new string('b',64),parts=new[]{new{index=0,placementState=ok?"native_valid":"native_invalid"}}}},NativeJson.Options));

    [Fact]
    public async Task BudgetDeduplicatesButKeepsUncheckedAndRejectedSeparate()
    {
        var seen=new List<LargeProjectStep>();
        var r=await RegionBuildPlanning.CheckCandidates(Request,[Candidate(1),Candidate(1),Candidate(2,2),Candidate(3)],2,(s,_)=>{
            seen.Add(s);return Task.FromResult(Reply(s,s.X==2));
        },TestContext.Current.CancellationToken);
        Assert.Equal(2,seen.Count);Assert.Equal(3,r["candidateCount"]!.GetValue<int>());
        Assert.Equal(1,r["passed"]!.GetValue<int>());Assert.Equal(1,r["unchecked"]!.GetValue<int>());
        var c=r["candidates"]!;
        Assert.Equal("rejected",c[0]!["state"]!.GetValue<string>());Assert.Null(c[0]!["planKey"]);
        Assert.Equal("native_preview_passed",c[1]!["state"]!.GetValue<string>());
        Assert.Equal(2,c[1]!["steps"]![0]!["rotation"]!.GetValue<int>());
        Assert.Equal(2,c[1]!["steps"]![0]!["x"]!.GetValue<int>());
        Assert.Equal(new string('b',64),c[1]!["planKey"]!.GetValue<string>());
        Assert.Equal("unchecked",c[2]!["state"]!.GetValue<string>());Assert.Null(c[2]!["steps"]);
    }
    [Fact]
    public async Task FailedTransportNeverBecomesAnotherCandidateAttempt()
    {
        int calls=0;
        await Assert.ThrowsAsync<HttpRequestException>(()=>RegionBuildPlanning.CheckCandidates(Request,[Candidate(1),Candidate(2)],2,(_,_)=>{
            calls++;throw new HttpRequestException();
        },TestContext.Current.CancellationToken));
        Assert.Equal(1,calls);
    }
    [Fact]
    public async Task NativeHelpersAndDependenciesRemainUnchanged()
    {
        var r=await RegionBuildPlanning.CheckCandidates(Request,[Candidate(1)],1,(s,_)=>{
            var node=System.Text.Json.Nodes.JsonNode.Parse(Reply(s,true).Data.GetRawText())!;
            s.DependsOn=[1];
            node["steps"]=JsonSerializer.SerializeToNode(new[]{s,new LargeProjectStep{Template="Path",X=1,Y=1,Z=3}},NativeJson.Options);
            node["cleanupCandidateIndices"]=JsonSerializer.SerializeToNode(new[]{1});
            return Task.FromResult(Reply(s,true) with {Data=JsonSerializer.SerializeToElement(node)});
        },TestContext.Current.CancellationToken);
        var selected=r["candidates"]![0]!;
        Assert.Equal(1,selected["steps"]![0]!["dependsOn"]![0]!.GetValue<int>());
        Assert.Equal("Path",selected["steps"]![1]!["template"]!.GetValue<string>());
        Assert.Equal(1,selected["cleanupCandidateIndices"]![0]!.GetValue<int>());
    }
    [Fact]
    public void BuildingSearchReasonsRejectOutOfRangeSamples()
    {
        var q=new System.Collections.Specialized.NameValueCollection {
            {"session",Request.Session},{"districtId",Request.DistrictId},{"template",Request.Template},
            {"x","0"},{"y","0"},{"z","3"},{"width","1"},{"height","1"},{"rotation","2"} };
        var request=BuildingPlanRequest.Parse(q);
        var data=new NativeBuildingPlan(Request.Template,Request.DistrictId,new(0,0,3),1,1,2,1,1,1,true,"area_exhausted",[],[],
            new(){{"required_support_missing",1}},[new(new(0,0,3),2,["required_support_missing"])]);
        var e=new BridgeEnvelope<NativeBuildingPlan>(1,Session,DateTimeOffset.UnixEpoch,"0.48.0",data);
        NativeClient.ValidateBuildingPlan(e,request);
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateBuildingPlan(e with{Data=data with{
            RejectedSites=[new(new(1,0,3),2,["required_support_missing"])]}},request));
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateBuildingPlan(e with{Data=data with{
            RejectionReasons=new(){{"required_support_missing",2}}}},request));
    }
    [Fact]
    public async Task SessionChangeAbortsAndCancellationDoesNotStartPreview()
    {
        await Assert.ThrowsAsync<BridgeRejectionException>(()=>RegionBuildPlanning.CheckCandidates(Request,[Candidate(1)],1,
            (s,_)=>Task.FromResult(Reply(s,true) with{SessionId="33333333-3333-4333-8333-333333333333"}),TestContext.Current.CancellationToken));
        using var stop=new CancellationTokenSource();stop.Cancel();int calls=0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>RegionBuildPlanning.CheckCandidates(Request,[Candidate(1)],1,
            (s,_)=>{calls++;return Task.FromResult(Reply(s,true));},stop.Token));
        Assert.Equal(0,calls);
    }
    [Fact]
    public async Task DisabledGateRejectsBeforeClientAccessAndCatalogIsNotReadOnly()
    {
        Assert.Empty(RegionBuildPlanning.Catalog(false));
        Assert.False(Assert.Single(RegionBuildPlanning.Catalog(true)).Annotations!.ReadOnlyHint);
        await Assert.ThrowsAsync<ArgumentException>(()=>RegionBuildPlanning.Invoke(null!,JsonSerializer.SerializeToElement(Request,NativeJson.Options),false,TestContext.Current.CancellationToken));
        Assert.True(RegionSurveyTools.Reader().Annotations!.ReadOnlyHint);
    }
    [Fact]
    public void CandidateOccupationRequiresExactBoundedNativeGeometryOn050()
    {
        var q=new System.Collections.Specialized.NameValueCollection{{"session",Request.Session},{"districtId",Request.DistrictId},
            {"template",Request.Template},{"x","0"},{"y","0"},{"z","3"},{"width","1"},{"height","2"},{"rotation","0"}};
        var request=BuildingPlanRequest.Parse(q);
        var option=new BuildingPlanOption(new(0,0,3),0,new(0,1,3),new(0,1,3),[],[new(0,1,3)],false,
            ["joint_game_validation_pending","construction_reachability_unproven","road_protection_incomplete"],new string('a',64),
            [new(0,0,3)],[new(0,0,3)],[new(new(0,0,3),"Full")]);
        var data=new NativeBuildingPlan(Request.Template,Request.DistrictId,new(0,0,3),1,2,0,2,1,2,true,"area_exhausted",[option],[]);
        var e=new BridgeEnvelope<NativeBuildingPlan>(1,Request.Session,DateTimeOffset.UnixEpoch,"0.50.0",data);
        NativeClient.ValidateBuildingPlan(e,request);
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateBuildingPlan(e with{Data=data with{Options=[option with{OccupiedCells=null}]}},request));
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateBuildingPlan(e with{Data=data with{Options=[option with{SolidCells=[new(0,1,3)]}]}},request));
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateBuildingPlan(e with{Data=data with{Options=[option with{CellOccupations=[new(new(4,1,3),"Full")]}]}},request));
    }
    [Theory]
    [InlineData(0)][InlineData(5)]
    public void InvalidCandidateBudgetRejected(int max)
    {
        var node=JsonSerializer.SerializeToNode(Request,NativeJson.Options)!;
        node["maxCandidates"]=max;
        Assert.Throws<ArgumentException>(()=>RegionBuildPlanning.Parse(JsonSerializer.SerializeToElement(node)));
    }
}
