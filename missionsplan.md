# Mission: Timberborn über MCP steuerbar machen

## Prioritäten

1. **Testsystem:** Spielstände und Kolonien sind austauschbares Testmaterial.
2. **Schnelle und effiziente Entwicklung:** funktionierende, nachvollziehbar geprüfte
   MCP-Fähigkeiten liefern; unnötige Freigabe-, Dokumentations- und Testschleifen vermeiden.

Maßgeblich sind die [Projektanweisungen](AGENTS.md). Das Endziel bleibt ein Agent,
welcher Timberborn per MCP spielen kann. Koloniepflege, Ressourcenschonung oder der
Erhalt eines konkreten Spielstands sind keine Anforderungen an die Entwicklung.
Frühere Ziele wie 100 Biber und alle Gebäude dienen allenfalls als spätere
Belastungs-/Abdeckungsszenarien, nicht als fortlaufende Aufgaben.

## Aktueller Stand und nächste Arbeit

0.25.0 ist installiert, 648 reguläre Tests bestehen. Die gemeinsame Bauvorschau
ist live belegt; die neue tatsächliche Ausführung steht noch zur Live-Prüfung an.

- [x] Eigene Mod und nativen MCP-Zugang ohne Fremdmod-Pflichtbasis bereitstellen.
- [x] Zustandsdaten, Güter, Personal, Baustellen, Flächen, Forschung und Produktionsgraph bereitstellen.
- [x] Basisaktionen, Lager-/Farmoptionen, Pause/Geschwindigkeiten und Ingame-Zeitläufe implementieren.
- [x] Ingame-Log mit kurzen Aktionsbegründungen implementieren und praktisch prüfen.
- [x] Bauplatzsuche, gemeinsame Gebäude-/Wegvorschau und schrittweise Ausführung implementieren.
- [ ] Reale Bauprojektausführung mit 0.25.0 prüfen; Fehler direkt eingrenzen und beheben.
- [ ] Bau-/Wegfunktionen zu einem praktisch nutzbaren Umfang ausbauen: weitere Vorlagen,
  sinnvolle Projektgrößen und reale Zugänge. Aktuelle Grenzen sind technische Entwicklungsaufgaben.
- [ ] Problemdiagnose vervollständigen und an repräsentativen Szenarien prüfen.

Ein funktionierender Nachweis genügt für den jeweiligen Fall; nicht pauschal alle
alten Tests wiederholen. Bauen, Abreißen oder Veränderung der Testkolonie ist ein
normales Testmittel. Rückabfragen dienen korrekten Ergebnissen. Schädliche Platzierungen
gezielt ablehnen zu können bleibt ein Produktfeature, kein Gebot zur Schonung des Testsaves.

[Projektstand](PROJECT_STATE.md) · [Backlog](BACKLOG.md) · [Werkzeuge](docs/tools.md) ·
[Ausführung in 0.25.0](docs/building-project-execution-proposal.md).

## Nachweise und Historie

Vorprüfung, Auftrag, Fertigstellung und Wirkung sind unterschiedliche Nachweise.
Historische Abnahmen bleiben im [Projektjournal](docs/project-journal.md), in den
[Fachverträgen](docs/README.md) und der [Kompatibilitätsdoku](docs/compatibility/timberborn.md).
Sie enthalten frühere Ingame-Vorgaben, die durch die heutige Testsystem-Direktive
überholt sind. Keine Kolonie rekonstruieren, nur um einen alten Testablauf zu wiederholen.
