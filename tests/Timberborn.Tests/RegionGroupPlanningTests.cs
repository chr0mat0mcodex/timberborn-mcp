using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class RegionGroupPlanningTests
{
    private static SurveyRequest Request => new("11111111-1111-4111-8111-111111111111",
        "22222222-2222-4222-8222-222222222222","Synthetic",0,0,3,8,8);
    private static SurveyCandidate Candidate(int x)=>new(new(0,0,3,8,8),0,0,new string('a',64),new(x,2,3),0);
    private static Dictionary<string,SurveyCandidate[]> Choices(int count)=>new(){{"Synthetic",Enumerable.Range(0,count).Select(Candidate).ToArray()}};
    private static BridgeEnvelope<JsonElement> Reply(LargeProjectStep[] targets,bool ok=true)
    {
        // Synthetic native planner: all buildings share one helper road.
        var steps=targets.Select(s=>new LargeProjectStep{Template=s.Template,X=s.X,Y=s.Y,Z=s.Z,Rotation=s.Rotation,
            DependsOn=[targets.Length]}).Append(new LargeProjectStep{Template="Path",X=0,Y=1,Z=3}).ToArray();
        return new(1,Request.Session,DateTimeOffset.UnixEpoch,"0.49.0",JsonSerializer.SerializeToElement(new {
            candidateFound=ok,reason=ok?"candidate":"collision",phase="native_joint_preview",blockedPart=targets.Length-1,
            steps,cleanupCandidateIndices=new[]{targets.Length},plan=new{pilotEligible=ok,restored=true,
                planKey=new string('b',64),parts=Array.Empty<object>()}},NativeJson.Options));
    }
    [Fact]
    public async Task EightRepeatedBuildingsUseCumulativePreviewAndOneSharedRoad()
    {
        var sizes=new List<int>();
        var r=await RegionGroupPlanning.Plan(Request,Enumerable.Repeat("Synthetic",8).ToArray(),3,Choices(8),(s,_)=>{
            sizes.Add(s.Length);return Task.FromResult(Reply(s));
        },TestContext.Current.CancellationToken);
        Assert.Equal(Enumerable.Range(1,8),sizes);Assert.Equal("complete",r["state"]!.GetValue<string>());
        Assert.Equal(9,r["steps"]!.AsArray().Count);Assert.Equal(8,r["nativeValidationCalls"]!.GetValue<int>());
        Assert.Equal(7,r["selected"]![7]!["stepIndex"]!.GetValue<int>());
        Assert.Equal(8,r["steps"]![7]!["dependsOn"]![0]!.GetValue<int>());
        Assert.Equal(8,r["cleanupCandidateIndices"]![0]!.GetValue<int>());
    }
    [Fact]
    public async Task RejectedAdditionKeepsLastValidJointPlanAndReportsPartial()
    {
        var r=await RegionGroupPlanning.Plan(Request,["Synthetic","Synthetic"],1,Choices(3),
            (s,_)=>Task.FromResult(Reply(s,s.Length==1)),TestContext.Current.CancellationToken);
        Assert.Equal("partial",r["state"]!.GetValue<string>());
        Assert.Equal(1,r["planned"]!.GetValue<int>());Assert.Equal(2,r["steps"]!.AsArray().Count);
        Assert.Equal("collision",r["unplaced"]![0]!["reason"]!.GetValue<string>());
        Assert.Equal(1,r["unplaced"]![0]!["unchecked"]!.GetValue<int>());
    }
    [Fact]
    public async Task GlobalBudgetIs32AndEmptySearchMakesNoPreview()
    {
        int calls=0;
        var r=await RegionGroupPlanning.Plan(Request,Enumerable.Repeat("Synthetic",8).ToArray(),4,Choices(8),
            (s,_)=>{calls++;return Task.FromResult(Reply(s,false));},TestContext.Current.CancellationToken);
        Assert.Equal(32,calls);Assert.Equal("none",r["state"]!.GetValue<string>());Assert.Null(r["planKey"]);
        await RegionGroupPlanning.Plan(Request,["Synthetic"],4,Choices(0),(_,_)=>throw new InvalidOperationException(),TestContext.Current.CancellationToken);
    }
    [Fact]
    public async Task DifferentTemplatesSharePlanAndExtentRejectionDoesNotDispatch()
    {
        var choices=Choices(1);choices["Other"]=[Candidate(1) with{Origin=new(1,2,20)}];
        int calls=0;
        var r=await RegionGroupPlanning.Plan(Request,["Synthetic","Other"],1,choices,
            (s,_)=>{calls++;return Task.FromResult(Reply(s));},TestContext.Current.CancellationToken);
        Assert.Equal(1,calls);Assert.Equal("partial",r["state"]!.GetValue<string>());
        Assert.Equal("project_extent_limit",r["unplaced"]![0]!["reason"]!.GetValue<string>());
        choices["Other"]=[Candidate(1)];
        var complete=await RegionGroupPlanning.Plan(Request,["Synthetic","Other"],1,choices,
            (s,_)=>Task.FromResult(Reply(s)),TestContext.Current.CancellationToken);
        Assert.Equal("Other",complete["steps"]![1]!["template"]!.GetValue<string>());
        Assert.Equal("complete",complete["state"]!.GetValue<string>());
    }
    [Fact]
    public async Task TransportSessionAndIncompleteEvidenceAbortWithoutRetry()
    {
        int calls=0;
        await Assert.ThrowsAsync<HttpRequestException>(()=>RegionGroupPlanning.Plan(Request,["Synthetic"],4,Choices(8),
            (_,_)=>{calls++;throw new HttpRequestException();},TestContext.Current.CancellationToken));
        Assert.Equal(1,calls);
        await Assert.ThrowsAsync<BridgeRejectionException>(()=>RegionGroupPlanning.Plan(Request,["Synthetic"],4,Choices(8),
            (s,_)=>Task.FromResult(Reply(s) with{SessionId="33333333-3333-4333-8333-333333333333"}),TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidDataException>(()=>RegionGroupPlanning.Plan(Request,["Synthetic"],4,Choices(8),(s,_)=>{
            var e=Reply(s);var n=JsonNode.Parse(e.Data.GetRawText())!;n["plan"]!["restored"]=false;
            return Task.FromResult(e with{Data=JsonSerializer.SerializeToElement(n)});
        },TestContext.Current.CancellationToken));
        using var stop=new CancellationTokenSource();stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>RegionGroupPlanning.Plan(Request,["Synthetic"],4,Choices(8),
            (_,_)=>throw new InvalidOperationException(),stop.Token));
    }
    [Fact]
    public async Task CompactPreservesExecutablePlanWithoutMutatingFullEvidence()
    {
        var data=await RegionGroupPlanning.Plan(Request,["Synthetic","Synthetic"],3,Choices(2),
            (s,_)=>Task.FromResult(Reply(s)),TestContext.Current.CancellationToken);
        var envelope=new JsonObject{["status"]="ok",["data"]=data};
        var before=envelope.ToJsonString();
        var compact=ResponsePresentation.Present(RegionBuildPlanning.Name,envelope,false);
        Assert.Equal(before,envelope.ToJsonString());
        Assert.Equal(before,ResponsePresentation.Present(RegionBuildPlanning.Name,envelope,true).ToJsonString());
        Assert.True(JsonNode.DeepEquals(data["steps"],compact["data"]!["steps"]));
        Assert.True(JsonNode.DeepEquals(data["planKey"],compact["data"]!["planKey"]));
        Assert.Null(compact["data"]!["attempts"]![0]!["evidence"]);
        Assert.NotNull(data["attempts"]![0]!["evidence"]);
    }
    [Theory]
    [InlineData(0)][InlineData(9)]
    public void InvalidGroupSizeRejected(int size)
    {
        var node=JsonSerializer.SerializeToNode(Request,NativeJson.Options)!.AsObject();node.Remove("template");
        node["templates"]=JsonSerializer.SerializeToNode(Enumerable.Repeat("Synthetic",size));
        Assert.Throws<ArgumentException>(()=>RegionGroupPlanning.Parse(JsonSerializer.SerializeToElement(node)));
    }
    [Fact]
    public void SchemaAndParserRequireExclusiveModeAndExposeFull()
    {
        var node=JsonSerializer.SerializeToNode(Request,NativeJson.Options)!.AsObject();
        node["templates"]=new JsonArray("Synthetic","Synthetic");
        Assert.Throws<ArgumentException>(()=>RegionGroupPlanning.Parse(JsonSerializer.SerializeToElement(node)));
        node.Remove("template");
        var parsed=RegionGroupPlanning.Parse(JsonSerializer.SerializeToElement(node));
        Assert.Equal(32,parsed.Survey.MaxResults);Assert.Equal(2,parsed.Templates.Length);
        var tool=ResponsePresentation.Describe(Assert.Single(RegionBuildPlanning.Catalog(true)));
        Assert.False(tool.Annotations!.ReadOnlyHint);
        Assert.Equal(8,tool.InputSchema.GetProperty("properties").GetProperty("templates").GetProperty("maxItems").GetInt32());
        Assert.True(tool.InputSchema.GetProperty("properties").TryGetProperty("detail",out _));
    }
}
