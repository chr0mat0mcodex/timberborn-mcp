using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.GameFactionSystem;
using Timberborn.Goods;
using Timberborn.ResourceCountingSystem;
using Timberborn.ScienceSystem;
using Timberborn.TemplateSystem;
using Timberborn.TerrainSystem;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Timberborn.GameDistricts;
using Timberborn.Navigation;

namespace Timberborn.AgentBridge;

// PreviewFactory uses the game's preview instantiation path, not EntityService placement.
// Bound hidden previews owned by the game scene; never use entity deletion for previews.
public sealed class SiteValidation(PreviewFactory factory, BlockObjectValidationService validators,
    TemplateNameMapper templates, FactionService faction, BuildingUnlockingService unlocks,
    EntityRegistry entities, ResourceCountingService resources, IGoodService goods, ITerrainService terrain, BuildingCatalog catalog, RoadProtection roads,
    BuildingProjectPlanner projectPlanner)
{
    private readonly Dictionary<string, Preview> previews = new();
    private bool faulted;
    private int attempts;
    private int buildingAttempts;
    private int projectAttempts;

    public object ValidateProject(BuildingProjectValidationRequest request)
    {
        if(faulted||projectAttempts>=16)throw new InvalidOperationException("validation_session_locked");
        var plan=request.Plan;
        var current=JObject.FromObject(projectPlanner.Plan(plan));
        var options=(JArray)current["options"]!;
        if(request.OptionIndex>=options.Count||(string?)options[request.OptionIndex]["planKey"]!=request.PlanKey)
            throw new BridgeRejectionException("state_conflict");
        var option=options[request.OptionIndex];
        Vector3Int Pos(JToken p)=>new((int)p["x"]!,(int)p["y"]!,(int)p["z"]!);
        var pathCells=option["newRoadCells"]!.Select(Pos).ToArray();
        if(pathCells.Length>8)throw new ArgumentException("project_preview_limit");
        var template=catalog.Resolve(plan.Template);var pathTemplate=catalog.Resolve("Path");
        if(!unlocks.Unlocked(template.GetSpec<BuildingSpec>())||!unlocks.Unlocked(pathTemplate.GetSpec<BuildingSpec>()))
            throw new BridgeRejectionException("template_locked");
        string buildingKey="project-building:"+plan.Template;
        var keys=Enumerable.Range(0,pathCells.Length).Select(i=>"project-path:"+i).Concat(new[]{buildingKey}).ToArray();
        if(previews.Count+keys.Count(k=>!previews.ContainsKey(k))>64)throw new InvalidOperationException("validation_session_locked");
        var district=entities.Entities.Single(e=>e.EntityId==Guid.Parse(plan.DistrictId)&&!e.Deleted).GetComponent<DistrictCenter>();
        var beforeIds=RegisteredIds();var beforeStock=Stocks();
        var active=new List<Preview>();var roadValid=new List<bool>();
        var stepLost=new List<int>();bool buildingValid=false,connection=false;
        RoadProtection.Sample? sample=null;bool[]? withPreview=null;
        var candidate=template.GetSpec<BlockObjectSpec>().GetBlocks(new Placement(Pos(option["origin"]!),
            new[]{Orientation.Cw0,Orientation.Cw90,Orientation.Cw180,Orientation.Cw270}[plan.Rotation],FlipMode.Unflipped)).Take(65).ToArray();
        if(candidate.Length==0||candidate.Length>64)throw new ArgumentException("unsupported_geometry");
        projectAttempts++;
        try {
            sample=roads.Capture();
            if(!sample.Complete)throw new InvalidOperationException("project_navigation_baseline_unknown");
            Preview Add(string key,PlaceableBlockObjectSpec spec,Vector3Int position,int rotation) {
                if(!previews.TryGetValue(key,out var preview)){preview=factory.Create(spec);previews.Add(key,preview);}
                active.Add(preview);preview.Hide();
                preview.Reposition(new Placement(position,new[]{Orientation.Cw0,Orientation.Cw90,Orientation.Cw180,Orientation.Cw270}[rotation],FlipMode.Unflipped));
                if(!preview.BlockObject.IsPreview||preview.BlockObject.AddedToService)throw new InvalidOperationException("unexpected_preview_state");
                return preview;
            }
            for(int i=0;i<pathCells.Length;i++) {
                var preview=Add(keys[i],pathTemplate.GetSpec<PlaceableBlockObjectSpec>(),pathCells[i],0);
                roadValid.Add(preview.BlockObject.IsValid()&&validators.IsValid(preview.BlockObject));
                preview.RemoveFromPreviewServices();preview.AddToPreviewServices();
                stepLost.Add(RoadProtectionPolicy.LostConnections(sample.Before,roads.Read(sample,true)).Length);
            }
            var building=Add(buildingKey,template.GetSpec<PlaceableBlockObjectSpec>(),Pos(option["origin"]!),plan.Rotation);
            buildingValid=building.BlockObject.IsValid()&&validators.IsValid(building.BlockObject);
            building.RemoveFromPreviewServices();building.AddToPreviewServices();
            // Recheck paths with the building present too, not just the growing road prefix.
            for(int i=0;i<pathCells.Length;i++)roadValid[i]=roadValid[i]&&active[i].BlockObject.IsValid()&&validators.IsValid(active[i].BlockObject);
            connection=district.IsOnPreviewDistrictRoad(NavigationCoordinateSystem.GridToWorld(Pos(option["entrance"]!)));
            withPreview=roads.Read(sample,true);
            roads.ObserveConstructionPreview(sample, active.SelectMany(p => p.BlockObject.PositionedBlocks.GetAllBlocks()));
        }
        catch {faulted=true;throw;}
        finally {
            Exception? cleanupError=null;
            foreach(var preview in active.AsEnumerable().Reverse()) {
                try {preview.Hide();}
                catch(Exception ex){faulted=true;cleanupError=ex;}
                try {preview.RemoveFromPreviewServices();}
                catch(Exception ex){faulted=true;cleanupError=ex;}
            }
            if(cleanupError is not null)throw new InvalidOperationException("project_preview_cleanup_failed",cleanupError);
        }
        RoadProtection.Report safety;
        bool unchanged;
        try { safety=roads.Finish(sample!,withPreview,candidate);unchanged=beforeIds.SetEquals(RegisteredIds())&&beforeStock.SequenceEqual(Stocks()); }
        catch {faulted=true;throw;}
        if(!unchanged||!safety.restored)faulted=true;
        return new {template=plan.Template,planKey=request.PlanKey,optionIndex=request.OptionIndex,option,
            buildingValid,roadValid=roadValid.ToArray(),roadStepLostConnections=stepLost.ToArray(),
            previewEntranceConnected=connection,roadProtection=safety,noPersistentChangeObserved=unchanged&&!faulted,
            sessionLocked=faulted,attemptsRemaining=16-projectAttempts,executable=false,
            limitations=new[]{"joint_preview_not_build_order","preview_connection_not_builder_reachability","construction_state_unproven",
                "registered_entities_and_stock_not_all_game_state","cached_previews_until_scene_unload","maximum_eight_new_roads","no_execution_token"} };
    }

    public object Validate(BridgeRequest request) => Validate(request, out _);

    public object Validate(BridgeRequest request, out bool allowed)
        => Validate(request, out allowed, out _);

    public object Validate(BridgeRequest request, out bool allowed, out RoadProtection.Report safety)
    {
        allowed = false;
        safety = new RoadProtection.Report { reasons = new[] { "game_validation_not_passed" } };
        if (faulted || (request.GenericBuilding ? buildingAttempts >= 256 : attempts >= 8) ||
            (!previews.ContainsKey(request.Template) && previews.Count >= 64)) throw new InvalidOperationException("validation_session_locked");
        if (!request.GenericBuilding && (request.Template is not ("Lodge.Folktails" or "Path") ||
            (request.Template == "Lodge.Folktails" && faction.Current.Id != "Folktails"))) throw new ArgumentException("invalid_template");
        var template = request.GenericBuilding ? catalog.Resolve(request.Template) : templates.GetTemplate(request.Template);
        var building = template.GetSpec<BuildingSpec>();
        var placeable = template.GetSpec<PlaceableBlockObjectSpec>();
        var spec = template.GetSpec<BlockObjectSpec>();
        if (!template.UsableWithCurrentFeatureToggles || !placeable.UsableWithCurrentFeatureToggles )
            throw new BridgeRejectionException("template_disabled");
        if (!unlocks.Unlocked(building)) throw new BridgeRejectionException("template_locked");
        var rotation = new[] { Orientation.Cw0, Orientation.Cw90, Orientation.Cw180, Orientation.Cw270 }[request.Rotation];
        var placement = new Placement(new Vector3Int(request.X, request.Y, request.Z), rotation, FlipMode.Unflipped);
        var cells = spec.GetBlocks(placement).Take(65).ToArray();
        if (cells.Length == 0 || cells.Length > 64 || cells.Any(c => !terrain.Contains(c.Coordinates)))
            throw new ArgumentException("invalid_region");
        var beforeIds = RegisteredIds();
        var beforeStock = Stocks();
        Preview? preview = null;
        bool valid = false;
        RoadProtection.Sample? sample = null;
        bool[]? previewConnections = null;
        if (request.GenericBuilding) buildingAttempts++; else attempts++;
        try
        {
            if (!previews.TryGetValue(request.Template, out preview))
            {
                preview = factory.Create(placeable);
                previews.Add(request.Template, preview);
            }
            preview.Hide();
            preview.Reposition(placement);
            if (!preview.BlockObject.IsPreview || preview.BlockObject.AddedToService)
                throw new InvalidOperationException("unexpected_preview_state");
            // The service alone accepted an occupied footprint in the 0.4.0 live pilot.
            // Include the object's own validity gate, as used by the reference placement flow.
            bool objectValid = preview.BlockObject.IsValid();
            bool serviceValid = validators.IsValid(preview.BlockObject);
            valid = objectValid && serviceValid;
            // Diagnose rejected geometry too: an occupied entrance is a useful negative
            // control. Placement still requires valid below; this only touches previews.
            preview.RemoveFromPreviewServices();
            sample = roads.Capture();
            if (sample.Complete)
            {
                preview.AddToPreviewServices();
                previewConnections = roads.Read(sample, true);
                roads.ObserveConstructionPreview(sample, cells);
            }
        }
        catch { faulted = true; throw; }
        finally
        {
            if (preview is not null)
            {
                try { preview.Hide(); preview.RemoveFromPreviewServices(); }
                catch { faulted = true; throw; }
            }
        }
        bool unchanged = beforeIds.SetEquals(RegisteredIds()) && beforeStock.SequenceEqual(Stocks());
        if (!unchanged) faulted = true;
        if (sample is not null) safety = roads.Finish(sample, previewConnections, cells);
        if (sample?.Complete == true && !safety.restored) faulted = true;
        unchanged = unchanged && !faulted;
        allowed = unchanged && valid && safety.status == "safe" && !faulted;
        return new { template = request.Template, origin = new { x = request.X, y = request.Y, z = request.Z },
            rotation = request.Rotation, gameValidated = true, valid = unchanged ? (bool?)valid : null, roadProtection = safety,
            noPersistentChangeObserved = unchanged, sessionLocked = faulted, attemptsRemaining = request.GenericBuilding ? 256 - buildingAttempts : 8 - attempts,
            limitations = new[] { "preview_validation_not_placement", "no_build_order_created", "no_material_delivery_or_completion_guarantee",
                "registered_entity_ids_and_global_stock_checked_not_all_game_state", "hidden_preview_cached_until_scene_unload" } };
    }

    private HashSet<Guid> RegisteredIds() => new(entities.Entities.Where(e => !e.Deleted).Select(e => e.EntityId));
    private int[] Stocks() => goods.Goods.OrderBy(id => id, StringComparer.Ordinal)
        .Select(id => resources.GetGlobalResourceCount(id).AllStock).ToArray();
}
