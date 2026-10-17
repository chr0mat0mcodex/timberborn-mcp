using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class OperatingReadinessTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string Center = "22222222-2222-4222-8222-222222222222";
    private const string Producer = "33333333-3333-4333-8333-333333333333";
    private static BridgeEnvelope<T> E<T>(T value) => new(1, Session, DateTimeOffset.UnixEpoch, "synthetic", value);
    private static OperatingRequest Request => new(Session, Center, [Producer], [new("Plank", 20)]);
    private static NativeOperation Operation(string id, bool flooded = false, int assigned = 1) => new(
        id, id == Center ? "DistrictCenter.Folktails" : "LumberMill.Folktails", true, false,
        new(assigned, 2, 2, assigned < 2, false, true),
        id == Center ? null : new(true, "PlankRecipe", true, true, true, false, true, 0), [], [], [],
        new(false, null, null, null, null, null), null, null, new(true, flooded, false, null, null, false, null));
    private static BridgeEnvelope<NativeSnapshot> Snapshot => E(new NativeSnapshot("colony", [], new(1, 0, 0, 1),
        new(1, 0, 0), new(new(1, 0, 0, 1, 0), new(0, 0, 0, 0, 0)), new(64, 64, 16), 0, [], false, []));
    private static BridgeEnvelope<NativeProductionGraph> Graph => E(new NativeProductionGraph("definitions", "test", "test", "test", true,
        new([], [new("PlankRecipe", [], [new("Plank", 1)], 1, null, 0, 0, ["LumberMill.Folktails"])], [], []), new([], [], []), []));
    private static BridgeEnvelope<NativeGoods> Goods(int offset, bool many = false) {
        int total = many ? 129 : 1;
        var items = many ? Enumerable.Range(offset, Math.Min(32, total - offset)).Select(i => Stock($"Good{i:D3}")).ToArray() : new[] { Stock("Plank") };
        return E(new NativeGoods("global_registered_goods", offset, 32, total, items, offset + items.Length < total, []));
    }
    private static GoodStock Stock(string id) => new(id, id, "material", "material", 5, 5, 5, 0, 0, 0, 0, 0, 10, 10);
    private static Task<BridgeEnvelope<NativeAccess>> Access(string id, CancellationToken ct) => Task.FromResult(E(
        new NativeAccess(id, true, new(1, 1, 1), false, false, false, true, 1, 1, 1, [])));
    private static Task<BridgeEnvelope<OperatingReport>> Read(OperatingRequest? request = null, bool flooded = false,
        bool missing = false, bool many = false, Func<int, CancellationToken, Task<BridgeEnvelope<NativeGoods>>>? readGoods = null,
        Func<string, CancellationToken, Task<BridgeEnvelope<NativeAccess>>>? readAccess = null, CancellationToken? token = null) =>
        OperatingReadinessTools.Observe(request ?? Request, ct => Task.FromResult(Snapshot), ct => Task.FromResult(Graph),
            ct => Task.FromResult(E(new OperationBatch([
                new(Center, "observed", Operation(Center, flooded, flooded ? 0 : 1)),
                new(Producer, missing ? "not_found" : "observed", missing ? null : Operation(Producer))], 1, []))),
            readGoods ?? ((offset, ct) => Task.FromResult(Goods(offset, many))), readAccess ?? Access,
            token ?? TestContext.Current.CancellationToken);

    [Fact]
    public async Task FloodComesBeforeStaffingAndShortageIsInformation()
    {
        var d = (await Read(flooded: true)).Data;
        Assert.Equal("building_flooded", d.NextSteps[0].Code);
        var staffing = Assert.Single(d.NextSteps, s => s.Target == Center && s.Code == "no_assigned_workers");
        Assert.True(staffing.Priority > d.NextSteps[0].Priority);
        Assert.Contains("Erst nach", staffing.Action);
        var material = Assert.Single(d.Materials);
        Assert.Equal(25d, material.CoveragePercent);
        Assert.Equal("potential_supply_unproven", material.SupplyState);
        Assert.Equal(new[] { Producer }, material.SelectedRecipeProducerIds);
        Assert.Contains(d.NextSteps, s => s.Code == "material_shortfall" && s.Action.Contains("keine pauschale Startsperre"));
        Assert.False(d.ProductionProven);
        Assert.False(d.Atomic);
        Assert.All(d.Materials, m => Assert.False(m.ProductionProven));
    }

    [Fact]
    public async Task MissingProducerDoesNotProveColonyWideAbsence()
    {
        var ids = new List<string>();
        var d = (await Read(missing: true, readAccess: (id, ct) => { ids.Add(id); return Access(id, ct); })).Data;
        Assert.Equal(new[] { Center }, ids);
        Assert.Equal("not_observed_in_selection", d.Materials[0].SupplyState);
        Assert.Contains(d.NextSteps, s => s.Target == Producer && s.Code == "building_not_found");
        Assert.Contains(d.Limitations, s => s.Contains("no_colony_wide"));
    }

    [Fact]
    public async Task BoundedGoodsScanPreservesUnknownStock()
    {
        int reads = 0;
        var d = (await Read(readGoods: (offset, ct) => { reads++; return Task.FromResult(Goods(offset, true)); })).Data;
        Assert.Equal(4, reads);
        Assert.False(d.GoodsComplete);
        Assert.Equal(128, d.GoodsNextOffset);
        Assert.Null(d.Materials[0].GlobalAvailableStock);
        Assert.Null(d.Materials[0].CoveragePercent);
        Assert.Contains(d.NextSteps, s => s.Code == "stock_unknown");
    }

    [Theory]
    [InlineData("session")]
    [InlineData("version")]
    [InlineData("total")]
    [InlineData("duplicate")]
    public async Task MixedOrChangingPagesAreRejectedWithoutRetry(string change)
    {
        int reads = 0;
        await Assert.ThrowsAsync<Timberborn.Bridge.Core.BridgeRejectionException>(() => Read(readGoods: (offset, ct) => {
            reads++; var page = Goods(offset, true);
            if (offset > 0) page = change switch {
                "session" => page with { SessionId = Center },
                "version" => page with { BridgeVersion = "other" },
                "total" => page with { Data = page.Data with { Total = 130 } },
                _ => page with { Data = page.Data with { Items = page.Data.Items.Select(s => s with { Id = "Good000" }).ToArray() } }
            };
            return Task.FromResult(page);
        }));
        Assert.InRange(reads, 2, 4);
    }

    [Fact]
    public async Task CancellationStopsReadsAndSessionMismatchStopsAtSnapshot()
    {
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        int reads = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Read(token: cancel.Token, readGoods: (offset, ct) => {
            reads++; cancel.Cancel(); return Task.FromResult(Goods(offset, true));
        }));
        Assert.Equal(1, reads);
        await Assert.ThrowsAsync<Timberborn.Bridge.Core.BridgeRejectionException>(() => Read(Request with { Session = Producer }));
    }

    [Fact]
    public async Task ZeroDemandHasNoCoverageRatio()
    {
        var result = await Read(Request with { Materials = [new("Plank", 0)] });
        Assert.Null(result.Data.Materials[0].CoveragePercent);
    }

    [Fact]
    public async Task DisconnectedProducerIsBlockedEvenWhenRecipeIsReady()
    {
        var d = (await Read(readAccess: async (id, ct) => {
            var e = await Access(id, ct);
            return id == Producer ? e with { Data = e.Data with { UnconnectedBlocked = true } } : e;
        })).Data;
        Assert.Equal("selected_producers_blocked", d.Materials[0].SupplyState);
        Assert.Contains(d.NextSteps, s => s.Target == Producer && s.Code == "access_blocked");
    }

    [Fact]
    public async Task UnknownAccessRemainsExplicitAndChangedAccessSessionIsRejected()
    {
        var d = (await Read(readAccess: async (id, ct) => {
            var e = await Access(id, ct);
            return e with { Data = e.Data with { EntranceInaccessible = null } };
        })).Data;
        Assert.Contains(d.NextSteps, s => s.Code == "access_unknown");
        Assert.False(d.ProductionProven);
        await Assert.ThrowsAsync<Timberborn.Bridge.Core.BridgeRejectionException>(() => Read(readAccess: async (id, ct) =>
            (await Access(id, ct)) with { SessionId = Producer }));
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, true)]
    [InlineData(null, false, true)]
    [InlineData(0, true, true)]
    public async Task AbsentOptionalBlockerIsNotMissingAccessEvidence(int? count, bool missingEntrance, bool expectedUnknown)
    {
        var d = (await Read(readAccess: async (id, ct) => {
            var e = await Access(id, ct);
            return e with { Data = e.Data with {
                UnconnectedBlocked = null, UnconnectedBlockerCount = count,
                EntranceInaccessible = missingEntrance ? null : false
            } };
        })).Data;
        Assert.Equal(expectedUnknown, d.NextSteps.Any(s => s.Code == "access_unknown"));
        Assert.False(d.ProductionProven);
    }

    private static string Args => JsonSerializer.Serialize(new { session = Session, constructionCenterId = Center,
        producerIds = new[] { Producer }, materials = new[] { new { good = "Plank", amount = 20 } } });

    [Fact]
    public void StrictInputAndReadOnlyCatalog()
    {
        Assert.Equal(Center, OperatingReadinessTools.Parse(JsonDocument.Parse(Args).RootElement).ConstructionCenterId);
        foreach (var json in new[] { Args.Replace(Producer, Center), Args.Replace("20", "-1"), Args.Replace("20", "1.5"),
            Args.Replace("Plank", ""), Args.Replace("\"producerIds\"", "\"unexpected\""), Args[..^1] + ",\"session\":\"" + Session + "\"}" })
            Assert.Throws<ArgumentException>(() => OperatingReadinessTools.Parse(JsonDocument.Parse(json).RootElement));
        var tool = Assert.Single(NativeTools.Catalog(), t => t.Name == OperatingReadinessTools.Name);
        Assert.True(tool.Annotations!.ReadOnlyHint);
        Assert.False(tool.Annotations.DestructiveHint);
        Assert.Contains(tool.InputSchema.GetProperty("required").EnumerateArray(), p => p.GetString() == "reasoning");
    }
}
