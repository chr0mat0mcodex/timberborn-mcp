using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Timberborn.TestFixtures;
using Xunit;

namespace Timberborn.Tests;

public sealed class AutoOperatingTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string Center = "22222222-2222-4222-8222-222222222222";
    private const string Producer = "33333333-3333-4333-8333-333333333333";
    private const string Other = "44444444-4444-4444-8444-444444444444";
    private static NativeBuilding Building(string id, string template, string? district) => new(Guid.Parse(id), true,
        new(template, new(1,1,1), true, false, false, null,
            new(district is not null, district is null ? null : Guid.Parse(district), null, null)), []);

    [Fact]
    public void DistrictEvidenceNeverUsesDistanceOrConflictingAssignments()
    {
        var b=Building(Producer,"Workshop",Center);
        Assert.Equal(Center, AutoOperatingTools.District(b));
        Assert.Null(AutoOperatingTools.District(Building(Producer,"Workshop",null)));
        Assert.Null(AutoOperatingTools.District(b with { Details = b.Details! with {
            District = new(true, Guid.Parse(Center), Guid.Parse(Other), null) } }));
        Assert.Equal(Center, AutoOperatingTools.District(Building(Center,"DistrictCenter.Folktails",null)));
    }

    [Fact]
    public void CostsIncludeEveryStepAndUnknownTemplatesAreExplicit()
    {
        var definitions=ProductionGraphFixture.Create().Definitions;
        var (demands,unknown)=AutoOperatingTools.Costs(definitions,
            [new(){Template="Workshop"},new(){Template="Workshop"},new(){Template="UnknownHelper"}]);
        Assert.Equal(new OperatingDemand("A",10),Assert.Single(demands));
        Assert.Equal(new[]{"UnknownHelper"},unknown);
        Assert.Contains("Workshop",AutoOperatingTools.ProducerTemplates(definitions,["A"]));
        Assert.Contains("Workshop",AutoOperatingTools.ProducerTemplates(definitions,["B"]));
    }

    private sealed class Reply : HttpMessageHandler
    {
        public List<string> Calls { get; }=[];
        public bool TwoDistricts, Truncated, Stale, FailGraph, Planks, FailChain, ExpireChain, StaleChain;
        public List<Uri> Requests { get; }=[];
        public CancellationTokenSource? CancelCaller;
        private NativeProductionGraph Graph() {
            if(!Planks)return ProductionGraphFixture.Create();
            var d=new GraphDefinitions([new("Log","Log",true),new("Plank","Plank",true)],
                [new("Forward",[new("Log",1)],[new("Plank",1)],1,null,0,0,["Workshop"])],
                [new("Workshop",[new("Plank",5)],0,true,"","",false,50,0)],
                [new("Tree.cut","Tree","cut","Log",1,1,null,null,true,true,["Workshop"],[],null)]);
            string revision=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                "synthetic-1\nSyntheticFaction\n"+JsonSerializer.Serialize(d,NativeJson.Options)))).ToLowerInvariant();
            return new("registered_definitions_and_active_scene_buildings","synthetic-1","SyntheticFaction",revision,true,d,new([],[],[]),[]);
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            var uri=request.RequestUri!; Requests.Add(uri);
            string route=uri.AbsolutePath.Split('/').Last(); Calls.Add(request.Method+" "+route);
            var q=System.Web.HttpUtility.ParseQueryString(uri.Query);
            if(route=="production-graph" && FailGraph) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            bool chain=Calls.Count(c=>c=="GET production-graph")>1;
            if(route=="production-graph" && chain && CancelCaller is {} cancel) {cancel.Cancel();ct.ThrowIfCancellationRequested();}
            if(route=="production-graph" && chain && ExpireChain)throw new OperationCanceledException(ct);
            if((route=="production-graph" && chain && FailChain) || route.StartsWith("large-project-") || route=="simulation-run-until")
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            object data=route switch {
                "production-graph"=>Graph(),
                "building-search"=>Search(int.Parse(q["offset"]!)),
                "building"=>q["id"]==Center ? Building(Center,"DistrictCenter.Folktails",null) :
                    q["id"]==Other ? Building(Other,"DistrictCenter.Folktails",null) : Building(Producer,"Workshop",Center),
                "building-operations"=>new OperationBatch(q["ids"]!.Split(',').Select(id=>new OperationBatchItem(id,"observed",Operation(id,q["diagnose"]=="true"))).ToArray(),1,[]),
                "building-access"=>new NativeAccess(q["id"]!,true,new(1,1,1),false,false,null,null,1,1,1,[],new("not_construction",[],false),0),
                "goods"=>new NativeGoods("global_registered_goods",0,32,1,[new(Planks?"Plank":"B","B","material","material",0,0,0,0,0,0,0,0,10,10)],false,[]),
                "snapshot"=>new NativeSnapshot("global",new[]{"Berries","Log","Water"}.Select(id=>new NativeResource(id,true,0,0,0,0,0,0)).ToArray(),
                    new(1,0,0,1),new(1,0,0),new(new(1,0,1,0,0),new(0,0,0,0,0)),new(64,64,16),0,[],false,[]),
                "simulation-run-for"=>new NativeSimulationRun(q["runId"]!,"starting","awaiting_speed",24,24+double.Parse(q["duration"]!,System.Globalization.CultureInfo.InvariantCulture),24,0,0,0,30,1,0,true,false,false,
                    GoodTargets:q["goodTargets"] is null ? null : [new(){Good=Planks?"Plank":"B",Amount=100,Available=0}],
                    BuildingIds:[],BuildingProduction:[]),
                _=>throw new InvalidOperationException(route)
            };
            string session=(Stale && route=="building-search") || (StaleChain && chain) ? Other : Session;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(
                new BridgeEnvelope<object>(1,session,DateTimeOffset.UnixEpoch,"0.74.0",data),NativeJson.Options))});
        }
        private BuildingSearch Search(int offset)
        {
            if(Truncated) {
                var items=Enumerable.Range(offset+1,32).Select(i=>new BuildingMatch(Guid.Parse($"00000000-0000-4000-8000-{i:D12}"),"Other",new(1,1,1),true,false,false,null)).ToArray();
                return new(offset,32,129,items,true,1,[]);
            }
            var result=new List<BuildingMatch>{new(Guid.Parse(Center),"DistrictCenter.Folktails",new(1,1,1),true,false,false,null),
                new(Guid.Parse(Producer),"Workshop",new(1,1,1),true,false,true,null)};
            if(TwoDistricts)result.Add(new(Guid.Parse(Other),"DistrictCenter.Folktails",new(1,1,1),true,false,false,null));
            return new(0,32,result.Count,result.ToArray(),false,1,[]);
        }
        private NativeOperation Operation(string id,bool diagnose=false)=>new(id,id==Center?"DistrictCenter.Folktails":"Workshop",true,false,
            new(1,1,1,false,true,true),id==Center?null:new(true,"Forward",false,false,true,true,true,0),[],[],[],
            Planks && id!=Center?new(true,true,false,true,0,50,true):new(false,null,null,null,null,null),null,
            diagnose?new BottleneckEvidence {State="observed",Complete=true,DistrictConnected=true,EntranceBlocked=false,
                Goods=[new(){Good="Plank",Role="product",Amount=1}]}:null,new(true,false,false,null,null,false,null));
    }
    private static NativeClient Client(Reply r)=>new(new(8081,new string('a',64)),r);

    [Fact]
    public async Task AutomaticSelectionUsesNativeDistrictAndPreservesMaterialShortfall()
    {
        using var reply=new Reply(); using var client=Client(reply);
        var e=await AutoOperatingTools.Observe(client,Session,null,[],[new("B",100)],[],"caller_demand",TestContext.Current.CancellationToken);
        Assert.Equal(Center,e.Data.Selection!.CenterId);
        Assert.Equal(new[]{Producer},e.Data.Selection.ProducerIds);
        Assert.Equal(0d,e.Data.Diagnosis!.Materials[0].CoveragePercent);
        Assert.Contains(e.Data.Diagnosis.NextSteps,s=>s.Code=="material_shortfall");
        Assert.All(reply.Calls,s=>Assert.StartsWith("GET ",s));
    }

    [Fact]
    public async Task MultipleDistrictsAndSearchLimitsRemainExplicit()
    {
        using var reply=new Reply{TwoDistricts=true}; using var client=Client(reply);
        var e=await AutoOperatingTools.Observe(client,Session,null,[],[new("B",0)],[],"operating_supply",TestContext.Current.CancellationToken);
        Assert.Null(e.Data.Selection!.CenterId);
        Assert.Null(e.Data.Selection.DistrictId);
        using var limited=new Reply{Truncated=true}; using var c=Client(limited);
        var partial=await AutoOperatingTools.Observe(c,Session,null,[],[new("B",0)],[],"operating_supply",TestContext.Current.CancellationToken);
        Assert.False(partial.Data.Selection!.SearchComplete);
        Assert.Equal(128,partial.Data.Selection.NextOffset);
        Assert.Equal(4,limited.Calls.Count(s=>s=="GET building-search"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreflightRunsBeforePostAndDoesNotGateOnShortfallOrUnavailable(bool unavailable)
    {
        using var reply=new Reply{FailGraph=unavailable}; using var native=new NativeTools(Client(reply),enableSpeedControl:true);
        var args=JsonSerializer.SerializeToElement(new{session=Session,runId=Other,duration=6,unit="hours",speed=1,expectedSpeed=0,maxRealSeconds=30,goodTargets=new[]{new{good="B",amount=100}}});
        var result=await native.Invoke("run_simulation_for",args,TestContext.Current.CancellationToken);
        Assert.Contains("POST simulation-run-for",reply.Calls);
        Assert.True(reply.Calls.IndexOf("GET production-graph")<reply.Calls.IndexOf("POST simulation-run-for"));
        Assert.Equal(unavailable?"unavailable":"observed",result["operatingPreflight"]![0]!["state"]!.GetValue<string>());
        Assert.True(result["status"]!.GetValue<string>()=="ok",result.ToJsonString());
        Assert.NotNull(ResponsePresentation.Present("run_simulation_for",result,false)["operatingPreflight"]);
        if (!unavailable) Assert.Contains(result["operatingPreflight"]![0]!["diagnosis"]!["nextSteps"]!.AsArray(),
            step=>step!["code"]!.GetValue<string>()=="material_shortfall");
        Assert.Single(reply.Calls, s=>s=="POST simulation-run-for");
    }

    [Fact]
    public async Task SessionConflictPreventsDispatchAndKeepsCancellationIndependent()
    {
        using var reply=new Reply{Stale=true}; using var native=new NativeTools(Client(reply),enableSpeedControl:true);
        var args=JsonSerializer.SerializeToElement(new{session=Session,runId=Other,duration=6,unit="hours",speed=1,expectedSpeed=0,maxRealSeconds=30});
        var result=await native.Invoke("run_simulation_for",args,TestContext.Current.CancellationToken);
        Assert.Equal("error",result["status"]!.GetValue<string>());
        Assert.DoesNotContain("POST simulation-run-for",reply.Calls);
        var q=new NameValueCollection{{"session",Session},{"runId",Other}};
        Assert.False(OperatingPreflight.NeedsSimulation(SimulationRunRequest.Parse("/agent-api/v1/simulation-run-cancel",q)));
    }

    [Fact]
    public async Task ShortSimulationDoesNotReadPreflightAndDoesNotLeakPreviousReports()
    {
        using var reply=new Reply(); using var native=new NativeTools(Client(reply),enableSpeedControl:true);
        var first=await native.Invoke("run_simulation_for",JsonSerializer.SerializeToElement(new{session=Session,runId=Producer,duration=6,unit="hours",speed=1,expectedSpeed=0,maxRealSeconds=30}),TestContext.Current.CancellationToken);
        Assert.True(first["status"]!.GetValue<string>()=="ok",first.ToJsonString());
        Assert.NotNull(first["operatingPreflight"]);
        reply.Calls.Clear();
        var args=JsonSerializer.SerializeToElement(new{session=Session,runId=Other,duration=1,unit="hours",speed=1,expectedSpeed=0,maxRealSeconds=30});
        var result=await native.Invoke("run_simulation_for",args,TestContext.Current.CancellationToken);
        Assert.True(result["status"]!.GetValue<string>()=="ok",result.ToJsonString());
        Assert.Null(result["operatingPreflight"]);
        Assert.Equal(new[]{"POST simulation-run-for"},reply.Calls);
    }

    [Theory]
    [InlineData("start")]
    [InlineData("run")]
    [InlineData("run-repair")]
    public async Task EveryStructureStartInvokesObserverBeforeDispatch(string operation)
    {
        using var reply=new Reply(); using var client=Client(reply);
        var q=new NameValueCollection{{"session",Session},{"districtId",Center},{"actionId",Other},
            {"planKey",new string('a',64)},{"mode","development_pilot"},
            {"steps",LargeProjectRequest.Encode([new(){Template="Workshop",X=1,Y=1,Z=1,Rotation=0,DependsOn=[]}])}};
        if(operation!="start") {q["speed"]="1";q["maxGameHours"]="6";q["maxRealSeconds"]="30";}
        if(operation=="run-repair")q["parentActionId"]=Producer;
        bool called=false;
        client.BeforeOperatingRun=(simulation,project,ct)=>{
            called=true;Assert.Null(simulation);Assert.Equal(operation,project!.Operation);
            Assert.Equal(Center,project.DistrictId);Assert.Single(project.Steps);
            throw new OperationCanceledException(ct);
        };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>client.LargeProject(
            LargeProjectRequest.Parse("/agent-api/v1/large-project-"+operation,q),TestContext.Current.CancellationToken));
        Assert.True(called);Assert.Empty(reply.Calls);
    }

    [Fact]
    public async Task InvalidAutomaticArgumentsDoNotReadAnything()
    {
        using var reply=new Reply(); using var native=new NativeTools(Client(reply));
        var result=await native.Invoke(AutoOperatingTools.Name,JsonSerializer.SerializeToElement(new{session=Session,extra=true}),TestContext.Current.CancellationToken);
        Assert.Equal("error",result["status"]!.GetValue<string>());
        Assert.Empty(reply.Calls);
        Assert.True(Assert.Single(NativeTools.Catalog(),t=>t.Name==AutoOperatingTools.Name).Annotations!.ReadOnlyHint);
    }

    [Theory]
    [InlineData(false,false,false,"observed")]
    [InlineData(true,false,false,"unavailable")]
    [InlineData(false,true,false,"budget_exhausted")]
    [InlineData(false,false,true,"district_unknown")]
    public async Task PlankChainIsAdvisoryAndRetainsBaseDiagnosisAndExactStartBudget(bool fail,bool expire,bool ambiguous,string state)
    {
        using var reply=new Reply{Planks=true,FailChain=fail,ExpireChain=expire,TwoDistricts=ambiguous};
        using var native=new NativeTools(Client(reply),enableSpeedControl:true);
        var result=await native.Invoke("run_simulation_for",JsonSerializer.SerializeToElement(new {
            session=Session,runId=Other,duration=6,unit="hours",speed=1,expectedSpeed=0,maxRealSeconds=30,
            goodTargets=new[]{new {good="Plank",amount=100}} }),TestContext.Current.CancellationToken);
        Assert.Equal("ok",result["status"]!.GetValue<string>());
        var pre=result["operatingPreflight"]![0]!;
        Assert.Equal("observed",pre["state"]!.GetValue<string>());
        Assert.NotNull(pre["diagnosis"]);
        Assert.Equal(state,pre["plankChain"]!["state"]!.GetValue<string>());
        if(state=="observed") {
            var findings=pre["plankChain"]!["report"]!["findings"]!.AsArray();
            Assert.Contains(findings,f=>f!["code"]!.GetValue<string>()=="consumer_not_powered");
            Assert.All(findings,f=>Assert.False(string.IsNullOrWhiteSpace(f!["nextStep"]!.GetValue<string>())));
            Assert.False(pre["plankChain"]!["report"]!["productionProven"]!.GetValue<bool>());
        } else Assert.NotEmpty(pre["plankChain"]!["nextSteps"]!.AsArray());
        var compact=ResponsePresentation.Present("run_simulation_for",result,false);
        Assert.Equal(pre["plankChain"]!.ToJsonString(),compact["operatingPreflight"]![0]!["plankChain"]!.ToJsonString());
        Assert.Single(reply.Calls,c=>c.StartsWith("POST "));
        Assert.Equal("POST simulation-run-for",reply.Calls.Last());
        var sent=System.Web.HttpUtility.ParseQueryString(reply.Requests.Last().Query);
        Assert.Equal("6",sent["duration"]);Assert.Equal("30",sent["maxRealSeconds"]);
        Assert.Equal(Other,sent["runId"]);Assert.Equal("1",sent["speed"]);
        if(ambiguous)Assert.Single(reply.Calls,c=>c=="GET production-graph");
    }

    [Fact]
    public async Task ChainSessionConflictStillPreventsDispatch()
    {
        using var reply=new Reply{Planks=true,StaleChain=true};using var native=new NativeTools(Client(reply),enableSpeedControl:true);
        var result=await native.Invoke("run_simulation_for",JsonSerializer.SerializeToElement(new {
            session=Session,runId=Other,duration=6,unit="hours",speed=1,expectedSpeed=0,maxRealSeconds=30}),TestContext.Current.CancellationToken);
        Assert.Equal("error",result["status"]!.GetValue<string>());
        Assert.DoesNotContain(reply.Calls,c=>c.StartsWith("POST "));
    }

    [Theory]
    [InlineData("start")]
    [InlineData("run")]
    [InlineData("run-repair")]
    public async Task PlankConstructionCostsTriggerChainBeforeNativeDispatch(string operation)
    {
        using var reply=new Reply{Planks=true};using var client=Client(reply);
        var observer=new OperatingPreflight(client);using var scope=observer.Begin();
        var q=new NameValueCollection{{"session",Session},{"districtId",Center},{"actionId",Other},
            {"planKey",new string('a',64)},{"mode","development_pilot"},
            {"steps",LargeProjectRequest.Encode([new(){Template="Workshop",X=1,Y=1,Z=1,Rotation=0,DependsOn=[]}])}};
        if(operation!="start"){q["speed"]="1";q["maxGameHours"]="6";q["maxRealSeconds"]="30";}
        if(operation=="run-repair")q["parentActionId"]=Producer;
        // The synthetic server rejects only the final write; preflight must have completed first.
        await Assert.ThrowsAsync<HttpRequestException>(()=>client.LargeProject(
            LargeProjectRequest.Parse("/agent-api/v1/large-project-"+operation,q),TestContext.Current.CancellationToken));
        var report=Assert.Single(scope.Reports);
        Assert.Equal("observed",report["plankChain"]!["state"]!.GetValue<string>());
        Assert.Equal(5,report["demands"]![0]!["amount"]!.GetValue<int>());
        Assert.Equal("full_planned_costs_including_helpers",report["demandBasis"]!.GetValue<string>());
        Assert.Equal("POST large-project-"+operation,reply.Calls.Last());
        Assert.Single(reply.Calls,c=>c.StartsWith("POST "));
        var sent=System.Web.HttpUtility.ParseQueryString(reply.Requests.Last().Query);
        Assert.Equal(q["steps"],sent["steps"]);
        if(operation!="start") {Assert.Equal("6",sent["maxGameHours"]);Assert.Equal("30",sent["maxRealSeconds"]);}
    }

    [Fact]
    public async Task OtherGoodsDoNotTriggerChainReads()
    {
        using var reply=new Reply();using var native=new NativeTools(Client(reply),enableSpeedControl:true);
        var result=await native.Invoke("run_simulation_for",JsonSerializer.SerializeToElement(new {
            session=Session,runId=Other,duration=6,unit="hours",speed=1,expectedSpeed=0,maxRealSeconds=30,
            goodTargets=new[]{new {good="B",amount=100}} }),TestContext.Current.CancellationToken);
        Assert.Null(result["operatingPreflight"]![0]!["plankChain"]);
        Assert.Single(reply.Calls,c=>c=="GET production-graph");
    }

    [Fact]
    public async Task CallerCancellationDuringChainDoesNotStart()
    {
        using var cancel=new CancellationTokenSource();using var reply=new Reply{Planks=true,CancelCaller=cancel};
        using var client=Client(reply);var observer=new OperatingPreflight(client);using var scope=observer.Begin();
        var q=new NameValueCollection{{"session",Session},{"runId",Other},{"duration","6"},{"unit","hours"},
            {"speed","1"},{"expectedSpeed","0"},{"maxRealSeconds","30"}};
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>client.SimulationRun(
            SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for",q),q,cancel.Token));
        Assert.DoesNotContain(reply.Calls,c=>c.StartsWith("POST "));
    }

    [Fact]
    public async Task UntilUsesChainAndKeepsExactDeadline()
    {
        using var reply=new Reply{Planks=true};using var client=Client(reply);
        var observer=new OperatingPreflight(client);using var scope=observer.Begin();
        var q=new NameValueCollection{{"session",Session},{"runId",Other},{"dayNumber","2"},{"hour","3"},
            {"speed","1"},{"expectedSpeed","0"},{"maxRealSeconds","30"}};
        await Assert.ThrowsAsync<HttpRequestException>(()=>client.SimulationRun(
            SimulationRunRequest.Parse("/agent-api/v1/simulation-run-until",q),q,TestContext.Current.CancellationToken));
        Assert.Equal("observed",Assert.Single(scope.Reports)["plankChain"]!["state"]!.GetValue<string>());
        Assert.Equal("POST simulation-run-until",reply.Calls.Last());
        var sent=System.Web.HttpUtility.ParseQueryString(reply.Requests.Last().Query);
        Assert.Equal("2",sent["dayNumber"]);Assert.Equal("3",sent["hour"]);Assert.Equal("30",sent["maxRealSeconds"]);
    }

    [Fact]
    public async Task FocusRecipeTriggersChainWithoutExplicitStockTarget()
    {
        using var reply=new Reply{Planks=true};using var client=Client(reply);
        var observer=new OperatingPreflight(client);using var scope=observer.Begin();
        var q=new NameValueCollection{{"session",Session},{"runId",Other},{"duration","6"},{"unit","hours"},
            {"speed","1"},{"expectedSpeed","0"},{"maxRealSeconds","30"},{"buildingIds",Producer}};
        await client.BeforeOperatingRun!(SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for",q),null,TestContext.Current.CancellationToken);
        var report=Assert.Single(scope.Reports);
        Assert.Equal("observed",report["plankChain"]!["state"]!.GetValue<string>());
        Assert.Equal("Plank",report["demands"]![0]!["good"]!.GetValue<string>());
        Assert.Equal(0,report["demands"]![0]!["amount"]!.GetValue<int>());
        Assert.All(reply.Calls,c=>Assert.StartsWith("GET ",c));
    }
}
