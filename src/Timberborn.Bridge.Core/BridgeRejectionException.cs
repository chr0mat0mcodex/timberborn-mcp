namespace Timberborn.Bridge.Core;

// Only explicit precondition failures may cross the game/network boundary.
// Arbitrary exception messages, paths and game data remain private.
public sealed class BridgeRejectionException : ArgumentException
{
    public string Code { get; }
    public BridgeRejectionException(string code) : base(IsCode(code) ? code : "invalid_rejection_code")
    {
        if (!IsCode(code)) throw new ArgumentException("invalid_rejection_code");
        Code = code;
    }
    public static bool IsCode(string? code) => code is "stale_session" or "template_locked" or
        "template_disabled" or "state_conflict" or "building_not_found" or "entity_not_found" or "not_pausable" or
        "finished_building_required" or "unsupported_storage_good" or "not_storage" or "not_farm" or
        "not_crop_prioritizer" or "unavailable_crop" or "unsupported_setting";
}
