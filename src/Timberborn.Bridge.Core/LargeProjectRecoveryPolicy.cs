namespace Timberborn.Bridge.Core;

// Fresh identity observations, never a retry authorization or inferred placement success.
public static class LargeProjectRecoveryPolicy
{
    public static string ConstructionReason(bool? manuallyPaused,bool? isOn,bool? materialsReady,bool unfinishedSupport)
        => manuallyPaused == true ? "construction_paused"
        : isOn == false && unfinishedSupport ? "waiting_for_support_completion"
        : isOn == false ? "construction_inactive_unknown"
        : materialsReady == false ? "materials_not_ready"
        : "awaiting_builders_or_work";

    // Retain a positive identity observation even after a terminal stop. This is
    // observation bookkeeping only: it never dispatches, resumes or finishes a job.
    public static string ConfirmIdentity(string recorded, string observed) => recorded == "unconfirmed" ? observed switch {
        "construction" or "construction_unreachable" or "construction_access_unknown" => "construction",
        "finished_access_missing" or "district_unavailable" => "awaiting_access",
        "finished" => "finished",
        _ => recorded
    } : recorded;

    public static bool Resolved(string recorded, string observed) =>
        recorded is "pending" or "rejected" ? observed == "missing" :
        observed == "finished" || observed == "missing" && recorded is "construction" or "finished" or "awaiting_access";

    public static bool Repairable(string recorded, string observed) =>
        Resolved(recorded, observed) || recorded is not ("pending" or "rejected") &&
        observed is "construction" or "construction_unreachable" or "finished_access_missing";

    public static bool AccessHelper(string template) => template is "Path" or
        "Stairs.Folktails" or "Platform.Folktails" or "DoublePlatform.Folktails" or "TriplePlatform.Folktails" or
        "Stairs.IronTeeth" or "Platform.IronTeeth" or "DoublePlatform.IronTeeth" or "TriplePlatform.IronTeeth";
}
