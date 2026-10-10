using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.BuildingsReachability;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.MechanicalSystem;
using Timberborn.Navigation;
using Timberborn.PathSystem;
using Timberborn.Goods;
using Timberborn.ResourceCountingSystem;
using Timberborn.ScienceSystem;
using Timberborn.TemplateSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

// Explicit 3D blueprints, public previews, normal placement services. No save editing.
public sealed class LargeProjectService(BuildingCatalog catalog, PreviewFactory factory, BlockObjectValidationService validators,
    EntityRegistry entities, IBlockService blocks, SpeedManager speed, RoadProtection roads, SiteValidation validation,
    BlockObjectPlacerService placers, BuildingUnlockingService unlocks, PowerObservations power,
    IGoodService goods, ResourceCountingService resources)
{
    private readonly LargeProjectLedger ledger=new();
    private readonly Dictionary<string,(LargeProjectRequest Request,RoadProtection.Sample Baseline)> jobs=new();
    private readonly Dictionary<string,Preview> previews=new();
    private readonly System.Diagnostics.Stopwatch clock=System.Diagnostics.Stopwatch.StartNew();
    private bool faulted;
    private int[] Stocks()=>goods.Goods.OrderBy(id=>id,StringComparer.Ordinal).Select(id=>resources.GetGlobalResourceCount(id).AllStock).ToArray();
    private static Orientation Rotation(int n)=>new[]{Orientation.Cw0,Orientation.Cw90,Orientation.Cw180,Orientation.Cw270}[n];
    private static Placement Placement(LargeProjectStep s)=>new(new(s.X,s.Y,s.Z),Rotation(s.Rotation),FlipMode.Unflipped);
    private static string Hash(string value) {using var hash=SHA256.Create();return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-","").ToLowerInvariant();}
    private static string Fingerprint(LargeProjectRequest r)=>r.Session+"|"+r.DistrictId+"|"+LargeProjectRequest.Encode(r.Steps);
    private static string EntityId(string action,int index) {using var hash=SHA256.Create();return new Guid(hash.ComputeHash(Encoding.UTF8.GetBytes(action+"|large-project|"+index)).Take(16).ToArray()).ToString("D");}
    private DistrictCenter District(string id)
    {
        var e=entities.Entities.SingleOrDefault(e=>!e.Deleted&&e.Initialized&&e.EntityId.ToString("D")==id);
        if(e is null || !e.TryGetComponent<DistrictCenter>(out var district) || !e.TryGetComponent<BlockObject>(out var b) || !b.IsFinished || b.IsPreview) throw new BridgeRejectionException("state_conflict");
        return district;
    }
    private TemplateSpec Resolve(LargeProjectStep step)
    {
        var template=catalog.Resolve(step.Template);
        // A new district has its own lifecycle and is not part of an existing-district blueprint.
        if(step.Template.StartsWith("DistrictCenter.",StringComparison.Ordinal)) throw new ArgumentException("district_lifecycle_not_supported");
        if(!template.UsableWithCurrentFeatureToggles || !template.GetSpec<PlaceableBlockObjectSpec>().UsableWithCurrentFeatureToggles) throw new BridgeRejectionException("template_disabled");
        if(!unlocks.Unlocked(template.GetSpec<BuildingSpec>())) throw new BridgeRejectionException("template_locked");
        return template;
    }
    public object Handle(LargeProjectRequest r)
    {
        if(r.Operation=="power-network") return power.Read(r.EntityId);
        if(r.Operation=="plan") return Plan(r);
        if(r.Operation=="inspect") return Receipt(r.ActionId);
        if(r.Operation=="stop") {ledger.Stop(r.ActionId);return Receipt(r.ActionId);}
        if(r.Operation=="start")
        {
            if(ledger.Existing(r.ActionId,Fingerprint(r)) is not null) return Receipt(r.ActionId);
            var plan=Plan(r);
            if((string?)plan["planKey"]!=r.PlanKey || (bool?)plan["pilotEligible"]!=true) throw new BridgeRejectionException("state_conflict");
            var baseline=roads.Capture();if(!baseline.Complete)throw new BridgeRejectionException("state_conflict");
            var ids=Enumerable.Range(0,r.Steps.Length).Select(i=>EntityId(r.ActionId,i)).ToArray();
            if(entities.Entities.Any(e=>ids.Contains(e.EntityId.ToString("D"))))throw new BridgeRejectionException("state_conflict");
            ledger.Start(r.ActionId,r.PlanKey,Fingerprint(r),LargeProjectRequest.Order(r.Steps),ids);
            jobs.Add(r.ActionId,(r,baseline));
            return Receipt(r.ActionId); // start records a plan; advance performs at most one order.
        }
        var job=jobs.TryGetValue(r.ActionId,out var stored)?stored:throw new BridgeRejectionException("state_conflict");
        if(speed.CurrentSpeed!=0)throw new BridgeRejectionException("state_conflict");
        var receipt=ledger.Inspect(r.ActionId);var district=District(job.Request.DistrictId);
        bool Guard()=>!faulted && ConstructionIsolation.Allows(entities,receipt.EntityIds.Where((_,i)=>receipt.PartStates[i]!="pending")) &&
            RoadProtectionPolicy.LostConnections(job.Baseline.Before,roads.Read(job.Baseline,false)).Length==0 &&
            receipt.Order.Take(receipt.Current).All(i=>Observe(job.Request.Steps[i],receipt.EntityIds[i],district)=="finished");
        ledger.Advance(r.ActionId,clock.Elapsed.TotalSeconds,Guard,i=>Observe(job.Request.Steps[i],receipt.EntityIds[i],district),i=>Preflight(job.Request.Steps[i],r.Session,district,i),i=>{
            var step=job.Request.Steps[i];var template=Resolve(step);var spec=template.GetSpec<BlockObjectSpec>();
            placers.GetMatchingPlacer(spec).Place(new EntitySetup.Builder(template.Blueprint).SetId(Guid.Parse(receipt.EntityIds[i])),Placement(step));
        });
        return Receipt(r.ActionId);
    }
    private object Receipt(string id)
    {
        var r=ledger.Inspect(id);
        var result=JObject.FromObject(r,JsonSerializer.Create(new JsonSerializerSettings{ContractResolver=new CamelCasePropertyNamesContractResolver()}));
        result["limitations"]=JArray.FromObject(new[]{"development_pilot_not_complete_construction_preflight","advance_one_order_only_when_paused","all_previous_parts_must_be_finished","inspect_never_places_or_advances","simulation_requires_separate_bounded_run","stopped_or_unconfirmed_never_replayed","no_rollback_or_automatic_demolition","ledger_lost_on_game_reload","power_and_production_read_separately"});
        return result;
    }
    private string Observe(LargeProjectStep s,string id,DistrictCenter district)
    {
        var e=entities.Entities.SingleOrDefault(e=>!e.Deleted&&e.EntityId.ToString("D")==id);
        if(e is null)return "missing";if(!e.Initialized)return "pending";
        if(!e.TryGetComponent<BlockObject>(out var b)||b.IsPreview||b.Coordinates!=new Vector3Int(s.X,s.Y,s.Z)||b.Orientation!=Rotation(s.Rotation)||!e.TryGetComponent<TemplateSpec>(out var t)||t.TemplateName!=s.Template)return "mismatch";
        if(b.IsUnfinished)return e.TryGetComponent<ReachableConstructionSite>(out var site)&&site.IsReachableByBuilders()?"construction":"pending";
        if(!b.IsFinished)return "mismatch";
        if(b.HasEntrance && !district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(b.PositionedEntrance.Coordinates)))return "mismatch";
        if(e.TryGetComponent<PathSpec>(out var path)&&!district.IsOnInstantDistrictRoad(NavigationCoordinateSystem.GridToWorld(b.TransformCoordinates(path.MainPathCoordinates))))return "mismatch";
        // Entrance-less supports/shafts have no door to invent; their construction access was observed separately.
        return "finished";
    }
    private bool PreviewAccess(BlockObject b,DistrictCenter district)=>
        (!b.HasEntrance||district.IsOnPreviewDistrictRoad(NavigationCoordinateSystem.GridToWorld(b.PositionedEntrance.Coordinates)))&&
        (!b.TryGetComponent<PathSpec>(out var path)||district.IsOnPreviewDistrictRoad(NavigationCoordinateSystem.GridToWorld(b.TransformCoordinates(path.MainPathCoordinates))));
    private bool Preflight(LargeProjectStep s,string session,DistrictCenter district,int index)
    {
        Resolve(s);
        var q=new NameValueCollection{{"template",s.Template},{"x",s.X.ToString()},{"y",s.Y.ToString()},{"z",s.Z.ToString()},{"rotation",s.Rotation.ToString()},{"session",session}};
        var evidence=JObject.FromObject(validation.Validate(BridgeRequest.Parse("/agent-api/v1/building-validation",q),out _,out var safety));
        bool valid=(bool?)evidence["valid"]==true && safety.restored && safety.lostConnections==0 && !safety.constructionAccessPreview.HasKnownFailure() &&
            (safety.status=="safe"||safety.status=="unknown"&&safety.reasons.SequenceEqual(new[]{"construction_and_road_node_coverage_unproven"}));
        if(!valid)return false;
        // Recheck the proposed entrance against the current district, not only the old plan.
        var sample=roads.Capture();if(!sample.Complete)return false;
        var preview=previews[index+":"+s.Template];
        try {preview.Hide();preview.Reposition(Placement(s));preview.RemoveFromPreviewServices();preview.AddToPreviewServices();valid=PreviewAccess(preview.BlockObject,district);}
        catch {faulted=true;throw;}
        finally {
            try {preview.Hide();}catch {faulted=true;}
            try {preview.RemoveFromPreviewServices();}catch {faulted=true;}
            if(!sample.Before.SequenceEqual(roads.Read(sample,false))||!sample.PreviewBefore.SequenceEqual(roads.Read(sample,true)))faulted=true;
        }
        return valid&&!faulted;
    }
    private JObject Plan(LargeProjectRequest r)
    {
        if(faulted||speed.CurrentSpeed!=0||!ConstructionIsolation.Allows(entities))throw new BridgeRejectionException("state_conflict");
        var district=District(r.DistrictId);var order=LargeProjectRequest.Order(r.Steps);
        var templates=r.Steps.Select(Resolve).ToArray();
        var cells=r.Steps.Select((s,i)=>templates[i].GetSpec<BlockObjectSpec>().GetBlocks(Placement(s)).Take(65).ToArray()).ToArray();
        if(cells.Any(c=>c.Length is <1 or >64))throw new ArgumentException("project_occupied_extent_limit");
        int minZ=cells.SelectMany(c=>c).Min(c=>c.Coordinates.z);
        if(cells.Any(c=>c.Length is <1 or >64)||cells.SelectMany(c=>c).Any(c=>!blocks.Contains(c.Coordinates)||c.Coordinates.z-minZ>=8)||
            cells.SelectMany(c=>c).Max(c=>c.Coordinates.x)-cells.SelectMany(c=>c).Min(c=>c.Coordinates.x)>=32||
            cells.SelectMany(c=>c).Max(c=>c.Coordinates.y)-cells.SelectMany(c=>c).Min(c=>c.Coordinates.y)>=32)throw new ArgumentException("project_occupied_extent_limit");
        var baseline=roads.Capture();if(!baseline.Complete)throw new BridgeRejectionException("state_conflict");
        var before=entities.Entities.Where(e=>!e.Deleted).Select(e=>e.EntityId).ToHashSet();
        var beforeStock=Stocks();
        var existingNodes=entities.Entities.Where(e=>!e.Deleted&&e.Initialized&&e.TryGetComponent<BlockObject>(out var b)&&!b.IsPreview)
            .Select(e=>e.GetComponent<MechanicalNode>()).Where(n=>n is not null).Take(513).ToArray();
        if(existingNodes.Length>512)throw new InvalidOperationException("power_node_limit");
        if(existingNodes.Any(n=>n.Transputs.IsDefault||n.Transputs.Length>64))throw new InvalidOperationException("power_ports_unavailable");
        var existingPorts=existingNodes.SelectMany(n=>n.Transputs).ToLookup(p=>p.Coordinates);
        var active=new List<Preview>();var results=new object[r.Steps.Length];var activeNodes=new List<(int Index,Transput[] Ports)>();var links=new List<object>();
        var deferred=new bool[r.Steps.Length];
        bool eligible=true;bool restored=false;bool[]? withPreview=null;
        try
        {
            foreach(int i in order)
            {
                string cacheKey=i+":"+r.Steps[i].Template;
                if(!previews.TryGetValue(cacheKey,out var preview))
                {
                    if(previews.Count>=128)throw new InvalidOperationException("project_preview_cache_limit");
                    preview=factory.Create(templates[i].GetSpec<PlaceableBlockObjectSpec>());previews.Add(cacheKey,preview);
                }
                active.Add(preview);preview.Hide();preview.Reposition(Placement(r.Steps[i]));
                if(!preview.BlockObject.IsPreview||preview.BlockObject.AddedToService)throw new InvalidOperationException("unexpected_preview_state");
                bool valid=preview.BlockObject.IsValid()&&validators.IsValid(preview.BlockObject);
                bool collision=cells[i].Any(c=>blocks.GetObjectsAt(c.Coordinates).Any(b=>!b.IsPreview&&b.IsIntersecting(c)))||
                    Enumerable.Range(0,cells.Length).Where(j=>j!=i).Any(j=>cells[i].Any(c=>cells[j].Any(c.IsIntersecting)));
                deferred[i]=LargeProjectSupportPolicy.CanDefer(valid,collision,r.Steps[i].DependsOn,
                    cells[i].Where(c=>c.IsFoundationBlock).Select(c=>new LargeProjectSupportPolicy.Cell{X=c.Coordinates.x,Y=c.Coordinates.y,Z=c.Coordinates.z,MatterBelow=c.MatterBelow.ToString()}).ToArray(),
                    r.Steps[i].DependsOn.SelectMany(j=>cells[j].Select(c=>new LargeProjectSupportPolicy.Cell{Step=j,X=c.Coordinates.x,Y=c.Coordinates.y,Z=c.Coordinates.z,Stackable=c.Stackable==BlockStackable.BlockObject})).ToArray());
                preview.RemoveFromPreviewServices();preview.AddToPreviewServices();
                bool? connected=preview.BlockObject.HasEntrance?district.IsOnPreviewDistrictRoad(NavigationCoordinateSystem.GridToWorld(preview.BlockObject.PositionedEntrance.Coordinates)):(bool?)null;
                if(preview.BlockObject.TryGetComponent<PathSpec>(out var path)) connected=district.IsOnPreviewDistrictRoad(NavigationCoordinateSystem.GridToWorld(preview.BlockObject.TransformCoordinates(path.MainPathCoordinates)));
                var observed=roads.Read(baseline,true);int lost=RoadProtectionPolicy.LostConnections(baseline.Before,observed).Length;
                eligible &= (valid||deferred[i]) && connected!=false && lost==0;
                var geometryPorts=PowerObservations.PreviewPorts(templates[i],preview.BlockObject);
                object[] ports=PowerObservations.PortData(geometryPorts,true);
                if(geometryPorts.Length>0)
                {
                    foreach(var prior in activeNodes) foreach(var a in geometryPorts) foreach(var b in prior.Ports)
                        if(a.Faces(b))links.Add(new{fromStep=i,toStep=prior.Index,from=PowerObservations.Vec(a.Coordinates),to=PowerObservations.Vec(b.Coordinates),rotationMatches=a.RotationMatches(b),kind="planned_geometry_not_live_network"});
                    foreach(var a in geometryPorts) foreach(var b in existingPorts[a.Target])
                        if(a.Faces(b))links.Add(new{fromStep=i,toEntityId=b.ParentNode.GetComponent<EntityComponent>().EntityId.ToString("D"),from=PowerObservations.Vec(a.Coordinates),to=PowerObservations.Vec(b.Coordinates),rotationMatches=a.RotationMatches(b),kind="existing_geometry_not_live_connection"});
                    if(links.Count>512)throw new InvalidOperationException("power_link_limit");
                    activeNodes.Add((i,geometryPorts));
                }
                results[i]=new{index=i,template=r.Steps[i].Template,position=PowerObservations.Vec(new(r.Steps[i].X,r.Steps[i].Y,r.Steps[i].Z)),rotation=r.Steps[i].Rotation,dependsOn=r.Steps[i].DependsOn,
                    placementValid=valid,placementState=deferred[i]?"deferred_native_validation":valid?"native_valid":"native_invalid",previewEntranceConnected=connected,lostConnections=lost,powerPorts=ports,constructionAccess="unknown"};
            }
            // Later parts can invalidate earlier previews too.
            eligible &= active.Select((p,k)=>(p,k)).All(v=>deferred[order[v.k]]||v.p.BlockObject.IsValid()&&validators.IsValid(v.p.BlockObject));
            eligible &= active.All(p=>PreviewAccess(p.BlockObject,district));
            withPreview=roads.Read(baseline,true);
            roads.ObserveConstructionPreview(baseline,active.SelectMany(p=>p.BlockObject.PositionedBlocks.GetAllBlocks()));
        }
        catch(Exception ex) {faulted=true;Debug.LogError("AgentBridge large-project preview failed ("+ex.GetType().Name+"): "+ex.Message);throw;}
        finally
        {
            foreach(var p in active.AsEnumerable().Reverse())
            {
                try {p.Hide();} catch {faulted=true;}
                try {p.RemoveFromPreviewServices();} catch {faulted=true;}
            }
            restored=before.SetEquals(entities.Entities.Where(e=>!e.Deleted).Select(e=>e.EntityId))&&beforeStock.SequenceEqual(Stocks())&&baseline.Before.SequenceEqual(roads.Read(baseline,false));
            if(!restored)faulted=true;
        }
        var safety=roads.Finish(baseline,withPreview,cells.SelectMany(c=>c).ToArray());
        eligible &= restored&&!faulted&&safety.restored&&safety.lostConnections==0&&!safety.constructionAccessPreview.HasKnownFailure()&&
            (safety.status=="safe"||safety.status=="unknown"&&safety.reasons.SequenceEqual(new[]{"construction_and_road_node_coverage_unproven"}));
        var result=JObject.FromObject(new{order,parts=results,powerLinks=links.ToArray(),pilotEligible=eligible,regularExecutionAllowed=false,
            restored=restored&&!faulted,sessionLocked=faulted,roadProtection=safety,
            limitations=new[]{"explicit_3d_blueprint_not_automatic_terrain_route_search","maximum_32_parts_32x32_eight_occupied_levels","topological_order_all_prior_parts_finish_before_next","joint_preview_not_builder_preflight","deferred_native_validation_is_unknown_until_real_supports_finish","entranceless_parts_have_no_invented_door","planned_power_ports_not_live_network_or_supply","new_districts_and_unsupported_catalog_layouts_excluded","no_reservation_or_execution_permission","no_material_delivery_or_production_guarantee"}});
        // Runtime diagnostic counters and cached preview identities are not plan identity.
        // Start always repeats native validation; this key binds the blueprint and sampled baseline.
        result["planKey"]=Hash(Fingerprint(r)+"|"+string.Join(",",before.OrderBy(id=>id))+"|"+string.Join(",",beforeStock)+"|"+
            string.Join(",",baseline.Before.Select(b=>b?"1":"0"))+"|"+eligible);
        return result;
    }
}
