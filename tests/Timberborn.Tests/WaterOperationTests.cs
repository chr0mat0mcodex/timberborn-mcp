using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class WaterOperationTests
{
    private static NativeOperation Pump(OperationWater? water) => new(
        "11111111-1111-4111-8111-111111111111","SyntheticPump",true,false,
        new(1,1,1,false,true,true),new(true,"Water",true,true,true,false,true,0),[],[],[],
        new(false,null,null,null,null,null),Bottleneck:new(){State="observed",Complete=true,
            DistrictConnected=true,EntranceBlocked=false,Goods=[]},Water:water);

    [Theory]
    [InlineData(false)][InlineData(true)]
    public void FloodedPumpIsBlockedEvenWhenPausedAndNoVisibleStatus(bool paused)
    {
        var pump=Pump(new(true,true,true,true,0,false,null)) with {Paused=paused};
        var report=OperationAssessmentBuilder.Read(pump);
        Assert.Contains("building_flooded",report.Blockers);
        Assert.Equal("building_flooded",report.Findings![0].Code);
        Assert.DoesNotContain(report.Findings!,f=>f.Code=="no_observed_bottleneck");
        Assert.False(report.ProductionProven);
    }

    [Fact]
    public void DryIntakeBlockedPipeAndContaminationRemainSeparate()
    {
        var dry=OperationAssessmentBuilder.Read(Pump(new(true,false,true,false,1,true,true)));
        Assert.Contains("water_intake_dry",dry.Blockers);
        Assert.Contains("water_intake_blocked",dry.Blockers);
        Assert.DoesNotContain("water_intake_contaminated",dry.Warnings);
        var wet=OperationAssessmentBuilder.Read(Pump(new(true,false,true,true,0.2f,true,false)));
        Assert.Contains("water_intake_contaminated",wet.Warnings);
        Assert.Empty(wet.Blockers);
        Assert.False(wet.ProductionProven);
    }

    [Fact]
    public void MissingWaterObservationIsNotDryAndLocalizedStatusIsNotParsed()
    {
        var pump=Pump(null) with {Statuses=["Flooded"]};
        var report=OperationAssessmentBuilder.Read(pump);
        Assert.Contains("water_operation_unknown",report.Unknown);
        Assert.DoesNotContain("building_flooded",report.Blockers);
        Assert.DoesNotContain(report.Findings!,f=>f.Code=="no_observed_bottleneck");
        Assert.Contains("flood_state_unknown",OperationAssessmentBuilder.Read(
            Pump(new(false,null,true,true,0,false,null))).Unknown);
    }
}
