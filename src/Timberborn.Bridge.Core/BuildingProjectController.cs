namespace Timberborn.Bridge.Core;

// Main-thread, session-local ledger. Once accepted, never replay a placement,
// including a throwing placement whose effects are unknown.
public sealed class BuildingProjectController
{
    public sealed class Step
    {
        public string EntityId { get; set; } = "";
        public string Template { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Rotation { get; set; }
        public string State { get; set; } = "pending";
    }
    public sealed class Receipt
    {
        public string ActionId { get; set; } = "";
        public string PlanKey { get; set; } = "";
        public string State { get; set; } = "running";
        public string Reason { get; set; } = "";
        public Step[] Steps { get; set; } = Array.Empty<Step>();
        public bool ConstructionPreflightProven { get; set; }
        public string[] Limitations { get; set; } = new[] { "development_pilot_only", "construction_preflight_unproven", "no_automatic_retry_or_rollback", "placed_objects_retained", "ledger_lost_on_session_end", "completed_means_order_and_access_confirmed_not_construction_finished" };
    }
    private Receipt? receipt;
    private string fingerprint = "";
    private int index;
    private double deadline;
    private Func<bool>? guard;
    private Action<Step>? place;
    private Func<Step, string>? confirm;
    public Receipt? Existing(Guid id, string requestFingerprint)
    {
        if (receipt is null) return null;
        if (receipt.ActionId != id.ToString("D") || fingerprint != requestFingerprint)
            throw new BridgeRejectionException("state_conflict");
        return receipt;
    }
    public Receipt Inspect(Guid id) => receipt is not null && receipt.ActionId == id.ToString("D")
        ? receipt : throw new BridgeRejectionException("state_conflict");
    public Receipt Start(Guid id, string requestFingerprint, string planKey, Step[] steps,
        Func<bool> check, Action<Step> placement, Func<Step, string> confirmation)
    {
        var existing = Existing(id, requestFingerprint); if (existing is not null) return existing;
        bool singleStair = steps.Length == 1 && steps[0].Template == "Stairs.Folktails";
        bool stairWithUpperPaths = steps.Length is >= 2 and <= 3 && steps[0].Template == "Stairs.Folktails" &&
            steps.Skip(1).All(s => s.Template == "Path");
        bool warehouseProject = steps.LastOrDefault()?.Template == "SmallWarehouse.Folktails" &&
            steps.Take(steps.Length - 1).All(s => s.Template == "Path");
        if (steps.Length is < 1 or > 5 || (!singleStair && !stairWithUpperPaths && !warehouseProject)) throw new ArgumentException();
        fingerprint = requestFingerprint; guard = check; place = placement; confirm = confirmation;
        return receipt = new Receipt { ActionId = id.ToString("D"), PlanKey = planKey, Steps = steps };
    }
    public void Tick(double now)
    {
        if (receipt is null || receipt.State != "running") return;
        var step = receipt.Steps[index];
        try
        {
            if (!guard!()) { Stop("guard_changed"); return; }
            if (step.State == "pending")
            {
                step.State = "unconfirmed"; // Commit before crossing the game mutation boundary.
                deadline = now + 5;
                place!(step);
                return; // Never place and confirm in the same frame.
            }
            string evidence = confirm!(step);
            if (evidence == "confirmed")
            {
                step.State = "confirmed";
                if (++index == receipt.Steps.Length) { receipt.State = "completed"; receipt.Reason = "order_and_access_confirmed"; }
            }
            else if (evidence == "mismatch" || now >= deadline) Stop(evidence == "mismatch" ? "object_mismatch" : "confirmation_timeout");
        }
        catch { Stop("operation_unconfirmed"); }
    }
    private void Stop(string reason)
    {
        receipt!.State = receipt.Steps.Any(s => s.State == "unconfirmed") ? "unconfirmed" : "stopped";
        receipt.Reason = reason;
    }
}
