using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class SpatialJournalTests
{
    private const string Session="11111111-1111-4111-8111-111111111111";
    private static SpatialJournalState Mark(int x,string resource="SyntheticCrop",string kind="planting_designation")=>
        new(kind+"/"+x,kind,null,null,x,1,3,resource);
    private static SpatialJournalState Plant(string value)=>new("plant/"+Session,"plant",Session,"SyntheticTree",2,1,3,value);
    private static void Capture(SpatialJournal tracker,RecentActionBuffer buffer,params SpatialJournalState[] entries)=>
        tracker.Capture(entries,buffer,DateTimeOffset.UnixEpoch,12,"unknown");
    [Fact] public void BaselineIsSilentAndChangesPreserveValuesAndStumps() {
        var tracker=new SpatialJournal();var buffer=new RecentActionBuffer();
        Capture(tracker,buffer,Mark(0),Plant("tree"));Assert.Equal(0,buffer.Latest);
        Capture(tracker,buffer,Mark(0,"OtherCrop"),Mark(1,"marked","cutting_designation"),Plant("stump"));
        var changes=buffer.Window(0).Items;Assert.Equal(3,changes.Length);
        var state=Assert.Single(changes,i=>i.Kind=="plant_state");Assert.Equal("tree",state.Before);Assert.Equal("stump",state.After);
        var designation=Assert.Single(changes,i=>i.Kind=="planting_designation");Assert.Equal("SyntheticCrop",designation.Before);Assert.Equal("OtherCrop",designation.After);
        Capture(tracker,buffer);var removed=buffer.Window(3).Items;
        Assert.Equal("stump",Assert.Single(removed,i=>i.Kind=="plant_removed").Before);
        Assert.All(removed,i=>Assert.Null(i.After));
        Capture(tracker,buffer,Plant("tree"));Assert.Equal("plant_added",Assert.Single(buffer.Window(6).Items).Kind);
        Capture(tracker,buffer,Plant("tree"));Assert.Equal(7,buffer.Latest);
    }
    [Fact] public void FailedOrOversizedSnapshotDoesNotEraseBaselineOrInventRemovals() {
        var tracker=new SpatialJournal();var buffer=new RecentActionBuffer();Capture(tracker,buffer,Mark(0));
        IEnumerable<SpatialJournalState> Broken() {yield return Mark(1);throw new IOException("synthetic");}
        Assert.Throws<IOException>(()=>tracker.Capture(Broken(),buffer,DateTimeOffset.UnixEpoch,12,"unknown"));
        Assert.Throws<ArgumentException>(()=>tracker.Capture(Enumerable.Range(0,SpatialJournal.MaxEntries+1).Select(i=>
            new SpatialJournalState("cell/"+i,"planting_designation",null,null,i%256,i/256,3,"SyntheticCrop")),buffer,DateTimeOffset.UnixEpoch,12,"unknown"));
        Assert.Equal(0,buffer.Latest);Assert.Equal(1,tracker.Count);
        Capture(tracker,buffer,Mark(0));Assert.Equal(0,buffer.Latest);
    }
    [Theory] [InlineData(true,true,"stump")] [InlineData(true,false,"tree")] [InlineData(true,null,"tree_unknown")] [InlineData(false,null,"plant")]
    public void TreeClassificationUsesYieldRemovalNotTemplateGuess(bool tree,bool? removed,string expected)=>
        Assert.Equal(expected,SpatialJournal.PlantState(tree,removed));
    [Fact] public void SpatialChangesUseExistingFiltersOverflowAndFreshSessionBaseline() {
        var tracker=new SpatialJournal();var buffer=new RecentActionBuffer();Capture(tracker,buffer);
        tracker.Capture(Enumerable.Range(0,520).Select(i=>Mark(i)),buffer,DateTimeOffset.UnixEpoch,12,"mcp");
        var q=new NameValueCollection{{"session",Session},{"after","0"},{"limit","1"},{"kind","planting_designation"},{"source","mcp"}};
        var page=buffer.Read(RecentActionsRequest.Parse(q));Assert.True(page.Gap);Assert.Equal(8,page.Dropped);Assert.True(page.HasMore);Assert.Equal(9,page.NextCursor);
        var fresh=new RecentActionBuffer();Capture(new SpatialJournal(),fresh,Mark(0));Assert.Equal(0,fresh.Latest);
    }
    private static NativeRecentActions Data()=>new(512,DateTimeOffset.UnixEpoch,DateTimeOffset.UnixEpoch,true,0,0,4096,1,
        new([new(1,DateTimeOffset.UnixEpoch,12,"planting_designation","unknown","state_difference",null,null,1,1,3,null,"SyntheticCrop"),
             new(2,DateTimeOffset.UnixEpoch,12,"planting_designation","unknown","state_difference",null,null,2,1,3,null,"SyntheticCrop"),
             new(3,DateTimeOffset.UnixEpoch,12,"planting_designation","mcp","state_difference",null,null,2,1,3,"SyntheticCrop",null)],1,3,3,false,false,0),
        RecentActionsRequest.Kinds,[],true,0,3,SpatialJournal.MaxEntries,DateTimeOffset.UnixEpoch,DateTimeOffset.UnixEpoch);
    [Fact] public void CompactGroupsOnlySameTileHeightSourceAndChangeWithoutChangingCursor() {
        var e=new BridgeEnvelope<NativeRecentActions>(1,Session,DateTimeOffset.UnixEpoch,"0.54.0",Data());
        var compact=RecentActionsTools.Present(e,"compact");var full=RecentActionsTools.Present(e,"full");
        Assert.Equal(2,compact["data"]!["page"]!["groups"]!.AsArray().Count);
        Assert.Equal(2,compact["data"]!["page"]!["groups"]![0]!["count"]!.GetValue<int>());
        Assert.Equal(3,full["data"]!["page"]!["items"]!.AsArray().Count);
        Assert.Empty(compact["data"]!["page"]!["items"]!.AsArray());
        Assert.True(JsonNode.DeepEquals(compact["data"]!["page"]!["nextCursor"],full["data"]!["page"]!["nextCursor"]));
        var moved=e.Data.Page.Items.Select((i,n)=>i with{X=n*8,Z=n}).ToArray();
        var separate=e with{Data=e.Data with{Page=e.Data.Page with{Items=moved}}};
        Assert.Equal(3,RecentActionsTools.Present(separate,"compact")["data"]!["page"]!["groups"]!.AsArray().Count);
    }
    [Theory] [InlineData("ok")] [InlineData("coverage")] [InlineData("identity")] [InlineData("value")] [InlineData("source")]
    public async Task NewTransportValidatesCoverageAndSpatialEntryShapes(string defect) {
        var d=Data();
        if(defect=="coverage")d=d with{SpatialTrackingComplete=null};
        if(defect=="identity")d=d with{Page=d.Page with{Items=[d.Page.Items[0] with{Id=Session}]}};
        if(defect=="value")d=d with{Page=d.Page with{Items=[d.Page.Items[0] with{Before=null,After=null}]}};
        if(defect=="source")d=d with{Page=d.Page with{Items=[d.Page.Items[0] with{Source="human"}]}};
        using var client=new NativeClient(new(8081,new string('a',64)),new Reply(d));
        var r=RecentActionsRequest.Parse(new NameValueCollection{{"session",Session}});
        if(defect=="ok")Assert.Equal(3,(await client.RecentActions(r,TestContext.Current.CancellationToken)).Data.Page.Items.Length);
        else await Assert.ThrowsAsync<InvalidDataException>(()=>client.RecentActions(r,TestContext.Current.CancellationToken));
    }
    [Fact] public void ReaderDetailAndSpatialKindsAreDiscoverable() {
        var tool=RecentActionsTools.Reader();Assert.True(tool.Annotations!.ReadOnlyHint);
        Assert.Contains("plant_removed",tool.InputSchema.GetRawText());Assert.Contains("detail",tool.InputSchema.GetRawText());
        var args=JsonSerializer.SerializeToElement(new{session=Session,detail="full"});
        Assert.Equal(args.GetRawText(),ResponsePresentation.ReadRequest(RecentActionsTools.Name,args).Arguments.GetRawText());
    }
    [Theory] [InlineData("0.54.0")] [InlineData("0.55.0")] [InlineData("0.56.0")] [InlineData("0.57.0")] [InlineData("0.58.0")] [InlineData("0.59.0")] [InlineData("0.60.0")]
    public async Task SpatialCoverageRemainsRequiredAcrossVersionChange(string version) {
        var request=RecentActionsRequest.Parse(new NameValueCollection{{"session",Session}});
        using var client=new NativeClient(new(8081,new string('a',64)),new Reply(Data(),version));
        Assert.Equal(3,(await client.RecentActions(request,TestContext.Current.CancellationToken)).Data.Page.Items.Length);
        using var invalid=new NativeClient(new(8081,new string('a',64)),new Reply(Data() with{SpatialTrackingComplete=null},version));
        await Assert.ThrowsAsync<InvalidDataException>(()=>invalid.RecentActions(request,TestContext.Current.CancellationToken));
    }
    private sealed class Reply(NativeRecentActions data,string version="0.54.0"):HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)=>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(
                new BridgeEnvelope<NativeRecentActions>(1,Session,DateTimeOffset.UnixEpoch,version,data),NativeJson.Options))});
    }
}
