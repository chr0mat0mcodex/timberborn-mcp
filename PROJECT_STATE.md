# Projektstand

Stand: 2026-10-03. Das Projekt entwickelt eine native MCP-Steuerung für Timberborn.
Das Spiel ist ausschließlich Testsystem; konkrete Spielstände gehören nicht zur
Projektbeschreibung.

## Verifizierter Stand

0.30.0: `inspect_selection` über den öffentlichen `EntitySelectionService`
installiert und live geprüft. Distriktzentrale und Erfinderwerkstatt korrekt
erkannt; Auswahlwechsel liefert die neue ID, Vorlage und Rasterposition, jeweils
über `inspect_building` gegengeprüft. Aufgehobene Auswahl liefert `state=none`
und `target=null`. Keine Auswahl-/Kameraänderung, Sitzungspflicht.
`unsupported` und allgemeine Entity-/Weltpositionsfälle sind durch synthetische
Tests abgedeckt, aber nicht separat live belegt. Auswahl allein ist kein
Änderungsauftrag; vor späteren Aktionen frisch lesen und das Ziel prüfen.

| Ebene | Stand |
| --- | --- |
| Bridge | Agent Bridge 0.30.0 installiert; UI-Auswahl, Plattformpilot und direkte vertikale Distriktanbindung live belegt |
| Automatisch | Menschliche Bereitmeldung nach Skript-Gate für 0.30.0; neue Testanzahl nicht übermittelt. Letzter Zahlenstand 0.29.3: 667 erfolgreich, 0 fehlgeschlagen, 3 übersprungen |
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

Aktiver Auftrag seit 2026-10-03: Etappen A–E aus missionsplan.md. Zuerst Bericht
über Wegschutz und neuen Zugang vereinheitlichen, dann Baustellenlücke schließen
und einen vollständigen Gebäudeablauf nachweisen. Beide Kernziele sind teilweise,
nicht allgemein erreicht. `RoadProtection.constructionCovered=false` verhindert
allgemeine sichere Baufreigaben; Entwicklungspiloten erlauben ausschließlich die
ausgewiesene Vorabnachweislücke. Distriktweg-Zugehörigkeit ersetzt keine
Bauarbeiter-Erreichbarkeit. Lagerpilot bestätigt Auftrag und Baustellenzugang,
nicht fertiggestelltes Gebäude mit anschließend geprüftem Zugang.

Der ungeprüfte Vier-Wege-Entwurf (vorgesehene 0.30.1) ist zurückgestellt, lokal als
benannter Git-Stash erhalten und nicht im aktiven Quellstand. Wiederaufnahme siehe
BACKLOG.md. Verifizierte Bridge bleibt 0.30.0; Etappe A ändert vorerst nur die
MCP-Auswertung vorhandener Validierungsergebnisse, keine Spiel-API/Baufreigabe.

Plattformpilot 0.29.2 abgeschlossen: Der begrenzte Wartezustand behandelt
Baustellen mit unverändertem Wegschutz und Bau nur bei Pause. Nach drei getrennten
begrenzten Bauphasen sind alle fünf Objekte fertig und der Auftrag abgeschlossen.
Die vertikale Distriktanbindung beider oberer Wege ist mit 0.29.3 direkt belegt;
vollständiger generischer Wegschutz bleibt offen. Details: [Plattformpilot](docs/vertical-platform-pilot.md).

0.29.3 live: `inspect_path_district` fragt die reale Hauptwegzelle gegen ein
konkretes Distriktnetz ab, ohne den Gebäude-Eingangsfilter. Beide oberen Testwege
verbunden; Lager und unfertige Treppe korrekt als unbekannt gemeldet.
[Nachweis](docs/path-district-observation.md).

Etappe A live bestanden am 2026-10-03: zusätzliche `assessment` in
Einzel-/Projektvalidierung, aus bereits geprüften nativen Belegen abgeleitet.
Sieben getrennte Befunde, Gesamtergebnis blocked/unknown; reguläre Baufreigabe
bleibt false. Keine neue Spielabfrage oder Modänderung. Tests für fehlende Basis,
Verluste, Vorschauzugang versus tatsächlichen Zugang und Antwortprüfung ergänzt.
Menschliche Bereitmeldung nach Skript-Gate. Freie Wegkontrolle, Sperrvorschau mit
zwei verlorenen oberen Wegen und gemeinsame Lager-/Wegvorschau korrekt gemeldet;
beide Wege nach Vorschau unabhängig wieder verbunden, Spielzeit unverändert.
Baustellen- und tatsächlich fertiger Zielzugang bleiben unknown. [Nachweis](docs/build-assessment.md).
Nächster Schritt Etappe B: öffentlicher Bauphasen-/Bauarbeiter-Vorabnachweis.

Details: [Fachverträge](docs/README.md), [Backlog](BACKLOG.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
