using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class PlantingBatchTests
{
    private static string Id(int i)=>new Guid(i,0,0,new byte[8]).ToString("D");
    private static PlantingBatchRequest Request(int count=1)=>new(){Session=Id(1),JobId=Id(2),Rectangles=Enumerable.Range(0,count)
        .Select(i=>new PlantingRectangle{X=i*8,Y=0,Z=3,Width=8,Height=8,Resource="SyntheticPlant",WorkBuildingId=Id(3)}).ToArray()};
    private sealed class Fake:IPlantingBatchPort
    {
        public int Writes,Reads;public bool BlockLast,Changed,Paused=true;public int FailAt=-1;
        public void RequirePaused(){if(!Paused)throw new BridgeRejectionException("planting_requires_pause");}
        public PlantingCell[] Observe(PlantingBatchRequest r) {
            Reads++;return r.Rectangles.SelectMany((a,i)=>Enumerable.Range(0,a.Height).SelectMany(y=>Enumerable.Range(0,a.Width).Select(x=>{
                bool blocked=BlockLast&&x==a.Width-1;
                return new PlantingCell{Rectangle=i,X=a.X+x,Y=a.Y+y,Z=a.Z,Resource=a.Resource,Existing=Changed?"OtherPlant":"unmarked",
                    NativeValid=!blocked,Reachable=true,SoilIsMoist=true,Eligible=!blocked,Result=blocked?"excluded":"not_attempted",
                    Reasons=blocked?["native_planting_blocked"]:[],Occupants=blocked?[new(){Id=Id(9),Kind="path",Template="Path"}]:[]};
            }))).ToArray();
        }
        public void Mark(PlantingCell c){Writes++;if(Writes==FailAt)throw new IOException("Synthetic failure");}
        public string ReadDesignation(PlantingCell c)=>c.Resource;
    }
    [Fact] public void EightRectanglesShareOneFreshObservationAndOneBatchReceipt() {
        var r=Request(8);var p=new Fake{BlockLast=true};var controller=new PlantingBatchController();
        var preview=controller.Handle(r,p);Assert.Equal(512,preview.Cells.Length);Assert.Equal(0,p.Writes);
        PlantingBatchContract.Validate(preview,r);
        r.Operation="apply";r.Key=preview.Key;
        var done=controller.Handle(r,p);Assert.Equal("applied",done.Outcome);Assert.Equal(448,p.Writes);Assert.Equal(2,p.Reads);
        Assert.Equal(64,done.Cells.Count(c=>c.Result=="excluded"));PlantingBatchContract.Validate(done,r);
        var again=controller.Handle(r,p);Assert.Same(done,again);Assert.Equal(448,p.Writes);Assert.Equal(2,p.Reads);
        r.Operation="status";r.Rectangles=[];r.Key="";Assert.Same(done,controller.Handle(r,p));
    }
    [Fact] public void ChangedSnapshotRejectsWholeBatchBeforeFirstMark() {
        var r=Request();var p=new Fake();var c=new PlantingBatchController();r.Key=c.Handle(r,p).Key;r.Operation="apply";p.Changed=true;
        Assert.Equal("planting_preview_changed",Assert.Throws<BridgeRejectionException>(()=>c.Handle(r,p)).Code);Assert.Equal(0,p.Writes);
    }
    [Fact] public void FailureStopsWithPartialReceiptAndNeverReplays() {
        var r=Request();var p=new Fake{FailAt=2};var c=new PlantingBatchController();r.Key=c.Handle(r,p).Key;r.Operation="apply";
        var d=c.Handle(r,p);Assert.Equal("unconfirmed",d.Outcome);Assert.Equal(2,p.Writes);Assert.Equal("marked",d.Cells[0].Result);
        Assert.Equal("unconfirmed",d.Cells[1].Result);Assert.Equal("not_attempted",d.Cells[2].Result);PlantingBatchContract.Validate(d,r);
        Assert.Same(d,c.Handle(r,p));Assert.Equal(2,p.Writes);
    }
    [Fact] public void ChangedJobParametersAndNewSessionDoNotReplay() {
        var r=Request();var p=new Fake();var c=new PlantingBatchController();r.Key=c.Handle(r,p).Key;r.Operation="apply";c.Handle(r,p);
        r.Rectangles[0].Resource="OtherPlant";Assert.Throws<BridgeRejectionException>(()=>c.Handle(r,p));Assert.Equal(64,p.Writes);
        r.Operation="status";r.Rectangles=[];r.Key="";r.Session=Id(4);Assert.Throws<BridgeRejectionException>(()=>c.Handle(r,p));
    }
    [Fact] public void PauseRequiredBeforeReadingOrWriting() {
        var p=new Fake{Paused=false};Assert.Throws<BridgeRejectionException>(()=>new PlantingBatchController().Handle(Request(),p));Assert.Equal(0,p.Reads);Assert.Equal(0,p.Writes);
    }
    [Theory] [InlineData(0)] [InlineData(9)] public void RectangleCountIsBounded(int count)=>Assert.Throws<ArgumentException>(()=>Request(count).Validate());
    [Fact] public void OverlapBoundsIdentifiersAndNullsRejected() {
        var r=Request(2);r.Rectangles[1].X=7;Assert.Throws<ArgumentException>(r.Validate);
        r=Request();r.Rectangles[0].Width=9;Assert.Throws<ArgumentException>(r.Validate);
        r=Request();r.JobId="friendly-name";Assert.Throws<ArgumentException>(r.Validate);
        r=Request();r.Rectangles=null!;Assert.Throws<ArgumentException>(r.Validate);
        r=Request();r.Rectangles[0]=null!;Assert.Throws<ArgumentException>(r.Validate);
    }
    [Fact] public void WireRoundTripAndIndependentAreaGate() {
        var r=Request(8);var q=new NameValueCollection{{"session",r.Session},{"jobId",r.JobId},{"rectangles",r.Rows()}};
        var parsed=PlantingBatchRequest.Parse("/agent-api/v1/planting-preview",q);Assert.Equal(r.Rows(),parsed.Rows());
        Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/planting-preview",false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/planting-apply",false,false,enableAreas:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/planting-apply",false,false));
        Assert.True(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/planting-apply",false,false,enableAreas:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/planting-apply",true,false,enableAreas:true));
        Assert.NotNull(BridgeRequest.Parse("/agent-api/v1/planting-preview",q).PlantingBatch);
    }
    [Fact] public void CompactKeepsCountsOccupationsAndFullHasEveryCell() {
        var r=Request();var d=new PlantingBatchController().Handle(r,new Fake{BlockLast=true});
        var e=new BridgeEnvelope<PlantingBatchReport>(1,r.Session,DateTimeOffset.UtcNow,"0.53.0",d);
        var full=PlantingBatchTools.Present(e,"full");var compact=PlantingBatchTools.Present(e,"compact");
        Assert.Equal(64,full["data"]!["cells"]!.AsArray().Count);Assert.Null(compact["data"]!["cells"]);
        Assert.True(JsonNode.DeepEquals(full["data"]!["summary"],compact["data"]!["summary"]));
        Assert.Equal("path",compact["data"]!["excludedExamples"]![0]!["occupants"]![0]!["kind"]!.GetValue<string>());
        Assert.Equal(64,d.Cells.Length);
    }
    [Fact] public void ContractRejectsMissingCellsOrInventedEligibility() {
        var r=Request();var d=new PlantingBatchController().Handle(r,new Fake());d.Cells=d.Cells.Skip(1).ToArray();Assert.Throws<InvalidDataException>(()=>PlantingBatchContract.Validate(d,r));
        d=new PlantingBatchController().Handle(r,new Fake());d.Cells[0].Reachable=null;Assert.Throws<InvalidDataException>(()=>PlantingBatchContract.Validate(d,r));
    }
    [Fact] public void CatalogAndDetailRemainConsistent() {
        Assert.DoesNotContain(PlantingBatchTools.Catalog(false),t=>t.Name==PlantingBatchTools.Apply);
        Assert.False(PlantingBatchTools.Catalog(true).Single(t=>t.Name==PlantingBatchTools.Apply).Annotations!.ReadOnlyHint);
        var args=JsonSerializer.SerializeToElement(new{session=Id(1),jobId=Id(2),detail="full"});
        Assert.Equal(args.GetRawText(),ResponsePresentation.ReadRequest(PlantingBatchTools.Status,args).Arguments.GetRawText());
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void EveryPlantingToolHasMcpObjectRootAndBoundedRectangleSchema(bool enabled) {
        var catalog=PlantingBatchTools.Catalog(enabled).ToArray();
        Assert.Equal(enabled?3:2,catalog.Length);
        foreach(var tool in catalog) {
            var schema=tool.InputSchema;
            Assert.Equal(JsonValueKind.String,schema.GetProperty("type").ValueKind);
            Assert.Equal("object",schema.GetProperty("type").GetString());
            Assert.Equal("compact",schema.GetProperty("properties").GetProperty("detail").GetProperty("default").GetString());
            if(tool.Name==PlantingBatchTools.Status)continue;
            var rectangles=schema.GetProperty("properties").GetProperty("rectangles");
            Assert.Equal(1,rectangles.GetProperty("minItems").GetInt32());
            Assert.Equal(8,rectangles.GetProperty("maxItems").GetInt32());
            Assert.Contains(rectangles.GetProperty("items").GetProperty("required").EnumerateArray(),p=>p.GetString()=="workBuildingId");
        }
    }
    [Theory]
    [InlineData(true,true,true,true,"unmarked",true)]
    [InlineData(false,true,true,true,"unmarked",false)]
    [InlineData(null,true,true,true,"unmarked",false)]
    [InlineData(true,false,true,true,"unmarked",false)]
    [InlineData(true,null,true,true,"unmarked",false)]
    [InlineData(true,true,false,true,"unmarked",false)]
    [InlineData(true,true,null,true,"unmarked",false)]
    [InlineData(true,true,false,false,"unmarked",true)]
    [InlineData(true,true,true,true,"OtherPlant",false)]
    [InlineData(true,true,true,true,"SyntheticPlant",false)]
    public void EligibilitySeparatesNativeRangeMoistureAndExistingDesignation(bool? native,bool? range,bool? moist,bool requireMoist,string existing,bool eligible) {
        var c=new PlantingCell{Resource="SyntheticPlant",Existing=existing,NativeValid=native,Reachable=range,SoilIsMoist=moist};
        PlantingEligibility.Evaluate(c,requireMoist,[]);Assert.Equal(eligible,c.Eligible);
        if(existing==c.Resource)Assert.Equal("already_marked",c.Result);
        if(range is null)Assert.Contains("work_range_unknown",c.Reasons);
    }
    [Fact] public void CompactClearanceDoesNotExposeEmptyProductionFieldsOrMutateFull() {
        var full=JsonNode.Parse("{\"clearanceRun\":{\"runId\":\"synthetic\",\"state\":\"completed\",\"pauseConfirmed\":true,\"goodTargets\":[],\"buildingProduction\":[],\"measureProduction\":false,\"observedScience\":null}}")!.AsObject();
        var compact=full.DeepClone().AsObject();ClearedGroupTools.Compact(compact);
        Assert.Null(compact["clearanceRun"]!["goodTargets"]);Assert.NotNull(full["clearanceRun"]!["goodTargets"]);
        Assert.True(compact["clearanceRun"]!["pauseConfirmed"]!.GetValue<bool>());
    }
    [Fact] public void ArgumentHintsAreFixedAndDoNotExposeArbitraryMessages() {
        Assert.Contains("UUID",ArgumentDiagnostics.Hint(new ArgumentException("canonical_uuid_required"))!);
        Assert.Contains("Rechtecke",ArgumentDiagnostics.Hint(new ArgumentException("planting_rectangles_overlap"))!);
        Assert.Null(ArgumentDiagnostics.Hint(new ArgumentException("private local path")));
    }
    [Fact] public async Task McpPreviewUsesSingleGetAndValidatesNativeEnvelope() {
        var r=Request();var h=new Handler(r);using var client=new NativeClient(new(8081,new string('a',64)),h);
        var args=JsonSerializer.SerializeToElement(new PlantingPreview(r.Session,r.JobId,r.Rectangles),NativeJson.Options);
        var response=await PlantingBatchTools.Invoke(client,PlantingBatchTools.Preview,args,false,TestContext.Current.CancellationToken);
        Assert.Equal("ok",response["status"]!.GetValue<string>());Assert.Equal(1,h.Reads);
    }
    [Fact] public async Task BadUuidAndMissingCoordinatesHaveConcreteErrorsWithoutTransport() {
        var r=Request();var h=new Handler(r);using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h),enableAreas:true);
        var bad=JsonSerializer.SerializeToElement(new PlantingPreview(r.Session,"friendly-name",r.Rectangles),NativeJson.Options);
        var response=await tools.Invoke(PlantingBatchTools.Preview,bad,TestContext.Current.CancellationToken);
        Assert.Equal("not_executed",response["error"]!["execution"]!.GetValue<string>());Assert.Contains("UUID",response["error"]!["message"]!.GetValue<string>());
        var missing=JsonSerializer.SerializeToNode(new PlantingPreview(r.Session,r.JobId,r.Rectangles),NativeJson.Options)!.AsObject();missing["rectangles"]![0]!.AsObject().Remove("x");
        response=await tools.Invoke(PlantingBatchTools.Preview,JsonSerializer.SerializeToElement(missing),TestContext.Current.CancellationToken);
        Assert.Contains("x, y, z",response["error"]!["message"]!.GetValue<string>());Assert.Equal(0,h.Reads);
    }
    [Fact] public void CompactSkipsMatchingOnlyCellsButKeepsAdditionalBlockers() {
        var r=Request();var d=new PlantingBatchController().Handle(r,new Fake());
        foreach(var cell in d.Cells) {
            cell.Eligible=false;cell.Existing=cell.Resource;cell.Result="already_marked";cell.Reasons=["already_marked"];
        }
        var e=new BridgeEnvelope<PlantingBatchReport>(1,r.Session,DateTimeOffset.UtcNow,"0.53.0",d);
        var compact=PlantingBatchTools.Present(e,"compact");
        Assert.Empty(compact["data"]!["excludedExamples"]!.AsArray());
        Assert.False(compact["data"]!["excludedExamplesTruncated"]!.GetValue<bool>());
        foreach(var cell in d.Cells.Take(9)) {cell.Reachable=false;cell.Reasons=["already_marked","outside_work_range"];}
        compact=PlantingBatchTools.Present(e,"compact");var full=PlantingBatchTools.Present(e,"full");
        Assert.Equal(8,compact["data"]!["excludedExamples"]!.AsArray().Count);
        Assert.True(compact["data"]!["excludedExamplesTruncated"]!.GetValue<bool>());
        Assert.Equal(64,full["data"]!["cells"]!.AsArray().Count);
        Assert.True(JsonNode.DeepEquals(compact["data"]!["summary"],full["data"]!["summary"]));
    }
    [Theory]
    [InlineData("preview_changed","not_executed")]
    [InlineData("other_conflict","unconfirmed")]
    [InlineData("malformed","unconfirmed")]
    [InlineData("lost","unconfirmed")]
    public async Task OnlyConfirmedStalePreviewRejectionProvesNoPlanting(string mode,string execution) {
        var r=Request();using var h=new RejectingHandler(mode);
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),h),enableAreas:true);
        var args=JsonSerializer.SerializeToElement(new PlantingApply(r.Session,r.JobId,r.Rectangles,new string('a',64)),NativeJson.Options);
        var result=await tools.Invoke(PlantingBatchTools.Apply,args,TestContext.Current.CancellationToken);
        Assert.Equal(execution,result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());Assert.Equal(1,h.Calls);
        if(mode=="preview_changed") {
            Assert.Equal("planting_preview_changed",result["error"]!["code"]!.GetValue<string>());
            Assert.Contains("native Vorprüfung",result["error"]!["message"]!.GetValue<string>());
        }
    }
    private sealed class RejectingHandler(string mode):HttpMessageHandler {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Calls++;Assert.Equal(HttpMethod.Post,request.Method);
            if(mode=="lost")throw new HttpRequestException();
            string body=mode=="preview_changed"?"{\"error\":\"planting_preview_changed\"}":
                mode=="other_conflict"?"{\"error\":\"state_conflict\"}":"{\"error\":\"planting_preview_changed\",\"unexpected\":true}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict){Content=new StringContent(body)});
        }
    }
    private sealed class Handler(PlantingBatchRequest r):HttpMessageHandler
    {
        public int Reads;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Assert.Equal(HttpMethod.Get,request.Method);Assert.StartsWith("/agent-api/v1/planting-preview?",request.RequestUri!.PathAndQuery);Reads++;
            var data=new PlantingBatchController().Handle(r,new Fake());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new BridgeEnvelope<PlantingBatchReport>(1,r.Session,DateTimeOffset.UtcNow,"0.53.0",data),NativeJson.Options),Encoding.UTF8,"application/json")});
        }
    }
}
