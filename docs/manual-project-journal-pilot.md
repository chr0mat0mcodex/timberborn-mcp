# A10-Pilot: manuelles Großprojektjournal

Stand 2026-10-18: menschliches Gate und gezielter lesender Livebeleg bestanden.

## Umfang

Nur `LargeProjectLedger` und sein manueller Startpfad. Kapazität bleibt 128;
abgeschlossene und freigegebene Einträge bleiben erhalten. Kein Löschen,
Archivieren, automatisches Neuladen oder Wiederverwenden alter Aktions-IDs.
Autonome Chargen und andere Journale bleiben unverändert.

Neue ID bei vollem Journal: `large_project_journal_limit` statt `state_conflict`.
Identische bekannte ID wird zuerst aufgelöst und liefert ihren bisherigen Beleg;
abweichender Fingerprint bleibt `state_conflict`. Ein ungelöstes Projekt unterhalb
des Limits bleibt ebenfalls `state_conflict`. Die Mod prüft Kapazität vor erneuter
Vorschau oder Registrierung, nachdem bestehende IDs geprüft wurden. Übergeordnete
Sperren (etwa aktive autonome Charge) behalten ihre bisherige Priorität.
Die gemeinsame jobs-Grenze bleibt zusätzlich unverändert: ein durch autonome
Chargen belegtes gemeinsames Kontingent ist nicht als manuelles Journal voll
auszugeben. Freie manuelle Kapazität beweist daher keine globale Startmöglichkeit.

Der freigegebene Fehlercode gelangt über HTTP 409 zum nativen Client und zur
MCP-Antwort. Ausschließlich `start_large_project` mit genau einem Schreibversuch
und diesem expliziten Code meldet `execution=not_executed`, `retryable=false` und
nennt Limit sowie fehlende Kapazitätsfreigabe. Andere Konflikte werden dadurch
nicht zu einem Nichtausführungsnachweis. Kurzer Aktivitätstext nennt denselben Grund.

Planberichte aus `LargeProjectService.Plan` und manuelle Projektbelege ergänzen
`manualJournal`: `scope=manual_large_projects`, `limit=128`, `count`, `remaining`,
`releaseFreesCapacity=false`. Auch gemeinsam verwendete Planpfade zeigen damit
nur das manuelle Journal, nicht die Kapazität autonomer Chargen. Compact erhält
den Block; Full ebenso. Fehlendes Feld bei älteren Bridges bedeutet unbekannt.
Der native Client prüft vorhandene Werte auf Typen, Grenzen und Konsistenz.
Zähler fließen nicht in den planKey ein, reservieren keine Kapazität und sind
keine Ausführungsfreigabe. Start prüft den aktuellen Stand erneut.
Bridge-API bleibt 0.74.0; Diagnose additiv, keine globale Versionslistenmigration.

## Tests und Abnahme

Synthetisch: 127/128 Einträge und Ablehnung des 129. Starts, jeweils abgeschlossene
und freigegebene Historie; Replay und abweichender Fingerprint bei vollem Journal;
Konflikt unterhalb der Grenze; konsistente/fehlerhafte Kapazitätsdaten, fehlender
Block bei älteren Antworten, Compact-Erhalt sowie HTTP-409-Transport bis zur
MCP-Diagnose mit genau einem Schreibversuch. Keine Agent-Test-/Buildläufe.

Nach erfolgreichem menschlichem Gate: verbundenes Backend starten, native Session
und Kennung prüfen. Ein geeignetes kleines, strukturiert ausgewähltes Projekt
nur planen; `manualJournal` in compact/full auf limit/count/remaining prüfen.
Erneute Planung darf den Zähler nicht erhöhen. Kein Bauauftrag erforderlich;
kein Anlegen von 128 Liveprojekten. Volles Journal und ID-Schutz sind synthetisch
belegt, nicht als Live-Grenztest ausgeben. Abbruch bei fehlendem/inkonsistentem
Diagnoseblock oder unerwarteter Zähleränderung; keine automatische Wiederholung.

## Bewertung und Restumfang

Bewertung positiv, kleiner und klarer Nutzen: Journalerschöpfung und Projektkonflikt werden
unterscheidbar, verbleibende Kapazität ist sichtbar. Aufwand: Core-/Adapteränderung,
additiver Vertrag, Fehlerdarstellung und gezielte Tests. Keine neue Abhängigkeit.
Aufbewahrung, andere Journale, autonome Chargen und ein möglicher Reloadbedarf
bleiben eigene Aufgaben. Keine weitere Migration. Der zusätzliche Prüfaufwand
ist durch die getrennten Nachweise für Kapazität, Identität und Transport gerechtfertigt.

## Tatsächlicher Nachweis

Gate: 13 Paket-, 35 Supervisor-, 2239 Unit- und 14 Integrationstests bestanden;
drei optionale Livefälle übersprungen. Alle 18 neuen Fälle bestanden, beide
Release-Builds ohne Warnungen/Fehler, fünf installierte Dateien per SHA-256 bestätigt.

Nach verbundenem Backendstart Bridge 0.74.0 und aktuelle Session bestätigt;
MCP-Kennung stimmt mit Gate überein. Spiel pausiert, kein aktiver Simulationslauf.
Kleiner einteiliger Bankplan in Full und erneut in Compact gelesen: beide Male
`manualJournal={scope:manual_large_projects, limit:128, count:0, remaining:128,
releaseFreesCapacity:false}`. Gleiche Kapazitätsdaten und gleicher planKey;
Planung hat keinen Journaleintrag angelegt. Native Vorschau jeweils restauriert,
keine Session-Sperre und keine verlorenen Bestandsverbindungen.

Der Plan blieb wegen fehlendem Eingangs-/Bauzugang nicht ausführbar; das ist
getrennt von der bestandenen Journaldiagnose. Kein Bauauftrag oder Ausbau nötig.
Volles Journal, Freigabe, ID-Replay und HTTP-Limitablehnung wurden ausschließlich
synthetisch geprüft; kein Live-129.-Start behauptet. Gesamt-A10 bleibt hinsichtlich
anderer Journale und Aufbewahrungsfragen offen, weitere Erweiterungen zurückgestellt.
