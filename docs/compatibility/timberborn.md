# Kompatibilität

Stand 2026-09-19. Diese Matrix beschreibt die tatsächlich geprüfte lokale Kombination.

| Komponente | Version | Nachweis |
|---|---|---|
| Timberborn | 1.1.2.4-52e959e-sw | Versionsdatei und API |
| More HTTP API | 11.0.0 | Manifest, Spiel-Log, API und fünf MCP-Leseaufrufe |
| Moddable Timberborn | 11.1.2 | Manifest und Spiel-Log |
| Mod Settings | 1.1.1.0 | Manifest und Spiel-Log |
| Harmony | 2.4.1 | Manifest und Spiel-Log |
| TimberUi | 11.0.1 | Manifest und Spiel-Log |

MCP-POC: stdio erfolgreich gegen diese Kombination getestet. Der Nutzer hat die gemeldeten
Zahlen der Testkolonie MCP mit der Spielanzeige verglichen und bestätigt.
Keine Aussage zur Kompatibilität anderer Versionen, großer Kolonien oder zusätzlicher Mods.

`http://localhost:8080/` funktioniert in dieser Umgebung. Die numerische IPv4-Adresse
`http://127.0.0.1:8080/` wurde vom Spiel abgelehnt. Der Client erhält den Hostnamen und setzt
ihn nicht durch eine numerische URL um. Daraus wird keine allgemeine Ursache abgeleitet.

## Native Bridge — 2026-09-20

Agent Bridge 0.2.0 mit Konfigurationspfad-Fix über ModRepository: im Spiel geladen,
HTTP-Port 8081 erreichbar, alle drei nativen MCP-Werkzeuge über stdio live erfolgreich.
Bevölkerung/Bestände/Wohnraum durch Nutzer bestätigt. Eine Kartenzelle technisch gelesen;
Semantik und Session-Wechsel noch offen. Bisherige Fremdmods weiterhin aktiv, aber vom
nativen Backend nicht angesprochen; isolierter Spielstart ohne Fremdmods noch nicht geprüft.

0.3.0 zusätzlich live geprüft: sechs native MCP-Werkzeuge, 21 Gebäude-/Wegeobjekte,
Fraktion/Freischaltung/Kosten von Lodge und Path, 64 Gelände-/Wasserzellen und vier
erklärbar abgewiesene Bauplatzvorprüfungen. Objektzahl und beobachtete Bestände unverändert.
Sitzungswechsel gegenüber vorher lokal gespeicherter ID direkt bestätigt.
0.4.0 kompiliert und synthetisch geprüft; Vorschau-Liveprüfung steht aus.
