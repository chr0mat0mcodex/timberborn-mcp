# A12/N10 — Bretterproduktion und Abtransport

Stand 2026-10-18: teilgeprüft, am vereinbarten Stoppkriterium beendet.
Negativkontrolle bestanden; positiver Produktions- und Liefernachweis offen.
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

## Konkrete Wiedervorlage — erneutes Go erforderlich

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
