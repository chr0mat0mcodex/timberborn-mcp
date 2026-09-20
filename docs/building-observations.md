# Baustellen und Distriktzuordnung — 0.6.1

## Materialkorrektur 0.6.1 — begrenzt live bestätigt

0.6.1 ist installiert. Gespeichertes Farmhaus live gelesen: Baukosten 25 Log,
Inventar vorhanden und leer, global 0 Log. Baustelle aktiv/unfertig/ungestartet,
Fortschritt 0 und bekannter Baudistrikt. Vier reine Leseaufrufe, keine Mutation.
Materiallieferung und Fortschrittsverlauf sind noch nicht beobachtet.

Die nicht geklärte Restbedarfsmethode wird nicht mehr aufgerufen. Öffentliche
BuildingSpec.BuildingCost und ConstructionSite.Inventory.Stock liefern getrennt:

- `construction.materials.buildingCosts`: gesamte Baukosten laut Vorlage.
- `inventoryAvailable`: ob das Baustelleninventar tatsächlich verfügbar ist.
- `siteStock`: aktuell darin enthaltene Güter; null bei unbekanntem Inventar,
  leere Liste bei vorhandenem leerem Inventar. Höchstens 32 eindeutige Güter je Liste.

Keine Subtraktion als behaupteter Restbedarf: bereits verbaute Materialien und
Lieferungen unterwegs fehlen in diesen Beobachtungen. Das alte Feld
`remainingRequiredGoods` entfällt. Baustellenantworten aus 0.6.0 werden vom neuen
Client mangels Materialvertrag als inkompatibel abgewiesen; fertige Objekte bleiben
lesbar. Die Ursache des früheren Nullwerts ist nicht geklärt. Lokale Referenzen und
gezielte öffentliche Suche lieferten keine belastbare Methodenbeschreibung;
kein Decompiling, keine privaten Felder und keine neue Abhängigkeit erforderlich.

Nach Update höchstens fünf reine Leseaufrufe am Nutzer-Farmhaus: Version, Gebäudeliste,
Gebäudedetails und Koloniebestände. Kosten, Baustellenbestand und globale Vorräte
getrennt ausweisen. Baustelle muss im geladenen Spielstand vorhanden sein.

## Historischer Baustellennachweis 0.6.0 und Materialgrenze

Nutzerplatziertes EfficientFarmHouse.Folktails live gelesen: unfertig, aktiv, ungestartet,
Material-/Baufortschritt 0, kein Material zur Fortsetzung, bekannter Baudistrikt.
RemainingRequiredGoods meldet dabei Log=0. Diese Methodensemantik ist noch ungeklärt:
den Wert nicht als verlässlichen Restbedarf oder Materialfreiheit verwenden. Baukosten
und Baustelleninventar müssen separat nachgewiesen werden. Kein zeitlicher Baufortschritt
beobachtet und kein Hausauftrag über MCP erteilt. Vier reine Leseaufrufe.

Die folgenden Abschnitte dokumentieren Vertrag und vorherigen Test an fertigen Objekten.

Rein lesende Erweiterung vor einem Hausauftrag. Keine neue Abhängigkeit oder Bauaktion.
Gebaut, synthetisch getestet und auf 0.6.0 begrenzt live geprüft: fertiges District
Center und Holzfällerflagge mit bekannter Betriebs-/Instant-Distrikt-ID; fertiger Path
ohne Zuordnungskomponente. Unbekannte Entity und fremde Sitzung geprüft. Neun fachliche
Leseaufrufe, keine beobachtete Objektzahl-/Bestandsänderung. Im geladenen Stand keine
Baustelle: Material-/Baufortschritt bleibt live unbelegt. Die frühere Testweg-ID war
nicht vorhanden; stattdessen bestehenden Path gelesen, keinen neuen gebaut.

`inspect_building(id, session)` liest eine Entity derselben Spielsitzung. ID stammt
aus find_buildings oder Bau-Receipt, session aus dessen Metadaten. Die Mod prüft
die Sitzung auf dem Hauptthread vor der Entity-Auflösung. Feste GET-Route
`/agent-api/v1/building`, keine generische HTTP-Weiterleitung.

- `found=false`, `details=null`: Entity fehlt, ist gelöscht/nicht initialisiert,
  ein Preview oder kein Gebäude/Path. Kein erfundener Leerzustand.
- `finished`/`unfinished`: direkte BlockObject-Beobachtung, keine Prognose.
- `constructionComponentPresent`: Baustellenkomponente vorhanden. Details nur bei
  unfertigem Objekt; fertige Objekte können die Komponente behalten.
- `construction`: an/gestartet/baubereit, Material-/Bauzeitfortschritt, bisherige
  Bauzeit in Stunden, Material zur Wiederaufnahme vorhanden und fertigstellbar.
  Direkte Spielwerte, keine prozentuale Umrechnung vor Live-Abgleich.
- `remainingRequiredGoods` (nur alter Vertrag 0.6.0): nicht verlässlich interpretiert,
  in 0.6.1 durch getrennte Kosten-/Inventardaten ersetzt.
- `district.componentPresent`: Zuordnungskomponente verfügbar. Bei false sind alle
  IDs null; daraus keine bestätigte Trennung vom Wegenetz ableiten.
- `assignedDistrictId`, `instantDistrictId`, `constructionDistrictId`: getrennte
  öffentliche DistrictBuilding-Zuordnungen. Null bedeutet keine Zuordnung in diesem
  Feld; nicht zu einem vermeintlich sicheren erreichbar=true zusammenfassen.

Distrikt-IDs referenzieren DistrictCenter-Entities, keine privaten Namen. Navigation
kann verzögert aktualisieren. Ein Baudistrikt garantiert keine freien Bauarbeiter,
Materialversorgung oder Fertigstellung. Paths können keine DistrictBuilding-Komponente
haben; das ist eine API-Grenze, kein Beweis einer Weglücke.

Öffentliche APIs lokal nachgewiesen: ConstructionSite-Leseeigenschaften und
RemainingRequiredGoods, DistrictBuilding.District/InstantDistrict/ConstructionDistrict.
Keine Assign/Unassign-, FinishNow-, Inventaränderungs- oder Tick-Aufrufe.

## Begrenzte Live-Abnahme

Nach Update maximal zehn fachliche Leseaufrufe: Version/Sitzung, Gebäudeliste,
bis zu drei repräsentative Objekte (District Center, Holzfällerflagge, neuer Testweg).
IDs, Vorlagen, Position und Fertigstatus gegen Liste abgleichen; Distrikt-IDs gegen
gelistete DistrictCenter-Entities prüfen. Fehlende Komponenten ausdrücklich als
unbekannt dokumentieren. Unbekannte gültige ID darf found=false liefern; alte Sitzung
muss abgewiesen werden. Keine Screenshots, Eingabesimulation oder Bauaufträge.

Ohne Baustelle bleiben Material-/Baufortschritt live unbelegt. Dieser Nachweis folgt
beim begrenzten Hausauftrag; kein Erfolg aus synthetischen Tests oder fertigen Paths.
