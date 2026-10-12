using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Timberborn.BlockSystem;
using Timberborn.RecoveredGoodSystem;
using Timberborn.Bridge.Core;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.Navigation;
using Timberborn.PathSystem;
using Timberborn.TemplateSystem;
using UnityEngine;
using Cell = Timberborn.Bridge.Core.AccessRouteSearch.Cell;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    // Bounded geometric proposals, followed by the existing native joint preview.
    // No construction-state inference from finished preview geometry.
    private object PlanAccess(LargeProjectRequest request)
    {
        if(faulted || speed.CurrentSpeed!=0 || activeBatch is not null || !ConstructionIsolation.Allows(entities))
            throw new BridgeRejectionException("state_conflict");
        var district=District(request.DistrictId);
        var steps=request.Steps.Select(s=>new LargeProjectStep {Template=s.Template,X=s.X,Y=s.Y,Z=s.Z,Rotation=s.Rotation,DependsOn=s.DependsOn.ToArray()}).ToList();
        var originals=steps.Select(s=>Resolve(s).GetSpec<BlockObjectSpec>().GetBlocks(Placement(s)).Take(BuildingPolicy.MaxGeometryCells + 1).ToArray()).ToArray();
        if(originals.Any(c=>c.Length is <1 or >BuildingPolicy.MaxGeometryCells)) throw new ArgumentException("project_occupied_extent_limit");
        var occupied=originals.SelectMany(c=>c).ToArray();
        int minX=Math.Max(0,occupied.Min(c=>c.Coordinates.x)-12), minY=Math.Max(0,occupied.Min(c=>c.Coordinates.y)-12);
        int maxX=Math.Min(4095,occupied.Max(c=>c.Coordinates.x)+12), maxY=Math.Min(4095,occupied.Max(c=>c.Coordinates.y)+12);
        int maxZ=occupied.Max(c=>c.Coordinates.z), minZ=Math.Max(0,maxZ-7);
        var permanent=new HashSet<int>(); var reports=new List<object>(); int visited=0;
        string reason="candidate";
        int? blockedPart=null; string phase="targets"; int workCellCount=0;
        var searchClock=System.Diagnostics.Stopwatch.StartNew();
        var specs=new Dictionary<string,BlockObjectSpec>();
        var unavailable=new HashSet<string>();
        var geometryCache=new Dictionary<(string,int,int,int,int),Block[]>();
        Block[] Geometry(LargeProjectStep s)
        {
            var key=(s.Template,s.X,s.Y,s.Z,s.Rotation);
            if(!geometryCache.TryGetValue(key,out var value))
            {
                if(unavailable.Contains(s.Template))return Array.Empty<Block>();
                if(!specs.TryGetValue(s.Template,out var spec))
                {
                    try {spec=Resolve(s).GetSpec<BlockObjectSpec>();specs.Add(s.Template,spec);}
                    catch(Exception ex) when((s.Template is "Stairs.Folktails" or "Platform.Folktails") && (ex is ArgumentException or BridgeRejectionException))
                    {unavailable.Add(s.Template);return Array.Empty<Block>();}
                }
                value=spec.GetBlocks(Placement(s)).Take(BuildingPolicy.MaxGeometryCells + 1).ToArray();
                geometryCache.Add(key,value);
            }
            return value;
        }
        bool Inside(Cell c)=>c.X>=minX&&c.X<=maxX&&c.Y>=minY&&c.Y<=maxY&&c.Z>=minZ&&c.Z<=maxZ&&blocks.Contains(new Vector3Int(c.X,c.Y,c.Z));
        bool Supported(Vector3Int p)=>terrain.OnGround(p) || p.z>0&&blocks.GetObjectsAt(p+new Vector3Int(0,0,-1)).Any(b=>!b.IsPreview&&b.IsFinished&&
            b.PositionedBlocks.GetAllBlocks().Any(c=>c.Coordinates==p+new Vector3Int(0,0,-1)&&c.Stackable==BlockStackable.BlockObject));
        bool Same(LargeProjectStep a,LargeProjectStep b)=>a.Template==b.Template&&a.X==b.X&&a.Y==b.Y&&a.Z==b.Z&&a.Rotation==b.Rotation;
        bool Reserved(LargeProjectStep s)=>Geometry(s).Any(c=>request.ReservedAreas.Any(a=>a.Contains(c.Coordinates.x,c.Coordinates.y,c.Coordinates.z)));
        bool Clear(LargeProjectStep s,IEnumerable<LargeProjectStep> additional)
        {
            var shape=Geometry(s);
            return !Reserved(s) && shape.Length is >0 and <=BuildingPolicy.MaxGeometryCells && shape.All(c=>Inside(new(c.Coordinates.x,c.Coordinates.y,c.Coordinates.z))&&
                (!terrain.Underground(c.Coordinates)||c.Underground||c.OptionallyUnderground)&&
                !blocks.GetObjectsAt(c.Coordinates).Any(b=>!b.IsPreview&&!b.HasComponent<RecoveredGoodStack>()&&b.IsIntersecting(c))&&
                !steps.Concat(additional).Where(other=>!Same(s,other)).Any(other=>Geometry(other).Any(c.IsIntersecting)));
        }
        // Add a support column only where the public template allows stackable support.
        bool Supports(LargeProjectStep s,List<LargeProjectStep> package)
        {
            foreach(var foundation in Geometry(s).Where(c=>c.IsFoundationBlock))
            {
                var p=foundation.Coordinates;
                if(Supported(p)) continue;
                if(foundation.MatterBelow.ToString() is not ("GroundOrStackable" or "Stackable")) return false;
                var column=new List<LargeProjectStep>();
                while(!Supported(p))
                {
                    var below=p+new Vector3Int(0,0,-1);
                    if(steps.Concat(package).Any(other=>Geometry(other).Any(c=>c.Coordinates==below&&c.Stackable==BlockStackable.BlockObject))) break;
                    var support=new LargeProjectStep{Template="Platform.Folktails",X=below.x,Y=below.y,Z=below.z};
                    if(!Inside(new(below.x,below.y,below.z))||!Clear(support,package.Concat(column))) return false;
                    column.Add(support); p=below;
                    if(column.Count+package.Count+steps.Count>32) return false;
                }
                column.Reverse(); package.AddRange(column);
            }
            return true;
        }
        int Add(LargeProjectStep s,IEnumerable<int> required,bool keep)
        {
            int existing=steps.FindIndex(other=>Same(s,other));
            if(existing>=0) {if(keep) permanent.Add(existing); return existing;}
            var foundations=Geometry(s).Where(c=>c.IsFoundationBlock).ToArray();
            var deps=required.Concat(Enumerable.Range(0,steps.Count).Where(i=>Geometry(steps[i]).Any(c=>c.Stackable==BlockStackable.BlockObject&&
                foundations.Any(f=>f.Coordinates==c.Coordinates+new Vector3Int(0,0,1))))).Distinct().OrderBy(i=>i).ToArray();
            s.DependsOn=deps; int index=steps.Count; steps.Add(s); if(keep)permanent.Add(index); return index;
        }
        var sources=new Dictionary<Cell,int>();
        foreach(var e in entities.Entities.Where(e=>!e.Deleted&&e.Initialized))
        {
            if(!e.TryGetComponent<BlockObject>(out var b)||b.IsPreview||!b.IsFinished||!e.TryGetComponent<PathSpec>(out var path))continue;
            var p=b.TransformCoordinates(path.MainPathCoordinates); var c=new Cell(p.x,p.y,p.z);
            if(Inside(c)&&district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(p))) sources[c]=-1;
        }
        var directions=new[]{(X:0,Y:-1),(X:-1,Y:0),(X:0,Y:1),(X:1,Y:0)};
        IEnumerable<AccessRouteSearch.Edge> Expand(Cell from)
        {
            for(int rotation=0;rotation<4;rotation++) foreach(int dz in new[]{0,1,-1})
            {
                if(searchClock.Elapsed.TotalSeconds>5)yield break;
                var d=directions[rotation]; int stride=dz==0?1:2;
                var to=new Cell(from.X+d.X*stride,from.Y+d.Y*stride,from.Z+dz);
                if(!Inside(to))continue;
                var package=new List<LargeProjectStep>();
                if(dz!=0)
                {
                    var stair=new LargeProjectStep{Template="Stairs.Folktails",X=from.X+d.X,Y=from.Y+d.Y,Z=Math.Min(from.Z,to.Z),Rotation=dz>0?rotation:(rotation+2)%4};
                    if(!Clear(stair,package)||!Supports(stair,package))continue;
                    package.Add(stair);
                }
                var road=new LargeProjectStep{Template="Path",X=to.X,Y=to.Y,Z=to.Z};
                if(!Clear(road,package)||!Supports(road,package))continue;
                package.Add(road);
                yield return new AccessRouteSearch.Edge{To=to,Parts=package.ToArray()};
            }
        }
        bool Route(HashSet<Cell> targets,bool keep,out int[] dependencies)
        {
            dependencies=Array.Empty<int>();
            if(targets.Count==0) {reason="no_access_candidate";return false;}
            if(visited>=4096) {reason="search_budget_exhausted";return false;}
            var route=AccessRouteSearch.Find(sources.Keys,targets,Expand,32-steps.Count,Math.Min(2048,4096-visited));
            visited+=route.Visited;
            if(!route.Found){reason=searchClock.Elapsed.TotalSeconds>5?"search_time_budget_exhausted":route.Reason;return false;}
            var used=new List<int>(); int previous=sources[route.Source];
            if(previous>=0)used.Add(previous);
            foreach(var edge in route.Edges)
            {
                foreach(var part in edge.Parts)
                { previous=Add(part,previous>=0?new[]{previous}:Array.Empty<int>(),keep); used.Add(previous); }
                sources[edge.To]=previous;
            }
            if(keep)
            {
                permanent.UnionWith(AccessRouteSearch.RequiredParts(steps,used));
            }
            dependencies=used.Distinct().ToArray(); return true;
        }
        foreach(int index in LargeProjectRequest.Order(request.Steps))
        {
            blockedPart=index; phase="target_geometry"; workCellCount=0;
            var s=steps[index]; var template=Resolve(s); var spec=template.GetSpec<BlockObjectSpec>();
            if(template.HasSpec<PathSpec>()) {reason="explicit_path_target_use_manual_plan";break;}
            if(Reserved(s)) {reason="reserved_area";break;}
            if(!Clear(s,Array.Empty<LargeProjectStep>())) {reason="target_intersects_existing_or_planned_object";break;}
            var support=new List<LargeProjectStep>();
            if(!Supports(s,support)){reason="unsupported_target_foundation";break;}
            int previous=-1;var deps=s.DependsOn.ToList();
            foreach(var part in support){previous=Add(part,previous>=0?new[]{previous}:Array.Empty<int>(),true);deps.Add(previous);}
            if(spec.Entrance.HasEntrance)
            {
                phase="finished_entrance";
                var p=PositionedEntrance.From(spec.GetBlocks(),spec.Entrance,Placement(s)).Coordinates;
                if(!Route(new(){new(p.x,p.y,p.z)},true,out var access)){break;}
                deps.AddRange(access);
            }
            // Use occupied native geometry, not the bounding volume (wind rotors have voids).
            // Diagonal perimeter cells matter too; native sites may expose those as accesses.
            phase="construction_work_cell";
            var obstacles=steps.Where(part=>!Resolve(part).HasSpec<PathSpec>()).SelectMany(Geometry).ToArray();
            var targets=WorkCells(originals[index],obstacles).Where(Inside).ToHashSet();
            workCellCount=targets.Count;
            if(!Route(targets,false,out var builder)){break;}
            deps.AddRange(builder);
            var directSupports=Enumerable.Range(0,steps.Count).Where(i=>i!=index&&Geometry(steps[i]).Any(c=>c.Stackable==BlockStackable.BlockObject&&
                originals[index].Any(f=>f.IsFoundationBlock&&f.Coordinates==c.Coordinates+new Vector3Int(0,0,1)))).ToArray();
            deps.AddRange(directSupports);
            permanent.UnionWith(AccessRouteSearch.RequiredParts(steps,directSupports));
            s.DependsOn=deps.Distinct().OrderBy(i=>i).ToArray();
            reports.Add(new{index,entrance=spec.Entrance.HasEntrance?"route_proposed":"not_applicable",constructionAccess="work_cell_route_proposed",candidateCells=workCellCount,requiredHelpers=builder});
        }
        JObject? plan=null;
        if(reason=="candidate")
        {
            blockedPart=null;phase="native_joint_preview";
            try
            {
                var generated=LargeProjectRequest.Parse("/agent-api/v1/large-project-plan",new NameValueCollection{{"session",request.Session},{"districtId",request.DistrictId},{"steps",LargeProjectRequest.Encode(steps.ToArray())}});
                plan=Plan(generated);
                if((bool?)plan["pilotEligible"]!=true)
                {
                    reason="native_joint_preview_rejected";
                    var failed=plan["parts"]!.FirstOrDefault(p=>(bool?)p["constructionWorkAccess"]?["required"]==true &&
                        (string?)p["constructionWorkAccess"]?["state"]!="road_reachable");
                    if(failed is not null) { blockedPart=(int?)failed["index"];phase="construction_work_cell"; }
                }
            }
            catch(ArgumentException){reason="generated_plan_extent_or_dependency_limit";}
        }
        return new {reservedAreasKey=ReservedArea.Encode(request.ReservedAreas),reservedAreasClear=!steps.Any(Reserved),candidateFound=reason=="candidate",reason,blockedPart,phase,workCellCount,steps=steps.Select(s=>new{template=s.Template,x=s.X,y=s.Y,z=s.Z,rotation=s.Rotation,dependsOn=s.DependsOn}).ToArray(),plan,
            assessment=new{requestedParts=request.Steps.Length,generatedParts=steps.Count-request.Steps.Length,visitedNodes=visited,
                region=new{minX,minY,minZ,maxX,maxY,maxZ},parts=reports.ToArray(),constructionAccess="unproven_until_live_construction",regularExecutionAllowed=false},
            cleanupCandidateIndices=Enumerable.Range(request.Steps.Length,steps.Count-request.Steps.Length).Where(i=>!permanent.Contains(i)).Reverse().ToArray(),
            limitations=new[]{"bounded_geometric_search_not_complete_route_coverage","existing_roads_required_within_twelve_cell_margin","maximum_32_parts_and_eight_occupied_levels","folktails_stairs_and_single_platforms_only","native_road_to_work_cell_not_hypothetical_builder_proof","cleanup_candidates_require_fresh_access_check","never_delete_recovered_resource_stacks","run_returned_steps_and_nested_plan_key_without_replanning"}};
    }
}
