using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ActivityCorrelationTests
{
    private const string Project="11111111-1111-1111-1111-111111111111";
    private const string Run="22222222-2222-2222-2222-222222222222";
    private const string Parent="33333333-3333-3333-3333-333333333333";
    private static ActivityCorrelation Ref(string kind,string id,string source="request")=>new(kind,id,source);
    private static JsonObject Result(object data)=>JsonSerializer.SerializeToNode(new{data})!.AsObject();

    [Theory]
    [InlineData("run_large_project")][InlineData("inspect_large_project")][InlineData("stop_large_project")]
    [InlineData("execute_building_project_pilot")][InlineData("inspect_building_completion")]
    [InlineData("develop_building_project")]
    public void KnownProjectRequestsAreExplicitReferences(string name)
    {
        var refs=ActivityCorrelationTools.Request(name,JsonSerializer.SerializeToElement(new{actionId=Project,runId=Run}));
        var item=Assert.Single(refs);Assert.Equal("project",item.Kind);Assert.Equal(Project,item.Id);Assert.Equal("request",item.Source);
    }
    [Fact]
    public void RepairRetainsParentRoleAndResultRunWithoutInferringAnExecution()
    {
        var args=JsonSerializer.SerializeToElement(new{actionId=Project,parentActionId=Parent});
        var request=ActivityCorrelationTools.Request("run_large_project_repair",args);
        Assert.Contains(request,r=>r.Kind=="parent_project"&&r.Id==Parent);
        var result=ActivityCorrelationTools.Result("run_large_project_repair",Result(new{actionId=Project,simulation=new{runId=Run}}));
        Assert.Contains(result,r=>r.Kind=="run"&&r.Id==Run&&r.Source=="result");
        Assert.DoesNotContain(request,r=>r.Kind=="run");
    }
    [Theory]
    [InlineData("inspect_agent_log")][InlineData("set_building_paused")][InlineData("unknown_tool")]
    [InlineData("inspect_power_network")]
    public void UnrelatedIdsAndLogFiltersNeverBecomeCorrelations(string name)
    {
        Assert.Empty(ActivityCorrelationTools.Request(name,JsonSerializer.SerializeToElement(new{actionId=Project,runId=Run,projectId=Project,id=Parent})));
        Assert.Empty(ActivityCorrelationTools.Result(name,Result(new{actionId=Project,runId=Run})));
    }
    [Fact]
    public void MissingMalformedAndDuplicateRequestIdsAreIgnored()
    {
        using var duplicate=JsonDocument.Parse("{\"actionId\":\""+Project+"\",\"actionId\":\""+Parent+"\"}");
        Assert.Empty(ActivityCorrelationTools.Request("inspect_large_project",duplicate.RootElement));
        Assert.Empty(ActivityCorrelationTools.Request("inspect_simulation_run",JsonSerializer.SerializeToElement(new{runId=42})));
        Assert.Empty(ActivityCorrelationTools.Result("inspect_simulation_run",Result(new{runId="not-a-guid"})));
        Assert.Empty(ActivityCorrelationTools.Result("inspect_simulation_run",new JsonObject{["data"]=null}));
    }
    [Fact]
    public void NestedCompletionAndObservedChildrenAreLinkedButReservedIdsAreNot()
    {
        var refs=ActivityCorrelationTools.Result("develop_building_project",Result(new{start=new{actionId=Project},completion=new{actionId=Project,runId=Run}}));
        Assert.Collection(refs,r=>Assert.Equal("project",r.Kind),r=>Assert.Equal("run",r.Kind));
        Assert.Empty(ActivityCorrelationTools.Result(ClearedGroupTools.Inspect,Result(new{actionId=Project,runId=Run})));
        var children=ActivityCorrelationTools.Result(ClearedGroupTools.Inspect,Result(new{clearanceRun=new{runId=Run},construction=new{actionId=Project}}));
        Assert.Contains(children,r=>r.Kind=="run"&&r.Id==Run);Assert.Contains(children,r=>r.Kind=="project"&&r.Id==Project);
    }

    private static ActivityRequest Event(string id,string state,params ActivityCorrelation[] refs)=>ActivityRequest.Parse(new NameValueCollection{
        {"id",id},{"tool","run_large_project"},{"state",state},{"session",state=="running"?"":Parent},
        {"reasoning","Synthetische Zuordnungsprobe."},{"summary",""},{"correlation",ActivityCorrelation.Encode(refs)}});
    private static ActivityLogQuery Query(string project=Project,string run="",string? cursor=null)
    {
        var values=new NameValueCollection{{"limit","1"}};
        if(project.Length>0)values.Add("projectId",project);
        if(run.Length>0)values.Add("runId",run);
        if(cursor is not null)values.Add("cursor",cursor);
        return ActivityLogQuery.Parse(values);
    }
    private static NativeActivityLog Page(ActivityLog log,ActivityLogQuery query)
    {
        var page=JsonSerializer.Deserialize<NativeActivityLog>(JsonSerializer.Serialize(log.Read(query,false,false)),NativeJson.Options)!;
        ActivityTools.ValidatePage(page,query);return page;
    }
    [Fact]
    public void CompletionAddsResultReferencesKeepsRequestAndPreservesSequence()
    {
        var log=new ActivityLog();log.Record(Event(Parent,"running",Ref("project",Project)),DateTimeOffset.UnixEpoch);
        log.Record(Event(Parent,"ok",Ref("project",Project,"result"),Ref("run",Run,"result")),DateTimeOffset.UnixEpoch);
        var entry=Assert.Single(log.Snapshot());Assert.Equal(1L,entry.Sequence);
        Assert.Contains(entry.Correlations!,r=>r.Kind=="project"&&r.Source=="request");
        Assert.Contains(entry.Correlations!,r=>r.Kind=="project"&&r.Source=="result");
        Assert.Single(Page(log,Query(run:Run)).Items);
        Assert.Empty(Page(log,Query(run:Parent)).Items);
        Assert.False(Page(log,Query(project:Run)).GapDetected);
    }
    [Fact]
    public void CorrelationFiltersBindCursorAndIncludeParentReferences()
    {
        var log=new ActivityLog();
        log.Record(Event(Project,"running",Ref("parent_project",Parent)),DateTimeOffset.UnixEpoch);
        log.Record(Event(Run,"running",Ref("project",Parent)),DateTimeOffset.UnixEpoch);
        var first=Page(log,Query(Parent));Assert.True(first.HasMore);
        var next=Page(log,Query(Parent,cursor:first.NextCursor));Assert.Single(next.Items);Assert.False(next.HasMore);
        Assert.Throws<ArgumentException>(()=>Query(Project,cursor:first.NextCursor));
        Assert.Throws<ArgumentException>(()=>Query(Parent,Run,first.NextCursor));
        Assert.Throws<InvalidDataException>(()=>ActivityTools.ValidatePage(first with{ProjectId=Project},Query(Parent)));
    }
    [Fact]
    public void MissingCompletionAndClearNeverInventOrResurrectReferences()
    {
        var log=new ActivityLog();log.Record(Event(Project,"running",Ref("project",Project)),DateTimeOffset.UnixEpoch);
        Assert.Single(Page(log,Query()).Items);Assert.Empty(Page(log,Query(run:Run)).Items);
        log.Clear();Assert.False(log.Record(Event(Project,"ok",Ref("run",Run,"result")),DateTimeOffset.UnixEpoch));
        Assert.Empty(log.Snapshot());
    }
    [Theory]
    [InlineData("project:not-a-guid:request")][InlineData("entity:11111111-1111-1111-1111-111111111111:request")]
    [InlineData("run:22222222-2222-2222-2222-222222222222:confirmed")]
    public void StrictWireFormatRejectsUntrustedValues(string text)=>Assert.Throws<ArgumentException>(()=>ActivityCorrelation.Decode(text));

    [Fact]
    public void DuplicateOversizedAndMissingReferenceMetadataAreRejected()
    {
        var reference=Ref("run",Run);var text=ActivityCorrelation.Encode(new[]{reference});
        Assert.Throws<ArgumentException>(()=>ActivityCorrelation.Decode(text+";"+text));
        Assert.Throws<ArgumentException>(()=>ActivityCorrelation.Encode(Enumerable.Repeat(reference,9)));
        var log=new ActivityLog();log.Record(Event(Project,"running",reference),DateTimeOffset.UnixEpoch);
        var query=Query(project:"",run:Run);var page=Page(log,query);
        var item=Assert.Single(page.Items);
        var without=new ActivityEntry(item.Id,item.Tool,item.State,item.Reasoning,item.Summary,item.StartedAtUtc,item.UpdatedAtUtc,item.Sequence);
        Assert.Throws<InvalidDataException>(()=>ActivityTools.ValidatePage(page with{Items=[without]},query));
        Assert.Throws<ArgumentException>(()=>Query(run:"invalid"));
    }

    [Fact]
    public async Task ResultReferenceCrossesHeaderAndSurvivesReadback()
    {
        var handler=new Handler();using var client=new NativeClient(new(8081,new string('a',64)),handler);
        Assert.Equal(Parent,await client.TryRecordActivity(Project,"run_large_project","running","","Synthetisch.","",[Ref("project",Project)]));
        Assert.Equal(Parent,await client.TryRecordActivity(Project,"run_large_project","ok",Parent,"","",[Ref("run",Run,"result")]));
        var result=await ActivityTools.Read(client,JsonSerializer.SerializeToElement(new{projectId=Project,runId=Run}),TestContext.Current.CancellationToken);
        var entry=Assert.Single(result["data"]!["items"]!.AsArray());
        Assert.Contains(entry!["correlations"]!.AsArray(),item=>item!["kind"]!.GetValue<string>()=="run"&&item["source"]!.GetValue<string>()=="result");
        Assert.Contains("Ergebnis",ActivityCorrelation.Display(Ref("run",Run,"result")));
    }

    [Fact]
    public async Task LoggedRejectedCallPreservesRequestAndNativeReaderFiltersIt()
    {
        var handler=new Handler();using var client=new NativeClient(new(8081,new string('a',64)),handler);
        using var tools=new NativeTools(client);
        var response=await tools.InvokeLogged("run_simulation_for",JsonSerializer.SerializeToElement(new{runId=Run,session=Parent,reasoning="Synthetischer gesperrter Lauf."}),TestContext.Current.CancellationToken);
        Assert.Equal("error",response["status"]!.GetValue<string>());
        var entry=Assert.Single(handler.Log.Snapshot());
        Assert.Equal(Run,Assert.Single(entry.Correlations!).Id);Assert.Equal("error",entry.State);
        Assert.Equal(0,handler.ActionCalls);
        var result=await ActivityTools.Read(client,JsonSerializer.SerializeToElement(new{runId=Run}),TestContext.Current.CancellationToken);
        Assert.Single(result["data"]!["items"]!.AsArray());
        Assert.Contains("angefragt",ActivityCorrelation.Display(Assert.Single(entry.Correlations!)));
    }
    private sealed class Handler:HttpMessageHandler
    {
        public ActivityLog Log=new();public int ActionCalls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            var query=System.Web.HttpUtility.ParseQueryString(request.RequestUri!.Query);object data;
            if(request.RequestUri.AbsolutePath.EndsWith("/activity")) {
                Assert.DoesNotContain("correlation",request.RequestUri.Query);
                query.Add("reasoning",ActivityRequest.Decode(request.Headers.GetValues("X-Agent-Reasoning").Single(),600));
                query.Add("summary",ActivityRequest.Decode(request.Headers.GetValues("X-Agent-Summary").Single(),400));
                if(request.Headers.TryGetValues("X-Agent-Correlation",out var header))query.Add("correlation",ActivityRequest.Decode(header.Single(),600));
                data=new NativeActivityAck(Log.Record(ActivityRequest.Parse(query),DateTimeOffset.UnixEpoch));
            } else if(request.RequestUri.AbsolutePath.EndsWith("/activity-log"))data=Log.Read(ActivityLogQuery.Parse(query),false,false);
            else {ActionCalls++;throw new InvalidOperationException("unexpected_game_request");}
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(
                new BridgeEnvelope<object>(1,Parent,DateTimeOffset.UnixEpoch,"0.56.0",data),NativeJson.Options))});
        }
    }
}
