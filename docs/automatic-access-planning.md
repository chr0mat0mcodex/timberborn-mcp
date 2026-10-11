# Automatische Zugangsplanung für Gesamtpläne

Stand 2026-10-11: nach menschlichem Gate und erneuter Bereitmeldung begrenzt
live abgenommen, gemeinsamer 0.37.0-Build. Compilerkorrektur (`Vector3Int`) und
Isolation des Vorschaltprozess-Dateitests enthalten. Neuer Werkzeugkatalog nach
Codex-Neustart übernommen.

## Live-Ergebnis

- Ebener Zweiteiler: Lager plus automatisch ergänzter Weg. Ein Run, beide Teile
  nach 4,11 Sekunden / 1,40625 Spielstunden fertig; Pause bestätigt.
- Höhen-Sechsteiler: Lager, zwei Plattformen, Treppe und zwei Wege automatisch
  geplant. Ein Run, alle sechs Teile nach 11,83 Sekunden / 4,0625 Spielstunden
  fertig; keine manuelle Zugangsreparatur oder Advance-Aufrufe pro Teil.
- Beide Lagereingänge separat frei und erreichbar, Distriktdistanz 35. Benachbarter
  Trägerposten und Bauhütte weiterhin erreichbar. Beide Projektledger freigegeben.
- Ziel auf Bestandsweg: `candidateFound=false`,
  `target_intersects_existing_or_planned_object`, null Plan, keine Bauaufträge.
- 48 bzw. 114 Suchknoten. Alle erzeugten Hilfen dienen hier den dauerhaften
  Eingängen/Trägern; leere Rückbaukandidatenlisten sind in diesen Fällen korrekt.

Rückbau: sieben der acht Testobjekte entfernt, ohne `remove_debris`. Die oberen
Rohstoffstapel wurden während begrenzter Sammelläufe abtransportiert. Ein Stapel
der Treppe bei (33,4,4) bleibt erhalten; die letzte Testwegzelle (33,5,4) bleibt
für dessen Sammlung stehen. Nach insgesamt 36 Spielstunden keine weitere
Warteschleife. Bestandswege unverändert, Spiel pausiert. Diese zwei Restobjekte
bei nächster Arbeit frisch prüfen, nur den Weg nach Sammlung entfernen.

Grenzen: kein separater Live-Umwegfall, keine positive temporäre Rückbauliste,
keine 32-Teile-/Acht-Ebenen-Vollabdeckung. Native hypothetische Baustellenreichweite
bleibt unbekannt; erfolgreiche tatsächliche Fertigstellung belegt diese zwei Fälle.

## Ablauf

`plan_large_project_access(session, districtId, steps)` erhält die gewünschten
Gebäude mit Position, Drehung und vorhandenen Abhängigkeiten. Es verschiebt keine
Gebäude und erteilt keine Aufträge. Der Planer ergänzt fehlende Plattformstützen,
Wege und Folktails-Treppen für den dauerhaften Eingang sowie eine angrenzende
Arbeitszelle auf Höhe des Gebäudefundaments. Vorhandene angeschlossene Wege und
gemeinsam benötigte Hilfen werden wiederverwendet. Eigene Wegziele gehören weiter
in den expliziten manuellen Plan.

Die Suche arbeitet gewichtet nach zusätzlichen Teilen im lokalen Graphen; sie
garantiert weder die global kürzeste Gesamtlösung noch vollständige Abdeckung.
Alle erzeugten Teile laufen anschließend durch die vorhandene gemeinsame native
Vorschau einschließlich Bestandszugängen, Stützen und Eingängen. Teilbudgets,
Abhängigkeiten und belegtes Volumen gelten für den erweiterten Gesamtplan.

- `candidateFound=true`: Vorschau hat den begrenzten Entwicklungspilot akzeptiert.
  Zur Ausführung die zurückgegebenen **`steps` und `plan.planKey`** gemeinsam an
  `run_large_project` übergeben. Der Start prüft denselben Plan erneut frisch.
- `candidateFound=false`: `reason` unterscheidet Hindernis, fehlende Stützung,
  fehlende Route im Suchraum, Suchbudget und native Ablehnung. Teilweise erzeugte
  Schritte nicht ausführen; `plan` kann null sein. Keine zweite Suche versteckt
  nachschieben, nachdem ein Budget verbraucht ist.
- `assessment` bündelt angefragte/ergänzte Teile, besuchte Knoten, Suchregion und
  vorgeschlagene Zugänge je Ziel. `plan.roadProtection` enthält den nativen
  Bestandsprüfbericht. Ungeprüfte Ziele fehlen im Teilbericht und sind nicht sicher.
- `cleanupCandidateIndices` benennt nur zusätzlich erzeugte Hilfen, die nicht
  für einen geplanten Eingang oder Gebäudeträger benötigt werden. Nach tatsächlicher
  Fertigstellung Bestandszugänge erneut lesen, dann gegebenenfalls rückbauen.
  Keine automatische Abrissfreigabe; Rohstoffstapel erhalten und einsammeln lassen.

## Grenzen und ehrliche Aussagen

- Höchstens 32 Gesamtteile, 32×32 belegte Grundfläche, acht belegte Höhen.
- Suchregion: Zielvolumen plus sechs Zellen Rand, höchstens sieben Ebenen unter
  der höchsten belegten Zielzelle. Vorhandener angeschlossener Distriktweg darin
  erforderlich. Maximal 2048 Knoten je Route, 4096 insgesamt, fünf Sekunden Suche.
- Die Ziele werden in Abhängigkeitsreihenfolge ergänzt. Eine ungünstige erste Route
  kann spätere Ziele blockieren; kein kombinatorischer Optimierer oder vollständiger
  Umplanungsalgorithmus. Ein nativer Fehlkandidat wird als solcher zurückgegeben.
- Unterstützung nur über öffentliche Vorlagengeometrie, Terrain und vorhandene
  fertige stapelbare Objekte; kein Raten bei erforderlichem Boden oder Spezialformen.
  Zusätzliche Teile nur Path, Stairs.Folktails und Platform.Folktails; gesperrte
  Hilfsvorlagen werden nicht freigeschaltet. Kein Räumen oder Abreißen im Aufruf.
- **Bauarbeiterzugang bleibt bis zur realen Baustelle ungeprüft.** Die angrenzende
  Arbeitszelle ist eine geometrische Planungshilfe. Die native fertige Vorschau
  liefert weiterhin keinen allgemeinen Nachweis des hypothetischen Bauzustands.
  `regularExecutionAllowed=false`; Laufzeitkontrollen und Reparaturverfahren bleiben.
- Gemeinsame Materiallieferung, Besetzung, Kraftbetrieb und Produktion separat prüfen.

## Testgate und Abnahme

Vorbereitete automatisierte Fälle: Umweg um gesperrte Zelle, gewichtete Route,
Höhenwechsel mit Treppen-/Stützenkosten, Teil-/Knotenbudget, bereits angeschlossener
Eingang ohne neue Teile, Schreibgate/HTTP-Methode/Schema, widersprüchliche Antwort
und unerlaubte Veränderung eines Zielgebäudes. Keine Agent-Testläufe.

Nach menschlichem Skriptlauf und `live bereit` drei begrenzte Kontrollen:

1. Ebenes kleines Gebäude mit nötigem Weg, einschließlich Hindernis/Umweg:
   Plan erzeugt erforderliche Wege; ein Run stellt alles fertig, Eingang separat
   zugänglich. Keine Agentenaufrufe je Bauteil.
2. Erhöhtes kleines Gebäude: benötigte Plattformen, Treppe und obere Wege werden
   ergänzt; Bauabhängigkeiten, realer Baustellenzugang, fertiger Eingang und
   benachbarte Zugänge nachweisen. Rückbaukandidaten dürfen keinen dauerhaften
   Eingang oder Gebäudeträger als entbehrlich ausgeben.
3. Ziel über belegtem Bestandsweg: Planung abweisen, keine Aufträge/Verluste,
   ursprüngliche Wege und Zugänge erhalten. Bei fehlerhafter Kontrolle stoppen.

Erfolg: beide positiven Fälle ohne manuelle Reparatur und die negative Kontrolle
korrekt. Bei fehlendem Kandidaten/Blockade gezielt Ursache prüfen, keine breite
Koloniesuche. Danach Testbauten/temporäre Straßen gezielt aufräumen, alle
Rückgewinnungsrohstoffe erhalten. Erst nach erfolgreichem Gate und Livetest committen.
