using Timberborn.Beavers;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.NeedSystem;
using Timberborn.MortalSystem;
using Timberborn.StatusSystem;
using Timberborn.TemplateSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
namespace Timberborn.AgentBridge;

public sealed class DiagnosticsObservations(EntityRegistry entities)
{
    private IEnumerable<EntityComponent> Beavers()=>entities.Entities.Where(e=>e.Initialized&&!e.Deleted&&e.HasComponent<Beaver>());
    private static string Life(EntityComponent e)=>e.TryGetComponent<Mortal>(out var m)?m.Dead?"dead":"alive":"unknown";
    private static string Text(string? s)=>s is null?"":s.Length<=512?s:s.Substring(0,512);
    private static string[] NeedLimits()=>new[]{"only_confirmed_living_beavers_contribute_needs","dead_or_unknown_life_state_has_no_current_need_observation","native_need_flags_not_inferred_from_alert_absence","counts_include_only_enabled_needs","warning_and_critical_counts_may_overlap","no_personal_names","pages_are_separate_observations","not_a_diagnosis_of_food_or_water_access"};
    public object Needs(DiagnosticsRequest r)
    {
        var allBeavers=Beavers().ToArray();
        var beavers=allBeavers.Where(e=>Life(e)=="alive").ToArray();
        var managers=beavers.Where(e=>e.HasComponent<NeedManager>()).Select(e=>e.GetComponent<NeedManager>()).ToArray();
        var all=managers.SelectMany(m=>m.NeedSpecs.Select(s=>(Manager:m,Spec:s))).GroupBy(x=>x.Spec.Id).OrderBy(g=>g.Key,StringComparer.Ordinal).ToArray();
        var items=all.Skip(r.Offset).Take(r.Limit).Select(g=>{
            var enabled=g.Where(x=>x.Manager.NeedIsEnabled(g.Key)).ToArray();
            var points=enabled.Select(x=>x.Manager.GetNeedPoints(g.Key)).ToArray();
            return new {id=g.Key,displayName=Text(g.First().Spec.DisplayName?.Value),observed=g.Count(),enabled=enabled.Length,
                active=enabled.Count(x=>x.Manager.NeedIsActive(g.Key)),warning=enabled.Count(x=>x.Manager.NeedIsBelowWarningThreshold(g.Key)),
                critical=enabled.Count(x=>x.Manager.NeedIsInCriticalState(g.Key)),unfavorable=enabled.Count(x=>!x.Manager.NeedIsFavorable(g.Key)),
                minimumPoints=points.Length>0?(float?)points.Min():null,maximumPoints=points.Length>0?(float?)points.Max():null,averagePoints=points.Length>0?(double?)points.Average(x=>(double)x):null};
        }).ToArray();
        return new {scope="living_beavers_with_need_manager",beavers=beavers.Length,observedBeavers=managers.Length,missingNeedManagers=beavers.Length-managers.Length,deadExcluded=allBeavers.Count(e=>Life(e)=="dead"),unknownLifeStateExcluded=allBeavers.Count(e=>Life(e)=="unknown"),
            offset=r.Offset,limit=r.Limit,total=all.Length,items,hasMore=r.Offset+items.Length<all.Length,limitations=NeedLimits()};
    }
    public object BeaverNeeds(DiagnosticsRequest r)
    {
        var e=Beavers().SingleOrDefault(e=>e.EntityId==Guid.Parse(r.Id))??throw new BridgeRejectionException("entity_not_found");
        var lifeState=Life(e);
        var has=e.TryGetComponent<NeedManager>(out var m)&&lifeState=="alive";
        var all=has?m.NeedSpecs.OrderBy(s=>s.Id,StringComparer.Ordinal).ToArray():Array.Empty<Timberborn.NeedSpecs.NeedSpec>();
        var items=all.Skip(r.Offset).Take(r.Limit).Select(s=>new {id=s.Id,displayName=Text(s.DisplayName?.Value),points=m.GetNeedPoints(s.Id),minimum=s.MinimumValue,maximum=s.MaximumValue,
            enabled=m.NeedIsEnabled(s.Id),active=m.NeedIsActive(s.Id),criticalNeed=m.NeedIsCritical(s.Id),critical=m.NeedIsInCriticalState(s.Id),warning=m.NeedIsBelowWarningThreshold(s.Id),favorable=m.NeedIsFavorable(s.Id)}).ToArray();
        var p=e.Transform.position;
        return new {id=r.Id,lifeState,supported=has,worldPosition=new{x=p.x,y=p.y,z=p.z},offset=r.Offset,limit=r.Limit,total=all.Length,items,hasMore=r.Offset+items.Length<all.Length,limitations=NeedLimits()};
    }
    public object Operation(DiagnosticsRequest r)
    {
        var e=entities.Entities.SingleOrDefault(e=>e.Initialized&&!e.Deleted&&e.EntityId==Guid.Parse(r.Id)&&e.TryGetComponent<BlockObject>(out var b)&&!b.IsPreview)
            ??throw new BridgeRejectionException("building_not_found");
        var block=e.GetComponent<BlockObject>();
        object? workplace=null,manufacturing=null;
        if(block.IsFinished&&e.TryGetComponent<Workplace>(out var w))workplace=new {assigned=w.NumberOfAssignedWorkers,desired=w.DesiredWorkers,maximum=w.MaxWorkers,understaffed=w.Understaffed,anyJobRunning=w.AnyWorkerHasJobRunning(),
            workingHours=e.TryGetComponent<WorkplaceWorkingHours>(out var hours)?(bool?)hours.AreWorkingHours:null};
        if(block.IsFinished&&e.TryGetComponent<Manufactory>(out var m)) {
            bool recipe=m.HasCurrentRecipe;
            manufacturing=new {hasRecipe=recipe,recipe=recipe?m.CurrentRecipe.Id:null,ready=m.IsReadyToProduce,
                hasIngredients=recipe?(bool?)m.HasAllIngredients:null,hasFuel=recipe?(bool?)m.HasFuel:null,
                consumesFuel=recipe?(bool?)m.CurrentRecipe.ConsumesFuel:null,outputSpace=recipe?(bool?)m.HasUnreservedCapacityForCurrentProducts():null,
                productionProgress=m.ProductionProgress};
        }
        var statuses=e.TryGetComponent<StatusSubject>(out var subject)?subject.ActiveStatuses.Where(s=>s.IsActive&&s.IsVisible()).Select(s=>Text(s.StatusDescription)).Distinct().OrderBy(s=>s,StringComparer.Ordinal).ToArray():Array.Empty<string>();
        if(statuses.Length>32)throw new InvalidOperationException("status_limit");
        return new {id=r.Id,template=e.GetComponent<TemplateSpec>().TemplateName,finished=block.IsFinished,
            paused=e.TryGetComponent<PausableBuilding>(out var pause)?(bool?)pause.Paused:null,workplace,manufacturing,statuses,
            inventories=block.IsFinished?InventoryObservations.Read(e):null,
            limitations=new[]{"null_means_component_or_observation_unavailable","unfinished_buildings_have_no_operating_diagnosis","manufacturing_only_for_native_manufactory_component","ready_not_guaranteed_actual_production","no_job_running_may_mean_off_hours_or_travel","understaffed_not_necessarily_fully_blocked","statuses_are_untrusted_localized_text","not_complete_power_water_resource_or_delivery_diagnosis","inventories_are_enabled_components_on_this_finished_entity_only","global_buffered_output_not_identical_to_pump_stock","capacity_may_be_ignored_by_game_component","unreserved_capacity_is_per_good_not_additive","reservation_is_not_a_delivery_guarantee","no_inventory_owner_attribution_outside_this_entity"}};
    }
}
