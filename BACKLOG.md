# Backlog

## Aktuelle Arbeit

Fortschrittsdiagnose im begrenzten Umfang abgeschlossen; kein Gate ausstehend.

## Aktuell offene Folgearbeiten

- Produktionsketten vor Bau gemeinsam planen: Zutaten, Lager/Transport, Personal
  und Kraft (B07); derzeit noch kein vollständiger Ausbauplan.
- Recovery: unbekannte Objektidentität deterministisch live nachweisen. Keine
  ungezielten Zeitrennen oder Save-Manipulation; Testmechanismus noch zu entscheiden.
- Parameterfehler bei Kartenabfragen um konkrete Kartengrenzen ergänzen.
- Weitere Zugangsgeometrien, insbesondere unterer Zielweg an gedrehter
  Bestandstreppe, gezielt live nachweisen.
- Projektwiederaufnahme nach Spielneuladen (B06) und allgemeine Kraft-/Höhenrouten
  (B03) bleiben eigenständige größere Arbeitspakete.

## Zuletzt abgeschlossen

- Fortschrittsdiagnose: Ausgangswerte, Materialwarten und Fertigstellung gegen
  Einzelabfragen live bestätigt. Zwei feste Zeitfenster stoppen ohne Verlängerung;
  Compact/Full erhalten die Messpunkte. Positive numerische Teildeltas weiterhin
  Gate-Nachweise. [Belege und Grenzen](docs/construction-progress.md).

- Bezirksweite Arbeitszeitdiagnose vom Nutzer abgenommen: Zentrum und Bauhütten
  gemeinsam; genaue unterschiedliche Schichten synthetisch geprüft. Keine weitere
  Live-Schichtprüfung offen. [Belege](docs/operating-readiness.md).
- Automatische Betriebsdiagnose vor längeren Läufen, Material ohne pauschale
  Startsperre; Gate und begrenzte Livepiloten bestanden.
- Notstopp/Statuswarten: eigenes Warten begrenzt, verbleibende Clientserialisierung
  eingegrenzt; kein offener Auftrag für weitere Transportversuche.

## Historische Nachweise und Entwicklungskatalog

Die folgenden Abschnitte dokumentieren damalige Zwischenstände. Formulierungen
wie „Gate offen“, „fehlgeschlagen“ oder „nächster Schritt“ gelten für den genannten
Zeitpunkt und werden durch die aktuelle Übersicht oben ersetzt. Der spätere
Entwicklungskatalog enthält zusätzliche Ideen, keine bereits beauftragten Arbeiten.


Bezirksweite Bauarbeitszeit **begrenzt live abgenommen**: Zentrum und
Bauhütten gemeinsam prüfen. Aktiver Job oder besetzte unpausierte Bauhütte in
Arbeitszeit verhindert gemeinsamen Feierabend; unbekannte Belege bleiben null.
Einzelner Feierabend am Zentrum ist keine Baupause im ganzen Bezirk.
[Vertrag, Grenzen und Pilot](docs/operating-readiness.md).



Arbeitszeitdiagnose erweitert, **Gate und Livepilot bestanden**: Feierabend am beobachteten
Arbeitsplatz wird konkret benannt. Frische Baudiagnose nutzt den Schichtstatus
des zuständigen Bezirkszentrums; unbekannte Werte und andere Blockaden bleiben
getrennt. Kein automatisches Verlängern von Laufbudgets.
[Vertrag und Pilot](docs/operating-readiness.md).


## Abschluss 2026-10-18 — automatische Betriebsdiagnose begrenzt live abgenommen

Gate: 1964 Unit- und 13 Integrationstests bestanden, drei optionale Live-Tests
übersprungen. Separater Auto-Reader nach Client-Neustart verfügbar: Auswahl von
DistrictCenter und zwei LumberMills ohne IDs; Diagnose bis auf Zeitstempel
identisch zur manuellen Gegenprobe. Simulation trotz 33 Prozent Deckung und
blockierter Produktion gestartet, nach sechs Stunden automatisch pausiert.

Baupilot: eine Lodge mit einem neuen Zugangsweg, gemeinsam nativ geplant,
pilotEligible und Vorschau wiederhergestellt. Vorprüfung im Compact-Startbeleg:
full_planned_costs_including_helpers, exakt 12 Holz laut Definitionsgraph
(Lodge 12, Weg 0), vier Holzfäller im angegebenen Bezirk automatisch gewählt.
Letzte Vorbeobachtung 23:27:14.579 UTC, Startbeleg 23:27:14.703 UTC.
Beide Aufträge anschließend bestätigt; Weg fertig und zugänglich, Lodge mit
vollständigem Materialbestand. Nach sechs Stunden/17,73 Echtzeitsekunden Pause,
Projekt game_time_limit, Lodge weiterhin awaiting_builders_or_work.
Keine beobachteten Bestandszugangsverluste. Kein aktiver Lauf/Tempoauftrag.

Abgenommen: automatische Auswahl, gemeinsame Diagnose, Kostenübernahme und
Vorprüfung vor Simulations-/Strukturstart. Materialunterdeckung live am
Simulationsziel belegt; der Baupilot hatte ausreichendes Holz. Kostenpflichtige
Zugangshilfen sind synthetisch geprüft, dieser Liveweg war kostenlos.
Keine Fertigstellungs-, Produktions- oder Liefergarantie aus Bereitschaft.
Die Lodge bleibt als Baustelle im pausierten Testspiel, das Projekt unreleased;
vor weiteren Baupiloten frisch diagnostizieren. Kein automatisches Zeitbudgetplus.
README, Einstieg, Backlog, Werkzeugvertrag und Agentenanleitung aktualisiert.
Keine Änderungen an globalen Skills oder Wiederaufnahme des Hermes-Experiments.

[Vertrag und Grenzen](docs/automatic-operating-readiness.md).

Aktueller fachlicher Stand: **0.74.0 plus Cancel-/Transport-Nacharbeit**,
Gate und begrenzter Livepilot bestanden. Grenzen siehe Abschlussbeleg unten.
Ältere Versionsabschnitte sind historische Nachweise.

## Cancel-Transport — Gate und begrenzter Livepilot bestanden (2026-10-18)

28 Supervisor-, 1936 Unit- und 13 Integrationstests bestanden; drei optionale
Integration-Livetests übersprungen. Codex vollständig neu gestartet, neuer
Supervisor-Werkzeugtext und Bridge 0.74.0 bestätigt. Kein Agent-Build/Testlauf.

Anforderung waitSeconds=30: tatsächlich 1,002 Sekunden internes Warten,
1,175 Sekunden bis Antwort. Cancel 200 ms nach Beginn bei noch offenem
Status-Promise angefordert: Antwort nach 2,986 Sekunden (zuvor 31,13 Sekunden).
Zunächst pausing/cancel_requested, anschließend frisch cancelled, terminal=true,
pauseConfirmed=true und observedSpeed=0. Kein aktiver Lauf/ausstehender Tempoauftrag.

Zeitmarken (Europe/Berlin, 18. Oktober): Status empfangen 00:22:19.364,
weitergeleitet 19.394, Backendantwort 20.484, Clientantwort 20.499.
Cancel erst empfangen 00:22:22.393, weitergeleitet 22.409, Backendantwort 22.493,
Clientantwort 22.509. Damit liegt die beobachtete Serialisierung vor dem
Supervisor: rund 1,894 Sekunden zwischen Statusantwort und Cancel-Eingang;
im Supervisor/Backend nur 116 ms vom Cancel-Eingang bis zur Antwort.
Die genaue interne Client-/Tooltransport-Komponente ist nicht weiter aufgelöst.

Abgenommen ist die Begrenzung des eigenen Statuswartens und der anschließende
bestätigte Stopp im bestehenden Codex-Pfad, keine echte parallele Zustellung
oder feste End-to-End-Latenzgarantie. Der parallele Backend-Cancel-Pfad ist
synthetisch geprüft; er wurde durch diesen seriellen Livepfad nicht ausgeübt.
Die Projektstatusvariante der Begrenzung ist Gate-, kein separater Live-Nachweis.
Andere lokale Dokumentationsänderungen, portable Teststarter und historischer
Entwicklungsbericht auf Nutzerauftrag einbezogen; keine Rohlogs oder privaten
Konfigurationen. Anleitung/Toolvertrag aktualisiert, Hermes-Skill bleibt entfernt.
Die folgenden Pilot-/Vorbereitungsabschnitte sind historische Nachweise.

## Transport-Nacharbeit 2026-10-18 — neues Gate und Frontend-Neustart erforderlich

Lokales MCP-Log des isolierten Piloten: Statushandler begann 22:01:49.387 UTC
und endete 22:02:19.434 UTC; Cancelhandler begann erst 22:02:22.152 UTC und
endete 22:02:22.227 UTC. Die zusätzliche Verzögerung liegt vor InvokeLogged,
nicht an der korrigierten NativeTools-Sperre. Der bisherige Supervisor-Quellcode
verarbeitet Requests bereits in getrennten Threads. Ohne Ankunftszeitmarken ist
die genaue Grenze Client/Tooltransport → Supervisor → SDK noch nicht bewiesen.

Vorbereitet: Der Codex-Supervisor begrenzt gültige Statuswarteparameter auf eine
Sekunde. Der Backendvertrag für direkte Clients bleibt unverändert; keine
allgemeine Parallelfreigabe oder Garantie externer Zustelllatenz. Der Tooltext
erklärt die Begrenzung. Ungültige Werte bleiben unverändert zur Validierung;
keine automatischen Folgeabfragen, kein Retry. Zeitmarken für client_received,
backend_forward, backend_response und client_response: maximal 128 Einträge
pro Supervisorinstanz, nur Zeit/Ereignis/Methoden-/Werkzeugname. Keine Argumente,
Antwortinhalte, Sitzungen oder Tokens. Lokal ignoriert, Diagnosefehler blockieren
keine Aktionen. Neue Supervisor-Regressionen vorbereitet, noch nicht ausgeführt.

Auf Nutzerauftrag übrige lokale Dokuänderungen und historische Entwicklungsnotiz
einbezogen. Neun Startervarianten auf gemeinsame portable Startlogik umgestellt;
keine fest eingebauten Benutzerpfade. start-native aktiviert Testaktionen,
-ReadOnly deaktiviert sie, -DisableValidation nur die separate Validierung.
Native Auftragsprüfungen bleiben aktiv. Bestehende private Konfiguration bleibt.

Nächste Abnahme: menschliches Gate, Codex vollständig beenden/neu starten (neuer
Supervisorcode!), Spiel laden, live bereit. Ein 30-Sekunden-Statuswunsch muss nach
höchstens einer Sekunde internen Wartens zurückkehren; danach expliziter Cancel
mit frischer Pausebestätigung. Vor-/Nachlauf-Transportlatenz separat messen.
Zeitmarken zeigen, ob Cancel erst nach client_response ankommt (vor Supervisor)
oder früher empfangen/weitergeleitet, aber verspätet verarbeitet wird.
Kein Commit/Push vor bestandenem Gate und End-to-End-Livepilot.

## Cancel-Livepilot 2026-10-18 — Gate bestanden, End-to-End-Abnahme fehlgeschlagen

Menschliches Gate bestätigt; Gate-Log: 1936 Unit-Tests und 13 Integrationstests
bestanden, drei optionale Integration-Livetests übersprungen. Backend über den
verbundenen Supervisor gestartet; Bridge 0.74.0 und frische Session bestätigt.

Erster Kontrollfall: 30-Sekunden-Statuswarten kehrt nach 30,35 Sekunden zurück;
der währenddessen angeforderte gewöhnliche Schreibaufruf wird erst danach
ausgeführt statt wegen Konkurrenz abgewiesen. Dieser Tempoaufruf unterbricht
den Testlauf, daher kein gültiger paralleler Cancel-Beleg. Explizit pausiert.

Isolierter Folgefall ohne Kontrollmutation: Status mit waitSeconds=30 gestartet,
Cancel nach zwei Sekunden bei noch offenem Status-Promise angefordert. Status
antwortet nach 30,32 Sekunden mit wait_limit; Cancel erst nach 31,13 Sekunden
ab seiner Anforderung, zunächst pausing/cancel_requested. Anschließend frisch
cancelled, terminal=true, pauseConfirmed=true, observedSpeed=0 bestätigt.
Endzustand: Tempo 0, kein aktiver Lauf, kein ausstehender Tempoauftrag.

Damit keine End-to-End-Freigabe: die Aufrufe erreichen den wirksamen Dispatch
im verbundenen Aufrufweg offenbar weiterhin nacheinander. Welche Schicht
(Client/Tooltransport, Supervisor-Zustellung oder MCP-SDK) serialisiert, ist
noch nicht belegt. Die Backend-Unit-Regressionen ersetzen diesen Nachweis nicht.
Keine weiteren Livefälle, kein Commit/Push. Nächster Schritt: Ankunftszeiten
an Supervisor und Backend vergleichen; danach gezielte Korrektur oder eine
ehrliche Begrenzung langer Statuswartefenster. Keine zusätzlichen stdio-Prozesse
als vermeintlicher Nachweis der bestehenden Codex-Verbindung starten.

## Journal-Korrekturen — Gate und gezielter Livetest bestanden

Menschliches Gate: 23 Supervisor-, 1931 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Bridge weiterhin 0.74.0.
Einzelkiefer markiert und geschützt, jeweils completed und genau ein bestätigtes
Ziel. Alte Markierungs-ID nach Schutz erneut gelesen: Kiefer blieb unmarkiert.
Mit neuer Action wieder markiert; alte Schutz-ID nach Backend-Neustart und
vollständiger Client-Neuverbindung erneut gelesen: Markierung blieb bestehen.
Damit keine erneute Spielmutation in beiden Gegenrichtungen beobachtet.
Abschluss durch neue Schutz-Action, readbackConfirmed=true. Endtempo 0, kein
aktiver Lauf oder Tempoauftrag; Spielzeit unverändert.
Supervisor zunächst noch alter persistenter Prozess. Nach menschlicher
Neuverbindung lagen die Elternprozess-Startzeiten nach der Skriptänderung.
Stop/Status bestätigten running=false/gate=true; Start running=true/gate=false,
Spielsession erhalten. Windows-Austauschfehler synthetisch im Gate geprüft,
nicht künstlich live erzeugt. Keine Live-Unterbrechung eines laufenden
Schreibauftrags; deren Nichtwiederholung bleibt Gate-Nachweis.
Learnings und Grenzen in docs/releases/0.74.0.md; Anleitung, Tool-Doku und
zurückgestellter Skillentwurf aktualisiert. Korrekturen fachlich abgeschlossen; Git-Sicherung: Journal-Korrekturen `c09750f`, ursprünglicher 0.74-Abschluss `680fd00`.

## 0.74.0 — Gate und gezielter Livetest bestanden

Menschliches Gate: 21 Supervisor-, 1929 Unit- und 13 Integrationstests bestanden, drei optionale
Integration-Livetests übersprungen. Bridge 0.74.0 in neuer Session bestätigt.
Materialwarten trotz repairEligible=true empfiehlt check_material_supply.
Gezielt entfernter Bestandsweg erzeugt zwei belegte Anschlussverluste und
review_access_repair_plan; Einzelbefund Materialwarten bleibt erhalten.
Weg regulär wiederhergestellt, Plattform 1/1 fertig und zugänglich; Empfehlung
review_close_finished_project trotz historischem game_time_limit. Compact/full
und explizite Watch-Diagnose bestätigt. Beide Journale freigegeben, null
Bestandsverluste, Tempo 0, kein aktiver Lauf oder ausstehender Tempoauftrag.
[Livebelege und Grenzen](docs/releases/0.74.0.md). Anleitung, Tool-Doku und
zurückgestellter Hermes-Skillentwurf mit Livebelegen aktualisiert; kein Skill
installiert. Keine Agent-Builds/Tests. Version 0.74.0 fachlich abgeschlossen; Git-Sicherung durch den Abschlusscommit.

Ergänzender Livetest: isolierte Plattformbaustelle nativ buildersReachable=false,
construction_unreachable, keine unfertigen Stützen und lostConnections=0.
Compact/full und Watch: observed_access_problem, accessProblemPartIndices=[0],
repairEligible=true, review_access_repair_plan. Testbau regulär entfernt und
Journal freigegeben; keine Baustellen, Endtempo 0. Unbekannte Identität bleibt
live offen: reguläre Werkzeuge ändern keine Objektidentität; automatische
Bestätigung verhindert einen deterministischen unbestätigten Abrissfall.
Bekannter Abriss wurde korrekt als previously_observed_object_removed erkannt.

## 0.73.0 — Gate und gezielter Livetest bestanden

Menschliches Gate: 21 Supervisor-, 1908 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Bridge 0.73.0 live bestätigt.
limit=100 und depth=5 melden den konkreten Bereich mit not_applicable;
korrigierte Abfragen funktionieren. Einzelplattform nach einer Spielstunde
blocked/game_time_limit und frisch 0/1 fertig; nach separaten zwölf Stunden
frisch 1/1 fertig und zugänglich bei unveränderter Historie. Compact/full,
explizite watch-Diagnose und rein historische Journalansicht bestätigt.
Keine beobachteten Bestandsverluste; Journal regulär freigegeben, Tempo 0,
kein aktiver Lauf. [Livebelege und Grenzen](docs/releases/0.73.0.md).
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert;
Workflow unverändert. Keine Agent-Builds/Tests. Version 0.73.0 fachlich abgeschlossen; Git-Sicherung durch den Abschlusscommit.

## 0.72.0 — Gate und vollständiger Plattform-Umwegpilot bestanden

Abschluss der seit 0.67 entwickelten Zugangsverbesserungen: automatische
Umwegsuche im sicheren Stand mit 191 Zuständen; 15/15 Teile (zehn Plattformen,
drei Wege, zwei Lager) fertig und zugänglich. Beide Lagereingänge und native
Verbindung zum Distriktzentrum bestätigt; untere Bestandswege trotz Überbau
weiter verbunden. Bestandsdiagnosen durchgehend lostConnections=0.
Journal regulär freigegeben, keine Baustellen, Endtempo 0, kein aktiver Lauf.
Zweites vorhandenes Sägewerk für die Brettlieferung aktiviert und besetzt.
Baupause/Stützwarten, beobachtete Mindestbesetzung und Menü-Sperre/Entsperre
bereits live bestanden. Menschliches Gate bestanden, keine Agent-Builds/Tests.
[Umfang, historische Vorläufe und Abschlussbelege](docs/releases/0.72.0.md).
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert;
Workflow geprüft. Skill bleibt zurückgestellt. Abnahme für den dokumentierten
Umfang abgeschlossen; Code und Nachweise zur Git-Sicherung freigegeben.

Grenzen: neuer unterer Zielweg einer gedrehten Bestandstreppe noch kein eigener
Livefall; keine allgemeine Such-/Bauphasengarantie. Zielgebäude kann vor seinem
Zugang fertig sein. Weganschluss ist kein Nachweis trockenen Betriebs.

## In 0.74 gezielt live bestanden — repairEligible und Reparaturempfehlung trennen

Ausgangspunkt 0.73: Im Livepilot war bei
materials_not_ready und lostConnections=0 bereits repairEligible=true samt
review_access_repair_plan sichtbar. Das beschreibt eine mögliche Operation,
nicht einen nachgewiesenen Reparaturbedarf.

- Technische Reparaturmöglichkeit beibehalten, tatsächlichen Reparaturbedarf
  anhand frischer Zugangsbelege separat ausweisen; unknown nicht als fehlenden
  Zugang behandeln.
- Bei reinem Material-/Stützwarten oder Personalproblem den passenden nächsten
  Schritt nennen. Kein automatischer Zugangsbau allein wegen repairEligible.
- Live bestanden: Materialwarten, belegter Bestandsverlust, Wiederherstellung
  und vollständiger Fertigbau sowie isolierte unerreichbare Baustelle. Unbekannter
  Zugang/Identität bleiben Live-Nachweisgrenzen; Gate-Regression vorhanden. Historischen
  Laufabbruch, aktuellen Bauzustand und Journalfreigabe weiter getrennt halten.
- Learning aus 0.73: Fähigkeitsflag ist keine Handlungsempfehlung. Belege und
  konkrete Gründe müssen den nächsten Schritt bestimmen.

## Offener Recovery-Nachweis nach 0.74

Unbekannte Identität benötigt einen deterministischen, ausdrücklich als injiziert
gekennzeichneten Entwicklungsfall oder einen echten passenden Laufzeitfehler.
Keine wiederholten Zeitrennen zwischen Platzierung und Abriss: Legacy-Projekte
beobachten automatisch auch bei Pause. Kein Save-/Ledger-Patching als Livebeleg.
Bei möglichem Testmechanismus native Sperren sowie repairEligible=false und
closeEligible=false mitprüfen; Präsentationsfixtures allein belegen diese nicht.
Begrenzte Fehler-Injektion ist ein Vorschlag, noch nicht implementiert/freigegeben.

Zusatzbefund: inspect_map_region außerhalb der Karte lieferte nur generisches
invalid_argument. Konkrete Kartengrenzen fehlen in dieser Fehlerausgabe; separat
präzisieren, nicht mit unbekannter Objektidentität verwechseln.

## Weitere Verbesserungen

- Betriebs-/Überflutungszustand des Bauzentrums und der Lieferkette vor längeren
  Bauzeitfenstern bündeln; connected allein reicht nicht.
  Erledigt im begrenzten Umfang: `inspect_operating_readiness` für ausgewählte
  Gebäude; Gate und Einzelabgleich live bestanden. Keine Kolonievollabdeckung. [Vertrag](docs/operating-readiness.md).
- Unteren neuen Zielweg an gedrehter Bestandstreppe separat live prüfen.
- Parameterdiagnose und Trennung Zeitlimit/Fertigbau sind mit 0.73 erledigt;
  keine erneute Umsetzung dieser Punkte.

## 0.71.0 — Gate, Anschlussprüfung und Umwegplatzierung bestanden; Fertigbau offen

Bridge 0.71 und neue Session bestätigt. Oberer Treppenanschluss sowie seitliche
Negativkontrolle bestanden. Zwei Anschlusswege fertig, realer Bauzugang belegt.
Automatische Umwegvorschau um ein mitgeplantes Lager: 819 Suchzustände,
17 Teile, native Eingänge und Arbeitszugänge bestätigt; 3477 Bestandsprüfungen
ohne Verlust. Alle 17 Objekte einmal platziert und bestätigt. Geprüfte reale
Baustellenzugänge erreichbar, Bestandsdiagnose weiter ohne Verlust.
17 Baustellen bleiben, Endpause bestätigt; Fertigbau und unterer neuer
Treppenzielweg noch nicht abgenommen. Kein Commit/Push.
[Belege, Grenzen und Learnings](docs/releases/0.71.0.md).
Nächste Diagnosekorrektur: construction_paused trotz paused=false bei unfertigen
Trägern; Inaktivität, manuelle Pause und Stützabhängigkeit trennen. Nullbesetzung
am Holzfäller unbestätigt; minimale Besetzung und Zeitsperrstatus sichtbar machen.
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert.
Cockpit-Gate erfolgreich; Darstellung nicht gesondert visuell abgenommen.
Backend verbunden, Startsperre frei. Baustellen nicht erneut beauftragen.

## 0.70.0 — Bestandstreppen-Höhenfall bestanden, erweiterter Umweg offen

Instant-Netz und Vorschau mit Zielweg getrennt: gedrehter oberer Austritt
von Folktails-Bestandstreppen nur als Kandidat; native Zielweg-/Stützenvorschau
bestätigt Distriktanschluss und Wiederherstellung. Keine ausgelesene direkte
Treppenkante behaupten. Maximal acht Proben, bestehende Suchlimits unverändert.
Diagnose Version 3 zeigt Instant-Kante und Vorschauergebnis separat.
[API-Befund, Learning, Grenzen und Pilot](docs/releases/0.70.0.md).
Regressionen und Versionsprofile ergänzt; Quell-/Diffprüfung, keine Agent-
Builds/Tests/Installation. Gate und bekannter Höhenfertigbau bestanden; erweiterter Umweg nicht abgenommen, kein vollständiger Abschluss/Commit/Push.
Anleitung, Toolbeschreibung und lokaler Hermes-Skillentwurf aktualisiert;
Entwurf bleibt zurückgestellt. Workflow geprüft, unverändert.

## 0.69.0 — Gate bestanden, Höhenpilot erneut fehlgeschlagen

Bestandstreppen werden mit AreConnectedRoadInstant statt AreConnectedInstant
geprüft. Diagnose Version 2 kennzeichnet den Weggraph und zeigt die allgemeine
Abfrage getrennt; kein Ersatz nativer Freigabe durch Geometrie. Regressionen
und Versionsprofile ergänzt. Höhenpilot: beide Graphabfragen am Treppenaustritt false; 2048 Zustände ausgeschöpft. Manuelle Zwei-Wege-Gegenprobe gültig, Bestandsverbindungen erhalten.
MCP-Log öffnet sich nach dem Laden automatisch; bleibt manuell schließbar.
[Änderungen, Learning und Abnahmeplan](docs/releases/0.69.0.md).
Quell-/Diffprüfung; keine Agent-Builds, Tests oder Installation. Kein Commit/Push.
Spielanleitung, Toolbeschreibung und lokaler Hermes-Skillentwurf gepflegt;
Skill bleibt zurückgestellt. Entwicklungsworkflow geprüft, unverändert.

## 0.68.0 — Gate bestanden, Höhenanschluss weiterhin fehlerhaft

Zielpriorisierung und begrenzte Diagnose der Bestands-/Treppenanschlüsse aus
0.67-Learning umgesetzt. Budgets unverändert. Zuerst bekannter Höhenfall,
bei Erfolg Umweg und tatsächlicher Bau mit Bestandskontrolle. Bei erneutem
Fehlschlag Diagnose auswerten und stoppen. [Abnahmeplan](docs/releases/0.68.0.md).

## 0.67.0 — Gate bestanden, Suchkorrektur noch nicht live abgenommen

P1-Eigenkollisionen und Nutzung vorhandener Treppen aus dem 0.66-Folgepilot
implementiert: Geometrie je Suchpfad, alternative Belegungen je Knoten und
nativ bestätigte Höhenanschlüsse mit regulärem Zielweg. Regressionen ergänzt.
Gate bestanden. Gleicher Höhenfall scheitert bei 2048 Suchzuständen, obwohl
manuelle Zwei-Wege-Gegenprobe gültig bleibt. Zielpriorisierung der geometrischen
Suchzustände und Diagnose bestätigter/abgelehnter Treppenaustritte fehlen.
Kein Bauauftrag, Bestandszugänge erhalten. Erst diesen Kontrollfall lösen,
dann Bauphasen-/Fertigzugangspilot; Limits nicht pauschal erhöhen.
Die folgenden 0.66-Befunde bleiben Herkunft der Änderung.
[Gateplan](docs/releases/0.67.0.md).

## 0.66.0 — Kernpilot bestanden; verbleibende Nachweise

Uferpumpen-Terraincheck berücksichtigt `Block.IsOccupied`; Laufstart-Ablehnungen
haben konkrete Codes und gezielt `not_executed`. Uferpumpe bis Fertigzugang,
Tempoabweichung, konkurrierender Start und gültiger Folgelauf live bestanden.
**P1 Höhen-/Umwegsuche:** Folgepilot findet einen selbstkollidierenden Plan
(Treppe und spätere Plattform auf derselben Zelle), obwohl eine native gültige
Gegenprobe mit zwei Wegen über die bestehende Treppe vorliegt. Suchzustand
berücksichtigt bisher nur Zielzelle/Kosten, nicht Geometrie des bisherigen
Routenpfads. Kantenpakete gegen Vorgängerkanten prüfen und zulässige Alternativen
bei unterschiedlicher Belegung erhalten. Vorhandene Treppenanschlüsse separat
prüfen: Quellen stammen bisher ausschließlich aus `MainPathCoordinates`.
Keine erfundene Verbindung; gültige Anschlüsse nativ bestätigen.
Regression: erhöhtes Ziel hinter Hindernis, Eigenkollision verworfen,
gültiger Umweg gefunden, Bestandsanschlüsse vor/während/nach Bau erhalten.
Native Schlussvorschau hat den Fehler korrekt abgefangen; nicht lockern.
Details und Nachweisgrenzen im Releasebericht.

Navigations-Baselinekonflikt bei
Wiederauftreten isoliert diagnostizieren. [Belege](docs/releases/0.66.0.md).
Die nachfolgenden 0.65-Befunde bleiben Herkunft und Live-Abnahmekriterien;
Cancel-Priorisierung ist nicht Teil dieser Version.

## 0.65.0 — begrenzt live abgenommen; Folgearbeiten

Rückstauszenario unter Sperrkrone, Abflussunsicherheit und kontaminiertes Zielwasser
sowie native Flooded-/Ansaugdiagnose implementiert; Bridge nach `live bereit`
verbunden. Rückstaupflicht und trockene Ansaugung live bestätigt; selbst gebaute
Kontrollpumpe hat einen nativen Produktionszyklus geliefert. Überflutungsfall
auch bei Gebäudepause positiv bestätigt; höhere Ersatzpumpe produziert.
[Nachweisplan und Grenzen](docs/releases/0.65.0.md). Kein vollständiges Hydraulikmodell.
Reserve-/Gefahrenstopps, Abflusskapazität und automatische sichere Umleitung bleiben
offen. Learnings: Kartenrandkontakt ist kein Auslassbeleg; Pause kann UI-Flutstatus
verdecken; natives Flooded vor Umverteilung von Versorgungsarbeitern prüfen.

Neue konkrete Befunde aus dem 0.65-Pilot:
- **P1 Notstopp während Statuswarten — Korrektur vorbereitet, Gate/Live offen:** Die Sperre liegt in NativeTools, nicht im parallel weiterleitenden Vorschaltprozess. Der bisherige Backendpfad serialisiert
  MCP-Aufrufe. Ein laufendes `inspect_simulation_run(waitSeconds=30)` blockiert
  auch Cancel mit `not_executed`. Nach Ende des Leseaufrufs funktionierte Cancel
  samt Pause. Eng begrenzter Cancel-Pfad während reinem Statuswarten vorbereitet;
  bis dahin Spiel-MCP seriell und bei Nutzersteuerung kurze Wartefenster nutzen.
- **P1 Zugangssuche/Geometrie:** `plan_large_project_access` verwirft eine
  Uferpumpe wegen Terrainüberschneidung. Dieselbe Position besteht native
  Einzel- und Gesamtvorschau und ist anschließend regulär fertiggebaut und
  produktiv. `SpatialObservations` hat denselben pauschalen Terraincheck.
  Native Belegungsregeln leerer/optionaler Pumpenzellen prüfen; keine pauschale
  Lockerung für echte Hindernisse. Eine Ebene höher erzeugt die Suche unnötige
  Plattformen. Kontrollfall für nächsten Fix: gültige Uferpumpe plus echte
  Terrainkollision, Objektkollision und nachgewiesener Bau-/Betriebszugang.
- **P1 Lauf-Fehlerweitergabe:** Starts meldeten generisch
  `state_conflict`/`unconfirmed`, danach `run_not_found`. Ein späterer Fall zeigte
  frisch Tempo 7 statt erwartetem Tempo 0. Batchbesitz ist als Ursache des
  früheren Falls nicht bewiesen; terminale Chargen geben ihren Besitzer frei.
  Frühe Ablehnung von unbestätigtem Transport trennen und konkreten Tempo-,
  Sperr- oder Besitzerkonflikt nennen. Nach frischer Diagnose eine neue Aktion
  planen, unbestätigte Starts nicht blind wiederholen.
- **Baukosten und Abschluss:** Höhenzugang benötigte zwölf Plattformen und
  drei Treppen. Materialbedarf samt Produktionszeit vor dem Auftrag anzeigen.
  Nach abgelaufenem Bauzeitfenster frische `diagnose_large_project` verwenden;
  eingefrorene Chargenzähler zeigen spätere Fertigstellung nicht.

## Fehler aus dem Überlebenslauf — Stand nach 0.64.0

Die drei P0-Korrekturen sind in [0.64.0](docs/releases/0.64.0.md) begrenzt live
abgenommen: Höhenbau bis Fertigzugang, stabile Pause und Journalfreigabe nach
Rückbau mit Folgeauftrag ohne Reload. Menschliches Gate laut Nutzer bestanden.
Der konkrete Ursprung des damaligen run_not_found bleibt offen; Antwortverlust
und Rennen innerhalb desselben Updates sind synthetische Gatefälle.

Kolonieverlust vom Nutzer gemeldet; Schutzmauer nicht fertiggestellt.
Keine hydraulische Schutz- oder autonome Überlebensabnahme. Priorisierte Folgearbeit:

- **Offener Diagnoseumfang:** bei erneutem unconfirmed/run_not_found Transport,
  Session und Laufregistrierung getrennt belegen. Kein Wiederholen aus bloßer
  Abwesenheit einer Kennung. Neue Felder activeRunId/pendingSpeed dafür nutzen.
- **Weitere Livegrenzen:** Game-Lock-Abbruch, Antwortverlust, erst nach terminalem
  Stop bestätigte Identität und separat ausgelöster lostConnections-Guard in
  dieser Version nicht live provoziert; vorhandene synthetische Regressionen.
- **P1 Wasserbau und Versorgung:** sichere Abflussroute, Rückstau und Umgehung
  getrennt von gültiger Geometrie bewerten. Pumpendiagnose um native Flooded-
  und Ansaugblockaden ergänzen; Reserve-/Gefahrenstopps für Zeitläufe. Kein
  Schutzversprechen aus Geländescan oder fertiggestellter Pumpe ableiten.
- **P1 vollständiger Zugang:** Räumung, offener Wasserübergang, Treppen, Kosten
  und Bauzeit gemeinsam planen. Plattformen kosteten hier sechs Bretter je Teil;
  Material aus aktuellem Katalog statt Erinnerungswerten. Markierte Bäume ohne
  erreichbaren Bauweg räumen sich nicht durch zusätzliche Simulationszeit.

Details und weitere Abnahmekriterien: [Pilotjournal](docs/survival-run-2026-10-17.txt).
Die verbleibenden Wasserbau-/Zugangsplanerarbeiten sind nicht implementiert oder
abgenommen. Learnings aus 0.64: gemeinsames Testfenster ohne parallele Nutzerbauten,
Materialbedarf aus Katalog, Quittung und frische Diagnose getrennt lesen.

## Bauplaner und Fehlerweitergabe — begrenzt live abgeschlossen

Priorität P0 aus dem Überlebenspilot vom 17. Oktober. Menschliches Gate und
15-Teile-Baupilot bestanden; [Nachweise und Grenzen](docs/planner-repair-2026-10-17.md).
Weitere Überlebens-Learnings stehen in `docs/survival-run-2026-10-17.txt`;
Wasserdiagnose, Ereignisstopps und Transportqueue sind nicht Teil dieses Fixes.

### Neue Learnings aus dem Baupilot

- Ingame-Fortschrittstext korrigieren: `inspect_large_project` meldete 0/15,
  strukturierter Bericht dagegen 14/15 fertig. Abnahme: laufende/terminale
  Chargenberichte korrekt zählen und Ledgerbeobachtung von frischer Diagnose trennen.
- Eingabeschema und Fehlerdetails präzisieren: unbekannte Vorlage bzw. nicht-UUID
  als actionId liefern nur `invalid_argument`. Feld und zulässiges Format nennen;
  UUID-/Enum-/Zahlengrenzen müssen auch in der Client-Werkzeugdarstellung ankommen.
- Suchbudget bleibt Qualitätsgrenze: acht Vorschauen fanden zwei Gebäude,
  16 fanden alle drei mit zwölf Umweg-Wegen. Vollständigkeit und Wegkosten
  getrennt berichten; kein Nachweis einer optimalen Route.

## B15: Gesamtstruktur — 0.63 mit dokumentierter Grenze abgeschlossen

**Ursprünglich P0, Nutzerauftrag vom 16. Oktober 2026.** Abschluss am 17. Oktober
mit ausdrücklicher Nutzerfreigabe der unten dokumentierten Nachweisgrenze.
Menschliches Gate bestanden; kein offener Implementierungsblocker dieses Umfangs.
Ebener Gesamtauftrag, Zeitlimit, Reparaturabschluss und Schema-2-Gebäudecharge live
belegt; manueller Höhenstart setzt auch oberen Weg auf unfertiger Plattform sofort.
Separater Vertikalpilot nach erneutem Gate mit fünf gemeinsamen Aufträgen bestanden;
ebener Dreiteilepilot fertig, konkrete Identitätsablehnung und Objektverlust ohne
Replay geprüft. Teilablehnung mitten im Durchgang nur synthetisch im Gate belegt;
begrenzter Versionsabschluss ausdrücklich vom Nutzer freigegeben. Zusätzlicher
nativer Livefehler und vollständiger Höhenfertigbau bleiben weitere Nachweise.

Gewünschter Ablauf: **Plan machen → einmal Go → komplette Struktur als
Bauaufträge platzieren → Baufortschritt verfolgen → erfolgreich beenden, wenn
alles gebaut ist.** Der Agent beschreibt das gesamte Bauvorhaben; MCP übernimmt
die gemeinsame Platzierung und meldet konkrete Probleme zurück. Die Biber bauen
die platzierten Baustellen anschließend nach und nach. MCP wartet nicht nach
jedem einzelnen Element auf dessen Fertigstellung, bevor es das nächste setzt.

- **Ein Gesamtplan:** Gebäude, Wege, Treppen, Stützen, Kraftverbindungen und
  Wasserbau einer Struktur gemeinsam beschreiben und prüfen. Geometrie,
  Distriktanschluss, Bauarbeiterzugang, fertiger Gebäudezugang und Bauphasen
  getrennt bewerten, einschließlich der Abhängigkeiten innerhalb des Plans.
- **Ein Go für die ganze Platzierungsphase:** Alle vom Spiel zulässigen Teile
  unmittelbar als Bauaufträge einreichen. Interne API-Aufrufe dürfen technisch
  sequenziell sein; zwischen ihnen keine Baufortschrittswartezeiten, keine
  Agentenrunde und keine erneute Einzelbestätigung. Klarstellung des Nutzers:
  erst sämtliche Aufträge einreichen, DANACH prüfen; auch keine Material-,
  Zugangs- oder Initialisierungsprüfungen zwischen den Platzierungen. Noch fehlendes Baumaterial
  ist kein Grund, weitere vom Spiel erlaubte Baustellen zurückzuhalten.
- **Echte Spielgrenzen konkret zurückmelden:** Verlangt das Spiel für ein Teil
  tatsächlich eine fertiggestellte Stütze, Räumung oder andere Vorleistung,
  dies als belegte Ausnahme mit betroffenem Teil und Abhängigkeit ausweisen.
  Keine pauschale Serialisierung des gesamten Vorhabens daraus ableiten.
  Keine vorsorglichen Fertigstellungsbarrieren aus dependsOn ableiten. Erst alle
  Teile versuchen; tatsächliche native Ablehnungen danach konkret ausweisen.
  Unbestätigte Aufträge niemals automatisch wiederholen.
- **Ein Platzierungsbericht:** Pro Teil geplant / platziert / abgewiesen /
  zurückgestellt / unbestätigt mit Objektzuordnung, Koordinaten, konkretem
  nativen Fehler und erforderlicher Korrektur. Teilerfolg offen ausweisen;
  keine vorgetäuschte atomare Alles-oder-nichts-Transaktion und kein blindes
  Wiederholen unbestätigter Aufträge.
- **Ein Lauf bis zum tatsächlichen Bauabschluss:** Gesamte Charge unter einer
  Auftrags-ID beobachten. Fortschritt und Blockaden wie Materialmangel,
  fehlende Bauarbeiter oder verlorener Zugang gesammelt melden. Erfolgsende
  erst, wenn alle beauftragten Teile nachweislich fertig sind. Platzierung
  allein ist kein Abschluss. Abbruch, Zeitlimit und dauerhaft blockierte Teile
  sind gesonderte End-/Fehlerzustände mit offenem Rest, niemals Erfolg.
  Zugang, Kraftanschluss und Betrieb zusätzlich getrennt ausweisen.

**Abnahme:** Eine mehrteilige Struktur wird nach einem Go vollständig als
Baustellen sichtbar, bevor das erste Teil fertig ist; der Agent muss keine
Elemente einzeln weiterreichen. Ein Kontrollfall mit ungültigem Teil liefert
einen präzisen Teilfehler und den tatsächlich platzierten Rest. Ein Fall mit
Materialmangel bleibt nach kompletter Platzierung in Bau und endet erst nach
Nachschub/Fertigstellung erfolgreich. Native Vorleistungsabhängigkeit und
unbestätigte Platzierung jeweils separat prüfen.

Bestehende Gruppen-, Großprojekt- und Bauchargenwege auf diesen Ablauf umbauen,
statt lediglich einen weiteren Wrapper um die bisherige Fertigstellungssequenz
zu setzen. Verknüpft B02 (Gesamtplanung), B03 (Kraft/Höhen), B07 (Versorgung)
und B14 (zusammenhängende Schutzstrukturen).

## 0.54.0 - Pflanzen- und Flaechenjournal begrenzt live abgenommen

Menschliches Gate und Abschluss-Livetest am 12. Oktober 2026 bestanden.
Pflanz-/Faellmarkierungen, reale Pflanzenwechsel und Baum-/Stumpfunterscheidung
mit ehrlicher Herkunft; Compact/Full, Filter und Cursor live bestaetigt.
Menschliche Eingriffe bleiben unknown. Neue Sitzung startet ohne Altbestands-
ereignisse; alte Session wird abgewiesen. Produktionsregression bestanden.
Grenzen: abgetastete Zustandswechsel, keine lueckenlose Ereignis-/Erntehistorie.
[Vertrag und Livebelege](docs/spatial-journal-0.54.md).


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
[Vertrag und Livepilot](docs/planting-batch-0.53.md).

## 0.52.0 begrenzt live abgenommen — Flächenräumung und Gruppenbau

Zusammenhängender Hintergrundablauf: Pflanzmarkierungen entfernen, bis 64
Vegetationsziele räumen, auf echte Fertigstellung warten, frisch gemeinsam planen
und bis acht Gebäude samt Wegen autonom bauen. Eine Fläche bis 8×8 auf einer Höhe;
Rohstoffstapel ignorieren und niemals löschen. Zeitlimits, keine automatische
Wiederholung nach Unsicherheit/Neustart. Compact/Full und lokale Statusabfrage.
Menschliches Gate bestanden; 0.52.0 am 12. Oktober 2026 begrenzt live abgenommen.
Räumbudget-Stopp, autonome Zweiergruppe, Pflanzmarkierungen und Bau auf
einer Rohstoffstapel-Zelle bestätigt; Testbauten entfernt, Endpause belegt.
[Vertrag und Abnahmepilot](docs/cleared-building-groups-0.52.md).

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
[Vertrag und Abnahmepilot](docs/group-search-0.51.md).

Stand: 2026-10-16, nach begrenzter Liveabnahme von **0.62.0**.
Diese Liste trennt noch offene Fähigkeiten von abgeschlossenen Teilumfängen.
Vorschläge sind keine Implementierungsfreigabe. Test-Gate unverändert:
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
Alle bisherigen Beobachtungen bleiben im [historischen Backlog](BACKLOG_HISTORY.md).

## Offene Entwicklung

| ID | Verbesserung | Vorhandene Grundlage und noch offener Umfang |
| --- | --- | --- |
| B15 — **0.63 abgeschlossen** | **Gesamtstruktur gemeinsam platzieren, dann bis Fertigstellung verfolgen** | Ein Plan, ein Go, gemeinsame Platzierungsphase und Bauabschluss. Menschliches Gate und begrenzte Liveabnahme bestanden; Teilablehnung mitten im Durchgang nur synthetisch geprüft, ausdrücklich akzeptiert. Umfang und Grenzen oben. |
| B01 | Bessere Standortwahl | 0.51: begrenzte Rückwärtssuche, Vergleich ganzer Pläne und Auslassen blockierender Wünsche abgenommen. Offen: alternative Zielreihenfolgen und breitere Suche; keine globale Optimalität. |
| B02 | Reservierungen und Gesamtplanung | 0.51: bis acht Ausschlussvolumen für einen Gruppenplan vorhanden. Offen: dauerhafte Reservierungen, planübergreifende Korridore und Suchabdeckung; keine globale Siedlungsoptimierung. |
| B03 | Kraft- und Höhenrouten planen | Ports, Netze, explizite Höhenpläne und Zugangshilfen existieren. Offen: allgemeine automatische Routenauswahl, Alternativstandorte und kombinierte Versorgungslayouts mit getrenntem Geometrie-/Netz-/Betriebsnachweis. |
| B04 | Flächenumwidmung in Gruppenpläne integrieren | 0.52 begrenzt live abgenommen: eine Fläche bis 8×8 → Räumung → gemeinsame Gruppe. Rohstoffstapel ignoriert, keine Löschung. Größere/mehrere Flächen bleiben offen. |
| B05 | Flächenvorprüfung und Sammelpflanzung | 0.53 begrenzt abgenommen: Zweiflaechenpilot, Ausschluesse und veraltete Vorschau bestanden. Wachstum/Aquakultur nicht nachgewiesen. |
| B06 | Projekte über Sitzungswechsel wiederaufnehmen | Wiederanschluss innerhalb derselben Spielsession funktioniert. Offen: gespeicherte Absichten nach Neuladen mit realen Objekten abgleichen; alte Aktionen nicht blind wiederholen. |
| B07 | Produktionsketten planen | Rezeptwahl, gemeinsame Einrichtung, Engpässe und Produktionsläufe existieren. Offen: Zutatenquellen, Lager/Transport, Personal und Kraft bereits vor Bau als gemeinsamen Ausbauplan berücksichtigen. |
| B08 | Weitere Produktions- und Wirkungsnachweise | Produktionsnachweis bisher für Manufactory. Weitere Produzenten und reale Effektzellen/aktive Ziele etwa des Bienenstocks separat erschließen; Reichweite ist kein Wirkungsnachweis. |
| B09 | Weitere Journaldaten | Pflanzen/Stuempfe und Flaechenmarkierungen mit 0.54 begrenzt live abgenommen. Forschung weiterhin offen; keine automatische Zuschreibung zum Menschen. |
| B10 | Agentenlog verbessern | 0.55: Cursor, Werkzeug-/Statusfilter und Lücken begrenzt live abgenommen. [Vertrag](docs/agent-log-pagination-0.55.md). 0.56: explizite Projekt-/Laufkorrelation mit ID-Filtern begrenzt live abgenommen; Ingame-Anzeige vom Menschen bestätigt. [Umfang](docs/agent-log-correlation-0.56.md). 0.57: Sitzungs-Dateilogs mit Save-Namen, neue Dateien beim Neuladen, vereinfachte Anzeige und automatischer Spielstart begrenzt live abgenommen. [Vertrag](docs/file-diagnostics-0.57.md). 0.58: verständliche Ergebniszeilen für Gebäudeeinstellungen, Simulation und Pflanzchargen begrenzt live abgenommen; Ingame-Lesbarkeit vom Menschen bestätigt. [Umfang](docs/agent-log-outcomes-0.58.md). 0.59: Ergebniszeilen für Bauprojekte und Bauchargen begrenzt live abgenommen (Einzelprojekt, Ein-Gebäude-Charge, Vier-Teile-Großprojekt und gezielter Großprojektstopp am Westufer); Ingame-Lesbarkeit bestätigt. Zusatzproben: Räumgruppe, Spielzeitbudgetende und separater Bauchargenstopp bestanden; Reparaturstart nach gültiger Vorschau mit state_conflict fehlgeschlagen. Neuer 0.59-Kontrollfall erfolgreich, ursprüngliche Ursache weiter offen. 0.60: falscher Planschlüssel korrekt ohne neuen Auftrag abgewiesen, gültiger Reparaturabschluss/Pause/Elternfreigabe live geprüft; zwölf Ergebniszeilen in beiden Dateilogs identisch. Ingame-Lesbarkeit vom Menschen bestätigt; [Befund und Livepilot](docs/repair-start-0.60.md). Zeitlimit-/Stopgrund im Kurztext präzisieren. [Testprotokoll](docs/agent-log-live-tests-0.59.md). [Umfang](docs/agent-log-building-outcomes-0.59.md). Weitere Auftragsfamilien bleiben offen; kein Ersatz für das Spieländerungsjournal. |
| B11 | Agenten-UI im Spiel | Screenshot mit Kamerametadaten und „schau mal“ existieren. Offen: „Zeigen“-Button nimmt die Ansicht beim Klick auf, begrenzte Aufbewahrung und Abhol-/Fehlerstatus. Aktive Benachrichtigung ist ein separat zu definierender Umfang. |
| B12 | Weitere Effizienz und Diagnosequalität | Noch große Werkzeugantworten/-beschreibungen gezielt messen, veralteten Clientkatalog erkennbar machen, unnötige interne Reads reduzieren. 0.54-Pilot: kleine Journalgruppen adaptiv darstellen; sechs Ereignisse mit vier Gruppen sparen nur 5 von 1555 page-Zeichen. Compact/Full und günstiges Warten sind bereits vorhanden. |
| B13 | Optionale Koloniebeobachtung | Geburten/Todesfälle im Intervall, Altersverteilung, bevorstehendes Erwachsenwerden und altersbedingte Personalengpässe, Familienwohnungen, tatsächliche Wohlbefindensursachen und historisches Bauregister. Prognosen mit Zeitraum, Annahmen und Unsicherheit; freie Betten beweisen keinen gesicherten Nachwuchs. Kein fortlaufender Auftrag, alle Gebäude oder eine bestimmte Bevölkerung zu erreichen. |
| B14 | Strategische Kartenübersicht und Spezialobjektsuche | Nutzerauftrag vom 2026-10-16: kartenweite, strukturierte Übersicht über Topologie, Höhenlagen, Höhlen, Wasserreservoirs und Spezialobjekte. 0.62 liefert kartenweiten Terrain-/Quellen-/Flussüberblick, priorisierte Restwassergefahren und bedingte Reservoir-Optionen; Ereignis-/Durchbruchprognosen bleiben offen. Umfang und Abnahme unten. |

### Spielbetrieb 0.61 — neue konkrete Lücken (2026-10-16)

- **Pumpenquelle fehlt in Betriebsdiagnose:** Beide Ansaugstellen hatten
  waterDepth=0, während inspect_building_operation keine Quellenblockade meldete.
  Ansaugzelle, Wasserstand, Verunreinigung und quellenseitige Betriebsbedingung
  strukturiert ausweisen; fehlende Warnung nicht als Förderfähigkeit behandeln.

- **Planschlüssel nach Zugangsergänzung:** `plan_large_project_access` lieferte
  einen gültigen Plan; direkter Start bei unveränderter Pause meldete
  `project_start_plan_changed`. Keine Baustelle entstanden. Frische explizite
  Gesamtvorschau mit denselben Schritten startete erfolgreich. Cache-/Vorschau-
  Identitäten als Ursache untersuchen, nicht ohne Beleg behaupten.
- **Gefahrenanzeige:** Unstable-Core-Alert enthält `{0} days` statt Countdown;
  Wirkungszone und gefährdete Wasser-/Wegeverbindungen fehlen. Ergänzt B14.
  Nutzerhinweis beim Versionsabschluss: Die Bombe/der instabile Kern auf dem Berg
  bleibt eine konkrete offene Gefahr. Objektposition, nativer Countdown, erwartete
  Geländeöffnung und Freisetzung hochliegenden Wassers zusammenführen. Der 0.62-
  Gefahrenkopf erkennt Restwasser, aber prognostiziert diese Explosion nicht.
  Umsetzung nach Nutzer-Go: native Radiuszellen, hypothetischer Vor/Nach-Geländevergleich
  und kompakte Kern–Faulwasser–Ziel–Schutzketten implementiert; Gate bestanden,
  kompakte Vorwarnung bei neun Kernen live belegt. Schutzquerschnitt beim Bergkern
  unvollständig, Vor-/Nach-Schwelle unverändert. Warnung samt soweit möglichen
  Eindämmungsvorschlägen gemäß Nutzerpräzisierung am 17. Oktober abgenommen;
  vollständiger Schutzbau und Ereignisvergleich sind Folgearbeiten, keine Abnahmeblocker.
  Countdown, endgültige Wirkungszone und Einsturz bleiben ungeprüft.
  Historischer Recheck vor der Erweiterung: vollständige Terrainabdeckung und alle 501 Spezialobjekte
  gelesen; neun `UnstableCore` werden nur als `special_or_unknown` geführt. Compact
  warnt vor hochliegendem kontaminiertem Restwasser, verknüpft die Kerne aber nicht
  mit einem Durchbruch. **Konkreter B14-Folgeauftrag:** native Kernzustände und
  belegbare Wirkungszellen ermitteln; Szenario vor/nach Geländeöffnung vergleichen,
  betroffenen Speicher, Kontamination, Zuflussrinne und Zielgewässer verbinden.
  Compact muss Kernposition, Auslöser/Frist (sonst ausdrücklich unbekannt), bedrohtes
  Gewässer und vorab prüfbare Schutz-/Umleitungsstellen zeigen. Ohne belegten
  Explosionsradius keine erfundene Öffnung. Abnahme: noch geschlossene Sperre,
  Faulwasser dahinter, Teich darunter; Warnung vor Auslösung, danach Routenvergleich.
  Bildorientierung ist ohne Kamerabezug keine sichere Objektzuordnung.
- **Kollisionsdiagnose:** precheck_building nennt belegte Zellen, aber keine
  verursachenden Objekt-IDs/-typen; erschwert gezielte Räumung und Standortwahl.
- **Kraftplanung:** Generator-/Verbraucher-Verbindung als explizites Planungsziel;
  `rotationMatches=false` bedeutet im belegten Laufradfall keine fehlende reale
  Verbindung. Portausrichtung, Netzverbindung und Produktion verständlich trennen.
- **Fortlaufende Simulation:** Bauchargen pausieren zwingend am Abschluss.
  Optionale ausdrücklich beauftragte Abschlussgeschwindigkeit prüfen, damit
  Spielbetrieb auf Tempo 7 keinen zusätzlichen Resume-Aufruf braucht.
- **Gemeinsamer Versorgungsstart (B07):** Wasserpumpe, Beerensammler und
  Holzfällerfahne einschließlich Wegen, Fällmarkierung, erreichbaren Quellen und
  Personal in einem Plan vorbereiten. Danach Wohnraum und Erfinder, anschließend
  Sägewerk mit Kraftversorgung. Fertige Gebäude allein reichen als Abschluss
  nicht: Förderung, Sammlung und Holzeintrag getrennt nachweisen. Vorhandene
  Gruppenplanung wiederverwenden; fehlende gemeinsame Quellen-/Personalprüfung
  ergänzen, keinen neuen Planer allein für diese Reihenfolge schaffen.

### B14 — Strategische Sicht statt isolierter Kartenausschnitte

Positive Terraforming-Optionen implementiert, Gate und begrenzter Livepilot bestanden: begrenzte 3×3-Reservoirsuche mit zwei Ebenen Absenkung, Höhlen-/Seitenwand-/Objektprüfung, geometrischem Volumen und expliziten Zufluss-/Kontaminationsblockaden. Freie Beckenformen, echte Vorratsreichweite, Nachfüll-/Abflussplanung und Bewässerungswirkung bleiben offen. B15 ist mit 0.63 im oben dokumentierten Umfang abgeschlossen.

Compact/Full-Nachtrag live geprüft: Bedrohung und Gegenmaßnahmen im Standard, vollständige Inventur/Geometrie nur in full. Gemeinsame Maßnahmen und offene Prüfpunkte ohne Wiederholung.

Nachtrag live abgenommen einschließlich Dürrefall: Gefahrenkopf über alle erreichbaren
nassen Hohlräume; Restwasser vor Quellhinweisen, Zielhöhe berücksichtigt;
priorisierte Schutzstellen mit Dachprüfung und geometrischem Nebenweg-Kontrollfall.
Weiter offen: Unstable-Core-Countdown/Wirkungszone, Durchbruch- und zeitliche
Pegel-/Geländeänderungsanalyse, Wasserbauwerkszustände und hydraulische Wirkung.


Ziel präzisiert: allgemeine Mapawareness und Flüssigkeitsübersicht, nicht nur
Flutschutz einer Zielzone. Kartenweit Quellen, Flussbetten, Tunnel, Becken,
Verzweigungen, Überläufe und Kartenabflüsse samt Verbindungen darstellen.
0.62 nach menschlichem Gate begrenzt live abgenommen: zielunabhängiger Flussbericht mit blätterbaren
Abschnitten/Verbindungen, nativen Quellen-/Wetterdaten und Vergleich zweier Scans.
Offen: tatsächliche Strömung/Wasserbauwerke, zukünftige Kontamination ohne native
Regel und verifizierter Wettercountdown. Dürrebeginn mit zehn auf null fallenden
Quellenstärken und korrekt gemeldeten Differenzen live belegt; Faulwasserflutwechsel
und spezielle Aktivierungsregeln weiterhin offen.
Der nächste fachliche Ausbau muss drei getrennte Ebenen liefern:
- **Mögliche Wege:** Geometrische Verbindungen und Schwellen, auch trockene Rinnen;
  Richtung nicht allein aus Nachbarschaft ableiten. Wasserbauwerke und deren
  tatsächliche Einstellungen berücksichtigen oder als unbekannte Barriere melden.
- **Aktueller Zustand:** Beobachtete Pegel, Wasser/Faulwasser und Mischungen mit
  gemessener Kontamination je Abschnitt; native Quellentypen und Aktivität.
  Quellentyp ist nicht automatisch der Flüssigkeitstyp im gesamten verbundenen Netz.
- **Erwartete Änderung:** Wetter-/Ereignisphase, verbleibende Zeit, belegtes
  Quellenverhalten und mögliche Ausbreitungswege zusammenführen. Bedingte Szenarien
  von Prognosen trennen; ohne Durchfluss-/Mischungsnachweis keine Ankunftszeit oder
  künftige Reinheit behaupten. Unbekannte Wetterdaten ausdrücklich ausweisen.
Abnahme: zusammenhängendes Flussnetz auch ohne Siedlungsziel, trockener Überlauf,
getrennte Höhlenstockwerke, Zusammenfluss von Wasser und Faulwasser sowie
Ereigniswechsel mit nachvollziehbarer Änderung von Quelle und Flüssigkeitszustand.

0.62 ist als erster zusammenhängender Umfang **begrenzt live abgenommen**:
Livepilot bestätigt Gesamtabdeckung bei Tempo 7 und einen verseuchten Tunnel.
Quellenhöhen-/Kronenfehler korrigiert und live nachgeprüft. Für den nächsten Ausbau:
einzelne freie Höhenintervalle gezielt abrufbar machen, vollständige Routen blättern,
Querschnitte gegen Tunneldecken prüfen und nicht platzierbare Bauhöhen explizit
kennzeichnen. Die Priorisierung gespeicherten Wassers vor trockenen Quellhinweisen ist inzwischen abgenommen.
Aktueller Umfang: fortsetzbarer Gesamtscan mit freien Höhenintervallen, Spezialobjekten, geometrischen
Zuflusskorridoren und Sperrquerschnitten. [Vertrag/Pilot](docs/strategic-map-0.62.md).
Offen bleiben insbesondere native Wasserbauwerks-/Strömungsmodellierung,
vollständige Gefahrenabdeckung statt repräsentativer Korridore, Wetterprognose,
globale Änderungsrevision, native Quellklassifikation und reale Schutzwirkung.

- Kartenweite Bestandsaufnahme mit Kartengrenzen, Höhenbereichen und expliziter Suchabdeckung; paginiert bzw. fortsetzbar, kompakte Übersicht mit gezielten Detailabfragen. Untersuchte, veraltete und unbekannte Bereiche unterscheiden. Im laufenden Spiel nutzbar, ohne lange Planungspause.
- Oberflächen und unterirdische Hohlräume getrennt darstellen: Höhlen, Überhänge, offene Schächte, Eingänge, Rampen, Engstellen und mögliche Übergänge zwischen Höhen. Zusammenhängende Regionen und Verbindungen als Topologie anbieten; geometrische Verbindung, Biberweg, Distriktanschluss und Bauarbeiterzugang getrennt belegen.
- Spezialobjekte nach nativer Kategorie/Vorlage und Position finden, etwa Wasser-/Schlechtwasserquellen, Ruinen und andere vorhandene Kartenobjekte. Unbekannte Typen sichtbar lassen; keine Namen oder Fähigkeiten erfinden. Eine lokale Abrisszielabfrage mit Kategorie `other` ersetzt diesen Index nicht.
- Wasserflächen und Reservoirs zusammenhängend erfassen: offene oder überdeckte Lage, Oberfläche/Boden, Tiefe, Verschmutzung, Ufer und mögliche Pumpenstandorte. Aktueller Wasserbestand ist keine Garantie für Dürredauer oder erreichbaren Trinkwasserbetrieb.
- Skalierung an Vegetation und großen Karten prüfen. Livebefund vom 2026-10-16: `navigation_object_limit` in der Einzelvorschau, während Projektplaner nur `state_conflict` meldeten. Globale Objektgrenzen dürfen weder unbemerkt die Abdeckung abschneiden noch den konkreten Ablehnungsgrund verbergen.
- Kleiner Abnahmepilot: offenes Wasserloch gegenüber überdecktem Wasser, zwei getrennte Höhen mit/ohne Übergang und ein Spezialobjekt außerhalb des Siedlungsumfelds. Übersicht gegen native Detaildaten prüfen; Abdeckung, Aufwand und Grenzen ausweisen, bevor auf die gesamte Karte skaliert wird. Keine Implementierungs- oder Architekturfreigabe durch diesen Backlogeintrag.

B01 plus begrenzte Korridore aus B02 sind mit 0.51 im beauftragten Umfang erledigt.
B04 ist als 0.52 im begrenzten Umfang live abgenommen. Mehrere oder größere Räumflächen bleiben offen.

## Offene Folgearbeiten nach Phase E

Die ursprünglichen IDs bleiben als Verweisziele erhalten; ihre alten Grenzen
sind nicht pauschal der aktuelle Funktionsumfang.

| ID | Aktueller Restumfang |
| --- | --- |
| F01 | Vollständiger Bauphasenschutz: Bestandsbaustellen, Zugänge während aller Änderungen und freie Rückwege betroffener Biber. Zugangsergänzung und begrenzte Bau-/Fertigkontrollen sind bereits abgenommen. |
| F02 | Ersatzumweg erst fertigstellen und seine Nutzbarkeit bestätigen, anschließend abhängige Bestandsänderung. Der Anschluss um ein Hindernis allein beweist diesen Ablauf nicht. |
| F03 | Teilweise erledigt: explizite 32-Teil-/acht-Höhen-Pläne, autonome Chargen und Zugangshilfen vorhanden. Allgemeine räumliche Standort-/Routensuche bleibt B01–B03. |
| F04 | Besondere Standort-, Geometrie- und Distriktfälle gezielt prüfen. Mehrere Türen nur bei belegter Spiel-API; Bauzugangszellen sind keine zusätzlichen Eingänge. |
| F05 | Repräsentative weitere Fraktionen, Karten und Spielversionen. Iron-Teeth-Rezepte sind generisch vorgesehen, aber nicht durch den Folktails-Pilot mitabgenommen. |

## Zurückgestellte Experimente

Der Hermes-Integrationstest wurde am 2026-10-17 auf Nutzerwunsch beendet.
Proxy, Integrationsanleitung und Skillvorlage wurden aus dem Projekt entfernt;
keine weitere Umsetzung, Installation oder Versionspflege für Hermes vorgesehen.
Historische Release-Erwähnungen bleiben als Nachweise früherer Arbeiten erhalten.
Andere lokale Startskript-/Budgetexperimente sind kein Auftrag für den nächsten
Release und bleiben außerhalb dieses Rückbaus.

## Zurückgestellte Nachweise

- Große Gebäude: bis 512 native Blockeinträge und DomedGarden begrenzt live belegt.
  Offen bleiben Großgebäude ohne Eingang, native Hex-Platzierung mit isolierter
  Außenzellenkontrolle und Mine auf ungeeignetem Standort. Vorbereitung ist keine
  Liveabnahme. Kein erneuter stundenlanger Forschungsaufbau ohne Umfangsentscheidung.
  [Nachweise](docs/large-building-geometry.md).
- Allgemeiner Kraftgeometrie-/Drehungsnachweis: widersprüchliche Vorschauindikatoren
  gegenüber realer Netzverbindung gezielt prüfen, nicht aus einzelnen erfolgreichen
  Kraftpiloten auf alle Kombinationen schließen.
- Historischer Vier-Wege-Stash: nur bei konkretem Bedarf gegen heutige Umsetzung
  prüfen; kein aktiver Entwicklungsauftrag und kein ungeprüftes Wiedereinspielen.

## Erledigt im jeweils dokumentierten Umfang

| Bereich | Abschluss / Beleg |
| --- | --- |
| Gruppenpläne, gemeinsame Wege, native Kandidaten | 0.48–0.50; [effiziente Gruppenplanung](docs/efficient-group-planning.md). Die drei Folgepunkte aus 0.49 sind damit im vereinbarten Umfang erledigt. |
| Kompakte Ergebnisse, Ausführungsgewissheit, günstiges Warten | 0.45–0.47; [Aktionsberichte](docs/action-results-and-run-reports.md), [Projektberichte](docs/compact-project-reports.md), [Status/Warten](docs/lightweight-status-wait.md). |
| Rezepte, Gebäudesuche, gemeinsame Konfiguration, Produktion und Engpässe | 0.38–0.44; [Projektstand](PROJECT_STATE.md). Keine automatische Produktionskettenplanung. |
| Spieländerungsjournal | 0.40, Pflanzen/Flaechen mit [0.54](docs/spatial-journal-0.54.md); [Vertrag](docs/recent-actions-journal.md). Erweiterungen siehe B09. |
| Autonome Baucharge, Abschluss, Diagnose und Zugangsergänzung | [Chargen](docs/autonomous-construction-batches.md), [Reparatur](docs/project-recovery.md), [Zugangsplanung](docs/automatic-access-planning.md). Kein vollständiger Navigationsschutz. |
| Waldreichweite, Stümpfe, artenbezogener Fällschutz und weniger Reads | [Waldnachweis](docs/forestry-range-protection.md). Fällschutz ist keine dauerhafte Schutzregel gegen spätere Eingriffe. |
| Screenshot und persistenter Codex-Vorschaltprozess | [Screenshot](docs/screenshot-0.36.0.md), [Lifecycle](docs/codex-mcp-supervisor.md). UI-Button bleibt B11. |

## Dokumentationsklärung

[Prüfbericht 2026-10-12](docs/documentation-audit-2026-10-12.md): korrigierte
Altstände und die anschließend getroffenen Nutzerentscheidungen. Die damalige
Hermes-Rückstellung ist durch die Entfernung am 2026-10-17 überholt. Lokale
Budgetänderungen nicht aus dem Versionsnamen automatisch als abgenommen behandeln.

## Folgebeobachtungen aus 0.51

- Suchbudget wirkt: 32 Vorschauen statt Greedy-Frühabschluss; Viererplan spart drei
  Hilfsteile. Das feste Tiefensuchbudget kann frühere Wünsche lange festhalten.
  Breitere Zielreihenfolge nur bei gemessenem Nutzen, keine automatische Budgeterhöhung.
- Die native Bauzugangsplanung kann für kleine Gruppen weite Umwege erzeugen.
  Gesamtplan-Kosten sichtbar lassen; Rückwärtssuche garantiert keine kürzeste Route.
- Ein zu schmales Suchfenster ergab keine Kandidaten, die größere Kontrollfläche
  funktionierte. Suchabdeckung nicht mit Reservierungsablehnung verwechseln.
- Client zeigte nach Neustart noch die 0.50-Werkzeugbeschreibung, Backend nahm
  0.51-Parameter korrekt an. Katalog-Veraltungsdiagnose bleibt B12.

## Folgebeobachtungen aus 0.52

- Compact ist im Zweierpilot 92,2 % kleiner (67.790 auf 5.268 JSON-Zeichen),
  enthält aber noch viele leere Produktionsfelder im Räumlauf. In 0.53 gekuerzt; automatischer Nachweis im bestandenen Gate.
- Ungültige jobId wurde korrekt vor Transport abgewiesen; die allgemeine
  Fehlermeldung verschweigt jedoch die konkrete UUID-Anforderung. Feste Hinweise in 0.53 implementiert und im Gate geprueft.
- B12 bleibt: neue Werkzeuge wurden erst nach Codex-Neustart verfügbar,
  obwohl der Vorschaltprozess bereits den aktuellen Katalog meldete.

## Nacharbeiten aus dem 0.53-Livepilot

Am 12. Oktober 2026 abgeschlossen: Bestaetigte native Vorschau-Ablehnung meldet
not_executed; Compact reduziert passende Markierungen auf Summen. Menschliches
Gate und gezielter Livetest bestanden; Full bleibt vollstaendig.
Weiter offen: MCP-Aufrufe sind seriell, parallele Leser werden sofort abgewiesen.
Die feste Argumentdiagnose nennt den laufenden Aufruf; keine Warteschlange.

### Pumpengeometrie im automatischen Zugangsplaner (Live 0.61)

- Reservoirpumpe wird von plan_large_project_access mit target_intersects_existing_or_planned_object abgewiesen, ohne Objekt im lokalen Baufeld. Dieselbe Pumpe ist in plan_large_project nativ gueltig; mit expliziter Treppe und Wegen auch verbunden und pilotEligible. Vermutung: unterirdische Any-Zellen werden pauschal als Hindernis behandelt. Mit nativen Supportregeln abgleichen und konkrete Zelle/Objekt/Regel ausgeben.
- Unbekannte Wegvorlage Path.Folktails ergibt im reinen Planaufruf invalid_argument mit pauschalem execution=unconfirmed ohne konkrete Ursache. Katalogvorlage ist Path. Vor Transport validieren oder native Argumentdiagnose erhalten; lesende Planung und moegliche Vorschauwirkungen von Bauauftrag unterscheiden.

### Hochwasserdiagnose und Ausweichplanung (Live 0.61)

- **Präventiver Gefährdungsscan (B14):** Wohn-/Versorgungszone als Zielgebiet wählen; zuführende Rinnen, Durchbrüche, Überlaufkanten und Quellen auch außerhalb des Siedlungsumfelds erfassen. Gefahrenpfad, Höhenprofil, Suchabdeckung und Unsicherheit ausgeben. Priorisierte Eingriffspunkte mit Bauzeit, Kosten, Zugang und möglichen Ausweichfließwegen vorschlagen; geometrisch möglicher Zufluss ist noch keine hydrologische Prognose.
- **Terraforming/Wasserbau im Early- und Midgame:** Verfügbare native Dämme, Deiche, Wassersteuerung und Geländeänderungen samt Freischaltungen systematisch erschließen. Schließen oder Umleiten gefährlicher Rinnen, Entwässerung, Rückhalt und Höhenzugänge als zusammenhängende Vorhaben planen; Bauphasen und Rückstau berücksichtigen. Reguläre Fähigkeiten von Entwicklungswerkzeugen trennen. Abnahmepilot: Zufluss zur Siedlung mit kontrollierter Ableitung sperren, Vorher-/Nachher-Pegel und neue Überläufe prüfen; keine Schutzgarantie aus bloßer Fertigstellung.
- inspect_building_operation meldet Flooded als Text, assessment.blockers aber nur fehlendes Personal/Arbeitszeit; ueberflutetes Inventar unblocked=false. Native Ueberflutung als expliziten Betriebsblocker abbilden, damit keine falsche Personalreparatur folgt.
- Globale Vorraete auch bei ueberfluteten Lagern positiv: tatsaechlich erreichbare Trinkwasser-/Nahrungsreserve getrennt ausweisen.
- Reserven nach Distrikt, Lager-/Produktionsausgabe und tatsächlichem Zugriff aufschlüsseln; unzugängliche, überflutete oder nicht trinkbare Bestände nicht als verfügbare Notreserve zählen. Verbrauchsreichweite aus beobachtetem Verbrauch mit Zeitfenster und Unsicherheit ableiten.
- Strategische Karte B14 um Flusszufluss/Abfluss, Wasseroberflaechenhoehe, Hoehenprofil, Kontaminationsfront und erreichbare hoehere Ausweichflaechen erweitern. Lokale Baugueltigkeit allein erkennt keine bevorstehende Ueberflutung.
- Hochwasser-Livebeleg: Paused kann Flooded im sichtbaren Statuslisting verdecken. Direkten nativen Flutzustand unabhaengig von sichtbaren Statusprioritaeten abfragen; fehlender Fluttext beweist keine Betriebsfaehigkeit. Wasserstandtrend an festen Messzellen statt wechselnden Regionen fuer Zu-/Abflussbewertung verwenden.
- Wetter-/Gefahrenphase, verbleibende Dauer und bekannte Vorwarnzeit strukturiert liefern; unbekannte Dauer explizit kennzeichnen. Dürre, Schlechtwasser und andere native Ereignisse unterscheiden. Keine exakte Flutprognose aus einer einzelnen Tiefenmessung behaupten.
- Evakuierung und Ausweichdistrikt als fehlende Fähigkeit prüfen: höher gelegenen erreichbaren Standort, sichere Route, Trinkwasser/Nahrung, Unterkünfte und Baupersonal gemeinsam bewerten; native Migration mit tatsächlichem Ankunftsnachweis ermöglichen. Vor Überflutung des Bauzentrums vorbereiten; ein Bauauftrag ohne verfügbare Bauarbeiter ist keine Rettung.
- Notentwässerung/Quellensteuerung: vorhandene native Bau-, Abriss- und Wassersteuerungsmöglichkeiten mit Voraussetzungen und Wirkungsnachweis erschließen. Im Livefall fehlten MCP-Eingriffe zum Öffnen eines Abflusses bzw. Abstellen der Flutquelle. Regulär erreichbare Maßnahmen von gesonderten Entwicklungs-/Geländeeingriffen unterscheiden; keine vorhandene Fähigkeit oder rechtzeitige Wirkung unterstellen.
