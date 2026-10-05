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
    }

    [Fact]
    public async Task ResumeDuringSurveyRejectsTheWholeReport()
    {
        var port = new Port { Fault = "resume" };
        await Assert.ThrowsAsync<BridgeRejectionException>(() => RegionSurvey.Observe(Request, port, TestContext.Current.CancellationToken));
        Assert.Equal(2, port.Speeds);
    }

    [Theory]
    [InlineData("other_one_tile", 8)]
    [InlineData("other_two_tiles", 16)]
    public async Task UnknownFootprintOnlyExcludesTilesReportingItsOverlap(string fault, int unknownWidth)
    {
        var report = await RegionSurvey.Observe(Request, new Port { Fault = fault }, TestContext.Current.CancellationToken);
        for (int y = 0; y < 8; y++) for (int x = 0; x < unknownWidth; x++)
            Assert.NotEqual('.', report.Rows[y][x]);
        Assert.Equal('.', report.Rows[9][1]); // unaffected lower tile remains usable
        if (unknownWidth == 8) Assert.Equal('.', report.Rows[1][9]);
        Assert.NotEmpty(report.EmptyGroundPatches);
        Assert.All(report.EmptyGroundPatches, p => Assert.True(p.Y >= 8 || p.X >= unknownWidth));
    }

    [Fact]
    public void WindowsOverlapAndPatchesDoNotOverlapOrCrossObstacles()
    {
        var rows = Enumerable.Repeat(".......p........", 16).ToArray();
        var windows = RegionSurvey.Windows(Request, rows);
        Assert.Equal(3, windows.Length);
        Assert.Contains(windows, w => w.Region.X == 4);
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
                    Cell(x, y) with { SoilIsMoist = Moist ? x != 2 : null })).ToArray(), []),
                Fault == "session" ? "33333333-3333-4333-8333-333333333333" : Session);
        }
        public Task<BridgeEnvelope<NativeObjects>> Objects(int offset, CancellationToken ct) {
            var items = roads.ToArray();
            if (Fault == "truncated") items[0] = items[0] with { CellsTruncated = true };
            if (Fault == "missing_geometry") items[0] = items[0] with { OccupiedCells = [] };
            return Result(new NativeObjects("buildings_and_paths", offset, 32, Fault == "objects_bound" ? 513 : items.Length, items, false, []));
        }
        public Task<BridgeEnvelope<NativeAreas>> Areas(string kind, int offset, CancellationToken ct) =>
            Result(new NativeAreas(kind, true, offset, 32, Fault == "designation_bound" ? 513 : 0, [], false, []));
        public Task<BridgeEnvelope<NativeRemovalTargets>> Targets(SurveyRect r, int offset, CancellationToken ct) {
            var t = new NativeRemovalTarget(Guid.NewGuid(), "vegetation", "SyntheticTree", new(r.X, r.Y, r.Z), "demolition_mark", true, false, "unknown", "current_components");
            NativeRemovalTarget[] items = Fault == "duplicate_targets" ? [t, t] : [];
            if (r.Y == 0 && (Fault == "other_two_tiles" || Fault == "other_one_tile" && r.X == 0)) items = [unknown];
            return Result(new NativeRemovalTargets("all", offset, 32, items.Length, items, false, []));
        }
        public Task<BridgeEnvelope<NativeBuildingPlan>> Plan(SurveyRequest s, SurveyRect r, int rotation, CancellationToken ct) {
            Plans++; var p = new Position(r.X + 1, r.Y + 1, r.Z);
            return Result(new NativeBuildingPlan(s.Template, s.DistrictId, new(r.X, r.Y, r.Z), r.Width, r.Height, rotation,
                1, 0, r.Width * r.Height, false, "option_limit", [new(p, rotation, p, p, [], [p], false, [], new string('a', 64))], []));
        }
    }
}
