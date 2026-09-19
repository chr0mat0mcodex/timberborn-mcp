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
