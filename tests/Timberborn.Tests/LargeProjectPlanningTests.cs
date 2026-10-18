using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class LargeProjectPlanningTests
{
    public static TheoryData<string> Codes => new() {
        "project_plan_requires_pause", "project_plan_faulted", "project_plan_busy",
        "project_plan_foreign_construction", "project_plan_district_unavailable",
        "project_plan_foreign_site_nearby", "project_plan_foreign_access_unknown", "project_plan_unresolved_identity",
        "project_plan_existing_access_lost",
        "project_plan_construction_inventory_incomplete", "project_plan_construction_initialization_pending",
        "project_plan_construction_state_unknown",
        "project_plan_navigation_access_limit", "project_plan_navigation_comparison_limit", "project_plan_navigation_no_probes",
        "project_plan_navigation_district_coverage_unknown", "project_plan_navigation_preview_baseline_mismatch",
        "project_plan_navigation_no_connected_baseline", "project_plan_navigation_coverage_unknown"
    };

    [Fact]
    public void DenseMapRetainsLateRoadsAccessesDistrictsAndEveryConstructionSite()
    {
        IEnumerable<(bool Site, bool District, bool Road, bool Access)> objects =
            Enumerable.Repeat((false,false,false,false), 10000)
            .Concat(new[] {(true,false,false,false), (false,true,false,false),
                (false,false,true,false), (false,false,false,true)});
        var candidates = RoadProtectionPolicy.CaptureCandidates(objects, o =>
            RoadProtectionPolicy.NeedsObservation(o.Site,o.District,o.Road,o.Access));
        Assert.Equal(4,candidates.Length);
        Assert.True(candidates[0].Site);
        Assert.True(candidates[1].District);
        Assert.True(candidates[2].Road);
        Assert.True(candidates[3].Access);
        // Filtering irrelevant objects must not hide a lost connection of a late target.
        Assert.Equal(new[]{2},RoadProtectionPolicy.LostConnections([true,true,true,true],[true,true,false,true]));
    }

    [Theory]
    [InlineData(true,true,false,true,"project_plan_faulted")]
    [InlineData(false,false,false,true,"project_plan_requires_pause")]
    [InlineData(false,true,true,true,"project_plan_busy")]
    [InlineData(false,true,false,false,"project_plan_foreign_construction")]
    public void PreconditionsRemainDistinct(bool faulted,bool paused,bool busy,bool isolated,string expected) =>
        Assert.Equal(expected,Assert.Throws<BridgeRejectionException>(() =>
            LargeProjectPlanningPolicy.RequireAvailable(faulted,paused,busy,isolated)).Code);

    [Theory]
    [InlineData("navigation_access_limit","project_plan_navigation_access_limit")]
    [InlineData("navigation_comparison_limit","project_plan_navigation_comparison_limit")]
    [InlineData("navigation_no_probes","project_plan_navigation_no_probes")]
    [InlineData("navigation_preview_baseline_mismatch","project_plan_navigation_preview_baseline_mismatch")]
    [InlineData("private diagnostic text","project_plan_navigation_coverage_unknown")]
    public void BaselineReasonsAreWhitelisted(string reason,string expected)
    {
        Assert.Equal(expected,Assert.Throws<BridgeRejectionException>(() =>
            LargeProjectPlanningPolicy.RequireBaseline(false,reason)).Code);
        LargeProjectPlanningPolicy.RequireBaseline(true,reason);
    }

    [Theory]
    [MemberData(nameof(Codes))]
    public async Task ConcreteRejectionSurvivesTransportAndActivityPresentation(string code)
    {
        foreach (var tool in new[]{"plan_large_project","plan_large_project_access","plan_large_project_repair",
            "start_large_project","run_large_project","run_large_project_repair"})
            await Check(tool,code,true);
    }

    [Theory]
    [InlineData("state_conflict")]
    [InlineData("unknown_conflict")]
    [InlineData("malformed")]
    [InlineData("lost")]
    public async Task AmbiguousResponsesRemainUnconfirmed(string code) => await Check("plan_large_project",code,false);

    [Theory]
    [InlineData("plan_large_project")]
    [InlineData("plan_large_project_access")]
    [InlineData("run_large_project")]
    public async Task Preview_runtime_error_keeps_its_code_without_claiming_restoration(string tool)
    {
        await Check(tool,"project_preview_failed",false);
    }

    [Fact]
    public async Task Missing_project_read_preserves_missing_code_and_never_posts()
    {
        using var handler=new MissingProjectHandler();
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler));
        var result=await tools.Invoke("inspect_large_project",JsonSerializer.SerializeToElement(new {
            session="11111111-1111-1111-1111-111111111111",
            actionId="22222222-2222-2222-2222-222222222222",waitSeconds=0
        }),TestContext.Current.CancellationToken);
        Assert.Equal("project_not_found",result["error"]!["code"]!.GetValue<string>());
        Assert.Equal("not_applicable",result["error"]!["execution"]!.GetValue<string>());
        Assert.Equal(1,handler.Calls);
    }

    private sealed class MissingProjectHandler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Calls++;Assert.Equal(HttpMethod.Get,request.Method);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict) {
                Content=new StringContent("{\"error\":\"project_not_found\"}") });
        }
    }

    private static async Task Check(string tool,string code,bool rejected)
    {
        using var handler = new RejectionHandler(code,!tool.StartsWith("plan_",StringComparison.Ordinal));
        using var tools = new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),
            enableBuildingPlacement:true,enableSpeedControl:true);
        var args = new Dictionary<string,object> {
            ["session"]="11111111-1111-1111-1111-111111111111",
            ["districtId"]="22222222-2222-2222-2222-222222222222",
            ["steps"]=new[]{new {template="Path",x=1,y=2,z=3,rotation=0,dependsOn=Array.Empty<int>()}}
        };
        if(tool.EndsWith("repair",StringComparison.Ordinal)) args["parentActionId"]="33333333-3333-3333-3333-333333333333";
        if(tool.StartsWith("start_",StringComparison.Ordinal)||tool.StartsWith("run_",StringComparison.Ordinal)) {
            args["actionId"]="44444444-4444-4444-4444-444444444444";
            args["planKey"]=new string('a',64); args["mode"]="development_pilot";
        }
        if(tool.StartsWith("run_",StringComparison.Ordinal)) {
            args["speed"]=7; args["maxGameHours"]=24; args["maxRealSeconds"]=60;
        }
        var result=await tools.Invoke(tool,JsonSerializer.SerializeToElement(args),TestContext.Current.CancellationToken);
        Assert.Equal(1,handler.Calls);
        bool starts = !tool.StartsWith("plan_",StringComparison.Ordinal);
        Assert.Equal(starts?1:0,handler.PreflightReads);
        if(starts) Assert.Equal("unavailable",result["operatingPreflight"]![0]!["state"]!.GetValue<string>());
        else Assert.Null(result["operatingPreflight"]);
        Assert.Equal(rejected?"not_executed":"unconfirmed",result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        if(code=="project_preview_failed") {
            Assert.Equal(code,result["error"]!["code"]!.GetValue<string>());
            Assert.Contains("Wiederherstellung unbekannt",result["error"]!["message"]!.GetValue<string>());
            Assert.Contains("Wiederherstellung unbekannt",ActivityOutcome.Describe(tool,result));
        }
        if(rejected) {
            Assert.Equal(code,result["error"]!["code"]!.GetValue<string>());
            Assert.Contains(ProjectPlanningOutcome.Describe(code),result["error"]!["message"]!.GetValue<string>());
            Assert.Equal(ProjectPlanningOutcome.Describe(code),ActivityOutcome.Describe(tool,result));
        }
    }

    [Fact]
    public void NewVersionRetainsConservativeCapabilityProfile()
    {
        var report=BuildingCapabilityTools.Describe("0.61.0","Folktails",true);
        Assert.Equal("known",report.ProfileState);
        Assert.False(report.RegularExecutionAllowed);
        Assert.False(report.ConstructionPreflightProven);
        Assert.NotEmpty(report.Modes);
        Assert.NotEqual("known",BuildingCapabilityTools.Describe("99.0.0","Folktails",true).ProfileState);
    }

    private sealed class RejectionHandler(string code,bool expectPreflight) : HttpMessageHandler
    {
        public int Calls;
        public int PreflightReads;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            if (request.Method == HttpMethod.Get) {
                Assert.Equal("/agent-api/v1/production-graph",request.RequestUri!.AbsolutePath);
                Assert.True(expectPreflight);
                Assert.Equal(0,Calls);
                Assert.Equal(0,PreflightReads);
                PreflightReads++;
                // Synthetic unavailable diagnosis: the original start rejection must still survive.
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            }
            Calls++;
            Assert.Equal(HttpMethod.Post,request.Method);
            if(code=="lost")throw new HttpRequestException();
            string body=code=="malformed"?"{\"error\":\"project_plan_requires_pause\",\"extra\":true}":JsonSerializer.Serialize(new{error=code});
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict){Content=new StringContent(body)});
        }
    }
}
