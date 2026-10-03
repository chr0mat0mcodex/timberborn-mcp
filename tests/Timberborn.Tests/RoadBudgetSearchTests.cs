using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class RoadBudgetSearchTests
{
    [Theory]
    [InlineData(4, true)]
    [InlineData(3, false)]
    public void IncludesEntranceButNotExistingConnectionInBudget(int budget, bool found)
    {
        var route = FlatRoadSearch.FindWithinRoadBudget(5, 1, 0, [],
            [true, true, true, true, true], [false, false, false, false, true],
            [false, false, false, false, true], budget);
        if (found) Assert.Equal(new[] { 4, 3, 2, 1, 0 }, route);
        else Assert.Null(route);
    }

    [Fact]
    public void LongerExistingRoadDetourFitsWhereShortestRouteDoesNot()
    {
        bool[] open = Enumerable.Repeat(true, 8).ToArray();
        bool[] goals = [false, false, false, true, false, false, false, false];
        bool[] roads = [false, false, false, true, true, true, true, true];
        Assert.Equal(new[] { 3, 7, 6, 5, 4, 0 },
            FlatRoadSearch.FindWithinRoadBudget(4, 2, 0, [], open, goals, roads, 1));
        Assert.Null(FlatRoadSearch.FindWithinRoadBudget(4, 2, 0, [4], open, goals, roads, 1));
        Assert.All(open, value => Assert.True(value));
    }

    [Fact]
    public void EqualRoadCostPrefersShorterRouteAndIsRepeatable()
    {
        bool[] open = Enumerable.Repeat(true, 8).ToArray();
        bool[] goals = [false, false, false, true, false, false, false, false];
        for (int i = 0; i < 2; i++)
            Assert.Equal(new[] { 3, 2, 1, 0 },
                FlatRoadSearch.FindWithinRoadBudget(4, 2, 0, [], open, goals, open, 0));
    }

    [Fact]
    public void ExistingEntranceNeedsNoNewRoadButFootprintStillBlocks()
    {
        Assert.Equal(new[] { 0 }, FlatRoadSearch.FindWithinRoadBudget(1, 1, 0, [], [true], [true], [true], 0));
        Assert.Null(FlatRoadSearch.FindWithinRoadBudget(1, 1, 0, [0], [true], [true], [true], 0));
        Assert.Null(FlatRoadSearch.FindWithinRoadBudget(1, 1, 0, [], [false], [true], [true], 0));
        Assert.Throws<ArgumentException>(() => FlatRoadSearch.FindWithinRoadBudget(1, 1, 0, [], [true], [true], [], 1));
        Assert.Throws<ArgumentException>(() => FlatRoadSearch.FindWithinRoadBudget(1, 1, 0, [], [true], [true], [true], -1));
    }
}
