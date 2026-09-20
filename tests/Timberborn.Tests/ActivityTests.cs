using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class ActivityTests
{
    private static ActivityRequest Event(string id,string state="running",string tool="inspect_colony",string reason="Wasserbestand prüfen.") => ActivityRequest.Parse(new NameValueCollection {
        {"id",id},{"tool",tool},{"state",state},{"session",state=="running"?"":Guid.NewGuid().ToString()}, {"reasoning",reason},{"summary","x=1"} });
    [Fact]
    public void BoundedLogCorrelatesConcurrentCallsAndDoesNotResurrectClearedEntries()
    {
        var log=new ActivityLog();var now=DateTimeOffset.UtcNow;
        var a=Guid.NewGuid().ToString();var b=Guid.NewGuid().ToString();
        Assert.True(log.Record(Event(a),now));Assert.True(log.Record(Event(b),now));
        Assert.False(log.Record(Event(a),now));
        Assert.False(log.Record(Event(a,"ok","wrong_tool"),now));
        Assert.True(log.Record(Event(b,"error"),now));Assert.True(log.Record(Event(a,"ok"),now));
        Assert.Equal(new[]{"ok","error"},log.Snapshot().Select(e=>e.State));
        Assert.False(log.Record(Event(a,"error"),now));
        for(int i=0;i<140;i++)log.Record(Event(Guid.NewGuid().ToString()),now);
        Assert.Equal(128,log.Snapshot().Length);Assert.DoesNotContain(log.Snapshot(),e=>e.Id==a);
        var pending=log.Snapshot()[0].Id;log.Clear();
        Assert.False(log.Record(Event(pending,"ok"),now));Assert.Empty(log.Snapshot());
    }
    [Theory]
    [InlineData("tool","Injected\nName")][InlineData("state","success")][InlineData("id","bad")]
    [InlineData("session","wrong")][InlineData("extra","value")]
    public void MalformedEventsAreRejected(string key,string value)
    {
        var q=new NameValueCollection{{"id",Guid.NewGuid().ToString()},{"tool","inspect_colony"},{"state","running"},{"session",""},{"reasoning",""},{"summary",""}};
        q[key]=value;Assert.Throws<ArgumentException>(()=>ActivityRequest.Parse(q));
    }
    [Fact]
    public void ReasoningIsOptionalBoundedAndNeverAnActionArgument()
    {
        var args=JsonSerializer.SerializeToElement(new {speed=1,reasoning="Weiterlaufen für Baufortschritt."});
        Assert.False(ActivityTools.WithoutReasoning(args).TryGetProperty("reasoning",out _));
        Assert.Equal(1,ActivityTools.WithoutReasoning(args).GetProperty("speed").GetInt32());
        Assert.Throws<ArgumentException>(()=>ActivityTools.WithoutReasoning(JsonSerializer.SerializeToElement(new{reasoning=2})));
        Assert.Throws<ArgumentException>(()=>ActivityTools.WithoutReasoning(JsonSerializer.SerializeToElement(new{reasoning=new string('x',601)})));
        using var duplicate=JsonDocument.Parse("{\"reasoning\":\"a\",\"reasoning\":\"b\"}");
        Assert.Throws<ArgumentException>(()=>ActivityTools.WithoutReasoning(duplicate.RootElement));
        Assert.All(NativeTools.Catalog(),t=>{
            Assert.Equal(600,t.InputSchema.GetProperty("properties").GetProperty("reasoning").GetProperty("maxLength").GetInt32());
            if(t.InputSchema.TryGetProperty("required",out var required))Assert.DoesNotContain(required.EnumerateArray(),e=>e.GetString()=="reasoning");
        });
    }
    [Fact]
    public void MetadataDoesNotIncludeAuthenticationOrSessionAndUtf8IsStrict()
    {
        var secret=new string('a',64);
        var detail=ActivityTools.Describe(JsonSerializer.SerializeToElement(new {token=secret,authorization="private",session=Guid.NewGuid(),x=2,reasoning="prüfen Bearer "+secret}));
        Assert.Equal("x=2",detail.Summary);Assert.DoesNotContain(secret,detail.Reasoning);
        var reason="Wasser prüfen – für die Kolonie.";
        Assert.Equal(reason,ActivityRequest.Decode(Convert.ToBase64String(Encoding.UTF8.GetBytes(reason)),600));
        Assert.Throws<ArgumentException>(()=>ActivityRequest.Decode("not-base64",600));
        Assert.Throws<ArgumentException>(()=>ActivityRequest.Clean("bad\0text",600));
        Assert.Throws<ArgumentException>(()=>ActivityRequest.Decode(Convert.ToBase64String(new byte[]{0xff}),600));
    }
    [Fact]
    public async Task InvalidLocalCallStillLogsButCannotExecuteAction()
    {
        var handler=new ActivityHandler();using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler));
        var result=await tools.InvokeLogged("set_storage_good",JsonSerializer.SerializeToElement(new {reasoning="Lager vorbereiten."}),TestContext.Current.CancellationToken);
        Assert.Equal("error",result["status"]!.GetValue<string>());Assert.Equal(0,handler.ActionCalls);
        Assert.Equal(new[]{"running","error"},handler.States);
    }
    [Fact]
    public async Task MissingCompletionDoesNotRepeatOrFailSuccessfulAction()
    {
        var handler=new ActivityHandler{FailCompletion=true};using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingSettings:true);
        var result=await tools.InvokeLogged("set_building_paused",JsonSerializer.SerializeToElement(new{id=handler.Id,session=handler.Session,paused=true,expectedPaused=false,reasoning="Pumpe kurz testen."}),TestContext.Current.CancellationToken);
        Assert.Equal("ok",result["status"]!.GetValue<string>());Assert.Equal(1,handler.ActionCalls);
        Assert.Equal(new[]{"running","applied"},handler.States);
    }
    [Fact]
    public void TelemetryGateNeverEnablesGameWrites()
    {
        Assert.True(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/activity",false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/activity",false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/activity",true,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/set-building-paused",false,false));
    }
    [Fact]
    public void GamePayloadUsesExplicitWireNamesWithoutSerializerNamingPolicy()
    {
        var now=DateTimeOffset.UtcNow;
        var entry=new ActivityEntry(Guid.NewGuid().ToString(),"inspect_colony","ok","Bestand prüfen.","",now,now);
        var wire=JsonSerializer.Serialize(ActivityLog.Payload(entry));
        Assert.Contains("\"reasoning\"",wire);Assert.DoesNotContain("\"Reasoning\"",wire);
        var read=JsonSerializer.Deserialize<ActivityEntry>(wire,NativeJson.Options)!;
        Assert.Equal(entry.Id,read.Id);Assert.Equal(entry.Reasoning,read.Reasoning);Assert.Equal(now,read.StartedAtUtc);
    }
    private sealed class ActivityHandler:HttpMessageHandler
    {
        public string Session=Guid.NewGuid().ToString();public Guid Id=Guid.NewGuid();public List<string> States=new();public int ActionCalls;public bool FailCompletion;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            object data;
            if(request.RequestUri!.AbsolutePath.EndsWith("/activity")) {
                var query=System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);var state=query["state"]!;States.Add(state);
                Assert.Equal(HttpMethod.Post,request.Method);Assert.Null(request.Content);
                Assert.DoesNotContain("reasoning",request.RequestUri.Query);
                if(state=="running")Assert.Equal("",query["session"]);else Assert.Equal(Session,query["session"]);
                if(FailCompletion&&state!="running")return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                data=new NativeActivityAck(true);
            } else {
                ActionCalls++;Assert.EndsWith("/set-building-paused",request.RequestUri.AbsolutePath);
                var observed=new NativeBuildingSettings(Id,"WaterPump.Folktails",new(1,2,3),true,new(true,true),false,null,false,null,["synthetic_test"]);
                data=new NativeSettingChange(Id,"WaterPump.Folktails","paused","false","true","true","applied",observed,["synthetic_test"]);
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new BridgeEnvelope<object>(1,Session,DateTimeOffset.UtcNow,"0.16.0",data),NativeJson.Options))});
        }
    }
}
