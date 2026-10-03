# Technische Übergabe

## Stand

Bridge 0.35.0 nach menschlichem Gate live bestanden: generischer ebener Pilot
über Katalog/Geometrie. Bank (ein Feld, zwei Wege) und großes Freiluftlager
(neun Felder, ein Weg), jeweils Drehung 1, fertig und erreichbar. Einzelne
Kollision am gegenüberliegenden Eckfeld des großen Grundrisses nativ abgelehnt;
Hauptweg-Negativkontrolle meldete 24 verlorene Vorschauverbindungen, restauriert.
Bestandszugänge einschließlich oberem Lager erhalten, beide Projekt-Replays
unverändert, 192 Gebäude/Wege am Ende, Simulation pausiert. Zehn exakt bestimmte
Testbüsche regulär entfernt. Installierte DLLs entsprechen den gebauten Dateien.
Öffentliche API liefert einen Gebäude-Eingang; mehrere Bauzugangszellen nicht
damit verwechseln. Direkt-fertig-Distriktzentrum und Umwegfall hier nicht live
geprüft. Bank-/LargePile-Historie beim nächsten ohnehin nötigen MCP-Gate ergänzen;
das ausgelieferte Profil enthält den Stand vor Abnahme.
[Nachweis und Grenzen](generic-building-project.md).

Bridge 0.34.0 nach menschlichem Gate live bestanden: Lodge.Folktails im ebenen
Projektpilot, Belege versionsgebunden und MCP-Historie je Vorlage getrennt.
Rotation 3 mit zwei tatsächlich neuen Bodenwegen vollständig gebaut; korrekter
Vier-Zellen-Grundriss, Bauarbeiterzugang und freier fertiger Eingang, Distanz 50.
Vergleichslager frei/Distanz 53 erhalten, Wege verbunden, Replay unverändert.
Ein exakter Achtstundenlauf, Simulation am Ende pausiert. Kein Abriss nötig.
MCP-Historie enthält noch Stand vor Abnahme; Drehung 3 beim nächsten ohnehin
nötigen Gate aufnehmen. [Details](lodge-project-pilot.md).

Bridge 0.33.1 nach menschlichem Gate live bestanden: vier gemeinsame sequenzielle
Vertikalprojektplätze und aktualisiertes MCP-Profil. Eine Treppe und ein
siebenstufiges Lagerprojekt in derselben Sitzung fertig, Startsperre bei
unfertigen Vorgängern/Schlusslager und altes Replay bestanden. Bau-/Fertigzugang
und ausgewählte Bestandsanschlüsse erhalten. Acht tatsächliche Projektobjekte,
sechs exakte Achtstundenläufe, Simulation abschließend pausiert.
[Details](vertical-sequential-pilot.md).

Historischer Ausgangspunkt vor 0.33.1: Agent Bridge 0.33.0 war installiert und live geprüft.
Der aktuelle Arbeitsbranch ist `codex/road-protection-pilot`. Ein begrenzter
Bauprojekt-Livefall D hat eine neue Treppe, drei Plattformen, zwei obere Wege und
ein kleines Lager vollständig gebaut. Wartephasen, tatsächlicher Baustellenzugang,
fertiger Lagerzugang, ausgewählte Bestandsanschlüsse und Idempotenz sind getrennt
live belegt. A und begrenztes C bestanden; B-Untersuchung abgeschlossen, aber
B jetzt im freigegebenen konservativen Ausschlussumfang geschlossen, allgemeiner
Vorschau-Schutz weiter offen. Details in construction-isolation.md und vertical-warehouse-pilot.md.
Erster E-Schritt auf unveränderter 0.32.1 bestanden: fehlende ebene Lagerdrehungen
0/2 mit tatsächlichem Baustellen-/Fertigzugang und erhaltenem gemeinsamem Weg.
Details und Grenzen in building-rotation-pilot.md; E nicht insgesamt abgeschlossen.

## Nächster Ablauf

Zusätzliche vertikale Treppendrehung 0 auf unveränderter 0.33.0 live bestanden:
sieben fertige Objekte, Lagerbaustelle erreichbar, fertiger Eingang frei mit
Distanz 34; unterer Bestandsweg und obere Wege verbunden, Vergleichslager bei
Distanz 25 erhalten. Replay ohne weitere Objekte. Spiel nach Zeitläufen pausiert.
Anschließend frische Sitzung: Treppendrehung 2 / Lagerdrehung 0 ebenfalls live
bestanden, sieben fertige Objekte, Baustellenzugang und freier Fertigeingang mit
Distanz 36. Bestandsweg verbunden, Vergleichslager bei Distanz 15 erhalten,
Replay unverändert sieben Objekte. Fünf begrenzte Bauzeitläufe jeweils pausiert
und ohne Überschreitung; eine tote Birke entfernt, Plattform regulär freigeschaltet.
Ein Vertikalprojekt im aktuellen Controller pro Sitzung; weitere Drehung braucht
neue Sitzung. Drehung 1 / Lagerdrehung 3 ebenfalls live bestanden: alle sieben
Objekte fertig, Lagerbaustelle erreichbar, freier Fertigeingang bei Distanz 50.
Bestandsweg und obere Wege verbunden, Vergleichslager unverändert bei Distanz 15;
Replay ohne Doppelbauten. Alle vier festen Treppendrehungen begrenzt dokumentiert.
MCP-Nachweiskatalog unverändert mit nur 3;
Späteres Profilupdate zusammen mit begrenzten sequenziellen Vertikalprojekten
als nächste Codefähigkeit prüfen und am menschlichen Gate halten. Keine parallelen
Baustellen oder allgemeine Schutzfreigabe. Details in vertical-warehouse-pilot.md.

MCP-only-Abfrage `inspect_building_capabilities` nach korrigiertem menschlichem
Gate live bestanden. Fünf erwartete Modi, Grenzen und Nachweise korrekt;
state_conflict bei falscher Sitzung, invalid_argument bei Zusatzparameter.
Objektzahl und pausierte Simulation unverändert. Bridge bleibt 0.33.0.
Details in building-capabilities.md. Allgemeiner Weg-/Builder-Vorabnachweis offen;
neue Umfänge erst als abgegrenzte Fähigkeit implementieren und live abnehmen.

0.33.0-Abschluss-Livetest bestanden: mittleres ebenes Lager mit sechs gedrehten
Grundrisszellen, tatsächlicher Baustellen-/Fertigzugang, erhaltene Bestandswege,
negative Grundrisskontrollen und Replay ohne neue Objekte. Keine neue
Vertikalfreigabe. Details in medium-warehouse-pilot.md.
Ergänzend auf unveränderter Installation: Rotation 1 mit drei neuen fertigen,
distriktverbundenen Bodenwegen, erreichbarer Baustelle und fertigem Lager bestanden.
Bestandsweg und Vergleichszugänge erhalten, Vier-Objekt-Replay ohne neue Objekte.
Auch Drehungen 0/2 sequenziell fertig gebaut, sechs korrekte Grundrissfelder,
Baustellen-/Fertigzugang und ursprüngliche Anschlüsse geprüft. Alle vier ebenen
Drehungen des mittleren Lagers begrenzt live belegt.

1. Etappe E aus ../missionsplan.md nur in explizit unterstützten Eigenschaften
   erweitern; Projektumfang ist inzwischen strukturiert ausweisbar,
   keine freie Höhenplanung
   oder Vollschutzbehauptung. Allgemeine B-Vorschaulücke bleibt.
2. Kandidat, Vorschau, Ausführung und Rücklesung weiterhin an dieselbe Session
   und eine neue Aktions-ID binden.
3. Bei Fehlern Ursache beheben; keine unbestätigte Aktion blind wiederholen.

## Wichtige Grenzen

- Die eigene Bridge ist die Laufzeitbasis; Legacy-Adapter sind keine Voraussetzung.
- Aktionen benötigen passende Freigaben in Mod und MCP-Prozess.
- Private Konfiguration bleibt lokal und wird nicht ausgegeben oder eingecheckt.
- Konkrete Spielstände und historische Baupläne sind nicht Teil der Übergabe.
- Allgemeiner Wegschutz ist nicht bewiesen; fertiger Lagerzugang nur in begrenzten
  C/D/E-Fällen, nicht für beliebige Vorlagen und Geometrien.
  Vier-Wege-Entwurf zurückgestellt und lokal separat erhalten, siehe BACKLOG.md.
