using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class RiverOverviewTests
{
    private static StrategicWeather Weather(bool hazardous)=>new(1,2,2.5f,hazardous,"Badtide",3,4,4,7);
    private static StrategicSource Source(int x,int z,float contamination=0)=>new(
        Guid.Parse("11111111-1111-4111-8111-111111111111"),new(x,0,z),[new(x,0,z)],false,1,1,contamination,false,["Badtide"],[]);
    [Fact]
    public void DryRidgeConnectsSeparateWaterAndBadwaterWithoutMixingTheirLabels()
    {
        StrategicColumn[] grid=[new(0,0,[new(1,8,1,0)]),new(1,0,[new(5,8,0,0)]),new(2,0,[new(1,8,1,1)])];
        var report=RiverOverviewAnalysis.Analyze(new(3,1,8),grid,[],true,Weather(false),Weather(false),TestContext.Current.CancellationToken);
        Assert.Equal(2,report.TotalSections);
        Assert.Contains(report.Sections,s=>s.Liquid=="water");Assert.Contains(report.Sections,s=>s.Liquid=="badwater");
        var link=Assert.Single(report.Links);Assert.Equal(5,link.TerrainPass);
        Assert.False(link.ALevelAbovePass);Assert.False(link.BLevelAbovePass);
        Assert.Equal("unknown_geometric_connection",link.Direction);
    }
    [Fact]
    public void SourceAndContaminationStayInTheirOwnCaveLevel()
    {
        StrategicColumn[] grid=[new(0,0,[new(1,3,1,1),new(5,8,1,0)])];
        var report=RiverOverviewAnalysis.Analyze(new(1,1,8),grid,[Source(0,1,1)],true,Weather(false),Weather(true),TestContext.Current.CancellationToken);
        Assert.Equal(2,report.TotalSections);Assert.Empty(report.Links);Assert.True(report.WeatherChanged);
        var source=Assert.Single(report.Sources);int section=Assert.Single(source.Sections);
        Assert.Equal("badwater",report.Sections[section].Liquid);
        Assert.Equal("selected_hazard_allowed_by_activation_rule_other_modifiers_may_block",source.WeatherExpectation);
        Assert.Equal("unknown_reobserve_at_weather_transition",source.FutureContamination);
    }
    [Fact]
    public void DryNativeSourceIsRetainedWithoutInventedLiquidInTerrain()
    {
        StrategicColumn[] grid=[new(0,0,[new(1,8,0,0)])];
        var report=RiverOverviewAnalysis.Analyze(new(1,1,8),grid,[Source(0,1) with {CurrentStrength=0,ActiveInHazardousWeather=[]}],true,
            Weather(true),Weather(true),TestContext.Current.CancellationToken);
        Assert.Equal("dry",Assert.Single(report.Sections).Liquid);
        Assert.Null(report.Sections[0].MaxLevel);
        Assert.Equal("selected_hazard_excluded_by_activation_rule",Assert.Single(report.Sources).WeatherExpectation);
    }
    [Fact]
    public void RegionalSectionsArePagedWithoutLosingTotalsOrWeatherChanges()
    {
        var grid=Enumerable.Range(0,272).Select(x=>new StrategicColumn(x,0,[new(1,8,1,0)])).ToArray();
        var before=RiverOverviewAnalysis.Analyze(new(272,1,8),grid,[Source(0,1)],true,Weather(false),Weather(false),TestContext.Current.CancellationToken);
        var after=RiverOverviewAnalysis.Analyze(new(272,1,8),grid,[Source(0,1,1) with {CurrentStrength=0}],true,Weather(true),Weather(true),TestContext.Current.CancellationToken);
        var compared=RiverOverviewAnalysis.Compare(after,before);
        Assert.True(compared.ComparisonWeatherChanged);
        Assert.Equal(-1f,compared.Sources[0].StrengthChange);Assert.Equal(1f,compared.Sources[0].ContaminationChange);
        var first=RiverOverviewAnalysis.Page(compared,0,0,0);var last=RiverOverviewAnalysis.Page(compared,32,0,0);
        Assert.Equal(34,first.TotalSections);Assert.Equal(32,first.Sections.Length);Assert.True(first.MoreSections);
        Assert.Equal(2,last.Sections.Length);Assert.False(last.MoreSections);
    }
    [Fact]
    public void PartialTerrainHasNoRiverNetwork()
    {
        var report=RiverOverviewAnalysis.Analyze(new(2,1,8),[new(0,0,[new(1,8,1,0)])],[],false,null,null,TestContext.Current.CancellationToken);
        Assert.Equal("incomplete_terrain",report.State);Assert.Empty(report.Links);Assert.False(report.SourcesComplete);
    }
}
