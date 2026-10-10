using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class RegionSurveyTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private static SurveyRequest Request => new(Session, "22222222-2222-4222-8222-222222222222", "Synthetic.Folktails", 0, 0, 3, 16, 16);
    private static MapCell Cell(int x, int y) => new(x, y, 3, false, true, 3, 0, 0, false);
    private static BuildingPosition Building(string template, Position origin, Position[] cells, Position? entrance = null) =>
        new(Guid.NewGuid(), template, origin, "Cw0", true, entrance, cells, false);

    [Fact]
    public async Task FarmingSurveyCombinesAllRangePagesWithMoistureAndSkipsBuildingPlans()
    {
        var port = new Port { Moist = true };
        var report = await RegionSurvey.Observe(Request with { WorkBuildingId = Session, PlanBuildings = false }, port, TestContext.Current.CancellationToken);
        Assert.Equal(2, port.RangeReads); Assert.Equal(0, port.Plans);
        Assert.Empty(report.BuildingCandidates); Assert.Empty(report.SearchWindows);
        Assert.Equal(15, report.NativeReads);
        Assert.NotNull(report.WorkRange); Assert.True(report.WorkRange.Supported);
        Assert.Equal(40, report.WorkRange.ObservedCells);
        Assert.NotEmpty(report.WorkRange.MoistEmptyGroundPatches);
        foreach (var p in report.WorkRange.MoistEmptyGroundPatches)
            foreach (int y in Enumerable.Range(p.Y, p.Height)) foreach (int x in Enumerable.Range(p.X, p.Width)) {
                Assert.Equal('r', report.WorkRange.Rows[y][x]);
                Assert.Equal('m', report.Moisture.Rows[y][x]);
                Assert.Equal('.', report.Rows[y][x]);
            }
        Assert.All(report.WorkRange.Rows[0], c => Assert.Equal('-', c));
    }

    [Theory]
    [InlineData("range_changed")]
    [InlineData("range_duplicate")]
    [InlineData("range_bound")]
    [InlineData("range_session")]
    public async Task IncompleteRangeNeverProducesRecommendations(string fault)
    {
        await Assert.ThrowsAnyAsync<Exception>(() => RegionSurvey.Observe(Request with {
            WorkBuildingId = Session, PlanBuildings = false }, new Port { Fault = fault }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UnsupportedRangeIsUnknownAndDefaultRequestsDoNotReadRange()
    {
        var unknown = await RegionSurvey.Observe(Request with { WorkBuildingId = Session, PlanBuildings = false },
            new Port { Fault = "range_unknown", Moist = true }, TestContext.Current.CancellationToken);
        Assert.NotNull(unknown.WorkRange); Assert.False(unknown.WorkRange.Supported);
        Assert.All(unknown.WorkRange.Rows, row => Assert.Equal(new string('?', 16), row));
        Assert.Empty(unknown.WorkRange.MoistEmptyGroundPatches);
        var port = new Port();
        var defaults = await RegionSurvey.Observe(Request, port, TestContext.Current.CancellationToken);
        Assert.Null(defaults.WorkRange); Assert.Equal(0, port.RangeReads); Assert.Equal(12, port.Plans);
    }

    [Fact]
    public void MoistureSeparatesDryUnknownWrongHeightAndOccupiedGround()
    {
        var r = Request with { Width = 8, Height = 1 };
        var map = Enumerable.Range(0, 8).Select(x => Cell(x, 0) with { SoilIsMoist = true }).ToArray();
        map[2] = map[2] with { SoilIsMoist = false };
        map[3] = map[3] with { SoilIsMoist = null };
        map[4] = map[4] with { OnGround = false };
        map[5] = map[5] with { TerrainHeight = 2 };
        var report = RegionSurvey.Moisture(r, map, ["......b?"]);
        Assert.Equal("mmd?--mm", Assert.Single(report.Rows));
        Assert.Equal(new SurveyRect(0, 0, 3, 2, 1), Assert.Single(report.MoistEmptyGroundPatches));
        // Positive moisture must not turn flooded/contaminated or planted cells into free patches.
        Assert.Empty(RegionSurvey.Moisture(r, map, ["~c....b?"]).MoistEmptyGroundPatches);
    }

    [Fact]
    public async Task OldBridgeMoistureRemainsUnknownWithoutExtraReads()
    {
        var report = await RegionSurvey.Observe(Request, new Port(), TestContext.Current.CancellationToken);
        Assert.All(report.Moisture.Rows, row => Assert.Equal(new string('?', 16), row));
        Assert.Empty(report.Moisture.MoistEmptyGroundPatches);
        Assert.Equal(25, report.NativeReads);
        var oldCell = JsonSerializer.Deserialize<MapCell>("""{"x":0,"y":0,"z":3,"underground":false,"onGround":true,"terrainHeight":3,"waterDepth":0,"contamination":0,"underwater":false}""", NativeJson.Options);
        Assert.Null(oldCell!.SoilIsMoist);
    }

    [Fact]
    public void FootprintsEntrancesWaterHeightsAndEmptyDesignationsAreExcluded()
    {
        var r = Request with { Width = 8, Height = 2 };
        var map = Enumerable.Range(0, 2).SelectMany(y => Enumerable.Range(0, 8).Select(x => Cell(x, y))).ToArray();
        map[0] = map[0] with { WaterDepth = 0.5f, Underwater = true };
        map[1] = map[1] with { TerrainHeight = 2, OnGround = false };
        var building = Building("Synthetic", new(-1, 0, 3), [new(2, 0, 3), new(3, 0, 3)], new(4, 0, 3));
        var road = Building("Path", new(0, 1, 3), [new(0, 1, 3)]);
        var tree = new NativeRemovalTarget(Guid.NewGuid(), "vegetation", "SyntheticTree", new(6, 0, 3), "demolition_mark", true, false, "unknown", "current_components");
        var rows = RegionSurvey.Classify(r, map, [building, road], [tree], new HashSet<Position> { new(5, 0, 3) });
        Assert.Equal("~hbbecv.", rows[0]); Assert.Equal("p.......", rows[1]);
        var patches = RegionSurvey.Patches(r, rows);
        Assert.NotEmpty(patches);
        Assert.All(patches, p => {
            Assert.InRange(p.Width, 1, 4); Assert.InRange(p.Height, 1, 4);
            foreach (int y in Enumerable.Range(p.Y, p.Height)) foreach (int x in Enumerable.Range(p.X, p.Width)) Assert.Equal('.', rows[y][x]);
        });
    }

    [Fact]
    public async Task SurveyUsesBoundedNativePlansAndReturnsCompactNonExecutableAdvice()
    {
        var port = new Port();
        var report = await RegionSurvey.Observe(Request, port, TestContext.Current.CancellationToken);
        Assert.Equal(4, port.Maps); Assert.Equal(12, port.Plans); Assert.Equal(12, report.PlanningCalls);
        Assert.Equal(25, report.NativeReads); // 2 speed + 4 map + 4 targets + 1 objects + 2 designations + 12 plans
        Assert.Equal(16, report.Rows.Length); Assert.All(report.Rows, row => Assert.Equal(16, row.Length));
        Assert.Equal(3, report.SearchWindows.Length); Assert.Equal(4, report.BuildingCandidates.Length);
        Assert.False(report.Atomic);
        Assert.Contains("patches_are_not_planting_or_work_range_validation", report.Limitations);
        Assert.All(report.BuildingCandidates, c => Assert.InRange(c.Region.Width, 1, 8));
        var input = RegionSurveyTools.Reader().InputSchema;
        Assert.Equal("object", input.GetProperty("type").GetString());
        Assert.True(RegionSurveyTools.Reader().Annotations!.ReadOnlyHint);
        Assert.DoesNotContain("occupiedCells", JsonSerializer.Serialize(report, NativeJson.Options));
    }

    [Theory]
    [InlineData("session")]
    [InlineData("truncated")]
    [InlineData("missing_geometry")]
    [InlineData("objects_bound")]
    [InlineData("duplicate_targets")]
    [InlineData("designation_bound")]
    public async Task IncompleteOrMixedReadsNeverBecomeFreeGround(string fault)
    {
        var port = new Port { Fault = fault };
        await Assert.ThrowsAnyAsync<Exception>(() => RegionSurvey.Observe(Request, port, TestContext.Current.CancellationToken));
        Assert.Equal(0, port.Plans);
    }

    [Fact]
    public async Task RunningGameAndInvalidBoundsStopBeforeMapReads()
    {
        var port = new Port { Fault = "running" };
        await Assert.ThrowsAsync<BridgeRejectionException>(() => RegionSurvey.Observe(Request, port, TestContext.Current.CancellationToken));
        Assert.Equal(0, port.Maps);
        await Assert.ThrowsAsync<ArgumentException>(() => RegionSurvey.Observe(Request with { X = 4090 }, port, TestContext.Current.CancellationToken));
        Assert.Equal(0, port.Maps);
        await Assert.ThrowsAsync<ArgumentException>(() => RegionSurvey.Observe(Request with { MaxHeights = 9 }, port, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => RegionSurvey.Observe(Request with { MaxWindows = 10 }, port, TestContext.Current.CancellationToken));
        Assert.Equal(0, port.Maps);
    }

    [Fact]
    public async Task ResumeDuringSurveyRejectsTheWholeReport()
    {
        var port = new Port { Fault = "resume" };
        await Assert.ThrowsAsync<BridgeRejectionException>(() => RegionSurvey.Observe(Request, port, TestContext.Current.CancellationToken));
        Assert.Equal(2, port.Speeds);
    }

    [Theory]
    [InlineData("other_one_tile", 1)]
    [InlineData("other_two_tiles", 2)]
    [InlineData("exact_overlap", 0)]
    public async Task UnknownFootprintIsLocalAndFreeNeighboursStillGetPlans(string fault, int unknownCells)
    {
        var report = await RegionSurvey.Observe(Request, new Port { Fault = fault }, TestContext.Current.CancellationToken);
        Assert.Equal(fault == "exact_overlap" ? 'o' : '?', report.Rows[1][7]);
        Assert.Equal('.', report.Rows[2][7]);
        Assert.Equal(unknownCells, report.Coverage.UnknownCells);
        Assert.NotEmpty(report.BuildingCandidates);
        Assert.NotEmpty(report.EmptyGroundPatches);
        if (fault == "exact_overlap") Assert.Equal(0, report.Coverage.RefinementReads);
        else Assert.InRange(report.Coverage.RefinementReads, 1, 64);
    }

    [Fact]
    public async Task RefinementBudgetAndChangedObjectsNeverBecomeFreeGround()
    {
        var bounded = await RegionSurvey.Observe(Request, new Port { Fault = "other_everywhere" }, TestContext.Current.CancellationToken);
        Assert.Equal(64, bounded.Coverage.RefinementReads);
        Assert.True(bounded.Coverage.RefinementBudgetExhausted);
        Assert.True(bounded.Coverage.UnknownCells > 0);
        await Assert.ThrowsAsync<InvalidDataException>(() => RegionSurvey.Observe(Request,
            new Port { Fault = "refinement_changed" }, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidDataException>(() => RegionSurvey.Observe(Request,
            new Port { Fault = "invalid_overlap" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AutomaticHeightsPrioritizeRoadsAndExposeSkippedLevels()
    {
        var report = await RegionSurvey.Observe(Request with { MaxHeights = 2 }, new Port { Fault = "heights" }, TestContext.Current.CancellationToken);
        Assert.Equal(4, Assert.Single(report.AdditionalLevels).Region.Z);
        Assert.Contains(report.Coverage.Heights, h => h.Z == 4 && h.ObstaclesInspected && h.ObservedPathCells > 0);
        Assert.Contains(report.Coverage.Heights, h => h.Z == 5 && !h.ObstaclesInspected);
        Assert.NotEmpty(report.AdditionalLevels[0].BuildingCandidates);
        var one = await RegionSurvey.Observe(Request with { ScanOtherHeights = false }, new Port { Fault = "heights" }, TestContext.Current.CancellationToken);
        Assert.Empty(one.AdditionalLevels);
        Assert.Contains(one.Coverage.Heights, h => h.Z == 4 && !h.ObstaclesInspected);
        var crops = await RegionSurvey.Observe(Request with { PlanBuildings = false }, new Port { Fault = "heights" }, TestContext.Current.CancellationToken);
        Assert.Empty(crops.AdditionalLevels); Assert.Equal(0, crops.PlanningCalls);
        Assert.All(crops.Coverage.PlanningRows, row => Assert.All(row, c => Assert.Equal('u', c)));
    }

    [Fact]
    public async Task SessionChangeOnAdditionalHeightRejectsEntireResult()
    {
        await Assert.ThrowsAsync<BridgeRejectionException>(() => RegionSurvey.Observe(Request,
            new Port { Fault = "height_session" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void CoverageDoesNotConfuseOptionsLimitWithExhaustiveSearch()
    {
        var r = Request with { Width = 2, Height = 1 };
        var maps = new[] { Cell(0, 0), Cell(1, 0) };
        var rect = new SurveyRect(0, 0, 3, 1, 1);
        var plans = Enumerable.Range(0, 4).Select(rot => new SurveyPlanCoverage(rect, rot, true, "area_exhausted")).ToArray();
        Assert.Equal("cu", RegionSurvey.Coverage(r, maps, [], [".."], plans, 0, false).PlanningRows[0]);
        plans[3] = plans[3] with { SearchComplete = false, StopReason = "option_limit" };
        Assert.Equal("pu", RegionSurvey.Coverage(r, maps, [], [".."], plans, 0, false).PlanningRows[0]);
    }

    [Fact]
    public void WindowsOverlapAndPatchesDoNotOverlapOrCrossObstacles()
    {
        var rows = Enumerable.Repeat(".......p........", 16).ToArray();
        var windows = RegionSurvey.Windows(Request, rows);
        Assert.Equal(3, windows.Length);
        Assert.True(windows.SelectMany(w => Enumerable.Range(w.Region.Y, w.Region.Height)
            .SelectMany(y => Enumerable.Range(w.Region.X, w.Region.Width).Select(x => (x, y)))).Distinct().Count() >= 160);
        var patches = RegionSurvey.Patches(Request, rows);
        var cells = patches.SelectMany(p => Enumerable.Range(p.Y, p.Height).SelectMany(y => Enumerable.Range(p.X, p.Width).Select(x => (x, y)))).ToArray();
        Assert.Equal(cells.Length, cells.Distinct().Count());
        Assert.DoesNotContain(cells, p => rows[p.y][p.x] != '.');
    }

    private sealed class Port : IRegionSurveyPort
    {
        public bool Moist { get; init; }
        public int RangeReads { get; private set; }
        public Task<BridgeEnvelope<NativeRange>> WorkRange(SurveyRequest r, int offset, CancellationToken ct) {
            RangeReads++;
            bool supported = Fault != "range_unknown";
            var cells = Enumerable.Range(0, 40).Select(i => new Position(i % 16, 1 + i / 16, 3)).ToArray();
            int total = supported ? 40 : 0;
            if (Fault == "range_bound") total = 2049;
            if (Fault == "range_changed" && offset > 0) total = 39;
            var page = supported ? cells.Skip(offset).Take(32).ToArray() : [];
            if (Fault == "range_duplicate" && offset > 0) page[0] = cells[0];
            return Result(new NativeRange(r.WorkBuildingId!, supported, [], offset, 32, total, page,
                offset + page.Length < total, [], supported ? "building_terrain_range" : "unavailable"),
                Fault == "range_session" ? "33333333-3333-4333-8333-333333333333" : Session);
        }
        public string Fault { get; init; } = "";
        private bool Heights => Fault is "heights" or "height_session";
        public int Maps { get; private set; }
        public int Plans { get; private set; }
        public int Speeds { get; private set; }
        private readonly BuildingPosition[] roads = (from y in new[] { 0, 7, 8, 15 }
            from x in new[] { 0, 7, 8, 15 } select Building("Path", new(x, y, 3), [new(x, y, 3)])).ToArray();
        private readonly NativeRemovalTarget unknown = new(Guid.NewGuid(), "other", "SyntheticSlope",
            new(7, 1, 3), "unsupported", false, false, "unknown", "current_components");
        private static Task<BridgeEnvelope<T>> Result<T>(T data, string session = Session) =>
            Task.FromResult(new BridgeEnvelope<T>(1, session, DateTimeOffset.UnixEpoch, "test", data));
        public Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct) {
            Speeds++; return Result(new NativeSimulation(Fault == "running" || Fault == "resume" && Speeds == 2 ? 7 : 0, 1, 0, 0));
        }
        public Task<BridgeEnvelope<NativeMap>> Map(SurveyRect r, CancellationToken ct) {
            Maps++;
            return Result(new NativeMap(new(r.X, r.Y, r.Z), r.Width, r.Height, 1,
                Enumerable.Range(r.Y, r.Height).SelectMany(y => Enumerable.Range(r.X, r.Width).Select(x =>
                    Cell(x, y) with { Z = r.Z, TerrainHeight = Heights ? (x < 8 ? 4 : 5) : 3,
                        OnGround = r.Z == (Heights ? (x < 8 ? 4 : 5) : 3), SoilIsMoist = Moist ? x != 2 : null })).ToArray(), []),
                Fault == "session" || Fault == "height_session" && r.Z == 4 ? "33333333-3333-4333-8333-333333333333" : Session);
        }
        public Task<BridgeEnvelope<NativeObjects>> Objects(int offset, CancellationToken ct) {
            var items = roads.ToArray();
            if (Heights) items = items.Concat([Building("Path", new(7, 7, 4), [new(7, 7, 4)])]).ToArray();
            if (Fault == "truncated") items[0] = items[0] with { CellsTruncated = true };
            if (Fault == "missing_geometry") items[0] = items[0] with { OccupiedCells = [] };
            return Result(new NativeObjects("buildings_and_paths", offset, 32, Fault == "objects_bound" ? 513 : items.Length, items, false, []));
        }
        public Task<BridgeEnvelope<NativeAreas>> Areas(string kind, int offset, CancellationToken ct) =>
            Result(new NativeAreas(kind, true, offset, 32, Fault == "designation_bound" ? 513 : 0, [], false, []));
        public Task<BridgeEnvelope<NativeRemovalTargets>> Targets(SurveyRect r, int offset, CancellationToken ct) {
            var t = new NativeRemovalTarget(Guid.NewGuid(), "vegetation", "SyntheticTree", new(r.X, r.Y, r.Z), "demolition_mark", true, false, "unknown", "current_components");
            NativeRemovalTarget[] items = Fault == "duplicate_targets" ? [t, t] : [];
            bool overlaps = r.Y <= 1 && r.Y + r.Height > 1 && r.X <= (Fault == "other_one_tile" ? 7 : 8) && r.X + r.Width > 7;
            if (Fault is "other_one_tile" or "other_two_tiles" or "exact_overlap" or "invalid_overlap" or "refinement_changed") {
                if (overlaps) {
                    var value = unknown;
                    if (Fault == "exact_overlap") value = value with { OverlapCells = new[] { new Position(7, 1, 3), new Position(8, 1, 3) }
                        .Where(p => p.X >= r.X && p.X < r.X + r.Width).ToArray() };
                    if (Fault == "invalid_overlap") value = value with { OverlapCells = [new(-1, 0, 3)] };
                    if (Fault == "refinement_changed" && r.Width < 8) value = value with { Marked = true };
                    items = [value];
                }
            }
            if (Fault == "other_everywhere") items = [unknown];
            return Result(new NativeRemovalTargets("all", offset, 32, items.Length, items, false, []));
        }
        public Task<BridgeEnvelope<NativeBuildingPlan>> Plan(SurveyRequest s, SurveyRect r, int rotation, CancellationToken ct) {
            Plans++; var p = new Position(r.X + 1, r.Y + 1, r.Z);
            return Result(new NativeBuildingPlan(s.Template, s.DistrictId, new(r.X, r.Y, r.Z), r.Width, r.Height, rotation,
                1, 0, r.Width * r.Height, false, "option_limit", [new(p, rotation, p, p, [], [p], false, [], new string('a', 64))], []));
        }
    }
}
