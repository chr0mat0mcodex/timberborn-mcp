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
        LargeProjectPlanningPolicy.RequireAvailable(faulted, speed.CurrentSpeed==0,
            activeBatch is not null, true);
        LargeProjectPlanningPolicy.RequireIsolation(ConstructionIsolation.Blocker(entities));
        var district=District(request.DistrictId, planning: true);
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
                !TerrainOccupancyPolicy.Intersects(c.IsOccupied,terrain.Underground(c.Coordinates),c.Underground,c.OptionallyUnderground)&&
                !blocks.GetObjectsAt(c.Coordinates).Any(b=>!b.IsPreview&&!b.HasComponent<RecoveredGoodStack>()&&b.IsIntersecting(c))&&
                !steps.Concat(additional).Where(other=>!Same(s,other)).Any(other=>Geometry(other).Any(c.IsIntersecting)));
        }
        // Add a support column only where the public template allows stackable support.
        bool Supports(LargeProjectStep s,List<LargeProjectStep> package,IReadOnlyList<LargeProjectStep>? prefix=null)
        {
            prefix ??= Array.Empty<LargeProjectStep>();
            foreach(var foundation in Geometry(s).Where(c=>c.IsFoundationBlock))
            {
                var p=foundation.Coordinates;
                if(Supported(p)) continue;
                if(foundation.MatterBelow.ToString() is not ("GroundOrStackable" or "Stackable")) return false;
                var column=new List<LargeProjectStep>();
                while(!Supported(p))
                {
                    var below=p+new Vector3Int(0,0,-1);
                    if(steps.Concat(prefix).Concat(package).Any(other=>Geometry(other).Any(c=>c.Coordinates==below&&c.Stackable==BlockStackable.BlockObject))) break;
                    var support=new LargeProjectStep{Template="Platform.Folktails",X=below.x,Y=below.y,Z=below.z};
                    if(!Inside(new(below.x,below.y,below.z))||!Clear(support,prefix.Concat(package).Concat(column))) return false;
                    column.Add(support); p=below;
                    if(column.Count+package.Count+prefix.Count+steps.Count>32) return false;
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
        var stairLandings=new Dictionary<Cell,Cell>();
        var stairLowerExits=new Dictionary<Cell,Cell>();
        var sourceEvidence=new List<(Cell Cell,string Template,bool Connected)>();
        var exitEvidence=new Dictionary<(Cell From,Cell To),JObject>();
        var routeEvidence=new List<object>();
        object Position(Cell cell)=>new{x=cell.X,y=cell.Y,z=cell.Z};
        int Distance(Cell a,Cell b)=>Math.Max(Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y),Math.Abs(a.Z-b.Z));
        int TargetDistance(Cell cell)=>request.Steps.Min(s=>Distance(cell,new(s.X,s.Y,s.Z)));
        foreach(var e in entities.Entities.Where(e=>!e.Deleted&&e.Initialized))
        {
            if(!e.TryGetComponent<BlockObject>(out var b)||b.IsPreview||!b.IsFinished||!e.TryGetComponent<PathSpec>(out var path))continue;
            var p=b.TransformCoordinates(path.MainPathCoordinates); var c=new Cell(p.x,p.y,p.z);
            if(!Inside(c))continue;
            bool connected=district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(p));
            sourceEvidence.Add((c,e.TryGetComponent<TemplateSpec>(out var template)?template.TemplateName:"Path",connected));
            if(connected) sources[c]=-1;
            if(connected&&template is not null&&template.TemplateName=="Stairs.Folktails")
            {
                // Candidate only: the ordinary straight stair rises towards its
                // rotated forward neighbour. Native registered preview is authority.
                for(int rotation=0;rotation<4;rotation++)if(b.Orientation==Rotation(rotation))
                {
                    stairLandings[c]=AccessRouteSearch.StraightStairLandingCandidate(new(b.Coordinates.x,b.Coordinates.y,b.Coordinates.z),rotation);
                    stairLowerExits[c]=AccessRouteSearch.StraightStairLowerCandidate(new(b.Coordinates.x,b.Coordinates.y,b.Coordinates.z),rotation);
                }
            }
        }
        var existingSources=sources.Keys.ToHashSet();
        var existingExits=new Dictionary<Cell,Cell[]>();
        var landingChecks=new Dictionary<Cell,bool>();int landingPreviewCount=0;
        bool? LandingConnected(Cell to)
        {
            // Existing connected destinations already are search sources.
            if(existingSources.Contains(to))return null;
            if(landingChecks.TryGetValue(to,out bool cached))return cached;
            if(landingPreviewCount>=8||searchClock.Elapsed.TotalSeconds>5)return null;
            var road=new LargeProjectStep{Template="Path",X=to.X,Y=to.Y,Z=to.Z};
            var package=new List<LargeProjectStep>();
            if(!Clear(road,Array.Empty<LargeProjectStep>())||!Supports(road,package))return null;
            package.Add(road);
            for(int i=0;i<package.Count;i++)package[i].DependsOn=i==0?Array.Empty<int>():new[]{i-1};
            landingPreviewCount++;
            // Reuse the guarded joint-preview lifecycle: no placement, validation
            // before registration, reverse cleanup and baseline restoration checks.
            // Preview district membership is not an observed instant stair edge.
            var probe=LargeProjectRequest.Parse("/agent-api/v1/large-project-plan",new NameValueCollection{
                {"session",request.Session},{"districtId",request.DistrictId},{"steps",LargeProjectRequest.Encode(package.ToArray())}});
            var report=Plan(probe);
            bool connected=(bool?)report["pilotEligible"]==true&&(bool?)report["restored"]==true;
            landingChecks.Add(to,connected);return connected;
        }
        var directions=new[]{(X:0,Y:-1),(X:-1,Y:0),(X:0,Y:1),(X:1,Y:0)};
        IEnumerable<AccessRouteSearch.Edge> Expand(Cell from,IReadOnlyList<LargeProjectStep> prefix)
        {
            if(searchClock.Elapsed.TotalSeconds>5) yield break;
            if(existingSources.Contains(from))
            {
                if(!existingExits.TryGetValue(from,out var exits))
                {
                    exits=AccessRouteSearch.HeightExits(from,to=> {
                        bool inside=Inside(to);
                        // PathSpec sources belong to the road graph. A general/terrain
                        // edge query must not veto a native road-only stair connection.
                        var origin=new Vector3Int(from.X,from.Y,from.Z);
                        var destination=new Vector3Int(to.X,to.Y,to.Z);
                        bool connected=inside&&navMesh.AreConnectedRoadInstant(origin,destination);
                        bool? generalConnected=inside?navMesh.AreConnectedInstant(origin,destination):null;
                        bool landingCandidate=inside&&stairLandings.TryGetValue(from,out var landing)&&landing.Equals(to);
                        bool? previewLandingConnected=landingCandidate&&!connected?LandingConnected(to):null;
                        exitEvidence[(from,to)]=JObject.FromObject(new{from=Position(from),to=Position(to),
                            insideRegion=inside,nativeConnected=inside?(bool?)connected:null,generalConnected,
                            landingCandidate,previewLandingConnected,
                            candidateAttempts=0,geometryBlockedAttempts=0,unsupportedAttempts=0});
                        return AccessRouteSearch.ConfirmedHeightExit(connected,previewLandingConnected);
                    }).ToArray();
                    existingExits.Add(from,exits);
                }
                foreach(var to in exits)
                {
                    var package=new List<LargeProjectStep>();
                    var road=new LargeProjectStep{Template="Path",X=to.X,Y=to.Y,Z=to.Z};
                    var evidence=exitEvidence[(from,to)];
                    if(!Clear(road,prefix)) { evidence["geometryBlockedAttempts"]=evidence.Value<int>("geometryBlockedAttempts")+1;continue; }
                    if(!Supports(road,package,prefix)) { evidence["unsupportedAttempts"]=evidence.Value<int>("unsupportedAttempts")+1;continue; }
                    evidence["candidateAttempts"]=evidence.Value<int>("candidateAttempts")+1;
                    package.Add(road);
                    yield return new AccessRouteSearch.Edge{To=to,Parts=package.ToArray()};
                }
            }
            for(int rotation=0;rotation<4;rotation++) foreach(int dz in new[]{0,1,-1})
            {
                if(searchClock.Elapsed.TotalSeconds>5)yield break;
                var d=directions[rotation]; int stride=dz==0?1:2;
                var to=new Cell(from.X+d.X*stride,from.Y+d.Y*stride,from.Z+dz);
                if(!Inside(to))continue;
                // A stair main node is not a four-way flat road. Its upper exit
                // is handled above; only its native-confirmed lower landing may
                // enter generic expansion. Never start a new stair at its side.
                bool stairSource=stairLowerExits.TryGetValue(from,out var lower);
                if(stairSource&&!AccessRouteSearch.AllowsGenericStairExit(lower,to,dz))continue;
                var package=new List<LargeProjectStep>();
                if(dz!=0)
                {
                    var stair=new LargeProjectStep{Template="Stairs.Folktails",X=from.X+d.X,Y=from.Y+d.Y,Z=Math.Min(from.Z,to.Z),Rotation=dz>0?rotation:(rotation+2)%4};
                    if(!Clear(stair,prefix.Concat(package))||!Supports(stair,package,prefix))continue;
                    package.Add(stair);
                }
                var road=new LargeProjectStep{Template="Path",X=to.X,Y=to.Y,Z=to.Z};
                if(!Clear(road,prefix.Concat(package))||!Supports(road,package,prefix))continue;
                if(stairSource&&!navMesh.AreConnectedRoadInstant(new Vector3Int(from.X,from.Y,from.Z),new Vector3Int(to.X,to.Y,to.Z))&&
                    LandingConnected(to)!=true)continue;
                package.Add(road);
                yield return new AccessRouteSearch.Edge{To=to,Parts=package.ToArray()};
            }
        }
        bool Route(HashSet<Cell> targets,bool keep,out int[] dependencies)
        {
            dependencies=Array.Empty<int>();
            if(targets.Count==0) {reason="no_access_candidate";return false;}
            if(visited>=4096) {reason="search_budget_exhausted";return false;}
            // Every generated edge moves at most one horizontal cell per part
            // (new stairs: two cells, at least stair+path); existing height exits
            // move one cell and still cost a path. max(horizontal, vertical) is
            // therefore a lower bound, not a straight-line reachability claim.
            var route=AccessRouteSearch.Find(sources.Keys,targets,Expand,32-steps.Count,Math.Min(2048,4096-visited),
                cell=>targets.Min(target=>Distance(cell,target)));
            visited+=route.Visited;
            routeEvidence.Add(new{part=blockedPart,phase,sourceCount=sources.Count,targetCount=targets.Count,
                reason=route.Reason,visitedStates=route.Visited,generatedStates=route.GeneratedStates,
                peakOpenStates=route.PeakOpenStates,closestRemainingCost=route.ClosestRemainingCost,
                selectedSource=route.Found?Position(route.Source):null,pathParts=route.Edges.Sum(edge=>edge.Parts.Length)});
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
            // Native rejection codes derive from ArgumentException; do not relabel them as geometry errors.
            catch(ArgumentException ex) when(ex is not BridgeRejectionException){reason="generated_plan_extent_or_dependency_limit";}
        }
        var sourceSample=sourceEvidence.OrderBy(s=>TargetDistance(s.Cell)).ThenBy(s=>s.Cell.X).ThenBy(s=>s.Cell.Y).ThenBy(s=>s.Cell.Z).Take(32).ToArray();
        var exitSample=exitEvidence.OrderBy(e=>TargetDistance(e.Key.To)).ThenBy(e=>e.Key.From.X).ThenBy(e=>e.Key.From.Y).ThenBy(e=>e.Key.To.Z).Take(64).Select(e=>e.Value).ToArray();
        return new {reservedAreasKey=ReservedArea.Encode(request.ReservedAreas),reservedAreasClear=!steps.Any(Reserved),candidateFound=reason=="candidate",reason,blockedPart,phase,workCellCount,steps=steps.Select(s=>new{template=s.Template,x=s.X,y=s.Y,z=s.Z,rotation=s.Rotation,dependsOn=s.DependsOn}).ToArray(),plan,
            searchDiagnostics=new{version=3,connectionGraph="instant_road_or_registered_landing",landingPreviewCount,landingPreviewLimit=8,
                strategy="a_star_geometry",existingSourceCount=existingSources.Count,
                observedPathCount=sourceEvidence.Count,examinedHeightSources=existingExits.Count,
                heightExitCount=exitEvidence.Count,nativeConnectedExits=exitEvidence.Values.Count(e=>(bool?)e["nativeConnected"]==true),
                sources=sourceSample.Select(s=>new{position=Position(s.Cell),template=s.Template,districtConnected=s.Connected,heightExitsExamined=existingExits.ContainsKey(s.Cell)}).ToArray(),
                sourcesTruncated=sourceEvidence.Count>sourceSample.Length,heightExits=exitSample,heightExitsTruncated=exitEvidence.Count>exitSample.Length,
                routes=routeEvidence.ToArray(),evidence="native_edges_or_registered_landing_not_build_permission"},
            assessment=new{requestedParts=request.Steps.Length,generatedParts=steps.Count-request.Steps.Length,visitedNodes=visited,
                region=new{minX,minY,minZ,maxX,maxY,maxZ},parts=reports.ToArray(),constructionAccess="unproven_until_live_construction",regularExecutionAllowed=false},
            cleanupCandidateIndices=Enumerable.Range(request.Steps.Length,steps.Count-request.Steps.Length).Where(i=>!permanent.Contains(i)).Reverse().ToArray(),
            limitations=new[]{"bounded_geometric_search_not_complete_route_coverage","simple_routes_with_geometry_variants_counted_as_search_states","existing_height_exit_requires_native_edge_or_restored_landing_preview","existing_roads_required_within_twelve_cell_margin","maximum_32_parts_and_eight_occupied_levels","folktails_stairs_and_single_platforms_only","native_road_to_work_cell_not_hypothetical_builder_proof","cleanup_candidates_require_fresh_access_check","never_delete_recovered_resource_stacks","run_returned_steps_and_nested_plan_key_without_replanning"}};
    }
}
