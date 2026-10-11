using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class BuildingConfigurationTests
{
    private static string Id(int n)=>new Guid(n,0,0,new byte[8]).ToString("D");
    private static NameValueCollection Query(int count=2,bool proof=false)
    {
        var q=new NameValueCollection{{"session",Id(100)},{"jobId",Id(101)},
            {"items",string.Join(";",Enumerable.Range(1,count).Select(n=>$"{Id(n)}|Bread|Flour|2|1|false|true"))}};
        if(proof){q["proofHours"]="2";q["proofSpeed"]="3";q["proofMaxRealSeconds"]="60";}return q;
    }
    private static BuildingConfigurationRequest Parse(NameValueCollection q)=>BuildingConfigurationRequest.Parse("/agent-api/v1/configure-buildings",q);
    private sealed class Fake:IBuildingConfigurationPort
    {
        public Dictionary<string,ConfigurationObservation> States=Enumerable.Range(1,8).ToDictionary(Id,_=>new ConfigurationObservation {Template="Synthetic",Recipe="Flour",DesiredWorkers=1,Paused=true});
        public List<string> Writes=new();public int Reads;public int? ChangeAt;public bool FailWorkers;
        public ConfigurationCheck Read(string id) {
            Reads++;if(Reads==ChangeAt)States[id].Recipe="Changed";
            var s=States[id];return new(){State=new(){Template=s.Template,Recipe=s.Recipe,DesiredWorkers=s.DesiredWorkers,Paused=s.Paused},Recipes=new[]{"Flour","Bread"},MaxWorkers=2,CanPause=true};
        }
        public void SetRecipe(string id,string value){Writes.Add("recipe:"+id);States[id].Recipe=value;}
        public void SetWorkers(string id,int value){Writes.Add("workers:"+id);if(FailWorkers)throw new InvalidOperationException();States[id].DesiredWorkers=value;}
        public void SetPaused(string id,bool value){Writes.Add("paused:"+id);States[id].Paused=value;}
    }
    [Fact]
    public void EightConfigurationsThenOneProofAndFrozenHistory()
    {
        var c=new BuildingConfigurationController();var p=new Fake();var r=Parse(Query(8,true));int starts=0;
        var result=c.Execute(r,p,()=>{},()=>{Assert.Equal(24,p.Writes.Count);starts++;});
        Assert.Equal("applied",result.Outcome);Assert.Equal("started",result.ProofState);Assert.Equal(r.JobId,result.RunId);Assert.Equal(1,starts);
        Assert.All(result.Items,i=>{Assert.Equal("applied",i.Outcome);Assert.Equal(new[]{"recipe","workers","paused"},i.AppliedFields);});
        p.States[Id(1)].Recipe="Later";Assert.Equal("Bread",c.Inspect(r.JobId).Items[0].Observed!.Recipe);
        Assert.Throws<BridgeRejectionException>(()=>c.Execute(r,p,()=>{},()=>{}));Assert.Equal(24,p.Writes.Count);
        BuildingConfigurationContract.Validate(result,r);
        Assert.Equal(8,r.ProofRequest().BuildingIds.Length);
    }
    [Theory]
    [InlineData("unavailable_recipe")]
    [InlineData("state_conflict")]
    [InlineData("staffing_out_of_range")]
    public void WholePreflightRejectsWithoutAnyMutation(string failure)
    {
        var q=Query();q["items"]=failure switch {
            "unavailable_recipe"=>q["items"]!.Replace("Bread|Flour","Missing|Flour"),
            "staffing_out_of_range"=>q["items"]!.Replace("|2|1|","|3|1|"),_=>q["items"]};
        var p=new Fake();if(failure=="state_conflict")p.States[Id(2)].Paused=false;
        var r=Parse(q);var result=new BuildingConfigurationController().Execute(r,p,()=>{},()=>throw new Exception());
        Assert.Equal("rejected",result.Outcome);Assert.Empty(p.Writes);Assert.Contains(result.Items,i=>i.Code==failure);
        BuildingConfigurationContract.Validate(result,r);
    }
    [Fact]
    public void ZeroWorkersIsRejectedForWholeBatchBeforeAnySetter()
    {
        var q=Query(2);q["items"]=q["items"]!.Replace("|2|1|","|0|1|");
        var p=new Fake();var r=Parse(q);
        var result=new BuildingConfigurationController().Execute(r,p,()=>{},()=>throw new Exception());
        Assert.Equal("rejected",result.Outcome);Assert.Empty(p.Writes);
        Assert.All(result.Items,i=>Assert.Equal("staffing_out_of_range",i.Code));
        BuildingConfigurationContract.Validate(result,r);
        var tool=BuildingConfigurationTools.Catalog(true).Single(t=>t.Name=="configure_buildings");
        Assert.Equal(1,tool.InputSchema.GetProperty("properties").GetProperty("items").GetProperty("items").GetProperty("properties").GetProperty("desiredWorkers").GetProperty("minimum").GetInt32());
    }

    [Fact]
    public void ConflictAfterPreflightStopsBeforeWriting()
    {
        var p=new Fake{ChangeAt=3};var r=Parse(Query());
        var result=new BuildingConfigurationController().Execute(r,p,()=>{},()=>{});
        Assert.Equal("partial",result.Outcome);Assert.Equal("state_conflict",result.Items[0].Code);Assert.Empty(p.Writes);
        Assert.Equal("not_attempted",result.Items[1].Outcome);BuildingConfigurationContract.Validate(result,r);
    }
    [Fact]
    public void SetterFailureRetainsConfirmedStepsAndDoesNotStartProof()
    {
        var p=new Fake{FailWorkers=true};var r=Parse(Query(2,true));
        var result=new BuildingConfigurationController().Execute(r,p,()=>{},()=>throw new Exception());
        Assert.Equal("partial",result.Outcome);Assert.Equal("unconfirmed",result.Items[0].Outcome);
        Assert.Equal(new[]{"recipe"},result.Items[0].AppliedFields);Assert.Equal("workers",result.Items[0].AttemptedField);
        Assert.Equal("not_attempted",result.Items[1].Outcome);Assert.Equal("not_started",result.ProofState);
        Assert.Equal(2,p.Writes.Count);BuildingConfigurationContract.Validate(result,r);
    }
    [Fact]
    public void ProofFailureKeepsSettingsAndInspectionIdentity()
    {
        var r=Parse(Query(1,true));var result=new BuildingConfigurationController().Execute(r,new Fake(),()=>{},()=>throw new BridgeRejectionException("building_proof_journal_gap"));
        Assert.Equal("applied",result.Outcome);Assert.Equal("unconfirmed",result.ProofState);Assert.Equal("building_proof_journal_gap",result.ProofCode);
        Assert.Equal(r.JobId,result.RunId);BuildingConfigurationContract.Validate(result,r);
    }
    [Fact]
    public void AvailabilityFailureDoesNotConsumeIdOrTouchSettings()
    {
        var c=new BuildingConfigurationController();var p=new Fake();var r=Parse(Query());
        Assert.Throws<BridgeRejectionException>(()=>c.Execute(r,p,()=>throw new BridgeRejectionException("state_conflict"),()=>{}));
        Assert.Empty(p.Writes);Assert.Equal("applied",c.Execute(r,p,()=>{},()=>{}).Outcome);
    }
    [Fact]
    public void InvalidSelectionsAndIncompleteProofAreRejected()
    {
        Assert.Throws<ArgumentException>(()=>Parse(Query(9)));
        var q=Query();q["items"]+=";"+q["items"]!.Split(';')[0];Assert.Throws<ArgumentException>(()=>Parse(q));
        q=Query();q["proofSpeed"]="3";Assert.Throws<ArgumentException>(()=>Parse(q));
        q=Query(1,true);q.Remove("proofSpeed");Assert.Throws<ArgumentException>(()=>Parse(q));
        q=Query(1,true);q["items"]=q["items"]!.Replace("|false|true","|true|true");Assert.Throws<ArgumentException>(()=>Parse(q));
    }
    [Theory]
    [InlineData(false,true,true)]
    [InlineData(true,false,true)]
    [InlineData(true,true,false)]
    [InlineData(true,true,true)]
    public void AllActionGatesAreRequired(bool settings,bool staffing,bool speed)
    {
        bool allowed=settings&&staffing&&speed;
        Assert.Equal(allowed,BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/configure-buildings",false,false,enableBuildingSettings:settings,enableStaffing:staffing,enableSpeedControl:speed));
        Assert.Equal(allowed,NativeTools.Catalog(enableBuildingSettings:settings,enableStaffing:staffing,enableSpeedControl:speed).Any(t=>t.Name=="configure_buildings"));
        Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/building-configuration",false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/configure-buildings",false,false,enableBuildingSettings:true,enableStaffing:true,enableSpeedControl:true));
    }

    [Fact]
    public void AlreadyConfiguredTargetsDoNotInvokeSetters()
    {
        var q=Query(1);q["items"]=q["items"]!.Replace("Bread|Flour|2|1|false|true","Flour|Flour|1|1|true|true");
        var p=new Fake();var r=Parse(q);var result=new BuildingConfigurationController().Execute(r,p,()=>{},()=>throw new Exception());
        Assert.Equal("applied",result.Outcome);Assert.Empty(p.Writes);Assert.Equal("not_requested",result.ProofState);
        BuildingConfigurationContract.Validate(result,r);
    }

    [Fact]
    public async Task StructuredRequestRoundTripsAndForgedSuccessIsRejected()
    {
        var q=Query();var r=Parse(q);var result=new BuildingConfigurationController().Execute(r,new Fake(),()=>{},()=>{});
        var payload=System.Text.Json.JsonSerializer.Serialize(new BridgeEnvelope<BuildingConfigurationResult>(1,r.Session,DateTimeOffset.UnixEpoch,"0.42.0",result),NativeJson.Options);
        using var handler=new Handler(payload);using var client=new NativeClient(new NativeConfiguration(8081,new string('a',64)),handler);
        var args=System.Text.Json.JsonSerializer.SerializeToElement(new {session=r.Session,jobId=r.JobId,items=r.Items},NativeJson.Options);
        var response=await BuildingConfigurationTools.Invoke(client,"configure_buildings",args,true,TestContext.Current.CancellationToken);
        Assert.Equal("applied",response["data"]!["outcome"]!.GetValue<string>());
        Assert.Equal(HttpMethod.Post,handler.Method);Assert.Contains("items=",handler.Uri!.Query);
        result.Items[0].Observed!.Recipe="Unexpected";
        Assert.Throws<InvalidDataException>(()=>BuildingConfigurationContract.Validate(result,r));
    }
    private sealed class Handler(string payload):HttpMessageHandler
    {
        public Uri? Uri;public HttpMethod? Method;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {Uri=request.RequestUri;Method=request.Method;return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=new StringContent(payload)});}
    }
}
