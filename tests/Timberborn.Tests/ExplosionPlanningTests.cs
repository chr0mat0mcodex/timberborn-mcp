using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ExplosionPlanningTests
{
    private static StrategicColumn[] Terrain(float water=3) => Enumerable.Range(0,5).SelectMany(y=>
        Enumerable.Range(0,7).Select(x=>new StrategicColumn(x,y,
            [new(y==2 && x!=3?1:6,8,x==0 && y==2?water:0,x==0 && y==2?1:0)]))).ToArray();
    private static StrategicObject Core(bool complete=true,bool blocked=false) =>
        new(Guid.Parse("11111111-1111-4111-8111-111111111111"),"UnstableCore",3,2,4,"special_or_unknown",true,
            new(3,1,blocked,"native_radius_terrain_scenario",Enumerable.Range(1,5).Select(z=>new Position(3,2,z)).ToArray(),complete,"unknown"));
    private static StrategicAnalysis Analyze(StrategicColumn[] terrain,StrategicObject core) =>
        StrategicMapAnalysis.Analyze(new(7,5,8),terrain,new(6,2,1,1,1),null,TestContext.Current.CancellationToken,[core],true);

    [Fact]
    public void ClosedRidgeWarnsOfBadwaterRouteBeforeAnyExplosion()
    {
        var terrain=Terrain(); var report=Analyze(terrain,Core());
        Assert.Equal(0,report.Hazards!.ContaminatedPotentialInflowNodes);
        var warning=Assert.Single(report.Explosions!.Priorities);
        Assert.Equal("badwater_breach_to_target_potential",warning.State);
        var chain=Assert.Single(warning.Chains);
        Assert.Equal(new Position(0,2,1),chain.StoredLiquid);
        Assert.Equal(new Position(6,2,1),chain.TargetEntry);
        Assert.Equal(6,chain.BeforePass);
        Assert.Equal(1,chain.AfterPass);
        Assert.Equal("terrain_pass_lowered",chain.ConnectionChange);
        Assert.Equal(5,warning.OpeningCells);
        Assert.Equal(chain.StoredLiquid,chain.Route[0]);
        Assert.Equal(chain.TargetEntry,chain.Route[^1]);
        Assert.NotEmpty(chain.Measures);
        Assert.Equal(6,terrain.Single(c=>c.X==3 && c.Y==2).Spans[0].Bottom);
        Assert.Equal("inspect_explosion_chains_and_protect_target_before_detonation",report.Hazards.NextAction);
    }

    [Fact]
    public void MissingOrTruncatedGeometryNeverInventsAChain()
    {
        foreach(var core in new[]{Core(false),Core() with {Explosion=null}}) {
            var warning=Assert.Single(Analyze(Terrain(),core).Explosions!.Priorities);
            Assert.Equal("explosion_risk_unassessed",warning.State);
            Assert.Empty(warning.Chains);
        }
    }

    [Fact]
    public void DryReservoirDoesNotInventReleasedBadwater()
    {
        var warning=Assert.Single(Analyze(Terrain(0),Core()).Explosions!.Priorities);
        Assert.Equal("no_chain_proven_not_safe",warning.State);
        Assert.Empty(warning.Chains);
    }

    [Fact]
    public void BlockedCoreIsStillAConditionalFutureThreat()
    {
        var warning=Assert.Single(Analyze(Terrain(),Core(blocked:true)).Explosions!.Priorities);
        Assert.True(warning.CurrentlyBlocked);
        Assert.Equal("badwater_breach_to_target_potential",warning.State);
        Assert.Equal("unknown",warning.Timing);
    }

    [Fact]
    public void CompactRetainsCoreWaterTargetProtectionAndUncertainty()
    {
        var report=Analyze(Terrain(),Core());
        var full=JsonSerializer.SerializeToNode(new {analysis=report},NativeJson.Options)!.AsObject();
        var compact=full.DeepClone().AsObject(); StrategicScanPresentation.Compact(compact);
        var chain=compact["analysis"]!["explosions"]!["priorities"]![0]!["chains"]![0]!;
        Assert.NotNull(chain["storedLiquid"]); Assert.NotNull(chain["targetEntry"]);
        Assert.NotNull(chain["protection"]!["at"]); Assert.NotNull(chain["measures"]);
        Assert.NotNull(chain["backwater"]);
        Assert.False(chain["backwater"]!["closureRecommended"]!.GetValue<bool>());
        Assert.Null(chain["route"]);
        Assert.NotNull(full["analysis"]!["explosions"]!["priorities"]![0]!["chains"]![0]!["route"]);
    }

    [Fact]
    public void InvalidNativeGeometryIsRejectedAtTransportBoundary()
    {
        var tile=new StrategicTile(0,1,new(7,5,8),0,0,7,5,Terrain(),[Core()],true,true,[]);
        NativeClient.ValidateStrategicTile(tile,0);
        var invalid=Core() with { Explosion=Core().Explosion! with {TerrainCells=[new(7,2,1)]} };
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateStrategicTile(tile with {Objects=[invalid]},0));
        var duplicate=Core() with { Explosion=Core().Explosion! with {TerrainCells=[new(3,2,1),new(3,2,1)]} };
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateStrategicTile(tile with {Objects=[duplicate]},0));
    }

    [Fact]
    public void NoProtectionRecommendationInsidePossibleBlast()
    {
        var warning=Assert.Single(Analyze(Terrain(),Core()).Explosions!.Priorities);
        var chain=Assert.Single(warning.Chains);
        Assert.Equal("protection_inside_possible_blast_relocate",chain.ProtectionAssessment);
        Assert.DoesNotContain("plan_closure_at_protection_cross_section_before_detonation",chain.Measures);
    }
}
