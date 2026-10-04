using Timberborn.Backend.Native;
using Xunit;

namespace Timberborn.Tests;

public sealed class ValidationBudgetContractTests
{
    [Theory]
    [InlineData(7)]
    [InlineData(15)]
    [InlineData(255)]
    public void ExpandedBudgetAcceptsFirstAndLastReceiptButRejectsInvalidCounters(int legacyMaximum)
    {
        Assert.True(ValidationBudgetContract.Accepts("0.35.1", 999_999, legacyMaximum));
        Assert.True(ValidationBudgetContract.Accepts("0.35.1", 0, legacyMaximum));
        Assert.True(ValidationBudgetContract.Accepts("0.35.1", legacyMaximum, legacyMaximum));
        Assert.False(ValidationBudgetContract.Accepts("0.35.1", 1_000_000, legacyMaximum));
        Assert.False(ValidationBudgetContract.Accepts("0.35.1", -1, legacyMaximum));
    }

    [Theory]
    [InlineData("0.24.1", 15)]
    [InlineData("0.35.0", 255)]
    [InlineData("0.35.0", 7)]
    public void HistoricalVersionsKeepTheirOriginalCounterBounds(string version, int maximum)
    {
        Assert.True(ValidationBudgetContract.Accepts(version, maximum, maximum));
        Assert.False(ValidationBudgetContract.Accepts(version, maximum + 1, maximum));
        Assert.False(ValidationBudgetContract.Accepts(version, -1, maximum));
    }
}
