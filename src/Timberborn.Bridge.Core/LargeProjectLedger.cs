namespace Timberborn.Bridge.Core;

// Only advance can cross the placement boundary. Inspect never places anything.
public sealed class LargeProjectLedger
{
    public sealed class Receipt
    {
        public string ActionId { get; set; } = "";
        public string PlanKey { get; set; } = "";
        public string State { get; set; } = "ready";
        public string Reason { get; set; } = "awaiting_advance";
        public int Current { get; set; }
        public int[] Order { get; set; } = Array.Empty<int>();
        public string[] EntityIds { get; set; } = Array.Empty<string>();
        public string[] PartStates { get; set; } = Array.Empty<string>();
        public bool RegularExecutionAllowed { get; set; }
    }
    private sealed class Entry
    {
        public Receipt Result = new();
        public string Fingerprint = "";
        public double ConfirmationDeadline;
    }
    private readonly Dictionary<string, Entry> entries = new();
    public Receipt? Existing(string id, string fingerprint)
    {
        if (!entries.TryGetValue(id, out var entry)) return null;
        if (entry.Fingerprint != fingerprint) throw new BridgeRejectionException("state_conflict");
        return entry.Result;
    }
    public Receipt Start(string id, string key, string fingerprint, int[] order, string[] entityIds)
    {
        var old = Existing(id, fingerprint); if (old is not null) return old;
        if (entries.Count >= 128 || entries.Values.Any(e => e.Result.State != "completed")) throw new BridgeRejectionException("state_conflict");
        if (!Guid.TryParseExact(id,"D",out var action) || action==Guid.Empty || order.Length is <1 or >32 || entityIds.Length!=order.Length || !order.OrderBy(x=>x).SequenceEqual(Enumerable.Range(0,order.Length)) || entityIds.Distinct().Count()!=entityIds.Length || entityIds.Any(s=>!Guid.TryParseExact(s,"D",out var value)||value==Guid.Empty)) throw new ArgumentException();
        var result = new Receipt { ActionId=id,PlanKey=key,Order=order.ToArray(),EntityIds=entityIds.ToArray(),PartStates=Enumerable.Repeat("pending",order.Length).ToArray() };
        entries.Add(id,new Entry {Result=result,Fingerprint=fingerprint}); return result;
    }
    public Receipt Inspect(string id) => Get(id).Result;
    private Entry Get(string id) => entries.TryGetValue(id,out var entry)?entry:throw new BridgeRejectionException("state_conflict");
    public Receipt Stop(string id)
    {
        var r=Get(id).Result;
        if(r.State is not ("completed" or "stopped" or "unconfirmed")) {r.State="stopped";r.Reason="stop_requested_objects_retained";}
        return r;
    }
    // Observation: missing/pending/construction/finished/mismatch. Placement is never replayed.
    public Receipt Advance(string id,double now,Func<bool> guard,Func<int,string> observe,Func<int,bool> preflight,Action<int> place)
    {
        var e=Get(id); var r=e.Result;
        if(r.State is "stopped" or "unconfirmed" or "completed") return r;
        try
        {
            if(!guard()) {r.State="stopped";r.Reason="guard_changed";return r;}
            int i=r.Order[r.Current];
            if(r.PartStates[i]!="pending")
            {
                var state=observe(i);
                if(state=="mismatch") {r.State="stopped";r.Reason="object_or_access_mismatch";return r;}
                if(state is "missing" or "pending")
                {
                    if(now>=e.ConfirmationDeadline) {r.State="unconfirmed";r.Reason="order_or_access_unconfirmed";}
                    return r;
                }
                if(state=="construction") {r.PartStates[i]="construction";r.State="waiting";r.Reason="awaiting_construction";return r;}
                if(state!="finished") throw new InvalidOperationException();
                r.PartStates[i]="finished";
                if(++r.Current==r.Order.Length) {r.State="completed";r.Reason="all_parts_finished_and_access_observed";return r;}
                i=r.Order[r.Current];
            }
            if(!preflight(i)) {r.State="stopped";r.Reason="native_step_validation_failed";return r;}
            r.PartStates[i]="unconfirmed"; r.State="waiting";r.Reason="awaiting_order_confirmation";e.ConfirmationDeadline=now+5;
            place(i); // Commit the attempt before invoking the game, even if it throws.
        }
        catch {r.State=r.PartStates.Any(s=>s=="unconfirmed")?"unconfirmed":"stopped";r.Reason="operation_failed_no_retry";}
        return r;
    }
}
