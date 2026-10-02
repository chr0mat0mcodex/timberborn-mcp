# Projektstand

Stand: 2026-10-02. **Testsystem; schnelle und effiziente Entwicklung haben Vorrang.**
Kein Erhaltungsziel für Spielstand oder Kolonie. [Direktiven](AGENTS.md), [Einstieg](START_HERE.md).

## Version und Verifikation

| Ebene | Stand |
| --- | --- |
| Code / installiert | Agent Bridge 0.25.0 |
| Automatisch | 648 reguläre Tests: 635 Unit, 13 Integration; drei opt-in Live-Tests übersprungen |
| Mod-Build | Timberborn 1.1.2.4 / Folktails, ohne Warnungen und Fehler |
| Installation | Vorversion gesichert, fünf Paketdateien per Hash geprüft, private Einstellungen erhalten |
| Live 0.25.0 | Tatsächliche Bauprojektausführung noch nicht bestätigt |
| Live zuvor | Gemeinsame Vorschau in 0.24.1, Kandidatensuche in 0.24.0, Zeitläufe in 0.22.0; frühere Basisfunktionen in dokumentierten Piloten |

Arbeitsbranch `codex/road-protection-pilot`; zuletzt geprüftes `origin/master` enthält
0.22.0. Implementierung `8349b80`, Installation `e92713c`, beide gepusht. Spätere
Dokumentationscheckpoints stehen in Git. Das installierte Manifest beweist noch
nicht, welche Bridge-Version in einer laufenden Spielsession geladen ist.

## Architektur

- Eigene `Timberborn.AgentBridge`, Mod-ID `chr0mat0mcodex.TimberbornAgentBridge`.
  `RequiredMods: []`; keine Fremdmod-Pflichtbasis.
- `Timberborn.McpServer` über stdio → `Timberborn.Backend.Native` → authentifiziertes
  Loopback-HTTP → Hauptthread-Queue → öffentliche Spielservices.
- Feste strukturierte Aufrufe, keine generischen HTTP-Werkzeuge, keine Screenshot-/Eingabesteuerung.
- Keine eigenen Save-Daten; Spielaktionen nutzen reguläre Spielmechanik als Funktionsnachweis.
- More HTTP API bleibt Legacy-/Vergleichsadapter. Community-Code ist Referenzmaterial.
  [Quellen](docs/references/README.md), [Architektur](docs/architecture/native-game-api.md).
- Direkter Serverstart ohne `TIMBERBORN_BACKEND` hat noch den Legacy-Default.
  Der dokumentierte Einstieg setzt `native`; der Lesestarter deaktiviert Aktionen.

## Implementierter Umfang

[Werkzeugkatalog](docs/tools.md): 33 Leser und 24 Aktions-/Validierungswerkzeuge,
abhängig von den technischen Aktionsschaltern.

Zustands-/Güter-/Personalabfragen, Alerts mit Zielen, Gebäudeinventare, Baustellen,
Bedürfnisse und Betriebsbelege, Erreichbarkeit und Tagesbilanzen, Produktionsgraph,
Forschung, Lager-/Farmoptionen, Gebäude-/Simulationspause, Zeitläufe, Prioritäten,
Flächen, Entfernung und Ingame-Log sind implementiert. Frühere Live-Belege gelten
für die jeweils geprüften Fälle, nicht automatisch für jede Vorlage oder Version.
[Nachweise](docs/compatibility/timberborn.md), [Fachverträge](docs/README.md).

## Bauausführung: tatsächlicher Stand des Codes

0.25.0 enthält `execute_building_project_pilot` und `inspect_building_project`:
kleines Lager, maximal zwei neue ebene Wege, ein akzeptierter Auftrag je Session.
Gemeinsame Vorschau, einzelne reguläre Platzierungen über mehrere Updates,
Status/ID-Wiederholschutz, reale Weg- und Bauarbeiter-Nachprüfung.

Der normale Baupfad verweigert derzeit unknown, weil vollständige Bauphasenabdeckung
fehlt. Der separate Entwicklungsmodus lässt genau die dokumentierte Nachweislücke
zu. **Diese Codegrenzen sind keine dauerhaften Arbeits- oder Nutzerfreigabegrenzen.**
Ihre zweckmäßige Weiterentwicklung ist Teil des MCP-Projekts. Die neue Direktive
ändert nicht rückwirkend das Verhalten bereits gebauter DLLs.
[Implementierter Vertrag](docs/building-project-execution-proposal.md).

## Nächster Arbeitsschritt

Laufende Version und Session lesen, geeigneten aktuellen Testfall für die neue
Bauausführung wählen und praktisch testen. Bei Fehlern Ursache klären und beheben;
bei Erfolg zum nächsten sinnvollen gebündelten Ausbau übergehen. Kein bestimmter
Save-Name, alter Gebäudebestand oder Ressourcenvorrat erforderlich.
[Technische Übergabe](docs/session-handoff.md), [Backlog](BACKLOG.md).

## Arbeitsmittel und Historie

- `src/`, `mod/Timberborn.AgentBridge/`, `tests/`, `scripts/`: Implementierung und Prüfungen.
- `.local/`: ignorierte Pakete, Mod-Sicherungen, Referenzen und lokale Testbelege.
- [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md): Build, Installation und GitHub-Checkpoints.
- GitHub: [chr0mat0mcodex/timberborn-mcp](https://github.com/chr0mat0mcodex/timberborn-mcp).
- Historische Kolonieversuche bleiben als Belege in [Journal](docs/project-journal.md),
  [Koloniecheckliste](docs/colony-goals.md) und [Bilanztests](docs/supply-balance.md).
  Daraus entstehen keine Wiederherstellungs-, Erhaltungs- oder Fortsetzungspflichten.
- Private Konfiguration, Spielstände, Spiel-DLLs und Rohlogs gehören nicht ins Repository.
