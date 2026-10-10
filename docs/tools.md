# Native MCP-Werkzeuge

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
