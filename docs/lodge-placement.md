# Lodge-Baupilot 0.7.0

> Historischer Pilot-/Nachweisbericht. Alte Versionsstände und Grenzen gelten für den damaligen Test. Aktueller Einstieg: [native Installation](native-bridge-install.md), [generischer Bau](generic-building.md) und [Projektstand](../PROJECT_STATE.md).

Status: implementiert, installiert und begrenzt live bestätigt (0.7.0). Der
zuletzt live geprüfte Stand ist 0.7.0. Ziel ist ein regulärer Hausauftrag über MCP
und dessen unabhängige Wiedererkennung als Baustelle, noch kein autonomer Hausbau.

## Vertrag

`place_lodge(template, x, y, z, rotation, session)` erlaubt ausschließlich
`Lodge.Folktails`. Rotation 0–3, ungespiegelt. Transport: authentifiziertes,
bodyloses POST `/agent-api/v1/lodge-placement` auf Loopback.
Eigenes Mod-Opt-in `enableLodgePlacement: true` und MCP-Prozessvariable
`TIMBERBORN_ENABLE_LODGE_PLACEMENT=1` sind erforderlich; Paket und Standardstart
lassen beide aus. Die bisherige Wegfreigabe aktiviert keine Lodge.

Weg und Lodge teilen dieselbe Singleton-Versuchssperre: zusammen ein Versuch
pro geladenem Spielkontext, auch bei Ablehnung oder Fehler. Keine automatische
Wiederholung, kein Neuladen zum Umgehen der Sperre. Aktuelle Session erforderlich.
Die Mod prüft alle vier Footprint-Zellen auf bestehende Objekte (auch ersetzbare),
Fraktion/Freischaltung/Featurestatus und frisch beide öffentlichen Spielvalidatoren.
Vorlagen mit PlaceFinished werden für Lodge abgewiesen. Anschließend genau ein
normaler Aufruf des Spielplatzierers mit vorgegebener Entity-ID. Keine Ressourcen,
Sofortfertigstellung, Löschung oder automatische Rückabwicklung.

`applied` bestätigt Entity-ID, Vorlage, Position und Rotation. `finished` separat
lesen; applied ist keine Bauabschluss- oder Erreichbarkeitszusage. Bei unconfirmed
oder Transportfehler nur lesend klären. Keine neuen externen Abhängigkeiten.

## Begrenzte Live-Abnahme

1. Spiel speichern/beenden; eigenes Update gesichert installieren, neues Opt-in
   gezielt aktivieren. Nach Nutzerstart zuerst Version 0.7.0 und Session lesen.
2. Frischen Katalog, Gebäude und kleinen Kartenbereich lesen. Freie vollständige
   2×2-Fläche mit plausibel an bestehendem Weg liegendem Eingang auswählen;
   lesende Vorprüfung. Kein umfassender Suchlauf und keine Umgestaltung.
3. Genau ein Lodge-Auftrag. Receipt lokal behalten und Objekt mit derselben ID
   über inspect_building abfragen. Kosten, Baustellenbestand, Fortschritt und
   Baudistrikt getrennt beurteilen. Keine Annahme, dass globale Vorräte geliefert sind.
4. Erfolg: eindeutige Lodge am angefragten Ort als normaler Auftrag nachgelesen.
   Fertigstellung/zusätzliche Betten sind ein späterer separater Wirkungsnachweis.
   Bei Ablehnung/Unklarheit stoppen und Ursache lesen; kein zweiter Bauversuch.

Automatisierte Tests prüfen separate Gates, falsche Vorlagen/Sessionsyntax,
Antwortkorrelation, unfertige Quittung und gemeinsame Sperre über echtes MCP-stdio
und simulierte Bridge. Die Spielplatzierung selbst bleibt bis zur Live-Abnahme offen.

Prüfung: 147 reguläre Tests bestanden (134 Unit, 13 Integration), drei Live-Tests
übersprungen. Separater Mod-Build gegen Timberborn 1.1.2.4 ohne Warnungen/Fehler.


Live-Ergebnis: ein Auftrag applied, Entity-ID separat als aktive/unfertige Lodge
nachgelesen; Eingang am Weg, Baudistrikt bekannt, 12 Log Kosten und leeres Inventar.
Fortschritt 0; Lieferung/Fertigstellung/Wohnraumwirkung bleiben offen.
