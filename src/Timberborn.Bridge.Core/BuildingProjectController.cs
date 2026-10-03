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
    private readonly Dictionary<Guid, (string Fingerprint, Receipt Receipt)> ledger = new();
    private readonly int capacity;
    private int index;
    private double deadline;
    private Func<bool>? guard;
    private Action<Step>? place;
    private Func<Step, string>? confirm;
    private Func<Step, string>? readiness;
    private double? waitDeadline;
    public BuildingProjectController(int capacity = 1)
    {
        if (capacity is < 1 or > 4) throw new ArgumentOutOfRangeException(nameof(capacity));
        this.capacity = capacity;
    }
    public Receipt? Existing(Guid id, string requestFingerprint)
    {
        if (ledger.TryGetValue(id, out var entry))
        {
            if (entry.Fingerprint != requestFingerprint) throw new BridgeRejectionException("state_conflict");
            return entry.Receipt;
        }
        // A failed or unconfirmed project must be diagnosed, never bypassed with
        // another ID. completed confirms orders/access, not construction finish.
        if (ledger.Count >= capacity || receipt is not null && receipt.State != "completed")
            throw new BridgeRejectionException("state_conflict");
        return null;
    }
    public Receipt Inspect(Guid id) => ledger.TryGetValue(id, out var entry)
        ? entry.Receipt : throw new BridgeRejectionException("state_conflict");
    public Receipt Start(Guid id, string requestFingerprint, string planKey, Step[] steps,
        Func<bool> check, Action<Step> placement, Func<Step, string> confirmation,
        Func<Step, string>? beforePlacement = null)
    {
        var existing = Existing(id, requestFingerprint); if (existing is not null) return existing;
        bool singleStair = steps.Length == 1 && steps[0].Template == "Stairs.Folktails";
        bool stairWithUpperPaths = steps.Length is >= 2 and <= 3 && steps[0].Template == "Stairs.Folktails" &&
            steps.Skip(1).All(s => s.Template == "Path");
        bool flatBuildingProject = steps.Length <= 5 && BuildingProjectPilotPolicy.SupportsTemplate(steps.LastOrDefault()?.Template) &&
            steps.Take(steps.Length - 1).All(s => s.Template == "Path");
        bool platformProject = steps.Select(s => s.Template).SequenceEqual(new[] { "Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path" });
        bool verticalWarehouseProject = steps.Select(s => s.Template).SequenceEqual(new[] { "Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path", "Platform.Folktails", "SmallWarehouse.Folktails" });
        if (steps.Length is < 1 or > 7 || (!singleStair && !stairWithUpperPaths && !flatBuildingProject && !platformProject && !verticalWarehouseProject)) throw new ArgumentException();
        guard = check; place = placement; confirm = confirmation; readiness = beforePlacement;
        index = 0; deadline = 0; waitDeadline = null;
        receipt = new Receipt { ActionId = id.ToString("D"), PlanKey = planKey, Steps = steps };
        ledger.Add(id, (requestFingerprint, receipt));
        return receipt;
    }
    public void Tick(double now)
    {
        if (receipt is null || receipt.State is not ("running" or "waiting")) return;
        var step = receipt.Steps[index];
        try
        {
            if (!guard!()) { Stop("guard_changed"); return; }
            if (step.State == "pending")
            {
                if (readiness is not null)
                {
                    if (waitDeadline.HasValue && now >= waitDeadline.Value) { Stop("construction_phase_timeout"); return; }
                    string ready = readiness(step);
                    if (ready is "construction" or "pause")
                    {
                        waitDeadline ??= now + 300;
                        receipt.State = "waiting";
                        receipt.Reason = ready == "construction" ? "awaiting_construction_finished" : "awaiting_pause";
                        return;
                    }
                    if (ready != "ready") { Stop("dependency_mismatch"); return; }
                    waitDeadline = null;
                    receipt.State = "running";
                    receipt.Reason = "";
                }
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
