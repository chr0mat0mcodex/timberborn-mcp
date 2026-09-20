using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;
public sealed class RemovalTests
{
    [Theory]
    [InlineData("buildings","delete")][InlineData("debris","delete")][InlineData("planted","mark")][InlineData("planted","unmark")][InlineData("vegetation","mark")]
    public void ValidOperations(string kind,string op)
    {var q=Query();q["kind"]=kind;q["operation"]=op;Assert.Equal(op,RemovalRequest.Parse("/agent-api/v1/remove-object",q).Operation);}
    [Theory]
    [InlineData("kind","all")][InlineData("kind","other")][InlineData("operation","mark")][InlineData("expectedMarked","true")][InlineData("id","bad")][InlineData("session","bad")][InlineData("x","-1")][InlineData("template","")]
    public void RejectsUnsafeDelete(string key,string value)
    {var q=Query();q[key]=value;Assert.Throws<ArgumentException>(()=>RemovalRequest.Parse("/agent-api/v1/remove-object",q));}
    [Fact]
    public void RejectsDuplicateAndUnknownParameter()
    {var q=Query();q.Add("id",Guid.NewGuid().ToString());Assert.Throws<ArgumentException>(()=>RemovalRequest.Parse("/agent-api/v1/remove-object",q));q=Query();q.Add("force","true");Assert.Throws<ArgumentException>(()=>RemovalRequest.Parse("/agent-api/v1/remove-object",q));}
    [Theory]
    [InlineData("GET",true,false,false)][InlineData("POST",false,false,false)][InlineData("POST",true,true,false)][InlineData("POST",true,false,true)]
    public void SeparateRemovalGate(string method,bool enabled,bool body,bool expected)
    {Assert.Equal(expected,BridgeHttpServer.MethodAllowed(method,"/agent-api/v1/remove-object",body,true,true,true,true,true,true,true,enabled));}
    [Theory]
    [InlineData("applied",true,false,true)][InlineData("applied",false,false,false)][InlineData("unconfirmed",false,false,true)][InlineData("applied",true,true,false)]
    public async Task DeleteReceiptMustCorrelate(string outcome,bool removed,bool wrongSession,bool valid)
    {
        var id=Guid.NewGuid();var session=Guid.NewGuid().ToString();
        var data=new NativeRemoval(id,"buildings","Synthetic",new(1,2,3),"delete",outcome,removed,removed?null:false,["synthetic_test"]);
        var h=new Handler(JsonSerializer.Serialize(new BridgeEnvelope<NativeRemoval>(1,wrongSession?Guid.NewGuid().ToString():session,DateTimeOffset.UtcNow,"0.13.0",data),NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h),enableRemoval:true);
        var result=await tools.Invoke("demolish_building",JsonSerializer.SerializeToElement(new{id,session,template="Synthetic",x=1,y=2,z=3,operation="delete",expectedMarked=false}),TestContext.Current.CancellationToken);
        Assert.Equal(valid?"ok":"error",result["status"]!.GetValue<string>());Assert.Equal(1,h.Calls);
        if(!valid)Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
    }
    [Theory]
    [InlineData("demolish_building")][InlineData("remove_planted")][InlineData("remove_vegetation")][InlineData("remove_debris")]
    public async Task DefaultGatePreventsTransport(string name)
    {
        var h=new Handler("{}");using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h));
        Assert.DoesNotContain(NativeTools.Catalog(),t=>t.Name==name);
        Assert.Equal("error",(await tools.Invoke(name,JsonSerializer.SerializeToElement(new{}),TestContext.Current.CancellationToken))["status"]!.GetValue<string>());Assert.Equal(0,h.Calls);
    }
    [Theory]
    [InlineData("mark","",true)][InlineData("remove","",false)][InlineData("mark","Pine",false)]
    public void TappingOnlyProtectsFromCutting(string operation,string resource,bool valid)
    {
        var q=new NameValueCollection{{"kind","tapping"},{"operation",operation},{"resource",resource},{"expectedResource","marked"},{"session",Guid.NewGuid().ToString()},{"x","1"},{"y","2"},{"z","3"},{"width","2"},{"height","2"}};
        if(valid)Assert.Equal("tapping",ManagementRequest.Parse("/agent-api/v1/set-area",q).Kind);
        else Assert.Throws<ArgumentException>(()=>ManagementRequest.Parse("/agent-api/v1/set-area",q));
    }
    [Theory]
    [InlineData("unmarked",true)][InlineData("marked",false)]
    public async Task TappingReceiptMustConfirmCuttingDisabled(string state,bool valid)
    {
        var session=Guid.NewGuid().ToString();
        var data=new NativeAreaChange("tapping","mark","applied",[new(new(1,2,3),state)],["synthetic_test"]);
        var h=new Handler(JsonSerializer.Serialize(new BridgeEnvelope<NativeAreaChange>(1,session,DateTimeOffset.UtcNow,"0.13.0",data),NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h),enableAreas:true);
        var result=await tools.Invoke("set_area",JsonSerializer.SerializeToElement(new{kind="tapping",operation="mark",resource="",expectedResource="marked",session,x=1,y=2,z=3,width=1,height=1}),TestContext.Current.CancellationToken);
        Assert.Equal(valid?"ok":"error",result["status"]!.GetValue<string>());Assert.Equal(1,h.Calls);
    }
    private static NameValueCollection Query()=>new(){{"kind","buildings"},{"operation","delete"},{"template","Synthetic"},{"expectedMarked","false"},{"session",Guid.NewGuid().ToString()},{"id",Guid.NewGuid().ToString()},{"x","1"},{"y","2"},{"z","3"}};
    private sealed class Handler(string json):HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct){Calls++;Assert.Equal(HttpMethod.Post,r.Method);Assert.Null(r.Content);return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});}
    }
}
