# Projektstand

Stand: 2026-10-03. Das Projekt entwickelt eine native MCP-Steuerung für Timberborn.
Das Spiel ist ausschließlich Testsystem; konkrete Spielstände gehören nicht zur
Projektbeschreibung.

## Verifizierter Stand

| Ebene | Stand |
| --- | --- |
| Bridge | Agent Bridge 0.29.3 installiert; Plattformpilot und direkte vertikale Distriktanbindung live belegt |
| Automatisch | 667 erfolgreich, 0 fehlgeschlagen, 3 Live-Tests übersprungen (menschlicher Skriptlauf) |
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
Die vertikale Distriktanbindung beider oberer Wege ist mit 0.29.3 direkt belegt;
vollständiger generischer Wegschutz bleibt offen. Details: [Plattformpilot](docs/vertical-platform-pilot.md).

0.29.3 live: `inspect_path_district` fragt die reale Hauptwegzelle gegen ein
konkretes Distriktnetz ab, ohne den Gebäude-Eingangsfilter. Beide oberen Testwege
verbunden; Lager und unfertige Treppe korrekt als unbekannt gemeldet.
[Nachweis](docs/path-district-observation.md).

1. Den Baupfad datengetrieben auf weitere Vorlagen, längere Anschlüsse und größere
   Vorhaben erweitern.
2. Wegkonflikte, Produktionsblockaden sowie Hunger-/Durst- und Versorgungsdiagnosen
   zu evidenzbasierten Befunden bündeln.

Details: [Fachverträge](docs/README.md), [Backlog](BACKLOG.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
