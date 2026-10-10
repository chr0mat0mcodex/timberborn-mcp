# 0.35.5: Holzdiagnose und effizientere Aufträge

## Abschluss 0.35.5 — 2026-10-10

Menschliches Test-/Installationsgate durch anschließende Bereitmeldung bestätigt;
gezielte Compiler-/Regressionstests und feature-spezifische Livepiloten bestanden.
Die Version ist im hier beschriebenen Umfang abgenommen.

Letzter Chargenpilot: erste kleine Suchfläche ohne Kandidat, ohne Eingriff beendet.
Auf 8×8-Fläche mit vier Rotationen fand die Planung einen Standort. Absichtlich
ungeeignete Lagerkonfiguration an einer Holzfällerflagge wurde nativ vor dem Bau
abgewiesen: stopped / build_rejected:unsupported_storage_good. Auch advance
lieferte denselben terminalen Stand und Zeitstempel, ohne erneuten Auftrag.
Unabhängige Baustellenabfrage: null Baustellen.

Kompakt-/Detailbericht desselben gespeicherten Stands stimmen in Status, Grund,
Fortschritt und Auftragskennung überein. JSON-Datenkörper: 439 statt 973 Zeichen
(rund 55 % kürzer); kein allgemeiner Token-/Laufzeitbenchmark.
Gültiger Kontrollauftrag ohne Lagerkonfiguration: eine Holzfällerflagge,
finished_accessible nach begrenztem 12-Stunden-Fenster, kein Überschwingen,
Pause bestätigt. Unabhängige Abschlussabfrage bestätigt fertiges Objekt,
freien/erreichbaren Eingang und currentSpeed=0. Der kompakte Zwischenstand
zeigte running und Restzeit; der Abschluss 1/1 und completed. Keine Aussage über
Besetzung oder Produktion. Verlorene Bestätigungen sind synthetisch abgedeckt,
nicht durch absichtlichen Live-Verbindungsabbruch getestet.

Offene Folgepunkte: regional gefilterte native Forstabfrage (weiter 31 Reads),
präziser Fehlertext bei abgewiesenen Forestry-Eingaben, Verbrauchsraten und
allgemeiner früher Baustopp. Screenshot-Funktion bleibt dokumentierter Wunsch.
Fremde Hermes-/Startskriptänderungen sind kein Bestandteil dieses Abschlusses.
Die folgenden Abschnitte dokumentieren die chronologische Vorarbeit.

## Erneuter Livepilot: Baumstumpf-Korrektur bestanden

2026-10-10 nach erneutem menschlichem Gate, native Bridge 0.35.5, pausiertes
Spiel. Kontrollregion 8×8×2 vollständig in zwei Detailseiten gelesen:
45 Objekte = 34 abgeerntete Baumreste + acht junge Bäume ohne Holzertrag + drei
fällbare Kiefern. Die drei Kandidaten melden jeweils cutYieldRemoved=false,
cutIsYielding=true, cutYieldGood=Log und cutYieldAmount=2. Kein Stumpf empfohlen;
Forestry-Ausschlusszähler und vollständige Einzelbeobachtungen stimmen überein.

Negativkontrolle: bestätigten Stumpf an mark_forestry übergeben, invalid_argument.
Positivkontrolle: zwei echte Kandidaten markiert, requested=confirmed=2;
identischer Replay liefert denselben Beleg. Nachlesen meldet beide als
already_marked, einen verbleibenden Kandidaten sowie unverändert 34 Reste und
acht Objekte ohne Holzertrag. Der irreführende generische Fehlertext bei
invalid_argument bleibt als bereits dokumentierter Folgepunkt offen.

Arbeitslauf: höchstens 24 Spielstunden/180 reale Sekunden, Tempo sieben,
Holzschwelle drei bei Ausgangsbestand zwei. Ergebnis completed mit
stock_target_reached nach 1,9375 Spielstunden/5,51 realen Sekunden, vier freie
Stämme, stockTargetReached=true, targetReached=false, pauseConfirmed=true.
Beide ausgewählten Bäume danach abgeerntet: cutYieldRemoved=true,
cutIsYielding=false, cutYieldAmount=0. Aktuelle Pause zusätzlich unabhängig
bestätigt (Tag 411, 5,78125 Uhr). Damit Ernte und früher Bestandsstopp belegt;
keine exakte Zuordnung der globalen Bestandsänderung zu einzelnen Lieferungen.

Forestry benötigte weiterhin 31 interne Reads je Diagnose. Die Korrektur spart
Fehlentscheidungen, noch keine internen Abfragen. Versionsabschluss bleibt bis
zur Liveprüfung der kompakten Chargenantworten und terminalen Bauablehnung offen.
Kein Commit/Push dieses noch gemeinsam offenen Änderungspakets.

## Livepilot und Korrektur: Fällertrag statt Wachstumsflag

2026-10-10: menschliches Gate erfolgreich, installierte fünf Paketdateien mit
0.35.5-20261010-180251-9ec61831 identisch, native Verbindung erreichbar.
Holzdiagnose lieferte fünf Kandidaten bei 31 internen Reads; deren Lebens-,
Reife-, Reichweiten- und Markierungsdaten stimmten mit den Einzelabfragen überein.
Dieser Vergleich war unzureichend: beide Wege lasen keine tatsächlichen Fällerträge.
Benutzer identifizierte das Problem der Baumstümpfe. Kein gültiger Baumnachweis.

Zwei IDs markiert, identisches Replay ohne weitere Änderungen, geänderte Parameter
und ausgeschlossene ID abgelehnt. Bereits erreichtes Holzziel zwei: completed mit
bestätigter Pause ohne Spielzeitfortschritt. Folgelauf Ziel fünf abgebrochen nach
33,78125 Spielstunden; cancelled/pauseConfirmed, Tag 411 um 3,875 Uhr, zwei freie
Stämme. Kein Nachweis des frühen Stopps nach neuer Holzlieferung.

Korrektur im Quellcode: Vegetationsdaten erhalten cutYieldRemoved, cutIsYielding,
cutYieldGood und cutYieldAmount aus öffentlichen Cuttable/Yielder-Eigenschaften.
GoodAmount verwendet GoodId und ganzzahlige Amount; lokal lesend anhand der
Spielmetadaten verifiziert. Keine private Reflection in der Mod.
Forestry liefert harvested_leftover, cut_yield_unknown, no_log_yield oder
not_currently_cuttable als getrennte Ausschlussgründe. Ältere Payloads ohne diese
Felder liefern keine Fällkandidaten. Synthetischer Kontrollfall: lebend, ausgewachsen
und dennoch IsYieldRemoved=true darf niemals Kandidat werden.

Abnahme erneut offen: ein echter Baum und ein abgeernteter Rest müssen im selben
Pilot unterschieden werden; erst danach Sammelauftrag und Holzstopp weiterprüfen.
Chargenprüfung noch nicht durchgeführt. Kein Commit/Push vor korrigierter Abnahme.
Nebenbefund: Forestry-Eingabefehler werden noch pauschal als möglicherweise
unbestätigte Aktion gemeldet; präzisere Fehlermeldungen bleiben ein Folgepunkt.


## Historischer Vorbereitungsstand vor diesem Livepilot

Stand 2026-10-10: MCP-/Test-Build und Unit-Tests im menschlichen Gate bestanden.
Integrationstest an veralteter Werkzeugliste gescheitert; Erwartung korrigiert.
Zweiter Lauf: Build und Unit-Tests erneut bestanden, danach veraltete
ReadOnly-Erwartung für mark_forestry. Schreibwerkzeugliste korrigiert und
explizite Annotationstests ergänzt; übrige Integrationstest-Erwartungen geprüft.
Noch kein erfolgreicher Gesamtlauf, Mod-Build, Installation oder Livetest.
Kein Versionsabschluss behauptet.

## Umfang

- `inspect_forestry`: pausierte Region bis 8×8×4, vorhandene native Schnittstellen
  für Baumkatalog, Arbeitsreichweite, Vegetation, Fällmarkierungen und Güter.
  Nur lebende, reife, erreichbare, nicht zum Abriss/Fällen markierte Bäume werden
  angeboten. Höchstens 16 Kandidaten im Ergebnis; Gesamtzahl und Trunkierung
  ausdrücklich sichtbar. Unbekannte Reichweite erzeugt keine Kandidaten.
- Optional bis acht `consumerIds`: Pause, Personal und beobachteter Holzbestand
  in den Gebäudeinventaren. Freier globaler Bestand bleibt eine separate Zahl;
  Gesamtbestand, Inventare und Reservierungen nicht addieren. Keine erfundene
  Verbrauchsrate, kein berechneter Baustellenrestbedarf.
- `mark_forestry`: 1–16 ausdrücklich ausgewählte IDs mit Region und eigener
  `actionId`. Vor dem ersten Eingriff frisch prüfen, anschließend sequenziell
  reguläre Fällmarkierungen setzen. Lokales Journal unter `.local/forestry`.
  Gleiche ID/Parameter lesen den Beleg ohne erneute Mutation. Abbruch nach
  unbestätigter Antwort; bestätigte Anzahl und betroffener Baum bleiben sichtbar.
  Markierung bedeutet keinen abgeschlossenen Holzeinschlag.
- `run_simulation_for` kann optional `stopAtAvailableLogs` (1–1.000.000) erhalten.
  Die Bridge beobachtet den freien globalen Holzbestand auf dem Spielthread und
  pausiert bei erreichtem Ziel. `duration` und `maxRealSeconds` bleiben Obergrenzen.
  `stockTargetReached`/`reason=stock_target_reached` sind vom zeitlichen
  `targetReached` getrennt. Abschluss erst mit bestätigter Pause. Ein bereits
  erreichtes Bestandsziel beschleunigt das Spiel nicht. Fehlende Bestandsbeobachtung
  führt zu einer fehlgeschlagenen, begrenzt pausierten Ausführung.
- Bauchargen: ausdrückliche native Ablehnung endet mit `build_rejected:<code>`.
  Verlorene Antworten bleiben unbestätigt; keine automatische erneute Übermittlung.
  Standardantworten enthalten aktuellen Fortschritt und nächste Aktion;
  `details=true` liefert weiterhin Suche, Schritte und Verlauf.

Version in Manifest, Assembly und Bridge auf 0.35.5; native Versionsverträge und
Fähigkeitsbeschreibung entsprechend erweitert. Alte Zeitläufe ohne Holzbedingung
bleiben unterstützt. Kein neuer externer Dienst oder zusätzliche Abhängigkeit.

## Effizienz und Grenzen

Vorgesehene normale Folge: Diagnose → Sammelmarkierung → begrenzter Zeitlauf mit
Holzziel → Abschlussbeobachtung. Das sind vier Client-Aufrufe, wenn das Zeitfenster
ohne Zwischenabfrage ausreicht; einmalige Abnahmekontrollen zählen separat.
Noch kein gemessener Geschwindigkeits- oder Tokengewinn. `nativeReads` macht die
internen Kosten sichtbar: Markierungs- und Reichweitenseiten werden serverseitig
vollständig gelesen, noch nicht in der Spiel-Bridge räumlich gefiltert.

Ein neuer Förster, automatisches Nachpflanzen, Verbrauchsraten, Kraftnetzplanung,
alternative Baukandidaten nach Ablehnung und frühes Stoppen jedes allgemeinen
Bauzeitfensters sind separate Folgearbeiten. Die Holzbedingung ist kein allgemeines
Ereignissystem. Der Screenshot-Wunsch steht im Backlog, ohne Implementierung.

## Abnahme nach menschlichem Gate

1. Native Verbindung und Version 0.35.5 bestätigen. Bei pausiertem Spiel eine
   kleine Holzfällerregion mit mindestens einem geeigneten und einem unreifen,
   markierten oder unerreichbaren Baum auswählen. Ergebnis mit vorhandenen
   Detailabfragen vergleichen; bei falschem Einschluss stoppen.
2. Zwei geeignete IDs markieren und Markierungen zurücklesen. Identischen Auftrag
   wiederholen: gleicher Beleg, keine weiteren Markierungen. Geänderte Parameter
   unter derselben ID ablehnen. Ungeeignete ID vor jedem Eingriff zurückweisen.
3. Holzschwelle, die bereits erreicht ist: keine Beschleunigung, bestätigte Pause.
   Kleine noch nicht erreichte Schwelle mit begrenztem Arbeitsfenster: nach Ernte
   vorzeitig pausieren oder mit ausdrücklich nicht erreichtem Holzziel am Zeitlimit.
   Ernte und freien Bestand getrennt vom angenommenen Fällauftrag bewerten.
4. Kleine Baucharge: kompakte Fortschrittsantwort und `details=true` vergleichen.
   Explizite native Ablehnung muss terminal stoppen; verlorene Bestätigung darf
   keinen neuen Bau auslösen. Keine größere Ausbaucharge für diese Abnahme.
5. Aufrufzahlen und Antwortumfang festhalten; erst nach bestandenem Skriptlauf
   und diesem Livetest Abschlussstatus, gezielten Commit und Push setzen.

Ergänzte synthetische Tests decken Eignung, unbekannte Reichweite, falsche Seiten,
Sitzungswechsel, Replay nach verlorener Bestätigung, Holzschwelle, Zeitlimit und
native Annahme des vorzeitigen Abschlussbelegs ab. Gezielte Compiler- und
Regressionstests erfolgen vor der Übergabe gemäß DEVELOPMENT_WORKFLOW.md;
vollständige Skriptkette und Installation bleiben beim menschlichen Gate.

## Lokaler Arbeitsbaum

Vorprüfung nach CS8602-Korrektur am 2026-10-10: MCP-/Testprojekte und Mod gegen
Spielreferenzen ohne Warnungen/Fehler kompiliert; 77 gezielte Unit-Tests und sechs
Aktionsschalter-Integrationstestvarianten bestanden. Darunter synthetische
Baumstumpf-/Ertragskontrollen. Der reale Stumpf-/Baumvergleich bleibt nach der
menschlichen Installation offen. Die Testhost-Verbindung war in der Sandbox
blockiert; erfolgreiche Testläufe erfolgten außerhalb der Sandbox.

Bereits vor dieser Arbeit vorhandene Hermes-/Starter-/Dokumentationsänderungen
bleiben erhalten und sind nicht durch diese Abnahme freigegeben. Die bestehenden
Änderungen an Standard-Aktionsschaltern und Budgetbeschreibungen wurden gelesen;
keine weiteren Schalteränderungen für 0.35.5. Neue Zeilen in `NativeTools.cs` und
den Werkzeugdokumenten liegen neben älteren Änderungen: später selektiv sichern.
