# A12/N10 — Bretterproduktion und Abtransport

Stand 2026-10-18: begrenzter Pilot nach erneutem Go abgeschlossen.
Negativkontrolle und positive Produktion bestanden; Ausgabeentlastung und
Lagerzufluss beobachtet. Einzelne Quelle-Ziel-Lieferzuordnung bleibt offen.
Der erste Abschnitt dokumentiert den ersten Versuch, die Fortsetzung steht unten.
Keine Code-, Test- oder Konfigurationsdatei geändert, kein neues Gate erforderlich.
Verwendet wurde der abgenommene N08-Laufzeitstand (Bridge 0.74.0); MCP-Build und
Katalog unverändert. Keine Spiel-/Objektkennungen oder Rohlogs gespeichert.

## Umfang und Auswahl

Ein bestehendes Sägewerk und ein vorhandenes Bretterlager; höchstens zwei kurze
Beobachtungsfenster. Vorprüfung über inspect_plank_chain, Betriebs-/Inventarbatch,
native Bezirkszuordnung und reales Kraftnetz. Kein Standort aus bloßer Nähe
als erreichbar angenommen, kein Neubau oder längeres Windwarten im Pilot.

Zwei fertige Sägewerke gefunden. Eines bereits pausiert, ohne Arbeiter und mit
teilweise unbekannten Lieferwegen. Gewählt wurde das zweite: Rezept Plank,
ein zugewiesener Arbeiter, Arbeitszeit aktiv, Zugang beobachtet, 13 Holz und
13 Bretter im Manufactory-Inventar. Ausgabe voll, Kraftbedarf 50 bei Leistung 0.
Realer Netzabruf zeigt Verbindung über zwei vertikale Wellen zu einem Windrad;
die Geometrie ist verbunden, die aktuelle Kraftversorgung dennoch nicht vorhanden.
Keine behauptete konkrete Wetterursache aus supply=0.

Fünf vorhandene kleine Warenlager geprüft: vier voll, eines leer, aber deren
beobachtete Gütermenge enthält keine Bretter. Keine ungeprüfte Umstellung auf Plank.
Anschließend vier kleine Stapellager geprüft: alle voll. Das vorhandene Bretterlager
steht auf Plank/obtain und enthält 20/20 Bretter; die übrigen enthalten Holz bzw.
behandelte Bretter. Der vollständige begrenzte Engpassscan meldet kein geeignetes
freies Produktziel. Daraus folgt keine allgemeine Behauptung über alle möglichen
Lagergrößen oder bisher nicht ausgewählte Gebäude.

## Praktischer Nachweis

Nur die Gebäudepause des gewählten Sägewerks ausdrücklich gesetzt und separat
bestätigt. Danach run_simulation_for mit buildingIds, measureProduction=true,
1,5 Spielstunden, Tempo 3 und 60 Sekunden Echtzeitbudget. Keine completionSpeed-
Option, keine Änderungen während der Messung.

Ergebnis nach rund 10,04 Echtzeitsekunden:

- completed / production_measurement_complete, exakt 1,5 Spielstunden, Pause bestätigt.
- completedCycles=0, buildingProductionReached=false, abgeleitete Brettermenge 0.
- Das gesamte Messfenster als multiple_blockers klassifiziert: Gebäudepause,
  fehlende Arbeiter, inaktiver Verbraucher und volle Ausgabe. Dies isoliert nicht
  die Gebäudepause als alleinige Ursache, bestätigt aber korrekt ausbleibende Zyklen.
- Sägewerksinventar unverändert: 13 Holz und 13 Bretter. Lager unverändert: 20 Bretter.
- Ein beendetes Messfenster wurde korrekt nicht als erfolgreiche Produktion ausgegeben.

Kein zweites Zeitfenster gestartet: Der erforderliche Ausgabeplatz blieb blockiert.
Die temporäre Gebäudepause anschließend aufgehoben und separat bestätigt. Spiel
zum Abschluss weiterhin Tempo 0, kein aktiver Lauf und kein ausstehender Tempobefehl.

## Aussage und Grenze

Die vorhandene Vorprüfung benennt die konkreten Engpässe; der native Zähler erfindet
bei blockierter Produktion keinen Erfolg. Das ist ein nützlicher Teilnachweis,
aber weder positive Produktionsabnahme noch Transport-/Lieferabnahme.

Die Produktionsmessung zählt native ProductionFinished-Ereignisse. Inventare
liefern Bestände/Reservierungen pro Gebäude; das vorhandene Änderungsjournal
liefert keine Warenübergabe mit Quelle und Ziel. Rezeptmengen, globaler Bestand,
Lagerreichweite oder gleichzeitig veränderte Inventare allein belegen keine
bestimmte Lieferung. Im Pilot wurde ohnehin kein solcher Warenfluss beobachtet.

Aufwand bisher klein bis mittel durch Auswahl und gezielte Rückabfragen; Nutzen
ist die Trennung einer blockierten Testumgebung von einem MCP-Funktionsfehler.
Weitere unveränderte Zeitläufe hätten keinen belegten Zusatznutzen. Daher Stopp
entsprechend dem vorab geplanten Kriterium, keine automatische Ausweitung.

## Historische Wiedervorlage — anschließend ausdrücklich freigegeben

Einen minimalen Testaufbau gesondert freigeben: freies geeignetes Bretter-Stapellager
und verlässliche Kraftversorgung am gewählten Sägewerk, bevorzugt vorhandene Teile
nutzen, sonst höchstens ein kleines Stapellager und ein bemanntes Kraftgerät samt
nötigen kurzen Anschlüssen planen. Noch keine geprüften Baukoordinaten oder Zusage,
dass genau diese Teile reichen; Geometrie, Bestandsschutz, Bau-/Fertigzugang,
Kosten inklusive Hilfen und tatsächliches Kraftnetz vorher prüfen.

Danach ein kurzes positives Produktionsfenster mit nativem Zyklusnachweis.
Abtransport separat gegen Ausgangs- und Zielinventar prüfen. Wenn Quelle/Ziel mit
den bestehenden Belegen nicht eindeutig zuordenbar sind, Liefernachweis offenlassen
und erst einen schmalen Telemetrieentwurf vorlegen. Keine automatische Liefer-
Instrumentation, kein größerer Kolonieumbau und keine Wiederholung ohne neuen Auftrag.


## Fortsetzung nach erneutem Go — kleiner Testaufbau

Freigegeben waren freier Bretterlagerplatz und verlässliche Kraft im kleinen
Testaufbau, anschließend gezielte Messung; keine weiteren Erweiterungen.
Ein SmallPile (2 Holz), ein PowerWheel (20 Holz) und zwei kostenlose Wege.
Materialdeckung vorher 260/22 Holz = 1181,8 Prozent; vier Holzernter gefunden,
zu diesem Zeitpunkt außerhalb der Arbeitszeit. Kein Produktions- oder Lieferbeweis
allein aus Vorrat und Produzentenauswahl.

Fünf exakt benötigte Bäume regulär zur Entfernung markiert und deren
Pflanzmarkierungen entfernt. Nach drei Spielstunden zwei, nach weiteren sechs
alle benötigten Hindernisse geräumt. Eine vorherige automatische Zugangssuche
endete am Suchbudget bei noch belegtem Eingang; keine Unerreichbarkeit behauptet.
Anschließend explizite gemeinsame Vier-Teile-Vorschau: native Platzierung und
Eingänge bestätigt, Arbeitszellen road_reachable, null verlorene Verbindungen.
Neue Baustellen bleiben in der Vorschau unbewiesen; begrenzter development_pilot.

Bau mit 24 Spielstunden/150 Echtzeitsekunden Obergrenze, Tempo 7: nach 5,78125
Spielstunden bzw. rund 17 Sekunden alle vier Teile fertig und zugänglich,
automatische Pause. Frische Abschlussdiagnose ohne beobachtete Zugangsverluste;
Ledger freigegeben. Lager auf Plank/accept eingestellt, 20 freie Plätze.
Kraftrad real mit Sägewerk verbunden, ein Arbeiter zugewiesen. Für die Messung
Windrad pausiert, danach wieder aktiviert. Keine weiteren Gebäude oder Kraftwellen.

## Positivmessung und Warenfluss

Ausgang bei Pause: Sägewerk 13 Holz/13 Bretter, neues Lager 0/20 Bretter,
altes Lager 20/20. Fünf Spielstunden measureProduction=true, Tempo 3,
60 Sekunden Echtzeitbudget, keine Einstellungsänderung während der Messung.

- Nach 33,80 Sekunden completed / production_measurement_complete, Endpause bestätigt.
- buildingProductionReached=true; measurement.completedCycles=3 und
  rezeptabgeleitet drei Bretter, 0,6 Zyklen je Spielstunde. Der separate Erstzyklus-
  Beleg completedCycles=1 ist nicht der Gesamtzähler des Messfensters.
- Beobachtete Zeitkategorien: producing 2,21875 h, power 1,5 h,
  multiple_blockers 0,15625 h, unknown 1,125 h. Keine durchgängige Versorgung behauptet.
- Kraftrad am Ende actualOutput=58, Netzbedarf 50, powered=true bei pausiertem
  Windrad. Damit windunabhängiger Betrieb praktisch belegt, keine Garantie über
  Abwesenheiten/Arbeitszeiten des Bedieners oder dauerhafte Leistungsreserve.
- Sägewerk am Ende 12 Holz/1 Brett, outputSpace=true, ready=true.
  Neues Lager 14 Bretter plus eine reservierte Kapazität; altes Lager unverändert 20.
- Native Wegverbindung Sägewerk→neues Lager supported=true, connected=true,
  Distanz 4. Wegverbindung bleibt ein eigener Nachweis, keine Lieferquittung.

Die Ausgabe wurde praktisch entlastet und das neue Lager erhielt Bretter.
Aus Anfangsbestand 13 plus drei produzierten minus Endbestand 1 ergeben sich
15 netto abgegangene Bretter am Werk, während das neue Lager netto 14 erhielt.
Eine weitere Reservierung ist sichtbar, aber weder bestätigte Lieferung noch
Beweis für den Verbleib der Differenz. Inventardeltas und zeitliches Zusammenfallen
liefern keine einzelnen Warenübergaben mit eindeutiger Quelle/Ziel-Zuordnung.
Keine exakte Bruttoliefermenge und keine vollständige Transportabnahme behauptet.

## Nutzen, Aufwand und Abschluss

Nutzen hoch: vorhandene Produktionsmessung positiv bestätigt, reale Kraft von
bloßer Anschlussgeometrie getrennt und den diagnostizierten Ausgabeengpass mit
minimalem Aufbau praktisch gelöst. Aufwand mittel: zwei Gebäude, zwei Wege,
fünf Räumziele, zwei begrenzte Räumfenster, ein automatisch beendetes Bau- und
ein Produktionsfenster. Kein Code, kein Build und keine Installation nötig.
Weitere gleiche Zeitfenster schließen die fehlende Lieferattribution nicht.

Spiel abschließend Tempo 0, kein aktiver Lauf und kein ausstehender Tempobefehl;
Windrad wieder aktiv, Testaufbau bleibt stehen. Anleitung, Werkzeugübersicht,
Brettervertrag und Einstieg aktualisiert; keine projektbezogenen Spielskills vorhanden.
Nur bei konkretem Bedarf als separates Paket einen schmalen Entwurf für native
Quelle/Ziel/Gut/Menge-Übergabetelemetrie erstellen. Keine automatische Umsetzung,
keine weiteren Auftragsfamilien und kein größerer Kolonieausbau.
