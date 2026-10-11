# Native MCP-Werkzeuge

## 0.44: Messmodus im Produktionslauf (begrenzt live abgenommen)

`run_simulation_for(..., buildingIds, measureProduction=true)` misst bis duration
statt zum Erstzyklusabschluss. Maximal acht Gebäude, keine Mischung mit Bestands-
oder Forschungszielen. `buildingProduction[].measurement` liefert native Zyklen,
rezeptabgeleitete Mengen/Raten und exklusive beobachtete Zeitanteile in Spielstunden.
`production_measurement_complete` ist Zeitfensterabschluss; Produktionsnachweis
bleibt separat. Historische Messung eingefroren, frische Engpassdiagnose weiterhin
separat datiert. Kein zusätzliches Werkzeug. [Vertrag](production-throughput.md).

## 0.43: gemeinsame Engpassdiagnose (begrenzt live abgenommen)

`inspect_building_operations(session, ids, diagnose=true, detail?)`: bis acht IDs,
Zutaten/Brennstoff, Quellen/Produktlager und native Wege, Personal/Kraft gemeinsam.
`summary.bottleneck` enthält Belege und Abdeckung, `assessment.findings` nächste
Schritte mit Bereich/Code/Schwere/Gut. Ohne diagnose bleibt die 32er-Übersicht.
`inspect_simulation_run` ergänzt für buildingIds aktuelle Betriebsdaten unter
currentBuildingOperations, bottleneckObservedGameHours und bottleneckState;
historische Produktionsbeweise bleiben getrennt. [Vertrag](production-bottlenecks.md).

## 0.42: gemeinsame Gebäudeeinrichtung (begrenzt live abgenommen)

`configure_buildings(session, jobId, items, proofHours?, proofSpeed?, proofMaxRealSeconds?)`:
1–8 eindeutige fertige Manufakturen, native Rezeptwahl, Sollpersonal, Pause;
alle expected-Werte erforderlich. Gemeinsame Vorprüfung, Teilfehler ohne Rollback.
`inspect_building_configuration(session, jobId)` liest die unveränderte Quittung.
Optionaler Produktionslauf verwendet den vorhandenen Lauf mit runId=jobId.
[Parameter, Fehler und Abnahmeplan](building-configuration-batch.md).

## 0.41: Erweiterter Produktionslauf (begrenzt live abgenommen)

`run_simulation_for` nimmt optional `buildingIds` (1–8 eindeutige IDs) entgegen,
exklusiv zu Güter-/Forschungs-/Holzzielen. `inspect_simulation_run` liefert
`buildingProduction`, `buildingProductionReached` und `buildingJournalCursor`.
Nur native Produktionsabschlüsse zählen. Konfigurationsänderungen und fehlende
Beobachtung invalidieren den Lauf; Zeitgrenzen und Pause bleiben im bisherigen
Controller. Keine zusätzlichen MCP-Werkzeuge.
[Vertrag und Nachweisgrenzen](building-production-proof.md).

## 0.40.0 — Änderungsjournal begrenzt live abgenommen

`inspect_recent_actions(session, limit=32, after?, kind=all, source=all)`:
letzte X Änderungen oder seit Cursor, maximal 64, 512 im sitzungsgebundenen
Speicher. Vorher/Nachher, Objekt/Position, Sequenz und Aufnahme-/Spielzeit.
Herkunft nur mcp/unknown; keine Behauptung menschlicher Urheberschaft.
Cursor und Erfassungslücken ausdrücklich lesen. [Vertrag](recent-actions-journal.md).

## 0.39.0 — begrenzt live abgenommen

- `find_buildings`: Filter template/finished/productionOnly/includePaths und
  vollständiger Ankerbereich x/y/z/width/height/depth; kompakt als Standard,
  geometry nur bei detail=full. Wege standardmäßig ausgeschlossen.
- `inspect_building_operations(ids, session, detail)`: 1–32 eindeutige IDs,
  ein nativer Beobachtungsrequest, Einzelstatus observed/not_found. Kompakte
  Betriebs-Summary; full ergänzt Inventare und Statustexte.
- `inspect_building_operation`: zusätzlich power.active für korrekte Einordnung
  inaktiver Verbraucher; gemeinsame Diagnose mit der Sammelabfrage.

[Vertrag und begrenzter Pilot](building-query-batch.md).

## 0.38.0 — Gate und begrenzter Livetest bestanden

- `inspect_building_settings`: zusätzlich aktuelle/erlaubte Rezepte mit
  Zutaten, Produkten, Zyklusdauer, Forschung und Brennstoff.
- `set_building_recipe(id, session, recipe, expectedRecipe)`: native Rezeptwahl
  fertiger Gebäude; bestehendes Settings-Gate, Konfliktprüfung und Rücklesung.
- `inspect_building_operation`: zusätzlich nativer Kraftzustand und gebündelte
  `assessment` mit Blockaden, Warnungen und unbekannten Prüfpunkten; ein Request,
  kein Produktionsnachweis aus Bereitschaft.

[Vertrag und begrenzter Pilot](recipe-operation-settings.md).

## Live abgenommen: gesamte Waldreichweite und Fällschutz

Folgeoptimierung live bestanden: interne 256er-Seiten und beim Schutz
Markierungs-Rücklesung statt zweitem Waldscan. MCP-Parameter unverändert; gemessen
32 statt 54 `nativeReads` für Übersicht, 40 statt 113 für Zweibaum-Schutz.

`inspect_forestry`: optional `fullRange=true`; dann genügen `session` und
`workBuildingId`. Vollständige Artenzählung über die native Reichweite auf allen
Höhen, explizite Abdeckung und Aufwand; regionale Altparameter bleiben kompatibel.
`protect_forestry`: `session`, `workBuildingId`, `template`, `actionId`, optional
`maxTrees` (1–512, Standard 256). Entfernt aktuelle Fällmarkierungen lebender Bäume
der gewählten Art. Bestehender Flächen-Aktionsschalter, persistenter Teilbeleg,
Rücklesung und lesendes Replay. Keine dauerhafte Regel, Fällung oder Simulation.
Menschliches Gate und begrenzter Livetest bestanden. [Nachweis und Grenzen](forestry-range-protection.md).

## 512-Zellen-Geometrie — begrenzter Großbau-Nachweis

`inspect_build_options` ergänzt `geometryCellCount` und `maxGeometryCells`.
`inspect_building_capabilities` übernimmt das tatsächlich gemeldete Limit;
fehlende neue Felder bleiben ältere Pakete. Vorprüfung/Objektlisten liefern bis
512 native Zellen, Großprojekte behalten 32 Teile/32×32/acht Höhen.
Kein neuer Werkzeugname. Hex nur als feste Einzelvorlage mit nativer Validierung.
DomedGarden samt Zugang live fertig; native Hex-Platzierung und weitere
Sondertests auf Nutzerentscheidung zurückgestellt. [Nachweis und Grenzen](large-building-geometry.md).

## Live abgenommen: Mehrfachziele für Simulationsläufe

`run_simulation_for`: optional `goodTargets=[{good,amount},…]` (1–8 eindeutige
Güter) und `stopAtScience`, jeweils ganzzahlig 1–1.000.000. Nicht zusammen mit
dem alten Holz-Ziel. Antwort ergänzt beobachtete Zielbestände, Forschung,
`productionTargetsReached` und frische `productionDiagnostics` mit Zeitstempel.
Alle Ziele gleichzeitig; automatische Pause. `production_time_limit` und
`production_observation_unavailable` unterscheiden Budget und Beobachtungslücke
von `production_targets_reached`. Kein neues Werkzeug und keine Änderung am
Zeitziel-Aufruf `run_simulation_until`. [Details/Gate](production-target-runs.md).

## Bauarbeitszugang mit Policy 1 (begrenzt live abgenommen)

`plan_large_project_access` sucht mit zwölf Zellen Rand und liefert zusätzlich
`blockedPart`, `phase`, `workCellCount`. Der verschachtelte Plan sowie
`plan_large_project` liefern `accessPolicyVersion=1` und pro Teil
`constructionWorkAccess` mit `required`, `state`, `candidateCount`,
`reachableCells`, `source`, `nativeConstructionProven=false`.
Status: `road_reachable`, `no_work_cell`, `no_connected_work_cell`,
`final_work_access_blocked` oder `helper_runtime_check` für Wege/Zugangshilfen.
Fehlende erforderliche Wegnachweise blockieren Planung und Auftrag. Laufzeitfehler
`construction_work_access_rejected` enthält denselben Bericht in der Diagnose.
Keine neuen Aufrufparameter; Teil-/Zeitbudgets bleiben unverändert. Eine erreichbare
geometrische Arbeitszelle ist keine native hypothetische Baustellenfreigabe.
[Umfang, Grenzen, Testgate](automatic-access-planning.md).

## Automatische Zugangsplanung (begrenzt live abgenommen)

`plan_large_project_access` (POST/Baugate, pausiert): explizite Gebäudeziele mit
`session`, `districtId`, `steps`; ergänzt Wege/Stützen/Folktails-Treppen und
Abhängigkeiten, danach gemeinsame native Vorschau. Antwort: `candidateFound`,
`reason`, vollständige `steps`, `plan` mit `planKey`, `assessment` und
`cleanupCandidateIndices`. Kein Bauauftrag, keine allgemeine Baustellenfreigabe.
[Grenzen, Ausführungsübergabe und Testfälle](automatic-access-planning.md).

## Projektwiederherstellung (begrenzter Livetest bestanden)

| Werkzeug | Art | Zweck |
| --- | --- | --- |
| `diagnose_large_project` | Lesen/GET | Frische Teilzustände, Baustellenmaterial, Zugänge und Fehlerbelege |
| `close_large_project` | Schreiben/POST, Baugate | Geklärtes terminales Ledger freigeben; Restteile aufgeben |
| `plan_large_project_repair` | Vorschau/POST, Baugate | Zusätzliche Wege/Treppen/Plattformen für `parentActionId` prüfen |
| `run_large_project_repair` | Schreiben/POST, Bau- und Tempogate | Eigene Reparaturcharge mit festem Budget; Originalaufträge nicht wiederholen |

Sessionbindung, frischer Zustandsabgleich, fremde Baustellen ausgeschlossen.
`closeOutcome` entscheidet über den Abschluss, nicht allein Transportstatus `ok`.
[Vertrag und Testfälle](project-recovery.md).

## Autonome Gesamtcharge (begrenzter Livetest bestanden)

`run_large_project` (POST, Bau- **und** Tempofreigabe): gleicher expliziter Plan
wie Plan/Start, zusätzlich `speed`, `maxGameHours`, `maxRealSeconds`. Alle
unabhängigen Aufträge pausiert setzen, danach serverseitig bis fertig begleiten;
explizite Abhängigkeiten automatisch nachziehen. `inspect_large_project` liest
die automatisch aktualisierte Batchquittung, `stop_large_project` stoppt und
fordert eine Pause an. Keine Advance-Aufrufe pro Teil. [Details](autonomous-construction-batches.md).

## 0.37.0 — begrenzte Bau-/Stützenpiloten live bestanden

| Werkzeug | Art | Zweck |
| --- | --- | --- |
| `plan_large_project` | Vorschau/POST | Expliziter Plan bis 32 Teile, 32×32 und acht belegte Höhen, Abhängigkeiten und Kraftgeometrie |
| `start_large_project` | Schreiben/POST | Frisch validierten Plan registrieren, noch kein Auftrag |
| `advance_large_project` | Schreiben/POST | Fertigstellung prüfen, höchstens einen Folgeauftrag erteilen |
| `inspect_large_project` | Lesen/GET | Gespeicherte Projektquittung, keine Fortschrittsaktualisierung |
| `stop_large_project` | Schreiben/POST | Folgeaufträge stoppen, Objekte behalten |
| `inspect_power_network` | Lesen/GET | Echte Ports, Netzmitglieder und Leistung des angegebenen Objekts |

Alle mit Sessionbindung und `reasoning`; vier Schreibwerkzeuge über den
Gebäudebauschalter, zwei Leser unabhängig davon. Kein automatischer Wiederholungs-
oder Hintergrundbau **im manuellen Altmodus dieser Tabelle**.
[Parameter, Grenzen und Statusbedeutung](large-projects-0.37.0.md).

Live abgenommen: `timberborn_server_control` (`action: stop|start|status`)
kommt vom persistenten Codex-Vorschaltprozess, nicht aus der Spiel-Bridge.
Stop sperrt das Backend vor dem menschlichen Gate; Start erst nach `live bereit`.
[Einrichtung, Fehlerverhalten und Abnahme](codex-mcp-supervisor.md).

Verifizierte Bridge: 0.35.0, generischer ebener Projektpilot begrenzt live geprüft.
Praktischer Ablauf: [Spielanleitung für Agenten](agent-playing-guide.md).
Historische Versionsabschnitte unten sind keine allgemeine Baufreigabe;
aktueller Auftrag folgt ../missionsplan.md.
Etappe A ergänzt `assessment` in bestehenden Einzel-/Projekt-Validierungen;
freie/gesperrte Vorschau und Vorschauzugang live geprüft, keine neue Baufreigabe.
[Bericht und Grenzen](build-assessment.md).
[Grund und Ergebnisvertrag](road-protection.md). Der verfügbare Werkzeugumfang
ergibt sich aus `tools/list` und den aktiven technischen Aktionsschaltern.
Neu: Ingame-Zeitläufe, automatisch und im begrenzten MCP-Live-Pilot geprüft. Die 30 bisherigen Leser
sind unter 0.21.1 live belegt. Nachweise und Grenzen stehen in den Fachdokumenten.

## Leser

- `timberborn_status`
- `inspect_colony`
- `inspect_goods`
- `inspect_production_graph` — [Definitionsgraph, Quellen und Grenzen](production-dependency-graph.md)
- `inspect_needs`
- `inspect_beaver_needs`
- `inspect_building_operation`
- `inspect_building_access`
- `inspect_road_connection`
- `inspect_work_range`
- `inspect_good_history`
- `inspect_alerts`
- `inspect_alert_targets`
- `inspect_map_region`
- `find_buildings`
- `inspect_build_catalog`
- `precheck_build_site`
- `inspect_building`
- `inspect_simulation_run` — Status eines Zeitlaufs
- `inspect_simulation`
- `inspect_workforce`
- `inspect_building_priority`
- `inspect_construction`
- `inspect_area_types`
- `inspect_areas`
- `inspect_removal_targets`
- `inspect_build_options`
- `inspect_building_capabilities`
- `plan_building_project`
- `inspect_building_project`
- `inspect_vertical_stair_pilot`
- `inspect_path_district`
- `precheck_building`
- `inspect_building_settings`
- `inspect_research`
- `inspect_agent_log`

## Freizugebende Werkzeuge

- `set_workplace_staffing`
- `run_simulation_for`, `run_simulation_until`, `cancel_simulation_run`
- `set_simulation_speed`
- `validate_build_site`
- `place_path`
- `place_lodge`
- `set_building_priority`
- `set_area`
- `demolish_building`
- `remove_planted`
- `remove_vegetation`
- `remove_debris`
- `validate_building`
- `validate_building_project`
- `execute_building_project_pilot`
- `execute_vertical_stair_pilot`
- `place_building`
- `set_building_paused`
- `set_storage_good`
- `set_storage_mode`
- `set_farm_priority`
- `set_farm_crop`
- `unlock_building`

## Auswahl und Ausführung

- Für neue Gebäude mit Anschlussweg: inspect_building_capabilities,
  inspect_build_options, precheck_building/plan_building_project,
  validate_building_project und execute_building_project_pilot. Den vollständigen
  Ablauf einschließlich tatsächlichem Bau-/Fertigzugang beschreibt die Spielanleitung.
  validate_building prüft einzelne Objekte; place_building bleibt ohne safe-Wegschutz
  gesperrt. inspect_build_catalog/precheck_build_site sowie validate_build_site/
  place_path/place_lodge sind erhaltene, begrenzte frühe Pilotwerkzeuge.
- Validierung mit Vorschau zählt nicht als rein lesend und benötigt eine eigene Freigabe.
- Aktionsfreigaben werden in Mod und MCP-Prozess geprüft; [vollständige Tabelle](native-bridge-install.md).
- reasoning ist Pflicht für alle MCP-Werkzeugaufrufe, einschließlich Leseabfragen,
  Fake und Legacy: kurze für den Spieler lesbare Absicht, keine internen
  Gedankengänge. String mit 1–600 Zeichen, nicht nur Leerraum. Fehlende oder
  ungültige Begründung wird vor Backend-/Logzugriff als InvalidParams abgewiesen.
  Der bestehende Feldname bleibt reasoning; kein neues reason-Alias.
- Eingabeschemata, verlangte Session-/Erwartungswerte und Grenzen liefert MCP tools/list.
  Kein generischer HTTP-Aufruf und kein automatischer Backendwechsel.
- Ergebnis separat prüfen; applied ist kein Beleg für abgeschlossenen Bau oder Versorgung.
  [Fachliche Fehlercodes](bridge-errors.md).

Neue Güter-/Statusverträge: [Details und Grenzen](economy-observations.md).

Weg-, Reichweiten- und Güterhistorien sind über eigene strukturierte Werkzeuge verfügbar.
`roadProtection` ergänzt ab 0.23.2 Weg-/Baustellen-Prüfpunktzahlen und `affected.kind`; bei `road_cell` bezeichnet `entrance` die Wegkoordinate. Diese Diagnose erweitert keine Baufreigabe.

## Bauplan-Kandidatensuche (0.24.0)

`plan_building_project` ist ein zusätzlicher Leser. Eingaben: template, districtId,
session, x/y/z, width/height (1..8), rotation (0..3). Bis zu vier Vorschläge mit
Gebäudeursprung, Eingang, Anschlussziel und Wegzellen in Anschlussreihenfolge.
Alle Optionen executable=false; noch keine gemeinsame Spielvalidierung oder
Ausführung. Suchabdeckung und Abbruchgrund ausdrücklich lesen.
[Umfang, Grenzen und Pilot](building-site-search-plan.md).

`validate_building_project` (0.24.1) benötigt die Plan-Suchparameter plus optionIndex
und planKey. Bau-Opt-in erforderlich, maximal acht neue Wege und 16 Prüfungen/Sitzung.
Temporäre gemeinsame Vorschau, kein Bauauftrag; executable bleibt false. Ergebnisse
und Grenzen stehen im [Bauplan-Vertrag](building-site-search-plan.md).

## Generischer ebener Bauprojekt-Pilot (0.35.0)

`execute_building_project_pilot` setzt ein geeignetes Gebäude aus dem aktiven
Spielkatalog und höchstens vier neue ebene Wege. Vollständige Spielgeometrie und
anschließbarer öffentlicher Eingang entscheiden, keine Gebäudenamensliste.
Ältere Bridges behalten ihren versionierten festen Pilotumfang.
Benötigt Suchparameter, optionIndex, planKey, session, actionId und
`mode=development_pilot`. Bau-Opt-in und Pause sind Pflicht. Bis 0.31.3 ein Auftrag
je Sitzung; ab 0.31.4 höchstens vier Aufträge nacheinander, neue ID nur nach
completed und tatsächlicher Fertigstellung bisheriger Baustellen. Unabhängige
offene Baustellen verhindern den Start. stopped/unconfirmed sperren weitere Projekte. Alte ID+Parameter lesen
nur das gespeicherte Ergebnis, geänderte Parameter werden abgewiesen. completed
ist keine Fertigstellung; vor weiteren Bauvorhaben reale Objekte neu prüfen.
`inspect_building_project` liest session/actionId ohne Schreibfreigabe.

running/completed/stopped/unconfirmed und Schritt-IDs erlauben gezielten Abgleich.
completed bestätigt regulären Auftrag, realen Eingang und Bauarbeiterzugang; keine
Fertigstellung. Bekannte Wegverluste bleiben verboten; nur der unbelegte Bauphasen-
Vorabnachweis darf im freigegebenen Pilot offenbleiben. Teilergebnisse bleiben stehen.
[Aktueller Umfang und Live-Abnahme](generic-building-project.md).

Ab 0.31.4 ergänzt `inspect_building_access` die tatsächlichen Baustellen-Zugangszellen:
constructionAccess.state=observed/unavailable/not_construction, cells (maximal 64),
expanded. unavailable liefert keine Teilmenge als vollständige Liste; erweiterte
Baustellen sind nicht unterstützt. Unabhängig von anderen Baustellen abfragbar.
Zellen sind aktuelle gecachte Spielzugänge, kein Preview- oder Einzelpunkt-
Erreichbarkeitsnachweis. buildersReachable bleibt die getrennte tatsächliche Abfrage.

## Holzwerkzeuge und Chargen 0.35.5 — live abgenommen

`inspect_forestry(session, workBuildingId, x, y, z, width, height, depth=1,
consumerIds?)` liefert höchstens 16 geeignete Bäume in einer Region bis 8×8×4,
Gesamtzahl/Trunkierung, Ausschlussgründe, freie/gesamte Holzbestände und optional
Inventar-/Personalzustände von bis acht Gebäuden. Nur pausiert. Keine Ertragsrate.
Baumstümpfe (`cutYieldRemoved=true`) und fehlende/unbekannte Holzerträge werden
ausgeschlossen; ausgewachsen allein ist kein Nachweis eines fällbaren Baums.

`mark_forestry(region, actionId, treeIds)` setzt nach frischer Prüfung reguläre
Fällmarkierungen für 1–16 gewählte IDs. Derselbe Aufruf liest nur seinen Beleg;
bei `unconfirmed` lesend klären. Keine Ernte- oder Lieferzusage.

`run_simulation_for` erhält optional `stopAtAvailableLogs`: Bestandsziel kann vor
dem Zeitlimit eine bestätigte Pause auslösen. `stockTargetReached` und
`targetReached` unterscheiden; die erste Beobachtung ist kein dauerhafter Vorrat.
Bauchargen antworten standardmäßig kompakt; `details=true` zeigt den Verlauf.
[Abnahme und Grenzen](forestry-efficiency-0.35.5.md).

## capture_screenshot ab 0.36.0 — begrenzt live abgenommen

„Schau mal“ im Chat ist ein ausdrücklicher Abruf: aktuelle Sitzung ermitteln,
einmal capture_screenshot aufrufen und das Bild ansehen. Erfasst wird die vom
Spieler eingestellte Kamera samt UI zum Aufnahmezeitpunkt, ohne Kameraänderung.

Expliziter Bildabruf mit session, optional maxWidth/maxHeight und reasoning.
Ein MCP-JPEG-Bildblock, keine Base64-Doppelung im Text. Metadaten enthalten
Zeitpunkt, Pixelgrößen, Kameraposition/Höhe, Richtung, Zielabstand, Bildwinkel
und Bildweite auf der Zielebene. Keine Kameraänderung oder Dateiablage.
1280×720 und 640×360 sowie falsche Session live geprüft.
[Abnahme und Grenzen](screenshot-0.36.0.md).

## Live geprüft: früh fertiggestellte Bauchargen

advance_building_batch und advance_building_project: bei fertigen Bauobjekten
zugehörigen Lauf früh pausieren, Abschlusszugang/Konfiguration prüfen. Zeitbudget
bleibt Obergrenze; sofort mögliche Chargenübergänge auch bei waitSeconds=0.
Kein Hintergrunddispatcher, reine inspect-Aufrufe ändern nichts.
Entwickler-Gate und Zwei-Lager-Livepilot bestanden: [Nachweis](building-batch-workflow.md).

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
