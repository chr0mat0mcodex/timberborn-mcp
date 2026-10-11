using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class OccupationMaskDomainTests
{
    [Fact]
    public void NegativeAllAndSparseBitsAreRealMasksNotRangeBounds()
    {
        var masks=OccupationMaskDomain.Create([0,1,256,257,-1],-1);
        Assert.Equal(new[]{0,1,256,257,-1},masks);
    }
    [Fact]
    public void CompositeAllIsNotDuplicatedAndUnknownBitsFailClosed()
    {
        Assert.Equal(new[]{0,1,2,3},OccupationMaskDomain.Create([0,1,2,3],3));
        Assert.Empty(OccupationMaskDomain.Create([0,1,6,-1],-1));
        Assert.Empty(OccupationMaskDomain.Create(Enumerable.Range(0,9).Select(i=>1<<i),-1));
    }
    [Fact]
    public void MaximumDomainIsBoundedEvenWithSentinel()
    {
        var masks=OccupationMaskDomain.Create(Enumerable.Range(0,8).Select(i=>1<<i).Append(-1),-1);
        Assert.Equal(257,masks.Length);
        Assert.Equal(masks.Length,masks.Distinct().Count());
    }
}
