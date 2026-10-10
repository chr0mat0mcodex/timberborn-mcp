# Projektstand

## 0.37.0 größere Bauvorhaben — Stützenpilot bestanden (2026-10-10)

Aktuell: Nach menschlichem Gate fünfteilige Höhenfolge vollständig gebaut und
als `completed` bestätigt. Trägerabhängigkeit negativ geprüft; oberer Weg und
Lager erst nach realer Trägerfertigstellung nativ validiert. Alle vier Baustellen
erreichbar, fertiger Lagereingang frei, Distriktdistanz 33. Bestandsblockade mit
sieben verlorenen Verbindungen korrekt abgewiesen und restauriert. Separaten
Plan vor Auftrag gestoppt, kein Objekt entstanden; dadurch weitere Großprojekte
in dieser Sitzung erwartungsgemäß blockiert. Tag 435, 19,40625 Uhr pausiert;
Sägewerk pausiert. Log geprüft, sieben priorisierte Verbesserungen im BACKLOG
festgehalten. Keine Rohlogs/Spiel-IDs gespeichert. Fachlicher Abschluss für 0.37.0;
keine pauschale Katalog-/Kraftdrehungsabnahme. Folgende Absätze sind Historie.

Aktuell: Kraftvorschau-Korrektur nach erneutem menschlichem Gate live bestanden.
Zweiteilige Folge Welle (26,6,3) → Lager (32,8,4) vollständig fertiggestellt;
Start erzeugt keine Baustelle, Advance wartet auf Fertigstellung, Bauarbeiterzugang
und fertiger Lagereingang separat bestätigt. Reales Dreiknotennetz mit 53 Angebot.
Noch offene Lücke: Vorschauplattformen zählen nicht als reale Träger für die native
Geometrieprüfung von oberem Weg/Lager. Korrektur `deferred_native_validation`
mit expliziter Trägerabhängigkeit, Kollisionskontrolle und zwingender frischer
Ausführungsprüfung vorbereitet; passende Policy-/Ausführungstests ergänzt.
Erneutes Gate nötig. Stützenfläche (33,8..10,4) frei, Plattform regulär freigeschaltet,
Sägewerk aktiviert. Tag 430, 15 Uhr pausiert, null offene Baustellen.
Keine Gesamt-Abnahme, kein Commit/Push. Folgende Absätze sind frühere Zwischenschritte.

Erster Livepilot nach Katalogübernahme: zwei identische Lagerpläne gültig mit
identischem Schlüssel, schwebendes Lager ohne Stütze korrekt ungültig; jeweils
Vorschau wiederhergestellt. Kraftleser bestätigt Sägewerk/Rad als verbundenes
Zweiknotennetz mit momentan null Leistung. Kraftwellen-Vorschau liefert jedoch
Backendfehler. Pilot gestoppt, null offene Baustellen rückgelesen; kein Bauauftrag.
Korrektur vorbereitet: Vorschauports als getrennte öffentliche Transput-Deskriptoren
aus TransputProviderSpec statt Runtime-Ports eines Vorschau-Knotens. Erneutes
menschliches Gate und Kraftvorschau-Livetest nötig, keine Abnahme/kein Push.

Explizite 3D-Pläne mit bis zu 32 Bauteilen, 32×32 Grundfläche und acht belegten
Höhenebenen implementiert. Abhängigkeiten, gemeinsame Vorschau, sequenzielle
Fertigstellungsprüfung und Kraftgeometrie/-netzleser. Neue Werkzeuge und
Regressionstests vorbereitet, Doku/Agentenanleitung ergänzt.
Nach menschlichem Gate/Bereitmeldung: Backendstart und Spielkontakt mit Bridge
0.37.0 bestätigt. Supervisor-Katalog enthält alle sechs neuen Werkzeuge; laufende
Codex-Sitzung bietet sie noch nicht an. Feature-Livetest wartet auf Client-Neustart,
kein weiterer Build erforderlich. Letzter vollständig live abgenommener Stand
bleibt 0.36.0. Kein Commit/Push vor fachlicher Abnahme.
[Umfang, Grenzen und Livepilot](docs/large-projects-0.37.0.md).

## Vorschaltprozess und Regionalsuche live abgenommen (2026-10-10)

Nach Katalogübernahme sind alle Spielwerkzeuge verfügbar. timberborn_status vor
und nach stop/status/start bestätigt dieselbe erreichbare Bridge 0.36.0 und Sitzung;
keine Client-Neustarts innerhalb dieses Zyklus. Menschlicher Vollgate-Lauf und acht
warnungsfreie Python-Tests liegen vor. Erstbefüllung/Toolkatalogänderungen können
weiterhin einen Client-Neustart benötigen; regulärer Backendwechsel wurde belegt.

Sägewerksfeld (31,4), 8×8, Start z=3: z=4 automatisch untersucht, auf beiden Ebenen
null unbekannte Zellen und null Legacy-Verfeinerungsreads. Rampe lokal als o;
vier Lagerkandidaten oben. 33 native Reads, vier Planungen. Lager (32,8,4)
in Spielvorschau gültig und angeschlossen, kein neuer Weg nötig.
Hauptsiedlungsfeld (30,24), 16×16, z=4: zwei getrennte Fenster, acht Planungen,
28 native Reads. Suchabdeckung unterscheidet p/c/u; elf weitere beobachtete Höhen
wegen maxHeights=1 ausdrücklich ungeprüft. Lager (35,28,4) ebenfalls gültig und
angeschlossen. Negative Lodge (33,27,4) trotz Vorschau-Anschluss geometrisch ungültig.
Alle drei Vorschauen melden Zustand wiederhergestellt, keine Sitzungssperre.
Keine Bauaufträge/Räumungen; Spiel bleibt pausiert. Bauarbeiter-/fertiger Gebäudezugang
weiterhin nicht nachgewiesen, keine neue reguläre Ausführungsfreigabe.

## Vorschaltprozess: Stop/Start live bestanden, Katalogübernahme offen (2026-10-10)

Menschlicher gezielter Test: alle acht Python-Tests ohne Warnungen bestanden.
Danach start → stop → status → start über dieselbe Codex-Verbindung erfolgreich:
Backend aktiv/ungesperrt, gestoppt/gesperrt, weiterhin gestoppt, wieder aktiv.
Kein Timeout und kein Transportverlust. Lokaler Katalog enthält 76 Backendwerkzeuge,
darunter timberborn_status und survey_region. Laufende Agentensitzung exponiert
trotz Änderungsnachricht weiterhin nur das Lifecycle-Werkzeug. Erstbefüllter Cache
braucht deshalb noch einen Client-Neustart zur tatsächlichen Werkzeugübernahme.
Backend zuletzt aktiv, Gatesperre aufgehoben. Kein neuer Build/Testlauf nötig.
Spielkontakt, Regionalsuche-Abnahme und Commit/Push bleiben offen.

## Vorschaltprozess: erster Livepilot, zwei Korrekturen offen (2026-10-10)

start meldet Backend aktiv; Verbindung bleibt auch nach stop/status/start nutzbar.
Stop meldete jedoch Timeout trotz beendetem Backend: alte Registry-PIDs waren
inzwischen RuntimeBroker/conhost/Codex-Prozessen zugeteilt. Prozessstartzeit wird
jetzt mit Registry-Heartbeat verglichen. Keine fremden Prozesse beendet.
Spielwerkzeuge wurden in dieser Agentensitzung nach tools/list_changed nicht
sichtbar; daher kein timberborn_status und keine Regionalsuche-Abnahme möglich.
Werkzeugkatalog wird jetzt nach erfolgreichem Backend-Handshake lokal gecacht und
beim nächsten Frontendstart auch während Gatesperre angeboten. Erstbefüllung braucht
einen Backendstart der neuen Fassung; tatsächliche Clientaktualisierung bleibt zu prüfen.
Zwei Regressionstests ergänzt (PID-Wiederverwendung, Katalog bei gesperrtem Start).
Menschlicher gezielter Python-Test und erneuter Livepilot offen; kein Commit/Push.

## Persistenter Codex-Vorschaltprozess vorbereitet — Gate offen (2026-10-10)

Menschlicher Gate-Lauf vollständig bestanden: sechs Python-Tests, Release-Build,
Unit-/Integrationstests, Bridge-Paketierung und Installation mit fünf geprüften
Dateien (138,9 Sekunden). Noch keine Liveabnahme.
ResourceWarning für offene stdout-Pipes erkannt: Reader schließt seinen Kanal nun
im finally; Stop wartet außerhalb des Protokolllocks auf Readerende. Stop-/Crashtests
prüfen geschlossene Kanäle zusätzlich. Diese nach dem Python-Testschritt vorgenommene
Korrektur braucht nur den gezielten menschlichen Python-Test, keinen erneuten Mod-Build;
danach Codex einmal neu starten, damit der Vorschaltprozess den neuen Code lädt.

Python-stdio-Frontend mit eigenem stop/start/status-Werkzeug, Backend-Handshake
nach Neustart und projektweiter Gatesperre implementiert. Vorbereitungsskript
stoppt über die Sperre und verwendet nur noch den exakten Releasepfad als Rückfall.
Synthetische Tests in verify.ps1 eingebunden, vom Agenten nicht ausgeführt.
Bestehender Codex-Servereintrag auf Vorschaltprozess umgestellt und zurückgeprüft.
[Einrichtung, Grenzen und Liveabnahme](docs/codex-mcp-supervisor.md).
Livebestätigung und Prüfung der Kanal-Korrektur offen; kein Commit/Push. Danach Rampenblindfleck
fachlich abnehmen; dessen offener Status bleibt bestehen.

## Rampenblindfleck: Liveabnahme an Clientverbindung blockiert (2026-10-10)

Nach menschlichem Gate erneut live bereit gemeldet. Erster timberborn_status
scheitert mit Transport closed; kein Bridgekontakt, keine neue Session und kein
fachlicher Livebeleg. Client-Reconnect-Werkzeug in dieser Sitzung nicht verfügbar.
Codex-MCP-Verbindung muss clientseitig neu aufgebaut werden; kein isolierter
stdio-Prozess als Ersatz. Keine Spieländerung, kein Commit/Push. Danach geplante
Regionalsuche-Abnahme fortsetzen. Die Bereitmeldung ersetzt kein Liveergebnis.

## Rampenblindfleck und Höhensuche vorbereitet — Gate offen (2026-10-10)

Native Überlappungszellen statt pauschaler unknown-Kachel; automatischer Scan
weiterer beobachteter Höhen, abdeckungsorientierte Fenster und explizite ungeprüfte
Bereiche implementiert. Grenzen einstellbar, Legacy-Fallback begrenzt. Tests ergänzt,
noch nicht ausgeführt; kein Build/Commit/Push. Ein Modpaket über menschliches Gate.
[Umfang und konkrete Liveabnahme](docs/regional-survey.md).


## Bauflächen oberhalb der Siedlung und beim Sägewerk geprüft (2026-10-10)

Obere Wege auf z=4 in beiden Gebieten nativ distriktverbunden. Kleines Lager
hinter Hauptsiedlung sowie kleines Lager/Wohnhaus beim Sägewerk bestehen
Spielvorschau mit Anschluss ohne neue Wege. Größerer Kandidat hinter Siedlung
abgelehnt; höherer Hang z=8 weiterhin teilweise unbekannt. Keine Bauten/Räumungen.
MCP-Suchlücke belegt: Slope/other setzt ganze 8×8-Kachel auf unknown und verhindert
survey-Planung trotz direkt gültiger Plätze. Maßnahmen im Backlog, noch kein Code.
[Prüfgrenzen und Verfahren gegen blinde Flecken](docs/regional-survey.md).
Endpause Tag 424, 21,375 Uhr; zwischenzeitlichen fremden Tempo-Wechsel beobachtet,
Urheber unbekannt. Kein neuer Nachweis von Gebäudetypen.


## Bauchargen-Frühabschluss live bestanden (2026-10-10)

Menschliches Entwickler-Gate durch anschließende Bereitmeldung bestätigt;
Bridge 0.36.0 erreichbar. Zwei kleine Lager in einer Charge ohne neue Wege:
Frühpause nach 4 und 3 statt jeweils 24 Spielstunden. Derselbe advance-Aufruf
schloss beide Bauten sequenziell ab; frische Einzelabfragen bestätigten
finished_accessible, freien Eingang, erhaltene Karotten-/accept-Konfiguration
und Pause. Laufkennungen und ursprüngliche Zeitobergrenzen blieben unverändert.
Identischer start wiederholte nur den terminalen Checkpoint, ohne Doppelbauten.

Kontrollfälle: unfertige erste Bank löste keinen Folgeauftrag aus; nach ihrer
Fertigstellung bei 6,90625 Stunden stoppte die nächste Materialprüfung im selben
Aufruf mit materials_missing (nur ein statt zwei benötigten Brettern vorhanden).
Keine zweite Bank beauftragt. Journalwechsel im Livepilot ohne Zugriffsfehler.
Die ursprüngliche externe Ursache des File.Move-Fehlers bleibt unbekannt.

Zwei Holzfällerplätze dienten als austauschbare Testfläche; Testbank wieder
entfernt, zwei Lager bleiben. Endstand Tag 424, 9,875 Uhr, Tempo 0. Keine neuen
Vorlagentypen: kumulative Abdeckung weiterhin 33/157. Keine künstlich erzeugte
verlorene Pausenbestätigung im Livetest; dafür vorbereitete Regressionen im Gate.
Kein Hintergrunddispatcher, Räumzeitfenster weiterhin unverändert.

[Details](docs/building-batch-workflow.md).

## Gebäudespieltest 0.36.0 (2026-10-10)

Aktueller Nutzerauftrag: reguläre Gebäudetypen einmal bauen, Verbesserungen
begleitend dokumentieren. Bauhütte neu finished_accessible; vorhandene fertige
Treppe bei vollständigem Bestandsabgleich nachgetragen. CSV jetzt 31/157,
126 offen; fünf Entwicklerwerkzeuge ausgeschlossen. Katalog weiterhin mit
15 nicht unterstützten regulären Vorlagen. Kein Neubau von Terrasse/Wasserfarm.
Tag 417, 17,78125 Uhr, pausiert, keine Baustellen. 49 Biber, 49 Betten,
23 freie Holzstämme. Sägewerk und Sauna für Holzansammlung pausiert.
Ein von drei geplanten Typen fertig; Charge wegen fehlendem weiterem Standort
gestoppt. Aufwandsalarm und neue Entscheidung vor Skalierung; kein MCP-Code geändert.
[Nachweise und Verbesserungsvorschläge](docs/playtest-0.36.0.md).

## 0.36.0 Screenshot abgeschlossen (2026-10-10)

Entwickler-Gate mit anschließender Bereitmeldung; native Bridge 0.36.0 bestätigt.
Zwei sichtbare Spielaufnahmen inklusive UI und Kamerametadaten live bestanden:
1280×720 / 122828 Bytes, 640×360 / 41678 Bytes. Position/Höhe, Blickrichtung,
Zielabstand, Bildwinkel und Bildweite konsistent; falsche Session ohne Bild
abgewiesen. Keine Bilddateien gespeichert. [Abnahme und Grenzen](docs/screenshot-0.36.0.md).
Einziger Release-Stand über Entwickler-Gate; keine weiteren Agent-Builds.
Vorbereitungsstände unten historisch; vorhandene Hermes-/Starteränderungen separat.

## 0.36.0 Screenshot vorbereitet — Entwickler-Gate offen (2026-10-10)

Erster Entwickler-Gatelauf: MCP-/Test-Build bestanden, Unit-Tests an veralteter
Negativerwartung für 0.36.0 gestoppt. Zwei entsprechende Teststellen gefunden
(BuildingCapabilityTests und GenericBuildingProjectTests): unbekannte Version
auf 99.0.0 geändert, positive 0.36.0-Vertrags-/Profilfälle ergänzt. Nur Quell-/
Diff-Prüfung; erneuter Entwickler-Gatelauf erforderlich, keine Agent-Testausführung.

Expliziter Unity-Spielbildabruf mit begrenztem JPEG und MCP-Bildblock implementiert;
Kameraposition/Höhe, Richtung, Ziel, Abstand, Bildwinkel und Bildweite auf Zielebene
in Metadaten. [Umfang und Prüfplan](docs/screenshot-0.36.0.md). Noch kein Livebeleg.
Finaler Stand nur quellgeprüft, vorbereitete Tests noch nicht ausgeführt.
Auf Nutzerkorrektur gilt wieder ausschließlich das Entwickler-Test-Gate:
keine Agent-Builds/Testläufe, keine Debug-/Parallelfassung. Frühere Vorprüfregel
und Zwischenstand-Builds unten sind historisch. Installation, Livetest, Commit
und Push für 0.36.0 stehen aus.

## 0.35.5 abgeschlossen (2026-10-10)

Holzdiagnose mit echter Ertragsprüfung, Sammelmarkierung/Replay, früher Holzstopp,
kompakte Chargenantworten und terminale Bauablehnung live bestanden. Gültiger
Einzelbau zusätzlich unabhängig als finished_accessible und pausiert bestätigt.
Kompakter Ablehnungsbericht 439 statt 973 Zeichen. Gezielte Vorprüfung: drei
Projekt-Builds ohne Warnungen/Fehler, 77 Unit-Tests und sechs Integrationsvarianten
bestanden; menschliches Gate durch anschließende Bereitmeldung bestätigt.
[Abschlussnachweis und Grenzen](docs/forestry-efficiency-0.35.5.md).
Screenshot-Wunsch, native Forstfilter und präzisere Eingabefehler bleiben im Backlog.
Frühere offene Abnahmestände unten sind historisch. Hermes-/Starteränderungen
bleiben separat im Arbeitsbaum.

## Baumstumpf-Korrektur und Holzstopp live bestanden (2026-10-10)

Nach erneutem menschlichem Gate native Bridge 0.35.5 erreichbar. Vollständige
Kontrollfläche: 45 Objekte, davon 34 abgeerntete Reste, acht junge Bäume ohne
Fällertrag und drei fällbare Kiefern mit je zwei Holz. Forestry und alle
Detailseiten stimmen überein. Stumpfauftrag abgewiesen; zwei echte Bäume markiert,
Replay identisch, beide anschließend als already_marked ausgeschlossen.

Begrenzter Lauf mit Holzschwelle drei: stock_target_reached nach 1,9375
Spielstunden und 5,51 realen Sekunden, vier freie Stämme statt zuvor zwei.
Beide Zielbäume anschließend als abgeerntet bestätigt. Pause separat gelesen:
Tag 411, 5,78125 Uhr, Tempo null. Kein Vollbeleg für 0.35.5: Chargenantwort und
terminale Bauablehnung noch live offen; deshalb noch kein Versionsabschluss,
Commit oder Push. [Nachweis](docs/forestry-efficiency-0.35.5.md).

## Vorprüfung vor Testübergabe korrigiert (2026-10-10)

CS8602 in ForestrySurvey behoben: nach der Nullprüfung wird durchgehend die
geprüfte lokale Vegetationsvariable verwendet. MCP-/Unit-Testprojekt,
Integrationstestprojekt und Mod gegen lokale Spielreferenzen erfolgreich
kompiliert, jeweils ohne Warnungen/Fehler. 77 gezielte Forestry-, SimulationRun-
und BuildBatch-Tests sowie alle sechs Varianten des Integrationstests
AuthenticatedBridgeThroughRealStdioHonorsIndependentActionGates bestanden.
Sandbox-Teststarts scheiterten vor der Ausführung am Testhost-Verbindungsaufbau;
die genannten erfolgreichen Testläufe erfolgten außerhalb der Sandbox.

Auf Nutzerauftrag AGENTS.md, DEVELOPMENT_WORKFLOW.md und globalen
general-mod-workflow aktualisiert: betroffene Projekte und Tests vor der Übergabe
wirklich ausführen, Compilerprüfung nicht durch Syntaxprüfung ersetzen.
Skill validiert; Diff-Prüfung sauber. Keine Installation, kein Commit/Push.
Erneutes menschliches Gate und Livevergleich Baumstumpf/erntefähiger Baum bleiben
für die korrigierten Ertragsdaten erforderlich; 0.35.5 ist noch nicht abgenommen.

## 0.35.5-Livepilot gestoppt: Baumreste nicht unterscheiden (2026-10-10)

Menschliches Gate und native Verbindung bestätigt; fünf installierte Paketdateien
mit Paket 0.35.5-20261010-180251-9ec61831 identisch. Sammelmarkierung zweier IDs,
identisches Replay, Parameterkonflikt und bereits erreichtes Holzziel geprüft.
Benutzerhinweis während des Pilots: angebotene Objekte können Baumstümpfe sein.
Lebens-/Wachstumsflags allein sind kein Nachweis eines vorhandenen Fällertrags.
Die bisherige Kandidatenprüfung ist deshalb nicht abgenommen. Lauf abgebrochen,
Pause bestätigt bei Tag 411, 3,875 Uhr; Holzziel fünf nicht erreicht.

Korrektur vorbereitet: öffentliche Cuttable.Yielder-Felder IsYieldRemoved,
IsYielding, Yield.GoodId und Yield.Amount in Vegetationsdaten aufnehmen. Forestry
schließt abgeerntete Reste, fehlende Ertragsdaten und fehlendes Holz aus.
Gezielte Tests für lebende/ausgewachsene Stümpfe, Nullbestand und unbekannte Daten
ergänzt; nur Syntax-/Diff-Prüfung. Erneutes menschliches Gate und stumpf-/baum-
spezifischer Livetest erforderlich. Versionsabschluss, Commit und Push bleiben offen.


## 0.35.5 vorbereitet – menschliches Gate offen (2026-10-10)

Auftrag: Versionsabschluss einschließlich Holz-/Effizienzfunktionen. Implementiert:
kompakte regionale Holzdiagnose mit optionalen Verbraucherinventaren, gebündelte
Fällmarkierungen mit Replay-Beleg, früher Simulationsstopp bei freiem Holzbestand,
terminale Bauablehnung und kompakte Chargenantworten mit optionalen Details.
Manifest/Bridge/Assembly und Client-Versionserkennung auf 0.35.5 vorbereitet.
[Umfang, Grenzen und Abnahme](docs/forestry-efficiency-0.35.5.md).

Menschlicher Gate-Lauf: MCP-/Test-Build und Unit-Tests bestanden. Integrationstest
stoppte an veralteter Werkzeugliste (inspect_forestry fehlte); Erwartungen für
lesende Holzdiagnose und separat freigeschaltete Markierung korrigiert. Erneuter
Gate-Lauf bestätigte Build/Unit-Tests, stoppte aber an der ebenfalls veralteten
ReadOnly-Erwartung für mark_forestry. Schreibwerkzeugliste korrigiert und explizite
Annotationstests ergänzt; entsprechende Erwartungen in den Integrationstests
zusammen geprüft. Erfolgreicher Gesamtlauf, Mod-Build, Installation und gezielter
Livetest stehen aus. Keine neue Spielbeobachtung;
letzter historischer Spielstand weiter unten. Kein Commit/Push dieser Implementierung
vor erfolgreichem Gate. Screenshot-Funktion als optionaler lesender Spielbildabruf
im Backlog vermerkt, noch nicht implementiert. Vorhandene lokale Integrationsänderungen
erhalten; sie sind kein Bestandteil des behaupteten Funktionsnachweises.


## Tagesabschluss und Übergabe (2026-10-05)

Letzter Live-Stand unverändert: Tag 409, 18,09375 Uhr, pausiert; 47 Biber,
29/157 reguläre Gebäudetypen. Holzausbau für die Fortsetzung vorbereitet,
aber auf Wunsch in diesem Nachtrag keine Spielaktion oder Codeänderung ausgeführt.
Konkrete Reihenfolge und Erfolgskriterien im
[Tagesabschluss](docs/playtest-2026-10-04.md#tagesabschluss-2026-10-05-und-geplanter-holzausbau).
Priorisierte MCP-Effizienzideen im [Backlog](BACKLOG.md): regionale Holzdiagnose,
Bestands-/Verbrauchserklärung, Sammelmarkierung, bedingtes Zeitfenster und kompakte
Antworten. Noch nicht implementiert oder abgenommen. Beim Wiedereinstieg zuerst
Live-Zustand frisch lesen; bestehende lokale Integrationsänderungen separat behandeln.


## Kraftanschluss und Sauna fertig (2026-10-05)

Weitere sieben Spieltage bis Tag 409, 18,09375 Uhr. Aktuell 47 Biber
(43 Erwachsene, vier Kinder), 49 Betten, keine Obdachlosen, keine kritischen
Bedürfnisflags und keine Stranded-Warnung. Alle Zeitfenster beendet, Pause bestätigt.

Neues Kraftlaufrad direkt neben der Zahnradwerkstatt fertig; vier Anschlusswege
fertig und Zugang frei. Kraftwarnung verschwunden und Zahnradproduktion tatsächlich
nachgewiesen: fünf Zahnräder in der neuen Sauna verbaut, vier weitere im
Werkstattausgang. Sauna finished_accessible, unpausiert, neun Holzstämme im
Brennstoffinventar, keine Betriebswarnung. Besucherwirkung nicht separat nachgewiesen.
CSV jetzt 29/157 reguläre Vorlagen fertig, 128 offen, fünf Entwickler-Vorlagen
ausgeschlossen. Das zusätzliche Rad erhöht die Typenabdeckung nicht.

Radbau zunächst nach 24 Stunden wegen fehlender Holzlieferung unfertig. Reife
Kiefern in tatsächlicher Holzfällerreichweite auf der unteren Ebene gefunden und
16 zusätzliche Fällzellen markiert; nach zwei weiteren Spieltagen Rad fertig und
Produktion angelaufen. Kein Zugangsfehler als Ursache beobachtet. Weitere elf
Karottenzellen für Rad, Wege und Sauna umgewidmet; rechnerisch 19 der ursprünglich
38 zusätzlichen Anbauzellen verbleiben. Endbestand 81 Karotten, 41 Beeren,
61 Wasser, zwei frei verfügbare Holzstämme, 31 Bretter und vier Zahnräder.

Nächster Schritt: Holzlieferung vor weiteren materialintensiven Gebäuden stärken,
dann zusätzliche Gebäudetypen. Kraftnetz-/Anschlussdiagnose im Backlog ergänzt.
Keine MCP-Codeänderung; bestehende lokale Änderungen außerhalb dieser Doku erhalten.


## Gebäudefokus: Farm, Sammelstelle, Zahnradwerkstatt (2026-10-05)

Vorläufiges Ziel 50 Biber; Gebäudeausbau hat Vorrang. Weitere acht Spieltage bis
Tag 402, 18,09375 Uhr. Aktuell 46 Biber (45 Erwachsene, ein Kind), 49 Betten,
keine Obdachlosen, keine kritischen Bedürfnisflags und keine Stranded-Warnung.
Die früher erreichten 50 sind ein historischer Zwischenstand, kein aktueller Wert.

Zweite Farm fertig/erreichbar mit einem neuen Anschlussweg; drei zugewiesene
Arbeiter. 38 neue feuchte Feldzellen in nativer Farmreichweite markiert und alle
als lebende Karotten ohne Wasserstress nachgewiesen. Später acht Zellen gezielt
für Werkstatt/Wege umgewidmet; 30 zusätzliche Anbauzellen verbleiben. Erste
Ernte angelaufen: Endbestand 48 Karotten und drei Beeren, 66 Wasser. Noch kein
langfristiger Versorgungsüberschuss nachgewiesen.

Neu für die Katalogabdeckung: ScavengerFlag.Folktails und GearWorkshop.Folktails,
beide finished_accessible samt bestätigter Endpause. CSV jetzt 28/157 reguläre
Vorlagen, 129 offen, fünf Entwickler-Vorlagen ausgeschlossen. Die Sammelstelle
hat nichts in Reichweite; Bau ist kein Altmetall-Ertragsnachweis. Zahnradwerkstatt
hat einen Arbeiter, Rezept Gear und zehn Bretter im Eingang, aber keinen Kraft-
anschluss: noch keine Zahnradproduktion. Drei neue Wege halten den südlichen
Zugang offen; keine neue Stranded-Warnung nach Fertigbau.

Nächster sinnvoller Schritt: Kraftanschluss der Werkstatt und danach weitere
Produktions-/Gebäudetypen. Freie Baustoffe zuletzt 21 Holz und acht Bretter.
Alle Zeitfenster abgeschlossen, Spiel pausiert. Verbesserungen nebenbei im
Backlog: Suchfenster samt Route erklären, wählbare Kandidaten/freie Korridore,
gebündelte Pflanz-/Umwidmungsaufträge und arbeitszeitbewusste Räumfenster.
Keine MCP-Codeänderung in diesem Spielabschnitt.


## Aktueller Spielauftrag (2026-10-05)

Vorläufiges Bevölkerungsziel auf 50 Biber geändert und bereits erreicht.
Schwerpunkt jetzt Gebäudetypen-Ausbau: jede reguläre Vorlage mindestens einmal
fertig nachweisen. Keine weitere gezielte Bevölkerungsexpansion; Nahrung und
Baustoffversorgung für den Ausbau stabilisieren. Historische Ziele unten sind
überholt. Verbesserungsvorschläge weiter nebenbei dokumentieren.


## Spielabschnitt abgeschlossen (2026-10-05): 50 Biber, 26 Gebäudetypen

Auf ausdrücklichen Spielauftrag etwa zehn Spieltage weitergespielt, Tag 384 bis
394 (18,09375 Uhr). Bevölkerung 38→50 (41 Erwachsene, neun Kinder).
Sechs Gebäude fertig/erreichbar nachgewiesen: drei Wohnhäuser, ein Miniwohnhaus,
ein Krankenbett und eine Hängematte. Ein südliches Wohnhaus anschließend zur
Öffnung eines blockierten Geländedurchgangs entfernt; fünf Neubauten verbleiben.
Krankenbett/Hängematte sind neue Typen: CSV 26 built_finished, 131 offen,
fünf Entwickler-Vorlagen ausgeschlossen; Nenner weiterhin 157 reguläre Vorlagen.

Drei Biber waren nach dem südlichen Wohnbau gestrandet, trotz nachgewiesener
Hauszugänge. Nach gezieltem Abriss und acht Spielstunden keine Stranded- oder
Exhausted-Warnung mehr, keine kritischen Bedürfnisflags. Endstand 49 Betten,
49 belegt, ein Obdachloser; weiterer Wohnraum bleibt nötig. Ein gezielter
Miniwohnhaus-Versuch auf breiterer Freifläche lieferte keinen Kandidaten und
keinen Auftrag. Kein weiteres Skalieren enger Wohnstandorte in diesem Abschnitt.

Wohncharge eins: ein Haus fertig; zweiter Kandidat nativ buildingValid=false,
kein Objekt/keine offenen Baustellen, Charge nach Diagnose beendet. Gemischte
Charge: Krankenbett plus Miniwohnhaus fertig, dann begrenzt kein Standort in
beiden Flächen. Hängematte auf zweiter Fläche fertig. Südcharge: 25 Vegetationsziele
in begrenztem Fenster geräumt, zwei Häuser fertig, eines wie oben entfernt.
Fünf Vorlagen regulär erforscht: Hecke, Laterne, Krankenbett, Doppelwohnhaus,
Hängematte. Forschung ist kein Gebäudenachweis; eingangslose/erhöhte Zugänge
passen nicht automatisch zum ebenen Projektablauf.

Abschließende Vorräte: 71 Wasser, 53 Beeren, null Karotten, 30 frei verfügbare
Holzstämme und 33 Bretter. Nahrungsproduktion vor weiterem Wachstum priorisieren.
Alle Chargen terminal, kein laufender Zeitauftrag, aktuelle Pause bestätigt.
Verbesserungen in BACKLOG.md: definitive Ablehnung statt pending_build-Endlosschleife,
Kandidatenalternativen nach sicherer Ablehnung, kompakte Fortschrittsantworten,
frühes Beenden fertiger Bauzeitfenster, ausführungsbezogene Katalogfähigkeit,
Wohnbelegung je Haus und Schutz freier Rückwege beweglicher Biber.
Nächster Spielschritt: zusätzlicher Wohnraum mit offenem südlichem Durchgang,
parallel Nahrungs-/Holzproduktion für Wachstum ausbauen. Gesamtziel bleibt offen.


## Live bestanden: eindeutige Eingabefehler (2026-10-05)

Menschliches Gate bestätigt; fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-182839-57bfd38b überein. advance_building_project und
develop_building_project lehnen waitSeconds=21 mit invalid_argument, Feldname,
erlaubtem Bereich 0–20 und ausdrücklich keinem gesendeten Bau-/Simulationsauftrag ab.
Gültiger develop-Aufruf auf frisch beobachteter belegter Wegzelle: status=ok,
not_started, requestSubmitted=false, area_exhausted und object_intersection.
Spielzeit unverändert (Tag 384, 16,375 Stunden), Tempo vor/nachher 0.

Das Projektledger der früheren Sitzung ist nach Modneustart nicht verfügbar:
gültige inspect_building_completion-Abfrage des alten Auftrags liefert state_conflict.
Daher positive Kontrolle durch frische reguläre Planung ersetzt; kein neuer
Fertigbau und keine sitzungsübergreifende Wiederaufnahme behauptet. Ungültige
Geschwindigkeit synthetisch im menschlichen Testlauf abgedeckt, hier nicht
zusätzlich live erzeugt. Parameterfehler anderer Werkzeugfamilien bleiben offen.
Die folgende Gate-Vorbereitung ist historisch.


## Vor menschlichem Gate: eindeutige Eingabefehler (2026-10-05)

Live-Verbindung auf 0.35.4 bestätigt; keine Spielmutation in diesem Schritt.
Quellcodebefund: zentrale Fehlerbehandlung meldet auch reine Parameterfehler
bei Bauwerkzeugen als möglicherweise unbestätigte Aktion. Für develop_building_project,
advance_building_project und inspect_building_completion jetzt reine Vorabprüfung
von späteren Spielzugriffen getrennt. invalid_argument meldet ausdrücklich keinen
Bau-/Simulationsauftrag; bekannte Zeit-/ID-Felder erhalten konkrete Hinweise.
Echte Fehler nach Beginn der Spielzugriffe behalten ihre bisherige Behandlung.
Werkzeugbeschreibungen nennen waitSeconds 0–20.

Vier Regressionen für Wartezeit 21 und Geschwindigkeit 2 bei develop/advance
mit absichtlich nicht vorhandenem Client ergänzt (jeder Zugriff wäre ein Fehler).
Roslyn-Syntax und Diff geprüft; Tests/Build nicht ausgeführt. Änderung noch nicht
live aktiv und nicht committed. Abnahme nach menschlichem Skript: ungültige
Eingabe eindeutig ohne Auftragswirkung ablehnen, danach gültige Abschlussabfrage
eines vorhandenen Projekts. Bei falscher Fehlerklasse keine weiteren Fälle skalieren.


## Live bestanden: Mehrflächen-Charge (2026-10-05)

Menschliches Gate abgeschlossen; fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-181012-9be85db6 überein. Native Vorprüfung: erste 1×1-Wegfläche
ohne Kandidat, zweite 2×2-Fläche mit zwei Kandidaten. Charge wechselte automatisch
auf regionIndex=1, dokumentierte die erste Fläche und baute genau einen Holzfäller
ohne zusätzliche Wege. Frische unabhängige Abschlussabfrage: finished_accessible,
Eingang frei/erreichbar, 24 Stunden exakt, Überschwingen 0, Pause bestätigt.
Identischer start-Aufruf liefert denselben unveränderten Abschluss/Zeitstempel.
Negativkontrolle mit zwei belegten Wegflächen: no_candidate_in_authorized_regions,
finishedCount=0, keine Auswahl, kein Bau-/Räumlauf. Spiel blieb pausiert.

Abnahme gilt für begrenzten automatischen Flächenwechsel und Abschluss/Replay.
Mehrflächen-Räumung, gemeinsame Budgets und verlorene Bestätigungen synthetisch
abgedeckt, in diesem Live-Pilot nicht zusätzlich erzeugt. Keine freie globale
Standortoptimierung. Vorbereitung und frühere offene Gate-Einträge unten historisch.


## Vor menschlichem Gate: mehrere Teilflächen (2026-10-05)

Nach ausdrücklichem Go implementiert: Basisfläche plus optional additionalRegions
mit bis zu drei weiteren Rechtecken (x/y/z/width/height, jeweils maximal 8×8).
Feste Listenreihenfolge, kein Rücksprung und keine freie Kartensuche. Pro Fläche
bisherige Planung und optionale Räumung; nur nach fehlendem Kandidaten bzw. fehlender
räumbarer Vegetation weiter. Offene/unbestätigte Eingriffe, Materialmangel,
Zugangsfehler oder ausgeschöpftes Räumbudget stoppen die gesamte Charge.

Ein gemeinsames maxClearTargets (maximal 64), keine Multiplikation je Fläche.
Worst-Case-Zeitbudget: clearanceHours × Flächenzahl + constructionHours × Gebäude
höchstens 672 Stunden. Individuelle Laufgrenzen unverändert. RegionIndex und
PreviousRegions bleiben gespeichert; Räumlauf-Kennungen je Fläche getrennt,
Baukennungen weiterhin je Gebäude. Bei Flächenwechsel keine erneute Einreichung
bereits gestarteter Aufträge. Fertignachweise enthalten die Teilfläche.

Alte Einflächenparameter serialisieren unverändert (additionalRegions bei null
weggelassen); alte Fingerprints und Räumkennungen bleiben gültig. Bestehende
terminale Chargen werden nicht nachträglich erweitert: neue Liste braucht neue ID.
Fünf Feature-Dateien syntaktisch geprüft; Regressionen ergänzt, aber noch nicht
ausgeführt. Keine Modänderung, Bridge bleibt 0.35.4. Kein Commit vor Gate/Livetest.

Abnahme nach menschlichem Skript: erste kleine Fläche ohne Kandidat, zweite
mit vorher nativ geprüftem Bauplatz. Eine Charge muss selbst wechseln und ein
Gebäude bis finished_accessible bauen; inspect/erneuter Start mit gleicher ID
darf keinen weiteren Auftrag erzeugen. Negativkontrolle: nur ungeeignete Flächen
stoppen begrenzt. Bei falscher Kontrollprobe oder unbestätigtem Eingriff stoppen.


## Ausbaupilot nach Reichweitenabnahme (2026-10-05)

13 zusätzliche Karottenzellen aus der kombinierten Suche markiert und sämtlich
als lebende wachsende Pflanzen ohne Wasserstress nachgewiesen. Insgesamt 46
Feldmarkierungen (33 vor Erweiterung). Neuer Holzfäller am natürlichen Abstieg:
Baustellenzugang und finished_accessible belegt, besetzt, unterer Wald in nativer
Reichweite. Zwölf reife Kiefern zur Ernte markiert; im Tagesfenster globales frei
verfügbares Holz 15→41. Eigener Ausgang bei Abfrage leer, deshalb keine exakte
gebäudebezogene Produktionsmenge behauptet.

Erste Wohncharge: 1/3 fertig/erreichbar, dann no_candidate_in_authorized_region.
Zweite Charge im selben Gebiet räumt 23 Vegetationsziele innerhalb 48 Stunden,
baut ein weiteres Wohnhaus mit zwei Anschlusswegen fertig/erreichbar, stoppt dann
1/2 ebenfalls mangels Kandidaten. Beide Chargen terminal; kein drittes Haus gebaut.
Alle Zeitfenster beendet und Pause bestätigt. Endstand: 38 Biber (35 Erwachsene,
3 Kinder), 42 Betten/vier frei, kein Obdachloser, keine kritischen Bedürfnisflags;
Wasser 78, Beeren 70, frei verfügbares Holz 21. Keine Gesamtzufriedenheit behauptet.
Gebäudetypenabdeckung unverändert: 24/157 reguläre Vorlagen, 133 noch offen.

Effizienzentscheidung angefragt: begrenzter automatischer Teilflächenwechsel für
Bauchargen oder ausdrücklich weitere manuelle Standortschritte. Zwei Stopps im
8×8-Gebiet zeigen die Grenze. Beispielprojektion bei 20 Aufrufen je Restvorlage:
mehr als 2600 Aufrufe, zusätzlich Bevölkerung und Versorgung; keine gemessene
Prognose. Bis neuer Nutzerentscheidung keine weitere kostspielige Bau-/Suchserie.
Reichweitenfeature bereits committed/gepusht als 4673105.


## Live bestanden: regionale Arbeitsreichweite (2026-10-05)

Menschliches Gate bestätigt, fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-174150-098e2061 überein. Farm: 490 native Reichweitenzellen,
36 interne Reads, planningCalls=0. Fünf freie feuchte Rechtecke mit 13 Zellen;
alle empfohlenen Zellen zugleich frei, feucht und in Reichweite. Unabhängige
32-Zellen-Reichweitenseite stimmt vollständig mit dem Raster überein.
Wohnhauskontrolle: supported=false/source=unavailable, vollständig unbekanntes
Raster und keine Reichweitenempfehlungen; ebenfalls keine Planung (21 Reads).
Keine Spielmutation im Abnahmetest. Feature im beschriebenen Umfang bestanden.
Die folgenden offenen Gate-Einträge sind historische Vorbereitung.


## Vor Test-Gate: regionale Arbeitsreichweite (2026-10-05)

Anbaupilot auf Bridge 0.35.4 bestanden: drei freie, feuchte Zellen mit separat
belegter Farmreichweite als Karotten markiert. Nach rund 23 Spielstunden alle
DREI tatsächlich lebend/wachsend, ohne Wasserstress. Zeitlauf nach exakt 24 Stunden
abgeschlossen, Überschwingen 0, Endpause bestätigt. Danach 35 Biber (26/9),
36 Betten, Wasser 77, Beeren 70, frei verfügbares Holz 7. Keine kritischen
Bedürfnisflags; Vorlagenabdeckung weiterhin 24/157, Gesamtziel offen.

Nächste Effizienzverbesserung MCP-seitig vorbereitet, Bridge bleibt 0.35.4:
survey_region optional workBuildingId und planBuildings=false. Vollständige
native Reichweite (höchstens 2048 Zellen/64 Seiten), Quellenangabe und separates
Raster; freie feuchte Rechtecke daraus neu berechnet, nicht nur vorhandene Top-6
gefiltert. Fehlende Reichweite unbekannt, wechselnde/unvollständige Daten abweisen.
Ohne workBuildingId bleibt bisheriges Verhalten; planBuildings=false spart bis
zu zwölf Planaufrufe. template/districtId bleiben aus Kompatibilität Pflicht.
Keine automatische Markierung und keine Aussage über Personal oder passenden Rohstoff.

Regressionen ergänzt: mehrseitige Schnittmenge, trockene/gesperrte Zellen,
unbekannte Reichweite, wechselnde Zähler, Duplikate, Budget, falsche Sitzung und
Standardverhalten. Drei Feature-Dateien per Roslyn syntaktisch geprüft, kein Build
oder Testlauf. Änderungen uncommitted bis menschliches Gate und Liveabnahme.
Abnahmepilot: Farm plus 16×16-Region, planBuildings=false; planningCalls=0,
Reichweitenzellen gegen gezielte inspect_work_range-Seite prüfen und Empfehlungen
gegen Feuchte/Hindernisse. Nicht unterstütztes Wohnhaus muss unbekannte Reichweite
und keine Empfehlungen liefern. Bei falscher Kontrollprobe stoppen.


## Live bestanden: Bodenfeuchte 0.35.4 (2026-10-05)

Menschliches Gate abgeschlossen; alle fünf installierten Dateien stimmen mit
Paket 0.35.4-20261005-172618-6954fbfd überein. Native MCP-Verbindung erreichbar.
Vier Forstzellen feucht, 16 obere Vergleichszellen trocken; vier Zellen über
Boden liefern null. Drei regionale Abfragen stimmen mit den direkten Proben
überein. Trockenes freies 2×2-Rechteck ausgeschlossen, feuchtes freies 2×1-Rechteck
gefunden und separat bestätigt. Rechteckzellen sind gleichzeitig hindernisfrei
und feucht; bestehende Pflanzflächen bleiben ausgeschlossen.
Regional 12 bzw. 17 native Reads, keine zusätzlichen Reads für Bodenfeuchte.
Keine Spielmutation oder Simulation; Spiel pausiert. Momentane Bodenfeuchte
belegt keine Arbeitsreichweite, pflanzenspezifische Eignung oder Dauerbewässerung.
Versionslisten-/Syntaxkorrektur eingeschlossen; frühere offene Gate-Einträge
unten sind historisch. Feature im beschriebenen Umfang abgenommen.


## 0.35.4: Live-Einstieg blockiert, Clientkorrektur vorbereitet

Nachfolgendes menschliches Gate beim Kompilieren abgebrochen: Versionsersetzung
hatte einen einzelnen !=-Vergleich ungültig erweitert. Auf is not (Versionen)
korrigiert; alle 29 geänderten C#-Dateien mit Roslyn syntaktisch geprüft, ohne
Syntaxfehler. Das ersetzt keine Typprüfung, Tests oder den offenen Livetest.

2026-10-05: Nach menschlichem Gate ist Paket 0.35.4 vorhanden; Spielprotokoll
meldet geladene Mod und bereiten Endpoint, keine passende Start-Exception.
Status und Simulation scheitern jedoch bereits vor dem Feuchtigkeitsnachweis.
Quellcodebefund: zentrale Envelope-Versionsliste und weitere Vertragsprüfungen
akzeptierten nur bis 0.35.3. Alle bisherigen 0.35.3-Prüfungen um 0.35.4 ergänzt,
historische Versionen und Vertragsgrenzen unverändert. Regression liest die
Version direkt aus dem Mod-Manifest und prüft den NativeClient mit synthetischer
Feuchteantwort sowie Fähigkeits-/Budgetprofil. Nur statisch geprüft.
Erneutes menschliches Skript-Gate nötig; kein bestandener Livetest, kein Commit.


## Test-Gate offen: Bodenfeuchte (0.35.4, 2026-10-05)

Auf Nutzerauftrag leere grüne/bewässerte Bodenflächen direkt abfragbar machen.
Öffentliches ISoilMoistureService.SoilIsMoist(Vector3Int) lokal per Metadaten
nachgewiesen und in die Kartenabfrage integriert; nur Boden, sonst null.
Regionalsuche ergänzt separate Feuchtigkeitszeilen und freie feuchte Rechtecke
ohne zusätzliche native Reads. Alte Bridge: unbekannt statt trocken.
Feuchte bedeutet weder Pflanzbarkeit noch Arbeitsreichweite oder künftigen Ertrag.
Zwei Regressionstests ergänzt; nur Quellcode/Diff geprüft, noch kein Build,
Deploy, Livetest oder Commit dieser Erweiterung. Übergabe: docs/regional-survey.md.


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



## Tagesabschluss 4./5. Oktober 2026 — Arbeit pausiert

Feierabend auf Nutzerwunsch. Gesamtziel offen; keine weitere Spielsteuerung,
Tests oder Installation bis zur Fortsetzung. Letzter bestätigter Spielzustand:
32 lebende Biber, 36 Betten (vier frei), 24 von 157 regulären Vorlagen gebaut.
Fünf Entwicklerwerkzeuge ausgeschlossen; 133 reguläre Vorlagen ohne Baunachweis.
Zufriedenheit insgesamt nicht nachgewiesen. Spiel zuletzt mit Tempo 0 beobachtet;
kein neuer Save oder erneuter Zustandsabruf zum Tagesabschluss behauptet.

Versorgung: zuletzt Wasser 83, Beeren 60, frei verfügbares Holz 1. Karottenbestand
zuletzt 0; 30 lebende Pflanzen vorhanden. Die 13 Kiefern der Forststichprobe
wachsen noch, ohne Wasserstress. Nächster wirtschaftlicher Schritt: größere
nachweislich erreichbare Holz- und Nahrungsproduktion, danach weitere Wohnplätze.

Abgenommen: Speichersperren im Zeitlauf, begrenzte Bauchargen, regionale Suche
und ein daraus tatsächlich fertig/erreichbar gebautes Wohnhaus. Letzter gepushter
Checkpoint: 4d117db; regionale Suche in e6006e3. Die nachfolgende unknown-Reparatur
ist nur statisch geprüft und bleibt uncommitted: RegionSurvey.cs plus zwei
Regressionsfälle in RegionSurveyTests.cs. Auch diese Abschlussdokumentation ist
lokal gespeichert. Andere bereits vorhandene Änderungen bleiben erhalten.

Wiedereinstieg: menschliches prepare-human-live-test.ps1 bei beendetem Spiel,
danach laden und live bereit. Zuerst unknown-Reparatur im bisherigen Nordbereich
gegenprüfen: nur tatsächlich betroffene Teilflächen unbekannt, keine Freigabe
unbekannter Geometrie. Erst nach bestandenem Gate/Livetest gezielt committen und
pushen, anschließend Versorgungsausbau. Ablauf: DEVELOPMENT_WORKFLOW.md.



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

## Gesamtziel — Ausbaucheckpoint 2026-10-04 nach Zeitlauf-Reparatur

Verifiziert: 31 lebende Biber (22 Erwachsene, 9 Kinder), 33 Betten, zwei frei,
keine Obdachlosen und keine kritischen Bedürfnisflags. Wasser 90, Beeren 70,
verfügbares Holz 18; Übersicht liefert keinen gesamten Nahrungsbestand.
Zwei weitere Lodge-Wohnhäuser und eine HaulingPost fertig und laut Charge
finished_accessible; Fertigstatus aller drei unabhängig zurückgelesen.
Transportstation mit fünf von fünf gewünschten Arbeitern, laufender Job.
Vorlagenabdeckung jetzt 24/157: HaulingPost neu, 133 noch ohne Baunachweis.

Erste Charge: ein von drei Häusern fertig, dann kein weiterer Kandidat im
kleinen Südostfenster. Zweite Charge: größeres 8x8-Fenster, 28 Vegetationsziele
geräumt, Wohnhaus und Transportstation nacheinander fertig; zwei neue Wege.
Keine blinde Wiederholung des gestoppten Auftrags. Vier begrenzte Zeitfenster
(48 + 24 + 24 + 24 Stunden) abgeschlossen; aktuelles Tempo separat 0 bestätigt.

Karottenfläche von 23 auf 30 Zellen erweitert; sieben neue Pflanzen unabhängig
lebend/wachsend und ohne Wasserstress beobachtet. Vier Bäume gezielt entfernt.
Farm tatsächlich mit drei von drei Arbeitern besetzt. Westliche vermeintliche
Freifläche liegt tiefer im Wasser, östliche Reserve höher: daher dort kein
Feldauftrag. Markierung und tatsächliche Pflanzung getrennt nachgewiesen.

Effizienz: rund 40 fachliche MCP-Aufrufe für drei fertige Gebäude (nur eine neue
Vorlage) und sieben Feldzellen. Bauchargen helfen, regionale Platzwahl und
Versorgungsaufbau bleiben manuell. Grobe lineare Warnprojektion bei diesem Mix:
133 übrige Vorlagen etwa 5.000 Aufrufe; keine verlässliche Gesamtprognose.
Aufwandsalarm gestellt, neue ausdrückliche Entscheidung gemäß AGENTS.md erbeten.
Nutzer entschied anschließend ausdrücklich: regionale Flächensuche entwickeln.
Gesamtziel nicht erreicht.



## Live bestanden — temporäre Spielsperren in Zeitläufen (2026-10-04)

Menschliches Test-/Installationsgate abgeschlossen; alle fünf installierten
Paketdateien stimmen mit Paket 0.35.3-20261004-231320-8e75fd0c überein.
Gezielter Vier-Tage-Lauf: beim vom Nutzer ausgelösten Speichern dreimal
running/game_speed_locked mit Tempo 0 beobachtet; danach derselbe Lauf wieder
running/advancing mit Tempo 7. Ziel und Echtzeitbudget unverändert.
Abschluss nach exakt 96 Spielstunden und 271,166 Echtzeitsekunden: completed,
target_reached, pauseConfirmed, Überschwingen 0. Aktuelles Tempo separat 0.

Negativkontrolle: ausdrückliche MCP-Pause unterbricht einen zweiten aktiven Lauf
mit explicit_speed_change; aktuelles Tempo danach separat 0 und Spielzeit stabil.
Der eingefrorene Unterbrechungsbeleg enthält noch Tempo 7 vor Befehlswirkung;
deshalb die aktuelle Pause immer gesondert lesen.

Öffentliches SpeedLockChangedEvent über EventBus angebunden; keine Entsperrung
oder Wiederbeschleunigung durch die Bridge. Sperre beim Speichern jetzt direkt
nachgewiesen; eine rückwirkende eindeutige Ursache aller früheren Abbrüche ist
nicht beweisbar. Budgetablauf/Abbruch während Sperre synthetisch geprüft, im
Livetest nicht zusätzlich provoziert. UI-Geschwindigkeitswechsel separat nicht
live getestet. Gesamtziel bleibt offen: zuletzt 27 Biber, 23/157 Vorlagen.

## Gesamtausbau: 27 Biber, 23/157 Vorlagen

Sieben weitere Gebäude fertig; Karottenanbau und Bretterlager wirken.
Spiel nach erfolgreicher Zeitlauf-Reparaturabnahme bestätigt pausiert. 27 Betten voll;
Holzversorgung und weitere Wohnplätze als nächstes. Zwölf neue Forstzellen
lebend/wachsend und ohne Wasserstress nachgewiesen. Gesamtziel weiterhin offen.
[Belege und offene Schritte](docs/playtest-2026-10-04.md),
[vollständige Vorlagenliste](docs/building-coverage.csv).


## Live bestanden — Bauchargen (2026-10-04)

Menschliches Gate mit anschließendem „live bereit“; fünf installierte Paketdateien
gegen das neue Paket geprüft. Zwei Gebäude automatisch nacheinander fertig und
zugänglich: kleines Lager mit Carrot/obtain, danach Mini-Wohnhaus. Zehn tote Bäume
automatisch geräumt, sechs lebende erhalten. Räumwartefenster tatsächlich gestartet;
bei der unabhängigen Rückfrage waren die Ziele bereits entfernt. Drei native
Zeitfenster je exakt 24 Stunden, kein Überschwingen, Pause jeweils bestätigt.
Fünf ausführende Chargenaufrufe bis Abschluss; Replay/Budgetkontrolle zusätzlich.
Identischer Start und abgeschlossene Fortsetzung unverändert, Budgetänderung
abgelehnt. Negative bebaute 1×1-Fläche stoppt vor Bau/Räumung und Folgegebäude.
Unabhängige Zugangs-/Lagerabfragen stimmen mit Abschluss überein. 21 Betten,
17 belegt; kein Nachweis für 100 zufriedene Biber oder neue Vorlagenabdeckung.

Grenzen: kein erzwungener Prozessabbruch im Livetest; Materialmangel und erschöpftes
Räumbudget synthetisch abgedeckt, hier nicht zusätzlich live provoziert.
Baustellenkonfiguration in diesem Pilot erst nach Fertigstellung zurückgelesen.
Verbesserungen: vollständiges Räumbudget läuft trotz früher geräumter Ziele aus;
Budgetkonflikt meldet zu allgemein invalid_argument; gespeicherter completion-Wert
kann während Räumung noch zum vorherigen Gebäude gehören. Keine falsche Aktualität
behaupten: checkpointOnly und Zeitstempel beachten. Breitere Vorlagen-/Höhenplanung
und kleinere gezielte Räumflächen bleiben offen.


## Vor menschlichem Gate — vollständige Bauchargen

Erster menschlicher Skriptlauf beim MCP-Build mit CS8604 abgebrochen; Installation
nicht erreicht. Nullprüfung des Chargenauftrags korrigiert (explizite Ablehnung
vor Feldzugriff). Erneuter menschlicher Skriptlauf und Livetest stehen aus.

Zweiter Skriptlauf: MCP- und Integrationstest-Projekt bauen erfolgreich;
Unit-Test-Projekt stoppt mit 22 xUnit1051-Meldungen. Alle 22 neuen asynchronen
Testaufrufe reichen jetzt TestContext.Current.CancellationToken weiter.
Tests/Installation noch nicht durchgelaufen; nächster menschlicher Lauf offen.

Dritter Skriptlauf: alle Projekte bauen; Tests scheitern am MCP-Eingabeschema
der Chargenwerkzeuge und verursachen dadurch weitere Katalog-/Startfehler.
Schemawurzel explizit auf object eingeschränkt, passend zur vorhandenen
Null-Ablehnung des Parsers. Gezielter Katalogtest mit/ohne Schreibfreigabe ergänzt.
Erneuter menschlicher Testlauf und Installation bleiben ausstehend.

Nach erneutem ausdrücklichem Go: MCP-Chargensteuerung für Standortwahl,
einmalige autorisierte Vegetationsräumung samt Wartebudget und sequenzielle
Fertigstellung implementiert. Private persistente Checkpoints vor Eingriffen,
lesende Klärung nach Abbruch, feste Budgets, kompakte Fortschrittsantworten.
Synthetische Prüfungen ergänzt; bislang nur Quellcode-/Diff-Prüfung, kein
Testlauf/Build/Installation/Livetest. Noch kein Commit oder Push dieser Änderung.
Nächster Schritt: menschliches `scripts/prepare-human-live-test.ps1`, danach
gezielter Zwei-Gebäude-Pilot mit Wiederaufnahme und negativem Kontrollfall.
[Umfang und Abnahme](docs/building-batch-workflow.md).

## Live bestanden — wiederaufnehmbare Bauabläufe

Reparatur nach erneutem menschlichem Gate abgenommen: neues Lager und zwei Wege
fertig, Eingang frei/zugänglich, Carrot/obtain erhalten. Komponentenanzahl 0
belegt fehlenden optionalen Anschlussblocker; Aggregat finished_accessible stimmt
mit Einzelabfragen überein. Exakt 24 Stunden, kein Überschwingen, aktuelle Pause.
Wiederaufnahme nach Abschluss erhält Lauf-ID/Ziel/Zeit; geändertes Budget wird
abgelehnt. Fünf Paketdateien identisch zur Installation. Vier Ablaufaufrufe bis
zum Abschluss, zusätzliche Kontrollen separat. Neue Mod-/MCP-Fähigkeit begrenzt
live bestanden; kein autonomer Gesamtausbau oder Zufriedenheitsbeleg.

Historischer Erstversuch: menschlicher Skriptlauf erfolgreich; Paketdateien verifiziert.
Live: Standortablehnung, Baustart, Bauzugang, Lagerkonfiguration, Wiederaufnahme,
Budgetkonflikt und exakt begrenzte Simulation bestanden. Lager fertig/erreichbar,
aber Aggregat fälschlich budget_exhausted wegen fehlendem optionalem Anschluss-
blocker. Reparatur ergänzt dessen Komponentenanzahl, unterscheidet nachgewiesene
Abwesenheit von unknown und meldet fehlenden Zugang separat. Reparatur inzwischen
nach erneutem menschlichem Gate live bestätigt.

Nach erneutem Nutzer-Go zur Ausbauautomatisierung: `develop_building_project`
bündelt Planung/Start und begrenzte Bauzeitbegleitung; `advance_building_project`
und `inspect_building_completion` setzen anhand vorhandener nativer Auftrags-
und Lauf-IDs fort. Ein unveränderliches Zeitfenster pro Bauprojekt; Abschluss
prüft alle Bauobjekte, Gebäudezugang, Anfangskonfiguration und aktuelle Pause.
Erfolglose Suche ergänzt eine Diagnose am Suchursprung. Keine neue Abhängigkeit
oder Bridge-Version; additiver Komponentenbeleg in der Mod, Zugangsgrenzen erhalten.
Synthetische Regressionen und Katalogintegration ergänzt; menschlicher Skriptlauf bestanden.
[Ablauf, Grenzen und Abnahme](docs/building-completion-workflow.md).

## Live bestanden — gebündelte Räumung und Baustart

Nach menschlichem Gate und Reparatur-Livetest auf Bridge 0.35.3 abgenommen.
`remove_vegetation_batch`: 1–16 konkrete Ziele seriell, Einzelbelege und Stopp
beim ersten Konflikt/unbestätigten Ergebnis. Drei Ziele in einem Aufruf entfernt,
separat bestätigt. Konfliktkontrolle nach Mod-Korrektur: state_conflict,
Folgeziel not_attempted, beide Pflanzen unverändert. Fünf Räum-Vorbedingungen
vor dem Lifecycle liefern jetzt explizite Konflikte statt HTTP-400-Unsicherheit.
Unsichere tatsächliche Änderungen bleiben unconfirmed; kein Retry/Rollback.
`start_building_project`: Planung und nativer Pilotstart in einem Aufruf,
explizit first_candidate/development_pilot, bestehende Zugangsprüfungen erhalten.
Belegtes Feld not_started/requestSubmitted=false. Positivfall: Lagerstart samt
Carrot/obtain an unfertiger Baustelle bestätigt, Bauarbeiterzugang nachgewiesen.
Nach einem Spieltag fertig und erreichbar, Einstellungen erhalten, Lauf exakt
24 Stunden mit bestätigter Pause. Fünf installierte Paketdateien verifiziert.
Drei Räumaktionen bzw. bisher Plan/Validierung/Start je in einem MCP-Aufruf;
Bauzeit und Abschlussprüfung bleiben separat. Kein allgemeiner Mehrgebäudebatch.

## Live bestanden — gebündelte Kolonieübersicht und Bauprüfdetails

Nach menschlichem Gate und gezieltem Livevergleich abgenommen. Neues lesendes `inspect_colony_overview`
bündelt Snapshot und höchstens vier Bedürfnis-Seiten in einem MCP-Aufruf.
Vollständigkeit, Fortsetzungsposition, fehlende Beobachtungen und schwankende
Bevölkerungszähler bleiben explizit; keine atomare Beobachtung, kein Gesamtfutter-
oder Zufriedenheitsnachweis. Sitzungs-/Versionswechsel und wechselnder Katalog
werden abgelehnt, ohne Wiederholung. Bridge bleibt unverändert bei 0.35.3.
Bauprüfung compact kürzt Zell-/Quellenangaben; unabhängige Diagnosezustände,
Grenzen, Verluste und Wiederherstellung bleiben. Live: Bauprüfung 3922 → 3304
Zeichen (16 %), projizierte Daten gleich außer laufendem Versuchszähler.
Kolonieübersicht 5732 statt zusammen 14363 Zeichen (60 %), ein statt drei
MCP-Aufrufen; alle 42 Bedürfnisse und Zustandswerte gegen Einzelabfragen geprüft.
Spiel pausiert, complete/countsStable=true, atomic=false. Fünf installierte
Paketdateien stimmen mit dem neuen menschlich erzeugten Paket überein.
Keine neue Baufreigabe und kein Gesamt-Zufriedenheitsnachweis.

## Live bestanden — kompakte MCP-Antworten, erster Pilot

Nutzer-Go für effizientere MCP-Abläufe nach Aufwandsalarm. MCP-Präsentationsschicht
für Status, Lagereinstellungen und generische Bauvalidierung: Standard compact,
optionales `detail=full`. Bestehende native Validierung vor der Projektion;
Fehler/unbestätigte Einstellungsänderungen vollständig, Unknown-/Zugangsbelege
bleiben sichtbar. Keine Spielmod- oder Versionsänderung (Bridge 0.35.3).
Menschliches Gate und Livevergleich bestanden: Lagerlesen 1258 → 593 Zeichen
(53 % weniger), identische Zustandswerte. Einmalige Modusänderung separat bestätigt,
veralteter Erwartungswert abgelehnt, ungültiges detail als -32602 abgelehnt.
Bauprüfung 3922 → 3647 Zeichen (7 % weniger), semantisch gleich bis auf den
Versuchszähler; valid=false, Wiederherstellung und Unknowns erhalten.
Kein breiter Effizienzgewinn für Bauprüfung behauptet; dafür gezielt weiterarbeiten.
Nachweise in docs/playtest-2026-10-04.md.

## Live bestanden — 0.35.3 Lagerkonfiguration im Bauauftrag

Auf Nutzerwunsch optionale initialStorageGood/initialStorageMode am ebenen
Bauprojekt. Direkt an der bestätigten, initialisierten Baustelle anwenden;
ausdrücklich kein Warten auf Bauabschluss. Getrennter Konfigurationsbeleg mit
Rücklesung auf späterem Frame, unveränderte Bauzugangsprüfungen. Settings-Opt-in
zusätzlich auf MCP- und Bridge-Seite. Unpassende Lagergüter vor Bau ablehnen,
Konfiguration in Replay-Identität aufnehmen, keine Wiederholung bei Unsicherheit.
Menschliches Gate abgeschlossen, fünf installierte Paketdateien stimmen per
SHA-256 überein. Feature-Livetest bestanden: Carrot/obtain an der unfertigen
Baustelle bestätigt, ungültiges Gut ohne Bau abgelehnt, Replay ohne Doppelbau,
verändertes Replay abgelehnt. Nach zwölf Spielstunden Lager fertig, Eingang frei,
Konfiguration erhalten, Simulation automatisch pausiert.
Farm/Personal/Prioritäten und große/Sondergebäude bleiben Folgearbeit.

## Abschluss am 2026-10-04 — 0.35.2 Projektbudget

Menschliches Gate und gezielter Live-Test bestanden: fünf sequenzielle ebene
Projekte derselben Sitzung fertig und erreichbar, erstes Replay ohne neue Objekte,
belegter Standort weiterhin abgelehnt, keine offenen Baustellen, Simulation pausiert.
1024 ebene und 1024 gemeinsam gezählte vertikale Projekte je Sitzung; historische
Profile, Geometrie-, Zugangs- und Isolationsgrenzen bleiben unverändert.
Grenzfall im automatischen Controllertest, kein vertikaler Massentest behauptet.
Nächster Nutzerwunsch: gewünschte Lagerkonfiguration direkt im Bauauftrag;
große und besondere Vorlagen bleiben genehmigte Folgearbeit.
[Nachweise und Playtestfortschritt](docs/playtest-2026-10-04.md).

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
