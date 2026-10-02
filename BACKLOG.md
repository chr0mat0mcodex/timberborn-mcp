# Offene Entwicklungsarbeit

**Testsystem; schnelle und effiziente Entwicklung zuerst.** Spielstände sind
verzichtbar. Aktuelle Regeln: [AGENTS.md](AGENTS.md). Kein Wiederaufbau oder Erhalt
historischer Kolonien als Voraussetzung für die folgenden Aufgaben.

## Als Nächstes

- [ ] Bauprojektausführung aus der installierten 0.25.0 live prüfen. Geeigneten
  aktuellen Testfall wählen, Ergebnis lesen, konkrete Fehler direkt bearbeiten.
- [ ] Danach sinnvoll gebündelt ausbauen: Gebäudetypen, Weglängen/Projektgrößen,
  Sonderlayouts und reale Erreichbarkeit. Ein-Lager-/Zwei-Wege-/Sitzungsgrenzen
  stammen aus dem jetzigen Code, nicht aus einer dauerhaften Beschränkung durch den Nutzer.
- [ ] Zuverlässige Wegkonfliktdiagnose und Bauarbeiterzugang weiter belegen.
  Vollständige hypothetische Bauphasenabdeckung ist noch offen. Unvollständige
  Belege kenntlich machen; effiziente reale Testfälle statt endloser Vorabdiagnose.

[Technischer Einstieg](docs/session-handoff.md) · [aktueller Vertrag](docs/building-project-execution-proposal.md).

## Weitere funktionale Lücken

- **Bauen:** große Vorlagen, Sonderformen, gespiegelte Platzierung, Treppen und
  besondere Geländeanschlüsse. Die frühere Katalogprüfung fand 15 begrenzte reguläre
  Vorlagen; bei Änderungen frisch ermitteln statt eine alte Kolonie vervollständigen.
- **Problemerkennung:** aktive Hunger-/Durst-UI-Alerts und weitere Statusfälle,
  vollständige Energie-, Wasser-, Rohstoff- und Lieferdiagnose.
  [Bedürfnisse/Betrieb](docs/needs-and-operation.md), [Alerts](docs/alerts-plan.md).
- **Erreichbarkeit:** weitere Gebäudetypen, Mehrdistriktfälle und besondere Weglayouts.
  Native Reichweite nicht mit Erntefähigkeit oder realem Betrieb gleichsetzen.
  [Logistik](docs/logistics.md).
- **Bilanz:** globale Güter, einzelne Gebäudeinventare, Baustellenbedarf und Produktion/
  Verbrauch passend zusammenführen. Repräsentative Wirtschaftstests nach Funktionsbedarf;
  keine Pflicht, eine bestimmte Kolonie dauerhaft zu versorgen. [Bilanz](docs/supply-balance.md).
- **Fehlerverträge:** Personal-, Prioritäts-, Flächen- und Entfernungsfälle weiter differenzieren.
- **Personal-Sonderfall:** `desiredWorkers=0` am Erfinder wurde nicht übernommen;
  für regulären Stillstand Gebäudepause verwenden, eine unbestätigte Änderung diagnostizieren.
- **Skalierung:** größere Kolonien, andere Fraktionen und Spielversionen gezielt prüfen.
- **Log-UI:** Filter, Leeren und Szenenwechsel bei Bedarf prüfen; Fenster/Scrollen bereits belegt.
- **Einstieg:** direkter Serverstart hat einen historischen Legacy-Default. Native Auswahl
  ausdrücklich setzen; mögliche Vereinheitlichung als technische Entscheidung behandeln.

## Später bei Bedarf

- [Frage-Popup mit Texteingabe im Spiel](docs/player-question-popup.md).
- Energieindustrie, Bots, Terraforming und Wasserbau als weitere API-/Testszenarien.
- Automatisches Speichern/Laden, falls technisch nützlich; kein Bedarf allein zum Save-Erhalt.
- Produktionsgraph: weitere Fraktionen, Ruinenerträge und spezielle Betriebsbedingungen.

Produktionsgraph, Ingame-Zeitläufe und Ingame-Log sind bereits implementiert.
Frühere Ziele „100 Biber“ und „jedes Gebäude“ sind historische Testszenarien und
keine aktuelle Fortsetzungsaufgabe. [Historische Checkliste](docs/colony-goals.md).
