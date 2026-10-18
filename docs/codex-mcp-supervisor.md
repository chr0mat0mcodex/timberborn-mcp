# Persistenter MCP-Vorschaltprozess

## A09-Eingangspilot (Gate und Lifecycle-Liveabnahme bestanden)

Clientzeilen maximal 1 MiB inklusive Zeilenende. Fehlerhafte oder übergroße
Nachrichten werden mit kontrolliertem JSON-RPC-Fehler verworfen; keine automatische
Wiederholung. Inflight-Grenzen und Eingangstimeouts noch nicht umgesetzt.
Nach vollständigem Codex-Neustart Stop/Status/Start mit gleicher Spielsitzung
live bestätigt. Python-Skriptänderungen benötigen weiterhin einen vollständigen
Client-Neustart; Backend-Stop/Start allein lädt den Supervisor-Code nicht neu.
[Umfang, Fehlergrenzen und Nachweise](supervisor-input-pilot.md).


## Statuswarten und Laufabbruch

Der Supervisor leitet Requests bereits parallel weiter. Die Backend-Sperre wurde
korrigiert; im Livepilot kam Cancel dennoch erst nach Ende des Statushandlers an.
Der Folgepilot lokalisiert die Serialisierung vor dem Supervisor: Cancel kam
erst 1,894 s nach der Statusantwort an und wurde intern in 116 ms beantwortet.
Nacharbeit mit 28 Supervisor-Tests und begrenztem Livepilot abgenommen:
Statuswarten im Codex-Supervisor auf eine Sekunde begrenzt, ergänzt um lokale,
auf 128 Einträge begrenzte Zeitmarken unter `.local/codex-mcp-supervisor/`.
Dateinamen enden auf `-timing.log`; keine Argumente, IDs oder Antwortinhalte.
Das erfordert nach dem menschlichen Gate einen vollständigen Codex-Neustart,
damit der persistente Supervisor seinen neuen Code lädt. Kein separater stdio-
Prozess als Ersatz. [Vertrag und offener Livepilot](lightweight-status-wait.md).
Lifecycle-Stop ist weiterhin kein Ingame-Pausennachweis.

## Gate-Stopp-Nacharbeit seit 0.66

Wiederholte CLI-Stopps behalten das vorhandene Gate-Ticket; keine erneute
Quittungsrunde nur wegen Wiederholung. Prozessidentitäten werden weiter geprüft.
Nur nachweislich verwaiste, unveränderte Instanzmeldungen werden gelöscht;
alte Zeitstempel allein reichen nicht. Vor einem Timeout ausstehende Quittungen
frisch lesen. Lebende/unlesbare Instanzen blockieren weiterhin den Build.
CLI-Stopp nach Bereinigung und Wiederholung erfolgreich; Regressionen sind im
menschlichen Gate enthalten und bestanden. [Befund](releases/0.66.0.md).
Der CLI-Helfer lädt die Änderung beim nächsten Aufruf; dafür ist kein Neustart
der verbundenen Frontends nötig. Schon laufende Frontends behalten ihren
geladenen Python-Code bis zum nächsten regulären Client-Neustart.

Aktueller Stand: menschliches Gate mit 23 Supervisor-Tests bestanden.
Nach vollständiger Client-Neuverbindung Stop/Status/Start mit aktualisiertem
Python-Skript live bestätigt. Die folgenden frühen Werkzeugzahlen sind historisch.
Stop/status/start über dieselbe Codex-Verbindung live bestanden; Spielstatus vor
und nach Backendwechsel erreichbar, gleiche Spielsitzung. Werkzeugübernahme nach
erstmaliger Cachebefüllung und Client-Neustart bestätigt (76 Backendwerkzeuge).
Regionalsuche anschließend erfolgreich über diese Verbindung live geprüft.

Codex startet `scripts/codex-mcp-supervisor.py` statt `start-native.ps1`.
Dieser stdio-Prozess bleibt während des Gates verbunden und startet ausschließlich
die vorhandene Release-DLL als Kindprozess. Kein zusätzlicher Build, keine Pakete,
keine zweite Modfassung. Voraussetzung: Python 3.8+ und dotnet im PATH.

## Einmalige Einrichtung in Codex

In der vorhandenen Codex-`config.toml` nur `command` und `args` des bestehenden
Abschnitts `[mcp_servers.timberborn]` ersetzen, keinen zweiten Server anlegen:

```toml
[mcp_servers.timberborn]
command = 'python'
args = ['-B', '-u', 'ABSOLUTER_PROJEKTPFAD\scripts\codex-mcp-supervisor.py', '--config', 'BISHERIGER_CONFIGPATH']
```

`BISHERIGER_CONFIGPATH` ist genau der bisherige Wert hinter `-ConfigPath`, nicht
der Tokeninhalt. Vorhandene Timeout-/Freigabeeinstellungen erhalten. Bei mehreren
Python-Installationen den absoluten Pfad des Interpreters aus `Get-Command python`
verwenden. Codex danach einmal vollständig beenden und neu starten, damit die alte
stdio-Verbindung ersetzt wird. Die globale Konfiguration wird nicht vom Projekt
verändert oder versioniert.

## Ablauf

- Vor dem Gate: `timberborn_server_control` mit `action: stop`. Alternativ
  `scripts/stop-codex-mcp.ps1`. Ergebnis muss `running: false`, `gate: true` sein.
- Der Vorschaltprozess beendet nur sein eigenes Kind. Alle Vorschaltinstanzen
  dieses Projekts bestätigen die dateibasierte Sperre. Kein Bestätigungsnachweis:
  Abbruch vor Build. Registrierte Prozesse werden auf Existenz und Prozessidentität geprüft.
- Das menschliche Vorbereitungsskript setzt dieselbe Sperre erneut. Als Rückfall
  beendet es direkt gestartete dotnet-Server nur mit genauem Projekt-Releasepfad.
  Fehler lassen die Sperre bestehen; kein automatischer Neustart oder Auftragsretry.
  Begrenzte Dateiaustauschversuche sind unten gesondert beschrieben.
- Nach erfolgreichem Gate, geladenem Spiel und `live bereit`: Agent ruft `action:
  start` auf. Dies hebt die Sperre auf und startet das Backend dieser Verbindung.
  Andere gestoppte Instanzen starten nicht automatisch. Danach `timberborn_status`
  und den fachlichen Feature-Nachweis abfragen.
- `action: status` startet nichts. Während der Pause bleibt die zuletzt bekannte
  Werkzeugliste verfügbar; Spielaufrufe werden mit Fehler zurückgewiesen.

Beim Start werden initialize/initialized und tools/list erneut mit dem Backend
ausgetauscht. Änderungen der Werkzeugliste werden per tools/list_changed gemeldet.
Zusätzlich wird der Katalog lokal gespeichert und beim nächsten Frontendstart auch
während Gatesperre angeboten. Im Livepilot übernahm die laufende Agentensitzung
die Änderungsnachricht nicht; nach Erstbefüllung ist daher ein weiterer Clientstart
nötig. Dasselbe kann bei neu hinzugefügten Werkzeugen erforderlich sein.
Ein beim Stoppen oder Absturz offener Aufruf erhält „action outcome unknown“.
Seine Wirkung muss im Spiel nachgeprüft werden; keine Wiederholung durch den Proxy.
MCP-IDs werden übersetzt, Abbruchbenachrichtigungen zugeordnet, Bildinhalte und
Schemas durchgereicht; die beiden Statuswerkzeugtexte erläutern die
Ein-Sekunden-Grenze. Gültige waitSeconds-Werte werden beim Weiterleiten gekürzt. Client-Callbacks werden nicht angeboten.
Der Proxy bietet nur tools und ping; keine Ressourcen-/Prompt-Fähigkeiten.

## Grenzen und Abnahme

Das ist Backend-Neustart, kein universeller Codex-Reconnect. Wird der Vorschaltprozess
selbst beendet, muss Codex die Verbindung erneut herstellen. Änderungen an seinem
Python-Code brauchen ebenfalls einen einmaligen Client-Neustart. Bei gewaltsamem
Beenden des Vorschaltprozesses kann ein Backend verwaisen; der genaue Pfadabgleich
im menschlichen Skript räumt solche Prozesse vor dem Build auf. Normales stdio-Ende
beendet das Kind. Keine allgemeinen Prozessabschüsse.

Synthetische Tests im menschlichen Gate: Handshake nach Neustart, Werkzeugliste
während Pause, Sperre schon beim Erststart, Unterbrechung eines offenen Auftrags,
Backend-Absturz ohne automatischen Neustart, mehrere Instanzen und fehlende
Stoppbestätigung. Keine echten Spielzugriffe in diesen Tests.

Live-Nachweis nach einmaliger Einrichtung: start → timberborn_status → stop →
status bleibt über dieselbe Verbindung erreichbar → start → timberborn_status.
Bei unverändertem Supervisor ist kein Client-Neustart zwischen stop und start nötig.
Nach Änderungen am Python-Skript muss die Client-Verbindung vollständig erneuert werden. Anschließend die offene
[Regionalsuche-Abnahme](regional-survey.md). Bei Transportverlust oder fehlender
Stoppbestätigung abbrechen und diagnostizieren; kein Commit/Push vor beiden Belegen.

Runtime-Dateien liegen ausschließlich unter `.local/codex-mcp-supervisor/`, enthalten
keine Zugangsdaten und werden nicht versioniert. stdout bleibt JSON-RPC; Diagnose
geht nach stderr. Das Skript gibt die private Bridge-Konfiguration niemals aus.
# Gate-Korrektur vom 2026-10-11 — Liveprobe bestanden

Nach menschlichem Gate/Bereitmeldung: Backendstart und Bridgekontakt erfolgreich.
Abschließend Stop mit running=false/gate=true, danach Start über dieselbe
Verbindung mit running=true/gate=false und unveränderter Spielsitzung bestätigt.
Kein erneuter Codex-Neustart für den Stop/Start-Pilot erforderlich.

FILE_SHARE_DELETE allein ist im menschlichen Gate fehlgeschlagen. Stattdessen
serialisiert ein benannter Windows-Mutex je Dateipfad Leser, Austausch und
Löschen über Prozesse hinweg. Sperrzeitlimit fünf Sekunden; kein Datei-Retry.
Ein Schreibfehler stoppt das Backend ohne weiteren Publish-Versuch im Fehlerpfad;
das Gate verlangt weiterhin eine tatsächlich geschriebene Stoppbestätigung.
Keine Wiederholung von Builds oder Spielaktionen. Zwei Regressionen im menschlichen Gate enthalten:
Ein separater Schreiber wartet bei offenem Leser und ersetzt danach erfolgreich;
Backend-Stopp bei ausgefallenem Statusschreiben. Alte Vorschaltprozesse laden
Codeänderungen erst beim Wiederaufbau ihrer Verbindung. Der nachgewiesen hängende
Altprozess wurde mit ausdrücklicher Nutzerfreigabe gezielt beendet; kein laufendes
Projekt-Backend verblieben. Anschließendes Gate und Wiederanschluss siehe oben.

Gate-Nachtrag 2026-10-11: Die temporären Dateien des Cross-Process-Austauschtests
liegen jetzt in einem eigenen Unterverzeichnis. Die bisherige globale `*.tmp`-
Prüfung erfasste gelegentlich einen regulären parallelen Heartbeat-Schreibvorgang
der Testfixture. Warteprüfung, erfolgreicher Austausch, Inhalt und restlose
Bereinigung des getesteten Schreibvorgangs bleiben unverändert geprüft.
Nur Testisolation geändert, kein Lifecycle-Code und keine Retries. Ausführung
der Korrektur erfolgte im anschließenden menschlichen Gate vor erneuter Bereitmeldung.

## Historische Gate-Diagnose in 0.53

Der menschliche Stop/Start-Test scheiterte am erneuten Start mit einer generischen
Fehlermeldung. Die darin versprochene lokale Diagnose wurde bisher nicht geschrieben;
die ursprüngliche Betriebssystemursache ist deshalb nicht nachgewiesen.
Startphasen Gate-Freigabe, vorheriger Prozessabschluss, Prozesserzeugung,
Katalogspeicherung und Statusspeicherung liefern jetzt feste Phasennamen und
numerische Betriebssystemfehler nach stderr und im Fehlerergebnis, ohne Pfade
oder Konfigurationsinhalte. Scheitert der Start nach Prozesserzeugung, beendet
der Fehlerpfad das Backend ohne erneutes Statusschreiben; ein solcher zweiter
Speicherfehler darf die ursprüngliche Ursache nicht verdecken.
Zwei Regressionen erzwingen Katalog- bzw. Statusschreibfehler und
prüfen Diagnose, Prozessbereinigung und ausbleibende Schreibwiederholung.
Menschliches Gate anschließend bestanden (siehe Nachtrag). Kein Nachweis,
dass die unbekannte Ursache des gemeldeten Startfehlers damit behoben ist.

Nachtrag: menschliches Gate anschliessend bestanden; expliziter Backendstart
und Bridgekontakt 0.53.0 live bestaetigt. Die urspruengliche sporadische Ursache
bleibt unbekannt; kein neuer Nachweis eines provozierten Speicherfehlers live.

## Dateiaustausch-Korrektur nach 0.74 (Gate/Lifecycle bestätigt)

Windows kann den atomaren Austausch der Lifecycle-Statusdatei trotz eigenem
Mutex vorübergehend verweigern. Bei WinError 5, 32, 33 oder 1175 sind
höchstens vier Austauschversuche mit insgesamt 175 ms Wartezeit vorgesehen.
Vor erneutem Austausch werden Ziel- und Tempbytes verglichen; schon vollständig
publizierte Bytes gelten als Erfolg. Veränderte oder nicht lesbare Dateien und
andere Fehler brechen ab. Das wiederholt keine Backend-Starts, Stop-Aktionen
oder MCP-Aufträge und bestätigt keinen Stopp ohne gültigen gespeicherten Beleg.
23 Supervisor-Tests im menschlichen Gate bestanden. Nach vollständiger Client-
Neuverbindung Prozessstart nach Skriptänderung bestätigt; Stop/Status/Start live
erfolgreich. Backend-Neustart allein aktualisiert den persistenten Supervisor
nicht. Live keine Dateisperre injiziert und keinen Schreibauftrag unterbrochen.
