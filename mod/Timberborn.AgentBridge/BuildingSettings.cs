using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.Fields;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Planting;
using Timberborn.StockpilePrioritySystem;
using Timberborn.Stockpiles;
using Timberborn.TemplateSystem;

namespace Timberborn.AgentBridge;

public sealed class BuildingSettings(EntityRegistry entities, IGoodService goods, AreaManagement areas)
{
    private EntityComponent Find(BuildingSettingsRequest r)
    {
        var e=entities.Entities.SingleOrDefault(e=>e.EntityId==Guid.Parse(r.Id) && e.Initialized && !e.Deleted);
        if(e is null || !e.HasComponent<Building>() || !e.TryGetComponent<BlockObject>(out var block) || block.IsPreview)
            throw new BridgeRejectionException("building_not_found");
        return e;
    }
    public object Read(BuildingSettingsRequest r) => Observe(Find(r));
    private string[] AllowedGoods(Stockpile s)
    {
        var result=goods.GetGoodsForType(s.WhitelistedGoodType).OrderBy(g=>g,StringComparer.Ordinal).ToArray();
        if(result.Length>128) throw new InvalidOperationException("goods_limit");
        return result;
    }
    private static string Mode(StockpilePriority p)
    {
        var modes=new[] { (p.IsAcceptActive,"accept"),(p.IsEmptyActive,"empty"),(p.IsObtainActive,"obtain"),(p.IsSupplyActive,"supply") }
            .Where(m=>m.Item1).Select(m=>m.Item2).ToArray();
        if(modes.Length!=1) throw new InvalidOperationException("ambiguous_storage_mode");
        return modes[0];
    }
    private static string Good(SingleGoodAllower a) => a.HasAllowedGood ? a.AllowedGood : "";
    private static string Crop(PlantablePrioritizer p) => p.PrioritizedPlantableSpec?.TemplateName ?? "";
    private static string FarmPriority(FarmHouse f) => f.PlantingPrioritized ? "planting" : "harvesting";

    private object Observe(EntityComponent e)
    {
        var b=e.GetComponent<BlockObject>(); var t=e.GetComponent<TemplateSpec>();
        object? storage=null, farm=null, pause=null;
        if(e.TryGetComponent<PausableBuilding>(out var p)) pause=new { paused=p.Paused, canPause=p.IsPausable() };
        bool hasStorage=e.TryGetComponent<Stockpile>(out var s), hasFarm=e.TryGetComponent<FarmHouse>(out var f);
        if(hasStorage && b.IsFinished) {
            bool hasSelection=e.TryGetComponent<SingleGoodAllower>(out var a);
            bool hasMode=e.TryGetComponent<StockpilePriority>(out var mode);
            var inv=s.Inventory;
            var stock=inv.Stock.OrderBy(g=>g.GoodId,StringComparer.Ordinal).Select(g=>new { id=g.GoodId, amount=g.Amount }).ToArray();
            if(stock.Length>128) throw new InvalidOperationException("stock_limit");
            storage=new { selectedGood=hasSelection?Good(a):null, canChangeGood=hasSelection && !e.HasComponent<FixedStockpile>(),
                allowedGoods=AllowedGoods(s), mode=hasMode?Mode(mode):null, canChangeMode=hasMode,
                capacity=inv.Capacity, stock, hasUnwantedStock=inv.HasUnwantedStock };
        }
        if(hasFarm && b.IsFinished) {
            bool hasPlants=e.TryGetComponent<PlanterBuilding>(out var planter);
            bool hasPreference=e.TryGetComponent<PlantablePrioritizer>(out var pref);
            var plants=hasPlants ? planter.AllowedPlantables.OrderBy(p=>p.TemplateName,StringComparer.Ordinal)
                .Select(p=>new { resource=p.TemplateName, unlocked=areas.IsPlantUnlocked(p) }).ToArray() : null;
            if(plants?.Length>64) throw new InvalidOperationException("plant_limit");
            farm=new { priority=FarmPriority(f), prioritizedResource=hasPreference?Crop(pref):null,
                canChangeCrop=hasPlants && hasPreference, canClearCropPriority=false, allowedPlants=plants };
        }
        return new { id=e.EntityId, template=t.TemplateName, position=new { x=b.Coordinates.x,y=b.Coordinates.y,z=b.Coordinates.z },
            finished=b.IsFinished, pause, storageComponentPresent=hasStorage, storage, farmComponentPresent=hasFarm, farm,
            limitations=new[] { "storage_and_farm_settings_only_for_finished_buildings", "area_designations_are_global_not_assigned_to_farm",
                "crop_preference_uses_plantable_prioritizer_not_forced_job_assignment", "no_farm_reachability_or_output_guarantee",
                "crop_priority_clear_not_exposed", "stock_may_include_previously_selected_goods" } };
    }

    public object Set(BuildingSettingsRequest r)
    {
        var e=Find(r); var b=e.GetComponent<BlockObject>();
        Func<string> read; Action set;
        if(r.Route=="set-building-paused") {
            if(!e.TryGetComponent<PausableBuilding>(out var p) || !p.IsPausable()) throw new BridgeRejectionException("not_pausable");
            read=()=>p.Paused?"true":"false";
            set=()=>{ if(r.Value=="true") p.Pause(); else p.Resume(); };
        } else {
            if(!b.IsFinished) throw new BridgeRejectionException("finished_building_required");
            switch(r.Route) {
                case "set-storage-good":
                    if(!e.TryGetComponent<Stockpile>(out var s) || !e.TryGetComponent<SingleGoodAllower>(out var a) ||
                        e.HasComponent<FixedStockpile>() || (r.Value!="" && !AllowedGoods(s).Contains(r.Value)))
                        throw new BridgeRejectionException("unsupported_storage_good");
                    read=()=>Good(a); set=()=>{ if(r.Value=="") a.Disallow(); else a.Allow(r.Value); }; break;
                case "set-storage-mode":
                    if(!e.HasComponent<Stockpile>() || !e.TryGetComponent<StockpilePriority>(out var mode)) throw new BridgeRejectionException("not_storage");
                    read=()=>Mode(mode); set=()=>{ switch(r.Value) { case "accept":mode.Accept();break;case "empty":mode.Empty();break;
                        case "obtain":mode.Obtain();break;case "supply":mode.Supply();break;default:throw new ArgumentException(); } }; break;
                case "set-farm-priority":
                    if(!e.TryGetComponent<FarmHouse>(out var f)) throw new BridgeRejectionException("not_farm");
                    read=()=>FarmPriority(f); set=()=>{ if(r.Value=="planting") f.PrioritizePlanting(); else f.UnprioritizePlanting(); }; break;
                case "set-farm-crop":
                    if(!e.HasComponent<FarmHouse>() || !e.TryGetComponent<PlanterBuilding>(out var planter) ||
                        !e.TryGetComponent<PlantablePrioritizer>(out var pref)) throw new BridgeRejectionException("not_crop_prioritizer");
                    var plant=planter.AllowedPlantables.SingleOrDefault(p=>p.TemplateName==r.Value);
                    if(plant is null || !areas.IsPlantUnlocked(plant)) throw new BridgeRejectionException("unavailable_crop");
                    read=()=>Crop(pref); set=()=>pref.PrioritizePlantable(plant); break;
                default: throw new BridgeRejectionException("unsupported_setting");
            }
        }
        var result=SettingChange.Execute(r.ExpectedValue,r.Value,read,set);
        return new { id=e.EntityId, template=e.GetComponent<TemplateSpec>().TemplateName, setting=BuildingSettingsRequest.ValueKey(r.Route),
            previousValue=result.Previous, requestedValue=result.Requested, observedValue=result.Observed, outcome=result.Outcome,
            observation=Observe(e), limitations=new[] { "single_setting_change", "no_stock_created_or_removed_by_bridge",
                "game_updates_may_be_deferred", "read_settings_to_confirm", "no_automatic_retry_or_rollback" } };
    }
}
