# Ergebniszeilen für Bauprojekte und Bauchargen — 0.59.0

Status 2026-10-16: nach menschlicher Bereitmeldung Bridge 0.59 bestätigt,
fachlicher Livepilot bestanden. Mensch bestätigt beide Bau-Ergebniszeilen als
vollständig und gut lesbar. Im unten genannten Umfang begrenzt live abgenommen.

## Umfang und Nachweisgrenzen

Die bestehende `outcomeSummary`-Leitung liefert die neuen Texte an das Agentenlog,
beide lokalen Dateilogs und die Ingame-Zeile „Ergebnis:“. Keine zusätzlichen
Spielabfragen, Aufträge oder Hintergrundaktualisierungen. Keine technischen
Kennungen oder rohen Fehlermeldungen im Ergebnistext. Fehlerantworten bestätigen
keine Wirkung; bei unbestätigter Ausführung nur lesend klären.

- `execute_building_project_pilot`, `inspect_building_project` und
  `start_building_project`: bestätigte Bauaufträge getrennt vom Fertigbau.
  `completed` beim Auftragsdienst ist kein Beleg fertiger Gebäude.
- `advance_building_project`, `inspect_building_completion`,
  `develop_building_project`: fertig beobachtete Bauteile, Gebäudezugang,
  laufende Simulation, fehlende Objekte, abweichende Einstellungen und erschöpftes
  Budget. Nur `finished_accessible` bestätigt den Abschluss einschließlich Pause;
  Betrieb/Produktion bleiben separat. Wege zählen als Bauteile.
- `start_building_batch`, `advance_building_batch`, `inspect_building_batch`,
  `stop_building_batch`: gespeicherter Chargenstand mit Zahl fertig und zugänglich
  belegter Gebäude. Stopp verhindert weitere Aufträge; ein vorhandener Lauf behält
  sein Budget. Kein neuer Zugangsbeweis aus einer gespeicherten Quittung.
- `plan_large_project`, `plan_large_project_access`, `plan_large_project_repair`:
  ausdrücklich nur Vorschau ohne Bauaufträge.
- `start_large_project`, `run_large_project`, `advance_large_project`,
  `inspect_large_project`, `stop_large_project`, `run_large_project_repair`:
  gespeicherter Projektstand, bestätigte Platzierungen, unbestätigte und offene
  Teile sowie historisch fertig/zugänglich belegte Bauteile. Pfade und Hilfsbauten
  zählen mit. Kein pauschaler Pausen-, Kraftnetz- oder Produktionsbeweis.
- `diagnose_large_project`, `close_large_project`: frische Bauprüfung mit
  fertig/zugänglich beobachteten Bauteilen. Freigabe kann offene, nicht erteilte
  Teile aufgeben; sie bedeutet nicht, dass alle geplanten Teile gebaut wurden.
- `plan_cleared_building_group`: Räumvorschau ohne Räum-/Bauauftrag.
  `start_cleared_building_group`, `inspect_cleared_building_group`,
  `stop_cleared_building_group`: gespeicherter Räum-/Baustand und enthaltene
  Baunachweise; angefragter Stopp ist noch kein Abschluss. Auch eine enthaltene
  ältere Abschlussprüfung bleibt bei dieser Abfrage ein gespeicherter Beleg.

Nicht enthalten: direkte Einzelplatzierung, separate Vertikalpiloten,
Sammelkonfiguration und andere Flächenaktionen. Fachberichte bleiben maßgeblich;
unbekannte Stopgründe verweisen darauf, statt eine Ursache zu erfinden.

## Gate-Nachweise und durchgeführter Livepilot

Gate-Tests: Auftrag versus Fertigbau, Bauabschluss versus Zugang/Budget,
historische Chargen und Stopp ohne Pausenversprechen, Compact/Full-Gleichheit,
Projektfreigabe mit offenen Teilen, fehlende/fehlerhafte Daten, Vorschauen,
Textlänge und Version 0.59 in bestehenden Vertragsregressionen. Der bestehende
Transport-/Dateilog-/Ingame-Pfad wird weiterverwendet.

Livebelege: anfänglicher Auftrag wegen bestehender Baustellen abgewiesen, ohne
Erfolgsbehauptung im Log. Nach begrenzter Fertigstellung dieser Baustellen neues
Einzelprojekt: erst laufender Auftrag, dann bestätigter Auftrag ohne Fertigbau,
unfertiger Bau, laufende Simulation und schließlich fertig/zugänglich mit Pause.
Eine Ein-Gebäude-Charge von 0/1 bis 1/1 fertig/zugänglich begleitet; spätere
Statusabfrage korrekt als gespeicherter Beleg bezeichnet. Zehn Bau-Ergebnistexte
zwischen Agentenlog und beiden Dateilogs übereinstimmend, keine Schreibverluste.
Ingame-Lesbarkeit vom Menschen bestätigt. Großprojekte, Räumgruppen, Teilfehler,
Budgeterschöpfung und Stopp bleiben synthetische Gate-Abdeckung, keine Liveprobe.

Versionspflege: Spielanleitung und `docs/hermes-skill/SKILL.md` aktualisiert;
Hermes bleibt eine lokale, nicht installierte und nicht abgenommene Vorlage.
