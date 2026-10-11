# Große Gebäude und feste Hex-Vorlagen

Stand 2026-10-11: menschliches Gate durchlaufen, regulärer Großbau live bestanden;
eingangslose Großgebäude und native Hex-/Standortkontrollen noch offen. Weiterhin 0.37.0,
keine Parallelfassung und keine pauschale Abnahme der sechs Vorlagen.

## Begrenzter Abschluss am 2026-10-11

Menschliches Build-/Test-/Installationsgate bestanden. Der Nutzer hat nach dem
Großbaupilot ausdrücklich entschieden: Großbau-Nachweis sichern, Sondertests
zurückstellen. Abgenommen ist der reguläre Großbaupfad am DomedGarden;
keine pauschale Bauabnahme aller sechs Vorlagen oder der Hex-Sonderform.

Alle sechs Zielvorlagen melden `supported=true` und `maxGeometryCells=512`:
DirtExcavator 90, DomedGarden 125, EarthRecultivator 392, FountainOfJoy 125,
HallOfAbundance 75 und Mine 75 native Blockeinträge. Fähigkeitsabfrage meldet
beobachtete 512 bei unveränderten 32 Teilen/32×32/acht Höhen.
Die Vorprüfung liefert DomedGarden mit 81 belegten/unterirdischen Zellen,
EarthRecultivator in Drehung 0 und 1 mit jeweils 264. Die Differenz zum Katalog
entsteht durch native Belegungsfilterung, nicht durch Abschneiden bei 64.
Hex-Geometrie ist damit lesend belegt, ihre native Platzierung noch nicht.

Nach erfolgloser erster Flächensuche und erneuter Nutzerfreigabe wurden westlich
der Brücke 17 abgestorbene Birken/abgeerntete Reste ohne Fällertrag regulär entfernt.
DomedGarden bei (10,18,3), Drehung 0: Vorprüfung mit 81 Zellen ohne Blockade.
Automatische Zugangsplanung ergänzte zwei Treppen und neun Wege; native
Gesamtvorschau gültig und restauriert, keine verlorene Bestandsverbindung.
Zwölfteilcharge autonom fertig nach 68,97 Spielstunden / 202,37 Echtzeitsekunden.
Baustellenzugang separat `buildersReachable=true`, 24 reale Zugangszellen.
Fertiger Eingang separat frei/erreichbar, Distriktdistanz 30. Abschluss-Ledger
freigegeben. Betrieb ist kein Bestandteil dieses Baunachweises.

Materialdiagnose meldete nachvollziehbar 40/60 Planken am Bau. Holzveredelung
während der Fertigstellung kurz pausiert und anschließend wieder aktiviert.
Spiel pausiert; keine laufenden Testaufträge. Gebäude und elf Zugangshelfer
stehen noch; Rückbau bleibt offen. Keine Rückgewinnungsrohstoffe gelöscht.

Drei besetzte Erfinder, keine freien erwachsenen Arbeitskräfte; gemessen
128 Forschung in rund 2,5 Spieltagen. Die übrigen rund 32.000 Forschungspunkte
würden bei gleichem Betrieb etwa 12–13 reale Stunden benötigen. Deshalb sind
auf Nutzerentscheidung Tests ohne Eingang, native Hex-Platzierung samt isolierter
Außenzellen-Negativkontrolle und Mine-Sonderstandort zurückgestellt. Kein weiterer
Produktions- oder Kolonieausbau zur Vorbereitung dieser Tests.

## Umfang

- Bis 512 native Blockeinträge pro Objekt statt 64. `BlockObjectSpec.Blocks`
  bzw. transformierte native Blöcke sind maßgeblich, nicht das Volumen von `Size`.
  Untergrundblöcke und unregelmäßige Geometrie bleiben erhalten. Überlauf wird
  abgewiesen; nur lesende Objektlisten dürfen ausdrücklich `cellsTruncated` melden.
- `Single` mit Werkzeugform `Hex` verwendet dieselbe feste, ungespiegelte
  Platzierung und native Vorschau wie andere Einzelobjekte. Kein Hex-Raster,
  kein eigener Füllalgorithmus, keine beliebigen Spezialformen oder Terrainseiten.
- Katalog, räumliche Vorprüfung, Einzel-/Projektvalidierung, Platzierung,
  autonome Gesamtpläne und automatische Zugangsplanung verwenden dasselbe Budget.
  Der ebene Suchbereich bleibt 8×8; Gebäude ohne ebenerdigen Eingang benötigen
  weiterhin den expliziten Großprojektpfad.
- Unverändert: höchstens 32 Teile, 32×32 und acht tatsächlich belegte Höhen
  pro Großprojekt. Gesamt-Wegschutz verarbeitet bis 32×512 native Blockeinträge,
  ohne die Kandidaten nach 64 oder 576 Einträgen abzuschneiden. Lesende
  Objektseiten erhalten ein begrenztes 1-MiB-Transportbudget.
- Neue Katalogfelder `geometryCellCount` und `maxGeometryCells` zeigen tatsächliche
  Blockzahl und installiertes Limit. Fehlende Werte sind ältere Pakete, keine
  Nullgeometrie. Die Fähigkeitsabfrage übernimmt das beobachtete Limit; die
  Versionsnummer allein reicht bei mehreren 0.37.0-Paketen nicht.

## Einordnung der sechs Zielvorlagen

Die vorangegangene Liveabfrage des alten Pakets meldete für DirtExcavator,
DomedGarden, FountainOfJoy, HallOfAbundance und Mine nur den 64-Zellen-Ausschluss;
EarthRecultivator zusätzlich `special_tool_shape` wegen `Hex`.
Die erneute Katalogabfrage ist oben dokumentiert. `supported` bedeutet Unterstützung des Baupfads,
keinen gültigen Standort, Forschungsfreigabe, Fertigstellung oder Betrieb.
Mine, Erdbohrer und andere Standortfälle durchlaufen weiterhin die nativen
Validatoren; keine Rohstofflagerstätte oder Sonderbedingung wird erfunden.

Arbeitszugang, fertiger Eingang und Betrieb bleiben getrennte Nachweise. Gebäude
ohne Eingang bekommen keine erfundene Tür. Hypothetische Bauarbeiterreichweite
bleibt unbekannt; geometrische Arbeitszellen und Distriktwege sind Vorprüfungen.
Die bestehende Grenze von 64 tatsächlichen Baustellen-Zugangspunkten bleibt:
größere/expandierte Zugriffsmengen melden unbekannt, niemals eine Teilabdeckung.

## Regressionen und verbleibende Testabdeckung

Im menschlichen Gate ausgeführte Regressionen: 65/512/513 native Zellen, leere Geometrie, sparse
Außenzellen, Hex nur mit Single, andere Sonderformen/Seitenbindung abgewiesen,
vollständige Precheck- und Objektseitenübertragung, Einzel-/Gesamt-Wegschutzbudget,
Katalogwidersprüche und beobachtetes Limit bei gleicher Versionsnummer.
Bestehende Projektgrenztests für 32 Teile/32×32/acht Höhen bleiben maßgeblich.
Agent hat keine Builds oder Tests gestartet.

Pilotplan: Punkt 1 bestanden; Sondertests aus Punkt 2 und 3 ausdrücklich zurückgestellt.

1. Alle sechs Vorlagen einschließlich exakter Blockzahl lesen. DomedGarden an
   erreichbar vorbereitetem Standort vorprüfen, gemeinsam validieren und bauen;
   Bauarbeiterzugang sowie fertigen Eingang separat bestätigen.
2. Ein Gebäude ohne Eingang (bevorzugt FountainOfJoy) auf Arbeitszugang prüfen,
   beauftragen und Fertigstellung nachweisen. Vorher Material-/Forschungsbudget
   prüfen; keine langwierige Kolonieentwicklung als Ersatz für den Pilot.
3. EarthRecultivator: vollständige native Hex-Geometrie und gedrehte Vorschau
   prüfen, belegte äußere Zelle als Negativkontrolle; keine Vollbauabnahme ohne
   tatsächliche Fertigstellung. Mine auf ungeeignetem Standort muss nativ scheitern.
4. Vorschauen müssen restauriert sein. Bei falscher Negativkontrolle stoppen.
   Testbauten/Wege nach Möglichkeit zurückbauen, Rückgewinnung einsammeln,
   keine Rohstoffe löschen. Teuren Pilot vor Skalierung neu bewerten.

Gate und regulärer Großbaupilot bilden den begrenzten Git-Checkpoint. Die
zurückgestellten Fälle benötigen einen eigenen Livetest vor ihrer Abnahme.
