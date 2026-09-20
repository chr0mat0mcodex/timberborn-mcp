using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.ConstructionSites;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.TemplateSystem;

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
                var remaining = new SortedSet<GoodAmount>(Comparer<GoodAmount>.Create((a, b) => StringComparer.Ordinal.Compare(a.GoodId, b.GoodId)));
                site.RemainingRequiredGoods(remaining);
                if (remaining.Count > 32) throw new InvalidOperationException("material_limit");
                construction = new
                {
                    wasStarted = site.WasStarted, isOn = site.IsOn, readyToBuild = site.ReadyToBuild,
                    materialProgress = site.MaterialProgress, buildTimeProgress = site.BuildTimeProgress,
                    buildTimeProgressInHours = site.BuildTimeProgressInHours,
                    hasMaterialsToResumeBuilding = site.HasMaterialsToResumeBuilding, readyToFinish = site.IsReadyToFinish,
                    remainingRequiredGoods = remaining.Select(g => new { id = g.GoodId, amount = g.Amount }).ToArray()
                };
            }
            bool hasDistrict = entity.TryGetComponent<DistrictBuilding>(out var district);
            Guid? DistrictId(DistrictCenter? center) => center is null ? null : center.GetComponent<EntityComponent>().EntityId;
            details = new
            {
                template = template.TemplateName,
                position = new { x = block.Coordinates.x, y = block.Coordinates.y, z = block.Coordinates.z },
                finished = block.IsFinished, unfinished = block.IsUnfinished,
                constructionComponentPresent = hasConstruction, construction,
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
                "remaining_required_goods_not_global_stock_or_delivery_eta", "missing_or_nonbuilding_entity_returns_found_false" } };
    }
}
