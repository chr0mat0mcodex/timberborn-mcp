namespace Timberborn.Bridge.Core;

public sealed class MainThreadQueue : IDisposable
{
    private sealed class Pending
    {
        public BridgeRequest Request { get; }
        public CancellationToken Token { get; }
        public TaskCompletionSource<string> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Pending(BridgeRequest request, CancellationToken token) { Request = request; Token = token; }
    }
    private readonly object sync = new();
    private readonly Queue<Pending> pending = new();
    private bool stopped;
    public async Task<string> Enqueue(BridgeRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var work = new Pending(request, ct);
        lock (sync)
        {
            if (stopped) throw new InvalidOperationException("session_closed");
            if (pending.Count >= 8) throw new InvalidOperationException("busy");
            pending.Enqueue(work);
        }
        using (ct.Register(() => work.Completion.TrySetCanceled()))
            return await work.Completion.Task.ConfigureAwait(false);
    }
    // Called only by the game's main-thread lifecycle hook. No game callbacks elsewhere.
    public void Pump(Func<BridgeRequest, string> observe)
    {
        lock (sync)
        {
            if (stopped || pending.Count == 0) return;
            var work = pending.Dequeue();
            if (work.Token.IsCancellationRequested) { work.Completion.TrySetCanceled(); return; }
            try { work.Completion.TrySetResult(observe(work.Request)); }
            catch (ArgumentException) { work.Completion.TrySetException(new ArgumentException("invalid_region")); }
            catch (Exception) { work.Completion.TrySetException(new InvalidOperationException("observation_failed")); }
        }
    }
    public void Dispose()
    {
        lock (sync)
        {
            stopped = true;
            while (pending.Count > 0) pending.Dequeue().Completion.TrySetCanceled();
        }
    }
}
