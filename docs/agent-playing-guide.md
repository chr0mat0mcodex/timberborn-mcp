# Spielanleitung für Agenten

## Bauarbeitszugang: Policy 1 begrenzt live abgenommen

Für neue Gebäude bevorzugt `plan_large_project_access`: Zielgebäude vorgeben,
Wege/Treppen/Stützen automatisch ergänzen lassen. Bei der neuen Policy muss der
verschachtelte Plan `accessPolicyVersion=1` melden und jede erforderliche
`constructionWorkAccess`-Prüfung `road_reachable`. Bei Ablehnung `blockedPart`,
`phase` und Teilbericht auswerten; keine teilweise erzeugten Schritte bauen.
Zur Ausführung nur den vollständigen zurückgegebenen Plan samt Schlüssel verwenden.
Zugangshilfen müssen als Abhängigkeiten vor ihrem Ziel fertig werden; die Mod
prüft den aktuellen Weganschluss unmittelbar vor jedem Gebäudeauftrag erneut.

`road_reachable` belegt einen Distriktweg zu einer geometrischen Arbeitszelle,
noch keinen nativen Bauarbeiterzugang. `helper_runtime_check` ist eine ausdrücklich
ungeprüfte Bauhilfe. Tatsächlichen Baustellenzugang und fertigen Eingang weiterhin
separat bestätigen; bei Blockade Diagnose/Reparaturablauf verwenden, nicht wiederholen.
Alte Antworten ohne Policy sind kein Nachweis der neuen Prüfung. Erst nach Gate
und bestandenem Pilot gemäß [Abnahmeplan](automatic-access-planning.md) als getestet behandeln.
Temporäre Hilfen nur nach Abschluss und frischer Zugangsprüfung entfernen;
Rückgewinnungsrohstoffe niemals löschen.

## Automatische Zugangsplanung — begrenzt live abgenommen

Für vorgegebene Gebäudeziele `plan_large_project_access` verwenden: ergänzt
Wege, Treppen, Stützen und Abhängigkeiten im begrenzten Umfeld und prüft den
Gesamtplan. Nur bei `candidateFound=true` die zurückgegebenen `steps` und
`plan.planKey` unverändert an `run_large_project` übergeben. Kein Bau im Planaufruf.
`assessment` und bei Ablehnung `reason` lesen; nicht mit den ursprünglichen,
unergänzten Schritten starten. Nicht gefundene Route bedeutet keine generelle
Unbebaubarkeit. Arbeitszelle ist vorgeschlagen, echter Baustellenzugang ungeprüft.
`cleanupCandidateIndices` nach Fertigstellung erneut auf Bestandszugänge prüfen;
Rohstoffstapel bleiben erhalten. [Umfang und Dreifall-Pilot](automatic-access-planning.md).

## Projektabschluss und Reparatur — begrenzter Livetest bestanden

Fünfteilige Zugangsreparatur samt Originalbau, Abschluss und Folgeprojekt in
derselben Sitzung geprüft. Belegte Wegzelle und fremde Baustelle abgewiesen:

- `diagnose_large_project` liest aktuelle Objektidentität, Bauzugang, Material-
  bereitschaft und betroffene Bestandsverbindungen. `lastGuardFailure` bzw.
  `lastValidationFailure` sind historische Fehlerbelege, andere Werte frisch.
- Terminales Projekt bei Pause mit `close_large_project` abschließen.
  Ausschließlich `closeOutcome=released/already_released` bestätigt Freigabe;
  `rejected` anhand der Diagnose klären. Restteile werden aufgegeben, nicht gebaut.
  Unbekannte Wirkungen nie durch eine neue ID umgehen.
- Fehlender Bauzugang: Projekt stoppen, terminalen Zustand und Pause nachlesen,
  dann `plan_large_project_repair` mit `parentActionId` und zusätzlichen Wegen,
  Treppen/Plattformen. Bekannte Originalbaustellen sind erlaubter Bestand;
  keine fremden Baustellen oder verschachtelten Reparaturen einschließen.
- `run_large_project_repair` mit neuer ID, frischem Plan und festem Budget einmal
  starten. Die Charge wartet auch auf beauftragte Originalteile; keine Wiederholung
  ursprünglicher Platzierungen. Danach Originalprojekt schließen, Rest frisch planen.
- Temporäre Bauten einschließlich Straßen anschließend gezielt entfernen und
  rücklesen. Dauerhafte Gebäudezugänge zuvor prüfen. Rückgewinnungsstapel nicht
  mit `remove_debris` löschen: Rohstoffe erhalten und regulär einsammeln lassen;
  nötige Zugänge bis zur Sammlung belassen. Nach menschlichem Eingriff frisch lesen.

Grenzen und konkreter Pilot: [Projektwiederherstellung](project-recovery.md).

## Autonome Gesamtpläne — begrenzter Livetest bestanden

Für vorbereitete Gesamtpläne `plan_large_project` → **einmal `run_large_project`** verwenden.
Gleiche Planparameter plus `actionId`, `mode=development_pilot`, `speed` (1/3/7),
`maxGameHours` (1–672) und `maxRealSeconds` (30–86400). Pausiert starten; die Mod
erteilt alle unabhängigen Aufträge gemeinsam und startet dann die Simulation.
Echte Stütz-/Zugangsabhängigkeiten mit `dependsOn` angeben; keine künstlichen
Ketten unabhängiger Gebäude. Zurückgestellte Teile erscheinen in `deferredParts`
und werden nach Fertigstellung ihrer Abhängigkeiten automatisch beauftragt.

Kein Advance und keine separaten Simulationsläufe pro Bauteil. Optional den Stand
mit `inspect_large_project` lesen; das Lesen löst nichts aus. `completed` erfordert
alle fertigen Teile, anwendbare Zugänge und bestätigte Pause. `blockedPart`/`reason`
erklären Stopps; unbekannte Wirkung nicht erneut beauftragen. Nach MCP-Abbruch
dieselbe Action-ID lesen, nach Spielneuladen neue Session beachten. Die Mod läuft
bei MCP-Trennung weiter; Tempowechsel unterbricht, Stop pausiert die Charge.
[Vertrag und Abnahmeplan](autonomous-construction-batches.md).

Die folgenden Einzelschritt-Anweisungen beschreiben den manuellen Altmodus.
Die automatische Standortsuche/Räumung von `start_building_batch` ist weiterhin
ein separater älterer Ablauf, kein Hintergrunddispatcher für Gesamtpläne.

## Erweiterung 0.37.0: größere Bauvorhaben (begrenzter Livepilot bestanden)

Nach Stützenkorrektur: `placementState=deferred_native_validation` bedeutet
noch keine gültige Geometrie. Direkt abhängige geplante Träger müssen zuerst
real fertig sein; Advance muss dann die vollständige native Prüfung bestehen.
Nicht als bestandene Vorschau oder Ausführungsfreigabe zusammenfassen.

Nur mit bestätigter Bridge 0.37.0: `plan_large_project` → `start_large_project`
→ `advance_large_project`. Explizit alle Stützen, Treppen, Wege, Gebäude und
Kraftstücke mit `dependsOn` angeben. Höchstens 32 Teile, 32×32 Grundfläche und
acht belegte Höhenebenen einschließlich Gebäudevolumen. Keine automatische
3D-Geländeroutensuche annehmen. Katalog/Freischaltung zuerst prüfen.

Plan/Start/Advance verlangen Pause. Ein Advance setzt höchstens einen Auftrag;
Vorgänger müssen wirklich fertig und anwendbare Zugänge nachgewiesen sein.
Bei `waiting` begrenzte Simulation separat ausführen, Pause bestätigen und
erneut Advance. `inspect_large_project` liest nur den gespeicherten Stand.
`stop_large_project` behält bestehende Objekte. Bei `unconfirmed`/`stopped`
diagnostizieren, keine neue Action-ID als Retry. `pilotEligible` bedeutet keine
reguläre Baufreigabe und keinen vollständigen Bauarbeiter-Vorabnachweis.

Kraftlinks im Plan sind Anschlussgeometrie. Nach dem Bau
`inspect_power_network` für reale Ports/Netzleistung lesen, Betrieb getrennt
prüfen. Entrancelose Bauteile brauchen keinen erfundenen Gebäude-Eingang.
Neue Distrikte, Gelände-Seitenanbauten und Mehrfacheingänge bleiben ausgeschlossen.
[Genaue Parameter, Zustände und Livepilot](large-projects-0.37.0.md).
Fünfteiliger Stützenpilot am 2026-10-10 vollständig fertiggestellt; Grenzen und
Einzelnachweise stehen im verlinkten Pilotbericht, keine pauschale Katalogabnahme.

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
