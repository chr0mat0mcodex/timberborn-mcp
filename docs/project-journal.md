# Projektjournal

## 2026-09-19 — Paket A

Nutzer hat Schritt 5 und damit die Umsetzung des Missionsplans freigegeben.
Sechs getrennte src-Projekte, zwei Testprojekte, projektlokale NuGet-/CLI-Caches und
gepinnten SDK-/Paketstand angelegt. Fake-Backend und fünf Read-only-Tool-Verträge umgesetzt.
Release-Build ohne Warnungen; drei Anwendungstests und ein echter MCP-stdio-Prozesstest bestanden.
Der echte Adapter folgt in Paket B. Noch keine MCP-Client-Konfiguration verändert.

Lesender API-Pilot erfolgreich: localhost:8080, Mod 11.0.0, Spiel 1.1.2.4.
`ping`, `misc`, `live-data`, `characters`, `buildings` und Einzelgebäude antworten.
Mapping-Besonderheiten: Mod-Aktivierung heißt `Active`, Bevölkerungslisten heißen
`Adult`/`Child`/`Bot`, Tagesfortschritt liegt in `TopBar.Cycle.Hours`.
Keine Live-Antworten, persönlichen Namen oder Spiel-IDs als Fixtures gespeichert.

## 2026-09-19 — Pakete B/C und technischer Live-Test

More HTTP API 11.0.0 angebunden. Zulässige Routen fest begrenzt; Loopback-Verbindung erhält den
Hostnamen localhost, deaktiviert Proxy/Redirects und begrenzt dekomprimierte Antwortkörper.
Einzelgebäude werden gegen die Gebäudeliste plausibilisiert. Fremdmod-Einstellungen und lokale
Verzeichnisse werden nicht weitergereicht. Wetterprognosen respektieren ShouldShowNext.

Alle fünf MCP-Werkzeuge wurden in einem echten stdio-Prozess gegen die vorbereitete Spielkolonie
erfolgreich aufgerufen. Simulationsmarker war false. Nutzer bestätigt den manuellen UI-Vergleich.
Die Spiel- und MCP-Client-Konfiguration wurde nicht geändert. Git-Identität auf Nutzerwunsch
pro Commit: Codex <codex@openai.com>; keine globale oder lokale Git-Konfiguration gesetzt.

Abschlussprüfung: Locked-Mode-Restore erfolgreich, Release-Build mit 0 Warnungen/0 Fehlern,
42 Anwendung-/Adaptertests und 5 deterministische Integrationstests erfolgreich.
Der Live-Test wird im normalen Lauf korrekt übersprungen und wurde separat mit explizitem Opt-in ausgeführt.
Nutzerabnahme erlaubt den Meilenstein `poc-readonly-v0.1` nach finaler Sicherung.

## 2026-09-19 — GitHub-Veröffentlichung vorbereitet

Nutzer beauftragt Veröffentlichung des Projekts einschließlich Agentenanweisungen und fortlaufende
Pushes geprüfter Checkpoints nach chr0mat0mcodex/timberborn-mcp. AGENTS.md bündelt projektspezifische
Grenzen und Einstiegspunkte; DEVELOPMENT_WORKFLOW.md dokumentiert Prüfung, Commit und Remote-Abgleich.
README und aktuelle Freigabegrenzen verweisen darauf. Keine persönlichen globalen Agentendateien übernommen.
Bestehende Commit-Dateilisten und verdächtige Textmuster vor Veröffentlichung geprüft; keine Zugangsdaten
oder persönlichen Benutzerpfade erkannt. Spielstände, Fremdbinärdateien und lokale Laufzeitdaten bleiben ausgeschlossen.
