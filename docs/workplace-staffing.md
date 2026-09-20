# Reguläre Sollbesetzung — 0.10.0

Status: implementiert und installiert; Live-Abnahme offen. Öffentliche Workplace-
API IncreaseDesiredWorkers()/DecreaseDesiredWorkers(), keine Fremdmod-Abhängigkeit.

`set_workplace_staffing(id, session, desiredWorkers, expectedDesiredWorkers)` setzt
an einem fertigen Arbeitsplatz die Sollbesetzung. Eigene Freigaben:
Mod enableStaffing=true, MCP TIMBERBORN_ENABLE_STAFFING=1; im Standard aus.
HTTP bodyloses POST `/agent-api/v1/workplace-staffing`, vier gleichnamige Parameter.

Session, gültige Entity-ID, fertiges reales Blockobjekt mit Workplace, unveränderte
Sollbesetzung und Ziel höchstens MaxWorkers werden auf dem Spielthread geprüft.
Ziel/Erwartungswert 0..64. Die Bridge ruft genau die nötige Anzahl regulärer +/-
Schritte auf, maximal 64; nach jedem Schritt wird eine Änderung um genau eins
verlangt. Bei Abweichung/Exception sofort stoppen, keine Wiederholung/Rückabwicklung.
Gleicher Zielwert ergibt eine bestätigte Beobachtung ohne +/-Aufruf.

Quittung: Gebäude-ID/Template, vorherige/gewünschte/beobachtete Sollbesetzung,
zugewiesene Arbeiter, Spielmaximum, applied oder unconfirmed. applied bestätigt
nur den Sollwert. Tatsächliche Besetzung anschließend über inspect_building und
inspect_workforce nachlesen; Beschäftigung/Produktion kann erst später reagieren.
Teiländerung bei Fehler möglich; kein automatisches Retry. Keine direkte Biber-
Zuweisung, Teleportation, Pauseänderung oder Beschäftigungspriorität.

## Begrenzter Live-Nachweis nach Installation

Frisch einen fertigen Arbeitsplatz mit freier Kapazität identifizieren, bevorzugt
Distriktzentrum statt Ein-Personen-Holzfäller. Aktuellen Sollwert und Worker-Roster
lesen; genau um eins erhöhen, Sollwert separat bestätigen. Falls Simulation läuft,
Besetzung begrenzt beobachten. Anschließend mit frisch geprüftem Erwartungswert
auf ursprüngliche Sollbesetzung zurückstellen und erneut lesen. Keine automatische
Rücksetzung nach unklarer Antwort; zunächst Zustand klären. Restliche Kolonie bleibt
außerhalb dieses Tests. Tatsächliche Arbeiteridentität kann sich regulär ändern.
