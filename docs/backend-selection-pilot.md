# A11-Pilot: explizite Backendwahl beim Direktstart

Stand 2026-10-18: Gate und gezielte Liveabnahme bestanden; A11 abgeschlossen.

## Entscheidung und Umfang

Der rohe DLL-Direktstart erhält keine neue automatische Backendwahl. Fehlt
`TIMBERBORN_BACKEND` oder ist sie leer/Whitespace, endet der Prozess mit Code 2
und einer festen Konfigurationsmeldung auf stderr. stdout bleibt leer. Die Prüfung
liegt vor Host, Dateilog und Backendkonstruktion. Explizite Werte bleiben exakt
`native`, `more-http-api` und `fake`; keine Groß-/Kleinschreibungsnormalisierung.
Unbekannte nichtleere Werte behalten ihre bisherige Ablehnung.

Kompatibilitätsentscheidung: Nur bisher implizite Legacy-Direktstarts müssen nun
`TIMBERBORN_BACKEND=more-http-api` setzen. Kein Adapter wird entfernt. Eine neue
stille Native-Voreinstellung wäre ebenfalls mehrdeutig, weil native Konfiguration
zusätzlich erforderlich ist. Die ausdrückliche Auswahl macht beide Fälle klar.
Supervisor und `scripts/start-native.ps1` setzen native bereits ausdrücklich und
bleiben unverändert. Konfiguration, globale Codex-Dateien und Supervisor werden
nicht geändert. Native Fehler führen weiterhin zu keinem Adapterwechsel.

Der spezielle Offline-Aufruf `--print-native-identity` bleibt vor dieser Prüfung:
Er liefert den Gatebericht ohne Backendwahl, Konfiguration oder Hoststart.
Bridge/API bleibt 0.74.0. Keine neue Abhängigkeit oder Werkzeugfamilie.

## Tests und Abnahmeplan

Fünf neue Integrationsfälle für die Release-DLL: fehlende, leere und Whitespace-
Backendwahl mit offenem stdin (beendet sich trotzdem, Code 2, stderr-Meldung,
leeres stdout); explizit native ohne Konfiguration (Fehler statt Legacy-/Fake-
Fallback); explizite Legacy-Auswahl mit stdio-Handshake und Werkzeugerkennung,
ohne Spielzugriff. Bestehende Fake-/Native-Integrationstests und Offline-Kennungs-
test bleiben erhalten. Kein Legacy-Spielservice-Livenachweis behauptet.
Agent führt keine Builds oder Tests aus; diese Fälle laufen im menschlichen Gate.

Nach Gate und `live bereit`: verbundenen Supervisor starten, nativen Status,
Session und Gatekennung bestätigen. Eine lesende Simulationsabfrage muss weiter
`meta.backend=native`, `simulated=false` liefern. Kein zusätzlicher stdio-Prozess,
kein Spielauftrag, kein erforderlicher Codex-Neustart (Supervisor unverändert).
Stoppkriterium: unerwarteter Backendmodus oder fehlende Erreichbarkeit; gezielt
diagnostizieren statt auf Legacy auszuweichen oder die Vollkette zu wiederholen.

## Abnahme und Bewertung

Menschliches Gate: 13 Paket-, 35 Supervisor-, 2239 Unit- und 19 Integrationstests
bestanden; drei optionale Livefälle übersprungen. Alle fünf neuen Prozessfälle
bestanden. Release-Builds ohne Warnungen/Fehler; fünf installierte Dateien per
SHA-256 bestätigt. Offline-Kennungsbericht und explizite Fake-/Native-Auswahl
bleiben durch die bestehenden Integrationstests gedeckt.

Live über den verbundenen Supervisor: Backend erfolgreich gestartet, Status
erreichbar mit Bridge 0.74.0. Build- und Katalogkennung entsprechen dem Gate.
Status und `inspect_simulation` liefern `backend=native`, `simulated=false` und
dieselbe aktuelle Sitzung. Simulation pausiert, kein aktiver Lauf oder ausstehender
Tempoauftrag. Kein Spielauftrag und kein zusätzlicher stdio-Prozess erforderlich.
Der Backend-Switch enthält keinen Fehlerfallback; ein beliebiger nativer
Verbindungsausfall und Legacy-Spielservices wurden nicht zusätzlich live getestet.

Wenige Zeilen Startprüfung beseitigen die implizite Fremdmod-Auswahl. Aufwand:
gezielte Prozessregressionen und Korrektur zweier Einstiegstexte. Bewusste kleine
Inkompatibilität für bisher unkonfigurierte Direktstarts, mit konkreter Meldung.
Nutzen/Aufwand positiv: eindeutige Auswahl bei kleinem, lokalem Eingriff und fünf
gezielten Prozessfällen. Keine grundlegende Startarchitekturänderung. Zusätzlicher
Gate-Durchlauf wegen der frühen Windows-stderr-Codierung war nötig. Keine weiteren
Startmodi oder Backendmigrationen; der ursprüngliche A11-Befund ist behoben.

## Korrektur nach erstem Gate

Release-Build ohne Warnungen/Fehler und 2239 Unit-Tests bestanden. Der erste
A11-Integrationsfehler betraf die exakte stderr-Meldung: das Umlautzeichen wurde
beim umgeleiteten Windows-Prozessstart als Ersatzzeichen gelesen. Exitcode 2
und leeres stdout waren bereits bestätigt. Frühe Konfigurationsmeldung auf
ASCII-Wortlaut ("setzen" statt "wählen") umgestellt, Testerwartung entsprechend
angepasst. Keine Änderung der Backendauswahl oder globalen Konsolencodierung.
Integrationstests wurden beim ersten Fehler abgebrochen; weitere Fälle dadurch
noch nicht vollständig nachgewiesen. Das anschließende menschliche Gate und der
oben dokumentierte Livebeleg sind bestanden.
