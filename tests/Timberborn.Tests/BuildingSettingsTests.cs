using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class BuildingSettingsTests
{
    [Theory]
    [InlineData("GET",true,false,false)] [InlineData("POST",false,false,false)]
    [InlineData("POST",true,true,false)] [InlineData("POST",true,false,true)]
    public void AllSettingWritesNeedOwnBodylessPost(string method,bool enabled,bool body,bool expected)
    {
        foreach(var route in new[]{"set-building-paused","set-storage-good","set-storage-mode","set-farm-priority","set-farm-crop"})
            Assert.Equal(expected,BridgeHttpServer.MethodAllowed(method,"/agent-api/v1/"+route,body,true,true,true,true,true,true,true,true,true,enabled));
    }
    [Fact]
    public void StaleOrMatchingSettingDoesNotMutate()
    {
        int calls=0;var value="accept";
        Assert.Throws<BridgeRejectionException>(()=>SettingChange.Execute("supply","empty",()=>value,()=>calls++));
        var result=SettingChange.Execute("accept","accept",()=>value,()=>calls++);
        Assert.Equal("applied",result.Outcome);Assert.Equal(0,calls);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FailureDoesNotRetryOrRollback(bool changes)
    {
        int calls=0;var value="accept";
        var result=SettingChange.Execute("accept","empty",()=>value,()=>{calls++;if(changes)value="empty";throw new IOException();});
        Assert.Equal("unconfirmed",result.Outcome);Assert.Equal(changes?"empty":"accept",result.Observed);Assert.Equal(1,calls);
    }
    [Fact]
    public void IgnoredGameSetterIsUnconfirmed()
    {Assert.Equal("unconfirmed",SettingChange.Execute("accept","empty",()=>"accept",()=>{}).Outcome);}

    [Theory]
    [InlineData("set-building-paused","paused","1")]
    [InlineData("set-storage-mode","mode","destroy")]
    [InlineData("set-farm-priority","priority","High")]
    [InlineData("set-farm-crop","resource","")]
    [InlineData("set-storage-good","good","Water&x=1")]
    public void InvalidValuesCannotReachGame(string route,string key,string value)
    {
        var q=BaseQuery();q[key]=value;q[BuildingSettingsRequest.ExpectedKey(route)]="";
        Assert.Throws<ArgumentException>(()=>BuildingSettingsRequest.Parse("/agent-api/v1/"+route,q));
    }
    [Fact]
    public void ExtraDuplicateOrMissingArgumentsAreRejected()
    {
        var q=BaseQuery();q["good"]="Berries";q["expectedGood"]="";
        Assert.Equal("Berries",BuildingSettingsRequest.Parse("/agent-api/v1/set-storage-good",q).Value);
        q.Add("good","Water");Assert.Throws<ArgumentException>(()=>BuildingSettingsRequest.Parse("/agent-api/v1/set-storage-good",q));
        q.Set("good","Berries");q["extra"]="x";Assert.Throws<ArgumentException>(()=>BuildingSettingsRequest.Parse("/agent-api/v1/set-storage-good",q));
        q.Remove("extra");q.Remove("expectedGood");Assert.Throws<ArgumentException>(()=>BuildingSettingsRequest.Parse("/agent-api/v1/set-storage-good",q));
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task PauseBooleanAndOptInValidatedBeforeTransport(bool enabled)
    {
        var handler=new Handler("{}");using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingSettings:enabled);
        var a=JsonSerializer.SerializeToElement(new{id=Guid.NewGuid(),session=Guid.NewGuid(),paused=enabled?"true":"false",expectedPaused=false});
        var result=await tools.Invoke("set_building_paused",a,TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument",result["error"]!["code"]!.GetValue<string>());Assert.Equal(0,handler.Calls);
    }
    [Theory]
    [InlineData("set_storage_good","good","","Berries")]
    [InlineData("set_storage_mode","mode","accept","supply")]
    [InlineData("set_farm_priority","priority","harvesting","planting")]
    [InlineData("set_farm_crop","resource","","Carrot")]
    [InlineData("set_building_paused","paused","false","true")]
    public async Task ValidReceiptsMatchSeparateSettingsObservation(string tool,string key,string before,string after)
    {
        var id=Guid.NewGuid();var session=Guid.NewGuid().ToString("D");var observation=Sample(id) with { Pause=new(true,true) };
        var receipt=new NativeSettingChange(id,observation.Template,key,before,after,after,"applied",observation,["synthetic_test"]);
        var handler=new Handler(JsonSerializer.Serialize(new BridgeEnvelope<NativeSettingChange>(1,session,DateTimeOffset.UtcNow,"0.15.0",receipt),NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingSettings:true);
        var a=new Dictionary<string,object?> { ["id"]=id,["session"]=session,[key]=key=="paused"?true:after,
            ["expected"+char.ToUpperInvariant(key[0])+key[1..]]=key=="paused"?false:before };
        var result=await tools.Invoke(tool,JsonSerializer.SerializeToElement(a),TestContext.Current.CancellationToken);
        Assert.Equal("ok",result["status"]!.GetValue<string>());Assert.Equal(1,handler.Calls);Assert.Equal(HttpMethod.Post,handler.Method);
    }
    [Theory]
    [InlineData("session")] [InlineData("id")] [InlineData("observation")] [InlineData("value")]
    [InlineData("version")] [InlineData("transport")]
    public async Task UntrustedReceiptsRejectedWithoutRetry(string variant)
    {
        var id=Guid.NewGuid();var session=Guid.NewGuid().ToString("D");var observation=Sample(variant=="observation"?Guid.NewGuid():id);
        var receipt=new NativeSettingChange(variant=="id"?Guid.NewGuid():id,observation.Template,"good","","Berries",variant=="value"?"Water":"Berries","applied",observation,["synthetic_test"]);
        var envelope=new BridgeEnvelope<NativeSettingChange>(1,variant=="session"?Guid.NewGuid().ToString("D"):session,DateTimeOffset.UtcNow,variant=="version"?"0.14.1":"0.15.0",receipt);
        var handler=new Handler(variant=="transport"?null:JsonSerializer.Serialize(envelope,NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingSettings:true);
        var result=await tools.Invoke("set_storage_good",JsonSerializer.SerializeToElement(new{id,session,good="Berries",expectedGood=""}),TestContext.Current.CancellationToken);
        Assert.Equal("error",result["status"]!.GetValue<string>());Assert.False(result["error"]!["retryable"]!.GetValue<bool>());Assert.Equal(1,handler.Calls);
    }
    [Theory]
    [InlineData("ok")] [InlineData("unfinished")] [InlineData("mode")] [InlineData("duplicate_good")] [InlineData("stock")]
    public async Task SettingsReadValidatesAvailabilityAndOptions(string variant)
    {
        var id=Guid.NewGuid();var session=Guid.NewGuid().ToString("D");var data=Sample(id);
        if(variant=="unfinished")data=data with { Finished=false };
        if(variant=="mode")data=data with { Storage=data.Storage! with { Mode="destroy" } };
        if(variant=="duplicate_good")data=data with { Storage=data.Storage! with { AllowedGoods=["Berries","Berries"] } };
        if(variant=="stock")data=data with { Storage=data.Storage! with { Stock=[new("Berries",-1)] } };
        var handler=new Handler(JsonSerializer.Serialize(new BridgeEnvelope<NativeBuildingSettings>(1,session,DateTimeOffset.UtcNow,"0.15.0",data),NativeJson.Options));
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler));
        var result=await tools.Invoke("inspect_building_settings",JsonSerializer.SerializeToElement(new{id,session}),TestContext.Current.CancellationToken);
        Assert.Equal(variant=="ok"?"ok":"error",result["status"]!.GetValue<string>());Assert.Equal(HttpMethod.Get,handler.Method);
    }
    private static NativeBuildingSettings Sample(Guid id)=>new(id,"SyntheticSettingsBuilding",new(1,2,3),true,new(false,true),true,
        new("Berries",true,["Berries","Carrot"],"supply",true,30,[new("Berries",2)],false),true,
        new("planting","Carrot",true,false,[new("Carrot",true)]),["synthetic_hybrid_fixture_not_a_real_game_template"]);
    private static NameValueCollection BaseQuery()=>new(){["id"]=Guid.NewGuid().ToString("D"),["session"]=Guid.NewGuid().ToString("D")};
    private sealed class Handler(string? json):HttpMessageHandler
    {
        public int Calls;public HttpMethod? Method;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct) {
            Calls++;Method=r.Method;Assert.Null(r.Content);if(json is null)throw new HttpRequestException();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});
        }
    }
}
