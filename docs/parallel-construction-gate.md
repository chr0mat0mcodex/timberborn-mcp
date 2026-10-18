# Parallele Bauprojekte und Projektpriorität — begrenzter Abschluss

Stand 2026-10-18: menschliches Gate erfolgreich, Liveabnahme noch nicht vollständig.
API-Version weiterhin 0.74.0. Geladene Build-/Katalogkennungen gegen Gate geprüft.
Neue Prioritätswerkzeuge wurden erst nach Aktualisierung der Clientverbindung sichtbar.
Der Nutzer akzeptiert die dokumentierte Teilabnahme am 2026-10-18 ausdrücklich
als begrenzten Abschluss und beauftragt Commit/Push des geprüften Stands.
Der gültige Sperr-/Umwegvergleich bleibt im Backlog offen; keine weitere Suche.
Dies erweitert weder Werkzeugfreigaben noch den belegten Schutzumfang.

## Livebefund 2026-10-18

### Abschließende Teilabnahme auf Nutzerentscheid

Gezielter Fertigübergang bestanden: Neues kleines Wächterlager geplant und
bestätigt, dann seine Baustelle pausiert. Der aktive Projektguard erfasste fünf
Altbaustellen einschließlich Stapellager und Plattform. Zwei Industriebaustellen
während des Materialtests pausiert, Plattform auf VeryHigh gesetzt. Sieben reife
tote Bäume regulär zur Räumung markiert; nach 24 Spielstunden Räumung erledigt,
Plattform mit Material-/Baufortschritt. Eine weitere gezielt geprüfte Kiefer und
ein anschließendes Sechs-Stunden-Fenster vollendeten das geschützte Stapellager.

Frische Endbelege: Stapellager fertig, Eingang frei und zugänglich, Distriktdistanz
39, `constructionAccess.state=not_construction`. Gleichzeitig Wächter weiter
`waiting`, `lastGuardFailure=null`, `lostConnections=0`; dessen Ausgangsdiagnose
enthielt genau diese Altbaustelle. Beide Zeitfenster endeten ohne Überschreitung
mit bestätigter Pause. Damit realer Übergang einer fremden Baustelle zu einem
fertigen Gebäude mit Eingang unter aktivem Guard belegt, nicht nur ein eigener
Projektabschluss oder eine eingangslose Plattform.

Sperr-/Umwegplanung: Gelände-/Stützhöhen am reparierten Höhenengpass und native
Belegungsregeln geprüft. Niedrige Welle am unteren Anschluss bereits im Precheck
wegen belegtem Path `object_intersection`; keine weitere ungültige Sperrvorschau.
Die zuvor gültige Plattform lässt den Weg bestehen, die zuvor sperrende Treppe
war ungültig. Für eine freie Anschlusszelle konnte kein belastbarer gültiger
Sperrkörper samt gleichem Körper im Umwegfall festgelegt werden. Das beweist
keine generelle Unmöglichkeit; der verlangte Vorab-Schutzbeleg bleibt offen.

Nach Aufwandsalarm entschied der Nutzer ausdrücklich: Teilabnahme festhalten,
keine weitere Suche. Zwölf Steuerungsaktionen in diesem Pilot; keine neue
Codeänderung, kein Agent-Build/Test/Deploy. Industriepausen wieder aufgehoben und
rückgelesen; Wächterprojekt gezielt gestoppt. Wächterlager bleibt pausiert/unfertig,
Altplattform unfertig/erreichbar. Endzeit Tag 52, Stunde 21,375, Tempo 0, kein Lauf.
Zu diesem Stoppzeitpunkt noch kein Feature-Commit/Push; anschließend ausdrückliche
Annahme des begrenzten Abschlusses und Sicherungsauftrag wie oben dokumentiert.
Nur der gültige Sperr-/Umwegvergleich bleibt als fachlicher Restnachweis offen.

### Aktuelle Restabnahme nach geklärtem Benutzereingriff

Der Nutzer bestätigte den vorherigen Tempokonflikt ausdrücklich als Benutzereingriff
und beauftragte die Fortsetzung. Kein ungeklärter MCP-Tempodefekt mehr behauptet.

- Materialpilot: Sollbesetzung im Bauzentrum von zwei auf vier erhöht, später vier
  tatsächlich zugewiesene Bauarbeiter bestätigt. Ein Lauf über 16 Spielstunden
  bei Tempo 7 endete nach 44,11 realen Sekunden ohne Überschreitung und mit Pause.
  Beide Prüflager danach fertig, Eingänge frei und zugänglich (Distriktdistanzen
  24 und 31). Drei markierte Bäume verschwunden. Räumwirkung und Bauabschluss
  belegt, einzelne Quelle-Ziel-Lieferungen und kausale Prioritätswirkung nicht.
- Zwei Engpasskandidaten geprüft: Plattform über Zuweg nativ gültig, keine
  verlorene Verbindung; quer gerichtete Treppe auf derselben Zelle nativ ungültig,
  35 verlorene Verbindungsproben, Rücknahme bestätigt. Der zweite Kandidat zählt
  ausdrücklich nicht als gültiger Sperrnachweis. Keine weitere Kandidatensuchserie.
- Unabhängige Guardkontrolle: gültiges Plattformprojekt neben vier Industrie-
  baustellen gestartet, Auftrag und Zugang bestätigt. Einen bekannten oberen
  Weg während eines begrenzten MCP-Laufs entfernt. Projekt `stopped/guard_changed`,
  Zeitlauf `cancelled/construction_guard_changed`, nach 1,125 Spielstunden Pause
  bestätigt. 32 verlorene Bestandsproben, darunter beide fertigen Lagereingänge.
- Genau denselben kostenfreien Weg per Mehrprojekt-Reparatur mit Elternjournal
  wiederhergestellt. Vorschau schützt fünf Altbaustellen einschließlich bereits
  erreichbarer Teile. Automatisch `completed/all_parts_finished_and_access_observed`
  nach 0,15625 Spielstunden; Pause bestätigt, Journal erfolgreich freigegeben,
  obwohl alle fünf anderen Baustellen unfertig bleiben. Endkontrolle aller fünf
  Builderzugänge true; beide fertigen Lager frei und zugänglich.

Endzustand: Tag 51, Stunde 15,375, Tempo 0, kein aktiver Lauf/Tempoauftrag.
Bauzentrum Soll/Ist vier; zusätzliche Plattformbaustelle bleibt erreichbar und
unfertig, ihr Guard-Testjournal terminal gestoppt. Sie wird nicht als fertiger Bau
oder laufend überwachtes Projekt ausgegeben. Sieben Steuerungsaktionen in dieser
Fortsetzung (Personal, zwei Zeitläufe, Projektstart, Wegabriss, Reparatur, Freigabe),
zusätzlich drei Plan-/Validierungsvorschauen; unter der Grenze von 20.

**Zu diesem Zwischenstand offen:** gültiger entfernter Sperr-/Umwegvergleich und
Fertigübergang unter aktivem Guard; letzterer ist inzwischen oben nachgewiesen.
Fertigbau der zwei ursprünglichen Prüflager und neuer Guardgrund sind nun belegt.
Keine vollständige Featureabnahme und kein Commit/Push. Nächster Schritt: einen
geometrisch gültigen Sperr-/Umweg-Testaufbau konkret entwerfen, statt weitere
ungültige Überlagerungen auf bestehenden Wegen zu testen.

### Vorherige Teilabnahme und zwischenzeitlicher Stopp

Erneutes menschliches Gate erfolgreich: 2389 Unit-Tests und 19 Integrationstests
bestanden, drei optionale Integrationsfälle übersprungen. Geladene Build- und
Katalogkennung gegen Gate bestätigt; Backend verbunden, Session unverändert.

- Mehrprojekt-Reparatur ohne Elternjournal geplant und einmal beauftragt:
  eine obere Treppe stellt den Zugang zu sechs bekannten unerreichbaren
  Altbaustellen wieder her. Vorschau erhält den Bestand und wird zurückgenommen.
  Nach tatsächlicher Fertigstellung alle sechs Baustellen erreichbar, Treppe
  fertig und zugänglich. Reparaturjournal trotz sechs unfertiger Altbaustellen
  freigegeben. Das ursprüngliche Acht-Stunden-Budget endete zuvor am Zeitlimit;
  kein Beleg automatischer Fertigstellung innerhalb dieses Budgets.
- Fremdes Industrie-/Lagergebäude im Reparaturplan mit `repair_helpers_only`
  ohne Auftrag abgelehnt. Veraltete Prioritätsrevision mit
  `project_priority_revision_changed`/`not_executed` abgelehnt; VeryHigh blieb
  beim Rücklesen unverändert. Ungültige Vorlage in normaler Planung behauptet
  keinen Bauauftrag mehr. Gültige Nahkontrolle weiter gesperrt.
- Ein kontrollierter Sechs-Stunden-Lauf bei Tempo 7 endete ohne Überschreitung
  mit bestätigter Pause. Lager A erreichte ebenfalls zwei von drei Holz und
  60 Prozent Baufortschritt; B blieb dort. Drei markierte reife tote Bäume
  blieben bestehen: Räumauftrag und möglicher Ertrag sind keine Lieferung.
  Zwischenzeitliches Tempo 7 ohne MCP-Lauf wurde vom Nutzer als manuell bestätigt;
  dieses Zeitfenster ist kein kontrollierter Simulationsnachweis.

Erneut freigegebener Resttest: höchstens 20 Steuerungsaktionen, Stopp beim ersten
unbestätigten Eingriff oder widersprüchlichen Kontrollfall. Bereits Aktion 1,
`set_simulation_speed` von erwartet 7 auf 0, lieferte
`invalid_argument`/`unconfirmed`. Direkte Rückabfrage: Tempo 0, kein aktiver Lauf,
kein ausstehender Tempoauftrag, Tag 50 um 22,09375 Stunden. Ingame-Aktionsjournal
bestätigt den unconfirmed-Beleg, nennt aber keinen konkreten Auslöser. Kein Retry,
kein weiterer schreibender Eingriff, kein neuer Simulationslauf. Die beobachtete
Pause allein beweist nicht die Ursache oder erfolgreiche Zuordnung des Auftrags.

Frische Bestandsdiagnose: beide Prüflager weiterhin unfertig, jeweils zwei von drei
Holz, Baufortschritt 60 Prozent, Priorität VeryHigh; beide Builderzugänge erreichbar
und Eingänge nicht blockiert. Bauzentrum zwei von zwei Sollarbeitern, zwei weitere
Worker ohne Zuordnung; nachts keine laufenden Jobs. Sägewerk besitzt drei Holz im
eigenen Inventar, global verfügbar null. Drei Räumbäume weiterhin markiert mit je
zwei potenziellen Holz. Kein Personal-, Prioritäts- oder Rezeptwechsel ausgeführt.

**Historischer Diagnosebefund, inzwischen als Benutzereingriff geklärt:**
`invalid_argument` nach Transportübergabe ohne konkreten Ablehnungsgrund bei
anschließend beobachteter Pause. Ursache offen; keine bewiesene Falschmeldung
oder belegte erfolgreiche Aktion aus dem damaligen Beleg allein. Nutzerklärung
und nachfolgende erfolgreiche Läufe stehen oben; kein offener Tempodefekt.
Zusätzliche Verbesserung: Räumdiagnose sollte Arbeitszugang, Jobzuweisung und
Lieferung getrennt erklären; aktuelle Worker-/Ertragsdaten belegen diese Kette nicht.

**Zum damaligen Stopp offen:** Fertigzugang der Prüflager, gültiger entfernter Sperr-/Umwegvergleich,
Fertigstellung geschützter Altteile und neuer Guardgrund `construction_guard_changed`
im Livefall. Resttest entsprechend Stoppregel beendet, keine vollständige Abnahme,
kein Commit/Push. Nächster Schritt ist die gezielte Diagnose des Tempofehlerbelegs.

### Historische Vorbereitung der Nachkorrektur

Erster Nachkorrektur-Gatelauf: MCP/Tests kompiliert, aber Prioritäts-Revisionskontrolle
fehlgeschlagen (unconfirmed statt not_executed). Beide neuen Prioritätscodes in der
zentralen Bridge-Allowlist ergänzt; Regression prüft Konstruktor und unveränderten
Transportcode. Das anschließende erfolgreiche Gate ist oben dokumentiert.

Implementierter Vertrag der Nachkorrektur: Mehrprojekt-Reparatur erfasst alle
aktuellen Baustellen (maximal 32, ein Distrikt) mit Identität, Geometrie und
bekanntem Ist-Zugang. `parentActionId` ist optional; nach Reload ohne Eltern-ID
planen. Das ist Bestandsprüfung, keine Journalrekonstruktion. Bekannte unerreichbare
Baustellen dürfen für reine Zugangshilfen bestehen. Unbekannte oder unvollständige
Zugangsdaten bleiben Pflichtsperre. Noch verbundene Zugänge in jedem Vorschau-
Zwischenstand erhalten, Vorschau vollständig zurücknehmen. Während des Bauens
auch wiedergewonnene Zugänge vor erneutem Verlust schützen. Abschluss verlangt
fertige Hilfen und erreichbare Altbaustellen/fertige Eingänge, keinen Industriefertigbau.
Alte Projekte bleiben terminal; keine Wiederholung alter Aufträge.

Fehlerzuordnung: `project_plan_existing_access_lost` für bekannte Unerreichbarkeit;
`project_priority_revision_changed`/`project_priority_identity_unavailable` werden
vor allen Settern als `not_executed` berichtet. Generische Konflikte/Transportfehler
bleiben unconfirmed. Fehlgeschlagene Planvorschau behauptet keinen Bauauftrag;
unbekannte Vorschau-Rücknahme bleibt sichtbar. Guard-Zeitstopp nennt
`construction_guard_changed` statt eines angeblichen Benutzer-Cancels.

Vorbereitete Regressionen: bekannte/unklare Mehrprojektteile, falsche Baseline,
Verlust eines verbleibenden Zugangs, optionale/ungültige Eltern-ID nach Reload,
Revisionsfehler gegen Transportverlust, Vorschau-Fehlermeldung, Guard-Pausenbeleg
und versionierter Mehrprojekt-Diagnosevertrag. Beim damaligen Übergabestand nur
Quell-/Diffprüfung; spätere Gate- und Livebelege stehen oben.

Nächster Livepilot nach Gate, maximal sechs Fälle, keine Suchserie:
1. Aktuelle Session und Bestand frisch lesen. Ohne altes Ledger Zugangshilfen
   planen; unbekannte Alt-IDs weder erfinden noch erneut beauftragen.
2. Naher Hilfsbau zum Wiederanschluss mehrerer unerreichbarer Baustellen muss
   zulässig sein, Industriegebäude im Reparaturplan weiterhin abgewiesen.
3. Wiederanschluss in einer begrenzten Charge; tatsächlichen Zugang aller
   erfassten Baustellen bestätigen, Abschluss trotz unfertiger Industrie.
4. Veraltete Prioritätsrevision ohne Setterwirkung, echte Vorschaufehler ohne
   behaupteten Auftrag; unbekannte Identitäten bleiben gesperrt.
5. Offener gültiger Sperr-/Umwegvergleich; bei falscher Negativkontrolle sofort stoppen.
6. Materialversorgung gezielt prüfen, kurzer Parallelbau bis mindestens einem
   Fertigzugang; Guardverlust nur mit vorbereitetem Wiederanschluss kontrollieren.

### Bereits beobachtete Teilabnahme vor der Nachkorrektur

- Zwei getrennte kleine Lagerprojekte neben vier vorhandenen Industriebaustellen
  gleichzeitig beauftragt und mit verschiedenen Entity-IDs als Baustellen bestätigt.
  Zweite Vorschau schützt fünf Altbaustellen, Zugang erhalten, Rücknahme vollständig.
- A von Normal auf Low, B von Normal auf VeryHigh gesetzt und frisch rückgelesen.
  Veraltete A-Revision beim Versuch High abgewiesen; danach A weiterhin Low und
  B weiterhin VeryHigh. Bauzentrum-Arbeitspriorität danach Normal; mangels separater
  Vorhermessung kein vollständiger Vorher-/Nachherbeleg für Arbeitsplatzprioritäten.
- Freier naher Lagerplatz separat nativ valid=true; Großprojekt lehnt denselben
  Platz mit project_plan_foreign_site_nearby ab. Path über A ebenfalls wegen
  XY-Abstand abgelehnt; dessen Träger-/Geometriegültigkeit nicht separat bewiesen.
- Tatsächlicher Bauarbeiterzugang von A über vorhandene Höhentreppen positiv.
  Gemeinsamer Lauf Tempo 7: 4,15625 Spielstunden, anschließend Pause bestätigt,
  keine beobachteten Zugangsverluste. Beide Lager weiterhin ohne Material-/Baufortschritt.
  Bestand an verfügbaren Logs beim Start null, aktive Erzeuger vorhanden, Lieferung unbewiesen.
- Geplante echte Guard-Negativkontrolle: genau eine zuvor identifizierte obere
  Treppe entfernt. A danach buildersReachable=false statt true, 31 verlorene
  Bestandsproben. Beide Projekte stopped/guard_changed. MCP-Zeitlauf automatisch
  cancelled, Pause bestätigt; kein Cancel vom Agenten gesendet. Laufbegründung
  nennt nur cancel_requested und unterscheidet den Guardauslöser nicht.
- Endzustand: Tempo 0, kein aktiver Lauf oder Tempoauftrag. Treppe bleibt für die
  Negativkontrolle entfernt; Plateauzugang unterbrochen, beide Projekte unfertig.
  Testsystem nicht wiederhergestellt. Keine Bauaufträge wiederholt.

**Offen:** gültiger entfernter Sperrkandidat mit gleicher Eingriffsgeometrie und
erhaltenem Umweg, tatsächlicher Fertigbauzugang, Fertigstellung geschützter Altteile.
Der echte Treppenabriss belegt die Laufzeitwache, nicht die hypothetische Bauvorschau.
Keine falsche Freigabe beobachtet, aber kein vollständiger Schutz-/Featureabschluss.

**MCP-Befunde:** Revisionskonflikt wird als generisches state_conflict/unconfirmed
ausgegeben, obwohl anschließend unveränderte Werte belegt wurden. Eine falsche
Vorlage Path.IronTeeth ergab auch bei der nicht auftragserteilenden Planung den
irreführenden Text „Schreibender Spielauftrag … Ausgang unbestätigt“; korrekte
Katalogvorlage ist Path. Nach Guardverlust ergibt eine weitere Planung
project_plan_unresolved_identity, obwohl frische Diagnose construction_unreachable
bei bekannter Entity zeigt: Identität und Erreichbarkeit werden im Fehlergrund
vermischt. Reparatur bleibt auf ein Elternprojekt ohne andere offene Projekte
beschränkt. Nächster Schritt: gezielter Wiederanschluss-/Testaufbauplan für mehrere
betroffene Projekte und verlässliche Materialversorgung, keine Blind-Simulationsverlängerung.

## Verhalten

`plan_large_project`, `plan_large_project_access` und `start_large_project` erlauben
räumlich getrennte offene Baustellen im unten begrenzten Nachweisumfang.
Mehrere `start_large_project`-Aufträge lassen sich bei Pause gemeinsam vorbereiten
und mit einem `run_simulation_for` bauen. Jeder Auftrag behält IDs, Fortschritt,
Abschluss und Fehlerzustand. Ein beendetes Zeitbudget wird nicht wiederaufgenommen.
`run_large_project` darf neben eindeutig bestätigten älteren Baustellen starten,
besitzt aber weiterhin exklusiv den einen Simulationscontroller. Vor einem weiteren
Start dessen Lauf gezielt beenden und die Pause bestätigen. Keine konkurrierenden
Geschwindigkeitsregler, keine automatische Budgetverlängerung.

`inspect_large_project_priority(session, actionId)` liefert je Teil Zustand und
Baupriorität sowie `revision`. `set_large_project_priority` übernimmt diese als
`expectedRevision` und setzt `priority` auf VeryLow/Low/Normal/High/VeryHigh.
Vor allen Settern werden sämtliche Identitäten und die Revision geprüft.
Fertige/unbeauftragte Teile werden übersprungen; bereits passende Werte bleiben.
Der erste unbestätigte Setter stoppt weitere Änderungen. `partial_unconfirmed`
und Einzelbelege auswerten, frisch lesen, keine blinde Wiederholung. Kein Rollback,
keine Arbeitsplatzpriorität und keine garantierte Material-/Arbeiterzuteilung.

## Zugangsvertrag und Grenzen

- Vollständige Inventur: nicht initialisierte und widersprüchliche Objekte sperren
  weiterhin, auch wenn vorher bereits eine fremde Baustelle gefunden wurde.
- Frühere unbestätigte oder unklar zugeordnete Aufträge sperren neue Planung.
  Diagnostik darf Identitäten bestätigen; Start wiederholt keinen alten Auftrag.
- Höchstens 32 bestehende Baustellen, eine fertige Distriktzentrale,
  regulär lesbare `ConstructionSiteAccessible`-Zugänge (höchstens 64 je Baustelle).
  Erweiterte Baustellen-Reachability bleibt ausgeschlossen. Unbekannt ist keine Freigabe.
- Neue belegte Zellen halten mindestens ein freies Feld Abstand in XY zu den
  vollständigen Baustellengeometrien **und** deren Arbeitszugängen. Unterschiedliche
  Höhe allein gilt nicht als Trennung. Nahe/gestapelte unabhängige Projekte bleiben gesperrt.
- Tatsächlicher Builderzugang und leere Vorschau müssen übereinstimmen. Jede
  Baustelle benötigt zusätzlich einen tatsächlichen Arbeitszugang am nativen Distriktweg.
- Jeder topologische Vorschau-Zwischenstand und die Gesamtstruktur müssen alle
  vorher verbundenen Bestandsproben und einzelnen Baustellenzugänge erhalten.
  Ein entfernter Wegeengpass wird daher nicht durch bloße Entfernung freigegeben.
  Vorschau-Rücknahme prüft auch die einzelnen Baustellenzugänge, nicht nur deren Summe.
- Während des Bauens bleiben Identität, tatsächlicher Builderzugang und bestehende
  Wegverbindungen unter Beobachtung. Fertig gewordene Fremdteile benötigen ihren
  fertigen Eingang/Wegeknoten. Neue nicht zugelassene Baustellen stoppen die Überwachung.
  Bei Guardverlust wird ein MCP-eigener aktiver Zeitlauf abgebrochen; manuell
  eingestelltes Tempo wird dadurch nicht überschrieben.
- Temporäre native Fertigvorschau bleibt ein begrenzter Entwicklungspilot;
  `regularExecutionAllowed=false` und die bestehenden Nachweisgrenzen bleiben erhalten.
  Gleichwertigkeit aller Bau-/Fertigphasen ist erst Gegenstand der Livekontrollen.
- Reparaturen verwenden den oben beschriebenen gesonderten Hilfsbauvertrag;
  der XY-Abstand entfällt nur dort zugunsten vollständiger Vorschau-/Ist-Prüfung.
  Alte Einzel- und Vertikalpiloten behalten ihre Isolation.
- Wasserführung, Rückstau, Abfluss, Materialversorgung und Betrieb sind separate
  Nachweise. Diese Änderung beweist keinen wirksamen Badwater-Schutz.

## Menschliches Gate

Backend vor Übergabe über den verbundenen Supervisor stoppen und Gatesperre
bestätigen. Danach keine Spiel-MCP-Aufrufe bis `live bereit`.

```powershell
./scripts/prepare-human-live-test.ps1 -TimberbornManagedDir '<Managed-Verzeichnis>'
```

Das Gate beendet Timberborn ohne Speichern, führt Tests und Release-Build aus,
paketiert, installiert und prüft die Paketdateien. Danach Spiel laden und
`live bereit` melden. Erst nach erfolgreichem Gate und folgendem Livetest committen/pushen.

## Begrenzte Liveabnahme

Maximal sechs fachliche Fälle; bei erster falscher Negativkontrolle oder
ungeklärtem Zugangsverlust stoppen. Keine lange Kolonieentwicklung.

1. Zwei getrennte, regulär erreichbare Baustellenprojekte: zweiten Plan/Start
   zulassen, während erstes noch unfertig ist. Beide IDs/Objekte rücklesen;
   keine Wiederholung des ersten Auftrags. Auch vorhandene Industriebaustellen
   als Altbestand prüfen, sofern deren Zugänge den obigen Vertrag erfüllen.
2. Projekt A auf Low und B auf VeryHigh setzen; alle offenen Teile frisch prüfen.
   Eine vorher gelesene Revision absichtlich veralten lassen: Setter darf keine
   weitere Änderung ausführen. Arbeitsprioritäten bleiben unverändert.
3. Gültige nahe Kollisions-/Arbeitszugangsvariante ablehnen. Höhenunterschied allein
   darf die XY-Trennung nicht umgehen. Keine neue Entity.
4. Gültige räumlich entfernte Variante, die einen bestehenden Zugang gefährdet,
   ablehnen; eine Variante mit erhaltenem Umweg zulassen. Gleiche Eingriffsgeometrie
   und reale positive/negative Zugangsbaseline verwenden; eine ungültige Vorschau
   zählt nicht als ausreichende Negativkontrolle. Historische Grenze aus 0.31.4 beachten.
5. Höhenzugang (bestehende Treppe/Plattform) erhalten: Vorschau und tatsächliche
   Baustellen-/Fertigzugänge getrennt rücklesen. Guardverlust in kleinem kontrolliertem
   Fall muss den MCP-Zeitlauf stoppen, nicht weiter disponieren.
6. Kurzer gemeinsamer Lauf: Fortschritt beider Projekte getrennt beobachten,
   mindestens ein Projekt fertig und zugänglich bestätigen. Fertigstellung eines
   geschützten Altteils darf keinen unbegründeten Identitätsfehler erzeugen.

Neue synthetische Fälle: räumlicher Abstand, Diagonale, fehlende/abweichende
Zugangsbelege, einzelner verlorener Alternativzugang, unbekannte Altaufträge,
zwei unabhängige Ledger-Abschlüsse, Prioritäts-Teilfehler ohne Folgeschreibzugriffe,
Revisions-/Rücklesevertrag und Katalog-/Schalterbindung. Ausführung ausschließlich im Gate.
