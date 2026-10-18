using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
using Cell = Timberborn.Bridge.Core.ParallelConstructionPolicy.Cell;

namespace Timberborn.Tests;

public sealed class ParallelConstructionTests
{
    [Fact]
    public async Task PriorityWriteUsesOnePostWithRevisionAndValidatedReadback()
    {
        var request=PriorityRequest();using var handler=new PriorityHandler();
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var result=await client.LargeProject(request,TestContext.Current.CancellationToken);
        Assert.Equal(1,handler.Calls);Assert.Equal("applied",result.Data.GetProperty("outcome").GetString());
    }
    private sealed class PriorityHandler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;Assert.Equal(HttpMethod.Post,request.Method);
            Assert.Equal("/agent-api/v1/large-project-set-priority",request.RequestUri!.AbsolutePath);
            Assert.Contains("expectedRevision="+new string('a',64),request.RequestUri.Query);
            Assert.Contains("priority=VeryHigh",request.RequestUri.Query);
            var data=Receipt();data["limitations"]=new JsonArray("synthetic_test");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {Content=new StringContent(JsonSerializer.Serialize(new {
                schemaVersion=1,sessionId="11111111-1111-1111-1111-111111111111",observedAtUtc=DateTimeOffset.UtcNow,bridgeVersion="0.74.0",data
            }))});
        }
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PriorityPartialFailureStopsRemainingWritesWithoutRollback(bool throws)
    {
        var values=new[]{"Normal","Low","Normal"};var writes=new List<int>();
        var result=ProjectPriorityChange.Apply(values,"High",i=>values[i],(i,v)=> {
            writes.Add(i);if(i==1){if(throws)throw new InvalidOperationException();return;}values[i]=v;
        });
        Assert.Equal(new[]{0,1},writes);Assert.Equal(new[]{"applied","unconfirmed","not_attempted"},result);
        Assert.Equal("High",values[0]);Assert.Equal("Normal",values[2]);
    }

    [Fact]
    public void PriorityFinishedAndUnchangedPartsDoNotWriteAndInvalidTargetRejectsWholeChange()
    {
        int writes=0;
        Assert.Equal(new[]{"skipped","unchanged"},ProjectPriorityChange.Apply([null,"High"],"High",_=>throw new InvalidOperationException(),(_,_)=>writes++));
        Assert.Throws<ArgumentException>(()=>ProjectPriorityChange.Apply(["Normal"],"Urgent",_=>"Normal",(_,_)=>writes++));
        Assert.Equal(0,writes);
    }
    [Fact]
    public void SeparationIncludesDiagonalWorkCellsAndDoesNotInferVerticalSafety()
    {
        Assert.True(ParallelConstructionPolicy.Separated([new Cell(12,10)],[new Cell(10,10)]));
        Assert.False(ParallelConstructionPolicy.Separated([new Cell(11,11)],[new Cell(10,10)]));
        Assert.False(ParallelConstructionPolicy.Separated([new Cell(10,10)],[new Cell(10,10)]));
        Assert.False(ParallelConstructionPolicy.Separated([], [new Cell(10,10)]));
        Assert.False(ParallelConstructionPolicy.Separated([new Cell(10,10)], []));
    }

    [Fact]
    public void DistantChokePointAndLossOfOneAlternativeAccessStillBlock()
    {
        Assert.True(ParallelConstructionPolicy.Separated([new Cell(1,1)],[new Cell(20,20)]));
        Assert.False(ParallelConstructionPolicy.Preserved([true,true],[true,true],[true,false]));
        Assert.False(ParallelConstructionPolicy.Preserved([true],[true],[false]));
        Assert.True(ParallelConstructionPolicy.Preserved([true,false],[true,false],[true,true]));
    }

    [Fact]
    public void UnknownEmptyMismatchedAndDisconnectedEvidenceNeverPasses()
    {
        Assert.False(ParallelConstructionPolicy.Preserved([],[],[]));
        Assert.False(ParallelConstructionPolicy.Preserved([false],[false],[true]));
        Assert.False(ParallelConstructionPolicy.Preserved([true],[false],[true]));
        Assert.False(ParallelConstructionPolicy.Preserved([true],[true],[]));
    }

    [Theory]
    [InlineData("construction","construction",true)]
    [InlineData("construction","finished",true)]
    [InlineData("construction","construction_unreachable",false)]
    [InlineData("construction","construction_access_unknown",false)]
    [InlineData("finished","identity_mismatch",false)]
    [InlineData("pending","missing",true)]
    [InlineData("pending","construction",false)]
    [InlineData("unconfirmed","missing",false)]
    [InlineData("unconfirmed","construction",false)]
    [InlineData("unknown","finished",false)]
    public void OldUnconfirmedOrdersNeverGainParallelPermission(string recorded,string observed,bool expected)
        => Assert.Equal(expected,ParallelConstructionPolicy.KnownPart(recorded,observed));

    [Fact]
    public void TwoAdmittedProjectsDispatchOnceAndRetainIndependentCompletion()
    {
        var ledger=new LargeProjectLedger();var a=Guid.NewGuid().ToString("D");var b=Guid.NewGuid().ToString("D");
        int writesA=0,writesB=0;
        ledger.Start(a,"key","a",[0],[Guid.NewGuid().ToString("D")]);
        ledger.Advance(a,0,()=>true,_=>"construction",_=>true,_=>writesA++);
        ledger.Advance(a,1,()=>true,_=>"construction",_=>true,_=>writesA++);
        Assert.Throws<BridgeRejectionException>(()=>ledger.Start(b,"key","b",[0],[Guid.NewGuid().ToString("D")]));
        ledger.Start(b,"key","b",[0],[Guid.NewGuid().ToString("D")],allowConcurrent:true);
        ledger.Advance(b,1,()=>true,_=>"construction",_=>true,_=>writesB++);
        ledger.Advance(b,2,()=>true,_=>"finished",_=>true,_=>writesB++);
        Assert.Equal("completed",ledger.Inspect(b).State);
        Assert.Equal("waiting",ledger.Inspect(a).State);
        ledger.Advance(a,3,()=>false,_=>"construction",_=>true,_=>writesA++);
        Assert.Equal("stopped",ledger.Inspect(a).State);
        Assert.Equal(1,writesA);Assert.Equal(1,writesB);
    }

    private static LargeProjectRequest PriorityRequest(bool write=true)=>LargeProjectRequest.Parse(
        "/agent-api/v1/large-project-"+(write?"set-priority":"priority"),write?new NameValueCollection {
            ["session"]="11111111-1111-1111-1111-111111111111",["actionId"]="22222222-2222-2222-2222-222222222222",
            ["priority"]="VeryHigh",["expectedRevision"]=new string('a',64)
        }:new NameValueCollection { ["session"]="11111111-1111-1111-1111-111111111111",["actionId"]="22222222-2222-2222-2222-222222222222" });

    private static JsonObject Receipt()=>JsonSerializer.SerializeToNode(new {
        actionId="22222222-2222-2222-2222-222222222222",revision=new string('b',64),previousRevision=new string('a',64),outcome="applied",
        items=new[]{new {index=0,id="33333333-3333-3333-3333-333333333333",state="construction",previousPriority="Normal",priority="VeryHigh",outcome="applied"}}
    })!.AsObject();

    [Fact]
    public void PriorityContractRejectsStaleRevisionAndFalseSuccess()
    {
        var r=PriorityRequest();var receipt=Receipt();
        ProjectPriorityContract.Validate(JsonSerializer.SerializeToElement(receipt),r);
        receipt["previousRevision"]=new string('c',64);
        Assert.Throws<InvalidDataException>(()=>ProjectPriorityContract.Validate(JsonSerializer.SerializeToElement(receipt),r));
        receipt=Receipt();receipt["items"]![0]!["priority"]="Normal";
        Assert.Throws<InvalidDataException>(()=>ProjectPriorityContract.Validate(JsonSerializer.SerializeToElement(receipt),r));
        receipt=Receipt();receipt["items"]![0]!["outcome"]="unconfirmed";
        Assert.Throws<InvalidDataException>(()=>ProjectPriorityContract.Validate(JsonSerializer.SerializeToElement(receipt),r));
        receipt["outcome"]="partial_unconfirmed";
        ProjectPriorityContract.Validate(JsonSerializer.SerializeToElement(receipt),r);
    }

    [Fact]
    public void PriorityCatalogRequiresExplicitRevisionAndPlacementSwitch()
    {
        Assert.Contains(LargeProjectTools.Catalog(false),t=>t.Name=="inspect_large_project_priority");
        Assert.DoesNotContain(LargeProjectTools.Catalog(false),t=>t.Name=="set_large_project_priority");
        var setter=Assert.Single(LargeProjectTools.Catalog(true),t=>t.Name=="set_large_project_priority");
        Assert.False(setter.Annotations!.ReadOnlyHint);
        Assert.Contains("expectedRevision",setter.InputSchema.GetProperty("required").EnumerateArray().Select(x=>x.GetString()));
        Assert.True(LargeProjectRequest.Writes(PriorityRequest().Operation));
        Assert.False(LargeProjectRequest.Writes(PriorityRequest(false).Operation));
    }
}
