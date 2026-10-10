# Persistenter MCP-Vorschaltprozess

Stand: menschlicher Vollgate-Lauf und acht gezielte Python-Tests bestanden.
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
  Abbruch vor Build. Registrierte PIDs werden ausschließlich auf Existenz geprüft.
- Das menschliche Vorbereitungsskript setzt dieselbe Sperre erneut. Als Rückfall
  beendet es direkt gestartete dotnet-Server nur mit genauem Projekt-Releasepfad.
  Fehler lassen die Sperre bestehen; kein automatischer Neustart, keine Retries.
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
Schemas unverändert durchgereicht. Client-Callbacks werden nicht angeboten.
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
Kein Codex-Neustart zwischen stop und start. Anschließend die offene
[Regionalsuche-Abnahme](regional-survey.md). Bei Transportverlust oder fehlender
Stoppbestätigung abbrechen und diagnostizieren; kein Commit/Push vor beiden Belegen.

Runtime-Dateien liegen ausschließlich unter `.local/codex-mcp-supervisor/`, enthalten
keine Zugangsdaten und werden nicht versioniert. stdout bleibt JSON-RPC; Diagnose
geht nach stderr. Das Skript gibt die private Bridge-Konfiguration niemals aus.
