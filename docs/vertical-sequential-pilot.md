# Sequenzielle Vertikalprojekte — 0.33.1, live bestanden

Stand: 2026-10-03. Nach menschlicher Bereitmeldung zum Skript-Gate installiert
und feature-spezifisch live bestanden. Bridge 0.33.1 strukturiert bestätigt;
eine neue automatische Testanzahl wurde nicht übermittelt.

## Änderung und Grenze

Der bestehende sitzungslokale Projektcontroller erhält vier statt eines
Ledger-Platzes. Alle vier Vertikalmodi teilen diese vier Projekt-IDs; nicht vier
pro Modus. Geometrie, reguläre Spielvalidierung, Zugangs- und Wegschutzprüfungen
bleiben unverändert. Jede neue ID benötigt frische Prüfungen und eine pausierte
Simulation. Ein vorheriger Auftrag muss completed sein und alle tatsächlichen
Baustellen müssen fertig sein, auch dessen letztes Lager. completed allein
bedeutet weiterhin nur bestätigten Auftrag und Zugang.

Laufende/wartende, gestoppte oder unbestätigte Projekte sperren weitere neue IDs.
Identische alte ID und Parameter liefern nur den gespeicherten Beleg, ohne neue
Bauten oder Austausch des aktiven Projekts. Geänderte Parameter zur selben ID
und eine fünfte neue ID werden abgewiesen. Sitzungsende verwirft das Ledger;
keine Persistenz, Parallelprojekte oder automatische Wiederholungen.

Das MCP-Profil ordnet 0.33.1 ausdrücklich zu und nennt vier gemeinsame Plätze.
Historische Drehungen 0..3 gelten für das feste siebenstufige kleine Lagerprojekt;
sie werden nicht auf andere Modi oder beliebige Bauplätze übertragen.
0.33.0 behält im Profil eine vertikale ID. Allgemeine Baufreigabe und
Builder-Vorabnachweis bleiben false. Backend-Versionslisten und Log-Leser
akzeptieren 0.33.1; keine Protokoll- oder Aktionsschemaänderung.

## Automatische Abdeckung und Live-Nachweisplan

Synthetische Tests: vier gemischte Modi mit gemeinsamem Budget, Ablehnung
während eines aktiven/wartenden Projekts, alte Belege während eines Nachfolgers,
kein Replay-Doppelbau, fünfte ID, Parameterkonflikt, stopped/unconfirmed,
frische Sitzung sowie Trennung von Ledger- und Baustellenbereitschaft.
Profil-/Routing- und Lagerbelegtests decken 0.33.1 zusätzlich ab.

Nach erfolgreichem menschlichem Skript-Gate zwei Vertikalprojekte in derselben
geladenen Sitzung prüfen, davon mindestens ein siebenstufiges Lagerprojekt:

1. Frisches MCP-Profil: 0.33.1, vier gemeinsame Plätze, Lagerdrehungen 0..3
   historisch; keine allgemeine Sicherheitsfreigabe.
2. Erste neue ID ausführen. Zweite neue ID während laufendem Auftrag bzw. nach
   completed bei noch unfertigem Schlussobjekt ohne zusätzliche Bauten abweisen.
3. Tatsächliche Fertigstellung kontrollieren, dann zweite ID ohne Neuladen
   annehmen. Altes Ergebnis während/nach dem zweiten Projekt abfragen und
   identisch wiederholen: keine Doppelbauten, kein Wechsel des aktiven Auftrags.
4. Reale Bau-/Fertigzugänge und ausgewählte Bestandsanschlüsse kontrollieren;
   Simulation am Ende pausieren. Vier-/Fünft-ID-Grenze synthetisch prüfen,
   nicht vier aufwendige Liveprojekte nur zur Zählkontrolle bauen.

Bei unbestätigter Aktion zuerst diagnostizieren, nicht blind wiederholen.
Bei fehlender Infrastruktur oder Zeitlimit Testaufbau gezielt anpassen;
kein autonomer Kolonieaufbau. Commit/Push erst nach beiden bestandenen Gates.

## Bestandene Live-Abnahme

- Eine einzelne Treppe, danach ein siebenstufiges Lagerprojekt in derselben
  Sitzung ohne Neuladen angenommen. Alle acht tatsächlichen Objekte fertig.
  Treppendrehung jeweils 1, daraus Lagerdrehung 3; kein zusätzlicher Drehungsnachweis.
- Zweiter Start vor Fertigstellung der ersten Treppe mit state_conflict abgewiesen,
  obwohl ihr Beleg bereits completed war. Nur ein Objekt im Testbereich.
  Weitere neue ID nach completed des Lagerauftrags bei unfertigem Schlusslager
  ebenfalls abgewiesen. Veränderte Parameter zur alten ID abgewiesen.
- Alter Treppenbeleg während des aktiven Lagerprojekts identisch abrufbar;
  das Lagerprojekt blieb aktiv und setzte seine sieben Schritte fort.
  Nach Abschluss beide IDs identisch wiederholt: dieselben acht Objekt-IDs,
  kein Doppelbau und keine Spielzeitänderung.
- Lagerbaustelle buildersReachable=true, Eingang frei, constructionAccess
  observed und constructionDistrict zugeordnet. Fertiges Lager separat
  finished=true, Eingang frei und distriktzugeordnet, Distanz 53.
- Beide oberen Wege, beide unteren Bestandswege und erste Treppe
  distriktverbunden. Bestehendes Vergleichslager vor/nach frei, Distanz 50.
  Kein Wegschutz-Guard-Abbruch, keine allgemeine Vorschau-Schutzabnahme.
- Sechs begrenzte Läufe mit je acht Spielstunden, jeweils Ziel erreicht,
  overshootHours=0 und Pause bestätigt. Testfixture: ein Wohnhaus und vier
  daraus entstandene Schuttstapel regulär entfernt; kein Save-Eingriff.
- MCP-Profil known/0.33.1/Folktails, vier Plätze aller Vertikalmodi und historische
  Lagerdrehungen 0..3 gelesen. Log-Leser mit 0.33.1 kompatibel.

Vier tatsächlich ausgeführte Liveprojekte, fünfte ID, stopped/unconfirmed und
frische Ledger-Sitzung wurden nicht zusätzlich live provoziert; dafür besteht
synthetische Testabdeckung. Zwei Projekte belegen den neuen Sequenzbetrieb,
nicht beliebige Höhenplanung oder allgemeine Bau-/Wegschutzsicherheit.
