using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public class MultiProjectRepairTests
{
    [Theory]
    [InlineData("project_priority_revision_changed")]
    [InlineData("project_priority_identity_unavailable")]
    public void PriorityPreconditionCodesCrossTheBridgeWithoutLosingTheirIdentity(string code)
    {
        Assert.True(BridgeRejectionException.IsCode(code));
        Assert.Equal(code,new BridgeRejectionException(code).Code);
        Assert.False(BridgeRejectionException.IsCode("project_priority_arbitrary_error"));
    }

    [Fact]
    public void RestoredAccessIsLatchedAndCompletionDoesNotRequireIndustrialCompletion()
    {
        Assert.True(ParallelConstructionPolicy.RetainsReachability(true,false,false));
        Assert.True(ParallelConstructionPolicy.RetainsReachability(true,false,true));
        Assert.False(ParallelConstructionPolicy.RetainsReachability(true,true,false));
        Assert.False(ParallelConstructionPolicy.RetainsReachability(false,false,false));
        Assert.True(ParallelConstructionPolicy.RepairAccessRestored("construction","construction"));
        Assert.False(ParallelConstructionPolicy.RepairAccessRestored("construction","construction_unreachable"));
        Assert.False(ParallelConstructionPolicy.RepairAccessRestored("finished","finished_access_missing"));
        Assert.False(ParallelConstructionPolicy.RepairAccessRestored("unconfirmed","finished"));
    }

    [Fact]
    public void RepairAcceptsKnownDisconnectedProjectsButNeverUnknownIdentityOrAccess()
    {
        Assert.All(new[]{"construction", "construction_unreachable", "finished_access_missing"},
            state=>Assert.True(ParallelConstructionPolicy.KnownRepairPart("construction",state)));
        Assert.All(new[]{"construction_access_unknown", "initializing", "identity_mismatch"},
            state=>Assert.False(ParallelConstructionPolicy.KnownRepairPart("construction",state)));
        Assert.False(ParallelConstructionPolicy.KnownRepairPart("unconfirmed","construction_unreachable"));
        Assert.False(ParallelConstructionPolicy.KnownRepairPart("pending","construction"));
        Assert.False(ParallelConstructionPolicy.KnownPart("construction","construction_unreachable"));
    }

    [Fact]
    public void RepairAllowsDisconnectedBaselineWithoutSacrificingAnyRemainingAccess()
    {
        Assert.True(ParallelConstructionPolicy.Preserved([false,false],[false,false],[true,false],repair:true));
        Assert.False(ParallelConstructionPolicy.Preserved([false,false],[false,false],[true,false]));
        Assert.False(ParallelConstructionPolicy.Preserved([true,false],[true,false],[false,true],repair:true));
        Assert.False(ParallelConstructionPolicy.Preserved([false],[true],[true],repair:true));
        Assert.False(ParallelConstructionPolicy.Preserved([],[],[],repair:true));
    }

    [Fact]
    public void ReloadRepairDoesNotRequireOrInventAnOldParent()
    {
        var args=new NameValueCollection { ["session"]="11111111-1111-1111-1111-111111111111",
            ["districtId"]="22222222-2222-2222-2222-222222222222", ["steps"]="Path,1,2,3,0," };
        var request=LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-repair",args);
        Assert.Equal("",request.ParentActionId);
        foreach(var tool in LargeProjectTools.Catalog(true,true).Where(t=>t.Name is "plan_large_project_repair" or "run_large_project_repair"))
            Assert.DoesNotContain("parentActionId",tool.InputSchema.GetProperty("required").EnumerateArray().Select(v=>v.GetString()));
        args["parentActionId"]="invalid";
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-repair",args));
        Assert.False(LargeProjectRecoveryPolicy.AccessHelper("SmallWarehouse.IronTeeth"));
    }

    [Theory]
    [InlineData("project_priority_revision_changed","not_executed")]
    [InlineData("project_priority_identity_unavailable","not_executed")]
    [InlineData("state_conflict","unconfirmed")]
    [InlineData("lost","unconfirmed")]
    public async Task PriorityPreconditionsAreCertainButGenericTransportFailuresAreNot(string code,string execution)
    {
        using var handler=new Reject(code);
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingPlacement:true);
        var result=await tools.Invoke("set_large_project_priority",JsonSerializer.SerializeToElement(new {
            session="11111111-1111-1111-1111-111111111111", actionId="22222222-2222-2222-2222-222222222222",
            expectedRevision=new string('a',64),priority="High"
        }),TestContext.Current.CancellationToken);
        Assert.Equal(execution,result["error"]!["execution"]!.GetValue<string>());
        Assert.Equal(1,handler.Calls);
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        if(execution=="not_executed") {
            Assert.Equal(code,result["error"]!["code"]!.GetValue<string>());
            Assert.Contains("Priorität unverändert",result["error"]!["message"]!.GetValue<string>());
        }
    }

    [Fact]
    public async Task FailedPreviewDoesNotClaimToHaveSubmittedABuildOrder()
    {
        using var handler=new Reject("invalid_argument");
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingPlacement:true);
        var result=await tools.Invoke("plan_large_project",JsonSerializer.SerializeToElement(new {
            session="11111111-1111-1111-1111-111111111111",districtId="22222222-2222-2222-2222-222222222222",
            steps=new[]{new {template="Path",x=1,y=2,z=3,rotation=0,dependsOn=Array.Empty<int>()}}
        }),TestContext.Current.CancellationToken);
        Assert.Contains("keine Bauaufträge",result["error"]!["message"]!.GetValue<string>());
        Assert.Contains("unbestätigt",result["error"]!["message"]!.GetValue<string>());
        Assert.Equal(1,handler.Calls);
    }

    private sealed class Reject(string code):HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++; Assert.Equal(HttpMethod.Post,request.Method);
            if(code=="lost")throw new HttpRequestException();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict) {
                Content=new StringContent(JsonSerializer.Serialize(new {error=code})) });
        }
    }
}
