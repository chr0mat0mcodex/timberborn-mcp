# Gemeinsame regionale Baupläne — 0.49.0

Status: begrenzter Livepilot mit Bridge 0.49.0 nach menschlicher Bereitmeldung
bestanden. Quellprüfung, menschliches Gate und tatsächlicher Baunachweis getrennt.

## Aufruf und Ausführung

`plan_region_buildings` erhält entweder `template` (bisherige Einzelalternativen)
oder `templates` (1–8 Vorlagen, auch mehrfach). Region, session, districtId und
Suchgrenzen entsprechen 0.48. Nur pausiert, Bauschalter erforderlich. Keine
Bauaufträge durch Planung; temporäre native Vorschau mit Wiederherstellung.

Je verschiedener Vorlage eine Regionalsuche; gleiche Vorlagen teilen die Treffer.
Gruppensuche behält bis zu 32 Treffer je Höhe, Einzelmodus standardmäßig vier
(`maxResults` 1–32). Gruppensuche verwendet immer 32, unabhängig von maxResults.
Die Suchfenster/-höhen begrenzen die tatsächliche Abdeckung weiterhin. Suche
erkennt keine neuen Standortmöglichkeiten, die erst durch andere Neubauten entstehen.

In Eingabereihenfolge höchstens `maxCandidates` (1–4, Standard drei) Versuche je
Gebäude. Jeder Versuch prüft alle bisher gewählten Gebäude plus neues Ziel gemeinsam.
Vorherige Hilfswege werden nicht zusammenkopiert: die native Zugangsplanung erzeugt
sie für die Gesamtgruppe und verwendet generierte Wege als gemeinsame Anschlüsse.
Keine Garantie minimaler Weglänge, kein Backtracking. Exakte bereits gewählte
Duplikate überspringen; andere Kollisionen entscheidet die native Gesamtprüfung.
Ankergrenzen werden vor dem Aufruf geprüft, native Geometrie/Volumen danach.
32 Teile einschließlich Hilfen, 32×32 und acht belegte Höhen bleiben unverändert.

`state=complete/partial/none` bezieht sich nur auf die Zahl eingeplanter Gebäude.
`selected` ordnet inputIndex und stepIndex zu. `unplaced` nennt Gründe, Versuche
und ungeprüfte Kandidaten. Bei partial enthält der ausführbare Plan ausschließlich
die erfolgreich aufgenommenen Gebäude. Kein Kandidat bedeutet nicht unbebaubar.
Transport-, Versions-, Sessionfehler brechen ab; kein automatischer Aktions-Replay.

Der Agent prüft selected/unplaced und übergibt `steps` plus `planKey` unverändert
an `run_large_project`. Keine einzeln geprüften Alternativen zusammenfügen.
Start prüft erneut nativ; Bauzugang, Fertigzugang und Betrieb separat nachweisen.
Status günstig mit begrenztem Warten lesen; nur bei Bedarf frische Diagnose.

## Ausgabe und Aufwand

Compact erhält den ausführbaren Plan, Schlüssel, Abhängigkeiten, Hilfsteilindizes,
Teilplanstatus, Ablehnungsgründe, unbekannte Bereiche und Suchabdeckung. Frühere
Vorschau-Vollberichte, Raster und geometrische Ablehnungsbeispiele entfallen.
`detail=full` liefert alle gespeicherten Belege desselben Aufrufs; keine Zusatzreads.
Planungsdetails bei Bedarf gleich anfordern. Fehlende Detailfelder sind unbekannt,
nicht leer oder erfolgreich. Full ist bei vielen Versuchen entsprechend groß.

Höchstens 32 native Gesamtvorschauen; Suchplanungen zusätzlich je unterschiedlicher
Vorlage bis maxHeights×maxWindows×4. Doppelte Vorlagen sparen ganze Regionalsuchen;
unterschiedliche Vorlagen teilen derzeit keine Karten-/Objektreads. Dies bleibt
eine mögliche Folgeoptimierung nach Messung. Beobachtungen sind nicht atomar;
Abschluss prüft Session, Version und Pause, Ausführung validiert erneut.

## Vorbereitete Regressionen und Livepilot

Regressionen: acht gleiche Gebäude mit gemeinsamer Hilfsstraße; kumulative
Prüfung und stabile Zielindizes; abgelehnte Ergänzung behält letzten gültigen Plan;
32-Aufruf-Limit, leere Suche, Transport-/Sessionfehler, fehlende Wiederherstellung,
Abbruch, Eingabegrenzen und Compact/Full ohne Mutation der Schritte/Schlüssel.
Tests werden ausschließlich im menschlichen Gate ausgeführt.

Livepilot nach Gate: drei kleine Gebäude, möglichst mindestens zwei Vorlagen,
gemeinsamer Hilfsweg, unverändert ausführen. Fertigstellung und Zugänge frisch
prüfen. Zu kleine Region/Blockade muss expliziten Teilplan oder none ergeben;
keine kollidierenden Ziele als freigegeben melden. Full/Compact-Nachweise prüfen.
Bei fehlerhafter Negativkontrolle stoppen und diagnostizieren. Danach Projekte
schließen, nur Testbauten/Hilfswege räumen; Bestandswege und Rohstoffe erhalten.
Keine allgemeine Katalog- oder globale Wegoptimalitäts-Abnahme daraus ableiten.


## Liveabnahme 2026-10-12

Nach menschlichem `live bereit` Backend gestartet, Bridge 0.49.0 und neue Sitzung
bestätigt; Simulation pausiert. Keine Agent-Builds oder automatischen Testläufe.

- Zwei kleine Regionsproben ergaben jeweils korrekt partial (zwei von drei Bänken).
  Sechs native Vorschauen: Zugangskonflikte und überlappende Zielgebäude abgelehnt,
  nur der letzte gültige Zweierplan blieb ausführbar. Full enthielt alle Belege;
  Compact ließ frühere Vollbelege und geometrische Suchbeispiele weg.
- Größere Fläche: drei Bänke, vier gemeinsam verwendete neue Wegstücke, vier
  Gesamtvorschauen einschließlich einer verworfenen Überschneidung. Dieselben
  Schritte und derselbe Planschlüssel unverändert einmal gestartet. Alle sieben
  Teile autonom fertig nach 3,03125 Spielstunden / 8,848 realen Sekunden;
  automatische Pause. Frische Diagnose: sieben fertig/zugänglich, keine verlorenen
  Bestandsanschlüsse, keine fremden Baustellen; Projekt freigegeben.
- Belegte Einzelfläche: none, null Schritte/Schlüssel, null native Vorschauen,
  beide Anfragen ausdrücklich unplaced. Keine Bauaufträge.
- Drei Testbänke und vier Hilfswege anschließend entfernt. Vollständige regionale
  Nachlesung bestätigt deren Abwesenheit, drei erhaltene Rohstoffstapel und
  erhaltene Bestandswege. Endpause bestätigt.

Live belegt sind drei gleiche Gebäude mit geteilten Wegen, Teilplan/Negativfall
und beide Ausgabeformen. Acht Gebäude, gemischte Vorlagen und Fehlerabbrüche sind
Gegenstand der vorbereiteten Gate-Regressionen, nicht zusätzlicher Live-Bauproben.
Keine Aussage über globale Optimalität, vollständige Suchabdeckung oder Produktion.

Folgeverbesserungen: Kollisionskandidaten zwischen schon gewählten Zielen und
weiteren Drehungen früher aussortieren; sie verbrauchten hier mehrfach das kleine
Vorschau-Budget. Karten-/Objektlesungen verschiedener Vorlagen gemeinsam halten.
Compact enthält weiterhin viele wiederholte Nachweisgrenzen; zentralen Verweis
plus konkrete Abweichungen prüfen, ohne unknown/Teilplanstatus zu verschweigen.
