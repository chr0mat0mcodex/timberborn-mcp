# Spielanleitung für Agenten

Codex-Lifecycle (live abgenommen): vor der Testübergabe
`timberborn_server_control(action=stop)`, nach menschlichem `live bereit`
`action=start`, dann Spielstatus und aktuelle Session prüfen. Der Vorschaltprozess
bleibt verbunden. Unterbrochene Aufträge nie ungeprüft erneut senden.
[Einmalige Einrichtung und Grenzen](codex-mcp-supervisor.md).

Stand: Bridge 0.36.0; aktualisiert am 10. Oktober 2026.
Diese Anleitung beschreibt die vorhandenen MCP-Fähigkeiten. Aktueller Auftrag
und Projektregeln stehen in [AGENTS.md](../AGENTS.md) und
[missionsplan.md](../missionsplan.md); das lokale Spiel ist das freigegebene
Testsystem. Ein Entwicklungsauftrag ist kein fortlaufendes Kolonieziel.

## Einstieg in eine geladene Sitzung

1. `timberborn_status` für Bridge-Version und frische Sitzungskennung lesen;
   anschließend `inspect_simulation` für Geschwindigkeit und Spielzeit.
2. `inspect_building_capabilities(session, reasoning)` für Projektmodi,
   technische Grenzen und historische Nachweise lesen. Bei unbekanntem Profil
   die Fähigkeiten nicht aus einer ähnlichen Version ableiten.
3. Nur die für die Aufgabe nötigen Daten lesen: etwa `inspect_colony`,
   `inspect_goods`, `find_buildings`, `inspect_map_region`, `inspect_construction`
   oder `inspect_building`. Seiten anhand `hasMore` vollständig lesen, wenn eine
   Aussage Vollständigkeit voraussetzt. IDs und Bauorte aus aktuellen Antworten
   übernehmen, keine historischen Testkoordinaten als Voraussetzung verwenden.

Jeder Werkzeugaufruf braucht `reasoning`: kurze lesbare Absicht, 1–600 Zeichen.
Das Feld heißt nicht `reason`. Verbindliche Argumente und Grenzen stehen im
aktuellen MCP-Eingabeschema. Strukturierte Werkzeuge verwenden.

## Generisches Gebäude mit Anschlussweg bauen

Seit 0.35.0 braucht der ebene Projektpilot keine Gebäudenamensliste mehr.
`templateSelection=native_catalog_supported_geometry_with_road_entrance` benennt
den dynamischen Umfang. Dabei bedeutet `objectTemplates=[]` keine feste Liste;
zulässige Vorlagen werden im tatsächlichen Spielkatalog ermittelt.

1. `inspect_build_options` seitenweise lesen, maximal 32 Einträge pro Seite.
   Vorlage, Verfügbarkeit, Freischaltung, Geometrie und Kosten prüfen. `supported`
   ist kein Livenachweis. Globale Vorräte garantieren keine Materiallieferung.
   `inspect_build_catalog` ist dagegen nur der alte Katalog mit zwei Pilotvorlagen.
2. Fertiges Distriktzentrum und passende Wege bestimmen. `inspect_path_district`
   prüft die native Verbindung eines Wegobjekts zu diesem Distrikt.
   Relevante bestehende Gebäudezugänge mit `inspect_building_access` festhalten.
3. `precheck_building` für konkrete Standorte oder `plan_building_project` für
   eine kleine Suchfläche verwenden. Der vollständige gedrehte Grundriss zählt,
   einschließlich äußerer und höher liegender belegter Zellen. Ein freier
   Ursprung oder ein Weg neben dem Gebäude genügt nicht.
   Seit 0.35.1 berücksichtigt die Suche bereits das Limit von vier neuen Wegen:
   Überlange Kandidaten werden übersprungen, die Suche läuft danach weiter.
   Vorhandene Wege werden bevorzugt; bei gleichem Neubedarf die kürzere Route.
   Das ist keine globale Standortoptimierung und kein nativer Sicherheitsnachweis.
4. Ein Plan enthält Ursprung, Drehung, Eingang, Anschlussziel, Wegzellen und
   `planKey`. Suchparameter unverändert mit `optionIndex` und `planKey` an
   `validate_building_project` geben. Planoptionen sind noch keine Bauaufträge.
5. Im Prüfbericht Platzierung, Wegpräfixe, Eingang, Bestandsverbindungen und
   Vorschau-Wiederherstellung beurteilen. `valid=true` allein genügt nicht.
   `blocked` bezeichnet einen konkreten Fehler, `unknown` eine Nachweislücke.
   Im Entwicklungspiloten bleibt ausschließlich die dokumentierte Bauphasen-
   Vorabnachweislücke zulässig; bekannte Fehler oder Verluste bleiben Ablehnungen.
6. Pausiert und ohne unabhängige offene Baustellen
   `execute_building_project_pilot` mit denselben Planparametern,
   `mode=development_pilot` und neuer `actionId` ausführen. Der Server prüft
   frisch und platziert Wege und Gebäude schrittweise über die Spielservices.
7. `inspect_building_project` bis zum bestätigten Ergebnis lesen.
   `completed` bestätigt Auftrag und Zugang. Danach mit `inspect_building`
   und `inspect_building_access` den tatsächlichen Baustellenzustand und
   `buildersReachable` prüfen.
8. Falls nötig einen begrenzten Lauf mit `run_simulation_for` oder
   `run_simulation_until` starten; Status über `inspect_simulation_run` verfolgen.
   Fertigstellung, tatsächlichen freien/zugänglichen Eingang, neue Wege und
   relevante Bestandsanschlüsse anschließend erneut lesen. Automatische Pause
   und erreichtes Zeitziel prüfen. Betrieb oder Lagerbelegung separat behandeln.

### Aktuelle technische Grenzen

- Suchfläche maximal 8x8, Objektgeometrie maximal 64 Zellen, eine Drehung und
  eine Bodenwegebene pro Anfrage; Grundriss und gesamte Route innerhalb der Suche.
- Höchstens vier neue Bodenwege und vier Projekte nacheinander pro Sitzung.
  Neues Projekt erst nach abgeschlossenem Vorgänger und fertigen Baustellen.
- Öffentliche Spielgeometrie liefert derzeit einen Gebäude-Eingang. Mehrere
  beobachtete `constructionAccess.cells` sind Bauzugänge, keine weiteren Türen.
- Ebenes Routing erfindet keine Verbindung über einen Höhenunterschied.
  `execute_vertical_stair_pilot` bietet separate feste Treppen-/Plattformmodi;
  Umfang und gemeinsame Sitzungsgrenze aus dem Fähigkeitsprofil lesen. Der feste
  vertikale Lagerablauf verwendet weiterhin SmallWarehouse.Folktails.
- Allgemeiner Bauphasen-Vorabnachweis weiterhin unvollständig; reguläres
  `place_building` bleibt bei fehlendem `safe` gesperrt. Die generischen
  Projektpiloten besitzen ihren eigenen ausdrücklich bezeichneten Modus.

## Hindernisse, Fehler und Wiederholungen

Bei leerem Plan einen konkreten Grundriss und seine Hindernisse prüfen, bevor
weitere Flächen durchsucht werden. `inspect_removal_targets` liest überlappende
Objekte mit exakten IDs. Freigegebene Räumung erfolgt über die passenden
Entfernungswerkzeuge. `marked=true` bestätigt einen Auftrag; erst `removed` oder
das Rücklesen der freien Zelle bestätigt die Entfernung. Pflanzmarkierungen
können Nachwuchs auslösen und werden separat behandelt.

Bei `stopped`, `unconfirmed` oder Verbindungsabbruch zunächst Projektstatus und
reale Objekte lesen. Keine neue ID als Umgehung eines unklaren Ergebnisses senden.
Identische `actionId` und Parameter liefern den gespeicherten Projektstatus;
dies erzeugt keine weiteren Bauobjekte. Das Ledger endet mit der Spielsitzung.

## Nachweisstand richtig nutzen

In 0.35.0 sind Bank und großes 3x3-Freiluftlager, jeweils Drehung 1, mit neuen
Wegen, Bau-/Fertigzugang, Bestandskontrolle und Replay live belegt. Eine einzelne
Kollision am äußeren Eckfeld verhinderte den großen Bau korrekt. Eine gefährliche
Kontrollvorschau erkannte 24 verlorene Verbindungen, einschließlich eines Zugangs
auf der oberen Ebene. [Details](generic-building-project.md).

Seit 0.35.1 enthält das Fähigkeitsprofil auch diese beiden historischen Abnahmen.
Die Fünf-/Vier-Wege-Grenze und das Weitersuchen nach einem zu langen Kandidaten
sind in einem kontrollierten ebenen Streifen live geprüft; Anschluss um einen
Baum samt Bau-/Fertigzugang separat bestanden. [Details](road-budget-planner.md).
Historische Nachweise ersetzen keine frische Standortprüfung. Spiel-Multi-Eingänge,
Direkt-fertig-Distriktzentrum und beliebige Katalogobjekte sind nicht pauschal
live abgenommen. Lagergut, Personal, Produktion und Versorgung separat prüfen.

## Holzdiagnose und kompakte Chargen ab 0.35.5

Bei Pause inspect_forestry für eine begrenzte Holzfällerregion verwenden.
Ausgewachsen bedeutet nicht fällbar: Baumreste und fehlender Holzertrag sind
separat ausgeschlossen. Nur angebotene IDs an mark_forestry übergeben;
completed bestätigt Markierungen, keine Ernte. Gleiche actionId liest den Beleg.
run_simulation_for kann mit stopAtAvailableLogs früh pausieren; stockTargetReached
vom normalen Zeitlimit targetReached unterscheiden. Aktuelle Pause und Ernte
bei Bedarf getrennt nachlesen. Bauchargen sind standardmäßig kompakt;
details=true nur für Such-/Verlaufsdiagnose. build_rejected ist terminal,
keine Wiederholung mit neuer ID ohne Diagnose und geänderte Voraussetzungen.
[Liveumfang und Grenzen](forestry-efficiency-0.35.5.md).

## Optionales Spielbild ab 0.36.0

Wenn der Spieler im Chat „schau mal“, „zeig ich dir“ oder sinngemäß darum bittet,
die aktuelle Ansicht anzusehen: aktuelle Sitzung ermitteln und einmal
capture_screenshot aufrufen. Der Spieler stellt zuvor Kamera und UI im Spiel
selbst ein; diese Ansicht unverändert erfassen. Das zurückgegebene Bild ansehen
und auf den gezeigten Sachverhalt antworten. Die Aufnahme entsteht beim Aufruf,
nicht rückwirkend beim Absenden der Nachricht. Kein Ingame-Knopf erforderlich.

capture_screenshot mit aktueller session liefert auf ausdrücklichen Abruf ein
Spielbild inklusive UI. Standard 1280×720; kleinere maxWidth/maxHeight sparen
Bilddaten, reduzieren aber die Lesbarkeit. Metadaten enthalten Position,
absolute Welthöhe (Unity Y), Blickrichtung, Zielabstand, Bildwinkel und Bildweite
auf der Ebene durch den Kamerazielpunkt. Keine Höhe über Gelände und keine
sichtbare Geländegrundfläche daraus behaupten. Nur ergänzende Orientierung;
Spielaktionen weiterhin auf strukturierte Zustandsdaten stützen. Keine Dateien
oder Bildhistorie anlegen. Mindestens zwei Sekunden zwischen Aufnahmen;
bei screenshot_unavailable Zustand klären, kein automatischer Retry.
[Nachweis und Grenzen](screenshot-0.36.0.md).

## Bauchargen-Frühabschluss (live geprüft am 2026-10-10)

Nach bestandenem Entwickler-Gate und Livetest nutzt advance bei fertig
beobachteten Bauobjekten die frühe Pause des zugehörigen Laufs. Erst bestätigte
Pause und frischer Zugang erlauben den Folgeauftrag. constructionHours bleibt
Obergrenze; waitSeconds begrenzt nur Warten, nicht sofort mögliche Übergänge.
Nach Rückgabe weiter advance derselben Charge aufrufen; kein Hintergrunddispatcher.
Ein cancelled-Lauf mit cancel_requested kann nach vollständiger Abschlussprüfung
zu finished_accessible gehören. inspect bleibt lesend; unbestätigte Pause zuerst
lesend klären. Zwei Lager mit Frühpause und unmittelbarem Folgeauftrag live geprüft.

## Live abgenommen: Rampengeometrie, Höhen und Suchabdeckung

survey_region erhält scanOtherHeights (Standard true), maxHeights (Standard 4,
1–8 inklusive Ausgangshöhe) und maxWindows (Standard 3, 1–9 je Höhe). Zusätzliche
Höhen samt Kandidaten stehen in additionalLevels; vorhandene Wege priorisieren
die Auswahl, sind ohne separate Abfrage kein Distriktbeleg. coverage.heights
zeigt ausgelassene Höhen. planningRows: u ungeprüft, p teilweise, c alle vier
Drehungen vollständig im Fenster gesucht, niemals automatisch bauvalidiert.
Native overlapCells lokalisieren Rampen/Hindernisse. Bei älteren Bridges ohne
Geometrie begrenzte Verfeinerung; verbleibendes unknown samt Budgetflag beachten.
planBuildings=false/workBuildingId behalten die einzelne angefragte Ebene.
Beide zuvor übersehenen Gebiete live geprüft: [Umfang und Nachweise](regional-survey.md).
