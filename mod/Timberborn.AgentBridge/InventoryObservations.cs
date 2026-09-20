using Timberborn.EntitySystem;
using Timberborn.InventorySystem;
namespace Timberborn.AgentBridge;

public static class InventoryObservations
{
    public static object[] Read(EntityComponent entity)
    {
        var inventories=entity.GetComponentsAllocating<Inventory>().Where(i=>i.Enabled).OrderBy(i=>i.ComponentName,StringComparer.Ordinal).ToArray();
        if(inventories.Length>8)throw new InvalidOperationException("inventory_limit");
        return inventories.Select(i=>{
            var roles=new List<string>();
            foreach(var id in i.InputGoods)roles.Add(id);
            foreach(var id in i.OutputGoods)roles.Add(id);
            var ids=i.Stock.Select(g=>g.GoodId).Concat(roles)
                .Concat(i.ReservedCapacity().Select(g=>g.GoodId)).Distinct().OrderBy(id=>id,StringComparer.Ordinal).ToArray();
            if(ids.Length>64||i.ComponentName.Length>160)throw new InvalidOperationException("inventory_goods_limit");
            return (object)new {component=i.ComponentName,capacity=i.Capacity,totalStock=i.TotalAmountInStock,
                isInput=i.IsInput,isOutput=i.IsOutput,publicInput=i.PublicInput,publicOutput=i.PublicOutput,
                full=i.IsFull,fullyReserved=i.IsFullyReserved,unblocked=i.IsUnblocked,
                goods=ids.Select(id=>new {id,stock=i.AmountInStock(id),unreservedStock=i.UnreservedAmountInStock(id),
                    reservedCapacity=i.ReservedCapacity(id),unreservedCapacity=i.UnreservedCapacity(id)}).ToArray()};
        }).ToArray();
    }
}
