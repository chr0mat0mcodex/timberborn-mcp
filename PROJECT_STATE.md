# Projektstand

Stand: 2026-09-20. Codeversion: **0.18.0**, gebaut und installiert; neue Live-Abnahme ausstehend.
Zuletzt gezielt live abgenommen: **0.17.2**.
Die Version im [Manifest](mod/Timberborn.AgentBridge/manifest.json) ist die Codeversion;
ältere datierte Einträge im Journal dokumentieren frühere Zustände.

## Produkt und Architektur

- Ziel: KI-Agent spielt Timberborn über MCP mit regulären Kosten, Bauzeit, Personal und Forschung.
- Aktiver Weg: `Timberborn.McpServer` → `Timberborn.Backend.Native` → eigene `Timberborn.AgentBridge`.
- Mod-ID: `chr0mat0mcodex.TimberbornAgentBridge`; keine erforderlichen Fremdmods.
- MCP: stdio; Modtransport: authentifiziertes Loopback-HTTP auf `localhost`, Port aus privater Konfiguration.
- Spielzugriffe auf dem Hauptthread, feste Routen, begrenzte Antworten und Aktionen.
- Keine eigenen Save-Daten. Reguläre Spieländerungen werden mit dem Spielstand gespeichert.
- `MoreHttpApi`, Fake sowie die frühen Contracts/Application-Schichten bleiben für Legacy/Regression erhalten.
  Der native Werkzeugpfad nutzt eigene Native-Verträge. Keine komplette Backend-Parität behaupten.
- Direkter Serverstart ohne `TIMBERBORN_BACKEND` fällt im aktuellen Code noch auf `more-http-api`
  zurück. Der dokumentierte Einstieg setzt deshalb ausdrücklich `native`; kein stiller Backendwechsel.

## Verifikation

| Ebene | Beleg |
| --- | --- |
| Automatisch | 409 reguläre Tests: 396 Unit, 13 Integration; drei opt-in Live-Tests im Standardlauf übersprungen |
| Mod-Build | Gegen Timberborn 1.1.2.4, ohne Warnungen/Fehler |
| Installation | Fünf Paketdateien per SHA-256 geprüft, private Konfiguration erhalten |
| Live 0.17.2 | template_locked, stale_session, state_conflict; unveränderter Zustand, rejected im Log |
| Live Forschung | Regulär produzierte Punkte, bezahlter Unlock 35 → 5, Wiederholung ohne zweiten Abzug, gültige Bauvalidierung danach |
| Live Eingang | Echte Path-Vorlage von Lodge-Belegung unterschieden; Distrikt und Personal separat bestätigt |
| Weitere Piloten | Generischer Bau, Lager/Farmoptionen, Prioritäten, Flächen, Entfernung, Zeitsteuerung und MCP-Log |

Die Live-Nachweise beziehen sich auf eine kleine Entwicklungskolonie mit Folktails.
Nicht jede Gebäudekombination oder Spielversion ist getestet. Nicht jeder reguläre
Testlauf startet das Spiel oder prüft alle nativen Leser live.

## Umfang und Grenzen

[22 Leser + 19 freizugebende Werkzeuge](docs/tools.md). Die Steuerungsbasis ist vorhanden.
Neu implementiert: vollständige registrierte Güter mit ResourceCount-Feldern sowie
sichtbare aktive Entity-Status und betroffene Ziele; [Vertrag](docs/economy-observations.md).
Live-Abnahme dieser drei Leser ausstehend. Offen bleiben vollständige UI-Meldungsabdeckung,
zuverlässige Blockade-/Bedürfnisdiagnose, Produktions-/Verbrauchsbilanzen und Erreichbarkeit.
Die Grundversorgungsabnahme ist nicht vollständig: einzelne Produktionsketten belegt,
Wohnraum und nachhaltige Gesamtversorgung noch offen. [Backlog](BACKLOG.md).

## Dateien und Veröffentlichung

- `src/`: MCP-Server, Native-Client, Bridge.Core, Legacy-/Fake-Code.
- `mod/Timberborn.AgentBridge/`: eigene Spielmod und Manifest.
- `tests/`: synthetische Tests und getrennte opt-in Live-Tests.
- `scripts/`: reproduzierbarer Build, Lesestart, Prüfungen und Referenzabruf.
- `.local/`: ignorierte lokale Pakete, Sicherungen und Testartefakte; keine Veröffentlichung.
- GitHub: [chr0mat0mcodex/timberborn-mcp](https://github.com/chr0mat0mcodex/timberborn-mcp), Branch `master`.
- Keine GitHub-Releases vorhanden beim Abgleich am 2026-09-20; lokale Modpakete sind keine veröffentlichten Releases.
- Private Konfiguration, Tokens, Saves und Spielbibliotheken gehören nicht ins Repository.
