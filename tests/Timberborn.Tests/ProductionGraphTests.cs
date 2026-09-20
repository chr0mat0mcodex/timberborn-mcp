using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Timberborn.TestFixtures;
using Xunit;
namespace Timberborn.Tests;

public sealed class ProductionGraphTests
{
    private static JsonObject Envelope()=>JsonSerializer.SerializeToNode(new BridgeEnvelope<object>(1,Guid.NewGuid().ToString(),DateTimeOffset.UtcNow,"0.21.0",ProductionGraphFixture.Create()),NativeJson.Options)!.AsObject();
    [Fact]
    public void RouteIsReadOnlyAndRejectsUnexpectedParameters()
    {
        const string path="/agent-api/v1/production-graph";
        Assert.Equal("production-graph",BridgeRequest.Parse(path,new()).Route);
        Assert.True(BridgeHttpServer.MethodAllowed("GET",path,false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST",path,false,true));
        Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse(path,new NameValueCollection{{"offset","0"}}));
    }
    [Fact]
    public async Task SingleReadPreservesAlternativesCyclesAndExplicitGaps()
    {
        using var h=new Reply(Envelope());using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h));
        var result=await tools.Invoke("inspect_production_graph",JsonSerializer.SerializeToElement(new{}),TestContext.Current.CancellationToken);
        Assert.Equal("ok",result["status"]!.GetValue<string>());Assert.Equal(1,h.Calls);
        Assert.Equal(3,result["data"]!["definitions"]!["recipes"]!.AsArray().Count);
        Assert.Equal("Unknown",result["data"]!["coverage"]!["goodsWithoutKnownSource"]![0]!.GetValue<string>());
    }
    [Theory]
    [InlineData("version")][InlineData("revision")][InlineData("duplicate")][InlineData("missingGood")]
    [InlineData("amount")][InlineData("building")][InlineData("coverage")][InlineData("nullArray")]
    [InlineData("duration")][InlineData("fuel")][InlineData("source")][InlineData("scope")]
    public async Task RejectsIncompleteOrInconsistentGraphs(string defect)
    {
        var e=Envelope();var g=e["data"]!;var d=g["definitions"]!;
        switch(defect){
            case "version":e["bridgeVersion"]="0.20.1";break;
            case "revision":g["graphRevision"]="invalid";break;
            case "duplicate":d["goods"]!.AsArray().Add(d["goods"]![0]!.DeepClone());break;
            case "missingGood":d["recipes"]![0]!["inputs"]![0]!["good"]="Missing";break;
            case "amount":d["recipes"]![0]!["outputs"]![0]!["amount"]=0;break;
            case "building":d["recipes"]![0]!["buildings"]![0]="Missing";break;
            case "coverage":g["coverage"]!["goodsWithoutKnownSource"]=new JsonArray();break;
            case "nullArray":d["sources"]=null;break;
            case "duration":d["recipes"]![0]!["hours"]=-1;break;
            case "fuel":d["recipes"]![1]!["fuelCycles"]=0;break;
            case "source":d["sources"]![0]!["planters"]![0]="Missing";break;
            case "scope":g["completeWithinScope"]=false;break;
        }
        // Correct checksum on malformed content ensures structural validation is exercised independently.
        if(defect!="revision")g["graphRevision"]=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(g["gameVersion"]!.GetValue<string>()+"\n"+g["faction"]!.GetValue<string>()+"\n"+d.ToJsonString()))).ToLowerInvariant();
        using var h=new Reply(e);using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h));
        var result=await tools.Invoke("inspect_production_graph",JsonSerializer.SerializeToElement(new{}),TestContext.Current.CancellationToken);
        Assert.Equal("backend_incompatible",result["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public async Task RejectsPaginationBeforeCallingGame()
    {
        using var h=new Reply(Envelope());using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h));
        var result=await tools.Invoke("inspect_production_graph",JsonSerializer.SerializeToElement(new{offset=0}),TestContext.Current.CancellationToken);
        Assert.NotEqual("ok",result["status"]!.GetValue<string>());Assert.Equal(0,h.Calls);
    }
    private sealed class Reply(JsonObject envelope):HttpMessageHandler
    {
        public int Calls{get;private set;}
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct)
        {
            Calls++;Assert.Equal(HttpMethod.Get,r.Method);Assert.Equal("/agent-api/v1/production-graph",r.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(envelope.ToJsonString())});
        }
    }
}
