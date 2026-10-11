# Effiziente Gruppenplanung â€” 0.50.0

Status: Umsetzung und QuellprÃ¼fung, menschliches Gate und Liveabnahme ausstehend.
Erweitert plan_region_buildings; kein neues Werkzeug oder zweiter Bauablauf.

Gate-RÃ¼cklauf: MCP-/Testkompilierung bestanden; Unit-Test meldete interne
Kandidatengeometrie in der kompakten survey_region-Ausgabe. Die drei internen
Geometriefelder sind jetzt ausdrÃ¼cklich von deren Serialisierung ausgeschlossen.
Eine Regression prÃ¼ft auch gefÃ¼llte Felder und deren fortbestehende interne
VerfÃ¼gbarkeit. Belegungsberichte der Gruppenplanung werden separat projiziert
und bleiben erhalten. Erneuter menschlicher Testlauf steht aus.

## Gemeinsame FlÃ¤chendaten

Ein SharedRegionSurveyPort pro Gruppenaufruf teilt Karten, Objektseiten,
Bereichsmarkierungen und Ãœberlappungsabfragen zwischen Vorlagen und HÃ¶hen.
SchlÃ¼ssel enthalten Region einschlieÃŸlich HÃ¶he bzw. Kategorie und Seitenoffset.
Keine Wiederverwendung zwischen MCP-Aufrufen. Nur erfolgreiche Antworten werden
gespeichert; native VertrÃ¤ge bleiben im Client geprÃ¼ft. Fehler brechen ab.

Simulationsgrenzen und vorlagenspezifische Suchplanungen werden immer frisch
gelesen. Session/Version mÃ¼ssen Ã¼bereinstimmen, Simulation muss pausiert bleiben.
Manuelle Eingriffe wÃ¤hrend Pause sind weiterhin mÃ¶glich: kein atomarer Snapshot.
Die gemeinsame native Vorschau und die erneute PrÃ¼fung vor Start bleiben maÃŸgeblich.
readMetrics.nativeReads zÃ¤hlt die tatsÃ¤chlichen Survey-Reads einschlieÃŸlich zweier
Ã¤uÃŸerer Grenzen; planningCalls ist darin enthalten. nativePreviewCalls zÃ¤hlt die
zusÃ¤tzlichen Gesamtvorschauen. cacheHits sind eingesparte Wiederholungsreads.
Die einzelnen Surveys nennen observationRequests, nicht vermeintliche NativeReads.

## Auswahl und Belegung

Die native Suche gibt exakt transformierte occupiedCells, solidCells und
cellOccupations weiter. Occupation stammt aus Block.Occupation; solidCells enthÃ¤lt
nur BlÃ¶cke mit derselben Belegung wie Block.FullFrom und bestÃ¤tigter nativer
Ãœberschneidung mit diesem Vollblock. Keine Bounding-Box- oder RotationsschÃ¤tzung.
Geometrie und Belegungsfelder werden auf Grenzen, ZugehÃ¶rigkeit und Nullwerte geprÃ¼ft.

Nur sich Ã¼berschneidende solidCells zweier bereits ausgewÃ¤hlter/neuer Ziele
werden vor der Gesamtvorschau gefiltert. Gleiche XY-Koordinaten auf anderer HÃ¶he
sind kein Treffer. Fehlende Geometrie oder andere Belegungsschichten werden nicht
als Kollision erfunden. Diese Kandidaten gehen weiter zur nativen PrÃ¼fung.
Filtertreffer verbrauchen keines der maximal vier nativen Versuche je GebÃ¤ude;
Suchtreffermenge und Gesamtgrenzen bleiben endlich. Kein Backtracking.

Filterbefunde nennen kind=planned_building, occupation=native_intersection_profile,
blockierenden inputIndex, Vorlage und im Vollbericht konkrete Schnittzellen.
Compact bÃ¼ndelt je abgewiesenem Anfrageindex und blockierendem GebÃ¤ude.
occupations beschreibt zusÃ¤tzlich die bereits beobachtete Regionsbelegung:
building/path, vegetation/planted, debris und unknown_object. ID, Vorlage,
Position, Markierung, bekannte Fertigstellung und Vegetationszustand bleiben
zuordenbar. cellsKnown=false bedeutet unvollstÃ¤ndige Ãœberlappungsgeometrie.
Rohstoffstapel sind debris/RecoveredGoodStack, keine Bauobjekte zum LÃ¶schen.
Baumzustand einschlieÃŸlich cutYieldRemoved bleibt erhalten; BaumstÃ¼mpfe nicht
als lebenden Holzbestand interpretieren. Regionskontext beweist nicht die Ursache
einer konkreten nativen Ablehnung. Keine automatische Rodung/Abrissentscheidung.

## Ausgabe und Agentenablauf

Compact behÃ¤lt steps, planKey, selected/unplaced, unknowns, konkrete Blockaden,
Aufrufzahlen und Suchabdeckung. Erfolgreiche Zwischenvorschauen und wiederholte
statische Nachweisgrenzen entfallen; Filterberichte werden gebÃ¼ndelt. Bestands-
beispiele sind auf acht begrenzt, byKind zÃ¤hlt alle beobachteten Objekte;
total/truncated melden die Abdeckung. Position/Art bleiben, volle Zellen nur Full.
detail=full erhÃ¤lt sÃ¤mtliche ursprÃ¼nglichen Belege ohne zusÃ¤tzliche Spielabfragen.
Nicht angezeigte Detailfelder sind weder leer noch als geprÃ¼ft anzunehmen.

Der Agent beurteilt Belegungsart und Teilplan, dann startet er den unverÃ¤nderten
Gesamtplan einmal. Keine separaten PlÃ¤ne ungeprÃ¼ft verbinden. Betroffene Objekte
vor einem spÃ¤teren Abriss frisch identifizieren; Rohstoffstapel erhalten.

## Gate und Pilot

Vorbereitete Regressionen: gemeinsame Reads bei gemischten Vorlagen, frische
Grenzen, getrennte Aufruflebensdauer, Session-/Pausefehler, Abbruch, exakte
Vollbelegung, unbekannte Geometrie, verschiedene HÃ¶hen, Belegungsarten und
Compact/Full mit unverÃ¤nderten Schritten. Keine Tests vom Agenten ausgefÃ¼hrt.

Nach menschlichem Gate: gemischte kleine GebÃ¤udegruppe und enge Ã¼berlappende
FlÃ¤che planen; readMetrics und FiltergrÃ¼nde prÃ¼fen. Ein Plan unverÃ¤ndert bauen,
FertigzugÃ¤nge/Pause nachweisen. Belegte FlÃ¤che mit aussagekrÃ¤ftigen Belegungsarten,
Compact/Full vergleichen. Fehlerhafte Negativkontrolle stoppt den Pilot.
Danach nur Testbauten und neue Hilfswege rÃ¤umen, Rohstoffe erhalten.
Erwartet: positive Cachetreffer bei mehreren Vorlagen, gefilterte Vollkollisionen
ohne Vorschauverbrauch, kÃ¼rzere Antwort bei unverÃ¤nderten AusfÃ¼hrungsdaten.


## Erster Livepilot und Korrektur vor Abnahme

Bridge 0.50.0 nach menschlicher Bereitmeldung bestätigt. Gemischte Planung
Bank/Holzfäller/Bank mit vier gemeinsamen Wegen: complete. 42 tatsächliche Reads
und 28 Cachetreffer statt 70 Reads ohne Wiederverwendung; acht Suchplanungen und
sechs zusätzliche Gesamtvorschauen. Compact 11.828 statt 43.474 JSON-Zeichen,
identische Schritte und Schlüssel. Wege, tote Vegetation und unbekanntes
Rampenobjekt getrennt ausgewiesen, neun Bestandsobjekte mit acht Compact-Beispielen.
Noch kein Bauauftrag, deshalb keine neuen Testobjekte und kein Baunachweis.

Frühfilter noch wirkungslos: Bank besitzt Bottom, Path, Middle, nicht Vollbelegung.
Abnahme deshalb offen. Öffentliche API-Metadaten mit vorhandenem Leser geprüft
(kein Build/Testlauf): BlockOccupationsExtensions.Intersects ist verfügbar.
Native Suche liefert jetzt je vorkommender Belegungsmaske die vom Spiel bestimmte
Konflikttabelle aus Kombinationen der definierten Einzelbits (maximal acht Bits
plus separatem All-Wert, maximal 257 Masken). Client nutzt nur bekannte Tabellen
an identischen 3D-Zellen; keine selbst erfundene Bitregel.
Vollbelegung bleibt zusätzliche bekannte Konfliktprobe. Unbekannte/neue Masken
werden weiter nativ geprüft. Regression für kompatible/unbekannte Schichten und
verschiedene Höhen ergänzt. Erneutes menschliches Gate und Livepilot ausstehend.

Gate-Ruecklauf: MCP-Build, Unit- und Integrationstests bestanden; Mod-Build
scheiterte an der konstant unmoeglichen Bereichspruefung fuer BlockOccupations.All.
Korrektur: definierte Einzelbits kombinieren, All separat aufnehmen statt als
numerische Obergrenze benutzen. Native Intersects bleibt der Konfliktentscheid.
Neue Regressionen fuer negativen All-Sentinel, hohe/lueckenhafte Bits,
unbekannte Kombinationen und maximal 257 Masken vorbereitet. Kein Agent-Testlauf;
erneutes menschliches Gate erforderlich. Version weiterhin nicht abgenommen.


## Abschluss-Livepilot 2026-10-12

Nach erneuter menschlicher Bereitmeldung 0.50.0 und neue Sitzung bestätigt.
Gemischte Gruppe Bank/Holzfäller/Bank: drei Konflikte mit konkreten Belegungsschichten
vorab gefiltert. Drei statt sechs native Gesamtvorschauen bei gleichem Gesamtplan.
42 tatsächliche Survey-/Grenzreads plus drei Vorschauen; 28 Cachetreffer sparen
gegenüber 70 ungepufferten Reads 40 %. Acht vorlagenspezifische Suchplanungen sind
in den 42 Reads enthalten. Kein atomarer Snapshot behauptet.

Compact/Full derselben pausierten Planung: identische Schritte und planKey;
11.756 statt 40.165 JSON-Zeichen (70,7 % kürzer). Beide nennen die tatsächlichen
Belegungsschichten, Compact bündelt die drei Filtertreffer zu zwei Befunden.
Bestandskontext unterscheidet Wege, tote Vegetation und unbekanntes Rampenobjekt;
Beispielbegrenzung acht von neun Objekten ausdrücklich ausgewiesen.

Plan unverändert einmal gestartet: zwei Bänke, ein Holzfäller, vier gemeinsame
Wege. Alle sieben Teile nach 7,9375 Spielstunden / 23,315 realen Sekunden fertig;
frischer Abschluss bestätigt sieben zugängliche Objekte, keine verlorenen
Bestandsanschlüsse und automatische Pause. Produktionsbetrieb ist kein Testziel.
Belegte Einzelfläche: none, keine Schritte/Schlüssel, null native Vorschauen,
Belegungsart path korrekt. Kein neuer Bauauftrag durch die Negativprobe.

Sieben Testobjekte danach entfernt. Vollständige regionale Nachlesung bestätigt
Abwesenheit, erhaltene Bestandswege und zwei erhaltene RecoveredGoodStack-Objekte.
Endpause bestätigt. Acht Gebäude, unbekannte Masken und kompatible Schichten
werden durch Gate-Regressionen abgesichert, nicht als zusätzliche Liveproben ausgegeben.

Offen: keine globale Standortoptimierung oder Rückwärtssuche. Regionsbelegung
ist weiterhin Kontext, nicht automatisch die konkrete Ursache jeder Ablehnung.
