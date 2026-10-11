# Engpassdiagnose — 0.43.0

Status: Menschliches Gate und begrenzter Livepilot bestanden, einschließlich
der abschließenden Beschreibungskorrektur. 0.43.0 ist im unten dokumentierten
Umfang abgenommen. Frühere Fehler- und Gateabschnitte sind Historie.

> Folgeänderung 0.47 begrenzt live abgenommen: öffentliche Simulationsstatuslesungen liefern frische
> Diagnose nur noch mit diagnose=true. [Vertrag](lightweight-status-wait.md).
> Die folgenden 0.43-Angaben beschreiben die ursprüngliche Abnahme.

## Vertrag

`inspect_building_operations(session, ids, diagnose=true)` erweitert die bestehende
Sammelabfrage für 1–8 IDs. Ohne diagnose bleibt die günstige 32er-Übersicht erhalten.
Ein nativer Request im Spielthread, keine Schreibaktion oder zusätzliche Simulation.
Kompakt enthält `summary.bottleneck` und `summary.assessment.findings`; Full ergänzt
wie bisher Inventare und Statustexte. Kein neuer Werkzeugname.

Je fertiger Manufaktur mit gewähltem nativem Rezept werden gemeinsam ausgewertet:

- Zutaten und Brennstoff: Rezeptbedarf, lokaler und unreservierter Eingangsbestand,
  global verfügbarer/gesamter Bestand und global zu Prozessoren getragene Güter.
  Brennstoffbedarf 1 ist eine Nachfüllung, kein Verbrauch je Produktionszyklus.
- Transport: nativer Distrikt-/Eingangsstatus; öffentliche ungesperrte Quellen
  mit unreserviertem Bestand. Nur bei jeweils genau einem gültigen Accessible
  wird der native Straßenweg in beiden Richtungen geprüft, sonst unknown.
- Lager: tatsächlich eingestelltes Produkt als Input, öffentlicher ungesperrter
  Eingang und unreservierte Kapazität für einen Rezeptausstoß; leerende Lager
  sind keine Ziele. Gezählt werden passende Lager, nicht nominelle Gesamtkapazität.
- Personal: Soll/Ist/Maximum, Unterbesetzung, Arbeitszeit; Soll ist keine Besetzung.
- Kraft: aktive Verbraucher, gültiges Netz, tatsächlicher Powered-Status und
  momentane Netzleistung/Nachfrage. Inaktiv beweist keinen Anschlussfehler.

`findings` nennt Bereich, stabilen Code, Schwere, gegebenenfalls Gut und konkreten
nächsten Schritt. Belege bleiben daneben maschinenlesbar. Bereits vorhandene
erreichbare Produktlager führen zuerst zur Transport-/Prioritätsprüfung.
Keine automatischen Einstellungen, Lagerbauten oder Umbauten.

## Grenzen und Kosten

Eine gemeinsame Inventargebäudeliste, zwischengespeicherte globale Güterwerte und
höchstens 128 andere Inventargebäude pro Ziel, GUID-sortiert. Pro Gebäude/Endpunkt
wird das Wegergebnis für alle Güter wiederverwendet: höchstens 2×8×128 gerichtete
Wegabfragen, oft weniger. Maximal 32 Zutaten-/Brennstoff-/Produktzeilen je Rezept;
bis drei erreichbare Beispiel-IDs je Gut. Große Kolonien können Teilabdeckung
liefern; candidateBuildings/inspectedBuildings/complete machen sie sichtbar.
Complete bezeichnet ausschließlich die Endpunktabdeckung.

Ein Weg belegt weder denselben Lieferauftrag noch verfügbare Träger oder Lieferung.
Globale Bestände/Transportmengen sind keinem Zielgebäude zugeordnet. Reservierungen,
Lagerprioritäten, Distrikttransfer, Arbeitswege und Spielnavigation können Lieferung
weiter verhindern. Kapazität ist je Gut geprüft und nicht additiv. Mehrfachzugänge
bleiben unbekannt. Windursachen und dauerhafter Durchsatz werden nicht erfunden.
Native Zutatenbereitschaft hat Vorrang vor leeren Inventaren eines laufenden Zyklus.
Andere Produzenten ohne Manufactory werden ausdrücklich als nicht unterstützt gezeigt.

## Produktionslauf

`inspect_simulation_run` ergänzt bei Läufen mit buildingIds automatisch
`currentBuildingOperations`, `bottleneckObservedGameHours` und `bottleneckState`.
Diese Daten sind frisch und können sich auch nach Laufende ändern. Dagegen bleiben
buildingProduction, seenBlockers, Erstnachweiszeit und terminaler Ausgang eingefroren.
Fehlende aktuelle Beobachtung ergibt unavailable und verändert den Lauf nicht.
Kein Endpunktscan beim Start, Abbruch oder auf Simulationsticks. Güter-/Forschungs-
zielläufe behalten ihre vorhandene aggregierte Diagnose; für Details bis acht
konkrete Produzenten auswählen und die Sammelabfrage nutzen.

## Gate und begrenzter Livepilot

Vorbereitete Tests: Achtfachgrenze, gültige/fehlerhafte Transportbelege, Lagerraum,
globale Knappheit gegenüber fehlender Lieferung, Brennstoff, Personal/Kraft,
Teilabdeckung, native Bereitschaft, kompakter Sammeltransport und Trennung des
historischen Produktionsnachweises von aktuellen Beobachtungen. Versionsprofile
um 0.43 ergänzt; unbekannte Version bleibt synthetisch 99.0.0.

Nach menschlichem Gate:

1. Bis drei geeignete Manufakturen mit diagnose=true lesen; ein Zutaten- und ein
   Produktlager mit Einzelinventar/Einstellungen/Wegabfrage gegenprüfen.
2. Einen belegten Engpass (z. B. voller Ausgang oder fehlende Zutat) und eine
   Personal-/Pause-/Kraftkonstellation vergleichen. Inaktiver Verbraucher darf
   keinen erfundenen Anschlussdefekt liefern. Keine aufwendige Kolonieerschließung.
3. Einen begrenzten buildingIds-Lauf lesen: frische Diagnose passt zu Einzelbelegen,
   eingefrorener Produktionsnachweis bleibt bei späterer Änderung unverändert.
4. Neunte Diagnose-ID vor Transport abweisen. Bei widersprüchlichem Kontrollfall
   stoppen und Ursache beheben, nicht weiter skalieren. Acht Gebäude und große
   Endpunktbudgets ohne Liveprüfung nicht als praktisch abgenommen bezeichnen.

Backend vor Übergabe stoppen/sperren; Commit und Push erst nach Gate und Livepilot.

## Erster Livepilot: Fehler, noch keine Abnahme

Am 2026-10-11 Backendstart und Mod 0.43 bestätigt. Normale Betriebsabfrage
funktioniert, diagnose=true scheitert für Grill/Papierfabrik/Bäckerei. Erfinder
liefert fälschlich unavailable mit Standardwerten. Ursache dieses Kontrollfalls:
native Newtonsoft-Standardschreibweise traf auf case-sensitiven Web-Vertrag;
vorherige Tests serialisierten nur mit Web-Optionen und verdeckten den Fehler.

Korrektur: explizite kleingeschriebene Wire-Felder auch verschachtelt, leerer
Pflichtzustand statt stiller unavailable-Vorbelegung; neue Default-Serializer-
Regression. Beschädigte Umlaute repariert. Inventarzugriffe nur auf dort bekannte
Güter; verbleibende Beobachtungsfehler liefern unavailable mit failureStage
(access/local_inventory/global_stock/endpoint_inventory/road). Das ist ein
Diagnosefehler, kein wirtschaftlicher Engpass und keine bestandene Liveabnahme.
Der zusätzliche Grill-Fehler ist noch nicht abschließend erklärt.

Keine Bau-, Einstellungs- oder Simulationsänderungen ausgeführt. Pilot gestoppt.
Erneutes menschliches Gate und dieselben gezielten Kontrollen erforderlich;
kein Commit/Push.

## Erweiterte Versionsabnahme auf Nutzerauftrag

Zusätzlich vorbereitete automatische Regressionen: komplette Achtfachdiagnose,
32er-Standardabfrage ohne Diagnose, Reihenfolge und not_found innerhalb einer Charge,
neunte ID/falscher Parametertyp/doppelte ID ohne Spielrequest, Personal/Arbeitszeit,
unbekannte Kraft-/Zugangsdaten sowie unfertige Gebäude, fehlendes Rezept und
nicht unterstützte Produktion. Die bestehenden Vertrags-, Versionsprofil-,
Konfigurations- und Produktionslauftests laufen im menschlichen Gate mit.

Live nach erfolgreichem Gate zusätzlich zum fachlichen Dreierpilot:

- Standardabfrage und Detailansicht gegen Diagnosebelege vergleichen; keine
  verlorenen Inventar-/Rezept-/Personalwerte und keine beschädigten Umlaute.
- Lagergut, Annahmemodus, Bestand und freien Platz sowie einen tatsächlich
  geprüften Weg mit den vorhandenen Einzelwerkzeugen gegenkontrollieren.
- Passende erreichbare Quelle bei lokalem Mangel von globaler Knappheit trennen;
  falls in diesem Spiel nicht vorhanden, als nicht live geprüft ausweisen.
- Missing-ID und neunte Diagnose-ID kontrollieren; Fehler dürfen weder Simulation
  noch Einstellungen ändern. Aktuelle Sessionbindung erhalten.
- Begrenzten Produktionslauf und eingefrorene Historie gegen spätere aktuelle
  Diagnose prüfen; unbekannte Diagnose darf den alten Erfolg nicht umdeuten.
- Vor/nach repräsentativer Sammelabfrage Antwortgröße und Laufzeit erfassen.
  Keine Lasttestschleife; bei Widerspruch oder Beobachtungsfehler zuerst stoppen.

Gate ausstehend: Diese Ergänzung ist noch kein ausgeführter Testnachweis.
Backend bleibt bis zum nächsten live bereit gestoppt und gesperrt.

## Ergänzender Livepilot der installierten Erstfassung

Auf ausdrücklichen Nutzerauftrag vor dem nächsten menschlichen Gate getestet:

- Neunte Diagnose-ID: invalid_argument. Fehlende ID: not_found mit leerem
  Summary. Fremde Session: stale_session. Kein Schreibauftrag dieser Kontrollen.
- Dreier-Standardabfrage mit Full liefert weiterhin Rezept, Inventare, Personal
  und Kraft. Papierfabrik: voller Ausgang und zeitweise belegtes Kraftdefizit;
  Grill: Produktplatz für den nächsten Ausstoß fehlt trotz freier Gesamtkapazität.
- Erfinder: ein nativer Zyklus nach 0,6875 Spielstunden / rund 4,58 realen Sekunden,
  automatischer Abschluss mit bestätigter Pause. Diagnose im Laufpfad funktioniert
  (dieser serialisiert bereits camelCase); 43 Inventargebäude abgedeckt, keine Güter.
- Erfinder anschließend pausiert: aktuelle Diagnose zeigt Pause und fehlende
  Besetzung, eingefrorener Erfolg bleibt identisch. Danach wieder aktiviert und
  Einstellung separat rückgelesen.
- Papierfabrik: 0,125 Spielstunden / rund 0,82 Sekunden, kein Zyklus,
  building_production_time_limit und bestätigte Pause. Aktuelle Diagnose unavailable.
  Damit betrifft der offene native Güter-/Inventarpfad auch ein Rezept ohne
  Brennstoff und ist nicht allein ein Grill- oder JSON-Schreibweisenfehler.
- Nach weiterer Spielzeit bleibt Erfinder-buildingProduction strukturell identisch;
  aktueller Diagnosezeitpunkt steigt unabhängig vom historischen Nachweiszeitpunkt.

Endzustand: Simulation pausiert, Erfinder aktiv. Insgesamt 0,8125 Spielstunden
fortgeschritten; keine Bauten, Wege oder Rohstoffe entfernt. Keine allgemeine
0.43-Abnahme: Quellen-/Produktlager-/Wegbelege und kompletter Diagnosevergleich
bleiben bis zur Fehlerkorrektur offen. Noch keine aussagekräftige Effizienzmessung
für eine erfolgreiche Güterdiagnose. Menschliches Gate der vorbereiteten Korrektur
und abschließender Livepilot erforderlich; kein Commit/Push.

## Korrekturpilot: Wire behoben, Endpunktzugriff offen

Nach erneutem menschlichem Gate: Erfinder liefert korrekt observed mit 43
Inventargebäuden; Feldnamen und Umlaute sind repariert. Grill und Papierfabrik
melden beide ausdrücklich unavailable / endpoint_inventory. Kein falscher
Knappheits- oder Lagerbaubefund; keine weiteren Simulationen oder Eingriffe.

Öffentliche lokale Inventory-Metadaten lesend geprüft (kein Build/Test): IsInput,
IsOutput, Gives, Takes, HasUnreservedStock und UnreservedCapacity verfügbar.
Endpunktprüfung jetzt mit Rollenprüfung vor rollenspezifischen Zugriffen und
nativen Gives/Takes statt direktem InputGoods/OutputGoods-Zugriff. Regression
prüft insbesondere, dass fehlende Rollendienste gar nicht aufgerufen werden.
Die konkrete Ausnahme im bisherigen Endpunktzugriff war nicht verfügbar;
die vollständige Fehlerbehebung ist erst im nächsten Livepilot nachzuweisen.
Erneutes menschliches Gate ausstehend. Kein Commit/Push.

## Abschluss-Livepilot — bestanden (2026-10-11)

Nach menschlichem Gate der Rollenprüfung: Grill und Papierfabrik liefern observed
statt endpoint_inventory. Zutaten, Brennstoff, Produkte und konkrete nächste
Schritte sind vorhanden. Erfinder ohne Güterzeilen ebenfalls korrekt beobachtet.

- Spadderdock-Quelle: native Ausgabe mit neun Einheiten, vier unreserviert;
  Weg separat bestätigt (53,4). Logquellen ebenfalls erreichbar erkannt.
- Papierfabrik: 20 Papier im vollen Ausgang, ausreichend Zutaten und Kraft
  (137 Angebot / 80 Nachfrage), vorhandenes Papierlager 30/30 voll.
- Leeres bestehendes Lager vorübergehend auf Papier/accept gestellt: ein
  erreichbares Ziel mit 30 freiem Platz erkannt; Weg separat bestätigt (27,4).
  Befund output_full_destination_available empfiehlt zuerst Abtransport.
  Dasselbe Lager im Leermodus wird ausgeschlossen. Anschließend ursprüngliche
  Auswahl Carrot/empty und leerer Bestand separat bestätigt.
- Grill kurz auf natives Kastanienrezept gestellt: lokal/global null,
  ingredients_not_ready, global_available_shortage und no_public_source.
  Vorhandener Brennstoff bleibt korrekt erkannt. Originalrezept danach separat
  bestätigt. Simulation während aller Einstellungsgegenproben pausiert.
- Gemeinsamer Lauf Erfinder/Papierfabrik endet nach 1,21875 Spielstunden
  (8,13 reale Sekunden) am Zeitlimit mit bestätigter Pause: ein Erfinderzyklus,
  kein Papierzyklus, kein falscher Gesamterfolg. Frische Diagnose für beide
  observed, Papierblockade stimmt mit Einzelbelegen überein.

Frühere Kontrollen zu Session, neunter ID, fehlender ID, Pause/Personal und
eingefrorener Historie ergänzen diesen Abschluss. Keine Bauten, Wege oder Rohstoffe
gelöscht. Endzustand: pausiert, ursprüngliche Lager-/Grilleinstellungen.

Grenzen: maximal drei verschiedene Gebäude live diagnostiziert, zwei gemeinsam
im Produktionslauf. Achtfachumfang, über 128 Endpunkte, unbekannte Wege und
weitere Fraktionsrezepte hier nur durch automatische Vertragsfälle bzw. noch
nicht live belegt. Kein formaler Laufzeit-/Antwortgrößenbenchmark. Globale
Knappheit live geprüft; gezielter lokaler Mangel trotz verfügbarer Quelle bleibt
eine automatische Gegenprobe, kein zusätzlicher Livebeleg.

## Abschließende Beschreibungskorrektur — abgenommen

Nach erneutem menschlichem Gate und live bereit am 2026-10-11 Backend gestartet,
Bridge 0.43.0 und neue Session bestätigt. Der vom Backend aktualisierte
Werkzeugkatalog enthält korrekte Umlaute in Gebäudesuche, Betriebsabfrage und
diagnose-Parameter. Erneute gemeinsame Diagnose von Grill und Papierfabrik:
beide observed, je 40 Endpunktgebäude vollständig geprüft, ein nativer Request.
Grill bereit mit erreichbaren Zutaten-/Brennstoffquellen; Papierfabrik voller
Ausgang ohne geeignetes freies Lager bei ausreichender Kraft. Keine Schreibaktion
oder neue Simulation für diese Textkorrektur. Fachliche Vorbelege bleiben gültig.
