using System.Text.Json;
using Timberborn.Application;
using Timberborn.Backend.Abstractions;
using Timberborn.Backend.Fake;
using Timberborn.Contracts;
using Xunit;

namespace Timberborn.Tests;

public sealed class QueryTests
{
    [Theory]
    [InlineData("inspect_population", "{\"limit\":5}")]
    [InlineData("find_buildings", "{\"limit\":101}")]
    [InlineData("find_buildings", "{\"offset\":-1}")]
    [InlineData("find_buildings", "{\"paused\":null}")]
    [InlineData("find_buildings", "{\"paused\":\"true\"}")]
    [InlineData("inspect_building", "{\"id\":\"../../set-speed\"}")]
    [InlineData("inspect_building", "{}")]
    [InlineData("timberborn_status", "{\"url\":\"http://bad.invalid\"}")]
    public async Task InvalidInputsNeverCallBackend(string name, string input)
    {
        var backend = new CountingBackend();
        var result = await new ObservationService(backend).InvokeAsync(name, JsonDocument.Parse(input).RootElement, TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0, backend.Calls);
    }
    [Fact]
    public async Task FalsePausedOnlyMatchesPausableActiveBuildings()
    {
        var result = await new ObservationService(new FakeTimberbornBackend())
            .InvokeAsync("find_buildings", JsonSerializer.SerializeToElement(new { paused = false }), TestContext.Current.CancellationToken);
        Assert.Equal(1, result["data"]!["page"]!["matchedTotal"]!.GetValue<int>());
    }
    [Fact]
    public async Task FiltersAndPaginationAreConsistent()
    {
        var service = new ObservationService(new FakeTimberbornBackend());
        var page = await service.InvokeAsync("find_buildings", JsonSerializer.SerializeToElement(new { nameContains = "TEST", offset = 1, limit = 1 }), TestContext.Current.CancellationToken);
        Assert.Equal(3, page["data"]!["page"]!["matchedTotal"]!.GetValue<int>());
        Assert.Equal(2, page["data"]!["page"]!["nextOffset"]!.GetValue<int>());
        var empty = await service.InvokeAsync("find_buildings", JsonSerializer.SerializeToElement(new { offset = int.MaxValue }), TestContext.Current.CancellationToken);
        Assert.Empty(empty["data"]!["items"]!.AsArray()); Assert.Null(empty["data"]!["page"]!["nextOffset"]);
    }
    [Fact]
    public async Task EmptySearchIsNotMissingEntityError()
    {
        var service = new ObservationService(new FakeTimberbornBackend());
        var search = await service.InvokeAsync("find_buildings", JsonSerializer.SerializeToElement(new { id = Guid.NewGuid() }), TestContext.Current.CancellationToken);
        Assert.Equal("ok", search["status"]!.GetValue<string>());
        var detail = await service.InvokeAsync("inspect_building", JsonSerializer.SerializeToElement(new { id = Guid.NewGuid() }), TestContext.Current.CancellationToken);
        Assert.Equal("entity_not_found", detail["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public void UnknownBuildingStateIsNotActive()
    {
        var aggregate = ObservationService.Aggregate([
            new(Guid.NewGuid(), "test", "test", null, null), new(Guid.NewGuid(), "test", "test", false, null)]);
        Assert.Equal(0, aggregate.Active); Assert.Equal(1, aggregate.PauseStateUnknown); Assert.Equal(1, aggregate.NotPausable);
    }
    [Fact]
    public async Task LargePagesAreReducedWithoutLosingNextOffset()
    {
        var service = new ObservationService(new CountingBackend(large: true));
        var result = await service.InvokeAsync("find_buildings", JsonSerializer.SerializeToElement(new { limit = 100 }), TestContext.Current.CancellationToken);
        Assert.True(JsonSerializer.SerializeToUtf8Bytes(result).Length <= 32768);
        int count = result["data"]!["items"]!.AsArray().Count;
        Assert.InRange(count, 1, 99);
        Assert.Equal(count, result["data"]!["page"]!["nextOffset"]!.GetValue<int>());
    }
    private sealed class CountingBackend(bool large = false) : ITimberbornReadBackend
    {
        public int Calls;
        public string Id => "fake";
        public bool Simulated => true;
        private Task<T> Fail<T>() { Calls++; throw new InvalidOperationException("Unexpected call"); }
        public Task PingAsync(CancellationToken ct) => Fail<bool>();
        public Task<GameInfo> GetGameInfoAsync(CancellationToken ct) => Fail<GameInfo>();
        public Task<LiveSnapshot> GetLiveAsync(CancellationToken ct) => Fail<LiveSnapshot>();
        public Task<IReadOnlyList<CharacterSummary>> GetPopulationAsync(CancellationToken ct) => Fail<IReadOnlyList<CharacterSummary>>();
        public Task<BuildingSummary> GetBuildingAsync(Guid id, CancellationToken ct) => Fail<BuildingSummary>();
        public Task<IReadOnlyList<BuildingSummary>> GetBuildingsAsync(CancellationToken ct)
        {
            if (!large) return Fail<IReadOnlyList<BuildingSummary>>();
            Calls++;
            return Task.FromResult<IReadOnlyList<BuildingSummary>>(Enumerable.Range(0, 100)
                .Select(_ => new BuildingSummary(Guid.NewGuid(), new string('x', 200), new string('y', 200), true, false)).ToArray());
        }
    }
}
