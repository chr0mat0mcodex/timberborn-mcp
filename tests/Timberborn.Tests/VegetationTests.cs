using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;
public sealed class VegetationTests
{
    [Theory]
    [InlineData("alive",false,false,true,true)]
    [InlineData("dead",false,false,true,false)]
    [InlineData("alive",true,false,true,false)]
    [InlineData("alive",false,true,true,false)]
    [InlineData("alive",false,false,false,false)]
    [InlineData("unknown",false,false,true,false)]
    [InlineData("alive",null,false,true,false)]
    [InlineData("alive",false,null,true,false)]
    [InlineData("alive",false,false,null,false)]
    public void TappingRequiresKnownLivingMatureAndHealthy(string life,bool? dying,bool? water,bool? grown,bool expected)
    {Assert.Equal(expected,new VegetationState(life,dying,water,grown,0.5f).IsTappingCandidate());}
    [Theory]
    [InlineData("0.13.2","alive",false,true)]
    [InlineData("0.13.2","dead",false,false)]
    [InlineData("0.13.2","alive",true,false)]
    [InlineData("0.13.1","alive",false,false)]
    [InlineData("0.13.2",null,false,false)]
    public async Task TappingClientRejectsDeadUnknownAndOldSemantics(string version,string? life,bool water,bool valid)
    {
        var state=life is null?null:new VegetationState(life,false,water,true,1);
        var data=new NativeAreas("tapping",true,0,32,1,[new(new(1,2,3),"unmarked",state)],false,["synthetic_test"]);
        var json=JsonSerializer.Serialize(new BridgeEnvelope<NativeAreas>(1,Guid.NewGuid().ToString(),DateTimeOffset.UtcNow,version,data),NativeJson.Options);
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),new Handler(json)));
        var result=await tools.Invoke("inspect_areas",JsonSerializer.SerializeToElement(new{kind="tapping",offset=0,limit=32}),TestContext.Current.CancellationToken);
        Assert.Equal(valid?"ok":"error",result["status"]!.GetValue<string>());
    }
    [Theory]
    [InlineData("dead",true)][InlineData("unknown",true)][InlineData(null,false)][InlineData("invalid",false)]
    public async Task RemovalListPreservesDeadAndUnknownState(string? life,bool valid)
    {
        var state=life is null?null:new VegetationState(life,null,null,null,null);
        var data=new NativeRemovalTargets("vegetation",0,32,1,[new(Guid.NewGuid(),"vegetation","Pine",new(1,2,3),"demolition_mark",true,false,"unknown","current_components",state)],false,["synthetic_test"]);
        var json=JsonSerializer.Serialize(new BridgeEnvelope<NativeRemovalTargets>(1,Guid.NewGuid().ToString(),DateTimeOffset.UtcNow,"0.13.2",data),NativeJson.Options);
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),new Handler(json)));
        var result=await tools.Invoke("inspect_removal_targets",JsonSerializer.SerializeToElement(new{kind="vegetation",x=1,y=2,z=3,width=1,height=1,depth=1,offset=0,limit=32}),TestContext.Current.CancellationToken);
        Assert.Equal(valid?"ok":"error",result["status"]!.GetValue<string>());
    }
    private sealed class Handler(string json):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});}
}
