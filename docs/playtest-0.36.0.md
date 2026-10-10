# Gebäudespieltest mit 0.36.0 — 2026-10-10

Nutzerauftrag: jedes reguläre Gebäude mindestens einmal fertigstellen;
Verbesserungsvorschläge begleitend notieren, keine MCP-Implementierung in diesem Lauf.
Erster Pilot: drei fehlende Typen, danach Aufwand und Ergebnis bewerten.
Unbestätigte Eingriffe oder blockierte Materiallieferung stoppen die Charge.

## Ausgangslage

Bridge 0.36.0, Tag 411 um 17,78125 Uhr, pausiert. 46 Biber, 49 Betten,
zehn freie Arbeitskräfte, keine kritischen Bedürfnisflags. Zehn frei verfügbare
Holzstämme, 33 Bretter und neun Zahnräder. Keine offenen Baustellen.
Alle sechs Bestandsseiten gelesen: 186 Gebäude/Wege, 30 fertige Vorlagentypen.
Die vorhandene Treppe fehlte in der CSV: 29 auf 30 von 157 korrigiert,
kein Neubau dieses Laufs. Katalog vollständig: 162 Vorlagen, fünf explizite
Entwicklervorlagen ausgeschlossen, 15 reguläre Vorlagen im Baupfad unsupported.

## Ergebnis des Ausbaupiloten

Bauhütte regulär für 100 Forschungspunkte freigeschaltet. Begrenzter Holzlauf:
höchstens zwei Spieltage, früher Stopp bei 30 frei verfügbaren Stämmen.
Holzlauf endete nach 48 Stunden mit 29 freien Stämmen, Bestandsziel 30 nicht
erreicht. Sägewerk und Sauna zur Holzansammlung vorübergehend pausiert.
Erste Charge räumte zwölf Vegetationsobjekte in zwei Gebieten, fand aber keinen
Standort; keine Neubauten. Diagnose: Pflanzmarkierungen bleiben Hindernisse.
Vier Karottenmarkierungen im südlichen Anschlusskorridor entfernt; neue Charge
mit geänderter Fläche räumte 15 weitere Vegetationsobjekte. Danach Bauhütte bei
(32,40,3) und vier Anschlusswege gebaut, Abschlussbeleg finished_accessible.
Die Charge stoppte anschließend ohne Standort für die Dachterrasse; auch die
Wasserpflanzenfarm nicht gebaut. Keine automatische Wiederholung.

Endstand: Tag 417, 17,78125 Uhr, Tempo 0, keine offenen Baustellen. 49 Biber,
49 Betten, keine Obdachlosen oder kritischen Bedürfnisflags; zehn freie Arbeiter.
82 Wasser, 43 Beeren, 23 freie Holzstämme. Sägewerk und Sauna bleiben bewusst
pausiert, damit Holz für weiteren Ausbau verfügbar bleibt; vor Bedarf reaktivieren.
CSV: 31/157 reguläre Vorlagen fertig, 126 offen, fünf Entwicklerwerkzeuge ausgeschlossen.

Aufwandsalarm: ein von drei geplanten Neubauten in rund elf Minuten und über
50 MCP-Aufrufen einschließlich vollständiger Bestands-/Katalogaufnahme. Keine
verlässliche lineare Laufzeitprognose; selbst bei künftig 20 Aufrufen je offenem
Typ wären etwa 2500 weitere Aufrufe nötig. Für 15 offene reguläre Typen fehlen
zusätzlich Baupfadfähigkeiten. Vor größerer Fortsetzung neue Nutzerentscheidung
gemäß AGENTS.md angefordert. Kein MCP-Code geändert.

## Verbesserungsbeobachtungen

- Bestandsübersicht nach Vorlagentyp mit Fertig-/Baustellenanzahl und exemplarischer
  Position: sechs Seiten mit 186 vollständigen Objektgeometrien waren für die
  reine Typenabdeckung nötig. Abgleich gegen regulären Katalog integrieren.
- Katalogfilter für fehlende Typen, verfügbare Materialien, Eingangshöhe und
  Baupfadunterstützung; aktuell müssen sechs weitere Katalogseiten gelesen werden.
- Regionalsuche nach erreichbarer nutzbarer Fläche priorisieren: im südlichen
  16×16-Gebiet 33 native Reads und zwölf Planungen ohne Kandidaten; ausgewählte
  Fenster enthalten viele Höhensprünge. Räumbare Anbauflächen separat ausweisen.
- Einheitliche Eingabeverträge: Übersicht lehnt zusätzliche session pauschal
  mit invalid_argument ab, während andere Leser session benötigen. Tool-Schema
  vor Aufruf beachten; präzisere Parameterfehlermeldung wäre hilfreich.
- Räumplanung muss Pflanzmarkierungen und den späteren Weg gemeinsam prüfen:
  nicht erst eine ganze Fläche räumen und danach fehlenden Anschluss entdecken.
- Frühstopp bei Fertigstellung: alle fünf Objekte der Bauhütte schon fertig,
  aber Charge wartet bei 28,625 von 48 Stunden weiter auf das feste Zeitfenster.
  Fertigstellung erkennen, pausieren und danach Zugang prüfen.
