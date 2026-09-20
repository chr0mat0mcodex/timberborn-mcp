using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.Demolishing;
using Timberborn.EntitySystem;
using Timberborn.NaturalResources;
using Timberborn.Planting;
using Timberborn.Ruins;
using Timberborn.RecoveredGoodSystem;
using Timberborn.TemplateSystem;
using UnityEngine;
namespace Timberborn.AgentBridge;

public sealed class RemovalManagement(EntityRegistry entities, EntityService service, PlantingService planting)
{
    private static object Vec(Vector3Int p)=>new{x=p.x,y=p.y,z=p.z};
    private static bool Candidate(EntityComponent e)=>e.Initialized&&!e.Deleted&&e.TryGetComponent<BlockObject>(out var b)&&!b.IsPreview;
    private string Kind(EntityComponent e)
    {
        if(e.HasComponent<RecoveredGoodStack>())return "debris";
        if(e.HasComponent<Building>() || e.TryGetComponent<TemplateSpec>(out var t)&&t.TemplateName=="Path")return "buildings";
        if(e.HasComponent<Ruin>())return "other";
        if(e.HasComponent<NaturalResource>()&&e.TryGetComponent<TemplateSpec>(out var p)) {
            var pos=e.GetComponent<BlockObject>().Coordinates;
            return planting.IsResourceAt(pos)&&planting.GetResourceAt(pos)==p.TemplateName?"planted":"vegetation";
        }
        return "other";
    }
    public object Targets(RemovalRequest r)
    {
        bool Inside(Vector3Int p)=>p.x>=r.X&&p.x<r.X+r.Width&&p.y>=r.Y&&p.y<r.Y+r.Height&&p.z>=r.Z&&p.z<r.Z+r.Depth;
        var all=entities.Entities.Where(Candidate).Where(e=>(r.Kind=="all"||Kind(e)==r.Kind)&&e.GetComponent<BlockObject>().PositionedBlocks.GetOccupiedCoordinates().Any(Inside)).OrderBy(e=>e.EntityId).ToArray();
        var items=all.Skip(r.Offset).Take(r.Limit).Select(e=>{
            var b=e.GetComponent<BlockObject>();var kind=Kind(e);bool demo=e.TryGetComponent<Demolishable>(out var d);
            string mode=kind is "buildings" or "debris"?"delete":kind is "planted" or "vegetation"&&demo?"demolition_mark":"unsupported";
            return new{id=e.EntityId,kind,template=e.TryGetComponent<TemplateSpec>(out var t)?t.TemplateName:"unknown",position=Vec(b.Coordinates),mode,canDelete=b.CanDelete(),marked=demo&&d.IsMarked,plantOrigin="unknown",classification=kind=="planted"?"matching_current_planting_designation":"current_components"};
        }).ToArray();
        return new{kind=r.Kind,offset=r.Offset,limit=r.Limit,total=all.Length,items,hasMore=r.Offset+items.Length<all.Length,limitations=new[]{"objects_overlapping_region_not_full_build_validation","planted_means_matching_current_designation_not_historical_origin","other_objects_not_removable_by_this_tool","pages_are_fresh_observations"}};
    }
    public object Remove(RemovalRequest r)
    {
        var e=entities.Entities.SingleOrDefault(e=>Candidate(e)&&e.EntityId.ToString("D")==r.Id);
        if(e is null||Kind(e)!=r.Kind||!e.TryGetComponent<TemplateSpec>(out var t)||t.TemplateName!=r.Template)throw new ArgumentException("target_changed");
        var b=e.GetComponent<BlockObject>();var p=b.Coordinates;
        if(p!=new Vector3Int(r.X,r.Y,r.Z))throw new ArgumentException("position_changed");
        bool demo=e.TryGetComponent<Demolishable>(out var d);
        if((demo&&d.IsMarked)!=r.ExpectedMarked)throw new ArgumentException("demolition_changed");
        if(r.Operation!="unmark"&&!b.CanDelete())throw new ArgumentException("deletion_blocked");
        if(r.Operation!="delete"&&!demo)throw new ArgumentException("demolition_unavailable");
        string outcome="unconfirmed";
        try {
            if(r.Kind=="debris")e.GetComponent<RecoveredGoodStack>().Delete();
            else if(r.Kind=="buildings")service.Delete(e);
            else if(r.Operation=="mark")d.Mark();else d.Unmark();
            bool gone=e.Deleted||!entities.Entities.Any(x=>x.EntityId==e.EntityId);
            // The regular Mark() can immediately remove an eligible resource; this is a completed action too.
            if(r.Operation=="delete"?gone:r.Operation=="mark"?gone||d.IsMarked:!gone&&!d.IsMarked)outcome="applied";
        } catch { /* Lifecycle may have partially run. Never repeat automatically. */ }
        bool removed=e.Deleted||!entities.Entities.Any(x=>x.EntityId==e.EntityId);
        return new{id=Guid.Parse(r.Id),kind=r.Kind,template=r.Template,position=Vec(p),operation=r.Operation,outcome,removed,marked=removed?(bool?)null:demo?d.IsMarked:false,
            limitations=new[]{"delete_is_irreversible_and_may_lose_goods","mark_is_worker_order_not_completed_removal","planting_designation_retained_may_regrow","no_automatic_retry_or_rollback","dependent_objects_not_deleted_as_batch"}};
    }
}
