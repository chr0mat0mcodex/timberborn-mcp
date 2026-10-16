namespace Timberborn.Bridge.Core;

// Main-thread, session-local ledger. Once accepted, never replay a placement,
// including a throwing placement whose effects are unknown.
public sealed class BuildingProjectController
{
    public const int MaxProjectsPerSession = 1024;
    public sealed class Step
    {
        public string EntityId { get; set; } = "";
        public string Template { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Rotation { get; set; }
        public string State { get; set; } = "pending";
        public string Reason { get; set; } = "not_submitted";
        public string? ExceptionType { get; set; }
    }
    public sealed class Receipt
    {
        public string ActionId { get; set; } = "";
        public string PlanKey { get; set; } = "";
        public string State { get; set; } = "running";
        public string Reason { get; set; } = "";
        public Step[] Steps { get; set; } = Array.Empty<Step>();
        public bool ConstructionPreflightProven { get; set; }
        public InitialStorageConfiguration? InitialConfiguration { get; set; }
        public string[] Limitations { get; set; } = new[] { "development_pilot_only", "construction_preflight_unproven", "no_automatic_retry_or_rollback", "placed_objects_retained", "ledger_lost_on_session_end", "completed_means_all_parts_finished_and_access_observed" };
    }
    private Receipt? receipt;
    private readonly Dictionary<Guid, (string Fingerprint, Receipt Receipt)> ledger = new();
    private readonly int capacity;
    private double deadline;
    private Func<bool>? guard;
    private Action<Step>? place;
    private Func<Step, string>? confirm;
    public BuildingProjectController(int capacity = 1)
    {
        if (capacity is < 1 or > MaxProjectsPerSession) throw new ArgumentOutOfRangeException(nameof(capacity));
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
        // another ID. completed requires every part finished and accessible.
        if (ledger.Count >= capacity || receipt is not null && receipt.State != "completed")
            throw new BridgeRejectionException("state_conflict");
        return null;
    }
    public Receipt Inspect(Guid id) => ledger.TryGetValue(id, out var entry)
        ? entry.Receipt : throw new BridgeRejectionException("state_conflict");
    public Receipt Start(Guid id, string requestFingerprint, string planKey, Step[] steps,
        Func<bool> check, Action<Step> placement, Func<Step, string> confirmation,
        Func<Step, string>? beforePlacement = null) // Legacy callback retained for callers; never used as a per-part barrier.
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
        guard = check; place = placement; confirm = confirmation;
        deadline = 0;
        receipt = new Receipt { ActionId = id.ToString("D"), PlanKey = planKey, Steps = steps };
        ledger.Add(id, (requestFingerprint, receipt));
        return receipt;
    }
    public void Tick(double now)
    {
        if (receipt is null || receipt.State is not ("running" or "ordered")) return;
        try
        {
            if (!guard!()) { Stop("guard_changed"); return; }
            if (receipt.Steps.Any(s => s.State == "pending"))
            {
                deadline = now + 5;
                foreach (var step in receipt.Steps)
                {
                    step.State = "unconfirmed"; step.Reason = "awaiting_order_confirmation";
                    try { place!(step); }
                    catch (BridgeRejectionException ex) { step.State = "rejected"; step.Reason = ex.Code; }
                    catch (Exception ex) when (ex is not OutOfMemoryException) { step.Reason = "placement_failed_no_retry"; step.ExceptionType = ex.GetType().Name; }
                }
                return; // Observe only after the entire submission pass and game initialization.
            }
            foreach (var step in receipt.Steps.Where(s => s.State != "rejected"))
            {
                string evidence = confirm!(step);
                if (evidence is "confirmed" or "finished") { step.State = evidence; step.Reason = evidence; }
                else if (evidence == "mismatch") { step.Reason = "object_mismatch"; Stop("object_mismatch"); }
                else if (step.State == "unconfirmed" && now >= deadline) { step.Reason = "confirmation_timeout"; Stop("confirmation_timeout"); }
            }
            if (receipt.State is "stopped" or "unconfirmed") return;
            if (receipt.Steps.All(s => s.State == "finished")) { receipt.State = "completed"; receipt.Reason = "all_parts_finished_and_access_observed"; }
            else if (receipt.Steps.All(s => s.State is "finished" or "rejected")) { Stop("partial_structure_rejected_parts"); }
            else if (receipt.Steps.All(s => s.State is "confirmed" or "finished" or "rejected")) { receipt.State = "ordered"; receipt.Reason = "awaiting_structure_completion"; }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException) { Stop("observation_failed"); }
    }
    private void Stop(string reason)
    {
        receipt!.State = receipt.Steps.Any(s => s.State == "unconfirmed") ? "unconfirmed" : "stopped";
        receipt.Reason = reason;
    }
}
