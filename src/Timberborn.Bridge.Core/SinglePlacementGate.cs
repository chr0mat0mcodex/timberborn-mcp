namespace Timberborn.Bridge.Core;

// Main-thread pilot budget. Consume before validation or mutation; never reset after failure.
public sealed class SinglePlacementGate
{
    private bool consumed;
    public T Execute<T>(Func<T> action)
    {
        if (consumed) throw new InvalidOperationException("placement_session_locked");
        consumed = true;
        return action();
    }
}
