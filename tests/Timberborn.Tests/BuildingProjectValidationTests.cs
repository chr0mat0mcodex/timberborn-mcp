using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class BuildingProjectValidationTests
{
    private static NameValueCollection Query()=>new() { ["template"]="Lodge.Folktails",["session"]="11111111-1111-1111-1111-111111111111",
        ["districtId"]="22222222-2222-2222-2222-222222222222",["x"]="0",["y"]="0",["z"]="1",["width"]="3",["height"]="2",["rotation"]="0",
        ["optionIndex"]="0",["planKey"]=new string('a',64) };
    [Theory]
    [InlineData("optionIndex","4")] [InlineData("optionIndex","00")] [InlineData("planKey","bad")]
    [InlineData("width","9")] [InlineData("session","bad")]
    public void InvalidSelectionRejected(string key,string value)
    {var q=Query();q[key]=value;Assert.Throws<ArgumentException>(()=>BuildingProjectValidationRequest.Parse(q));}
    [Fact]
    public void RequestUsesFreshSessionAndPreservesCallerQuery()
    {var q=Query();var r=BridgeRequest.Parse("/agent-api/v1/building-project-validation",q);Assert.Equal(q["session"],r.Session);Assert.Equal(11,q.Count);}
    [Fact]
    public void PreviewRequiresIndependentOptInAndPost()
    {
        const string route="/agent-api/v1/building-project-validation";
        Assert.False(BridgeHttpServer.MethodAllowed("GET",route,false,false,enableBuildingPlacement:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST",route,false,false));
        Assert.True(BridgeHttpServer.MethodAllowed("POST",route,false,false,enableBuildingPlacement:true));
        Assert.DoesNotContain(BuildingTools.Catalog(false),t=>t.Name=="validate_building_project");
        Assert.Contains(BuildingTools.Catalog(true),t=>t.Name=="validate_building_project");
    }
    private static BridgeEnvelope<NativeProjectValidation> Evidence()
    {
        var q=Query();var option=new BuildingPlanOption(new(0,1,1),0,new(0,0,1),new(1,0,1),[new(0,0,1)],[new(1,0,1),new(0,0,1)],false,
            ["joint_game_validation_pending","construction_reachability_unproven","road_protection_incomplete"],q["planKey"]);
        var road=new NativeRoadProtection("unknown",["construction_and_road_node_coverage_unproven"],4,0,true,false,[],false,[],[],2,1,0);
        return new(1,q["session"]!,DateTimeOffset.UtcNow,"0.24.1",new(q["template"]!,q["planKey"]!,0,option,true,[true],[0],true,road,true,false,15,false,[]));
    }
    [Fact]
    public void JointEvidenceStillNotExecutable()=>NativeClient.ValidateProjectEvidence(Evidence(),BuildingProjectValidationRequest.Parse(Query()));
    [Fact]
    public async Task JointClientAddsAssessmentWithoutChangingExecutableOrNativeEvidence()
    {
        var e=Evidence();
        using var client=new NativeClient(new(8081,new string('a',64)),new PreviewHandler(
            System.Text.Json.JsonSerializer.Serialize(e,NativeJson.Options)));
        var result=await client.ValidateProject(BuildingProjectValidationRequest.Parse(Query()),TestContext.Current.CancellationToken);
        Assert.False(result.Data.Executable);
        Assert.False(result.Data.Assessment!.RegularExecutionAllowed);
        Assert.Equal("unknown",result.Data.Assessment.Decision);
        Assert.Equal("passed",result.Data.Assessment.Checks.Single(c=>c.Name=="candidateEntrance").Status);
        Assert.Equal("unknown",result.Data.Assessment.Checks.Single(c=>c.Name=="newConstructionAccess").Status);
    }
    private sealed class PreviewHandler(string json):HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(json)});
    }
    [Theory]
    [InlineData(999999, true)]
    [InlineData(15, true)]
    [InlineData(0, true)]
    [InlineData(1000000, false)]
    [InlineData(-1, false)]
    public async Task ExpandedProjectBudgetSurvivesFullResponseValidation(int remaining, bool accepted)
    {
        var e = Evidence();
        var construction = new NativeConstructionPreview("unknown", "no_existing_construction_sites",
            null, null, 0, [], [], new("unknown", "no_existing_construction_sites", null, null, null, [], []));
        e = e with { BridgeVersion = "0.35.1", Data = e.Data with {
            AttemptsRemaining = remaining,
            RoadProtection = e.Data.RoadProtection with { ConstructionAccessPreview = construction }
        } };
        using var client = new NativeClient(new(8081, new string('a', 64)), new PreviewHandler(
            System.Text.Json.JsonSerializer.Serialize(e, NativeJson.Options)));
        var request = BuildingProjectValidationRequest.Parse(Query());
        if (!accepted) {
            await Assert.ThrowsAsync<InvalidDataException>(() => client.ValidateProject(request, TestContext.Current.CancellationToken));
            return;
        }
        var result = await client.ValidateProject(request, TestContext.Current.CancellationToken);
        Assert.Equal(remaining, result.Data.AttemptsRemaining);
        Assert.False(result.Data.Executable);
        Assert.False(result.Data.Assessment!.RegularExecutionAllowed);
    }
    [Theory]
    [InlineData("executable")] [InlineData("key")] [InlineData("count")] [InlineData("restoration")]
    [InlineData("lock")] [InlineData("budget")] [InlineData("session")]
    public void InconsistentEvidenceRejected(string defect)
    {
        var e=Evidence();e=defect switch {
            "executable"=>e with{Data=e.Data with{Executable=true}},
            "key"=>e with{Data=e.Data with{PlanKey=new string('b',64)}},
            "count"=>e with{Data=e.Data with{RoadValid=[]}},
            "restoration"=>e with{Data=e.Data with{RoadProtection=e.Data.RoadProtection with{Restored=false}}},
            "lock"=>e with{Data=e.Data with{SessionLocked=true}},
            "budget"=>e with{Data=e.Data with{AttemptsRemaining=16}},
            _=>e with{SessionId=Guid.NewGuid().ToString("D")}
        };
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateProjectEvidence(e,BuildingProjectValidationRequest.Parse(Query())));
    }
}
