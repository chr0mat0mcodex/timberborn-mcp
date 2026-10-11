namespace Timberborn.Backend.Native;

// Per async invocation, not per client: concurrent calls cannot contaminate each other.
// Activity logging is outside the scope. A dispatch attempt is not proof of delivery.
public sealed class NativeDispatchTrace : IDisposable
{
    private static readonly AsyncLocal<NativeDispatchTrace?> Current = new();
    private readonly NativeDispatchTrace? parent;
    private int writeAttempts;
    public int WriteAttempts => Volatile.Read(ref writeAttempts);
    public NativeDispatchTrace() { parent=Current.Value;Current.Value=this; }
    internal static void Dispatch(HttpMethod method) {
        if(method==HttpMethod.Get||method==HttpMethod.Head)return;
        for(var scope=Current.Value;scope is not null;scope=scope.parent)
            Interlocked.Increment(ref scope.writeAttempts);
    }
    public void Dispose() => Current.Value=parent;
}
