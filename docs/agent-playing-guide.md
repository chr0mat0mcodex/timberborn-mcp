# Spielanleitung für Agenten

## A09-Eingangspilot (Gate und Lifecycle-Liveabnahme bestanden)

Clientzeilen maximal 1 MiB inklusive Zeilenende. Fehlerhafte oder übergroße
Nachrichten werden mit kontrolliertem JSON-RPC-Fehler verworfen; keine automatische
Wiederholung. Inflight-Grenzen und Eingangstimeouts noch nicht umgesetzt.
Nach vollständigem Codex-Neustart Stop/Status/Start mit gleicher Spielsitzung
live bestätigt. Python-Skriptänderungen benötigen weiterhin einen vollständigen
Client-Neustart; Backend-Stop/Start allein lädt den Supervisor-Code nicht neu.
[Umfang, Fehlergrenzen und Nachweise](supervisor-input-pilot.md).


Automatische Bretterketten-Vorprüfung **begrenzt live abgenommen**:
Vor Bauaufträgen mit Bretterkosten und langen Simulationsläufen mit Bretterbezug
ergänzt `operatingPreflight[].plankChain` Engpässe und konkrete nächste Schritte.
Gemeinsames Diagnosebudget unverändert fünf Sekunden; keine Materialsperre,
kein Ausbau und keine Verlängerung des Laufbudgets.
[Vertrag und Livebelege](plank-chain.md#automatische-einbindung-vor-starts).

Bretterketten-Vorprüfung **begrenzt live abgenommen**: `inspect_plank_chain`
bündelt Holzversorgung, Sägewerksbetrieb, Kraft und Lager-/Transportbelege im
gewählten Bezirk. Nur Diagnose und konkrete nächste Schritte; kein automatischer
Ausbau oder Material-Startverbot. [Ablauf und Grenzen](plank-chain.md).

Fortschrittsdiagnose **begrenzt live abgenommen**: frische Baudiagnose vor
und nach einem begrenzten Zeitfenster vergleichen. Lieferung, Baufortschritt
und unveränderten Fortschritt anhand belegter Messwerte trennen. Keine automatische
Budgetverlängerung oder Material-Startsperre. [Ablauf und Grenzen](construction-progress.md).

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


Automatische Bauzentrum-/Produzentenauswahl und Vorprüfung vor Strukturstarts bzw. längeren Simulationen sind nach Gate sowie Simulations- und Baupilot begrenzt live abgenommen. Befunde bleiben beratend, Materialmangel sperrt den Start nicht. [Vertrag und Livebelege](automatic-operating-readiness.md).

Gemeinsame Betriebsdiagnose vor längeren Bau-/Simulationsläufen: `inspect_operating_readiness` bündelt ausgewähltes Bauzentrum, Überflutung, Personal und Materialproduzenten mit priorisierten nächsten Schritten. Gate und gezielter Livevergleich bestanden; kein Produktionsnachweis. [Aufruf und Grenzen](operating-readiness.md).

## A08: direkte Mod-Adapter (Gate und begrenzter Livepilot bestanden)

Core-Tests prüfen die Entscheidung aus synthetischen Beobachtungen. Sie beweisen
keine echten Spielservice-Aufrufe oder Bereinigung temporärer Vorschauen.
Beim Wegschutz sind `restored`, verlorene Verbindungen und Bauphasenabdeckung
getrennte Aussagen. Auch bei `restored=true` bleibt ohne Verlust der Status
`unknown`, solange `constructionCovered=false` gilt. Ein unbekannter
Baustellen-Wiederherstellungswert ist kein positiver Baustellennachweis.
Positive Bankvorschau, fünf native Verluste bei größerer Wohnhausvorschau und
anschließende freie Kontrollvorschau bestanden; Wiederherstellung bestätigt.
Die einzellige Bank war am Kontrollpunkt als Verlustprobe ungeeignet.
Geometrische Überlappung ersetzt keinen Verlustnachweis; Vorlagentyp und
Grundrissgröße wurden nicht unabhängig voneinander untersucht.
[Adapterpilot, Nachweise und Grenzen](mod-adapter-evidence-pilot.md).

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

Zugangssuche berücksichtigt die bisher geplante Geometrie und kann vorhandene
Höhentreppen über native Bestandsverbindungen nutzen. Ein neuer Weg am Austritt
bleibt Teil des Plans. Suchbudget-/Vorschauablehnung beweist keine allgemeine
Unerreichbarkeit; nicht blind mit anderen IDs erneut bauen. Nur `candidateFound`
mit erfolgreicher Gesamtvorschau weiterverwenden, `steps`/`planKey` beibehalten.
Bauarbeiterzugang, Fertigzugang und Bestandsanschlüsse separat nachweisen.
[Fehlerfall, Grenzen und Pilot](releases/0.67.0.md). Noch keine neue Liveabnahme.

## 0.66 — Uferzugang und Startfehler, Kernpilot begrenzt bestanden

Zugangssuche und Vorprüfung ignorieren leere native Blockzellen beim
Terrainkonflikt. Keine generelle Freigabe von Uferpumpen: native Gesamtvorschau,
Unterbau, Bau-/Fertigzugang und Bestandsanschlüsse weiter prüfen.
`run_start_speed_changed`, `run_start_speed_pending`, `run_start_speed_locked`,
`run_start_batch_busy`, `run_start_active`, `run_start_capacity` und
`run_start_stock_unavailable` sind konkrete Startablehnungen. Bei direktem
Simulationsstart bedeutet `not_executed`: kein neuer Lauf registriert.
Aktuelles Tempo, `pendingSpeed` und `activeRunId` lesen, dann neu entscheiden;
kein automatisches Retry. Andere Konflikte/Transportverlust bleiben unbestätigt,
`run_not_found` allein beweist keine Nichtausführung. [Pilot](releases/0.66.0.md).

## Automatische Vorprüfung vor längeren Läufen

**Basisdiagnose und automatische Bretterketten-Ergänzung begrenzt live abgenommen.**
Es gilt dieser Ablauf zusätzlich zu den nativen Geometrie-/Zugangsprüfungen:

1. Aktuelle Session und geplanten Bezirk bestimmen. Vor einer Bauentscheidung
   vollständige Kosten einschließlich Zugangshilfen berechnen und
   `inspect_operating_readiness_auto(session, districtId, materials, reasoning)`
   lesen. Für Produktionsläufe alternativ `buildingIds` als Bezug übergeben.
   Die Schreibweise zeigt Parameter, keine ausführbare JSON-Vorlage.
2. `selection` prüfen: Ist das Zentrum belegt, welcher Bezirk wurde gewählt,
   welche Gebäude/Güter fehlen wegen Suchgrenzen oder unbekannter Zuordnung?
   Bei Mehrdeutigkeit Bezirk/Bezugsgebäude präzisieren. Für gezielte Kontrollen
   bleibt `inspect_operating_readiness` mit expliziten Gebäude-IDs verfügbar.
3. `diagnosis.nextSteps` gemeinsam mit den Auswahlhinweisen auswerten:
   Überflutung vor Personalmaßnahmen; Pause, Zutaten, Kraft und Produktplatz
   getrennt behandeln. Fehlende Produzenten in einer begrenzten Auswahl
   bedeuten keine bewiesene Abwesenheit in der Kolonie.
4. Bauentscheidung ausdrücklich anhand der Befunde treffen. Materialmangel
   verlangt weder 100 Prozent Vorrat noch eine zusätzliche Nutzerfreigabe.
   Danach den regulären Auftrag einmal senden. Der automatische Startbericht
   ersetzt diese Entscheidung nicht: Er wird vor dem nativen Start erhoben,
   aber zusammen mit dessen Antwort zurückgegeben.
5. Im Startergebnis `operatingPreflight` und den eigentlichen Startbeleg getrennt
   lesen. Bei Bretterbezug zusätzlich `plankChain.state` und
   `plankChain.report.findings` mit `nextStep` auswerten. Bei `district_unknown`,
   `unavailable` oder `budget_exhausted` die genannten Einzelprüfungen nachholen;
   die normale Betriebsdiagnose bleibt erhalten. Kein automatischer Ausbau.
   Beide Diagnosen teilen fünf Sekunden; Laufbudgets bleiben unverändert. Diagnosezustand `observed` bedeutet nicht „betriebsbereit“; bei
   `unavailable`/`budget_exhausted` fehlen Belege. Nach einem bereits gesendeten
   Auftrag nur lesend nachprüfen, niemals für bessere Diagnosedetails neu starten.

Automatisch erfasst: alle Strukturstarts einschließlich Reparatur und interner
Bauchargen/Räumgruppen; Simulations-for ab sechs Spielstunden und jedes until,
auch interne Fertigbauläufe. Kurze for-Läufe, Status, Cancel und direktes Setzen
der Spielgeschwindigkeit haben keine automatische Vorprüfung.

`demands` des Strukturstarts enthält die vollen geplanten Kosten, keinen
Restbedarf. Simulationsziele sind Bestandsziele, keine Baukosten. Ohne positive
Bedarfsmenge bleibt `coveragePercent` null. Der Werkzeugwert ist das rohe
Bestand/Bedarf-Verhältnis und kann über 100 Prozent liegen; für die unten
beschriebene gedeckelte Bauentscheidungsquote pro Gut auf 100 Prozent begrenzen.
Globale Bestände und Arbeitslose garantieren keine lokale Belieferung/Besetzung.
Details: [Automatische Betriebsdiagnose](automatic-operating-readiness.md).

## Materialdeckung und Bauentscheidung

Fehlender Vorrat verhindert keinen Bauauftrag. Vor der Entscheidung einmal
Materialdeckung und vorhandene Produktion prüfen, dann entscheidet der Agent.
Keine Mindestquote, kein Warten auf 100 Prozent und keine zusätzliche
Nutzerbestätigung allein wegen fehlender Baumaterialien. Nach positiver
Entscheidung die geplante Struktur regulär beauftragen; Lieferung und Fertigbau
folgen während der Bauphase. Geometrie und Bau-/Bestandszugang weiter prüfen.

- Gesamtkosten aller geplanten Teile einschließlich Wege, Treppen und Plattformen
  aus aktuellem Baukatalog (`costs`) oder Produktionsgraph ermitteln und je Gut
  summieren. `inspect_goods` liefert den aktuellen verfügbaren Bestand; alle
  für diese Güter erforderlichen Seiten lesen. Fehlende Werte bleiben unbekannt.
- Pro Gut: `Deckung = 100 × min(verfügbarer Bestand, Bedarf) / Bedarf`.
  Gesamtdeckung: `100 × Summe(min(Bestand je Gut, Bedarf je Gut)) / Summe(Bedarf)`.
  So kann überschüssiges Holz fehlende Bretter nicht ausgleichen. Nur Güter mit
  positivem Bedarf zählen; kostenfreier Plan: 100 Prozent, keine Materialien nötig.
  Ist ein benötigter Bestand unbekannt, keine vollständige Gesamtquote behaupten.
  Dies ist mengenbezogene Vorratsdeckung, kein Baufortschritt oder Zeitversprechen.
- Erzeuger über `inspect_production_graph` zuordnen, tatsächlich vorhandene
  Gebäude mit `find_buildings` prüfen und deren aktuellen Zustand über
  `inspect_building_operations` lesen. Rezeptdefinition allein ist keine
  vorhandene Produktion. Für Holz/Sammelgüter auch Erntegebäude und erreichbare
  markierte Quellen berücksichtigen. Vorhanden, betriebsbereit und tatsächlich
  produzierend getrennt benennen; für die Bauentscheidung ist kein zusätzlicher
  Produktionsmesslauf vorgeschrieben. Fehlende/pausierte/blockierte Erzeuger
  oder unbekannte Daten sind Hinweise für die Entscheidung, keine Startsperre.

Kurzer Entscheidungsbericht, etwa: „Holz 20/40 = 50 %, Bretter 0/10 = 0 %;
gesamt 40 %. Holzfäller vorhanden, Sägewerk vorhanden und besetzt. Bau beauftragen,
Material nachliefern lassen.“ Beispielwerte sind keine Livebeobachtung.
Globale Vorräte sind nicht für das Projekt reserviert und garantieren keine
Lieferbarkeit. Bei mehreren gleichzeitigen Vorhaben konkurrierenden Bedarf nennen.

Nach Auftrag begrenzte Bauzeitfenster nutzen und tatsächliche Baustellen,
Materialzufuhr und Versorgung beobachten. Materialmangel rechtfertigt weder
Abriss noch erneutes Platzieren. Nach Zeitlimit frisch diagnostizieren und bei
weiterhin sinnvoller Entscheidung zusätzliche Simulationszeit geben, ohne die
Aufträge zu wiederholen. Eine unveränderte fehlende Produktion nicht durch
blindes Warten lösen; der Agent entscheidet über Produktionsaufbau oder Umplanung.
Spätere Vorräte nicht als Restbedarf aus Gesamtkosten zurückrechnen: bereits
verbaute Materialien und Lieferungen unterwegs sind darin nicht vollständig erfasst.

## Bauplaner-Korrektur auf 0.63 — begrenzt live geprüft

`project_preview_failed` meldet einen nativen
Vorschaufehler mit unbewiesener Wiederherstellung; `project_plan_faulted` die
anschließende Sperre. Spiel neu laden, neue Session prüfen, keinen Bauauftrag
wiederholen. Die Sperre wird nicht automatisch entfernt. `project_not_found`
bedeutet, dass die gelesene Projektkennung in dieser Sitzung nicht existiert.

Chargen behalten `failure.code/stage/execution/recoveryAction` in Compact und Full.
Ein Planungsfehler stoppt die Charge; `advance` plant sie nicht erneut.
`no_build_order` gilt nur für diese Planungsphase, nicht für ältere Räumarbeiten
oder eine garantierte Vorschau-Bereinigung. Bei `pending_build`/`ledger_required`
nur die vorhandene Kennung lesend abgleichen, nie mit neuer Kennung wiederholen.
`previewRegistered=false`/`native_invalid` ist ein negativer Planungsbefund,
kein Bauauftrag. [Gate und Nachweisgrenzen](planner-repair-2026-10-17.md).
Menschliches Gate und ebener 15-Teile-Pilot bestanden: zwei Tanks, Lager und
zwölf Umweg-Wege fertig/zugänglich, Bestandsanschlüsse erhalten. Native Ausnahme
und Chargenpersistenz nur synthetisch geprüft, kein neuer Höhenfertigbaunachweis.
Ingame-Fortschrittszähler derzeit fehlerhaft; strukturierte summary und frische
Diagnose verwenden. Nach Zeitlimit vorhandene Baustellen diagnostizieren; bei
geklärtem Zustand nur Simulationszeit ergänzen, nie den Bauauftrag wiederholen.

## Positive Terraforming-Optionen — begrenzt live geprüft

Der vollständige Zielscan liefert zusätzlich `analysis.terraforming`: bis zu drei
räumlich getrennte 3×3-Reservoirkandidaten mit zwei Ebenen Absenkung. Suchgebiet:
32 Felder um die Zielzone; feste Geometrie, keine erschöpfende Landschaftsplanung.
Kompakt bleiben Nutzen, Ursprung, Fläche, Aushubtiefe, Zielbodenhöhe, geometrisches
Zusatzvolumen, Zuflussbeleg, Blockaden und nächste Maßnahme sichtbar. Nur die
neun `excavationCells` je Kandidat stehen ausschließlich in `detail="full"`.

18 Rasterwürfel Zusatzvolumen sind weder 18 Wassergüter noch eine Reichweite in
Tagen. Ebene offene Oberfläche erforderlich; ein fester Block bleibt unter dem
neuen Boden. Höhlen im Aushub/Boden, seitlich offene Beckenwände, Zielzone samt
Rand und beobachtete Objektanker samt Rand schließen Standorte aus. Objektanker
ersetzen keine vollständige Belegungs-/Fundamentprüfung. Unvollständige Objekt-
abdeckung blockiert den Übergang zur Planung. Örtliches Faulwasser und fehlendes
Wasser über Randhöhe werden als Blockaden ausgewiesen. Sauberes örtliches Wasser
beweist keine dauerhaft saubere Quelle oder künftige Nachfüllung.

Vor Ausführung native Werkzeug-/Forschungs-, Belegungs-, Zugangs- und Bauphasen-
prüfung sowie kontrollierte Nachfüllung, Abfluss, Rückstau, Verdunstung und Pumpen-
nutzung klären. Überregionale Faulwasserpfade aus hazards weiterhin berücksichtigen.
Keine automatische Grabung; keine erfundenen Kosten oder Bewässerungsreichweiten.
`overviewOnly=true` bleibt ohne zielbezogene Terraforming-Optionen.

## Kompakte Scanantwort — live geprüft

`scan_strategic_map` liefert standardmäßig `detail="compact"`: Bedrohung,
betroffenes Ziel, beobachteter Pegel, minimale Zuflussschwelle, Kontamination,
Schutzpunkt/Kronenhöhe, konkrete Blockade und nächste Maßnahme. Die vollständigen
Gefahrenzähler und bis zu acht priorisierte Beispiele bleiben erhalten.
`hazards.commonMeasures` und `commonUnresolved` gelten für alle Prioritäten;
individuelle `measures`/`unresolved` ergänzen diese gemeinsamen Angaben.
Abdeckung, Beobachtungsalter und fachliche Grenzen bleiben sichtbar.

Kacheln, Korridorpfade, Querschnittszellen, Spezialobjekte und die allgemeine
Fluss-/Quelleninventur stehen in `detail="full"`. Mit identischer Zielzone und
derselben `scanId` nachlesen: kein erneuter Kartenscan; die bestehende frische
Sessionprüfung bleibt. `overviewOnly=true` liefert kompakt nur den Flussbericht-
Überblick mit Zählern/Wetter/Seitengrenzen, keine zielbezogene Gefahrenbewertung.
Für Inventurseiten full plus entsprechende Offsets verwenden. Fehlende Details
sind keine leeren Beobachtungen. Fehler bleiben vollständig erhalten.

## Gefahrenbewertung im Zielscan — begrenzt live abgenommen

`analysis.hazards` zuerst auswerten, danach die Korridorbeispiele. Die Bewertung
zählt alle erreichbaren nassen Hohlräume und diejenigen mit beobachtetem Pegel
mehr als 0,01 über Zielhöhe **und** minimaler Geländeschwelle. Kontaminiertes
Restwasser bleibt auch bei stillstehenden Quellen eine priorisierte Gefahr.
Die Zähler sind Hohlräume, keine unabhängigen Flutereignisse. Bis zu acht
repräsentative Gefahren erscheinen unter `priorities`; weitere bleiben im Zähler.
`target_water_observed` bedeutet bereits beobachtetes Wasser über der Zielhöhe.
Ab 0.65 unterscheidet `target_badwater_observed` kontaminiertes
Zielwasser; `contaminatedTargetNodes` zählt die betroffenen Hohlräume. Aktuelle
Zielüberflutung zuerst auf ausgefallene Versorgung prüfen, dann weitere Kernrisiken.
`no_current_head_exceedance_not_safe` ist keine Entwarnung für künftige Ereignisse.

Pro Gefahr: Ursprung, Pegel, Höhenüberschuss, Kontamination, Zielzutritt sowie
Schutzquerschnitt und angenommene Krone. `closureAssessment` prüft Dachfreiheit,
Kartenhöhe, vollständige Ufer und geometrische Umwege bei hypothetischer Sperre.
`terrain_route_cut_at_observed_level` ist nur ein geometrischer Schnitt, keine
Empfehlung zur Sperre. Ab 0.65 zuerst `backwater` und einen
unabhängig geprüften Abfluss bewerten. Dieser Nachweis betrifft einen Ursprung
bei beobachtetem Pegel; weitere Zuflüsse, Bauwerke, Rückstau, Bauphasen, Material,
Forschung, Arbeitszugang und Dauer bleiben zu prüfen. Bei Dachkonflikt, offenem
Umweg oder unvollständigem Querschnitt einen anderen Sperrpunkt untersuchen.
Keinen Abfluss aufgraben, bevor dessen Auslass und Rückwirkungen belegt sind.
Ab 0.65 bleibt `backwater.closureRecommended=false`. Eine Gegenprobe unter der
Sperrkrone kann `target_route_reopens_below_barrier_crest` melden. Auch ohne diesen
Umweg bleiben Abflusskapazität und Rückstau unbewiesen; Randkontakt ist kein
sicherer Auslass. `reviewLevel` ist ein Szenario, keine Pegelprognose.
Ein niedriger Szenariopegel verkleinert den Schutzvorschlag für beobachtetes Wasser nicht.
`overviewOnly=true` liefert weiterhin keine zielbezogene Gefahrenbewertung.
Explosionszeit, tatsächliche Ereigniswirkung, zukünftige Pegel und Ankunftszeiten bleiben unbekannt.

### Instabile Kerne und Faulwasserdurchbruch — 0.63, Warnung im Nutzerumfang abgenommen

Live 17. Oktober: neun Kerne bewertet; Kern (168,130,12) mit Faulwasserherkunft,
Zielzutritt und Schutzposition schon in Compact sichtbar. Der Schutzquerschnitt
war unvollständig; Vor-/Nach-Schwelle unverändert. Damit Warnung bestätigt,
aber weder neu entstandener Durchbruch noch wirksame Schutzanlage nachgewiesen.
Nutzerabnahme: Frühe Warnung mit nachvollziehbarem Gefahrenweg und soweit möglich
Eindämmungsvorschlägen genügt. Ein vollständig validierter Bauplan ist keine
Voraussetzung für diese Warnung; fehlende Nachweise weiter ausdrücklich nennen.

`analysis.explosions` bleibt auch in Compact sichtbar. Zuerst Kernposition,
`state`, `currentlyBlocked`, `timing`, `chains` und `unresolved` lesen.
`badwater_breach_to_target_potential` verbindet Kern, beobachteten Speicher,
Kontamination, Geländeschwelle vor/nach möglicher Öffnung, Zielzutritt und
Schutzkandidat. Das Zielgebiet ist die angegebene Schutz-/Teichzone, kein
automatisch identifiziertes Gewässer. Kernposition allein bestimmt keine Gefahr.

Die Bridge liest öffentlichen nativen Radius und Radiuszellen, filtert gegen
festes Terrain und verändert nichts im Spiel. Die Analyse öffnet diese Zellen
nur in einer Kopie. Das ist `native_radius_terrain_scenario`: tatsächliche
Zerstörung, Einstürze, Bauwerkswirkung und Wasserverteilung sind nicht vollständig
modelliert. Countdown und Ankunftszeit bleiben unbekannt. Ein aktuell blockierter
Kern ist keine dauerhafte Entwarnung. Unvollständige Daten/Budgets ergeben
`explosion_risk_unassessed`, niemals eine erfundene Kette oder Sicherheit.

Vor Auslösung Schutzquerschnitt und Krone außerhalb des möglichen Sprengbereichs
prüfen; `protection_inside_possible_blast_relocate` verwirft den Standort.
Auch bei `terrain_route_cut_at_observed_level` zuerst Abfluss und Rückstau prüfen; keine Sperrempfehlung allein aus diesem Wert.
Material, Bauzugang, Fertigstellung vor Auslösung, alternative Rinnen und Rückstau
prüfen. Zielzutritt/Wasserentnahme absichern; Umleitung erst mit belegtem sicheren
Auslass. Compact nennt Kern, Wasserherkunft, Zielzutritt, Schutzposition/-höhe und
Maßnahmen; `detail=full` ergänzt Route, Radius-Terrainzellen und Querschnitt.



## Strategischer Kartenscan 0.62 — begrenzt live abgenommen

Für allgemeine Mapawareness `scan_strategic_map(session, overviewOnly=true, detail="full")` ohne
Siedlungsziel verwenden. `rivers` enthält nach vollständigem Scan Abschnitte mit
lokalen Pegel-/Kontaminationsbereichen, geometrische Verbindungen, Quellen und Wetter.
Mit `sectionOffset` (32), `linkOffset` (64), `sourceOffset` (16) vollständig blättern.
Quellenstärke ist kein gemessener Durchfluss, geometrische Kanten keine beobachtete
Fließrichtung. Aktivierungsregeln und aktuellen Zustand getrennt lesen; zukünftige
Kontamination bleibt ohne belegte Regel unbekannt. Vor/nach Wetterwechsel jeweils
neu scannen: Unterschiede zur vorherigen vollständigen Beobachtung derselben Sitzung
werden ausgewiesen. Gate und Livepilot einschließlich Dürrebeginn bestanden:
zehn Quellen stellten die Förderung ein, lokale Restflüssigkeit blieb getrennt.
Faulwasserflutwechsel und spezielle Aktivierungsregeln sind noch nicht live belegt.
Korridorpfade, Spezialobjekt- und Flussseiten erfordern `detail="full"`;
Compact enthält Gefahren und Terraforming-Optionen mit Handlungsbedarf.

`scan_strategic_map(session, x, y, z, width, height)` verwendet die angegebene
Region als **Schutzzone** und scannt die ganze Karte. Zunächst `tilesPerCall=1`
für einen kleinen Pilot; mit derselben `scanId` und Zielzone fortsetzen.
Bei vollständiger Terrainabdeckung folgen mögliche Zuflusskorridore, Spezialobjekte,
Geländeschwellen und Sperrquerschnitte. Keine Pause erforderlich. `scenarioWaterLevel`
ist optional eine angenommene absolute Rasterhöhe, kein prognostizierter Pegel.
`objectOffset` blättert Spezialobjekte; ein neuer Scan ohne Kennung aktualisiert
den historischen Stand. Abdeckung, Beobachtungsalter und Analysestatus mitlesen.

Pfade und Sperrquerschnitte sind geometrische Planungshilfen. Bestehende Dämme,
Schleusen und Wasserphysik sind nicht modelliert; Quellnamen nur Vorlagenhinweise.
Die acht Korridorbeispiele decken nicht alle Gefahren ab. Kandidaten lokal frisch
prüfen, Alternativzuflüsse/Rückstau untersuchen und erst dann native Bauplanung
verwenden. Keine Bauaufträge direkt aus dem Scan ableiten. Wassersemantik unter
Überhängen und Laufzeit zuerst im [Livepilot](strategic-map-0.62.md) prüfen.
Der erste Livepilot bestätigte Gesamtabdeckung bei Tempo 7 und einen verseuchten
Tunnel. Die dabei gefundenen Quellenhöhen-/Kronenfehler sind korrigiert und live
nachgeprüft. Tunneldecken können unter der angenommenen Krone liegen:
angegebene Bauhöhen keinesfalls als tatsächlich platzierbare Blöcke übernehmen.

## Überlebensstart und Learnings aus dem Spielversuch vom 16. Oktober 2026

Diese Reihenfolge gilt für einen beauftragten Überlebenslauf. Vorhandene Versorgung
zuerst lesen und nutzen; keine funktionierenden Gebäude doppelt bauen.

**Als Erstes die Karte auf Gefahren scannen.** Wohn- und Versorgungszone abgrenzen
und Zuflüsse bis zu ihren Quellen bzw. relevanten Engstellen verfolgen, auch
außerhalb des unmittelbaren Siedlungsumfelds. Besonders Rinnen, tiefe Einschnitte,
Durchbrüche und Überlaufkanten suchen, die Wasser oder Schlechtwasser in diese
Zone führen können. Höhen, Abflussmöglichkeiten und gefährliche Spezialobjekte
einbeziehen. Ungeprüfte Bereiche ausdrücklich unbekannt lassen. Den ersten Scan
gezielt halten, damit der unmittelbare Versorgungsstart nicht unnötig verzögert wird.
Erkannte akute Gefahren können die folgende Standard-Baureihenfolge übersteuern.

1. **Wasserpumpe, Beerensammler und Holzfällerfahne in einem Aufwasch planen und
   beauftragen**, einschließlich gemeinsamer Wege und Fällmarkierungen. Unabhängige
   Teile gemeinsam bauen lassen; nur echte Bauabhängigkeiten seriell machen.
   Wasser an der Ansaugstelle, erreichbare reife Beeren und fällbare Bäume prüfen.
   Für alle drei Besetzung, Zugang und tatsächlichen Ertrag kontrollieren.
2. **Danach Wohnraum und einen Forscher (Erfinder) aufbauen.** Betten am Bedarf
   ausrichten und Nachwuchs ermöglichen. Forschung besetzen, sobald die
   Grundversorgung trägt; keinen Versorgungsarbeiter dafür ersatzlos abziehen.
3. **Dann Bretter:** Sägewerk und direkt verbundenes Laufrad gemeinsam planen.
   Zwei Arbeitskräfte sowie Holz für Bau und Verarbeitung einrechnen. Fertigbau,
   echte Kraftnetzverbindung und erzeugte Bretter getrennt nachweisen. Bei
   Personalmangel zeitweise fertigen, danach Arbeiter in die Versorgung zurückgeben.

**Vorräte früh absichern.** Passende Wasser-, Nahrungs- und Holzlager einplanen
und konfigurieren. Erreichbarer Bestand und Verbrauch über Nacht bzw. während
einer Dürre entscheiden über Ausbau, nicht ein einzelner positiver Lagerwert.
Ausgabepuffer sind keine zusätzliche Reserve, die zu Gesamtbeständen addiert wird.
Überflutete Lager können weiter im globalen Bestand stehen; Zugriff separat prüfen.
Bei vollen Produktionspuffern Transport verbessern statt weitere Produzenten bauen.

**Vor dem Ausbau die Umgebung verstehen.** Ansaugstelle bei Niedrigwasser,
Wasseroberfläche bei Hochwasser, Kontamination, Zufluss und Abfluss sowie einen
höheren Ausweichstandort prüfen. Gleiche Messzellen mehrfach lesen, um einen
Pegeltrend zu erkennen. Lokale Baugeometrie beweist keine Flutsicherheit.
Nicht die gesamte Versorgung in einem Becken konzentrieren. Ein Ausweichweg und
handlungsfähige Bauarbeiter müssen vor der Überflutung verfügbar sein; nach dem
Ausfall des Distriktzentrums war im Versuch kein Rettungsbau mehr möglich.

**Terraforming und Wasserbau gehören ins Early- und Midgame.** Gefährliche
Zuflussrinnen rechtzeitig schließen oder umleiten, bevor das Wasser die Siedlung
erreicht. Dabei einen geeigneten Abfluss erhalten: Eine Sperre kann Wasser
aufstauen oder über eine andere Kante in die Siedlung drücken. Gelände formen,
Schutzlinien, Entwässerung, Wasserrückhalt und sichere Höhenzugänge als aktive
Ausbauziele behandeln, nicht erst als Notmaßnahmen. Je nach tatsächlich
freigeschalteten Fähigkeiten mit Dämmen, Deichen oder anderen Wasserbauten
beginnen und später gezielte Geländeänderungen ergänzen. Material, Forschung,
Bauarbeiterzugang, Bauzeit und Bauzustände bis zur geschlossenen Schutzlinie
einplanen. Wirkung anschließend an Wasserständen und verbleibenden Zuflüssen
prüfen; fertige Bauteile allein beweisen keinen Schutz. Nach Ereignissen und
größeren Eingriffen Gefahrenkarte aktualisieren. Fehlt ein nötiger MCP-Eingriff,
die konkrete Lücke früh melden und im Backlog festhalten, statt Terraforming
stillschweigend aus der Strategie zu streichen.

**Personal als knappe Ressource behandeln.** Wasser und Nahrung zuerst besetzen,
danach Holz und benötigten Transport. Standard-Sollbesetzungen neu gebauter
Gebäude prüfen: Der Trägerposten verlangte zunächst fünf Arbeiter bei nur vier
verbliebenen Erwachsenen. Unproduktive trockene Pumpen und volle Betriebe
vorübergehend pausieren. Alterung und Nachwuchs berücksichtigen; Rückgang der
Bevölkerung nicht ohne Beleg Hunger oder Durst zuschreiben.

**Nicht auf scheinbar beruhigende Diagnosen verlassen.** `ready=true` beweist
weder Wasserförderung noch tatsächlichen Betrieb. Sichtbares `Paused` kann
`Flooded` verdecken; fehlender Fluttext ist kein Trockenheitsnachweis.
Ab 0.65 `water.flooded` in der Betriebsdiagnose lesen:
native Überflutung auch bei Pause. `intakeUnderwater`, `intakeContamination` und
`intakeBlocked` getrennt prüfen; null bleibt unbekannt/nicht anwendbar. Erst
Wasserblockaden klären, dann Personal-/Transportmaßnahmen. Kontamination am
Ansaugpunkt beweist weder Trinkbarkeit noch Förderleistung. Gegenprobe mit
trockener Kontrollpumpe und realen Zyklen; [Gateumfang](releases/0.65.0.md).
Bedürfniswarnungen allein sind keine akute Notlage, und gespeicherte Bedürfniswerte
können trotz bereits ausgefallener Versorgung noch gut aussehen. Bei Warntexten
mit Platzhaltern keine Frist erfinden. Fehlende Gefahreninformation früh klären.

**Tempo und Planungszeit:** Im Lauf gewünschtes Tempo halten; nur ausdrücklich
erlaubte Planungspausen nutzen. Baupersonal vor der endgültigen Vorschau einstellen,
damit eigene Änderungen den Planschlüssel nicht sofort veralten lassen.
Automatische Bauabschlüsse können pausieren: frisch prüfen, Projekt abschließen
und das vereinbarte Tempo zeitnah wiederherstellen. Konkrete Ablehnungen
diagnostizieren; unbestätigte Aufträge niemals blind wiederholen.

Im Versuch gelangen Bretterproduktion, Aufforstung, Reservoirpumpe mit Treppe
und Trägerposten. Großflächiges verseuchtes Hochwasser legte anschließend die
Versorgung still; der Nutzer beendete den Lauf. Das ist kein bestandener
Überlebensnachweis. Benötigte neue MCP-Fähigkeiten stehen im [Backlog](../BACKLOG.md).

## Navigationsprüfung 0.61 — begrenzt live abgenommen

Große Karte, Höhenfehler, gefährdeter Bestand und gültiger Umweg live geprüft.
Pausenfehler als not_executed bestätigt; keine Bauaufträge erteilt.
Initialisierung und unbekannter Objektzustand sind keine fremden Baustellen.

Die pauschale Grenze von 4.096 Kartenobjekten entfällt. Relevant bleiben alle
Baustellen, Distrikte, Wege und Zugangsobjekte; 4.096 Zugangsmesspunkte und
16.384 Distriktvergleiche sind weiterhin Grenzen. Große Karten sind damit nicht
automatisch vollständig prüfbar. Bauarbeiterzugang und Lieferung separat belegen.
Bei direkter Großprojektplanung/-start bedeutet ein konkreter `project_plan_*`-
Fehler mit `not_executed`: kein neuer Bauauftrag durch diesen Aufruf.
Pause, fremde Baustellen, gesperrten Planer oder gemeldete Navigationsursache
gezielt klären. `unconfirmed` bleibt ausschließlich lesend zu klären.
Kein automatisches Retry. [Details und Livebelege](navigation-scope-0.61.md).

## Reparaturstart 0.60 — begrenzt live abgenommen

0.60 unterscheidet bestätigte Startablehnung und ungewisse Ausführung.
Bei `run_large_project_repair`/`run_large_project` bedeutet `not_executed` mit
konkretem Startfehler: dieser Aufruf hat keinen neuen Bauauftrag erteilt.
`project_start_plan_changed`: Zustand frisch lesen, Reparaturvorschau neu erstellen
und fachlich beurteilen. `repair_parent_ineligible`: Elternprojekt diagnostizieren.
Andere Gründe wie fremdes offenes Projekt, falscher Distrikt oder belegte
Simulation zuerst klären. Kein automatisches Retry; frühere Aufträge bleiben bestehen.
Bei `unconfirmed` weiterhin ausschließlich lesend klären, niemals erneut starten.
Der Grund erscheint als verständliche Ergebniszeile auch ingame und in Dateilogs.

Der ursprüngliche 0.59-Konflikt ist nicht als behoben nachgewiesen. Neuer 0.59-Pilot:
zwei gleiche Vorschauen mit stabilem Schlüssel, erfolgreicher Reparaturstart,
Helfer und Elternbaustelle abgeschlossen, Pause und Elternfreigabe bestätigt.
Kein Nachweis für Reparatur tatsächlich zerstörter Zugänge. Unter 0.60 zusätzlich
falscher Planschlüssel korrekt ohne Bauauftrag abgewiesen und gültiger Reparatur-
lauf bis Weg-/Elternabschluss, Pause und Freigabe geprüft. Zwölf Ergebniszeilen
in beiden Dateilogs identisch. Mensch bestätigt Ablehnungs- und Abschlusszeile vollständig und verständlich lesbar. [Befund und Livebelege](repair-start-0.60.md).

## Historischer Nachweis: 0.59 begrenzt live abgenommen

Stand 16. Oktober 2026: 0.55–0.59 sind im dokumentierten Umfang live abgenommen.
0.58: Gebäudepause, Ein-Stunden-Lauf und Pflanzbeleg live geprüft; Mensch bestätigt die Ergebniszeilen.
Beim Einstieg installierte Bridge und aktuelles Werkzeugschema lesen; vorbereitete
Funktionen nicht allein aufgrund dieser Anleitung als verfügbar behandeln.
[Projektstand](../PROJECT_STATE.md) · [0.58-Vertrag](agent-log-outcomes-0.58.md).

## Agentenlog lesen: Cursor, Filter und Bezüge ab 0.55/0.56

`inspect_agent_log` liest den sitzungslokalen Aufrufpuffer: höchstens 128 Aufrufe,
pro Seite `limit` 1–32, neueste Startsequenz zuerst. `tool` und `state` filtern
vor der Seitengröße. `nextCursor` nur mit denselben Filtern weitergeben; bei
`hasMore=false` ist die aktuelle Trefferfolge ausgeschöpft. Neue Aufrufe verschieben
Folgeblätter nicht. Status und nachträglich ergänzte Bezüge bleiben veränderlich:
kein eingefrorener Snapshot. Der Logleser kann sich selbst als laufend enthalten.

`gapDetected`/`unavailableBeforeCursor` melden verdrängte ältere Einträge vor
Filtern, nicht die Anzahl verlorener Filtertreffer. Null Lücken heißt nicht,
dass nie Telemetrie verloren ging. Löschen/Neuladen macht alte Cursor ungültig;
dann neu beginnen, keinen alten Cursor auf die neue Sitzung übertragen.

Ab 0.56 zusätzlich `projectId` und `runId`, alle Filter gelten gemeinsam mit UND.
`projectId` trifft auch Elternprojekte. `correlations` unterscheiden `request`
(angefragt) und `result` (vom Fachbericht geliefert). Auch eine abgewiesene Anfrage
kann eine Kennung tragen; weder Bezug noch Status beweisen Bauabschluss oder Wirkung.
Keine Zuordnung aus beliebigen Gebäude-, Job- oder Action-IDs erfinden. Neu ergänzte
Bezüge hinter einer schon gelesenen Grenze erfordern bei Bedarf eine frische Suche.
[Pagingvertrag](agent-log-pagination-0.55.md) · [Korrelationsumfang](agent-log-correlation-0.56.md).

## Ingame-Anzeige und Dateilogs ab 0.57

Das Fenster „MCP-Aufrufe“ öffnet sich nach dem Laden eines Spielstands automatisch
(ab 0.69; Öffnen und Sichtbarkeit live bestätigt).
Es lässt sich schließen und über „MCP-Log“ erneut öffnen. Es zeigt Werkzeug, Status,
Parameterzusammenfassung und lesbare Aufrufbegründung. Die in 0.56 ergänzten
Projekt-/Laufkennungszeilen sind seit 0.57 wieder entfernt; strukturierte Bezüge
bleiben über MCP und in Dateilogs verfügbar. Der Aufrufstatus allein erklärt keine
Wirkung: „Erfolgreich“ kann eine erfolgreiche Leseabfrage bedeuten.

Lokale JSON-Zeilenlogs liegen unter `%LOCALAPPDATA%\TimberbornMcp\Logs\bridge`
und `...\mcp`. Pro Laden eine neue Datei mit Kolonie-/Spielstandname und UTC-Zeitstempel;
MCP-Sitzungsdatei ab erstem bestätigten Kontakt, zusätzlich ein Prozesslog.
Keine Größenbegrenzung, Rotation oder automatische Löschung. Neustart/Neuladen
setzt alte Dateien nicht fort. Bei Diagnose gezielt Komponente, Zeit und Session
wählen, nicht sämtliche wachsenden Dateien vollständig einlesen. Activity-Aufrufkennung
verknüpft beide Komponenten; `failedWrites`/Sequenz können Schreibverluste zeigen.
Logs bleiben lokal und werden nicht committed. Kein vollständiges Unity-/stderr-
Abbild und kein Ersatz für das Spieländerungsjournal. [Dateilogvertrag](file-diagnostics-0.57.md).

## Bau-Ergebniszeilen 0.59 — begrenzt live abgenommen

Einzelprojekt, Ein-Gebäude-Charge und Vier-Teile-Großprojekt am anderen Flussufer
live geprüft; Ingame-Lesbarkeit bestätigt. Gezielter Großprojektstopp belegt
2/4 fertig plus eine Baustelle und einen nicht erteilten Auftrag. Erst pausing,
dann bestätigte Pause. Nach Fertigbau der vorhandenen Baustelle Freigabe mit 3/4;
der vierte Auftrag bleibt aufgegeben. Freigabe ist kein vollständiger Fertigbau.
Zusätzlich live geprüft: Räumgruppe mit sechs Stümpfen, Spielzeitbudgetende und
separater Bauchargenstopp. Reparaturvorschau erfolgreich, Start mit state_conflict
abgewiesen; in dieser Probe kein Reparaturerfolg belegt. Ein späterer unabhängiger
Kontrollfall gelang, siehe Abschnitt 0.60. Unbestätigten Start nur lesend klären,
nicht wiederholen. Zeitlimit-/Stopgrund im Kurztext noch allgemein, Fachbericht
lesen. Andere Teilfehler und längere Räumläufe nicht pauschal abgenommen.
[Zusatzprotokoll](agent-log-live-tests-0.59.md). Auftragsstatus
`ordered` bezeichnet ab 0.63 bestätigte Einzelprojekt-Aufträge; `completed` verlangt Fertigbau. Bis 0.62 bezeichnete `completed` lediglich den Auftrag.
Bauabschlussberichte trennen fertige Bauteile, Gebäudezugang und Zeitbudget.
`finished_accessible` bestätigt zusätzlich die Pause, keinen Betrieb.

Bauchargen nennen fertig/zugänglich belegte Gebäude als gespeicherten Stand.
Großprojekte zählen Bauteile einschließlich Wege/Hilfsbauten; bestätigte Platzierung,
Fertigbau und unbestätigte/offene Teile bleiben getrennt. `inspect_large_project`
liefert gespeicherte Nachweise; Diagnose/Close prüfen frisch. Projektfreigabe kann
noch offene Teile aufgeben. Räum-/Baugruppen geben gespeicherte Belege wieder;
„Stopp angefragt“ ist kein abgeschlossener Stopp. Bei `stop_large_project` erst
`pauseConfirmed=true` und tatsächliches Tempo 0 als Pausennachweis verwenden.
Bei `stop_building_batch` behält
ein bestehender Simulationslauf sein Budget. Live bestätigt: Tempo blieb zunächst
7, erst das ursprüngliche Budgetende pausierte. Der historische Chargenstand
blieb 0/2, obwohl eine spätere Einzelbauabfrage 1/1 fertig belegte. Nie aus einer Ergebniszeile allein
Pause, Produktion oder allgemeine Bausicherheit ableiten.

Für Fortschritt dieselbe Projekt-/Chargenkennung lesen; alte Startzeilen werden
nicht automatisch aktualisiert. Keine Wiederholung unbestätigter Aufträge.
[Werkzeugumfang und Test-Gate](agent-log-building-outcomes-0.59.md).

## Ergebniszeilen ab 0.58 — begrenzt live abgenommen

`outcomeSummary` ergänzt nach Aufrufabschluss eine eigene Zeile „Ergebnis:“ ingame
und in beiden Dateilogs. Bei älteren Bridges/anderen Werkzeugen kann sie leer sein;
leerer Text bedeutet keinen nachgewiesenen Erfolg. Nicht als MCP-Argument senden.

- Einzelne Gebäudeeinstellungen: geändert, bereits passend oder unbestätigt.
  `inspect_building_settings` meldet den gelesenen Pausenzustand. Freigabe oder
  Rezeptwahl beweist keinen Betrieb. Bei unbestätigtem Ausgang nur lesend klären.
- Simulationsstart ist kein Abschluss. Startbeleg bleibt historisch; mit derselben
  Laufkennung `inspect_simulation_run` lesen. Dessen Ergebniszeile unterscheidet
  laufend/beendet/abgebrochen/unterbrochen/Fehler, verstrichene Spielzeit und Pause.
  Zeitlimit oder abgeschlossenes Messfenster beweist kein Produktionsziel.
- Pflanzchargen: Vorschau setzt nichts. Apply-Beleg zählt gesetzte, bereits
  vorhandene, ausgeschlossene, unbestätigte und nicht versuchte Markierungen.
  Statusbeleg ist historisch, kein neuer Auftrag oder aktueller Wachstumsnachweis.
  Auch eine wieder gelieferte Apply-Quittung bedeutet keine erneute Ausführung.

Ergebniszeilen verdichten validierte Fachberichte; für Entscheidungen bei Unsicherheit
weiterhin deren strukturierte Nachweise verwenden. Kein automatisches Nachführen alter
Startzeilen, keine zusätzlichen Hintergrundabfragen. Sammelkonfiguration und übrige
Auftragsfamilien erhalten in diesem Teil noch keine Ergebniszeilen.

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


Bei "Ueberpruef mal meine letzten Aktionen" ab 0.54 auch page.groups lesen:
Gruppen sind nach Rasterkachel/Hoehe und gleichem Zustandswechsel gebuendelt,
keine ausgefuellten Rechtecke. Cursor/Limit zaehlen Rohereignisse. Bei Bedarf
mit demselben after/limit/Filter detail=full lesen. Markierung, reale Pflanze,
Stumpf und Erntewirkung unterscheiden. source=unknown nicht als Mensch ausgeben.
spatialTrackingComplete und spatialObservationFailures zusaetzlich beachten;
bei Luecken frisch beobachten. Keine automatischen Ruecknahmen.

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

Pflanzablauf ab 0.53: `inspect_area_types` und passende Arbeitsgebaeude bestimmen,
dann `preview_planting_batch` mit session, neuer jobId (UUID) und rectangles.
Je Rechteck x/y/z/width/height/resource/workBuildingId; requireMoist standardmaessig
true. false ist eine bewusste Freigabe trockener/unbekannter Feuchte, niemals eine
Umgehung von nativer Pflanzpruefung oder Arbeitsreichweite. Geeignete und
abgewiesene Zellen vor Auftrag beurteilen; bei Bedarf detail=full lesen.
Exakt dieselben rectangles plus key an `apply_planting_batch`; danach nur
`inspect_planting_batch`. Bei unconfirmed nicht neu senden, reale Markierungen
abgleichen. Nach neuer Spielsession keine alten Auftraege wiederholen.
Erfolg bedeutet Pflanzmarkierung. Gewachsene Pflanzen erst durch getrennte
Vegetationsbeobachtung nach einem begrenzten Simulationslauf nachweisen.

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

Für 0.52: `plan_cleared_building_group` mit neuer jobId als UUID und konkreter spec lesen;
bei passender Räumvorschau genau diese spec/clearanceKey einmal starten. Danach
`inspect_cleared_building_group`, kein Advance je Pflanze/Gebäude. Für Belege
`detail=full` am selben Job lesen. Zeitbudget passend zu Arbeitsschichten wählen.
Bei interrupted/unconfirmed native runId/actionId abgleichen, keine neue jobId als
Wiederholung. Rohstoffstapel dürfen im Baugebiet bleiben; Spiel verschiebt sie.
Teilplan nach Räumung ist kein Auftrag: Lage/Zugänge prüfen und neu entscheiden.

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

Für Gruppen zuerst Plan lesen: `selected` ordnet Wunschindex und Schrittindex zu;
`unplaced` enthält ausgelassene Wünsche, nicht ausführbare Platzhalter. `score`,
`search.stopReason` und `searchSummary` erklären die beste geprüfte Auswahl.
`complete` bedeutet vollständig geplant, nicht fertig gebaut. Nur zurückgegebene
`steps`/`planKey` unverändert ausführen. Nach Umplanung oder neuen Reservierungen
neu planen. Teilplan nicht stillschweigend um ausgelassene Gebäude ergänzen.

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

## 0.45: Ausführungsgewissheit und kurze Laufberichte (begrenzt live abgenommen)

Bei Fehlern error.execution lesen: not_executed bedeutet kein schreibender
Spielauftrag übergeben; Argument/Ursache korrigieren. unconfirmed bedeutet
Transportübergabe begonnen, Ergebnis lesend anhand bestehender IDs klären.
Kein automatisches Retry. ledger_required verweist auf gespeicherte Bauchargen,
not_applicable bezeichnet Lesefehler. Fachliche data.outcome-Belege bleiben gültig.
Laufberichte sind standardmäßig kompakt. Für vollständige Belege denselben Lauf
mit inspect_simulation_run(..., detail=full) lesen, keinen Start wiederholen.
Historische Messung und separat datierte aktuelle Diagnose trennen. Compact lässt
bekannte Null-Zeitkategorien und Rohbelege weg, verändert keine Messwerte.
[Details](action-results-and-run-reports.md).

## 0.44: über ein Zeitfenster messen (begrenzt live abgenommen)

Für Durchsatz `run_simulation_for` mit buildingIds (maximal acht) und
measureProduction=true verwenden. Der Lauf endet an der Zeitgrenze statt beim
ersten Zyklus je Gebäude. Danach measurement unter buildingProduction lesen:
Zykluszahl, measuredHours, Raten, recipeDerivedProducts und times.
Äußeres completedCycles bleibt der Erstnachweis 0/1. Zeitfensterabschluss ist
kein Nachweis, dass jedes Gebäude produziert hat. Pause separat bestätigen.
Zeitanteile sind beobachtete Schätzungen; unknown/multiple_blockers nicht in
exakte Ursachen umdeuten. Aktuelle 0.43-Diagnose und eingefrorenen Messbericht
trennen. Abgeleitete Rezeptmengen sind weder Lagerzuwachs noch Liefermengen.
Für Vergleiche erst Lauf beenden, dann Einstellungen ändern und neuen Lauf starten.
Keine enge Pollschleife; bei Fehlern letzte Messabdeckung beachten. Ohne Option
weiterhin günstigen Erstzyklusnachweis nutzen. [Details](production-throughput.md).

## 0.43: Engpässe gemeinsam erklären (begrenzt live abgenommen)

`inspect_building_operations` mit `diagnose=true` und höchstens acht IDs verwenden.
Zuerst assessment.findings, dann die zugehörigen bottleneck-Belege lesen:
Zutaten/Brennstoff lokal und global, passende Quellen/Produktlager samt Wegstatus,
Personal und Kraft. Erreichbares Lager mit Platz zuerst für Abtransport nutzen;
keinen Lagerneubau allein aus output_capacity_unavailable ableiten. complete=false
oder unbekannter Weg ist kein Negativnachweis. Globale Transportmengen nicht dem
Zielgebäude zuschreiben. Vorschläge ausführen nur als getrennten kontrollierten Auftrag.
Bei buildingIds-Läufen liefert inspect_simulation_run dieselbe aktuelle Diagnose
separat datiert; sie darf eingefrorene Produktionsnachweise nicht umdeuten.
Für günstige Übersicht diagnose weglassen (weiter bis 32 IDs). Keine enge Pollschleife.
[Umfang und Nachweisgrenzen](production-bottlenecks.md).

## 0.42: mehrere Gebäude gemeinsam einrichten (begrenzt live abgenommen)

Vorher native Rezepte und tatsächliche Einstellungen lesen. `configure_buildings`
mit bis acht vollständigen Zielen und passenden expected-Werten verwenden.
Sollpersonal mindestens 1; Deaktivierung über paused=true, nicht über Soll 0.
Simulation muss pausiert und frei von aktiven Läufen sein. Alle Vorbedingungen
werden gemeinsam geprüft; Teilfehler stoppen die weitere Einrichtung. Kein Replay.
Nach unbestätigtem Aufruf `inspect_building_configuration` mit derselben jobId.
Optional proofHours/proofSpeed/proofMaxRealSeconds gemeinsam angeben; anschließend
Produktion über `inspect_simulation_run` mit runId=jobId prüfen. applied ist nur
Einrichtungserfolg, kein Betriebsnachweis. [Vertrag](building-configuration-batch.md).

## 0.41: Produktion je Gebäude nachweisen (begrenzt live abgenommen)

Bis acht fertige Manufakturen zuerst per Sammeldiagnose auswählen und konfigurieren.
Dann einmal `run_simulation_for` mit `buildingIds` und Zeitgrenzen starten.
Nicht mit Bestands-/Forschungszielen mischen. Native Produktionsabschlüsse werden
in der Mod gesammelt; keine Agentenaktion je Gebäude. Nach Laufende Gesamtnachweis,
Teilnachweise und Blockaden getrennt berichten. `targetReached` ist kein
Produktionsnachweis. Nach Änderungen neuer Lauf; nach unbestätigtem Start nur lesen.
[Vertrag, Grenzen und Pilot](building-production-proof.md).

## 0.40.0 — Änderungen seit dem letzten Kontakt

Bei „Überprüf mal meine letzten Aktionen“
`inspect_recent_actions` mit frischer session, limit und gegebenenfalls after
aufrufen. page.nextCursor auch bei leerem Ergebnis merken; Cursor je Session und
Filter führen. Ohne Cursor letzte X Einträge, keine vollständige Vorgeschichte.
Herkunft unknown nicht als menschlich ausgeben. gap/dropped, Startzeit,
trackingComplete und observationFailures prüfen; bei Lücken aktuelle Objekte
gezielt neu lesen. Konflikte erklären, menschliche Änderungen nicht automatisch
zurücknehmen. Keine unbegrenzten Vollscans oder Pollschleifen.
[Abdeckung und Pilot](recent-actions-journal.md).

## 0.39.0 live abgenommen — Such- und Diagnoseeffizienz

`find_buildings` gezielt mit template, finished oder
productionOnly verwenden. Wege/Treppen sind standardmäßig ausgeschlossen;
includePaths=true bei Wegeaufgaben. Bereiche filtern den Anker einschließlich Z,
nicht den Gebäudegrundriss. Geometrie nur mit detail=full unter geometry.
Gefundene IDs gemeinsam an `inspect_building_operations` übergeben (maximal 32,
aktuelle session). not_found und unbekannte Belege nicht als Erfolg interpretieren.
Full nur für Inventare/Statustexte nachladen. Inaktive Kraftverbraucher sind kein
Beweis einer kaputten Verbindung. [Vertrag und Pilot](building-query-batch.md).

## 0.38.0: Rezeptwahl und Inbetriebnahme — begrenzt live abgenommen

`inspect_building_settings` für
Rezeptoptionen lesen, `set_building_recipe` mit dem gelesenen `expectedRecipe`
aufrufen und Auswahl separat bestätigen. Nur native Optionen fertiger Gebäude.
Rezept-IDs immer aus dem konkreten Gebäude lesen: derselbe generische Pfad gilt
für andere Fraktionen einschließlich Iron Teeth; keine Folktails-IDs voraussetzen.
`inspect_building_operation.assessment` bündelt belegte Blockaden, Warnungen und
Unbekanntes einschließlich Kraft in einem Aufruf. Bereitschaft und erfolgreiche
Auswahl beweisen keine Produktion; Output separat im begrenzten Produktionslauf
nachweisen. Keine automatische Wiederholung bei Konflikt oder unbestätigter
Aktion. [Vertrag, Grenzen und Pilot](recipe-operation-settings.md).

## Waldreichweite und Fällschutz — begrenzt live abgenommen

Folgeoptimierung live bestanden: intern größere Seiten und gezielte
Schlussmarkierungslesung. Aufruf unverändert; gemessen 32 statt 54 `nativeReads`
für Übersicht, 40 statt 113 für zwei Schutzänderungen. Vollständige Zählung und
Gegenkontrolle bleiben erforderlich.

Für die Rohstoffsuche eines Arbeitsgebäudes `inspect_forestry` mit `fullRange=true`
verwenden: komplette native Reichweite auf allen Höhen statt einzelner Fenster.
`species` trennt lebende Bäume, Stümpfe, tote/unbekannte Zustände, Reife und Ertrag.
Baumnachweis stammt aus `vegetation.isTree`, nicht allein aus der Försterkategorie;
Löwenzahn ist dort pflanzbar, aber kein Baum. Fehlender Nachweis blockiert Vollabdeckung.
`coverage`, `rangeCells`, `scannedRegions` und `nativeReads` prüfen; gekürzte
Fällkandidaten sind nicht die vollständige Artenzählung. Budgetfehler nicht als
leere Reichweite interpretieren.
Für beispielsweise Kiefern `protect_forestry` mit exakter Katalogvorlage, Gebäude,
Session und neuer Aktions-ID verwenden. Ein Aufruf entfernt die aktuellen
Fällmarkierungen passender lebender Bäume, einschließlich junger Bäume.
`completed` und `readbackConfirmed=true` verlangen; Teilfortschritt/unbestätigte
Ziele zunächst lesen, gleiche ID nicht als neuen Auftrag verwenden. Kein Schutz
gegen zukünftige Markierungen, keine Harz-/Liefergarantie. Andere Arten, Stümpfe,
Pflanz- und Abrissmarkierungen unverändert lassen. [Pilot](forestry-range-protection.md).

## Große Geometrie — regulärer Großbau live belegt

Installiertes Paket über `inspect_build_options.geometryCellCount/maxGeometryCells`
prüfen: bis 512 native Blöcke, auch feste `Single`/`Hex`-Vorlagen. Fehlende Felder
bedeuten älteres Paket; 0.37.0 allein belegt die Erweiterung nicht. Native Formen
und Untergrundzellen vollständig prüfen, keine gefüllten Bounding-Boxen erfinden.
Großprojektgrenzen bleiben 32 Teile, 32×32, acht belegte Höhen. Sonderstandorte
wie Mine bleiben Sache der nativen Validierung. Ohne ebenerdigen Eingang den
expliziten Großprojektpfad verwenden. `supported` ist kein Bau-/Betriebsnachweis.
DomedGarden samt automatischem Zugang live fertig; Hex-Platzierung, eingangslose
Großgebäude und Sonderstandorte ausdrücklich zurückgestellt. Keine pauschale
Abnahme aus dem Katalog ableiten. [Nachweis und Grenzen](large-building-geometry.md).

## Produktionsziele — gezielt live abgenommen

Für Materialwartezeit `run_simulation_for` mit `goodTargets` (bis acht eindeutige
`good`/`amount`) und optional `stopAtScience` verwenden. Zuvor verfügbare Bestände
und Produktionsmöglichkeiten lesen. Feste Spiel-/Echtzeitgrenzen wählen; alle Ziele
müssen gleichzeitig erreicht sein. Nicht mit `stopAtAvailableLogs` kombinieren.
Nach einem Start nur lesen; Mod pausiert selbst. Zielerfolg ausschließlich anhand
`productionTargetsReached`, `reason` und bestätigter Pause melden, nicht aus
`completed` oder `targetReached` allein. Zeitablauf ist kein Produktionsnachweis.
`productionDiagnostics` ist eine frische, begrenzte Manufakturbeobachtung;
terminale Zielbestände sind historisch eingefroren. Zutaten-, Personal-, Kapazitäts-
und Netzbefunde beachten, unbekannte Lieferung/Windursache nicht erfinden.
[Vertrag und menschliches Testgate](production-target-runs.md).

## Bauarbeitszugang: Policy 1 begrenzt live abgenommen

Für neue Gebäude bevorzugt `plan_large_project_access`: Zielgebäude vorgeben,
Wege/Treppen/Stützen automatisch ergänzen lassen. Bei der neuen Policy muss der
verschachtelte Plan `accessPolicyVersion=1` melden und jede erforderliche
`constructionWorkAccess`-Prüfung `road_reachable`. Bei Ablehnung `blockedPart`,
`phase` und Teilbericht auswerten; keine teilweise erzeugten Schritte bauen.
Zur Ausführung nur den vollständigen zurückgegebenen Plan samt Schlüssel verwenden.
Zugangshilfen müssen als Abhängigkeiten vor ihrem Ziel fertig werden; die Mod
prüft den aktuellen Weganschluss unmittelbar vor jedem Gebäudeauftrag erneut.

`road_reachable` belegt einen Distriktweg zu einer geometrischen Arbeitszelle,
noch keinen nativen Bauarbeiterzugang. `helper_runtime_check` ist eine ausdrücklich
ungeprüfte Bauhilfe. Tatsächlichen Baustellenzugang und fertigen Eingang weiterhin
separat bestätigen; bei Blockade Diagnose/Reparaturablauf verwenden, nicht wiederholen.
Alte Antworten ohne Policy sind kein Nachweis der neuen Prüfung. Erst nach Gate
und bestandenem Pilot gemäß [Abnahmeplan](automatic-access-planning.md) als getestet behandeln.
Temporäre Hilfen nur nach Abschluss und frischer Zugangsprüfung entfernen;
Rückgewinnungsrohstoffe niemals löschen.

## Automatische Zugangsplanung — begrenzt live abgenommen

Für vorgegebene Gebäudeziele `plan_large_project_access` verwenden: ergänzt
Wege, Treppen, Stützen und Abhängigkeiten im begrenzten Umfeld und prüft den
Gesamtplan. Nur bei `candidateFound=true` die zurückgegebenen `steps` und
`plan.planKey` unverändert an `run_large_project` übergeben. Kein Bau im Planaufruf.
`assessment` und bei Ablehnung `reason` lesen; nicht mit den ursprünglichen,
unergänzten Schritten starten. Nicht gefundene Route bedeutet keine generelle
Unbebaubarkeit. Arbeitszelle ist vorgeschlagen, echter Baustellenzugang ungeprüft.
`cleanupCandidateIndices` nach Fertigstellung erneut auf Bestandszugänge prüfen;
Rohstoffstapel bleiben erhalten. [Umfang und Dreifall-Pilot](automatic-access-planning.md).

## Projektabschluss und Reparatur — begrenzter Livetest bestanden

Fünfteilige Zugangsreparatur samt Originalbau, Abschluss und Folgeprojekt in
derselben Sitzung geprüft. Belegte Wegzelle und fremde Baustelle abgewiesen:

- `diagnose_large_project` liest aktuelle Objektidentität, Bauzugang, Material-
  bereitschaft und betroffene Bestandsverbindungen. `lastGuardFailure` bzw.
  `lastValidationFailure` sind historische Fehlerbelege, andere Werte frisch.
- Terminales Projekt bei Pause mit `close_large_project` abschließen.
  Ausschließlich `closeOutcome=released/already_released` bestätigt Freigabe;
  `rejected` anhand der Diagnose klären. Restteile werden aufgegeben, nicht gebaut.
  Unbekannte Wirkungen nie durch eine neue ID umgehen.
- Fehlender Bauzugang: Projekt stoppen, terminalen Zustand und Pause nachlesen,
  dann `plan_large_project_repair` mit `parentActionId` und zusätzlichen Wegen,
  Treppen/Plattformen. Bekannte Originalbaustellen sind erlaubter Bestand;
  keine fremden Baustellen oder verschachtelten Reparaturen einschließen.
- `run_large_project_repair` mit neuer ID, frischem Plan und festem Budget einmal
  starten. Die Charge wartet auch auf beauftragte Originalteile; keine Wiederholung
  ursprünglicher Platzierungen. Danach Originalprojekt schließen, Rest frisch planen.
- Temporäre Bauten einschließlich Straßen anschließend gezielt entfernen und
  rücklesen. Dauerhafte Gebäudezugänge zuvor prüfen. Rückgewinnungsstapel nicht
  mit `remove_debris` löschen: Rohstoffe erhalten und regulär einsammeln lassen;
  nötige Zugänge bis zur Sammlung belassen. Nach menschlichem Eingriff frisch lesen.

Grenzen und konkreter Pilot: [Projektwiederherstellung](project-recovery.md).

## Gesamtstruktur 0.63 — mit dokumentierter Grenze abgeschlossen

Live belegt: ebener Gesamtauftrag, Reparaturabschluss, Schema-2-Charge sowie
manueller Gesamtstart mit Weg auf unfertiger Plattform. Ein unabhängiger Stapel
wurde trotz materialblockierter Höhenbauteile fertig. Separater Vertikalpilot:
fünf Teile einschließlich beider oberer Wege gemeinsam bestätigt. Ebener Pilot:
zwei Wege und Lager gemeinsam beauftragt und fertig/zugänglich nachgewiesen.
Objektverlust wird ohne Replay gemeldet; geänderter Plan unter belegter ID konkret
abgewiesen. Teilablehnungen mitten im Durchgang mit ausdrücklicher Nutzerfreigabe nur synthetisch im Gate geprüft;
fertige Höhennavigation nicht neu live belegt. Versionsabnahme siehe Projektstand.
Nachbesserung: konkrete Startcodes unterscheiden Pause, Isolation, Distrikt,
Ledger und Geometrie. Nur explizite Vorauftragsablehnung belegt `not_executed`;
generischer Konflikt oder Transportverlust bleibt `unconfirmed`, niemals blind wiederholen.

`plan_large_project` → **einmal `run_large_project`**: alle Teile in einem
Main-Thread-Durchgang platzieren, erst danach Bauzustand und Probleme beobachten.
Zwischen den Aufträgen gibt es keine Material-, Zugangs-, Initialisierungs- oder
Fertigstellungsprüfung. `dependsOn` ist nur die topologische Einreichreihenfolge,
keine Wartebedingung. Auch Treppen, Plattformen, obere Wege, Kraft- und Wasserbau
werden gemeinsam eingereicht. Ob die installierte Spielversion einen konkreten
Auftrag tatsächlich angenommen hat, muss die Rückbeobachtung zeigen.

Planparameter plus `actionId`, `mode=development_pilot`, `speed` (1/3/7),
`maxGameHours` (1–672), `maxRealSeconds` (30–86400). Plan und Start pausiert;
nach gemeinsamer Platzierung läuft die native Simulation automatisch bis zum
Bauabschluss oder Zeitlimit. Keine Advance-Aufrufe pro Bauteil. Platzierungsfehler
werden gesammelt; übrige Teile werden trotzdem versucht. Unbestätigte Wirkung
niemals wiederholen. Bei Fehlern ID, Position, reason und exceptionType auswerten.
`inspect_large_project` bleibt lesend, `diagnose=true` ergänzt frische Material-/
Zugangsdiagnose. `completed` verlangt alle Teile fertig und Zugang beobachtet,
beim autonomen Lauf zusätzlich bestätigte Pause. Kraft/Produktion separat prüfen.

Neue `start_building_batch`-Chargen planen die Gebäude gemeinsam samt Zugang
und starten einen solchen Gesamtauftrag. Zusatzregionen sind alternative
Standorte für die ganze Gruppe. Räumung erfolgt bei Bedarf vor der Bauphase.
`constructionHours` ist jetzt ein gemeinsames Budget. Nach Ende per
`advance_building_batch` den lokalen Beleg und optionale Lagerkonfiguration
abschließen. Alte Schema-1-Journale behalten ihren alten Fortsetzungsvertrag.

Ebene und vertikale Baupiloten platzieren ebenfalls alle Teile vor Beobachtung:
`ordered` = bestätigte Aufträge, `completed` = alle Teile fertig. Ihre einfachen
Startwerkzeuge besitzen kein eigenes Simulationsbudget; für autonomes Tempo und
Abschluss den Gesamtplan über `run_large_project` verwenden. `start_large_project`
platziert jetzt sofort und beobachtet automatisch, ohne selbst Tempo zu setzen.
Einzelplatzierungen bleiben Einzelaktionen und können als Teile eines Gesamtplans
zusammengefasst werden. [Vertrag/Pilot](construction-structures-0.63.md).

Die folgenden ausdrücklich versionierten Altabschnitte dokumentieren frühere
Abnahmen. Ihre Einzelschritt- und Auftrags-completed-Semantik gilt nicht für 0.63.

## Erweiterung 0.37.0: größere Bauvorhaben (begrenzter Livepilot bestanden)

Nach Stützenkorrektur: `placementState=deferred_native_validation` bedeutet
noch keine gültige Geometrie. Direkt abhängige geplante Träger müssen zuerst
real fertig sein; Advance muss dann die vollständige native Prüfung bestehen.
Nicht als bestandene Vorschau oder Ausführungsfreigabe zusammenfassen.

Nur mit bestätigter Bridge 0.37.0: `plan_large_project` → `start_large_project`
→ `advance_large_project`. Explizit alle Stützen, Treppen, Wege, Gebäude und
Kraftstücke mit `dependsOn` angeben. Höchstens 32 Teile, 32×32 Grundfläche und
acht belegte Höhenebenen einschließlich Gebäudevolumen. Keine automatische
3D-Geländeroutensuche annehmen. Katalog/Freischaltung zuerst prüfen.

Plan/Start/Advance verlangen Pause. Ein Advance setzt höchstens einen Auftrag;
Vorgänger müssen wirklich fertig und anwendbare Zugänge nachgewiesen sein.
Bei `waiting` begrenzte Simulation separat ausführen, Pause bestätigen und
erneut Advance. `inspect_large_project` liest nur den gespeicherten Stand.
`stop_large_project` behält bestehende Objekte. Bei `unconfirmed`/`stopped`
diagnostizieren, keine neue Action-ID als Retry. `pilotEligible` bedeutet keine
reguläre Baufreigabe und keinen vollständigen Bauarbeiter-Vorabnachweis.

Kraftlinks im Plan sind Anschlussgeometrie. Nach dem Bau
`inspect_power_network` für reale Ports/Netzleistung lesen, Betrieb getrennt
prüfen. Entrancelose Bauteile brauchen keinen erfundenen Gebäude-Eingang.
Neue Distrikte, Gelände-Seitenanbauten und Mehrfacheingänge bleiben ausgeschlossen.
[Genaue Parameter, Zustände und Livepilot](large-projects-0.37.0.md).
Fünfteiliger Stützenpilot am 2026-10-10 vollständig fertiggestellt; Grenzen und
Einzelnachweise stehen im verlinkten Pilotbericht, keine pauschale Katalogabnahme.

## Abgenommene Grundlage und Lifecycle

Codex-Lifecycle (live abgenommen): vor der Testübergabe
`timberborn_server_control(action=stop)`, nach menschlichem `live bereit`
`action=start`, dann Spielstatus und aktuelle Session prüfen. Der Vorschaltprozess
bleibt verbunden. Unterbrochene Aufträge nie ungeprüft erneut senden.
[Einmalige Einrichtung und Grenzen](codex-mcp-supervisor.md).
Das menschliche Vorbereitungsskript startet seit 0.57 nach erfolgreichen Tests,
Installation und Paketprüfung Timberborn automatisch. Spielstand laden und
`live bereit` bleiben menschliche Schritte; der Agent startet das Gate nicht selbst.

Stand: Bridge 0.74.0 mit abgeschlossenen Journal-Korrekturen; aktualisiert am 17. Oktober 2026. Abgenommener Stand siehe
[Projektstand](../PROJECT_STATE.md). Versionsnummer allein unterscheidet die
zuletzt mehrfach gebauten Pakete nicht; Abnahme und Paketstand beachten.
Diese Anleitung beschreibt die vorhandenen MCP-Fähigkeiten. Aktueller Auftrag
und Projektregeln stehen in [AGENTS.md](../AGENTS.md) und
[missionsplan.md](../missionsplan.md); das lokale Spiel ist das freigegebene
Testsystem. Ein Entwicklungsauftrag ist kein fortlaufendes Kolonieziel.

Cancel-Nacharbeit begrenzt live abgenommen: Der Codex-Supervisor begrenzt
Statuswarten auf eine Sekunde. Cancel erreichte ihn im Pilot erst nach der
Statusantwort; tatsächliche Zustellung blieb vor dem Supervisor seriell.
Keinen sofortigen/parallel zugestellten Notstopp versprechen. Nach Cancel
Pause frisch nachlesen; keine automatische Wiederholung.
[Vertrag und Grenzen](lightweight-status-wait.md).

## Einstieg in eine geladene Sitzung

Die vorhandene MCP-Verbindung des jeweiligen Clients verwenden. Der Client
verwaltet seinen stdio-Prozess; der Agent startet für Spielaufgaben keine
zusätzlichen Start-/Buildskripte und wechselt keinen Server für Schreibzugriff.
Die tatsächlich verfügbaren Aktionsgruppen aus dem geladenen Schema prüfen.
Der Codex-Verbindungsablauf steht unter [MCP-Lifecycle](codex-mcp-supervisor.md).

Technische Aktionsschalter und Budgets aus dem installierten Fähigkeitsbericht
und aktuellen Schema lesen. Experimentelle Budgetänderungen sind keine zusätzliche
Live-Abnahme und keine allgemeine Freigabe hoher Grenzwerte. Ein Serverneustart
setzt spielseitige Budgets nicht zurück; Legacy- und Gruppenmodi getrennt behandeln.
1. `timberborn_status` für Bridge-Version und frische Sitzungskennung lesen;
   anschließend `inspect_simulation` für Geschwindigkeit und Spielzeit.
   A07-Pilot (Gate und begrenzter Liveabgleich bestanden): `data.mcp.buildId` und `catalogId`
   mit dem aktuellen MCP-Gatebeleg vergleichen. `buildScope` benennt die geladenen
   Module, `catalogScope` den Backend-Katalog einschließlich Opt-ins, ohne
   Supervisor-Zusätze. `clientCatalogState=not_observed` bedeutet: Clientcache
   unbekannt. Abweichung oder fehlende Kennung klären, keine automatische
   Neustart-/Wiederholungsaktion; Kennung beweist keine Liveabnahme.
   [Kennungsvertrag und Grenzen](mcp-runtime-identity-pilot.md).
2. `inspect_building_capabilities(session, reasoning)` für Projektmodi,
   technische Grenzen und historische Nachweise lesen. Bei unbekanntem Profil
   die Fähigkeiten nicht aus einer ähnlichen Version ableiten.
3. Nur die für die Aufgabe nötigen Daten lesen: etwa `inspect_colony`,
   `inspect_goods`, `find_buildings`, `inspect_map_region`, `inspect_construction`
   oder `inspect_building`. Seiten anhand `hasMore` vollständig lesen, wenn eine
   Aussage Vollständigkeit voraussetzt. IDs und Bauorte aus aktuellen Antworten
   übernehmen, keine historischen Testkoordinaten als Voraussetzung verwenden.

Jeder Werkzeugaufruf braucht `reasoning`: kurze lesbare Absicht, 1–600 Zeichen.
Das Feld heißt nicht `reason`. Verbindliche Argumente und Grenzen stehen im
aktuellen MCP-Eingabeschema. Strukturierte Werkzeuge verwenden.

## Effizienter Ablauf mit dem abgenommenen Stand

1. Sitzung und Tempo frisch lesen. Für Flächensuche/Baustart pausierten Zustand
   verlangen; keine historischen Sitzungs- oder Objekt-IDs übernehmen.
2. `inspect_colony_overview` bündelt Bevölkerung, Betten, Personal und Bedürfnisse.
   Es zeigt nur drei Beispielgüter, nicht die gesamte Nahrung. Benötigte Güter
   über `inspect_goods` lesen; Warnflags allein sind kein Notfall und kein
   Gesamtwert für Zufriedenheit. Globaler Vorrat ist kein Liefernachweis.
3. `survey_region` bündelt bis 16×16 auf einer Bodenhöhe. Für Anbau
   `workBuildingId` des fertigen Farm-/Forstgebäudes und `planBuildings=false`
   angeben; template und districtId bleiben Pflichtfelder. `rows` zeigt Hindernisse,
   `moisture.rows` Bodenfeuchte, `workRange.rows` die native Arbeitsreichweite.
   `workRange.moistEmptyGroundPatches` enthält bis sechs freie, feuchte Rechtecke
   innerhalb dieser Reichweite. Fehlende Reichweite bleibt unbekannt; vollständige
   Abfrage auf 2048 Zellen/64 Seiten begrenzt. Keine Personal-, Pflanzenarten- oder
   Ertragsgarantie. `soilIsMoist=null` bedeutet unbekannt/nicht anwendbar.
4. Für Bauen `planBuildings=true` (Standard): bis drei 8×8-Fenster mit vier
   Drehungen; höchstens vier Alternativkandidaten, keine gemeinsam nutzbare
   Chargenkapazität. `nativeReads`/`planningCalls` zeigen Aufwand. Unbekannte
   Geometrie sperrt betroffene Teilflächen; unknown ist nicht frei. Keine
   Kandidaten bedeuten nicht grundsätzlich unbebaubar. Frische native Bauprüfung
   bleibt erforderlich. Details: [Regionalsuche](regional-survey.md).
5. Ein Gebäude: `develop_building_project` plant, beauftragt und begleitet den
   Entwicklungspilot. `advance_building_project` setzt dasselbe feste Budget
   fort. `waitSeconds` hier 0–20; `inspect_building_completion` liest den Stand.
   `awaiting_order` ist kein fehlgeschlagener Bau; `awaiting_simulation` erlaubt
   erst nach Auftrags-/Baustellenzugangsprüfung einen begrenzten Lauf.
6. Mehrere Gebäude: `start_building_batch` mit 1–8 Einträgen; Basisfläche und
   optional bis drei `additionalRegions`, jede höchstens 8×8. Bei fehlendem
   Standort automatischer Wechsel in Listenreihenfolge, kein Rücksprung.
   `advance_building_batch`, `waitSeconds` 0–45; kein Hintergrunddispatcher.
   `regionIndex`, `previousRegions` und `finished` getrennt lesen. Räumung bei
   Bedarf einmal je Fläche, maximal 64 Ziele insgesamt. Zeitbudget:
   clearanceHours × Flächenzahl + constructionHours × Gebäude höchstens 672 h.
   `clearing=all_vegetation` räumt alle passenden natürlichen Pflanzen der Fläche,
   nicht nur den Gebäudegrundriss. Offene/unbestätigte Eingriffe oder erschöpfte
   Räumzeit stoppen die Charge. Materialmangel ist keine Startsperre; nach Auftrag
   Lieferung abwarten und Zeitgrenzen getrennt beachten. Keine automatische Forschung.
   Gleiche batchId liest nur den gespeicherten Stand; geänderte Parameter werden
   abgelehnt. Gestoppte Chargen nicht nachträglich erweitern. Details:
   [Bauchargen](building-batch-workflow.md).
   Lagergut/-modus bereits im Auftrag mitgeben; Lagerart vorher prüfen.
7. `finished_accessible` bestätigt Fertigstellung und tatsächlichen Zugang,
   nicht Lieferung, Besetzung, Produktion oder Zufriedenheit. Diese getrennt
   lesen. Gestoppte Chargen nicht durch neue IDs blind wiederholen; erst Grund
   und Zustand klären. `checkpointOnly=true` ist gespeicherter, kein frischer Stand.

### Zeitläufe, Felder und Diagnosefallen

- Begrenzte Zeitläufe pausieren am Ziel. Beim Speichern kann `game_speed_locked`
  mit Tempo 0 auftreten; derselbe Lauf bleibt aktiv und behält sein Ziel/Budget.
  Keine zusätzliche Beschleunigung senden. Explizite Pause unterbricht weiterhin.
  Eingefrorener Unterbrechungsbeleg kann noch das alte Tempo enthalten; aktuelle
  Pause separat über `inspect_simulation` bestätigen.
- Parameterfehler können derzeit pauschal als möglicherweise unbestätigte Aktion
  erscheinen. Erst lesend feststellen, ob Auftrag/Zeitlauf existiert; nur nach
  geklärtem Vor-Ausführungsfehler den Parameter korrigieren. Keine blinden Retries.
- `inspect_removal_targets(kind=vegetation)` schließt passend markierte Pflanzen
  aus. Für Pflanznachweise `kind=all` oder `planted` lesen. Markierung allein ist
  weder Pflanzung noch Ernte; Leben, Wachstum und Wasserstress gesondert prüfen.
- Bei `set_area` bedeutet fehlende Markierung `expectedResource=unmarked`, nicht
  leerer Text. Gelände-Z, ganze Grundrisse und bestehende Pflanzaufträge beachten.
  `inspect_work_range` belegt Reichweite, aber keine tatsächliche Erntefähigkeit.
- Gezielte Piloten vor Skalierung; Aufwandsalarm nach Projektregeln. Bauchargen
  sparen Modellentscheidungen, beseitigen aber keine Wirtschaftsengpässe.
- Gebäudenachweise in [building-coverage.csv](building-coverage.csv) aktualisieren:
  fertige reguläre Vorlagen zählen, nicht Baustellen; Entwicklerwerkzeuge anhand
  expliziter Katalogklassifikation ausschließen, nicht allein anhand des Namens.

Aktuelle Bevölkerung, Vorlagenabdeckung und laufende Arbeiten stehen im
[Projektstand](../PROJECT_STATE.md); historische Koordinaten sind keine Bauaufträge.

## Bauphasen, Pause und Recovery ab 0.64

0.64.0 ist begrenzt live abgenommen; [Belege und Grenzen](releases/0.64.0.md).
`awaiting_access` bezeichnet ein identitätsbestätigtes fertiges Teil ohne aktuellen
Anschluss, etwa die Landung hinter unfertigen Plattformen. Kein fehlendes Objekt,
kein fertiger Gesamtzugang: gleiche Projektkennung bis Fertigbau oder Budgetende
beobachten. Unbekannter Bauzugang und Identitätsfehler bleiben Stopgründe.
Diagnose behält positive Identitätsbelege auch nach Stop. Nach nachgewiesenem
Rückbau kann `close_large_project` freigeben; fehlende nie bestätigte Objekte
bleiben unaufgelöst. Diagnose/Close platzieren nichts erneut.

Ab 0.64 ergänzt `inspect_simulation` `activeRunId` und `pendingSpeed`. Nach
unbestätigtem Start zunächst dieselbe Laufkennung und diese aktuelle Übersicht
lesen. Ein fehlender Lauf beweist nicht allein, dass keine Wirkung eingetreten ist.
Tempoänderungen wirken verzögert; ein unmittelbar passender Wert oder historisches
`pauseConfirmed` ersetzt keine frische Beobachtung nach der Übergabe. Bei
`pendingSpeed != null` steht noch eine Tempoabsicht aus. Frühere 0.63-Läufe zeigten
falsche terminale Pausenbelege. Start/Cancel, explizite MCP-Tempo-Unterbrechung,
Höhenbau bis 3/3 fertig und Recovery mit Folgeauftrag ohne Reload sind jetzt live
belegt. Antwortverlust und Rennen innerhalb desselben Updates bleiben synthetisch.

## Generisches Gebäude mit Anschlussweg bauen

Seit 0.35.0 braucht der ebene Projektpilot keine Gebäudenamensliste mehr.
`templateSelection=native_catalog_supported_geometry_with_road_entrance` benennt
den dynamischen Umfang. Dabei bedeutet `objectTemplates=[]` keine feste Liste;
zulässige Vorlagen werden im tatsächlichen Spielkatalog ermittelt.

1. `inspect_build_options` seitenweise lesen, maximal 32 Einträge pro Seite.
   Vorlage, Verfügbarkeit, Freischaltung, Geometrie und Kosten prüfen. `supported`
   ist kein Livenachweis. Globale Vorräte garantieren keine Materiallieferung.
   `inspect_build_catalog` ist dagegen nur der alte Katalog mit zwei Pilotvorlagen.
2. Fertiges Distriktzentrum und passende Wege bestimmen. `inspect_path_district`
   prüft die native Verbindung eines Wegobjekts zu diesem Distrikt.
   Relevante bestehende Gebäudezugänge mit `inspect_building_access` festhalten.
3. `precheck_building` für konkrete Standorte oder `plan_building_project` für
   eine kleine Suchfläche verwenden. Der vollständige gedrehte Grundriss zählt,
   einschließlich äußerer und höher liegender belegter Zellen. Ein freier
   Ursprung oder ein Weg neben dem Gebäude genügt nicht.
   Seit 0.35.1 berücksichtigt die Suche bereits das Limit von vier neuen Wegen:
   Überlange Kandidaten werden übersprungen, die Suche läuft danach weiter.
   Vorhandene Wege werden bevorzugt; bei gleichem Neubedarf die kürzere Route.
   Das ist keine globale Standortoptimierung und kein nativer Sicherheitsnachweis.
4. Ein Plan enthält Ursprung, Drehung, Eingang, Anschlussziel, Wegzellen und
   `planKey`. Suchparameter unverändert mit `optionIndex` und `planKey` an
   `validate_building_project` geben. Planoptionen sind noch keine Bauaufträge.
5. Im Prüfbericht Platzierung, Wegpräfixe, Eingang, Bestandsverbindungen und
   Vorschau-Wiederherstellung beurteilen. `valid=true` allein genügt nicht.
   `blocked` bezeichnet einen konkreten Fehler, `unknown` eine Nachweislücke.
   Im Entwicklungspiloten bleibt ausschließlich die dokumentierte Bauphasen-
   Vorabnachweislücke zulässig; bekannte Fehler oder Verluste bleiben Ablehnungen.
6. Pausiert und ohne unabhängige offene Baustellen
   `execute_building_project_pilot` mit denselben Planparametern,
   `mode=development_pilot` und neuer `actionId` ausführen. Der Server prüft
   frisch und platziert Wege und Gebäude schrittweise über die Spielservices.
7. `inspect_building_project` bis zum bestätigten Ergebnis lesen.
   `completed` bestätigt Auftrag und Zugang. Danach mit `inspect_building`
   und `inspect_building_access` den tatsächlichen Baustellenzustand und
   `buildersReachable` prüfen.
8. Falls nötig einen begrenzten Lauf mit `run_simulation_for` oder
   `run_simulation_until` starten; Status über `inspect_simulation_run` verfolgen.
   Fertigstellung, tatsächlichen freien/zugänglichen Eingang, neue Wege und
   relevante Bestandsanschlüsse anschließend erneut lesen. Automatische Pause
   und erreichtes Zeitziel prüfen. Betrieb oder Lagerbelegung separat behandeln.

### Technische Grenzen des ebenen Einzelprojektmodus

- Suchfläche maximal 8x8, Objektgeometrie maximal 512 native Blockeinträge, eine Drehung und
  eine Bodenwegebene pro Anfrage; Grundriss und gesamte Route innerhalb der Suche.
- Höchstens vier neue Bodenwege je ebenem Projekt. Das aktuelle Projektledger
  hat Kapazität 1024; alte Angaben zu vier Projekten sind historisch. Aktuellen
  installierten Umfang über inspect_building_capabilities prüfen. Neues Projekt
  erst nach abgeschlossenem Vorgänger und fertigen Baustellen.
- Öffentliche Spielgeometrie liefert derzeit einen Gebäude-Eingang. Mehrere
  beobachtete `constructionAccess.cells` sind Bauzugänge, keine weiteren Türen.
- Ebenes Routing erfindet keine Verbindung über einen Höhenunterschied.
  `execute_vertical_stair_pilot` bietet separate feste Treppen-/Plattformmodi;
  Umfang und gemeinsame Sitzungsgrenze aus dem Fähigkeitsprofil lesen. Der feste
  vertikale Lagerablauf verwendet weiterhin SmallWarehouse.Folktails.
- Allgemeiner Bauphasen-Vorabnachweis weiterhin unvollständig; reguläres
  `place_building` bleibt bei fehlendem `safe` gesperrt. Die generischen
  Projektpiloten besitzen ihren eigenen ausdrücklich bezeichneten Modus.

## Hindernisse, Fehler und Wiederholungen

Bei leerem Plan einen konkreten Grundriss und seine Hindernisse prüfen, bevor
weitere Flächen durchsucht werden. `inspect_removal_targets` liest überlappende
Objekte mit exakten IDs. Freigegebene Räumung erfolgt über die passenden
Entfernungswerkzeuge. `marked=true` bestätigt einen Auftrag; erst `removed` oder
das Rücklesen der freien Zelle bestätigt die Entfernung. Pflanzmarkierungen
können Nachwuchs auslösen und werden separat behandelt.

Bei `stopped`, `unconfirmed` oder Verbindungsabbruch zunächst Projektstatus und
reale Objekte lesen. Keine neue ID als Umgehung eines unklaren Ergebnisses senden.
Identische `actionId` und Parameter liefern den gespeicherten Projektstatus;
dies erzeugt keine weiteren Bauobjekte. Das Ledger endet mit der Spielsitzung.

## Nachweisstand richtig nutzen

In 0.35.0 sind Bank und großes 3x3-Freiluftlager, jeweils Drehung 1, mit neuen
Wegen, Bau-/Fertigzugang, Bestandskontrolle und Replay live belegt. Eine einzelne
Kollision am äußeren Eckfeld verhinderte den großen Bau korrekt. Eine gefährliche
Kontrollvorschau erkannte 24 verlorene Verbindungen, einschließlich eines Zugangs
auf der oberen Ebene. [Details](generic-building-project.md).

Seit 0.35.1 enthält das Fähigkeitsprofil auch diese beiden historischen Abnahmen.
Die Fünf-/Vier-Wege-Grenze und das Weitersuchen nach einem zu langen Kandidaten
sind in einem kontrollierten ebenen Streifen live geprüft; Anschluss um einen
Baum samt Bau-/Fertigzugang separat bestanden. [Details](road-budget-planner.md).
Historische Nachweise ersetzen keine frische Standortprüfung. Spiel-Multi-Eingänge,
Direkt-fertig-Distriktzentrum und beliebige Katalogobjekte sind nicht pauschal
live abgenommen. Lagergut, Personal, Produktion und Versorgung separat prüfen.

## Holzdiagnose und kompakte Chargen ab 0.35.5

Bei Pause inspect_forestry für eine begrenzte Holzfällerregion verwenden.
Ausgewachsen bedeutet nicht fällbar: Baumreste und fehlender Holzertrag sind
separat ausgeschlossen. Nur angebotene IDs an mark_forestry übergeben;
completed bestätigt Markierungen, keine Ernte. Gleiche actionId liest den Beleg.
run_simulation_for kann mit stopAtAvailableLogs früh pausieren; stockTargetReached
vom normalen Zeitlimit targetReached unterscheiden. Aktuelle Pause und Ernte
bei Bedarf getrennt nachlesen. Bauchargen sind standardmäßig kompakt;
details=true nur für Such-/Verlaufsdiagnose. build_rejected ist terminal,
keine Wiederholung mit neuer ID ohne Diagnose und geänderte Voraussetzungen.
[Liveumfang und Grenzen](forestry-efficiency-0.35.5.md).

## Optionales Spielbild ab 0.36.0

Ein Blickauftrag im Spielkontext ist ein ausdrücklicher Screenshot-Abruf:
auch allein „schauen“, „SCHAUEN!!!!“, „schau mal“, „guck mal“, „sieh hin“,
„Screenshot jetzt“, „ich schau genau drauf“ oder „da wo ich hinschaue“.
Großschreibung, Ausrufezeichen und offensichtliche Tippfehler ändern den Auftrag
nicht. Aktuelle Sitzung ermitteln, einmal `capture_screenshot` aufrufen und den
zurückgegebenen Bildblock tatsächlich ansehen, nicht nur Metadaten lesen.
Keine vorherige Objektmarkierung, Koordinaten oder zusätzliche Bestätigung
verlangen. Erst wenn Bild und strukturierter Abgleich das Ziel nicht klären,
gezielt nachfragen. „Schau in den Code“ ist dagegen kein Spielbildauftrag.
Der Spieler stellt zuvor Kamera und UI im Spiel
selbst ein; diese Ansicht unverändert erfassen. Das zurückgegebene Bild ansehen
und auf den gezeigten Sachverhalt antworten. Die Aufnahme entsteht beim Aufruf,
nicht rückwirkend beim Absenden der Nachricht. Kein Ingame-Knopf erforderlich.

capture_screenshot mit aktueller session liefert auf ausdrücklichen Abruf ein
Spielbild inklusive UI. Standard 1280×720; kleinere maxWidth/maxHeight sparen
Bilddaten, reduzieren aber die Lesbarkeit. Metadaten enthalten Position,
absolute Welthöhe (Unity Y), Blickrichtung, Zielabstand, Bildwinkel und Bildweite
auf der Ebene durch den Kamerazielpunkt. Keine Höhe über Gelände und keine
sichtbare Geländegrundfläche daraus behaupten. Nur ergänzende Orientierung;
Spielaktionen weiterhin auf strukturierte Zustandsdaten stützen. Keine Dateien
oder Bildhistorie anlegen. Mindestens zwei Sekunden zwischen Aufnahmen;
bei screenshot_unavailable Zustand klären, kein automatischer Retry.
[Nachweis und Grenzen](screenshot-0.36.0.md).

## Bauchargen-Frühabschluss (live geprüft am 2026-10-10)

Nach bestandenem Entwickler-Gate und Livetest nutzt advance bei fertig
beobachteten Bauobjekten die frühe Pause des zugehörigen Laufs. Erst bestätigte
Pause und frischer Zugang erlauben den Folgeauftrag. constructionHours bleibt
Obergrenze; waitSeconds begrenzt nur Warten, nicht sofort mögliche Übergänge.
Nach Rückgabe weiter advance derselben Charge aufrufen; kein Hintergrunddispatcher.
Ein cancelled-Lauf mit cancel_requested kann nach vollständiger Abschlussprüfung
zu finished_accessible gehören. inspect bleibt lesend; unbestätigte Pause zuerst
lesend klären. Zwei Lager mit Frühpause und unmittelbarem Folgeauftrag live geprüft.


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
