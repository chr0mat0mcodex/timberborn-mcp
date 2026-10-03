# E: ebenes Wohnhausprojekt — 0.34.0, live bestanden

Stand: 2026-10-03. Nach menschlicher Bereitmeldung zum Skript-Gate installiert
und feature-spezifisch live bestanden; Bridge 0.34.0 strukturiert bestätigt.
Neue automatische Testzählung nicht übermittelt. Keine allgemeinen
Schutzfreigaben oder Agentenregeln geändert.

## Abgegrenzte Fähigkeit

Der bestehende ebene development_pilot unterstützt zusätzlich Lodge.Folktails.
Reguläre Spielgeometrie liefert vier Grundrisszellen (2×2) und einen versetzten,
mitgedrehten Eingang. Kein eigenes Gebäuderaster oder neuer Platzierungsweg.
Die bestehende Planung und gemeinsame Vorschau prüfen den gesamten Grundriss
und Anschluss; jede Mutation prüft erneut, danach realen Baustellenzugang.
Vorläufige Geometriebeobachtung in der laufenden 0.33.1 erfolgte rein lesend,
kein Wohnhausbau vor dem neuen Gate und kein Freigabenachweis daraus.

Unverändert: 8×8-Suchregion, maximal vier neue Bodenwege und fünf Schritte,
vier sequenzielle ebene Projekt-IDs, Sitzungsbindung, Replay-Schutz,
unabhängige Baustellensperre und Bestandsverbindungsprüfung. Kein vertikales
Wohnhaus, keine größeren Wohnhausvarianten, kein automatischer Einzugstest.
constructionPreflightProven bleibt false. completed bleibt Auftrag plus Zugang,
nicht fertiges oder bewohntes Gebäude.

Wohnhaus-Ausführungsbelege werden ausdrücklich nur mit Bridge 0.34.0 akzeptiert,
auch bei Statuslesung. Bisherige Lager und Vertikalmodi bleiben kompatibel.
MCP-Profil nur für exakt zugeordnete Version/Fraktion: flat.TemplateEvidence
nennt die historischen Nachweise je Gebäudevorlage. Das neue Wohnhaus erhält
keine Lager-Drehungsnachweise; LiveRotations auf Modusebene bezeichnet gemeinsame
Abdeckung aller Gebäudevorlagen und bleibt bei gemischter Abdeckung leer.
Technische Drehungen 0..3 sind nicht mit bestandenen Livefällen gleichzusetzen.

## Vorbereitete automatische Prüfung

Wohnhausanfrage, Vorlagen-/Drehungs-/Sitzungs-/Regionsbindung, alte Versionen
auch im Statusweg, keine behauptete Vorab-Sicherheit, geordnete vier Wege,
Replay ohne erneute Platzierung und Ablehnung im festen Vertikalprojekt.
Bestehende Negativtests für ausgeschlossene Vorlagen angepasst; Lagerbelege,
Vertikalbelege, Log-Leser und Profilrouting mit 0.34.0 zusätzlich abgedeckt.
Keine automatischen Tests oder Builds durch den Agenten gestartet.

## Gezielte Live-Abnahme nach menschlichem Gate

1. Bridge 0.34.0 und Folktails bestätigen; Katalog-Vorlage, tatsächliche Kosten,
   Freischaltung und gedrehten Eingang lesen. MCP-Profil zeigt Wohnhaus nur eben
   und keine übertragenen historischen Lager-Nachweise.
2. Negative Grundrisskontrolle: freie Ursprungszelle bei belegter anderer Zelle
   bzw. zu kleiner Suchregion darf keinen passenden Kandidaten ergeben.
   Kandidaten und ihre Ursprünge prüfen, nicht zwingend den ganzen Suchraum leer
   erwarten. Keine Objekte durch Plan/Vorschau erzeugen.
3. Ein gedrehtes Wohnhaus über Plan → gemeinsame Spielvorschau → Pilot bauen,
   möglichst mit mindestens einem tatsächlich neuen Anschlussweg.
4. Vier belegte Grundrisszellen, richtige Vorlage/Drehung und tatsächlichen
   Bauarbeiterzugang getrennt rücklesen. Nach begrenztem Zeitlauf finished=true,
   freien Eingang und Distriktanschluss nachweisen. Kein Einzug/Betrieb behaupten.
5. Ausgewählte bestehende Wege und Vergleichsgebäude vor/nach kontrollieren;
   identische Aktions-ID ohne Doppelbauten, Simulation abschließend pausiert.

Ein positiver vollständiger Fall plus gezielte Negativkontrolle reicht für
diesen Schritt; keine Vier-Drehungs-Serie. Bei unbestätigter Aktion zunächst
diagnostizieren. Bei ungeeigneter Fläche nur einen begrenzten Testaufbau anpassen.
Commit/Push erst nach menschlichem Skriptlauf und bestandener Live-Abnahme.

## Bestandene Live-Abnahme

- Katalog: Folktails, Lodge verfügbar/freigeschaltet, normales Einzellayout,
  Größe 2×2×1, lokaler Eingang (1,-1,0), Kosten zwölf Holzstämme.
- Grundriss-Negativkontrolle: freie Ursprungszelle und bereits verbundener
  Eingang genügen nicht; zwei weitere belegte Grundrisszellen führten zu
  object_intersection und Ausschluss des Kandidaten. Eine 1×1-Suchregion um
  den freien positiven Ursprung lieferte ebenfalls keinen Kandidaten.
- Vollständige 5×3-Suche: 15 Ursprünge geprüft, 13 verworfen, zwei Kandidaten.
  Gewähltes Haus in Drehung 3 mit versetztem Eingang benötigt zwei neue Wege.
  Gemeinsame Vorschau: Haus und beide Wege gültig, Eingang verbunden, keine
  verlorenen geprüften Bestandsverbindungen, Wiederherstellung bestätigt.
  Bauphasensicherheit weiterhin unknown; vor/nach Planung/Vorschau elf
  tatsächliche Objekte im Testbereich, keine Bauaufträge durch diese Prüfungen.
- Regulärer Pilot: zwei neue Bodenwege fertig und verbunden; vier vollständige
  Hauszellen, Cw270, richtiger Eingang rückgelesen. Baustelle separat
  buildersReachable=true, constructionAccess observed und Baudistrikt zugeordnet,
  Eingang frei, Distanz 50. Anschließend completed-Auftragsbeleg gelesen.
- Ein begrenzter Achtstundenlauf, Ziel erreicht mit overshootHours=0 und Pause.
  Haus danach tatsächlich finished=true, Eingang frei, Distriktzuordnung und
  Distanz 50 bestätigt. Einzug/Belegung nicht geprüft oder zugesagt.
- Beide neuen Wege, unterer Bestandsweg und ausgewählter oberer Bestandsweg
  verbunden; Vergleichslager vor/nach frei, Distanz 53. Kein Guard-Abbruch.
  Kein Abriss oder sonstiger Testflächenumbau für diesen Livefall erforderlich.
- Identischer Auftrag liefert alten dreistufigen completed-Beleg. Projektbereich
  vor/nach Replay dieselben 14 Objekt-IDs; keine Doppelbauten oder Spielzeitänderung.
- Versioniertes MCP-Profil korrekt gelesen: Wohnhaus nur im ebenen Modus,
  Lagerhistorie je Vorlage getrennt, keine allgemeine Baufreigabe.

Grenze: ein gedrehter positiver Wohnhausfall und gezielte Grundrisskontrollen,
kein vollständiger Weg-/Builder-Vorabnachweis und keine weiteren Wohnhausdrehungen.
Das ausgelieferte MCP-Historienprofil beschreibt den Stand vor dieser Abnahme
und meldet Lodge noch not_live_proven. Dokumentierter Nachweis ist jetzt Drehung 3;
die reine Historienaktualisierung mit dem nächsten ohnehin nötigen MCP-Gate bündeln,
keine Neuinstallation allein für diesen Metadateneintrag.
