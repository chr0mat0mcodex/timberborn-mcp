# Native MCP-Werkzeuge

Bezirksweite Bauarbeitszeit **begrenzt live abgenommen**: Zentrum und
Bauhütten gemeinsam prüfen. Aktiver Job oder besetzte unpausierte Bauhütte in
Arbeitszeit verhindert gemeinsamen Feierabend; unbekannte Belege bleiben null.
Einzelner Feierabend am Zentrum ist keine Baupause im ganzen Bezirk.
[Vertrag, Grenzen und Pilot](operating-readiness.md).



Arbeitszeitdiagnose erweitert, **Gate und Livepilot bestanden**: Feierabend am beobachteten
Arbeitsplatz wird konkret benannt. Frische Baudiagnose nutzt den Schichtstatus
des zuständigen Bezirkszentrums; unbekannte Werte und andere Blockaden bleiben
getrennt. Kein automatisches Verlängern von Laufbudgets.
[Vertrag und Pilot](operating-readiness.md).


## Automatische Betriebsdiagnose — begrenzt live abgenommen

| Aufruf/Feld | Bedeutung |
| --- | --- |
| `inspect_operating_readiness_auto` | Lesende automatische Auswahl; `session` und `reasoning` erforderlich. Optional `districtId`, `buildingIds`, `materials`. |
| `selection` | Belegter Bezirk/Zentrum, gewählte Produzenten, Suchabdeckung, unbekannte IDs und ausgelassene Güter/Kosten. |
| `diagnosis` | Gemeinsamer Betriebsbericht für ausgewählte Gebäude; null bedeutet keine entsprechende Beobachtung. |
| `operatingPreflight` | Optionales Wurzelfeld der Startantwort, auch kompakt; Liste der vor internen Starts erhobenen Berichte. Kein Ersatz für den Startbeleg. |

Strukturstart/-lauf/-reparatur immer mit Vorprüfung; Simulations-for ab sechs
Spielstunden, until immer. Status und Cancel ohne Vorprüfung. Materialbefunde
sind beratend. Diagnoseausfall/Zeitbudget ausdrücklich auswerten; bereits
gesendete Aufträge nicht erneut senden. [Parameter, Grenzen und Pilot](automatic-operating-readiness.md).


Automatische Bauzentrum-/Produzentenauswahl und Vorprüfung vor Strukturstarts bzw. längeren Simulationen sind nach Gate sowie Simulations- und Baupilot begrenzt live abgenommen. Befunde bleiben beratend, Materialmangel sperrt den Start nicht. [Vertrag und Livebelege](automatic-operating-readiness.md).

Gemeinsame Betriebsdiagnose vor längeren Bau-/Simulationsläufen: `inspect_operating_readiness` bündelt ausgewähltes Bauzentrum, Überflutung, Personal und Materialproduzenten mit priorisierten nächsten Schritten. Gate und gezielter Livevergleich bestanden; kein Produktionsnachweis. [Aufruf und Grenzen](operating-readiness.md).

## Aktueller Nachweisumfang

Stand: **0.74.0 mit Journal-Korrekturen**, abgeschlossen in `680fd00` und `c09750f`.
Materialwarten, Bestandsverlust, isolierte unerreichbare Baustelle und Fertigbau
sind getrennt live geprüft. `repairEligible` bezeichnet die technische Möglichkeit;
`recoveryAdvice` empfiehlt den nächsten Schritt anhand des frischen Befunds.
Unbekannte Identität ist weiterhin keine live geschlossene Nachweisgrenze.

Forstbelege überstehen Backend-Neustart und Client-Neuverbindung ohne erneute
Spielmutation. Ein wiedergegebener Beleg ist historisch; aktuellen Zustand separat
lesen. Supervisor-Skriptänderungen erfordern eine neue Client-Verbindung.
Windows-Dateifehler und unterbrochene Aufträge wurden synthetisch im Gate geprüft.
Ältere Versionsabschnitte unten sind historische Teilabnahmen.

## Supervisor-Dateiaustausch (Gate und Lifecycle live bestätigt)

Die Lifecycle-Statusdatei verwendet begrenzte, bytegeprüfte Austauschversuche
bei ausgewählten Windows-Fehlern. Keine Wiederholung von Spielaufrufen oder
Backend-Starts. Fehler beim Statusspeichern sind keine Stoppbestätigung und
keine Freigabe fürs Bauen. Details stehen in `docs/codex-mcp-supervisor.md` im Repository.

## Forstjournal-Korrektur nach 0.74 (gezielt live bestätigt)

Forstmarkierung und Forstschutz verwenden denselben bytegeprüften
Dateiaustausch wie Baujournale: höchstens vier Versuche bei ausgewählten Windows-
Dateisperr-/Austauschfehlern, insgesamt höchstens 175 ms Wartezeit. Bereits identisch
publizierte Bytes gelten als gespeichert; fremde Änderungen oder unklare Lesezustände
brechen ab. Keine Spielaktion wird dadurch wiederholt. Ein Journalfehler ist keine
Freigabe für erneutes Markieren. Gate bestanden. Einzelbaum-Markieren/Schützen sowie gespeicherte Belege nach
Backend-Neustart und Client-Neuverbindung live bestätigt: keine erneute Mutation.
Supervisor-Skriptänderungen benötigen eine vollständige Client-Neuverbindung;
Backend-Stop/Start allein lädt den persistenten Python-Prozess nicht neu.
Dateifehler-Injektion und unterbrochene Aufträge bleiben synthetische Gate-Belege.

## 0.74 — Reparaturbedarf statt bloßer Reparaturmöglichkeit (gezielt live bestätigt)

Zahlenformatkorrektur nach erstem Diagnosefehler erneut durch Gate und live
bestätigt. Materialwarten empfiehlt Versorgung prüfen; zwei gezielt erzeugte
Bestandsverluste empfehlen Reparaturplanung. Nach Wegwiederherstellung und
Plattform-Fertigbau empfiehlt die Diagnose Abschluss trotz altem Zeitlimit.
Compact/full und Watch konsistent, beide Journale freigegeben, Endpause bestätigt.
Belegte Negativfälle: Bestandsweg/Gebäudeeingang und isolierte Baustelle mit
buildersReachable=false, construction_unreachable, ohne unfertige Stützen oder
Bestandsverluste. Compact/full und Watch empfehlen dafür Zugangsreparatur.
Unbekannte Identität bleibt live offen; bekannter Abriss ist keine unbekannte
Identität. Auch pausierte Legacy-Projekte bestätigen Objekte automatisch.
Werkzeugfehler ist kein Zugangsbefund und keine Freigabe zum erneuten Bauauftrag.

repairEligible bleibt die native technische Möglichkeit, keine Empfehlung.
Frische Diagnose liefert recoveryAdvice in compact/full und unter watch.diagnosis:
accessRepairState ist observed_access_problem, unknown,
deferred_until_support_finished oder no_observed_access_problem. Fehlende
Identitäts-/Zugangsbelege bleiben unknown; unfertige Stützen erfordern zuerst
Fertigbau und erneute Beobachtung. no_observed_access_problem ist keine allgemeine
Zugangsgarantie. Bestandsverluste haben Vorrang vor Materialwarten.

nextStep empfiehlt bei reinem Materialwarten check_material_supply, bei Stützwarten
wait_for_support_completion, bei Baupause review_construction_pause und sonst
check_workers_and_work_progress bzw. inspect_construction_inactivity. Fehlender
Zugang empfiehlt nur bei repairEligible review_access_repair_plan, andernfalls
inspect_access_repair_conditions. parts[].nextStep erhält den einzelnen Befund,
accessProblemPartIndices/unknownPartIndices identifizieren betroffene Teile.
Freigegebene Projekte empfehlen keinen Neubau, laufendes Tempo erst Pausenklärung.
Keine zusätzlichen Spielabfragen, Aufträge oder Änderung der nativen Freigabe.

## 0.73 — frischer Fertigbaustand und Parameterdiagnose (gezielt live bestätigt)

Bauchargenfehler enthalten zusätzlich `failure.exceptionKind` und `failure.hResult`
(bei älteren Belegen null). Dies sind Fehlerkategorie und numerischer Exception-Code,
keine Freigabe zum Wiederholen; `stage`, `execution` und `recoveryAction` weiter beachten.
Journal-Dateiaustausch darf intern höchstens vier Versuche bei ausgewählten Windows-
Dateisperr-/Austauschfehlern machen (insgesamt 175 ms Wartezeit), ausschließlich nach
Bytevergleich unveränderter Dateien. Das wiederholt keine Spielaufträge; unbekannte
oder veränderte Dateien brechen ab. Bereits vollständig publizierte Bytes gelten als Erfolg.

Livebeleg: Einzelplattform nach historischem Ein-Stunden-Zeitlimit später 1/1 fertig,
compact/full und watch-Diagnose konsistent; null Bestandsverluste, regulär freigegeben.
Kein allgemeiner Betriebsnachweis. repairEligible allein bedeutet nicht, dass bei
Materialwarten eine Zugangsreparatur nötig ist; konkreten reason lesen.

Lesende invalid_argument-Fehler verwenden execution=not_applicable. Hinweise nennen
bei generischen Fehlern fehlende Pflichtfelder, falsche Typen oder gültige
Ganzzahlbereiche (limit: 1..32; Karten-depth: 1..4). Spezielle Hinweise haben Vorrang.
Nur Parameter korrigieren; unconfirmed-Schreibaufträge weiterhin niemals wiederholen.

Frische Projektdiagnosen liefern currentConstruction in compact und full:
state=finished_accessible nur bei sämtlichen beobachteten fertigen, zugänglichen
Teilen; existingAccess separat prüfen. lost_or_unknown ist kein Bestandsschutzbeleg.
Historisches state=blocked und game_time_limit im Journal bleiben bestehen.
inspect_large_project ohne diagnose=true liefert weiterhin nur Journalwissen.
review_close_finished_project empfiehlt die Prüfung der Freigabe; bei
inspect_release_conditions die aktuellen Freigabebedingungen lesen. Kein Neubau
wegen eines alten Zeitlimits. Fertigbau ist kein Betriebs-/Versorgungsnachweis.

## 0.72 — Laufzeitbelege im begrenzten Livepilot bestätigt

Bei Recovery `manuallyPaused` und `unfinishedSupportIndices` lesen.
`construction_paused` bedeutet ausdrücklich manuelle Pause;
`waiting_for_support_completion` nennt geometrisch direkte, noch unfertige
Projektstützen bei inaktiver Baustelle. Ohne diesen Beleg bleibt
`construction_inactive_unknown`. Ein solcher Status fordert keinen Neubau.

`inspect_building.operations.workplace` und Personaländerungsbelege zeigen
`minimumWorkers`/`minimumWorkersSource`: zunächst null/unknown_public_api.
`observed_decrease_boundary` entsteht erst durch einen ohnehin angeforderten
nativen Absenkversuch, der den Sollwert nicht ändert. Kein garantierter
Vorlagenwert, keine automatischen Probeeingriffe; unconfirmed frisch nachlesen.
Eine reguläre Gebäudepause ist separat von Sollbesetzung null zu behandeln.

`inspect_simulation`: speedLocked/pauseSettled bleiben vor erstem beobachteten
Sperrereignis null (speedLockSource=not_observed). Danach Quelle
speed_lock_changed_event; settled verlangt Tempo null, keine Sperre und keinen
pendingSpeed. Sperrgrund nicht öffentlich verfügbar, keine Dialognamen raten.
Bei run_start_speed_locked Nutzerzustand klären; keine Entsperrung oder blinden
Startwiederholungen. Live bestätigt: Materialmangel unten, direkte unfertige
Stützen oben, manuelle Pause/Resume separat; Holzfäller-Grenze 1 nach
unbestätigtem Nullauftrag in Beleg und Gebäudeabruf erhalten. Esc-Menü offen:
speedLocked=true/pauseSettled=false; geschlossen und pausiert: false/true.
Vor erstem Ereignis beide null. Plattform-Umweg im sicheren Stand vollständig
abgenommen: 15/15 Teile fertig und zugänglich, beide Lagereingänge und native
Verbindung zum Distriktzentrum bestätigt. Untere Bestandswege trotz Überbau
weiter verbunden; lostConnections=0, Journal freigegeben, keine Baustellen.
Während der Bauphase konnte ein Lager fertig sein, aber sein Zugang noch fehlen.
Frische Recovery und native Zugänge lesen; historisches blocked/game_time_limit
allein ist nach weiteren Zeitfenstern keine aktuelle Fertigbauaussage.
Vor längeren Bauzeitfenstern Betriebszustand und Beschäftigungsfähigkeit
mitlesen: Im verworfenen Teststand fiel die Produktion durch Überflutung aus,
obwohl alle Wegverbindungen bestanden. Quelle, Netz und aktuelle Kraft sowie
Lieferung separat prüfen; vorhandene Windkraft garantiert keine Dauerleistung.
Unterer neuer Zielweg einer gedrehten Bestandstreppe bleibt separater Livefall.

## Historisch 0.71 — Treppenabgänge und Umwegplatzierung; Fertigbau damals offen

Bestandstreppen sind keine ebenen Vierwegeknoten. Oberer bestätigter Austritt
und seitliche Negativkontrolle live bestanden. Unterer gedrehter Nachbar braucht
native Bestätigung; dessen neuer Zielweg noch nicht separat live geprüft.
`landingPreviewCount` zählt obere/untere Zielwegproben zusammen (maximal acht),
`heightExits` weiterhin nur Höhenkanten.
Automatischer Umweg um ein mitgeplantes Lager: 17 Teile, native Eingänge und
Arbeitszugänge bestätigt, alle Teile platziert, keine Bestandsverluste.
Reale geprüfte Baustellenzugänge erreichbar; Fertigbau bleibt offen.
`deferred_native_validation` ist kein fertiger Träger. `construction_paused`
gegen tatsächliches `operations.pause.paused` prüfen: isOn=false kann bei
unfertigen Trägern ohne manuelle Pause auftreten. Fehlende Lieferung separat
von Zugang und Personal prüfen. Sollbesetzung null kann unbestätigt bleiben;
frisch lesen, nicht wiederholen. Bei run_start_speed_locked Sperre klären;
Pause allein ist keine Freigabe der Zeitsteuerung. Bestehende Baustellen mit
Projekt-ID lesen, keine wiederholten Bauaufträge.

## Historisch 0.70 — Registrierter Treppen-Zielweg, damalige Umweggrenze

Diagnose Version 3: `connectionGraph=instant_road_or_registered_landing`.
`nativeConnected` bleibt die Instant-Wegkante; `generalConnected` separat.
`landingCandidate` kennzeichnet den geometrisch abgeleiteten oberen Austritt
nur für gerade Folktails-Treppen. `previewLandingConnected=true` bestätigt
native Zielweg-/Stützenvorschau und Wiederherstellung, keine direkt ausgelesene
Treppenkante. false bedeutet abgelehnte Vorschau, null bedeutet ungeprüft
(Vorprüfung, vorhandene Quelle oder Budget). `landingPreviewCount` zählt die
höchstens acht Proben. Die Suche nimmt weiter einen echten Zielweg als Bauteil
auf; volle gemeinsame Vorschau und tatsächliche Zugänge bleiben erforderlich.
Fehler beim Aufräumen bleiben native Fehler mit Sitzungssperre; nicht erneut
blind planen. Zuerst bekannten Höhenfall prüfen; bei Fehler Diagnose und
manuelle Gegenprobe, anschließend stoppen. Live: fünf Suchzustände, zwei Wege, Lager fertig und zugänglich, Bestand erhalten. Erweiterter Umweg mit neuen Plattformen nativ abgelehnt; seitlichen Abgang von der Treppen-Hauptzelle nicht als verbunden annehmen. Kein allgemeiner Umwegnachweis.

## 0.69 — Bestandstreppen über nativen Weggraph, Höhenpilot fehlgeschlagen

`searchDiagnostics.version=2`, `connectionGraph=instant_road`:
`nativeConnected` ist jetzt die native Wegverbindung (AreConnectedRoadInstant).
`generalConnected` protokolliert die bisherige allgemeine Abfrage separat.
Nur die Wegverbindung darf einen Höhenkandidaten zulassen; ein negatives
allgemeines Ergebnis verwirft sie nicht. Außerhalb des Suchgebiets beide null.
Stichprobengrenzen unverändert; gemeinsame Vorschau und reale Zugänge weiter
prüfen. Bei erneutem Scheitern des bekannten Höhenfalls stoppen und beide
Graphwerte auswerten. MCP-Aufrufe seriell. Das Logfenster öffnet nach Laden
von selbst: sichtbar und angehängt live bestätigt. Gate bestanden; Höhenfall erneut am Suchbudget gescheitert. Beide Graphwerte am Treppenaustritt false, manuelle Gegenprobe gültig. Zielwegregistrierung und tatsächliche Treppen-Endpunkte klären. Weitere Baupiloten gestoppt.

## 0.68 — Zielpriorisierung und sichtbare Anschlüsse, Höhenpilot fehlgeschlagen

`plan_large_project_access` priorisiert Teilekosten plus konservative Zielkosten;
Umwege und Geometrievarianten bleiben möglich. `searchDiagnostics` bleibt auch
kompakt erhalten: `sources` zeigt Vorlage, Position, `districtConnected` und
`heightExitsExamined`; `heightExits` zeigt Start/Ziel, `insideRegion`,
`nativeConnected` und Versuche als `candidateAttempts`, `geometryBlockedAttempts`
oder `unsupportedAttempts`. Kandidaten sind keine native Bauzulassung.

Gesamtzähler gelten vollständig, Stichproben umfassen höchstens 32 Quellen
und 64 Übergänge; `sourcesTruncated`/`heightExitsTruncated` beachten.
Nicht geprüfte Anschlüsse bleiben unbekannt; außerhalb des Suchgebiets ist
`nativeConnected=null`. Fehlende Stichprobeneinträge beweisen keine Sperre.
`routes` zeigt pro Teil/Phase Suchaufwand, Restschätzung und gewählte Quelle.
Äußerer Abbruchgrund bleibt maßgeblich. Native Gesamtvorschau, Bauarbeiterzugang,
Fertigzugang und Bestandsanschlüsse weiterhin getrennt prüfen. Budgets unverändert.
Gate bestanden; Höhenpilot am Suchbudget gescheitert. Diagnose: 104 Höhenproben ohne bestätigten Übergang, manuelle Zwei-Wege-Gegenprobe gültig. Weitere Piloten gestoppt; native Anschlussprüfung klären.

## 0.67 — Gate bestanden, Höhenpilot nicht abgenommen

`plan_large_project_access` berücksichtigt Vorgängergeometrie und nativ
bestätigte Höhenanschlüsse vorhandener Wege/Treppen. Zielwege bleiben reguläre
Bauteile. Suchzustände unterscheiden Belegungsvarianten; Budgets und native
Schlussvorschau bleiben verbindlich. Keine Parameteränderung, kein neuer
Ausführungsmodus. [Details und Livepilot](releases/0.67.0.md).

## 0.66 — Kernpilot begrenzt bestanden

`precheck_building` und `plan_large_project_access`: Terraincheck anhand nativer
Zellbelegung; leere Vorlagenzellen sperren keine Uferposition. Native Vorschau
bleibt erforderlich. Direkte Simulationsstarts unterscheiden explizite
`run_start_*`-Ablehnungen als `not_executed`; generische Konflikte und
Transportverlust bleiben `unconfirmed`. [Codes und Pilot](releases/0.66.0.md).

## 0.65 — Kernpilot begrenzt bestanden

Zielscan: `backwater` bleibt auch kompakt erhalten; `closureRecommended=false`,
Kartenrandkontakt kein sicherer Abfluss. `target_badwater_observed` kennzeichnet
kontaminiertes Wasser über Zielhöhe. Sperrschnitt ist keine Bauempfehlung.
Betriebsdiagnose ergänzt `water` mit nativer Überflutung auch bei Pause,
Ansaugwasser/-kontamination und Rohrblockade. Kompakte Mehrfachdiagnose führt
dieselben Befunde; alte Bridges bleiben unbekannt statt angeblich trocken.
[Vertrag und Livebelege](releases/0.65.0.md).

## 0.64 — Gate und begrenzte Liveabnahme bestanden

Großprojektteile können `awaiting_access` melden: Identität bestätigt, fertiges
Teil noch ohne Zugang. Compact zählt sie in `confirmedPlaced` und separat in
`awaitingAccess`, nicht in `finishedAccessRecorded`. Diagnose behält positive
Identitätsbelege für Close nach später nachgewiesener Entfernung. Keine erneute
Platzierung, keine Freigabe aus bloßer Abwesenheit eines unbestätigten Auftrags.
`inspect_simulation` ergänzt optionale `activeRunId`/`pendingSpeed`; null bedeutet
kein aktiver Lauf/kein ausstehender Tempoauftrag. Alte Bridgeversionen liefern
diese Felder nicht. Neue Tempoübergabe und fachliche Grenzen:
[0.64-Vertrag, Livebelege und Nachweisgrenzen](releases/0.64.0.md).

## 0.63 / B15 — Gesamtauftrag, mit dokumentierter Grenze abgeschlossen

Alle Teile eines Plans werden in einem Game-Thread-Durchgang platziert; erst
anschließend folgen Zustands-/Problembewertung und Bauüberwachung. Keine
Material-, Zugangs- oder Fertigstellungsbarriere zwischen Aufträgen.
`dependsOn` ordnet nur die Einreichung. Teilfehler verhindern die übrigen
Platzierungsversuche nicht. Unbestätigte Wirkung wird nie wiederholt.

Großprojekte, Reparatur-/Räumgruppen, ebene und vertikale Piloten sowie neue
Gebäudechargen verwenden diese Semantik. `run_large_project` übernimmt außerdem
Tempo und begrenzte Bauzeit; einfache Startpiloten ändern kein Tempo.
`ordered` ist Auftragsbeleg, `completed` tatsächlicher Gesamtfertigbau.
Alte Schema-1-Chargenjournale bleiben kompatibel.
[Vertrag und Abnahme](construction-structures-0.63.md). Teilablehnung mitten im
Platzierungsdurchgang nur synthetisch im Gate geprüft; vom Nutzer als Grenze akzeptiert.


Zusätzlich `analysis.terraforming`: bedingte 3×3-Reservoir-Optionen, zwei Ebenen Absenkung, Nutzen und Blockaden kompakt, Zellbelege full. Gate und begrenzter Livepilot bestanden.

Strategischer Scan: `detail="compact"` als Standard für Bedrohung und Gegenmaßnahmen; `detail="full"` für komplette Scanbelege. Gate und Livevergleich bestanden.

Gefahrenbewertung einschließlich Restwasser bei ruhenden Quellen live bestätigt: Zielscan liefert `analysis.hazards` mit priorisiertem Restwasser, Schutzquerschnitten und Dach-/Umwegprüfung.

## 0.62 — strategischer Kartenscan, begrenzt live abgenommen

Erweiterung: `overviewOnly=true, detail="full"` ohne Zielkoordinaten liefert `rivers` mit
Flussabschnitten, geometrischen Verbindungen und nativen Quellen-/Wetterdaten.
`sectionOffset`/`linkOffset`/`sourceOffset` blättern 32/64/16 Einträge; Gesamtzahlen
und `more*` beachten. Stärke-/Kontaminationsänderungen gegenüber einem vorherigen
vollständigen Scan derselben Sitzung werden separat vom Wetterwechsel gemeldet.
Keine gemessene Fließrichtung, zukünftige Kontamination oder Ankunftszeit;
Aktivierungsregeln sind keine Garantie des tatsächlichen Ausstoßes.

`scan_strategic_map`: aktuelle Session und Zielzone x/y/z/width/height, optional
scanId, tilesPerCall (1–8, Standard 4), scenarioWaterLevel (absolute angenommene
Rasterhöhe) und objectOffset. Ganze Karte in 32×32-Kacheln bis 512×512×64,
ohne Pause; mit derselben Kennung fortsetzen. Erst vollständige Terrainabdeckung
liefert bis zu acht geometrische Zuflusskorridore, Geländeschwellen und konkrete
Sperrquerschnitte. Spezialobjekte seitenweise zu 32. Abdeckung und Zeitspanne lesen;
fertige Scans historisch, neue Kennung aktualisiert. Kein Wasserbauwerksmodell,
keine Strömungs-/Flutprognose, kein ausführbarer Bauplan. Zwei Scans je Client,
15 Minuten Leerlauf. Gate und Livepilot einschließlich Dürrewechsel bestanden;
Faulwasserflutwechsel noch nicht belegt. [Vertrag und Livebelege](strategic-map-0.62.md).

## 0.54.0 - Pflanzen- und Flaechenjournal begrenzt live abgenommen

Menschliches Gate und Abschluss-Livetest am 12. Oktober 2026 bestanden.
Pflanz-/Faellmarkierungen, reale Pflanzenwechsel und Baum-/Stumpfunterscheidung
mit ehrlicher Herkunft; Compact/Full, Filter und Cursor live bestaetigt.
Menschliche Eingriffe bleiben unknown. Neue Sitzung startet ohne Altbestands-
ereignisse; alte Session wird abgewiesen. Produktionsregression bestanden.
Grenzen: abgetastete Zustandswechsel, keine lueckenlose Ereignis-/Erntehistorie.
[Vertrag und Livebelege](spatial-journal-0.54.md).


0.53-Nacharbeiten live abgenommen: Bestaetigte Ablehnung einer veralteten
Pflanzvorschau meldet not_executed; danach frisch vorpruefen und entscheiden.
Andere Unsicherheit weiterhin nur lesend klaeren. Compact zaehlt passende
Markierungen und zeigt Beispiele nur bei weiteren Blockaden. Full bleibt komplett.


## 0.53.0 - Sammelpflanzung begrenzt live abgenommen

Menschliches Gate bestanden; nativer Pilot am 12. Oktober 2026 bestaetigt.
Bis acht Rechtecke je 8x8, gemeinsame native Vorpruefung mit Arbeitsreichweite,
Feuchte und Belegungsart. Zwei Rechtecke mit acht Zellen live geprueft:
zwei Markierungen gesetzt, vier passende belassen, fremdes Feld und Weg ausgeschlossen.
Falsche Pflanzenart/Hoehe und veraltete Vorschau abgewiesen; Zustand frisch gelesen.
Compact/Full gleiche Summen; Testmarkierungen wiederhergestellt, Endpause bestaetigt.
Nachweis gilt fuer Markierungen, nicht Wachstum; 512-Zellen-Grenze automatisiert
getestet, nicht als grosser Liveauftrag. Kleine 0.52-Nacharbeiten im Testgate geprueft.
Nacharbeiten zu Ablehnungsstatus und Compact ebenfalls live abgenommen.
Fruehere sporadische Gatefehler bleiben ohne bewiesene Ursache.
Kein allgemeiner Aquakultur- oder anderer Fraktionsnachweis.
[Vertrag und Livepilot](planting-batch-0.53.md).

## 0.52.0 begrenzt live abgenommen — Flächenräumung und Gruppenbau

Zusammenhängender Hintergrundablauf: Pflanzmarkierungen entfernen, bis 64
Vegetationsziele räumen, auf echte Fertigstellung warten, frisch gemeinsam planen
und bis acht Gebäude samt Wegen autonom bauen. Eine Fläche bis 8×8 auf einer Höhe;
Rohstoffstapel ignorieren und niemals löschen. Zeitlimits, keine automatische
Wiederholung nach Unsicherheit/Neustart. Compact/Full und lokale Statusabfrage.
Menschliches Gate bestanden; 0.52.0 am 12. Oktober 2026 begrenzt live abgenommen.
Räumbudget-Stopp, autonome Zweiergruppe, Pflanzmarkierungen und Bau auf
einer Rohstoffstapel-Zelle bestätigt; Testbauten entfernt, Endpause belegt.
[Vertrag und Abnahmepilot](cleared-building-groups-0.52.md).

## 0.51.0 — Gruppen-Rückwärtssuche begrenzt live abgenommen

`plan_region_buildings` erweitert Gruppen um begrenzte Rückwärtssuche und
`reservedAreas` (bis acht Ausschlussvolumen). `searchBudget` 1–32 native Vorschauen,
Standard 32; höchstens 256 Suchzustände einschließlich Kandidatenprüfungen.
`maxCandidates` 1–4 gilt pro Erweiterungszustand. Bewertung: mehr Wunschgebäude,
danach weniger neue Wege, danach weniger Zugangshilfen. Bestätigter bester
Teilplan bleibt bei Budgetende erhalten; keine globale Optimalitätsbehauptung.
Native Prüfung hält auch neue Wege/Treppen/Stützen aus den Volumen heraus.
Bestehende Wege bleiben nutzbar; Reservierungen gelten nur für den zurückgegebenen
Plan. Keine dauerhaften Siedlungsreservierungen. Compact/Full erhalten denselben
Plan und Suchstatus. Grenzen acht Gebäude/32 Teile/32×32/acht Höhen unverändert.
Nach menschlichem Gate live bestätigt: Rücksprung senkt einen Viererplan von
13 auf zehn Hilfsteile. Ziel-/Wegeausschluss und Höhenkontrolle bestanden.
Zweiergruppe samt Treppe/zwei Wegen autonom fertig: 3,65625 Spielstunden,
10,72 reale Sekunden; Zugänge und null Bestandsverluste bestätigt. Compact
61.686 → 11.944 JSON-Zeichen (80,6 % kleiner), gleicher Plan/Schlüssel/Suchstatus.
Fünf Testobjekte entfernt, keine Rohstoffe gelöscht, Endpause bestätigt.
[Vertrag und Abnahmepilot](group-search-0.51.md).

## 0.50.0 — effizientere Gruppenplanung begrenzt live abgenommen

Gemeinsame Flächendaten innerhalb eines Gruppenaufrufs, frische Pause-/Sessiongrenzen
und unveränderte native Gesamtprüfung. Frühfilter anhand nativer Belegungsverträglichkeit (BlockOccupations.Intersects);
unbekannte Profile/fehlende Geometrie bleiben nativ zu prüfen.
`readMetrics` nennt tatsächliche Reads, Cachetreffer und native Vorschauen.
Antworten nennen Belegungsarten: geplantes Gebäude mit Index/Vorlage, Bestandsgebäude,
Weg, Vegetation, Rohstoffstapel oder unbekanntes Objekt. Regionsbelegung ist Kontext,
kein erfundener Ablehnungsgrund. Compact bündelt Filterbefunde und beschränkt
Bestandsbeispiele mit total/truncated; `detail=full` enthält alle Belege.
Acht Gebäude/32 Teile bleiben. Nach menschlichem Gate: gemischte Dreiergruppe
mit vier gemeinsamen Wegen live fertig und zugänglich, Endpause bestätigt.
28 Reads eingespart, drei statt sechs native Vorschauen, Compact 71 % kürzer.
Testobjekte entfernt; Bestandswege und Rohstoffstapel erhalten.
[Vertrag und Pilot](efficient-group-planning.md).

## 0.49.0 — gemeinsame regionale Gebäudegruppe begrenzt live abgenommen

`plan_region_buildings` erweitert: exklusiv zu `template` eine Liste `templates`
mit 1–8 Gebäuden, Wiederholungen erlaubt. Suche einmal je unterschiedlicher Vorlage,
gemeinsame native Vorschau nach jeder Ergänzung; Zugangshilfen und Wege gemeinsam
berechnen. Maximal vier Kandidaten je Gebäude / 32 native Vorschauen insgesamt.
`complete/partial/none` bezeichnet Planung, nicht Bauabschluss. Unveränderte
`steps` und `planKey` an `run_large_project` übergeben; fehlende Gebäude stehen
in `unplaced`. Compact standardmäßig, `detail=full` mit allen Vorschau-Belegen.
Greedy in Eingabereihenfolge, keine optimale oder vollständige Standortsuche.
32 Gesamtteile, 32×32 und acht Höhen bleiben. Nach menschlicher Bereitmeldung
Bridge 0.49.0 bestätigt: drei Bänke plus vier gemeinsame Wegstücke in einem
Startauftrag fertig und zugänglich, 3,03125 Spielstunden / 8,85 reale Sekunden.
Teilplan- und belegte Einzelflächenprobe bestanden; Testbauten/Wege entfernt,
Bestandswege und drei zurückgewonnene Rohstoffstapel erhalten, Endpause bestätigt.
[Vertrag und Pilot](group-building-plans.md).

## 0.48: regionale Baupläne (begrenzt live abgenommen)

Für Suche mit nativ geprüften Bauplanalternativen plan_region_buildings nutzen:
Survey-Parameter plus maxCandidates=1–4 (Standard 3). Bauschalter erforderlich;
Vorschau verändert temporär Spielservices, beauftragt aber keinen Bau.
native_preview_passed liefert steps/planKey für genau einen ausgewählten
run_large_project-Auftrag. rejected und unchecked niemals als baubar behandeln.
Alten geometrischen Survey-planKey nicht verwenden. Ungeprüfte Höhen und
Suchgründe in survey.coverage beachten; null Diagnose bleibt unbekannt.
Alternativen nicht ungeprüft zusammenführen. Start validiert erneut, keine
Wiederholung unbestätigter Aufträge. Limits und Testgate bleiben bestehen.
[Vertrag, Grenzen und Pilot](regional-building-plans.md).

## 0.47: günstiger Status und Warten (begrenzt live abgenommen)

inspect_simulation_run/inspect_large_project standardmäßig ohne frische Diagnose.
waitSeconds=0 liefert sofort; 1–30 wartet auf Änderung/Blockade/Ende. Mit after
letztes watch.changeToken übergeben. Zeitfortschritt allein weckt nicht.
diagnose=true fordert einmal am Ende frische Diagnose an; detail=full allein tut
das nicht. Beim Projekt steht diese separat mit eigener Zeit unter watch.diagnosis.
watch.reason/changed/statusReads/diagnosticRequests und meta.observedAtUtc lesen;
wait_limit ist kein Laufabschluss, not_requested keine Entwarnung. Fehler nicht
blind wiederholen. Benutzerabbruch beendet nur das Warten, nicht den Spielauftrag.
Manuelle Bauprojekte werden durch Warten nicht fortgesetzt. Keine engen Pollschleifen.
[Vertrag, Grenzen und Pilot](lightweight-status-wait.md).

## 0.46: kompakte Bauprojektberichte (begrenzt live abgenommen)

Große Projektwerkzeuge liefern standardmäßig Compact, detail=full die nativen
Vollbelege. summary trennt attempted/confirmedPlaced/finishedAccessRecorded;
Unknowns und unconfirmed bleiben getrennt. Ledger ist keine frische Zugangsprüfung.
Für tatsächlichen Zustand diagnose_large_project nutzen; finishedAccessible ist
getrennt von bloß vorhandenen oder unzugänglichen fertigen Gebäuden.
attentionParts im Ledger enthält nicht fertige/blockierte IDs mit Originalindex;
alle fertigen IDs stehen in Full. Die frische Diagnose zählt erfolgreiche Teile
aggregiert; Problemteile bleiben in parts, summary.attentionPartIndices verweist
auf ihre Originalindizes. knownBlockedParts ist keine vollständige Diagnose.
Mehr Details nur mit inspect/diagnose und derselben actionId lesen, niemals
Aufträge erneut senden. Planungsdetails bei Bedarf schon mit detail=full anfordern.
nextStep ist ein Hinweis ohne Ausführung/Freigabe; Pause, Blockaden und mögliche
Aufgabe unbeauftragter Restteile vor Abschluss beachten. Alte Standortchargen bleiben
bei details=true. [Vertrag und Pilot](compact-project-reports.md).

## 0.45: Aktionsergebnisse und kompakte Läufe (begrenzt live abgenommen)

Native Fehler ergänzen error.execution: not_executed, unconfirmed oder
not_applicable; unterbrochene Bauchargen ledger_required. Kein automatisches Retry.
run_simulation_for/until, inspect_simulation_run und cancel_simulation_run:
optionales detail=compact|full, Standard compact. Kernstatus, Pause, Messwerte,
Blockaden und aktuelle Diagnosebefunde bleiben; vollständige Rohbelege in Full.
Mehr Details nur lesend mit derselben runId anfordern.
[Vertrag](action-results-and-run-reports.md).

## 0.44: Messmodus im Produktionslauf (begrenzt live abgenommen)

`run_simulation_for(..., buildingIds, measureProduction=true)` misst bis duration
statt zum Erstzyklusabschluss. Maximal acht Gebäude, keine Mischung mit Bestands-
oder Forschungszielen. `buildingProduction[].measurement` liefert native Zyklen,
rezeptabgeleitete Mengen/Raten und exklusive beobachtete Zeitanteile in Spielstunden.
`production_measurement_complete` ist Zeitfensterabschluss; Produktionsnachweis
bleibt separat. Historische Messung eingefroren, frische Engpassdiagnose weiterhin
separat datiert. Kein zusätzliches Werkzeug. [Vertrag](production-throughput.md).

## 0.43: gemeinsame Engpassdiagnose (begrenzt live abgenommen)

`inspect_building_operations(session, ids, diagnose=true, detail?)`: bis acht IDs,
Zutaten/Brennstoff, Quellen/Produktlager und native Wege, Personal/Kraft gemeinsam.
`summary.bottleneck` enthält Belege und Abdeckung, `assessment.findings` nächste
Schritte mit Bereich/Code/Schwere/Gut. Ohne diagnose bleibt die 32er-Übersicht.
`inspect_simulation_run` ergänzt für buildingIds aktuelle Betriebsdaten unter
currentBuildingOperations, bottleneckObservedGameHours und bottleneckState;
historische Produktionsbeweise bleiben getrennt. [Vertrag](production-bottlenecks.md).

## 0.42: gemeinsame Gebäudeeinrichtung (begrenzt live abgenommen)

`configure_buildings(session, jobId, items, proofHours?, proofSpeed?, proofMaxRealSeconds?)`:
1–8 eindeutige fertige Manufakturen, native Rezeptwahl, Sollpersonal, Pause;
alle expected-Werte erforderlich. Gemeinsame Vorprüfung, Teilfehler ohne Rollback.
`inspect_building_configuration(session, jobId)` liest die unveränderte Quittung.
Optionaler Produktionslauf verwendet den vorhandenen Lauf mit runId=jobId.
[Parameter, Fehler und Abnahmeplan](building-configuration-batch.md).

## 0.41: Erweiterter Produktionslauf (begrenzt live abgenommen)

`run_simulation_for` nimmt optional `buildingIds` (1–8 eindeutige IDs) entgegen,
exklusiv zu Güter-/Forschungs-/Holzzielen. `inspect_simulation_run` liefert
`buildingProduction`, `buildingProductionReached` und `buildingJournalCursor`.
Nur native Produktionsabschlüsse zählen. Konfigurationsänderungen und fehlende
Beobachtung invalidieren den Lauf; Zeitgrenzen und Pause bleiben im bisherigen
Controller. Keine zusätzlichen MCP-Werkzeuge.
[Vertrag und Nachweisgrenzen](building-production-proof.md).

## 0.40.0 — Änderungsjournal begrenzt live abgenommen

`inspect_recent_actions(session, limit=32, after?, kind=all, source=all)`:
letzte X Änderungen oder seit Cursor, maximal 64, 512 im sitzungsgebundenen
Speicher. Vorher/Nachher, Objekt/Position, Sequenz und Aufnahme-/Spielzeit.
Herkunft nur mcp/unknown; keine Behauptung menschlicher Urheberschaft.
Cursor und Erfassungslücken ausdrücklich lesen. [Vertrag](recent-actions-journal.md).

## 0.39.0 — begrenzt live abgenommen

- `find_buildings`: Filter template/finished/productionOnly/includePaths und
  vollständiger Ankerbereich x/y/z/width/height/depth; kompakt als Standard,
  geometry nur bei detail=full. Wege standardmäßig ausgeschlossen.
- `inspect_building_operations(ids, session, detail)`: 1–32 eindeutige IDs,
  ein nativer Beobachtungsrequest, Einzelstatus observed/not_found. Kompakte
  Betriebs-Summary; full ergänzt Inventare und Statustexte.
- `inspect_building_operation`: zusätzlich power.active für korrekte Einordnung
  inaktiver Verbraucher; gemeinsame Diagnose mit der Sammelabfrage.

[Vertrag und begrenzter Pilot](building-query-batch.md).

## 0.38.0 — Gate und begrenzter Livetest bestanden

- `inspect_building_settings`: zusätzlich aktuelle/erlaubte Rezepte mit
  Zutaten, Produkten, Zyklusdauer, Forschung und Brennstoff.
- `set_building_recipe(id, session, recipe, expectedRecipe)`: native Rezeptwahl
  fertiger Gebäude; bestehendes Settings-Gate, Konfliktprüfung und Rücklesung.
- `inspect_building_operation`: zusätzlich nativer Kraftzustand und gebündelte
  `assessment` mit Blockaden, Warnungen und unbekannten Prüfpunkten; ein Request,
  kein Produktionsnachweis aus Bereitschaft.

[Vertrag und begrenzter Pilot](recipe-operation-settings.md).

## Live abgenommen: gesamte Waldreichweite und Fällschutz

Folgeoptimierung live bestanden: interne 256er-Seiten und beim Schutz
Markierungs-Rücklesung statt zweitem Waldscan. MCP-Parameter unverändert; gemessen
32 statt 54 `nativeReads` für Übersicht, 40 statt 113 für Zweibaum-Schutz.

`inspect_forestry`: optional `fullRange=true`; dann genügen `session` und
`workBuildingId`. Vollständige Artenzählung über die native Reichweite auf allen
Höhen, explizite Abdeckung und Aufwand; regionale Altparameter bleiben kompatibel.
`protect_forestry`: `session`, `workBuildingId`, `template`, `actionId`, optional
`maxTrees` (1–512, Standard 256). Entfernt aktuelle Fällmarkierungen lebender Bäume
der gewählten Art. Bestehender Flächen-Aktionsschalter, persistenter Teilbeleg,
Rücklesung und lesendes Replay. Keine dauerhafte Regel, Fällung oder Simulation.
Menschliches Gate und begrenzter Livetest bestanden. [Nachweis und Grenzen](forestry-range-protection.md).

## 512-Zellen-Geometrie — begrenzter Großbau-Nachweis

`inspect_build_options` ergänzt `geometryCellCount` und `maxGeometryCells`.
`inspect_building_capabilities` übernimmt das tatsächlich gemeldete Limit;
fehlende neue Felder bleiben ältere Pakete. Vorprüfung/Objektlisten liefern bis
512 native Zellen, Großprojekte behalten 32 Teile/32×32/acht Höhen.
Kein neuer Werkzeugname. Hex nur als feste Einzelvorlage mit nativer Validierung.
DomedGarden samt Zugang live fertig; native Hex-Platzierung und weitere
Sondertests auf Nutzerentscheidung zurückgestellt. [Nachweis und Grenzen](large-building-geometry.md).

## Live abgenommen: Mehrfachziele für Simulationsläufe

`run_simulation_for`: optional `goodTargets=[{good,amount},…]` (1–8 eindeutige
Güter) und `stopAtScience`, jeweils ganzzahlig 1–1.000.000. Nicht zusammen mit
dem alten Holz-Ziel. Antwort ergänzt beobachtete Zielbestände, Forschung,
`productionTargetsReached` und frische `productionDiagnostics` mit Zeitstempel.
Alle Ziele gleichzeitig; automatische Pause. `production_time_limit` und
`production_observation_unavailable` unterscheiden Budget und Beobachtungslücke
von `production_targets_reached`. Kein neues Werkzeug und keine Änderung am
Zeitziel-Aufruf `run_simulation_until`. [Details/Gate](production-target-runs.md).

## Bauarbeitszugang mit Policy 1 (begrenzt live abgenommen)

`plan_large_project_access` sucht mit zwölf Zellen Rand und liefert zusätzlich
`blockedPart`, `phase`, `workCellCount`. Der verschachtelte Plan sowie
`plan_large_project` liefern `accessPolicyVersion=1` und pro Teil
`constructionWorkAccess` mit `required`, `state`, `candidateCount`,
`reachableCells`, `source`, `nativeConstructionProven=false`.
Status: `road_reachable`, `no_work_cell`, `no_connected_work_cell`,
`final_work_access_blocked` oder `helper_runtime_check` für Wege/Zugangshilfen.
Fehlende erforderliche Wegnachweise blockieren Planung und Auftrag. Laufzeitfehler
`construction_work_access_rejected` enthält denselben Bericht in der Diagnose.
Keine neuen Aufrufparameter; Teil-/Zeitbudgets bleiben unverändert. Eine erreichbare
geometrische Arbeitszelle ist keine native hypothetische Baustellenfreigabe.
[Umfang, Grenzen, Testgate](automatic-access-planning.md).

## Automatische Zugangsplanung (begrenzt live abgenommen)

`plan_large_project_access` (POST/Baugate, pausiert): explizite Gebäudeziele mit
`session`, `districtId`, `steps`; ergänzt Wege/Stützen/Folktails-Treppen und
Abhängigkeiten, danach gemeinsame native Vorschau. Antwort: `candidateFound`,
`reason`, vollständige `steps`, `plan` mit `planKey`, `assessment` und
`cleanupCandidateIndices`. Kein Bauauftrag, keine allgemeine Baustellenfreigabe.
[Grenzen, Ausführungsübergabe und Testfälle](automatic-access-planning.md).

## Projektwiederherstellung (begrenzter Livetest bestanden)

| Werkzeug | Art | Zweck |
| --- | --- | --- |
| `diagnose_large_project` | Lesen/GET | Frische Teilzustände, Baustellenmaterial, Zugänge und Fehlerbelege |
| `close_large_project` | Schreiben/POST, Baugate | Geklärtes terminales Ledger freigeben; Restteile aufgeben |
| `plan_large_project_repair` | Vorschau/POST, Baugate | Zusätzliche Wege/Treppen/Plattformen für `parentActionId` prüfen |
| `run_large_project_repair` | Schreiben/POST, Bau- und Tempogate | Eigene Reparaturcharge mit festem Budget; Originalaufträge nicht wiederholen |

Sessionbindung, frischer Zustandsabgleich, fremde Baustellen ausgeschlossen.
`closeOutcome` entscheidet über den Abschluss, nicht allein Transportstatus `ok`.
[Vertrag und Testfälle](project-recovery.md).

## Autonome Gesamtcharge (begrenzter Livetest bestanden)

`run_large_project` (POST, Bau- **und** Tempofreigabe): gleicher expliziter Plan
wie Plan/Start, zusätzlich `speed`, `maxGameHours`, `maxRealSeconds`. Alle
unabhängigen Aufträge pausiert setzen, danach serverseitig bis fertig begleiten;
explizite Abhängigkeiten automatisch nachziehen. `inspect_large_project` liest
die automatisch aktualisierte Batchquittung, `stop_large_project` stoppt und
fordert eine Pause an. Keine Advance-Aufrufe pro Teil. [Details](autonomous-construction-batches.md).

## 0.37.0 — begrenzte Bau-/Stützenpiloten live bestanden

| Werkzeug | Art | Zweck |
| --- | --- | --- |
| `plan_large_project` | Vorschau/POST | Expliziter Plan bis 32 Teile, 32×32 und acht belegte Höhen, Abhängigkeiten und Kraftgeometrie |
| `start_large_project` | Schreiben/POST | Ab 0.63 gesamten frisch validierten Plan sofort beauftragen |
| `advance_large_project` | Schreiben/POST | Gesamtprojekt beobachten; Start hat bereits alle Aufträge eingereicht |
| `inspect_large_project` | Lesen/GET | Gespeicherte Projektquittung, keine Fortschrittsaktualisierung |
| `stop_large_project` | Schreiben/POST | Folgeaufträge stoppen, Objekte behalten |
| `inspect_power_network` | Lesen/GET | Echte Ports, Netzmitglieder und Leistung des angegebenen Objekts |

Alle mit Sessionbindung und `reasoning`; vier Schreibwerkzeuge über den
Gebäudebauschalter, zwei Leser unabhängig davon. Kein automatischer Wiederholungs-
oder Hintergrundbau **im manuellen Altmodus dieser Tabelle**.
[Parameter, Grenzen und Statusbedeutung](large-projects-0.37.0.md).

## Cancel-Nacharbeit — Gate und begrenzter Livepilot bestanden

`cancel_simulation_run` darf während `inspect_simulation_run` oder
`inspect_large_project` mit `waitSeconds > 0` parallel passieren. Andere
konkurrierende Aufrufe bleiben gesperrt. Der Codex-Supervisor begrenzt
gültige Statuswartezeiten zusätzlich auf eine Sekunde. Externe Zustelllatenz
bleibt separat: im Livepilot 2,986 s bis Cancel-Antwort, dann Pause bestätigt.
Die beobachtete Serialisierung liegt vor dem Supervisor.
[Vertrag und Nachweisgrenzen](lightweight-status-wait.md).

## Abgenommene Grundlage

Live abgenommen: `timberborn_server_control` (`action: stop|start|status`)
kommt vom persistenten Codex-Vorschaltprozess, nicht aus der Spiel-Bridge.
Stop sperrt das Backend vor dem menschlichen Gate; Start erst nach `live bereit`.
[Einrichtung, Fehlerverhalten und Abnahme](codex-mcp-supervisor.md).

Letzter dokumentierter Abschluss: Bridge 0.72.0, begrenzt live abgenommen. Paket-/Abnahmestand: [PROJECT_STATE.md](../PROJECT_STATE.md).
Praktischer Ablauf: [Spielanleitung für Agenten](agent-playing-guide.md).
Der tatsächliche Werkzeugumfang und die Pflichtargumente ergeben sich aus
`tools/list` und den aktiven technischen Aktionsschaltern; keine feste Werkzeugzahl.
Historische Versionsabschnitte unten beschreiben frühere Nachweise und Grenzen.

## Gebündelte Abläufe

| Werkzeug | Zweck und Grenze |
| --- | --- |
| `inspect_colony_overview` | Bevölkerung, Betten, Personal, Bedürfnisse und ausgewählte Güter bündeln; keine vollständige Nahrungsbilanz. |
| `inspect_map_region` | Gelände einschließlich `soilIsMoist`; null bleibt unbekannt bzw. auf dieser Höhe nicht anwendbar. |
| `survey_region` | Pausierte Region bis 16×16: Hindernisse, Feuchte und freie Rechtecke. Optional `workBuildingId` für native Arbeitsreichweite; `planBuildings=false` spart Bauplanung bei Anbausuchen. `template` und `districtId` bleiben Pflicht. Keine Pflanz-/Ertragsgarantie. |
| `develop_building_project` | Einzelnen ausgewählten Bauplan prüfen, beauftragen und durch ein begrenztes Bauzeitfenster begleiten. |
| `advance_building_project` | Bereits gestartetes Projekt beobachten/fortführen, dasselbe Zeitbudget niemals automatisch verlängern. |
| `inspect_building_completion` | Tatsächliche Fertigstellung und Zugang lesen; Betrieb getrennt prüfen. |
| `start_building_batch` | 1–8 Gebäude, Basisfläche plus höchstens drei `additionalRegions`, jede bis 8×8. Automatischer Flächenwechsel nur vorwärts; kein Rücksprung. |
| `advance_building_batch` | Gemeinsame Standortwahl/Räumung und Strukturbeleg fortsetzen; `waitSeconds` 0–45, höchstens 64 Zustandsübergänge pro Aufruf. |
| `inspect_building_batch` | Gespeicherter Status (`checkpointOnly`), keine frische Spielbeobachtung. |
| `stop_building_batch` | Weitere Chargenaufträge dauerhaft stoppen; ein bereits laufendes Zeitfenster endet erst an seinem Budget. Für sofortigen Laufabbruch `cancel_simulation_run` verwenden. |

Chargen räumen bei Bedarf einmal pro Teilfläche passende natürliche Vegetation;
`maxClearTargets` gilt über alle Flächen zusammen (höchstens 64). Gesamtzeit:
`clearanceHours × Flächenzahl + constructionHours × Gebäudezahl ≤ 672 Stunden`.
Unbestätigte Eingriffe oder erschöpfte Räumbudgets stoppen die Charge.
Fehlendes Material ist keine Startsperre; Deckung und Erzeuger prüfen, Agent
entscheidet und verfolgt Lieferungen während des Baus. `regionIndex` und `previousRegions` zeigen Flächenfortschritt.
Gleiche `batchId` mit gleichen Parametern liefert Status, keine erneute Ausführung;
geänderte Parameter werden abgewiesen. Ohne `advance` kein Hintergrunddispatcher,
nur ein bereits gestarteter begrenzter Spielzeitlauf kann weiterlaufen.
`finished_accessible` bestätigt Fertigstellung, Zugang und Pause, keinen Betrieb.

[Regionalsuche](regional-survey.md) · [Bauchargen](building-batch-workflow.md) ·
[Bauprüfbericht](build-assessment.md) · [Wegschutz](road-protection.md)

## Leser

- `timberborn_status`
- `inspect_colony`
- `inspect_goods`
- `inspect_production_graph` — [Definitionsgraph, Quellen und Grenzen](production-dependency-graph.md)
- `inspect_needs`
- `inspect_beaver_needs`
- `inspect_building_operation`
- `inspect_building_access`
- `inspect_road_connection`
- `inspect_work_range`
- `inspect_good_history`
- `inspect_alerts`
- `inspect_alert_targets`
- `inspect_map_region`
- `find_buildings`
- `inspect_build_catalog`
- `precheck_build_site`
- `inspect_building`
- `inspect_simulation_run` — Status eines Zeitlaufs
- `inspect_simulation`
- `inspect_workforce`
- `inspect_building_priority`
- `inspect_construction`
- `inspect_area_types`
- `inspect_areas`
- `inspect_removal_targets`
- `inspect_build_options`
- `inspect_building_capabilities`
- `plan_building_project`
- `inspect_building_project`
- `inspect_vertical_stair_pilot`
- `inspect_path_district`
- `precheck_building`
- `inspect_building_settings`
- `inspect_research`
- `inspect_agent_log`

  0.56 begrenzt live abgenommen: `projectId` und `runId` als kombinierte Filter;
  `correlations` trennt angefragte und im Ergebnis gemeldete Bezüge. Dieselben
  Kennungen erscheinen im Ingame-Fenster MCP-Aufrufe. Keine Existenz-/Wirkungsgarantie.
  [Umfang und Gate](agent-log-correlation-0.56.md).

  0.55 begrenzt live abgenommen: `limit` 1–32, exakte `tool`-/`state`-Filter und `cursor`.
  `nextCursor` blättert bis zu 128 gespeicherte Aufrufe in Startreihenfolge;
  `gapDetected` und `unavailableBeforeCursor` melden verdrängte ältere Aufrufe
  vor Filtern. Kein eingefrorener Status-Snapshot und keine vollständige
  Aktionshistorie. [Vertrag und Gate](agent-log-pagination-0.55.md).

## Aktionswerkzeuge (technische Aktionsschalter)

- `set_workplace_staffing`
- `run_simulation_for`, `run_simulation_until`, `cancel_simulation_run`
- `set_simulation_speed`
- `validate_build_site`
- `place_path`
- `place_lodge`
- `set_building_priority`
- `set_area`
- `demolish_building`
- `remove_planted`
- `remove_vegetation`
- `remove_debris`
- `validate_building`
- `validate_building_project`
- `execute_building_project_pilot`
- `execute_vertical_stair_pilot`
- `place_building`
- `set_building_paused`
- `set_storage_good`
- `set_storage_mode`
- `set_farm_priority`
- `set_farm_crop`
- `unlock_building`

## Auswahl und Ausführung

- Für neue Gebäude mit Anschlussweg: inspect_building_capabilities,
  inspect_build_options, precheck_building/plan_building_project,
  validate_building_project und execute_building_project_pilot. Den vollständigen
  Ablauf einschließlich tatsächlichem Bau-/Fertigzugang beschreibt die Spielanleitung.
  validate_building prüft einzelne Objekte; place_building bleibt ohne safe-Wegschutz
  gesperrt. inspect_build_catalog/precheck_build_site sowie validate_build_site/
  place_path/place_lodge sind erhaltene, begrenzte frühe Pilotwerkzeuge.
- Validierung mit Vorschau zählt nicht als rein lesend und benötigt den technischen Bau-Aktionsschalter. Im beauftragten Testumfang ist keine zusätzliche Rückfrage nötig.
- Aktionsfreigaben werden in Mod und MCP-Prozess geprüft; [vollständige Tabelle](native-bridge-install.md).
- reasoning ist Pflicht für alle MCP-Werkzeugaufrufe, einschließlich Leseabfragen,
  Fake und Legacy: kurze für den Spieler lesbare Absicht, keine internen
  Gedankengänge. String mit 1–600 Zeichen, nicht nur Leerraum. Fehlende oder
  ungültige Begründung wird vor Backend-/Logzugriff als InvalidParams abgewiesen.
  Der bestehende Feldname bleibt reasoning; kein neues reason-Alias.
- Eingabeschemata, verlangte Session-/Erwartungswerte und Grenzen liefert MCP tools/list.
  Kein generischer HTTP-Aufruf und kein automatischer Backendwechsel.
- Ergebnis separat prüfen; applied ist kein Beleg für abgeschlossenen Bau oder Versorgung.
  [Fachliche Fehlercodes](bridge-errors.md).

Neue Güter-/Statusverträge: [Details und Grenzen](economy-observations.md).

Weg-, Reichweiten- und Güterhistorien sind über eigene strukturierte Werkzeuge verfügbar.
`roadProtection` ergänzt ab 0.23.2 Weg-/Baustellen-Prüfpunktzahlen und `affected.kind`; bei `road_cell` bezeichnet `entrance` die Wegkoordinate. Diese Diagnose erweitert keine Baufreigabe.

## Bauplan-Kandidatensuche (0.24.0)

`plan_building_project` ist ein zusätzlicher Leser. Eingaben: template, districtId,
session, x/y/z, width/height (1..8), rotation (0..3). Bis zu vier Vorschläge mit
Gebäudeursprung, Eingang, Anschlussziel und Wegzellen in Anschlussreihenfolge.
Alle Optionen executable=false; noch keine gemeinsame Spielvalidierung oder
Ausführung. Suchabdeckung und Abbruchgrund ausdrücklich lesen.
[Umfang, Grenzen und Pilot](building-site-search-plan.md).

`validate_building_project` (0.24.1) benötigt die Plan-Suchparameter plus optionIndex
und planKey. Bau-Opt-in erforderlich, maximal acht neue Wege und 16 Prüfungen/Sitzung.
Temporäre gemeinsame Vorschau, kein Bauauftrag; executable bleibt false. Ergebnisse
und Grenzen stehen im [Bauplan-Vertrag](building-site-search-plan.md).

## Generischer ebener Bauprojekt-Pilot (0.35.0)

`execute_building_project_pilot` setzt ein geeignetes Gebäude aus dem aktiven
Spielkatalog und höchstens vier neue ebene Wege. Vollständige Spielgeometrie und
anschließbarer öffentlicher Eingang entscheiden, keine Gebäudenamensliste.
Ältere Bridges behalten ihren versionierten festen Pilotumfang.
Benötigt Suchparameter, optionIndex, planKey, session, actionId und
`mode=development_pilot`. Bau-Opt-in und Pause sind Pflicht. Bis 0.31.3 ein Auftrag
je Sitzung; ab 0.31.4 zunächst vier Aufträge, ab 0.35.2 Ledger-Kapazität 1024 für ebene und vertikale Projekte. Weiterhin sequenziell: neue ID nur nach
completed und tatsächlicher Fertigstellung bisheriger Baustellen. Unabhängige
offene Baustellen verhindern den Start. stopped/unconfirmed sperren weitere Projekte. Alte ID+Parameter lesen
nur das gespeicherte Ergebnis, geänderte Parameter werden abgewiesen. Bis 0.62 war completed kein Fertigbau; ab 0.63 ist ordered der Auftragsbeleg
und completed die beobachtete Fertigstellung.
`inspect_building_project` liest session/actionId ohne Schreibfreigabe.

running/completed/stopped/unconfirmed und Schritt-IDs erlauben gezielten Abgleich.
Ab 0.63 bestätigt ordered den Auftrag und completed die Fertigstellung;
Betrieb/Produktion sind weiterhin separat nachzuweisen. Bekannte Wegverluste bleiben verboten; nur der unbelegte Bauphasen-
Vorabnachweis darf im freigegebenen Pilot offenbleiben. Teilergebnisse bleiben stehen.
[Aktueller Umfang und Live-Abnahme](generic-building-project.md).

Ab 0.31.4 ergänzt `inspect_building_access` die tatsächlichen Baustellen-Zugangszellen:
constructionAccess.state=observed/unavailable/not_construction, cells (maximal 64),
expanded. unavailable liefert keine Teilmenge als vollständige Liste; erweiterte
Baustellen sind nicht unterstützt. Unabhängig von anderen Baustellen abfragbar.
Zellen sind aktuelle gecachte Spielzugänge, kein Preview- oder Einzelpunkt-
Erreichbarkeitsnachweis. buildersReachable bleibt die getrennte tatsächliche Abfrage.

## Holzwerkzeuge und Chargen 0.35.5 — live abgenommen

`inspect_forestry(session, workBuildingId, x, y, z, width, height, depth=1,
consumerIds?)` liefert höchstens 16 geeignete Bäume in einer Region bis 8×8×4,
Gesamtzahl/Trunkierung, Ausschlussgründe, freie/gesamte Holzbestände und optional
Inventar-/Personalzustände von bis acht Gebäuden. Nur pausiert. Keine Ertragsrate.
Baumstümpfe (`cutYieldRemoved=true`) und fehlende/unbekannte Holzerträge werden
ausgeschlossen; ausgewachsen allein ist kein Nachweis eines fällbaren Baums.

`mark_forestry(region, actionId, treeIds)` setzt nach frischer Prüfung reguläre
Fällmarkierungen für 1–16 gewählte IDs. Derselbe Aufruf liest nur seinen Beleg;
bei `unconfirmed` lesend klären. Keine Ernte- oder Lieferzusage.

`run_simulation_for` erhält optional `stopAtAvailableLogs`: Bestandsziel kann vor
dem Zeitlimit eine bestätigte Pause auslösen. `stockTargetReached` und
`targetReached` unterscheiden; die erste Beobachtung ist kein dauerhafter Vorrat.
Bauchargen antworten standardmäßig kompakt; `details=true` zeigt den Verlauf.
[Abnahme und Grenzen](forestry-efficiency-0.35.5.md).

## capture_screenshot ab 0.36.0 — begrenzt live abgenommen

Auch allein „schauen“/„SCHAUEN!!!!“, „guck mal“ oder „da wo ich hinschaue“ im
Spielkontext ist ein ausdrücklicher Abruf, ohne vorherige Objektmarkierung:
aktuelle Sitzung ermitteln,
einmal capture_screenshot aufrufen und das Bild ansehen. Erfasst wird die vom
Spieler eingestellte Kamera samt UI zum Aufnahmezeitpunkt, ohne Kameraänderung.

Expliziter Bildabruf mit session, optional maxWidth/maxHeight und reasoning.
Ein MCP-JPEG-Bildblock, keine Base64-Doppelung im Text. Metadaten enthalten
Zeitpunkt, Pixelgrößen, Kameraposition/Höhe, Richtung, Zielabstand, Bildwinkel
und Bildweite auf der Zielebene. Keine Kameraänderung oder Dateiablage.
1280×720 und 640×360 sowie falsche Session live geprüft.
[Abnahme und Grenzen](screenshot-0.36.0.md).

## Live geprüft: früh fertiggestellte Bauchargen

advance_building_batch und advance_building_project: bei fertigen Bauobjekten
zugehörigen Lauf früh pausieren, Abschlusszugang/Konfiguration prüfen. Zeitbudget
bleibt Obergrenze; sofort mögliche Chargenübergänge auch bei waitSeconds=0.
Kein Hintergrunddispatcher, reine inspect-Aufrufe ändern nichts.
Entwickler-Gate und Zwei-Lager-Livepilot bestanden: [Nachweis](building-batch-workflow.md).


## Live abgenommen: Rampengeometrie, Höhen und Suchabdeckung

survey_region erhält scanOtherHeights (Standard true), maxHeights (Standard 4,
1–8 inklusive Ausgangshöhe) und maxWindows (Standard 3, 1–9 je Höhe). Zusätzliche
Höhen samt Kandidaten stehen in additionalLevels; vorhandene Wege priorisieren
die Auswahl, sind ohne separate Abfrage kein Distriktbeleg. coverage.heights
zeigt ausgelassene Höhen. planningRows: u ungeprüft, p teilweise, c alle vier
Drehungen vollständig im Fenster gesucht, niemals automatisch bauvalidiert.
Native overlapCells lokalisieren Rampen/Hindernisse. Bei älteren Bridges ohne
Geometrie begrenzte Verfeinerung; verbleibendes unknown samt Budgetflag beachten.
planBuildings=false/workBuildingId behalten die einzelne angefragte Ebene.
Beide zuvor übersehenen Gebiete live geprüft: [Umfang und Nachweise](regional-survey.md).

### 0.63-Erweiterung: Vor-Explosionswarnung (im Nutzerumfang abgenommen)

`scan_strategic_map` ergänzt bei Zielzone `analysis.explosions`: native Kernradius-
Terrainöffnung als Szenario, beobachtetes Wasser/Faulwasser, Vor-/Nach-Schwelle,
Zielzutritt und Schutzkandidaten. Compact behält diese Kette und Maßnahmen.
Full enthält Route und Geometrie. Keine Auslösung, kein Countdown und keine
vollständige Zerstörungs-/Flutprognose; unbekannte Geometrie bleibt sichtbare Gefahr.
