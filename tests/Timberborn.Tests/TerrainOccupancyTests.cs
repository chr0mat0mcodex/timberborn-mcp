using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class TerrainOccupancyTests
{
    [Theory]
    [InlineData(false,true,false,false,false)] // Empty pump cell inside the bank.
    [InlineData(true,true,false,false,true)] // Actual occupied cell still collides.
    [InlineData(true,false,false,false,false)]
    [InlineData(true,true,true,false,false)]
    [InlineData(true,true,false,true,false)]
    public void OnlyOccupiedNonUndergroundCellsConflictWithTerrain(bool occupied,bool terrain,
        bool underground,bool optional,bool expected) =>
        Assert.Equal(expected,TerrainOccupancyPolicy.Intersects(occupied,terrain,underground,optional));

    [Fact]
    public void EmptyCellsDoNotHideAdjacentOccupiedCollision()
    {
        var occupied=new[]{false,false,true,false};
        Assert.Contains(occupied,cell=>TerrainOccupancyPolicy.Intersects(cell,true,false,false));
        Assert.DoesNotContain(occupied.Take(2),cell=>TerrainOccupancyPolicy.Intersects(cell,true,false,false));
    }
}
