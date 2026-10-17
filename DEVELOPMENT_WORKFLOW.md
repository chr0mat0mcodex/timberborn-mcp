# Entwicklungsablauf

## Ziel

Die teure Test-, Build-, Paketierungs- und Installationskette wird pro Feature
einmal vom Menschen ausgelöst. Der Agent entwickelt und bereitet den gezielten
Livetest vor; er führt keine Vollkette und keine Mod-Installation selbst aus.

## Ablauf pro Feature

Bauentwicklung berücksichtigt die Nachweisziele des historischen missionsplan.md
(A–E abgeschlossen im begrenzten Umfang); neue Pakete stehen in BACKLOG.md. Bericht/Diagnose erweitert keine
Baufreigabe. Vor dem Gate aktive Änderungen von zurückgestellten Entwürfen trennen;
ungeprüfte Erweiterungen nicht beiläufig mitdeployen. Für MCP-only-Auswertungen
kann die Spiel-Bridge unverändert bleiben; trotzdem menschliches Test-Gate und
gezielter MCP-Livetest. Erfolgsnachweis nennt fachliche Aussage und deren Grenze,
nicht nur Werkzeugaufruf oder erfolgreiche Paketierung.

1. **Agent: Umsetzung und Quellprüfung.** Klar abgegrenzte Änderung implementieren,
   relevante Tests ergänzen, Quellcode und Diff gezielt prüfen. Zu jeder neuen
   Version die [Anleitung für den spielenden Agenten](docs/agent-playing-guide.md)
   und die projektbezogenen Spielskills fachlich mitführen: neue Fähigkeiten
   ergänzen, geänderte Abläufe/Parameter/Grenzen/Fehlerbehandlung überarbeiten und
   widersprüchliche Alttexte ersetzen. Noch ungeprüfte Fähigkeiten als solche
   kennzeichnen. Geprüfte Dateien und Änderungen beziehungsweise begründeten
   Nichtänderungsbedarf knapp im Projektstand vermerken. Keine eigenen
   Build-/Testläufe; keine alternative Debug- oder Parallelausgabe.
2. **Agent: Testübergabe.** Erwarteten fachlichen Nachweis, bekannte Grenzen und
   den Aufruf des menschlichen Skripts nennen. Vorher
   `timberborn_server_control(action=stop)` oder `scripts/stop-codex-mcp.ps1`
   ausführen: Backendende und Gatesperre bestätigen. Der Vorschaltprozess bleibt
   verbunden. Kein Spiel-MCP-Aufruf mehr bis `live bereit`. Noch nicht committen.
3. **Mensch: Test und Deploy.**
   `scripts/prepare-human-live-test.ps1 -TimberbornManagedDir <Managed-Verzeichnis>`
   ausführen. Das Skript beendet zuerst `Timberborn.exe` hart ohne Speichern.
   Danach führt es die automatischen Tests aus, baut und paketiert die
   Bridge, sichert die bisherige Installation, erhält `bridge.local.json`,
   installiert die neue Mod und verifiziert die Paketdateien.
4. **Mensch: Spiel bereitstellen.** Timberborn starten, einen beliebigen
   passenden Spielstand laden und dem Agenten `live bereit` melden.
5. **Agent: Wiederanschluss und Abschluss-Livetest.** Nach `live bereit` mit
   `timberborn_server_control(action=start)` das neue Backend starten; zuerst
   timberborn_status und aktuelle Session bestätigen. Der Vorschaltprozess wiederholt
   nur den Protokoll-Handshake, niemals unterbrochene Spielaufträge. Kein separates
   Start-Process/dotnet als Ersatz für einen stdio-Wiederanschluss.
   Genau den angekündigten MCP-Nachweis ausführen,
   Ergebnis und relevante Grenze berichten. Bei Fehlschlag gezielt diagnostizieren;
   keine automatische Vollketten-Wiederholung.
6. **Agent: Abschluss.** Nur wenn Skript und feature-spezifischer Livetest
   erfolgreich waren: Anleitung und Spielskills mit tatsächlichen Livebelegen und
   verbleibenden Grenzen abgleichen, Dokumentation knapp aktualisieren, committen und zu `origin`
   pushen.

## Skriptoptionen

Das Gate speichert Phasendauern lokal in `.local/gate-timings.json` (nicht Git).
Jede der sechs Phasen verwendet beim nächsten Lauf ihre letzte erfolgreiche
Dauer als Zeitreferenz. Der Livebalken ist eine Schätzung, bis zum Erfolg auf
95 Prozent begrenzt; bei Überschreitung bleibt die Phase sichtbar aktiv.
Ohne Referenz wird die Laufzeit gemessen, keine Prozentprognose erfunden.
Footer und Abschlusszeile zeigen erfolgreiche Phasen mit vollem Balken.
Auch bei Abbruch werden die tatsächlichen Phasenzustände/Dauern gespeichert;
fehlgeschlagene und offene Phasen überschreiben keine erfolgreiche Referenz.
Bei umgeleiteter Ausgabe bleibt die Textanzeige ohne Livebalken erhalten.
Fehler beim Laden/Speichern der Anzeigehistorie ändern das Gate-Ergebnis nicht.

Einmalige Codex-Einrichtung, Lifecycle-Werkzeug und Abnahme:
[Persistenter MCP-Vorschaltprozess](docs/codex-mcp-supervisor.md).
Das Gate prüft dessen synthetische Python-Tests ebenfalls ohne Wiederholung.
Bei Erfolg zeigt es nur Testanzahl und Schrittdauer; Warnungen und Hinweise auf
übersprungene Tests bleiben sichtbar. Bei Fehlern erscheint die vollständige
Python-Testdiagnose vor dem Abbruch.
Python 3.8 oder neuer muss als `python` erreichbar sein; keine Zusatzpakete.

`-ModsRoot <Pfad>` setzt bei Bedarf einen abweichenden Timberborn-Mod-Ordner.
`-Port <Port>` erzeugt für eine Erstinstallation eine passende lokale
Bridge-Konfiguration. Bestehende `bridge.local.json` wird unverändert übernommen
und niemals ausgegeben oder versioniert.

## Grenzen

- Das Skript wird vom Menschen lokal gestartet, weil es die Mod-Installation
  verändert. Es beendet laufende `Timberborn.exe`-Prozesse ohne Speichern und
  bricht vor den Tests ab, wenn deren Ende nicht bestätigt werden kann.
- Build, automatische Tests, Paketierung und Installation erfolgen ausschließlich
  im vom menschlichen Entwickler gestarteten Gate. Ein Release-Build als Grundlage
  für Prüfung und Livetest; keine Debug-/Parallelfassung als Ausweichweg.
- Der projektgenaue MCP-Stopp unmittelbar vor dem Gate ist ausdrücklich beauftragt.
  Keine allgemeinen dotnet-/Codex-Abschüsse und keine anderen Ausgabeverzeichnisse.
  Das menschliche Skript behält seinen eigenen Prozessstopp als zusätzliche Sicherung.
- Kein Feature-Commit/Push ohne bestätigten erfolgreichen Skriptlauf und
  erfolgreichen Abschluss-Livetest. Ausnahme für reine Dokumentationspflege siehe unten.

## Vorprüfung vor der Übergabe

- Betroffene Verträge, Referenzen, Nullability-Fluss und Diffs gezielt lesen;
  passende Regressionstests vorbereiten. Syntaxprüfung ersetzt keinen Build.
- Bei neuen MCP-Werkzeugen Namen, Aktionsschalter, Read-only-Annotationen und
  Vertragsprüfungen zusammen aktualisieren.
- Nach einem Gate-Fehler die konkrete Ursache im Quellcode korrigieren und den
  erwarteten Nachweis nennen. Der Entwickler startet das Gate erneut; keine
  automatische Wiederholung oder zusätzliche Buildvariante durch den Agenten.
- Ausgeführte Prüfungen und vorbereitete Tests getrennt benennen. Änderungen sind
  bis zum erfolgreichen Gate und fachlichen Livetest nicht abgenommen.

## Reine Dokumentations- und Regelpflege

Ausdrücklich freigegeben am 2026-10-12: Änderungen ausschließlich an Dokumentation
oder freigegebenen Agentenregeln dürfen nach Inhalts-, Diff- und Linkprüfung
separat committed und gepusht werden. Kein Build, MCP-Stopp, Deploy oder Livetest
nur für diese Pflege. Änderungen an Code, Tests, Build, Konfiguration oder
Laufzeitverhalten fallen nicht unter diese Ausnahme. Experimentelle oder noch
nicht abgenommene Implementierungen nicht im Dokumentations-Commit mitnehmen.
Freigabepflichten für dauerhafte Regeländerungen bleiben bestehen.

## Git-Sicherung

GitHub-Ziel: https://github.com/chr0mat0mcodex/timberborn-mcp

Vor jedem zulässigen Commit die Dateiliste und den Diff auf unbeabsichtigte
Änderungen, Geheimnisse sowie private oder generierte Daten prüfen. Nur ausgewählte
Projektdateien stagen. Commit-Identität pro Befehl setzen; keine globale Identität
konfigurieren. Vor dem Push `origin` prüfen, keine fremden Commits überschreiben und
keinen Force-Push verwenden. Erfolgreichen Push gegen lokalen HEAD abgleichen.
