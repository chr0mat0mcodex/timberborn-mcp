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
    public float ObservedSpeed { get; set; }
    public bool PauseConfirmed { get; set; }
    public bool TargetReached { get; set; }
    public bool Terminal { get; set; }
}

public sealed class SimulationRunController
{
    public const int Capacity = 128;
    private readonly Dictionary<string, SimulationRunStatus> history = new();
    private SimulationRunStatus? active;
    private double started, phaseStarted, lastProgress, lastHours;
    private float initialSpeed;
    private bool pauseAfterUnlock, wasLocked;
    private string finishState = "completed", finishReason = "target_reached";

    public SimulationRunStatus Inspect(string id) => history.TryGetValue(id, out var r) ? r : throw new BridgeRejectionException("run_not_found");

    public SimulationRunStatus Start(SimulationRunRequest r, double hours, float speed, double now, Action<int> changeSpeed)
    {
        if (!r.Starts) throw new ArgumentException();
        ValidateClock(hours, speed, now);
        if (history.ContainsKey(r.RunId)) throw new BridgeRejectionException("run_id_used");
        if (active is not null || history.Count >= Capacity || speed != r.ExpectedSpeed) throw new BridgeRejectionException("state_conflict");
        double target = r.Route == "simulation-run-for" ? hours + r.DurationHours : r.TargetHours;
        if (target < hours || target - hours > 672) throw new BridgeRejectionException("invalid_time_target");
        active = new SimulationRunStatus { RunId = r.RunId, StartGameHours = hours, TargetGameHours = target,
            ObservedGameHours = hours, RequestedSpeed = r.Speed, ObservedSpeed = speed, MaxRealSeconds = r.MaxRealSeconds };
        history.Add(r.RunId, active);
        started = phaseStarted = lastProgress = now; lastHours = hours; initialSpeed = speed;
        pauseAfterUnlock = wasLocked = false;
        var result = active;
        if (hours >= target) BeginPause("completed", "target_reached", now, changeSpeed);
        else RequestSpeed(r.Speed, changeSpeed);
        Observe(result, hours, speed, now);
        return result;
    }

    public void Tick(double hours, float speed, double now, Action<int> changeSpeed, bool speedLocked = false)
    {
        if (active is null) return;
        ValidateClock(hours, speed, now);
        var r = active; Observe(r, hours, speed, now);
        if (speedLocked) {
            wasLocked = true;
            if (r.State == "pausing") { pauseAfterUnlock = true; return; }
            r.Reason = "game_speed_locked";
            if (hours < lastHours) BeginPause("failed", "clock_reversed", now, changeSpeed, true);
            else if (hours >= r.TargetGameHours) BeginPause("completed", "target_reached", now, changeSpeed, true);
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
                if (speed != 0 && speed != r.RequestedSpeed && speed != initialSpeed) { Finish("interrupted", "speed_changed"); return; }
                pauseAfterUnlock = false; phaseStarted = now;
                RequestSpeed(0, changeSpeed); return; // Confirm on a later unlocked update.
            }
            if (speed == 0) Finish(finishState, finishReason);
            else if (speed != r.RequestedSpeed && speed != initialSpeed) Finish("interrupted", "speed_changed");
            else if (now - phaseStarted >= 5) Finish("failed", "pause_unconfirmed");
            return;
        }
        if (r.State == "starting") {
            if (speed == r.RequestedSpeed) { r.State = "running"; r.Reason = "advancing"; }
            else if (speed != initialSpeed) { Finish("interrupted", "speed_changed"); return; }
            else if (now - phaseStarted >= 5) { BeginPause("failed", "speed_unconfirmed", now, changeSpeed); return; }
        } else if (speed != r.RequestedSpeed) { Finish("interrupted", "speed_changed"); return; }
        if (hours < lastHours) { BeginPause("failed", "clock_reversed", now, changeSpeed); return; }
        if (hours > lastHours) { lastHours = hours; lastProgress = now; }
        if (hours >= r.TargetGameHours) BeginPause("completed", "target_reached", now, changeSpeed);
        else if (now - started >= r.MaxRealSeconds) BeginPause("failed", "real_time_limit", now, changeSpeed);
        else if (now - lastProgress >= 120) BeginPause("failed", "simulation_stalled", now, changeSpeed);
    }

    public SimulationRunStatus Cancel(string id, double hours, float speed, double now, Action<int> changeSpeed, bool speedLocked = false)
    {
        var r = Inspect(id);
        if (!r.Terminal) { Observe(r, hours, speed, now); BeginPause("cancelled", "cancel_requested", now, changeSpeed, speedLocked); }
        return r;
    }

    // An explicit speed call supersedes the run even when its speed value is unchanged.
    public void Interrupt() { if (active is not null) Finish("interrupted", "explicit_speed_change"); }
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
        active.State = state; active.Reason = reason; active.Terminal = true; active = null;
    }
    private void Observe(SimulationRunStatus r, double hours, float speed, double now)
    {
        r.ObservedGameHours = hours; r.ObservedSpeed = speed; r.ElapsedGameHours = Math.Max(0, hours - r.StartGameHours);
        r.ElapsedRealSeconds = Math.Max(0, now - started); r.OvershootHours = Math.Max(0, hours - r.TargetGameHours);
        r.TargetReached = hours >= r.TargetGameHours; r.PauseConfirmed = speed == 0;
    }
    private static void ValidateClock(double hours, float speed, double now)
    {
        if (double.IsNaN(hours) || double.IsInfinity(hours) || hours < 0 || float.IsNaN(speed) || float.IsInfinity(speed) || speed < 0 || double.IsNaN(now) || double.IsInfinity(now) || now < 0) throw new InvalidOperationException("invalid_clock");
    }
}
