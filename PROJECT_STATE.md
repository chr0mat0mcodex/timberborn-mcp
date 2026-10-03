# Projektstand

Stand: 2026-10-03. Das Projekt entwickelt eine native MCP-Steuerung für Timberborn.
Das Spiel ist ausschließlich Testsystem; konkrete Spielstände gehören nicht zur
Projektbeschreibung.

## Verifizierter Stand

| Ebene | Stand |
| --- | --- |
| Bridge | Agent Bridge 0.29.2 installiert und mit fünfteiligen Plattformpilot live belegt |
| Automatisch | Menschlicher Test-/Deploy-Ablauf durchgeführt; letzte explizit dokumentierte Zählung: 650 reguläre Tests plus 3 übersprungene Live-Tests (0.28.1) |
| Mod-Build | Timberborn 1.1.2.4 / Folktails, ohne Warnungen und Fehler |
| Laufzeit | Bridge und Schreibfreigabe strukturiert erreichbar |
| Bauprojekt | 0.29.2 live: Treppe, zwei Plattformen, zwei obere Wege; Bauphasen, completed und alle fünf fertigen Objekte rückgelesen |

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

Plattformpilot 0.29.2 abgeschlossen: Der begrenzte Wartezustand behandelt
Baustellen mit unverändertem Wegschutz und Bau nur bei Pause. Nach drei getrennten
begrenzten Bauphasen sind alle fünf Objekte fertig und der Auftrag abgeschlossen.
Vertikale Distriktanbindung und vollständiger generischer Wegschutz sind nicht
bewiesen. Details: [Plattformpilot](docs/vertical-platform-pilot.md).

1. Den Baupfad datengetrieben auf weitere Vorlagen, längere Anschlüsse und größere
   Vorhaben erweitern.
2. Wegkonflikte, Produktionsblockaden sowie Hunger-/Durst- und Versorgungsdiagnosen
   zu evidenzbasierten Befunden bündeln.

Details: [Fachverträge](docs/README.md), [Backlog](BACKLOG.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
