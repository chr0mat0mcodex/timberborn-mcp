# Projektstand

Stand: 2026-10-03. Das Projekt entwickelt eine native MCP-Steuerung für Timberborn.
Das Spiel ist ausschließlich Testsystem; konkrete Spielstände gehören nicht zur
Projektbeschreibung.

## Verifizierter Stand

| Ebene | Stand |
| --- | --- |
| Bridge | Agent Bridge 0.25.0 gebaut, paketiert und installiert |
| Automatisch | 648 reguläre Tests: 635 Unit, 13 Integration |
| Mod-Build | Timberborn 1.1.2.4 / Folktails, ohne Warnungen und Fehler |
| Laufzeit | Bridge und Schreibfreigabe strukturiert erreichbar |
| Bauprojekt | begrenzter Pilot live: Vorschau, Weg, Lagerauftrag und Zugang rückgelesen |

Arbeitsbranch: `codex/road-protection-pilot`. Die eigene Mod nutzt keine
Fremdmod-Pflichtbasis.

## Architektur

MCP über stdio → lokales C#-Backend → authentifiziertes Loopback-HTTP →
Hauptthread-Queue → öffentliche Timberborn-Spielservices. Strukturierte Werkzeuge
ersetzen UI-Automatisierung, Screenshotauswertung und Save-Manipulation.

## Implementierter Umfang

Zustands-, Güter-, Personal-, Bau-, Forschungs-, Flächen-, Entfernungs- und
Simulationswerkzeuge sind implementiert. Aktionen verwenden technische Freigaben,
frische Sessions, fachliche Fehlercodes und Rücklesungen. Bauaufträge bleiben an
Vorschau, Wegschutzdiagnose und schrittweise Bestätigung gebunden.

## Offene Arbeit

1. Den Baupfad datengetrieben auf weitere Vorlagen, längere Anschlüsse
   und größere Vorhaben erweitern.
2. Wegkonflikte, Produktionsblockaden sowie Hunger-/Durst- und Versorgungsdiagnosen
   zu evidenzbasierten Befunden bündeln.

Details: [Fachverträge](docs/README.md), [Backlog](BACKLOG.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
