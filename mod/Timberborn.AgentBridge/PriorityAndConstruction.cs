using System.Collections.Specialized;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.BuilderPrioritySystem;
using Timberborn.ConstructionSites;
using Timberborn.EntitySystem;
using Timberborn.PrioritySystem;
using Timberborn.TemplateSystem;
using Timberborn.WorkSystem;
namespace Timberborn.AgentBridge;

public sealed class PriorityAndConstruction(EntityRegistry entities, BuildingObservations buildings)
{
    public object Priority(ManagementRequest r)
    {
        var e=entities.Entities.SingleOrDefault(e=>e.EntityId.ToString("D")==r.Id&&e.Initialized&&!e.Deleted);
        if(e is null||!e.TryGetComponent<BlockObject>(out var b)||b.IsPreview)throw new ArgumentException("entity_not_found");
        Func<Timberborn.PrioritySystem.Priority> read; Action<Timberborn.PrioritySystem.Priority> write;
        if(r.Kind=="workplace"&&b.IsFinished&&e.TryGetComponent<WorkplacePriority>(out var wp)){read=()=>wp.Priority;write=wp.SetPriority;}
        else if(r.Kind=="construction"&&b.IsUnfinished&&e.HasComponent<ConstructionSite>()&&e.TryGetComponent<BuilderPrioritizable>(out var cp)){read=()=>cp.Priority;write=cp.SetPriority;}
        else throw new ArgumentException("priority_not_available");
        string previous=read().ToString();string outcome="observed";
        if(r.Route=="set-priority"){
            if(previous!=r.ExpectedPriority)throw new ArgumentException("priority_changed");
            try { write((Timberborn.PrioritySystem.Priority)Enum.Parse(typeof(Timberborn.PrioritySystem.Priority),r.Priority));outcome=read().ToString()==r.Priority?"applied":"unconfirmed"; }
            catch {outcome="unconfirmed";}
        }
        return new {id=e.EntityId,kind=r.Kind,previousPriority=previous,priority=read().ToString(),outcome,
            limitations=new[]{"priority_not_assignment_or_delivery_guarantee","read_back_after_change","no_automatic_retry"}};
    }
    public object Construction(ManagementRequest r)
    {
        var candidates=entities.Entities.Where(e=>e.Initialized&&!e.Deleted&&e.HasComponent<ConstructionSite>()&&
            e.TryGetComponent<BlockObject>(out var b)&&!b.IsPreview&&b.IsUnfinished&&e.HasComponent<TemplateSpec>()).OrderBy(e=>e.EntityId).ToArray();
        var items=candidates.Skip(r.Offset).Take(r.Limit).Select(e=>new {
            building=buildings.Observe(e.EntityId),
            priority=e.TryGetComponent<BuilderPrioritizable>(out var p)?p.Priority.ToString():null }).ToArray();
        return new {offset=r.Offset,limit=r.Limit,total=candidates.Length,items,hasMore=r.Offset+items.Length<candidates.Length,
            limitations=new[]{"all_unfinished_construction_sites_not_only_agent_orders","pages_are_fresh_observations","costs_not_remaining_deliveries"}};
    }
}
