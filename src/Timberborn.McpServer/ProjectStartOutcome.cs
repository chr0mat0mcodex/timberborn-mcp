namespace Timberborn.McpServer;

public static class ProjectStartOutcome
{
    public static string Describe(string? code) => "Kein neuer Bauauftrag durch diesen Start. " + (code switch {
        "project_start_plan_changed" => "Plan oder Ausgangszustand verändert; frisch prüfen und neu planen.",
        "project_start_ineligible" => "Frische Bauprüfung erlaubt den Plan nicht.",
        "project_start_busy" => "Eine andere Großprojektcharge läuft noch.",
        "project_start_capacity" => "Projektlimit dieser Sitzung erreicht.",
        "project_start_identity_conflict" => "Kennung gehört bereits zu einem anderen Projekt; vorhandenen Beleg lesen.",
        "project_start_entity_conflict" => "Vorgesehene Bauteilkennung ist bereits belegt; Bestand prüfen.",
        "project_start_unresolved" or "repair_other_project_unresolved" => "Ein anderes Projekt ist noch nicht geklärt und freigegeben.",
        "project_start_simulation_unavailable" => "Simulation belegt, nicht pausiert oder vom Spiel gesperrt.",
        "project_start_baseline_unavailable" => "Aktueller Wegvergleich nicht vollständig verfügbar.",
        "repair_parent_ineligible" => "Ursprungsprojekt fehlt oder ist derzeit nicht reparierbar; Diagnose lesen.",
        "repair_district_mismatch" => "Reparatur und Ursprungsprojekt gehören nicht zum selben Distrikt.",
        "repair_helpers_only" => "Reparatur darf nur Wege, Treppen und Plattformen ergänzen.",
        "repair_nested" => "Eine Reparatur kann nicht selbst zum Reparatur-Ursprungsprojekt werden.",
        _ => "Native Vorprüfung widerspricht dem Start; Pause, Baustellen und Vorschauzustand prüfen."
    });
}
