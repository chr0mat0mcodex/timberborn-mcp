# 0.53 - Flächenvorprüfung und Sammelpflanzung

## 0.53-Nacharbeiten abgeschlossen

Menschliches Gate und gezielter Livetest am 12. Oktober 2026 bestanden.
Native Ablehnung planting_preview_changed meldet not_executed; anschliessende
frische Vorpruefung bestaetigt unveraenderte Markierungen. Transportunsicherheit
und andere Konflikte bleiben unconfirmed (automatische Gegenproben im Gate).
Compact laesst rein passende Zellbeispiele weg: 1165 statt 2929 JSON-Zeichen,
60,2 Prozent kleiner bei gleichem Schluessel und gleichen Summen. Zusatzblockaden
bleiben sichtbar; Full vollstaendig. Testmarkierung wiederhergestellt, alle acht
Zellen frisch nachgelesen, Endpause bestaetigt. Keine Pflanzen oder Stapel entfernt.



Status: menschliches Gate bestanden, Mod installiert; am 12. Oktober 2026
begrenzt live abgenommen. Werkzeugkatalog nach Codex-Neustart verfuegbar.

## Live-Nachweis

- Native Bridge 0.53.0, Pause vor und nach dem gesamten Pilot.
- Vorpruefung von 32 Zellen trennt Gebaeude, Wege und Vegetation sowie fremde
  und passende Markierungen. Falsche Pflanzenart am Farmhaus und Ziel vier
  Hoehen darueber ausgeschlossen (unbekannte bzw. fehlende Reichweite).
- Zwei Rechtecke mit acht Zellen: zwei zuvor gezielt entfernte Karotten-/Weizen-
  markierungen gesammelt wiederhergestellt; vier passende Markierungen belassen,
  fremdes Feld und Weg ausgeschlossen. Frische Vorschau bestaetigt alle Zellen.
- Kontrollierte Aenderung nach Vorschau fuehrt zu planting_preview_changed.
  Frische Lesung belegt: entfernte Markierung blieb entfernt, kein Neuauftrag.
  Historischer erfolgreicher Beleg bleibt unveraendert lesbar; kein Apply-Replay
  live provoziert (Idempotenz und Teilfehler durch automatische Gate-Tests).
- Compact/Full gleiche Summen; 2372/2880 JSON-Zeichen der Datenobjekte.
- Testaenderungen vollstaendig wiederhergestellt und frisch nachgelesen. Keine
  Pflanzen, Wege oder Rohstoffstapel entfernt. Kein Wachstum oder Ertrag getestet.
- Acht Rechtecke/512 Zellen, Teilfehler und Sessionwechsel automatisch getestet;
  keine pauschale Liveabnahme aller Pflanzen, Fraktionen oder Aquakultur.
- 0.52-Nacharbeiten durch menschliches Gate geprueft; kein weiterer Raeumbaupilot.
- Die damals offenen Nacharbeiten sind inzwischen abgenommen (Nachweis oben).

## Vertrag

- `preview_planting_batch(session, jobId, rectangles, detail)` liest pausiert.
- `apply_planting_batch(session, jobId, rectangles, key, detail)` beauftragt einmal.
- `inspect_planting_batch(session, jobId, detail)` liest den historischen Beleg.

Kennungen sind nichtleere kanonische UUIDs. Ein bis acht Rechtecke, jedes maximal
8×8 auf einer Höhe, zusammen maximal 512 Zellen. Überlappende Zellen abweisen,
auch bei gleicher Pflanzenwahl. Unterschiedliche Höhen sind getrennte Zellen.
Je Rechteck: x/y/z, width/height, resource, workBuildingId; requireMoist optional
und standardmäßig true. Alle Koordinaten innerhalb 0–4095, einschließlich Enden.
Pflanzen stammen aus inspect_area_types, keine Fraktions-Namensliste.

Eine native Vorprüfung für die gesamte Charge auf dem Spielthread:

- Regulärer PlantingAreaValidator.CanPlant; false ist native Ablehnung,
  deren interne Einzelursache nicht erfunden wird.
- Fertiges Arbeitsgebäude mit PlanterBuilding und der gewünschten Pflanze in
  AllowedPlantables. Seine BuildingTerrainRange ist der Reichweitenbeleg;
  bloße Anzeige-/Effektreichweiten sind kein Ersatz. Fehlender Beleg ist unknown.
- Bodenfeuchte über den bestehenden SoilMoisture-Service. requireMoist=false
  lässt trockene oder nicht anwendbare/unbekannte Feuchte zu; native Pflanzprüfung
  und Reichweite bleiben Pflicht. Das ist insbesondere keine Aquakultur-Garantie.
- Aktuelle Pflanzmarkierung: passende bleibt already_marked; fremde wird
  ausgeschlossen und nicht überschrieben. Pflanzen werden nicht entfernt.
- Belegung an der angefragten Rasterzelle mit ID, Vorlage und Art: Weg, Gebäude,
  Vegetation, Rohstoffstapel oder sonstiges Objekt. Kontext, keine erfundene
  native Ablehnungsursache; keine vollständige Grundrissdiagnose der Pflanze.
  Stapel werden niemals gelöscht; native Pflanzprüfung bleibt maßgeblich.

Reichweite je Arbeitsgebäude einmal beobachten, maximal 65.536 Rangezellen.
Maximal acht Belegungsobjekte pro Zielzelle; Überschreitung stoppt die Vorprüfung
statt still Belege abzuschneiden. Transportantwort maximal 2 MiB. Kein neuer
Hintergrundprozess, keine Einzel-HTTP-Aufrufe pro Pflanzzelle.

## Ausführung und Fehler

Vorschau-key bindet Sitzung, jobId, Rechtecke und sämtliche Zellbelege. Apply
liest alles frisch auf dem Spielthread und verlangt denselben Schlüssel vor
der ersten Mutation. Änderungen führen zur Ablehnung, nicht zum stillen Wechsel
der freigegebenen Zellmenge. Ausführung nutzt PlantingService.SetPlantingCoordinates
und liest die jeweilige Markierung zurück. Alle anderen Zellen bleiben unberührt.

Native Sitzungshistorie hält maximal 128 Aufträge. Gleiche jobId mit gleichen
Parametern liefert den alten Bericht, kein zweiter Auftrag. Abweichende Parameter
werden abgewiesen. Ein Teilfehler stoppt sofort mit unconfirmed; einzelne Zellen
bleiben marked/unconfirmed/not_attempted unterscheidbar. Keine Rücknahme oder
automatische Wiederholung. Historie überlebt MCP-Neustart, nicht Spielneuladen.
Aktuelle Sessionpflicht verhindert Replay alter Sitzungsaufträge.

Apply benötigt den bestehenden Flächen-Aktionsschalter in MCP und Mod; Preview
und Status sind Leser. Eine aktive Räum-/Baugruppe sperrt parallele Pflanzaktionen.
Keine automatische Änderung von Tempo, Personal, Arbeitszeiten oder Gebäude-Pause.
Arbeitsreichweite garantiert weder Arbeiter noch die tatsächliche Ausführung.

Compact nennt Summen, Gründe und bis acht ausgeschlossene Zellbeispiele samt
Belegungsarten, mit expliziter Abschneideanzeige. detail=full enthält alle Zellen.
Belegter Erfolg ist eine Pflanzmarkierung, kein Wachstum und kein Ernteertrag.

## Kleine 0.52-Nacharbeiten

Feste Argumenthinweise für UUIDs, Rechteckgrenzen, Schlüssel, Parameterkonflikte
und Detailwahl; keine beliebigen Exceptiontexte oder privaten Pfade ausgeben.
Strukturfehler der neuen Pflanz- und Räumwerkzeuge werden als Argumentfehler
normalisiert. Bestehender Vertrag not_executed/unconfirmed bleibt erhalten.
Kompakte Räumgruppenberichte wenden die vorhandene Laufprojektion auch auf
clearanceRun an: leere Produktionsfelder entfallen, Full bleibt unverändert.
Die veralteten Codex-Werkzeugkataloge (B12) sind damit nicht behoben.

## Vorbereitete Tests und Livepilot

Automatische Regressionen: acht Rechtecke/512 Zellen, reine Vorschau, ein frischer
Gesamtcheck vor Mutation, Ausschlüsse, fremde/passende Markierungen, Feuchte und
unbekannte Reichweite, geänderte Vorschau, Teilfehler ohne Replay, Sitzungswechsel,
Grenzen/Überlappung, Ein-/Ausgangsvertrag, Aktionsschalter, Werkzeugkatalog,
Compact/Full, konkrete Fehler ohne Transport und kompakter 0.52-Räumbericht.
Versionsprofile um 0.53 ergänzen; unbekannte Testversion bleibt synthetisch 99.0.0.
Agent führt keine Tests, Builds oder Installation aus.

Nach menschlichem Gate:

1. Zwei kleine Rechtecke mit geeigneten und blockierten Zellen prüfen, darunter
   Bestandsweg oder fremde Pflanzmarkierung. Gründe und Belegungsart kontrollieren.
2. Ungeeignetes Arbeitsgebäude oder Ziel außerhalb seiner Reichweite als
   Negativkontrolle: keine geeigneten Zellen behaupten, keine dortigen Markierungen.
3. Gemischte Charge einmal ausführen; geeignete Markierungen frisch nachlesen,
   ausgeschlossene unverändert. Gleiche jobId nur als alten Beleg lesen.
4. Eine Vorschau durch gezielte Markierungsänderung veralten lassen: Apply muss
   vor jeder neuen Markierung ablehnen. Anschließend Testmarkierungen aufräumen.
5. Compact/Full vergleichen. Einen kleinen geeigneten Pflanzauftrag optional mit
   begrenztem Simulationslauf begleiten; echte Pflanzen separat beobachten.
   Wenn Personal/Arbeitszeit fehlen, keine Koloniepflege erzwingen: fehlenden
   Wachstumsnachweis getrennt nennen. Bei falscher Negativkontrolle sofort stoppen.

Kein erneuter umfassender Baupilot; Endpause und keine Löschung von Rohstoffstapeln.

## Gate-Korrektur

Erster menschlicher Lauf brach bei der MCP-Kompilierung mit CS0103 ab:
Eine zu breit angewendete Quelltextersetzung hatte die Argumentprüfung auch
in Catalog eingefügt, obwohl nur Invoke Argumente erhält. Fremde Prüfung aus
Catalog entfernt; Invoke behält sie. Bestehende Katalog-/Argumenttests bleiben
vorbereitet. Quell-/Diffprüfung erfolgt, erneuter Compiler-/Testnachweis offen.
Keine Paketierung, Installation oder Liveabnahme aus diesem Gate ableiten.

Zweiter Lauf: MCP-/Testkompilierung bestanden, Unit-Gate beim Aufbau des
Werkzeugkatalogs abgebrochen. Dem generierten Eingabeschema fehlte die explizite
MCP-Wurzel `type: object`; Referenztypen werden vom Exporter nullable beschrieben.
Wie bei den vorhandenen Räumwerkzeugen jetzt auf object normalisiert.
Gezielte Regression prüft alle drei Pflanzwerkzeuge, mit/ohne Aktionsschalter,
Wurzeltyp, Detailstandard und Rechteckgrenzen. Erneutes menschliches Gate offen;
keine Agent-Testausführung oder Abnahme.

Dritter Lauf: Kompilierung bestanden; bestehender Fällschutz-Positivtest meldete
`stopped` statt `completed`. Der bisherige Assertion-Text enthielt keinen
Abbruchgrund; der temporäre Beleg wird beim Testende entfernt. Ursache daher
noch offen, insbesondere kein belegter Zusammenhang mit der Sammelpflanzung.
Fällschutz meldet abgefangene Fehler jetzt mit Phase, fester Fehlerklasse und
HResult ohne private Exception-Texte. Der Positivtest zeigt bei Abweichung seinen
synthetischen Beleg und die Schreibanzahl. Eine zusätzliche Gegenprobe injiziert
einen Rücklesefehler nach zwei bestätigten Änderungen und prüft Diagnose,
Datenschutz und ausbleibende Wiederholung. Erfolgserwartung unverändert;
keine automatischen Wiederholungen ergänzt. Nur statisch geprüft, erneuter
menschlicher Testnachweis erforderlich. Ein grüner Wiederholungslauf allein
beweist keine Behebung der bisher unbekannten Ursache.
