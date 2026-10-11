namespace Timberborn.Bridge.Core;

// Game-thread controller. The simulation owner supplies bounded time and pause confirmation.
// Inspect has no effects; Tick alone dispatches each dependency-ready part exactly once.
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
        public int[] DeferredParts { get; set; } = Array.Empty<int>();
        public int Ordered => PartStates.Count(s => s != "pending");
        public int Finished => PartStates.Count(s => s == "finished");
        public bool RegularExecutionAllowed { get; set; }
        public SimulationRunStatus Simulation { get; set; } = new();
    }

    private readonly Receipt result;
    private readonly int[][] dependencies;
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
        if (request.Operation != "run" || entityIds.Length != request.Steps.Length || entityIds.Distinct().Count() != entityIds.Length || entityIds.Any(s => !Guid.TryParseExact(s,"D",out var id) || id == Guid.Empty)) throw new ArgumentException();
        Fingerprint = fingerprint;
        dependencies = request.Steps.Select(s => s.DependsOn.ToArray()).ToArray();
        deadlines = new double[entityIds.Length];
        result = new Receipt { ActionId = request.ActionId, PlanKey = request.PlanKey, Order = LargeProjectRequest.Order(request.Steps), EntityIds = entityIds.ToArray(), PartStates = Enumerable.Repeat("pending", entityIds.Length).ToArray(), DeferredParts = Enumerable.Range(0,entityIds.Length).Where(i=>dependencies[i].Length>0).ToArray() };
    }
    public void Stop() { if (!Terminal && finishState is null) Finish("stopped", "stop_requested_objects_retained"); }
    private void Finish(string state, string reason, int? part = null)
    {
        finishState = state; result.State = "pausing"; result.Reason = reason; result.BlockedPart = part;
    }
    public void Tick(double now, SimulationRunStatus simulation, bool locked, Func<bool> guard, Func<int,string> observe, Func<int,bool> preflight, Action<int> place, Action cancel, Action beginSimulation)
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
                    foreach (int i in result.Order.Where(i => result.PartStates[i] != "pending"))
                    {
                        currentPart = i;
                        string observed = observe(i);
                        if (observed is "construction" or "finished") result.PartStates[i] = observed;
                        else if (observed == "mismatch") { Finish("blocked", "object_or_access_mismatch", i); break; }
                        else if (observed is "missing" or "pending")
                        {
                            if (now >= deadlines[i]) { Finish("unconfirmed", "order_or_access_unconfirmed", i); break; }
                        }
                        else throw new InvalidOperationException();
                    }
                    // No wait per part. A single tick can issue all 32 independent orders.
                    if (finishState is null && !result.PartStates.Contains("unconfirmed"))
                    foreach (int i in result.Order.Where(i => result.PartStates[i] == "pending"))
                    {
                        currentPart = i;
                        if (dependencies[i].Any(d => result.PartStates[d] != "finished")) continue;
                        if (!preflight(i)) { Finish("blocked", "native_step_validation_failed", i); break; }
                        result.PartStates[i] = "unconfirmed"; deadlines[i] = now + 5;
                        // Record BEFORE crossing the placement boundary; exceptions cannot cause replay.
                        try { place(i); }
                        catch { Finish("unconfirmed", "placement_failed_no_retry", i); break; }
                    }
                    if (finishState is null && result.Finished == result.PartStates.Length) Finish("completed", "all_parts_finished_and_access_observed");
                    else if (finishState is null) result.Reason = "awaiting_construction_and_dependencies";
                    currentPart = null;
                    if (finishState is null && initialDispatch) beginSimulation();
                }
            }
            catch { Finish("blocked", "observation_or_validation_failed", currentPart); }
        }
        result.DeferredParts = result.Order.Where(i => result.PartStates[i] == "pending" && dependencies[i].Any(d => result.PartStates[d] != "finished")).ToArray();
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
