# Aktionsergebnisse und kompakte Laufberichte — 0.45.0

Status: menschliches Gate und begrenzter Livepilot am 2026-10-12 bestanden.

Im ersten Gate bestanden Build und Unit-Tests; Integration stoppte an einer alten
Log-Erwartung für einen per POST abgewiesenen Freischaltauftrag. Erwartung auf
`unconfirmed` samt `error.execution` korrigiert; lesende Session-Abweisung als
Gegenprobe mit `not_applicable`/Log `rejected` ergänzt. Danach erneutes menschliches
Gate und Bereitmeldung; installierte Bridge 0.45.0 live bestätigt.

## Fehler vor und nach Übergabe eines Spielauftrags

Zentrale native Fehlerantworten enthalten zusätzlich `error.execution`:

| Wert | Beleg und nächster Schritt |
|---|---|
| not_executed | Kein schreibender Spielauftrag an den HTTP-Transport übergeben. Ursache korrigieren; kein automatischer Wiederholungsversuch. |
| unconfirmed | Mindestens eine Übergabe an den Schreibtransport begonnen. Empfang, vollständige Ausführung oder Rückmeldung sind nicht ausreichend bestätigt. Mit vorhandenen IDs lesend klären; nicht erneut senden. |
| not_applicable | Fehlgeschlagene Leseabfrage, kein unbestätigter Schreibauftrag. |
| ledger_required | Unterbrochene Baucharge: gespeicherter Checkpoint und bestehende Auftrags-IDs sind maßgeblich. Das kann Aufträge früherer Aufrufe betreffen. |

Ein pro asynchronem Aufruf isolierter Zähler wird unmittelbar vor SendAsync
gesetzt; schon vorher abgebrochene Requests zählen nicht. Nach Übergabe bleiben
auch Parameterfehler, beschädigte Antwortdaten, Konfliktantworten, Timeout und
Verbindungsabbruch vorsichtig unconfirmed. Ein Fehlercode allein beweist nicht,
dass bei einer mehrteiligen Aktion nichts verändert wurde. Die Übergabe beweist
umgekehrt keinen Empfang durch das Spiel. Kein Auslesen privater HttpClient-Interna.

Ingame-Aktivitätsmeldungen liegen außerhalb dieses Zählers. not_executed bezieht
sich auf schreibende Spielaufträge, nicht auf Logeinträge oder lokale Plan-/Ledger-
Dateien. Bestehende erfolgreiche oder fachlich abgewiesene Ergebnisbelege unter
data (applied/rejected/partial/unconfirmed usw.) bleiben unverändert maßgeblich.
Die Fehlerkennzeichnung ersetzt keinen solchen Fachbeleg.

Syntaktisch ungültiges detail/reasoning wird bereits vor Protokoll-Dispatch als
InvalidParams mit „Nicht ausgeführt“ abgewiesen. Der Bauabschluss-Parser weist
seine lokalen Eingabefehler ebenfalls ausdrücklich not_executed aus. Der bestehende
Bauchargenfehler erhält ledger_required statt einer pauschalen Aussage über
sämtliche historischen Aufträge. Kein neues Retry oder eigener Wiederanlauf.

## Laufberichte

Alle vier bestehenden Werkzeuge unterstützen `detail=compact|full`:
run_simulation_for, run_simulation_until, inspect_simulation_run und
cancel_simulation_run. Standard ist compact. detail wird vor Logging und nativen
Parametern entfernt; es verändert weder Auftrag noch Simulation.

Compact behält unverändert:

- Run-ID, Zustand, Grund, terminal, Pause, Tempo, Lauf-/Zielzeit und Zeitbudgets.
- Aktive Bestands-/Forschungsziele oder Gebäudenachweise, einschließlich aktueller
  und historisch beobachteter Blockaden sowie historischer Beobachtungszeit.
- Native Zykluszahlen, Messdauer, ungerundete Raten, rezeptabgeleitete Mengen,
  größte beobachtete Wartekategorie und alle Zeitkategorien mit positiver Dauer.
- Aktuelle Diagnosebefunde samt eigenem Beobachtungszeitpunkt; unavailable und
  unbekannte Befunde bleiben sichtbar. Ein aktueller Befund ist kein historischer
  Produktions- oder Ursachenbeweis.

Full ergänzt redundante Gebäudeauswahl, Journalcursor, Rezeptausstoß je Zyklus,
Null-Zeitkategorien, vollständige aktuelle Inventar-/Quellen-/Wege-/Betriebsbelege
und Einschränkungen. Inaktive Zielgruppen entfallen aus Compact. Forschungsausstoß
entfällt nur bei einem Rezept ohne Forschung. Eine null Rate bei fehlender
Messdauer bleibt null. Ausgelassene Zeitkategorien sind nach vollständiger
Validierung bekannte null Stunden; andere ausgelassene Detailfelder sind keine
leeren oder unbekannten Beobachtungen.

Native Antworten werden vor der Darstellung vollständig validiert. Die Projektion
arbeitet auf einer Kopie, verändert weder Ledger noch Messwerte und ruft keine
zusätzlichen Spielwerkzeuge auf. Text und StructuredContent zeigen dieselbe
Ansicht. Fehlerantworten werden nicht gekürzt. Das spart Antwortumfang, nicht die
internen nativen Diagnoseabfragen. Keine neue Ausgabeversion neben der Bridge.

Für mehr Details eines bereits gestarteten Laufs **inspect_simulation_run mit
derselben runId und detail=full** verwenden. Keinen Start oder Abbruch wiederholen,
nur um mehr Text zu erhalten. Kompakt/Full am besten am terminalen Lauf vergleichen;
frische Diagnosedaten können sich zwischen zwei Lesungen ändern.

## Vorbereitete Tests und Livepilot

Regressionen: Eingabefehler ohne Transport, Abbruch vor Übergabe, Verlust/
beschädigte Daten/Konflikt/Argumentfehler nach POST, isolierte parallele Aufrufe,
Aktivitätslogging ohne falschen Schreibnachweis, Lesefehler. Ansichten: exakte
Messwerte, vollständiger unveränderter Ausgangsbericht, Null-/Unbekanntwerte,
unbestätigte Pause, aktive Ziele, optionale Schemafelder und identischer Text/
StructuredContent. Bekannte Version 0.45 ergänzt; negative Fixture bleibt 99.0.0.
Tests ausschließlich durch das menschliche Gate, kein Agent-Build/Testlauf.

Abnahme nach Gate:

1. Messmodus ohne Gebäude und neunte ID abweisen: invalid_argument plus
   not_executed; run_not_found bestätigt keine Laufanlage. Keine Fehlerwiederholung.
2. Kurzen bestehenden Gebäudemesslauf beenden lassen; Compact und Full desselben
   terminalen Laufs lesen. Identische historische Werte, Stoppgrund und Pause;
   vollständige Belege nur in Full. Ziel mindestens 25 % weniger JSON-Bytes im
   repräsentativen Compact-Bericht. Messung benennt ihre begrenzte Stichprobe.
3. Bei abweichenden Messwerten oder falscher Ausführungsgewissheit stoppen und
   korrigieren. Keine künstlichen Live-Verbindungsabbrüche zur Fehlerprovokation;
   Transportfehler gehören in synthetische Gate-Tests.

Keine neuen Bauten oder Rohstofflöschung nötig. Nach bestandenem Pilot Doku
abschließen, gezielt committen und pushen; andere lokale Arbeiten erhalten.

## Liveergebnis 2026-10-12

Beide Auswahlfehler (Messmodus ohne Gebäude, neun IDs) lieferten invalid_argument,
not_executed und retryable=false. Anschließendes inspect meldete jeweils
run_not_found/not_applicable: keine Laufanlage.

Ein Messlauf über exakt drei Spielstunden mit Erfinder und Papierfabrik endete mit
completed/production_measurement_complete und bestätigter Pause. Erfinder: zwei
native Zyklen, 1,375 Stunden producing, 1,4375 außerhalb der Arbeitszeit und
0,1875 unknown. Papierfabrik: null Zyklen, drei Stunden multiple_blockers.
Dies ist ein Ansichtennachweis, keine zusätzliche Betriebsabnahme der Papierfabrik.

Compact und Full desselben terminalen Laufs: alle verbleibenden Werte identisch,
einschließlich Diagnose, Pause und ungerundeten Messwerten. Bekannte Null-Zeitanteile
entfallen in Compact. Text und StructuredContent stimmen je Ansicht überein.
Vollständige Inventarbelege sind in Full vorhanden und in Compact ausgelassen.
Serialisierter StructuredContent in UTF-8: 3.436 gegenüber 8.206 Bytes, 58,1 %
Reduktion. Begrenzte Stichprobe, keine allgemeine Größen- oder Latenzgarantie.
Keine neuen Bauten, Einstellungen oder Rohstofflöschungen; Simulation pausiert.
Transportabbrüche bleiben synthetische Gate-Fälle. Keine Wiederholung von Aktionen.
