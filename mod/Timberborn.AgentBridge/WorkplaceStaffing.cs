using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.TemplateSystem;
using Timberborn.WorkSystem;
namespace Timberborn.AgentBridge;

public sealed class WorkplaceStaffing(EntityRegistry entities)
{
    public object Set(BridgeRequest request)
    {
        var id = Guid.Parse(request.EntityId);
        var entity = entities.Entities.SingleOrDefault(e => e.EntityId == id && e.Initialized && !e.Deleted);
        if (entity is null || !entity.TryGetComponent<BlockObject>(out var block) || block.IsPreview || !block.IsFinished ||
            !entity.TryGetComponent<Workplace>(out var workplace) || !entity.TryGetComponent<TemplateSpec>(out var template))
            throw new ArgumentException("finished_workplace_required");
        int before = workplace.DesiredWorkers;
        if (before != request.ExpectedDesiredWorkers || request.DesiredWorkers > workplace.MaxWorkers)
            throw new ArgumentException("staffing_changed_or_out_of_range");
        string outcome = "applied";
        try
        {
            int steps = Math.Abs(request.DesiredWorkers - before);
            for (int i = 0; i < steps; i++)
            {
                int previous = workplace.DesiredWorkers;
                int direction = request.DesiredWorkers > previous ? 1 : -1;
                if (direction > 0) workplace.IncreaseDesiredWorkers(); else workplace.DecreaseDesiredWorkers();
                if (workplace.DesiredWorkers != previous + direction) { outcome = "unconfirmed"; break; }
            }
        }
        catch { outcome = "unconfirmed"; }
        if (workplace.DesiredWorkers != request.DesiredWorkers) outcome = "unconfirmed";
        return new { id, template = template.TemplateName, previousDesiredWorkers = before,
            requestedDesiredWorkers = request.DesiredWorkers, observedDesiredWorkers = workplace.DesiredWorkers,
            assignedWorkers = workplace.NumberOfAssignedWorkers, maxWorkers = workplace.MaxWorkers, outcome,
            limitations = new[] { "desired_staffing_not_immediate_assignment", "no_worker_teleport_or_forced_assignment",
                "read_building_and_workforce_to_confirm", "partial_change_possible_no_retry_or_rollback" } };
    }
}
