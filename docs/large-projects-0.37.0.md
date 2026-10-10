# Größere Bauvorhaben 0.37.0

## Stützenkorrektur live bestanden (2026-10-10)

Nach menschlichem Gate und `live bereit`: Bridge 0.37.0 erreichbar, neue Sitzung,
Pause bestätigt. Fünfteilige Folge Treppe (33,8,4), Plattform (33,9,4), Weg
(33,9,5), Plattform (33,10,4), Lager (33,10,5) vollständig fertiggestellt.
Weg und Lager in der Vorschau `deferred_native_validation`; ohne direkte
Trägerabhängigkeit wird der Weg `native_invalid` und der Plan ungeeignet.
Vorschauen restauriert. Identischer Start liefert denselben Auftrag ohne Bau.
Advance wartet auf tatsächliche Vorgängerfertigstellung und setzt höchstens
einen Nachfolger. Beide oberen Bauteile bestehen erst auf den real fertigen
Trägern die vollständige native Einzelprüfung. Keine Validatorumgehung.

Baustellenzugang von Treppe, beiden Plattformen und Lager separat erreichbar
beobachtet. Weg direkt fertig, daher kein behaupteter Bauphasennachweis für ihn.
Ledger am Ende `completed`, alle fünf Teile `finished`; Lagereingang frei und
zugänglich, native Distriktdistanz 33. Keine Betriebs-/Lagerkonfiguration behauptet.
Negative Bestandskontrolle: Lager auf vorhandenem Zugangsweg meldet sieben
verlorene Verbindungen, `blocked`, nicht pilotgeeignet; Vorschau restauriert.
Diese Probe ist zugleich geometrisch ungültig, kein isolierter Access-only-Test.
Separater gültiger Plattformplan vor erstem Advance gestoppt; geplante Entity
nachweislich nicht vorhanden. Gestopptes Ledger blockiert weitere große Projekte
in dieser Sitzung erwartungsgemäß; kein Versuch, es durch Advance wiederzubeleben.

Unterbrochener Aufruf vor Fortsetzung anhand Ledger/Spielprotokoll geprüft;
keine weitere Wirkung beobachtet. Ein normaler Teilstatus `unconfirmed` bei
Projekt `waiting` führte zu Aktionsprüfungs-Verwechslungen. Rein lesender Refresh
und eindeutige Zustandsnamen sind im Backlog vorgeschlagen. Ein Simulationslauf
endete `speed_changed`; Urheber unbekannt. Nach frischer Zustandsdiagnose pausiert,
kein blindes Replay. Materialengpass separat gelöst: zwei erreichbare echte
Kiefern markiert, Sägewerk bei ausreichenden Brettern pausiert. Kein Ertragsnachweis
allein aus Markierung oder laufendem Holzfällerjob abgeleitet.

Endstand: Tag 435, 19,40625 Uhr, Tempo 0, Baufolge fertig. Sägewerk bleibt pausiert.
Sieben konkrete Log-/Effizienzvorschläge mit Belegen und Abnahmekriterien im
[Backlog](../BACKLOG.md#mcp-log-und-aufrufeffizienz--konkrete-livebefunde-2026-10-10).
Keine Rohlogs gespeichert. Begrenzter Stützenpilot bestanden; keine pauschale
Abnahme aller Vorlagen, acht real gebauter Ebenen oder aller Kraftdrehungen.
Fachlicher Release-Nachweis damit abgeschlossen. Die folgenden Gate-Abschnitte
sind Historie. Der Fähigkeitenauskunft im gebauten Server fehlen noch die neuen
Livebelege (`not_live_proven`); sie bleibt konservativ. Aktualisierung dieser
Metadaten mit dem nächsten regulären Gate, kein zusätzlicher Build nur dafür.

## Zweiter Livepilot: Kraft und Ausführung bestanden, Stützenkorrektur am Gate

Nach erneutem menschlichem Gate am 2026-10-10: Kraftwellen-Vorschau funktioniert,
Ports und Link zum Sägewerk sichtbar, vollständig zurückgenommen. Ungültiger
benachbarter Wellenplatz korrekt abgewiesen. Kleine Folge Welle (26,6,3) → Lager
(32,8,4): Start ohne Baustelle; erster Advance genau eine Welle; weiterer Advance
bei unfertiger Welle bleibt waiting. Nach realer Fertigstellung genau ein Lager;
completed erst nach dessen Fertigstellung. Beide Baustellen builder-erreichbar,
fertiger Lagereingang frei, Distriktdistanz 29. Welle tatsächlich mit Sägewerk/Rad
verbunden; drei Netzmitglieder, 53 momentanes Leistungsangebot. Vorschauwert
rotationMatches=false verhinderte diese reale Verbindung nicht: kein allgemeines
Verbindungs-Verbot daraus ableiten.

Für Stützenpilot Plattform regulär für 100 Forschungspunkte freigeschaltet,
Sägewerk aktiviert und drei tote Birken auf (33,8..10,4) regulär entfernt.
Keine dieser Vorbereitungen ersetzt einen Höhenbau-Nachweis. Geplante Folge:
Treppe (33,8,4), Drehung 2 → Plattform (33,9,4) → Weg (33,9,5) → Plattform
(33,10,4) → Lager (33,10,5), Drehung 0. Alle Teile abhängig vom direkten Vorgänger.
Treppe/Plattformen gültig, alle oberen Zugänge in Vorschau verbunden; oberer Weg
und Lager dennoch nativ ungültig, solange die Träger nur Vorschauen sind.
Stützenfolge nicht gestartet, Pilot an dieser Lücke gestoppt.

Korrektur vorbereitet, noch nicht gebaut/getestet:
`placementState` unterscheidet native_valid, native_invalid und
deferred_native_validation. Letzteres bleibt ausdrücklich unbekannt, nicht gültig.
Nur wenn jede Fundamentzelle GroundOrStackable/Stackable verlangt, direkt darunter
ein ausdrücklich abhängiger geplanter BlockObject-Träger liegt und keine echte
oder geplante Kollision vorliegt, darf die native Prüfung aufgeschoben werden.
Keine Aussage, dass allein fehlende Träger die native Ablehnung verursachten.
Vor Platzierung ist der reale Vorgänger fertig und dieselbe vollständige native
Einzelprüfung muss bestehen; sonst Stopp ohne Auftrag. Keine Validatorumgehung.
Öffentliche Validierungs-API geprüft; keine Option für ein zukünftiges reales
Trägermodell gefunden, keine privaten Internals verwendet.

Vorbereitete Regressionen: fehlende/falsche/unvollständige Träger, falsche Höhe,
nicht stapelbar, Ground-only, Kollision, fehlende Abhängigkeit und fehlgeschlagene
frische Ausführungsprüfung trotz aufgeschobener Planung. Nächstes Gate und danach
zuerst die obige Stützenvorschau inklusive negativer Trägerkontrolle. Anschließend
Fertigstellung der Träger vor oberem Weg/Lager live belegen. Letzter Zustand:
Tag 430, 15 Uhr, Pause, keine offenen Baustellen. Kein Commit/Push.

## Erster Livepilot und Korrektur (2026-10-10)

Katalog nach Client-Neustart vollständig verfügbar. Spiel pausiert, keine offenen
Baustellen. Zwei Lagerpläne auf derselben oberen Fläche: gültig, angeschlossen,
identischer Schlüssel, null verlorene Bestandsverbindungen und Vorschau restauriert.
Negative Höhenkontrolle ohne Stütze: ungültig und nicht pilotgeeignet, ebenfalls
restauriert. Kraftleser: Sägewerk und Rad mit realer Portverbindung und zwei
Netzmitgliedern; nachts null Angebot/Nachfrage, kein Betriebsnachweis behauptet.

Erste Kraftwellen-Vorschau: backend_unavailable, keine Wiederholung. Lesende
Baustellenkontrolle weiterhin null. Kein Start/Advance und kein permanenter Bau.
Vermutete Fehlerstelle: Runtime-Ports sind bei Vorschauknoten nicht initialisiert.
Korrektur verwendet öffentliche TransputProviderSpec/TransputSpec und den öffentlichen
Transput-Konstruktor für getrennte Geometriebeschreibungen. Keine Registrierung,
Connect-/Disconnect-Aufrufe oder behaupteten realen Verbindungen an diesen Ports.
Öffentliche API-Metadaten geprüft; noch kein Build/Testlauf der Korrektur.
Erneutes menschliches Gate; danach zuerst genau die fehlgeschlagene Kraftvorschau
und deren Rücknahme prüfen, erst bei Erfolg den Baupilot fortsetzen.

## Ursprüngliche Gate-Übergabe

Stand 2026-10-10: nach menschlichem Gate/Bereitmeldung Backend gestartet und
Spielkontakt mit Bridge 0.37.0 bestätigt. Alle sechs Werkzeuge im Supervisor-Katalog,
aber noch nicht im Werkzeugangebot der laufenden Codex-Sitzung. Client-Neustart
für Katalogübernahme erforderlich, kein erneuter Build. Feature-spezifische
Liveabnahme **ausstehend**; noch keine Bauaufträge dieses Pilots.
Ein Release-Build, kein experimenteller Parallelzweig. Noch kein Commit/Push.

## Umfang

- Expliziter 3D-Bauplan mit 1–32 Bauteilen innerhalb 32×32 Rasterzellen und
  acht **belegten** Höhenebenen (`maxZ-minZ <= 7`, einschließlich Gebäudevolumen).
  Nicht acht Höhenwechsel und nicht acht zusätzliche Ebenen über dem Start.
- Jedes Bauteil nennt Vorlage, Position, Drehung 0–3 und nullbasierte
  `dependsOn`-Indizes. Zyklen, doppelte Bauteile und ungültige Verweise werden
  vor Spielzugriff abgewiesen. Stabile topologische Reihenfolge; bei mehreren
  verfügbaren Teilen zuerst der kleinere Index.
- Stützen, Treppen, Wege, entrancelose Gebäude und Kraftteile aus dem unterstützten
  nativen Katalog lassen sich kombinieren. Freie Koordinaten statt fest eingebauter
  Treppen-/Plattformfolge. Höchstens 64 belegte Zellen je Objekt.
- Gemeinsame native Vorschau, Prüfung jedes Baupräfixes und der fertigen Vorschau,
  Bestandszugänge, Rücknahmeprüfung über registrierte IDs, Warenbestand und Wege.
  Fehler bei der Vorschau-Rücknahme sperren weitere Projekte dieser Spielsitzung.
- Sequentielle Ausführung: höchstens ein Auftrag je `advance`; erst tatsächliche
  Fertigstellung und anwendbarer Zugang aller Vorgänger erlauben den nächsten.
  `inspect` liest ausschließlich das Ledger und aktualisiert keinen Baufortschritt.
- Öffentliche Kraft-API: Ports mit Rasterposition/Ziel/Richtung, geometrische
  Verbindungen zwischen geplanten Teilen und zu bestehenden Gebäuden. Separater
  Leser für echte Verbindungen, Netzmitglieder, Angebot/Nachfrage und Batterien.

Keine automatische Geländeroutensuche, kein neuer Distrikt-Lebenszyklus,
keine Gelände-Seitenanbauten, Spezialwerkzeugformen oder erfundenen zusätzlichen
Gebäudetüren. Der Katalog und die nativen Validatoren entscheiden über Vorlagen.
Freischaltung, Ressourcenlieferung, Besetzung und Produktion bleiben separate
Prüfungen. Keine pauschale Abnahme aller Gebäude/Fraktionen/Drehungen.

## Werkzeuge und Agentenablauf

| Werkzeug | Wirkung |
| --- | --- |
| `plan_large_project` | Pausierte gemeinsame Vorschau; `planKey`, Reihenfolge, Teile, Kraftgeometrie, Bestandsbericht und `pilotEligible` |
| `start_large_project` | Identischen Plan erneut prüfen und unter `actionId` registrieren; noch kein Bauauftrag |
| `advance_large_project` | Frisch lesen, bisherige Teile prüfen und höchstens einen neuen Auftrag erteilen |
| `inspect_large_project` | Gespeicherte Quittung: Zustand, aktueller Index in `order`, Teile-IDs und Teilezustände |
| `stop_large_project` | Weitere Aufträge dauerhaft stoppen; vorhandene Objekte bleiben |
| `inspect_power_network` | Frische native Kraftbeobachtung eines realen Objekts über dessen `id` |

Alle Aufrufe verlangen frische `session` und MCP-`reasoning`. Plan/Start erhalten
`districtId` und `steps`. Beispiel eines einzelnen Bauteils (synthetische Position):

```json
{"template":"Path","x":10,"y":10,"z":3,"rotation":0,"dependsOn":[]}
```

`start` erhält zusätzlich eine neue UUID `actionId`, den unveränderten `planKey`
und `mode: "development_pilot"`. Advance/Inspect/Stop verwenden nur Session und
Action-ID. Leser sind ohne Bauschalter verfügbar; Plan/Start/Advance/Stop benötigen
auf Server und Bridge `enableBuildingPlacement`. Planung benutzt temporäre
Vorschauen und ist deshalb nicht als read-only annotiert.

1. Status/Session und Pause bestätigen; Katalog, Freischaltung, Gebiet/Höhen und
   vorhandene Zugänge lesen. Stützen, Wege und Kraftstücke explizit einplanen.
2. Plan prüfen. `pilotEligible=true` ist nur der dokumentierte Entwicklungspilot,
   niemals allgemeine Bausicherheit. `regularExecutionAllowed` bleibt false;
   vollständiger Bauarbeiterzugang vor Auftrag bleibt unbekannt. Bekannte negative
   Zugangs-/Geometriekontrollen blockieren auch den Pilot.
3. Start mit gleichem Plan. Dann Advance: neue Teile-ID und Quittung zurücklesen.
   Keine zweite Action-ID als Retry verwenden. Gleiche ID mit verändertem Plan
   wird abgelehnt; identischer Start liefert lediglich den bestehenden Stand.
4. Bei `waiting`: kontrollierter separater Simulationslauf mit bestehendem
   `run_simulation_until`/`run_simulation_for`, anschließend Pause bestätigen und
   Advance. Kein Hintergrundbau und kein langes Echtzeit-Wartebudget im Projekt.
   Einen fertigen Auftrag nicht nur aus verstrichener Zeit ableiten.
5. `completed`: alle Teile fertig, vorhandener Gebäudeeingang und Wegeanschluss
   aktuell geprüft. Entrancelose Stützen/Wellen besitzen keine erfundene Tür.
   Bauarbeiterzugang wird an beobachteten Baustellen getrennt gelesen; direkt
   fertig erscheinende Objekte belegen keine vorherige Bauphase.
6. Kraft separat prüfen: `powerLinks` ist Geometrie, `connected` reale Verbindung,
   `network.powerSupply/powerDemand` momentane Leistung. Erst danach Betrieb prüfen.

Bei `stopped`/`unconfirmed` nur diagnostizieren. Auch ein schon vom Spiel
angenommener Auftrag kann bei Transportfehler unbestätigt sein. Keine Wiederholung,
kein Rollback, kein automatischer Abriss. Eine verschwundene/unerreichbare Baustelle
wird spätestens beim nächsten Advance nach Ablauf des fünfsekündigen
Bestätigungsfensters als unbestätigt behandelt; das Fenster löst keine Aufträge aus.

Das Ledger gilt für die geladene Spielsitzung, höchstens 128 Projekte. Ein offenes,
gestopptes oder fehlgeschlagenes Projekt blockiert neue große Projekte; menschliches
Diagnose-/Neuladeverfahren statt stiller Wiederfreigabe. 128 gecachte Vorschauen,
512 reale Kraftknoten/Netzmitglieder, 64 Ports je Knoten und 512 geometrische Links
begrenzen Aufwand. Antwortlimit dieser Routen 1 MiB, übrige normale Routen
unverändert 128 KiB. Grenzüberschreitung wird nicht als vollständiger Bericht ausgegeben.

## Implementierung und API-Grundlage

Bestehende Schichten: Bridge.Core-Request und Ledger, native Spielservice-Schicht,
NativeClient-Vertragsprüfung, sechs MCP-Werkzeuge. Keine neue Laufzeitabhängigkeit.
Öffentliche API-Metadaten der lokal installierten Spielassemblies geprüft:
`MechanicalNode.Transputs/Actuals/Graph`, `Transput.Coordinates/Target/Direction`,
`Faces/RotationMatches`, `MechanicalGraph.Nodes/PowerSupply/PowerDemand`.
Keine private Reflection und keine Manipulation der mechanischen Netze.
Normale `PreviewFactory`-/Placer-Services und bestehender RoadProtection-Bericht.
Das neue Ledger verändert die alten Projektquittungen nicht.

## Abnahmeplan und Stoppkriterien

Vorbereitete automatisierte Tests: 32/33 Teile, acht/neun Ebenen, Grenzen,
Zyklen/Referenzen, stabile Reihenfolge, strikte Requests, Pause-/Aktionsgate-Verträge,
Fertigstellungswarten, ein Auftrag je Advance, kein Replay nach Ausnahme,
fehlende/falsche Objekte, Stopp, falsche Session, vorzeitiges completed,
Kraft-Netzmitgliedschaft und reale HTTP-Routenbindung. Bestehende stdio-Katalogtests
um Namen und Schreibannotationen erweitert. Noch nicht ausgeführt.

Nach erfolgreichem menschlichem Skript und `live bereit`:

1. Backend starten, Version 0.37.0 und frische Sitzung bestätigen.
2. Zwei identische kleine Pläne müssen denselben Schlüssel liefern; Start darf
   keine Entität erzeugen. Kleine Folge aus Stütze, Höhenanschluss und Gebäude
   über zwei Ebenen ausführen. Advance während Bau darf keinen Nachfolger setzen;
   nach Fertigstellung genau einen. Bauzugang und fertigen Eingang getrennt lesen.
3. Negative Geometrie, fehlende Stütze und gefährdeter Bestandszugang müssen
   Ablehnung/fehlende Pilot-Eignung ergeben; kein permanenter Vorschauzustand.
4. Wenige Kraftteile zu einem vorhandenen Netz: passende und unpassende Drehung
   geometrisch unterscheiden, positive Variante bauen und echte Ports/Netzleistung
   nachweisen. Kein Produktionsnachweis allein aus Netzmitgliedschaft.
5. Ein direkt fertiger Weg sowie ein entranceloser Stütz-/Kraftfall; Stop und
   identischer wiederholter Start erzeugen keine weiteren Aufträge.

Beim ersten fehlerhaften Kontrollfall nicht skalieren, sondern diagnostizieren.
32 Teile/acht Ebenen zunächst Grenztests, keine große Koloniebauserie als Ersatz
für einen kleinen aussagekräftigen Pilot. Erst danach Doku-Abnahme, Commit und Push.
