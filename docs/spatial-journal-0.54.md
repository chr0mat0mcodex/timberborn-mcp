# 0.54 - Pflanzen und Flaechen im Aenderungsjournal

Status: nach menschlichem Testgate am 12. Oktober 2026 begrenzt live abgenommen.
Alle geplanten repraesentativen Livegegenproben bestanden; Grenzen siehe unten.

## Umfang

Bestehendes `inspect_recent_actions` erweitern, kein zweites oeffentliches Journal:

- `planting_designation`: Pflanzmarkierung gesetzt, entfernt oder umgewidmet;
  before/after ist die Pflanzenvorlage, null bedeutet keine Markierung.
- `cutting_designation`: before/after marked oder null. Kein Faellnachweis.
- `plant_added` / `plant_removed`: reale Pflanzenobjekte erschienen/verschwunden,
  mit Objekt-ID, Vorlage und Position. Kein Beweis von Pflanz-, Ernte- oder
  Abrissauftrag; die Ursache wird nicht geraten.
- `plant_state`: Wechsel der Klassifikation tree/stump/tree_unknown/plant.
  Stumpf nur bei TreeComponent und belegtem IsYieldRemoved. Fehlender Yieldbeleg
  bleibt tree_unknown. tree bedeutet Baumobjekt, nicht garantiert lebend oder reif.
  Keine kontinuierlichen Wachstumswerte, Forschungsereignisse oder Automatik.

Pflanzen gehoeren zum nativen PlantableSpec-Katalog und haben NaturalResource.
Keine fraktionsspezifische Pflanzenliste, keine privaten Hooks oder Reflection.
PlantingService.PlantingCoordinates/GetResourceAt, TreeCuttingArea.CuttingArea,
EntityRegistry und Cuttable.Yielder sind bereits im Projekt verwendete APIs.

## Erfassung und Herkunft

Vollstaendiger begrenzter Zustandsvergleich etwa einmal pro realer Sekunde, auch
in Pause, sowie beim Journalabruf und vor/nach direkten relevanten MCP-Eingriffen.
set-area und planting-apply sind eingeschlossen. Erster erfolgreicher Snapshot
ist Baseline ohne Altauftraege. Kurze Zwischenaenderungen koennen fehlen; keine
garantierte Ereignislieferung fuer Pflanzen oder Flaechen behaupten.

source=mcp nur beim direkten synchronen MCP-Abgleich. Sonst unknown; auch nach
einer fehlgeschlagenen vorherigen Beobachtung keine sichere Zuschreibung zum MCP.
Menschen und Simulation werden nicht geraten. Pflanzen, die erst spaeter durch
Biber entstehen, sind daher unknown. Keine automatische Ruecknahme von Eingriffen.

Maximal 65.536 kombinierte Pflanzen-/Markierungseintraege pro Snapshot,
100.000 gepruefte Registry-Objekte und 256 Pflanzenvorlagen. Erst vollstaendig
beobachten und validieren, dann Differenzen speichern. Ueberschreitung/Fehler
behalten den letzten Snapshot und erzeugen keine erfundenen Entfernungen.
SpatialTrackingComplete, SpatialObservationFailures, TrackedSpatialEntries,
MaxSpatialEntries, SpatialStartedAtUtc und SpatialLastScanAtUtc werden auf dem
Draht wie ueblich camelCase ausgegeben. Gesamt-trackingComplete beruecksichtigt
beide Erfassungen; Fehlerzaehler bleiben kumulativ. Grenzen belegen keine lueckenlose
Historie. Nach einer Luecke sind spaetere Differenzen nur letzte bekannte Zustandswechsel.

Bestehende Gebaeude-/Wegeerfassung behaelt ihre Limits. Der interne Gebaeude-/Tempo-
Puffer fuer Produktionsnachweise bleibt getrennt; Pflanzenereignisse duerfen
dessen Cursor und Ueberlaufbudget nicht verbrauchen. Oeffentlich bleibt ein
gemeinsamer chronologischer Puffer mit 512 Rohereignissen, maximal 64 je Seite.
Eine grosse Flaechenaenderung kann diesen Puffer ueberlaufen: gap/dropped beachten.
Kein Speichern in Dateien oder Saves, neue Spielsession beginnt neu.

## Ausgabe und Agentenablauf

`detail=full` liefert unveraenderte page.items mit einzelnen Zellen/Objekten.
Standard compact verschiebt raeumliche Ereignisse nach page.groups: gleiche Art,
Quelle, Belegart, Vorlage, Vorher-/Nachherwert und 8x8-Rasterkachel auf derselben
Hoehe. count ist Ereigniszahl, keine Anzahl eindeutiger Zellen. bounds umschreibt
die Gruppe; nicht jede eingeschlossene Zelle muss betroffen sein. Sequenz- und
Zeitgrenzen sind keine lueckenlose Reihenfolge innerhalb einer Gruppe.
Andere Ereignisse bleiben unter page.items. eventCount zaehlt die Rohereignisse
der Seite. Die Gruppierung verbindet keine Seiten und veraendert weder Filter
noch Cursor. hasMore/nextCursor/gap/dropped immer auswerten.

Bei "Ueberpruef mal meine letzten Aktionen": Session und bekannten Cursor nutzen,
sonst begrenzte letzte Eintraege. Fuer Details denselben after/limit/Filter mit
full wieder lesen; bei weiterlaufender Historie sind Seiten nicht eingefroren.
Bei fehlendem Cursor, Erfassungsfehler oder Ueberlauf frisch beobachten und Grenzen
nennen. Bereits 0.40-0.53 bleiben mit ihren alten supportedKinds lesbar.

## Vorbereitete Nachweise

Automatische Tests: stille Baseline, unveraenderte Folge, Hinzufuegen/Entfernen/
Umwidmen, Baum zu Stumpf, fehlender Yieldbeleg, partielle/zu grosse Beobachtung,
Filter/Cursor/Ueberlauf/frische Baseline, Gruppengrenzen und unveraenderter Cursor,
Full, Transportvalidierung neuer Felder und alte 0.40-Fixtures. Versionsprofile
erhalten 0.54 als positive Version; unbekannte 99.0.0 bleibt negativ.

Nach menschlichem Gate: Baseline ohne Spam; kleine MCP-Pflanz-/Faellmarkierungen
setzen und entfernen, korrekte Werte/Quelle und Compact/Full vergleichen.
Einen begrenzten Pflanzenentfernungs-/Neupflanzpilot begleiten; Erscheinen und
Verschwinden getrennt von Markierung nachweisen. Ein Baum-/Stumpfvergleich nur mit
tatsaechlichem passenden Objekt, kein grossflaechiger Waldumbau. Menschliche
Markierungsaenderung bleibt unknown. Neue Session/frischer Cursor sowie alter
Sessioncursor kontrollieren. Produktionsnachweis durch kurze repraesentative
Gegenprobe gegen unbeabsichtigte Journalbeeinflussung absichern.
Bei falscher Klassifikation, erfundenen Entfernungen, falscher Herkunft oder
Cursorverlust sofort stoppen. Testmarkierungen aufraeumen, Rohstoffstapel nicht
loeschen, Endpause. Commit/Push erst nach Gate und feature-spezifischer Liveabnahme.


## Live-Zwischenstand am 12. Oktober 2026

Nach menschlichem Gate Bridge 0.54.0 verbunden. Stille Baseline mit 1482
Pflanzen-/Flaecheneintraegen; vollstaendige letzte Beobachtung, keine Fehler,
kein Pufferverlust. Zwei verschwundene Birkenstumpfobjekte korrekt als stump
und Quelle unknown erfasst. Das belegt Entfernung, nicht eine Ernteursache.

Zwei benachbarte Karottenmarkierungen entfernt und wiederhergestellt: jeweils
zwei Rohereignisse in einer raeumlichen Gruppe, exakte Vorher-/Nachherwerte,
Quelle mcp. Eine Faellmarkierung entfernt und wieder gesetzt, beide Richtungen
korrekt. Folgeabruf ab letztem Cursor leer, Cursor unveraendert, keine Duplikate.
Frische Pflanzvorpruefung bestaetigt beide Markierungen; Faellmarkierung separat
nachgelesen. Simulation abschliessend pausiert. Keine Pflanzen oder Stapel durch
den Agenten geloescht.

Abschluss noch offen: Compact/Full-Vergleich und neue Artfilter, Pflanzen-
Neuzugang/Zustandswechsel, menschliche Markierungs-Gegenprobe, Sitzungswechsel
und kurze Produktionsnachweis-Regression. Der lokale Serverkatalog enthaelt
bereits detail und alle fuenf neuen Ereignisarten; die diesem Chat exponierte
Werkzeugdefinition ist noch alt. Vor diesen Parameterproben Codex-Verbindung
mit frischem Werkzeugkatalog herstellen; kein erneuter Build erforderlich.
Keine abschliessende Abnahme und kein Feature-Commit/Push vor Abschluss.


### Fortsetzung mit aktualisiertem Werkzeugkatalog

Compact/Full auf identischem Abschnitt: sechs Rohereignisse, vier Gruppen,
identischer Cursor. Alle fuenf neuen Artfilter erfolgreich live verwendet.
Einzelseiten teilen Gruppen ohne Ereignisverlust; Folgecursor korrekt.
Sechs kleine Ereignisse ergeben 1550 Zeichen fuer page in Compact gegen 1555
in Full: funktional kompakt gruppiert, hier praktisch keine Platzersparnis.
Adaptive Darstellung kleiner Gruppen als Effizienzfolgepunkt in B12 vermerkt.

Begrenzter Lauf: acht Spielstunden in 23,5 realen Sekunden, automatische Pause,
kein Ueberschwingen. Neue Eiche und Wasserpflanze sowie verschwundener Stumpf
und Wasserpflanze korrekt als getrennte reale Objekte erfasst; Quelle unknown.
Eiche tree -> stump am selben Objekt nachgewiesen. Unabhaengige Waldabfrage
bestaetigt dort genau einen Rest und an der neu bepflanzten Zelle einen lebenden,
noch unreifen Baum ohne Rest. Keine falsche Ernte-/Reifeaussage aus tree.

Produktionsregression am Erfinder: acht native Zyklen in acht Spielstunden,
buildingProductionReached=true; Messlauf abgeschlossen. Beobachtete Zeiten
4,46875 Stunden producing, 3 ausserhalb Arbeitszeit, 0,53125 unknown. Pflanzen-
ereignisse stoeren den getrennten Produktionspuffer im Pilot nicht. Kein
allgemeiner Ueberlauf-Stresstest im Spiel; dieser bleibt automatischer Nachweis.
Erfassung weiterhin vollstaendig, null Beobachtungsfehler und kein Pufferverlust.
Noch offen: angeforderte menschliche Markierungsprobe und neue Sitzung.


### Menschliche Gegenprobe bestanden

Nach bestaetigtem Nutzereingriff fuenf neue Markierungsereignisse: eine
Karottenmarkierung entfernt, zwei Weizenmarkierungen und zwei Faellmarkierungen
gesetzt. Alle Quelle unknown, keine falsche MCP-Zuschreibung. Spielzeit dabei
unveraendert; keine Beobachtungsfehler, kein Pufferverlust. Nutzermarkierungen
belassen. Letzter Cursor 29. Abschliessend noch Spielstand neu laden und neue
Baseline sowie Ablehnung der alten Session pruefen; kein erneutes Buildgate.


## Abschluss: Sitzungswechsel bestanden

Neue Spielsession bestaetigt. Frischer Journalcursor 0, keine Ereignisse trotz
1482 erfasster Pflanzen-/Flaecheneintraege; beide Erfassungen vollstaendig und
fehlerfrei. Abfrage mit alter Session und altem Cursor abgewiesen: stale_session.
Damit alle oben vorgesehenen repraesentativen Liveproben bestanden. Historische
Zwischenstaende dokumentieren den Ablauf, keine verbleibende Abnahmesperre.
Kein Live-Stresstest der Maximalgroessen und keine vollstaendige Ereignishistorie
behauptet. Menschliches Gate liefert die automatischen Testnachweise.
