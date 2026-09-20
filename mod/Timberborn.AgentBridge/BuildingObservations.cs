using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.ConstructionSites;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.TemplateSystem;
using Timberborn.WorkSystem;

namespace Timberborn.AgentBridge;

public sealed class BuildingObservations(EntityRegistry entities)
{
    public object Observe(BridgeRequest request)
    {
        var id = Guid.Parse(request.EntityId);
        var entity = entities.Entities.SingleOrDefault(e => e.EntityId == id && e.Initialized && !e.Deleted);
        object? details = null;
        if (entity is not null && entity.TryGetComponent<BlockObject>(out var block) && !block.IsPreview &&
            entity.TryGetComponent<TemplateSpec>(out var template) && (entity.HasComponent<Building>() || template.TemplateName == "Path"))
        {
            object? construction = null;
            bool hasConstruction = entity.TryGetComponent<ConstructionSite>(out var site);
            if (hasConstruction && block.IsUnfinished)
            {
                // Do not infer remaining deliveries from RemainingRequiredGoods: its live
                // semantics were inconsistent with our interpretation on an unstarted site.
                var costs = template.GetSpec<BuildingSpec>().BuildingCost
                    .Select(g => new { id = g.Id, amount = g.Amount }).OrderBy(g => g.id, StringComparer.Ordinal).ToArray();
                var inventory = site.Inventory;
                var stock = inventory is null ? null : inventory.Stock
                    .Select(g => new { id = g.GoodId, amount = g.Amount }).OrderBy(g => g.id, StringComparer.Ordinal).ToArray();
                if (costs.Length > 32 || stock?.Length > 32) throw new InvalidOperationException("material_limit");
                construction = new
                {
                    wasStarted = site.WasStarted, isOn = site.IsOn, readyToBuild = site.ReadyToBuild,
                    materialProgress = site.MaterialProgress, buildTimeProgress = site.BuildTimeProgress,
                    buildTimeProgressInHours = site.BuildTimeProgressInHours,
                    hasMaterialsToResumeBuilding = site.HasMaterialsToResumeBuilding, readyToFinish = site.IsReadyToFinish,
                    materials = new { buildingCosts = costs, inventoryAvailable = inventory is not null, siteStock = stock }
                };
            }
            bool hasDistrict = entity.TryGetComponent<DistrictBuilding>(out var district);
            bool hasPause = entity.TryGetComponent<PausableBuilding>(out var pause);
            bool hasWorkplace = entity.TryGetComponent<Workplace>(out var workplace);
            Guid? DistrictId(DistrictCenter? center) => center is null ? null : center.GetComponent<EntityComponent>().EntityId;
            details = new
            {
                template = template.TemplateName,
                position = new { x = block.Coordinates.x, y = block.Coordinates.y, z = block.Coordinates.z },
                finished = block.IsFinished, unfinished = block.IsUnfinished,
                constructionComponentPresent = hasConstruction, construction,
                operations = new
                {
                    pauseComponentPresent = hasPause,
                    pause = hasPause ? new { paused = pause.Paused, canPause = pause.IsPausable() } : null,
                    workplaceComponentPresent = hasWorkplace,
                    workplace = hasWorkplace && block.IsFinished ? new
                    {
                        desiredWorkers = workplace.DesiredWorkers, assignedWorkers = workplace.NumberOfAssignedWorkers,
                        maxWorkers = workplace.MaxWorkers, understaffed = workplace.Understaffed,
                        overstaffed = workplace.Overstaffed, anyWorkerHasJobRunning = workplace.AnyWorkerHasJobRunning()
                    } : null
                },
                district = new
                {
                    componentPresent = hasDistrict,
                    assignedDistrictId = hasDistrict ? DistrictId(district.District) : null,
                    instantDistrictId = hasDistrict ? DistrictId(district.InstantDistrict) : null,
                    constructionDistrictId = hasDistrict ? DistrictId(district.ConstructionDistrict) : null
                }
            };
        }
        return new { id, found = details is not null, details,
            limitations = new[] { "district_assignments_not_worker_or_delivery_guarantee", "navigation_updates_may_lag",
                "construction_details_only_for_unfinished_entities", "progress_values_from_game_not_completion_prediction",
                "building_costs_are_total_template_costs_not_remaining_deliveries",
                "site_stock_excludes_already_consumed_materials_and_incoming_deliveries",
                "remaining_delivery_requirement_unknown", "missing_or_nonbuilding_entity_returns_found_false",
                "workplace_state_only_for_finished_entities", "assigned_workers_not_actual_production",
                "running_job_not_confirmed_output", "production_blocking_reasons_not_observed" } };
    }
}
