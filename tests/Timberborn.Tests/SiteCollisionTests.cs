using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class SiteCollisionTests
{
    private static readonly Guid Id = new("10000000-0000-0000-0000-000000000001");
    private static SiteCell Cell(bool intersects, SiteCollision? collision, bool inside=true) =>
        new(new(1,2,3),inside,false,true,intersects,"Ground",collision);

    [Fact]
    public void LegacyEvidenceRemainsUnknown() => SiteCollisionContract.Validate(Cell(true,null));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TerrainIsIndependentOfObjectCollision(bool terrain)
    {
        SiteCollisionContract.Validate(Cell(false,new(terrain,"observed",[],false)));
        SiteCollisionContract.Validate(Cell(true,new(terrain,"observed",[new(Id,"SyntheticTree")],false)));
    }

    [Fact]
    public void UnknownIdentityDoesNotHideObservedCollision() =>
        SiteCollisionContract.Validate(Cell(true,new(false,"observed",[new(null,null)],false)));

    [Fact]
    public void OutsideMapIsUnobserved() =>
        SiteCollisionContract.Validate(Cell(false,new(null,"outside_map",null,false),inside:false));

    [Theory]
    [InlineData(32,true,true)]
    [InlineData(32,false,true)]
    [InlineData(31,true,false)]
    [InlineData(33,true,false)]
    public void TruncationCannotClaimCompleteOrOverflowingEvidence(int count,bool truncated,bool accepted)
    {
        var objects=Enumerable.Range(1,count).Select(i=>new SiteCollisionObject(new Guid(i,0,0,new byte[8]),"SyntheticObject")).ToArray();
        var cell=Cell(true,new(false,"observed",objects,truncated));
        if(accepted) SiteCollisionContract.Validate(cell);
        else Assert.Throws<InvalidDataException>(()=>SiteCollisionContract.Validate(cell));
    }

    [Fact]
    public void ContradictoryOrMalformedEvidenceIsRejected()
    {
        SiteCell[] invalid = [
            Cell(true,new(false,"observed",[],false)),
            Cell(false,new(false,"observed",[new(Id,"Synthetic")],false)),
            Cell(true,new(false,"observed",null,false)),
            Cell(true,new(null,"observed",[new(Id,"Synthetic")],false)),
            Cell(true,new(false,"observed",[new(Guid.Empty,"Synthetic")],false)),
            Cell(true,new(false,"observed",[new(Id,"")],false)),
            Cell(true,new(false,"observed",[new(Id,"Synthetic"),new(Id,"Synthetic")],false)),
            Cell(true,new(false,"observed",[null!],false)),
            Cell(false,new(false,"outside_map",null,false),false),
            Cell(false,new(null,"outside_map",[],false),false),
            Cell(false,new(null,"outside_map",null,false)),
            Cell(false,new(false,"unknown_state",[],false))
        ];
        foreach(var cell in invalid) Assert.Throws<InvalidDataException>(()=>SiteCollisionContract.Validate(cell));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NativeTransportPreservesEvidenceAndRejectsContradictions(bool malformed)
    {
        var cell=Cell(!malformed,new(false,"observed",[new(Id,"SyntheticTree")],false));
        var site=new NativeSite("Path",new(1,2,3),0,"blocked",false,["object_intersection"],[cell],null,null,[],[]);
        var json=JsonSerializer.Serialize(new BridgeEnvelope<NativeSite>(1,"20000000-0000-0000-0000-000000000002",
            DateTimeOffset.UnixEpoch,"0.74.0",site),NativeJson.Options);
        using var client=new NativeClient(new(8081,new string('a',64)),new Handler(json));
        var request=BridgeRequest.Parse("/agent-api/v1/building-precheck",new() {
            ["template"]="Path",["x"]="1",["y"]="2",["z"]="3",["rotation"]="0" });
        if(malformed) await Assert.ThrowsAsync<InvalidDataException>(()=>client.Precheck(request,TestContext.Current.CancellationToken));
        else {
            var result=await client.Precheck(request,TestContext.Current.CancellationToken);
            Assert.Equal(Id,result.Data.Cells[0].Collision!.Objects![0].Id);
            Assert.Equal("SyntheticTree",result.Data.Cells[0].Collision!.Objects![0].Template);
            Assert.False(result.Data.GameValidated);
        }
    }

    private sealed class Handler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content=new StringContent(json) });
    }
}
