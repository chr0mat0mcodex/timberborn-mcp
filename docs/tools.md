# Native MCP-Werkzeuge

Stand: Agent Bridge 0.24.0 (Diagnose-Prototyp; sämtliche Bauaufträge vorläufig gesperrt).
[Grund, Ergebnisvertrag und nächster Pilot](road-protection.md). 32 Leser und 22 Werkzeuge für Aktionen/Vorschauvalidierung.
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
- `precheck_building`
- `inspect_building_settings`
- `inspect_research`
- `inspect_agent_log`

## Freizugebende Werkzeuge

- `set_workplace_staffing`
- `run_simulation_for`, `run_simulation_until`, `cancel_simulation_run` — [Zeitläufe](simulation-runs-plan.md)
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
- `place_building`
- `set_building_paused`
- `set_storage_good`
- `set_storage_mode`
- `set_farm_priority`
- `set_farm_crop`
- `unlock_building`

## Auswahl und Ausführung

- Für reguläre neue Bauaufgaben: inspect_build_options, precheck_building,
  validate_building und place_building. inspect_build_catalog/precheck_build_site sowie
  validate_build_site/place_path/place_lodge sind erhaltene, begrenzte frühe Pilotwerkzeuge.
- Validierung mit Vorschau zählt nicht als rein lesend und benötigt eine eigene Freigabe.
- Aktionsfreigaben werden in Mod und MCP-Prozess geprüft; [vollständige Tabelle](native-bridge-install.md).
- reasoning ist optional für alle nativen Werkzeuge: kurze für den Spieler lesbare
  Absicht, keine internen Gedankengänge. [Ingame-Log](activity-log.md).
- Eingabeschemata, verlangte Session-/Erwartungswerte und Grenzen liefert MCP tools/list.
  Kein generischer HTTP-Aufruf und kein automatischer Backendwechsel.
- Ergebnis separat prüfen; applied ist kein Beleg für abgeschlossenen Bau oder Versorgung.
  [Fachliche Fehlercodes](bridge-errors.md).

Neue Güter-/Statusverträge: [Details und Grenzen](economy-observations.md).

[Wege, Reichweiten und Güterhistorie](logistics.md).
`roadProtection` ergänzt ab 0.23.2 Weg-/Baustellen-Prüfpunktzahlen und `affected.kind`; bei `road_cell` bezeichnet `entrance` die Wegkoordinate. Diese Diagnose erweitert keine Baufreigabe.

## Bauplan-Kandidatensuche (0.24.0)

`plan_building_project` ist ein zusätzlicher Leser. Eingaben: template, districtId,
session, x/y/z, width/height (1..8), rotation (0..3). Bis zu vier Vorschläge mit
Gebäudeursprung, Eingang, Anschlussziel und Wegzellen in Anschlussreihenfolge.
Alle Optionen executable=false; noch keine gemeinsame Spielvalidierung oder
Ausführung. Suchabdeckung und Abbruchgrund ausdrücklich lesen.
[Umfang, Grenzen und Pilot](building-site-search-plan.md).
