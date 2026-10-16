namespace Timberborn.McpServer;

public static class ProjectPlanningOutcome
{
    public static bool Handles(string name) => name is "plan_large_project" or "plan_large_project_access" or
        "plan_large_project_repair" or "start_large_project" or "run_large_project" or "run_large_project_repair" or "execute_vertical_stair_pilot";

    public static string Describe(string? code) => "Vorprüfung abgewiesen; kein neuer Bauauftrag durch diesen Aufruf. " + (code switch {
        "project_plan_requires_pause" => "Bauplanung benötigt eine pausierte Simulation.",
        "project_plan_faulted" => "Planer nach einem Vorschaufehler gesperrt; Diagnose prüfen.",
        "project_plan_busy" => "Eine andere Großprojektcharge läuft noch.",
        "project_plan_foreign_construction" => "Fremde Baustellen verhindern die isolierte Bauprüfung.",
        "project_plan_construction_inventory_incomplete" => "Objektbestand für die Baustellenprüfung unvollständig.",
        "project_plan_construction_initialization_pending" => "Mindestens ein Objekt ist noch nicht initialisiert.",
        "project_plan_construction_state_unknown" => "Mindestens ein Objekt hat keinen eindeutigen Fertig-/Baustellenzustand.",
        "project_plan_district_unavailable" => "Angegebener Distrikt fehlt oder ist nicht fertig verfügbar.",
        "project_plan_navigation_access_limit" => "Mehr als 4.096 Zugangsmesspunkte; Navigationsprüfung nicht vollständig.",
        "project_plan_navigation_comparison_limit" => "Mehr als 16.384 Distriktvergleiche; Navigationsprüfung nicht vollständig.",
        "project_plan_navigation_no_probes" => "Keine Zugangsmesspunkte im Ausgangszustand verfügbar.",
        "project_plan_navigation_district_coverage_unknown" => "Navigationsprüfung benötigt 1 bis 16 fertige Distrikte.",
        "project_plan_navigation_preview_baseline_mismatch" => "Realer Wegzustand und Vorschau-Ausgangszustand stimmen nicht überein.",
        "project_plan_navigation_no_connected_baseline" => "Kein verbundener Zugang im Ausgangszustand nachgewiesen.",
        _ => "Navigations-Ausgangszustand nicht vollständig verfügbar."
    });
}
