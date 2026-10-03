# Pflichtbegründung für MCP-Werkzeugaufrufe

Alle angebotenen Werkzeuge verlangen `reasoning`: String, 1–600 Zeichen,
nicht ausschließlich Leerraum. Das gilt für native Leser und Aktionen sowie
Fake-/Legacy-Backend. Kein `reason`-Alias; bestehender Feldname bleibt erhalten.
Eine kurze sichtbare Absicht, keine internen Gedankengänge oder Zugangsdaten.

Schema und zentrale Eingangsprüfung erzwingen die Pflicht. Falscher Typ,
doppelte Felder, verbotene Steuerzeichen und Überlänge werden ebenfalls abgewiesen.
Ungültige Metadaten erzeugen InvalidParams vor Backendzugriff und Aktivitätslog.
Die Begründung wird nicht an fachliche Aktionsparameter weitergereicht.
Log-Abschlussmeldungen sind interne Telemetrie, keine neuen Werkzeugaufrufe;
deren bestehendes Format bleibt unverändert.

## Nachweis 2026-10-03

Menschliches Skript-Gate durch anschließende Bereitmeldung bestätigt; Testanzahl
nicht übermittelt. Live gegen installierte Bridge 0.31.0: Status-/Zustandsabfragen
mit Begründung erfolgreich; fehlende und nur aus Leerraum bestehende Begründung
liefern -32602. Unabhängige inspect_agent_log-Abfrage zeigt die akzeptierten
Aufrufe, nicht die beiden abgewiesenen. Keine Spielaktion für diesen Nachweis.

Synthetische Tests prüfen alle angebotenen Schemaarten, Fehlerfälle, maximale
Länge und fehlenden Backendzugriff; vorhandene Integrationstests verwenden eine
gemeinsame Begründungshilfe. Eine weitere unabhängige Stdio-Negativkontrolle
prüft das Fake-Backend. Keine Behauptung einer Liveprüfung sämtlicher Werkzeuge.
