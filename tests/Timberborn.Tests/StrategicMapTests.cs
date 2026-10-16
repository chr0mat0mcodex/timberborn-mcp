using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class StrategicMapTests
{
    private const string Session="11111111-1111-4111-8111-111111111111";
    private static readonly DateTimeOffset Time=DateTimeOffset.Parse("2026-01-01T00:00:00Z");
    private static StrategicColumn[] Grid(int width,int height,Func<int,int,StrategicSpan[]> spans) =>
        Enumerable.Range(0,height).SelectMany(y=>Enumerable.Range(0,width).Select(x=>new StrategicColumn(x,y,spans(x,y)))).ToArray();

    [Fact]
    public void NarrowInletFindsPassAndConcreteCrossSection()
    {
        var cells=Grid(7,5,(x,y)=>[new(y==2 ? x==3 ? 2 : 1 : 6,8,x==0&&y==2?3:0,x==0&&y==2?1:0)]);
        var result=StrategicMapAnalysis.Analyze(new(7,5,8),cells,new(6,2,1,1,1),4,TestContext.Current.CancellationToken);
        var route=Assert.Single(result.Corridors,r=>r.Kind=="observed_water_to_zone");
        Assert.Equal(2,route.MinimumTerrainPassLevel);
        Assert.True(route.LevelReachesPass);
        Assert.Equal(new Position(3,2,2),route.Candidate.At);
        Assert.True(route.Candidate.BothTerrainBanksFound);
        Assert.Equal(new ProtectionCell(new(3,2,2),3),Assert.Single(route.Candidate.CrossSection));
        Assert.Equal(new Position(6,2,1),route.Path[^1]);
    }

    [Fact]
    public void LowerBypassWinsOverDirectHighRidge()
    {
        var cells=Grid(5,3,(x,y)=>[new(x==2&&y!=0?5:1,8,x==0&&y==1?2:0,0)]);
        var result=StrategicMapAnalysis.Analyze(new(5,3,8),cells,new(4,1,1,1,1),null,TestContext.Current.CancellationToken);
        var route=Assert.Single(result.Corridors,r=>r.Kind=="observed_water_to_zone");
        Assert.Equal(1,route.MinimumTerrainPassLevel);
        Assert.False(route.PathTruncated);
        Assert.Equal(new Position(0,1,1),route.Path[0]);
        Assert.Equal(new Position(4,1,1),route.Path[^1]);
        Assert.All(route.Path,p=>Assert.Equal(1,p.Z));
        // Path contains turning points: the bypass cell may lie inside a straight segment.
        var segments=route.Path.Zip(route.Path.Skip(1),(a,b)=>(Start:a,End:b)).ToArray();
        Assert.All(segments,s=>Assert.True(s.Start.X==s.End.X || s.Start.Y==s.End.Y));
        Assert.Contains(segments,s=>s.Start.Y==0 && s.End.Y==0 &&
            Math.Min(s.Start.X,s.End.X)<=2 && Math.Max(s.Start.X,s.End.X)>=2);
    }

    [Fact]
    public void RoofDoesNotHideTunnelButSolidWallDisconnectsIt()
    {
        var cells=Grid(3,1,(x,_)=>x==1?[new(1,3,0,0),new(6,8,0,0)]:[new(1,8,x==0?1:0,0)]);
        var result=StrategicMapAnalysis.Analyze(new(3,1,8),cells,new(2,0,1,1,1),null,TestContext.Current.CancellationToken);
        Assert.Equal(1,result.CaveColumns);
        Assert.Contains(result.Corridors,r=>r.Kind=="observed_water_to_zone"&&r.MinimumTerrainPassLevel==1);
        cells[1]=new(1,0,[]);
        var blocked=StrategicMapAnalysis.Analyze(new(3,1,8),cells,new(2,0,1,1,1),null,TestContext.Current.CancellationToken);
        Assert.DoesNotContain(blocked.Corridors,r=>r.Origin.X==0);
        Assert.Contains("no_safe_or_unreachable_claim_from_missing_route",blocked.Limitations);
    }

    [Fact]
    public void DisjointVerticalIntervalsAreNotConnectedThroughSolidFloor()
    {
        var cells=Grid(3,1,(x,_)=>x==0?[new(0,2,1,0)]:x==1?[new(3,5,0,0)]:[new(3,6,0,0)]);
        var report=StrategicMapAnalysis.Analyze(new(3,1,6),cells,new(2,0,3,1,1),4,TestContext.Current.CancellationToken);
        Assert.DoesNotContain(report.Corridors,r=>r.Origin.X==0);
    }

    [Fact]
    public void PartialTerrainCannotRecommendProtection()
    {
        var result=StrategicMapAnalysis.Analyze(new(3,1,8),[new(0,0,[new(1,8,1,0)])],new(0,0,1,1,1),null,TestContext.Current.CancellationToken);
        Assert.Equal("incomplete_terrain_no_recommendations",result.State);
        Assert.Empty(result.Corridors);
    }

    [Fact]
    public void DrySourceIsReportedAsTemplateHintWithoutInventedWaterLevel()
    {
        var grid=Grid(5,3,(_,_)=>[new(1,8,0,0)]);
        var source=new StrategicObject(Guid.Parse("22222222-2222-4222-8222-222222222222"),"WaterSource",2,1,1,"special_or_unknown",true);
        var report=StrategicMapAnalysis.Analyze(new(5,3,8),grid,new(4,1,1,1,1),null,TestContext.Current.CancellationToken,objects:[source]);
        var route=Assert.Single(report.Corridors,r=>r.Kind=="source_template_hint_to_zone");
        Assert.Null(route.ObservedWaterLevel); Assert.Null(route.LevelReachesPass);
        Assert.Equal("terrain_pass_plus_one_level_not_forecast",route.Candidate.CrestBasis);
    }

    [Fact]
    public void SourceHintDoesNotLeakThroughRoofIntoAnotherVoid()
    {
        var grid=Grid(3,3,(x,y)=>x==1&&y==1?[new(1,3,0,0),new(5,8,0,0)]:[new(1,8,0,0)]);
        var source=new StrategicObject(Guid.Parse("22222222-2222-4222-8222-222222222222"),"WaterSource",1,1,1,"special_or_unknown",true);
        var report=StrategicMapAnalysis.Analyze(new(3,3,8),grid,new(2,1,1,1,1),null,TestContext.Current.CancellationToken,objects:[source]);
        var route=Assert.Single(report.Corridors,r=>r.Kind=="source_template_hint_to_zone");
        Assert.Equal(new Position(1,1,1),route.Origin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(3f)]
    public void BarrierCrestCannotBeBelowItsTerrainPass(float? scenarioLevel)
    {
        var grid=Grid(3,1,(x,_)=>[new(x==1?5:1,8,x==0?1:0,0)]);
        var report=StrategicMapAnalysis.Analyze(new(3,1,8),grid,new(2,0,1,1,1),scenarioLevel,TestContext.Current.CancellationToken);
        var route=Assert.Single(report.Corridors,r=>r.Kind=="observed_water_to_zone");
        Assert.Equal(5,route.MinimumTerrainPassLevel);
        Assert.False(route.LevelReachesPass);
        Assert.Equal(6,route.Candidate.AssumedBarrierCrest);
        Assert.Equal("terrain_pass_plus_one_level_above_lower_reference_not_forecast",route.Candidate.CrestBasis);
        Assert.All(route.Candidate.CrossSection,c=>Assert.True(c.HeightToCrest>0));
    }

    private static BridgeEnvelope<StrategicTile> Tile(int index, string session=Session)
    {
        int x=index*32;
        var columns=Grid(32,2,(_,_)=>[new(1,8,0,0)]).Select(c=>c with{X=c.X+x}).ToArray();
        return new(1,session,Time.AddSeconds(index),"0.62.0",
            new(index,2,new(64,2,8),x,0,32,2,columns,[],true,true,[]));
    }
    private static StrategicScanRequest Request => new(Session,1,0,1,1,1,TilesPerCall:1);

    [Fact]
    public async Task ScanResumesAndDoesNotPretendPartialCoverageIsComplete()
    {
        var store=new StrategicScanStore(); var calls=new List<int>();
        Task<BridgeEnvelope<StrategicTile>> Read(string _,int i,CancellationToken token) {calls.Add(i);return Task.FromResult(Tile(i));}
        var first=await store.Observe(Request,Read,TestContext.Current.CancellationToken);
        Assert.False(first.Coverage.TerrainComplete); Assert.Null(first.Analysis); Assert.Equal(1,first.Coverage.NextTile);
        var last=await store.Observe(Request with{ScanId=first.ScanId},Read,TestContext.Current.CancellationToken);
        Assert.True(last.Coverage.TerrainComplete); Assert.False(last.Coverage.Atomic);
        Assert.Equal(128,last.Coverage.ObservedColumns); Assert.NotNull(last.Analysis);
        Assert.Equal(new[]{0,1},calls);
        var cached=await store.Observe(Request with{ScanId=first.ScanId},Read,TestContext.Current.CancellationToken);
        Assert.Equal(0,cached.NativeReadsThisCall); Assert.Equal(2,calls.Count);
    }

    [Fact]
    public async Task FailedTileDoesNotAdvanceScanAndChangedTargetIsRejected()
    {
        var store=new StrategicScanStore();
        var first=await store.Observe(Request,(_,i,_)=>Task.FromResult(Tile(i)),TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<InvalidDataException>(()=>store.Observe(Request with{ScanId=first.ScanId},
            (_,i,_)=>Task.FromResult(Tile(i) with{Data=Tile(i).Data with{Columns=[]}}),TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(()=>store.Observe(Request with{ScanId=first.ScanId,X=2},
            (_,i,_)=>Task.FromResult(Tile(i)),TestContext.Current.CancellationToken));
        var last=await store.Observe(Request with{ScanId=first.ScanId},(_,i,_)=>Task.FromResult(Tile(i)),TestContext.Current.CancellationToken);
        Assert.True(last.Coverage.TerrainComplete);
    }

    [Fact]
    public async Task StaleSessionNeverAddsPage()
    {
        var store=new StrategicScanStore();
        await Assert.ThrowsAsync<BridgeRejectionException>(()=>store.Observe(Request,
            (_,i,_)=>Task.FromResult(Tile(i,"33333333-3333-4333-8333-333333333333")),TestContext.Current.CancellationToken));
    }

    [Fact]
    public void TileContractRejectsDuplicateColumnsAndOverlappingVoids()
    {
        var tile=Tile(0).Data;
        var duplicate=tile.Columns.ToArray(); duplicate[1]=duplicate[0];
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateStrategicTile(tile with{Columns=duplicate},0));
        var overlap=tile.Columns.ToArray(); overlap[0]=overlap[0] with{Spans=[new(1,4,0,0),new(3,8,0,0)]};
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateStrategicTile(tile with{Columns=overlap},0));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("256")]
    [InlineData("1&tile=2")]
    public void TileArgumentsAreBounded(string value)
    {
        Assert.Throws<ArgumentException>(()=>StrategicMapRequest.Parse(new NameValueCollection{{"session",Session},{"tile",value}}));
    }

    [Fact]
    public async Task OverviewWithoutTargetSupportsPagingAndDoesNotInventMissingWeather()
    {
        var store=new StrategicScanStore();var request=new StrategicScanRequest(Session,OverviewOnly:true,TilesPerCall:2);
        var report=await store.Observe(request,(_,i,_)=>Task.FromResult(Tile(i)),TestContext.Current.CancellationToken);
        Assert.True(report.Coverage.TerrainComplete);Assert.Null(report.Analysis);Assert.NotNull(report.Rivers);
        Assert.Null(report.Rivers.LastWeather);Assert.False(report.Rivers.SourcesComplete);
        var page=await store.Observe(request with{ScanId=report.ScanId,SectionOffset=32,LinkOffset=64,SourceOffset=16},
            (_,i,_)=>Task.FromResult(Tile(i)),TestContext.Current.CancellationToken);
        Assert.Equal(0,page.NativeReadsThisCall);Assert.Equal(32,page.Rivers!.SectionOffset);
    }

    [Fact]
    public void SourceAndWeatherContractRejectsNonfiniteOrUnprovenData()
    {
        var tile=Tile(0).Data;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateStrategicTile(tile with{SourcesComplete=true},0));
        var source=new StrategicSource(Guid.Parse("22222222-2222-4222-8222-222222222222"),new(1,0,1),[new(1,0,1)],false,
            1,float.NaN,0,false,null,[]);
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateStrategicTile(tile with{Sources=[source]},0));
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateStrategicTile(tile with{
            Weather=new(1,1,float.NaN,false,"Drought",3,4,4,7)},0));
    }

    [Fact]
    public void ReaderIsExposedWithoutWriteFlagsAndUsesValidatedSchema()
    {
        var tool=StrategicMapTools.Reader();
        Assert.Equal("object",tool.InputSchema.GetProperty("type").GetString());
        Assert.True(tool.Annotations!.ReadOnlyHint); Assert.False(tool.Annotations.DestructiveHint);
        Assert.Equal(8,tool.InputSchema.GetProperty("properties").GetProperty("tilesPerCall").GetProperty("maximum").GetInt32());
        Assert.Equal(32768,tool.InputSchema.GetProperty("properties").GetProperty("linkOffset").GetProperty("maximum").GetInt32());
        Assert.DoesNotContain(tool.InputSchema.GetProperty("required").EnumerateArray(),p=>p.GetString()=="x");
        Assert.Equal("strategic-map",BridgeRequest.Parse("/agent-api/v1/strategic-map",new NameValueCollection{{"session",Session},{"tile","0"}}).Route);
        Assert.Throws<ArgumentException>(()=>StrategicScanStore.Validate(Request with{ScenarioWaterLevel=float.NaN}));
        Assert.Contains(NativeTools.Catalog(),t=>t.Name==StrategicMapTools.Name);
        Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/strategic-map",false,false));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/strategic-map",false,false));
        Assert.Equal("known",BuildingCapabilityTools.Describe("0.62.0","Folktails",true).ProfileState);
    }

    [Fact]
    public async Task FullMcpRouteUsesOnlyReadsAtSpeedSevenAndReturnsConcreteStaleHandleError()
    {
        var handler=new Handler(); using var client=new NativeClient(new(12345,new string('a',64)),handler);
        using var tools=new NativeTools(client);
        var args=JsonSerializer.SerializeToElement(Request,BuildBatchState.Json);
        var result=await tools.Invoke(StrategicMapTools.Name,args,TestContext.Current.CancellationToken);
        Assert.Equal("ok",result["status"]!.GetValue<string>());
        Assert.Equal("scanning",result["data"]!["state"]!.GetValue<string>());
        Assert.Equal(new[]{"/agent-api/v1/simulation","/agent-api/v1/strategic-map"},handler.Paths);
        args=JsonSerializer.SerializeToElement(Request with{ScanId="44444444-4444-4444-8444-444444444444"},BuildBatchState.Json);
        result=await tools.Invoke(StrategicMapTools.Name,args,TestContext.Current.CancellationToken);
        Assert.Equal("strategic_scan_missing_or_expired_start_new_scan",result["error"]!["code"]!.GetValue<string>());
    }
    private sealed class Handler:HttpMessageHandler
    {
        public List<string> Paths {get;}=[];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Assert.Equal(HttpMethod.Get,request.Method); Paths.Add(request.RequestUri!.AbsolutePath);
            object envelope=request.RequestUri.AbsolutePath.EndsWith("simulation",StringComparison.Ordinal)
                ? new BridgeEnvelope<NativeSimulation>(1,Session,Time,"0.62.0",new(7,1,0.5f,12)) : Tile(0);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(envelope,NativeJson.Options))});
        }
    }
}
