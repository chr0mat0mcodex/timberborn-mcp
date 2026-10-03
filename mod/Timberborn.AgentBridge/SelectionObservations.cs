using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using Timberborn.SelectionSystem;
using Timberborn.TemplateSystem;
namespace Timberborn.AgentBridge;

// Observe the UI selection only; never select, focus, follow or highlight anything.
public sealed class SelectionObservations(EntitySelectionService selection)
{
    public object Read()
    {
        object? target = null;
        var state = "none";
        if (selection.IsAnythingSelected)
        {
            state = "unsupported";
            var selected = selection.SelectedObject;
            if (selected && selected.TryGetComponent<EntityComponent>(out var entity) &&
                entity.Initialized && !entity.Deleted && entity.EntityId != Guid.Empty &&
                selected.TryGetComponent<TemplateSpec>(out var template) && !string.IsNullOrEmpty(template.TemplateName))
            {
                var hasBlock = selected.TryGetComponent<BlockObject>(out var block);
                if (!hasBlock || !block.IsPreview)
                {
                    object? grid = null;
                    if (hasBlock) { var c = block.Coordinates; grid = new { x = c.x, y = c.y, z = c.z }; }
                    var p = entity.Transform.position;
                    target = new { id = entity.EntityId, template = template.TemplateName,
                        kind = hasBlock ? "block_object" : "entity", gridPosition = grid,
                        worldPosition = new { x = p.x, y = p.y, z = p.z } };
                    state = "selected";
                }
            }
        }
        return new { state, target, limitations = new[] {
            "current_ui_selection_not_hover_or_area_marking", "selection_is_not_authorization_to_modify",
            "read_again_before_resolving_a_later_action", "world_axes_differ_from_grid_axes",
            "unsupported_means_selected_object_cannot_be_resolved", "no_personal_names" } };
    }
}
