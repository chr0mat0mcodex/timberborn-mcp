namespace Timberborn.Bridge.Core;

// Main-thread owned. Wall time is monotonic and injected; no Unity or network dependency.
public sealed class SimulationRunStatus
{
    public string RunId { get; set; } = "";
    public string State { get; set; } = "starting";
    public string Reason { get; set; } = "awaiting_speed";
    public double StartGameHours { get; set; }
    public double TargetGameHours { get; set; }
    public double ObservedGameHours { get; set; }
    public double ElapsedGameHours { get; set; }
    public double OvershootHours { get; set; }
    public double ElapsedRealSeconds { get; set; }
    public int MaxRealSeconds { get; set; }
    public int RequestedSpeed { get; set; }
    public int CompletionSpeed { get; set; }
    public int? EffectiveCompletionSpeed { get; set; }
    public bool CompletionSpeedConfirmed { get; set; }
    public bool CompletionPauseObserved { get; set; }
    public string? CompletionFallbackReason { get; set; }
    public float ObservedSpeed { get; set; }
    public bool PauseConfirmed { get; set; }
    public bool TargetReached { get; set; }
    public bool Terminal { get; set; }
    public int? StopAtAvailableLogs { get; set; }
    public int? ObservedAvailableLogs { get; set; }
    public bool StockTargetReached { get; set; }
    public ProductionTargetState[] GoodTargets { get; set; } = Array.Empty<ProductionTargetState>();
    public int? StopAtScience { get; set; }
    public int? ObservedScience { get; set; }
    public bool ProductionTargetsReached { get; set; }
    public string[] BuildingIds { get; set; } = Array.Empty<string>();
    public BuildingProductionState[] BuildingProduction { get; set; } = Array.Empty<BuildingProductionState>();
    public long BuildingJournalCursor { get; set; }
    public bool BuildingProductionReached { get; set; }
    public bool MeasureProduction { get; set; }
    public bool HasBuildingTargets => BuildingIds.Length>0;
    public bool HasProductionTargets => GoodTargets.Length > 0 || StopAtScience is not null;
}

public sealed class SimulationRunController
{
    public const int Capacity = 128;
    private readonly Dictionary<string, SimulationRunStatus> history = new();
    private SimulationRunStatus? active;
    private double started, phaseStarted, lastProgress, lastHours;
    private float initialSpeed;
    private bool pauseAfterUnlock, wasLocked;
    private bool deferredStart;
    private bool completionAttempted;
    private string finishState = "completed", finishReason = "target_reached";

    public SimulationRunStatus Inspect(string id) => history.TryGetValue(id, out var r) ? r : throw new BridgeRejectionException("run_not_found");

    public void RequireUnusedRun(string id) {
        if(history.ContainsKey(id))throw new BridgeRejectionException("run_id_used");
        if(history.Count>=Capacity)throw new BridgeRejectionException("state_conflict");
    }
    public bool StockConditionActive => active?.StopAtAvailableLogs is not null;
    public bool HasActiveRun => active is not null;
    public string? ActiveRunId => active?.RunId;
    public bool IsCompleting => active?.State == "completing";
    public bool IsPausing => active?.State == "pausing";
    public SimulationRunStatus? ActiveBuildingRun => active?.HasBuildingTargets==true?active:null;
    public SimulationRunStatus? ActiveProductionRun => active?.HasProductionTargets == true ? active : null;
    public bool BuildingObservationNeedsReconcile(double hours,double now) =>
        active is {HasBuildingTargets:true,MeasureProduction:true} r && r.State!="pausing" &&
        (hours>=r.TargetGameHours||now-started>=r.MaxRealSeconds);

    public SimulationRunStatus Start(SimulationRunRequest r, double hours, float speed, double now, Action<int> changeSpeed, int? availableLogs = null, bool deferSpeedStart = false, ProductionSample? production = null, BuildingProductionSample? buildingProduction = null)
    {
        if (!r.Starts || deferSpeedStart && r.CompletionSpeed != 0) throw new ArgumentException();
        ValidateClock(hours, speed, now);
        if (history.ContainsKey(r.RunId)) throw new BridgeRejectionException("run_id_used");
        if (active is not null) throw new BridgeRejectionException("run_start_active");
        if (history.Count >= Capacity) throw new BridgeRejectionException("run_start_capacity");
        if (speed != r.ExpectedSpeed) throw new BridgeRejectionException("run_start_speed_changed");
        if (r.StopAtAvailableLogs is not null && (availableLogs is null || availableLogs < 0)) throw new BridgeRejectionException("run_start_stock_unavailable");
        double target = r.Route == "simulation-run-for" ? hours + r.DurationHours : r.TargetHours;
        if (target < hours || target - hours > 672) throw new BridgeRejectionException("invalid_time_target");
        var pending = new SimulationRunStatus { RunId = r.RunId, StartGameHours = hours, TargetGameHours = target,
            ObservedGameHours = hours, RequestedSpeed = r.Speed, CompletionSpeed = r.CompletionSpeed, ObservedSpeed = speed, MaxRealSeconds = r.MaxRealSeconds,
            StopAtAvailableLogs = r.StopAtAvailableLogs, ObservedAvailableLogs = r.StopAtAvailableLogs is null ? null : availableLogs,
            GoodTargets = r.GoodTargets.Select(t => new ProductionTargetState { Good=t.Good, Amount=t.Amount }).ToArray(), StopAtScience=r.StopAtScience, BuildingIds=r.BuildingIds,MeasureProduction=r.MeasureProduction };
        if (pending.HasProductionTargets && !ProductionTargets.Observe(pending, production))
            throw new BridgeRejectionException("state_conflict");
        if(pending.HasBuildingTargets&&BuildingProductionProof.Observe(pending,buildingProduction,true) is not null)throw new BridgeRejectionException("building_proof_observation_unavailable");
        active = pending;
        history.Add(r.RunId, active);
        started = phaseStarted = lastProgress = now; lastHours = hours; initialSpeed = speed;
        pauseAfterUnlock = wasLocked = false;
        deferredStart = deferSpeedStart;
        completionAttempted = false;
        var result = active;
        if (active.ProductionTargetsReached) BeginPause("completed", "production_targets_reached", now, changeSpeed);
        else if (r.StopAtAvailableLogs is { } threshold && availableLogs >= threshold) {
            active.StockTargetReached = true; BeginPause("completed", "stock_target_reached", now, changeSpeed);
        }
        else if (hours >= target) BeginPause("completed", "target_reached", now, changeSpeed);
        else if (deferredStart) active.Reason = "awaiting_dispatch";
        else RequestSpeed(r.Speed, changeSpeed);
        Observe(result, hours, speed, now);
        return result;
    }

    // A construction batch reserves the controller while paused, dispatches, then starts once.
    public void BeginDeferred(string id, double now, Action<int> changeSpeed)
    {
        if (active is null || active.RunId != id || active.State != "starting" || !deferredStart) throw new BridgeRejectionException("state_conflict");
        deferredStart = false; phaseStarted = now; active.Reason = "awaiting_speed";
        RequestSpeed(active.RequestedSpeed, changeSpeed);
    }

    public void Tick(double hours, float speed, double now, Action<int> changeSpeed, bool speedLocked = false, int? availableLogs = null, ProductionSample? production = null, BuildingProductionSample? buildingProduction = null)
    {
        if (active is null) return;
        if (active.CompletionSpeed == 1 && !ValidObservation(hours, speed, now)) {
            active.CompletionFallbackReason ??= "completion_observation_unavailable";
            if (IsPausing) {
                finishState = "failed"; finishReason = active.Reason = "completion_observation_unavailable";
            } else {
                double failureTime = double.IsNaN(now) || double.IsInfinity(now) || now < 0 ? phaseStarted : now;
                BeginPause("failed", "completion_observation_unavailable", failureTime, changeSpeed, speedLocked);
            }
            return;
        }
        ValidateClock(hours, speed, now);
        var r = active; Observe(r, hours, speed, now);
        if (deferredStart && r.State == "starting" && !speedLocked)
        {
            if (speed != initialSpeed) Finish("interrupted", "speed_changed");
            else if (now - started >= r.MaxRealSeconds) BeginPause("failed", "real_time_limit", now, changeSpeed);
            return;
        }
        if (speedLocked) {
            CompletionLocked(now, changeSpeed);
            wasLocked = true;
            if (r.State == "pausing") { pauseAfterUnlock = true; return; }
            r.Reason = "game_speed_locked";
            if (hours < lastHours) BeginPause("failed", "clock_reversed", now, changeSpeed, true);
            else if (hours >= r.TargetGameHours) BeginPause("completed", r.MeasureProduction ? "production_measurement_complete" : r.HasBuildingTargets ? "building_production_time_limit" : r.HasProductionTargets ? "production_time_limit" : "target_reached", now, changeSpeed, true);
            else if (now - started >= r.MaxRealSeconds) BeginPause("failed", "real_time_limit", now, changeSpeed, true);
            return;
        }
        if (wasLocked) {
            // Do not count a game-owned pause as stalled simulation or extend the real-time budget.
            wasLocked = false; lastProgress = now; phaseStarted = now;
            if (r.State != "pausing") {
                r.Reason = r.State == "starting" ? "awaiting_speed" : "advancing";
                return; // SpeedManager applies restored speed in LateUpdate, after the lock event.
            }
        }
        if (r.State == "pausing") {
            if (pauseAfterUnlock) {
                if (speed != 0 && speed != r.RequestedSpeed && speed != initialSpeed && !(completionAttempted && speed == 1)) { Finish("interrupted", "speed_changed"); return; }
                pauseAfterUnlock = false; phaseStarted = now;
                RequestSpeed(0, changeSpeed); return; // Confirm on a later unlocked update.
            }
            if (speed == 0) {
                if (r.CompletionSpeed == 1 && finishState == "completed" && r.CompletionFallbackReason is null) {
                    r.CompletionPauseObserved = true;
                    if (hours < lastHours || !r.TargetReached) {
                        r.CompletionFallbackReason = "clock_reversed";
                        Finish("failed", "clock_reversed");
                    } else if (now - started > r.MaxRealSeconds - 5) {
                        r.CompletionFallbackReason = "real_time_limit";
                        Finish("failed", "real_time_limit");
                    } else {
                        r.State = "completing"; r.Reason = "awaiting_completion_speed";
                        phaseStarted = now; completionAttempted = true;
                        try { changeSpeed(1); }
                        catch { SpeedCommandFailed(now, changeSpeed); }
                    }
                } else Finish(finishState, finishReason);
            }
            else if (speed != r.RequestedSpeed && speed != initialSpeed && !(completionAttempted && speed == 1)) Finish("interrupted", "speed_changed");
            else if (now - phaseStarted >= 5) Finish("failed", "pause_unconfirmed");
            return;
        }
        if (r.State == "completing") {
            if (speed != 0 && speed != 1) { Finish("interrupted", "speed_changed"); return; }
            if (hours < lastHours) BeginPause("failed", "clock_reversed", now, changeSpeed);
            else if (now - started >= r.MaxRealSeconds || now - phaseStarted >= 5)
                BeginPause("failed", "completion_speed_unconfirmed", now, changeSpeed);
            else if (speed == 1) Finish("completed", "target_reached");
            return;
        }
        if (r.State == "starting") {
            if (speed == r.RequestedSpeed) { r.State = "running"; r.Reason = "advancing"; }
            else if (speed != initialSpeed) { Finish("interrupted", "speed_changed"); return; }
            else if (now - phaseStarted >= 5) { BeginPause("failed", "speed_unconfirmed", now, changeSpeed); return; }
        } else if (speed != r.RequestedSpeed) { Finish("interrupted", "speed_changed"); return; }
        if (hours < lastHours) { BeginPause("failed", "clock_reversed", now, changeSpeed); return; }
        if (hours > lastHours) { lastHours = hours; lastProgress = now; }
        if(r.HasBuildingTargets) {
            string? failure=BuildingProductionProof.Observe(r,buildingProduction);
            if(failure is not null) {BeginPause("failed",failure,now,changeSpeed);return;}
            if(r.BuildingProductionReached&&!r.MeasureProduction) {BeginPause("completed","building_production_proven",now,changeSpeed);return;}
        }
        if (r.HasProductionTargets) {
            if (!ProductionTargets.Observe(r, production)) { BeginPause("failed", "production_observation_unavailable", now, changeSpeed); return; }
            if (r.ProductionTargetsReached) { BeginPause("completed", "production_targets_reached", now, changeSpeed); return; }
        }
        if (r.StopAtAvailableLogs is { } threshold) {
            if (availableLogs is null || availableLogs < 0) { BeginPause("failed", "stock_observation_unavailable", now, changeSpeed); return; }
            r.ObservedAvailableLogs = availableLogs;
            if (availableLogs >= threshold) {
                r.StockTargetReached = true; BeginPause("completed", "stock_target_reached", now, changeSpeed); return;
            }
        }
        if (hours >= r.TargetGameHours) BeginPause("completed", r.MeasureProduction ? "production_measurement_complete" : r.HasBuildingTargets ? "building_production_time_limit" : r.HasProductionTargets ? "production_time_limit" : "target_reached", now, changeSpeed);
        else if (now - started >= r.MaxRealSeconds) BeginPause("failed", "real_time_limit", now, changeSpeed);
        else if (now - lastProgress >= 120) BeginPause("failed", "simulation_stalled", now, changeSpeed);
    }

    public SimulationRunStatus Cancel(string id, double hours, float speed, double now, Action<int> changeSpeed, bool speedLocked = false, bool constructionGuard = false)
    {
        string reason = constructionGuard ? "construction_guard_changed" : "cancel_requested";
        var r = Inspect(id);
        if (!r.Terminal) {
            if (r.CompletionSpeed == 1) {
                r.CompletionFallbackReason = reason;
                // Cancel can replace a successful completion pause without resetting its deadline.
                if (r.State == "pausing") { finishState = "cancelled"; finishReason = r.Reason = reason; }
            }
            Observe(r, hours, speed, now);
            BeginPause("cancelled", reason, now, changeSpeed, speedLocked);
        }
        return r;
    }

    // An explicit speed call supersedes the run even when its speed value is unchanged.
    public void Interrupt() { if (active is not null) Finish("interrupted", "explicit_speed_change"); }
    // Called on the lock event too: a lock/unlock between updates must still discard the option.
    public void CompletionLocked(double now, Action<int> changeSpeed)
    {
        if (active?.CompletionSpeed != 1) return;
        active.CompletionFallbackReason ??= "game_speed_locked";
        if (IsCompleting) BeginPause("completed", "target_reached", now, changeSpeed, true);
    }

    // Recheck immediately before dispatch, not only when the command was queued.
    public bool PrepareCompletionDispatch(double hours, float speed, double now, Action<int> changeSpeed)
    {
        if (!IsCompleting) return true;
        if (!ValidObservation(hours, speed, now)) {
            BeginPause("failed", "completion_observation_unavailable", phaseStarted, changeSpeed); return false;
        }
        Observe(active!, hours, speed, now);
        if (speed != 0) { Finish("interrupted", "speed_changed"); return false; }
        if (hours < lastHours) { BeginPause("failed", "clock_reversed", now, changeSpeed); return false; }
        if (now - started >= active!.MaxRealSeconds || now - phaseStarted >= 5) {
            BeginPause("failed", "completion_speed_unconfirmed", now, changeSpeed); return false;
        }
        return true;
    }

    public void SpeedCommandFailed(double now, Action<int> changeSpeed)
    {
        if (IsCompleting) BeginPause("failed", "speed_command_failed", now, changeSpeed);
        else SpeedCommandFailed();
    }
    public void SpeedCommandFailed() {
        Finish("failed", "speed_command_failed");
    }
    public void Unload(Action<int> changeSpeed)
    {
        if (active is null) return;
        RequestSpeed(0, changeSpeed);
        if (active is not null) Finish("interrupted", "session_ended_pause_unconfirmed");
    }

    private void BeginPause(string state, string reason, double now, Action<int> changeSpeed, bool speedLocked = false)
    {
        if (active is null) return;
        if (active.State == "pausing") return; // no repeated commands or deadline extension
        if (active.CompletionSpeed == 1 && state != "completed") active.CompletionFallbackReason ??= reason;
        active.State = "pausing"; active.Reason = reason;
        finishState = state; finishReason = reason; phaseStarted = now;
        pauseAfterUnlock = speedLocked;
        if (!speedLocked) RequestSpeed(0, changeSpeed);
    }
    private void RequestSpeed(int speed, Action<int> changeSpeed)
    {
        try { changeSpeed(speed); }
        catch { Finish("failed", "speed_command_failed"); }
    }
    private void Finish(string state, string reason)
    {
        if (active is null) return;
        if (active.CompletionSpeed == 1 && state != "completed") active.CompletionFallbackReason ??= reason;
        bool confirmedPause = active.ObservedSpeed == 0 && (state is "completed" or "cancelled");
        bool confirmedContinuation = state == "completed" && IsCompleting && active.ObservedSpeed == 1;
        if (confirmedPause || confirmedContinuation) {
            active.EffectiveCompletionSpeed = (int)active.ObservedSpeed;
            active.CompletionSpeedConfirmed = true;
        }
        active.State = state; active.Reason = reason; active.Terminal = true; active = null;
    }
    private void Observe(SimulationRunStatus r, double hours, float speed, double now)
    {
        r.ObservedGameHours = hours; r.ObservedSpeed = speed; r.ElapsedGameHours = Math.Max(0, hours - r.StartGameHours);
        r.ElapsedRealSeconds = Math.Max(0, now - started); r.OvershootHours = Math.Max(0, hours - r.TargetGameHours);
        r.TargetReached = hours >= r.TargetGameHours; r.PauseConfirmed = speed == 0;
    }
    private static bool ValidObservation(double hours, float speed, double now) =>
        !double.IsNaN(hours) && !double.IsInfinity(hours) && hours >= 0 &&
        !float.IsNaN(speed) && !float.IsInfinity(speed) && speed >= 0 &&
        !double.IsNaN(now) && !double.IsInfinity(now) && now >= 0;

    private static void ValidateClock(double hours, float speed, double now)
    {
        if (double.IsNaN(hours) || double.IsInfinity(hours) || hours < 0 || float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0 || double.IsNaN(now) || double.IsInfinity(now) || now < 0) throw new InvalidOperationException("invalid_clock");
    }
}
