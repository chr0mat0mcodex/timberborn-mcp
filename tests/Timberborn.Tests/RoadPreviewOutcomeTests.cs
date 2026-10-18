using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

// Synthetic observations test the policy, not Timberborn services or cleanup.
public sealed class RoadPreviewOutcomeTests
{
    [Theory]
    [InlineData(true, true, 0, true, "unknown", "construction_and_road_node_coverage_unproven")]
    [InlineData(true, true, 2, true, "blocked", "existing_district_access_lost")]
    [InlineData(true, null, 0, true, "unknown", "construction_and_road_node_coverage_unproven")]
    [InlineData(true, null, 2, true, "blocked", "existing_district_access_lost")]
    [InlineData(true, false, 0, false, "unknown", "navigation_not_restored")]
    [InlineData(true, false, 2, false, "unknown", "navigation_not_restored")]
    [InlineData(false, true, 0, false, "unknown", "navigation_not_restored")]
    [InlineData(false, true, 2, false, "unknown", "navigation_not_restored")]
    [InlineData(false, null, 0, false, "unknown", "navigation_not_restored")]
    [InlineData(false, null, 2, false, "unknown", "navigation_not_restored")]
    [InlineData(false, false, 0, false, "unknown", "navigation_not_restored")]
    [InlineData(false, false, 2, false, "unknown", "navigation_not_restored")]
    public void RestorationAndLossRetainTheirPrecedence(bool navigationRestored,
        bool? constructionRestored, int lost, bool expectedRestored, string expectedStatus, string expectedReason)
    {
        var result = RoadProtectionPolicy.EvaluatePreview(navigationRestored, constructionRestored, lost);

        Assert.Equal(expectedRestored, result.Restored);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedReason, result.Reason);
    }

    [Fact]
    public void InvalidLossCountIsRejectedEvenWhenRestorationFailed() =>
        Assert.Throws<ArgumentException>(() => RoadProtectionPolicy.EvaluatePreview(false, null, -1));
}
