using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class LargeProjectStartTests
{
    [Fact]
    public void RepairPreviewAndRunPreserveTheBlueprintAndParent()
    {
        var query = new NameValueCollection {
            {"session","11111111-1111-1111-1111-111111111111"},
            {"districtId","22222222-2222-2222-2222-222222222222"},
            {"parentActionId","33333333-3333-3333-3333-333333333333"},
            {"steps","Path,1,2,3,0,"}
        };
        var preview = LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-repair",query);
        query.Add("actionId","44444444-4444-4444-4444-444444444444");
        query.Add("planKey",new string('a',64)); query.Add("mode","development_pilot");
        query.Add("speed","7"); query.Add("maxGameHours","24"); query.Add("maxRealSeconds","60");
        var run = LargeProjectRequest.Parse("/agent-api/v1/large-project-run-repair",query);
        Assert.Equal(preview.Session,run.Session);
        Assert.Equal(preview.DistrictId,run.DistrictId);
        Assert.Equal(preview.ParentActionId,run.ParentActionId);
        Assert.Equal(LargeProjectRequest.Encode(preview.Steps),LargeProjectRequest.Encode(run.Steps));
        LargeProjectStartPolicy.RequirePlan(run.PlanKey,run.PlanKey,true);
        Assert.Equal("project_start_plan_changed",Assert.Throws<BridgeRejectionException>(() =>
            LargeProjectStartPolicy.RequirePlan(new string('b',64),run.PlanKey,true)).Code);
        Assert.Equal("project_start_ineligible",Assert.Throws<BridgeRejectionException>(() =>
            LargeProjectStartPolicy.RequirePlan(run.PlanKey,run.PlanKey,false)).Code);
    }

    [Theory]
    [InlineData("project_start_plan_changed",true)]
    [InlineData("project_start_ineligible",true)]
    [InlineData("project_start_busy",true)]
    [InlineData("project_start_capacity",true)]
    [InlineData("project_start_identity_conflict",true)]
    [InlineData("project_start_unresolved",true)]
    [InlineData("project_start_simulation_unavailable",true)]
    [InlineData("project_start_baseline_unavailable",true)]
    [InlineData("project_start_entity_conflict",true)]
    [InlineData("project_start_preflight_conflict",true)]
    [InlineData("repair_parent_ineligible",true)]
    [InlineData("repair_district_mismatch",true)]
    [InlineData("repair_helpers_only",true)]
    [InlineData("repair_nested",true)]
    [InlineData("repair_other_project_unresolved",true)]
    [InlineData("state_conflict",false)]
    [InlineData("unknown_conflict",false)]
    [InlineData("malformed",false)]
    [InlineData("lost",false)]
    public async Task OnlyExplicitPreDispatchRejectionsProveNoNewOrder(string code,bool rejected)
    {
        using var handler = new RejectionHandler(code);
        using var tools = new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),
            enableBuildingPlacement:true,enableSpeedControl:true);
        var args = JsonSerializer.SerializeToElement(new {
            session="11111111-1111-1111-1111-111111111111",
            districtId="22222222-2222-2222-2222-222222222222",
            parentActionId="33333333-3333-3333-3333-333333333333",
            actionId="44444444-4444-4444-4444-444444444444",
            steps=new[]{new {template="Path",x=1,y=2,z=3,rotation=0,dependsOn=Array.Empty<int>()}},
            planKey=new string('a',64),mode="development_pilot",speed=7,maxGameHours=24,maxRealSeconds=60
        });
        var result = await tools.Invoke("run_large_project_repair",args,TestContext.Current.CancellationToken);
        Assert.Equal(rejected?"not_executed":"unconfirmed",result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1,handler.Calls);
        var text = ActivityOutcome.Describe("run_large_project_repair",result);
        if(rejected) {
            Assert.Equal(code,result["error"]!["code"]!.GetValue<string>());
            Assert.Equal(ProjectStartOutcome.Describe(code),text);
            Assert.InRange(text.Length,1,400);
        } else Assert.Contains("unbestätigt",text);
    }

    private sealed class RejectionHandler(string code) : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;
            Assert.Equal(HttpMethod.Post,request.Method);
            Assert.Equal("/agent-api/v1/large-project-run-repair",request.RequestUri!.AbsolutePath);
            if(code=="lost") throw new HttpRequestException();
            string body=code=="malformed"?"{\"error\":\"project_start_plan_changed\",\"extra\":true}":
                JsonSerializer.Serialize(new {error=code});
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict){Content=new StringContent(body)});
        }
    }
}
