# Projektjournal

## 2026-09-20 — Baustellen-/Distriktbeobachtung 0.6.0 vorbereitet

Nach Nutzer-Go als nächsten kleinen Schritt die Ergebnisbeobachtung vor Hausbau
erweitert: inspect_building(id, session), sieben native Lesewerkzeuge. Feste GET-Route,
Session-Prüfung im Spielhauptthread, nur initialisierte Gebäude/Paths ohne Previews.
Fertigstatus, verbleibende Güter/Baufortschritt für unfertige ConstructionSites und
getrennte DistrictBuilding-Zuordnungen. Keine Gleichsetzung mit Arbeiter-/Liefergarantie.
Keine neue Abhängigkeit und keine Änderung am Wegbau-Pilot oder laufenden Spiel.

Öffentliche Signaturen gegen lokale Spielbibliotheken geprüft. 132 reguläre Tests
bestanden (121 Unit, 11 Integration), drei Live-Tests übersprungen; Mod-Build erfolgreich.
0.6.0 noch nicht installiert/live geprüft; laufendes Spiel bleibt 0.5.0 mit Testweg.
Nächste Abnahme rein lesend gemäß docs/building-observations.md. Materialfortschritt
benötigt später eine echte Baustelle; fehlt sie, bleibt dieser Teil ausdrücklich offen.

## 2026-09-20 — Erster regulärer Wegbau über natives MCP bestätigt

0.5.0 live erreichbar. Neun fachliche MCP-Aufrufe: Zustand/Katalog/Karte/Wege gelesen,
freien trockenen Einzelfeldstandort neben bestehendem fertigem Path vorgeprüft und
genau einen place_path-Aufruf ausgeführt. Ergebnis applied; folgende unabhängige
Gebäudeabfrage bestätigt exakt die zurückgegebene Entity-ID, Path und Zielposition,
finished=true. Genau ein neues Struktur-Objekt, Gesamtobjektzahl +1, drei beobachtete
Ressourcenwerte unverändert. Kein zweiter Auftrag, keine Löschung oder explizite Speicherung.
Testweg bleibt bestehen; Einmaligkeitsgate dieser Sitzung verbraucht. Private Versuchsdaten
und Receipt nur im ignorierten lokalen Pilotordner, keine Entity-/Session-ID eingecheckt.
Damit regulären Platzierer und SetId-Ergebniszuordnung für diesen Path live nachgewiesen.
Keine Aussage zur Distriktanbindung oder zu Häusern mit Material-/Bauzeitbedarf.

## 2026-09-20 — 0.5.0 für Wegbau-Pilot installiert

Nach Nutzerbestätigung bei beendetem Timberborn das finale vorbereitete 0.5.0-Paket
installiert. Fünf Dateien SHA256-geprüft, vorherige Installation samt privater Konfiguration
in neuem lokalen Backup gesichert. Schlüssel und Port erhalten; enablePlacement für den
beauftragten Einzelweg-Pilot aktiviert. Zusätzliches MCP-Opt-in weiterhin erforderlich;
Standardstarter bleibt lesend. Kein Spielstart und kein Bauaufruf ausgeführt.
Nächster Schritt nach Laden von MCP: Version/Sitzung und Standort frisch prüfen,
genau einen Wegauftrag ausführen und das Ergebnis lesend anhand der Entity-ID bestätigen.

## 2026-09-20 — Einzelner regulärer Wegauftrag 0.5.0 vorbereitet

Nach ausdrücklichem Go für kontrollierte Bauaufträge zunächst Path-only umgesetzt.
Keine neue Abhängigkeit. Öffentliche Metadaten belegen TemplateSpec.Blueprint,
EntitySetup.Builder.SetId und IBlockObjectPlacer.Place. Feste POST-Route und separates
Mod-/MCP-Opt-in; Hauptthread prüft Session vor Einmaligkeitsgate und frischer Validierung.
Ein Versuch je Szene, auch bei Ablehnung/Fehler. Platzierung regulär über passenden
Spielplatzierer; Erfolg nur bei exakter Entity-ID/Vorlage/Position/Orientierung,
Fertigstatus separat. Unklare Ergebnisse niemals automatisch wiederholen.

123 reguläre Tests bestanden (112 Unit, 11 Integration), drei Live-Tests im Standardlauf
übersprungen. Integration prüft echte stdio-/HTTP-Kette mit synthetischem Platzierer,
alle vier Opt-in-Kombinationen und Zweitversuchsperre. Mod-Build ohne Warnungen/Fehler,
neues lokales 0.5.0-Paket erstellt. Nicht installiert, kein Bauauftrag im Spiel ausgeführt.
Live bleibt 0.4.1. Nächste Nutzerhilfe: Spiel beenden für Update; danach genau ein
Wegauftrag und lesende Ergebniszuordnung gemäß docs/path-placement.md.

## 2026-09-20 — 0.4.1 Vorschau-Kontrollpilot bestanden

Version 0.4.1 über MCP bestätigt. Neun fachliche MCP-Aufrufe in einer Sitzung,
darunter genau zwei Vorschauvalidierungen: Lodge auf belegtem District-Center-Standort
korrekt valid=false; vollständig vorgeprüfter einfeldriger Path-Standort valid=true.
Beide internen Entity-/Bestandswachen unverändert, keine Sitzungssperre. Abschließende
Kolonieabfrage: Objektzahl und drei Ressourcenbeobachtungen gegenüber Beginn identisch.
Keine Platzierung oder Produktionssteuerung. Der Nachweis gilt für diese zwei Fälle;
allgemeine Bebaubarkeit, Distriktanbindung und Fertigstellung sind dadurch nicht bewiesen.
Nächste technische Lücke: regulären Bauauftrag mit stabiler Ergebniszuordnung ermöglichen
und anschließend Auftrag, Materialversorgung und fertiges Bauwerk getrennt beobachten.

## 2026-09-20 — Korrekturkandidat 0.4.1 installiert

Auf Nutzer-Go bei beendetem Timberborn das vorbereitete Paket installiert. Fünf
Paketdateien per SHA256 geprüft, bisherige Dateien samt privater Konfiguration in
neuem lokalen Backup gesichert. Private Konfiguration bytegleich erhalten, einschließlich
Schlüssel, Port und bestehendem Vorschau-Opt-in. Kein Spielstart oder Live-Aufruf.
Nächster Schritt nach Nutzer-Neustart: Version prüfen und belegten Standort erneut als
Negativkontrolle verwenden. Bei erneutem Widerspruch sofort stoppen; keine Platzierung.

## 2026-09-20 — Validator-Negativkontrolle auf 0.4.0 fehlgeschlagen

Version 0.4.0 über MCP bestätigt. Kleinen Kartenbereich, vollständige Gebäudeseite und
Bauplatzvorprüfungen gelesen; freier einfeldriger Path-Kandidat vor dem Schreibpilot
identifiziert. Erster Vorschauversuch: Lodge auf District Center fälschlich valid=true.
Sofortiger Abbruch, zweiter Versuch unterblieb. Entity-/Bestandswache meldete keine
Änderung. Anschließend separater nativer Lesetest über alle sechs Werkzeuge bestanden.
Keine Platzierung; keine weiteren Vorschauaufrufe in dieser Sitzung.

Referenzmuster und öffentliche Metadaten belegen BlockObject.IsValid(). Korrekturkandidat
0.4.1 verlangt diese Prüfung zusätzlich zum einzelnen Service-IsValid-Aufruf.
Hypothese: bisheriger Service-Aufruf deckt die direkte Objektprüfung nicht vollständig ab;
Ursache noch nicht bestätigt. Installation/erneuter begrenzter Kontrolltest ausstehend.
109 reguläre Tests bestanden, drei Live-Tests im Standardlauf übersprungen; Mod-Build
ohne Warnungen/Fehler und neues unveränderliches 0.4.1-Paket erstellt. Diese Prüfungen
belegen die Live-Semantik der direkten Objektprüfung noch nicht.

## 2026-09-20 — Bridge 0.4.0 installiert, Neustart ausstehend

Auf Nutzerauftrag bei beendetem Spiel das vorbereitete 0.4.0-Paket installiert.
Fünf Paketdateien mit SHA256 gegen die Quelle geprüft; vorherige Dateien und private
Konfiguration in einem neuen ignorierten lokalen Backup gesichert. Bestehenden Port
und Schlüssel erhalten, enableValidation für den angekündigten Pilot aktiviert.
MCP-Opt-in bleibt separat erforderlich; Standardstarter aktiviert es nicht.
Noch kein Laufzeitnachweis für 0.4.0. Nächster Schritt nach Nutzer-Spielstart und Laden
der Testkolonie: Version/Lesefunktionen prüfen, dann maximal zwei Vorschauvalidierungen
nach docs/native-validation.md. Keine Bauaufträge.

## 2026-09-20 — Native Bridge ohne aktive Fremdmods live geprüft

Nach Nutzerbestätigung des Spielstarts nur mit MCP-Mod den vorhandenen nativen
Lesetest gezielt wiederholt: ein Test bestanden, alle sechs MCP-Werkzeuge erfolgreich
in einer Sitzung. Schreib-/Vorschau-Opt-ins deaktiviert; keine Spielmutation.
Keine erneute vollständige Testsuite, da seit dem geprüften Checkpoint kein Code geändert.
Der unabhängige lesende Betrieb ist für die Testkolonie nachgewiesen; Mod-Auswahl beruht
auf Nutzerbestätigung. Installierter Stand bleibt 0.3.0. Nächster Schritt: vorbereiteten
0.4.0-Validator installieren und begrenzt live prüfen; DLL-Austausch erst bei beendetem Spiel.

## 2026-09-20 — Lokalen nativen Startweg konsolidiert

Nutzer beauftragt schrittweise eine lokal nutzbare Mod mit MCP und möglichst wenigen
Fremdmod-Abhängigkeiten. Bestehende Architektur beibehalten; keine neue Abhängigkeit.
Eigener stdio-Starter wählt native explizit und deaktiviert Vorschau-/Schreibaktionen.
verify.ps1 unterstützt nun -NativeConfig: reguläre Prüfungen und anschließend genau
einen begrenzten nativen Lesetest. Auch der Live-Test selbst unterbindet geerbtes
Vorschau-Opt-in. 109 reguläre Tests und ein nativer Live-Test über sechs Werkzeuge
bestanden; Starter beendet sich bei stdin-EOF mit Exit 0 ohne stdout-Rauschen.

Installiert bleibt 0.3.0, Quellstand 0.4.0; keine Mod-Dateien oder Client-Einstellungen
ersetzt. Aktuelle CLI findet keine Registrierung namens timberborn; die historische
Einrichtungsnotiz ist kein Nachweis der heutigen Client-Verfügbarkeit.
Nächster Nachweis: Spiel mit ausschließlich eigener Bridge starten, Testkolonie laden,
nativen Lesetest wiederholen. Dafür ist Nutzerhilfe beim Mod-Menü/Neustart nötig.
Bislang waren Fremdmods parallel aktiv; fehlende RequiredMods und erfolgreiche native
Abfragen belegen noch keine isolierte Laufzeit. Danach geschützten 0.4-Validator prüfen,
bevor reguläre Bauaufträge folgen. Kein autonomer Kolonieaufbau erfolgt.

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

## 2026-09-20 — Erster Startversuch

Spiel lädt die Agent Bridge, aber ihr Listener startet nicht; Port 8081 verweigert
die Verbindung. Startdiagnose um feste Phasenkennung und Exception-Typ ergänzt,
ohne Fehlermeldungsinhalt, Pfade oder Schlüssel auszugeben. Mod-Build erfolgreich.
Live-Abnahme weiterhin offen; erneuter Spielstart für die Diagnose erforderlich.

## 2026-09-20 — Konfigurationspfad beim Spielstart korrigiert

Diagnose meldet ArgumentException in locate_configuration, vor Listener-Erstellung.
Assembly.Location ist für vom Spiel geladene Mod-Assemblies kein zuverlässiger Dateipfad.
Konfiguration jetzt über öffentlichen ModRepository, eindeutige aktivierte Manifest-ID und
ModDirectory.Path auflösen. Öffentliche Signaturen lokal geprüft; keine zusätzliche Mod nötig.
Mod-Build ohne Warnungen/Fehler, 89 reguläre Tests bestanden. Korrigierte DLL mit Sicherung
der vorherigen Version installiert und per SHA256 geprüft; privater Schlüssel unverändert.
Ursprüngliches Paket bleibt historischer Snapshot und enthält diesen Fix nicht; neue Pakete
aus aktuellem Code bauen. Nächster Spielneustart muss den Fix zur Laufzeit bestätigen.

## 2026-09-20 — Native Bridge live erreichbar und MCP-Lesetest bestanden

Nach Spielneustart Listener erfolgreich gestartet. Bevölkerung 9 Erwachsene/4 Kinder/0 Bots,
0 Betten/13 Obdachlose und verfügbare Bestände 250 Wasser/300 Beeren/0 Holz vom Nutzer im UI bestätigt.
Echter stdio-Livetest aller drei nativen MCP-Werkzeuge erfolgreich, Sitzung über die Aufrufe konsistent,
eine Kartenzelle an beobachteter Objektposition gelesen. Kein Rückgriff auf More HTTP API.
89 reguläre Tests bestanden; drei Live-Tests im Standardlauf übersprungen, anschließend genau ein
nativer Lesetest separat bestanden. verify.ps1 deaktiviert dessen Opt-in im normalen Prüflauf.
Keine Mutation ausgeführt. Fremdmods waren im Spiel weiterhin aktiv; Isolation ohne diese noch nicht
live getestet. Koordinaten-/Geländesemantik und Session-Wechsel nach Menü/Reload noch offen.
Die dauerhafte Client-Konfiguration wurde nicht umgestellt; Livetest nutzt eigenen nativen Prozess.

## 2026-09-20 — Wiederverbindung nach Menü und Neuladen bestätigt

Nutzer hat Hauptmenü und erneutes Laden der Testkolonie durchgeführt. Spiel-Log bestätigt
Menü-/Spiel-Ladevorgang und erneuten Listener-Start. Snapshot erreichbar, anschließend alle
drei nativen MCP-Werkzeuge im separaten Lesetest erneut erfolgreich (ein Test bestanden).
Kein Spielneustart und keine Mutation erforderlich. Vorherige Session-ID wurde nicht erhalten;
ein tatsächlicher ID-Wechsel ist deshalb nicht direkt verglichen. Aktuelle ID nur lokal unter
.local als Vergleichsbasis gespeichert, keine IDs oder Rohlogs im Repository.
Wiederverbindung abgenommen; räumlicher UI-Abgleich und Bauvalidierung bleiben offen.

## 2026-09-20 — Strukturierte Raumabfragen und reine Bauplatzvorprüfung 0.3.0

Nutzer beauftragt autonomes Weiterarbeiten bis echte Hilfe nötig ist und präzisiert:
keine Screenshots; Spielzustand per API, kontrollierte programmierte Interaktionen.
Räumlichen UI-Abgleich im aktuellen Ablauf durch API-Konsistenzprüfungen ersetzt.
Abhängigkeiten möglichst vermeiden, bei sinnvollem Mehrwert aber vor Aufnahme abwägen und fragen.

Neue native Leserouten/Werkzeuge für Gebäude/Wege, zwei Pilotvorlagen und Bauplatzvorprüfung.
Öffentliche TemplateSpec-/Positions-/Block-/Fraktions-/Freischaltungsservices genutzt;
keine privaten Felder, neue Fremdmods, Preview-Erzeugung, Spawn-/Delete- oder Bauaufrufe.
BuildingBlueprints-Referenz verwendet teils Probeplatzierung; bewusst nicht übernommen.
Vorprüfung liefert nur blocked oder requires_game_validation, gameValidated stets false.
Volle Spielvalidierung, Stützregeln für stapelbare Objekte und Distriktanbindung bleiben offen.

99 reguläre Tests erfolgreich, drei Live-Tests im Standardlauf übersprungen. Mod-Build
gegen Spiel-DLLs ohne Fehler/Warnungen. Synthetischer stdio-/HTTP-Test umfasst alle sechs
Werkzeuge, zusätzlich Parametergrenzen und Abweisung irreführender Baufreigaben geprüft.
Vier Quellen ergänzt: 21 Referenzdateien/61.934 Bytes lokal gesichert, nicht ins Paket übernommen.
0.3.0 als neues Paket gebaut und fünf Dateien mit Backup in delegierter Installation ersetzt,
SHA256 geprüft; vorhandener privater Schlüssel unverändert. Aktuelle Session-ID nur lokal
als Vergleichsbasis gesichert. Neue DLLs noch nicht im laufenden Spiel geladen.
Nächste notwendige Nutzerhilfe: vollständiger Spielneustart und MCP laden; anschließend
begrenzter strukturierter Live-Pilot gemäß docs/spatial-precheck.md ohne weitere Umfangsrückfrage.

## 2026-09-20 — Räumlicher 0.3.0-Pilot live bestätigt

Version 0.3.0 aktiv, Sitzungswechsel gegenüber lokaler Vergleichs-ID direkt bestätigt.
Ein lesender HTTP-Verbindungscheck und zehn fachliche MCP-Abfragen ausgeführt, innerhalb
des Zwölf-Abfragen-Budgets. Alle sechs Werkzeuge genutzt; 21 Gebäude/Wege, 64 Kartenzellen,
aktive Fraktion und zwei freigeschaltete Vorlagen gelesen. Lodge benötigt 12 Holz bei 0 Bestand.
Vier Bauplatzvorprüfungen ergeben erklärbare Objekt-/Terrain-/Kartenrandhindernisse; keine
freie vollständige Grundfläche nachgewiesen. Registrierte Objektzahl und Bestandsbeobachtung
vor/nach identisch. Keine Bauaufträge, Screenshots oder Eingabesimulation ausgeführt.

## 2026-09-20 — Validator-Prototyp vorbereitet, Mission auf Analyse fokussiert

Öffentliche PreviewFactory-/Validator-/Placer-/EntitySetup-Signaturen gezielt geprüft.
0.4.0 mit eigener temporärer Vorschau und Spielvalidator gebaut; gesondertes Mod-/MCP-Opt-in,
POST-only, Session-Prüfung, maximal acht Versuche und Sperre bei Fehler/Zustandsabweichung.
Keine Platzierungsroute. 109 reguläre Tests bestanden, drei Live-Tests übersprungen.
Neues lokales Paket erstellt; ausdrücklich noch nicht installiert oder im Spiel getestet.

Nutzer stellt klar: Vorrang hat die Analyse dessen, was der Agent zum Spielen benötigt;
reales Spielen ist sekundär. Auf ausdrücklichen Auftrag in AGENTS.md und missionsplan.md
gespeichert und die Einstiege in README/Phase-2-Plan angepasst. Die bevorstehende Installation
von 0.4.0 unterbleibt. Spiel nutzt weiterhin 0.3.0, privater Schlüssel und Client-Konfiguration
unverändert. Nächster Schwerpunkt: Fähigkeiten, benötigte Daten/Aktionen und Evidenzlücken
systematisch priorisieren; praktische Tests nur mit konkretem Erkenntniszweck.
