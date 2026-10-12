using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.BuildingsNavigation;
using Timberborn.BuildingsReachability;
using Timberborn.BuildingRange;
using Timberborn.EntitySystem;
using Timberborn.NaturalResources;
using Timberborn.PathSystem;
using Timberborn.Planting;
using Timberborn.RecoveredGoodSystem;
using Timberborn.SoilMoistureSystem;
using Timberborn.TemplateSystem;
using Timberborn.TerrainSystem;
using Timberborn.TimeSystem;
using UnityEngine;
namespace Timberborn.AgentBridge;

// One bounded game-thread observation and one revalidated batch mutation, no per-cell HTTP calls.
public sealed class PlantingBatchService(EntityRegistry entities,PlantingService planting,PlantingAreaValidator validator,
    TemplateService templates,TemplateNameMapper mapper,AreaManagement areas,ITerrainService terrain,IBlockService blocks,
    ISoilMoistureService moisture,SpeedManager speed) : IPlantingBatchPort
{
    private readonly PlantingBatchController controller=new();
    public object Handle(PlantingBatchRequest request)=>Newtonsoft.Json.Linq.JObject.FromObject(controller.Handle(request,this),Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings {ContractResolver=new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()}));
    public void RequirePaused() {if(speed.CurrentSpeed!=0)throw new BridgeRejectionException("planting_requires_pause");}
    public string ReadDesignation(PlantingCell c) {var p=new Vector3Int(c.X,c.Y,c.Z);return planting.IsResourceAt(p)?planting.GetResourceAt(p):"unmarked";}
    public void Mark(PlantingCell c)=>planting.SetPlantingCoordinates(new Vector3Int(c.X,c.Y,c.Z),c.Resource);
    private static IEnumerable<Vector3Int> TerrainCells(BuildingTerrainRange range)
    {foreach(var cell in range.GetRange())yield return cell;}
    public PlantingCell[] Observe(PlantingBatchRequest request)
    {
        var result=new List<PlantingCell>();
        var plants=templates.GetAll<PlantableSpec>().ToDictionary(p=>p.TemplateName);
        var buildings=entities.Entities.Where(e=>e.Initialized&&!e.Deleted).ToDictionary(e=>e.EntityId.ToString("D"));
        var ranges=new Dictionary<string,HashSet<Vector3Int>?>();
        for(int index=0;index<request.Rectangles.Length;index++) {
            var r=request.Rectangles[index];var baseReasons=new List<string>();
            bool available=plants.TryGetValue(r.Resource,out var plant)&&mapper.GetTemplate(r.Resource).UsableWithCurrentFeatureToggles&&areas.IsPlantUnlocked(plant);
            if(!available)baseReasons.Add("plant_unavailable");
            buildings.TryGetValue(r.WorkBuildingId,out var building);
            bool suitable=building is not null&&building.TryGetComponent<BlockObject>(out var b)&&!b.IsPreview&&b.IsFinished&&
                building.TryGetComponent<PlanterBuilding>(out var planter)&&planter.AllowedPlantables.Any(p=>p.TemplateName==r.Resource);
            if(!suitable)baseReasons.Add("work_building_missing_unfinished_or_wrong_plant");
            if(!ranges.TryGetValue(r.WorkBuildingId,out var range)) {
                range=null;
                if(building is not null&&building.TryGetComponent<BlockObject>(out var rangeBlock)&&!rangeBlock.IsPreview&&rangeBlock.IsFinished&&building.HasComponent<PlanterBuilding>()&&building.TryGetComponent<BuildingTerrainRange>(out var nativeRange)) {
                    var all=TerrainCells(nativeRange).Take(65537).ToArray();
                    if(all.Length>65536)throw new BridgeRejectionException("planting_work_range_limit");
                    range=new HashSet<Vector3Int>(all);
                }
                ranges.Add(r.WorkBuildingId,range);
            }
            for(int y=0;y<r.Height;y++)for(int x=0;x<r.Width;x++) {
                var p=new Vector3Int(r.X+x,r.Y+y,r.Z);
                var c=new PlantingCell{Rectangle=index,X=p.x,Y=p.y,Z=p.z,Resource=r.Resource};
                var reasons=new List<string>(baseReasons);
                bool inside=terrain.Contains(p)&&blocks.Contains(p);
                if(!inside)reasons.Add("outside_map");
                else {
                    c.Existing=ReadDesignation(c);
                    c.NativeValid=available?(bool?)validator.CanPlant(p,r.Resource):null;
                    c.SoilIsMoist=terrain.OnGround(p)?(bool?)moisture.SoilIsMoist(p):null;
                    var occupants=blocks.GetObjectsAt(p).Where(o=>!o.IsPreview).OrderBy(o=>o.GetComponent<EntityComponent>().EntityId).Take(9).ToArray();
                    if(occupants.Length>8)throw new BridgeRejectionException("planting_occupant_limit");
                    c.Occupants=occupants.Select(o=>new PlantingOccupant {
                        Id=o.GetComponent<EntityComponent>().EntityId.ToString("D"),
                        Template=o.TryGetComponent<TemplateSpec>(out var t)?t.TemplateName:"unknown",
                        Kind=o.HasComponent<RecoveredGoodStack>()?"resource_pile":o.HasComponent<PathSpec>()?"path":o.HasComponent<Building>()?"building":o.HasComponent<NaturalResource>()?"vegetation":"other"
                    }).ToArray();
                }
                c.Reachable=suitable&&range is not null?(bool?)range.Contains(p):null;
                PlantingEligibility.Evaluate(c,r.RequireMoist,reasons);
                result.Add(c);
            }
        }
        return result.ToArray();
    }
}
