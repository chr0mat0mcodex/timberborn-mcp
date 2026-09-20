# Native MCP-Werkzeuge

Stand: Agent Bridge 0.21.1 mit Inventaren in inspect_building_operation; Installiert, Live-Abnahme offen. 0.21.0 mit allen 30 Lesern live bestätigt.
0.21.0 ist installiert; der Lebenszustandsfix ist mit 11 lebenden und zwei ausgeschlossenen toten Bibern live bestätigt. Aus `NativeTools.Catalog` mit allen Freigaben abgeglichen:
30 Leser und 19 Werkzeuge für Aktionen/Vorschauvalidierung. Nicht jede Kombination
ist live geprüft; Nachweise und Grenzen stehen in den Fachdokumenten.

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
