namespace Timberborn.Bridge.Core;

// The first dispatch submits every part; subsequent ticks only observe. No observation dispatches.
public sealed class LargeProjectLedger
{
    public const int Capacity = 128;
    public int Count => entries.Count;
    public int Remaining => Capacity - Count;
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
        public string[] PartReasons { get; set; } = Array.Empty<string>();
        public string?[] PartExceptionTypes { get; set; } = Array.Empty<string?>();
        public bool Released { get; set; }
        public bool RegularExecutionAllowed { get; set; }
    }
    private sealed class Entry
    {
        public Receipt Result = new();
        public string Fingerprint = "";
        public double ConfirmationDeadline;
        public bool PlacementAttempted;
    }
    private readonly Dictionary<string, Entry> entries = new();
    public bool BlocksNewProject => entries.Values.Any(e => !e.Result.Released && e.Result.State != "completed" && !(e.Result.State == "stopped" && e.Result.Reason == "stop_requested_objects_retained" && !e.PlacementAttempted));
    public Receipt? Existing(string id, string fingerprint)
    {
        if (!entries.TryGetValue(id, out var entry)) return null;
        if (entry.Fingerprint != fingerprint) throw new BridgeRejectionException("state_conflict");
        return entry.Result;
    }
    public Receipt Start(string id, string key, string fingerprint, int[] order, string[] entityIds)
    {
        var old = Existing(id, fingerprint); if (old is not null) return old;
        if (entries.Count >= Capacity) throw new BridgeRejectionException("large_project_journal_limit");
        if (entries.Values.Any(e =>
            !e.Result.Released && e.Result.State != "completed" &&
            !(e.Result.State == "stopped" && e.Result.Reason == "stop_requested_objects_retained" && !e.PlacementAttempted)))
            throw new BridgeRejectionException("state_conflict");
        if (!Guid.TryParseExact(id,"D",out var action) || action==Guid.Empty || order.Length is <1 or >32 || entityIds.Length!=order.Length || !order.OrderBy(x=>x).SequenceEqual(Enumerable.Range(0,order.Length)) || entityIds.Distinct().Count()!=entityIds.Length || entityIds.Any(s=>!Guid.TryParseExact(s,"D",out var value)||value==Guid.Empty)) throw new ArgumentException();
        var result = new Receipt { ActionId=id,PlanKey=key,Order=order.ToArray(),EntityIds=entityIds.ToArray(),PartStates=Enumerable.Repeat("pending",order.Length).ToArray() };
        result.PartReasons=Enumerable.Repeat("not_submitted",order.Length).ToArray();
        result.PartExceptionTypes=new string?[order.Length];
        entries.Add(id,new Entry {Result=result,Fingerprint=fingerprint}); return result;
    }
    public void Release(string id, string[] observations)
    {
        var r = Get(id).Result;
        if (r.Released) return;
        if (r.State is not ("completed" or "stopped" or "unconfirmed") || observations.Length != r.PartStates.Length ||
            observations.Where((s,i)=>!LargeProjectRecoveryPolicy.Resolved(r.PartStates[i],s)).Any())
            throw new BridgeRejectionException("state_conflict");
        r.Released = true;
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
            if(!e.PlacementAttempted)
            {
                e.PlacementAttempted=true; e.ConfirmationDeadline=now+5;
                foreach(int i in r.Order)
                {
                    r.PartStates[i]="unconfirmed";
                    r.PartReasons[i]="awaiting_order_confirmation";
                    try { place(i); }
                    catch (BridgeRejectionException ex) { r.PartStates[i]="rejected";r.PartReasons[i]=ex.Code; }
                    catch (Exception ex) when(ex is not OutOfMemoryException) { r.PartReasons[i]="placement_failed_no_retry";r.PartExceptionTypes[i]=ex.GetType().Name; }
                }
                r.State="waiting"; r.Reason="awaiting_order_confirmation";
                return r;
            }
            foreach(int i in r.Order)
            {
                if(r.PartStates[i]=="rejected") continue;
                var observed=observe(i);
                r.PartReasons[i]=observed;
                if(observed is "construction" or "construction_unreachable") r.PartStates[i]="construction";
                else if(observed=="finished_access_missing") r.PartStates[i]="awaiting_access";
                else if(observed=="construction_access_unknown") {r.PartStates[i]="construction";r.State="stopped";r.Reason="construction_access_unknown";}
                else if(observed=="finished") r.PartStates[i]="finished";
                else if(observed=="mismatch") { r.State="stopped";r.Reason="object_or_access_mismatch"; }
                else if(observed is "missing" or "pending") {
                    if(now>=e.ConfirmationDeadline) {r.State="unconfirmed";r.Reason="order_or_access_unconfirmed";}
                }
                else throw new InvalidOperationException();
            }
            r.Current=r.PartStates.Count(s=>s=="finished");
            if(r.State is "stopped" or "unconfirmed") return r;
            if(r.PartStates.All(s=>s is "finished" or "rejected") && r.Current!=r.Order.Length) {r.State="stopped";r.Reason="partial_structure_rejected_parts";return r;}
            r.State=r.Current==r.Order.Length?"completed":"waiting";
            r.Reason=r.State=="completed"?"all_parts_finished_and_access_observed":"awaiting_structure_completion";
        }
        catch {r.State=r.PartStates.Any(s=>s=="unconfirmed")?"unconfirmed":"stopped";r.Reason="operation_failed_no_retry";}
        return r;
    }
}
