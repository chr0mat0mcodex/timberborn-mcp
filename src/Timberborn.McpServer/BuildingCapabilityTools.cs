using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record ProjectModeCapability(string Mode, string Tool, string[] ObjectTemplates,
    int[] Rotations, int[] UpperPathCounts, int MinSteps, int MaxSteps, int MaxNewGroundRoads,
    int? MaxSearchWidth, int? MaxSearchHeight, int MaxProjectsPerSession,
    int? MaxWaitRealSeconds, int[] LiveRotations, string EvidenceSource, string EvidenceReference)
{
    public TemplateLiveEvidence[] TemplateEvidence { get; init; } = [];
    public string TemplateSelection { get; init; } = "fixed_template_list";
    public string EntranceModel { get; init; } = "public_block_object_spec_single_entrance";
}

public sealed record TemplateLiveEvidence(string Template, int[] LiveRotations, string EvidenceSource, string EvidenceReference);

public sealed record BuildingCapabilityReport(string BridgeVersion, string Faction, string ProfileState,
    int? MaxObjectFootprintCells,
    bool ServerBuildingPlacementEnabled, string BridgePlacementGate, bool SiteValidationPerformed,
    bool RegularExecutionAllowed, bool ConstructionPreflightProven,
    ProjectModeCapability[] Modes, string[] RequiredChecks, string[] Limitations);

public static class BuildingCapabilityTools
{
    public static Tool Reader()
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        return new Tool {
            Name = "inspect_building_capabilities",
            Description = "Liest Bridge-Version und Fraktion und ordnet den versionierten MCP-Projektumfang zu: Vorlagen, Modi, Drehungen, Schritt-/Weg-/Sitzungsgrenzen und dokumentierte begrenzte Live-Nachweise. Frische session erforderlich. Rein lesend, keine Spielvorschau oder Platzierung. profileState=unknown bedeutet kein passendes Profil; keine Fähigkeiten erfinden. Server-Schreibschalter separat, Bridge-Schreibfreigabe nicht beobachtet. Live-Nachweise sind Projekthistorie, keine Prüfung des aktuellen Bauplatzes. Freischaltung/Kosten über inspect_build_options prüfen. regularExecutionAllowed=false und constructionPreflightProven=false: keine allgemeine Sicherheitsfreigabe. Nur Projektpiloten, kein Ersatz für den allgemeinen Gebäudekatalog.",
            InputSchema = JsonSerializer.SerializeToElement(new JsonObject {
                ["type"] = "object", ["properties"] = new JsonObject {
                    ["session"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" } },
                ["required"] = new JsonArray("session"), ["additionalProperties"] = false }),
            OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<BuildingCapabilityReport>))),
            Annotations = new() { ReadOnlyHint = true, DestructiveHint = false, IdempotentHint = true, OpenWorldHint = false } };
    }

    public static string ParseSession(JsonElement args)
    {
        if (args.ValueKind != JsonValueKind.Object) throw new ArgumentException();
        var fields = args.EnumerateObject().ToArray();
        if (fields.Length != 1 || fields[0].Name != "session" || fields[0].Value.ValueKind != JsonValueKind.String ||
            !Guid.TryParseExact(fields[0].Value.GetString(), "D", out var id) || id == Guid.Empty)
            throw new ArgumentException();
        return id.ToString("D");
    }

    public static BuildingCapabilityReport Describe(string bridgeVersion, string faction, bool serverEnabled)
    {
        // Exact profile binding: new deployments require explicit review, never a >= version guess.
        bool generic = bridgeVersion is ("0.35.0" or "0.35.1" or "0.35.2" or "0.35.3");
        bool known = bridgeVersion is ("0.33.0" or "0.33.1" or "0.34.0") && faction == "Folktails" ||
            generic && faction is ("Folktails" or "IronTeeth");
        bool lodge = bridgeVersion == "0.34.0" || generic;
        int flatCapacity = bridgeVersion is ("0.35.2" or "0.35.3") ? BuildingProjectController.MaxProjectsPerSession : 4;
        int verticalCapacity = bridgeVersion is ("0.35.2" or "0.35.3") ? VerticalStairRequest.MaxProjectsPerSession :
            bridgeVersion is ("0.33.1" or "0.34.0" or "0.35.0" or "0.35.1") ? 4 : 1;
        ProjectModeCapability[] modes = known ? [
            new("development_pilot", "execute_building_project_pilot",
                generic ? [] : lodge ? ["Path", "SmallWarehouse.Folktails", "MediumWarehouse.Folktails", "Lodge.Folktails"] :
                    ["Path", "SmallWarehouse.Folktails", "MediumWarehouse.Folktails"],
                [0, 1, 2, 3], [], 1, 5, BuildingProjectPilotPolicy.MaxNewRoads, 8, 8, flatCapacity, null, lodge ? [] : [0, 1, 2, 3],
                generic || lodge ? "mixed_template_coverage_see_template_evidence" : "documented_bounded_live_cases_not_current_site_validation",
                generic ? "docs/generic-building-project.md" : "docs/medium-warehouse-pilot.md;docs/building-rotation-pilot.md") {
                    TemplateEvidence = faction == "Folktails" ? FlatEvidence(lodge, generic) : [],
                    TemplateSelection = generic ? "native_catalog_supported_geometry_with_road_entrance" : "fixed_template_list"
                },
            new("single_stair_pilot", "execute_vertical_stair_pilot", ["Stairs.Folktails"],
                [0, 1, 2, 3], [0], 1, 1, 0, null, null, verticalCapacity, 300, [],
                "live_rotation_coverage_not_catalogued", ""),
            new("stair_with_upper_paths_pilot", "execute_vertical_stair_pilot", ["Stairs.Folktails", "Path"],
                [0, 1, 2, 3], [1, 2], 2, 3, 0, null, null, verticalCapacity, 300, [],
                "live_rotation_coverage_not_catalogued", ""),
            new("stair_platform_pilot", "execute_vertical_stair_pilot", ["Stairs.Folktails", "Platform.Folktails", "Path"],
                [0, 1, 2, 3], [2], 5, 5, 0, null, null, verticalCapacity, 300, [3],
                "documented_bounded_live_cases_not_current_site_validation", "docs/vertical-platform-pilot.md;docs/construction-isolation.md"),
            new("stair_platform_warehouse_pilot", "execute_vertical_stair_pilot",
                ["Stairs.Folktails", "Platform.Folktails", "Path", "SmallWarehouse.Folktails"],
                [0, 1, 2, 3], [2], 7, 7, 0, null, null, verticalCapacity, 300, [0, 1, 2, 3],
                "documented_bounded_live_cases_not_current_site_validation", "docs/vertical-warehouse-pilot.md")
        ] : [];
        if (faction != "Folktails") modes = modes.Where(m => m.Tool != "execute_vertical_stair_pilot").ToArray();
        return new(bridgeVersion, faction, known ? "known" : "unknown", known ? 64 : null, serverEnabled, "not_observed",
            false, false, false, modes,
            ["fresh_session_and_action_identity", "template_unlock_and_costs_from_build_catalog",
                "native_geometry_and_joint_preview", "preserve_sampled_existing_connections",
                "no_independent_open_construction_sites", "previous_project_completed_and_all_construction_finished",
                "finished_connected_predecessors_before_next_placement",
                "pause_before_placement", "actual_builder_access_after_order", "actual_finished_entrance_after_construction"],
            ["profile_is_server_knowledge_bound_to_observed_bridge_and_faction",
                "mode_rotations_are_request_rotation_stair_rotation_for_vertical_modes",
                "supported_templates_are_not_currently_unlocked_or_affordable_guarantees",
                "generic_flat_templates_require_catalog_geometry_and_entrance_not_historical_live_evidence",
                "only_public_single_entrance_observed_no_multiple_entrance_coverage_claim",
                "server_switch_is_not_bridge_write_gate_or_user_authorization",
                "max_projects_shared_across_vertical_modes_not_per_mode",
                "historical_live_coverage_is_not_all_rotations_in_all_geometries",
                "flat_mode_live_rotations_cover_all_building_templates_use_template_evidence_for_individual_coverage",
                "empty_live_rotations_with_not_catalogued_does_not_mean_never_tested",
                "no_site_validation_preview_or_build_order", "no_general_road_or_builder_preflight_safety",
                "completed_receipt_is_not_finished_construction", "no_delivery_staffing_or_operation_guarantee"]);
    }

    private static TemplateLiveEvidence[] FlatEvidence(bool lodge, bool lodgeProven)
    {
        var evidence = new List<TemplateLiveEvidence> {
            new("SmallWarehouse.Folktails", [0, 1, 2, 3], "documented_bounded_live_cases_not_current_site_validation", "docs/building-rotation-pilot.md"),
            new("MediumWarehouse.Folktails", [0, 1, 2, 3], "documented_bounded_live_cases_not_current_site_validation", "docs/medium-warehouse-pilot.md")
        };
        if (lodge) evidence.Add(new("Lodge.Folktails", lodgeProven ? [3] : [],
            lodgeProven ? "documented_bounded_live_cases_not_current_site_validation" : "not_live_proven", "docs/lodge-project-pilot.md"));
        if (lodgeProven) {
            evidence.Add(new("Bench.Folktails", [1], "documented_bounded_live_cases_not_current_site_validation", "docs/generic-building-project.md"));
            evidence.Add(new("LargePile.Folktails", [1], "documented_bounded_live_cases_not_current_site_validation", "docs/generic-building-project.md"));
        }
        return evidence.ToArray();
    }

    public static async Task<JsonObject> Invoke(NativeClient client, JsonElement args, bool serverEnabled, CancellationToken ct)
    {
        string session = ParseSession(args);
        // Read only the catalogue header/faction, not the full template inventory.
        var query = new NameValueCollection { ["offset"] = "0", ["limit"] = "1" };
        var e = await client.BuildingCatalog(BridgeRequest.Parse("/agent-api/v1/building-catalog", query), ct);
        if (e.SessionId != session) throw new BridgeRejectionException("state_conflict");
        var report = Describe(e.BridgeVersion, e.Data.Faction, serverEnabled);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<BuildingCapabilityReport>(1, "ok", report,
            new("native", false, e.SessionId, e.ObservedAtUtc), null), NativeJson.Options)!;
    }
}
