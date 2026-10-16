using Timberborn.Backend.Native;

namespace Timberborn.McpServer;

public sealed record ProtectedZone(int X, int Y, int Z, int Width, int Height);
public sealed record TerrainRoute(string Kind, Position Origin, float? ObservedWaterLevel, float Contamination,
    int MinimumTerrainPassLevel, bool? LevelReachesPass, Position[] Path, int TotalPathCells, bool PathTruncated,
    ProtectionCandidate Candidate);
public sealed record ProtectionCandidate(Position At, Position[] NearbyTerrainBanks, string Action,
    int AssumedBarrierCrest, bool CrestWithinMapHeight, string CrestBasis, ProtectionCell[] CrossSection, bool BothTerrainBanksFound,
    string[] RequiredChecks);
public sealed record ProtectionCell(Position At, int HeightToCrest);
public sealed record StrategicAnalysis(string State, int VoidNodes, int ConnectedVoidNodes, int CoveredColumns,
    int CaveColumns, int TargetVoidNodes, int CandidateCount, TerrainRoute[] Corridors, Position[] HigherGround,
    string[] Limitations);

public static class StrategicMapAnalysis
{
    private sealed record Node(int X, int Y, StrategicSpan Span);
    private static readonly (int X, int Y)[] Directions = [(1,0),(-1,0),(0,1),(0,-1)];
    private static readonly string[] Limits = ["terrain_connectivity_not_flow_direction_or_flood_forecast",
        "buildings_dams_gates_and_water_physics_not_modelled", "no_safe_or_unreachable_claim_from_missing_route",
        "water_samples_and_tiles_not_simultaneous", "cave_pressure_and_water_depth_semantics_need_live_validation",
        "protection_candidates_not_complete_cutsets_or_build_blueprints", "higher_ground_not_proven_beaver_access"];

    // Minimax terrain pass: the lowest maximum floor along any geometrically connected void route.
    // All voids are retained; a roof is never silently collapsed to a single height field.
    public static StrategicAnalysis Analyze(Position size, IReadOnlyList<StrategicColumn> columns,
        ProtectedZone target, float? scenarioLevel, CancellationToken ct = default, IReadOnlyList<StrategicObject>? objects = null)
    {
        var nodes = new List<Node>(); var byColumn = new Dictionary<(int,int), List<int>>();
        int caves = 0;
        foreach (var c in columns)
        {
            ct.ThrowIfCancellationRequested();
            if (c.Spans.Length > 1 || c.Spans.Any(s => s.Top < size.Z)) caves++;
            var ids = new List<int>();
            foreach (var span in c.Spans) { ids.Add(nodes.Count); nodes.Add(new(c.X, c.Y, span)); }
            if (!byColumn.TryAdd((c.X,c.Y), ids)) throw new InvalidDataException("strategic_duplicate_column");
            if (nodes.Count > 524288) return Empty("analysis_node_budget_exceeded");
        }
        if (columns.Count != size.X * size.Y) return Empty("incomplete_terrain_no_recommendations");
        var distance = Enumerable.Repeat(int.MaxValue, nodes.Count).ToArray();
        var parent = Enumerable.Repeat(-1, nodes.Count).ToArray();
        var queue = new PriorityQueue<int,int>(); int roots = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (n.X >= target.X && n.X < target.X + target.Width && n.Y >= target.Y && n.Y < target.Y + target.Height &&
                n.Span.Bottom <= target.Z && target.Z < n.Span.Top)
            { distance[i] = n.Span.Bottom; queue.Enqueue(i, distance[i]); roots++; }
        }
        if (roots == 0) return Empty("target_has_no_observed_void");
        while (queue.TryDequeue(out int current, out int score))
        {
            ct.ThrowIfCancellationRequested(); if (score != distance[current]) continue;
            var n = nodes[current];
            foreach (var d in Directions)
            {
                if (!byColumn.TryGetValue((n.X+d.X,n.Y+d.Y), out var others)) continue;
                foreach (int next in others)
                {
                    var b = nodes[next].Span;
                    if (Math.Max(n.Span.Bottom,b.Bottom) >= Math.Min(n.Span.Top,b.Top)) continue;
                    int pass = Math.Max(score,b.Bottom);
                    if (pass >= distance[next]) continue;
                    distance[next] = pass; parent[next] = current; queue.Enqueue(next,pass);
                }
            }
        }
        bool InTarget(Node n) => n.X >= target.X && n.X < target.X + target.Width && n.Y >= target.Y && n.Y < target.Y + target.Height;
        var sourceNodes = (objects ?? []).Where(o => o.Kind == "special_or_unknown" &&
            (o.Template.Contains("WaterSource",StringComparison.OrdinalIgnoreCase) || o.Template.Contains("WaterSeep",StringComparison.OrdinalIgnoreCase)))
            .SelectMany(o => byColumn.TryGetValue((o.X,o.Y),out var ids)
                ? ids.Where(i => nodes[i].Span.Bottom <= o.Z && o.Z < nodes[i].Span.Top)
                : Enumerable.Empty<int>()).ToHashSet();
        var candidates = Enumerable.Range(0,nodes.Count).Where(i => distance[i] != int.MaxValue && !InTarget(nodes[i]) &&
            (sourceNodes.Contains(i) || nodes[i].Span.WaterDepth > 0.01f || nodes[i].X == 0 || nodes[i].Y == 0 || nodes[i].X == size.X-1 || nodes[i].Y == size.Y-1)).ToArray();
        // Limit representative corridors, not analysis coverage. Prefer water with head above the pass.
        var selected = candidates.OrderByDescending(i => sourceNodes.Contains(i))
            .ThenByDescending(i => nodes[i].Span.WaterDepth > 0.01f && nodes[i].Span.Bottom + nodes[i].Span.WaterDepth > distance[i])
            .ThenByDescending(i => nodes[i].Span.Contamination).ThenBy(i => distance[i])
            .ThenBy(i => Math.Abs(nodes[i].X-target.X)+Math.Abs(nodes[i].Y-target.Y))
            .GroupBy(i => (nodes[i].X/16,nodes[i].Y/16,nodes[i].Span.Bottom, Wet:nodes[i].Span.WaterDepth>0.01f))
            .Select(g => g.First()).Take(8).ToArray();
        var routes = selected.Select(Route).ToArray();
        var higher = Enumerable.Range(0,nodes.Count).Where(i => distance[i] != int.MaxValue && nodes[i].Span.Top == size.Z &&
                nodes[i].Span.Bottom > Math.Max(target.Z, scenarioLevel ?? target.Z) && nodes[i].Span.WaterDepth <= 0.01f)
            .OrderBy(i => Math.Abs(nodes[i].X-target.X)+Math.Abs(nodes[i].Y-target.Y))
            .GroupBy(i => (nodes[i].X/8,nodes[i].Y/8)).Select(g => Point(g.First())).Take(8).ToArray();
        return new("terrain_analysis_complete",nodes.Count,distance.Count(d => d!=int.MaxValue),columns.Count,caves,roots,candidates.Length,routes,higher,Limits);

        Position Point(int i) => new(nodes[i].X,nodes[i].Y,nodes[i].Span.Bottom);
        TerrainRoute Route(int start)
        {
            var chain = new List<int>(); int p = start;
            while (p >= 0) { chain.Add(p); p = parent[p]; }
            // A candidate is a saddle on this one route, not proof that blocking it closes all alternatives.
            int saddle = chain.First(i => nodes[i].Span.Bottom == distance[start]);
            var n = nodes[start]; var at = Point(saddle); var banks = new List<Position>();
            foreach (var d in Directions)
                if (byColumn.TryGetValue((at.X+d.X,at.Y+d.Y),out var ids))
                    foreach (int id in ids.Where(i => nodes[i].Span.Bottom > at.Z)) banks.Add(Point(id));
            float? observed = n.Span.WaterDepth > 0.01f ? n.Span.Bottom+n.Span.WaterDepth : null;
            float? level = scenarioLevel ?? observed;
            var corners = new List<Position> {Point(chain[0])};
            for(int j=1;j<chain.Count-1;j++)
            {
                var a=Point(chain[j-1]);var b=Point(chain[j]);var c=Point(chain[j+1]);
                if((b.X-a.X,b.Y-a.Y,b.Z-a.Z)!=(c.X-b.X,c.Y-b.Y,c.Z-b.Z))corners.Add(b);
            }
            if(chain.Count>1)corners.Add(Point(chain[^1]));
            var path = corners.Take(64).ToList();
            if (corners.Count > 64) path.Add(corners[^1]);
            int crest = Math.Max((int)Math.Ceiling(level ?? distance[start]),distance[start]) + 1;
            int k = chain.IndexOf(saddle); var along = Point(chain[Math.Min(k+1,chain.Count-1)]);
            int dx = along.X-at.X, dy = along.Y-at.Y;
            if (dx == 0 && dy == 0 && k > 0) { var previous = Point(chain[k-1]); dx=at.X-previous.X; dy=at.Y-previous.Y; }
            if (dx == 0 && dy == 0) dx=1;
            var section = new List<ProtectionCell> { new(at,Math.Max(0,crest-at.Z)) }; int bankCount=0;
            foreach (int sign in new[]{-1,1}) for (int step=1;step<=16;step++)
            {
                int x=at.X-sign*dy*step, y=at.Y+sign*dx*step;
                if (!byColumn.TryGetValue((x,y),out var ids)) break;
                var open = ids.Where(i => nodes[i].Span.Bottom < crest && nodes[i].Span.Top > at.Z).ToArray();
                if (open.Length==0) { bankCount++; break; }
                int floor=open.Min(i=>nodes[i].Span.Bottom);
                section.Add(new(new(x,y,floor),crest-floor));
            }
            return new(sourceNodes.Contains(start) ? "source_template_hint_to_zone" : observed.HasValue ? "observed_water_to_zone" : "map_boundary_to_zone", Point(start), observed,
                n.Span.Contamination,distance[start],level.HasValue ? level.Value > distance[start] : null,
                path.ToArray(),chain.Count,corners.Count>64,new(at,banks.ToArray(),"inspect_cross_section_for_closure_or_diversion",
                    crest,crest<size.Z,level.HasValue && level.Value < distance[start] ? "terrain_pass_plus_one_level_above_lower_reference_not_forecast" : scenarioLevel.HasValue ? "requested_scenario_plus_one_level" : observed.HasValue ? "observed_level_plus_one_level" : "terrain_pass_plus_one_level_not_forecast",
                    section.ToArray(),bankCount==2,
                    ["survey_full_cross_section_and_parallel_bypasses", "check_existing_water_structures_and_caves",
                    "preserve_safe_outlet_and_check_backwater", "validate_unlock_cost_builder_access_and_construction_phases",
                    "reobserve_water_before_and_after_intervention"]));
        }
        StrategicAnalysis Empty(string state) => new(state,nodes.Count,0,columns.Count,caves,0,0,[],[],Limits);
    }
}
