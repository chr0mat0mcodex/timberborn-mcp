using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class GenericBuildingTests
{
    [Theory]
    [InlineData("Single", "Square", false, false, 12, true)]
    [InlineData("Rectangle", "Square", false, false, 1, true)]
    [InlineData("TwoSegmentLine", "Square", false, false, 1, true)]
    [InlineData("SideLine", "Square", false, false, 2, true)]
    [InlineData("Half", "Square", false, false, 12, false)]
    [InlineData("Single", "Hex", false, false, 12, false)]
    [InlineData("Single", "Square", true, false, 12, false)]
    [InlineData("Single", "Square", false, true, 12, false)]
    [InlineData("Single", "Square", false, false, 65, false)]
    public void SpecialFormsRequireExplicitSupport(string layout,string shape,bool side,bool dev,int cells,bool allowed) =>
        Assert.Equal(allowed,BuildingPolicy.UnsupportedReasons(layout,shape,side,dev,cells).Length==0);

    [Theory]
    [InlineData("GET",true,false,false)]
    [InlineData("POST",false,false,false)]
    [InlineData("POST",true,true,false)]
    [InlineData("POST",true,false,true)]
    public void OwnOptInCannotBeReplacedByOtherGates(string method,bool enabled,bool body,bool allowed)
    {
        foreach(var route in new[]{"building-validation","building-placement"})
            Assert.Equal(allowed,BridgeHttpServer.MethodAllowed(method,"/agent-api/v1/"+route,body,true,true,true,true,true,true,true,true,enabled));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DistinctActionsWorkButDuplicatesNeverReexecuteEvenAfterFailure(bool fail)
    {
        var gate=new BuildingActionGate(); var id=Guid.NewGuid(); int calls=0;
        int Action(){calls++;if(fail)throw new IOException();return calls;}
        if(fail)Assert.Throws<IOException>(()=>gate.Execute(id,Action)); else Assert.Equal(1,gate.Execute(id,Action));
        Assert.Throws<InvalidOperationException>(()=>gate.Execute(id,Action));
        Assert.Equal(1,calls);
        Assert.Equal(2,gate.Execute(Guid.NewGuid(),()=>++calls));
    }

    [Fact]
    public void BudgetDoesNotEvictOldActionIds()
    {
        var gate=new BuildingActionGate();int calls=0;
        for(int i=0;i<256;i++)gate.Execute(Guid.NewGuid(),()=>++calls);
        Assert.Throws<InvalidOperationException>(()=>gate.Execute(Guid.NewGuid(),()=>++calls));Assert.Equal(256,calls);
    }

    [Theory]
    [InlineData("template","../WaterPump")]
    [InlineData("template","WaterPump&x=1")]
    [InlineData("actionId","bad")]
    [InlineData("session","00000000-0000-0000-0000-000000000000")]
    [InlineData("rotation","4")]
    [InlineData("x","-1")]
    [InlineData("extra","1")]
    public void InvalidRequestsRejected(string key,string value)
    {
        var q=Query();q[key]=value;
        Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse("/agent-api/v1/building-placement",q));
    }

    [Fact]
    public void DuplicateTemplateParameterRejected()
    {
        var q=Query();q.Add("template","Path");Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse("/agent-api/v1/building-placement",q));
    }

    [Theory]
    [InlineData("ok")]
    [InlineData("id")]
    [InlineData("session")]
    [InlineData("position")]
    [InlineData("transport")]
    [InlineData("old_version")]
    public async Task ReceiptCorrelationAndNoRetry(string variant)
    {
        var q=Query();var session=q["session"]!;var id=Guid.Parse(q["actionId"]!);
        var envelope=new BridgeEnvelope<NativePlacement>(1,variant=="session"?Guid.NewGuid().ToString("D"):session,DateTimeOffset.UtcNow,variant=="old_version"?"0.13.2":"0.14.1",
            new("WaterPump.Folktails",new(variant=="position"?2:1,2,3),0,variant=="id"?Guid.NewGuid():id,"applied",false,false,["synthetic_test"]));
        var handler=new Handler(variant=="transport"?null:JsonSerializer.Serialize(envelope,NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingPlacement:true);
        var result=await tools.Invoke("place_building",Args(q),TestContext.Current.CancellationToken);
        Assert.Equal(variant=="ok"?"ok":"error",result["status"]!.GetValue<string>());
        if(variant!="ok")Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1,handler.Calls);Assert.Contains("actionId="+id,handler.Url!);
    }

    [Fact]
    public async Task DisabledActionAndWrongJsonTypesNeverReachTransport()
    {
        var handler=new Handler("{}");
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler));
        var result=await tools.Invoke("place_building",Args(Query()),TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument",result["error"]!["code"]!.GetValue<string>());
        result=await tools.Invoke("precheck_building",JsonSerializer.SerializeToElement(new{template="Path",x="1",y=2,z=3,rotation=0}),TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument",result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0,handler.Calls);
        Assert.DoesNotContain(NativeTools.Catalog(),t=>t.Name=="place_building");
        Assert.Contains(NativeTools.Catalog(),t=>t.Name=="inspect_build_options");
    }

    [Theory]
    [InlineData("ok")]
    [InlineData("duplicate")]
    [InlineData("unsupported")]
    [InlineData("pagination")]
    [InlineData("cost")]
    public async Task CatalogRejectsInconsistentPages(string variant)
    {
        var entry=new BuildingOption("WaterPump.Folktails",true,true,true,variant=="unsupported"?["special_layout"]:[],"Single","Square","Water",new(1,2,3),null,false,[new("Log",variant=="cost"?-1:12,20)]);
        var items=variant=="duplicate"?new[]{entry,entry}:new[]{entry};
        var data=new NativeBuildingCatalog("Folktails",0,32,items.Length,items,variant=="pagination",["synthetic_test"]);
        var json=JsonSerializer.Serialize(new BridgeEnvelope<NativeBuildingCatalog>(1,Guid.NewGuid().ToString("D"),DateTimeOffset.UtcNow,"0.14.1",data),NativeJson.Options);
        using var client=new NativeClient(new(8081,new string('a',64)),new CatalogHandler(json));
        var r=BridgeRequest.Parse("/agent-api/v1/building-catalog",new(){["offset"]="0",["limit"]="32"});
        if(variant=="ok")Assert.Single((await client.BuildingCatalog(r,TestContext.Current.CancellationToken)).Data.Items);
        else await Assert.ThrowsAsync<InvalidDataException>(()=>client.BuildingCatalog(r,TestContext.Current.CancellationToken));
    }
    private sealed class CatalogHandler(string json):HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct) {
            Assert.Equal(HttpMethod.Get,r.Method);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});
        }
    }
    private static NameValueCollection Query()=>new(){["template"]="WaterPump.Folktails",["x"]="1",["y"]="2",["z"]="3",["rotation"]="0",["session"]=Guid.NewGuid().ToString("D"),["actionId"]=Guid.NewGuid().ToString("D")};
    private static JsonElement Args(NameValueCollection q)=>JsonSerializer.SerializeToElement(new{template=q["template"],x=1,y=2,z=3,rotation=0,session=q["session"],actionId=q["actionId"]});
    private sealed class Handler(string? json):HttpMessageHandler
    {
        public int Calls;public string? Url;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct)
        {
            Calls++;Url=r.RequestUri!.ToString();Assert.Equal(HttpMethod.Post,r.Method);Assert.Null(r.Content);
            if(json is null)throw new HttpRequestException();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});
        }
    }
}
