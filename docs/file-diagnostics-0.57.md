# Dateilogs und vereinfachte Ingame-Anzeige — 0.57.0

Status 2026-10-16: menschliches Gate und begrenzter Abschluss-Livetest bestanden.
Bridge 0.57.0 und aktuelle Sitzung bestätigt. Leseaufruf und synthetisches
`run_not_found` in beiden Dateilogs mit Begründung, Status und übereinstimmender
Activity-Kennung nachgewiesen; strukturierte Laufkorrelation erhalten,
`failedWrites=0`. Dateinamen enthalten Kolonie-/Spielstandname und UTC-Zeitstempel.
Mensch bestätigt Anzeige ohne Korrelationskennungen sowie automatischen Spielstart.
Erneutes Laden desselben Saves erzeugt eine neue Session und neue Dateien in beiden
Komponenten. Alte MCP-Datei unverändert; alte Bridge-Datei nur um `bridge_unloading`
ergänzt, bisheriger Inhalt per Präfix-Hash unverändert bestätigt.
Keine Größen-/Rotationseingriffe im Livetest; diese Grenzen sind synthetische
Gate-Nachweise. Keine Vollständigkeitsgarantie für Unity-/Framework-Ereignisse.
## Dateien und Lebensdauer

- Bridge: `%LOCALAPPDATA%\TimberbornMcp\Logs\bridge`.
- MCP-Server: `%LOCALAPPDATA%\TimberbornMcp\Logs\mcp`.
- UTF-8, pro Zeile ein JSON-Objekt, direkt angehängt und nach jedem Eintrag geleert.
- Dateiname: `bridge_<Kolonie>--<Spielstand>_<UTC-Zeitstempel>_<Eindeutigkeit>.log`
  beziehungsweise `mcp_...`. Windows-ungültige Zeichen werden ersetzt; der Namensteil
  ist auf 80 Zeichen begrenzt. Zeitstempel enthält Datum, Uhrzeit, Sekundenbruchteile und Z.
- Jede neu geladene Spielsitzung erzeugt beim Bridge-Start eine eigene Datei,
  auch beim erneuten Laden desselben Saves. Keine Rotation, Größenbegrenzung,
  automatische Löschung oder Überschreibung alter Dateien.
- Der Name stammt aus der öffentlichen Schnittstelle
  `GameLoader.LoadedSave.SettlementReference.SettlementName` und `SaveName`.
  Neues Spiel ohne geladenen Save: `neues-spiel`. Späteres Speichern benennt die
  laufende Sitzungsdatei nicht um.
- Der MCP-Server erfährt den Namen über die Activity-Quittung und eröffnet seine
  Sitzungsdatei beim ersten bestätigten Kontakt. Ohne Kontakt entsteht keine
  erfundene Save-Datei. Prozessstart, Ereignisse vor der Sessionzuordnung und
  Prozessende stehen in einer zusätzlichen `mcp_prozess_...log`.
  Serverneustart erzeugt neue Dateien; ältere Bridges ohne Namensfeld ergeben
  `unbekannter-spielstand`.

## Inhalt und Grenzen

Bridge protokolliert Laden/Bereitschaft/Entladen, Start und Rückkehr jeder an den
Spielthread zugestellten Anfrage, Fehlercodes sowie empfangene Aktivitätsmeldungen.
MCP protokolliert Prozessereignisse, frühe Parameterablehnungen, native Werkzeugstarts,
Bestätigung der Starttelemetrie, Abschlussstatus/Fehlercode und Telemetrieausfälle.
`activity_started` bedeutet bestätigte Starttelemetrie, keinen ausgeführten Spielauftrag.
`returned` bedeutet zurückgegebene Bridge-Antwort, keinen Wirkungsnachweis.

Enthalten sind Zeitpunkt, Komponente, Prozessinstanz, Sequenz, Aufruf-/Sessionkennung,
Projekt-/Laufkorrelation, die vom Aufrufer angegebene Begründung und vorhandene
Parameterzusammenfassung. Die bestehende Zusammenfassung ist eine Auswahl der
Parameter, kein vollständiger Requestdump. Begründungen bleiben maximal 600,
Zusammenfassungen maximal 400 Zeichen entsprechend dem bestehenden Fachvertrag;
die Dateigröße ist unbeschränkt. Bekannte Bearer-/64-Hex-Muster werden redigiert.
Keine Auth-Header, privaten Konfigurationsinhalte, Rohantworten, Screenshots oder
rohen Exceptionmeldungen. Keine privaten Gedankengänge.

Kein vollständiges Unity-/stderr-Abbild: Fehler vor dem Bridge-Dispatcher,
Framework-Meldungen und autonome Änderungen zwischen Abfragen werden nicht sämtlich
erfasst. Unity Player.log und stderr bleiben zusätzliche Diagnosequellen.
Das Log dokumentiert Aufrufe, nicht sämtliche Spielereignisse.

Schreibfehler werden ohne Aktionswiederholung abgefangen. Eine Warnung pro Logger
geht an stderr beziehungsweise Unity; `failedWrites` und Sequenz zeigen Verluste
in später wieder erfolgreichen Einträgen. Bei dauerhaftem Schreibausfall oder
Prozessabbruch kann kein letzter Dateieintrag garantiert werden. Beide Komponenten
können über die Activity-Aufrufkennung abgeglichen werden. Die Session im
MCP-Abschluss ist die durch die Starttelemetrie bestätigte Session.

Die in 0.56 eingeführten Projekt-/Laufkennungszeilen entfallen im Ingame-Fenster.
Strukturierte Korrelation und Filter bleiben über MCP und in den Dateien verfügbar.
Dateien sind lokale Diagnosedaten und werden nicht ins Repository aufgenommen.

## Vorbereitete Nachweise

Das menschliche Vorbereitungsskript startet nach allen erfolgreichen Prüfungen
automatisch `Timberborn.exe` aus dem Spielverzeichnis oberhalb von
`Timberborn_Data/Managed`. Die EXE wird vor der Buildkette auf Existenz geprüft.
Bei Fehlern erfolgt kein Spielstart; scheitert nur der Start, bleibt die bereits
geprüfte Installation erhalten. Spielstand laden und `live bereit` bleiben manuell.

Automatische Tests im menschlichen Gate: sichere Dateinamen, Sessionwechsel bei
gleichem Namen, Prozessneustart ohne Überschreibung, Anhängen jenseits der früheren
Größengrenze, parallele Einträge, gesperrte/unbeschreibbare Datei, Ausfallwarnung,
Tokenredaktion, Namensübernahme aus der Bridge und keine doppelte Spielaktion bei
Logfehlern. Versionsverträge erkennen 0.57.0; fehlende Paginierungsdaten bleiben Fehler.

Livetest nach `live bereit`: Version und Session bestätigen; Leseaufruf und gezielte
synthetische Ablehnung auslösen, JSON-Zeilen/Begründung/Abschluss/Korrelation in beiden
Dateien vergleichen. Mensch bestätigt die Ingame-Anzeige ohne Korrelationskennungen.
Anschließend denselben Save erneut laden: neue Session und neue Bridge-Datei mit
korrektem Namen; nach Kontakt neue MCP-Datei, alte Dateien unverändert vorhanden.
Beim ersten fehlgeschlagenen Kontrollfall diagnostizieren, keine automatische
Wiederholung der Build-/Installationskette. Erst nach diesen Nachweisen committen/pushen.
