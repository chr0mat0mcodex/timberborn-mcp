# Agentenlog: Cursor, Filter und Aufbewahrungslücken (0.55)

Stand 2026-10-16: nach menschlicher Gate-Freigabe durch `live bereit` begrenzt
live abgenommen; installierte Bridge 0.55.0 bestätigt.

Gate-Rücklauf: Kompilierung durch xUnit2013 bei der Einzelelementprüfung
abgebrochen. Auf `Assert.Single` korrigiert, auch in der neuen Integrationsprobe.
Danach Bereitmeldung des Menschen und gezielter erfolgreicher Livetest.

## Live-Ergebnis am 16. Oktober 2026

- Erste Seite mit Limit 1: Sequenz 2, Folgeseite nach zusätzlichem Statusaufruf:
  Sequenz 1, keine Duplikate, keine weiteren älteren Treffer. Puffer wuchs auf
  vier Aufrufe, die Cursorgrenze blieb unverändert.
- Werkzeug `inspect_agent_log` und Status `ok` gemeinsam: genau ein passender
  abgeschlossener Eintrag (Sequenz 4), weitere passende Einträge vorhanden.
- Synthetischer unbekannter Werkzeugname: leere Seite, kein Folgecursor,
  keine erfundene Aufbewahrungslücke.
- Synthetischer Fremdcursor: `stale_activity_cursor`, nicht wiederholbar,
  `execution=not_applicable` für die Leseabfrage.
- Frischer Puffer: null Verdrängungen, `gapDetected=false`, konsistente
  Puffergrenzen. Kein künstlicher Liveüberlauf; Überlauf und Löschen bleiben
  synthetische Gate-Nachweise. Keine Spielaktionen ausgeführt.
- Clientbeschreibung war noch alt, neue Argumente und strukturierte Ergebnisse
  funktionierten über die bestehende Verbindung. Katalog-Veraltung bleibt B12.

## Vertrag

`inspect_agent_log` behält seine bestehenden Einträge und Pflichtbegründung.
Optional: `limit` 1–32 (Standard 32), `tool` als exakter Werkzeugname, `state`
als einer von running/ok/applied/rejected/unconfirmed/error/cancelled, `cursor`.
Filter werden mit UND verknüpft und vor der Seitengröße angewandt. Keine Treffer
ist ein gültiges Ergebnis. Unbekannte, doppelte oder falsch typisierte Argumente
werden abgewiesen. Höchstens 128 Aufrufe bleiben im Arbeitsspeicher.

Einträge tragen eine monotone Startsequenz `sequence`. Neueste zuerst;
Abschlussmeldungen aktualisieren denselben Eintrag, ohne die Reihenfolge zu ändern.
`hasMore` und `nextCursor` ermöglichen den Abruf aller noch gespeicherten Treffer.
Den Cursor unverändert mit denselben Filtern weitergeben; `limit` darf wechseln.
Neue Aufrufe landen vor der Cursorgrenze und verschieben Folgeseiten nicht.
Eine frische Abfrage ohne Cursor holt den aktuellen Anfang.

Die Seiten sind **kein eingefrorener Snapshot**. Status und Statusfilter gelten
zum jeweiligen Lesezeitpunkt. Bereits überblätterte, später abgeschlossene Aufrufe
werden nicht erneut geliefert. Für vollständiges Blättern keinen Statusfilter
verwenden; laufende Aufrufe bei Bedarf mit einer frischen Abfrage beobachten.
Der eigene Leseaufruf kann als running enthalten sein und verdrängt wie bisher
gegebenenfalls den ältesten Eintrag. Es gibt keinen Änderungsstream.

`paginationVersion=1`, `logEpoch`, `retainedCount`, `oldestSequence` und
`newestSequence` beschreiben den Puffer zum Lesezeitpunkt. Bei leerem Log sind
die Sequenzgrenzen null. `evictedCount` zählt seit Beginn dieser Log-Instanz
verdrängte Aufrufe. `unavailableBeforeCursor` zählt davon diejenigen unterhalb
der angefragten Cursorgrenze; ohne Cursor sind dies alle verdrängten Aufrufe.
`gapDetected` ist genau dann wahr, wenn dieser Wert positiv ist. Das gilt auch
bei einer leeren oder letzten Seite. Die Zahl gilt **vor Filtern**: Ob verlorene
Aufrufe zum Filter gepasst hätten, ist unbekannt. Gefilterte Sequenzsprünge allein
sind keine Aufbewahrungslücke. Auch der erste Abruf kann eine Lücke melden.

Löschen im Ingame-Fenster und Neuladen erzeugen eine neue Log-Instanz. Alte Cursor
werden mit `stale_activity_cursor` abgewiesen; ohne Cursor neu beginnen.
Ein Backend-Neustart allein verwirft den spielseitigen Log nicht.
Fehlgeschlagene Telemetrieübertragung lässt sich damit weiterhin nicht zählen.
`gapDetected=false` verspricht deshalb keine vollständige Aktionshistorie.
Projekt-/Laufkorrelation und Wirkungsnachweise sind nicht Teil dieses Pakets.

Der argumentlose Abruf älterer Bridges bleibt möglich; fehlende Seitendaten
werden nicht erfunden. Neue Optionen benötigen die aktualisierte Bridge.
0.55 muss Seitendaten liefern. Widersprüchliche Reihenfolge, Filter, Grenzen,
Cursor oder Lückenangaben werden vom MCP-Leser abgewiesen.

## Vorbereitete Prüfungen

- Core-/Transporttests: 128 Einträge vollständig, variable Seitengröße,
  neue Aufrufe und Abschlussupdates zwischen Seiten, Verdrängung bis zur leeren
  Folgeseite, Filter vor Limit, Nulltreffer, Filterwechsel, Löschen und fremder Log.
- Argument- und Antwortfehler sowie Altbridge ohne erfundene Seitenmetadaten.
- MCP-Integration: aktualisierte Bridge-Route, öffentliches Schema und Filter.
- Versionsregression: 0.55-Fähigkeitsprofil; Pflanzenjournal verlangt weiterhin
  räumliche Abdeckungsangaben. Unbekannte Version 99.0.0 bleibt unbekannt.

## Menschliches Gate und begrenzter Livetest

Übergabe gemäß [Entwicklungsablauf](../DEVELOPMENT_WORKFLOW.md). Vor Übergabe
Backend stoppen und Gatesperre bestätigen; keine weiteren Spiel-MCP-Aufrufe bis
`live bereit`. Kein Commit/Push vor erfolgreichem Gate und Livetest.

Nach erfolgreichem Skriptlauf, Spielstart und Bereitmeldung:

1. Backend starten und Bridge 0.55.0 samt aktueller Session bestätigen.
2. Zwei kleine Seiten ohne Statusfilter lesen, dazwischen einen harmlosen
   Statusaufruf. Cursor muss strikt ältere, nicht doppelte Sequenzen liefern.
3. `tool=inspect_agent_log,state=ok,limit=1` prüfen: passende abgeschlossene
   Einträge; ein unbekannter Werkzeugname muss eine leere Seite liefern.
4. Einen synthetischen Cursor mit fremder Log-Kennung gezielt zurückweisen.
   Keine Session-/Spielmanipulation erforderlich.
5. Aufbewahrungsangaben auf Konsistenz prüfen. Falls schon Verdrängung vorliegt,
   muss sie sichtbar sein; den Puffer nicht mit 129 Liveaufrufen künstlich füllen.
   Überlauf und Löschen sind gezielte synthetische Gate-Nachweise.

Erfolg: korrekte Seitengrenzen, Filter, Metadaten und Cursorablehnung.
Stopp beim ersten widersprüchlichen Kontrollfall; gezielt diagnostizieren,
keine automatische vollständige Wiederholung der Kette.
