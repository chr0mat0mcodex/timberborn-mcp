namespace Timberborn.McpServer;

public static class SimulationStartOutcome
{
    public static bool Handles(string name) => name is "run_simulation_for" or "run_simulation_until";
    public static string Describe(string code) => "Kein neuer Simulationslauf gestartet. " + (code switch {
        "run_start_speed_locked" => "Das Spiel sperrt die Zeitsteuerung; Sperrzustand klären.",
        "run_start_speed_pending" => "Ein Tempowechsel ist noch ausstehend; pendingSpeed und aktuelles Tempo frisch lesen.",
        "run_start_batch_busy" => "Eine Baucharge besitzt die Zeitsteuerung; ihren Laufstatus prüfen.",
        "run_start_active" => "Ein anderer Simulationslauf ist aktiv; activeRunId lesen und bestehenden Lauf prüfen.",
        "run_start_capacity" => "Das Laufjournal dieser Sitzung ist voll.",
        "run_start_speed_changed" => "Aktuelles Tempo weicht von expectedSpeed ab; inspect_simulation lesen und neu entscheiden.",
        "run_start_stock_unavailable" => "Der für das Vorratsziel erforderliche Holzbestand ist nicht beobachtbar.",
        _ => "Native Startvoraussetzung nicht erfüllt."
    }) + " Kein automatisches Retry.";
}
