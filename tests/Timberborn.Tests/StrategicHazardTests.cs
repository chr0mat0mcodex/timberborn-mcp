using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class StrategicHazardTests
{
    private static StrategicColumn[] Grid(int width,int height,Func<int,int,StrategicSpan[]> spans) =>
        Enumerable.Range(0,height).SelectMany(y=>Enumerable.Range(0,width).Select(x=>new StrategicColumn(x,y,spans(x,y)))).ToArray();

    [Fact]
    public void StoredBadwaterWithoutSourceProducesConcreteClosurePlan()
    {
        var grid=Grid(7,5,(x,y)=>[new(y==2?x==3?2:1:6,8,x==0&&y==2?3:0,x==0&&y==2?1:0)]);
        var report=StrategicMapAnalysis.Analyze(new(7,5,8),grid,new(6,2,1,1,1),null,TestContext.Current.CancellationToken);
        var hazards=Assert.IsType<StrategicHazards>(report.Hazards);
        Assert.Equal("potential_inflow_requires_action",hazards.State);
        Assert.Equal(1,hazards.ContaminatedPotentialInflowNodes);
        var hazard=Assert.Single(hazards.Priorities);
        Assert.Equal(new Position(3,2,2),hazard.Candidate.At);
        Assert.Equal(5,hazard.Candidate.AssumedBarrierCrest);
        Assert.Equal(2f,hazard.HeadAboveTargetAndPass);
        Assert.Equal("terrain_route_cut_at_observed_level",hazard.ClosureAssessment);
        Assert.Contains("plan_barrier_at_candidate_cross_section_with_native_build_validation",hazard.Measures);
    }

    [Fact]
    public void DrySourcesCannotHideStoredWaterAndLowerScenarioCannotUndersizeBarrier()
    {
        var grid=Grid(160,3,(x,y)=>[new(y==1?x==150?2:1:7,10,x==140&&y==1?3:0,x==140&&y==1?1:0)]);
        var sources=Enumerable.Range(0,9).Select(i=>new StrategicObject(Guid.NewGuid(),"WaterSource",i*16,1,1,"special_or_unknown",true)).ToArray();
        var report=StrategicMapAnalysis.Analyze(new(160,3,10),grid,new(159,1,1,1,1),1,TestContext.Current.CancellationToken,sources);
        Assert.Equal(new Position(140,1,1),report.Corridors[0].Origin);
        var hazard=Assert.Single(Assert.IsType<StrategicHazards>(report.Hazards).Priorities);
        Assert.Equal(5,hazard.Candidate.AssumedBarrierCrest);
        Assert.Equal("observed_level_plus_one_level",hazard.Candidate.CrestBasis);
    }

    [Fact]
    public void CaveRoofBlocksNominalBarrier()
    {
        var grid=Grid(7,5,(x,y)=>[new(y==2?x==3?2:1:6,y==2?5:8,x==0&&y==2?3.5f:0,1)]);
        var report=StrategicMapAnalysis.Analyze(new(7,5,8),grid,new(6,2,1,1,1),null,TestContext.Current.CancellationToken);
        var hazard=Assert.Single(Assert.IsType<StrategicHazards>(report.Hazards).Priorities);
        Assert.Equal("blocked_by_cave_roof",hazard.ClosureAssessment);
        Assert.DoesNotContain("plan_barrier_at_candidate_cross_section_with_native_build_validation",hazard.Measures);
    }

    [Fact]
    public void ParallelBypassRejectsSingleCorridorClosure()
    {
        var grid=Grid(7,7,(x,y)=>[new(y==2||y==4||x==0||x==6?x==3?2:1:7,10,x==0&&y==2?3:0,0)]);
        var report=StrategicMapAnalysis.Analyze(new(7,7,10),grid,new(6,2,1,1,1),null,TestContext.Current.CancellationToken);
        var hazard=Assert.Single(Assert.IsType<StrategicHazards>(report.Hazards).Priorities);
        Assert.Equal("alternate_terrain_route_remains",hazard.ClosureAssessment);
    }

    [Fact]
    public void WaterBelowTargetHeightIsNotHighInflowAndWetTargetIsReportedSeparately()
    {
        var grid=Grid(3,1,(x,_)=>[new(0,8,x==0?2:0,1)]);
        var report=StrategicMapAnalysis.Analyze(new(3,1,8),grid,new(2,0,3,1,1),null,TestContext.Current.CancellationToken);
        Assert.Equal(0,Assert.IsType<StrategicHazards>(report.Hazards).PotentialInflowNodes);
        grid[2]=new(2,0,[new(0,8,4,1)]);
        report=StrategicMapAnalysis.Analyze(new(3,1,8),grid,new(2,0,3,1,1),null,TestContext.Current.CancellationToken);
        Assert.Equal("target_water_observed",Assert.IsType<StrategicHazards>(report.Hazards).State);
        Assert.Equal(1,report.Hazards!.FloodedTargetNodes);
    }

    [Fact]
    public void HazardCountsAreNotLimitedToEightRepresentatives()
    {
        var grid=Grid(160,1,(x,_)=>[new(1,8,x<159?2:0,1)]);
        var report=StrategicMapAnalysis.Analyze(new(160,1,8),grid,new(159,0,1,1,1),null,TestContext.Current.CancellationToken);
        var hazards=Assert.IsType<StrategicHazards>(report.Hazards);
        Assert.Equal(159,hazards.PotentialInflowNodes);
        Assert.Equal(159,hazards.ContaminatedPotentialInflowNodes);
        Assert.Equal(8,hazards.RepresentedPotentialInflowNodes);
        Assert.Equal(8,hazards.Priorities.Length);
    }

    [Fact]
    public void PartialMapNeverClaimsHazardAssessment()
    {
        var report=StrategicMapAnalysis.Analyze(new(3,1,8),[new(0,0,[new(1,8,4,1)])],new(2,0,1,1,1),null,TestContext.Current.CancellationToken);
        Assert.Null(report.Hazards);
    }
}
