# Kolonieweite Arbeitskräfteliste — 0.9.0

`inspect_workforce(offset, limit)` liest alle initialisierten, nicht gelöschten
Entities mit öffentlicher Worker-Komponente, stabil nach Entity-ID sortiert.
Maximal 32 Einträge pro Seite, offset 0–65535. GET `/agent-api/v1/workforce` mit
beiden Parametern. Keine Schreibfreigabe nötig, keine Fremdmod-Abhängigkeit.

Pro Eintrag: Arbeiter-ID, WorkerType, Employed, JobRunning, assignmentStatus sowie
gegebenenfalls Arbeitsplatz-ID, Gebäudetype/Template und Position. Die ID ermöglicht
Zuordnung und spätere gezielte Abfragen; persönliche Bibernamen werden nicht benötigt.

- assigned: Arbeitsplatzreferenz als reguläres Blockobjekt aufgelöst.
- unassigned: Worker.Workplace ist null.
- unresolved: Referenz vorhanden, Ziel aktuell nicht als initialisiertes reguläres
  Blockobjekt auflösbar. Daraus keine Arbeitslosigkeit ableiten.

Employed bleibt der separate Spielwert. Gesamtzahl, employed und unemployed beziehen
sich auf diese Worker-Komponenten, nicht auf die gesamte Bevölkerung oder auf die
Zahl einsatzfähiger Erwachsener. Kinder oder andere Entity-Typen werden nicht anhand
von Namen geraten; tatsächliche Zusammensetzung im Live-Test abgleichen.
JobRunning ist kein genauer Tätigkeitsname oder Produktionsnachweis. Arbeitsplatz-
Position ist nicht der momentane Standort des Arbeiters. Keine Personenbewegung,
Personalzuweisung oder Jobmanipulation.

Jede Seite ist eine neue Beobachtung mit Session und Zeitstempel. Mehrere Seiten
sind kein atomarer Snapshot; bei laufender Simulation können Zuordnungen wechseln.
Für konsistentere Analysen kann der Agent die bestätigte Spielpause nutzen und danach
zur gewünschten Geschwindigkeit zurückkehren. Kein automatisch erzwungenes Pausieren.

## Prüfung und Abnahme

Automatisiert: Parametergrenzen, Seitenanzahl/hasMore, Summen, doppelte Arbeiter-IDs,
fehlende/widersprüchliche Arbeitsplatzreferenzen, unresolved bei employed=true,
MCP-stdio/HTTP-Durchlauf. Öffentliche Worker-API lokal anhand Spielmetadaten geprüft.
Live offen: kleine Kolonie vollständig lesen und aufgelöste Zuordnungen mit
inspect_building.operations.workplace.assignedWorkers abgleichen; Änderungen
zwischen Beobachtungen berücksichtigen. Danach genügt im normalen Agentenablauf
eine kleine Zahl von Listenaufrufen statt einer Abfrage pro Gebäude.

Gehört in das noch nicht installierte 0.9.0-Paket zusammen mit der
[Gebäudediagnose](building-operations.md). Das ältere lokale 0.9.0-Paket ohne diese
Erweiterung wird nicht überschrieben; für Installation das neueste Paket verwenden.
