# Projektstand

Stand: 2026-10-03. Das Projekt entwickelt eine native MCP-Steuerung für Timberborn.
Das Spiel ist ausschließlich Testsystem; konkrete Spielstände gehören nicht zur
Projektbeschreibung.

## Phase E abgeschlossen

Mit 0.35.1 ist Phase E im vereinbarten begrenzten Umfang abgeschlossen.
[Abschluss und Nachweise](missionsplan.md#abschluss-phase-e).
Vollständiger Bauphasenschutz, Umweg-Schutzfolge, freie Höhen-/größere
Projektplanung und besondere Gebäude-/Kompatibilitätsfälle bleiben als
[F01–F05 im Backlog](BACKLOG.md#offene-folgearbeiten-nach-phase-e) offen.
Sie sind Folgearbeiten, keine ausstehende E-Abnahme; keine Schutzgrenze gelockert.

## Letzter technischer Abschluss — 0.35.1 budgetgerechte ebene Routensuche

Nach menschlichem Gate als 0.35.1 geladen. Bauablauf und isolierte
Fünf-Wege-Ablehnung mit anschließender Vier-Wege-Kandidatensuche live bestanden. Der Planer
minimiert neue Wegfelder, bei Gleichstand die Routenlänge, und verwirft Standorte
über dem unveränderten Vier-Wege-Limit vor Belegung der vier Vorschlagsplätze.
Volle Grundfläche bleibt gesperrt; native Gemeinschaftsvorschau und Zugangsnachweise
bleiben erforderlich. Bank-/LargePile-Livehistorie im MCP-Profil ergänzt.
Fünf Kandidaten geprüft, einer verworfen, vier Vorschläge mit 4/3/2/1 Wegen;
Wiederholung identisch. Zwei Banken um Hindernis fertig und erreichbar,
gefährliche Hauptwegvorschau wegen 28 Anschlussverlusten abgelehnt. Sechs Wege
für den abschließenden reinen Planungstest entfernt, Teststreifen bleibt offen.
[Nachweis und Grenzen](docs/road-budget-planner.md). Simulation pausiert.

## Vorheriger Abschluss — 0.35.0 generischer ebener Baupilot live bestanden

Gebäudenamensliste durch aktiven Katalog und vollständige gedrehte Spielgeometrie
ersetzt. Routenbaustein für mehrere Zugangskandidaten vorbereitet; öffentliche
Spiel-API liefert derzeit nur einen Wegzugang. Schutzprüfungen und feste
Vertikalpiloten unverändert. Lodge-Historie Drehung 3 im neuen Profil nachgetragen.
Nach menschlichem Gate Bank (ein Feld, zwei neue Wege) und großes Freiluftlager
(3x3 Grundfläche, ein neuer Weg), beide Drehung 1, fertig gebaut; Bau-/Fertigzugang
bestätigt. Einzelnes belegtes äußeres Eckfeld des großen Grundrisses nativ
abgelehnt. Blockierter Hauptweg hätte 24 erfasste Anschlüsse
verloren und wurde in der nativen Vorschau abgelehnt. Höhenkontrolle,
Bestandszugänge auf beiden Ebenen und Replay beider Projekte bestanden. Begrenzte Läufe exakt,
Abschluss pausiert. Direkt-fertig-Distriktzentrum und Spiel-Multi-Eingang nicht
live belegt; Bank-/LargePile-Historie seit 0.35.1 im MCP-Profil ergänzt.
[Umsetzung und Nachweis](docs/generic-building-project.md).

## Letzter Abschluss — 0.34.0 live bestanden

Ebener Projektpilot um Lodge.Folktails erweitert; vorhandene Spielgeometrie und
Zugangsprüfungen unverändert genutzt. MCP-Nachweise je Gebäudevorlage getrennt,
keine Lagerabdeckung auf Wohnhäuser übertragen. Gedrehtes Wohnhaus in Rotation 3
mit zwei neuen fertigen Anschlusswegen, tatsächlichem Bau-/Fertigzugang und
erhaltenen Vergleichsanschlüssen live bestanden. Grundriss-Negativkontrollen
und Replay ohne neue Objekte bestanden. Profil-Historie noch vor Abnahme;
neuen Wohnhausnachweis bei nächstem ohnehin nötigen MCP-Gate aufnehmen.
[Nachweis](docs/lodge-project-pilot.md).

## Letzter Abschluss — 0.33.1 live bestanden

0.33.1 ist installiert und live geprüft: bis zu vier sequenzielle Vertikalprojekte pro Sitzung,
gemeinsames Budget aller Modi, erhaltene Replay-/Baustellensperren. MCP-Profil
aktualisiert auf die vier historisch belegten Lager-Treppendrehungen.
Eine Treppe und anschließend ein siebenstufiges Lagerprojekt ohne Neuladen
fertiggebaut. Startsperre bei unfertigem Vorgänger/Schlusslager und Replay
während/nach dem Folgeprojekt bestanden; acht Objekte ohne Doppelbau.
Bau-/Fertigzugang, obere/untere Wege und Vergleichslagerzugang geprüft.
[Umfang und Nachweis](docs/vertical-sequential-pilot.md).

## Verifizierter Stand

Zusätzlicher E-Livefall auf unveränderter 0.33.0: vertikales Lagerprojekt mit
Treppendrehung 0 / Lagerdrehung 2, alle sieben Objekte fertig. Lagerbaustelle
builder-erreichbar, fertiger Eingang frei, Distanz 34; beide oberen Wege und
unterer Bestandsweg verbunden, Vergleichslager unverändert bei Distanz 25.
Replay ohne Doppelbauten. Ergänzend Treppendrehung 2 / Lagerdrehung 0 live
bestanden: sieben fertige Objekte, Bau-/Fertigzugang und Distanz 36, Bestandsweg
verbunden, Vergleichslager unverändert bei Distanz 15, Replay ohne Doppelbauten.
Treppendrehung 1 / Lagerdrehung 3 ebenfalls live bestanden: sieben fertige
Objekte, Bau-/Fertigzugang und Distanz 50; Bestandsweg verbunden,
Vergleichslager unverändert bei Distanz 15, Replay ohne Doppelbauten.
Alle vier Treppendrehungen des festen vertikalen Lagerprojekts begrenzt dokumentiert.
Der damalige MCP-Nachweiskatalog enthielt noch 3; Aktualisierung in 0.33.1 bestanden.
[Nachweis](docs/vertical-warehouse-pilot.md).

MCP-only-Abfrage `inspect_building_capabilities` nach korrigiertem menschlichem
Gate live bestanden: Profil 0.33.0/Folktails, fünf Modi mit erwarteten Grenzen,
historische Live-Abdeckung und Server-Schalter getrennt. Falsche Sitzung und
Zusatzparameter abgewiesen; Objektzahl, Pause und Spielzeit unverändert.
Keine Baufreigabe, Bridge bleibt 0.33.0. [Nachweis](docs/building-capabilities.md).

0.33.0 nach menschlichem Skript-Gate installiert und live bestanden: mittleres
ebenes Folktails-Lager im bestehenden Entwicklungspilot, zusätzliche Vorlagenbindung
der Belege; keine Lockerung der B-Sperren. Rotation 3 mit sechs Zellen (gedreht
2×3), tatsächlicher Bauarbeiterzugang und anschließend fertig mit freiem Eingang,
Distriktdistanz 25. Blockierte entfernte Grundrisszellen und zu kleine Suchregion
korrekt ausgeschlossen. Zwei Bestandswege verbunden, Vergleichslager unverändert
bei Distanz 19, Aktions-ID-Replay ohne neue Objekte.
[Nachweis und Grenzen](docs/medium-warehouse-pilot.md).

Ergänzend auf unveränderter 0.33.0 live bestanden: mittleres Lager Rotation 1
mit drei tatsächlich neuen, einzeln fertig und distriktverbunden rückgelesenen
Bodenwegen. Baustellenzugang vorhanden; fertiger Eingang frei, Distanz 36.
Ursprünglicher Anschlussweg erhalten, Vergleichslager unverändert bei Distanz
19/25; Vier-Objekt-Beleg wiederabfragbar ohne Doppelbauten.

Ebene Drehungen 0/2 des mittleren Lagers ebenfalls auf unveränderter 0.33.0 live
bestanden: jeweils sechs korrekte Grundrissfelder, tatsächlicher Bauarbeiterzugang,
fertige Lager mit freiem Eingang und Distanz 31/33. Beide Anschlusswege erhalten,
Vergleichslager unverändert bei Distanz 19. Aufträge sequenziell nach tatsächlicher
Fertigstellung. Alle vier ebenen Drehungen der zusätzlichen Vorlage damit begrenzt
belegt. Nächster Vorschlag: unterstützten Projektumfang und Nachweisniveau im MCP
strukturiert ausweisen; keine freie Höhenplanung oder allgemeine Schutzbehauptung.

0.32.1 nach menschlichem Skript-Gate installiert und live bestanden: konservative
Baustellensperre B. Neue Bauaktionen benötigen vollständige Bestandsinventur
ohne offene unabhängige Baustellen. Eigene Baustellen dürfen während Bestätigung
und Warten bestehen, aber vor jeder Folgeschritt-Platzierung müssen sie fertig
sein. Neue unabhängige Baustellen stoppen laufende Projekte. Alle Treppenmodi
verwenden jetzt die Wartefolge. Vorschau bleibt unknown, constructionCovered=false.
Lager-/Treppenprojekt und Einzelplatzierung mit unabhängiger Baustelle abgelehnt;
Objektzahl unverändert, bestehender Builderzugang erhalten. Nach deren Fertigstellung
eigener Plattformablauf mit allen fünf fertigen Objekten und beiden verbundenen
oberen Wegen bestanden. B im freigegebenen Ausschlussumfang geschlossen.

Erster E-Schritt auf unveränderter 0.32.1 live bestanden: ebene kleine Lager
mit Rotation 0 und 2 als Baustelle builder-erreichbar und anschließend fertig,
freie Eingänge und Distriktdistanz jeweils 32. Gemeinsamer Bestandsweg verbunden;
Vergleichslager unverändert bei Distanz 19. Zusammen mit früheren C-Fällen sind
alle vier ebenen Lagerdrehungen begrenzt live belegt. E nicht insgesamt beendet;
der begrenzte 0.33.0-Vorlagen-/Drehungsausbau ist ebenfalls belegt, kein freier
3-D-Ausbau. [Nachweis](docs/building-rotation-pilot.md).

0.32.0 nach menschlichem Skript-Gate installiert und live geprüft: Etappe D
mit `stair_platform_warehouse_pilot`: Treppe, drei Plattformen, zwei obere Wege
und ein kleines Lager mit rückwärtsgerichtetem Eingang. Folgeschritte warten auf
fertige Vorgänger; vor dem Lager müssen beide oberen Wege distriktverbunden sein.
Erfasste ursprüngliche Distriktverbindungen werden während des Auftrags geprüft.
Dies ersetzt keinen vollständigen Bauphasen-Vorabnachweis (B bleibt offen).
Sieben Objekte fertig rückgelesen; Lagerbaustelle builder-erreichbar, fertiger
Eingang frei und native Distriktdistanz 22. Beide oberen Wege verbunden; unterer
Bestandsweg bleibt verbunden, Vergleichslager unverändert bei Distanz 11.
Identische Aktions-ID liefert alten Beleg und weiterhin genau sieben Objekte.
Vier Drehungen synthetisch geprüft, eine davon live; keine allgemeine 3-D-Planung.

0.30.0: `inspect_selection` über den öffentlichen `EntitySelectionService`
installiert und live geprüft. Distriktzentrale und Erfinderwerkstatt korrekt
erkannt; Auswahlwechsel liefert die neue ID, Vorlage und Rasterposition, jeweils
über `inspect_building` gegengeprüft. Aufgehobene Auswahl liefert `state=none`
und `target=null`. Keine Auswahl-/Kameraänderung, Sitzungspflicht.
`unsupported` und allgemeine Entity-/Weltpositionsfälle sind durch synthetische
Tests abgedeckt, aber nicht separat live belegt. Auswahl allein ist kein
Änderungsauftrag; vor späteren Aktionen frisch lesen und das Ziel prüfen.

| Ebene | Stand |
| --- | --- |
| Bridge | Agent Bridge 0.34.0 installiert; ebenes Wohnhausprojekt mit Bau-/Fertigzugang begrenzt live bestanden; allgemeiner Vorschau-Schutz offen |
| Automatisch | Menschliche Bereitmeldung nach Skript-Gate für 0.34.0; neue Testanzahl nicht übermittelt. Letzter Zahlenstand 0.29.3: 667 erfolgreich, 0 fehlgeschlagen, 3 übersprungen |
| Mod-Build | Timberborn 1.1.2.4 / Folktails, ohne Warnungen und Fehler |
| Laufzeit | Bridge und Schreibfreigabe strukturiert erreichbar |
| Bauprojekt | 0.32.0 live: Treppe, drei Plattformen, zwei obere Wege und kleines Lager; Bauphasen, Baustellen- und fertiger Zugang separat geprüft |

Arbeitsbranch: `codex/road-protection-pilot`. Die eigene Mod nutzt keine
Fremdmod-Pflichtbasis.

## Architektur

MCP über stdio → lokales C#-Backend → authentifiziertes Loopback-HTTP →
Hauptthread-Queue → öffentliche Timberborn-Spielservices. Strukturierte Werkzeuge
ersetzen UI-Automatisierung, Screenshotauswertung und Save-Manipulation.

## Implementierter Umfang

Zustands-, Güter-, Personal-, Bau-, Forschungs-, Flächen-, Entfernungs- und
Simulationswerkzeuge sind implementiert. Aktionen verwenden technische Freigaben,
frische Sessions, fachliche Fehlercodes und Rücklesungen. Bauaufträge bleiben an
Vorschau, Wegschutzdiagnose und schrittweise Bestätigung gebunden.

## Offene Arbeit

0.31.4 nach menschlichem Skript-Gate installiert und live geprüft. Drei Lageraufträge
in derselben Sitzung, zwei neue Wege; alte Belege nach neuen Aufträgen lesbar,
identische erste Anfrage liefert nur ihren ursprünglichen Beleg. Höchstzahl vier,
Fehlersperren und Sonderfälle synthetisch geprüft im menschlichen Gate, nicht
zusätzlich live ausgereizt. Tatsächliche Zugangszellen für drei parallele Baustellen
separat gelesen (acht/sechs/sechs); an fertigen Objekten not_construction.
Ein Plateauzugang erstmals real getrennt: Builder true→false, alle sechs Zugänge
auf gleicher Geländehöhe. Freie Vorschau im getrennten Zustand bestätigt negative
Baseline und Rücknahme. Regulärer Treppenwiederaufbau; danach drei Lager, zwei Wege
und Treppe fertig, drei Eingänge und Straßenverbindungen positiv. Ein begrenzter
Lauf: 14 Spielstunden, etwa 41,4 Echtzeitsekunden, Pause bestätigt, kein Überschuss.
Kontrollierte Bestandsverbindung verbunden, Distanz 11→11. C im begrenzten Umfang
bestanden. B-Untersuchung mit belegter API-Grenze abgeschlossen, B-Schutzabnahme
nicht bestanden: Engpassvorschau geometrisch ungültig, Bezirksverluste 30, aber
lostSites=0; reale Treppentrennung ist kein identischer Vorschau-Eingriff.
Keine allgemeine Vorschau-/Bauphasensicherheit daraus ableiten.

Aktiver Auftrag seit 2026-10-03: Etappen A–E aus missionsplan.md. Nutzer priorisiert
jetzt C samt begrenztem B-Abschluss; keine weitere Blockadesuchserie.
Ein vollständiger Gebäudeablauf mit bestehendem Anschluss ist live nachgewiesen.
Beide Kernziele sind teilweise,
nicht allgemein erreicht. `RoadProtection.constructionCovered=false` verhindert
allgemeine sichere Baufreigaben; Entwicklungspiloten erlauben ausschließlich die
ausgewiesene Vorabnachweislücke. Distriktweg-Zugehörigkeit ersetzt keine
Bauarbeiter-Erreichbarkeit. C-Pilot bestätigt Vorschau, Auftrag, Baustellenzugang,
tatsächliche Fertigstellung und fertigen Eingang samt Straßenverbindung.
Keine neuen Wege erforderlich; Nachbarverbindung erhalten, Distanz jedoch von
10 auf 11 gestiegen. Kein Nachweis unveränderter Wegqualität oder vollständigen
Bestandsschutzes. Neuer Anschlussweg im selben Ablauf inzwischen live bestanden.
Details: [Phase-C-Nachweis](docs/building-completion-pilot.md).

Der ungeprüfte Vier-Wege-Entwurf (vorgesehene 0.30.1) ist zurückgestellt, lokal als
benannter Git-Stash erhalten und nicht im aktiven Quellstand. Wiederaufnahme siehe
BACKLOG.md. Etappe A ändert nur die MCP-Auswertung vorhandener
Validierungsergebnisse, keine Baufreigabe. Aktuell installierte Bridge 0.31.4.

Etappe B: Bridge 0.31.0 installiert und begrenzt live geprüft nach menschlicher
Bereitmeldung. Road-Spill-Baseline passt zur tatsächlichen Builder-Abfrage;
freie Kontrolle und Vorschau-Rücknahme bestanden. Negative Baustellenkontrolle
nicht belegt: zwei Sperrvorschauen verlieren elf Distriktverbindungen, aber
keinen diagnostizierten Baustellenzugang. Kein Abschluss von B, keine Baufreigabe.
Testserie gestoppt; Nutzer hat die begrenzte Detaildiagnose anschließend freigegeben.
Eine echte Lagerbaustelle mit Anschlussweg ist als Testfixture angelegt.
Details und Stopkriterien: [Baustellendiagnose](docs/construction-access-preview.md).

0.31.1: Detailvergleich für eine Baustelle mit maximal acht Zugängen live geprüft.
Freie Vorschau und breite Sperrkontrolle zeigen getrennte Reichweiten-,
Verbindungs- und Belegungswerte; Baseline und Rücknahme bestanden. Alle acht
Verbindungswerte vom Zentralenursprung bereits vorher false bei realem
Builderzugang true: kein geeigneter Verbindungsnachweis. Sechs Zugänge bleiben
trotz Kandidatenbelegung im Reichweitenfeld. Nächster Schritt native Start-/
Zielgültigkeit klären, keine Suchserie. Etappe B und allgemeine Baufreigabe offen.

0.31.2 ist nach menschlicher Bereitmeldung installiert. Nutzer hat eine kleine
Lagerbaustelle bereitgestellt und markiert; Auswahl und realer Builderzugang
wurden strukturiert gelesen. Zentraleingang auf tatsächlichem NavMesh und
Bezirksweg, alle acht Ziele auf tatsächlichem NavMesh. Trotzdem connected und
roadConnected an allen acht Zielen in allen fünf Zuständen false, realer
Builderzugang vor/nach Vorschau true. Vier Ziele im Reichweitenfeld; Baseline
und Rücknahme bestanden, Pause/Spielzeit unverändert. Startpunktwechsel allein
löst die Lücke nicht. Positive Kontrolle gescheitert, keine Sperrvorschau,
kein Commit. Nach erneutem go öffentliche Signaturen geprüft: AreConnected
vermutlich direkte Kante, nicht ganze Route; noch kein abschließender Beweis.
Vorhandene Accessible-Pfadsuche Zentrale zu fertigem Lager live positiv.
Öffentliche tatsächliche Road-Spill-/Pfadsuche vorhanden, keine explizite
Preview-Pfadsuche gefunden. Vorschlag: begrenzte Nachbar-/Fernkontrolle und
tatsächliche Spill-Baseline, danach erst Preview-Engpass. Keine weitere Suchserie.
Etappe B bleibt offen; Details im Fachdokument.

0.31.3 nach menschlichem Skript-Gate live geprüft: drei positive Nachbarkanten,
drei echte Straßenpfade, vier erreichbare/Spill-Ziele bei acht negativen Fern-
Kantenwerten. Freie Vorschau und Ist-Builderzugang passen; Baseline/Rücknahme
bestanden. Genau eine ungültige Sperrvorschau deckt alle vier erreichbaren
Zugangspunkte ab und verliert sieben Bezirksverbindungen, aber keinen
diagnostizierten Baustellenzugang. Ist-Abfragen bleiben unverändert; tatsächlicher
Builderzugang danach true, Pause/Spielzeit unverändert. Diagnosescope bestanden,
keine negative Baustellenkontrolle und kein Abschluss B. Nächster Schritt:
fachlich gültige Blockadekontrolle statt weiterer überlappender Bezirkszentralen;
keine Suchserie. Allgemeine Baufreigabe bleibt gesperrt.

Zusätzliche gültige Einzelpunkt-Vorschau: kleines Lager belegt einen von vier
erreichbaren Zugangspunkten, übrige drei offen; kein Reichweiten-/Bezirksverlust,
Rücknahme und echter Builderzugang unverändert. Kein Negativfall. Diese Fixture
für weitere Blockadesuche gestoppt. Nächster Vorschlag nach erneutem go: isolierter
Einzelzugang mit tatsächlicher Negativkontrolle, dann passende Vorschau.

Erhöhte Nutzer-Fixture nach begrenztem Trägerbau praktisch getrennt: eine fertige
Treppe entfernt, oberer Wegzugang ohne Reichweite, aber vier niedrigere
Geländezugänge weiterhin erreichbar. Builderzugang bleibt true; keine negative
Baustellenkontrolle. Pilot gestoppt, kein automatischer Wiederaufbau. Lager bleibt
unfertig/aktiv, Spiel pausiert. Isolation muss gegen alle tatsächlichen Zugänge
und höheres Nachbargelände geprüft werden, nicht nur gegen die Treppe.

Nach erneutem go genau ein naher Wegpunkt entfernt: Bezirksanbindung des Asts
wechselt true→false, Builderzugang und vier niedrigere Spill-/Reichweitenpunkte
bleiben true. Baseline/Rücknahme der freien Diagnose passen. Negativpilot erneut
gestoppt, keine weitere Abrissserie. Entfernt bleiben Testtreppe und ein Wegpunkt.
Nächste Entscheidung: minimaler separater Testaufbau oder Herkunftsdiagnose der
Spill-Zugänge, statt weiterer Trennversuche im vernetzten Bestand. B bleibt offen.

Zusätzlicher Nutzerauftrag: `reasoning` für sämtliche MCP-Werkzeugaufrufe
verpflichtend (auch Fake/Legacy und Leser). Zentrale Schema-/Eingangsprüfung vor
Backend und Log implementiert; Negativtests und angepasste Integrationstests
im menschlichen Skript-Gate. Live bestanden: gueltige Begruendung akzeptiert,
fehlend/blank als InvalidParams ohne Logeintrag abgewiesen. Feldname und
Ingame-Logformat bleiben unverändert; reine MCP-Anforderung, keine neue Spiel-API.

Plattformpilot 0.29.2 abgeschlossen: Der begrenzte Wartezustand behandelt
Baustellen mit unverändertem Wegschutz und Bau nur bei Pause. Nach drei getrennten
begrenzten Bauphasen sind alle fünf Objekte fertig und der Auftrag abgeschlossen.
Die vertikale Distriktanbindung beider oberer Wege ist mit 0.29.3 direkt belegt;
vollständiger generischer Wegschutz bleibt offen. Details: [Plattformpilot](docs/vertical-platform-pilot.md).

0.29.3 live: `inspect_path_district` fragt die reale Hauptwegzelle gegen ein
konkretes Distriktnetz ab, ohne den Gebäude-Eingangsfilter. Beide oberen Testwege
verbunden; Lager und unfertige Treppe korrekt als unbekannt gemeldet.
[Nachweis](docs/path-district-observation.md).

Etappe A live bestanden am 2026-10-03: zusätzliche `assessment` in
Einzel-/Projektvalidierung, aus bereits geprüften nativen Belegen abgeleitet.
Sieben getrennte Befunde, Gesamtergebnis blocked/unknown; reguläre Baufreigabe
bleibt false. Keine neue Spielabfrage oder Modänderung. Tests für fehlende Basis,
Verluste, Vorschauzugang versus tatsächlichen Zugang und Antwortprüfung ergänzt.
Menschliche Bereitmeldung nach Skript-Gate. Freie Wegkontrolle, Sperrvorschau mit
zwei verlorenen oberen Wegen und gemeinsame Lager-/Wegvorschau korrekt gemeldet;
beide Wege nach Vorschau unabhängig wieder verbunden, Spielzeit unverändert.
Baustellen- und tatsächlich fertiger Zielzugang bleiben unknown. [Nachweis](docs/build-assessment.md).
Nächster Schritt Etappe B: öffentlicher Bauphasen-/Bauarbeiter-Vorabnachweis.

Details: [Fachverträge](docs/README.md), [Backlog](BACKLOG.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
