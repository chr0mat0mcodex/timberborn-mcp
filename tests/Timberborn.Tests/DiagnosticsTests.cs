using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class DiagnosticsTests
{
    private static readonly string Session=Guid.NewGuid().ToString(),Id=Guid.NewGuid().ToString();
    private static JsonObject Envelope(object d)=>JsonSerializer.SerializeToNode(new BridgeEnvelope<object>(1,Session,DateTimeOffset.UtcNow,"0.20.1",d),NativeJson.Options)!.AsObject();
    private static async Task<JsonObject> Call(string name,object data,object args)
    {
        using var h=new Reply(data is JsonObject j?j.ToJsonString():Envelope(data).ToJsonString());using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h));
        var r=await tools.Invoke(name,JsonSerializer.SerializeToElement(args),TestContext.Current.CancellationToken);Assert.Equal(1,h.Calls);return r;
    }
    [Theory]
    [InlineData("needs")][InlineData("beaver-needs")][InlineData("building-operation")]
    public void RequestsAreStrictAndReadOnly(string route)
    {
        var q=new NameValueCollection();if(route!="needs"){q.Add("id",Id);q.Add("session",Session);}if(route!="building-operation"){q.Add("offset","0");q.Add("limit","32");}
        string path="/agent-api/v1/"+route;
        Assert.Equal(route,BridgeRequest.Parse(path,q).Diagnostics!.Route);
        Assert.True(BridgeHttpServer.MethodAllowed("GET",path,false,false));Assert.False(BridgeHttpServer.MethodAllowed("POST",path,false,true));
        q.Add(q.GetKey(0),q.Get(0));Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,q));
    }
    private static NativeNeeds Overview()=>new("living_beavers_with_need_manager",3,2,1,0,32,1,[new("Thirst","Thirst",2,2,2,1,1,1,0,1,0.5)],false,[],0,0);
    private static NativeBeaverNeeds Detail()=>new(Id,true,new(1,2,3),0,32,1,[new("Thirst","Thirst",0,0,1,true,true,true,true,true,false)],false,[],"alive");
    private static NativeOperation Operation()=>new(Id,"SyntheticWorkshop",true,false,new(0,2,3,true,false,false),new(true,"SyntheticRecipe",false,false,true,false,true,0),[],[]);
    [Fact]
    public async Task NeedCountsCanOverlapAndMissingManagersAreExplicit()
    {
        var r=await Call("inspect_needs",Overview(),new{offset=0,limit=32});Assert.Equal("ok",r["status"]!.GetValue<string>());Assert.Equal(1,r["data"]!["missingNeedManagers"]!.GetValue<int>());
        var n=Overview() with{Items=[new("Thirst","Thirst",2,0,0,0,0,0,null,null,null)]};
        Assert.Equal("ok",(await Call("inspect_needs",n,new{offset=0,limit=32}))["status"]!.GetValue<string>());
    }
    [Theory]
    [InlineData("dead_count")][InlineData("warning")][InlineData("missing")][InlineData("average")][InlineData("empty")][InlineData("page")][InlineData("version")]
    public async Task RejectsMisleadingNeedOverview(string defect)
    {
        var e=Envelope(Overview());var d=e["data"]!;var n=d["items"]![0]!;
        switch(defect){case "dead_count":d["deadExcluded"]=-1;break;case "warning":n["warning"]=3;break;case "missing":d["missingNeedManagers"]=0;break;case "average":n["averagePoints"]=2;break;case "empty":n["minimumPoints"]=null;break;case "page":d["hasMore"]=true;break;case "version":e["bridgeVersion"]="0.20.0";break;}
        Assert.Equal("backend_incompatible",(await Call("inspect_needs",e,new{offset=0,limit=32}))["error"]!["code"]!.GetValue<string>());
    }
    [Theory]
    [InlineData("session")][InlineData("target")][InlineData("bounds")][InlineData("supported")][InlineData("duplicate")]
    public async Task RejectsInvalidIndividualNeedReceipt(string defect)
    {
        var e=Envelope(Detail());var d=e["data"]!;
        switch(defect){case "session":e["sessionId"]=Guid.NewGuid().ToString();break;case "target":d["id"]=Guid.NewGuid().ToString();break;case "bounds":d["items"]![0]!["points"]=2;break;case "supported":d["supported"]=false;break;case "duplicate":d["items"]!.AsArray().Add(d["items"]![0]!.DeepClone());d["total"]=2;break;}
        Assert.Equal("backend_incompatible",(await Call("inspect_beaver_needs",e,new{id=Id,session=Session,offset=0,limit=32}))["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public async Task MissingComponentsRemainUnknownAndNightIsSeparate()
    {
        Assert.Equal("ok",(await Call("inspect_beaver_needs",Detail(),new{id=Id,session=Session,offset=0,limit=32}))["status"]!.GetValue<string>());
        var r=await Call("inspect_building_operation",Operation(),new{id=Id,session=Session});Assert.Equal("ok",r["status"]!.GetValue<string>());Assert.False(r["data"]!["workplace"]!["workingHours"]!.GetValue<bool>());
        r=await Call("inspect_building_operation",Operation() with{Finished=false,Workplace=null,Manufacturing=null,Paused=null},new{id=Id,session=Session});Assert.Equal("ok",r["status"]!.GetValue<string>());Assert.Null(r["data"]!["manufacturing"]);
    }
    [Theory]
    [InlineData("recipe")][InlineData("unknown")][InlineData("unfinished")][InlineData("staff")][InlineData("progress")]
    public async Task RejectsContradictoryOperations(string defect)
    {
        var e=Envelope(Operation());var d=e["data"]!;
        switch(defect){case "recipe":d["manufacturing"]!["recipe"]=null;break;case "unknown":d["manufacturing"]!["hasIngredients"]=null;break;case "unfinished":d["finished"]=false;break;case "staff":d["workplace"]!["anyJobRunning"]=true;break;case "progress":d["manufacturing"]!["productionProgress"]=-1;break;}
        Assert.Equal("backend_incompatible",(await Call("inspect_building_operation",e,new{id=Id,session=Session}))["error"]!["code"]!.GetValue<string>());
    }
    [Theory]
    [InlineData("dead")][InlineData("unknown")]
    public async Task NonLivingNeedsAreNotCurrentObservations(string life)
    {
        var valid=Detail() with{LifeState=life,Supported=false,Total=0,Items=[]};
        Assert.Equal("ok",(await Call("inspect_beaver_needs",valid,new{id=Id,session=Session,offset=0,limit=32}))["status"]!.GetValue<string>());
        var invalid=Detail() with{LifeState=life};
        Assert.Equal("backend_incompatible",(await Call("inspect_beaver_needs",invalid,new{id=Id,session=Session,offset=0,limit=32}))["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public async Task InventoryMayExceedCapacityWithoutBeingRejected()
    {
        var e=Envelope(Operation() with{Inventories=[new("Output",20,78,false,true,false,true,true,true,true,[new("Water",78,70,2,-60)])]});
        e["bridgeVersion"]="0.21.1";
        var result=await Call("inspect_building_operation",e,new{id=Id,session=Session});
        Assert.Equal("ok",result["status"]!.GetValue<string>());
        Assert.Equal(78,result["data"]!["inventories"]![0]!["totalStock"]!.GetValue<int>());
        Assert.Equal(-60,result["data"]!["inventories"]![0]!["goods"]![0]!["unreservedCapacity"]!.GetValue<int>());
    }
    [Fact]
    public async Task OlderBridgeLeavesInventoriesUnknownNotEmpty()
    {
        var e=Envelope(Operation());e["data"]!.AsObject().Remove("inventories");
        var r=await Call("inspect_building_operation",e,new{id=Id,session=Session});
        Assert.Equal("ok",r["status"]!.GetValue<string>());Assert.Null(r["data"]!["inventories"]);
    }
    [Theory]
    [InlineData("nullGoods")][InlineData("negative")][InlineData("unreserved")][InlineData("duplicate")]
    [InlineData("total")][InlineData("missing")][InlineData("unfinished")]
    public async Task RejectsInvalidInventoryObservations(string defect)
    {
        var e=Envelope(Operation() with{Inventories=[new("Output",30,10,false,true,false,true,false,false,true,[new("Water",10,8,2,18)])]});
        e["bridgeVersion"]="0.21.1";var d=e["data"]!;var i=d["inventories"]![0]!;
        switch(defect){
            case "nullGoods":i["goods"]=null;break;
            case "negative":i["goods"]![0]!["stock"]=-1;break;
            case "unreserved":i["goods"]![0]!["unreservedStock"]=11;break;
            case "duplicate":i["goods"]!.AsArray().Add(i["goods"]![0]!.DeepClone());i["totalStock"]=20;break;
            case "total":i["totalStock"]=11;break;
            case "missing":d.AsObject().Remove("inventories");break;
            case "unfinished":d["finished"]=false;d["workplace"]=null;d["manufacturing"]=null;break;
        }
        Assert.Equal("backend_incompatible",(await Call("inspect_building_operation",e,new{id=Id,session=Session}))["error"]!["code"]!.GetValue<string>());
    }
    private sealed class Reply(string body):HttpMessageHandler
    {
        public int Calls{get;private set;}
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct){Calls++;Assert.Equal(HttpMethod.Get,r.Method);return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(body)});}
    }
}
