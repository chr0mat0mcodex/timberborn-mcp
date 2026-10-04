using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ColonyOverviewTests
{
    private static BridgeEnvelope<T> Envelope<T>(T data) => new(1,
        "11111111-1111-4111-8111-111111111111", DateTimeOffset.UnixEpoch, "test", data);
    private static Task<BridgeEnvelope<NativeSnapshot>> Snapshot(CancellationToken ct) => Task.FromResult(Envelope(
        new NativeSnapshot("colony", [], new(7, 1, 0, 8), new(5, 2, 0),
            new(new(5, 2, 0, 5, 0), new(0, 0, 0, 0, 0)), new(64, 64, 16), 0, [], false, [])));
    private static BridgeEnvelope<NativeNeeds> Page(int offset, int total) => Envelope(new NativeNeeds(
        "living_beavers_with_need_manager", 7, 6, 1, offset, 32, total,
        Enumerable.Range(offset, Math.Min(32, total - offset)).Select(i => new NeedSummary(
            $"Need{i:D3}", "Synthetic need", 6, i == 0 ? 0 : 6,
            i == 0 ? 0 : 2, i == 0 ? 0 : 3, i == 0 ? 0 : 1, i == 0 ? 0 : 4,
            i == 0 ? null : 0f, i == 0 ? null : 1f, i == 0 ? null : 0.5d)).ToArray(),
        offset + 32 < total, [], 2, 1));

    [Theory]
    [InlineData(0, 1, true, 0)]
    [InlineData(42, 2, true, 42)]
    [InlineData(129, 4, false, 128)]
    public async Task AggregationIsBoundedAndReportsCoverage(int total, int reads, bool complete, int count)
    {
        var offsets = new List<int>();
        var result = await ColonyOverviewTools.Observe(Snapshot, (offset, ct) => {
            offsets.Add(offset); return Task.FromResult(Page(offset, total));
        }, TestContext.Current.CancellationToken);
        Assert.Equal(reads, offsets.Count);
        Assert.Equal(Enumerable.Range(0, reads).Select(i => i * 32), offsets);
        var needs = result.Data.Needs;
        Assert.Equal(count, needs.ReadCount);
        Assert.Equal(complete, needs.Complete);
        Assert.Equal(complete ? (int?)null : count, needs.NextOffset);
        Assert.True(needs.CountsStable);
        Assert.False(result.Data.Atomic);
        Assert.Equal(1, needs.MissingNeedManagers);
        Assert.Equal(2, needs.DeadExcluded);
        Assert.Equal(1, needs.UnknownLifeStateExcluded);
        if (count > 0) Assert.Equal("Need000", Assert.Single(needs.DisabledIds));
        Assert.All(needs.Items, n => { Assert.Equal(1, n.Critical); Assert.Equal(4, n.Unfavorable); });
        var json = JsonSerializer.Serialize(result, NativeJson.Options);
        Assert.DoesNotContain("objectSample", json);
        Assert.DoesNotContain("averagePoints", json);
    }

    [Theory]
    [InlineData("session")]
    [InlineData("version")]
    [InlineData("total")]
    [InlineData("duplicate")]
    public async Task MixedObservationsAreRejectedWithoutRetry(string change)
    {
        int reads = 0;
        var error = await Assert.ThrowsAsync<BridgeRejectionException>(() => ColonyOverviewTools.Observe(Snapshot, (offset, ct) => {
            reads++;
            var page = Page(offset, 42);
            if (offset > 0) page = change switch {
                "session" => page with { SessionId = "22222222-2222-4222-8222-222222222222" },
                "version" => page with { BridgeVersion = "other" },
                "total" => page with { Data = page.Data with { Total = 43 } },
                _ => page with { Data = page.Data with { Items = page.Data.Items.Select(n => n with { Id = "Need000" }).ToArray() } }
            };
            return Task.FromResult(page);
        }, TestContext.Current.CancellationToken));
        Assert.NotNull(error);
        Assert.Equal(2, reads);
    }

    [Fact]
    public async Task ChangingCountsAreExplicitAndCancellationStopsPaging()
    {
        var result = await ColonyOverviewTools.Observe(Snapshot, (offset, ct) => {
            var page = Page(offset, 42);
            return Task.FromResult(offset == 0 ? page : page with { Data = page.Data with { Beavers = 8 } });
        }, TestContext.Current.CancellationToken);
        Assert.False(result.Data.Needs.CountsStable);
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        int reads = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ColonyOverviewTools.Observe(Snapshot, (offset, ct) => {
            reads++; cancel.Cancel(); return Task.FromResult(Page(offset, 42));
        }, cancel.Token));
        Assert.Equal(1, reads);
    }

    [Fact]
    public void CatalogRegistersReadOnlyOverviewWithReasoning()
    {
        var tool = Assert.Single(NativeTools.Catalog(), t => t.Name == ColonyOverviewTools.Name);
        Assert.True(tool.Annotations!.ReadOnlyHint);
        Assert.Contains(tool.InputSchema.GetProperty("required").EnumerateArray(), p => p.GetString() == "reasoning");
    }
}
