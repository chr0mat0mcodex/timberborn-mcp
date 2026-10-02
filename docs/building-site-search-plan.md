# Bauvorhaben mit Anschlussweg: Bauplatzsuche und Wegschutz

Status: Gemeinsamer Bauplan am 2026-10-02 zur Umsetzung freigegeben, noch nicht implementiert.
Wegschutz als **experimentelle Diagnose 0.23.2** in Arbeit, kein vollständiger Schutz. Priorität: **WICHTIG**.
[Aktueller Diagnosevertrag, Bausperre und Pilot](road-protection.md).
Nutzerauftrag vom 2026-09-21. Ergänzt [generisches Bauen](generic-building.md)
und die vorhandenen [Erreichbarkeitsabfragen](logistics.md).

## Gemeinsames Bauvorhaben — beschlossener nächster Ausbau

Gebäude und nötigen Anschlussweg als einen Auftrag anbieten. Der Agent wählt
Gebäude und Suchbereich beziehungsweise einen geprüften Vorschlag; die eigene Mod
ermittelt Position, Drehung, Eingang und Anschlussweg. Der MCP-Server stellt Planung
und Ausführung bereit; Spielprüfungen und kontrollierte Aktionen bleiben in der Mod.
Keine zusätzliche Mod-Abhängigkeit und keine vom Agenten selbst erratenen Wegzellen.

Erste Stufe: ebene reguläre Wege auf vorhandenen tragfähigen Flächen, ohne Abriss,
Rodung, Terraforming, Treppen oder neue Plattformen. Höhenwechsel ausdrücklich als
nicht unterstützt melden; später getrennt erweitern. Vorhandene nutzbare Wegzellen
wiederverwenden. Anschluss muss zu einem tatsächlich verbundenen Netz des gewählten
Distrikts führen; bloße Nachbarschaft zu einem Weg genügt nicht.

1. **Planen:** Begrenzte Kandidatensuche liefert Gebäudeplatz, Drehung, Eingang,
   genaue neue Wegzellen, Anschlussziel, Voraussetzungen und Prüfstatus. Suche
   und Weglänge begrenzen, tatsächliche Abdeckung offenlegen; keine Spieländerung.
2. **Gemeinsam prüfen:** Gebäude und Anschlussweg im Gesamtzustand und in den
   Ausführungsschritten prüfen. Geometrie, Freischaltung, Bauarbeiterzugang,
   späterer Gebäudeeingang und Erhalt bestehender Zugänge getrennt ausweisen.
   Unbekannte Pflichtprüfung verhindert Ausführung. Geplanter Weg allein ist
   weder nutzbarer Umweg noch Nachweis der Baustellen-Erreichbarkeit.
3. **Ausführen:** Gewählten Plan mit Session, Plan-ID und Aktions-ID aufrufen;
   unmittelbar erneut validieren. Neue Wege vom vorhandenen Netz zum Eingang
   regulär setzen, jeweils Ergebnis zurücklesen. Erst nach bestätigtem nutzbarem
   Anschluss und erneuter Schutzprüfung den Gebäudeauftrag erteilen.
4. **Berichten:** Tatsächlich gesetzte Wegzellen/Objekte, Gebäudeauftrag und
   beobachteten Anschluss getrennt zurückgeben. Auftragsannahme, Fertigstellung
   und Betriebsfähigkeit nicht gleichsetzen.

Die Ausführung ist **keine atomare Transaktion**. Bei Fehler, Zustandswechsel oder
unklarem Ergebnis stoppen; gesetzte Wege erhalten und Teilergebnis ausweisen.
Keine automatische Abriss-Rücknahme, kein blindes Wiederholen des Gesamtauftrags.
Wiederholte Aktions-ID darf keine doppelten Aufträge erzeugen. Nach Prozess- oder
Sessionwechsel zuerst tatsächlichen Bestand abgleichen, keine unbelegte Fortsetzung.
Konkrete Toolnamen, Limits und Ergebnisfelder bei der Implementierung festlegen.

Direkte Gebäudeplatzierungen bleiben derselben Anschluss- und Schutzprüfung
unterworfen: vorhandenen Zugang nutzen oder fehlenden Anschluss mit Verweis auf
den gemeinsamen Plan ablehnen. Keine ungeprüfte Umgehung über ältere Pilotaufrufe.
Für Gebäudetypen ohne regulären Eingang nicht stillschweigend einen Eingang
erfinden; ihre Sonderbedingungen separat unterstützen oder explizit ablehnen.

Die Kopplung ersetzt keinen Wegschutz: Ein gut angeschlossenes neues Gebäude
kann andere Zugänge sperren. Der Live-Test 0.23.2 zeigt außerdem, dass
Distriktweg-Zugehörigkeit und Bauarbeiter-Erreichbarkeit verschieden sind.
Diese Nachweislücke bleibt Voraussetzung für sichere Ausführung; bis dahin
bleibt die bestehende Bausperre aktiv.

### Erste Abnahme für das gemeinsame Bauvorhaben

- Freier ebener Platz mit fehlendem Anschluss: geprüfter Plan, regulär gesetzter
  Weg und anschließend genau ein Gebäudeauftrag; Anschluss separat bestätigt.
- Bereits angeschlossener Platz: vorhandenen Weg nutzen, keine doppelten Wege.
- Engpass oder blockierter fremder Eingang: vor Änderungen ablehnen.
- Fehlender tragfähiger Weg, Höhenwechsel oder unklare Baustellen-Erreichbarkeit:
  explizite Ablehnung, kein scheinbar geeigneter ausführbarer Vorschlag.
- Veralteter Plan, Sessionwechsel und Fehler nach einem gesetzten Wegstück:
  kontrollierter Abbruch mit vollständigem Teilergebnis, kein Gebäudeauftrag
  ohne bestätigten Anschluss und keine automatische Rücknahme.
- Wiederholte Aktions-ID: keine doppelten Aktionen. Zunächst kleiner repräsentativer
  Pilot; bei falschen Anschlusszusagen stoppen und Ursache klären.

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
