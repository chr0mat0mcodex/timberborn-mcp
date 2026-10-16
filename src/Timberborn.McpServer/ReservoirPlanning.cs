using Timberborn.Backend.Native;

namespace Timberborn.McpServer;

public sealed record ReservoirOptions(string State, int ExaminedFootprints, int GeometricCandidates,
    string[] Limitations, ReservoirOption[] Options);
public sealed record ReservoirOption(string Kind, string Benefit, Position Origin, int Width, int Height,
    int ExcavationDepth, int TargetFloor, int AddedGeometricVolume, string RefillEvidence,
    string NextAction, string[] Blockers, ProtectionCell[] ExcavationCells);

// Bounded extension of the terrain scan, not a second hydraulic or building planner.
public static class ReservoirPlanning
{
    public static ReservoirOptions Analyze(Position size, IReadOnlyList<StrategicColumn> columns,
        ProtectedZone target, IReadOnlyList<StrategicObject> objects, bool objectsComplete, CancellationToken ct)
    {
        string[] limits=["geometric_volume_in_grid_cubes_not_water_goods_or_days",
            "future_refill_contamination_evaporation_and_pumping_unverified",
            "object_anchors_not_full_footprints_native_occupancy_check_required",
            "research_excavation_tools_cost_access_and_construction_phases_unverified",
            "fixed_3x3_two_level_candidates_within_32_cells_not_exhaustive",
            "no_automatic_excavation_or_water_supply_guarantee"];
        if(columns.Count!=size.X*size.Y) return new("incomplete_terrain_no_options",0,0,limits,[]);
        var map=columns.ToDictionary(c=>(c.X,c.Y));
        var occupied=objects.Select(o=>(o.X,o.Y)).ToHashSet();
        var candidates=new List<ReservoirOption>(); int examined=0;
        for(int y=Math.Max(1,target.Y-32);y<=Math.Min(size.Y-4,target.Y+target.Height+31);y++)
        for(int x=Math.Max(1,target.X-32);x<=Math.Min(size.X-4,target.X+target.Width+31);x++)
        {
            ct.ThrowIfCancellationRequested(); examined++;
            // Preserve the requested inhabited zone and a one-cell margin.
            if(x<=target.X+target.Width && x+2>=target.X-1 && y<=target.Y+target.Height && y+2>=target.Y-1) continue;
            var surface=map[(x,y)].Spans.LastOrDefault(s=>s.Top==size.Z);
            if(surface is null || surface.Bottom<3) continue;
            int floor=surface.Bottom, bed=floor-2; bool blocked=false, refill=false, dirty=false;
            var cells=new List<ProtectionCell>();
            for(int yy=y-1;yy<=y+3;yy++) for(int xx=x-1;xx<=x+3;xx++)
            {
                var column=map[(xx,yy)]; bool inside=xx>=x && xx<x+3 && yy>=y && yy<y+3;
                if(occupied.Contains((xx,yy))) blocked=true;
                // Keep one solid block under the excavation, and no lateral opening through its banks.
                if(column.Spans.Any(s=>s.Bottom<floor && s.Top>(inside?bed-1:bed))) blocked=true;
                var top=column.Spans.LastOrDefault(s=>s.Top==size.Z);
                if(inside)
                {
                    if(top is null || top.Bottom!=floor) blocked=true;
                    cells.Add(new(new(xx,yy,floor),2));
                }
                if(top is {WaterDepth: >0.01f})
                {
                    if(top.Contamination>0.00001f) dirty=true;
                    if(top.Bottom<=floor && top.Bottom+top.WaterDepth>floor+0.01f) refill=true;
                }
            }
            if(blocked) continue;
            var blockers=new List<string>();
            if(dirty) blockers.Add("local_contaminated_water_not_a_clean_reserve");
            if(!refill) blockers.Add("no_observed_water_above_rim_refill_route_required");
            if(!objectsComplete) blockers.Add("object_inventory_incomplete");
            candidates.Add(new("deepen_3x3_basin_by_two_levels","additional_potential_water_storage",
                new(x,y,floor),3,3,2,bed,18,dirty?"local_contamination_observed":refill?"local_water_above_rim_not_future_supply_proof":"refill_unproven",
                blockers.Count>0?"resolve_water_and_inventory_blockers_before_excavation_planning":"validate_excavation_tools_access_occupancy_and_controlled_refill",
                blockers.ToArray(),cells.ToArray()));
        }
        var selected=new List<ReservoirOption>();
        foreach(var candidate in candidates.OrderBy(c=>c.Blockers.Length).ThenBy(c=>Math.Abs(c.Origin.X-target.X)+Math.Abs(c.Origin.Y-target.Y)))
        {
            if(selected.Any(c=>Math.Abs(c.Origin.X-candidate.Origin.X)<4 && Math.Abs(c.Origin.Y-candidate.Origin.Y)<4)) continue;
            selected.Add(candidate); if(selected.Count==3)break;
        }
        return new(selected.Count>0?"conditional_reservoir_options":"no_candidate_in_bounded_search",examined,candidates.Count,limits,selected.ToArray());
    }
}
