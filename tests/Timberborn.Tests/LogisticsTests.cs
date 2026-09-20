using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class LogisticsTests
{
    private static readonly string Session=Guid.NewGuid().ToString(),Id=Guid.NewGuid().ToString(),To=Guid.NewGuid().ToString();
    [Theory]
    [InlineData("building-access")][InlineData("road-connection")][InlineData("work-range")][InlineData("good-history")]
    public void StrictReadRequests(string route)
    {
        var q=new NameValueCollection();
        if(route=="good-history")q.Add("good","Water");else{q.Add("id",Id);q.Add("session",Session);}
        if(route=="road-connection")q.Add("toId",To);
        if(route is "work-range" or "good-history"){q.Add("offset","0");q.Add("limit","32");}
        string path="/agent-api/v1/"+route;
        Assert.Equal(route,BridgeRequest.Parse(path,q).Logistics!.Route);
        Assert.True(BridgeHttpServer.MethodAllowed("GET",path,false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST",path,false,true));
        q.Add(q.GetKey(0),q.Get(0));Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,q));
    }
    private static JsonObject Envelope(object d)=>JsonSerializer.SerializeToNode(new BridgeEnvelope<object>(1,Session,DateTimeOffset.UtcNow,"0.19.0",d),NativeJson.Options)!.AsObject();
    private static async Task<JsonObject> Call(string name,object data,object args)
    {
        using var h=new Reply(data is JsonObject j?j.ToJsonString():Envelope(data).ToJsonString());using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h));
        var r=await tools.Invoke(name,JsonSerializer.SerializeToElement(args),TestContext.Current.CancellationToken);Assert.Equal(1,h.Calls);return r;
    }
    private static NativeRoad Road()=>new(Id,To,true,1,1,true,15,[]);
    [Theory]
    [InlineData(true,true,15)][InlineData(true,false,null)][InlineData(false,null,null)]
    public async Task UnknownAndDisconnectedAreDifferent(bool supported,bool? connected,int? distance)
    {
        var d=Road() with{Supported=supported,Connected=connected,Distance=distance};
        var r=await Call("inspect_road_connection",d,new{id=Id,toId=To,session=Session});Assert.Equal("ok",r["status"]!.GetValue<string>());
        Assert.Equal(connected,r["data"]!["connected"]?.GetValue<bool>());
    }
    [Theory]
    [InlineData("distance")][InlineData("supported")][InlineData("access")][InlineData("target")][InlineData("session")]
    public async Task RejectsContradictoryRoadReceipts(string defect)
    {
        var e=Envelope(Road());var d=e["data"]!;
        switch(defect){case "distance":d["distance"]=-1;break;case "supported":d["supported"]=false;break;case "access":d["sourceAccessCount"]=2;break;case "target":d["toId"]=Id;break;case "session":e["sessionId"]=Guid.NewGuid().ToString();break;}
        Assert.Equal("backend_incompatible",(await Call("inspect_road_connection",e,new{id=Id,toId=To,session=Session}))["error"]!["code"]!.GetValue<string>());
    }
    private static NativeGoodHistory History()=>new("Water",true,0,32,2,[new(0,1,1,100,200,30,10,20),new(1,1,2,105,200,20,15,5)],false,50,25,5,[]);
    [Fact]
    public async Task NativeFlowsAndStockDeltaStayDistinct()
    {
        var r=await Call("inspect_good_history",History(),new{good="Water",offset=0,limit=32});
        Assert.Equal("ok",r["status"]!.GetValue<string>());Assert.Equal(5,r["data"]!["stockChange"]!.GetValue<long>());Assert.Equal(50,r["data"]!["pageProduction"]!.GetValue<long>());
    }
    [Theory]
    [InlineData("sum")][InlineData("delta")][InlineData("net")][InlineData("index")][InlineData("page")][InlineData("negative")]
    public async Task RejectsMisleadingHistory(string defect)
    {
        var e=Envelope(History());var d=e["data"]!;
        switch(defect){case "sum":d["pageProduction"]=25;break;case "delta":d["stockChange"]=25;break;case "net":d["items"]![0]!["netProduction"]=30;break;case "index":d["items"]![0]!["index"]=1;break;case "page":d["hasMore"]=true;break;case "negative":d["items"]![0]!["consumption"]=-1;break;}
        Assert.Equal("backend_incompatible",(await Call("inspect_good_history",e,new{good="Water",offset=0,limit=32}))["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public async Task EmptyHistoryAndMissingComponentsRemainUnknown()
    {
        var h=History() with{Total=0,Items=[],PageProduction=0,PageConsumption=0,StockChange=null};
        Assert.Equal("ok",(await Call("inspect_good_history",h,new{good="Water",offset=0,limit=32}))["status"]!.GetValue<string>());
        var a=new NativeAccess(Id,true,new(1,2,3),null,null,null,null,null,0,0,[]);
        var r=await Call("inspect_building_access",a,new{id=Id,session=Session});Assert.Equal("ok",r["status"]!.GetValue<string>());Assert.Null(r["data"]!["entranceBlocked"]);
    }
    [Fact]
    public async Task RangePagesMustBeSortedUniqueAndBounded()
    {
        var d=new NativeRange(Id,true,["work"],0,32,2,[new(1,2,3),new(2,2,3)],false,[]);
        Assert.Equal("ok",(await Call("inspect_work_range",d,new{id=Id,session=Session,offset=0,limit=32}))["status"]!.GetValue<string>());
        foreach(var bad in new[]{d with{Items=[new(1,2,3),new(1,2,3)]},d with{Items=d.Items.Reverse().ToArray()},d with{Supported=false}})
            Assert.Equal("backend_incompatible",(await Call("inspect_work_range",bad,new{id=Id,session=Session,offset=0,limit=32}))["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public void CatalogRequiresNoActionGates()
    {
        foreach(var t in LogisticsTools.Catalog()){Assert.True(t.Annotations!.ReadOnlyHint);Assert.False(t.Annotations.DestructiveHint);Assert.Contains(NativeTools.Catalog(),i=>i.Name==t.Name);}
    }
    private sealed class Reply(string body):HttpMessageHandler
    {
        public int Calls{get;private set;}
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct){Calls++;Assert.Equal(HttpMethod.Get,r.Method);return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(body)});}
    }
}
