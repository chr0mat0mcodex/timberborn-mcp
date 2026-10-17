using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class BuildingQueryTests
{
    private const string Session="11111111-1111-1111-1111-111111111111";
    private const string Id="22222222-2222-2222-2222-222222222222";
    private const string Missing="33333333-3333-3333-3333-333333333333";
    private static NameValueCollection Query()=>new(){["offset"]="0",["limit"]="32"};
    private static BuildingQueryRequest Search(NameValueCollection q)=>BuildingQueryRequest.Parse("/agent-api/v1/building-search",q);
    private static BuildingQueryRequest Batch(string ids)=>BuildingQueryRequest.Parse("/agent-api/v1/building-operations",new(){["session"]=Session,["ids"]=ids});
    private static NativeOperation Operation()=>new(Id,"SyntheticFactory.IronTeeth",true,false,new(1,1,1,false,false,true),
        new(true,"SyntheticRecipe",true,true,true,false,true,0),[],[],[],new(true,true,false,true,0,50,true));

    [Fact]
    public void FiltersAreConjunctiveAndBoundsUseHalfOpenAnchorCoordinates()
    {
        var q=Query();q["productionOnly"]="true";q["finished"]="true";q["template"]="SyntheticFactory.IronTeeth";
        q["x"]="10";q["y"]="20";q["z"]="4";q["width"]="3";q["height"]="2";q["depth"]="2";
        var r=Search(q);
        Assert.True(r.Matches("SyntheticFactory.IronTeeth",true,false,true,10,20,4));
        Assert.True(r.Matches("SyntheticFactory.IronTeeth",true,false,true,12,21,5));
        Assert.False(r.Matches("SyntheticFactory.IronTeeth",true,false,true,13,21,5));
        Assert.False(r.Matches("SyntheticFactory.IronTeeth",true,false,true,12,21,6));
        Assert.False(r.Matches("SyntheticFactory.Folktails",true,false,true,10,20,4));
        Assert.False(r.Matches("SyntheticFactory.IronTeeth",false,false,true,10,20,4));
        Assert.False(r.Matches("SyntheticFactory.IronTeeth",true,false,false,10,20,4));
        Assert.False(Search(Query()).Matches("Path",true,true,false,0,0,0));
        q=Query();q["includePaths"]="true";Assert.True(Search(q).Matches("Path",true,true,false,0,0,0));
    }
    [Theory]
    [InlineData("x","1")][InlineData("limit","33")][InlineData("finished","yes")]
    [InlineData("detail","huge")][InlineData("template","")][InlineData("unknown","1")]
    public void InvalidSearchRejected(string key,string value)
    {
        var q=Query();q[key]=value;Assert.Throws<ArgumentException>(()=>Search(q));
    }
    [Fact]
    public void DuplicateArgumentsAndDuplicateOrTooManyIdsAreRejected()
    {
        var q=Query();q.Add("offset","0");Assert.Throws<ArgumentException>(()=>Search(q));
        Assert.Throws<ArgumentException>(()=>Batch(Id+","+Id));
        Assert.Throws<ArgumentException>(()=>Batch(""));
        Assert.Throws<ArgumentException>(()=>Batch(string.Join(",",Enumerable.Range(0,33).Select(_=>Guid.NewGuid()))));
        Assert.Equal(32,Batch(string.Join(",",Enumerable.Range(0,32).Select(_=>Guid.NewGuid()))).Ids.Length);
        foreach(var path in new[]{"building-search","building-operations"}) {
            Assert.True(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/"+path,false,false));
            Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/"+path,false,true));
        }
    }
    [Theory]
    [InlineData(false)][InlineData(true)]
    public async Task BulkIsOneRequestAndCompactOmitsDetailedInventory(bool full)
    {
        var data=new OperationBatch([new(Id,"observed",Operation() with {Water=new(true,true,true,true,0,true,false)}),new(Missing,"not_found",null)],1,[]);
        using var handler=new Reply(data);using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var result=await BuildingQueryTools.Invoke(client,"inspect_building_operations",JsonSerializer.SerializeToElement(new{ids=new[]{Id,Missing},session=Session,detail=full?"full":"compact"}),TestContext.Current.CancellationToken);
        Assert.Equal(1,handler.Calls);Assert.Contains("building-operations?",handler.Url);
        var items=result["data"]!["items"]!;
        Assert.Equal("not_found",items[1]!["state"]!.GetValue<string>());Assert.Null(items[1]!["summary"]);
        Assert.False(items[0]!["summary"]!["assessment"]!["productionProven"]!.GetValue<bool>());
        Assert.Equal(full,items[0]!["details"] is not null);
        Assert.Null(items[0]!["summary"]!["inventories"]);
        Assert.True(items[0]!["summary"]!["water"]!["flooded"]!.GetValue<bool>());
        Assert.Contains(items[0]!["summary"]!["assessment"]!["blockers"]!.AsArray(),n=>n!.GetValue<string>()=="building_flooded");
    }
    [Theory]
    [InlineData("comma")][InlineData("duplicate")][InlineData("detail")]
    public async Task InvalidMcpArgumentsNeverReachTransport(string defect)
    {
        using var handler=new Reply(new{});using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var args=JsonSerializer.SerializeToElement(new{ids=defect=="comma"?new[]{Id+","+Missing}:defect=="duplicate"?new[]{Id,Id}:new[]{Id},session=Session,detail=defect=="detail"?"invalid":"compact"});
        var parsed=ResponsePresentation.ReadRequest("inspect_building_operations",args);
        await Assert.ThrowsAsync<ArgumentException>(()=>BuildingQueryTools.Invoke(client,"inspect_building_operations",parsed.Arguments,TestContext.Current.CancellationToken));
        Assert.Equal(0,handler.Calls);
    }
    [Theory]
    [InlineData("session")][InlineData("order")][InlineData("missing")][InlineData("correlation")][InlineData("power")]
    public async Task RejectsUncorrelatedOrIncompleteBatch(string defect)
    {
        var data=new OperationBatch([new(Id,"observed",Operation()),new(Missing,"not_found",null)],1,[]);
        if(defect=="order")data=data with{Items=data.Items.Reverse().ToArray()};
        if(defect=="missing")data=data with{Items=[data.Items[0]]};
        if(defect=="correlation")data=data with{Items=[new(Id,"observed",Operation() with{Id=Missing}),data.Items[1]]};
        if(defect=="power")data=data with{Items=[new(Id,"observed",Operation() with{Power=Operation().Power! with{Active=null}}),data.Items[1]]};
        using var handler=new Reply(data,defect=="session"?Missing:Session);using var client=new NativeClient(new(8081,new string('a',64)),handler);
        await Assert.ThrowsAsync<InvalidDataException>(()=>client.BuildingOperations(Batch(Id+","+Missing),TestContext.Current.CancellationToken));
        Assert.Equal(1,handler.Calls);
    }
    [Theory]
    [InlineData("ok")][InlineData("full")][InlineData("filter")][InlineData("count")][InlineData("detail")]
    public async Task SearchValidatesFilteredPageAndExplicitGeometryAvailability(string defect)
    {
        var q=Query();q["productionOnly"]="true";
        if(defect=="full")q["detail"]="full";
        var item=new BuildingMatch(Guid.Parse(Id),"SyntheticFactory.IronTeeth",new(1,2,3),true,false,defect!="filter",
            defect is "detail" or "full"?new("Cw0",null,[new(1,2,3)],false):null);
        var data=new BuildingSearch(0,32,defect=="count"?2:1,[item],false,1,[]);
        using var handler=new Reply(data);using var client=new NativeClient(new(8081,new string('a',64)),handler);
        if(defect is "ok" or "full")Assert.Single((await client.SearchBuildings(Search(q),TestContext.Current.CancellationToken)).Data.Items);
        else await Assert.ThrowsAsync<InvalidDataException>(()=>client.SearchBuildings(Search(q),TestContext.Current.CancellationToken));
        Assert.Equal(1,handler.Calls);
    }
    [Fact]
    public void InactiveConsumersAreNotReportedAsPowerSupplyFailures()
    {
        var inactive=Operation() with{Power=new(true,true,false,true,0,0,false)};
        var report=OperationAssessmentBuilder.Read(inactive);
        Assert.DoesNotContain("consumer_not_powered",report.Blockers);Assert.Contains("consumer_inactive",report.Warnings);
        report=OperationAssessmentBuilder.Read(inactive with{Power=inactive.Power! with{Active=null}});
        Assert.DoesNotContain("consumer_not_powered",report.Blockers);Assert.Contains("consumer_activity_unknown",report.Unknown);
        Assert.Contains("consumer_not_powered",OperationAssessmentBuilder.Read(Operation()).Blockers);
    }
    private sealed class Reply(object data,string session=Session):HttpMessageHandler
    {
        public int Calls;public string Url="";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct)
        {
            Calls++;Url=r.RequestUri!.AbsoluteUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(
                JsonSerializer.Serialize(new BridgeEnvelope<object>(1,session,DateTimeOffset.UnixEpoch,"0.39.0",data),NativeJson.Options))});
        }
    }
}
