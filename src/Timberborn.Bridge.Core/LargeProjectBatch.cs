namespace Timberborn.Bridge.Core;

// Game-thread controller. The simulation owner supplies bounded time and pause confirmation.
// Submit the entire structure in one game-thread pass, then observe it. Never replay a placement.
public sealed class LargeProjectBatch
{
    public sealed class Receipt
    {
        public string ExecutionMode { get; set; } = "batch";
        public string ActionId { get; set; } = "";
        public string PlanKey { get; set; } = "";
        public string State { get; set; } = "running";
        public string Reason { get; set; } = "dispatching_ready_orders";
        public int? BlockedPart { get; set; }
        public int[] Order { get; set; } = Array.Empty<int>();
        public string[] EntityIds { get; set; } = Array.Empty<string>();
        public string[] PartStates { get; set; } = Array.Empty<string>();
        public string[] PartReasons { get; set; } = Array.Empty<string>();
        public string?[] PartExceptionTypes { get; set; } = Array.Empty<string?>();
        public int[] DeferredParts { get; set; } = Array.Empty<int>();
        public int Ordered => PartStates.Count(s => s is "unconfirmed" or "construction" or "finished");
        public int Finished => PartStates.Count(s => s == "finished");
        public bool RegularExecutionAllowed { get; set; }
        public SimulationRunStatus Simulation { get; set; } = new();
    }

    private readonly Receipt result;
    private readonly double[] deadlines;
    private string? finishState;
    private bool cancelRequested;
    private double lastTick;
    private bool wasLocked;
    public string Fingerprint { get; }
    public bool Terminal => result.State is "completed" or "blocked" or "stopped" or "unconfirmed" or "interrupted";
    public Receipt Inspect() => result;

    public LargeProjectBatch(LargeProjectRequest request, string fingerprint, string[] entityIds)
    {
        if (request.Operation is not ("run" or "run-repair") || entityIds.Length != request.Steps.Length || entityIds.Distinct().Count() != entityIds.Length || entityIds.Any(s => !Guid.TryParseExact(s,"D",out var id) || id == Guid.Empty)) throw new ArgumentException();
        Fingerprint = fingerprint;
        deadlines = new double[entityIds.Length];
        result = new Receipt { ActionId = request.ActionId, PlanKey = request.PlanKey, Order = LargeProjectRequest.Order(request.Steps), EntityIds = entityIds.ToArray(), PartStates = Enumerable.Repeat("pending", entityIds.Length).ToArray(), PartReasons = Enumerable.Repeat("not_submitted", entityIds.Length).ToArray(), DeferredParts = Array.Empty<int>() };
        result.PartExceptionTypes = new string?[entityIds.Length];
    }
    public void Stop() { if (!Terminal && finishState is null) Finish("stopped", "stop_requested_objects_retained"); }
    private void Finish(string state, string reason, int? part = null)
    {
        finishState = state; result.State = "pausing"; result.Reason = reason; result.BlockedPart = part;
    }
    public void Tick(double now, SimulationRunStatus simulation, bool locked, Func<bool> guard, Func<int,string> observe, Action<int> place, Action cancel, Action beginSimulation, Func<bool>? completionReady = null)
    {
        if (Terminal) return;
        result.Simulation = simulation;
        // A game-owned lock can delay entity initialization. Only confirmation grace is frozen;
        // the simulation controller's overall real-time limit is never extended.
        if (locked || wasLocked)
            for (int i = 0; i < deadlines.Length; i++) if (result.PartStates[i] == "unconfirmed") deadlines[i] += Math.Max(0, now - lastTick);
        lastTick = now; wasLocked = locked;
        if (simulation.Terminal && finishState is null)
            Finish(simulation.State == "interrupted" ? "interrupted" : "blocked", simulation.State == "completed" ? "game_time_limit" : "simulation_" + simulation.Reason);
        bool initialDispatch = simulation.State == "starting" && simulation.Reason == "awaiting_dispatch" && simulation.ObservedSpeed == 0;
        if (finishState is null && !locked && (initialDispatch || simulation.State == "running" && simulation.ObservedSpeed == simulation.RequestedSpeed))
        {
            int? currentPart = null;
            try
            {
                if (!guard()) Finish("blocked", "guard_changed");
                else
                {
                    if (initialDispatch)
                    {
                        // No observation, preview, material/access test, frame yield or build wait
                        // between orders. dependsOn determines submission order, not finish waits.
                        foreach (int i in result.Order.Where(i => result.PartStates[i] == "pending"))
                        {
                            result.PartStates[i] = "unconfirmed"; deadlines[i] = now + 5;
                            result.PartReasons[i] = "awaiting_order_confirmation";
                            try { place(i); }
                            catch (BridgeRejectionException ex) {
                                result.PartStates[i] = "rejected"; result.PartReasons[i] = ex.Code;
                            }
                            catch (Exception ex) when (ex is not OutOfMemoryException) {
                                // Unknown side effects never authorize a retry. Other orders still submit.
                                result.PartReasons[i] = "placement_failed_no_retry";
                                result.PartExceptionTypes[i] = ex.GetType().Name;
                            }
                        }
                    }
                    else foreach (int i in result.Order.Where(i => result.PartStates[i] is "unconfirmed" or "construction" or "finished"))
                    {
                        currentPart = i;
                        string observed = observe(i);
                        if (observed == "construction_unreachable") {
                            result.PartStates[i] = "construction";
                            result.PartReasons[i] = "builders_unreachable";
                        }
                        else if (observed is "construction" or "finished") {
                            result.PartStates[i] = observed;
                            result.PartReasons[i] = observed == "finished" ? "finished_access_observed" : "awaiting_materials_and_workers";
                        }
                        else if (observed == "mismatch") {
                            result.PartReasons[i] = "object_or_access_mismatch";
                            Finish("blocked", "object_or_access_mismatch", i);
                        }
                        else if (observed is "missing" or "pending") {
                            if (now >= deadlines[i]) {
                                result.PartReasons[i] = "order_or_access_unconfirmed";
                                Finish("unconfirmed", "order_or_access_unconfirmed", i);
                            }
                        }
                        else throw new InvalidOperationException();
                    }
                    if (finishState is null && result.Finished == result.PartStates.Length && (completionReady?.Invoke() ?? true)) Finish("completed", "all_parts_finished_and_access_observed");
                    else if (finishState is null && result.PartStates.Contains("rejected") && result.PartStates.All(s => s is "finished" or "rejected")) Finish("blocked", "partial_structure_rejected_parts", Array.IndexOf(result.PartStates, "rejected"));
                    else if (finishState is null) result.Reason = result.Finished == result.PartStates.Length ? "awaiting_repaired_sites" : "awaiting_structure_completion";
                    currentPart = null;
                    if (finishState is null && initialDispatch) beginSimulation();
                }
            }
            catch { Finish("blocked", "observation_or_validation_failed", currentPart); }
        }
        if (finishState is null) return;
        if (!cancelRequested && !simulation.Terminal) { cancelRequested = true; cancel(); }
        if (simulation.Terminal)
        {
            // Completion requires a confirmed pause; a user interruption never triggers a speed override.
            result.State = simulation.PauseConfirmed && !(finishState == "completed" && simulation.State != "cancelled") ? finishState : "interrupted";
            if (!simulation.PauseConfirmed) result.Reason = "pause_unconfirmed_" + result.Reason;
            else if(result.State=="interrupted"&&finishState=="completed")result.Reason="completion_pause_interrupted";
        }
    }
}
