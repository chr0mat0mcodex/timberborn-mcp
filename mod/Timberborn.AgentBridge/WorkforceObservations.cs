using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.TemplateSystem;
using Timberborn.WorkSystem;

namespace Timberborn.AgentBridge;

public sealed class WorkforceObservations(EntityRegistry entities)
{
    public object Observe(BridgeRequest request)
    {
        var workers = entities.Entities.Where(e => e.Initialized && !e.Deleted && e.HasComponent<Worker>())
            .OrderBy(e => e.EntityId).ToArray();
        var items = workers.Skip(request.Offset).Take(request.Limit).Select(entity =>
        {
            var worker = entity.GetComponent<Worker>();
            var workplace = worker.Workplace;
            object? assignment = null;
            if (workplace is not null)
            {
                var target = workplace.GetComponent<EntityComponent>();
                if (target.Initialized && !target.Deleted && target.TryGetComponent<TemplateSpec>(out var template) &&
                    target.TryGetComponent<BlockObject>(out var block) && !block.IsPreview)
                    assignment = new { id = target.EntityId, template = template.TemplateName,
                        position = new { x = block.Coordinates.x, y = block.Coordinates.y, z = block.Coordinates.z } };
            }
            return new { id = entity.EntityId, workerType = worker.WorkerType, employed = worker.Employed,
                jobRunning = worker.JobRunning, assignmentStatus = workplace is null ? "unassigned" : assignment is null ? "unresolved" : "assigned",
                workplace = assignment };
        }).ToArray();
        int employed = workers.Count(e => e.GetComponent<Worker>().Employed);
        return new { scope = "entities_with_worker_component", offset = request.Offset, limit = request.Limit,
            total = workers.Length, employed, unemployed = workers.Length - employed, items,
            hasMore = request.Offset + items.Length < workers.Length,
            limitations = new[] { "pages_are_fresh_observations_not_atomic_roster", "worker_component_not_employability_or_population",
                "assignment_not_physical_location", "job_running_not_confirmed_production", "exact_job_task_not_observed",
                "unresolved_workplace_is_not_unemployed" } };
    }
}
