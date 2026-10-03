# Backlog

## Priorität

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
