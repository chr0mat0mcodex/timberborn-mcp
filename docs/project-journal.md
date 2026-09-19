# Projektjournal

## 2026-09-19 — Paket A

Nutzer hat Schritt 5 und damit die Umsetzung des Missionsplans freigegeben.
Sechs getrennte src-Projekte, zwei Testprojekte, projektlokale NuGet-/CLI-Caches und
gepinnten SDK-/Paketstand angelegt. Fake-Backend und fünf Read-only-Tool-Verträge umgesetzt.
Release-Build ohne Warnungen; drei Anwendungstests und ein echter MCP-stdio-Prozesstest bestanden.
Der echte Adapter folgt in Paket B. Noch keine MCP-Client-Konfiguration verändert.

Lesender API-Pilot erfolgreich: localhost:8080, Mod 11.0.0, Spiel 1.1.2.4.
`ping`, `misc`, `live-data`, `characters`, `buildings` und Einzelgebäude antworten.
Mapping-Besonderheiten: Mod-Aktivierung heißt `Active`, Bevölkerungslisten heißen
`Adult`/`Child`/`Bot`, Tagesfortschritt liegt in `TopBar.Cycle.Hours`.
Keine Live-Antworten, persönlichen Namen oder Spiel-IDs als Fixtures gespeichert.

## 2026-09-19 — Pakete B/C und technischer Live-Test

More HTTP API 11.0.0 angebunden. Zulässige Routen fest begrenzt; Loopback-Verbindung erhält den
Hostnamen localhost, deaktiviert Proxy/Redirects und begrenzt dekomprimierte Antwortkörper.
Einzelgebäude werden gegen die Gebäudeliste plausibilisiert. Fremdmod-Einstellungen und lokale
Verzeichnisse werden nicht weitergereicht. Wetterprognosen respektieren ShouldShowNext.

Alle fünf MCP-Werkzeuge wurden in einem echten stdio-Prozess gegen die vorbereitete Spielkolonie
erfolgreich aufgerufen. Simulationsmarker war false. Nutzer bestätigt den manuellen UI-Vergleich.
Die Spiel- und MCP-Client-Konfiguration wurde nicht geändert. Git-Identität auf Nutzerwunsch
pro Commit: Codex <codex@openai.com>; keine globale oder lokale Git-Konfiguration gesetzt.

Abschlussprüfung: Locked-Mode-Restore erfolgreich, Release-Build mit 0 Warnungen/0 Fehlern,
42 Anwendung-/Adaptertests und 5 deterministische Integrationstests erfolgreich.
Der Live-Test wird im normalen Lauf korrekt übersprungen und wurde separat mit explizitem Opt-in ausgeführt.
Nutzerabnahme erlaubt den Meilenstein `poc-readonly-v0.1` nach finaler Sicherung.

## 2026-09-19 — GitHub-Veröffentlichung vorbereitet

Nutzer beauftragt Veröffentlichung des Projekts einschließlich Agentenanweisungen und fortlaufende
Pushes geprüfter Checkpoints nach chr0mat0mcodex/timberborn-mcp. AGENTS.md bündelt projektspezifische
Grenzen und Einstiegspunkte; DEVELOPMENT_WORKFLOW.md dokumentiert Prüfung, Commit und Remote-Abgleich.
README und aktuelle Freigabegrenzen verweisen darauf. Keine persönlichen globalen Agentendateien übernommen.
Bestehende Commit-Dateilisten und verdächtige Textmuster vor Veröffentlichung geprüft; keine Zugangsdaten
oder persönlichen Benutzerpfade erkannt. Spielstände, Fremdbinärdateien und lokale Laufzeitdaten bleiben ausgeschlossen.

## 2026-09-19 — Lokaler Codex-Client eingerichtet

Auf ausdrücklichen Folgeauftrag `codex mcp add timberborn` ausgeführt: stdio mit absoluten lokalen
dotnet-/DLL-Pfaden, explizitem More-HTTP-API-Backend und localhost:8080. Der Eintrag ist aktiviert;
`codex mcp get timberborn --json` bestätigt die Konfiguration. Maschinenlokale Konfiguration bleibt außerhalb Git.
Vorhandenen LiveSmokeTests erneut separat ausgeführt: ein Test bestanden, alle fünf lesenden MCP-Werkzeuge
gegen die laufende Kolonie erfolgreich. Das prüft den Server über stdio; die dynamische Werkzeugaufnahme
im bereits laufenden Codex-Chat wurde nicht nachgewiesen.

## 2026-09-19 — Ersten Schreib-POC geplant

Auf Nutzer-Go Quellenprüfung und Missionsplan Abschnitt 6 ergänzt: genau ein Gebäude pausieren und
seinen ursprünglichen Pausenstatus wiederherstellen. Aktueller Herstellerquellcode nennt Mod 11.0.0
und eine Route mit explizitem booleschem Zielzustand. Geplant sind getrennte Schreibfähigkeit,
standardmäßig deaktiviertes Opt-in, frische Vor-/Nachprüfung und keine blinden Wiederholungen bei unklarem Ausgang.
Kein Schreibcode implementiert, keine Spielaktion ausgeführt und Client-Schreibfähigkeit nicht aktiviert.
Nächste Freigabe betrifft Implementierung E/F; konkreter Live-Pilot G wird danach abgestimmt.

## 2026-09-19 — Schreib-POC E/F implementiert

Nutzer hat die Umsetzung und anschließend den Test an seiner einzigen Holzfällerflagge freigegeben.
Name ist nicht änderbar; typbasierte eindeutige Auswahl vorgesehen und lesend bestätigt.
MCP-Werkzeug set_building_paused hinter explizitem Prozess-Opt-in implementiert. Vorprüfung,
Serialisierung und Nachprüfung; unklare Schreibausgänge werden nicht wiederholt oder automatisch rückgängig gemacht.
74 reguläre Tests bestanden; Build ohne Warnungen/Fehler. Beide Live-Tests im Standardlauf übersprungen.
Hersteller-HTTP-Helper für Mod 11.0.0 bestätigt HTTP 204. Lokale Codex-Konfiguration bleibt zunächst lesend.

## 2026-09-19 — Technischer Schreibpilot bestanden

LivePauseTests mit explizitem Opt-in nach erneutem Go erfolgreich: einzige Holzfällerflagge anhand
des Templates identifiziert, false -> true -> false jeweils über MCP bestätigt, abschließend erneut gelesen.
Ein Live-Test bestanden; keine automatische Wiederholung und kein anderer Gebäudetyp angesprochen.
Ursprünglicher aktiver Pausenstatus wiederhergestellt. UI-Abgleich angefragt, noch nicht bestätigt.
Kein dauerhaftes Schreib-Opt-in im Codex-Client gesetzt. Code-Checkpoint vor dem Pilot: fafa4d9.

## 2026-09-19 — Sichtbare Nutzerabnahme bestanden

Nutzer bestätigte den aktiven Zustand nach dem Hin-/Rücktest. Auf gesonderten Auftrag anschließend
die einzige Holzfällerflagge über MCP pausiert und Paused=true separat nachgelesen; bewusst nicht zurückgesetzt.
Nutzer bestätigt den sichtbaren Erfolg im Spiel. Begrenzter Schreib-POC damit abgenommen.
Letzter bestätigter Zustand absichtlich pausiert; Wiederaktivierung benötigt einen entsprechenden Auftrag.

## 2026-09-19 — Phase 2 geplant

Nutzer wählt Aufbau der Grundversorgung und ergänzt Wohnraum als Pflichtumfang. docs/phase-2-plan.md
trennt benötigte Beobachtungen/Aktionen von bereits verfügbaren, quellbelegten und unbelegten Fähigkeiten.
Herstellerhandler und ModdableTimberborn-Dokumentation geprüft: Bestands-/Bettenstatistiken sind als
Mod-interne Ansätze dokumentiert; räumliche Daten, Bauprüfung und reguläre Bauaufträge noch nicht nachgewiesen.
Empfohlen sind begrenzter Schnittstellenpilot 2A, Lagebild 2B, einzelner Wohnbau mit Weg 2C,
Versorgungsketten 2D und begrenzter Agentenlauf 2E. Nur Planung, kein Spielzugriff oder neuer Code.

## 2026-09-19 — Phase-2A-Schnittstellenpilot

Freigegebenen Pilot mit sechs lesenden HTTP-Abfragen ausgeführt. Acht Wohnbauvorlagen gefunden;
Lodge.Folktails liefert Kosten, Kapazität und Geometrie. Gebäude enthalten auch Wege, aber keine Positionen.
Workplace.Value ist nach Herstellerimplementierung Sollbesetzung, keine Ist-Besetzung.
GameStatService bietet dokumentierte Mod-interne Güter-/Betten-/Arbeitskräftestatistiken; HTTP-Brücke fehlt.
Areas unterstützt Charaktere; Gebäude-Tracking ist laut Dokumentation unvollständig, kein Ersatz für eine Karte.
Am Raum-/Bauprüfungsblocker begrenzt gestoppt. docs/phase-2a-results.md enthält Abdeckung, Grenzen und
konkreten nächsten Freigabevorschlag. Keine neuen Mutationen, Rohdaten gespeichert oder Mods installiert.

## 2026-09-19 — Eigene Spielschnittstelle als Ziel

Nach Nutzer-Go offizielle Modding-Beispiele und lokale DLL-Metadaten geprüft. Direkte öffentliche Zugänge
für Güter, Betten, Personal, Positionen, Terrain/Wasser und Bauvalidator gefunden. Öffentliche Interfaces
vermeiden den Zugriff auf interne TerrainService-/WaterService-Implementierungen. Keine Methodenkörper
dekompiliert, kein Spielcode ausgeführt. Temporärer Metadatenprüfer ausschließlich unter .local.
docs/architecture/native-game-api.md ersetzt die zuvor bevorzugte ModdableTimberborn-Pflichtbasis;
bestehender MCP-Server/Tests bleiben, Community-Mods dienen zunächst als Referenz/Vergleich.
Nebenwirkungsfreie Vorschauprüfung und reguläre Platzierung noch nicht zur Laufzeit belegt.

## 2026-09-20 — Native Diagnosebrücke und gesicherte Referenzen

Eigene Mod mit Game-Kontext und öffentlichen Spielservices implementiert; netstandard2.1,
keine Fremdmod- oder NuGet-Abhängigkeit. Explizites natives MCP-Backend mit drei Lesewerkzeugen.
Loopback/Bearer-Transport, feste Routen, begrenzte Hauptthread-Queue, Session-ID und Unload-Abbruch.
Mod-Build erfolgreich; 89 reguläre Tests bestanden, zwei Live-Tests übersprungen. Stdio-Integration
prüft unseren echten Transport mit synthetischen Beobachtungen, keine Behauptung eines Spiel-Livetests.
Fehlerfalltests fanden einen fehlenden InvalidDataException-Catch; korrigiert und erneut geprüft.
Benutzer installiert die Mod manuell; kein Spiel-/Client-Setup geändert, keine Mutation ausgeführt.

Nutzer bestätigt Endziel aktives Spielen und wünscht Unabhängigkeit von Fremdmods bei erhaltener
Referenzbasis. 17 benötigte Dateien aus offizieller Mechanistry-Werkzeugbasis und datvm-Quellen
an feste Commits gebunden lokal abgerufen (40.818 Bytes), Lizenzen mitgesichert, SHA256-Inventar erstellt.
Extrahierte API-Erkenntnisse und good-reference-Katalog bleiben im Projekt; fremde Quellen und
Binärdateien werden nicht mitgebaut oder veröffentlicht. Spielplattform und externes MCP-SDK bleiben.
Nächster Nachweis: Live-Lagebild, danach Vorschauvalidierung und regulärer Wohnbau/Wege.

## 2026-09-20 — Lokale Installation auf ausdrücklichen Auftrag

Nutzer hat das Kopieren diesmal ausdrücklich delegiert. Agent Bridge 0.2.0 in den
Standard-Modordner installiert, sechs Dateien einschließlich privater Konfiguration
per SHA256 mit dem gebauten Paket abgeglichen. Kein vorhandener Modordner überschrieben.
Spielstart und Aktivierung übernimmt der Nutzer; Live-Abnahme weiterhin offen.

