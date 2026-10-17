using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ForestryRangeTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string Action = "22222222-2222-4222-8222-222222222222";
    private static ForestryRequest Request => new(Session, Session, FullRange: true);
    private static NativeRemovalTarget Tree(int id, string species, int x, int z, bool leftover = false, bool grown = true) =>
        new(new Guid(id, 0, 0, new byte[8]), "planted", species, new(x, 1, z), "demolition_mark", true, false,
            "unknown", "synthetic", new("alive", false, false, grown, grown ? 1 : .5f, leftover, !leftover, "Log", 2, true));

    private sealed class Port : IForestryPort
    {
        public List<NativeRemovalTarget> Objects = [Tree(1, "SyntheticPine", 1, 2), Tree(2, "SyntheticPine", 17, 9, grown: false),
            Tree(3, "SyntheticPine", 18, 9, leftover: true), Tree(4, "SyntheticOak", 19, 9), Tree(5, "SyntheticPine", 20, 9)];
        public HashSet<Position> Marks;
        public Position[] Cells;
        public int Writes, TreeReads;
        public int BulkPageSize { get; set; } = 32;
        public int LoseAfter = 1;
        public bool Supported = true, LostAck, ChangeTarget, ChangeOther, BadPage, BadCutPage, Unpaused, FailReadback;
        public int? RangeTotal;
        public Port() { Cells = Objects.Take(4).Select(t => t.Position).ToArray(); Marks = Objects.Select(t => t.Position).ToHashSet(); }
        private static Task<BridgeEnvelope<T>> E<T>(T data) => Task.FromResult(new BridgeEnvelope<T>(1, Session, DateTimeOffset.UnixEpoch, "0.37.0", data));
        public Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct) => E(new NativeSimulation(Unpaused ? 1 : 0, 1, 0, 0));
        public Task<BridgeEnvelope<NativeAreaTypes>> Types(CancellationToken ct) => E(new NativeAreaTypes([], [
            new("SyntheticPine", "tree_planting", "Wood", true), new("SyntheticOak", "tree_planting", "Wood", true),
            new("SyntheticHerb", "tree_planting", "Wood", true)], []));
        public Task<BridgeEnvelope<NativeRange>> Range(ForestryRequest r, int offset, CancellationToken ct) {
            var all = Supported ? Cells : [];
            return E(new NativeRange(Session, Supported, [], offset, BulkPageSize, RangeTotal ?? all.Length,
                all.Skip(offset).Take(BulkPageSize).ToArray(), offset + BulkPageSize < all.Length, [], "native"));
        }
        public Task<BridgeEnvelope<NativeAreas>> Cutting(int offset, CancellationToken ct) {
            if (FailReadback && Writes > 0) throw new IOException("private_diagnostic_must_not_escape");
            var all = Marks.OrderBy(p => p.Z).ThenBy(p => p.Y).ThenBy(p => p.X).Select(p => new NativeAreaCell(p, "marked")).ToArray();
            return E(new NativeAreas("tree_cutting", true, offset, BulkPageSize, BadCutPage ? all.Length + 1 : all.Length, all.Skip(offset).Take(BulkPageSize).ToArray(), offset + BulkPageSize < all.Length, []));
        }
        public Task<BridgeEnvelope<NativeRemovalTargets>> Trees(ForestryRequest r, int z, int offset, CancellationToken ct) {
            TreeReads++;
            var all = Objects.Where(t => t.Position.X >= r.X && t.Position.X < r.X + r.Width && t.Position.Y >= r.Y &&
                t.Position.Y < r.Y + r.Height && t.Position.Z == z).ToArray();
            if (ChangeTarget && r.Width == 1) all = [];
            return E(new NativeRemovalTargets("all", offset, 32, BadPage ? all.Length + 1 : all.Length,
                all.Skip(offset).Take(32).ToArray(), offset + 32 < all.Length, []));
        }
        public Task<BridgeEnvelope<NativeGoods>> Goods(int offset, CancellationToken ct) => E(new NativeGoods(
            "global_registered_goods", offset, 32, 1, [new("Log", "Logs", "Material", "Wood", 2, 27, 0, 0, 0, 0, 0, 0, 0, 100)], false, []));
        public Task<BridgeEnvelope<NativeOperation>> Operation(string session, string id, CancellationToken ct) => throw new NotSupportedException();
        public Task<BridgeEnvelope<NativeAreaChange>> Mark(ForestryRequest r, Position p, CancellationToken ct) => throw new NotSupportedException();
        public Task<BridgeEnvelope<NativeAreaChange>> Protect(ForestryRequest r, Position p, CancellationToken ct) {
            Writes++; Marks.Remove(p);
            if (ChangeOther) Marks.Remove(Objects[3].Position);
            if (LostAck && Writes == LoseAfter) throw new IOException("synthetic_lost_ack");
            return E(new NativeAreaChange("tree_cutting", "remove", "applied", [new(p, "unmarked")], []));
        }
    }

    [Fact]
    public async Task Entire_range_includes_far_and_high_young_trees_but_separates_stumps_and_outside_origins()
    {
        var port = new Port();
        var report = await ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken);
        var pine = Assert.Single(report.Species, s => s.Template == "SyntheticPine");
        Assert.Equal(3, pine.Total); Assert.Equal(2, pine.Living); Assert.Equal(1, pine.Leftovers);
        Assert.Equal(1, pine.MatureLiving); Assert.Equal(2, pine.MarkedLiving);
        Assert.Equal(4, report.RangeCells); Assert.Equal(2, report.ScannedRegions); Assert.Equal(2, port.TreeReads);
        var json = JsonSerializer.Serialize(report, NativeJson.Options);
        Assert.DoesNotContain("reachableTrees", json); Assert.DoesNotContain("markedCells", json);
    }

    [Fact]
    public async Task Forester_plantable_herb_is_not_a_tree_and_old_identity_is_unknown()
    {
        var port = new Port();
        var herb = Tree(6, "SyntheticHerb", 4, 2) with {
            Vegetation = new("alive", false, false, true, 1, null, null, null, null, false) };
        port.Objects.Add(herb); port.Cells = [.. port.Cells, herb.Position]; port.Marks.Add(herb.Position);
        var report = await ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken);
        Assert.DoesNotContain(report.Species, s => s.Template == "SyntheticHerb");
        Assert.Equal(1, report.Excluded["not_tree"]);
        port.Objects[0] = port.Objects[0] with { Vegetation = new("alive", false, false, true, 1, false, true, "Log", 2) };
        var error = await Assert.ThrowsAsync<BridgeRejectionException>(() => ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken));
        Assert.Equal("forestry_tree_identity_unknown", error.Code);
    }

    [Fact]
    public async Task Bulk_pages_keep_identical_coverage_and_reduce_reads_and_final_check_does_not_scan_trees()
    {
        var port = new Port();
        port.Cells = Enumerable.Range(0, 446).Select(i => new Position(i % 32, i / 32, 2)).ToArray();
        port.Marks = Enumerable.Range(0, 300).Select(i => new Position(i % 32, i / 32, 2)).ToHashSet();
        var small = await ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken);
        port.BulkPageSize = 256;
        var bulk = await ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken);
        Assert.Equal(small.Species, bulk.Species);
        Assert.Equal(small.Candidates, bulk.Candidates);
        Assert.Equal(small.RangeCells, bulk.RangeCells);
        Assert.True(small.MarkedCells.SetEquals(bulk.MarkedCells));
        Assert.Equal(20, small.NativeReads - bulk.NativeReads); // range 14->2, markings 10->2
        int treeReads = port.TreeReads;
        var final = await ForestrySurvey.ReadMarkings(Session, port, TestContext.Current.CancellationToken);
        Assert.Equal(4, final.NativeReads); // pause + two complete pages + pause
        Assert.Equal(treeReads, port.TreeReads);
        Assert.True(port.Marks.SetEquals(final.MarkedCells));
    }

    [Fact]
    public async Task Final_marking_read_rejects_unknown_or_invalid_page_size()
    {
        var port = new Port { BulkPageSize = 64 };
        await Assert.ThrowsAsync<InvalidDataException>(() => ForestrySurvey.ReadMarkings(Session, port, TestContext.Current.CancellationToken));
        Assert.Equal(0, port.Writes);
        port.BulkPageSize = 256; port.BadCutPage = true;
        await Assert.ThrowsAsync<InvalidDataException>(() => ForestrySurvey.ReadMarkings(Session, port, TestContext.Current.CancellationToken));
        port.BadCutPage = false; port.Unpaused = true;
        await Assert.ThrowsAsync<BridgeRejectionException>(() => ForestrySurvey.ReadMarkings(Session, port, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Range_paging_beyond_first_page_is_complete()
    {
        var port = new Port();
        port.Objects = Enumerable.Range(1, 33).Select(i => Tree(i, "SyntheticPine", i, 3)).ToList();
        port.Cells = port.Objects.Select(t => t.Position).ToArray(); port.Marks.Clear();
        var result = await ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken);
        Assert.Equal(33, result.RangeCells); Assert.Equal(33, Assert.Single(result.Species).Living);
        Assert.Equal(33, result.TotalCandidates); Assert.Equal(16, result.Candidates.Length); Assert.True(result.CandidatesTruncated);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("overflow")]
    [InlineData("incomplete")]
    public async Task Missing_or_partial_range_never_produces_partial_success(string failure)
    {
        var port = new Port { Supported = failure != "unknown", RangeTotal = failure == "overflow" ? 2049 : null, BadPage = failure == "incomplete" };
        if (failure == "incomplete") await Assert.ThrowsAsync<InvalidDataException>(() => ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken));
        else await Assert.ThrowsAsync<BridgeRejectionException>(() => ForestrySurvey.Observe(Request, port, TestContext.Current.CancellationToken));
        Assert.Equal(0, port.Writes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Protection_preserves_other_species_leftovers_and_outside_range_and_never_replays(bool lostAck)
    {
        var port = new Port { LostAck = lostAck };
        string dir = Path.Combine(Path.GetTempPath(), "forestry-range-" + Guid.NewGuid().ToString("N"));
        try {
            var request = new ForestryProtectionRequest(Session, Session, "SyntheticPine", Action);
            var args = JsonSerializer.SerializeToElement(request, NativeJson.Options);
            var result = await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken);
            string expectedState = lostAck ? "unconfirmed" : "completed";
            Assert.True(result["data"]!["state"]!.GetValue<string>() == expectedState,
                $"Expected {expectedState}; forestry receipt: {result["data"]!.ToJsonString()}; writes={port.Writes}");
            Assert.Equal(!lostAck, result["data"]!["readbackConfirmed"]!.GetValue<bool>());
            Assert.Equal(lostAck ? 1 : 2, port.Writes);
            foreach (var tree in port.Objects.Skip(2)) Assert.Contains(tree.Position, port.Marks);
            await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal(lostAck ? 1 : 2, port.Writes);
            await Assert.ThrowsAsync<ArgumentException>(() => ForestryTools.Execute(ForestryTools.Protect,
                JsonSerializer.SerializeToElement(request with { Template = "SyntheticOak" }, NativeJson.Options), true, port, dir, TestContext.Current.CancellationToken));
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Theory]
    [InlineData("budget")]
    [InlineData("unknown_template")]
    [InlineData("changed_target")]
    [InlineData("gate")]
    public async Task Preconditions_fail_before_mutation(string failure)
    {
        var port = new Port { ChangeTarget = failure == "changed_target" };
        string dir = Path.Combine(Path.GetTempPath(), "forestry-range-" + Guid.NewGuid().ToString("N"));
        try {
            var request = new ForestryProtectionRequest(Session, Session, failure == "unknown_template" ? "Missing" : "SyntheticPine", Action,
                failure == "budget" ? 1 : 256);
            var args = JsonSerializer.SerializeToElement(request, NativeJson.Options);
            if (failure == "changed_target") {
                var result = await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken);
                Assert.Equal("stopped", result["data"]!["state"]!.GetValue<string>());
            } else if (failure == "gate") await Assert.ThrowsAsync<ArgumentException>(() => ForestryTools.Execute(ForestryTools.Protect, args, false, port, dir, TestContext.Current.CancellationToken));
            else await Assert.ThrowsAsync<BridgeRejectionException>(() => ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken));
            Assert.Equal(0, port.Writes);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Changed_target_receipt_survives_transient_exchange_without_game_retry(bool publishedBeforeError)
    {
        var port = new Port { ChangeTarget = true };
        string dir = Path.Combine(Path.GetTempPath(), "forestry-exchange-" + Guid.NewGuid().ToString("N"));
        try {
            int attempts = 0;
            void Replace(string temp, string target) {
                attempts++;
                if (publishedBeforeError || attempts == 3) {
                    File.WriteAllBytes(target, File.ReadAllBytes(temp));
                    File.Delete(temp);
                    if (!publishedBeforeError) return;
                }
                throw new IOException("synthetic exchange failure", unchecked((int)0x80070497));
            }
            var args = JsonSerializer.SerializeToElement(
                new ForestryProtectionRequest(Session, Session, "SyntheticPine", Action), NativeJson.Options);
            var result = await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir,
                TestContext.Current.CancellationToken, Replace);
            Assert.Equal("stopped", result["data"]!["state"]!.GetValue<string>());
            Assert.Equal("forestry_tree_changed", result["data"]!["reason"]!.GetValue<string>());
            Assert.Equal(publishedBeforeError ? 1 : 3, attempts);
            Assert.Equal(0, port.Writes);
            var replay = await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir,
                TestContext.Current.CancellationToken, Replace);
            Assert.Equal("forestry_tree_changed", replay["data"]!["reason"]!.GetValue<string>());
            Assert.Equal(publishedBeforeError ? 1 : 3, attempts);
            Assert.Equal(0, port.Writes);
            Assert.Empty(Directory.EnumerateFiles(dir, "*.tmp"));
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task Partial_receipt_keeps_confirmed_prefix_and_noop_needs_no_write()
    {
        string dir = Path.Combine(Path.GetTempPath(), "forestry-range-" + Guid.NewGuid().ToString("N"));
        try {
            var port = new Port { LostAck = true, LoseAfter = 2 };
            var args = JsonSerializer.SerializeToElement(new ForestryProtectionRequest(Session, Session, "SyntheticPine", Action), NativeJson.Options);
            var result = await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal("unconfirmed", result["data"]!["state"]!.GetValue<string>());
            Assert.Equal(1, result["data"]!["confirmed"]!.GetValue<int>());
            await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal(2, port.Writes);
            var noop = new Port(); noop.Marks.Clear();
            result = await ForestryTools.Execute(ForestryTools.Protect,
                JsonSerializer.SerializeToElement(new ForestryProtectionRequest(Session, Session, "SyntheticPine", "33333333-3333-4333-8333-333333333333"), NativeJson.Options),
                true, noop, dir, TestContext.Current.CancellationToken);
            Assert.Equal("completed", result["data"]!["state"]!.GetValue<string>());
            Assert.Equal(0, result["data"]!["selected"]!.GetValue<int>());
            Assert.Equal(0, noop.Writes);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task Readback_failure_reports_phase_without_private_message_and_never_replays()
    {
        var port = new Port { FailReadback = true };
        string dir = Path.Combine(Path.GetTempPath(), "forestry-range-" + Guid.NewGuid().ToString("N"));
        try {
            var args = JsonSerializer.SerializeToElement(new ForestryProtectionRequest(Session, Session, "SyntheticPine", Action), NativeJson.Options);
            var result = await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal("stopped", result["data"]!["state"]!.GetValue<string>());
            Assert.StartsWith("readback_io_", result["data"]!["reason"]!.GetValue<string>());
            Assert.DoesNotContain("private_diagnostic", result.ToJsonString());
            Assert.Equal(2, result["data"]!["confirmed"]!.GetValue<int>());
            Assert.False(result["data"]!["readbackConfirmed"]!.GetValue<bool>());
            var replay = await ForestryTools.Execute(ForestryTools.Protect, args, true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal(result["data"]!.ToJsonString(), replay["data"]!.ToJsonString());
            Assert.Equal(2, port.Writes);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task Intervening_other_marking_change_is_reported_without_undo()
    {
        var port = new Port { ChangeOther = true };
        string dir = Path.Combine(Path.GetTempPath(), "forestry-range-" + Guid.NewGuid().ToString("N"));
        try {
            var result = await ForestryTools.Execute(ForestryTools.Protect,
                JsonSerializer.SerializeToElement(new ForestryProtectionRequest(Session, Session, "SyntheticPine", Action), NativeJson.Options),
                true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal("stopped", result["data"]!["state"]!.GetValue<string>());
            Assert.False(result["data"]!["readbackConfirmed"]!.GetValue<bool>());
            Assert.DoesNotContain(port.Objects[3].Position, port.Marks);
            var saved = JsonSerializer.Deserialize<ForestryProtectionReport>(
                File.ReadAllText(Path.Combine(dir, Session + "-protect-" + Action + ".json")), NativeJson.Options)!;
            Assert.Equal("stopped", saved.State);
            Assert.Equal(2, saved.Confirmed);
            Assert.False(saved.ReadbackConfirmed);
            Assert.Empty(Directory.EnumerateFiles(dir, "*.tmp"));
            var replay = await ForestryTools.Execute(ForestryTools.Protect,
                JsonSerializer.SerializeToElement(new ForestryProtectionRequest(Session, Session, "SyntheticPine", Action), NativeJson.Options),
                true, port, dir, TestContext.Current.CancellationToken);
            Assert.Equal(result["data"]!.ToJsonString(), replay["data"]!.ToJsonString());
            Assert.Equal(2, port.Writes);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
}
