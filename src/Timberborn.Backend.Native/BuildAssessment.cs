namespace Timberborn.Backend.Native;

public sealed record BuildCheck(string Name, string Status, string Phase, string Source, string Reason);
public sealed record BuildAssessment(string Decision, bool RegularExecutionAllowed, string Scope,
    BuildCheck[] Checks, NativeRoadAffected[] Affected, bool AffectedTruncated, string[] Limitations)
{
    // A derived explanation of validated evidence, not another game observation or a build permit.
    public static BuildAssessment FromValidation(NativeValidation d) => Create(d.Valid,
        d.NoPersistentChangeObserved, d.SessionLocked, d.RoadProtection, null, false);
    public static BuildAssessment FromProject(NativeProjectValidation d) => Create(
        d.BuildingValid && d.RoadValid.All(valid => valid), d.NoPersistentChangeObserved,
        d.SessionLocked, d.RoadProtection, d.PreviewEntranceConnected,
        d.RoadStepLostConnections.Any(lost => lost > 0));

    private static BuildAssessment Create(bool? geometry, bool unchanged, bool locked,
        NativeRoadProtection? roads, bool? entrance, bool prefixLoss)
    {
        string Result(bool? value) => value.HasValue ? value.Value ? "passed" : "failed" : "unknown";
        bool baseline = roads is { Restored: true, ConnectedBefore: > 0 } &&
            (roads.Status == "safe" || roads.Status == "unknown" &&
                roads.Reasons.SequenceEqual(new[] { "construction_and_road_node_coverage_unproven" }));
        bool lost = prefixLoss || roads is { LostConnections: > 0 };
        bool constructionLoss = roads?.ConstructionAccessPreview is { Status: "observed", LostSites: > 0 };
        var checks = new[] {
            new BuildCheck("placement", Result(geometry), "preview", "native_object_and_service_validation",
                geometry.HasValue ? geometry.Value ? "placement_valid_in_preview" : "placement_invalid_in_preview" : "placement_evidence_unavailable"),
            new BuildCheck("existingDistrictConnections", lost ? "failed" : baseline ? "passed" : "unknown",
                "finished_preview_and_road_prefixes", "native_district_membership_comparison",
                prefixLoss ? "existing_connection_lost_in_road_prefix" : lost ? "existing_district_connection_lost" :
                    baseline ? "no_loss_in_sampled_district_connections" : "baseline_or_preview_evidence_unavailable"),
            new BuildCheck("existingConstructionAccess", constructionLoss ? "failed" : "unknown",
                "existing_sites_under_finished_preview", roads?.ConstructionAccessPreview is null ? "not_proven" : "native_construction_range_diagnostic",
                constructionLoss ? "existing_site_range_access_lost_in_preview" : "full_builder_access_preservation_not_proven"),
            new BuildCheck("candidateEntrance", Result(entrance), "finished_preview", entrance.HasValue ? "native_joint_project_preview" : "not_observed",
                entrance.HasValue ? entrance.Value ? "entrance_connected_in_preview" : "entrance_not_connected_in_preview" : "single_object_validation_has_no_entrance_connection_evidence"),
            new BuildCheck("newConstructionAccess", "unknown", "construction", "not_observed",
                "new_site_builder_reachability_not_proven"),
            new BuildCheck("newFinishedAccess", "unknown", "actual_finished_building", "not_observed",
                "preview_is_not_a_finished_building_access_observation"),
            new BuildCheck("previewRestoration", locked || !unchanged ? "failed" : roads is null ? "unknown" : Result(roads.Restored),
                "after_preview_cleanup", "native_entities_stock_and_navigation_comparison",
                locked || !unchanged ? "validation_session_locked_or_state_changed" : roads is null ? "navigation_restoration_evidence_unavailable" :
                    roads.Restored ? "checked_state_restored" : "navigation_restoration_not_confirmed")
        };
        return new(checks.Any(c => c.Status == "failed") ? "blocked" : "unknown", false,
            "registered_district_probes_in_finished_preview_not_complete_construction_safety",
            checks, roads?.Affected ?? [], roads?.AffectedTruncated ?? false,
            ["derived_from_validated_native_evidence", "no_new_game_probe_or_permission",
             "unknown_is_not_safe_or_unreachable", "preview_not_actual_construction_or_finished_access",
             "no_delivery_staffing_or_operation_guarantee", "no_complete_disconnected_network_coverage"]);
    }
}
