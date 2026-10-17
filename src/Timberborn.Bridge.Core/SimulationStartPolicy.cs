namespace Timberborn.Bridge.Core;

// These codes are emitted only before registering a new run or changing speed.
public static class SimulationStartPolicy
{
    public static bool IsPreDispatchCode(string? code) => code is
        "run_start_speed_locked" or "run_start_speed_pending" or "run_start_batch_busy" or
        "run_start_active" or "run_start_capacity" or "run_start_speed_changed" or "run_start_stock_unavailable";

    public static void RequireAvailable(bool speedLocked, bool hasBatchOwner, bool speedPending)
    {
        if(speedLocked) throw new BridgeRejectionException("run_start_speed_locked");
        if(hasBatchOwner) throw new BridgeRejectionException("run_start_batch_busy");
        if(speedPending) throw new BridgeRejectionException("run_start_speed_pending");
    }
}
