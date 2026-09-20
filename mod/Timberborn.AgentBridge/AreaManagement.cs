using Timberborn.Bridge.Core;
using Timberborn.Forestry;
using Timberborn.Planting;
using Timberborn.PlantingUI;
using Timberborn.TemplateSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using Timberborn.SelectionToolSystem;
using UnityEngine;
namespace Timberborn.AgentBridge;

public sealed class AreaManagement(TreeCuttingArea cutting, PlantingService planting, PlantingAreaValidator validator,
    ITerrainService terrain, TemplateService templates, TemplateNameMapper mapper, ToolUnlockingService unlocking, SelectionToolProcessorFactory processors,
    PlantableDescriber describer, PlantingSelectionService selection, DevModePlantableSpawner devSpawner)
{
    private static object Vec(Vector3Int p)=>new{x=p.x,y=p.y,z=p.z};
    private PlantableSpec[] Plants()=>templates.GetAll<PlantableSpec>().Where(p=>mapper.GetTemplate(p.TemplateName).UsableWithCurrentFeatureToggles).ToArray();
    private static string Kind(PlantableSpec p)=>p.ResourceGroup=="Forester"?"tree_planting":"crops";
    private bool Unlocked(PlantableSpec p)=>!unlocking.IsLocked(new PlantingTool(describer,selection,devSpawner,unlocking,processors,p,p.ResourceGroup));
    public object Types()=>new {
        kinds=new[]{new{kind="tree_cutting",canRead=true,canMark=true,canRemove=true,reason="native_cutting_area"},
            new{kind="crops",canRead=true,canMark=true,canRemove=true,reason="native_planting_designations_not_harvest_zones"},
            new{kind="tree_planting",canRead=true,canMark=true,canRemove=true,reason="native_planting_designations"},
            new{kind="tapping",canRead=false,canMark=false,canRemove=false,reason="no_independent_marking_api_verified_tappers_gather_tappable_resources"}},
        plants=Plants().OrderBy(p=>p.TemplateName).Take(65).Select(p=>new{resource=p.TemplateName,kind=Kind(p),resourceGroup=p.ResourceGroup,unlocked=Unlocked(p)}).ToArray(),
        limitations=new[]{"tapping_uses_gathering_not_verified_as_a_markable_area","planting_not_instant_growth_or_harvest","plant_tools_checked_without_entering_tool_or_unlocking"}};
    private string State(Vector3Int p,string kind)=>kind=="tree_cutting"?(cutting.IsInCuttingArea(p)?"marked":"unmarked"):(planting.IsResourceAt(p)?planting.GetResourceAt(p):"unmarked");
    public object Areas(ManagementRequest r)
    {
        if(r.Kind=="tapping")return new{kind=r.Kind,supported=false,offset=r.Offset,limit=r.Limit,total=(int?)null,items=Array.Empty<object>(),hasMore=false,limitations=new[]{"no_independent_tapping_area_api_verified"}};
        var catalog=Plants().ToDictionary(p=>p.TemplateName,p=>Kind(p));
        var coordinates=(r.Kind=="tree_cutting"?cutting.CuttingArea:planting.PlantingCoordinates.Where(p=>catalog.TryGetValue(planting.GetResourceAt(p),out var k)&&k==r.Kind))
            .OrderBy(p=>p.z).ThenBy(p=>p.y).ThenBy(p=>p.x).ToArray();
        var items=coordinates.Skip(r.Offset).Take(r.Limit).Select(p=>new{position=Vec(p),resource=State(p,r.Kind)}).ToArray();
        return new{kind=r.Kind,supported=true,offset=r.Offset,limit=r.Limit,total=(int?)coordinates.Length,items,hasMore=r.Offset+items.Length<coordinates.Length,
            limitations=new[]{"cells_not_named_rectangular_zones","pages_are_fresh_observations","planting_removal_does_not_remove_plants","no_reachability_or_yield_guarantee"}};
    }
    public object Set(ManagementRequest r)
    {
        var cells=Enumerable.Range(0,r.Width).SelectMany(x=>Enumerable.Range(0,r.Height).Select(y=>new Vector3Int(r.X+x,r.Y+y,r.Z))).ToArray();
        if(cells.Any(p=>!terrain.Contains(p)))throw new ArgumentException("outside_map");
        if(cells.Any(p=>State(p,r.Kind)!=r.ExpectedResource))throw new ArgumentException("area_changed_or_mixed");
        if(r.Kind!="tree_cutting"){
            var all=Plants();
            if(r.ExpectedResource!="unmarked"&&!all.Any(p=>p.TemplateName==r.ExpectedResource&&Kind(p)==r.Kind))throw new ArgumentException("wrong_existing_area_kind");
            if(r.Operation=="mark"){
                var plant=all.SingleOrDefault(p=>p.TemplateName==r.Resource&&Kind(p)==r.Kind);
                if(plant is null||!Unlocked(plant))throw new ArgumentException("plant_unavailable");
                if(cells.Any(p=>!validator.CanPlant(p,r.Resource)))throw new ArgumentException("planting_blocked");
            }
        }else if(r.Operation=="mark"&&cells.Any(p=>terrain.Underground(p)||!terrain.OnGround(p)))throw new ArgumentException("invalid_cutting_ground");
        string expected=r.Operation=="remove"?"unmarked":r.Kind=="tree_cutting"?"marked":r.Resource;
        string outcome="applied";
        try{
            if(r.Kind=="tree_cutting"){if(r.Operation=="mark")cutting.AddCoordinates(cells);else cutting.RemoveCoordinates(cells);}
            else foreach(var p in cells){if(r.Operation=="mark")planting.SetPlantingCoordinates(p,r.Resource);else planting.UnsetPlantingCoordinates(p);}
        }catch{outcome="unconfirmed";}
        var items=cells.Select(p=>new{position=Vec(p),resource=State(p,r.Kind)}).ToArray();
        if(items.Any(i=>i.resource!=expected))outcome="unconfirmed";
        return new{kind=r.Kind,operation=r.Operation,outcome,items,limitations=new[]{"marking_only_not_instant_harvest_or_planting","partial_change_possible_no_retry_or_rollback","read_areas_to_confirm"}};
    }
}
