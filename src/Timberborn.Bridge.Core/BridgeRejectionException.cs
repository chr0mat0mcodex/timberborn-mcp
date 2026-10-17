namespace Timberborn.Bridge.Core;

// Only whitelisted failure codes may cross the game/network boundary.
// A code is not automatically proof of non-execution (e.g. project_preview_failed).
// Arbitrary exception messages, paths and game data remain private.
public sealed class BridgeRejectionException : ArgumentException
{
    public string Code { get; }
    public BridgeRejectionException(string code) : base(IsCode(code) ? code : "invalid_rejection_code")
    {
        if (!IsCode(code)) throw new ArgumentException("invalid_rejection_code");
        Code = code;
    }
    public static bool IsCode(string? code) => SimulationStartPolicy.IsPreDispatchCode(code) || LargeProjectPlanningPolicy.IsPreDispatchCode(code) || LargeProjectStartPolicy.IsPreDispatchCode(code) || code is "planting_job_not_found" or "planting_job_parameters_changed" or "planting_requires_pause" or "planting_work_range_limit" or "planting_occupant_limit" or "planting_preview_changed" or "planting_job_limit" or "configuration_not_found" or "configuration_id_used" or "building_unavailable" or "building_proof_journal_gap" or "building_proof_observation_unavailable" or "cursor_ahead" or "forestry_tree_identity_unknown" or "forestry_range_unknown" or "forestry_budget_exceeded" or "unknown_tree_template" or "forestry_tree_changed" or "screenshot_busy" or "screenshot_unavailable" or "screenshot_too_large" or "stale_session" or "template_locked" or
        "project_preview_failed" or "project_not_found" or
        "stale_activity_cursor" or "template_disabled" or "state_conflict" or "building_not_found" or "entity_not_found" or "not_pausable" or
        "finished_building_required" or "unsupported_storage_good" or "not_storage" or "not_farm" or
        "not_manufactory" or "unavailable_recipe" or "not_crop_prioritizer" or "unavailable_crop" or "unsupported_setting" or "run_not_found" or "run_id_used" or "invalid_time_target";
}
