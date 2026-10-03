# Etappe C: Lager bis zur tatsächlichen Fertigstellung

Stand 2026-10-03, Bridge 0.31.3 nach menschlichem Skript-Gate. Nutzer priorisiert C
vor weiterer B-Diagnose. Keine Codeänderung oder erneute Installation für diesen
Livetest; vorhandene strukturierte MCP-Werkzeuge und Entwicklungspilot verwendet.

## Belegter Ablauf

Genau ein begrenzter 2×2-Suchbereich für SmallWarehouse.Folktails, ein Kandidat
mit bestehendem Anschlussweg und ohne neue Wegzellen:

1. Gemeinsame Vorschau geometrisch gültig, geplanter Eingang verbunden;
   lostConnections=0, lostSites=0, Baseline/Rücknahme bestätigt.
   constructionCovered=false bleibt sichtbar.
2. Auftrag einmal als development_pilot erteilt. Projektstatus completed bedeutet
   hier ausschließlich order_and_access_confirmed, nicht fertiges Gebäude.
3. Tatsächliche Baustelle separat rückgelesen: finished=false,
   buildersReachable=true, entranceBlocked=false, entranceInaccessible=false;
   Materialbedarf drei Holzstämme.
4. Ein begrenzter Simulationslauf: zehn Spielstunden, etwa 29,6 Echtzeitsekunden,
   danach Pause bestätigt, kein Zeitüberschuss.
5. Tatsächliches Gebäude separat rückgelesen: finished=true, unfinished=false,
   Bezirk zugeordnet. Eingang nicht blockiert und nicht unzugänglich.
   Native Straßenpfadsuche Zentrale→Lager: supported=true, connected=true,
   distance=10. Builderwert am fertigen Gebäude nicht verfügbar, nicht false.

## Bestandskontrolle und Grenze

Ein benachbartes fertiges Lager vor/nach Bau kontrolliert. Verbindung zur Zentrale
weiterhin true, Eingang nach Bau nicht blockiert und nicht unzugänglich.
Gemessene native Distanz änderte sich jedoch von 10 auf 11. Ursache nicht belegt;
keinen unveränderten Wegverlauf oder unveränderte Wegqualität behaupten.
Vorschau mit null Verbindungsverlusten erfasst diese Distanzänderung nicht.

Damit ist der vollständige kleine Lagerablauf für **bestehenden Anschluss**
nachgewiesen, nicht der allgemeine Schutz aller Wege und Baustellen. Keine
neuen Anschlusswege, keine Höhen, keine Warenlieferung oder Lagerbetrieb geprüft.
Etappe B bleibt offen, reguläre sichere Baufreigabe unverändert gesperrt.

## Weiterer C-Nachweis — in 0.31.4 bestanden

Ein einzelner Lagerkandidat mit kurzem neu erforderlichem Weganschluss.
Wegauftrag und wirklich fertigen/nutzbaren Weg unterscheiden; anschließend
Baustellenzugang, tatsächliche Lagerfertigstellung und fertigen Eingang prüfen.
Relevante Bestandsverbindungen samt Distanz vor/nach vergleichen und Änderungen
sichtbar ausweisen. Keine Suchserie oder Erweiterung auf Höhen als Ersatz.

0.31.4 nach menschlichem Skript-Gate live: konkret geprüftes Pumpenhindernis und
vier erzeugte Abrissreste entfernt. Ein Lager mit einem neuen ebenen Weg geplant,
gemeinsame Vorschau geometrisch gültig, Eingang verbunden, keine geprüften
Verbindungsverluste, Rücknahme bestätigt. Weg fertig und im Bezirk, Baustelle real
erreichbar; acht tatsächliche Baustellenzugangszellen separat gelesen.

Zweiter sequenzieller Auftrag mit einem neuen Weg schließt einen bekannten
Wegunterbruch; ein einzelner toter Baum am Lagerplatz entfernt. Dritter Auftrag
erzeugt die isolierbare Plateau-Baustelle für B, ohne neue Wege. Alte Projektbelege
bleiben nach neuen Aufträgen lesbar. Identische erste Ausführungsanfrage liefert
nur den gespeicherten Beleg, auch nachdem das Gebäude längst fertig ist.

B-Trennung und regulärer Treppenwiederaufbau siehe construction-access-preview.md.
Ein gemeinsamer begrenzter Simulationslauf: 14 Spielstunden, etwa 41,4 Echtzeitsekunden,
kein Zeitüberschuss, Pause bestätigt. Drei Lager und Treppe finished=true,
unfinished=false. Beide neuen Wege zuvor fertig und im Bezirk bestätigt. Alle drei
Lagereingänge nicht blockiert/nicht unzugänglich, native Straßenverbindungen true;
Distanzen etwa 13 / 26,4 / 42,2 (keine Reisezeiten). Kontrollierte Nachbarverbindung
vor/nach true, Distanz 11→11, diesmal keine beobachtete Verschlechterung.

C im begrenzten SmallWarehouse-Umfang abgeschlossen: Vorschau, nutzbarer neuer
Anschluss, echte Baustelle, echte Fertigstellung und tatsächlicher fertiger Zugang.
Keine Zusage für alle Bestandsverbindungen, Bauphasen oder Betriebsfunktionen.
Pumpe und Abrissreste bleiben entfernt; tote Birke entfernt, Testtreppe regulär
wieder aufgebaut. Keine Wiederherstellung des gesamten Ausgangsspielstands.
