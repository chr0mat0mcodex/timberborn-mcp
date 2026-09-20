# Lager, Farmen und Gebäudepause (0.15.0)

Implementiert, automatisiert geprüft und am Entwicklungsspielstand live bestätigt. Keine neue Fremdmod-Abhängigkeit. Öffentliche Spielmethoden, keine
UI-Steuerung oder Bestandsmanipulation.

## Werkzeuge

Alle neuen Aufrufe benötigen eine aktuelle `session` und eine Gebäude-`id`.
`inspect_building_settings` liest Pause, Fertigstellung und vorhandene Lager-/Farmoptionen.
Nicht vorhandene Komponenten und noch unfertige Gebäude werden unterschieden.

| Werkzeug | Änderung | Erwarteter bisheriger Wert |
|---|---|---|
| `set_building_paused` | `paused`: true/false | `expectedPaused` |
| `set_storage_good` | `good`: ID aus allowedGoods; leer hebt Auswahl auf | `expectedGood` |
| `set_storage_mode` | `mode`: accept/empty/obtain/supply | `expectedMode` |
| `set_farm_priority` | `priority`: planting/harvesting | `expectedPriority` |
| `set_farm_crop` | `resource`: freigeschaltete Vorlage aus allowedPlants | `expectedResource` |

Lagermodi entsprechen Annehmen, Leeren, Beschaffen und Liefern. Lager- und
Farmeinstellungen erfordern fertige Gebäude. Feste Lagergüter sind nicht umstellbar.
Alte Bestände bleiben erhalten; eine Auswahl- oder Modusänderung transportiert selbst
keine Güter. Pause verwendet PausableBuilding.Pause/Resume und prüft IsPausable.
Fortsetzen garantiert weder Besetzung noch laufende Produktion.

FarmHouse.PrioritizePlanting/UnprioritizePlanting steuert Pflanzen/Ernten zuerst.
PlantablePrioritizer.PrioritizePlantable wählt die bevorzugte Pflanze. Ein Rücksetzen
auf keine Präferenz ist mangels belegter öffentlicher Rücksetzsemantik noch nicht
angeboten (`canClearCropPriority=false`). Keine Jobs werden erzwungen.

## Anbauflächen

Bereits vorhandene Werkzeuge bleiben zuständig: `inspect_area_types` liefert
verfügbare Pflanzen, `inspect_areas(kind="crops", offset=0, limit=32)` die Markierungen.
`set_area` verwendet `kind="crops"`, `operation="mark"` oder `"remove"`, `resource`,
`expectedResource`, `session`, `x/y/z`, `width/height` (je 1–4).
Markieren benötigt eine Pflanzenvorlage; Entfernen verwendet resource="".
expectedResource ist der gelesene vorherige Zustand ("unmarked" oder Pflanzenvorlage).
Ein Wechsel der Markierung entfernt keine bereits gewachsene Vegetation.
Flächen sind globale Markierungen, keine feste Zuordnung zu einem bestimmten Farmhaus.
Erreichbarkeit, Bodenbedingungen, Arbeiter und Ernteertrag müssen separat geprüft werden.

## Freigaben und Prüfungen

Neue Schreibfreigaben: Mod `enableBuildingSettings=true`, MCP-Prozess
`TIMBERBORN_ENABLE_BUILDING_SETTINGS=1`. Standardmäßig aus. Flächen behalten ihre
separate Freigabe `enableAreas` / `TIMBERBORN_ENABLE_AREAS`.

Session und erwarteter Wert verhindern Änderungen auf veralteter Grundlage.
Gleicher Zielwert ist ein No-op. Jede Änderung erfolgt einmal, wird direkt nachgelesen
und als applied/unconfirmed quittiert; anschließend separat lesen. Bei Fehler oder
unconfirmed keine automatische Wiederholung oder Rücknahme.

350 reguläre Tests bestanden (337 Unit, 13 Integration), drei Live-Tests im Standardlauf
übersprungen. HTTP-/MCP-Integration nutzt ausdrücklich synthetische Daten; sie beweist
keine Spielwirkung. Mod gegen Timberborn 1.1.2.4 gebaut, null Warnungen/Fehler.
Live-Abnahme: geeignete fertige Gebäude lesen, Pause hin/zurück, Lagergut/Modes samt
Rücklesung, Farmpriorität und Pflanzenwahl, danach kleine Anbaufläche markieren/entfernen.

## Live-Abnahme 0.15.0 am 2026-09-20

- Neue Settings-Abfrage an neun Gebäuden: fertige Gebäude und Baustellen korrekt unterschieden.
- Fertige Wasserpumpe pausiert und fortgesetzt; beide Zustände separat nachgelesen, abschließend aktiv.
- Farm regulär fertiggestellt. Pflanzen zuerst -> Ernten zuerst -> Pflanzen zuerst bestätigt.
- Bevorzugte Feldfrucht keine -> Kartoffel -> Karotte bestätigt; abschließend Karotte.
- Vier Karotten-Markierungen angelegt, separat gezählt (0 -> 4), entfernt und erneut gelesen (4 -> 0).
- Kleines Lager mit drei Holz regulär gebaut. Lagerauswahl keine -> Beeren -> keine -> Karotten bestätigt.
- Lagermodi Annehmen -> Beschaffen -> Liefern -> Leeren -> Annehmen bestätigt, jeweils separat gelesen.
- Absichtlich veralteter erwarteter Lagerwert abgewiesen; nachfolgende Abfrage weiterhin Karotte/Annehmen.

Zwei begrenzte 30-Sekunden-Fenster auf 7x dienten regulärem Baufortschritt; abschließend
Simulation pausiert, Tag 4, etwa 15:49 Uhr. Kleines Lager leer, Kapazität 30; keine
Umlagerung/Ernte als bewiesen behaupten. Farm und kleines Lager fertig, zwei neue
Wege fertig; zusätzliche Pumpe besitzt jetzt einen Baudistrikt, bleibt aber Baustelle.
Test-Anbaufläche wieder entfernt. Weitere Wohn-/Lagerbaustellen bleiben bestehen.

Bekannte Diagnosegrenze: Die serverseitige Ablehnung eines veralteten Zustands wird
noch als backend_unavailable statt spezifischem Zustandskonflikt gemeldet. retryable=false;
keine Wiederholung ausgeführt, tatsächliche unveränderte Auswahl separat bestätigt.
Künftige Verbesserung: fachliche Konflikte von Transportfehlern unterscheiden.

## 2026-09-20 — Karottenkette vollständig live nachgewiesen (0.15.0)

Ausgangslage: Farm fertig/aktiv mit drei von drei Arbeitern, kleines Lager auf
Karotte/Annehmen mit leerem Bestand. Vier freie Felder regulär als Karotte markiert.
Sieben begrenzte 40-Sekunden-Fenster auf regulärer Stufe 7, jeweils abschließend
Pause. Nach dem ersten Pilot seltener abgefragt, da Wachstum stetig und gesund.

- Vier echte Pflanzenobjekte nach Markierung beobachtet; alive, kein Wasserstress.
- Wachstum über mehrere Beobachtungen von etwa 1 % bis 99 % verfolgt.
- Anschließend 12 Karotten im kleinen Lager (Kapazität 30) beobachtet und nach Pause
  über eine separate MCP-Sitzung erneut bestätigt. Kein Bestand durch Bridge erzeugt.
- Vier neue Pflanzen mit anderen IDs und etwa 3–5 % Wachstum auf denselben Feldern:
  reguläre Ernte und Nachpflanzung zusammen mit neuem Lagerbestand nachgewiesen.
- Schlusszustand: Tag 8, 22:45, Simulation pausiert; Farm aktiv mit drei Arbeitern,
  vier Karotten-Markierungen bleiben für weiteren Betrieb bestehen.

Nebenläufig wurde die vorherige Lodge fertig: sechs Betten, sieben Obdachlose.
Letzte Beispielbestände: Wasser 222, Beeren 270, Holz 20. Keine Aussage über gesamte
Nahrung oder dauerhaft ausreichende Versorgung aus diesen drei Beispielen ableiten.
Dieser Pilot bestätigt einen Erntezyklus samt Lagerung und Nachpflanzung, keine
langfristige Versorgung der ganzen Kolonie. Keine Mod-/Produktcodeänderung nötig.
Nächste offene Versorgungsnachweise: tatsächliche Wassergewinnung/-lagerung und
vollständiger Wohnraum; außerdem Ressourcenübersicht über alle Güter statt drei Beispiele.
