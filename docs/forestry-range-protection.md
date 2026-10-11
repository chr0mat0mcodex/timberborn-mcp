# Vollständige Waldreichweite und artenbezogener Fällschutz

Stand 2026-10-11: menschliches Gate nach Klassifikationskorrektur und begrenzter
Abschluss-Livetest bestanden. Bestehende 0.37.0, keine Parallelfassung.

## Live-Nachweis

Vollabfrage an vorhandenem Harzsammler: 446 Reichweitenzellen, 16 Teilflächen,
54 interne Leseaufrufe. 18 lebende Kiefern, sieben Kiefernstümpfe, eine tote
Kiefer; 71 lebende Eichen und ein Eichenstumpf, 14 tote Birken. Löwenzahn bleibt
korrekt ausgeschlossen. Vorheriger Pilot hatte Försterkategorie fälschlich als
Baumnachweis benutzt; korrigiert über natives `TreeComponent`/`vegetation.isTree`.
Fehlender Nachweis liefert `forestry_tree_identity_unknown`, keine Teilzählung.

Zwei zuvor unmarkierte lebende Kiefern in getrennten Teilflächen testweise
markiert, Simulation pausiert. Limit eins liefert `forestry_budget_exceeded`;
Rücklesung bestätigt beide Markierungen unverändert. Neuer Auftrag mit Limit
zwei: `completed`, zwei ausgewählt/bestätigt, `readbackConfirmed=true`, 113 interne
Reads. Alle 71 markierten lebenden Eichen und übrigen Fällmarkierungen unverändert.
Gleiche Aktions-ID liefert identischen gespeicherten Beleg. Abschließende frische
Vollzählung bestätigt null markierte lebende Kiefern und unveränderte Gegenkontrolle.
Beide Testmarkierungen damit entfernt; keine Pflanzen oder Rohstoffe gelöscht.
Spieltag 628, Stunde 17,0625 und Tempo null vor/nach Test unverändert.

Client-Katalog nach Backendstart zunächst veraltet; nach Codex-Neustart neue
Werkzeuge verfügbar. Keine generische HTTP-Umgehung. Verlust einer Antwort,
Fremdeingriff, junge Bäume und extreme Budgetgrenzen sind synthetisch abgedeckt,
nicht separat live provoziert. Kein dauerhafter Schutz vor späteren Markierungen.
Interne Seitenaufrufe bleiben Optimierungspotential trotz eines MCP-Schutzaufrufs.

## Umfang

`inspect_forestry(session, workBuildingId, fullRange=true)` untersucht alle
nativen Reichweitenzellen auf allen vorkommenden Höhen. Regionskoordinaten sind
dabei nicht nötig. Der bisherige regionale Modus bleibt Standard und kompatibel.
`coverage` unterscheidet `full_native_range` von `requested_region_only`.

Die kompakte `species`-Übersicht zählt je exakter Baumvorlage lebende Bäume,
abgeerntete Reste, tote und unbekannte Zustände, ausgewachsene lebende Bäume,
aktuell ertragfähige Bäume, bekannten aktuellen Holzertrag, unbekannten Ertrag
und Fällmarkierungen lebender Bäume. Reste haben Vorrang vor dem noch vorhandenen
Growable-/Lebendstatus. Pflanzen mit und ohne Pflanzmarkierung werden erfasst.
Unreife lebende Bäume bleiben für den Schutz sichtbar. Holzertrag ist kein
Transport-, Erntezeit- oder nachhaltiger Produktionsnachweis.

Reichweite wird in disjunkte 8×8-Teilflächen pro Höhe zerlegt; ausschließlich
Baumursprünge innerhalb der tatsächlichen Reichweite zählen. Grenzen:
2048 Reichweitenzellen, 512 Teilflächen, 256 überlappende Objekte pro Teilfläche,
4096 Fällmarkierungen und begrenzte interne Leseaufrufe. Überschreitung oder
unvollständige Seiten liefern keinen Teilerfolg. `nativeReads`, `rangeCells`
und `scannedRegions` machen Aufwand und Abdeckung sichtbar. Die bisherige
Liste mit höchstens 16 Fällkandidaten bleibt ausdrücklich gekürzt; die
Artenzählung wird vollständig über den gewählten Bereich gebildet.

`protect_forestry(session, workBuildingId, template, actionId, maxTrees=256)`
entfernt Fällmarkierungen lebender Bäume genau dieser Vorlage in der gesamten
Reichweite. `maxTrees` liegt zwischen 1 und 512; zu viele Ziele verhindern den
ersten Schreibaufruf. Stümpfe, unbekannter Lebens-/Restzustand, andere Arten,
Baumursprünge außerhalb der Reichweite und bereits zum Abriss markierte Bäume
werden nicht verändert. Pflanzen und Abrissmarkierungen bleiben unverändert.
Es handelt sich um das Entfernen aktueller Fällmarkierungen, keine dauerhafte
Schutzregel gegen spätere menschliche Eingriffe oder neue Bäume.

## Ablauf und Grenzen

Eine MCP-Anfrage übernimmt vollständige Übersicht, Auswahl, Einzelzielprüfung,
reguläre `set-area`-Aufrufe und abschließende Rücklesung. Kein Agentenaufruf je
Baum, keine Simulation und keine neue Mod-API nötig. Vor jedem Eingriff werden
Pause und Baumidentität/Lebendzustand gelesen; die Spielmutation verlangt den
erwarteten Zustand `marked`. Abschließend muss die gesamte Fällmarkierungsmenge
der Ausgangsmenge abzüglich der ausgewählten Zellen entsprechen. Fremde Änderungen
werden gemeldet und niemals zurückgedreht.

Alle Beobachtungen bleiben nicht atomar; ein Mensch kann zwischen Aufrufen
eingreifen. Der Beleg speichert bestätigten Teilfortschritt und das möglicherweise
unbestätigte Ziel vor jedem Schreibaufruf. Gleiche Aktions-ID liest ausschließlich
diesen Beleg; geänderte Parameter werden abgelehnt. Keine automatische Wiederholung
nach Verbindungsabbruch. `completed` plus `readbackConfirmed=true` bestätigt die
Markierungen zum Beobachtungszeitpunkt, keinen dauerhaften Schutz.
Der bestehende Flächen-Aktionsschalter schaltet auch diesen Schreiber frei.

## Regressionen und Pilotverfahren

Regressionen: entfernte/höhere und junge Bäume, Baumstümpfe, andere Arten,
außerhalb liegende Ursprünge, mehrseitige Reichweite, fehlende Reichweite,
Budgetgrenze/unvollständige Seite, Mengenlimit vor Mutation, geänderte Identität,
No-op, Teilfortschritt mit verlorener Antwort, Replay ohne weitere Mutation,
erkannter Fremdeingriff sowie Katalog/Aktionsschalter/Schreibannotation.
Quellcode-/Diffprüfung durch Agent; Build und automatische Tests im menschlichen
Gate. Der begrenzte praktische Nachweis steht oben.

Nach `live bereit`: vollständige Übersicht an einem vorhandenen Arbeitsgebäude
mit mehreren Baumarten prüfen. Wenige geeignete lebende Bäume einer Art sowie
eine andere Art als Gegenkontrolle regulär zum Fällen markieren, pausiert lassen.
Ein Schutzaufruf muss die Zielart über die ganze Reichweite erfassen und deren
Markierungen entfernen; andere Art bleibt markiert. Rücklesung und Replay müssen
dies bestätigen. Anschließend nur die eigens gesetzten Kontrollmarkierungen
zurücknehmen, keine Bäume oder Rohstoffe löschen. Kein Kolonieausbau nötig.
Bei falscher Zählung, Fremdänderung oder unvollständiger Abdeckung stoppen und
diagnostizieren; Erfolg und interne Aufrufzahl knapp dokumentieren.
