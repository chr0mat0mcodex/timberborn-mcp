using Timberborn.Contracts;

namespace Timberborn.Backend.Abstractions;

public interface ITimberbornReadBackend
{
    string Id { get; }
    bool Simulated { get; }
    Task PingAsync(CancellationToken ct);
    Task<GameInfo> GetGameInfoAsync(CancellationToken ct);
    Task<LiveSnapshot> GetLiveAsync(CancellationToken ct);
    Task<IReadOnlyList<CharacterSummary>> GetPopulationAsync(CancellationToken ct);
    Task<IReadOnlyList<BuildingSummary>> GetBuildingsAsync(CancellationToken ct);
    Task<BuildingSummary> GetBuildingAsync(Guid id, CancellationToken ct);
}

public sealed class BackendException(Fault fault) : Exception(fault.Message)
{
    public Fault Fault { get; } = fault;
}

public static class Faults
{
    public static Fault Create(string code) => code switch
    {
        "invalid_argument" => new(code, "Ungültige Werkzeugparameter.", false, "Parameter und Wertebereich prüfen."),
        "writes_disabled" => new(code, "Schreibzugriff ist deaktiviert.", false, "Explizite Schreibfreigabe erforderlich."),
        "not_pausable" => new(code, "Pausierbarkeit oder Pausenstatus nicht bestätigt.", false, "Gebäude erneut prüfen."),
        "state_conflict" => new(code, "Pausenstatus entspricht nicht dem erwarteten Ausgangszustand.", false, "Neu lesen und Auftrag prüfen."),
        "action_unconfirmed" => new(code, "Änderung möglicherweise ausgeführt; Ergebnis nicht bestätigt.", false, "Nur lesend klären; nicht automatisch wiederholen oder zurücksetzen."),
        "cancelled" => new(code, "Auftrag vor dem Schreibversuch abgebrochen.", false, "Auftrag bei Bedarf neu prüfen."),
        "backend_unavailable" => new(code, "Spiel-API nicht erreichbar.", true, "Spiel, API-Start und localhost-Port prüfen."),
        "authentication_failed" => new(code, "Spiel-API verweigert den Zugriff.", false, "Lokale Zugangskonfiguration prüfen."),
        "timeout" => new(code, "Zeitlimit der Abfrage erreicht.", true, "Später erneut abfragen."),
        "capability_unavailable" => new(code, "Diese Leseroute ist nicht verfügbar.", false, "Backend-Version prüfen."),
        "entity_not_found" => new(code, "Gebäude ist nicht mehr vorhanden.", false, "Gebäudesuche wiederholen."),
        "entity_type_mismatch" => new(code, "Entity ist nicht als Gebäude bestätigt.", false, "Eine ID aus find_buildings verwenden."),
        "backend_incompatible" => new(code, "Antwort entspricht nicht dem erwarteten Datenschema.", false, "Mod-Version und Mapping prüfen."),
        "response_too_large" => new(code, "Antwort überschreitet das Größenlimit.", false, "Abfrageumfang oder konfigurierte Limits prüfen."),
        "backend_error" => new(code, "Spiel-API meldet einen Fehler.", false, "Lokale API-Diagnose prüfen."),
        _ => new("internal_error", "Interner Serverfehler.", false, "Serverdiagnose prüfen.")
    };
    public static BackendException Exception(string code) => new(Create(code));
}
