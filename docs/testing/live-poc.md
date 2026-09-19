# Live-Abnahme POC

Datum: 2026-09-19. Vom Nutzer vorbereitete und freigegebene Testkolonie: MCP.

- Spiel und Mod-Ladevorgang anhand gezielter Logauszüge bestätigt.
- Kein Save-Inhalt gelesen, entpackt oder verändert.
- Alle fünf Tools über einen echten MCP-stdio-Client erfolgreich aufgerufen.
- Status: erreichbar, More HTTP API 11.0.0, keine Simulation.
- Kolonie: Zyklus 1, Tag 1; gemäßigtes Wetter.
- Bevölkerung: 9 Erwachsene, 4 Kinder, 0 Bots; 13 Biber insgesamt.
- Gebäude: ein Gebäude; Einzelabfrage desselben Gebäudes erfolgreich.
- Nutzer bestätigt: Zyklus/Tag, Bevölkerung und Gebäudezahl stimmen mit der Spielanzeige überein.
- Keine Write-Aktion, kein Speichern/Laden, kein Pausieren oder Neustart durch den Server.

Der Testlauf ist opt-in über scripts/verify.ps1 -Live. Dabei muss der Nutzer zuvor die gewünschte
Testkolonie laden. Der Test darf nicht automatisch als allgemeiner CI-Test laufen.
Grenzen: Kein Test großer Kolonien, kein API-Nachweis des Kolonienamens, keine Ressourcenabfrage.
