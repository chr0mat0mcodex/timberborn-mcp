using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ReservoirPlanningTests
{
    private static StrategicColumn[] Grid(Func<int,int,StrategicSpan[]> spans) =>
        Enumerable.Range(0,5).SelectMany(y=>Enumerable.Range(0,8).Select(x=>new StrategicColumn(x,y,spans(x,y)))).ToArray();
    private static ReservoirOptions Analyze(StrategicColumn[] cells, StrategicObject[]? objects=null, bool complete=true) =>
        ReservoirPlanning.Analyze(new(8,5,10),cells,new(7,2,3,1,1),objects??[],complete,TestContext.Current.CancellationToken);

    [Fact]
    public void TwoLevelReservoirReportsGeometryNotGuaranteedWater()
    {
        var result=Analyze(Grid((_,_)=>[new(3,10,1,0)]));
        var option=Assert.Single(result.Options);
        Assert.Equal(1,option.TargetFloor);Assert.Equal(18,option.AddedGeometricVolume);
        Assert.Equal(9,option.ExcavationCells.Length);Assert.Empty(option.Blockers);
        Assert.Equal("local_water_above_rim_not_future_supply_proof",option.RefillEvidence);
        Assert.Contains("geometric_volume_in_grid_cubes_not_water_goods_or_days",result.Limitations);
    }

    [Fact]
    public void CaveBelowExcavationAndOpenLateralBankRejectCandidates()
    {
        Assert.Empty(Analyze(Grid((_,_)=>[new(0,2,0,0),new(3,10,0,0)])).Options);
        Assert.Empty(Analyze(Grid((_,y)=>[new(y==0?1:3,10,0,0)])).Options);
    }

    [Fact]
    public void OccupiedZoneIsNotProposedAndUnknownInventoryBlocksPlanning()
    {
        var cells=Grid((_,_)=>[new(3,10,0,0)]);
        var obstacle=new StrategicObject(Guid.NewGuid(),"synthetic_building",3,2,3,"building",true);
        Assert.Empty(Analyze(cells,[obstacle]).Options);
        var option=Assert.Single(Analyze(cells,complete:false).Options);
        Assert.Contains("object_inventory_incomplete",option.Blockers);
        Assert.Contains("no_observed_water_above_rim_refill_route_required",option.Blockers);
    }

    [Fact]
    public void BadwaterIsNeverDescribedAsCleanReserve()
    {
        var option=Assert.Single(Analyze(Grid((_,_)=>[new(3,10,1,1)])).Options);
        Assert.Equal("local_contamination_observed",option.RefillEvidence);
        Assert.Contains("local_contaminated_water_not_a_clean_reserve",option.Blockers);
    }

    [Fact]
    public void CompactKeepsBenefitGeometryAndBlockersWhileFullKeepsCells()
    {
        var options=Analyze(Grid((_,_)=>[new(3,10,0,0)]));
        var raw=JsonSerializer.SerializeToNode(new{status="ok",data=new{analysis=new{terraforming=options}}},NativeJson.Options)!.AsObject();
        var compact=ResponsePresentation.Present(StrategicMapTools.Name,raw,false);
        var option=compact["data"]!["analysis"]!["terraforming"]!["options"]![0]!;
        Assert.Null(option["excavationCells"]);Assert.Equal(18,option["addedGeometricVolume"]!.GetValue<int>());
        Assert.NotEmpty(option["blockers"]!.AsArray());
        Assert.Equal(raw.ToJsonString(),ResponsePresentation.Present(StrategicMapTools.Name,raw,true).ToJsonString());
    }

    [Fact]
    public void IncompleteTerrainProducesNoOptions()
    {
        Assert.Equal("incomplete_terrain_no_options",Analyze([]).State);
    }
}
