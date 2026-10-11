using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Navigation;
using Timberborn.TemplateSystem;
using Timberborn.TerrainSystem;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
namespace Timberborn.AgentBridge;

// Bounded, observational candidate search; never a replacement for native path validation.
public sealed class BuildingProjectPlanner(EntityRegistry entities, SpatialObservations spatial,
    IBlockService blocks, ITerrainService terrain)
{
    public object Plan(BuildingPlanRequest r)
    {
        var entity=entities.Entities.SingleOrDefault(e=>e.Initialized&&!e.Deleted&&e.EntityId==Guid.Parse(r.DistrictId));
        if(entity is null || !entity.TryGetComponent<DistrictCenter>(out var district) ||
            !entity.TryGetComponent<BlockObject>(out var center) || !center.IsFinished || center.IsPreview)
            throw new ArgumentException("district_unavailable");
        int count=r.Width*r.Height,checkedCandidates=0,rejectedCandidates=0;
        var road=new bool[count];var passable=new bool[count];var goals=new bool[count];
        Vector3Int Cell(int i)=>new(r.X+i%r.Width,r.Y+i/r.Width,r.Z);
        bool Inside(Vector3Int p)=>p.x>=r.X&&p.x<r.X+r.Width&&p.y>=r.Y&&p.y<r.Y+r.Height;
        JObject Precheck(string template,Vector3Int p,int rotation) => JObject.FromObject(spatial.Precheck(
            BridgeRequest.Parse("/agent-api/v1/building-precheck",new NameValueCollection {
                ["template"]=template,["x"]=p.x.ToString(),["y"]=p.y.ToString(),["z"]=p.z.ToString(),["rotation"]=rotation.ToString() })));
        for(int i=0;i<count;i++) {
            var p=Cell(i);if(!terrain.Contains(p)||!blocks.Contains(p)||!terrain.OnGround(p))continue;
            road[i]=blocks.GetObjectsAt(p).Any(b=>!b.IsPreview&&b.IsFinished&&b.TryGetComponent<TemplateSpec>(out var t)&&t.TemplateName=="Path");
            passable[i]=road[i]||(string?)Precheck("Path",p,0)["assessment"]=="requires_game_validation";
            goals[i]=road[i]&&district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(p));
        }
        var options=new List<object>();
        var rejectionReasons=new Dictionary<string,int>();
        var rejectedSites=new List<object>();
        void Reject(Vector3Int origin,IEnumerable<string> reasons) {
            rejectedCandidates++;var codes=reasons.Distinct().ToArray();
            foreach(var reason in codes) rejectionReasons[reason]=rejectionReasons.TryGetValue(reason,out int n)?n+1:1;
            if(rejectedSites.Count<4)rejectedSites.Add(new{origin=Vec(origin),rotation=r.Rotation,reasons=codes});
        }
        for(int i=0;i<count&&options.Count<4;i++) {
            checkedCandidates++;
            var p=Cell(i);var site=Precheck(r.Template,p,r.Rotation);
            if((string?)site["assessment"]!="requires_game_validation" || site["entrance"] is not JObject entrance) { Reject(p,site["reasons"]!.Values<string>().Where(v=>v is not null).Select(v=>v!).DefaultIfEmpty("entrance_unavailable"));continue; }
            Vector3Int Pos(JToken v)=>new((int)v["x"]!,(int)v["y"]!,(int)v["z"]!);
            var entry=Pos(entrance);var occupied=site["cells"]!.Select(c=>Pos(c["position"]!)).ToArray();
            if(entry.z!=r.Z||!Inside(entry)||occupied.Any(c=>!Inside(c))) { Reject(p,new[]{"footprint_or_entrance_outside_search"});continue; }
            int start=(entry.y-r.Y)*r.Width+entry.x-r.X;
            // Public BlockObjectSpec currently exposes one entrance; never
            // synthesize doors. Skip over-budget sites before the option limit.
            var footprint=occupied.Where(c=>c.z==r.Z).Select(c=>(c.y-r.Y)*r.Width+c.x-r.X).Distinct().ToArray();
            var route=FlatRoadSearch.FindWithinRoadBudget(r.Width,r.Height,start,footprint,passable,goals,
                road,BuildingProjectPilotPolicy.MaxNewRoads);
            if(route is null) { Reject(p,new[]{"no_ground_route_within_road_budget"});continue; }
            var option=new { origin=Vec(p),rotation=r.Rotation,entrance=Vec(entry),
                connection=Vec(Cell(route[0])),newRoadCells=route.Where(n=>!road[n]).Select(n=>Vec(Cell(n))).ToArray(),
                routeCells=route.Select(n=>Vec(Cell(n))).ToArray(),executable=false,
                reasons=new[]{"joint_game_validation_pending","construction_reachability_unproven","road_protection_incomplete"} };
            var payload=JObject.FromObject(option);
            using(var hash=SHA256.Create())
                payload["planKey"]=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(r.Session+"|"+r.DistrictId+"|"+r.Template+"|"+payload.ToString(Newtonsoft.Json.Formatting.None)))).Replace("-","").ToLowerInvariant();
            options.Add(payload);
        }
        return new {template=r.Template,districtId=r.DistrictId,origin=Vec(new(r.X,r.Y,r.Z)),width=r.Width,height=r.Height,rotation=r.Rotation,
            checkedCandidates,rejectedCandidates,rejectionReasons,rejectedSites=rejectedSites.ToArray(),totalCandidates=count,searchComplete=checkedCandidates==count,
            stopReason=checkedCandidates==count?"area_exhausted":"option_limit",options=options.ToArray(),
            limitations=new[]{"candidate_plan_only_not_executable","one_height_one_rotation_ground_paths_only","route_and_footprint_inside_search_area",
                "public_block_object_spec_single_entrance_only","grid_route_not_native_navigation_proof","new_road_budget_applied_before_option_limit","no_preview_or_build_order","no_persisted_plan_or_execution_token","not_globally_optimal","no_water_or_hazard_safety_verdict"} };
    }
    private static object Vec(Vector3Int p)=>new{x=p.x,y=p.y,z=p.z};
}
