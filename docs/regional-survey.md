# Regionale Flächensuche — begrenzt live abgenommen

## Fortsetzung 2026-10-05 — unknown-Reparatur live bestanden

Nutzer hat die Arbeit mit live bereit fortgesetzt. Neues Paket
0.35.3-20261005-165123-33f2985e: alle fünf installierten Paketdateien stimmen.
Anfangs Transport closed; nach erneuter Bereitmeldung reguläre MCP-Verbindung
wieder erreichbar. Kein zusätzlicher Server oder Spielsteuerungs-Fallback.

Gleicher 16×16-Nordpilot liefert jetzt drei Bodenrechtecke und vier Baukandidaten,
33 interne Leseaufrufe inklusive zwölf Planungen in etwa 2,65 Sekunden. Die
überlappende Teilfläche mit natürlicher Slope bleibt ohne freie Bodenrechtecke.
Eine unbeteiligte 4×1-Fläche ist separat als hindernisfrei, trocken und auf
richtiger Höhe geprüft. Reparatur im angekündigten Umfang live bestanden;
Mehrteilflächen-Überlappung synthetisch abgedeckt, nicht zusätzlich live erzeugt.
Nächster Schritt: nachweislich erreichbare Produktionsflächen vergrößern.



## Vor menschlichem Gate — unbekannte Objekte regional begrenzen (2026-10-05)

Fortsetzung des Gesamtziels: Nordscan 16×16 liefert wegen unbekannter Objekte
keine Bodenrechtecke. Eine natürliche Slope auf niedrigerer Ebene wurde in einer
überlappenden Teilfläche direkt nachgewiesen. Fehler im Klassifikator: jedes
other-Objekt sperrte alle ansonsten freien Zellen der gesamten Region.

Korrektur: unbekannte Grundrisse sperren nur die nativen 8×8-Abfrageflächen,
die ihre Überlappung tatsächlich melden. Überlappt ein Objekt zwei Teilflächen,
bleiben beide gesperrt. Bekannte Gebäude, Eingänge und Pflanzmarkierungen bleiben
geschützt. Keine Annahme über den unbekannten Grundriss, keine neue Baufreigabe.
Zwei synthetische Regressionsfälle ergänzt; Quellcode/Diff geprüft. Kein Build,
keine Installation und kein Commit/Push dieser Reparatur.

Nach menschlichem Gate denselben Nordausschnitt prüfen: unbekannte Objekte dürfen
unbeteiligte Teilflächen nicht mehr sperren. Betroffene Überlappungsflächen müssen
unbekannt bleiben; freie Rechtecke stichprobenweise nativ prüfen. Bei falschem
Kontrollfall stoppen. Aktueller Ausbau unverändert: 32 Biber, 36 Betten, 24/157;
Holz frei 1. Keine neuen Spielaktionen in dieser Diagnose. Aufforstung/Nahrung
bleiben der nächste Schritt nach Reparatur. Die untersuchte Nordreihe enthält
auch einen versetzten Wohnhausgrundriss und wurde deshalb nicht bepflanzt.



## Praxistest: regionaler Kandidat bis zum fertigen Wohnhaus

2026-10-04: Ersten regional gefundenen Lodge-Kandidaten im Entwicklungspilot
frisch geplant und gebaut. Tatsächlicher Baustellenzugang buildersReachable=true
vor Zeitlauf, anschließend finished_accessible: Gebäude fertig, Eingang frei
und erreichbar. Exakt 24 Spielstunden, Überschwingen 0, Endpause bestätigt.
Bevölkerung danach 32 (25 Erwachsene, 7 Kinder), 36 Betten, vier frei;
keine kritischen Bedürfnisflags. Vorlagenabdeckung unverändert 24/157.

Versorgungsgrenze: Wasser 83, Beeren 63, frei verfügbares Holz 1. Karottenbestand
während des Laufs 0; nach Abschluss 30 lebende Karottenpflanzen, davon zwei reif,
keine mit Wasserstress. Forststichprobe: 13 lebende Kiefern, noch nicht ausgewachsen,
kein Wasserstress. Besetzte westliche Holzfällerflagge meldet laufenden Job;
das allein beweist keinen ausreichenden Holzoutput. Nächster Ausbau muss zuerst
Holz- und Nahrungsproduktion vergrößern, nicht weitere Baumaterialien verplanen.

Bedienungsfehler im Test: advance_building_project akzeptiert waitSeconds nur
0–20, Bauchargen dagegen 0–45. Versuch mit 30 vor Ausführung abgelehnt; lesend
awaiting_simulation und run=null bestätigt, dann gezielt auf 20 korrigiert.
Backlog: Parametergrenzen in kurzen Werkzeugbeschreibungen nennen und reine
Parameterfehler von möglicherweise unbestätigten Aktionen unterscheiden.



## Live bestanden — regionale Flächensuche (2026-10-04)

Menschliches Gate abgeschlossen, fünf installierte Paketdateien mit Paket
0.35.3-20261004-234748-393364c9 abgeglichen. 16×16-Pilot: 32 native Leseaufrufe,
darunter zwölf Planungen, in etwa 2,6 Sekunden. Vier Baukandidaten und vier
Bodenrechtecke in einer kompakten Antwort. Richtwert etwa 30 Aufrufe knapp
überschritten; feste Obergrenze 82 eingehalten. Kein zusätzlicher Scan nötig.

Alle 30 Feldmarkierungen als belegt, sechs unabhängig abgefragte tiefere
Wasserzellen als andere Bodenhöhe ausgeschlossen. Stichprobe von 17 vorhandenen
Gebäuden/Wegen: belegte Grundrisszellen und Eingänge nicht frei. Größtes freies
Bodenrechteck unabhängig ohne überlappende Objekte. Erster Kandidat nativ
validiert: Geometrie gültig, Eingang angeschlossen, keine verlorenen Verbindungen
in den geprüften Bestandsproben, Vorschau wiederhergestellt. Baustellenzugang
und tatsächlicher Fertigzugang bleiben unknown; keine allgemeine Baufreigabe.

Negativkontrolle bei bestätigtem Tempo 1 liefert state_conflict ohne Bericht.
Kontrolllauf danach abgebrochen, Pause separat bestätigt. Feature im begrenzten
Umfang abgenommen; keine Behauptung vollständiger Regionssuche, Bewirtschaftung
oder neuer Gebäude durch diese lesende Abfrage. Nächster Schritt: Kandidaten für
den weiteren Gesamtausbau verwenden. Letzter Ausbaucheckpoint: 31 Biber, 24/157.



`survey_region` ist eine rein lesende MCP-Zusammenfassung auf vorhandenen nativen
Schnittstellen. Keine Änderung an Bridge, Baugrenzen, Spielzeit oder Spielstand.
Eingabe: session, districtId, template, x/y/z, width/height (1–16).
Vorher und nachher muss Tempo 0 beobachtet werden; keine automatische Pause.

Ein Aufruf bündelt Gelände, vollständige Gebäudegrundrisse, vorhandene Eingänge,
Vegetation und auch noch leere Feld-/Forstmarkierungen. Rasterzeilen laufen von
kleinem nach großem y; Spalten von kleinem nach großem x. Legende im Ergebnis.
Bis zu sechs nicht überlappende Bodenrechtecke, jeweils höchstens 4×4, dienen als
Anbauhinweise. Bewässerung, Bodenfruchtbarkeit, Pflanzenvalidierung und
Arbeiterreichweite sind damit ausdrücklich nicht nachgewiesen.

Bis zu drei überlappende Suchfenster von höchstens 8×8 mit beobachtetem Weg und
möglichst viel trockenem freien Boden werden priorisiert. Für jedes Fenster
prüft die vorhandene native Gebäudeplanung vier Drehungen. Bis zu vier nach
neuem Wegbedarf sortierte Kandidaten enthalten Ursprungsposition, Fenster,
Drehung und Planbezug. Das sind keine ausführbaren Bauaufträge: gemeinsame
Validierung, Bauzugang und Schutz bestehender Zugänge bleiben unverändert nötig.
Leere Ergebnisliste beweist nicht, dass die ganze Region unbebaubar ist.

Grenzen: eine Bodenhöhe, keine automatische Räumplanung oder Höhenanschlüsse;
Vegetationsursprünge sind keine vollständigen Pflanzen-Grundrisse. Ein Objekt,
dessen Ursprung außerhalb des abgefragten Teilbereichs liegt, macht dessen freie
Zellen konservativ unbekannt. Gebäude mit abgeschnittenem Grundriss verhindern
den gesamten Bericht, auch bei Ursprung außerhalb der Region. Bestehende Wege
sind noch kein Distriktanschlussnachweis. Seiten sind trotz Pausenkontrollen nicht
atomar; Sitzungs-/Versionswechsel, unstabile Gesamtzahlen und Duplikate brechen ab.

Feste Grenzen: 512 Gebäude, je 512 Feld-/Forstmarkierungen, 128 Objekte je
8×8-Teilfläche; maximal 82 native Leseaufrufe einschließlich höchstens zwölf
Planungen. Keine Wiederholungen, keine Mutation. NativeReads und PlanningCalls
machen den tatsächlichen Aufwand sichtbar. Unvollständige Daten liefern keinen
Teilbericht mit vermeintlich freien Flächen.

## Prüfung und Übergabe

Gezielte synthetische Tests ergänzt: Wasser/Höhen, versetzter Gebäudegrundriss,
Bestandseingang, unbepflanzte Markierung, begrenzte überlappende Suche, kompakte
Antwort, fehlende/abgeschnittene Geometrie, Daten-/Sitzungswechsel und laufendes
Spiel. Katalogintegration ergänzt. Nur Quellcode/Diff geprüft, nicht ausgeführt.

Nach menschlichem Skriptlauf und `live bereit`: ein 16×16-Ausschnitt mit
bekannten Feldern, Wasser/Höhen und Gebäuden. Raster anhand weniger gezielter
Rückfragen abgleichen; keine geschützte Fläche als freies Bodenrechteck.
Ein gelieferter Baukandidat wird mit der bestehenden nativen Bauprüfung geprüft;
kein Kandidat wird als Gesamtablehnung interpretiert. Negativkontrolle bei
laufender Simulation muss vor dem Flächenscan ablehnen. Ziel: eine gebündelte
Entscheidung statt der bisherigen einzelnen Gelände-/Hindernis-/Markierungs-
und Drehungssuchen; im aktuellen kleinen Spielstand höchstens etwa 30 native
Leseaufrufe für den Hauptpilot. Bei falscher Klassifikation stoppen und reparieren.
