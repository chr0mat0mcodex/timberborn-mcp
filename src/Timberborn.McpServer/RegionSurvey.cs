using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record SurveyRequest(string Session, string DistrictId, string Template,
    int X, int Y, int Z, int Width, int Height);
public sealed record SurveyRect(int X, int Y, int Z, int Width, int Height);
public sealed record SurveyWindow(SurveyRect Region, int FreeCells, int VegetationCells, int PathCells);
public sealed record SurveyCandidate(SurveyRect Region, int Rotation, int OptionIndex,
    string PlanKey, Position Origin, int NewPaths);
public sealed record RegionSurveyReport(string Template, SurveyRect Region, string[] Rows,
    Dictionary<string, string> Legend, SurveyRect[] EmptyGroundPatches, SurveyWindow[] SearchWindows,
    SurveyCandidate[] BuildingCandidates, int NativeReads, int PlanningCalls,
    DateTimeOffset ObservationStartedAtUtc, DateTimeOffset ObservationEndedAtUtc,
    bool Atomic, string[] Limitations);

public interface IRegionSurveyPort
{
    Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct);
    Task<BridgeEnvelope<NativeMap>> Map(SurveyRect region, CancellationToken ct);
    Task<BridgeEnvelope<NativeObjects>> Objects(int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeAreas>> Areas(string kind, int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeRemovalTargets>> Targets(SurveyRect region, int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeBuildingPlan>> Plan(SurveyRequest request, SurveyRect region, int rotation, CancellationToken ct);
}

// Bounded read composition only. A geometric patch never authorizes construction or planting.
public static class RegionSurvey
{
    public static void Validate(SurveyRequest r)
    {
        BuildBatchState.Id(r.Session); BuildBatchState.Id(r.DistrictId);
        if (string.IsNullOrWhiteSpace(r.Template) || r.Template.Length > 160 ||
            r.X < 0 || r.Y < 0 || r.Z is < 0 or > 4095 || r.Width is < 1 or > 16 || r.Height is < 1 or > 16 ||
            (long)r.X + r.Width > 4096 || (long)r.Y + r.Height > 4096) throw new ArgumentException();
    }

    public static async Task<RegionSurveyReport> Observe(SurveyRequest r, IRegionSurveyPort port, CancellationToken ct)
    {
        Validate(r);
        ct.ThrowIfCancellationRequested();
        int reads = 0, planningCalls = 0;
        string? version = null;
        DateTimeOffset? first = null; DateTimeOffset last = default;
        T Check<T>(BridgeEnvelope<T> e) {
            ct.ThrowIfCancellationRequested(); reads++;
            if (e.SessionId != r.Session || version is not null && e.BridgeVersion != version)
                throw new BridgeRejectionException("stale_session");
            version = e.BridgeVersion; first ??= e.ObservedAtUtc; last = e.ObservedAtUtc;
            return e.Data;
        }
        if (Check(await port.Simulation(ct)).CurrentSpeed != 0) throw new BridgeRejectionException("state_conflict");
        var maps = new List<MapCell>(); var targets = new Dictionary<Guid, NativeRemovalTarget>();
        var uncertain = new HashSet<(int X, int Y)>();
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
                    if (item.Kind != "buildings" && (item.Position.Z != r.Z || item.Position.X < x || item.Position.X >= x + tile.Width ||
                        item.Position.Y < y || item.Position.Y >= y + tile.Height))
                        for (int yy = y; yy < y + tile.Height; yy++) for (int xx = x; xx < x + tile.Width; xx++) uncertain.Add((xx, yy));
                }
                if (!p.HasMore) { if (seen.Count != p.Total) throw new InvalidDataException("survey_targets_incomplete"); break; }
            }
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
        var windows = Windows(r, rows);
        var candidates = new List<SurveyCandidate>();
        foreach (var window in windows)
        for (int rotation = 0; rotation < 4; rotation++) {
            var p = Check(await port.Plan(r, window.Region, rotation, ct)); planningCalls++;
            if (p.Template != r.Template || p.DistrictId != r.DistrictId || p.Rotation != rotation ||
                p.Origin != new Position(window.Region.X, window.Region.Y, r.Z) ||
                p.Width != window.Region.Width || p.Height != window.Region.Height)
                throw new InvalidDataException("survey_plan_mismatch");
            for (int i = 0; i < p.Options.Length; i++) {
                var o = p.Options[i];
                if (o.Executable || string.IsNullOrEmpty(o.PlanKey)) throw new InvalidDataException("survey_invalid_plan");
                candidates.Add(new(window.Region, rotation, i, o.PlanKey, o.Origin, o.NewRoadCells.Length));
            }
        }
        if (Check(await port.Simulation(ct)).CurrentSpeed != 0) throw new BridgeRejectionException("state_conflict");
        return new(r.Template, new(r.X, r.Y, r.Z, r.Width, r.Height), rows,
            new() { ["."] = "dry_ground_no_observed_obstacle", ["v"] = "vegetation", ["c"] = "plant_or_designation",
                ["b"] = "building_or_debris", ["p"] = "finished_path", ["e"] = "existing_entrance",
                ["~"] = "water_or_contamination", ["h"] = "different_ground_height", ["?"] = "unknown" },
            patches, windows, candidates.OrderBy(c => c.NewPaths).ThenBy(c => c.Origin.Y).ThenBy(c => c.Origin.X)
                .DistinctBy(c => (c.Origin, c.Rotation)).Take(4).ToArray(), reads, planningCalls, first!.Value, last, false,
            ["read_only_non_atomic", "rows_y_ascending_columns_x_ascending", "single_ground_level",
             "patches_are_not_planting_or_work_range_validation", "irrigation_and_yield_unknown",
             "paths_are_not_district_membership_proof", "top_three_overlapping_windows_only",
             "no_candidate_does_not_mean_region_unbuildable", "candidates_not_executable_revalidate_before_build",
             "no_clearing_or_existing_access_safety_claim", "vegetation_origins_not_full_footprints"]);
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
                foreach (var t in targets.Where(t => t.Position == pos))
                    label = t.Kind switch { "vegetation" when label == '.' => 'v', "planted" => 'c', "debris" => 'b', _ => label };
                if (occupied.Contains((x, y))) label = 'b';
                if (paths.Contains((x, y))) label = 'p';
                if (entrances.Contains((x, y)) && !paths.Contains((x, y))) label = 'e';
                // Unknown non-building overlap (e.g. origin outside the read tile) is not free land.
                if (label == '.' && targets.Any(t => t.Kind is not ("vegetation" or "planted" or "debris" or "buildings"))) label = '?';
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
        return options.OrderByDescending(w => w.FreeCells).ThenByDescending(w => w.PathCells)
            .ThenBy(w => w.Region.Y).ThenBy(w => w.Region.X).Take(3).ToArray();
    }
}
