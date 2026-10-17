using Timberborn.Bridge.Core;
using Xunit;
using Cell = Timberborn.Bridge.Core.AccessRouteSearch.Cell;
using Edge = Timberborn.Bridge.Core.AccessRouteSearch.Edge;

namespace Timberborn.Tests;

public sealed class AccessRouteGeometryTests
{
    private static LargeProjectStep Part(string template,int x,int z=1) =>
        new(){Template=template,X=x,Y=0,Z=z};
    private static Edge Step(Cell to,params LargeProjectStep[] parts) => new(){To=to,Parts=parts};

    [Fact]
    public void MoreExpensiveArrivalSurvivesWhenCheapArrivalBlocksItsContinuation()
    {
        var start=new Cell(0,0,1); var via=new Cell(0,1,1);
        var shared=new Cell(2,0,2); var target=new Cell(2,1,2);
        IEnumerable<Edge> Expand(Cell cell,IReadOnlyList<LargeProjectStep> prefix)
        {
            if(cell.Equals(start))
                return [Step(shared,Part("Stairs",1)),Step(via,Part("Path",5))];
            if(cell.Equals(via)) return [Step(shared,Part("Stairs",6))];
            // Synthetic native occupancy control: the next support would occupy
            // the cheap route's stair. The expensive arrival leaves it free.
            if(cell.Equals(shared) && !prefix.Any(p=>p.Template=="Stairs"&&p.X==1))
                return [Step(target,Part("Platform",1),Part("Path",1,2))];
            return [];
        }
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{target},Expand,4);
        Assert.True(result.Found);
        Assert.Equal(start,result.Source);
        Assert.Equal(3,result.Edges.Length);
        Assert.DoesNotContain(result.Edges.SelectMany(e=>e.Parts),p=>p.Template=="Stairs"&&p.X==1);
        Assert.Contains(result.Edges.SelectMany(e=>e.Parts),p=>p.Template=="Platform"&&p.X==1);
        Assert.False(AccessRouteSearch.Find([start],new HashSet<Cell>{target},Expand,3).Found);
    }

    [Fact]
    public void EveryEarlierEdgeIsVisibleWhenCheckingLaterGeometry()
    {
        var start=new Cell(0,0,0); var target=new Cell(3,0,1);
        IEnumerable<Edge> Expand(Cell cell,IReadOnlyList<LargeProjectStep> prefix)
        {
            if(cell.X==0) return [Step(new(1,0,1),Part("Stairs",10))];
            if(cell.X==1) return [Step(new(2,0,1),Part("Path",11))];
            if(cell.X==2)
            {
                Assert.Equal(2,prefix.Count);
                Assert.Contains(prefix,p=>p.Template=="Stairs"&&p.X==10);
                return prefix.Any(p=>p.X==10)?[]:[Step(target,Part("Platform",10))];
            }
            return [];
        }
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{target},Expand,4);
        Assert.False(result.Found);
        Assert.Equal("no_route_in_region",result.Reason);
    }

    [Theory]
    [InlineData(1,0,1)] [InlineData(-1,0,1)]
    [InlineData(0,1,1)] [InlineData(0,-1,1)]
    [InlineData(1,0,-1)] [InlineData(0,-1,-1)]
    public void ExistingHeightExitNeedsNativeEdgeAndStillAddsDestinationPath(int dx,int dy,int dz)
    {
        var start=new Cell(4,4,2); var landing=new Cell(4+dx,4+dy,2+dz);
        IEnumerable<Edge> Expand(Cell cell,IReadOnlyList<LargeProjectStep> _) =>
            cell.Equals(start)?AccessRouteSearch.HeightExits(cell,c=>c.Equals(landing))
                .Select(c=>Step(c,new LargeProjectStep{Template="Path",X=c.X,Y=c.Y,Z=c.Z})):[];
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{landing},Expand,1);
        Assert.True(result.Found);
        var path=Assert.Single(Assert.Single(result.Edges).Parts);
        Assert.Equal("Path",path.Template);
        Assert.Equal(landing,new Cell(path.X,path.Y,path.Z));
        Assert.Empty(AccessRouteSearch.HeightExits(start,_=>false));
        Assert.False(AccessRouteSearch.Find([start],new HashSet<Cell>{landing},Expand,0).Found);
    }

    [Fact]
    public void GeometryVariantsRemainBounded()
    {
        var start=new Cell(0,0,1); var unreachable=new Cell(99,0,1);
        IEnumerable<Edge> Expand(Cell cell,IReadOnlyList<LargeProjectStep> prefix) =>
            [Step(new(cell.X+1,0,1),Part("Path",prefix.Count))];
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{unreachable},Expand,32,3);
        Assert.Equal("search_budget_exhausted",result.Reason);
        Assert.Equal(3,result.Visited);
    }

    [Theory]
    [InlineData(false,null,false)]
    [InlineData(false,false,false)]
    [InlineData(false,true,true)]
    [InlineData(true,null,true)]
    public void RegisteredLandingCanConnectAnOtherwiseMissingInstantEdge(bool instant,bool? preview,bool expected)
    {
        var start=new Cell(4,4,2);var landing=new Cell(5,4,3);
        IEnumerable<Edge> Expand(Cell cell,IReadOnlyList<LargeProjectStep> _) => cell.Equals(start)
            ?AccessRouteSearch.HeightExits(cell,c=>c.Equals(landing)&&AccessRouteSearch.ConfirmedHeightExit(instant,preview))
                .Select(c=>Step(c,new LargeProjectStep{Template="Path",X=c.X,Y=c.Y,Z=c.Z})):[];
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{landing},Expand,1);
        Assert.Equal(expected,result.Found);
        if(expected)Assert.Equal("Path",Assert.Single(Assert.Single(result.Edges).Parts).Template);
        Assert.False(AccessRouteSearch.Find([start],new HashSet<Cell>{landing},Expand,0).Found);
    }

    [Theory]
    [InlineData(0,4,3)] [InlineData(1,3,4)]
    [InlineData(2,4,5)] [InlineData(3,5,4)]
    public void StraightStairLandingIsRotatedUpperCandidateNotAnAssumedConnection(int rotation,int x,int y)
    {
        var landing=AccessRouteSearch.StraightStairLandingCandidate(new Cell(4,4,2),rotation);
        Assert.Equal(new Cell(x,y,3),landing);
        Assert.False(AccessRouteSearch.ConfirmedHeightExit(false,null));
        Assert.Throws<ArgumentException>(()=>AccessRouteSearch.StraightStairLandingCandidate(new Cell(4,4,2),4));
    }

    [Fact]
    public void PendingGeometryVariantsCannotGrowWithoutBound()
    {
        var start=new Cell(0,0,1);
        IEnumerable<Edge> Expand(Cell cell,IReadOnlyList<LargeProjectStep> _) =>
            Enumerable.Range(1,33).Select(i=>Step(new(i,0,1),Part("Path",i)));
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{new(99,0,1)},Expand,32,2);
        Assert.Equal("search_budget_exhausted",result.Reason);
        Assert.Equal(1,result.Visited);
    }

    [Theory]
    [InlineData(0,4,5)] [InlineData(1,5,4)]
    [InlineData(2,4,3)] [InlineData(3,3,4)]
    public void ExistingStairRejectsSidewaysAndNewStairDepartures(int rotation,int x,int y)
    {
        var anchor=new Cell(4,4,2);
        var lower=AccessRouteSearch.StraightStairLowerCandidate(anchor,rotation);
        Assert.Equal(new Cell(x,y,2),lower);
        Assert.True(AccessRouteSearch.AllowsGenericStairExit(lower,new Cell(x,y,2),0));
        foreach(var offset in new[]{(0,-1),(-1,0),(0,1),(1,0)})
        {
            var neighbour=new Cell(4+offset.Item1,4+offset.Item2,2);
            if(!neighbour.Equals(lower))Assert.False(AccessRouteSearch.AllowsGenericStairExit(lower,neighbour,0));
            Assert.False(AccessRouteSearch.AllowsGenericStairExit(lower,new Cell(4+2*offset.Item1,4+2*offset.Item2,3),1));
            Assert.False(AccessRouteSearch.AllowsGenericStairExit(lower,new Cell(4+2*offset.Item1,4+2*offset.Item2,1),-1));
        }
        Assert.False(AccessRouteSearch.ConfirmedHeightExit(false,null));
    }

    [Fact]
    public void PlatformDetourUsesUpperRoadInsteadOfCheaperLateralStairShortcut()
    {
        var stair=new Cell(4,4,2);var upper=new Cell(5,4,3);
        var side=new Cell(4,5,2);var goal=new Cell(4,6,3);
        var route=new[]{upper,new Cell(5,5,3),new Cell(4,5,3),goal};
        IEnumerable<Edge> Expand(Cell c,IReadOnlyList<LargeProjectStep> _)
        {
            if(c.Equals(stair)&&AccessRouteSearch.AllowsGenericStairExit(
                AccessRouteSearch.StraightStairLowerCandidate(stair,3),side,0))
                yield return Step(side,Part("Path",0));
            if(c.Equals(side))yield return Step(goal,Part("Path",1));
            int index=Array.IndexOf(route,c);
            if(index<0||index==route.Length-1)yield break;
            var to=route[index+1];
            yield return new Edge{To=to,Parts=[
                new LargeProjectStep{Template="Platform.Folktails",X=to.X,Y=to.Y,Z=to.Z-1},
                new LargeProjectStep{Template="Path",X=to.X,Y=to.Y,Z=to.Z}]};
        }
        int Estimate(Cell c)=>Math.Max(Math.Abs(c.X-goal.X)+Math.Abs(c.Y-goal.Y),Math.Abs(c.Z-goal.Z));
        var result=AccessRouteSearch.Find([stair,upper],new HashSet<Cell>{goal},Expand,6,16,Estimate);
        Assert.True(result.Found);Assert.Equal(upper,result.Source);
        Assert.Equal(3,result.Edges.Length);
        Assert.DoesNotContain(result.Edges,e=>e.To.Equals(side));
        Assert.All(result.Edges,e=>Assert.Equal(new[]{"Platform.Folktails","Path"},e.Parts.Select(p=>p.Template)));
        Assert.False(AccessRouteSearch.Find([stair,upper],new HashSet<Cell>{goal},Expand,5,16,Estimate).Found);
    }

    [Fact]
    public void GoalPriorityFindsShortHeightConnectionBeforeUnrelatedSourcesUseBudget()
    {
        var start=new Cell(40,40,1);var landing=new Cell(41,40,2);var goal=new Cell(41,41,2);
        var sources=Enumerable.Range(0,40).Select(i=>new Cell(i,0,1)).Append(start).ToArray();
        IEnumerable<Edge> Expand(Cell c,IReadOnlyList<LargeProjectStep> _) => c.Equals(start)
            ?[Step(landing,new LargeProjectStep{Template="Path",X=41,Y=40,Z=2})]
            :c.Equals(landing)?[Step(goal,new LargeProjectStep{Template="Path",X=41,Y=41,Z=2})]:[];
        int LowerBound(Cell c)=>Math.Max(Math.Abs(c.X-goal.X)+Math.Abs(c.Y-goal.Y),Math.Abs(c.Z-goal.Z));
        Assert.False(AccessRouteSearch.Find(sources,new HashSet<Cell>{goal},Expand,2,3).Found);
        var result=AccessRouteSearch.Find(sources,new HashSet<Cell>{goal},Expand,2,3,LowerBound);
        Assert.True(result.Found);Assert.Equal(3,result.Visited);Assert.Equal(start,result.Source);
        Assert.Equal(2,result.Edges.Length);Assert.Equal(0,result.ClosestRemainingCost);
    }

    [Fact]
    public void GoalPriorityStillTakesDetourAndPreservesPartLimit()
    {
        var start=new Cell(0,0,1);var goal=new Cell(2,0,1);var blocked=new Cell(1,0,1);
        IEnumerable<Edge> Expand(Cell c,IReadOnlyList<LargeProjectStep> prefix)
        {
            foreach(var d in new[]{(1,0),(-1,0),(0,1),(0,-1)})
            {
                var to=new Cell(c.X+d.Item1,c.Y+d.Item2,1);
                if(to.X<0||to.X>2||to.Y<0||to.Y>1||to.Equals(blocked)||prefix.Any(p=>p.X==to.X&&p.Y==to.Y))continue;
                yield return Step(to,new LargeProjectStep{Template="Path",X=to.X,Y=to.Y,Z=1});
            }
        }
        int LowerBound(Cell c)=>Math.Abs(c.X-goal.X)+Math.Abs(c.Y-goal.Y);
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{goal},Expand,4,16,LowerBound);
        Assert.True(result.Found);Assert.Equal(4,result.Edges.Length);
        Assert.DoesNotContain(result.Edges,e=>e.To.Equals(blocked));
        Assert.False(AccessRouteSearch.Find([start],new HashSet<Cell>{goal},Expand,3,16,LowerBound).Found);
    }
}
