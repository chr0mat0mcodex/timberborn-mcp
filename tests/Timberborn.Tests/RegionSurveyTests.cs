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
        public string Fault { get; init; } = "";
        public int Maps { get; private set; }
        public int Plans { get; private set; }
        public int Speeds { get; private set; }
        private readonly BuildingPosition[] roads = (from y in new[] { 0, 7, 8, 15 }
            from x in new[] { 0, 7, 8, 15 } select Building("Path", new(x, y, 3), [new(x, y, 3)])).ToArray();
        private static Task<BridgeEnvelope<T>> Result<T>(T data, string session = Session) =>
            Task.FromResult(new BridgeEnvelope<T>(1, session, DateTimeOffset.UnixEpoch, "test", data));
        public Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct) {
            Speeds++; return Result(new NativeSimulation(Fault == "running" || Fault == "resume" && Speeds == 2 ? 7 : 0, 1, 0, 0));
        }
        public Task<BridgeEnvelope<NativeMap>> Map(SurveyRect r, CancellationToken ct) {
            Maps++;
            return Result(new NativeMap(new(r.X, r.Y, r.Z), r.Width, r.Height, 1,
                Enumerable.Range(r.Y, r.Height).SelectMany(y => Enumerable.Range(r.X, r.Width).Select(x => Cell(x, y))).ToArray(), []),
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
            return Result(new NativeRemovalTargets("all", offset, 32, items.Length, items, false, []));
        }
        public Task<BridgeEnvelope<NativeBuildingPlan>> Plan(SurveyRequest s, SurveyRect r, int rotation, CancellationToken ct) {
            Plans++; var p = new Position(r.X + 1, r.Y + 1, r.Z);
            return Result(new NativeBuildingPlan(s.Template, s.DistrictId, new(r.X, r.Y, r.Z), r.Width, r.Height, rotation,
                1, 0, r.Width * r.Height, false, "option_limit", [new(p, rotation, p, p, [], [p], false, [], new string('a', 64))], []));
        }
    }
}
