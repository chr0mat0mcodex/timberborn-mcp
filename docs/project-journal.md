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
