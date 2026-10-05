using Timberborn.Backend.Native;
using Xunit;

namespace Timberborn.Tests;

public sealed class ValidationBudgetContractTests
{
    [Theory]
    [InlineData("0.35.1", 7)]
    [InlineData("0.35.1", 15)]
    [InlineData("0.35.1", 255)]
    [InlineData("0.35.2", 7)]
    [InlineData("0.35.2", 15)]
    [InlineData("0.35.2", 255)]
    [InlineData("0.35.3", 255)]
    [InlineData("0.35.4", 255)]
    public void ExpandedBudgetAcceptsFirstAndLastReceiptButRejectsInvalidCounters(string version, int legacyMaximum)
    {
        Assert.True(ValidationBudgetContract.Accepts(version, 999_999, legacyMaximum));
        Assert.True(ValidationBudgetContract.Accepts(version, 0, legacyMaximum));
        Assert.True(ValidationBudgetContract.Accepts(version, legacyMaximum, legacyMaximum));
        Assert.False(ValidationBudgetContract.Accepts(version, 1_000_000, legacyMaximum));
        Assert.False(ValidationBudgetContract.Accepts(version, -1, legacyMaximum));
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
