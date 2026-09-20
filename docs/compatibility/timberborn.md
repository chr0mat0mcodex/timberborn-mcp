# Kompatibilität und Live-Nachweise

## Aktueller Stand

| Komponente | Belegter Stand |
| --- | --- |
| Eigene Mod | 0.19.0 installiert; 0.19.1 korrigiert beim Live-Pilot entdeckte Weg-/Reichweitenlücken, neue Abnahme offen |
| Spiel | Timberborn 1.1.2.4, Folktails, kleine Entwicklungskolonie |
| Tests | 433 reguläre Tests; Live-Piloten getrennt |
| Fremdmods | Keine Pflichtabhängigkeit der eigenen Mod |
| MCP | Externer C#-Server, native Auswahl, stdio |

Aktuelle Funktionen und Grenzen: [Projektstand](../../PROJECT_STATE.md).
Keine Zusage für sämtliche Spielversionen, Fraktionen, Vorlagen oder Modkombinationen.

## Historisches Nachweisprotokoll

Die folgenden datierten Einträge behalten ihren damaligen Versions-/Installationsstand.
Ein altes „offen“ oder „installiert bleibt …“ ist kein aktueller Status.


## Live-Abnahme 0.11.0 — 2026-09-20

26 MCP-Aufrufe: 0/1/3/7/1 mit Zeitstillstand/-fortschritt bestätigt; Distriktzentrum
Soll/Ist 2 -> 3 -> 2. Schlusszustand ursprüngliche Besetzung und Simulation 1×.
Geschwindigkeitsänderung weiterhin erst nach Quittung im nächsten Update sichtbar.

## Installiert 0.11.0 — 2026-09-20

Alle Standardgeschwindigkeiten 0/1/3/7 anhand mitgeliefertem SpeedControlPanel.uxml
belegt. 200 Tests bestanden, Mod-Build/Installation geprüft. 3×/7× und Sollbesetzung
noch nicht live abgenommen; ältere Live-Nachweise bleiben begrenzt gültig.

## Buildstand 0.10.0 — 2026-09-20

Sollbesetzung mit öffentlichen Workplace.IncreaseDesiredWorkers/DecreaseDesiredWorkers
gebaut. 191 reguläre Tests, Mod-Build ohne Warnungen/Fehler. Noch nicht installiert
oder live geprüft; 0.9.0 bleibt installierter/live bestätigter Stand.

## Live-Abnahme 0.9.0 — 2026-09-20

Workforce-Roster und Gebäudebesetzung abgeglichen: 10 Worker, 3 beschäftigt, 7 frei,
2 am Distriktzentrum/1 an Holzfällerflagge. Keine ungeklärten Referenzen. Lodge und
Path ohne Arbeitsplatz; Path canPause=false trotz Pausekomponente. Nur gelesen.

## Entwicklungsstand 0.9.0 — 2026-09-20

Betriebsdiagnose über öffentliche PausableBuilding-/Workplace-APIs erweitert.
168 reguläre Tests bestanden. Installation und Live-Abnahme offen; 0.8.0 bleibt
installierter/live bestätigter Stand. Ältere Bridge liefert operations=null.

## Live-Abnahme 0.8.0 — 2026-09-20

Pause/1× über MCP bestanden: Zeitstillstand und erneuter Fortschritt separat gelesen,
MCP in Pause erreichbar. ChangeSpeed wirkt verzögert; unmittelbare Quittung meldet
noch alten Wert. Schlusszustand 1×, keine Spielsperren entsperrt oder Retries.

## Buildstand 0.8.0 — 2026-09-20

Öffentliche SpeedManager-/IDayNightCycle-API gegen lokale 1.1.2.4 gebaut.
158 reguläre Tests bestanden, separater Mod-Build ohne Warnungen/Fehler.
Pause/1× und Zeitwerte noch nicht live bestätigt; installiert bleibt 0.7.0.

## Buildstand 0.7.0 — 2026-09-20

Lodge-Pilot gegen lokale Timberborn-1.1.2.4-Assemblies gebaut, keine Warnungen/Fehler.
147 reguläre Tests bestanden; drei Live-Tests übersprungen. Noch nicht installiert
oder live abgenommen. Letzter installierter/live geprüfter Stand: 0.6.1.

## Fortschrittsbeobachtung 0.6.1 — 2026-09-20

Farmhauslieferung und Bauarbeit über zwei Lesestichproben nachgewiesen:
Baustellenbestand 2 -> 4 Log, MaterialProgress 0.08 -> 0.16, BuildTimeProgress
0 -> 0.109375. Noch unfertig. Holzversorgung vom Nutzer eingerichtet; native
Produktionssteuerung dadurch nicht nachgewiesen. Globalbestand und Baustellenbestand
sind getrennte Beobachtungen und waren in der zweiten Probe verschieden (0 bzw. 4 Log).

## Materialbeobachtung 0.6.1 — 2026-09-20

EfficientFarmHouse.Folktails als gespeicherte Baustelle live gelesen: Vorlagenkosten
25 Log, Inventar verfügbar/leer, global Log=0, Fortschritt 0, Baudistrikt bekannt.
Kosten und aktueller Bestand getrennt bestätigt; kein Restbedarfs-/Lieferzeitversprechen,
kein Nachweis zeitlicher Lieferung oder Fertigstellung. Vier reine Leseaufrufe.

## Gebäude-/Distriktbeobachtung 0.6.0 — 2026-09-20

District Center und Holzfällerflagge mit bekannten Betriebs-/Instant-Distrikt-IDs
live gelesen, fertiger Path ohne DistrictBuilding-Komponente. Fehlende Entity und
fremde Sitzung abgewiesen. Keine unfertigen Strukturen vorhanden, daher Baustellen-
und Materialfortschritt noch nicht live bestätigt. Keine Mutation im Lesepilot.

## Aktueller nativer Bau-Pilot — 2026-09-20

Bridge 0.5.0: genau einen Path über den regulären Platzierer per MCP gebaut.
Separate Abfrage bestätigt erwartete Entity-ID, Vorlage, Position und finished=true.
Ein neues Objekt, beobachtete Vorräte unverändert. Live-Nachweis nur für diesen
Einzelfall; kein allgemeiner Gebäude-/Versorgungsbau und kein Distriktnachweis.

Stand 2026-09-19. Diese Matrix beschreibt die tatsächlich geprüfte lokale Kombination.

| Komponente | Version | Nachweis |
|---|---|---|
| Timberborn | 1.1.2.4-52e959e-sw | Versionsdatei und API |
| More HTTP API | 11.0.0 | Manifest, Spiel-Log, API und fünf MCP-Leseaufrufe |
| Moddable Timberborn | 11.1.2 | Manifest und Spiel-Log |
| Mod Settings | 1.1.1.0 | Manifest und Spiel-Log |
| Harmony | 2.4.1 | Manifest und Spiel-Log |
| TimberUi | 11.0.1 | Manifest und Spiel-Log |

MCP-POC: stdio erfolgreich gegen diese Kombination getestet. Der Nutzer hat die gemeldeten
Zahlen der Testkolonie MCP mit der Spielanzeige verglichen und bestätigt.
Keine Aussage zur Kompatibilität anderer Versionen, großer Kolonien oder zusätzlicher Mods.

`http://localhost:8080/` funktioniert in dieser Umgebung. Die numerische IPv4-Adresse
`http://127.0.0.1:8080/` wurde vom Spiel abgelehnt. Der Client erhält den Hostnamen und setzt
ihn nicht durch eine numerische URL um. Daraus wird keine allgemeine Ursache abgeleitet.

## Native Bridge — 2026-09-20

Agent Bridge 0.2.0 mit Konfigurationspfad-Fix über ModRepository: im Spiel geladen,
HTTP-Port 8081 erreichbar, alle drei nativen MCP-Werkzeuge über stdio live erfolgreich.
Bevölkerung/Bestände/Wohnraum durch Nutzer bestätigt. Eine Kartenzelle technisch gelesen;
Semantik und Session-Wechsel noch offen. Bisherige Fremdmods weiterhin aktiv, aber vom
nativen Backend nicht angesprochen; isolierter Spielstart ohne Fremdmods noch nicht geprüft.

0.3.0 zusätzlich live geprüft: sechs native MCP-Werkzeuge, 21 Gebäude-/Wegeobjekte,
Fraktion/Freischaltung/Kosten von Lodge und Path, 64 Gelände-/Wasserzellen und vier
erklärbar abgewiesene Bauplatzvorprüfungen. Objektzahl und beobachtete Bestände unverändert.
Sitzungswechsel gegenüber vorher lokal gespeicherter ID direkt bestätigt.
0.4.0 kompiliert und synthetisch geprüft; Vorschau-Liveprüfung steht aus.

## Isolierter Betrieb — 2026-09-20

Nutzer bestätigt Spielstart mit ausschließlich eigener MCP-Mod. Anschließend separaten
nativen stdio-Lesetest ausgeführt: alle sechs Werkzeuge in derselben Sitzung erfolgreich,
keine übersprungenen Tests. Damit ist der lesende Betrieb der installierten Bridge 0.3.0
ohne aktive Fremdmods für diese Testkolonie nachgewiesen. Die Mod-Auswahl wurde durch den
Nutzer bestätigt, nicht durch eine zusätzliche API-Inventur. Kein Vorschau- oder Bauaufruf.
Dieser Nachweis gilt nicht für den noch nicht live geprüften Validator aus 0.4.0.

0.4.0 später live erreichbar, sechs Lesewerkzeuge auch nach Vorschauversuch erfolgreich.
Validator-Abnahme fehlgeschlagen: belegter Standort fälschlich akzeptiert; Pilot nach
einem Versuch abgebrochen. 0.4.1 als Korrekturkandidat gebaut/gepackt und auf Nutzer-Go
installiert und anschließend begrenzt live abgenommen: belegte Lodge abgewiesen,
freier Path akzeptiert. Zwei Vorschauversuche, keine beobachtete Entity-/Bestandsänderung,
keine Platzierung. Kein Nachweis für beliebige Vorlagen oder Baugeometrien.

## Agent Bridge 0.12.0 — gebaut, noch nicht live geprüft

Gegen dieselben installierten öffentlichen Spielbibliotheken kompiliert; Mod-Build
ohne Fehler/Warnungen. 224 reguläre Tests bestanden, 3 Live-Tests übersprungen.
WorkplacePriority, BuilderPrioritizable, TreeCuttingArea und PlantingService werden
verwendet. PlantingTool-Konstruktion zur Sperrabfrage kompiliert, DI und Verhalten
müssen im Spiel noch bestätigt werden. Keine unabhängige tapping-Markierung belegt.
Installierter Live-Stand bleibt 0.11.0 bis zum nächsten gesicherten Modwechsel.

## Agent Bridge 0.13.0 — gebaut, noch nicht live geprüft

255 reguläre Tests bestanden; 3 Live-Tests übersprungen. Mod-Build gegen vorhandene
Spielversion ohne Fehler/Warnungen. Neue öffentliche APIs: BlockObject.CanDelete,
EntityService.Delete, Demolishable.Mark/Unmark, RecoveredGoodStack.Delete. Verhalten
und Folgeereignisse im Spiel noch nicht abgenommen. Kein Saveformat-/Fremdmod-Zuwachs.
Öffentliche NaturalResource-/Plantable-Signaturen liefern keine Herkunftshistorie.
Die Kategorien planted/vegetation basieren auf aktueller Pflanzmarkierung und sind
keine belegte Unterscheidung zwischen natürlich gewachsen und selbst gepflanzt.
Installierter Stand weiterhin 0.11.0. 0.13.0 enthält auch die ungetesteten Erweiterungen
von 0.12.0; beide benötigen die gemeinsame nächste Live-Abnahme.

0.13.0 inzwischen bei beendetem Spiel gesichert installiert; fünf Datei-Hashes
bestätigt. Installationsprüfung bestanden, Laufzeit-/Live-Abnahme weiterhin offen.

0.13.0 Live-Abnahme: alle 14 Lesewerkzeuge bestanden. Arbeitsplatz- und Baupriorität
jeweils hin/zurück mit separatem Readback bestätigt, Ausgangszustände wiederhergestellt.
Katalog-/AreaManagement-DI läuft; crops/tree_planting/tapping lesbar. Simulation blieb
pausiert. Flächenänderungen, Löschsperren und Abriss-Folgeereignisse weiterhin offen.

0.13.0 Flächenmutation live bestätigt: tapping entfernt vorhandene Fällmarkierung,
tree_cutting stellt sie wieder her (191 -> 190 -> 191). crops/Carrot und
tree_planting/Pine jeweils setzen/lesen/entfernen/lesen erfolgreich (0 -> 1 -> 0).
Spiel pausiert; physische Pflanzung/Ernte bleibt ungeprüft. Abrissfreigabe aktiviert,
wirksam nach neuem Laden des Spielstands; Abriss selbst weiterhin offen.

0.13.0: Gebäude-/Wegabriss plus Wegneubau, Aufträge vegetation/planted und Schuttlöschung
live bestätigt. Sonderfall: reguläres Demolishable.Mark kann unmittelbar entfernen;
separate Abwesenheitsprüfung bestätigt. 0.13.1 korrigiert ausschließlich Antwortbewertung
und Clientvalidierung dafür; Patch noch nicht installiert/live abgenommen.

0.13.1 gesichert installiert, fünf Datei-Hashes bestätigt, private Konfiguration
unverändert. Live-Abnahme der korrigierten Antwort nach Neustart noch offen.

0.13.1 nun live bestätigt: sowohl wartende Markierung als auch unmittelbare reguläre
Entfernung mit korrektem applied/removed und separatem Zustandsabgleich. Weiterer
wartender Auftrag nach kurzer 7×-Simulation als verschwunden beobachtet, Holzfällung
am Ziel ausgeschlossen; Simulation anschließend wieder pausiert. Patch-Abnahme erledigt.

0.13.2 gegen unveränderte öffentliche Spielbibliotheken gebaut. 279 reguläre Tests
bestanden, 3 Live-Tests übersprungen. Neue Gesundheitsdaten und Kandidatenfilter noch
nicht live abgenommen. Installiert bleibt 0.13.1 bis zum nächsten Modwechsel.

0.13.2 inzwischen gesichert installiert, fünf Paketdateien verifiziert und private
Konfiguration unverändert. Lebenszustands-/Filter-Liveabnahme nach Neustart noch offen.

0.13.2 rein lesend live abgenommen: 101 Stichprobenkiefern mit 61 alive/40 dead,
35 nicht ausgewachsen. Vollständige Kandidatenliste mit 19 geeigneten Kiefern geprüft.
Kein toter Kandidat, keine Änderungen, Simulation weiterhin pausiert. Positive Fälle
von waterStress/isDying in dieser Stichprobe nicht nachgewiesen.

0.14.0: generischer Bauzugang mit vier neuen Werkzeugen implementiert. 316 reguläre
Tests bestanden, Mod gegen öffentliche 1.1.2.4-Referenzen kompiliert. Keine neue
Fremdmod-Abhängigkeit. Installation und Live-Abnahme noch offen; 0.13.2 bleibt der
aktuelle live bestätigte Stand. Siehe ../generic-building.md.

0.14.0 anschließend bei beendetem Spiel gesichert installiert. Fünf Paketdateien
per SHA256 geprüft; bisherige Konfiguration erhalten, nur neue Bau-Freigabe ergänzt.
Neustart und Live-Abnahme noch offen.

0.14.0 live bestätigt: kompletter Katalog und vier generische Bauaufträge samt
Rücklesung; belegter Standort in Validator/Platzierer abgelehnt. Farm im Lauf auf
76 % Materialfortschritt, alte Pumpe fertig/besetzt, Schlusszustand pausiert.
Layoutfilter für kleine Lager und Wege zu streng; Korrektur 0.14.1 vorbereitet.

0.15.0: native Lager-/Farmoptionen und Gebäudepause implementiert; 350 reguläre
Tests bestanden, Mod gegen 1.1.2.4 mit null Warnungen/Fehlern gebaut. Vorhandene
Anbauflächen-Werkzeuge weiterverwendet. Neue Settings benötigen eigene Freigabe.
Layoutkorrektur 0.14.1 enthalten. Live-Abnahme der neuen Version noch offen.

0.15.0 anschließend bei beendetem Spiel gesichert installiert; fünf Datei-Hashes
geprüft und bisherige Konfiguration erhalten. Neue Settings-Freigabe aktiviert.
Neustart und Live-Abnahme noch offen.

0.15.0 live bestätigt: Gebäudepause hin/zurück, Lagergut wählen/abwählen, alle vier
Lagermodi, Farmpriorität und bevorzugte Feldfrucht sowie Anbaufläche 0 -> 4 -> 0.
Separat nachgelesen. SideLine-Lager und zwei TwoSegmentLine-Wege generisch gebaut.
Kein Nachweis tatsächlicher Ernte/Umlagerung; Simulation abschließend pausiert.

0.15.0 zusätzlich: Karotten-Pflanzen/Wachsen/Ernten/Einlagern live nachgewiesen.
Vier Pflanzen bis rund 99 % beobachtet, danach 12 Karotten im Lager separat bestätigt
und vier neue Pflanzen mit neuen IDs. Anbaufläche bleibt erhalten; Simulation pausiert.

0.15.0: SmallTank regulär gebaut, auf Wasser gestellt und 30/30 Bestand separat
bestätigt. Globaler Wasserbestand im ersten Betriebsintervall 217 -> 219; später
209. Laufender Nachschub und Lagerung belegt, keine nachhaltige Gesamtbilanz.

0.16.0: Ingame-MCP-Log mit reasoning implementiert, 362 reguläre Tests bestanden.
Mod gegen öffentliche 1.1.2.4-Referenzen gebaut und gesichert installiert; bestehende
Konfiguration bytegleich. UI-/Live-Abnahme nach Neustart noch offen.

0.16.0 Log live bestätigt: Fenster vom Nutzer geöffnet; UI-Anbindung und Sichtbarkeit
über MCP wahr. Leser und zwei Pumpenaktionen mit reasoning/Abschlussstatus nachgelesen.
Kein UI-Patch nötig, Version bleibt 0.16.0.

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
