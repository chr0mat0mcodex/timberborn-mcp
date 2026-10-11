using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class RecentActionsTests
{
    private const string Session="11111111-1111-1111-1111-111111111111";
    private static RecentActionsRequest Request(long? after=null,int limit=32,string kind="all",string source="all") {
        var q=new NameValueCollection{{"session",Session},{"limit",limit.ToString()},{"kind",kind},{"source",source}};
        if(after is {} n)q.Add("after",n.ToString());return RecentActionsRequest.Parse(q);
    }
    private static void Add(RecentActionBuffer b,string kind="speed",string source="unknown")=>
        b.Add(DateTimeOffset.UnixEpoch,12,kind,source,"event",before:"0",after:"1");
    [Fact] public void TailThenCursorAndFilteringDoNotReplayOrSkipMatchingEntries() {
        var b=new RecentActionBuffer();for(int i=0;i<5;i++)Add(b,source:i%2==0?"mcp":"unknown");
        var tail=b.Read(Request(limit:2));Assert.Equal(new long[]{4,5},tail.Items.Select(e=>e.Sequence));Assert.Equal(5,tail.NextCursor);Assert.False(tail.HasMore);
        var filtered=b.Read(Request(0,1,source:"mcp"));Assert.Equal(1,filtered.NextCursor);Assert.True(filtered.HasMore);
        var next=b.Read(Request(filtered.NextCursor,1,source:"mcp"));Assert.Equal(3,next.NextCursor);
        var last=b.Read(Request(next.NextCursor,1,source:"mcp"));Assert.Equal(5,last.NextCursor);Assert.False(last.HasMore);
        Assert.Empty(b.Read(Request(last.NextCursor)).Items);
        var empty=b.Read(Request(0,kind:"recipe"));Assert.Empty(empty.Items);Assert.Equal(5,empty.NextCursor);
        Assert.Throws<BridgeRejectionException>(()=>b.Read(Request(6)));
    }
    [Fact] public void OverflowIsExplicitEvenWhenFilterMatchesNothing() {
        var b=new RecentActionBuffer();for(int i=0;i<520;i++)Add(b);
        var page=b.Read(Request(1,kind:"recipe"));Assert.True(page.Gap);Assert.Equal(9,page.OldestAvailable);Assert.Equal(8,page.Dropped);Assert.Equal(520,page.NextCursor);
        Assert.False(b.Read(Request(8)).Gap);Assert.Equal(64,b.Read(Request(8,64)).Items.Length);
        Assert.False(b.Read(Request()).Gap);
        var fresh=new RecentActionBuffer().Read(Request(0));Assert.Empty(fresh.Items);Assert.Equal(0,fresh.Latest);Assert.Equal(1,fresh.OldestAvailable);
    }
    [Theory]
    [InlineData("limit","0")][InlineData("limit","65")][InlineData("after","-1")][InlineData("after","9007199254740992")]
    [InlineData("source","human")][InlineData("kind","invented")][InlineData("session","not-session")][InlineData("extra","1")]
    public void InvalidArgumentsRejected(string key,string value) {
        var q=new NameValueCollection{{"session",Session}};q[key]=value;Assert.Throws<ArgumentException>(()=>RecentActionsRequest.Parse(q));
    }
    [Fact] public void DuplicatesAndUnsupportedAttributionRejected() {
        var q=new NameValueCollection{{"session",Session},{"limit","1"},{"limit","2"}};
        Assert.Throws<ArgumentException>(()=>RecentActionsRequest.Parse(q));
        Assert.Throws<ArgumentException>(()=>Add(new RecentActionBuffer(),source:"human"));
        Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/recent-actions",false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/recent-actions",false,true));
        Assert.True(RecentActionsTools.Reader().Annotations!.ReadOnlyHint);
    }
    private static NativeRecentActions Data()=>new(512,DateTimeOffset.UnixEpoch,DateTimeOffset.UnixEpoch,true,0,0,4096,1,
        new([new(1,DateTimeOffset.UnixEpoch,12,"speed","unknown","event",null,null,null,null,null,"0","1")],1,1,1,false,false,0),RecentActionsRequest.Kinds,[]);
    [Theory]
    [InlineData("ok")][InlineData("session")][InlineData("cursor")][InlineData("gap")][InlineData("source")][InlineData("order")][InlineData("filter")]
    public async Task TransportValidatesSessionCursorAttributionAndFilters(string defect) {
        var d=Data();
        if(defect=="cursor")d=d with{Page=d.Page with{NextCursor=0}};
        if(defect=="gap")d=d with{Page=d.Page with{Gap=true}};
        if(defect=="source")d=d with{Page=d.Page with{Items=[d.Page.Items[0] with{Source="human"}]}};
        if(defect=="order")d=d with{Page=d.Page with{Items=[d.Page.Items[0],d.Page.Items[0]]}};
        using var handler=new Reply(d,defect=="session"?"22222222-2222-2222-2222-222222222222":Session);
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var r=Request(0,kind:defect=="filter"?"recipe":"all");
        if(defect=="ok")Assert.Single((await client.RecentActions(r,TestContext.Current.CancellationToken)).Data.Page.Items);
        else await Assert.ThrowsAsync<InvalidDataException>(()=>client.RecentActions(r,TestContext.Current.CancellationToken));
        Assert.Equal(1,handler.Calls);
    }
    private sealed class Reply(object data,string session):HttpMessageHandler {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Calls++;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(
                new BridgeEnvelope<object>(1,session,DateTimeOffset.UnixEpoch,"0.40.0",data),NativeJson.Options))});
        }
    }
}
