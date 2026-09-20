using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.BuildingRange;
using Timberborn.BuildingsReachability;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.GoodsSampling;
using Timberborn.Navigation;
namespace Timberborn.AgentBridge;

public sealed class LogisticsObservations(EntityRegistry entities,IGoodService goods,GlobalGoodSamplingRegistry samples)
{
    private EntityComponent Find(string id)=>entities.Entities.SingleOrDefault(e=>e.Initialized&&!e.Deleted&&e.EntityId==Guid.Parse(id)&&e.TryGetComponent<BlockObject>(out var b)&&!b.IsPreview)
        ??throw new BridgeRejectionException("building_not_found");
    private static object Vec(UnityEngine.Vector3Int p)=>new{x=p.x,y=p.y,z=p.z};
    public object Access(LogisticsRequest r)
    {
        var e=Find(r.Id);var b=e.GetComponent<BlockObject>();
        int? distance=null;
        if(e.TryGetComponent<DistrictBuildingDistance>(out var d)&&d.TryGetDistanceToDistrict(out int value))distance=value;
        bool? entranceBlocked=null,entranceInaccessible=null,unconnectedBlocked=null,buildersReachable=null;
        if(e.TryGetComponent<BlockableEntranceBuilding>(out var entrance)){entranceBlocked=entrance.IsEntranceBlocked();entranceInaccessible=entrance.IsEntranceInaccessible();}
        var blockers=e.AllComponents.OfType<IUnconnectedBuildingBlocker>().ToArray();
        if(blockers.Length>0)unconnectedBlocked=blockers.Any(b=>b.IsUnconnectedBlocked);
        if(b.IsUnfinished&&e.TryGetComponent<ReachableConstructionSite>(out var site))buildersReachable=site.IsReachableByBuilders();
        var access=e.GetComponentsAllocating<Accessible>();
        return new {id=r.Id,finished=b.IsFinished,position=Vec(b.Coordinates),entranceBlocked,entranceInaccessible,unconnectedBlocked,buildersReachable,distanceToDistrict=distance,
            accessibleCount=access.Count,validAccessibleCount=access.Count(a=>a.ValidAccessible),
            limitations=new[]{"null_means_component_or_value_unavailable","game_navigation_may_update_after_ticks","distance_is_native_metric_not_travel_time","connection_not_staffing_or_delivery_guarantee"}};
    }
    public object Road(LogisticsRequest r)
    {
        var from=Find(r.Id);var to=Find(r.ToId);
        var a=from.GetComponentsAllocating<Accessible>().Where(a=>a.ValidAccessible).ToArray();
        var b=to.GetComponentsAllocating<Accessible>().Where(a=>a.ValidAccessible).ToArray();
        bool supported=from.GetComponent<BlockObject>().IsFinished&&to.GetComponent<BlockObject>().IsFinished&&a.Length==1&&b.Length==1;
        bool? connected=null;float? distance=null;
        if(supported){connected=a[0].FindInstantRoadPath(b[0],out float value);if(connected==true)distance=value;}
        return new {id=r.Id,toId=r.ToId,supported,sourceAccessCount=a.Length,targetAccessCount=b.Length,connected,distance,
            limitations=new[]{"instant_game_road_path_query","requires_finished_objects_and_one_valid_accessible_each","direction_is_source_to_target","no_terrain_shortcuts","native_distance_not_travel_time","navigation_may_lag","no_worker_or_delivery_guarantee"}};
    }
    public object Range(LogisticsRequest r)
    {
        var e=Find(r.Id);// Interface implementations need not be registered as component lookup keys.
        var providers=e.AllComponents.OfType<IBuildingWithRange>().ToArray();
        bool supported=e.GetComponent<BlockObject>().IsFinished&&providers.Length>0;
        // Bound enumeration as well as response size; don't silently claim a partial union is complete.
        var cells=supported?providers.SelectMany(p=>p.GetBlocksInRange()).Take(65537).ToArray():Array.Empty<UnityEngine.Vector3Int>();
        if(cells.Length>65536)throw new InvalidOperationException("range_limit");
        var all=cells.Distinct().OrderBy(p=>p.z).ThenBy(p=>p.y).ThenBy(p=>p.x).ToArray();
        var items=all.Skip(r.Offset).Take(r.Limit).Select(Vec).ToArray();
        var names=providers.Select(p=>p.RangeName??"").Distinct().Take(17).ToArray();
        if(names.Length>16||names.Any(n=>n.Length>160))throw new InvalidOperationException("range_names_limit");
        return new{id=r.Id,supported,rangeNames=names,offset=r.Offset,limit=r.Limit,total=all.Length,items,hasMore=r.Offset+items.Length<all.Length,
            limitations=new[]{"union_of_public_building_range_providers","range_not_selected_job_or_harvest_eligibility","no_provider_is_unknown_not_zero_work_range","pages_are_separate_observations","game_navigation_may_lag"}};
    }
    public object History(LogisticsRequest r)
    {
        bool available=goods.HasGood(r.Good);
        var history=available?samples.GoodSamplingRegistry.GetGoodSampleHistory(r.Good):null;
        var all=history?.GoodSamples.ToArray()??Array.Empty<GoodSample>();
        var page=all.Skip(r.Offset).Take(r.Limit).ToArray();
        var items=page.Select((s,i)=>new{index=r.Offset+i,cycle=s.Cycle,day=s.Day,stock=s.Stock,capacity=s.Capacity,production=s.Production,consumption=s.Consumption,netProduction=(long)s.Production-s.Consumption}).ToArray();
        return new{good=r.Good,available,offset=r.Offset,limit=r.Limit,total=all.Length,items,hasMore=r.Offset+items.Length<all.Length,
            pageProduction=page.Sum(s=>(long)s.Production),pageConsumption=page.Sum(s=>(long)s.Consumption),stockChange=page.Length>1?(long?)page.Last().Stock-page.First().Stock:null,
            limitations=new[]{"native_saved_global_good_samples_in_game_order","sample_bucket_duration_not_assumed","production_consumption_are_game_counters_not_stock_deltas","net_production_not_guaranteed_stock_change","page_sums_only_not_full_history_totals","history_empty_not_zero_activity","pages_are_separate_observations","not_instantaneous_rates"}};
    }
}
