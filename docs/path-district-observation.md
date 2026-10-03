# Wegzelle im Distriktnetz — Testübergabe 0.29.3

`inspect_path_district(id, districtId, session)` liest die tatsächliche Hauptwegzelle
eines fertigen `PathSpec`-Objekts: `TransformCoordinates(MainPathCoordinates)` und
`DistrictCenter.IsOnInstantDistrictRoad(GridToWorld(cell))`. Die öffentliche
Spielabfrage wird bereits im Wegschutz und im ebenen Baupilot verwendet; kein
eigener Graph, kein Eingangsfilter, keine Reflexion oder Vorschau.

`supported=true` und `connected=true/false` sind eine beobachtete Zugehörigkeit
beziehungsweise Nichtzugehörigkeit zum konkret abgefragten Distrikt. Unfertige
oder nicht passende Objekte liefern `supported=false`, `connected=null` und
`pathCell=null` mit Grund. Ein ungültiges/unfertiges Distriktzentrum wird abgelehnt;
eine veraltete Session ebenfalls. Keine Biberreise-/Liefergarantie.

Die bisherige Gebäude-Wegsuche bleibt unverändert: Sie braucht je einen gültigen
`Accessible` und unterstützt die oberen Pilotwege deshalb nicht. Ihr
`supported=false` war kein Nachweis einer fehlenden vertikalen Verbindung.

Erwarteter Livetest nach menschlichem Skriptlauf: bestehenden unteren Weg als
Positivkontrolle lesen, beide fertigen oberen Pilotwege gegen dasselbe Distriktzentrum
abfragen und die transformierten Zellen prüfen. Auch ein Gebäude ohne `PathSpec`
als Unbekannt-Kontrolle abfragen. Ein negatives Ergebnis wird nicht umgedeutet;
danach Geometrie/Navigation diagnostizieren. Keine Neuplatzierung für die reine
Abfrage nötig; bei einem sauberen Stand kann der geprüfte Plattformpilot neu
aufgebaut werden.

Menschlicher Skriptlauf: 667 Tests bestanden, drei Live-Tests übersprungen.
Der Mod-Build scheiterte an einer durch die nullable Kurzform ausgelösten
`int3`-Assemblyanforderung. Transformation auf die bereits kompilierte lokale
`var`-Form des Wegschutzes angepasst; anschließend menschlich gebaut/installiert.

## Live-Nachweis 2026-10-03 — bestanden

Unterer fertiger Bodenweg: `supported=true`, `connected=true`. Lager ohne
`PathSpec`: `supported=false`, `connected=null`, `reason=not_path_object`.
Unfertige Pilottreppe: ebenfalls unbekannt, `reason=unfinished_path`.
Das Distriktzentrum besitzt selbst eine unterstützte Wegkomponente.

Auf sauber geladenem Stand wurde der fünfteilige Plattformpilot regulär neu
aufgebaut und mit drei begrenzten Bauphasen abgeschlossen. Beide fertigen oberen
Wege meldeten gegen dasselbe Distriktzentrum `supported=true`, `connected=true`;
ihre transformierten Hauptwegzellen lagen auf der oberen Ebene. Endstatus
`completed`, Spiel pausiert. Damit ist die vertikale Distriktanbindung dieses
Testaufbaus belegt; kein Nachweis beliebiger 3-D-Routen oder tatsächlicher Lieferungen.
`connected=false` ist synthetisch geprüft, nicht durch zusätzlichen Abriss live
provoziert. Keine Rohdaten oder Spiel-IDs im Nachweis abgelegt.
