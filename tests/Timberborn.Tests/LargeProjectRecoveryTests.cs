using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class LargeProjectRecoveryTests
{
    private const string Session="11111111-1111-1111-1111-111111111111";
    private const string Parent="22222222-2222-2222-2222-222222222222";
    private const string Child="33333333-3333-3333-3333-333333333333";
    private const string Entity="44444444-4444-4444-4444-444444444444";
    private static LargeProjectRequest Read(string operation)=>LargeProjectRequest.Parse("/agent-api/v1/large-project-"+operation,new(){{"session",Session},{"actionId",Parent}});

    [Theory]
    [InlineData("pending","missing",true,true)]
    [InlineData("pending","finished",false,false)]
    [InlineData("unconfirmed","missing",false,false)]
    [InlineData("unconfirmed","initializing",false,false)]
    [InlineData("unconfirmed","finished",true,true)]
    [InlineData("construction","missing",true,true)]
    [InlineData("construction","construction_unreachable",false,true)]
    [InlineData("construction","construction_access_unknown",false,false)]
    [InlineData("construction","identity_mismatch",false,false)]
    [InlineData("finished","finished_access_missing",false,false)]
    [InlineData("finished","finished",true,true)]
    public void FreshEvidenceDistinguishesAbsentOrdersFromRemovedObjects(string recorded,string observed,bool close,bool repair)
    {
        Assert.Equal(close,LargeProjectRecoveryPolicy.Resolved(recorded,observed));
        Assert.Equal(repair,LargeProjectRecoveryPolicy.Repairable(recorded,observed));
    }

    [Fact]
    public void CloseAfterConfirmedRemovalAllowsNewProjectWithoutReplayingOldOrders()
    {
        var ledger=new LargeProjectLedger();
        ledger.Start(Parent,new string('a',64),"original",[0,1],[Entity,Session]);
        int placements=0;
        ledger.Advance(Parent,0,()=>true,_=>"construction",_=>true,_=>placements++);
        ledger.Advance(Parent,1,()=>true,_=>"construction",_=>true,_=>placements++);
        ledger.Stop(Parent);
        Assert.Throws<BridgeRejectionException>(()=>ledger.Release(Parent,["construction","missing"]));
        Assert.True(ledger.BlocksNewProject);
        ledger.Release(Parent,["missing","missing"]);
        Assert.False(ledger.BlocksNewProject);
        Assert.True(ledger.Inspect(Parent).Released);
        ledger.Release(Parent,["missing","missing"]);
        ledger.Start(Child,new string('b',64),"new",[0],[Child]);
        ledger.Advance(Parent,10,()=>true,_=>"finished",_=>true,_=>placements++);
        Assert.Equal(2,placements);
        Assert.Equal("stopped",ledger.Inspect(Parent).State);
    }

    [Fact]
    public void FailedPlacementCannotBeReleasedFromAbsenceButFreshMatchingFinishResolvesIt()
    {
        var ledger=new LargeProjectLedger();
        ledger.Start(Parent,new string('a',64),"original",[0],[Entity]);
        ledger.Advance(Parent,0,()=>true,_=>"missing",_=>true,_=>throw new InvalidOperationException());
        ledger.Advance(Parent,6,()=>true,_=>"missing",_=>true,_=>throw new InvalidOperationException());
        Assert.Throws<BridgeRejectionException>(()=>ledger.Release(Parent,["missing"]));
        ledger.Release(Parent,["finished"]);
        Assert.False(ledger.BlocksNewProject);
        Assert.Equal("unconfirmed",ledger.Inspect(Parent).State);
    }

    [Theory]
    [InlineData(false,false)] [InlineData(true,false)] [InlineData(false,true)] [InlineData(true,true)]
    public void RecoveryGatesAndMethodsAreIndependent(bool building,bool speed)
    {
        var catalog=LargeProjectTools.Catalog(building,speed).ToArray();
        Assert.True(Assert.Single(catalog,t=>t.Name=="diagnose_large_project").Annotations!.ReadOnlyHint);
        Assert.Equal(building,catalog.Any(t=>t.Name=="close_large_project"));
        Assert.Equal(building,catalog.Any(t=>t.Name=="plan_large_project_repair"));
        Assert.Equal(building&&speed,catalog.Any(t=>t.Name=="run_large_project_repair"));
        foreach(var operation in new[]{"close","plan-repair","run-repair"})
        {
            string route="/agent-api/v1/large-project-"+operation;
            Assert.Equal(building&&(operation!="run-repair"||speed),BridgeHttpServer.MethodAllowed("POST",route,false,false,enableBuildingPlacement:building,enableSpeedControl:speed));
            Assert.False(BridgeHttpServer.MethodAllowed("GET",route,false,false,enableBuildingPlacement:building,enableSpeedControl:speed));
        }
        Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/large-project-diagnose",false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/large-project-diagnose",false,false,enableBuildingPlacement:true));
    }

    [Fact]
    public void RepairBindsParentAndRequiresItsOwnIdentityAndBudget()
    {
        var args=new NameValueCollection {{"session",Session},{"parentActionId",Parent},{"districtId",Session},
            {"steps","Path,1,2,3,0,"},{"actionId",Child},{"planKey",new string('a',64)},
            {"mode","development_pilot"},{"speed","7"},{"maxGameHours","24"},{"maxRealSeconds","30"}};
        var parsed=LargeProjectRequest.Parse("/agent-api/v1/large-project-run-repair",args);
        Assert.Equal(Parent,parsed.ParentActionId);Assert.Equal(Child,parsed.ActionId);
        Assert.Equal(24,parsed.MaxGameHours);
        args["actionId"]=Parent;
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-run-repair",args));
        args["actionId"]=Child;args.Remove("speed");
        Assert.Throws<ArgumentException>(()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-run-repair",args));
        Assert.True(LargeProjectRecoveryPolicy.AccessHelper("Stairs.Folktails"));
        Assert.False(LargeProjectRecoveryPolicy.AccessHelper("WindTurbine.Folktails"));
    }

    private static JsonObject Report()=>new() {
        ["actionId"]=Parent,["state"]="blocked",["released"]=false,["closeOutcome"]="rejected",["currentSpeed"]=0,
        ["closeEligible"]=false,["repairEligible"]=true,["faulted"]=false,["busy"]=false,
        ["lostConnections"]=0,["affected"]=new JsonArray(),["affectedTruncated"]=false,
        ["foreignSites"]=new JsonArray(),["foreignSitesTruncated"]=false,["unissuedParts"]=0,
        ["parts"]=new JsonArray(new JsonObject{["index"]=0,["id"]=Entity,["template"]="Synthetic",
            ["position"]=new JsonObject{["x"]=1,["y"]=2,["z"]=3},["recorded"]="unconfirmed",
            ["observed"]="construction_unreachable",["reason"]="builders_unreachable"}),
        ["limitations"]=new JsonArray("synthetic") };
    private static BridgeEnvelope<JsonElement> Envelope(JsonObject report)=>new(1,Session,DateTimeOffset.UtcNow,"0.37.0",JsonSerializer.SerializeToElement(report));

    [Fact]
    public void RecoveryContractRejectsFalseCloseAndForeignSiteRepair()
    {
        var report=Report();NativeClient.ValidateLargeProject(Envelope(report),Read("diagnose"));
        report["closeEligible"]=true;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(report),Read("close")));
        report["closeEligible"]=false;report["foreignSites"]=new JsonArray(Child);
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(report),Read("diagnose")));
    }

    [Theory]
    [InlineData("diagnose","GET")] [InlineData("close","POST")]
    public async Task RecoveryTransportUsesExactIdentityAndNoRetry(string operation,string method)
    {
        var handler=new Handler(operation,method);
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        await client.LargeProject(Read(operation),TestContext.Current.CancellationToken);
        Assert.Equal(1,handler.Calls);
    }
    private sealed class Handler(string operation,string method):HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;Assert.Equal(method,request.Method.Method);
            Assert.Equal("/agent-api/v1/large-project-"+operation,request.RequestUri!.AbsolutePath);
            Assert.Equal("?session="+Session+"&actionId="+Parent,request.RequestUri.Query);
            Assert.Equal("Bearer",request.Headers.Authorization!.Scheme);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(Envelope(Report()),NativeJson.Options))});
        }
    }
}
