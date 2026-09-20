# Dokumentation

Start: [README](../README.md) · [Projektstand](../PROJECT_STATE.md) ·
[Mission](../missionsplan.md) · [Backlog](../BACKLOG.md).

## Aktueller Einstieg

- [Installation und Aktionsfreigaben](native-bridge-install.md)
- [Alle nativen MCP-Werkzeuge](tools.md)
- [Architektur](architecture/native-game-api.md)
- [Entwicklung und Tests](../DEVELOPMENT_WORKFLOW.md)
- [Kompatibilität und Nachweise](compatibility/timberborn.md)

## Funktionsverträge

| Bereich | Dokument |
| --- | --- |
| Generischer Bau | [Vorprüfung, Validierung und Bauauftrag](generic-building.md) |
| Gebäude/Baufortschritt | [Baustellen und Distrikt](building-observations.md), [Betrieb](building-operations.md) |
| Lager/Farm/Pause | [Gebäudeeinstellungen](building-settings.md) |
| Personal | [Arbeitskräfteliste](workforce-roster.md), [Sollbesetzung](workplace-staffing.md) |
| Prioritäten und Flächen | [Prioritäten, Baustellen, Flächen](priorities-construction-areas.md) |
| Entfernung/Kiefern | [Getrennte Aktionen](removal-and-pine-protection.md), [Lebenszustand](vegetation-state.md) |
| Bedürfnisse und Betriebsbelege | [Native Werte, Warnflags und Produktionsvoraussetzungen](needs-and-operation.md) |
| Güter und Status | [Bestände, Warnungen und Ziele](economy-observations.md) |
| Erreichbarkeit und Bilanz | [Wege, Reichweiten und Güterhistorie](logistics.md), [Mehrtagspilot](supply-balance.md) |
| Zeit | [Pause und Geschwindigkeiten](simulation-control.md) |
| Produktionsgraph | [Rohstoffe, Rezepte, Gebäude und Quellen](production-dependency-graph.md), ab 0.21.0; begrenzter Live-Pilot bestanden |
| Forschung | [Punkte und Freischaltungen](research.md) |
| Transparenz | [Ingame-Log](activity-log.md), [fachliche Fehler](bridge-errors.md) |

Versionsnummern in älteren Fachdokumenten bezeichnen häufig die Einführung eines
Vertrags. Aktuelle Gesamtversion und Testzahlen stehen zentral im Projektstand.
Datierte frühere Abnahmen sind kein Beweis für jede spätere Kombination.

## Geplante Funktionen

- [Weitere Alert-Abdeckung](alerts-plan.md) — aktive Entity-Status implementiert; zusätzliche Fälle offen.
- [Frage-Popup im Spiel](player-question-popup.md) — öffentlich untersucht, noch nicht implementiert.
- Weitere Lücken: [Backlog](../BACKLOG.md).

## Historie und Referenzen

- [Projektjournal](project-journal.md) — datierte Ereignisse.
- [Historische Mission](history/missionsplan-2026-09-20.md) und [ursprünglicher Phase-2-Plan](history/phase-2-original.md).
- [Legacy-Adapter](legacy-backend.md), [erster POC](testing/live-poc.md), [Phase-2A-Befunde](phase-2a-results.md).
- Frühe native Piloten: [räumliche Vorprüfung](spatial-precheck.md), [Validator](native-validation.md),
  [Weg](path-placement.md), [Lodge](lodge-placement.md). Für neue Bauaufgaben: generische Werkzeuge.
- [Architekturentscheidungen](architecture/decisions.md) — chronologisch.
- [Offizielle Quellen und good references](references/README.md).

- [Güter und aktive Statusmeldungen](economy-observations.md): 0.18.0, Güter und eine Lagerwarnung live bestätigt.
