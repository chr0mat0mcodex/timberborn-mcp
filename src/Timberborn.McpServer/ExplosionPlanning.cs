using Timberborn.Backend.Native;

namespace Timberborn.McpServer;

public sealed record ExplosionWarnings(string State, int Cores, int AssessedCores, bool Complete,
    ExplosionWarning[] Priorities, string[] Limitations);
public sealed record ExplosionWarning(Guid CoreId, Position Core, string State, float? Radius,
    bool? CurrentlyBlocked, string Timing, int OpeningCells, ExplosionChain[] Chains, string[] Unresolved);
public sealed record ExplosionChain(Position StoredLiquid, float Contamination, float ObservedLevel,
    string ConnectionChange, int? BeforePass, int AfterPass, Position TargetEntry, Position[] Route,
    bool RouteTruncated, ProtectionCandidate Protection, string ProtectionAssessment,
    string[] Measures, BackwaterAssessment? Backwater = null);

// A counterfactual terrain analysis, never an explosion or a fluid simulation.
public static class ExplosionPlanning
{
    public static ExplosionWarnings Analyze(Position size, IReadOnlyList<StrategicColumn> columns,
        ProtectedZone target, IReadOnlyList<StrategicObject> objects, bool objectsComplete, CancellationToken ct)
    {
        var cores=objects.Where(o=>o.Explosion is not null || o.Template=="UnstableCore")
            .OrderBy(o=>Math.Abs(o.X-target.X)+Math.Abs(o.Y-target.Y)).ThenBy(o=>o.Id).ToArray();
        var warnings=new List<ExplosionWarning>(); int assessed=0;
        int budget=Math.Min(16,2_097_152/Math.Max(1,columns.Count));
        foreach(var core in cores) {
            ct.ThrowIfCancellationRequested();
            var position=new Position(core.X,core.Y,core.Z); var e=core.Explosion;
            if(e is null || !e.Complete || assessed>=budget) {
                warnings.Add(new(core.Id,position,"explosion_risk_unassessed",e?.Radius,e?.Blocked,"unknown",
                    e?.TerrainCells.Length ?? 0,[],[e is null?"native_explosion_geometry_missing":!e.Complete?"native_explosion_geometry_truncated":"explosion_analysis_budget_exceeded",
                    "do_not_treat_missing_chain_as_safe"]));
                continue;
            }
            assessed++;
            var removed=e.TerrainCells.ToHashSet();
            var changed=OpenTerrain(size,columns,removed,ct);
            // The same native footprint filters both observations; unrelated distant
            // reservoirs must not displace the core's chains from the short warning.
            var after=StrategicMapAnalysis.Analyze(size,changed,target,null,ct,includeDerived:false,routeThrough:removed);
            var before=StrategicMapAnalysis.Analyze(size,columns,target,null,ct,includeDerived:false,
                origins:after.Corridors.Select(r=>r.Origin).ToHashSet());
            var old=before.Corridors.ToDictionary(r=>r.Origin,r=>r);
            var chains=(after.Hazards?.Priorities ?? []).Select(h=> {
                old.TryGetValue(h.Origin,out var previous);
                var route=after.Corridors.First(r=>r.Origin==h.Origin);
                bool protectionInBlast=h.Candidate.CrossSection.Any(c=>removed.Any(p=>p.X==c.At.X && p.Y==c.At.Y &&
                    p.Z>=c.At.Z && p.Z<h.Candidate.AssumedBarrierCrest));
                // Even previously empty cells can be inside the blast envelope. Do
                // not call a barrier safe merely because no terrain was removed there.
                bool nearCore=h.Candidate.CrossSection.Any(c=>Math.Abs(c.At.X-core.X)<=e.Radius+1 && Math.Abs(c.At.Y-core.Y)<=e.Radius+1);
                string assessment=protectionInBlast||nearCore?"protection_inside_possible_blast_relocate":h.ClosureAssessment;
                return new ExplosionChain(h.Origin,h.Contamination,h.ObservedLevel,
                    previous is null?"post_breach_route_before_connection_unresolved":previous.MinimumTerrainPassLevel>route.MinimumTerrainPassLevel?
                        "terrain_pass_lowered":"existing_connection_may_worsen",previous?.MinimumTerrainPassLevel,
                    route.MinimumTerrainPassLevel,h.TargetEntry,route.Path,route.PathTruncated,h.Candidate,assessment,
                    ["do_not_close_cross_section_without_verified_outlet_and_backwater_review",
                    "find_barrier_outside_blast_and_recheck_all_bypasses",
                    "protect_target_entry_and_isolate_drinking_water_intake",
                    "verify_build_access_materials_and_completion_before_trigger",
                    "diversion_only_after_verified_safe_outlet_and_backwater_check"],h.Backwater);
            }).OrderByDescending(c=>c.Contamination>0.00001f).ThenByDescending(c=>c.ObservedLevel-c.AfterPass).Take(3).ToArray();
            warnings.Add(new(core.Id,position,chains.Any(c=>c.Contamination>0.00001f)?"badwater_breach_to_target_potential":
                chains.Length>0?"water_breach_to_target_potential":"no_chain_proven_not_safe",e.Radius,e.Blocked,e.Timing,removed.Count,chains,
                ["activation_time_and_arrival_time_unknown","native_radius_scenario_not_final_destruction_or_collapse",
                 "stored_water_head_scenario_not_volume_conserving_flow_prediction","waterworks_and_object_barriers_not_modelled",
                 "protected_zone_is_user_target_not_verified_pond_identity",
                 e.Blocked?"currently_blocked_is_not_permanent_safety":"activation_state_unknown"]));
        }
        return new(cores.Length==0?"no_core_observed_not_safe":"pre_explosion_action_required",cores.Length,assessed,
            objectsComplete && assessed==cores.Length,warnings.OrderByDescending(w=>w.Chains.Any(c=>c.Contamination>0.00001f))
                .ThenByDescending(w=>w.Chains.Length).ToArray(),
            ["bounded_native_radius_terrain_removal_scenarios","all_core_warnings_retained_in_compact",
             "no_explosion_triggered_no_terrain_mutation","no_arrival_time_or_complete_cutset_guarantee"]);
    }

    internal static StrategicColumn[] OpenTerrain(Position size, IReadOnlyList<StrategicColumn> columns,
        IReadOnlySet<Position> removed, CancellationToken ct)
    {
        var byColumn=removed.GroupBy(p=>(p.X,p.Y)).ToDictionary(g=>g.Key,g=>g.Select(p=>p.Z).ToHashSet());
        return columns.Select(c=> {
            ct.ThrowIfCancellationRequested();
            if(!byColumn.TryGetValue((c.X,c.Y),out var levels)) return c;
            var spans=new List<StrategicSpan>(); int z=0;
            bool Open(int level)=>levels.Contains(level)||c.Spans.Any(s=>s.Bottom<=level && level<s.Top);
            while(z<size.Z) {
                if(!Open(z)) {z++;continue;}
                int bottom=z; while(z<size.Z && Open(z)) z++;
                var wet=c.Spans.Where(s=>s.Bottom<z && s.Top>bottom && s.WaterDepth>0.01f).ToArray();
                float head=wet.Length==0?bottom:wet.Max(s=>s.Bottom+s.WaterDepth);
                spans.Add(new(bottom,z,Math.Max(0,head-bottom),wet.Length==0?0:wet.Max(s=>s.Contamination)));
            }
            return new StrategicColumn(c.X,c.Y,spans.ToArray());
        }).ToArray();
    }
}
