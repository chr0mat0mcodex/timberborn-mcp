using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.BuildingsNavigation;
using Timberborn.BuildingsReachability;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.InventorySystem;
using Timberborn.Goods;
using Timberborn.Navigation;
using Timberborn.ResourceCountingSystem;
using Timberborn.Stockpiles;
using Timberborn.StockpilePrioritySystem;
using Timberborn.Workshops;
namespace Timberborn.AgentBridge;

// One bounded endpoint inventory for the whole request, never on simulation ticks.
public sealed class BottleneckObservations
{
    private readonly ResourceCountingService resources;
    private readonly EntityComponent[] endpoints;
    private readonly Dictionary<Guid,Inventory[]> inventories;
    private readonly Dictionary<string,(int Available,int All,int Carried)> global=new(StringComparer.Ordinal);
    public BottleneckObservations(EntityRegistry entities, ResourceCountingService resources)
    {
        this.resources=resources;
        endpoints=entities.Entities.Where(e=>e.Initialized&&!e.Deleted&&
            e.TryGetComponent<BlockObject>(out var b)&&b.IsFinished&&!b.IsPreview&&
            e.GetComponentsAllocating<Inventory>().Any(i=>i.Enabled)).OrderBy(e=>e.EntityId).ToArray();
        inventories=endpoints.Take(129).ToDictionary(e=>e.EntityId,e=>e.GetComponentsAllocating<Inventory>().Where(i=>i.Enabled).ToArray());
    }
    public BottleneckEvidence Read(EntityComponent e)
    {
        var result=new BottleneckEvidence();
        string stage="access";
        try {
        if(!e.GetComponent<BlockObject>().IsFinished) {result.State="unfinished";return result;}
        if(!e.TryGetComponent<Manufactory>(out var m)) {result.State="not_manufactory";return result;}
        if(!m.HasCurrentRecipe) {result.State="recipe_missing";return result;}
        result.State="observed";
        if(e.TryGetComponent<DistrictBuildingDistance>(out var district))
            result.DistrictConnected=district.TryGetDistanceToDistrict(out int _);
        if(e.TryGetComponent<BlockableEntranceBuilding>(out var entrance))
            result.EntranceBlocked=entrance.IsEntranceBlocked()||entrance.IsEntranceInaccessible();
        var recipe=m.CurrentRecipe;
        if(recipe.Ingredients.Length+recipe.Products.Length+(recipe.ConsumesFuel?1:0)>32)
            throw new InvalidOperationException("bottleneck_goods_limit");
        var candidates=endpoints.Where(x=>x!=e).ToArray();
        var inspected=candidates.Take(128).ToArray();
        result.CandidateBuildings=candidates.Length;result.InspectedBuildings=inspected.Length;
        result.Complete=candidates.Length<=128;
        stage="local_inventory";
        var own=e.GetComponentsAllocating<Inventory>().Where(i=>i.Enabled).ToArray();
        var paths=new Dictionary<Guid,bool?>();
        var sourceAccess=e.GetComponentsAllocating<Accessible>().Where(x=>x.ValidAccessible).ToArray();
        bool? Path(EntityComponent other) {
            if(paths.TryGetValue(other.EntityId,out var cached))return cached;
            stage="road";
            var a=sourceAccess;
            var b=other.GetComponentsAllocating<Accessible>().Where(x=>x.ValidAccessible).ToArray();
            // Both directions required; supported only for unambiguous native accesses.
            bool? value=a.Length==1&&b.Length==1?
                a[0].FindInstantRoadPath(b[0],out float _)&&b[0].FindInstantRoadPath(a[0],out float _):null;
            paths[other.EntityId]=value;return value;
        }
        GoodFlowEvidence Flow(string good,int amount,string role) {
            bool output=role=="product";
            stage="global_stock";
            if(!global.TryGetValue(good,out var stock)) {
                var count=resources.GetGlobalResourceCount(good);
                stock=(count.AvailableStock,count.AllStock,count.CarriedToProcessors);global[good]=stock;
            }
            stage="local_inventory";
            var local=own.Where(i=>(output?i.IsOutput:i.IsInput)&&
                (i.InputGoods.Contains(good)||i.OutputGoods.Contains(good)||i.Stock.Any(g=>g.GoodId==good))).ToArray();
            var flow=new GoodFlowEvidence {Good=good,Role=role,Amount=amount,
                LocalStock=local.Sum(i=>i.AmountInStock(good)),LocalUnreservedStock=local.Sum(i=>i.UnreservedAmountInStock(good)),
                GlobalAvailable=stock.Available,GlobalAll=stock.All,CarriedToProcessors=stock.Carried};
            var examples=new List<string>();
            foreach(var other in inspected) {
                stage="endpoint_inventory";
                // Products: actual configured stockpiles, not arbitrary nominal capacity.
                if(output&&(!other.HasComponent<Stockpile>()||
                    other.TryGetComponent<StockpilePriority>(out var mode)&&mode.IsEmptyActive))continue;
                if(output&&other.TryGetComponent<SingleGoodAllower>(out var selection)&&
                    (!selection.HasAllowedGood||selection.AllowedGood!=good))continue;
                bool matches=inventories[other.EntityId].Any(i=>InventoryEndpointPolicy.Matches(output,i.IsInput,i.IsOutput,
                    ()=>i.PublicInput&&i.IsUnblocked&&i.Takes(good)&&i.UnreservedCapacity(good)>=amount,
                    ()=>i.PublicOutput&&i.IsUnblocked&&i.Gives(good)&&i.HasUnreservedStock(good)));
                if(!matches)continue;
                flow.MatchingEndpoints++;
                bool? connected=Path(other);
                if(connected==true) {flow.ReachableEndpoints++;if(examples.Count<3)examples.Add(other.EntityId.ToString("D"));}
                else if(connected==false)flow.DisconnectedEndpoints++;
                else flow.UnknownEndpoints++;
            }
            flow.ReachableExamples=examples.ToArray();return flow;
        }
        result.Goods=recipe.Ingredients.Select(g=>Flow(g.Id,g.Amount,"ingredient"))
            .Concat(recipe.ConsumesFuel?new[]{Flow(recipe.Fuel,1,"fuel")}:Array.Empty<GoodFlowEvidence>())
            .Concat(recipe.Products.Select(g=>Flow(g.Id,g.Amount,"product"))).ToArray();
        return result;
        } catch(Exception error) when(error is ArgumentException or InvalidOperationException or KeyNotFoundException or NullReferenceException) {
            return new BottleneckEvidence {State="unavailable",FailureStage=stage};
        }
    }
}
