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
    private readonly HashSet<Pending> active = new();
    public async Task<string> Enqueue(BridgeRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var work = new Pending(request, ct);
        lock (sync)
        {
            if (stopped) throw new InvalidOperationException("session_closed");
            if (pending.Count + active.Count >= 8) throw new InvalidOperationException("busy");
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
            catch (BridgeRejectionException ex) { work.Completion.TrySetException(new BridgeRejectionException(ex.Code)); }
            catch (ArgumentException) { work.Completion.TrySetException(new ArgumentException("invalid_region")); }
            catch (Exception) { work.Completion.TrySetException(new InvalidOperationException("observation_failed")); }
        }
    }
    // Start on the game thread; frame-dependent observations complete without blocking it.
    public void PumpAsync(Func<BridgeRequest, Task<string>> observe) {
        Pending work;
        lock(sync) {
            if(stopped || pending.Count==0) return;
            work=pending.Dequeue(); active.Add(work);
        }
        if(work.Token.IsCancellationRequested) { lock(sync) active.Remove(work); work.Completion.TrySetCanceled(); return; }
        _ = Complete(work,observe);
    }
    private async Task Complete(Pending work,Func<BridgeRequest,Task<string>> observe) {
        try { work.Completion.TrySetResult(await observe(work.Request).ConfigureAwait(false)); }
        catch(BridgeRejectionException ex) { work.Completion.TrySetException(new BridgeRejectionException(ex.Code)); }
        catch(ArgumentException) { work.Completion.TrySetException(new ArgumentException("invalid_request")); }
        catch(Exception) { work.Completion.TrySetException(new InvalidOperationException("observation_failed")); }
        finally { lock(sync) active.Remove(work); }
    }
    public void Dispose()
    {
        lock (sync)
        {
            stopped = true;
            foreach(var work in active) work.Completion.TrySetCanceled();
            active.Clear();
            while (pending.Count > 0) pending.Dequeue().Completion.TrySetCanceled();
        }
    }
}
