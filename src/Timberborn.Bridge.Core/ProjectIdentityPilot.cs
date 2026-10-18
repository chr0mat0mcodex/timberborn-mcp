namespace Timberborn.Bridge.Core;

// Session-local observation override only. Never owns or edits a project ledger/entity.
public sealed class ProjectIdentityPilot
{
    public string Session { get; private set; } = "";
    public string ActionId { get; private set; } = "";
    public int PartIndex { get; private set; } = -1;
    public bool Active => PartIndex >= 0;

    public static bool MatchingIdentity(string observed) => observed is
        "construction" or "construction_unreachable" or "construction_access_unknown" or
        "finished" or "finished_access_missing" or "district_unavailable";

    public void Enable(string session, string actionId, int index, string recorded,
        string observed, bool terminal, bool released, bool idle)
    {
        if (!Guid.TryParseExact(session, "D", out var s) || s == Guid.Empty ||
            !Guid.TryParseExact(actionId, "D", out var a) || a == Guid.Empty || index is < 0 or >= 32)
            throw new ArgumentException("invalid_identity_pilot_target");
        if (!terminal || released || !idle ||
            recorded is not ("construction" or "awaiting_access" or "finished") || !MatchingIdentity(observed) ||
            Active && !Matches(session, actionId, index))
            throw new BridgeRejectionException("state_conflict");
        Session = session; ActionId = actionId; PartIndex = index;
    }

    public bool Matches(string session, string actionId, int index) =>
        Active && Session == session && ActionId == actionId && PartIndex == index;

    public string Observe(string session, string actionId, int index, string realObserved) =>
        Matches(session, actionId, index) ? "identity_mismatch" : realObserved;

    public void Disable(string session, string actionId, int index)
    {
        if (Active && !Matches(session, actionId, index)) throw new BridgeRejectionException("state_conflict");
        Clear();
    }

    public void Clear() { Session = ""; ActionId = ""; PartIndex = -1; }
}
