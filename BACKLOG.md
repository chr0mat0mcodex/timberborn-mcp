# Backlog

## Weitere Live-Lücken beim Kolonieausbau

- Flächenvorprüfung mit konkreten blockierten Zellen und Gründen: `set_area`
  meldet bei besetzter Pflanzfläche nur `invalid_argument` und einen pauschalen
  Hinweis auf möglicherweise unbestätigte Wirkung. Live: Kiefern vor der Farm,
  keine Markierungen verändert. Validierungsfehler vor Mutation von tatsächlich
  unbestätigten Änderungen unterscheiden; `precheck_area` vorsehen.
- Bauplanung ohne Kandidaten: Ablehnungsgründe aggregieren (Gelände, Objekte,
  Anschluss, Wegbudget), damit kein Suchraster durchprobiert werden muss.
- Räumliche Filter für Fäll-/Pflanzmarkierungen und Reichweiten: zwanzig lokale
  Fällfelder erfordern derzeit den Abgleich von 190 kolonieweiten Markierungen.
- Fortschrittsbericht: lebende Bevölkerung, beobachtete Grundbedürfnisse und
  echte Wohlbefindenspunkte sowie gebaute/fehlende Katalogvorlagen gemeinsam.
  Vorhandener Fertigbestand ist kein vollständiges historisches Bauregister.

## Kurzfristig priorisiert — kompakte MCP-Antworten (P1)

Nutzerauftrag vom 2026-10-04: zeitnah umsetzen, direkt nach dem laufenden
0.35.3-Test-Gate. Standardantworten sollen nur das für die konkrete Aktion oder
Entscheidung Nötige enthalten. Im Playtest wiederholen sich lange `limitations`,
verschachtelte Prüfberichte, Katalog-/Fähigkeitsbeschreibungen und vollständige
Objektzustände auch bei einfachen Status- und Einstellungsabfragen.

- Kompakte Standardansicht: Ergebnis, relevante Werte/Änderungen, notwendige IDs,
  konkrete Fehler oder Blockierungsgründe. Unbekannt/unbestätigt und wesentliche
  Nachweisgrenzen müssen eindeutig bleiben.
- Statische Erläuterungen einmal über Fähigkeiten/Dokumentation bereitstellen;
  ausführliche Geometrie, Prüfbelege und vollständige Objektdetails gezielt abrufbar
  machen. Keine wiederholten vollständigen Zustände bei kleinen Änderungen.
- MCP-Text und `structuredContent` auf unnötige Doppelübertragung prüfen;
  Protokoll- und Client-Kompatibilität erhalten. Lange Werkzeugbeschreibungen
  ebenfalls auf Wiederholungen und veraltete Aussagen prüfen.
- Kleiner Pilot mit Status, Lagereinstellung und Bauprüfung: Antwortgröße vorher/
  nachher messen, deutlich reduzieren und dieselben Entscheidungen ermöglichen.
  Fehler-, Konflikt- und Unknown-Fälle sowie abrufbare Details mitprüfen;
  erst danach auf weitere Werkzeuge ausweiten.

Status: erster Pilot nach menschlichem Gate live abgenommen:
Status, Lagerlesen/-änderungen und generische Bauprüfung mit optionalem
`detail=full`. MCP-Text bleibt aus Kompatibilitätsgründen inhaltlich gleich zur
strukturierten Antwort; beide tragen dieselbe gekürzte Projektion. Gebündelte
Ausbauabläufe und weitere Werkzeuggruppen folgen nach gemessenem Pilotnutzen.
Lagerlesen live 53 % kürzer; Bauprüfung nur 7 % kürzer. Dort bleibt der Großteil
in verschachtelten Diagnosebelegen: nächster Schritt ist eine kompakte fachliche
Zusammenfassung mit vollständigen Belegen auf Abruf, keine pauschale Feldlöschung.
Kein Aufschub hinter den vollständigen Gebäudekatalogausbau.

## Offene Folgearbeiten nach Phase E

Phase E ist mit 0.35.1 im vereinbarten begrenzten Umfang abgeschlossen.
Die folgenden Punkte sind eigenständige Folgearbeiten, keine offenen
Abschlussbedingungen von E und keine pauschale Baufreigabe.

| ID | Offener Punkt | Erforderlicher Nachweis / Grenze |
| --- | --- | --- |
| F01 | Vollständiger Weg- und Bauphasenschutz | Bestehende Baustellenzugänge unter geplanten Änderungen und Zugang neuer Baustellen vorab belastbar prüfen; positive und negative Kontrollen. Bis dahin unabhängige offene Baustellen ausschließen und `unknown` nicht als sicher behandeln. |
| F02 | Umweg als Schutz für eine spätere Bestandsänderung | Neuen Umweg zuerst real fertigstellen und seine Nutzbarkeit prüfen, dann die davon abhängige Änderung einschließlich betroffener Bestandszugänge testen. Der bestandene Anschluss um einen Baum ersetzt diesen Nachweis nicht. |
| F03 | Freie Höhenplanung und größere Bauprojekte | Bedarfsgerecht über feste Treppen-/Plattformfolgen, 8x8-Suche und vier neue Bodenwege hinausgehen; Abhängigkeiten, Bauphasen und Bestandszugänge für den erweiterten Umfang prüfen. Keine automatische Erhöhung der aktuellen Limits. |
| F04 | Besondere Gebäude- und Eingangsfälle | Direkt-fertig-Distriktzentrum mit eigenem Distrikt-Lebenszyklus gezielt live prüfen. Mehrere echte Gebäude-Eingänge erst bei belegter Spiel-API unterstützen; Bauzugangszellen sind keine Türen. |
| F05 | Breitere Kompatibilitäts- und Katalogabdeckung | Weitere relevante Geometrien, Fraktionen, Karten und Spielversionen mit repräsentativen Fällen prüfen. Generische Unterstützung ist keine pauschale Live-Abnahme aller Kataloggebäude; keine vollständige Vorlagen-/Drehungsserie ohne konkreten Bedarf. |

Weitere bereits erfasste Projektaufgaben stehen unter „Technische Lücken“;
Versorgung und Betrieb bleiben eine separate Folgestufe. Priorisierung des
nächsten Arbeitspakets erfolgt eigenständig, nicht als Fortsetzung einer offenen
Phase E. Abschlussumfang und Nachweise: [Missionsplan](missionsplan.md#abschluss-phase-e).

## Abgeschlossene Schritte und historische Priorisierung

Abgeschlossen: 0.35.1, Bauablauf und isolierte Überbudgetkontrolle live bestanden.
Vier Vorschläge nicht mit überlangen Anschlussprojekten füllen; vorhandene Wege
bevorzugt verwenden. [Abnahmekriterien](docs/road-budget-planner.md).
Historische Bank-/LargePile-Nachweise im geladenen MCP-Profil live bestätigt.
Fünf-Wege-Kandidat verworfen, spätere 4/3/2/1-Wege-Kandidaten gefunden;
keine weitere Grenzfallserie nötig. Sechs Wege für den reinen Planungstest
entfernt; Teststreifen bleibt offen, Simulation pausiert.

Generischer ebener Projektpilot 0.35.0 nach menschlichem Gate live bestanden:
Bank mit zwei neuen Wegen und großes 3x3-Freiluftlager mit einem Weg, beide
Drehung 1, mit realem Bau-/Fertigzugang, erhaltenen Bestandsanschlüssen und Replay.
Einzelne Kollision am äußeren Eckfeld des großen Grundrisses nativ abgelehnt.
Höhen-/Blockadekontrolle bestanden. Gefährliche Vorschau
erkannte 24 verlorene Verbindungen und wurde restauriert. Keine Vorlage mehr
einzeln freischalten. [Nachweis](docs/generic-building-project.md).

Offen: Direkt-fertig-Distriktzentrum mit eigenem Distrikt-Lebenszyklus nicht
live geprüft; Mehrfacheingänge spielseitig nicht öffentlich beobachtet. Keine
pauschale Katalogabnahme. Bank-/LargePile-Historie ist seit 0.35.1 im Profil.
Weitere Arbeit nach Missionsplan auf konkrete Lücken bei Weg-/Bauzugangsschutz
ausrichten, einschließlich Umwegfall; keine Drehungs-/Vorlagenserie ohne Bedarf.

### Ausgangspunkt vor 0.35.0

E-Schritt 0.34.0 live abgeschlossen, Lodge.Folktails im ebenen Bauprojekt.
Gedrehter Wohnhausablauf mit zwei neuen Wegen, Grundriss-Negativkontrolle,
realem Bau-/Fertigzugang, Bestandsanschlüssen und Replay bestanden.
Keine neue Vier-Drehungs-Serie und keine Lockerung der Schutzgrenzen.
[Nachweis](docs/lodge-project-pilot.md).
Bei nächstem ohnehin nötigen MCP-Gate Wohnhaus-Drehung 3 in TemplateEvidence
aufnehmen; aktuelles ausgeliefertes Profil beschreibt den Historienstand vor
dieser Abnahme. Keine Neuinstallation allein für diese Metadaten.

Aktiver Schritt abgeschlossen: 0.33.1 mit vier gemeinsamen
sequenziellen Vertikalprojektplätzen und aktualisiertem MCP-Nachweisprofil.
Nach menschlichem Gate zwei Projekte in derselben Sitzung fertiggebaut,
Baustellensperre und Replay bestanden. Nächsten E-Ausbau an einer konkreten
Fähigkeitslücke ausrichten; keine weiteren identischen Drehungstests nötig.
[Nachweis](docs/vertical-sequential-pilot.md).

### Ausgangspunkt vor 0.33.1

Vertikales Lagerprojekt zusätzlich in Treppendrehung 0 / Lagerdrehung 2 live
bestanden auf unveränderter 0.33.0. Sieben fertige Objekte, Bau-/Fertigzugang,
Bestandsanschlüsse und Replay geprüft. Drehung 2 / Lagerdrehung 0 ebenfalls
mit sieben fertigen Objekten, Bau-/Fertigzugang, Bestandsanschlüssen und Replay
live bestanden. Drehung 1 / Lagerdrehung 3 ebenfalls bestanden; damit alle vier
Treppendrehungen des festen Lagerprojekts begrenzt belegt. Derzeitiges
MCP-Profil enthält weiter nur historischen Nachweis 3. Profilaktualisierung
als MCP-only-Änderung mit eigenem Gate bündeln, keine Modneuinstallation nur
für weitere Tests unveränderten Codes. Pro Sitzung nur ein Vertikalprojekt;
weiterer Start braucht eine frische Sitzung. Nächster sinnvoller Code-Schritt:
begrenzte sequenzielle Vertikalprojekte bei erhaltenem Replay-/Sitzungsschutz und
frischen Zugangsprüfungen, dazu aktualisiertes Nachweisprofil. Kein paralleler
Bau oder Lockerung der Baustellensperre. [Nachweis](docs/vertical-warehouse-pilot.md).

Strukturierte Projektumfangsabfrage nach menschlichem Gate live bestanden:
fünf Modi, Grenzen und historische Nachweise; keine Modversionsänderung oder
allgemeine Baufreigabe. [Nachweis](docs/building-capabilities.md).

Nutzerpriorität: B minimal vor E. 0.32.1 nach menschlichem Gate live bestanden:
neues Projekt/Einzelplatzierung bei unabhängiger Baustelle ohne neue Objekte
abgelehnt; nach deren Fertigstellung eigener Plattformablauf vollständig gebaut.
B im freigegebenen Ausschlussumfang geschlossen, allgemeiner Vorschau-Schutz
weiter ungelöst. Erster E-Schritt auf unveränderter 0.32.1 live bestanden:
Rotation 0/2 des kleinen ebenen Lagers, Baustellen- und Fertigzugang sowie
gemeinsamer Bestandsweg geprüft. [Nachweis](docs/building-rotation-pilot.md).
0.33.0 nach menschlichem Gate live bestanden: mittleres ebenes Folktails-Lager
mit Rotation 3 und sechs Grundrisszellen, Bau-/Fertigzugang und Bestandsanschlüsse.
[Nachweis](docs/medium-warehouse-pilot.md). Ergänzung auf unveränderter 0.33.0:
Rotation 1 mit drei tatsächlich neuen Bodenwegen, fertigem Lager, Baustellen-/
Fertigzugang und erhaltenen Vergleichsanschlüssen live bestanden. Drehungen 0/2
ebenfalls fertig und erreichbar mit erhaltenen Bestandsanschlüssen; alle vier
ebenen Drehungen dieser Vorlage begrenzt live belegt. Nächster Vorschlag:
Projektvorlagen, Modi, Grenzen und Nachweisniveau strukturiert im MCP ausweisen;
kein freier Vorlagen-/Höhenausbau.

Etappe D / 0.32.0 nach menschlichem Gate live bestanden: festes Lagerprojekt über
neue Treppe, drei Plattformen und zwei obere Wege, alle sieben Objekte fertig.
Tatsächlicher Baustellen-/fertiger Lagerzugang, beide oberen Wege, ausgewählte
Bestandsanschlüsse und gleiche Aktions-ID ohne neue Objekte geprüft.
Nächster Umfang E nur begrenzt: weitere explizite Eigenschaften statt freier
Höhenplanung. B-Schutzabnahme bleibt offen; D ist keine allgemeine Baufreigabe.

Kombinierter B/C-Pilot nach Skript-Gate in 0.31.4 live abgeschlossen: drei
sequenzielle Aufträge und separat lesbare tatsächliche Baustellenzugangszellen.
C mit zwei neuen Wegen und fertigen Lagern bestanden. B-Untersuchung erstmals
mit echter Builder-Positiv-/Negativbaseline; Vorschau-Schutzabnahme weiter offen.
Keine weitere Suchserie: aktuelle Reichweitendiagnose nicht zur Baufreigabe verwenden.

1. Etappe A bestanden: gemeinsamer Bericht, freie/gesperrte Vorschau und
   Vorschau-Eingang separat von tatsächlichen Zugängen live geprüft.
2. Etappe B: Untersuchung abgeschlossen mit technischer Grenze; Schutzabnahme offen.
   Baustellen-Erreichbarkeit unter geplanten Änderungen prüfen.
   0.31.0 live: Baseline/freie Kontrolle bestanden; negative Baustellenkontrolle
   nicht belegt. Begrenzte Detaildiagnose erneut freigegeben und in 0.31.1
   live geprüft. Acht Einzelbefunde und Rücknahme passen; Verbindung vom
   Zentralenursprung bereits in der erreichbaren Baseline überall false. Nächster
   Schritt: Start-/Zielgültigkeit und positiven Verbindungsbezug klären.
   0.31.2 installiert: Zentraleingang und alle acht Ziele auf tatsächlichem
   NavMesh, Start auch auf Bezirksweg. Dennoch alle Verbindungen false bei
   realem Builderzugang true. Positive Kontrolle gescheitert, Pilot gestoppt.
   Nach erneutem go öffentliche Signaturen geprüft: AreConnected vermutlich
   direkte Kante, echte Accessible-Pfadsuche an fertigem Lager positiv.
   Nächster Vorschlag: Nachbar-/Fernkontrolle und tatsächlichen Road-Spill mit
   Builderzugang vergleichen, danach erst gezielte Preview-Engpasskontrolle.
   Keine explizite öffentliche Preview-Pfadsuche gefunden, keine neue Suchserie
   0.31.2 wurde nicht separat committed.
   0.31.3 nach Skript-Gate live belegt: drei Nachbarkanten/echte Straßenpfade,
   vier erreichbare Spill-Ziele, Fern-Kantenwerte weiterhin false. Einzige
   Sperrvorschau ungültig, sieben Bezirksverluste, kein Baustellen-Reichweitenverlust.
   Diagnose bestanden, negative Baustellenkontrolle offen. Nächster Schritt:
   gültige Blockadekontrolle; Ist-Daten ausdrücklich kein Preview-Nachweis.
   Eine gültige Einzelpunkt-Lagervorschau belegt nur einen von vier Zugängen,
   ohne Verlust; kein Negativfall. Weitere Suche in dieser Fixture gestoppt.
   Nach erneutem go isolierten Einzelzugang mit echter Negativbaseline prüfen.
   Erhöhte Fixture tatsächlich getestet: Treppe entfernt, oberer Zugang getrennt,
   vier niedrigere Geländezugänge bleiben erreichbar. Kein Builder-Negativfall.
   Pilot gestoppt; nach erneutem go Isolation gegen sämtliche native Accesses
   und höchstes Nachbargelände herstellen, nicht nur gegen Treppenanschluss.
   Einzelner weiterer Wegabriss trennt Bezirkswegast nachweislich, nicht die vier
   Builder-Spill-Zugänge. Pilot gestoppt. Vor erneuter Fortsetzung konkreten Ansatz
   wählen: separate minimale Fixture oder Herkunftsdiagnose des Spill-Zugangs.
   Keine vollständige Bauzustandssimulation; allgemeine Baufreigabe bleibt gesperrt.
3. Aktuell Etappe C: kleiner Lagerpilot mit bestehendem Weganschluss bis zur
   tatsächlichen Fertigstellung live bestanden. Baustellenzugang und fertiger
   Straßenanschluss positiv; Nachbarlager weiterhin erreichbar, Distanz 10→11.
   Ergänzung 0.31.4: zwei neue fertige Anschlusswege, drei fertige Lager mit freien
   Eingängen/positiven Straßenverbindungen; Bestandskontrolle diesmal Distanz 11→11.
   C im begrenzten Lagerumfang bestanden, kein pauschaler Bestandsschutz.
   [Nachweis und Grenzen](docs/building-completion-pilot.md).
4. Etappe D: Gebäude mit vertikalem Anschluss; danach E: Breitenausbau.
   Abnahmekriterien: [Etappenplan](missionsplan.md).

## Technische Lücken

- Vollständiger Wegschutz für Bauvorhaben.
- Diagnose der tatsächlichen Ursachen von Produktionsstillstand.
- Belastbarkeit auf unterschiedlichen Fraktionen, Karten und Spielversionen.
- Persistenz oder Wiederaufnahme von Bauprojekten nach Sessionwechseln.

## Zurückgestellt

- Vier-Wege-Entwurf (vorgesehene 0.30.1): implementiert, nicht gebaut/live geprüft.
  Lokal wiederherstellbar als benannter Git-Stash
  `deferred-four-path-pilot-before-build-safety-stages`. Nicht Teil von Etappe A;
  erst nach C/D Nutzen prüfen und Konflikte vor Wiederaufnahme gezielt abgleichen.
- Größere Bauprojekte, freie 3-D-Suche und weitere Versorgungsdiagnosen erst nach
  belastbarem Wegschutz und Gebäudezugang.

Historische Baupläne, Kolonieziele und Spielstände sind bewusst kein Backlog mehr.

Aktuelle UI-Objektauswahl in 0.30.0 erledigt: Gebäude, Auswahlwechsel und keine
Auswahl live geprüft; Grenzen und Nachweis stehen in PROJECT_STATE.md.
