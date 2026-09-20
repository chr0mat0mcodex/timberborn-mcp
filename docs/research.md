# Forschung und Gebäudefreischaltung

Implementiert seit 0.17.0 und **live bestätigt**, einschließlich Speichern/Neustart.
Keine zusätzliche Mod. [Aktueller Projektstand](../PROJECT_STATE.md).

## MCP-Vertrag

- `inspect_research(offset, limit)`: Forschungspunkte und seitenweise Vorlagen mit
  scienceCost, available, unlocked und regulärem unlockable. Alle Seiten lesen;
  Session und Beobachtungszeit beachten. Keine atomare Gesamtsicht.
- `unlock_building(template, session, expectedCost)`: vor dem Eingriff Vorlage,
  Verfügbarkeit, erwartete Kosten, Punkte und reguläre Freischaltbarkeit prüfen.
  Genau ein normaler Unlock-Aufruf, danach Punkte und Zustand zurücklesen.
- Eigenes Opt-in: `enableResearch=true` in der Mod und `TIMBERBORN_ENABLE_RESEARCH=1`
  im MCP-Prozess. Beide standardmäßig aus; der Leser braucht keine Schreibfreigabe.
- reasoning und Ingame-Log über den zentralen nativen Aufrufpfad.

| outcome | Bedeutung |
| --- | --- |
| cost_changed, unavailable, insufficient_points, not_unlockable | Ablehnung ohne Freischaltung |
| already_unlocked | Unverändert; kein weiterer Unlock-Aufruf oder Punkteabzug |
| applied | Freischaltung und exakter unmittelbarer Kostenabzug bestätigt |
| unconfirmed | Ergebnis abweichend/unklar; ausschließlich nachlesen, nicht automatisch wiederholen |

Die normalen [Fehlercodes](bridge-errors.md) ergänzen Transport-/Session-Ablehnungen.
Freischaltung bedeutet weder Bauauftrag noch ausreichende Baumaterialien oder Betrieb.

## Öffentliche Spiel-APIs

Geprüft an Timberborn 1.1.2.4: ScienceService.SciencePoints, BuildingSpec.ScienceCost,
BuildingUnlockingService.Unlocked/Unlockable/Unlock. UnlockIgnoringCost, AddPoints und
SubtractPoints werden nicht als Agentenaktionen verwendet. Konkrete Knoten einer
Technologie-Voraussetzungsliste sind durch diese Schnittstelle nicht verfügbar.

## Belegte Abnahme

- 162 Vorlagen seitenweise gelesen (Anzahl gilt für die Testszene).
- Falsche erwartete Kosten und fehlende Punkte abgewiesen; bereits freigeschaltete
  Vorlage unverändert. Separate Rückabfragen ohne Punkteänderung.
- Erfinder regulär gebaut, am Distrikt angeschlossen und besetzt; 35 Punkte produziert.
- Förster für 30 Punkte freigeschaltet: 35 → 5. Zustand separat bestätigt.
- Wiederholungsaufruf: already_unlocked, 5 → 5.
- Vorher template_locked in der Vorprüfung, danach gültige Spielvalidierung; kein
  zusätzlicher Förster-Bauauftrag für diesen Nachweis nötig.
- Nach Speichern/Neustart Freischaltung und fünf Restpunkte erhalten.

Der zunächst unbrauchbare Erfinderstandort offenbarte eine Lücke der Eingangsvorprüfung;
0.17.1 korrigiert den Wegnachweis und zeigt Eingangsbelegung separat. Datierte Details
stehen im [Projektjournal](project-journal.md).
