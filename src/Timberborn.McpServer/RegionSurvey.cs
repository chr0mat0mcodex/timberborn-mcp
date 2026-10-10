using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record SurveyRequest(string Session, string DistrictId, string Template,
    int X, int Y, int Z, int Width, int Height, string? WorkBuildingId = null, bool PlanBuildings = true,
    bool ScanOtherHeights = true, int MaxHeights = 4, int MaxWindows = 3);
public sealed record SurveyRect(int X, int Y, int Z, int Width, int Height);
public sealed record SurveyWindow(SurveyRect Region, int FreeCells, int VegetationCells, int PathCells);
public sealed record SurveyCandidate(SurveyRect Region, int Rotation, int OptionIndex,
    string PlanKey, Position Origin, int NewPaths);
public sealed record SurveyMoisture(string[] Rows, Dictionary<string, string> Legend,
    SurveyRect[] MoistEmptyGroundPatches);
public sealed record SurveyWorkRange(string BuildingId, bool Supported, string Source,
    int ObservedCells, string[] Rows, Dictionary<string, string> Legend, SurveyRect[] MoistEmptyGroundPatches);
public sealed record SurveyHeight(int Z, int TerrainCells, int ObservedPathCells, bool ObstaclesInspected);
public sealed record SurveyPlanCoverage(SurveyRect Region, int Rotation, bool SearchComplete, string StopReason);
public sealed record SurveyCoverage(int RefinementReads, bool RefinementBudgetExhausted,
    int UnknownCells, string[] PlanningRows, SurveyPlanCoverage[] Plans, SurveyHeight[] Heights);
public sealed record SurveyLevel(SurveyRect Region, string[] Rows, SurveyRect[] EmptyGroundPatches,
    SurveyWindow[] SearchWindows, SurveyCandidate[] BuildingCandidates, SurveyMoisture Moisture, SurveyCoverage Coverage);
public sealed record RegionSurveyReport(string Template, SurveyRect Region, string[] Rows,
    Dictionary<string, string> Legend, SurveyRect[] EmptyGroundPatches, SurveyWindow[] SearchWindows,
    SurveyCandidate[] BuildingCandidates, int NativeReads, int PlanningCalls,
    DateTimeOffset ObservationStartedAtUtc, DateTimeOffset ObservationEndedAtUtc,
    bool Atomic, string[] Limitations, SurveyMoisture Moisture, SurveyWorkRange? WorkRange, SurveyCoverage Coverage, SurveyLevel[] AdditionalLevels);

public interface IRegionSurveyPort
{
    Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct);
    Task<BridgeEnvelope<NativeMap>> Map(SurveyRect region, CancellationToken ct);
    Task<BridgeEnvelope<NativeObjects>> Objects(int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeAreas>> Areas(string kind, int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeRemovalTargets>> Targets(SurveyRect region, int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeBuildingPlan>> Plan(SurveyRequest request, SurveyRect region, int rotation, CancellationToken ct);
    Task<BridgeEnvelope<NativeRange>> WorkRange(SurveyRequest request, int offset, CancellationToken ct);
}

// Bounded read composition only. A geometric patch never authorizes construction or planting.
public static class RegionSurvey
{
    public static void Validate(SurveyRequest r)
    {
        BuildBatchState.Id(r.Session); BuildBatchState.Id(r.DistrictId);
        if (r.WorkBuildingId is not null) BuildBatchState.Id(r.WorkBuildingId);
        if (string.IsNullOrWhiteSpace(r.Template) || r.Template.Length > 160 ||
            r.MaxHeights is < 1 or > 8 || r.MaxWindows is < 1 or > 9 || r.X < 0 || r.Y < 0 || r.Z is < 0 or > 4095 || r.Width is < 1 or > 16 || r.Height is < 1 or > 16 ||
            (long)r.X + r.Width > 4096 || (long)r.Y + r.Height > 4096) throw new ArgumentException();
    }

    private sealed class ObservationContext { public string? Version; }
    public static Task<RegionSurveyReport> Observe(SurveyRequest r, IRegionSurveyPort port, CancellationToken ct) =>
        ObserveLevel(r, port, new ObservationContext(), ct);
    private static async Task<RegionSurveyReport> ObserveLevel(SurveyRequest r, IRegionSurveyPort port, ObservationContext context, CancellationToken ct)
    {
        Validate(r);
        ct.ThrowIfCancellationRequested();
        int reads = 0, planningCalls = 0;
        DateTimeOffset? first = null; DateTimeOffset last = default;
        T Check<T>(BridgeEnvelope<T> e) {
            ct.ThrowIfCancellationRequested(); reads++;
            if (e.SessionId != r.Session || context.Version is not null && e.BridgeVersion != context.Version)
                throw new BridgeRejectionException("stale_session");
            context.Version = e.BridgeVersion; first ??= e.ObservedAtUtc; last = e.ObservedAtUtc;
            return e.Data;
        }
        if (Check(await port.Simulation(ct)).CurrentSpeed != 0) throw new BridgeRejectionException("state_conflict");
        var maps = new List<MapCell>(); var targets = new Dictionary<Guid, NativeRemovalTarget>();
        var uncertain = new HashSet<(int X, int Y)>();
        var targetCells = new Dictionary<Guid, HashSet<Position>>();
        var uncertainTiles = new List<SurveyRect>();
        int refinementReads = 0; bool refinementBudgetExhausted = false;
        bool UnknownOverlap(NativeRemovalTarget item, SurveyRect tile) =>
            item.Kind is not ("buildings" or "vegetation" or "planted" or "debris") ||
            item.Kind != "buildings" && (item.Position.Z != r.Z || item.Position.X < tile.X ||
                item.Position.X >= tile.X + tile.Width || item.Position.Y < tile.Y || item.Position.Y >= tile.Y + tile.Height);
        async Task Refine(SurveyRect tile) {
            // Only absence in a complete native overlap query can clear uncertainty.
            var found = new List<NativeRemovalTarget>(); int? total = null;
            var seen = new HashSet<Guid>();
            for (int offset = 0; ; offset += 32) {
                if (refinementReads >= 64) { refinementBudgetExhausted = true; return; }
                refinementReads++;
                var page = Check(await port.Targets(tile, offset, ct));
                if (page.Kind != "all" || page.Offset != offset || page.Limit != 32 || page.Total > 128 ||
                    total is not null && total != page.Total) throw new InvalidDataException("survey_refinement_changed");
                total = page.Total;
                foreach (var item in page.Items) {
                    if (!seen.Add(item.Id) || !targets.TryGetValue(item.Id, out var old) ||
                        old.Kind != item.Kind || old.Template != item.Template || old.Position != item.Position || old.Marked != item.Marked)
                        throw new InvalidDataException("survey_refinement_changed");
                    found.Add(item);
                }
                if (!page.HasMore) {
                    if (seen.Count != page.Total) throw new InvalidDataException("survey_refinement_incomplete");
                    break;
                }
                if (offset >= 96) throw new InvalidDataException("survey_refinement_bound");
            }
            if (!found.Any(t => UnknownOverlap(t, tile))) {
                for (int yy = tile.Y; yy < tile.Y + tile.Height; yy++)
                    for (int xx = tile.X; xx < tile.X + tile.Width; xx++) uncertain.Remove((xx, yy));
                return;
            }
            if (tile.Width == 1 && tile.Height == 1) return;
            int w = Math.Max(1, tile.Width / 2), h = Math.Max(1, tile.Height / 2);
            for (int yy = tile.Y; yy < tile.Y + tile.Height; yy += h)
                for (int xx = tile.X; xx < tile.X + tile.Width; xx += w)
                    await Refine(new(xx, yy, tile.Z, Math.Min(w, tile.X + tile.Width - xx), Math.Min(h, tile.Y + tile.Height - yy)));
        }
        for (int y = r.Y; y < r.Y + r.Height; y += 8)
        for (int x = r.X; x < r.X + r.Width; x += 8) {
            var tile = new SurveyRect(x, y, r.Z, Math.Min(8, r.X + r.Width - x), Math.Min(8, r.Y + r.Height - y));
            var map = Check(await port.Map(tile, ct));
            if (map.Origin != new Position(x, y, r.Z) || map.Width != tile.Width || map.Height != tile.Height || map.Depth != 1)
                throw new InvalidDataException("survey_map_mismatch");
            maps.AddRange(map.Cells);
            int? total = null; var seen = new HashSet<Guid>();
            for (int offset = 0; ; offset += 32) {
                if (offset >= 128) throw new InvalidDataException("survey_target_bound");
                var p = Check(await port.Targets(tile, offset, ct));
                if (p.Kind != "all" || p.Total > 128 || p.Offset != offset || p.Limit != 32 || total is not null && total != p.Total)
                    throw new InvalidDataException("survey_targets_changed");
                total = p.Total;
                foreach (var item in p.Items) {
                    if (!seen.Add(item.Id) || targets.TryGetValue(item.Id, out var old) &&
                        (old.Kind != item.Kind || old.Template != item.Template || old.Position != item.Position || old.Marked != item.Marked))
                        throw new InvalidDataException("survey_targets_changed");
                    targets[item.Id] = item;
                    if (item.OverlapCells is { } overlap) {
                        if (overlap.Length == 0 || overlap.Length > tile.Width * tile.Height || overlap.Distinct().Count() != overlap.Length ||
                            overlap.Any(cell => cell is null || cell.Z != r.Z || cell.X < x || cell.X >= x + tile.Width || cell.Y < y || cell.Y >= y + tile.Height))
                            throw new InvalidDataException("survey_invalid_overlap");
                        if (!targetCells.TryGetValue(item.Id, out var cells)) targetCells[item.Id] = cells = [];
                        cells.UnionWith(overlap);
                    } else if (UnknownOverlap(item, tile)) {
                        if (!uncertainTiles.Contains(tile)) uncertainTiles.Add(tile);
                        for (int yy = y; yy < y + tile.Height; yy++) for (int xx = x; xx < x + tile.Width; xx++) uncertain.Add((xx, yy));
                    }
                }
                if (!p.HasMore) { if (seen.Count != p.Total) throw new InvalidDataException("survey_targets_incomplete"); break; }
            }
        }
        foreach (var tile in uncertainTiles) await Refine(tile);
        foreach (var (id, cells) in targetCells) {
            targets[id] = targets[id] with { OverlapCells = cells.ToArray() };
        }
        var buildings = new List<BuildingPosition>(); int? objectTotal = null;
        for (int offset = 0; ; offset += 32) {
            if (offset >= 512) throw new InvalidDataException("survey_building_bound");
            var p = Check(await port.Objects(offset, ct));
            if (p.Total > 512 || p.Offset != offset || p.Limit != 32 || objectTotal is not null && objectTotal != p.Total)
                throw new InvalidDataException("survey_objects_changed");
            objectTotal = p.Total; buildings.AddRange(p.Items);
            if (!p.HasMore) {
                if (buildings.Count != p.Total || buildings.Select(b => b.Id).Distinct().Count() != buildings.Count)
                    throw new InvalidDataException("survey_objects_incomplete");
                break;
            }
        }
        // A truncated footprint could reach into the survey even with its origin outside it.
        if (buildings.Any(b => b.CellsTruncated || b.OccupiedCells.Length == 0))
            throw new InvalidDataException("survey_geometry_incomplete");
        if (targets.Values.Any(t => t.Kind == "buildings" && !buildings.Any(b => b.Id == t.Id)))
            throw new InvalidDataException("survey_objects_changed");
        var designations = new HashSet<Position>();
        foreach (string kind in new[] { "crops", "tree_planting" }) {
            int? total = null; var seen = new HashSet<Position>();
            for (int offset = 0; ; offset += 32) {
                if (offset >= 512) throw new InvalidDataException("survey_designation_bound");
                var p = Check(await port.Areas(kind, offset, ct));
                if (!p.Supported || p.Kind != kind || p.Total is null or > 512 || p.Offset != offset || p.Limit != 32 ||
                    total is not null && total != p.Total) throw new InvalidDataException("survey_designations_changed");
                total = p.Total;
                foreach (var a in p.Items) if (!seen.Add(a.Position)) throw new InvalidDataException("survey_duplicate_designation");
                if (!p.HasMore) { if (seen.Count != p.Total) throw new InvalidDataException("survey_designations_incomplete"); break; }
            }
            designations.UnionWith(seen);
        }
        var rows = Classify(r, maps, buildings, targets.Values.ToArray(), designations, uncertain);
        var patches = Patches(r, rows);
        var windows = r.PlanBuildings ? Windows(r, rows) : [];
        var moisture = Moisture(r, maps, rows);
        SurveyWorkRange? workRange = null;
        if (r.WorkBuildingId is not null) {
            var cells = new HashSet<Position>(); int? total = null; string? source = null;
            bool supported = false;
            for (int offset = 0; ; offset += 32) {
                if (offset >= 2048) throw new InvalidDataException("survey_work_range_bound");
                var p = Check(await port.WorkRange(r, offset, ct));
                if (p.Id != r.WorkBuildingId || p.Offset != offset || p.Limit != 32 || p.Total is < 0 or > 2048 ||
                    total is not null && (total != p.Total || source != p.Source || supported != p.Supported) ||
                    p.Items.Length != Math.Min(32, Math.Max(0, p.Total - offset)) ||
                    p.HasMore != (offset + p.Items.Length < p.Total) || !p.Supported && p.Total != 0)
                    throw new InvalidDataException("survey_work_range_changed");
                total = p.Total; source = p.Source; supported = p.Supported;
                foreach (var cell in p.Items) if (!cells.Add(cell)) throw new InvalidDataException("survey_duplicate_work_range_cell");
                if (!p.HasMore) {
                    if (cells.Count != p.Total) throw new InvalidDataException("survey_work_range_incomplete");
                    break;
                }
            }
            var rangeRows = new string[r.Height]; var eligible = new string[r.Height];
            for (int y = 0; y < r.Height; y++) {
                rangeRows[y] = new string(Enumerable.Range(0, r.Width).Select(x => !supported ? '?' :
                    cells.Contains(new(r.X + x, r.Y + y, r.Z)) ? 'r' : '-').ToArray());
                eligible[y] = new string(Enumerable.Range(0, r.Width).Select(x =>
                    rangeRows[y][x] == 'r' && rows[y][x] == '.' && moisture.Rows[y][x] == 'm' ? '.' : 'x').ToArray());
            }
            workRange = new(r.WorkBuildingId, supported, source!, cells.Count, rangeRows,
                new() { ["r"] = "in_native_work_range", ["-"] = "outside_observed_work_range", ["?"] = "work_range_unknown" }, Patches(r, eligible));
        }
        var candidates = new List<SurveyCandidate>();
        var coveragePlans = new List<SurveyPlanCoverage>();
        foreach (var window in windows)
        for (int rotation = 0; rotation < 4; rotation++) {
            var p = Check(await port.Plan(r, window.Region, rotation, ct)); planningCalls++;
            if (p.Template != r.Template || p.DistrictId != r.DistrictId || p.Rotation != rotation ||
                p.Origin != new Position(window.Region.X, window.Region.Y, r.Z) ||
                p.Width != window.Region.Width || p.Height != window.Region.Height)
                throw new InvalidDataException("survey_plan_mismatch");
            coveragePlans.Add(new(window.Region, rotation, p.SearchComplete, p.StopReason));
            for (int i = 0; i < p.Options.Length; i++) {
                var o = p.Options[i];
                if (o.Executable || string.IsNullOrEmpty(o.PlanKey)) throw new InvalidDataException("survey_invalid_plan");
                candidates.Add(new(window.Region, rotation, i, o.PlanKey, o.Origin, o.NewRoadCells.Length));
            }
        }
        if (Check(await port.Simulation(ct)).CurrentSpeed != 0) throw new BridgeRejectionException("state_conflict");
        var report = new RegionSurveyReport(r.Template, new(r.X, r.Y, r.Z, r.Width, r.Height), rows,
            new() { ["."] = "unflooded_ground_no_observed_obstacle", ["v"] = "vegetation", ["c"] = "plant_or_designation",
                ["b"] = "building_or_debris", ["o"] = "other_object_observed_occupied_cell", ["p"] = "finished_path", ["e"] = "existing_entrance",
                ["~"] = "water_or_contamination", ["h"] = "different_ground_height", ["?"] = "unknown" },
            patches, windows, candidates.OrderBy(c => c.NewPaths).ThenBy(c => c.Origin.Y).ThenBy(c => c.Origin.X)
                .DistinctBy(c => (c.Origin, c.Rotation)).Take(4).ToArray(), reads, planningCalls, first!.Value, last, false,
            ["read_only_non_atomic", "rows_y_ascending_columns_x_ascending", "each_level_separate_ground_plane_no_vertical_route_planning",
             "patches_are_not_planting_or_work_range_validation", "moisture_is_current_not_future_irrigation_or_yield",
             "paths_are_not_district_membership_proof", "bounded_coverage_ranked_windows",
             "no_candidate_does_not_mean_region_unbuildable", "candidates_not_executable_revalidate_before_build",
             "no_clearing_or_existing_access_safety_claim", "overlap_cells_clipped_to_query_legacy_vegetation_origin_only",
             "work_range_not_staffing_selected_job_or_crop_suitability",
             "heights_with_obstaclesInspected_false_remain_unchecked",
             "planning_coverage_is_search_not_placement_validation"], moisture, workRange,
            Coverage(r, maps, buildings, rows, coveragePlans, refinementReads, refinementBudgetExhausted), []);
        if (!r.ScanOtherHeights || !r.PlanBuildings || r.WorkBuildingId is not null || r.MaxHeights == 1) return report;
        var levels = new List<SurveyLevel>();
        foreach (var height in report.Coverage.Heights.Where(h => h.Z != r.Z && h.Z >= 0 && h.Z <= 4095)
            .OrderByDescending(h => h.ObservedPathCells > 0).ThenByDescending(h => h.TerrainCells)
            .ThenBy(h => Math.Abs(h.Z - r.Z)).ThenBy(h => h.Z).Take(r.MaxHeights - 1)) {
            var other = await ObserveLevel(r with { Z = height.Z, ScanOtherHeights = false }, port, context, ct);
            reads += other.NativeReads; planningCalls += other.PlanningCalls; last = other.ObservationEndedAtUtc;
            levels.Add(new(other.Region, other.Rows, other.EmptyGroundPatches, other.SearchWindows,
                other.BuildingCandidates, other.Moisture, other.Coverage));
        }
        var inspected = levels.Select(l => l.Region.Z).Append(r.Z).ToHashSet();
        var coverage = report.Coverage with { Heights = report.Coverage.Heights
            .Select(h => h with { ObstaclesInspected = inspected.Contains(h.Z) }).ToArray() };
        return report with { AdditionalLevels = levels.ToArray(), Coverage = coverage,
            NativeReads = reads, PlanningCalls = planningCalls, ObservationEndedAtUtc = last };
    }

    public static SurveyCoverage Coverage(SurveyRequest r, IReadOnlyList<MapCell> maps,
        IReadOnlyList<BuildingPosition> buildings, string[] rows, IReadOnlyList<SurveyPlanCoverage> plans,
        int refinementReads, bool exhausted)
    {
        var paths = buildings.Where(b => b.Template == "Path" && b.Finished)
            .SelectMany(b => b.OccupiedCells).Where(p => p.X >= r.X && p.X < r.X + r.Width && p.Y >= r.Y && p.Y < r.Y + r.Height)
            .Distinct().ToArray();
        var heights = maps.Select(c => c.TerrainHeight).Concat(paths.Select(p => p.Z)).Distinct().Order()
            .Select(z => new SurveyHeight(z, maps.Count(c => c.TerrainHeight == z), paths.Count(p => p.Z == z), z == r.Z)).ToArray();
        var planningRows = new string[r.Height];
        for (int y = 0; y < r.Height; y++) planningRows[y] = new string(Enumerable.Range(0, r.Width).Select(x => {
            var covering = plans.Where(p => r.X + x >= p.Region.X && r.X + x < p.Region.X + p.Region.Width &&
                r.Y + y >= p.Region.Y && r.Y + y < p.Region.Y + p.Region.Height).ToArray();
            // complete requires all four orientations to report exhausted search at this origin.
            return Enumerable.Range(0, 4).All(rot => covering.Any(p => p.Rotation == rot && p.SearchComplete)) ? 'c' :
                covering.Length > 0 ? 'p' : 'u';
        }).ToArray());
        return new(refinementReads, exhausted, rows.Sum(row => row.Count(c => c == '?')), planningRows, plans.ToArray(), heights);
    }

    public static SurveyMoisture Moisture(SurveyRequest r, IReadOnlyList<MapCell> maps, string[] obstacleRows)
    {
        var cells = maps.ToDictionary(c => (c.X, c.Y));
        var rows = new string[r.Height];
        var eligible = new string[r.Height];
        for (int y = 0; y < r.Height; y++) {
            var row = new char[r.Width]; var free = new char[r.Width];
            for (int x = 0; x < r.Width; x++) {
                if (!cells.TryGetValue((r.X + x, r.Y + y), out var c) || c.Z != r.Z)
                    throw new InvalidDataException("survey_missing_cells");
                row[x] = !c.OnGround || c.TerrainHeight != r.Z ? '-' : c.SoilIsMoist switch {
                    true => 'm', false => 'd', null => '?' };
                free[x] = row[x] == 'm' && obstacleRows[y][x] == '.' ? '.' : 'x';
            }
            rows[y] = new string(row); eligible[y] = new string(free);
        }
        return new(rows, new() { ["m"] = "moist_ground", ["d"] = "dry_ground",
            ["?"] = "moisture_unknown", ["-"] = "not_ground_at_requested_height" }, Patches(r, eligible));
    }

    public static string[] Classify(SurveyRequest r, IReadOnlyList<MapCell> maps, IReadOnlyList<BuildingPosition> buildings,
        IReadOnlyList<NativeRemovalTarget> targets, ISet<Position> designations, ISet<(int X, int Y)>? uncertain = null)
    {
        var byCell = maps.ToDictionary(c => (c.X, c.Y));
        if (byCell.Count != r.Width * r.Height) throw new InvalidDataException("survey_missing_cells");
        var occupied = buildings.SelectMany(b => b.OccupiedCells.Where(p => p.Z >= r.Z).Select(p => (p.X, p.Y))).ToHashSet();
        var paths = buildings.Where(b => b.Template == "Path" && b.Finished)
            .SelectMany(b => b.OccupiedCells.Where(p => p.Z == r.Z).Select(p => (p.X, p.Y))).ToHashSet();
        var entrances = buildings.Where(b => b.Entrance?.Z == r.Z).Select(b => (b.Entrance!.X, b.Entrance.Y)).ToHashSet();
        var rows = new List<string>();
        for (int y = r.Y; y < r.Y + r.Height; y++) {
            var row = new char[r.Width];
            for (int x = r.X; x < r.X + r.Width; x++) {
                if (!byCell.TryGetValue((x, y), out var c) || c.Z != r.Z) throw new InvalidDataException("survey_missing_cells");
                var pos = new Position(x, y, r.Z);
                char label = c.TerrainHeight != r.Z || !c.OnGround ? 'h' : c.Underwater || c.WaterDepth > 0 || c.Contamination > 0 ? '~' : '.';
                if (uncertain?.Contains((x, y)) == true) label = '?';
                if (designations.Contains(pos)) label = 'c';
                foreach (var t in targets.Where(t => t.OverlapCells is { } cells ? cells.Contains(pos) : t.Position == pos))
                    label = t.Kind switch { "vegetation" when label == '.' => 'v', "planted" => 'c', "debris" => 'b', "other" when t.OverlapCells is not null => 'o', _ => label };
                if (occupied.Contains((x, y))) label = 'b';
                if (paths.Contains((x, y))) label = 'p';
                if (entrances.Contains((x, y)) && !paths.Contains((x, y))) label = 'e';
                row[x - r.X] = label;
            }
            rows.Add(new string(row));
        }
        return rows.ToArray();
    }

    public static SurveyRect[] Patches(SurveyRequest r, string[] rows)
    {
        var choices = new List<SurveyRect>();
        for (int y = 0; y < r.Height; y++) for (int x = 0; x < r.Width; x++)
        for (int h = 1; h <= Math.Min(4, r.Height - y); h++) for (int w = 1; w <= Math.Min(4, r.Width - x); w++)
            if (w * h >= 2 && Enumerable.Range(y, h).All(yy => Enumerable.Range(x, w).All(xx => rows[yy][xx] == '.')))
                choices.Add(new(r.X + x, r.Y + y, r.Z, w, h));
        var picked = new List<SurveyRect>();
        foreach (var p in choices.OrderByDescending(p => p.Width * p.Height).ThenBy(p => p.Y).ThenBy(p => p.X)) {
            if (picked.Any(q => p.X < q.X + q.Width && p.X + p.Width > q.X && p.Y < q.Y + q.Height && p.Y + p.Height > q.Y)) continue;
            picked.Add(p); if (picked.Count == 6) break;
        }
        return picked.ToArray();
    }

    public static SurveyWindow[] Windows(SurveyRequest r, string[] rows)
    {
        int w = Math.Min(8, r.Width), h = Math.Min(8, r.Height);
        int[] Starts(int length, int size) => Enumerable.Range(0, length - size + 1).Where(i => i % 4 == 0 || i == length - size).ToArray();
        var options = new List<SurveyWindow>();
        foreach (int y in Starts(r.Height, h)) foreach (int x in Starts(r.Width, w)) {
            var cells = Enumerable.Range(y, h).SelectMany(yy => rows[yy].Substring(x, w)).ToArray();
            var window = new SurveyWindow(new(r.X + x, r.Y + y, r.Z, w, h), cells.Count(c => c == '.'), cells.Count(c => c == 'v'), cells.Count(c => c == 'p'));
            if (window.FreeCells > 0 && window.PathCells > 0) options.Add(window);
        }
        var selected = new List<SurveyWindow>(); var covered = new HashSet<(int X, int Y)>();
        IEnumerable<(int X, int Y)> Cells(SurveyWindow w) =>
            from yy in Enumerable.Range(w.Region.Y, w.Region.Height)
            from xx in Enumerable.Range(w.Region.X, w.Region.Width) select (xx, yy);
        while (options.Count > 0 && selected.Count < r.MaxWindows) {
            var next = options.OrderByDescending(w => Cells(w).Count(c => !covered.Contains(c)))
                .ThenByDescending(w => w.FreeCells).ThenByDescending(w => w.PathCells)
                .ThenBy(w => w.Region.Y).ThenBy(w => w.Region.X).First();
            selected.Add(next); covered.UnionWith(Cells(next)); options.Remove(next);
        }
        return selected.ToArray();
    }
}
