# Baustellen und Distriktzuordnung — 0.6.0

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
- `remainingRequiredGoods`: höchstens 32 vom Spiel noch benötigt gemeldete Güter.
  Nicht globaler Lagerbestand und keine Lieferzusage.
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
