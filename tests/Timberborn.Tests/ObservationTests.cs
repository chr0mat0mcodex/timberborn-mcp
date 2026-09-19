using System.Text.Json;
using Timberborn.Application;
using Timberborn.Backend.Fake;
using Xunit;

namespace Timberborn.Tests;

public sealed class ObservationTests
{
    [Fact]
    public async Task FakeCountsAreExplicitAndCorrect()
    {
        var service = new ObservationService(new FakeTimberbornBackend());
        var result = await service.InvokeAsync("inspect_population", JsonSerializer.SerializeToElement(new { }), TestContext.Current.CancellationToken);
        Assert.True(result["meta"]!["simulated"]!.GetValue<bool>());
        Assert.Equal(15, result["data"]!["counts"]!["beavers"]!.GetValue<int>());
        Assert.Equal(17, result["data"]!["counts"]!["totalEntities"]!.GetValue<int>());
    }
    [Fact]
    public async Task PartialColonyKeepsSuccessfulSections()
    {
        var result = await new ObservationService(new FakeTimberbornBackend("partial"))
            .InvokeAsync("inspect_colony", JsonSerializer.SerializeToElement(new { }), TestContext.Current.CancellationToken);
        Assert.Equal("partial", result["status"]!.GetValue<string>());
        Assert.Null(result["data"]!["buildings"]);
        Assert.NotNull(result["data"]!["population"]);
        Assert.Null(result["data"]!["weather"]!["next"]);
    }
    [Fact]
    public async Task OfflineStatusIsSuccessfulDiagnosis()
    {
        var result = await new ObservationService(new FakeTimberbornBackend("offline"))
            .InvokeAsync("timberborn_status", JsonSerializer.SerializeToElement(new { }), TestContext.Current.CancellationToken);
        Assert.Equal("ok", result["status"]!.GetValue<string>());
        Assert.Equal("unreachable", result["data"]!["connection"]!.GetValue<string>());
    }
}
