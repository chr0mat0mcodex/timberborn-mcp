# Regionale Flächensuche — begrenzt live abgenommen

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
