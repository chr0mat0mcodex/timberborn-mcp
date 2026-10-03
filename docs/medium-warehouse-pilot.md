# E: mittleres ebenes Lager

Stand: 2026-10-03. 0.33.0 nach menschlichem Skript-Gate installiert und gezielt
live bestanden. Bereitmeldung ohne neue automatische Testzählung.

## Änderung

Der bestehende ebene Gebäudeprojekt-Pilot unterstützt neben
`SmallWarehouse.Folktails` ausdrücklich `MediumWarehouse.Folktails`.
Aktiver Spielkatalog: 3×2×1, lokaler Eingang (1,-1,0), normales Einzellayout,
freigeschaltet. Keine neue API, kein Eigenbau von Gebäudegeometrie: bestehende
Vorprüfung und Spielvorschau nutzen die gedrehten vollständigen Spielblöcke.

Gemeinsame Vorlagenliste in Anfrage und Auftragsledger; Belegprüfung bindet das
letzte Objekt zusätzlich an die angefragte Vorlage. Mittlere Lagerbelege werden
nur ab der neu zugelassenen Bridge 0.33.0 akzeptiert. Alte kleine Lager bleiben
kompatibel. Statuslesung prüft ebenfalls die begrenzte Vorlagenliste.

Unverändert: höchstens vier neue Bodenwege, Suchregion maximal 8×8, kompletter
Grundriss und Route innerhalb der Region, normale Spielvalidatoren,
Bestandsverbindungsprüfung, B-Baustellensperre und reale Zugangsrücklesung.
Vertikaler Lagerpilot bleibt auf das kleine Lager beschränkt.
`constructionCovered=false` / Vorabnachweis unknown werden nicht gelockert.

## Abnahmeumfang nach menschlichem Skript-Gate

1. Negativkontrolle am vollständigen Grundriss: freie Ursprungszelle reicht nicht;
   ein Hindernis an einer entfernten Grundrisszelle oder zu kleine Suchregion
   muss den betroffenen Kandidaten ausschließen. Keine Bauobjekte dabei erzeugen.
2. Ein positiver ebener Auftrag für das mittlere Lager über den regulären
   Plan-/Vorschau-/Pilotablauf; bevorzugt eine Drehung mit vertauschten
   Grundrissachsen. Neue Wege nur bei tatsächlich erforderlichem Anschluss.
3. Baustelle separat rücklesen: korrekte Vorlage/Rotation und vollständige
   belegte Zellen, tatsächlicher Bauarbeiterzugang und Eingangsverbindung.
4. Begrenzter Simulationslauf, anschließend Fertigzustand, freier Eingang und
   Distriktanschluss prüfen. Ausgewählte Bestandswege und Vergleichszugang vor/
   nach dem Auftrag vergleichen.
5. Identische Aktions-ID liefert denselben Beleg ohne neue Objekte.

Bei falscher Geometrie, verlorener Bestandsverbindung, fehlendem Zugang oder
unbestätigter Aktion stoppen und diagnostizieren; keine Erweiterung auf weitere
Vorlagen. Materialmangel ist kein erfolgreicher Fertigstellungsnachweis.

Synthetische Tests im menschlichen Skript-Gate: vier Drehungen im Belegvertrag, falsche Vorlage,
alte Bridge, Statuslesung, kompatibler Altbeleg, Weglimit, Ledger/Idempotenz und
Ausschluss aus der festen vertikalen Sequenz. Neue Testzählung nicht übermittelt.

## Konkretes Liveergebnis

- Freie Ursprungszelle mit Hindernissen auf entfernten Grundrisszellen:
  Vorprüfung blocked, begrenzte Planung ohne Kandidat. Objektzahl unverändert.
- Nach regulärer Entfernung von einem Wohnhaus und zwei Birken war dieselbe
  Testfläche frei. Zu kurze Suchregion weiterhin ohne Kandidat; vollständige
  Region liefert genau einen passenden Plan.
- Gemeinsame Spielvorschau: alle sechs gedrehten Lagerzellen erfasst, Gebäude
  gültig, Eingang verbunden, keine verlorenen geprüften Distriktverbindungen,
  Wiederherstellung bestätigt. Gesamtbewertung weiterhin unknown, keine reguläre
  safe-Freigabe und kein Bauarbeiter-Vorabnachweis.
- Ein Lager mit Rotation 3 (Cw270), gedrehtem 2×3-Grundriss, ohne neue Wege
  platziert. Reales Objekt mit genau sechs Zellen und korrektem Eingang gelesen.
- Baustelle builder-erreichbar, Eingang frei, native Distriktdistanz 25.
- Nach begrenztem Simulationslauf fertig zurückgelesen, Distrikt zugeordnet,
  Eingang frei und Distanz weiterhin 25. Lauf abgeschlossen, Pause bestätigt.
- Zwei ausgewählte ursprüngliche Bodenwege vorher/nachher distriktverbunden;
  Vergleichslager unverändert bei Distanz 19 und freiem Eingang.
- Identische Aktions-ID liefert denselben completed-Beleg mit einem Objekt;
  globale Objektzahl beim Replay unverändert. Kein Doppelauftrag.

Dieser E-Schritt ist bestanden, E insgesamt nicht abgeschlossen. Nächster kleiner
Schritt: mittleres Lager mit tatsächlich neuem Bodenanschluss abnehmen. Die
übrigen Drehungen dieser Vorlage und vertikale Mehrzellengebäude bleiben offen.

Keine Zusage für alle Drehungen live, andere Vorlagen, freie Höhenplanung,
allgemeinen Weg-/Builder-Vorschau-Schutz, Warenversorgung oder Lagerbetrieb.
