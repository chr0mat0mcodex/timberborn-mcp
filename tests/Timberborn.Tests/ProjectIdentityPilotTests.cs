using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ProjectIdentityPilotTests
{
    private const string Session = "11111111-1111-1111-1111-111111111111";
    private const string Parent = "22222222-2222-2222-2222-222222222222";
    private const string Other = "33333333-3333-3333-3333-333333333333";
    private const string Tool = "set_large_project_identity_pilot";
    private const string Route = "/agent-api/v1/large-project-identity-pilot";

    private static NameValueCollection Query(string action = "enable") => new() {
        {"session",Session},{"actionId",Parent},{"partIndex","0"},
        {"pilotAction",action},{"mode","development_pilot"}
    };

    [Fact]
    public void InactiveByDefaultAndBoundToOneExactTarget()
    {
        var pilot = new ProjectIdentityPilot();
        Assert.False(pilot.Active);
        Assert.Equal("finished", pilot.Observe(Session, Parent, 0, "finished"));
        pilot.Enable(Session, Parent, 0, "finished", "finished", true, false, true);
        Assert.Equal("identity_mismatch", pilot.Observe(Session, Parent, 0, "finished"));
        Assert.Equal("finished", pilot.Observe(Other, Parent, 0, "finished"));
        Assert.Equal("finished", pilot.Observe(Session, Other, 0, "finished"));
        Assert.Equal("finished", pilot.Observe(Session, Parent, 1, "finished"));
        Assert.Throws<BridgeRejectionException>(() => pilot.Enable(Session, Other, 0, "finished", "finished", true, false, true));
        Assert.Throws<BridgeRejectionException>(() => pilot.Disable(Other, Parent, 0));
        Assert.True(pilot.Active);
        pilot.Disable(Session, Parent, 0);
        Assert.Equal("missing", pilot.Observe(Session, Parent, 0, "missing"));
        Assert.False(pilot.Active);
        pilot.Disable(Session, Parent, 0);
        pilot.Enable(Session, Parent, 0, "finished", "finished", true, false, true);
        pilot.Clear(); // Game-session unload.
        Assert.False(pilot.Active);
        Assert.Equal("", pilot.Session);
    }

    [Theory]
    [InlineData("unconfirmed", "finished", true, false, true)]
    [InlineData("pending", "finished", true, false, true)]
    [InlineData("finished", "missing", true, false, true)]
    [InlineData("finished", "initializing", true, false, true)]
    [InlineData("finished", "identity_mismatch", true, false, true)]
    [InlineData("finished", "finished", false, false, true)]
    [InlineData("finished", "finished", true, true, true)]
    [InlineData("finished", "finished", true, false, false)]
    public void ActivationRequiresConfirmedRealIdentityAndIdleTerminalProject(string recorded, string observed, bool terminal, bool released, bool idle)
    {
        var pilot = new ProjectIdentityPilot();
        Assert.Throws<BridgeRejectionException>(() => pilot.Enable(Session, Parent, 0, recorded, observed, terminal, released, idle));
        Assert.False(pilot.Active);
    }

    [Fact]
    public void InjectedObservationBlocksReleaseWithoutChangingLedger()
    {
        var ledger = new LargeProjectLedger();
        var receipt = ledger.Start(Parent, new string('a',64), "synthetic", [0], [Other]);
        int placements = 0;
        ledger.Advance(Parent, 0, () => true, _ => "construction", _ => true, _ => placements++);
        ledger.Advance(Parent, 1, () => true, _ => "construction", _ => true, _ => placements++);
        ledger.Stop(Parent);
        string before = JsonSerializer.Serialize(receipt);
        var pilot = new ProjectIdentityPilot();
        pilot.Enable(Session, Parent, 0, receipt.PartStates[0], "finished", true, false, true);
        string observed = pilot.Observe(Session, Parent, 0, "finished");
        Assert.False(LargeProjectRecoveryPolicy.Resolved(receipt.PartStates[0], observed));
        Assert.False(LargeProjectRecoveryPolicy.Repairable(receipt.PartStates[0], observed));
        Assert.Throws<BridgeRejectionException>(() => ledger.Release(Parent, [observed]));
        Assert.Equal(before, JsonSerializer.Serialize(receipt));
        Assert.Equal(1, placements);
        pilot.Disable(Session, Parent, 0);
        ledger.Release(Parent, [pilot.Observe(Session, Parent, 0, "finished")]);
        Assert.True(receipt.Released);
        Assert.Equal(1, placements);
    }

    [Theory]
    [InlineData("mode", "regular")]
    [InlineData("pilotAction", "missing")]
    [InlineData("partIndex", "-1")]
    [InlineData("partIndex", "32")]
    [InlineData("session", "00000000-0000-0000-0000-000000000000")]
    public void RequestRejectsWiderOrUnboundInjection(string key, string value)
    {
        var query = Query(); query[key] = value;
        Assert.Throws<ArgumentException>(() => LargeProjectRequest.Parse(Route, query));
    }

    [Fact]
    public void RequestAndCatalogRequireExplicitWriteOptIn()
    {
        var query = Query(); query.Add("observed", "finished");
        Assert.Throws<ArgumentException>(() => LargeProjectRequest.Parse(Route, query));
        query = Query(); query.Remove("mode");
        Assert.Throws<ArgumentException>(() => LargeProjectRequest.Parse(Route, query));
        Assert.DoesNotContain(LargeProjectTools.Catalog(false), t => t.Name == Tool);
        var tool = Assert.Single(LargeProjectTools.Catalog(true), t => t.Name == Tool);
        Assert.False(tool.Annotations!.ReadOnlyHint);
        Assert.True(LargeProjectTools.Writes(Tool));
        Assert.Contains("mode", tool.InputSchema.GetProperty("required").EnumerateArray().Select(v => v.GetString()));
        Assert.False(BridgeHttpServer.MethodAllowed("GET", Route, false, false, enableBuildingPlacement:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST", Route, false, false));
        Assert.True(BridgeHttpServer.MethodAllowed("POST", Route, false, false, enableBuildingPlacement:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST", Route, true, false, enableBuildingPlacement:true));
        Assert.Equal("identity-pilot",BridgeRequest.Parse(Route,Query()).LargeProject!.Operation);
    }

    [Fact]
    public async Task DisabledToolDoesNotDispatch()
    {
        var handler=new Handler(new JsonObject());
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        await Assert.ThrowsAsync<ArgumentException>(()=>LargeProjectTools.Invoke(client,Tool,
            JsonSerializer.SerializeToElement(new {session=Session,actionId=Parent,partIndex=0,pilotAction="enable",mode="development_pilot"}),
            false,TestContext.Current.CancellationToken));
        Assert.Equal(0,handler.Calls);
    }

    [Fact]
    public void ActivationAcknowledgementCannotInventMatchingIdentity()
    {
        var report=new JsonObject { ["actionId"]=Parent,["pilotAction"]="enable",
            ["identityInjection"]=Evidence(),["limitations"]=new JsonArray("synthetic") };
        var request=LargeProjectRequest.Parse(Route,Query());
        NativeClient.ValidateLargeProject(Envelope(report),request);
        report["identityInjection"]!["realObserved"]="missing";
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(report),request));
        report["identityInjection"]!["realObserved"]="finished";
        report["identityInjection"]!["active"]=false;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(report),request));
    }

    private static JsonObject Evidence(bool active = true) => new() {
        ["source"]="development_identity_injection",["injected"]=active,["active"]=active,
        ["session"]=Session,["actionId"]=Parent,["partIndex"]=0,
        ["injectedObservation"]="identity_mismatch",["realObserved"]="finished",
        ["scope"]="recovery_observation_only_no_entity_or_ledger_mutation"
    };
    private static BridgeEnvelope<JsonElement> Envelope(JsonObject data) =>
        new(1, Session, DateTimeOffset.UtcNow, "0.74.0", JsonSerializer.SerializeToElement(data));

    [Theory]
    [InlineData("enable")]
    [InlineData("disable")]
    public async Task PilotWireUsesOnePostAndRetainsEvidence(string action)
    {
        var report = new JsonObject { ["actionId"]=Parent,["pilotAction"]=action,
            ["identityInjection"]=Evidence(action=="enable"),["limitations"]=new JsonArray("synthetic") };
        var handler = new Handler(report);
        using var client = new NativeClient(new(8081,new string('a',64)),handler);
        var result = await LargeProjectTools.Invoke(client, Tool, JsonSerializer.SerializeToElement(new {
            session=Session, actionId=Parent, partIndex=0, pilotAction=action, mode="development_pilot"
        }), true, TestContext.Current.CancellationToken);
        Assert.Equal(1,handler.Calls);
        Assert.Equal(Route,handler.LastUri!.AbsolutePath);
        Assert.Equal($"?session={Session}&actionId={Parent}&pilotAction={action}&partIndex=0&mode=development_pilot",handler.LastUri.Query);
        Assert.Equal(action=="enable",result["data"]!["identityInjection"]!["injected"]!.GetValue<bool>());
        Assert.Contains(action=="enable" ? "INJIZIERT" : "deaktiviert", ActivityOutcome.Describe(Tool,result));
    }

    [Theory]
    [InlineData("source", "natural")]
    [InlineData("session", Other)]
    [InlineData("actionId", Other)]
    [InlineData("injectedObservation", "finished")]
    [InlineData("realObserved", "invented")]
    public void ContractRejectsMisleadingEvidence(string field, string value)
    {
        var evidence=Evidence(); evidence[field]=value;
        Assert.Throws<InvalidDataException>(() => ProjectIdentityPilotContract.Validate(JsonSerializer.SerializeToElement(evidence),Session,Parent,true));
    }

    [Fact]
    public async Task NativeRepairRejectionRetainsInjectedEvidenceAsErrorWithoutRetry()
    {
        var report=new JsonObject { ["parentActionId"]=Parent,["rejectionCode"]="repair_parent_ineligible",
            ["outcome"]="rejected",["execution"]="not_executed",["identityInjection"]=Evidence(),
            ["limitations"]=new JsonArray("synthetic") };
        var handler=new Handler(report);
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var result=await LargeProjectTools.Invoke(client,"plan_large_project_repair",JsonSerializer.SerializeToElement(new {
            session=Session,parentActionId=Parent,districtId=Other,
            steps=new[]{new { template="Path",x=1,y=2,z=3,rotation=0,dependsOn=Array.Empty<int>() }}
        }),true,TestContext.Current.CancellationToken);
        Assert.Equal("error",result["status"]!.GetValue<string>());
        Assert.Equal("repair_parent_ineligible",result["error"]!["code"]!.GetValue<string>());
        Assert.Equal("not_executed",result["error"]!["execution"]!.GetValue<string>());
        Assert.True(result["data"]!["identityInjection"]!["injected"]!.GetValue<bool>());
        ProjectReportPresentation.Compact("plan_large_project_repair",result["data"]!.AsObject());
        Assert.Equal("finished",result["data"]!["identityInjection"]!["realObserved"]!.GetValue<string>());
        Assert.Equal(1,handler.Calls);
    }

    [Theory]
    [InlineData("diagnose")]
    [InlineData("close")]
    public void RecoveryContractAndCompactOutputPreserveInjectedBlocker(string operation)
    {
        var request=LargeProjectRequest.Parse("/agent-api/v1/large-project-"+operation,
            new NameValueCollection{{"session",Session},{"actionId",Parent}});
        // Minimal older recovery fixture isolates the additive injection contract.
        var report=new JsonObject {
            ["actionId"]=Parent,["state"]="stopped",["released"]=false,["closeOutcome"]="rejected",
            ["currentSpeed"]=0,["closeEligible"]=false,["repairEligible"]=false,["faulted"]=false,["busy"]=false,
            ["lostConnections"]=0,["affected"]=new JsonArray(),["affectedTruncated"]=false,
            ["foreignSites"]=new JsonArray(),["foreignSitesTruncated"]=false,["unissuedParts"]=0,
            ["identityInjection"]=Evidence(),["limitations"]=new JsonArray("synthetic"),
            ["parts"]=new JsonArray(new JsonObject {
                ["index"]=0,["id"]=Other,["template"]="Synthetic",["recorded"]="finished",
                ["observed"]="identity_mismatch",["reason"]="identity_mismatch",
                ["position"]=new JsonObject{["x"]=1,["y"]=2,["z"]=3}
            })
        };
        BridgeEnvelope<JsonElement> Wrap()=>new(1,Session,DateTimeOffset.UtcNow,"0.37.0",JsonSerializer.SerializeToElement(report));
        NativeClient.ValidateLargeProject(Wrap(),request);
        foreach(string field in new[]{"closeEligible","repairEligible","released"}) {
            report[field]=true;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Wrap(),request));
            report[field]=false;
        }
        report["identityInjection"]!["partIndex"]=1;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Wrap(),request));
        report["identityInjection"]!["partIndex"]=0;
        ProjectReportPresentation.Compact(operation=="close" ? "close_large_project" : "diagnose_large_project",report);
        Assert.True(report["identityInjection"]!["injected"]!.GetValue<bool>());
        Assert.Equal("identity_mismatch",report["parts"]![0]!["observed"]!.GetValue<string>());
    }

    private sealed class Handler(JsonObject data) : HttpMessageHandler
    {
        public int Calls;
        public Uri? LastUri;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++; LastUri=request.RequestUri;
            Assert.Equal(HttpMethod.Post,request.Method);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content=new StringContent(JsonSerializer.Serialize(Envelope(data),NativeJson.Options)) });
        }
    }
}
