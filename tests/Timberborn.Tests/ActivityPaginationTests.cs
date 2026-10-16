using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ActivityPaginationTests
{
    private static readonly DateTimeOffset Now=DateTimeOffset.UnixEpoch;
    private static ActivityLogQuery Query(int limit=32,string? cursor=null,string? tool=null,string? state=null)
    {
        var values=new NameValueCollection { {"limit",limit.ToString()} };
        if(cursor is not null)values.Add("cursor",cursor);
        if(tool is not null)values.Add("tool",tool);
        if(state is not null)values.Add("state",state);
        return ActivityLogQuery.Parse(values);
    }
    private static string Record(ActivityLog log,string tool="inspect_colony",string state="running",string? id=null)
    {
        id??=Guid.NewGuid().ToString("D");
        Assert.True(log.Record(ActivityRequest.Parse(new NameValueCollection {
            {"id",id},{"tool",tool},{"state",state},{"session",state=="running"?"":"11111111-1111-1111-1111-111111111111"},
            {"reasoning","Synthetische Logprobe."},{"summary",""}
        }),Now));
        return id;
    }
    private static NativeActivityLog Read(ActivityLog log,ActivityLogQuery query)
    {
        // Same explicit camelCase payload used by the game; no serializer naming policy.
        var page=JsonSerializer.Deserialize<NativeActivityLog>(JsonSerializer.Serialize(log.Read(query,false,false)),NativeJson.Options)!;
        ActivityTools.ValidatePage(page,query);
        return page;
    }

    [Fact]
    public void PaginationDoesNotSkipRetainedEntriesDespiteNewCallsAndCompletionUpdates()
    {
        var log=new ActivityLog();
        for(int i=0;i<127;i++)Record(log);
        var pending=Record(log);
        var first=Read(log,Query());
        Assert.Equal(Enumerable.Range(97,32).Reverse().Select(i=>(long)i),first.Items.Select(e=>e.Sequence));
        Record(log,state:"ok",id:pending);
        Record(log); // Evicts sequence 1, but does not shift the cursor boundary.
        var sequences=first.Items.Select(e=>e.Sequence).ToList();
        var page=first;
        while(page.HasMore==true) {
            page=Read(log,Query(cursor:page.NextCursor));
            sequences.AddRange(page.Items.Select(e=>e.Sequence));
        }
        Assert.Equal(Enumerable.Range(2,127).Reverse().Select(i=>(long)i),sequences);
        Assert.True(page.GapDetected);
        Assert.Equal(1L,page.UnavailableBeforeCursor);
        Assert.Null(page.NextCursor);
        Assert.Equal("ok",Read(log,Query(tool:"inspect_colony",state:"ok")).Items.Single().State);
    }

    [Fact]
    public void All128RetainedCallsAreReachableAndLimitMayChange()
    {
        var log=new ActivityLog();for(int i=0;i<128;i++)Record(log);
        var page=Read(log,Query(1));var all=page.Items.ToList();
        while(page.HasMore==true){page=Read(log,Query(32,page.NextCursor));all.AddRange(page.Items);}
        Assert.Equal(Enumerable.Range(1,128).Reverse().Select(i=>(long)i),all.Select(e=>e.Sequence));
        Assert.False(page.GapDetected);
    }

    [Fact]
    public void FilteringPrecedesPagingAndNoMatchesAreNotAGap()
    {
        var log=new ActivityLog();
        for(int i=0;i<12;i++) {
            var id=Record(log,i%2==0?"inspect_colony":"inspect_goods");
            Record(log,i%2==0?"inspect_colony":"inspect_goods","ok",id);
        }
        var first=Read(log,Query(2,tool:"inspect_goods",state:"ok"));
        Assert.Equal(new long[]{12,10},first.Items.Select(e=>e.Sequence));
        var second=Read(log,Query(2,first.NextCursor,"inspect_goods","ok"));
        Assert.Equal(new long[]{8,6},second.Items.Select(e=>e.Sequence));
        Assert.False(second.GapDetected);
        var empty=Read(log,Query(tool:"missing_tool"));
        Assert.Empty(empty.Items);Assert.False(empty.HasMore);Assert.False(empty.GapDetected);
        Assert.Throws<ArgumentException>(()=>Query(2,first.NextCursor,"inspect_colony","ok"));
        Assert.Throws<ArgumentException>(()=>Query(2,first.NextCursor,"inspect_goods","running"));
    }

    [Fact]
    public void EvictionCanExhaustCursorAndMissingFilteredCountRemainsUnknown()
    {
        var log=new ActivityLog();
        for(int i=0;i<40;i++)Record(log);
        var first=Read(log,Query(2,tool:"inspect_colony"));
        for(int i=0;i<128;i++)Record(log,"inspect_goods");
        var last=Read(log,Query(2,first.NextCursor,"inspect_colony"));
        Assert.Empty(last.Items);Assert.False(last.HasMore);Assert.True(last.GapDetected);
        Assert.Equal(38L,last.UnavailableBeforeCursor);Assert.Equal(40L,last.EvictedCount);
    }

    [Fact]
    public void ClearAndNewLogRejectOldCursorAndEmptyLogHasHonestBounds()
    {
        var log=new ActivityLog();Record(log);Record(log);
        var first=Read(log,Query(1));
        Assert.Equal("stale_activity_cursor",Assert.Throws<BridgeRejectionException>(()=>Read(new ActivityLog(),Query(1,first.NextCursor))).Code);
        log.Clear();
        Assert.Throws<BridgeRejectionException>(()=>Read(log,Query(1,first.NextCursor)));
        var empty=Read(log,Query());
        Assert.Empty(empty.Items);Assert.Null(empty.OldestSequence);Assert.Null(empty.NewestSequence);
        Assert.False(empty.GapDetected);Assert.Equal(0L,empty.EvictedCount);
    }

    [Theory]
    [InlineData("limit","0")][InlineData("limit","33")][InlineData("limit","1.5")]
    [InlineData("tool","Bad Tool")][InlineData("tool","")][InlineData("state","success")]
    [InlineData("cursor","broken")][InlineData("offset","1")][InlineData("Tool","inspect_colony")]
    public void BridgeRejectsInvalidArguments(string key,string value) =>
        Assert.Throws<ArgumentException>(()=>BridgeRequest.Parse("/agent-api/v1/activity-log",new NameValueCollection{{key,value}}));

    [Fact]
    public void DuplicateArgumentsAreRejected()
    {
        Assert.Throws<ArgumentException>(()=>ActivityLogQuery.Parse(new NameValueCollection{{"limit","1"},{"limit","2"}}));
    }

    [Theory]
    [InlineData("{\"limit\":33}")][InlineData("{\"limit\":\"2\"}")]
    [InlineData("{\"tool\":null}")][InlineData("{\"state\":\"success\"}")]
    [InlineData("{\"limit\":1,\"limit\":2}")]
    public async Task McpRejectsMalformedQueryBeforeTransport(string json)
    {
        var handler=new PageHandler();using var client=new NativeClient(new(8081,new string('a',64)),handler);
        using var doc=JsonDocument.Parse(json);
        await Assert.ThrowsAsync<ArgumentException>(()=>ActivityTools.Read(client,doc.RootElement,TestContext.Current.CancellationToken));
        Assert.Equal(0,handler.Calls);
    }

    [Fact]
    public async Task NativeTransportAndMcpSchemaExposePageAndFilters()
    {
        var handler=new PageHandler();using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var args=JsonSerializer.SerializeToElement(new{limit=1,tool="inspect_colony",state="ok"});
        var first=await ActivityTools.Read(client,args,TestContext.Current.CancellationToken);
        var cursor=first["data"]!["nextCursor"]!.GetValue<string>();
        var second=await ActivityTools.Read(client,JsonSerializer.SerializeToElement(new{limit=1,tool="inspect_colony",state="ok",cursor}),TestContext.Current.CancellationToken);
        Assert.Single(second["data"]!["items"]!.AsArray());
        Assert.False(second["data"]!["hasMore"]!.GetValue<bool>());
        Assert.True(ActivityTools.Reader().InputSchema.GetProperty("properties").TryGetProperty("cursor",out _));
        Assert.True(ActivityTools.Reader().Annotations!.ReadOnlyHint);
    }

    [Fact]
    public void CorruptOrderingFiltersCursorAndGapAreRejected()
    {
        var log=new ActivityLog();Record(log);Record(log);Record(log);
        var query=Query(2);var page=Read(log,query);
        Assert.Throws<InvalidDataException>(()=>ActivityTools.ValidatePage(page with {Items=page.Items.Reverse().ToArray()},query));
        Assert.Throws<InvalidDataException>(()=>ActivityTools.ValidatePage(page with {NextCursor="bad"},query));
        Assert.Throws<InvalidDataException>(()=>ActivityTools.ValidatePage(page with {GapDetected=true},query));
        Assert.Throws<InvalidDataException>(()=>ActivityTools.ValidatePage(page with {Tool="other"},query));
        Assert.Throws<InvalidDataException>(()=>ActivityTools.ValidatePage(page with {RetainedCount=129},query));
    }

    private sealed class PageHandler:HttpMessageHandler
    {
        public int Calls;
        private readonly ActivityLog log=new();
        public PageHandler() {
            for(int i=0;i<2;i++){var id=Record(log);Record(log,state:"ok",id:id);}
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;Assert.Equal(HttpMethod.Get,request.Method);
            var route=BridgeRequest.Parse(request.RequestUri!.AbsolutePath,System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query));
            var data=log.Read(route.ActivityQuery!,false,false);
            var envelope=new BridgeEnvelope<object>(1,"11111111-1111-1111-1111-111111111111",Now,"0.55.0",data);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(envelope,NativeJson.Options))});
        }
    }
}
