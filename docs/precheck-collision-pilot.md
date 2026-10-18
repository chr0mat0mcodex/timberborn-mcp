# A12/N06: Objektursachen der Baukollision

Stand 2026-10-18: begrenzter Pilot nach menschlichem Gate und lesendem
Abschluss-Livetest abgenommen. Bridge/API bleibt 0.74.0; Mod und MCP gemeinsam
ausgeliefert. Neue optionale Antwortfelder, keine neuen Werkzeuge oder Eingaben.

## Umfang und Vertrag

`precheck_building` ergänzt pro nativer Prüfzelle `collision`:

- `intersectsTerrain`: native Terrain-Occupancy-Prüfung, unabhängig von Objekten;
  außerhalb der Karte null. Terrain erhält keine erfundene Objekt-ID.
- `objectState=observed`: `objects` enthält ausschließlich die tatsächlich mit
  dem platzierten nativen Block schneidenden Objekte. Kein Treffer bedeutet
  leeres Array. Bloße Nachbarschaft oder gemeinsamer Anker genügt nicht.
- `objects[].id`: Entity-ID, bei fehlender Komponente/Identität null.
  `objects[].template`: Template-Typ, bei fehlender Komponente null. Keine
  erfundenen Typen oder internen Unity-Objektnamen. Unbekannte Identität hebt
  die positiv beobachtete Objektkollision nicht auf.
- Höchstens 32 Objektbelege je Zelle, nach ID/Template sortiert. `truncated=true`
  bedeutet weitere Treffer; das Array ist dann kein vollständiges Inventar.
  Dasselbe Objekt kann an mehreren tatsächlich betroffenen Prüfzellen erscheinen.
- `objectState=outside_map`: Objektbelegung nicht beobachtet, `objects=null`,
  `intersectsTerrain=null`. Nicht als freie Zelle behandeln.
- Fehlendes/null `collision` bei älterem Paket bedeutet unbekannte Detailbelege,
  nicht keine Kollision. Bestehendes `intersectsObject` bleibt erhalten.

Die Mod verwendet weiterhin `GetObjectsAt` und `IsIntersecting` mit den
gedrehten nativen Blöcken. Vorschauobjekte und Rückgewinnungsstapel bleiben wie
bisher ausgeschlossen. Rückgewinnungsstapel werden weder gelöscht noch als
Abrissziele ausgegeben. Native Gründe, assessment und gameValidated bleiben
unverändert. Keine Vorschau, Mutation, Baufreigabe, automatische Räumung oder
zusätzliche Registry-Gesamtsuche. Eingangshindernisse bleiben beim bisherigen
`entranceOccupants`; weitere Werkzeuge/Familien werden nicht erweitert.

Der MCP-Vertrag prüft Konsistenz zwischen Objektliste und `intersectsObject`,
Kartenstatus, Terrain-null, Kürzung, IDs und Templates. Zwölf synthetische
Testfälle decken Altantworten, Terrain/Objekt getrennt, unbekannte Identität,
Kartenausschluss, 32er-Grenze, widersprüchliche Daten und HTTP-Transport ab.
Enthalten ist eine Sammlung von zwölf gezielt ungültigen Belegen. Alle zwölf
Testfälle im menschlichen Gate bestanden; keine Agent-Builds oder Testläufe.

## Verwendeter begrenzter Liveplan

1. Nach Gate Backend starten; Kennungen und neue Session bestätigen.
2. Bekannte Gebäudekollision: einen kleinen Lagergrundriss über ein vorhandenes
   Gebäude legen. Gemeldete ID/Template und betroffene native Zelle unabhängig
   mit Gebäudesuche/Geometrie bzw. `inspect_removal_targets` abgleichen.
3. Vegetationskollision an einer vorhandenen Pflanze: ID und Template getrennt
   bestätigen. Keine Entfernung zur Prüfung erforderlich.
4. Kontrollfälle: freie Bodenfläche ohne Objektbelege, unterirdische Zelle mit
   Terrainkollision und Kartenaußenbereich mit unbekannter Belegung. Unterirdisch
   vorhandene Objekte ggf. als zusätzlichen Befund ausweisen, nicht weginterpretieren.
5. Bei einer widersprüchlichen Identität/Zellzuordnung stoppen; keine Fallserie.
   Alle Antworten bleiben Vorprüfungen ohne Spielvalidierung oder Baufreigabe.
   Unbekannte Objektidentität und Kürzung nur synthetisch, sofern nicht natürlich
   beobachtet; keine zusätzliche Fehler-Injektion für diesen Pilot.

## Gate und Livebelege

- Gate: 2304 Unit-, 19 Integrations-, 13 Paket- und 35 Supervisor-Prüfungen
  bestanden; drei optionale Livefälle übersprungen. Beide Builds ohne Warnungen
  oder Fehler, fünf installierte Dateien per SHA-256 bestätigt. Neue MCP-Build-
  und Katalogkennung stimmen mit dem Gate überein.
- Prüflager Cw90 bei (29,30,3): genau DistrictCenter.Folktails als Ursache,
  ID und betroffene Zelle stimmen mit unabhängig gelesenen Removal-Belegen
  überein. `object_intersection`, Terrain false, keine Kürzung.
- Prüflager bei (30,31,3): genau Carrot mit passender ID und Zellbelegung.
  Zusätzlich geprüfter Kandidat (28,34,3) war entgegen der anfänglichen
  Freiflächenvermutung mit Wheat belegt; ebenfalls korrekte ID/Typ-Zuordnung.
- Unterirdisch (30,31,2): Terrain true, Objektliste beobachtet leer,
  `intersectsObject=false`; keine erfundene Terrain-ID. Bestehende Gründe
  `terrain_intersection` und `required_support_missing` erhalten.
- Außerhalb bei (50,31,3): `outside_map`, `insideMap=false`, Terrain und
  Objektliste null. Das bisherige false bei intersectsObject wird durch den
  ausdrücklichen Nichtbeobachtungsstatus eingeordnet, nicht als freie Zelle.
- Freie Kontrolle (33,30,4): onGround=true, Terrain false, Objektliste leer,
  keine Gründe und assessment=requires_game_validation. Unabhängige genaue
  Einzellenabfrage meldet null Objekte. Die zunächst gewählte Ebene z=3 war
  Terrain und wurde korrekt so diagnostiziert; kein widersprüchlicher Befund.
- In allen Fällen gameValidated=false. Rein lesend, keine Entfernung, Bau-
  oder Simulationsaufträge. Spieltempo 0, kein aktiver Lauf/Tempoauftrag.

Unbekannte Objektidentität, Altantworten und Kürzung sind synthetisch geprüft;
keine natürlich auftretende Identitätslücke oder mehr als 32 Treffer live
beobachtet. Keine Abnahme weiterer Grundrisse/Diagnosefamilien. Keine Rohlogs,
Spiel-IDs oder Aufnahmen im Repository.

## Bewertung

Hoher praktischer Nutzen bei kleinem bis mittlerem Aufwand über Mod, Transport
und Schema: konkrete Ursache direkt an der betroffenen Zelle statt zusätzlicher
breiter Suchabfrage. Keine neue Abhängigkeit oder Architekturänderung. Die
Grenze ist bewusst eine lesende Kollisionsdiagnose, kein allgemeiner Räumplan.
Der Pilot bestätigt den Nutzen: konkrete Ursachen ohne zusätzliche Suchabfrage
im normalen Vorprüfungsablauf. Aufwand begrenzt auf native Beobachtung, additive
Transportmodelle, Vertragsprüfung, Werkzeugbeschreibung und zwölf Testfälle.
Keine neue Abhängigkeit, kein Anlass für ein größeres Refactoring. Weitere
Diagnosefamilien bleiben unverändert; größere Abdeckung nur bei konkretem Bedarf.
