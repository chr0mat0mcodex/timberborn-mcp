# Einstieg

**Stand 0.74.0 mit abgeschlossenen Journal- und Cancel-/Transport-Korrekturen:** Reparaturbedarf wird aus frischen Zugangsbelegen abgeleitet. Materialwarten, Bestandsverlust, unerreichbare Baustelle und Fertigbau sind getrennt live geprüft. Forstjournale verhindern erneute Mutationen bei Wiederaufrufen, auch nach Neuverbindung.

Letzte Abnahme: **28 Supervisor-, 1936 Unit- und 13 Integrationstests**, drei optionale Live-Integrationstests übersprungen. Supervisor-Stop/Start und Forstjournal-Persistenz gezielt live bestätigt. [Belege und Grenzen](docs/releases/0.74.0.md).

Offen bleiben insbesondere unbekannte Objektidentität im Livebetrieb und gesonderte weitere Zugangsgeometrien. Dateifehler sowie unterbrochene Schreibaufträge sind synthetische Gate-Nachweise. [Projektstand](PROJECT_STATE.md) · [Offene Aufgaben](BACKLOG.md) · [Agenten-Anleitung](docs/agent-playing-guide.md).

Cancel-Transport begrenzt live abgenommen: 30 Sekunden angefordert, intern
1,002 Sekunden gewartet; Cancel-Antwort nach 2,986 Sekunden, Pause bestätigt.
Zeitmarken lokalisieren die serielle Zustellung vor dem Supervisor.
[Aktueller Vertrag](docs/lightweight-status-wait.md). Kein Parallelitätsversprechen.

## Frühere Abnahmen

Die folgenden Versionsabschnitte beschreiben damalige Ergebnisse; sie ersetzen nicht den aktuellen Projektstand.

**0.74.0 abgeschlossen:** Materialwarten, echter Bestandszugangsverlust und Fertigbau korrekt unterschieden; Weg wiederhergestellt und Journale freigegeben. [Livebelege](docs/releases/0.74.0.md).

**0.73.0 abgeschlossen:** Gate und Livetest für Parameterdiagnose sowie frischen Fertigbau trotz altem Zeitlimit bestanden. [Livebelege](docs/releases/0.73.0.md).

**0.72.0 abgeschlossen:** Gate, Laufzeitdiagnosen und vollständiger Plattform-Umwegpilot bestanden: 15/15 Teile fertig und zugänglich, Bestandswege erhalten, Journal freigegeben. [Livebelege](docs/releases/0.72.0.md).

**0.71.0: Gate und Umwegplatzierung bestanden, Fertigbau offen.** Oberer Treppenanschluss und seitliche Negativkontrolle bestätigt; automatischer Plattformumweg nativ gültig, 17/17 Teile platziert, keine verlorene Bestandsverbindung. [Belege und Learnings](docs/releases/0.71.0.md).

**0.70.0: Höhenkernpilot bestanden, Umweg offen:** native Vorschau registrierter Treppen-Zielwege, getrennte Anschlussbelege. Gate und bekannter Höhenbau bis Fertigzugang bestanden; erweiterter Umweg nativ abgelehnt. [Nachweisplan](docs/releases/0.70.0.md).

**0.69.0: Gate bestanden, Höhenpilot fehlgeschlagen:** native Weggraph-Prüfung für Bestandstreppen und automatisch geöffnetes MCP-Log. Beide Graphabfragen bleiben negativ, manuelle Gegenprobe gültig. Logfenster sichtbar und angehängt. [Nachweisplan](docs/releases/0.69.0.md).

Historisch 0.68.0: Gate bestanden, Höhenpilot damals fehlgeschlagen.
Die spätere Höhen-/Umwegabnahme steht unter 0.72.0.
[Nachweis und Stoppkriterium](docs/releases/0.68.0.md). Kein Agent-Build/Testlauf.

## 0.54.0 - Pflanzen- und Flaechenjournal begrenzt live abgenommen

Menschliches Gate und Abschluss-Livetest am 12. Oktober 2026 bestanden.
Pflanz-/Faellmarkierungen, reale Pflanzenwechsel und Baum-/Stumpfunterscheidung
mit ehrlicher Herkunft; Compact/Full, Filter und Cursor live bestaetigt.
Menschliche Eingriffe bleiben unknown. Neue Sitzung startet ohne Altbestands-
ereignisse; alte Session wird abgewiesen. Produktionsregression bestanden.
Grenzen: abgetastete Zustandswechsel, keine lueckenlose Ereignis-/Erntehistorie.
[Vertrag und Livebelege](docs/spatial-journal-0.54.md).


## 0.53-Nacharbeiten abgeschlossen

Menschliches Gate und gezielter Livetest am 12. Oktober 2026 bestanden.
Native Ablehnung planting_preview_changed meldet not_executed; anschliessende
frische Vorpruefung bestaetigt unveraenderte Markierungen. Transportunsicherheit
und andere Konflikte bleiben unconfirmed (automatische Gegenproben im Gate).
Compact laesst rein passende Zellbeispiele weg: 1165 statt 2929 JSON-Zeichen,
60,2 Prozent kleiner bei gleichem Schluessel und gleichen Summen. Zusatzblockaden
bleiben sichtbar; Full vollstaendig. Testmarkierung wiederhergestellt, alle acht
Zellen frisch nachgelesen, Endpause bestaetigt. Keine Pflanzen oder Stapel entfernt.



## Maßgebliche Arbeitsweise

Das Spiel ist ein austauschbares Testsystem. Maßgeblich sind [AGENTS.md](AGENTS.md)
und der unveränderte [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md): Agent entwickelt
und prüft Quelltext/Diff; der Mensch testet, baut und installiert; danach gezielter
MCP-Livetest. Keine historische Kolonie oder konkreter Spielstand erforderlich.

## Aktueller Auftrag

0.67.0: Gate bestanden; Höhenpilot scheitert am Suchbudget trotz gültiger
Gegenprobe. Suchpriorisierung und Treppenanschlüsse gezielt nacharbeiten. [Übergabe](docs/releases/0.67.0.md). Kein Agent-Build/Testlauf.

0.66.0: menschliches Gate und begrenzter Uferpumpen-/Startfehlerpilot bestanden.
[Livebelege und Grenzen](docs/releases/0.66.0.md). Folgearbeiten im Backlog.

0.65.0: Nach menschlicher Bereitmeldung Kernpilot begrenzt bestanden: Wasser-/
Rückstauwarnungen, Überflutung bei Pause und produktive Pumpenkontrolle.
[Livebelege und Grenzen](docs/releases/0.65.0.md).

0.64.0: menschliches Gate und begrenzter Bauphasen-/Pause-/Recovery-Pilot bestanden.
[Livebelege und Grenzen](docs/releases/0.64.0.md). Keine eigenen Builds/Tests.

0.63.0 und anschließende Bauplaner-Korrektur sind begrenzt live abgenommen.
Überlebenslauf nach vom Nutzer gemeldetem Kolonieverlust beendet; Rettungsziel
nicht erreicht. Aktuelle Nachweise stehen im [Projektstand](PROJECT_STATE.md),
verbleibende Fehler und Nachweisgrenzen im [Backlog](BACKLOG.md).
Keine automatische Wiederaufnahme historischer Kolonieziele.

## Technischer Stand

Bridge 0.64.0; MCP-Nachträge derselben Version werden durch Commit und
feature-spezifische Abnahme unterschieden. Die älteren Abschnitte oben sind
historische Nachweise, keine aktuelle Aufgabenliste.

Aktuellen Laufzeitstand nach Wiederanschluss aus `timberborn_status` und Session
lesen. Die Versionsnummer allein beweist nicht den Abnahmestatus lokaler Änderungen.
Arbeitsbranch: `codex/road-protection-pilot`; Git-Status vor Änderungen prüfen.
Der Hermes-Integrationstest wurde auf Nutzerwunsch beendet; Proxy,
Integrationsanleitung und Skillvorlage sind entfernt. Historische Erwähnungen
dokumentieren frühere Arbeiten und begründen keine weitere Pflege.

Phase A–E ist historisch im begrenzten Umfang abgeschlossen; die verbliebenen
Nachweislücken und Folgefähigkeiten stehen im [aktuellen Backlog](BACKLOG.md).
Nächstes Paket konkret auswählen, keine alten Kolonieziele automatisch fortsetzen.

## Orientierung

- [Projektstand und datierte Abnahmen](PROJECT_STATE.md)
- [Offene Arbeit](BACKLOG.md) / [historischer Backlog](BACKLOG_HISTORY.md)
- [Werkzeuge](docs/tools.md) / [Agentenanleitung](docs/agent-playing-guide.md)
- [Installation](docs/native-bridge-install.md) / [Codex-Lifecycle](docs/codex-mcp-supervisor.md)
- [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md) / [historischer Etappenplan](missionsplan.md)
