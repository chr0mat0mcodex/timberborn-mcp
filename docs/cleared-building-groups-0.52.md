# Flächenräumung und gemeinsamer Gruppenbau — 0.52.0

Status: implementiert und quellgeprüft; menschliches Test-/Build-/Installationsgate
und gezielter Livetest offen. Letzte abgenommene Version bleibt 0.51.0.

## Vertrag

Vier Werkzeuge: `plan_cleared_building_group`, `start_cleared_building_group`,
`inspect_cleared_building_group`, `stop_cleared_building_group`.
Plan/Start benötigen gemeinsam Bau-, Flächen-, Entfernungs- und Temposchalter.
Inspect liest ausschließlich den lokalen Checkpoint. Stop fordert das Ende des
Dispatchers an; ein bereits laufender Einzelaufruf kann noch abschließen.

`spec` enthält session, jobId, districtId, x/y/z, width/height, templates und
reservedAreas (auch leeres Array). Genau eine Fläche bis 8×8 auf einer Höhe,
1–8 Wunschgebäude; bis acht Ausschlussvolumen. Beim Räumen werden deren ganze
XY-Spalten konservativ ausgespart, beim Bau gelten die exakten Volumen aus 0.51.
Die gemeinsame Baugruppe bleibt auf 32 Teile, 32×32 und acht Höhen begrenzt;
Zugangshilfen können außerhalb der geräumten Fläche liegen und werden dort nicht
automatisch freigeräumt. Bestehender Distriktweg und native Prüfungen bleiben nötig.

Optionale spec-Werte: clearanceHours=48, constructionHours=120 (je 1–672,
zusammen höchstens 672), maxRealSeconds=600 (60–3600, gemeinsames Budget),
speed=7 (1/3/7), searchBudget=16 (1–32), maxCandidates=3 (1–4).
Arbeitszeiten werden berücksichtigt, indem auf tatsächliche Räumung gewartet wird;
keine Änderung der Schicht, Soforternte oder künstliche Arbeiterzuweisung.
Das Räumbudget muss gegebenenfalls die Nacht überbrücken.

## Ablauf

1. Vorschau bei Pause: konkrete natürliche/gepflanzte Vegetation und zu löschende
   Pflanzmarkierungen lesen, clearanceKey zurückgeben. Noch keine Aussage, dass
   alle Gebäude später platzierbar sind. `detail=full` zeigt Einzelziele.
2. Start mit identischer spec und clearanceKey. Geänderte Räumziele/Markierungen
   verwerfen die Vorschau. Gleiche jobId liest nur; andere Parameter werden abgelehnt.
3. Anbau-/Baumpflanzmarkierungen entfernen, homogene Rechtecke bis 4×4 gemeinsam.
   Neu lesen, weil `planted` nun als `vegetation` klassifiziert wird. Bis 64
   explizit beobachtete Pflanzen über reguläre Demolishable-Markierungen räumen.
   Markierung bestätigt nur den Auftrag; sie ist kein Fertigstellungsnachweis.
4. Falls noch Ziele existieren: einen begrenzten Simulationslauf starten, etwa alle
   zwei Sekunden Ziele prüfen. Bei tatsächlicher Räumung früh pausieren; nach
   bestätigter Pause Markierungen und Vegetation erneut prüfen.
5. Neue gemeinsame Standort-/Zugangsplanung mit frischen Flächendaten. Nur bei
   vollständiger Gruppe deren Schritte/Schlüssel unverändert einmal an den
   bestehenden autonomen Großbaulauf übergeben. Teilplan: blockiert, kein Bauauftrag.
6. Bauabschluss abwarten, Pause und frischen Projektabschluss prüfen. `completed`
   bedeutet fertige Gruppe mit beobachteten Zugängen und freigegebenem Ledger;
   keine Produktions- oder Kraftversorgungsbehauptung.

**Rohstoffstapel werden ignoriert, niemals gelöscht oder eigens umgeräumt.**
Ihre reguläre Verschiebung übernimmt das Spiel beim Bauen. Die vorläufige
Kollisions-/Flächensuche behandelt RecoveredGoodStack nicht als festen Baukörper;
native Spielvalidierung bleibt maßgeblich. Bestandsgebäude/Wege werden nicht
abgerissen. Pflanzenräumung ist kein Holzernteauftrag und garantiert keinen Ertrag.

## Ausführung, Unterbrechung und Belege

Der MCP-Server hält einen begrenzten Hintergrunddispatcher; die Mod führt die
vorhandenen Simulations-/Bauläufe aus. Ein Agentenaufruf pro Bauteil ist unnötig.
Andere schreibende Werkzeuge desselben Backends werden während des Dispatchers
abgewiesen; lesende Abfragen bleiben möglich. Menschliche Eingriffe sind weiter
möglich, Erwartungswerte und native Tempo-/Bauprüfungen erkennen Konflikte.

Vor jeder Mutation steht ein atomarer lokaler Checkpoint mit pendingAction und
runId/actionId. Unbestätigte Aktionen werden nie erneut gesendet. Die lokale
Dateisperre verhindert parallele Räumdispatcher in mehreren Backendprozessen.
Nach Backendneustart: `interrupted`, nur Diagnose über die gespeicherten nativen
IDs, keine automatische Wiederaufnahme. Keine Garantie gegen fremde Backend-
oder menschliche Änderungen zwischen zwei Beobachtungen; kein atomarer Gesamtauftrag.

Bei Backendende/Verbindungsfehler bleibt ein bereits gestarteter nativer Lauf an
sein Zeitlimit gebunden und pausiert selbst; keine behauptete sofortige Pause.
Stop fordert eigenen Laufstopp einmal an, anschließend Status nachlesen.
Bei realem Budgetende kann die Pause noch ausstehen; `unconfirmed` und native IDs
beachten. Dispatcher-Abbruchreserve maximal 15 Sekunden, keine Budgetverlängerung
des Spielruns. Vor neuer jobId alte Effekte frisch prüfen, niemals als Retry nutzen.

`inspect` kostet keine Spielreads. Compact nennt Phase, Grund, verbliebene Ziele,
Pausebeleg, IDs, Checkpointzeit und nächsten Schritt. `detail=full` behält letzte
Beobachtung, Suchbelege und Bau-/Abschlussbericht. Fehlende Detailfelder sind keine
Leerbeobachtung. Reale Räumblockaden nennen verbliebene Ziele im Vollbericht;
fehlende Arbeiter, Zugang oder Arbeitszeit bleiben Prüfpunkte, keine erfundene
exakte Ursache. Keine automatische Budgeterhöhung oder neue Räumwelle.

Native Hilfslesungen: `areas` optional regional bis 8×8 auf einer Höhe;
`removal-targets` intern bis 256 Objekte einer 8×8×4-Beobachtung in einem Aufruf.
Damit keine instabilen Seiten während Arbeiter Pflanzen entfernen. Öffentliche
Einzelwerkzeuge behalten ihre bisherigen kleineren Abfragelimits.

## Vorbereitete Nachweise und menschliches Gate

Automatische Tests vorbereitet: vollständige Räumung→Pause→Neuplanung→Gruppenbau,
Rohstoffstapel unberührt, Ausschlüsse, gemischte/gebündelte Pflanzmarkierungen,
Zeitlimit mit Restzielen, unbestätigte Markierung ohne Replay, geänderte Vorschau,
Teilplan ohne Bau, Tempo-/Markierungseingriff, Stop vor Mutation, Neustart ohne
Replay, Compact/Full, Aktionsschalter und native Abfragegrenzen.
Nicht vom Agenten ausgeführt; Compiler-/Testnachweis entsteht im menschlichen Gate.

Livetest nach erfolgreichem Skript und `live bereit`:

- Kleine bewachsene/markierte Fläche und zwei Gebäude: Vorschau/Start einmal,
  nur Statusabfragen; echte Räumung, gemeinsame Wege, Fertigstellung, Zugänge und
  Endpause nachweisen. Reale Arbeitswartezeit sichtbar, kein Advance pro Teil.
- Vorhandener Rohstoffstapel im Bauumfeld: kein Entfernen/Räumauftrag dafür;
  nativen Bau über/bei dem Stapel gezielt prüfen. Ortswechsel nicht als Verlust werten.
- Begrenzter Räumfall mit verbleibenden Zielen: Budgetende ohne Gruppenbau;
  konkrete Restziele, kein stilles Wiederholen oder Budgetwachstum.
- Checkpoint-Compact/Full und Wiederholung derselben jobId nur lesend prüfen.
- Testbauten einschließlich entbehrlicher Wege danach entfernen; Rohstoffstapel
  erhalten. Bei falscher Negativkontrolle stoppen, nicht weiter skalieren.

Keine Versionsabnahme oder Veröffentlichung vor diesen Nachweisen.

## Gate-Korrektur 2026-10-12

Erster menschlicher Lauf: MCP-/Testkompilierung erfolgreich, Unit-Gate abgebrochen
bei Cancellation vor Transport. Die neue Aufrufsperre lag ausserhalb der
Fehlernormalisierung. Aufnahme jetzt innerhalb derselben Ergebnisgrenze;
Abbruch ohne Transport bleibt not_executed. Konkurrierende Aufrufe werden sofort
abgewiesen, nicht hinter einem unklaren Auftrag eingereiht. Nur der Besitzer gibt
die Sperre frei. Vorbereitete Regression prueft Abbruch unter belegter Sperre,
keinen zweiten Transport und anschliessende Freigabe. Erneutes Gate offen;
keine Agent-Testausfuehrung, noch keine Mod-/Liveabnahme.

## Wiederanschluss nach Gate-Korrektur

Nach erneutem menschlichem Gate und `live bereit`: Backend gestartet,
Gatesperre aufgehoben, native Bridge meldet 0.52.0 mit aktueller Sitzung.
Der Vorschaltprozess hat alle vier neuen cleared-building-group-Werkzeuge im
Katalog. Der laufenden Codex-Agentensitzung fehlen sie weiterhin trotz
Katalogwechselmeldung. Gezielter Livetest deshalb noch nicht begonnen;
keine Bau-/Raeumauftraege gesendet, keine Abnahme und kein Commit/Push.
Ein Client-Neustart muss den bereits aktualisierten Katalog uebernehmen;
kein erneuter Build oder Spielneustart erforderlich. Bekannte B12-Grenze.

## Begrenzte Liveabnahme 2026-10-12

Nach erneutem menschlichem Gate, installiertem Paket und Client-Neustart waren
alle vier Werkzeuge verfügbar; Bridge 0.52.0 und aktuelle Sitzung bestätigt.
Die vorherigen offenen Gate-/Katalogmeldungen sind damit erledigt.

- Negativpilot: fünf Birken zur Räumung markiert. Nach genau einer
  Spielstunde blieben zwei Ziele; blocked, Endpause und kein Bauplan/-auftrag.
  Dieselbe jobId mit gleichen Parametern lieferte nur den unveränderten
  Endcheckpoint, keinen zweiten Lauf.
- Positivpilot: zwei Restziele geräumt; früher Abbruch des Räumzeitfensters
  nach 1,40625 Spielstunden statt 24. Anschließend frische gemeinsame Planung,
  Bank und Holzfäller autonom fertig und zugänglich, Projekt freigegeben,
  null verlorene Bestandsverbindungen und Endpause. Rund 14,8 Sekunden vom
  angenommenen Auftrag bis Abschluss; keine Agentenfortschaltung pro Teil.
- Markierungsprobe: zwei Karotten-Pflanzmarkierungen in einer Gruppe entfernt;
  anschließend dieselbe Zweiergruppe gebaut. Die Bank stand exakt auf der zuvor
  belegten Rohstoffstapel-Zelle. Kein Löschauftrag für Stapel; reguläre native
  Bauprüfung und Fertigstellung bestanden. Keine Behauptung zur exakten
  Güterbilanz oder zum späteren Verbleib des alten Stapels.
- Compact/Full: 5.268 statt 67.790 JSON-Zeichen (92,2 % weniger), gleicher
  terminaler Zustand, IDs, Zeitstempel und Plan. Full enthält Einzelbelege.
- Beide Zweiergruppen nach den Proben gezielt abgerissen. Bestehender Weg und
  Rampe erhalten; neuer Rohstoffstapel bei Endkontrolle vorhanden; Tempo null.

Nachweisgrenze: kleine 2×5-Fläche auf einer Höhe, zwei Gebäudetypen,
vorhandener gemeinsamer Weg ohne neue Hilfsteile. Größere Gruppen, neue
Höhenhilfen und Prozessausfall wurden in diesem Livepilot nicht erneut geprüft;
bestehende Fachnachweise und die automatischen Gate-Tests bleiben getrennt.
Kein allgemeiner Nachweis für beliebige Landschaften oder Gebäude.
