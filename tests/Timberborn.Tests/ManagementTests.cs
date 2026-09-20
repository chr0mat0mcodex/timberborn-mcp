using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;
public sealed class ManagementTests
{
    [Theory]
    [InlineData("VeryLow")][InlineData("Low")][InlineData("Normal")][InlineData("High")][InlineData("VeryHigh")]
    public void AllRegularPriorities(string priority)
    {
        var q = new NameValueCollection { {"id",Guid.NewGuid().ToString()}, {"session",Guid.NewGuid().ToString()}, {"kind","workplace"}, {"priority",priority}, {"expectedPriority","Normal"} };
        Assert.Equal(priority,ManagementRequest.Parse("/agent-api/v1/set-priority",q).Priority);
        q.Add("priority",priority);
        Assert.Throws<ArgumentException>(()=>ManagementRequest.Parse("/agent-api/v1/set-priority",q));
    }
    [Theory]
    [InlineData("width","5")][InlineData("height","0")][InlineData("kind","invalid")][InlineData("session","bad")][InlineData("operation","destroy")][InlineData("expectedResource","")][InlineData("resource","Pine")][InlineData("x","-1")]
    public void RejectsUnsafeAreaArguments(string key,string value)
    {
        var q=Area();q[key]=value;
        Assert.Throws<ArgumentException>(()=>ManagementRequest.Parse("/agent-api/v1/set-area",q));
    }
    [Fact]
    public void AreaBoundsAndExtraArguments()
    {
        var q=Area();q["width"]="4";q["height"]="4";
        Assert.Equal(4,ManagementRequest.Parse("/agent-api/v1/set-area",q).Width);
        q.Add("extra","x");Assert.Throws<ArgumentException>(()=>ManagementRequest.Parse("/agent-api/v1/set-area",q));
    }
    [Theory]
    [InlineData("set-priority",true,false,true)][InlineData("set-priority",false,true,false)]
    [InlineData("set-area",false,true,true)][InlineData("set-area",true,false,false)]
    public void IndependentHttpGates(string route,bool priorities,bool areas,bool allowed)
    {
        string path="/agent-api/v1/"+route;
        Assert.Equal(allowed,BridgeHttpServer.MethodAllowed("POST",path,false,true,true,true,true,true,priorities,areas));
        Assert.False(BridgeHttpServer.MethodAllowed("GET",path,false,true,true,true,true,true,priorities,areas));
        Assert.False(BridgeHttpServer.MethodAllowed("POST",path,true,true,true,true,true,true,priorities,areas));
    }
    [Theory]
    [InlineData("applied","High",false,true)][InlineData("applied","Normal",false,false)]
    [InlineData("unconfirmed","Normal",false,true)][InlineData("applied","High",true,false)]
    public async Task PriorityReceipt(string outcome,string observed,bool wrongSession,bool valid)
    {
        var id=Guid.NewGuid();var session=Guid.NewGuid().ToString();
        var data=new NativePriority(id,"workplace","Normal",observed,outcome,["synthetic_test"]);
        var h=new Handler(JsonSerializer.Serialize(new BridgeEnvelope<NativePriority>(1,wrongSession?Guid.NewGuid().ToString():session,DateTimeOffset.UtcNow,"0.12.0",data),NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h),enablePriorities:true);
        var result=await tools.Invoke("set_building_priority",JsonSerializer.SerializeToElement(new{id,session,kind="workplace",priority="High",expectedPriority="Normal"}),TestContext.Current.CancellationToken);
        Assert.Equal(valid?"ok":"error",result["status"]!.GetValue<string>());Assert.Equal(1,h.Calls);
        if(!valid)Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
    }
    [Theory]
    [InlineData(false)][InlineData(true)]
    public async Task DisabledMutationNeverReachesTransport(bool area)
    {
        var h=new Handler("{}");using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h));
        var result=await tools.Invoke(area?"set_area":"set_building_priority",JsonSerializer.SerializeToElement(new{}),TestContext.Current.CancellationToken);
        Assert.Equal("error",result["status"]!.GetValue<string>());Assert.Equal(0,h.Calls);
    }
    private static NameValueCollection Area()=>new(){{"kind","tree_cutting"},{"operation","mark"},{"resource",""},{"expectedResource","unmarked"},{"session",Guid.NewGuid().ToString()},{"x","1"},{"y","1"},{"z","0"},{"width","1"},{"height","1"}};
    private sealed class Handler(string json):HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct){Calls++;Assert.Equal(HttpMethod.Post,r.Method);Assert.Null(r.Content);return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});}
    }
}
