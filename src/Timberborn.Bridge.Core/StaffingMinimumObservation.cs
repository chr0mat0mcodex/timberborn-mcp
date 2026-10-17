namespace Timberborn.Bridge.Core;

// A session-local command observation, not a template constant or mutation probe.
public sealed class StaffingMinimumObservation
{
    public int? MinimumWorkers { get; private set; }
    public string Source => MinimumWorkers is null ? "unknown_public_api" : "observed_decrease_boundary";
    public void Observe(int current)
    {
        if (MinimumWorkers is int minimum && current < minimum) MinimumWorkers = null;
    }
    public void DecreaseReturned(int before,int after)
    {
        Observe(after);
        if (before > 0 && before == after) MinimumWorkers = after;
    }
}
