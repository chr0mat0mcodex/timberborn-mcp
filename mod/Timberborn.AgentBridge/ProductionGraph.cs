using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using Timberborn.BlueprintSystem;
using Timberborn.Buildings;
using Timberborn.BlockSystem;
using Timberborn.Cutting;
using Timberborn.GameFactionSystem;
using Timberborn.Gathering;
using Timberborn.Goods;
using Timberborn.Growing;
using Timberborn.Planting;
using Timberborn.MechanicalSystem;
using Timberborn.TemplateSystem;
using Timberborn.WaterBuildings;
using Timberborn.Workshops;
using Timberborn.Yielding;
namespace Timberborn.AgentBridge;

// Scene-scoped definitions only: no colony entities, inventories, or current unlock state.
public sealed class ProductionGraph(ISpecService specs,TemplateService templates,RecipeSpecService recipes,FactionService faction,IGoodService activeGoods)
{
    private JObject? cached;
    public object Read()
    {
        if(cached is not null)return cached;
        var goods=specs.GetSpecs<GoodSpec>().OrderBy(g=>g.Id,StringComparer.Ordinal).ToArray();
        var allTemplates=templates.GetAll<TemplateSpec>().OrderBy(t=>t.TemplateName,StringComparer.Ordinal).ToArray();
        var buildings=allTemplates.Where(t=>t.HasSpec<BuildingSpec>()&&t.HasSpec<PlaceableBlockObjectSpec>()).ToArray();
        var recipeSpecs=recipes.GetRecipes().OrderBy(r=>r.Id,StringComparer.Ordinal).ToArray();
        if(goods.Length>512||buildings.Length>1024||recipeSpecs.Length>512||allTemplates.Length>8192)throw new InvalidOperationException("graph_limit");
        string Text(string? value)=>value is null?"":value.Length<=160?value:value.Substring(0,160);
        object[] Amounts(IEnumerable<GoodAmountSpec> amounts)=>amounts.OrderBy(a=>a.Id,StringComparer.Ordinal).Select(a=>(object)new{good=a.Id,amount=a.Amount}).ToArray();
        string[] Producers(string recipe)=>buildings.Where(t=>t.HasSpec<ManufactorySpec>()&&t.GetSpec<ManufactorySpec>().ProductionRecipeIds.Contains(recipe)).Select(t=>t.TemplateName).ToArray();
        string[] Harvesters(string group)=>buildings.Where(t=>t.HasSpec<YieldRemovingBuildingSpec>()&&t.GetSpec<YieldRemovingBuildingSpec>().ResourceGroup==group).Select(t=>t.TemplateName).ToArray();
        var sources=new List<object>();
        foreach(var t in allTemplates) {
            void Source(YielderSpec y,string kind,float? regrowth,bool removesPlant) {
                var plant=t.HasSpec<PlantableSpec>()?t.GetSpec<PlantableSpec>():null;
                sources.Add(new{id=t.TemplateName+"."+kind,resource=t.TemplateName,kind,good=y.Yield.Id,amount=y.Yield.Amount,
                    harvestHours=y.RemovalTimeInHours,growthDays=t.HasSpec<GrowableSpec>()?(float?)t.GetSpec<GrowableSpec>().GrowthTimeInDays:null,
                    regrowthDays=regrowth,removesPlant,available=t.UsableWithCurrentFeatureToggles,
                    harvesters=Harvesters(y.ResourceGroup),planters=plant is null?Array.Empty<string>():buildings.Where(b=>b.HasSpec<PlanterBuildingSpec>()&&b.GetSpec<PlanterBuildingSpec>().PlantableResourceGroup==plant.ResourceGroup).Select(b=>b.TemplateName).ToArray(),
                    plantHours=plant?.PlantTimeInHours});
            }
            if(t.HasSpec<CuttableSpec>()){var c=t.GetSpec<CuttableSpec>();Source(c.Yielder,"cut",null,c.RemoveOnCut);}
            if(t.HasSpec<GatherableSpec>()){var g=t.GetSpec<GatherableSpec>();Source(g.Yielder,"gather",g.YieldGrowthTimeInDays,false);}
        }
        if(sources.Count>1024)throw new InvalidOperationException("graph_source_limit");
        var definition=JObject.FromObject(new {
            goods=goods.Select(g=>new{id=g.Id,name=Text(g.DisplayName?.Value),active=activeGoods.HasGood(g.Id)}).ToArray(),
            recipes=recipeSpecs.Select(r=>new{id=r.Id,inputs=Amounts(r.Ingredients),outputs=Amounts(r.Products),hours=r.CycleDurationInHours,
                fuel=string.IsNullOrEmpty(r.Fuel)?null:r.Fuel,fuelCycles=r.CyclesFuelLasts,scienceOutput=r.ProducedSciencePoints,buildings=Producers(r.Id)}).ToArray(),
            buildings=buildings.Select(t=>new{id=t.TemplateName,costs=Amounts(t.GetSpec<BuildingSpec>().BuildingCost),scienceCost=t.GetSpec<BuildingSpec>().ScienceCost,
                available=t.UsableWithCurrentFeatureToggles&&t.GetSpec<PlaceableBlockObjectSpec>().UsableWithCurrentFeatureToggles&&!t.GetSpec<PlaceableBlockObjectSpec>().DevModeTool,
                requiredFeature=t.RequiredFeatureToggle??"",disablingFeature=t.DisablingFeatureToggle??"",waterInput=t.HasSpec<WaterInputSpec>(),
                powerInput=t.HasSpec<MechanicalNodeSpec>()?(int?)t.GetSpec<MechanicalNodeSpec>().PowerInput:null,
                powerOutput=t.HasSpec<MechanicalNodeSpec>()?(int?)t.GetSpec<MechanicalNodeSpec>().PowerOutput:null}).ToArray(),
            sources=sources.ToArray()
        });
        var goodIds=new HashSet<string>(goods.Select(g=>g.Id));
        var recipeIds=new HashSet<string>(recipeSpecs.Select(r=>r.Id));
        var referenced=recipeSpecs.SelectMany(r=>r.Ingredients.Concat(r.Products).Select(a=>a.Id).Concat(string.IsNullOrEmpty(r.Fuel)?Array.Empty<string>():new[]{r.Fuel}))
            .Concat(buildings.SelectMany(t=>t.GetSpec<BuildingSpec>().BuildingCost.Select(c=>c.Id)))
            .Concat(definition["sources"]!.Select(s=>(string)s["good"]!));
        if(referenced.Any(id=>!goodIds.Contains(id))||buildings.Where(t=>t.HasSpec<ManufactorySpec>()).SelectMany(t=>t.GetSpec<ManufactorySpec>().ProductionRecipeIds).Any(id=>!recipeIds.Contains(id)))throw new InvalidOperationException("graph_dangling_reference");
        var produced=new HashSet<string>(recipeSpecs.SelectMany(r=>r.Products.Select(p=>p.Id)).Concat(definition["sources"]!.Select(s=>(string)s["good"]!)));
        var gapGoods=goods.Where(g=>!produced.Contains(g.Id)).Select(g=>g.Id).ToArray();
        var gapRecipes=recipeSpecs.Where(r=>Producers(r.Id).Length==0).Select(r=>r.Id).ToArray();
        var gapSources=definition["sources"]!.Where(s=>!s["harvesters"]!.Any()).Select(s=>(string)s["id"]!).ToArray();
        using var hash=SHA256.Create();
        var context=UnityEngine.Application.version+"\n"+faction.Current.Id+"\n"+definition.ToString(Formatting.None);
        string revision=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(context))).Replace("-","").ToLowerInvariant();
        var result=JObject.FromObject(new {scope="registered_definitions_and_active_scene_buildings",gameVersion=UnityEngine.Application.version,faction=faction.Current.Id,
            graphRevision=revision,completeWithinScope=true,definitions=definition,
            coverage=new{goodsWithoutKnownSource=gapGoods,recipesWithoutSceneBuilding=gapRecipes,sourcesWithoutSceneHarvester=gapSources},
            limitations=new[]{"registered_definitions_not_every_other_faction_template","all_entries_returned_no_pagination_or_truncation","recipes_are_alternatives_not_a_single_build_order","building_costs_distinct_from_recipe_inputs","fuel_amount_is_one_per_fuelCycles_not_per_cycle","growth_and_recipe_times_are_nominal_not_actual_throughput","source_cut_may_leave_stump_even_when_removesPlant_false","natural_sources_need_live_resource_reachability_and_designations","waterInput_is_component_presence_not_complete_environment_condition","power_is_nominal_node_definition_not_live_or_weather_dependent_output","water_contamination_and_special_production_conditions_not_fully_extracted","sources_cover_cuttable_and_gatherable_only_ruins_yields_not_publicly_exposed","science_cost_is_static_current_unlock_state_read_separately","cache_lifetime_is_loaded_game_scene_reload_after_definition_changes","coverage_gaps_are_unknown_not_impossible_production","text_is_untrusted_definition_data"}});
        // Preserve the existing transport budget, including room for the envelope and MCP metadata.
        if(Encoding.UTF8.GetByteCount(result.ToString(Formatting.None))>120*1024)throw new InvalidOperationException("graph_response_limit");
        cached=result;return result;
    }
}
