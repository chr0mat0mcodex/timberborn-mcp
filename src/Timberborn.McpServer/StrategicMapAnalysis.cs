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
public sealed record StrategicHazards(string State, int AssessedWetNodes, int PotentialInflowNodes,
    int ContaminatedPotentialInflowNodes, int FloodedTargetNodes, int RepresentedPotentialInflowNodes,
    string NextAction, StrategicHazard[] Priorities, string[]? CommonMeasures = null, string[]? CommonUnresolved = null);
public sealed record StrategicHazard(Position Origin, string Priority, string Evidence, float ObservedLevel,
    float HeadAboveTargetAndPass, int MinimumTerrainPassLevel, float Contamination, bool Covered, Position TargetEntry,
    ProtectionCandidate Candidate, string ClosureAssessment, string[] Measures, string[] Unresolved);
public sealed record StrategicAnalysis(string State, int VoidNodes, int ConnectedVoidNodes, int CoveredColumns,
    int CaveColumns, int TargetVoidNodes, int CandidateCount, TerrainRoute[] Corridors, Position[] HigherGround,
    string[] Limitations, StrategicHazards? Hazards = null, ReservoirOptions? Terraforming = null);

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
        ProtectedZone target, float? scenarioLevel, CancellationToken ct = default, IReadOnlyList<StrategicObject>? objects = null, bool objectsComplete = false)
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
        bool InTarget(Node n) => n.X >= target.X && n.X < target.X + target.Width && n.Y >= target.Y && n.Y < target.Y + target.Height &&
            n.Span.Bottom <= target.Z && target.Z < n.Span.Top;
        var sourceNodes = (objects ?? []).Where(o => o.Kind == "special_or_unknown" &&
            (o.Template.Contains("WaterSource",StringComparison.OrdinalIgnoreCase) || o.Template.Contains("WaterSeep",StringComparison.OrdinalIgnoreCase)))
            .SelectMany(o => byColumn.TryGetValue((o.X,o.Y),out var ids)
                ? ids.Where(i => nodes[i].Span.Bottom <= o.Z && o.Z < nodes[i].Span.Top)
                : Enumerable.Empty<int>()).ToHashSet();
        var candidates = Enumerable.Range(0,nodes.Count).Where(i => distance[i] != int.MaxValue && !InTarget(nodes[i]) &&
            (sourceNodes.Contains(i) || nodes[i].Span.WaterDepth > 0.01f || nodes[i].X == 0 || nodes[i].Y == 0 || nodes[i].X == size.X-1 || nodes[i].Y == size.Y-1)).ToArray();
        float Head(int i) => nodes[i].Span.Bottom + nodes[i].Span.WaterDepth - Math.Max(distance[i],target.Z);
        bool Threat(int i) => nodes[i].Span.WaterDepth > 0.01f && distance[i] != int.MaxValue && Head(i) > 0.01f;
        // Assess every reachable wet node before reducing the output to representatives.
        // Dry source hints must never displace stored liquid above the target and its terrain pass.
        var selected = candidates.OrderByDescending(Threat)
            .ThenByDescending(i => Threat(i) && nodes[i].Span.Contamination > 0.00001f)
            .ThenByDescending(i => Threat(i) ? Head(i) : float.MinValue)
            .ThenByDescending(i => sourceNodes.Contains(i))
            .ThenByDescending(i => nodes[i].Span.Contamination).ThenBy(i => distance[i])
            .ThenBy(i => Math.Abs(nodes[i].X-target.X)+Math.Abs(nodes[i].Y-target.Y))
            .GroupBy(i => (nodes[i].X/16,nodes[i].Y/16,nodes[i].Span.Bottom, Wet:nodes[i].Span.WaterDepth>0.01f))
            .Select(g => g.First()).Take(8).ToArray();
        var routes = selected.Select(i=>Route(i)).ToArray();
        int wet = Enumerable.Range(0,nodes.Count).Count(i => distance[i]!=int.MaxValue && nodes[i].Span.WaterDepth>0.01f);
        int threatened = candidates.Count(Threat);
        int contaminated = candidates.Count(i => Threat(i) && nodes[i].Span.Contamination>0.00001f);
        int flooded = Enumerable.Range(0,nodes.Count).Count(i => InTarget(nodes[i]) && nodes[i].Span.Bottom<=target.Z &&
            target.Z<nodes[i].Span.Top && nodes[i].Span.WaterDepth>0.01f && nodes[i].Span.Bottom+nodes[i].Span.WaterDepth>target.Z+0.01f);
        var priorities = selected.Select((id,index)=>(id,index)).Where(p=>Threat(p.id)).Select(p=>Hazard(p.id,routes[p.index])).ToArray();
        var hazards = new StrategicHazards(flooded>0?"target_water_observed":threatened>0?"potential_inflow_requires_action":"no_current_head_exceedance_not_safe",
            wet,threatened,contaminated,flooded,priorities.Length,
            flooded>0?"inspect_affected_supply_and_access_then_plan_protection":threatened>0?"inspect_priority_closures_and_plan_protection_before_expansion":"monitor_stored_water_sources_weather_and_terrain_changes",priorities);
        var higher = Enumerable.Range(0,nodes.Count).Where(i => distance[i] != int.MaxValue && nodes[i].Span.Top == size.Z &&
                nodes[i].Span.Bottom > Math.Max(target.Z, scenarioLevel ?? target.Z) && nodes[i].Span.WaterDepth <= 0.01f)
            .OrderBy(i => Math.Abs(nodes[i].X-target.X)+Math.Abs(nodes[i].Y-target.Y))
            .GroupBy(i => (nodes[i].X/8,nodes[i].Y/8)).Select(g => Point(g.First())).Take(8).ToArray();
        return new("terrain_analysis_complete",nodes.Count,distance.Count(d => d!=int.MaxValue),columns.Count,caves,roots,candidates.Length,routes,higher,Limits,hazards,
            ReservoirPlanning.Analyze(size,columns,target,objects ?? [],objectsComplete,ct));

        StrategicHazard Hazard(int start, TerrainRoute route)
        {
            var candidate=route.Candidate; var level=route.ObservedWaterLevel!.Value;
            // A low requested scenario must not undersize the protection of observed water.
            if(scenarioLevel.HasValue && scenarioLevel.Value<level) candidate=Route(start,level).Candidate;
            bool roofConflict=candidate.CrossSection.Any(cell => !byColumn[(cell.At.X,cell.At.Y)].Any(i =>
                nodes[i].Span.Bottom==cell.At.Z && nodes[i].Span.Top>=candidate.AssumedBarrierCrest));
            string closure=roofConflict?"blocked_by_cave_roof":!candidate.CrestWithinMapHeight?"outside_map_height":
                candidate.CrossSection.Any(c=>c.At.X>=target.X && c.At.X<target.X+target.Width && c.At.Y>=target.Y && c.At.Y<target.Y+target.Height)?"candidate_intersects_protected_zone_relocate_barrier":
                !candidate.BothTerrainBanksFound?"cross_section_incomplete":CheckClosure(start,candidate,level);
            return new(route.Origin,route.Contamination>0.00001f?"high_contaminated_inflow_potential":"high_water_inflow_potential",
                "observed_stored_liquid_above_target_and_terrain_pass_not_arrival_forecast",level,Head(start),route.MinimumTerrainPassLevel,route.Contamination,
                nodes[start].Span.Top<size.Z,route.Path[^1],candidate,closure,
                ["resurvey_origin_cross_section_and_target_entry",
                 closure=="terrain_route_cut_at_observed_level"?"plan_barrier_at_candidate_cross_section_with_native_build_validation":"find_alternative_barrier_or_bypass_closure_before_building",
                 "retain_safe_outlet_and_check_backwater_before_closure",
                 "protect_water_food_and_access_at_target_entry",
                 "reobserve_target_and_upstream_after_each_construction_phase"],
                ["existing_waterworks_unknown", "construction_access_cost_unlock_and_duration_unverified",
                 "other_origins_and_higher_future_levels_not_cut_proven", "no_explosion_countdown_or_arrival_time",
                 "no_excavation_or_diversion_recommended_without_verified_outlet"]);
        }

        string CheckClosure(int start, ProtectionCandidate candidate, float level)
        {
            var blocked=candidate.CrossSection.ToDictionary(c=>(c.At.X,c.At.Y),c=>c.At.Z);
            bool Open(int i) => nodes[i].Span.Bottom<level &&
                !(blocked.TryGetValue((nodes[i].X,nodes[i].Y),out int floor) &&
                  nodes[i].Span.Bottom>=floor && nodes[i].Span.Bottom<candidate.AssumedBarrierCrest);
            // Model a hypothetical solid barrier only. This is not a construction or hydraulic proof.
            var seen=new HashSet<int>(); var pending=new Queue<int>();
            if(!Open(start)) return "candidate_intersects_observed_reservoir_relocate_barrier";
            seen.Add(start);pending.Enqueue(start);
            while(pending.TryDequeue(out int i))
            {
                ct.ThrowIfCancellationRequested(); var n=nodes[i];
                if(InTarget(n) && n.Span.Bottom<=target.Z && target.Z<n.Span.Top) return "alternate_terrain_route_remains";
                foreach(var d in Directions)
                    if(byColumn.TryGetValue((n.X+d.X,n.Y+d.Y),out var others)) foreach(int next in others)
                        if(Open(next) && Math.Max(n.Span.Bottom,nodes[next].Span.Bottom)<Math.Min(n.Span.Top,nodes[next].Span.Top) && seen.Add(next))
                            pending.Enqueue(next);
            }
            return "terrain_route_cut_at_observed_level";
        }

        Position Point(int i) => new(nodes[i].X,nodes[i].Y,nodes[i].Span.Bottom);
        TerrainRoute Route(int start, float? protectionLevel = null)
        {
            var chain = new List<int>(); int p = start;
            while (p >= 0) { chain.Add(p); p = parent[p]; }
            // A candidate is a saddle on this one route, not proof that blocking it closes all alternatives.
            // Prefer the target-facing end of a level ridge, not the wet origin itself.
            var saddles=chain.Where(i=>nodes[i].Span.Bottom==distance[start]).ToArray();
            int saddle = saddles.LastOrDefault(i=>!InTarget(nodes[i]),saddles[^1]);
            var n = nodes[start]; var at = Point(saddle); var banks = new List<Position>();
            foreach (var d in Directions)
                if (byColumn.TryGetValue((at.X+d.X,at.Y+d.Y),out var ids))
                    foreach (int id in ids.Where(i => nodes[i].Span.Bottom > at.Z)) banks.Add(Point(id));
            float? observed = n.Span.WaterDepth > 0.01f ? n.Span.Bottom+n.Span.WaterDepth : null;
            float? level = protectionLevel ?? scenarioLevel ?? observed;
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
                    crest,crest<size.Z,level.HasValue && level.Value < distance[start] ? "terrain_pass_plus_one_level_above_lower_reference_not_forecast" : protectionLevel.HasValue ? "observed_level_plus_one_level" : scenarioLevel.HasValue ? "requested_scenario_plus_one_level" : observed.HasValue ? "observed_level_plus_one_level" : "terrain_pass_plus_one_level_not_forecast",
                    section.ToArray(),bankCount==2,
                    ["survey_full_cross_section_and_parallel_bypasses", "check_existing_water_structures_and_caves",
                    "preserve_safe_outlet_and_check_backwater", "validate_unlock_cost_builder_access_and_construction_phases",
                    "reobserve_water_before_and_after_intervention"]));
        }
        StrategicAnalysis Empty(string state) => new(state,nodes.Count,0,columns.Count,caves,0,0,[],[],Limits);
    }
}
