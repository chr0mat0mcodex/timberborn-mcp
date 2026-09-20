# Projektjournal

> Chronologisches Journal. Aktuell: [Projektstand](../PROJECT_STATE.md), [Mission](../missionsplan.md) und [Backlog](../BACKLOG.md). Frühere Einträge beschreiben ihren damaligen Zustand.

## 2026-09-20 — Alle Geschwindigkeiten und Sollbesetzung live bestanden

0.11.0, begrenzter MCP-Pilot mit 26 fachlichen Aufrufen. Anfangszustand Pause;
0/1/3/7/1 jeweils separat nachgelesen. Pausenbeobachtungen unverändert; bei ca.
1,2 Sekunden Messabstand DayProgress-Deltas 0.00260418 (1×), 0.00781250 (3×),
0.01822915 (7×), 0.00260416 (abschließend 1×). Keine Zusage konstanter realer
Beschleunigung unter anderer Last. Quittungen weiterhin verzögert wirksam.

Distriktzentrum Sollbesetzung 2 -> 3 -> 2 per regulärer Workplace-API, beide
Quittungen applied und separat bestätigt. Worker-Roster bestätigt tatsächliche
Zuordnung 2 -> 3 -> 2. Schlusszustand Soll/Ist/Max 2/2/4, keine Unter-/Überbesetzung.
Keine direkten Biberzuweisungen, kein Retry, kein Bauauftrag. Simulation abschließend
1×. Damit reguläre Geschwindigkeitssteuerung und begrenzter Staffing-Hin-/Rücktest
live abgenommen; die allgemeinen Produktions-/Fällgebietsfragen bleiben offen.

## 2026-09-20 — Alle regulären Geschwindigkeiten umgesetzt und installiert

0.11.0: set_simulation_speed akzeptiert für Ziel/Erwartungswert exakt 0,1,3,7.
Herstellerdaten aus Modding/UI.zip, Views/Game/SpeedControlPanel.uxml belegen
Speed0/Speed1/Speed3/Speed7 als Pause und drei Standardstufen. Keine Screenshots,
UI-Automation oder fremden Assets kopiert. Öffentliche SpeedManager-API bleibt.

200 reguläre Tests bestanden (187 Unit, 13 Integration), drei Live-Tests übersprungen.
MCP-Integration testet 0 -> 1 -> 3 -> 7 -> 1 und ungültige Zwischenwerte.
Mod-Build ohne Warnungen/Fehler. Bei weiterhin beendetem Spiel 0.11.0 installiert:
vorherige Mod gesichert, fünf Paketdateien SHA256-geprüft, Konfiguration bytegenau
beibehalten. Sollbesetzungsfunktion aus 0.10.0 enthalten und weiter freigeschaltet.

Umsetzung/automatisierte Abnahme abgehakt. 3×/7× und Sollbesetzung noch nicht live
getestet. Nach Nutzerstart beide begrenzten Piloten ausführen; keine vollständige
Live-Freigabe aus Build-/Simulationstests ableiten.

## 2026-09-20 — Sollbesetzungssteuerung 0.10.0 installiert

Bei beendetem Spiel bisherigen Mod-Ordner lokal gesichert und fünf Paketdateien
ersetzt; SHA256 jeweils abgeglichen. Bestehende Einstellungen erhalten und
enableStaffing gezielt aktiviert. Keine fremden Mods geändert. Live-Test wartet
auf Nutzerstart und Laden der MCP-Kolonie: Sollbesetzung eines fertigen Arbeitsplatzes
um eins erhöhen, separat nachlesen, auf ursprünglichen Sollwert zurückstellen.

## 2026-09-20 — Popup-Recherche und Sollbesetzung 0.10.0

Nutzeridee für später kurz anhand öffentlicher Spielmetadaten geprüft:
InputBoxShower mit Action<string>-Antwortcallback und DialogBoxShower mit eigener
Nachricht/Inhalt vorhanden. Theoretisch machbar; Frage-ID, asynchrones Antwortlesen,
Abbruch und Szenenwechsel als Zukunftsfeature dokumentiert. Kein Popup implementiert.
Siehe [Popup-Konzept](player-question-popup.md) und Missionsplan 7.27.

Danach freigegebenen MCP-Ausbau fortgesetzt: set_workplace_staffing mit eigener
Freigabe, Session/Entity/Sollwertvergleich und regulären +/-Methoden. Nur fertige
Arbeitsplätze, Ziel 0..64 und höchstens MaxWorkers; keine direkte Biberzuweisung.
Teiländerungen als unconfirmed, niemals automatisch wiederholen/zurückrollen.
191 reguläre Tests bestanden (178 Unit, 13 Integration), drei Live-Tests übersprungen.
Mod 0.10.0 gebaut/gepackt ohne Warnungen/Fehler. Installiert/live bleibt 0.9.0;
Update benötigt beendetes Spiel. [Vertrag und Live-Abnahme](workplace-staffing.md).

## 2026-09-20 — Workforce und Betriebsdiagnose live bestanden

0.9.0 mit neun angebotenen Lesewerkzeugen; begrenzter Pilot mit zehn fachlichen
MCP-Leseaufrufen. Spiel bereits pausiert (Speed 0), unverändert belassen.
Roster: 10 Worker vom Typ Beaver, 3 employed, 7 unemployed, 0 unresolved.
Zwei Zuordnungen zum Distriktzentrum und eine zur Holzfällerflagge stimmen exakt
mit inspect_building überein. Kolonieaggregation ebenfalls 10 einsatzfähig,
7 unbeschäftigt, 3 besetzte Sollstellen. Roster am Ende unverändert nachgelesen.

Distriktzentrum aktiv, Soll/Ist/Max 2/2/4; Holzfällerflagge aktiv, 1/1/1.
Beide melden laufenden Arbeitsauftrag. Das ist auch bei pausierter Simulation
möglich: JobRunning beschreibt Jobzustand, keinen aktuell fortschreitenden Tick.
Lodge noch unfertig, ohne Workplace-Komponente. Path ebenfalls ohne Workplace,
PausableBuilding vorhanden, aber canPause=false. Komponentenpräsenz daher nicht
mit tatsächlicher Pausefähigkeit gleichsetzen. Wohnraum weiter 0 Betten/13 obdachlos.

Arbeitskräfteliste und Betriebsdiagnose im begrenzten Szenario live bestätigt.
Keine Personal-, Pause-, Bau- oder Geschwindigkeitsänderung. Nächste mögliche
Erweiterung: reguläre Sollbesetzung kontrolliert verändern und separat nachlesen;
noch nicht implementiert oder live geprüft.

## 2026-09-20 — Betriebsdiagnose und Workforce-Liste installiert

Neuestes 0.9.0-Paket mit inspect_workforce bei beendetem Spiel installiert.
Bisherigen Mod-Ordner vollständig lokal gesichert, fünf Dateien per SHA256 geprüft.
Private Konfiguration bytegenau erhalten; keine neuen Aktionsfreigaben nötig.
Live-Abnahme wartet auf Nutzerstart und Laden der MCP-Testkolonie. Danach neun
Lesewerkzeuge und Arbeitsplatzzuordnungen gegen Gebäude-Besetzung prüfen.

## 2026-09-20 — Kolonieweite Arbeitskräfteliste in 0.9.0 ergänzt

Auf ausdrücklichen Nutzerwunsch inspect_workforce(offset, limit): Worker-ID/-Typ,
Employed, JobRunning und aufgelöster Arbeitsplatz mit Entity-ID, Template und
Position. Maximal 32 pro Seite, stabile ID-Sortierung; Gesamtzahl, employed und
unemployed für Entities mit Worker-Komponente. Zuordnung, Beschäftigung und
laufender Auftrag getrennt, unresolved nicht als arbeitslos interpretiert.
Keine Personaleinstellung und keine Abhängigkeit von Fremdmods.

179 reguläre Tests bestanden (166 Unit, 13 Integration), drei Live-Tests übersprungen.
Mod-Build ohne Warnungen/Fehler; neues 0.9.0-Paket erzeugt, altes Paket erhalten.
Installiert bleibt 0.8.0. Live-Abgleich der Zuordnungen mit Gebäude-Besetzung steht
aus. [Vertrag und Grenzen](workforce-roster.md). Für Installation neuestes Paket
mit Arbeitskräfteliste verwenden, nicht älteres 0.9.0-Paket nur mit Gebäudediagnose.

## 2026-09-20 — Native Betriebsdiagnose vorbereitet (0.9.0)

Nächster freigegebener MCP-Ausbau: inspect_building um operations erweitert.
Pausefähigkeit/-status und bei fertigen Arbeitsgebäuden Soll-/Ist-/Maximalbesetzung,
Unter-/Überbesetzung sowie laufender Arbeitsauftrag. Fehlende Komponente und
unfertiger Arbeitsplatz bleiben explizit nicht verfügbar, keine erfundenen Nullwerte.
Keine neuen Aktionen, Produktionszusagen oder Fremdmod-Abhängigkeiten.

Öffentliche PausableBuilding-/Workplace-Signaturen lokal geprüft. Community-
WorkplaceSettings bleibt good reference; dessen SetDesiredWorkers existiert in der
aktuellen öffentlichen Workplace-Signatur nicht. Personalsteuerung deshalb noch
separat zu planen. 168 reguläre Tests bestanden (155 Unit, 13 Integration), drei
Live-Tests übersprungen. Installation und begrenzter Lesepilot offen; installiert
bleibt 0.8.0. [Vertrag und Abnahme](building-operations.md).

## 2026-09-20 — Pause/Weiterlauf über MCP live bestanden

Bridge 0.8.0: nach Laden zunächst Geschwindigkeit 0. Erster Lauf nach zwei
Leseaufrufen ohne Änderung beendet. Angepasster Pilot mit zehn MCP-Aufrufen und
drei eindeutigen Befehlen: 0 -> 1 als Ausgangszustand, 1 -> 0, schließlich 0 -> 1.
Jeder Wechsel separat nachgelesen; kein Retry und keine Spielsperre aufgehoben.

Tag 2: erste Weiterlaufkontrolle DayProgress 0.2421875 -> 0.24609375.
Während Pause zwei Beobachtungen im Abstand von 2,5 Sekunden identisch:
DayProgress 0.24609375 / HoursPassedToday 5.90625. MCP antwortet auch in Pause.
Nach Rückkehr zu 1x DayProgress 0.24739583 -> 0.25260416 und Stunden
5.9375 -> 6.0625. Schlusszustand 1 mit nachgewiesenem Zeitfortschritt.

Alle unmittelbaren Änderungsquittungen meldeten matchedImmediately=false und noch
den alten Wert; nachfolgende Leseaufrufe bestätigten die Änderung. Das ist die
belegte verzögerte Wirkung von ChangeSpeed, kein Anlass für Wiederholungsbefehle.
Pause/Normalgeschwindigkeit live bestätigt; höhere Geschwindigkeiten bleiben offen.

## 2026-09-20 — Zeitsteuerung 0.8.0 installiert

Bei beendetem Timberborn bisherigen Mod-Ordner vollständig lokal gesichert und
fünf Paketdateien ersetzt; SHA256 jeweils abgeglichen. Bestehende Konfiguration
erhalten, enableSpeedControl aktiviert. Keine fremden Mods geändert. Neustart
und Laden der MCP-Testkolonie durch Nutzer erforderlich; Pause/1× noch nicht live
geprüft. Nächster Test gemäß [Zeitsteuerungsvertrag](simulation-control.md).

## 2026-09-20 — Pause/Normalgeschwindigkeit vorbereitet (0.8.0)

Auf Nutzerauftrag öffentliche Zeit-API geprüft und inspect_simulation sowie separat
freigeschaltetes set_simulation_speed ergänzt. Erster Umfang Pause (0)/Normal (1),
Session und expectedSpeed vor Änderung auf dem Spielthread geprüft. Keine entsperrten
Spielsperren, Zeitsprünge oder Fremdmod-Abhängigkeiten. Höhere Standardstufen nicht
belegt und deshalb noch nicht freigegeben. Rücklesen nach jedem Eingriff erforderlich.
158 reguläre Tests bestanden (145 Unit, 13 Integration), drei Live-Tests übersprungen.
Separater Mod-Build/Paket ohne Warnungen oder Fehler. Installation/Live-Abnahme offen,
Timberborn läuft noch mit 0.7.0. [Vertrag und Abnahme](simulation-control.md).

Vier reine MCP-Leseaufrufe zur gespeicherten Lodge: aktiv/unfertig/ungestartet,
12 Log Kosten, Inventar leer, Baudistrikt bekannt. Global jetzt 2 Log, weiter
0 Betten/13 Obdachlose. Nutzer meldet Simulation auf einfacher Geschwindigkeit;
noch keine eigene Geschwindigkeitsabfrage in installierter 0.7.0 verfügbar.

## 2026-09-20 — Eigener Lodge-Auftrag live bestätigt

Bridge 0.7.0 über echtes MCP-stdio geprüft. Erster begrenzter Suchlauf mit sechs
Leseaufrufen fand nur eine blockierte Fläche; keine Mutation. Zweiter Lauf mit
neun fachlichen Aufrufen: eine weitere blockierte und eine freie gedrehte Fläche
geprüft, genau ein place_lodge-Auftrag. Ergebnis applied; gleiche Entity-ID separat
als Lodge.Folktails bei (29,25,3), Rotation Cw90, nachgelesen. Eingang (28,24,3)
liegt am vorhandenen Weg; bekannter Baudistrikt bestätigt.

Normale aktive Baustelle: unfertig, noch ungestartet, beide Fortschritte 0,
Vorlagenkosten 12 Log, Baustelleninventar vorhanden/leer, global Log=0.
Damit eigener Hausauftrag und unabhängige Wiedererkennung live bestanden.
Materiallieferung, Fertigstellung und zusätzliche Betten dieser Lodge noch offen.
Keine Ressourcen hinzugefügt, kein Retry, keine Sofortfertigstellung. Die Sitzung
bleibt für weitere Platzierungen gesperrt. Nächster sinnvoller Nachweis: reguläre
Lieferung/Fertigstellung und Wohnraumwirkung gezielt lesend beobachten.

## 2026-09-20 — Bridge 0.7.0 installiert

Spielprozess vor Installation beendet. Bisherigen Mod-Ordner vollständig lokal
gesichert; fünf Paketdateien ersetzt und per SHA256 abgeglichen. Token und Port
erhalten, enableLodgePlacement gezielt aktiviert. Keine fremden Mods verändert.
Live-Nachweis wartet auf Nutzerstart und Laden der MCP-Testkolonie.

## 2026-09-20 — Lodge-Auftrag vorbereitet (0.7.0)

Separat geschütztes place_lodge ergänzt, gemeinsame Versuchssperre mit Path.
Vollständiger Footprint, frische Spielvalidierung und normale Platzierung mit
Entity-ID-Korrelation. Keine Fremdabhängigkeiten. Installation/Live-Abnahme offen;
Spiel läuft noch mit 0.6.1. Vertrag und Abnahme: [Lodge-Pilot](lodge-placement.md).

## 2026-09-20 — Materiallieferung und Baufortschritt live beobachtet

Nach Nutzerbestätigung der regulären Holzversorgung zwei begrenzte Lesestichproben,
insgesamt acht fachliche MCP-Aufrufe. Farmhaus weiterhin aktiv/unfertig und bekanntem
Baudistrikt zugeordnet. Erste Probe: Baustellenbestand 2 Log, MaterialProgress 0.08,
WasStarted=true, Bauzeitfortschritt/-stunden 0. Zweite Probe: Baustellenbestand 4 Log,
MaterialProgress 0.16, BuildTimeProgress 0.109375, BuildTimeProgressInHours 0.21875.
Vorlagenkosten unverändert 25 Log. Damit Lieferung und echter zeitlicher Baufortschritt
live nachgewiesen; noch keine Fertigstellung und kein eigener Gebäudeauftrag.

Zweite Probe meldet gleichzeitig global Log=0 bei 4 Log im Baustelleninventar.
Globales ResourceCounting ersetzt daher die separate Baustellenbeobachtung nicht;
keine Gesamtinventaridentität oder exakte Reservierungs-/Liefersemantik daraus ableiten.
HasMaterialsToResumeBuilding blieb in beiden Proben false: bedeutet nicht, dass seit
der letzten Beobachtung keine Arbeit erfolgt ist. Keine Spielmutation durch den Agenten.
Holzfäller-/Fällmarkierungssteuerung bleibt eine native Funktionslücke; Nutzerhilfe
hat diesen Lieferpilot ermöglicht. Nach zwei nützlichen Proben kein weiteres Polling.

## 2026-09-20 — Materialvertrag 0.6.1 am Farmhaus live bestätigt

Vier rein lesende MCP-Aufrufe nach Nutzer-Neustart. Genau ein gespeichertes
EfficientFarmHouse.Folktails: Vorlagenkosten 25 Log, Baustelleninventar verfügbar
und leer, global Log=0. Auftrag aktiv/unfertig/ungestartet, Material-/Bauzeitfortschritt
0, keine Materialien zur Fortsetzung, bekannter Baudistrikt. Neue getrennte Kosten-
und Inventarausgabe damit für diesen Fall live bestätigt; keine Mutation.
Keine Erklärung des alten RemainingRequiredGoods-Nullwerts und kein zeitlicher
Liefer-/Baufortschrittsnachweis. Nächste Abnahme benötigt reguläre Holzversorgung
und erneute begrenzte Beobachtungen derselben Baustelle.

## 2026-09-20 — Materialkorrektur 0.6.1 installiert

Nutzer bestätigt Speichern des Farmhaus-Spielstands und Beenden des Spiels.
Bei geprüft beendetem Timberborn Paket 0.6.1 installiert, fünf Dateien SHA256-geprüft,
vorherige Installation samt privater Konfiguration in neuem lokalen Backup gesichert.
Private Konfiguration bytegleich erhalten. Kein Spielstart oder API-Aufruf.
Nächster Schritt nach Laden des gespeicherten Farmhaus-Stands: höchstens fünf reine
Leseaufrufe für Version, Identifikation, Vorlagenkosten, Baustellenbestand und globale
Vorräte. Materialkorrektur weiterhin ohne Live-Abnahme.

## 2026-09-20 — Materialvertrag 0.6.1 korrigiert, Live-Abnahme offen

Nach Nutzer-Go lokale Referenzen, öffentliche Signaturen und gezielte Websuche geprüft;
keine belastbare Beschreibung von RemainingRequiredGoods gefunden. Nullwert-Ursache
bleibt ungeklärt. Aufruf und missverständliches remainingRequiredGoods-Feld entfernt.
Stattdessen gesamte BuildingSpec.BuildingCost und tatsächliche ConstructionSite.Inventory.Stock
getrennt lesen. Inventar fehlt: available=false/stock=null; vorhanden und leer: leere Liste.
Kein berechneter Restbedarf, da verbrauchtes Material und laufende Lieferungen unbekannt.
Keine privaten Felder, kein Reverse Engineering, keine neue Abhängigkeit.

Neue Baustellenantworten benötigen Materialvertrag 0.6.1; alte 0.6.0-Baustellen werden
als inkompatibel abgewiesen statt die alte Nullmenge weiter als Bedarf auszugeben.
136 reguläre Tests erfolgreich (125 Unit, 11 Integration); drei Live-Tests übersprungen.
Mod 0.6.1 gebaut/gepackt, noch nicht installiert; laufendes Spiel unverändert 0.6.0.
Nächster Schritt: Update bei beendetem Spiel, danach höchstens fünf reine Leseaufrufe
am Farmhaus zum Nachweis tatsächlicher Kosten und Baustellenbestände. Kein Bauauftrag.

## 2026-09-20 — Nutzer-Farmhaus als echte Baustelle gelesen

Nutzer hat Farmhaus-Bauauftrag platziert. Vier rein lesende MCP-Aufrufe auf 0.6.0:
genau eine EfficientFarmHouse.Folktails gefunden, unfinished=true, finished=false,
isOn=true, wasStarted=false, readyToBuild=false, Material-/Bauzeitfortschritt und
Bauzeitstunden 0. HasMaterialsToResumeBuilding=false, readyToFinish=false.
ConstructionDistrict referenziert bekanntes District Center; Betriebs-/Instant-Distrikt
null. Global Log=0, Water=250, Berries=300. Keine Mutation oder eigener Hausauftrag.

Wichtiger Semantikbefund: RemainingRequiredGoods liefert Log mit Amount=0 trotz
Materialfortschritt 0 und fehlender Fortsetzungsmaterialien. Die Bedeutung/Verwendung
dieser Methode ist damit nicht als verlässlicher Restbedarf abgenommen. Nicht als
"kein Material nötig" interpretieren. Nächste Untersuchung: Baukosten, Baustelleninventar
und Methodenvoraussetzungen getrennt prüfen. Echte Baustellendaten/Zuordnung jetzt
live lesbar; Materialmengen-Semantik und tatsächlicher Fortschrittsverlauf bleiben offen.

## 2026-09-20 — Gebäude-/Distrikt-Lesepilot auf 0.6.0 bestanden

Neun fachliche MCP-Aufrufe, Schreib-/Vorschau-Opt-ins im Client aus. District Center
und Holzfällerflagge: fertig, ConstructionSite-Komponente vorhanden, keine aktiven
Baustellendetails; Betriebs-/Instant-Distrikt entspricht gelistetem District Center,
Baudistrikt null. Fertiger Path: keine DistrictBuilding-Komponente, alle Zuordnungen
null; daraus keine Trennung vom Wegenetz ableiten. Entity-/Vorlagen-/Positionsabgleich
mit Gebäudeliste erfolgreich. Unbekannte Entity liefert found=false; fremde Sitzung
abgewiesen (derzeit generischer backend_unavailable-Fehler). Objektzahl und drei
Ressourcenbeobachtungen vor/nach identisch.

Die im früheren Wegbau-Receipt enthaltene Entity-ID fehlt im aktuell geladenen Stand.
Stattdessen bestehenden Path geprüft; keine Wiederholung des Bauauftrags. Ursache bzw.
Persistenz des früheren Testwegs nicht nachgewiesen, keine automatische Speicherung.
Keine unfertigen Strukturen in vollständiger Gebäudeseite: Material-/Baufortschritt
weiterhin nur synthetisch geprüft. Nächster Schritt: begrenzten Hausauftrag vorbereiten
und anschließend Baustelle, verbleibende Materialien und Baudistrikt gezielt nachweisen.

## 2026-09-20 — 0.6.0 installiert, lesende Live-Abnahme ausstehend

Auf Nutzer-Go bei beendetem Timberborn das finale 0.6.0-Paket installiert.
Fünf Paketdateien SHA256-geprüft; vorherige Dateien samt privater Konfiguration in
neuem lokalen Backup gesichert. Private Konfiguration bytegleich erhalten.
Noch kein Spielstart oder API-Aufruf. Nach Laden der Testkolonie ausschließlich
lesenden Pilot gemäß docs/building-observations.md ausführen, ohne Placement-Opt-in
im MCP-Prozess. Letzter bestätigter Laufzeitstand bleibt bis dahin 0.5.0.

## 2026-09-20 — Baustellen-/Distriktbeobachtung 0.6.0 vorbereitet

Nach Nutzer-Go als nächsten kleinen Schritt die Ergebnisbeobachtung vor Hausbau
erweitert: inspect_building(id, session), sieben native Lesewerkzeuge. Feste GET-Route,
Session-Prüfung im Spielhauptthread, nur initialisierte Gebäude/Paths ohne Previews.
Fertigstatus, verbleibende Güter/Baufortschritt für unfertige ConstructionSites und
getrennte DistrictBuilding-Zuordnungen. Keine Gleichsetzung mit Arbeiter-/Liefergarantie.
Keine neue Abhängigkeit und keine Änderung am Wegbau-Pilot oder laufenden Spiel.

Öffentliche Signaturen gegen lokale Spielbibliotheken geprüft. 132 reguläre Tests
bestanden (121 Unit, 11 Integration), drei Live-Tests übersprungen; Mod-Build erfolgreich.
0.6.0 noch nicht installiert/live geprüft; laufendes Spiel bleibt 0.5.0 mit Testweg.
Nächste Abnahme rein lesend gemäß docs/building-observations.md. Materialfortschritt
benötigt später eine echte Baustelle; fehlt sie, bleibt dieser Teil ausdrücklich offen.

## 2026-09-20 — Erster regulärer Wegbau über natives MCP bestätigt

0.5.0 live erreichbar. Neun fachliche MCP-Aufrufe: Zustand/Katalog/Karte/Wege gelesen,
freien trockenen Einzelfeldstandort neben bestehendem fertigem Path vorgeprüft und
genau einen place_path-Aufruf ausgeführt. Ergebnis applied; folgende unabhängige
Gebäudeabfrage bestätigt exakt die zurückgegebene Entity-ID, Path und Zielposition,
finished=true. Genau ein neues Struktur-Objekt, Gesamtobjektzahl +1, drei beobachtete
Ressourcenwerte unverändert. Kein zweiter Auftrag, keine Löschung oder explizite Speicherung.
Testweg bleibt bestehen; Einmaligkeitsgate dieser Sitzung verbraucht. Private Versuchsdaten
und Receipt nur im ignorierten lokalen Pilotordner, keine Entity-/Session-ID eingecheckt.
Damit regulären Platzierer und SetId-Ergebniszuordnung für diesen Path live nachgewiesen.
Keine Aussage zur Distriktanbindung oder zu Häusern mit Material-/Bauzeitbedarf.

## 2026-09-20 — 0.5.0 für Wegbau-Pilot installiert

Nach Nutzerbestätigung bei beendetem Timberborn das finale vorbereitete 0.5.0-Paket
installiert. Fünf Dateien SHA256-geprüft, vorherige Installation samt privater Konfiguration
in neuem lokalen Backup gesichert. Schlüssel und Port erhalten; enablePlacement für den
beauftragten Einzelweg-Pilot aktiviert. Zusätzliches MCP-Opt-in weiterhin erforderlich;
Standardstarter bleibt lesend. Kein Spielstart und kein Bauaufruf ausgeführt.
Nächster Schritt nach Laden von MCP: Version/Sitzung und Standort frisch prüfen,
genau einen Wegauftrag ausführen und das Ergebnis lesend anhand der Entity-ID bestätigen.

## 2026-09-20 — Einzelner regulärer Wegauftrag 0.5.0 vorbereitet

Nach ausdrücklichem Go für kontrollierte Bauaufträge zunächst Path-only umgesetzt.
Keine neue Abhängigkeit. Öffentliche Metadaten belegen TemplateSpec.Blueprint,
EntitySetup.Builder.SetId und IBlockObjectPlacer.Place. Feste POST-Route und separates
Mod-/MCP-Opt-in; Hauptthread prüft Session vor Einmaligkeitsgate und frischer Validierung.
Ein Versuch je Szene, auch bei Ablehnung/Fehler. Platzierung regulär über passenden
Spielplatzierer; Erfolg nur bei exakter Entity-ID/Vorlage/Position/Orientierung,
Fertigstatus separat. Unklare Ergebnisse niemals automatisch wiederholen.

123 reguläre Tests bestanden (112 Unit, 11 Integration), drei Live-Tests im Standardlauf
übersprungen. Integration prüft echte stdio-/HTTP-Kette mit synthetischem Platzierer,
alle vier Opt-in-Kombinationen und Zweitversuchsperre. Mod-Build ohne Warnungen/Fehler,
neues lokales 0.5.0-Paket erstellt. Nicht installiert, kein Bauauftrag im Spiel ausgeführt.
Live bleibt 0.4.1. Nächste Nutzerhilfe: Spiel beenden für Update; danach genau ein
Wegauftrag und lesende Ergebniszuordnung gemäß docs/path-placement.md.

## 2026-09-20 — 0.4.1 Vorschau-Kontrollpilot bestanden

Version 0.4.1 über MCP bestätigt. Neun fachliche MCP-Aufrufe in einer Sitzung,
darunter genau zwei Vorschauvalidierungen: Lodge auf belegtem District-Center-Standort
korrekt valid=false; vollständig vorgeprüfter einfeldriger Path-Standort valid=true.
Beide internen Entity-/Bestandswachen unverändert, keine Sitzungssperre. Abschließende
Kolonieabfrage: Objektzahl und drei Ressourcenbeobachtungen gegenüber Beginn identisch.
Keine Platzierung oder Produktionssteuerung. Der Nachweis gilt für diese zwei Fälle;
allgemeine Bebaubarkeit, Distriktanbindung und Fertigstellung sind dadurch nicht bewiesen.
Nächste technische Lücke: regulären Bauauftrag mit stabiler Ergebniszuordnung ermöglichen
und anschließend Auftrag, Materialversorgung und fertiges Bauwerk getrennt beobachten.

## 2026-09-20 — Korrekturkandidat 0.4.1 installiert

Auf Nutzer-Go bei beendetem Timberborn das vorbereitete Paket installiert. Fünf
Paketdateien per SHA256 geprüft, bisherige Dateien samt privater Konfiguration in
neuem lokalen Backup gesichert. Private Konfiguration bytegleich erhalten, einschließlich
Schlüssel, Port und bestehendem Vorschau-Opt-in. Kein Spielstart oder Live-Aufruf.
Nächster Schritt nach Nutzer-Neustart: Version prüfen und belegten Standort erneut als
Negativkontrolle verwenden. Bei erneutem Widerspruch sofort stoppen; keine Platzierung.

## 2026-09-20 — Validator-Negativkontrolle auf 0.4.0 fehlgeschlagen

Version 0.4.0 über MCP bestätigt. Kleinen Kartenbereich, vollständige Gebäudeseite und
Bauplatzvorprüfungen gelesen; freier einfeldriger Path-Kandidat vor dem Schreibpilot
identifiziert. Erster Vorschauversuch: Lodge auf District Center fälschlich valid=true.
Sofortiger Abbruch, zweiter Versuch unterblieb. Entity-/Bestandswache meldete keine
Änderung. Anschließend separater nativer Lesetest über alle sechs Werkzeuge bestanden.
Keine Platzierung; keine weiteren Vorschauaufrufe in dieser Sitzung.

Referenzmuster und öffentliche Metadaten belegen BlockObject.IsValid(). Korrekturkandidat
0.4.1 verlangt diese Prüfung zusätzlich zum einzelnen Service-IsValid-Aufruf.
Hypothese: bisheriger Service-Aufruf deckt die direkte Objektprüfung nicht vollständig ab;
Ursache noch nicht bestätigt. Installation/erneuter begrenzter Kontrolltest ausstehend.
109 reguläre Tests bestanden, drei Live-Tests im Standardlauf übersprungen; Mod-Build
ohne Warnungen/Fehler und neues unveränderliches 0.4.1-Paket erstellt. Diese Prüfungen
belegen die Live-Semantik der direkten Objektprüfung noch nicht.

## 2026-09-20 — Bridge 0.4.0 installiert, Neustart ausstehend

Auf Nutzerauftrag bei beendetem Spiel das vorbereitete 0.4.0-Paket installiert.
Fünf Paketdateien mit SHA256 gegen die Quelle geprüft; vorherige Dateien und private
Konfiguration in einem neuen ignorierten lokalen Backup gesichert. Bestehenden Port
und Schlüssel erhalten, enableValidation für den angekündigten Pilot aktiviert.
MCP-Opt-in bleibt separat erforderlich; Standardstarter aktiviert es nicht.
Noch kein Laufzeitnachweis für 0.4.0. Nächster Schritt nach Nutzer-Spielstart und Laden
der Testkolonie: Version/Lesefunktionen prüfen, dann maximal zwei Vorschauvalidierungen
nach docs/native-validation.md. Keine Bauaufträge.

## 2026-09-20 — Native Bridge ohne aktive Fremdmods live geprüft

Nach Nutzerbestätigung des Spielstarts nur mit MCP-Mod den vorhandenen nativen
Lesetest gezielt wiederholt: ein Test bestanden, alle sechs MCP-Werkzeuge erfolgreich
in einer Sitzung. Schreib-/Vorschau-Opt-ins deaktiviert; keine Spielmutation.
Keine erneute vollständige Testsuite, da seit dem geprüften Checkpoint kein Code geändert.
Der unabhängige lesende Betrieb ist für die Testkolonie nachgewiesen; Mod-Auswahl beruht
auf Nutzerbestätigung. Installierter Stand bleibt 0.3.0. Nächster Schritt: vorbereiteten
0.4.0-Validator installieren und begrenzt live prüfen; DLL-Austausch erst bei beendetem Spiel.

## 2026-09-20 — Lokalen nativen Startweg konsolidiert

Nutzer beauftragt schrittweise eine lokal nutzbare Mod mit MCP und möglichst wenigen
Fremdmod-Abhängigkeiten. Bestehende Architektur beibehalten; keine neue Abhängigkeit.
Eigener stdio-Starter wählt native explizit und deaktiviert Vorschau-/Schreibaktionen.
verify.ps1 unterstützt nun -NativeConfig: reguläre Prüfungen und anschließend genau
einen begrenzten nativen Lesetest. Auch der Live-Test selbst unterbindet geerbtes
Vorschau-Opt-in. 109 reguläre Tests und ein nativer Live-Test über sechs Werkzeuge
bestanden; Starter beendet sich bei stdin-EOF mit Exit 0 ohne stdout-Rauschen.

Installiert bleibt 0.3.0, Quellstand 0.4.0; keine Mod-Dateien oder Client-Einstellungen
ersetzt. Aktuelle CLI findet keine Registrierung namens timberborn; die historische
Einrichtungsnotiz ist kein Nachweis der heutigen Client-Verfügbarkeit.
Nächster Nachweis: Spiel mit ausschließlich eigener Bridge starten, Testkolonie laden,
nativen Lesetest wiederholen. Dafür ist Nutzerhilfe beim Mod-Menü/Neustart nötig.
Bislang waren Fremdmods parallel aktiv; fehlende RequiredMods und erfolgreiche native
Abfragen belegen noch keine isolierte Laufzeit. Danach geschützten 0.4-Validator prüfen,
bevor reguläre Bauaufträge folgen. Kein autonomer Kolonieaufbau erfolgt.

## 2026-09-19 — Paket A

Nutzer hat Schritt 5 und damit die Umsetzung des Missionsplans freigegeben.
Sechs getrennte src-Projekte, zwei Testprojekte, projektlokale NuGet-/CLI-Caches und
gepinnten SDK-/Paketstand angelegt. Fake-Backend und fünf Read-only-Tool-Verträge umgesetzt.
Release-Build ohne Warnungen; drei Anwendungstests und ein echter MCP-stdio-Prozesstest bestanden.
Der echte Adapter folgt in Paket B. Noch keine MCP-Client-Konfiguration verändert.

Lesender API-Pilot erfolgreich: localhost:8080, Mod 11.0.0, Spiel 1.1.2.4.
`ping`, `misc`, `live-data`, `characters`, `buildings` und Einzelgebäude antworten.
Mapping-Besonderheiten: Mod-Aktivierung heißt `Active`, Bevölkerungslisten heißen
`Adult`/`Child`/`Bot`, Tagesfortschritt liegt in `TopBar.Cycle.Hours`.
Keine Live-Antworten, persönlichen Namen oder Spiel-IDs als Fixtures gespeichert.

## 2026-09-19 — Pakete B/C und technischer Live-Test

More HTTP API 11.0.0 angebunden. Zulässige Routen fest begrenzt; Loopback-Verbindung erhält den
Hostnamen localhost, deaktiviert Proxy/Redirects und begrenzt dekomprimierte Antwortkörper.
Einzelgebäude werden gegen die Gebäudeliste plausibilisiert. Fremdmod-Einstellungen und lokale
Verzeichnisse werden nicht weitergereicht. Wetterprognosen respektieren ShouldShowNext.

Alle fünf MCP-Werkzeuge wurden in einem echten stdio-Prozess gegen die vorbereitete Spielkolonie
erfolgreich aufgerufen. Simulationsmarker war false. Nutzer bestätigt den manuellen UI-Vergleich.
Die Spiel- und MCP-Client-Konfiguration wurde nicht geändert. Git-Identität auf Nutzerwunsch
pro Commit: Codex <codex@openai.com>; keine globale oder lokale Git-Konfiguration gesetzt.

Abschlussprüfung: Locked-Mode-Restore erfolgreich, Release-Build mit 0 Warnungen/0 Fehlern,
42 Anwendung-/Adaptertests und 5 deterministische Integrationstests erfolgreich.
Der Live-Test wird im normalen Lauf korrekt übersprungen und wurde separat mit explizitem Opt-in ausgeführt.
Nutzerabnahme erlaubt den Meilenstein `poc-readonly-v0.1` nach finaler Sicherung.

## 2026-09-19 — GitHub-Veröffentlichung vorbereitet

Nutzer beauftragt Veröffentlichung des Projekts einschließlich Agentenanweisungen und fortlaufende
Pushes geprüfter Checkpoints nach chr0mat0mcodex/timberborn-mcp. AGENTS.md bündelt projektspezifische
Grenzen und Einstiegspunkte; DEVELOPMENT_WORKFLOW.md dokumentiert Prüfung, Commit und Remote-Abgleich.
README und aktuelle Freigabegrenzen verweisen darauf. Keine persönlichen globalen Agentendateien übernommen.
Bestehende Commit-Dateilisten und verdächtige Textmuster vor Veröffentlichung geprüft; keine Zugangsdaten
oder persönlichen Benutzerpfade erkannt. Spielstände, Fremdbinärdateien und lokale Laufzeitdaten bleiben ausgeschlossen.

## 2026-09-19 — Lokaler Codex-Client eingerichtet

Auf ausdrücklichen Folgeauftrag `codex mcp add timberborn` ausgeführt: stdio mit absoluten lokalen
dotnet-/DLL-Pfaden, explizitem More-HTTP-API-Backend und localhost:8080. Der Eintrag ist aktiviert;
`codex mcp get timberborn --json` bestätigt die Konfiguration. Maschinenlokale Konfiguration bleibt außerhalb Git.
Vorhandenen LiveSmokeTests erneut separat ausgeführt: ein Test bestanden, alle fünf lesenden MCP-Werkzeuge
gegen die laufende Kolonie erfolgreich. Das prüft den Server über stdio; die dynamische Werkzeugaufnahme
im bereits laufenden Codex-Chat wurde nicht nachgewiesen.

## 2026-09-19 — Ersten Schreib-POC geplant

Auf Nutzer-Go Quellenprüfung und Missionsplan Abschnitt 6 ergänzt: genau ein Gebäude pausieren und
seinen ursprünglichen Pausenstatus wiederherstellen. Aktueller Herstellerquellcode nennt Mod 11.0.0
und eine Route mit explizitem booleschem Zielzustand. Geplant sind getrennte Schreibfähigkeit,
standardmäßig deaktiviertes Opt-in, frische Vor-/Nachprüfung und keine blinden Wiederholungen bei unklarem Ausgang.
Kein Schreibcode implementiert, keine Spielaktion ausgeführt und Client-Schreibfähigkeit nicht aktiviert.
Nächste Freigabe betrifft Implementierung E/F; konkreter Live-Pilot G wird danach abgestimmt.

## 2026-09-19 — Schreib-POC E/F implementiert

Nutzer hat die Umsetzung und anschließend den Test an seiner einzigen Holzfällerflagge freigegeben.
Name ist nicht änderbar; typbasierte eindeutige Auswahl vorgesehen und lesend bestätigt.
MCP-Werkzeug set_building_paused hinter explizitem Prozess-Opt-in implementiert. Vorprüfung,
Serialisierung und Nachprüfung; unklare Schreibausgänge werden nicht wiederholt oder automatisch rückgängig gemacht.
74 reguläre Tests bestanden; Build ohne Warnungen/Fehler. Beide Live-Tests im Standardlauf übersprungen.
Hersteller-HTTP-Helper für Mod 11.0.0 bestätigt HTTP 204. Lokale Codex-Konfiguration bleibt zunächst lesend.

## 2026-09-19 — Technischer Schreibpilot bestanden

LivePauseTests mit explizitem Opt-in nach erneutem Go erfolgreich: einzige Holzfällerflagge anhand
des Templates identifiziert, false -> true -> false jeweils über MCP bestätigt, abschließend erneut gelesen.
Ein Live-Test bestanden; keine automatische Wiederholung und kein anderer Gebäudetyp angesprochen.
Ursprünglicher aktiver Pausenstatus wiederhergestellt. UI-Abgleich angefragt, noch nicht bestätigt.
Kein dauerhaftes Schreib-Opt-in im Codex-Client gesetzt. Code-Checkpoint vor dem Pilot: fafa4d9.

## 2026-09-19 — Sichtbare Nutzerabnahme bestanden

Nutzer bestätigte den aktiven Zustand nach dem Hin-/Rücktest. Auf gesonderten Auftrag anschließend
die einzige Holzfällerflagge über MCP pausiert und Paused=true separat nachgelesen; bewusst nicht zurückgesetzt.
Nutzer bestätigt den sichtbaren Erfolg im Spiel. Begrenzter Schreib-POC damit abgenommen.
Letzter bestätigter Zustand absichtlich pausiert; Wiederaktivierung benötigt einen entsprechenden Auftrag.

## 2026-09-19 — Phase 2 geplant

Nutzer wählt Aufbau der Grundversorgung und ergänzt Wohnraum als Pflichtumfang. docs/phase-2-plan.md
trennt benötigte Beobachtungen/Aktionen von bereits verfügbaren, quellbelegten und unbelegten Fähigkeiten.
Herstellerhandler und ModdableTimberborn-Dokumentation geprüft: Bestands-/Bettenstatistiken sind als
Mod-interne Ansätze dokumentiert; räumliche Daten, Bauprüfung und reguläre Bauaufträge noch nicht nachgewiesen.
Empfohlen sind begrenzter Schnittstellenpilot 2A, Lagebild 2B, einzelner Wohnbau mit Weg 2C,
Versorgungsketten 2D und begrenzter Agentenlauf 2E. Nur Planung, kein Spielzugriff oder neuer Code.

## 2026-09-19 — Phase-2A-Schnittstellenpilot

Freigegebenen Pilot mit sechs lesenden HTTP-Abfragen ausgeführt. Acht Wohnbauvorlagen gefunden;
Lodge.Folktails liefert Kosten, Kapazität und Geometrie. Gebäude enthalten auch Wege, aber keine Positionen.
Workplace.Value ist nach Herstellerimplementierung Sollbesetzung, keine Ist-Besetzung.
GameStatService bietet dokumentierte Mod-interne Güter-/Betten-/Arbeitskräftestatistiken; HTTP-Brücke fehlt.
Areas unterstützt Charaktere; Gebäude-Tracking ist laut Dokumentation unvollständig, kein Ersatz für eine Karte.
Am Raum-/Bauprüfungsblocker begrenzt gestoppt. docs/phase-2a-results.md enthält Abdeckung, Grenzen und
konkreten nächsten Freigabevorschlag. Keine neuen Mutationen, Rohdaten gespeichert oder Mods installiert.

## 2026-09-19 — Eigene Spielschnittstelle als Ziel

Nach Nutzer-Go offizielle Modding-Beispiele und lokale DLL-Metadaten geprüft. Direkte öffentliche Zugänge
für Güter, Betten, Personal, Positionen, Terrain/Wasser und Bauvalidator gefunden. Öffentliche Interfaces
vermeiden den Zugriff auf interne TerrainService-/WaterService-Implementierungen. Keine Methodenkörper
dekompiliert, kein Spielcode ausgeführt. Temporärer Metadatenprüfer ausschließlich unter .local.
docs/architecture/native-game-api.md ersetzt die zuvor bevorzugte ModdableTimberborn-Pflichtbasis;
bestehender MCP-Server/Tests bleiben, Community-Mods dienen zunächst als Referenz/Vergleich.
Nebenwirkungsfreie Vorschauprüfung und reguläre Platzierung noch nicht zur Laufzeit belegt.

## 2026-09-20 — Native Diagnosebrücke und gesicherte Referenzen

Eigene Mod mit Game-Kontext und öffentlichen Spielservices implementiert; netstandard2.1,
keine Fremdmod- oder NuGet-Abhängigkeit. Explizites natives MCP-Backend mit drei Lesewerkzeugen.
Loopback/Bearer-Transport, feste Routen, begrenzte Hauptthread-Queue, Session-ID und Unload-Abbruch.
Mod-Build erfolgreich; 89 reguläre Tests bestanden, zwei Live-Tests übersprungen. Stdio-Integration
prüft unseren echten Transport mit synthetischen Beobachtungen, keine Behauptung eines Spiel-Livetests.
Fehlerfalltests fanden einen fehlenden InvalidDataException-Catch; korrigiert und erneut geprüft.
Benutzer installiert die Mod manuell; kein Spiel-/Client-Setup geändert, keine Mutation ausgeführt.

Nutzer bestätigt Endziel aktives Spielen und wünscht Unabhängigkeit von Fremdmods bei erhaltener
Referenzbasis. 17 benötigte Dateien aus offizieller Mechanistry-Werkzeugbasis und datvm-Quellen
an feste Commits gebunden lokal abgerufen (40.818 Bytes), Lizenzen mitgesichert, SHA256-Inventar erstellt.
Extrahierte API-Erkenntnisse und good-reference-Katalog bleiben im Projekt; fremde Quellen und
Binärdateien werden nicht mitgebaut oder veröffentlicht. Spielplattform und externes MCP-SDK bleiben.
Nächster Nachweis: Live-Lagebild, danach Vorschauvalidierung und regulärer Wohnbau/Wege.

## 2026-09-20 — Lokale Installation auf ausdrücklichen Auftrag

Nutzer hat das Kopieren diesmal ausdrücklich delegiert. Agent Bridge 0.2.0 in den
Standard-Modordner installiert, sechs Dateien einschließlich privater Konfiguration
per SHA256 mit dem gebauten Paket abgeglichen. Kein vorhandener Modordner überschrieben.
Spielstart und Aktivierung übernimmt der Nutzer; Live-Abnahme weiterhin offen.

## 2026-09-20 — Erster Startversuch

Spiel lädt die Agent Bridge, aber ihr Listener startet nicht; Port 8081 verweigert
die Verbindung. Startdiagnose um feste Phasenkennung und Exception-Typ ergänzt,
ohne Fehlermeldungsinhalt, Pfade oder Schlüssel auszugeben. Mod-Build erfolgreich.
Live-Abnahme weiterhin offen; erneuter Spielstart für die Diagnose erforderlich.

## 2026-09-20 — Konfigurationspfad beim Spielstart korrigiert

Diagnose meldet ArgumentException in locate_configuration, vor Listener-Erstellung.
Assembly.Location ist für vom Spiel geladene Mod-Assemblies kein zuverlässiger Dateipfad.
Konfiguration jetzt über öffentlichen ModRepository, eindeutige aktivierte Manifest-ID und
ModDirectory.Path auflösen. Öffentliche Signaturen lokal geprüft; keine zusätzliche Mod nötig.
Mod-Build ohne Warnungen/Fehler, 89 reguläre Tests bestanden. Korrigierte DLL mit Sicherung
der vorherigen Version installiert und per SHA256 geprüft; privater Schlüssel unverändert.
Ursprüngliches Paket bleibt historischer Snapshot und enthält diesen Fix nicht; neue Pakete
aus aktuellem Code bauen. Nächster Spielneustart muss den Fix zur Laufzeit bestätigen.

## 2026-09-20 — Native Bridge live erreichbar und MCP-Lesetest bestanden

Nach Spielneustart Listener erfolgreich gestartet. Bevölkerung 9 Erwachsene/4 Kinder/0 Bots,
0 Betten/13 Obdachlose und verfügbare Bestände 250 Wasser/300 Beeren/0 Holz vom Nutzer im UI bestätigt.
Echter stdio-Livetest aller drei nativen MCP-Werkzeuge erfolgreich, Sitzung über die Aufrufe konsistent,
eine Kartenzelle an beobachteter Objektposition gelesen. Kein Rückgriff auf More HTTP API.
89 reguläre Tests bestanden; drei Live-Tests im Standardlauf übersprungen, anschließend genau ein
nativer Lesetest separat bestanden. verify.ps1 deaktiviert dessen Opt-in im normalen Prüflauf.
Keine Mutation ausgeführt. Fremdmods waren im Spiel weiterhin aktiv; Isolation ohne diese noch nicht
live getestet. Koordinaten-/Geländesemantik und Session-Wechsel nach Menü/Reload noch offen.
Die dauerhafte Client-Konfiguration wurde nicht umgestellt; Livetest nutzt eigenen nativen Prozess.

## 2026-09-20 — Wiederverbindung nach Menü und Neuladen bestätigt

Nutzer hat Hauptmenü und erneutes Laden der Testkolonie durchgeführt. Spiel-Log bestätigt
Menü-/Spiel-Ladevorgang und erneuten Listener-Start. Snapshot erreichbar, anschließend alle
drei nativen MCP-Werkzeuge im separaten Lesetest erneut erfolgreich (ein Test bestanden).
Kein Spielneustart und keine Mutation erforderlich. Vorherige Session-ID wurde nicht erhalten;
ein tatsächlicher ID-Wechsel ist deshalb nicht direkt verglichen. Aktuelle ID nur lokal unter
.local als Vergleichsbasis gespeichert, keine IDs oder Rohlogs im Repository.
Wiederverbindung abgenommen; räumlicher UI-Abgleich und Bauvalidierung bleiben offen.

## 2026-09-20 — Strukturierte Raumabfragen und reine Bauplatzvorprüfung 0.3.0

Nutzer beauftragt autonomes Weiterarbeiten bis echte Hilfe nötig ist und präzisiert:
keine Screenshots; Spielzustand per API, kontrollierte programmierte Interaktionen.
Räumlichen UI-Abgleich im aktuellen Ablauf durch API-Konsistenzprüfungen ersetzt.
Abhängigkeiten möglichst vermeiden, bei sinnvollem Mehrwert aber vor Aufnahme abwägen und fragen.

Neue native Leserouten/Werkzeuge für Gebäude/Wege, zwei Pilotvorlagen und Bauplatzvorprüfung.
Öffentliche TemplateSpec-/Positions-/Block-/Fraktions-/Freischaltungsservices genutzt;
keine privaten Felder, neue Fremdmods, Preview-Erzeugung, Spawn-/Delete- oder Bauaufrufe.
BuildingBlueprints-Referenz verwendet teils Probeplatzierung; bewusst nicht übernommen.
Vorprüfung liefert nur blocked oder requires_game_validation, gameValidated stets false.
Volle Spielvalidierung, Stützregeln für stapelbare Objekte und Distriktanbindung bleiben offen.

99 reguläre Tests erfolgreich, drei Live-Tests im Standardlauf übersprungen. Mod-Build
gegen Spiel-DLLs ohne Fehler/Warnungen. Synthetischer stdio-/HTTP-Test umfasst alle sechs
Werkzeuge, zusätzlich Parametergrenzen und Abweisung irreführender Baufreigaben geprüft.
Vier Quellen ergänzt: 21 Referenzdateien/61.934 Bytes lokal gesichert, nicht ins Paket übernommen.
0.3.0 als neues Paket gebaut und fünf Dateien mit Backup in delegierter Installation ersetzt,
SHA256 geprüft; vorhandener privater Schlüssel unverändert. Aktuelle Session-ID nur lokal
als Vergleichsbasis gesichert. Neue DLLs noch nicht im laufenden Spiel geladen.
Nächste notwendige Nutzerhilfe: vollständiger Spielneustart und MCP laden; anschließend
begrenzter strukturierter Live-Pilot gemäß docs/spatial-precheck.md ohne weitere Umfangsrückfrage.

## 2026-09-20 — Räumlicher 0.3.0-Pilot live bestätigt

Version 0.3.0 aktiv, Sitzungswechsel gegenüber lokaler Vergleichs-ID direkt bestätigt.
Ein lesender HTTP-Verbindungscheck und zehn fachliche MCP-Abfragen ausgeführt, innerhalb
des Zwölf-Abfragen-Budgets. Alle sechs Werkzeuge genutzt; 21 Gebäude/Wege, 64 Kartenzellen,
aktive Fraktion und zwei freigeschaltete Vorlagen gelesen. Lodge benötigt 12 Holz bei 0 Bestand.
Vier Bauplatzvorprüfungen ergeben erklärbare Objekt-/Terrain-/Kartenrandhindernisse; keine
freie vollständige Grundfläche nachgewiesen. Registrierte Objektzahl und Bestandsbeobachtung
vor/nach identisch. Keine Bauaufträge, Screenshots oder Eingabesimulation ausgeführt.

## 2026-09-20 — Validator-Prototyp vorbereitet, Mission auf Analyse fokussiert

Öffentliche PreviewFactory-/Validator-/Placer-/EntitySetup-Signaturen gezielt geprüft.
0.4.0 mit eigener temporärer Vorschau und Spielvalidator gebaut; gesondertes Mod-/MCP-Opt-in,
POST-only, Session-Prüfung, maximal acht Versuche und Sperre bei Fehler/Zustandsabweichung.
Keine Platzierungsroute. 109 reguläre Tests bestanden, drei Live-Tests übersprungen.
Neues lokales Paket erstellt; ausdrücklich noch nicht installiert oder im Spiel getestet.

Nutzer stellt klar: Vorrang hat die Analyse dessen, was der Agent zum Spielen benötigt;
reales Spielen ist sekundär. Auf ausdrücklichen Auftrag in AGENTS.md und missionsplan.md
gespeichert und die Einstiege in README/Phase-2-Plan angepasst. Die bevorstehende Installation
von 0.4.0 unterbleibt. Spiel nutzt weiterhin 0.3.0, privater Schlüssel und Client-Konfiguration
unverändert. Nächster Schwerpunkt: Fähigkeiten, benötigte Daten/Aktionen und Evidenzlücken
systematisch priorisieren; praktische Tests nur mit konkretem Erkenntniszweck.

## 2026-09-20 — 0.12.0 Prioritäten, Baustellen, Flächen vorbereitet

224 reguläre Tests bestanden (211 Unit-, 13 Integrationstests); drei Live-Tests
im Standardlauf übersprungen. Release-Build und separater Mod-Build ohne Warnungen
oder Fehler; neues eindeutiges lokales Installationspaket erstellt. Reales stdio/
authentifiziertes HTTP mit synthetischer Bridge prüft neue Lesewerkzeuge und beide
Schreibfreigaben inklusive Prioritäts- und Flächen-Hin-/Rückweg. Parameterzahlfehler
für set_area im Test gefunden und korrigiert; vollständiger Wiederholungslauf bestanden.

Neu: vier Lesewerkzeuge, Arbeitsplatz-/Baupriorität, Baustellenübersicht, Flächenkatalog
und Markierungen. Nur öffentliche Spiel-APIs, keine neue Fremdmod-Abhängigkeit.
Zapfmarkierung nicht belegt und ausdrücklich unsupported. Details und Live-Abnahmeplan:
[Prioritäten, Baustellen und Flächen](priorities-construction-areas.md).

Spiel läuft noch mit 0.11.0. Kein Live-Prioritätstest behauptet: bisher wurde nur
Sollbesetzung getestet. Nutzer um Speichern/Beenden für sicheren Modwechsel gebeten.
Installation und Live-Abnahme von 0.12.0 stehen aus.

## 2026-09-20 — 0.13.0 Kiefernschutz und Entfernung vorbereitet

255 reguläre Tests bestanden (242 Unit-, 13 Integrationstests); drei Live-Tests
übersprungen. Lösung und Spielmod ohne Warnungen/Fehler gebaut, separates lokales
Installationspaket erzeugt. 14 native Lesewerkzeuge plus vier separat gesperrte
Entfernungswerkzeuge. stdio/HTTP-Integration prüft Kategorien und Freigaben synthetisch;
Kiefernschutz-Antwort muss entfernte Fällmarkierung bestätigen.

Nutzerdefinition tapping umgesetzt: Fällmarkierungen im gewählten Bereich entfernen;
Abfrage liefert abgeleitete unmarkierte Kiefern, keine eigene persistierte Zone.
Gebäude/Wege über regulären Entity-Lebenszyklus, Schutt über RecoveredGoodStack.Delete;
Vegetation über Biber-Entfernungsaufträge. Frische Zielprüfung und CanDelete-Sperren,
kein Force-Delete. Historisch selbst gepflanzt öffentlich nicht belegbar; stattdessen
explizit passende aktuelle Pflanzmarkierung, Herkunft unknown. Ruinen nicht als Schutt.

Bauplatz-Hindernisprüfung existiert bereits; neue regionale Objektabfrage liefert
zusätzlich Identitäten/Kategorien. Keine vollständige Baufreigabe daraus ableiten.
[Vertrag](removal-and-pine-protection.md). Timberborn weiterhin geöffnet; installierte
0.11.0 unverändert. Keine realen Objekte entfernt. Live-Abnahme einschließlich
Arbeitsplatz-/Baustellenpriorität bleibt nach Installation erforderlich.

## 2026-09-20 — 0.13.0 gesichert installiert

Timberborn-Prozess beendet geprüft. Vollständige lokale Sicherung von 0.11.0 erstellt;
fünf Paketdateien von 0.13.0 installiert und per SHA256 verglichen. Schlüssel, Port
und bisherige Felder erhalten. enablePriorities/enableAreas aktiv, enableRemoval aus.
Keine Spielobjekte geändert. Nach Start und Laden von MCP zunächst Lesetest und
Prioritäts-Hin-/Rücktest; Flächen und Entfernung noch nicht live abgenommen.

## 2026-09-20 — 0.13.0 Leserouten und Prioritäten live bestätigt

Opt-in-Livetest aller 14 Lesewerkzeuge erfolgreich. Anschließend separater Pilot
mit 18 MCP-Aufrufen: Arbeitsplatzpriorität Normal -> High -> Normal und Priorität
an einer von zwei offenen Baustellen Low -> High -> Low. Jede Mutation mit eigenem
Readback bestätigt; beide Ausgangszustände wiederhergestellt. Simulation 0 -> 0,
keine Zeitsteuerung, Flächenänderung oder Entfernung ausgeführt.

crops und tree_planting liefern jeweils 0 markierte Zellen; tapping liefert 124
aktuell nicht zum Fällen markierte Kiefern. Kein Ernte-/Reichweitennachweis. Der neue
DI-/Pflanzkatalogpfad läuft im Spiel. Flächenmutationen und Abriss bleiben ungeprüft.
Rohdaten/Session- und Entity-IDs nur im ignorierten lokalen Pilotordner gespeichert.

## 2026-09-20 — Korrektur zur Interpretation der Kiefernabfrage

Nutzer weist auf zum Fällen markierte Kiefern hin. Rein lesende Diagnose: komplette
Listen mit 124 tapping-Kandidaten und 191 Fällmarkierungszellen verglichen. Keine
Überschneidung, auch nicht bei XY ohne Höhenvergleich. Zwei gezielte Regionen gelesen:
in der ersten Stichprobe 32 Kiefern, alle 32 fällmarkiert und keine tapping-Kandidaten;
in der zweiten 32 Kiefern, davon 15 fällmarkiert und 17 unmarkiert. Regionenseiten
waren begrenzt, keine Aussage über vollständige regionale Baumzahlen.

Damit kein Koordinatenfehler in diesen Stichproben belegt. Die frühere Formulierung
„geschützte Kiefern“ war missverständlich: tapping listet kartenweit unmarkierte
Kiefern als Kandidaten, nicht eingerichtete Zapfflächen. Es wurde bisher keine
Fällmarkierung entfernt. Nutzeraussage über die markierten Kiefern ist bestätigt.
Keine Spieländerungen; Flächenmutation und Zapferreichweite bleiben offen.

## 2026-09-20 — Flächensteuerung live bestanden

Kiefernschutz-/Fällpilot: eine bestehende Fällmarkierung über tapping entfernt und
separat über inspect_areas bestätigt; Markierungszahl 191 -> 190. Über tree_cutting
wiederhergestellt und separat 191 bestätigt. 10 MCP-Aufrufe inklusive Pflanzkatalog
und kleiner Geländeabfrage zur Vorbereitung des nächsten Piloten.

Pflanzpilot: freie Zelle über precheck_build_site geprüft, Carrot und danach Pine
jeweils markiert/abgefragt/entfernt/abgefragt; 0 -> 1 -> 0 für beide Arten bestätigt.
13 MCP-Aufrufe, Simulation unverändert pausiert. Kein Nachweis physischer Pflanzung.

Nutzer erlaubt ausdrücklich notwendige Bau-/Abrissversuche im Entwicklungsspielstand.
Private Konfiguration einmalig gesichert und enableRemoval=true gesetzt, alle anderen
Felder erhalten. Laufende Bridge liest Konfiguration erst beim Kontextladen: Nutzer
um Hauptmenü und erneutes Laden von MCP gebeten. Abriss noch nicht ausgeführt.

## 2026-09-20 — Abrisspiloten und unmittelbar abgeschlossene Entfernung

Neue Spielsitzung und Abrissfreigabe bestätigt. Wegabriss/Abwesenheit/Bauplatzprüfung/
regulärer Neubau erfolgreich. Vegetationsauftrag mark/unmark erfolgreich (Pilot 15 Aufrufe).
Pine auf aktueller Pflanzmarkierung als planted klassifiziert, Auftrag mark/unmark geprüft,
Markierung entfernt und vegetation-Kategorie wiederhergestellt (11 Aufrufe).

Farmhausbaustelle mit 14 Holz im Baustellenbestand entfernt. Sechs neue Schuttstapel
im Bereich beobachtet, einer regulär entfernt und separat als abwesend bestätigt
(9 Aufrufe). Farmhaus bleibt entfernt, fünf Stapel zunächst vorhanden; Güter im
entfernten Stapel verloren. Bestehende Nutzerfreigabe für Entwicklungsspielstand genutzt.

Bei weiterer kolonienaher Kiefer zunächst Fällmarkierung entfernt, dann Demolishable.Mark.
Spiel entfernte das Objekt unmittelbar in Pause. Receipt removed=true, outcome=unconfirmed;
separate Diagnose bestätigte Entity abwesend, Zelle leer und Geschwindigkeit 0. Kein
Retry und keine Zeitsteuerung ausgeführt. 0.13.0 bewertete nur wartende Markierungen
als Erfolg: 0.13.1 korrigiert Mod und Clientvertrag für regulär sofort erledigte mark-
Aktionen. Sechs Regressionstestfälle; keine Änderung am eigentlichen Spieleingriff.
Installation/Live-Antwortprüfung des Patches offen. Nutzer um Speichern/Beenden gebeten.

Patchprüfung 0.13.1: 261 reguläre Tests bestanden (248 Unit-, 13 Integrationstests),
drei Live-Tests im Standardlauf übersprungen. Lösung ohne Warnungen/Fehler gebaut.

## 2026-09-20 — Patch 0.13.1 installiert

Bei beendetem Spiel vollständige Sicherung von 0.13.0 erstellt und fünf Paketdateien
installiert. Alle SHA256-Vergleiche erfolgreich; private Konfiguration byteidentisch.
Vorhandene Abriss-/Flächen-/Prioritätsfreigaben erhalten. Nächster Schritt nach Start:
Version/Sitzung lesen und korrigierte Sofortentfernungs-Antwort live nachweisen.

## 2026-09-20 — 0.13.1 vollständig für den Antwortfehler live abgenommen

Neue Sitzung/Version bestätigt. Erster Pilot: wartender Vegetationsauftrag meldet
applied/removed=false, separat gelesen und zurückgenommen (8 MCP-Aufrufe).
Weitere Stichprobe auf drei Kandidaten begrenzt; bereits beim zweiten Kandidaten
reguläre Sofortentfernung: applied/removed=true, separate Abfrage bestätigt Abwesenheit.
Erster wartender Auftrag zurückgenommen, Stichprobe beendet (10 MCP-Aufrufe).

Abschließender Ausführungspilot an bekannter wartender Kiefer: Fällmarkierung entfernt,
regulärer Abrissauftrag zunächst pending, dann Simulation auf 7×. Nach zwei Abfragen
(etwa 6 Sekunden reale Laufzeit) Objekt nicht mehr vorhanden. Anschließend Pause
wiederhergestellt und separat gelesen (16 MCP-Aufrufe). Kein direkter Worker-Trace;
Ausführung des wartenden Auftrags aus Zustand und Abwesenheit abgeleitet, Holzfällung
am Ziel ausgeschlossen. Kein Timeout und kein Retry. Zwei weitere Kiefern entfernt.

Patch 0.13.1 beantwortet beide Fälle korrekt. Alle Testaufträge erledigt oder
zurückgenommen; Simulation zuletzt 0. Lokale Rohdaten bleiben ausschließlich unter .local.

## 2026-09-20 — Nutzerhinweis: Lebenszustand bislang nicht berücksichtigt

Codeprüfung bestätigt: Pine-Vorlage und fehlende Fällmarkierung allein wurden als
tapping-Kandidat gewertet; Objektabfrage ohne Lebenszustand. 0.13.2 ergänzt öffentliche
LivingNaturalResource.IsDead, DyingNaturalResource.IsDying, WateredNaturalResource-
DyingProgress und Growable-Werte. Tote/sterbende/junge/unbekannte Kandidaten ausgeschlossen,
Lebenszustand in Objektlisten sichtbar. Keine Änderung der regulären Abrissaktion.

261 vorherige Tests und Live-Abnahmen belegten Transport/Aktionen, nicht Baumgesundheit.
Die ungefilterten 124 Kiefern sind kein Nachweis gesunder Harzbäume; der Zustand bereits
entfernter Kiefern lässt sich nicht rekonstruieren. Vertrag: vegetation-state.md.
Installation und rein lesender Live-Nachweis der neuen Zustandsfelder stehen aus.

Prüfstand 0.13.2: 279 reguläre Tests bestanden (266 Unit-, 13 Integrationstests), drei
Live-Tests übersprungen. Lösung und separater Mod-Build ohne Fehler/Warnungen.

0.13.2 nach Nutzerfreigabe bei beendetem Spiel installiert. Vorversion vollständig
lokal gesichert, fünf Paketdateien per SHA256 bestätigt, private Konfiguration
byteidentisch erhalten. Explizite camelCase-Payload für den Newtonsoft-Spielserializer
verwendet. Nächster Schritt nach Start: Lebenszustände und korrigierte Kandidaten
rein lesend abgleichen, kein erneuter Abriss notwendig.

## 2026-09-20 — 0.13.2 Lebenszustände live bestätigt

Acht reine MCP-Aufrufe, vier begrenzte Objektregionen. 101 eindeutige Kiefern gelesen:
61 alive, 40 dead; 35 isGrown=false. Die Wachstumskategorie überlappt Lebenszustände,
nicht addieren. Kein waterStress=true in der Stichprobe; positive Wasserstress-
Darstellung damit noch nicht an einem betroffenen Objekt bestätigt.

Vollständige tapping-Liste: 19 Kandidaten. Alle nachweislich alive, nicht dying,
waterStress=false und ausgewachsen; kein toter Stichprobenbaum enthalten. Das ersetzt
die frühere ungefilterte Kandidateninterpretation, ist aber wegen zwischenzeitlicher
Spieländerungen kein exakter Vorher-/Nachher-Zähler derselben Baumgesamtheit.
Simulation unverändert pausiert. Keine Markierungen, Aufträge oder Objekte verändert.

### 2026-09-20 — generischer Bauzugang 0.14.0 vorbereitet

Nutzer hat generischen Gebäudebau und anschließendes praktisches Spielen freigegeben.
Neue Werkzeuge inspect_build_options, precheck_building, validate_building und
place_building. Vorlagen aus TemplateService; bestehende öffentliche Vorschau- und
Platzierer-Services wiederverwendet. Keine Fremdmod-Abhängigkeit. Sonderlayouts
explizit nicht unterstützt. Neue separate Freigabe enableBuildingPlacement /
TIMBERBORN_ENABLE_BUILDING_PLACEMENT; Paketstandard aus, 16 Leser standardmäßig.

Aktions-ID ist Entity-ID; Einmal-Ausführung je ID, auch nach Fehler. 256 generische
Auftragsversuche/Prüfungen und 64 Vorschauvorlagen begrenzen Sitzungszustand.
Legacy-Piloten bleiben getrennt. Gesundheitsprüfung bei Versionswechsel erhalten.
316 reguläre Tests erfolgreich (303 Unit/13 Integration), drei Live-Tests übersprungen.
Mod gegen öffentliche lokale 1.1.2.4-Referenzen kompiliert; letzte Paketierung und
Live-Abnahme stehen aus. Laufendes Spiel noch 0.13.2. Details: docs/generic-building.md.

0.14.0 abschließend gebaut und als eindeutiges lokales Paket erstellt. Manifest-ID,
Version und Code-Archiv mit genau fünf Dateien geprüft; keine private Konfiguration
im Archiv. Timberborn läuft noch; Nutzer um Speichern/Beenden gebeten. Installation
und anschließende Live-Abnahme bleiben offen. Quellcode-Checkpoint: 4f26990.

0.14.0 bei nachweislich beendetem Spiel gesichert installiert. Fünf Paketdateien per
SHA256 identisch geprüft. Alle bisherigen Konfigurationsfelder unverändert; allein
enableBuildingPlacement=true ergänzt. Vollständige lokale Sicherung von 0.13.2
vorhanden. Nutzer um Neustart/MCP laden gebeten; Live-Abnahme noch offen.

### 2026-09-20 — Zukunftsfeature Alerts mit Ortsdetails geprüft

Auf Nutzerwunsch nur Kurzrecherche und Planung: öffentliche StatusSubject-/StatusInstance-
und Aggregator-APIs liefern Warnungen und Zielbezug; SelectNextSubject belegt den
Auswahlpfad. Separate Notifications/QuickNotifications haben unterschiedliche Orts-
und Historiengrenzen. API-Signaturen geprüft, keine Live-Abdeckung behauptet.
In missionsplan.md 7.45 und Phase-2-Zukunftsliste aufgenommen; Details docs/alerts-plan.md.
Keine Mod-/Spieländerung. Nächster Entwicklungsschritt bleibt Bau-Liveabnahme 0.14.0.

### 2026-09-20 — generischer Bau 0.14.0 live geprüft

MCP-Katalog vollständig gelesen: 162 Einträge, 119 unterstützt. Terrain-/Hindernis-
vorprüfung erkannte zunächst ungeeignete Standorte; zwei gezielte Kartenregionen
lieferten vier freie Plätze westlich des Weges. Spielvalidator bestätigte alle vier,
belegte Lodge wurde abgelehnt. Vier reguläre Aufträge (Farm, Lodge, mittleres Lager,
Pumpe) in derselben Sitzung mit unabhängigen ID-/Vorlagen-/Baustellenrücklesungen.
Belegter Standort auch im Platzierer rejected. Alle Änderungen regulär, keine Cheats.

Zwei 30-Sekunden-Simulationsfenster auf 7x; anschließend wieder Pause bestätigt.
Farm: Materialfortschritt 0 -> 76 %, Baustellenbestand 19 Holz; Bauzeit rund 70 %.
Vorhandene Pumpe separat als fertig und 1/1 besetzt mit laufendem Job gelesen.
Neue Pumpe noch ohne Baudistrikt, zwei Verbindungswege fehlen. Wohnraum bleibt 3 Betten,
10 Obdachlose; die neu beauftragte Lodge ist noch nicht fertig. Vier neue Aufträge
bleiben erhalten. Kein vollständiger Produktions- oder Versorgungsnachweis.

Live gefundener Filterfehler: UI-Layouts SideLine/TwoSegmentLine schlossen kleine
Lager, Flaggen und einzelne Wege aus. 0.14.1 erweitert genau diese Layoutklassen für
feste Einzelgeometrie; sämtliche anderen Bauprüfungen bleiben bestehen. Line/Half,
Terrainseiten, Entwicklerwerkzeuge, Hexform und übergroße Geometrie weiterhin abgewiesen.

Patchprüfung: 318 reguläre Tests bestanden (305 Unit, 13 Integration); drei separate
Live-Tests im Standardlauf übersprungen. 0.14.1 noch nicht installiert/live getestet.

0.14.1 abschließend gebaut (0 Warnungen/Fehler) und eindeutig paketiert; Manifest
und Code-Archiv mit genau fünf Dateien ohne private Konfiguration geprüft. Nutzer
um Speichern des Entwicklungsspielstands und Beenden für die Installation gebeten.
Spiel bleibt auf Pause. GitHub-Codecheckpoint be1b982 verifiziert synchronisiert.

## 2026-09-20 — Gebäudeeinstellungen 0.15.0

Nutzerauftrag Lageroptionen, Anbaufläche und Gebäudepause umgesetzt. Neue native
Settings-Abfrage plus fünf Schreibwerkzeuge: Pause, Lagergut, Lagermodus,
Pflanzen-/Erntenpriorität und bevorzugte Feldfrucht. Bestehende Flächenwerkzeuge
bleiben zuständig; Markierungen sind keiner bestimmten Farm fest zugewiesen.
Öffentliche Spielmethoden aus lokalen Referenzen; keine neue Abhängigkeit oder
private Reflection. Crop-Priorität löschen mangels belegter Rücksetzsemantik offen.
350 reguläre Tests bestanden (337 Unit, 13 Integration), drei Live-Tests übersprungen;
Mod-Build null Warnungen/Fehler. Neue Spielwirkung ausdrücklich noch nicht live belegt.
0.14.1-Layoutkorrektur in 0.15.0 enthalten. Vertrag: building-settings.md.

0.15.0 bei beendetem Spiel gesichert installiert. Paketarchiv enthält genau fünf
Code-/Dokumentdateien, keinen Schlüssel. Alle fünf installierten Datei-Hashes stimmen;
vorhandene private Konfiguration erhalten und nur enableBuildingSettings aktiviert.
Neustart/Laden des Entwicklungsspielstands vom Nutzer angefordert; Live-Abnahme offen.

## Live-Abnahme 0.15.0 am 2026-09-20

- Neue Settings-Abfrage an neun Gebäuden: fertige Gebäude und Baustellen korrekt unterschieden.
- Fertige Wasserpumpe pausiert und fortgesetzt; beide Zustände separat nachgelesen, abschließend aktiv.
- Farm regulär fertiggestellt. Pflanzen zuerst -> Ernten zuerst -> Pflanzen zuerst bestätigt.
- Bevorzugte Feldfrucht keine -> Kartoffel -> Karotte bestätigt; abschließend Karotte.
- Vier Karotten-Markierungen angelegt, separat gezählt (0 -> 4), entfernt und erneut gelesen (4 -> 0).
- Kleines Lager mit drei Holz regulär gebaut. Lagerauswahl keine -> Beeren -> keine -> Karotten bestätigt.
- Lagermodi Annehmen -> Beschaffen -> Liefern -> Leeren -> Annehmen bestätigt, jeweils separat gelesen.
- Absichtlich veralteter erwarteter Lagerwert abgewiesen; nachfolgende Abfrage weiterhin Karotte/Annehmen.

Zwei begrenzte 30-Sekunden-Fenster auf 7x dienten regulärem Baufortschritt; abschließend
Simulation pausiert, Tag 4, etwa 15:49 Uhr. Kleines Lager leer, Kapazität 30; keine
Umlagerung/Ernte als bewiesen behaupten. Farm und kleines Lager fertig, zwei neue
Wege fertig; zusätzliche Pumpe besitzt jetzt einen Baudistrikt, bleibt aber Baustelle.
Test-Anbaufläche wieder entfernt. Weitere Wohn-/Lagerbaustellen bleiben bestehen.

Bekannte Diagnosegrenze: Die serverseitige Ablehnung eines veralteten Zustands wird
noch als backend_unavailable statt spezifischem Zustandskonflikt gemeldet. retryable=false;
keine Wiederholung ausgeführt, tatsächliche unveränderte Auswahl separat bestätigt.
Künftige Verbesserung: fachliche Konflikte von Transportfehlern unterscheiden.

## 2026-09-20 — Karottenkette vollständig live nachgewiesen (0.15.0)

Ausgangslage: Farm fertig/aktiv mit drei von drei Arbeitern, kleines Lager auf
Karotte/Annehmen mit leerem Bestand. Vier freie Felder regulär als Karotte markiert.
Sieben begrenzte 40-Sekunden-Fenster auf regulärer Stufe 7, jeweils abschließend
Pause. Nach dem ersten Pilot seltener abgefragt, da Wachstum stetig und gesund.

- Vier echte Pflanzenobjekte nach Markierung beobachtet; alive, kein Wasserstress.
- Wachstum über mehrere Beobachtungen von etwa 1 % bis 99 % verfolgt.
- Anschließend 12 Karotten im kleinen Lager (Kapazität 30) beobachtet und nach Pause
  über eine separate MCP-Sitzung erneut bestätigt. Kein Bestand durch Bridge erzeugt.
- Vier neue Pflanzen mit anderen IDs und etwa 3–5 % Wachstum auf denselben Feldern:
  reguläre Ernte und Nachpflanzung zusammen mit neuem Lagerbestand nachgewiesen.
- Schlusszustand: Tag 8, 22:45, Simulation pausiert; Farm aktiv mit drei Arbeitern,
  vier Karotten-Markierungen bleiben für weiteren Betrieb bestehen.

Nebenläufig wurde die vorherige Lodge fertig: sechs Betten, sieben Obdachlose.
Letzte Beispielbestände: Wasser 222, Beeren 270, Holz 20. Keine Aussage über gesamte
Nahrung oder dauerhaft ausreichende Versorgung aus diesen drei Beispielen ableiten.
Dieser Pilot bestätigt einen Erntezyklus samt Lagerung und Nachpflanzung, keine
langfristige Versorgung der ganzen Kolonie. Keine Mod-/Produktcodeänderung nötig.
Nächste offene Versorgungsnachweise: tatsächliche Wassergewinnung/-lagerung und
vollständiger Wohnraum; außerdem Ressourcenübersicht über alle Güter statt drei Beispiele.

## 2026-09-20 — Wassergewinnung und Tanklagerung live (0.15.0)

Beide vorhandenen Pumpen fertig, aktiv und mit je einem Arbeiter besetzt gelesen.
SmallTank regulär mit 15 Holz am bestehenden Weg errichtet, nach Fertigstellung
Water/accept gewählt. Öffentliche Spielvalidierung und anschließende Gebäudeabfrage
bestätigen Platzierung, Fertigstellung und Distriktanbindung.

Zwei begrenzte 40-Sekunden-Fenster auf Stufe 7: zunächst Bau, danach Lagerung.
Tankbestand 0 -> 30 Wasser, Kapazität 30; in separatem MCP-Leselauf bestätigt.
Im ersten Betriebsintervall global AllStock 217 -> 219, StockpiledStock 0 -> 30.
Der positive Gesamtzuwachs belegt Wassernachschub neben der bloßen Umlagerung,
nicht die genaue Förderleistung oder den Anteil einer einzelnen Pumpe.
Über das gesamte Betriebsfenster sank AllStock anschließend auf 209. Keine dauerhaft
positive Versorgungsbilanz behaupten; Produktions-/Verbrauchsraten bleiben offen.

Schlusszustand: Tank voll (Water/accept), Spiel pausiert, Tag 10 um etwa 04:07 Uhr.
Beispielbestände Wasser 209, Beeren 255, Holz 20; sechs Betten und sieben Obdachlose.
Keine Güter künstlich erzeugt, keine Mod-/Produktcodeänderung erforderlich.
Nächster getrennt geplanter Schritt: verbleibenden Wohnraumbedarf decken; Wasserbilanz
über längere Zeit samt Produktionsdiagnose und ausreichendem Speicher später prüfen.

## 2026-09-20 — Ingame-MCP-Log 0.16.0 implementiert

Öffentliche UILayout-/UI-Toolkit-Schnittstelle geprüft und nichtmodales Log gebaut.
Native MCP-Aufrufe mit optionalem reasoning, Beginn/Abschluss, 128 Einträgen im RAM,
Filter/Leeren und separater Logabfrage. Keine neue Fremdmod, keine Speicherung und
keine Änderung von Aktionsfreigaben. Fehlerhafte Logübertragung löst kein Retry aus.
Installation und sichtbare Live-Abnahme noch offen. Siehe activity-log.md.

0.16.0: 362 reguläre Tests bestanden (349 Unit, 13 Integration), drei Live-Tests
im Standardlauf übersprungen. Mod-Build null Warnungen/Fehler; Codearchiv exakt fünf
Dateien ohne private Konfiguration. Nach Nutzerbestätigung gesichert installiert,
fünf Datei-Hashes korrekt und private Konfiguration bytegleich erhalten.
Neustart und sichtbare Log-Abnahme angefordert; noch nicht live bestätigt.

## 2026-09-20 — Ingame-Log 0.16.0 live bestätigt

Nutzer hat Schalter rechts unten gefunden und sichtbares Fenster bestätigt.
MCP meldet uiAttached=true und nach Öffnen windowVisible=true. Abfragen samt
Unicode-Begründungen und Abschlussstatus aus dem laufenden Spiel zurückgelesen.
Kontrollierter Pumpentest: Pause false -> true -> false, beide Zustände separat
bestätigt; zwei zugehörige Logeinträge mit reasoning und applied gelesen.
Simulation durchgehend pausiert, Tag 10 etwa 04:07 Uhr; Pumpe abschließend aktiv.
Keine Anzeigeänderung erforderlich, keine neue Modversion. Filter/Leeren und
Szenenwechselverhalten noch nicht manuell abgenommen; automatisierte Logtests bestehen.

## 2026-09-20 — Forschung: öffentliche API und Abnahmeplan geprüft

Nutzer bestätigt zusätzlich Scrollen im MCP-Log und möchte die Anzeige unverändert
lassen. Forschung und Gebäudefreischaltung als nächster Funktionsbereich freigegeben.
Öffentliche Metadaten von Timberborn 1.1.2.4 geprüft: ScienceService.SciencePoints,
BuildingSpec.ScienceCost, BuildingUnlockingService.Unlocked/Unlockable/Unlock vorhanden.
Keine neue Fremdabhängigkeit erforderlich. Kostenabzug noch nicht praktisch belegt.
Vertrag und negative/positive Abnahmefälle: docs/research.md, Missionsplan 7.52.
Noch keine Code-, Installations- oder Spielzustandsänderung in diesem Schritt.

## 2026-09-20 — Forschung 0.17.0 gebaut und installiert

inspect_research ergänzt Punkte, Freischaltkosten und Zustände als 19. nativen Leser.
unlock_building verwendet reguläres Unlock mit eigener Mod-/MCP-Freigabe, Session,
erwarteten Kosten, Punkteprüfung und unmittelbarer Rückabfrage. Bereits entsperrte
Vorlagen werden nicht erneut freigeschaltet. Keine künstlichen Forschungspunkte.
Rejection/Noop, exakter Abzug, fehlgeschlagene Nachbedingungen, fehlende Freigabe,
veraltete Session und Wiederholung automatisch geprüft, auch über echten MCP-stdio.
378 reguläre Tests bestanden (365 Unit, 13 Integration), drei Live-Tests bewusst
übersprungen; separater Spiel-Mod-Build ohne Warnungen/Fehler.
0.17.0 bei beendetem Spiel installiert, vollständige Sicherung, fünf Datei-Hashes
geprüft. Bestehende Konfigurationswerte erhalten, nur enableResearch aktiviert.
Nutzer um Start/Laden gebeten. Echter Kostenabzug und Bauvalidierung vor/nach Unlock
noch offen. Logfenster unverändert. Vertrag: docs/research.md.

## 2026-09-20 — Forschung 0.17.0: erster Livepilot

Über echten MCP-Zugriff bestätigt: Version 0.17.0, 162 Forschungsvorlagen auf sechs
Seiten, null Forschungspunkte. Kostenabweichung ergibt cost_changed, zu wenige Punkte
insufficient_points, bereits freigeschaltete Vorlage already_unlocked. Separate
Forschungsabfrage bestätigt weiterhin null Punkte und gesperrte Bank.

Ein Erfinder wurde regulär für 12 Holz gebaut und in einem begrenzten 40-Sekunden-
Fenster fertiggestellt. Er blieb ohne Distrikt/Arbeiter und erzeugte keine Punkte.
Simulation abschließend pausiert: Tag 10, etwa 18:58 Uhr. Wasser 203, Beeren 250,
Holz 20; sechs Betten/sieben Obdachlose. Keine künstlichen Forschungspunkte.

Bauvorprüfung-Lücke: pathAtEntrance=true genügt nicht als Nachweis eines begehbaren
Weges. Der gewählte Erfindereingang liegt an einer durch die Lodge belegten Zelle;
GetPathObjectAt kann hier nicht als alleinige positive Zugangsaussage dienen.
Zwei benachbarte freie Wegzellen geprüft, aber keinen ungeprüften Verbindungsbau
oder weiteren Produktionslauf begonnen. Erfinder bleibt fertig, aktiv und unbesetzt.
Nächster Pilot benötigt einen freien Erfindereingang und einen nachgewiesenen Anschluss.

Förster-Bauvalidierung vor Unlock liefert einen Fehler, derzeit zu allgemein als
backend_unavailable klassifiziert. Das ist kein vollständiger Nachweis der konkreten
Sperrursache. Fehlermeldungen und Eingangs-/Wegprüfung gezielt verbessern.
Positiver Kostenabzug, Wiederholung nach bezahltem Unlock und Bauvalidierung danach
bleiben offen. 378 automatische Tests des Implementierungsstands unverändert gültig.

## 2026-09-20 — Forschung live bestanden; Eingangskorrektur 0.17.1 vorbereitet

Ersatz-Erfinder mit vier regulären Wegzellen angeschlossen, alten unbenutzbaren
Erfinder entfernt. Fertigstellung, Distriktzuordnung, ein Arbeiter und laufender Job
separat gelesen. In vier begrenzten 40-Sekunden-Fenstern regulär 35 Forschungspunkte
erzeugt; nach jedem Fenster pausiert. Keine künstlichen Punkte.
Förster zunächst mit template_locked in der Vorprüfung gesperrt. Regulärer Unlock
kostet exakt 30 Punkte (35 -> 5), Rückabfrage bestätigt unlocked=true. Wiederholung
ergibt already_unlocked ohne Abzug (5 -> 5). Anschließende Spielvalidierung gültig,
kein Förster-Bauauftrag erteilt. Abschließend pausiert, Tag 13 etwa 06:32 Uhr.

0.17.1 korrigiert pathAtEntrance: GetPathObjectAt bezeichnet eine Belegungsschicht,
keinen sicheren Nachweis einer Wegvorlage. Nur fertige, nicht als Vorschau markierte
Path-Vorlagen zählen jetzt als Weg. entranceOccupants listet die belegenden Vorlagen
(maximal 32 unterschiedliche Namen); ältere Bridge-Antworten dürfen das Feld weglassen.
Belegung ist keine vollständige Begehbarkeitsprüfung; Treppen/Sonderwege und tatsächliche
Distriktverbindung werden nicht als gewöhnlicher Path klassifiziert. Keine neue Abhängigkeit.
378 Tests bestanden; Mod-Build fehlerfrei und separates Paket erstellt. Noch nicht
installiert oder live geprüft; Nutzer um Speichern und Beenden gebeten.
Allgemeine Fehlerklassifikation der Bauvalidierung bleibt ein eigener offener Punkt.

## 2026-09-20 — Eingangskorrektur 0.17.1 installiert

Nach Nutzerbestätigung und geprüftem Spielende vollständig gesichert installiert.
Fünf Paketdateien per SHA-256 verglichen; private Konfiguration bytegleich erhalten.
Keine neuen Freigaben oder Abhängigkeiten. Live-Vergleich von belegtem Eingang und
fertigem Anschlussweg wartet auf Neustart/Laden. Forschungsabnahme von 0.17.0 bleibt gültig.

## 2026-09-20 — Eingangskorrektur 0.17.1 live bestätigt

Sechs ausschließlich lesende MCP-Aufrufe inklusive Status: Bridge 0.17.1 erreichbar.
Alter Erfindereingang: pathAtEntrance=false, entranceOccupants=[Lodge.Folktails].
Neuer Erfindereingang: pathAtEntrance=true, entranceOccupants=[Path].
Die Gesamtvorprüfung am neuen Standort meldet erwartungsgemäß object_intersection,
weil der fertige Erfinder selbst dort steht; das widerspricht dem freien Anschlussweg
am Eingang nicht. Am alten Standort bleibt die Gesamtbewertung requires_game_validation:
Eingangsbelegung wird separat ausgewiesen, nicht als vollständige Begehbarkeitsprüfung.

Erfinder nach Neustart fertig, Distrikt zugeordnet, ein Arbeiter und laufender Job.
Förster weiterhin freigeschaltet, Forschungspunkte weiterhin fünf. Simulation bleibt
pausiert, Tag 13 um 06:30 Uhr. Keine Bau-, Freischalt- oder Geschwindigkeitsänderung.
Installation und gezielte Live-Abnahme von 0.17.1 abgeschlossen. Vollständige
Erreichbarkeit und präzisere Fehlermeldungen bleiben getrennte offene Fähigkeiten.

## 2026-09-20 — Fachliche Fehlercodes 0.17.2 vorbereitet

Zentrale Session-Prüfung, gesperrte/deaktivierte Bauvorlagen und Gebäudeeinstellungen
liefern feste fachliche Codes statt pauschal backend_unavailable. BridgeRejectionException
besitzt eine Positivliste; Queue/HTTP erhalten ausschließlich den Code (HTTP 409).
Client lehnt unbekannte Codes, Zusatzfelder und doppelte Schlüssel ab. Keine privaten
Exception-Texte. MCP retryable=false, Ingame-Log rejected; keine automatische Wiederholung.
HTTP 400 invalid_request wird als invalid_argument eingeordnet. Andere Fachbereiche
sind noch nicht vollständig umgestellt; siehe docs/bridge-errors.md.

388 reguläre Tests bestanden (375 Unit, 13 Integration), drei Live-Tests übersprungen.
Echter MCP-stdio-Pfad für stale_session und Logstatus geprüft; Mod-Build ohne Warnungen
oder Fehler. Separates Paket 0.17.2 erstellt. Nutzer um Speichern/Beenden gebeten;
Installation und gezielte Live-Ablehnungen noch offen. Kein Spielzustand geändert.

0.17.2 nach bestätigtem Spielende gesichert installiert. Fünf Paketdateien per Hash
verglichen; private Konfiguration bytegleich erhalten. Begrenzter MCP-Liveprüfer für
template_locked, stale_session und state_conflict vorbereitet. Nutzer startet das Spiel;
Live-Abnahme wartet auf Bestätigung des geladenen Spielstands.

## 2026-09-20 — Fachliche Fehlercodes 0.17.2 live bestätigt

Elf MCP-Aufrufe im geladenen Entwicklungsspielstand: gesperrte Bank bei Bauvalidierung
als template_locked, absichtlich falsche Session bei Gebäudeabfrage als stale_session,
abweichender erwarteter Pausenwert als state_conflict. Alle drei mit retryable=false.
Separate Rückabfragen bestätigen unveränderten Gebäude-Pausenwert, Forschungspunktestand
und vollständige Simulationsbeobachtung. Ingame-Log enthält alle drei als rejected.
Keine Aktion wiederholt, keine Zustandsänderung durch die Tests. Gezielte Live-Abnahme
von 0.17.2 abgeschlossen; übrige Fehlerbereiche bleiben wie dokumentiert offen.

## 2026-09-20 — Projektmetadaten und Dokumentation konsolidiert

Nutzerauftrag: veraltete More-HTTP-API-/Read-only-Außendarstellung vollständig bereinigen.
README, Installation, Workflow und Beispielkonfiguration auf den nativen Einstieg
umgestellt; PROJECT_STATE, BACKLOG und Dokumentations-/Werkzeugübersicht ergänzt.
NativeTools.Catalog offline abgeglichen: 19 Leser und 19 freizugebende Werkzeuge.
Alte Mission und ursprünglicher Phase-2-Plan separat archiviert; aktueller Missionsplan
enthält Ziele, belegte Fähigkeiten und echte offene Schritte. Historische Berichte bleiben
als solche gekennzeichnet, Fremdmods als good references/Legacy abgegrenzt.
AGENTS.md nach gesondertem Nutzer-Go von den überholten POC-/Installationsverboten bereinigt.
GitHub-About und Topics sowie Name/Beschreibung im Quellmanifest aktualisiert.

Keine C#-Implementierung, Spielsteuerung, Versionsnummer oder laufende Installation geändert.
Der direkte Server-Default bleibt ein dokumentierter Legacy-Restpunkt; native Einrichtung
setzt das Backend ausdrücklich. Mod-Metadaten im bereits installierten Paket bleiben
bis zum nächsten Paketupdate auf ihrem bisherigen Textstand. Keine Release-Veröffentlichung.
Aktuelle funktionale Abnahme bleibt 0.17.2 / 388 Tests; für diesen Metadaten-Schritt
Werkzeugkatalog, lokale Links, JSON, UTF-8 und Diff geprüft, keine unnötigen Spieltests.


## 2026-09-20 — 0.18.0: Güter und aktive Status mit Zielen

Drei neue native Leser: inspect_goods, inspect_alerts, inspect_alert_targets. Vollständige
registrierte Güter inklusive Nullbeständen und separaten öffentlichen ResourceCount-Feldern;
aktive sichtbare Entity-Status mit nativen Flags, eindeutiger Zielzahl und aktuellen
Raster-/Weltpositionen. Opaque Gruppenkennungen sind an die Session gebunden; keine
Behauptung stabiler Problemcodes oder vollständiger UI-Benachrichtigungshistorie.
Kein fremder Modcode, neue Abhängigkeiten, Save-Felder oder Spielschreibaktionen.
[Vertrag und geplanter Live-Pilot](economy-observations.md).

409 reguläre Tests bestanden: 396 Unit + 13 Integration; drei opt-in Live-Tests
übersprungen. Die zusätzlichen stdio-/HTTP-Aufrufe prüfen Leser, stale_session und
Log-Einträge mit Begründung. Mod-Build gegen 1.1.2.4 ohne Warnungen/Fehler. Katalog
offline bestätigt: 22 Leser + 19 Aktions-/Validierungswerkzeuge. Dokumentlinks geprüft.

0.18.0 bei beendetem Timberborn installiert, vorherigen Modordner vollständig gesichert,
fünf Paketdateien per SHA-256 verglichen, private Konfiguration bytegleich erhalten.
Neue Live-Abnahme steht aus; Nutzerstart und geladenen Entwicklungsspielstand abwarten.
Keine Versorgungslage oder neue Statusabdeckung im laufenden Spiel bereits bestätigt.


## 2026-09-20 — 0.18.0 Güter-/Status-Live-Pilot bestanden

Zehn kontrollierte rein lesende MCP-Aufrufe: 40 Güter über zwei Seiten, vollständiger
Vergleich der sechs gemeinsamen Zählerfelder für die drei bisherigen Beispielgüter;
Simulation pausiert und zeitlich unverändert. Eine aktive sichtbare Lagerwarnung
„No good is selected.“ mit einem konkreten mittleren Lager, Raster-/Weltpositionen
getrennt. Zwei ergänzende Leseraufrufe bestätigen frische Session und leere Güterwahl
über inspect_building_settings. Unbekannte Alert-ID ergibt found=false, fremde Session
stale_session. Keine Bau-, Lager-, Simulations- oder sonstigen Spieländerungen.

Zusätzlich der bestehende opt-in Live-Test aller 22 Leser bestanden. 409 reguläre
Tests bleiben der automatische Nachweis; der Live-Test wird getrennt gezählt.
Nur eine Warnungsgruppe vorhanden: Biberwarnungen, Mehrfachziele und tatsächliches
Verschwinden einer zuvor aktiven Meldung bleiben offen. Rohantworten/IDs ausschließlich
lokal ignoriert; kein vollständiger UI-Abdeckungsnachweis behauptet.


## 2026-09-20 — Weitere Statusfälle und öffentlicher Logistikausbau

0.18.0 live: nach kurzem normalen Simulationslauf weiterhin nur die vorhandene
Lagerwarnung. Mit vorübergehend aufgehobener Güterwahl eines zweiten Lagers zwei
Ziele derselben Warnung bestätigt. Nach gültiger Güterwahl beider Lager verschwand
die zuvor aktive Kennung: found=false. Ursprüngliche Optionen (Carrot bzw. leer)
anschließend separat zurückgelesen und wiederhergestellt; Simulation pausiert.
Biberwarnungen sind mangels vorhandenem Fall weiter offen; nicht pauschal als bestanden
markiert. Keine künstlichen Status oder Rohdaten im Repository.

0.19.0 implementiert vier öffentliche Leser für Gebäudezugang, echte Straßenverbindung,
Arbeitsreichweiten und native gespeicherte Güterhistorien. Der Spielservice bietet
bereits Production/Consumption je GoodSample; Vorratsdifferenzen werden nicht dafür
umgedeutet. [Verträge und Pilot](logistics.md). Installation/Live-Abnahme ausstehend.


Abschlussprüfung 0.19.0: 431 reguläre Tests bestanden (418 Unit, 13 Integration),
drei opt-in Live-Tests übersprungen. Neue vier Leser über echten stdio-/HTTP-Testpfad
und Logstatus geprüft. Modpaket gegen 1.1.2.4 ohne Warnungen/Fehler erstellt. Insgesamt
26 Leser und 19 Aktionen/Validierungen. Nutzer um Speichern/Beenden gebeten; installierte
Version bleibt 0.18.0 bis zum gesicherten Austausch. Neue Live-Tests noch ausstehend.


## 2026-09-20 — 0.19.0 installiert

Nach bestätigtem Spielende Prozess erneut geprüft. Vorherigen Modordner vollständig
in einer neuen lokalen Sicherung erhalten. Fünf Dateien aus dem geprüften Paket
installiert und SHA-256 verglichen; private Konfiguration unverändert. Keine fremden
Bibliotheken installiert. Nutzerstart und Laden von MCP für die neue Live-Abnahme
abwarten; Biberwarnungen und neue Logistikfälle bleiben offen.


## 2026-09-20 — 0.19.0 Live-Pilot: Historie bestätigt, zwei Logistiklücken

Opt-in Test aller 26 Leser bestanden (Protokoll-/Vertragsabnahme). Gezielt bestätigte
Wege vom Distriktzentrum zu Erfinder/Farm/Holzfäller mit Distanzen 10/9/17. Güterhistorien
Water/Berries/Log mit 13 Samples lesbar. Ein zusätzlicher Tageswechsel bestätigt neue
Stichprobe: Water Produktion 2, Verbrauch 13, Nettobilanz -11, Bestand 186 → 175.
Innerhalb desselben Spieltages blieb die Historie unverändert. Ende Tag 14, ca. 00:49,
Simulation wieder pausiert. Keine Biberwarnung im geprüften Zeitraum.

Funktionaler Negativtest nicht bestanden: nach Entfernen eines einzelnen Wegstücks
meldete FindRoadPath im pausierten Spiel weiter connected=true, obwohl die native
Distriktdistanz bereits null war. Weg regulär wiedergebaut und Antwort erneut gelesen.
Farm und Holzfäller lieferten über GetComponentsAllocating<IBuildingWithRange> keine
Provider; die gewünschte Reichweitenfunktion war damit nicht live erfüllt.

0.19.1 wechselt auf öffentliche FindInstantRoadPath und AllComponents.OfType für die
Interface-Implementierungen. Korrektur gebaut, erneuter Live-Nachweis ausstehend.
Native-Client verweigert Weg-/Reichweitenantworten aus der unzureichenden 0.19.0.
Keine privaten Interna, Fremdmods oder neuen Abhängigkeiten. Rohdaten/IDs lokal ignoriert.

Abschlussprüfung 0.19.1: 433 reguläre Tests bestanden (420 Unit, 13 Integration),
drei opt-in Live-Tests übersprungen. Mod-Build ohne Warnungen/Fehler.

0.19.1 nach bestätigtem Speichern/Spielende installiert. Vorherigen Modordner vollständig
gesichert, fünf Paketdateien per SHA-256 geprüft, private Konfiguration unverändert.
Nutzer um Neustart/Laden gebeten; gezielte erneute Abnahme ausstehend.


## 2026-09-20 — 0.19.1 Wegunterbrechung bestanden; Terrain-Reichweite nachgebessert

Kontrollierter Live-Pilot bei pausierter Simulation: Distriktzentrum → Erfinder zunächst
verbunden (Distanz 10), nach Entfernen eines einzelnen Wegstücks connected=false und
Distanz null, nach regulärem Wiederaufbau wieder verbunden (10). Eingangsblockaden
blieben false, Distriktdistanz war während der Trennung null. Weg wiederhergestellt.

Farm/Holzfäller meldeten dagegen weiterhin supported=false. AllComponents lieferte
keine allgemeinen IBuildingWithRange-Provider; der vorige Korrekturansatz reicht nicht.
Weitere öffentliche Metadaten: BuildingsNavigation.BuildingTerrainRange.GetRange()
liefert natives ReadOnlyHashSet von Rasterzellen. 0.19.2 ergänzt diesen konkreten Zugriff,
kennzeichnet die Quelle explizit und verwirft alte Reichweitenantworten. Keine private
Reflection oder Radius-Schätzung. Installation/erneute Reichweitenabnahme ausstehend.

Abschlussprüfung 0.19.2: 439 reguläre Tests bestanden (426 Unit, 13 Integration);
drei opt-in Live-Tests übersprungen. Mod-Build ohne Warnungen/Fehler, neues Paket erstellt.
Nutzer um Speichern/Beenden für Installation und erneute konkrete Reichweitenprüfung gebeten.


## 2026-09-20 — 0.19.2 installiert

Nach bestätigtem Spielende erneut Prozessfreiheit geprüft, vorherigen Modordner vollständig
gesichert und fünf Paketdateien per SHA-256 verglichen. Private Konfiguration unverändert.
Keine Spieländerungen während der Installation. Neustart/Laden durch Nutzer und erneute
Farm-/Holzfällerreichweitenabnahme stehen aus.

## 2026-09-20 — 0.19.2 Reichweiten und Lesekatalog live bestätigt

Nach Neustart und Laden durch den Nutzer: Holzfäller 611 und Farm 485 Rasterzellen
über BuildingTerrainRange, supported=true und explizite Quelle bestätigt. Erste und
letzte Seite jeweils geprüft; letzte Seite genau ein Eintrag, keine Folgeseite.
Straßenverbindungen zum Distriktzentrum positiv (Distanzen 17/9). Keine Spieländerungen.
Anschließend opt-in Integrationstest mit allen 26 Lesewerkzeugen bestanden.

Missionsplan und aktuelle Dokumentation abgeglichen. 439 reguläre Tests und Mod-Build
bleiben der Nachweis des unveränderten Codes; keine unnötige Wiederholung für diese
Dokumentationsänderung. Biberwarnungen und allgemeine Bedürfnis-/Produktionsdiagnose
bleiben offen; einzelne Terrain-Piloten belegen keine vollständige Gebäudedeckung.

## 2026-09-20 — 0.20.0 Bedürfnisse und Gebäudebetrieb gebaut

Drei Leser: aggregierte native Bedürfnisse, Einzelbiber und Gebäudebetriebsbelege.
Öffentliche NeedManager-/NeedSpec-, Workplace-/Arbeitszeit- und Manufactory-APIs
geprüft und direkt verwendet. Fehlende Komponenten/Rezepte bleiben unbekannt;
keine aus fehlenden Meldungen oder Stillstand erfundene Gesundheit/Blockade.
Session-Prüfung und entity_not_found für Einzelbiber ergänzt. Keine neue Abhängigkeit.
460 reguläre Tests bestanden (447 Unit, 13 Integration), drei opt-in Live-Tests
übersprungen. Mod-Build ohne Warnungen/Fehler. Installation und Live-Pilot folgen.
