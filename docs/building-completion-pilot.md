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

## Nächster C-Nachweis

Ein einzelner Lagerkandidat mit kurzem neu erforderlichem Weganschluss.
Wegauftrag und wirklich fertigen/nutzbaren Weg unterscheiden; anschließend
Baustellenzugang, tatsächliche Lagerfertigstellung und fertigen Eingang prüfen.
Relevante Bestandsverbindungen samt Distanz vor/nach vergleichen und Änderungen
sichtbar ausweisen. Keine Suchserie oder Erweiterung auf Höhen als Ersatz.
