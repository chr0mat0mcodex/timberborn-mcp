using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;
using Cell=Timberborn.Bridge.Core.AccessRouteSearch.Cell;

namespace Timberborn.Tests;

public sealed class ConstructionWorkCellsTests
{
    [Fact]
    public void FoundationPerimeterIncludesCornersAndExcludesOccupiedCenter()
    {
        // Pure geometry fixture; native rotor collision remains a live-test case.
        var shaft=new Cell(4,5,2);
        var cells=ConstructionWorkCells.Around([shaft],c=>!c.Equals(shaft)&&c.X>=0&&c.Y>=0);
        Assert.Equal(8,cells.Length);
        Assert.Contains(new Cell(3,4,2),cells); // corner, not just cardinal access
        Assert.Contains(new Cell(5,5,2),cells);
        Assert.DoesNotContain(shaft,cells);
    }

    [Fact]
    public void LaterBuildingAndMapEdgeRemoveCandidatesWithoutInventingOtherHeights()
    {
        var footprint=new HashSet<Cell>{new(0,0,3),new(1,0,3)};
        var later=new Cell(1,1,3);
        var cells=ConstructionWorkCells.Around(footprint,c=>c.X>=0&&c.Y>=0&&!footprint.Contains(c)&&!c.Equals(later));
        Assert.DoesNotContain(later,cells);
        Assert.All(cells,c=>Assert.Equal(3,c.Z));
        Assert.DoesNotContain(cells,c=>c.X<0||c.Y<0);
        Assert.Equal(cells.Length,cells.Distinct().Count());
        Assert.Empty(ConstructionWorkCells.Around(footprint,_=>false));
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("no_work_cell")]
    [InlineData("no_connected_work_cell")]
    [InlineData("final_work_access_blocked")]
    [InlineData("helper_runtime_check")]
    public void MissingRequiredWorkAccessStopsBeforePlacement(string state)
    {
        var ledger=new LargeProjectLedger();const string id="11111111-1111-1111-1111-111111111111";
        ledger.Start(id,new string('a',64),"fixture",[0],["22222222-2222-2222-2222-222222222222"]);
        int placements=0;
        var receipt=ledger.Advance(id,0,()=>true,_=>"finished",_=>ConstructionWorkCells.Accepts(true,state),_=>placements++);
        Assert.Equal(0,placements);Assert.Equal("stopped",receipt.State);
    }

    [Fact]
    public void ReachableWorkCellAllowsOneOrderButDoesNotCompleteConstruction()
    {
        var ledger=new LargeProjectLedger();const string id="11111111-1111-1111-1111-111111111111";
        ledger.Start(id,new string('a',64),"fixture",[0],["22222222-2222-2222-2222-222222222222"]);
        int placements=0;
        var receipt=ledger.Advance(id,0,()=>true,_=>"construction",_=>ConstructionWorkCells.Accepts(true,"road_reachable"),_=>placements++);
        Assert.Equal(1,placements);Assert.NotEqual("completed",receipt.State);
    }

    [Fact]
    public void HelperExceptionCannotClaimARequiredWorkCellWasChecked()
    {
        var data=JsonSerializer.SerializeToNode(new {required=false,state="helper_runtime_check",
            source="geometric_foundation_perimeter_and_native_district_road",candidateCount=0,
            reachableCells=Array.Empty<object>(),nativeConstructionProven=false})!.AsObject();
        ConstructionWorkAccessContract.Validate(JsonSerializer.SerializeToElement(data),true);
        data["required"]=true;
        Assert.Throws<InvalidDataException>(()=>ConstructionWorkAccessContract.Validate(JsonSerializer.SerializeToElement(data),true));
    }

    [Fact]
    public void WorkAccessContractRejectsInventedNativeProofAndContradictorySuccess()
    {
        var data=JsonSerializer.SerializeToNode(new {required=true,state="road_reachable",
            source="geometric_foundation_perimeter_and_native_district_road",candidateCount=8,
            reachableCells=new[]{new{x=2,y=3,z=1}},nativeConstructionProven=false})!.AsObject();
        void Validate(bool eligible=true)=>ConstructionWorkAccessContract.Validate(JsonSerializer.SerializeToElement(data),eligible);
        Validate();
        data["nativeConstructionProven"]=true;
        Assert.Throws<InvalidDataException>(()=>Validate());
        data["nativeConstructionProven"]=false;data["state"]="no_connected_work_cell";
        Assert.Throws<InvalidDataException>(()=>Validate());
        data["reachableCells"]=new JsonArray();
        Assert.Throws<InvalidDataException>(()=>Validate());
        Validate(false);
        data["state"]="road_reachable";
        Assert.Throws<InvalidDataException>(()=>Validate());
    }
}
