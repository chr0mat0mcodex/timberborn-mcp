namespace Timberborn.Bridge.Core;

// SpeedManager applies changes after the bridge update. Coalesce intentions before
// dispatch and never observe completion in the update that dispatched a command.
public sealed class DeferredSpeedCommand
{
    private int? pending;
    private int? submitted;
    public bool HasPending => pending.HasValue;
    public int? PendingSpeed => pending ?? submitted;
    public void ObserveNextUpdate() => submitted = null;
    public void Request(int speed) => pending = speed;
    public bool Apply(Action<int> changeSpeed)
    {
        if (pending is not { } value) return false;
        pending = null; // A throwing command is never replayed.
        submitted = value;
        changeSpeed(value);
        return true;
    }
    public void Clear() { pending = null; submitted = null; }
}
