# A09-Pilot: begrenzter Supervisor-Eingang

Stand 2026-10-18: menschliches Gate und Lifecycle-Liveabnahme nach vollständigem
Codex-Neustart bestanden.

## Umfang

Nur der lokale Client-Eingang von `codex-mcp-supervisor.py` wird geändert.
`client_requests` liest binäre UTF-8-Zeilen mit maximal 1.048.576 Bytes inklusive
Zeilenende. Eine größere Zeile wird in höchstens 64-KiB-Stücken bis zum Zeilenende
oder EOF verworfen. Weder der Frame noch sein Rest werden weitergeleitet.
Das Limit liegt bewusst weit über gewöhnlichen Werkzeugargumenten; Antworten,
Bilder und der Backend-Katalog sind nicht betroffen. Keine neue Abhängigkeit.

Ungültiges JSON/UTF-8, nicht standardkonforme numerische Konstanten und zu tiefe
JSON-Verschachtelung ergeben einen kontrollierten Parsefehler (-32700, id=null).
Übergroße Frames und ungültige Requestformen ergeben -32600, id=null. Nur einzelne
JSON-RPC-2.0-Requests/Notifications mit Methodennamen und benannten Parametern
werden zugelassen; keine Batch-Unterstützung. IDs dürfen nicht aus Containern
oder bool bestehen; Cancel-Ziel-IDs werden vor Zugriff auf die Zuordnung geprüft.
Fehlerantworten enthalten keine Eingabeinhalte, Pfade oder Konfiguration.
Die nächste vollständige gültige Zeile wird regulär verarbeitet. Keine Warteschlange,
kein Replay, kein Backendneustart als Fehlerbehandlung.

Inflight-/Threadgrenzen bleiben ausdrücklich offen. Die bisherige parallele
Weiterleitung und Lifecycle-Verarbeitung werden nicht umgebaut. Ein Client, der
keinen Zeilenabschluss sendet, kann weiterhin den Eingang binden; kein Zeitlimit.
Stop über denselben stdio-Eingang kann einen unvollständigen Frame nicht überholen.
Die bestehende separate dateibasierte Gate-Steuerung bleibt unverändert.

## Synthetische Nachweise (Gate bestanden)

Sieben neue Testmethoden in `tests/supervisor/test_supervisor.py`:

- Parse-/UTF-8-/Verschachtelungsfehler mit anschließender gültiger Nachricht.
- Ungültige Nachrichtenformen und Cancel-Ziel-ID erreichen keinen Dispatch.
- Übergroßer Frame einschließlich scheinbarem JSON im Rest wird einmal verworfen.
- Exakte Bytegrenze mit mehrbyteigem UTF-8 und Überschreitung um ein Byte.
- EOF, letzte Zeile ohne Zeilenende und übergroße unvollständige Schlusszeile.
- Gültige Stop-/Cancel-Nachrichten bleiben nach Parsefehler erhalten, ohne Replay.
- Synthetisches Backend bleibt nach Parse-/Größenfehler und drei Ping-Requests
  derselbe Prozess; anschließender Stop beendet ihn und setzt die Gatesperre.

Keine Agent-Test-/Buildläufe. Der bestehende menschliche Gate-Aufruf führt die
Supervisor-Tests aus; Fehler nicht automatisch wiederholen.

## Liveabnahme

Nach erfolgreichem Gate **Codex vollständig beenden und neu starten**, damit der
persistente Supervisor das geänderte Python-Skript lädt. Nur Backend-Stop/Start
reicht dafür nicht. Anschließend Spiel laden bzw. geladen lassen und `live bereit`
melden. Keine globale Konfigurationsänderung oder zweite stdio-Instanz nötig.

Über die bestehende Verbindung: Backendstart, native Status-/Sessionabfrage,
Supervisor-Stop mit running=false/gate=true, Status ohne Neustart, expliziter
Start und erneuter nativer Status mit gleicher Spielsitzung. Kein Spielauftrag.
Fehlerframes werden ausschließlich synthetisch geprüft, nicht in die aktive
Codex-Protokollverbindung injiziert. Abbruch bei fehlender Stopbestätigung,
unerwartetem Neustart oder nicht erreichbarem Status; kein blindes Retry.

## Nutzen und Aufwand

Kleine isolierte Eingangsänderung mit begrenztem Speicher für rohe Frames und
lokaler Fehlerbehandlung. Wert: einzelne defekte Nachrichten beenden nicht mehr
den Frontendprozess. Aufwand: Parserfunktion, sieben Testmethoden und einmaliger
Client-Neustart zur Abnahme. Kein Last-/DoS-Gesamtnachweis, keine Aussage über
Inflight-Begrenzung. Gesamt-A09 bleibt offen; weitere Ausbaustufen nicht umgesetzt.
Bewertung positiv: klarer Zuverlässigkeitsgewinn mit kleinem, isoliertem
Codeumfang und sieben gezielten Testmethoden; keine umfassende Lastabsicherung.

## Tatsächliche Abnahme

Menschliches Gate: 13 Paket-, 35 Supervisor-, 2221 Unit- und 14 Integrationstests
bestanden; drei optionale Livefälle übersprungen. Beide Release-Builds ohne
Warnungen/Fehler, fünf installierte Paketdateien per SHA-256 bestätigt.
Alle sieben neuen Supervisor-Testmethoden bestanden. Keine Agent-Testläufe.

Der Nutzer bestätigte den vollständigen Codex-Neustart. Anschließend über die
bestehende Verbindung Backend gestartet und native Bridge 0.74.0 samt aktueller
Session erreicht. Build-/Katalogkennung entsprach dem Gatebericht (diese Kennung
attestiert nicht den Python-Supervisor). Stop lieferte running=false/gate=true;
anschließender Lifecycle-Status blieb ebenso, ohne Backendneustart. Expliziter
Start lieferte running=true/gate=false; nativer Status war wieder erreichbar,
mit derselben Spielsitzung und unveränderter MCP-Kennung. Kein Spielauftrag.

Fehlerframes und Größenüberschreitungen sind synthetisch geprüft; live ist der
normale Lifecycle nach Client-Neustart belegt. Inflight-/Threadlimits und
Eingangstimeouts bleiben zurückgestellt. Gesamt-A09 bleibt teilweise offen.
