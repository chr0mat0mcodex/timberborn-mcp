# Wichtige Future-Features: Bauplatzsuche und Schutz der Wegverbindungen

Status: **geplant, nicht implementiert**. Priorität: **WICHTIG**.
Nutzerauftrag vom 2026-09-21. Ergänzt [generisches Bauen](generic-building.md)
und die vorhandenen [Erreichbarkeitsabfragen](logistics.md).

## Bauplatzvorschläge innerhalb eines Suchbereichs

Der Agent soll fragen können: „In diesem Bereich möchte ich Gebäude X bauen.
Welche konkreten Plätze funktionieren?“ Das MCP soll die Suche und Prüfung
übernehmen, statt den Agenten Koordinaten und Drehungen ausprobieren zu lassen.

- Eingabe: Gebäudevorlage, begrenzter XY-Suchbereich, zulässige Höhen/Drehungen,
  gewünschter Distrikt beziehungsweise Anschluss und maximale Anzahl Vorschläge.
  Festlegen, ob der gesamte Gebäudegrundriss im Suchbereich liegen muss; Standard: ja.
- Ausgabe: priorisierte, einzeln geprüfte Optionen mit Ursprung XYZ, Drehung,
  belegten Zellen, Eingang, erreichbarem Anschluss und nachvollziehbarer Begründung.
  Vorhandene Spielvalidatoren einbeziehen; reine Geometriefilter reichen nicht.
- Gelände, Höhenunterschiede, tragende Flächen, Hindernisse, Freischaltung,
  Baumeisterzugang und spätere Gebäudeanbindung getrennt ausweisen. Bei speziellen
  Gebäuden relevante Bedingungen wie Uferlage prüfen oder ausdrücklich als ungeprüft
  kennzeichnen. Keine pauschale Betriebs-/Produktionsgarantie aus einem gültigen Bauplatz.
- **Sofort geeignete Plätze** klar von **erst nach Vorarbeiten geeigneten Optionen**
  trennen. Letztere nennen exakte Voraussetzungen: etwa Bäume entfernen, Weg bauen,
  Treppe/Plattform ergänzen oder Vorlage freischalten. Keine stillen Abrisse, Rodungen
  oder zusätzlichen Bauaufträge während der Suche.
- Wenn ein Anschluss fehlt, konkrete geprüfte Weg-/Treppenoptionen als Vorarbeiten
  liefern, soweit unterstützt. Eine benachbarte Wegzelle allein beweist keine Verbindung.
- Begrenzte Suche mit offengelegter Abdeckung, Anzahl geprüfter Kandidaten und
  Abbruchgrund. „Kein Platz gefunden“ von „Suche unvollständig/Prüfung nicht unterstützt“
  unterscheiden; keine unbegrenzte Kombination aller Koordinaten und Drehungen.
- Vorschläge an Session und beobachteten Zustand binden. Gewählten Platz unmittelbar
  vor Ausführung erneut prüfen; veraltete Optionen nachvollziehbar ablehnen.
  Die Zusage gilt für den geprüften Zustand, nicht für spätere ungeprüfte Änderungen.

## Bauaufträge dürfen die Erschließung nicht vollständig versperren

Diese Prüfung gehört standardmäßig in den ausführenden MCP-/Mod-Baupfad, nicht nur
als optionaler Hinweis in die Bauplatzsuche. Auch direkte Platzierungen müssen sie nutzen.

- Vor einem Auftrag prüfen, ob das Bauwerk beziehungsweise schon seine Baustelle
  die letzte nutzbare Wegverbindung abschneidet, einen Zugang blockiert oder bisher
  erreichbare Gebäude/Bereiche vom vorgesehenen Anschluss trennt.
- Bestehende Verbindungen vor/nach der geplanten Änderung vergleichen. Alternative
  Routen, Treppen, Brücken, Höhenebenen und Eingänge berücksichtigen; ein bloßes
  XY-Nachbarschaftsraster ist kein ausreichender Nachweis.
- **Vollständige Wegversperrung standardmäßig verweigern**, bevor ein Bauauftrag
  erzeugt wird. Antwort mit maschinenlesbarem Grund, verständlicher Erklärung,
  Konfliktzellen und betroffenen Gebäude-/Zugangsreferenzen; soweit möglich sichere
  Alternativposition oder benötigten Umweg nennen.
- Unbekannte Erreichbarkeit nicht als sicher behandeln. Bei nicht zuverlässig
  prüfbarer Auswirkung den Auftrag mit eigener Diagnose ablehnen, statt eine
  bestätigte Wegversperrung vorzutäuschen oder ungeprüft zu bauen.
- In Bauchargen jede Änderung gegen den inzwischen aktuellen Zustand prüfen.
  Ein nur geplanter Umweg zählt erst nach Fertigstellung und bestätigter Nutzbarkeit.
- Die vorausschauende Prüfung darf selbst keine realen Wege/Gebäude entfernen oder
  Probeaufträge mit anschließendem Rollback erzeugen. Geeignete öffentliche APIs
  für eine Prüfung des hypothetischen Zustands sind vor der Umsetzung zu untersuchen.

## Geplante Abnahme

- Suchbereich mit mehreren Drehungen, Hang, Ufer, gestapelten Flächen, Hindernissen,
  fehlendem Anschluss und keinem gültigen Platz: Optionen und Ablehnungen nachvollziehbar.
- Repräsentative als sofort geeignet gemeldete Optionen anschließend regulär bauen;
  Auftragsannahme, Fertigstellung und tatsächliche Zugänglichkeit getrennt bestätigen.
- Engpass ohne Umweg, blockierter Gebäudeeingang und Sperre erst durch Fertigstellung:
  Auftrag abgelehnt, Ursache und betroffene Ziele benannt, Spielbestand unverändert.
- Nutzbarer alternativer Weg: keine fälschliche vollständige Sperre melden.
  Erst geplanter/noch unfertiger Umweg darf eine Ablehnung nicht umgehen.
- Veralteter Vorschlag, Sessionwechsel, Navigation noch nicht aktualisiert und
  nicht unterstützte Geometrie: expliziter Status, keine falsche Sicherheitszusage.

Vorhandene Einzelplatzierung und Erreichbarkeitsleser sind Grundlagen. Eine
Bereichssuche und der vorausschauende Schutz vor Wegversperrung sind damit noch
nicht implementiert oder live nachgewiesen.
