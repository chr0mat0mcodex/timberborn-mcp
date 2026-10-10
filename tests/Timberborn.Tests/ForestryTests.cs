using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ForestryTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string TreeId = "22222222-2222-4222-8222-222222222222";
    private static readonly ForestryRequest Request = new(Session, Session, 0, 0, 2, 8, 8, 2);
    private static NativeRemovalTarget Tree => new(Guid.Parse(TreeId), "vegetation", "SyntheticTree", new(2, 2, 2),
        "demolition_mark", true, false, "unknown", "vegetation", new("alive", false, false, true, 1, false, true, "Log", 2));
    private sealed class Port : IForestryPort
    {
        public bool Supported = true, LostAck, BadPage, Stale;
        public int Writes;
        private Task<BridgeEnvelope<T>> E<T>(T data) => Task.FromResult(new BridgeEnvelope<T>(1,
            Stale ? TreeId : Session, DateTimeOffset.UnixEpoch, "0.35.4", data));
        public Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct) => E(new NativeSimulation(0, 1, 0, 0));
        public Task<BridgeEnvelope<NativeAreaTypes>> Types(CancellationToken ct) => E(new NativeAreaTypes([], [new("SyntheticTree", "tree_planting", "Wood", true)], []));
        public Task<BridgeEnvelope<NativeRange>> Range(ForestryRequest r, int offset, CancellationToken ct) => E(new NativeRange(
            Session, Supported, [], offset, 32, Supported ? 1 : 0, Supported ? [Tree.Position] : [], false, [], "native"));
        public Task<BridgeEnvelope<NativeAreas>> Cutting(int offset, CancellationToken ct) => E(new NativeAreas(
            "tree_cutting", true, offset, 32, 0, [], false, []));
        public Task<BridgeEnvelope<NativeRemovalTargets>> Trees(ForestryRequest r, int z, int offset, CancellationToken ct) => E(new NativeRemovalTargets(
            "all", offset, 32, BadPage ? 2 : z == 2 ? 1 : 0, z == 2 ? [Tree] : [], false, []));
        public Task<BridgeEnvelope<NativeGoods>> Goods(int offset, CancellationToken ct) => E(new NativeGoods(
            "global_registered_goods", offset, 32, 1, [new("Log", "Logs", "Material", "Wood", 2, 27, 0, 0, 0, 0, 0, 0, 0, 100)], false, []));
        public Task<BridgeEnvelope<NativeOperation>> Operation(string session, string id, CancellationToken ct) => E(new NativeOperation(
            id, "SyntheticConsumer", true, false, null, null, [], [], [new("Input", 10, 9, true, false, false, false, false, false, true, [new("Log", 9, 9, 0, 1)])]));
        public Task<BridgeEnvelope<NativeAreaChange>> Mark(ForestryRequest r, Position p, CancellationToken ct) {
            Writes++; if (LostAck) throw new IOException("synthetic_lost_ack");
            return E(new NativeAreaChange("tree_cutting", "mark", "applied", [new(p, "marked")], []));
        }
    }

    [Fact]
    public async Task Survey_combines_heights_and_keeps_unknown_out_of_candidates() {
        var report = await ForestrySurvey.Observe(Request, new Port(), TestContext.Current.CancellationToken);
        Assert.Equal(Tree.Id, Assert.Single(report.Candidates).Id);
        Assert.Equal(2, report.AvailableLogs); Assert.Equal(27, report.AllLogs);
        Assert.Equal(8, report.NativeReads);
        var withConsumer = await ForestrySurvey.Observe(Request with { ConsumerIds = [Session] }, new Port(), TestContext.Current.CancellationToken);
        Assert.Equal(9, Assert.Single(withConsumer.Consumers).StoredLogs); Assert.Equal(2, withConsumer.AvailableLogs);
        report = await ForestrySurvey.Observe(Request, new Port { Supported = false }, TestContext.Current.CancellationToken);
        Assert.Empty(report.Candidates); Assert.Equal(1, report.Excluded["range_unknown"]);
    }

    [Fact]
    public void Immature_marked_dead_and_unreachable_trees_are_not_candidates() {
        var templates = new HashSet<string> { Tree.Template }; var range = new HashSet<Position> { Tree.Position };
        Assert.Equal("not_observed_mature", ForestrySurvey.Exclusion(Tree with { Vegetation = new("alive", false, false, false, .5f, false, true, "Log", 2) }, templates, true, range, new HashSet<Position>()));
        Assert.Equal("already_marked", ForestrySurvey.Exclusion(Tree, templates, true, range, range));
        Assert.Equal("outside_range", ForestrySurvey.Exclusion(Tree, templates, true, new HashSet<Position>(), new HashSet<Position>()));
        Assert.Equal("not_observed_alive", ForestrySurvey.Exclusion(Tree with { Vegetation = new("dead", null, null, true, 1, false, true, "Log", 2) }, templates, true, range, new HashSet<Position>()));
    }

    [Theory]
    [InlineData(true, true, "Log", 2, "harvested_leftover")]
    [InlineData(null, null, null, null, "cut_yield_unknown")]
    [InlineData(false, true, "Log", 0, "no_log_yield")]
    [InlineData(false, true, "Berries", 2, "no_log_yield")]
    [InlineData(false, false, "Log", 2, "not_currently_cuttable")]
    public void Mature_alive_leftovers_never_become_felling_candidates(bool? removed, bool? yielding, string? good, int? amount, string reason) {
        var tree = Tree with { Vegetation = new("alive", false, false, true, 1, removed, yielding, good, amount) };
        Assert.Equal(reason, ForestrySurvey.Exclusion(tree, new HashSet<string> { tree.Template }, true,
            new HashSet<Position> { tree.Position }, new HashSet<Position>()));
    }

    [Fact]
    public async Task Broken_pages_or_session_never_yield_a_partial_recommendation() {
        await Assert.ThrowsAsync<InvalidDataException>(() => ForestrySurvey.Observe(Request, new Port { BadPage = true }, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<BridgeRejectionException>(() => ForestrySurvey.Observe(Request, new Port { Stale = true }, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Saved_mark_receipt_prevents_repeat_including_lost_ack(bool lost) {
        string dir = Path.Combine(Path.GetTempPath(), "forestry-test-" + Guid.NewGuid().ToString("N"));
        try {
            var request = new ForestryMarkRequest(Request, "33333333-3333-4333-8333-333333333333", [TreeId]);
            var args = JsonSerializer.SerializeToElement(request, NativeJson.Options);
            var port = new Port { LostAck = lost };
            var first = await ForestryTools.Execute(ForestryTools.Mark, args, true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal(lost ? "unconfirmed" : "completed", first["data"]!["state"]!.GetValue<string>());
            await ForestryTools.Execute(ForestryTools.Mark, args, true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal(1, port.Writes);
            var changed = JsonSerializer.SerializeToElement(request with { Region = Request with { Depth = 1 } }, NativeJson.Options);
            await Assert.ThrowsAsync<ArgumentException>(() => ForestryTools.Execute(ForestryTools.Mark, changed, true, port, dir, TestContext.Current.CancellationToken));
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public void Catalog_exposes_marking_only_with_area_gate() {
        var reader = Assert.Single(ForestryTools.Catalog(false));
        Assert.Equal(ForestryTools.Inspect, reader.Name);
        Assert.True(reader.Annotations!.ReadOnlyHint);
        Assert.False(reader.Annotations.DestructiveHint);
        var writer = Assert.Single(ForestryTools.Catalog(true), t => t.Name == ForestryTools.Mark);
        Assert.False(writer.Annotations!.ReadOnlyHint);
        Assert.True(writer.Annotations.DestructiveHint);
        Assert.True(writer.Annotations.IdempotentHint);
        Assert.Equal(2, ForestryTools.Catalog(true).Count());
        Assert.All(ForestryTools.Catalog(true), t => Assert.Equal("object", t.InputSchema.GetProperty("type").GetString()));
    }
}
