using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class EconomyTests
{
    private static readonly string Session=Guid.NewGuid().ToString("D");
    private static readonly string AlertId=EconomyRequest.GroupId(Session,"No water","Thirsty",true,true,false);
    private static NameValueCollection Query(bool targets=false)=>targets ? new(){{"offset","0"},{"limit","32"},{"session",Session},{"alertId",AlertId}} : new(){{"offset","0"},{"limit","32"}};
    [Theory]
    [InlineData("goods")][InlineData("alerts")][InlineData("alert-targets")]
    public void StrictBoundedReadRoutes(string route)
    {
        var q=Query(route=="alert-targets");string path="/agent-api/v1/"+route;
        Assert.Equal(route,BridgeRequest.Parse(path,q).Economy!.Route);
        Assert.True(BridgeHttpServer.MethodAllowed("GET",path,false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST",path,false,true,true,true));
        q.Add("limit","32");Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,q));
        q.Set("limit","0");Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,q));
        q.Set("limit","33");Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,q));
        q.Set("limit","32");q.Set("offset","65536");Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,q));
        q.Set("offset","0");q.Add("extra","1");Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,q));
    }
    [Fact]
    public void OpaqueGroupingPreservesFullTextAndSessionAndFlags()
    {
        string Key(string s,string a,bool show=true)=>EconomyRequest.GroupId(Session,s,a,show,false,false);
        Assert.Equal(Key("a","bc"),Key("a","bc"));Assert.NotEqual(Key("a","bc"),Key("ab","c"));
        Assert.NotEqual(Key("a","bc"),Key("a","bc",false));
        Assert.NotEqual(Key(new string('a',600),""),Key(new string('a',600)+"b",""));
        Assert.NotEqual(AlertId,EconomyRequest.GroupId(Guid.NewGuid().ToString("D"),"No water","Thirsty",true,true,false));
        Assert.True(EconomyRequest.ValidAlertId(AlertId));
        var q=Query(true);q.Set("session",Guid.Empty.ToString());Assert.Throws<ArgumentException>(()=>EconomyRequest.Parse("/agent-api/v1/alert-targets",q));
        q.Set("session",Session);q.Set("alertId","unsafe/target");Assert.Throws<ArgumentException>(()=>EconomyRequest.Parse("/agent-api/v1/alert-targets",q));
    }
    private static NativeGoods Goods()=>new("global_registered_goods",0,32,1,[new("Carrot","Carrots","Food","Food",0,0,0,0,0,0,0,0,0,0)],false,[]);
    private static NativeAlerts Alerts()=>new("visible_active_entity_statuses",0,32,1,[new(AlertId,"No water","Thirsty",true,true,false,2,2)],false,[]);
    private static NativeAlertTargets Targets()=>new(AlertId,true,0,32,1,[new(Guid.NewGuid(),"SyntheticBeaver","entity",null,new(1.5f,3,2.5f))],false,[]);
    private static JsonObject Envelope(object data)=>JsonSerializer.SerializeToNode(new BridgeEnvelope<object>(1,Session,DateTimeOffset.UtcNow,"0.18.0",data),NativeJson.Options)!.AsObject();
    private static async Task<JsonObject> Call(string name,JsonObject response,object args)
    {
        using var handler=new Reply(response.ToJsonString());
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler));
        var result=await tools.Invoke(name,JsonSerializer.SerializeToElement(args),TestContext.Current.CancellationToken);
        Assert.Equal(1,handler.Calls);Assert.Equal(HttpMethod.Get,handler.Method);
        return result;
    }
    [Theory]
    [InlineData("inspect_goods")][InlineData("inspect_alerts")][InlineData("inspect_alert_targets")]
    public async Task ReadToolsExposeStructuredDataAndDoNotRequireActionGates(string name)
    {
        var tool=Assert.Single(NativeTools.Catalog(),t=>t.Name==name);
        Assert.True(tool.Annotations!.ReadOnlyHint);Assert.False(tool.Annotations.DestructiveHint);
        Assert.True(tool.InputSchema.GetProperty("properties").TryGetProperty("reasoning",out _));
        object data=name=="inspect_goods"?Goods():name=="inspect_alerts"?Alerts():Targets();
        object args=name=="inspect_alert_targets"?new{offset=0,limit=32,session=Session,alertId=AlertId}:(object)new{offset=0,limit=32};
        var result=await Call(name,Envelope(data),args);
        Assert.Equal("ok",result["status"]!.GetValue<string>());
        Assert.Equal(Session,result["meta"]!["sessionId"]!.GetValue<string>());
    }
    [Theory]
    [InlineData("total")][InlineData("offset")][InlineData("hasMore")][InlineData("negative")][InlineData("missing")][InlineData("duplicate")][InlineData("version")]
    public async Task RejectsIncompleteOrContradictoryGoods(string defect)
    {
        var e=Envelope(Goods());var d=e["data"]!.AsObject();
        switch(defect) {
            case "total":d["total"]=2;break;case "offset":d["offset"]=1;break;case "hasMore":d["hasMore"]=true;break;
            case "negative":d["items"]![0]!["stockpiledStock"]=-1;break;
            case "missing":d["items"]![0]!.AsObject().Remove("bufferedInput");break;
            case "duplicate":d["items"]!.AsArray().Add(d["items"]![0]!.DeepClone());d["total"]=2;break;
            case "version":e["bridgeVersion"]="0.17.2";break;
        }
        Assert.Equal("backend_incompatible",(await Call("inspect_goods",e,new{offset=0,limit=32}))["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public async Task LastPageMayBeEmptyAndZeroStockIsNotFiltered()
    {
        var d=Goods() with {Offset=32,Items=[]};
        Assert.Equal("ok",(await Call("inspect_goods",Envelope(d),new{offset=32,limit=32}))["status"]!.GetValue<string>());
        var result=await Call("inspect_goods",Envelope(Goods()),new{offset=0,limit=32});
        Assert.Equal(0,result["data"]!["items"]![0]!["allStock"]!.GetValue<int>());
    }
    [Theory]
    [InlineData("session")][InlineData("alertId")][InlineData("position")][InlineData("found")]
    public async Task RejectsMismatchedAlertTargetObservation(string defect)
    {
        var e=Envelope(Targets());var d=e["data"]!;
        switch(defect) {
            case "session":e["sessionId"]=Guid.NewGuid().ToString();break;
            case "alertId":d["alertId"]=new string('0',64);break;
            case "position":d["items"]![0]!["kind"]="block_object";break;
            case "found":d["found"]=false;break;
        }
        Assert.Equal("backend_incompatible",(await Call("inspect_alert_targets",e,new{offset=0,limit=32,session=Session,alertId=AlertId}))["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public async Task DisappearedAlertIsExplicitEmptyObservation()
    {
        var d=Targets() with {Found=false,Total=0,Items=[]};
        var result=await Call("inspect_alert_targets",Envelope(d),new{offset=0,limit=32,session=Session,alertId=AlertId});
        Assert.Equal("ok",result["status"]!.GetValue<string>());Assert.False(result["data"]!["found"]!.GetValue<bool>());
    }
    [Fact]
    public async Task InvalidAffectedCountsAreNotAccepted()
    {
        var d=Alerts() with {Items=[Alerts().Items[0] with {AffectedCount=3}]};
        Assert.Equal("backend_incompatible",(await Call("inspect_alerts",Envelope(d),new{offset=0,limit=32}))["error"]!["code"]!.GetValue<string>());
    }
    private sealed class Reply(string body):HttpMessageHandler
    {
        public int Calls {get;private set;} public HttpMethod? Method {get;private set;}
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {Calls++;Method=request.Method;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(body)});}
    }
}
