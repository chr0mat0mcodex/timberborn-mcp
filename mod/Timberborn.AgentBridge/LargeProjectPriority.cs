using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.BuilderPrioritySystem;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    private object ProjectPriority(LargeProjectRequest r)
    {
        if (!jobs.ContainsKey(r.ActionId)) throw new BridgeRejectionException("project_not_found");
        var ids=Ids(r.ActionId); var recorded=Recorded(r.ActionId);
        var states=Enumerable.Range(0,ids.Length).Select(i=>IdentityObservation(r.ActionId,i)).ToArray();
        var priorities=ids.Select(id=>entities.Entities.SingleOrDefault(e=>!e.Deleted && e.Initialized && e.EntityId.ToString("D")==id))
            .Select(e=>e is not null && e.TryGetComponent<BlockObject>(out var b) && b.IsUnfinished &&
                e.TryGetComponent<BuilderPrioritizable>(out var p) ? p : null).ToArray();
        string?[] Read()=>priorities.Select(p=>p?.Priority.ToString()).ToArray();
        string Revision(string?[] values)=>Hash(string.Join("|",ids.Select((id,i)=>id+":"+states[i]+":"+values[i])));
        var before=Read();var revision=Revision(before);
        var outcomes=Enumerable.Repeat("observed",ids.Length).ToArray();
        bool write=r.Operation=="set-priority";
        if(write)
        {
            if(revision!=r.ExpectedRevision) throw new BridgeRejectionException("project_priority_revision_changed");
            if(Enumerable.Range(0,ids.Length).Any(i=>
                !ParallelConstructionPolicy.KnownPart(recorded[i],states[i]) || states[i]=="construction" && priorities[i] is null))
                throw new BridgeRejectionException("project_priority_identity_unavailable");
            // Every identity and compare-and-set token was checked before the first setter.
            outcomes=ProjectPriorityChange.Apply(before,r.Priority,i=>priorities[i]!.Priority.ToString(),
                (i,p)=>priorities[i]!.SetPriority((Timberborn.PrioritySystem.Priority)Enum.Parse(typeof(Timberborn.PrioritySystem.Priority),p)));
        }
        var after=Read();
        return new {actionId=r.ActionId,revision=Revision(after),previousRevision=revision,
            outcome=!write?"observed":outcomes.Any(o=>o=="unconfirmed")?"partial_unconfirmed":"applied",
            items=ids.Select((id,i)=>new {index=i,id,state=states[i],previousPriority=before[i],priority=after[i],outcome=outcomes[i]}).ToArray(),
            limitations=new[]{"construction_priority_not_workplace_priority","priority_not_assignment_or_delivery_guarantee","no_rollback_or_automatic_retry","finished_and_unissued_parts_skipped"}};
    }
}
