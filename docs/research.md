# Forschung und Gebäudefreischaltung

Freigegebene nächste Erweiterung nach 0.16.0. Noch nicht implementiert oder live getestet.

## Öffentliche API geprüft

Die öffentlichen Metadaten der lokal installierten Timberborn-Version 1.1.2.4 bieten:

- `ScienceService.SciencePoints`: aktuelle Forschungspunkte.
- `BuildingSpec.ScienceCost`: Freischaltkosten der Vorlage.
- `BuildingUnlockingService.Unlocked(BuildingSpec)`: Freischaltzustand.
- `BuildingUnlockingService.Unlockable(BuildingSpec)`: reguläre Freischaltbarkeit.
- `BuildingUnlockingService.Unlock(BuildingSpec)`: regulärer Freischaltaufruf.

`UnlockIgnoringCost`, `AddPoints` und `SubtractPoints` werden nicht als Agentenaktionen
angeboten. Die Signaturen belegen den API-Zugang; den tatsächlichen Kostenabzug
beweist erst ein Live-Test. Keine neue Fremdmod oder Paketabhängigkeit erforderlich.

Der bestehende BuildingCatalog liest bereits `Unlocked`. SiteValidation weist
gesperrte Vorlagen vor der Vorschau mit `template_unavailable` zurück.

## MCP-Vertrag

- `inspect_research`: Forschungspunkte und seitenweise Vorlagen mit Kosten,
  Freischaltzustand, Verfügbarkeit und regulärer Freischaltbarkeit. Session und
  Beobachtungszeit wie bei vorhandenen Lesern. Keine erfundenen Technologie-Abhängigkeiten:
  die geprüfte öffentliche API bietet keine Liste konkreter Voraussetzungsknoten.
- `unlock_building`: konkrete Vorlage, aktuelle Session und erwartete Kosten.
  Eigenes Opt-in in Mod und MCP. Auf dem Spielthread zuerst Vorlage, Verfügbarkeit,
  Kosten, Punkte und Freischaltbarkeit prüfen. Bereits freigeschaltet ergibt einen
  unveränderten Zustand ohne zweiten Unlock-Aufruf. Danach genau ein regulärer
  Unlock-Aufruf und Rückabfrage von Punkten sowie Freischaltzustand.
- Ergebnis unterscheidet Ablehnung, bereits freigeschaltet, bestätigt und unbestätigt.
  Keine automatischen Wiederholungen oder kompensierenden Punktänderungen.
- Optionales reasoning und Ingame-Protokoll über den vorhandenen zentralen Call-Pfad.

## Abnahme

1. Parser-, Gate-, Session-, Schema- und Transaktionstests mit synthetischen Daten.
2. Live zuerst Punkte und Kosten lesen; eine gesperrte Vorlage auswählen.
3. Unzureichende Punkte und abweichende erwartete Kosten ohne Änderung abweisen.
4. Bei ausreichenden regulär erwirtschafteten Punkten einmal freischalten.
5. Exakten Kostenabzug und neuen Freischaltzustand separat lesen; erneuter Aufruf
   darf keine weiteren Punkte kosten.
6. Bauvalidierung vor/nach Freischaltung vergleichen. Freischaltung allein bestätigt
   weder einen Bauauftrag noch ausreichende Baumaterialien oder funktionierenden Betrieb.

Falls Punkte fehlen, höchstens einen begrenzten Produktionspilot mit einem regulären
Erfindergebäude vorbereiten; keine Punkte injizieren und keine unbegrenzte Simulation.

## Freigabe und Ergebnisse

Mod-Konfiguration: `enableResearch: true`; MCP-Prozess: `TIMBERBORN_ENABLE_RESEARCH=1`.
Beide standardmäßig aus. `inspect_research` bleibt lesend verfügbar (19 native Leser).

`outcome`: `cost_changed`, `unavailable`, `insufficient_points`, `not_unlockable`
sind Ablehnungen ohne Änderung; `already_unlocked` ist ein unveränderter Erfolg.
`applied` bestätigt Freischaltung und den erwarteten Punkteabzug auf dem Spielthread.
`unconfirmed` oder ein Transportfehler erfordern ausschließlich eine neue Abfrage.
Die Aktion verwendet keine eigenen Save-Daten und verändert keine Baugeometrie.

## Erster Livepilot

Katalog (162 Vorlagen), Kostenabweichung, fehlende Punkte und bereits freigeschaltete
Vorlage live bestätigt; Punkte unverändert null. Ein regulär gebauter Erfinder blieb
wegen fehlenden Anschlusses unbesetzt. Nach 40 Sekunden Simulationspilot wieder pausiert.
Kostenabzug und positiver Unlock sind ausdrücklich noch offen. Details und entdeckte
Bauvorprüfung-Lücke im Projektjournal; kein weiterer Produktionslauf ohne Anschluss.
