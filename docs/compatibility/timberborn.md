# Kompatibilität

## Buildstand 0.8.0 — 2026-09-20

Öffentliche SpeedManager-/IDayNightCycle-API gegen lokale 1.1.2.4 gebaut.
158 reguläre Tests bestanden, separater Mod-Build ohne Warnungen/Fehler.
Pause/1× und Zeitwerte noch nicht live bestätigt; installiert bleibt 0.7.0.

## Buildstand 0.7.0 — 2026-09-20

Lodge-Pilot gegen lokale Timberborn-1.1.2.4-Assemblies gebaut, keine Warnungen/Fehler.
147 reguläre Tests bestanden; drei Live-Tests übersprungen. Noch nicht installiert
oder live abgenommen. Letzter installierter/live geprüfter Stand: 0.6.1.

## Fortschrittsbeobachtung 0.6.1 — 2026-09-20

Farmhauslieferung und Bauarbeit über zwei Lesestichproben nachgewiesen:
Baustellenbestand 2 -> 4 Log, MaterialProgress 0.08 -> 0.16, BuildTimeProgress
0 -> 0.109375. Noch unfertig. Holzversorgung vom Nutzer eingerichtet; native
Produktionssteuerung dadurch nicht nachgewiesen. Globalbestand und Baustellenbestand
sind getrennte Beobachtungen und waren in der zweiten Probe verschieden (0 bzw. 4 Log).

## Materialbeobachtung 0.6.1 — 2026-09-20

EfficientFarmHouse.Folktails als gespeicherte Baustelle live gelesen: Vorlagenkosten
25 Log, Inventar verfügbar/leer, global Log=0, Fortschritt 0, Baudistrikt bekannt.
Kosten und aktueller Bestand getrennt bestätigt; kein Restbedarfs-/Lieferzeitversprechen,
kein Nachweis zeitlicher Lieferung oder Fertigstellung. Vier reine Leseaufrufe.

## Gebäude-/Distriktbeobachtung 0.6.0 — 2026-09-20

District Center und Holzfällerflagge mit bekannten Betriebs-/Instant-Distrikt-IDs
live gelesen, fertiger Path ohne DistrictBuilding-Komponente. Fehlende Entity und
fremde Sitzung abgewiesen. Keine unfertigen Strukturen vorhanden, daher Baustellen-
und Materialfortschritt noch nicht live bestätigt. Keine Mutation im Lesepilot.

## Aktueller nativer Bau-Pilot — 2026-09-20

Bridge 0.5.0: genau einen Path über den regulären Platzierer per MCP gebaut.
Separate Abfrage bestätigt erwartete Entity-ID, Vorlage, Position und finished=true.
Ein neues Objekt, beobachtete Vorräte unverändert. Live-Nachweis nur für diesen
Einzelfall; kein allgemeiner Gebäude-/Versorgungsbau und kein Distriktnachweis.

Stand 2026-09-19. Diese Matrix beschreibt die tatsächlich geprüfte lokale Kombination.

| Komponente | Version | Nachweis |
|---|---|---|
| Timberborn | 1.1.2.4-52e959e-sw | Versionsdatei und API |
| More HTTP API | 11.0.0 | Manifest, Spiel-Log, API und fünf MCP-Leseaufrufe |
| Moddable Timberborn | 11.1.2 | Manifest und Spiel-Log |
| Mod Settings | 1.1.1.0 | Manifest und Spiel-Log |
| Harmony | 2.4.1 | Manifest und Spiel-Log |
| TimberUi | 11.0.1 | Manifest und Spiel-Log |

MCP-POC: stdio erfolgreich gegen diese Kombination getestet. Der Nutzer hat die gemeldeten
Zahlen der Testkolonie MCP mit der Spielanzeige verglichen und bestätigt.
Keine Aussage zur Kompatibilität anderer Versionen, großer Kolonien oder zusätzlicher Mods.

`http://localhost:8080/` funktioniert in dieser Umgebung. Die numerische IPv4-Adresse
`http://127.0.0.1:8080/` wurde vom Spiel abgelehnt. Der Client erhält den Hostnamen und setzt
ihn nicht durch eine numerische URL um. Daraus wird keine allgemeine Ursache abgeleitet.

## Native Bridge — 2026-09-20

Agent Bridge 0.2.0 mit Konfigurationspfad-Fix über ModRepository: im Spiel geladen,
HTTP-Port 8081 erreichbar, alle drei nativen MCP-Werkzeuge über stdio live erfolgreich.
Bevölkerung/Bestände/Wohnraum durch Nutzer bestätigt. Eine Kartenzelle technisch gelesen;
Semantik und Session-Wechsel noch offen. Bisherige Fremdmods weiterhin aktiv, aber vom
nativen Backend nicht angesprochen; isolierter Spielstart ohne Fremdmods noch nicht geprüft.

0.3.0 zusätzlich live geprüft: sechs native MCP-Werkzeuge, 21 Gebäude-/Wegeobjekte,
Fraktion/Freischaltung/Kosten von Lodge und Path, 64 Gelände-/Wasserzellen und vier
erklärbar abgewiesene Bauplatzvorprüfungen. Objektzahl und beobachtete Bestände unverändert.
Sitzungswechsel gegenüber vorher lokal gespeicherter ID direkt bestätigt.
0.4.0 kompiliert und synthetisch geprüft; Vorschau-Liveprüfung steht aus.

## Isolierter Betrieb — 2026-09-20

Nutzer bestätigt Spielstart mit ausschließlich eigener MCP-Mod. Anschließend separaten
nativen stdio-Lesetest ausgeführt: alle sechs Werkzeuge in derselben Sitzung erfolgreich,
keine übersprungenen Tests. Damit ist der lesende Betrieb der installierten Bridge 0.3.0
ohne aktive Fremdmods für diese Testkolonie nachgewiesen. Die Mod-Auswahl wurde durch den
Nutzer bestätigt, nicht durch eine zusätzliche API-Inventur. Kein Vorschau- oder Bauaufruf.
Dieser Nachweis gilt nicht für den noch nicht live geprüften Validator aus 0.4.0.

0.4.0 später live erreichbar, sechs Lesewerkzeuge auch nach Vorschauversuch erfolgreich.
Validator-Abnahme fehlgeschlagen: belegter Standort fälschlich akzeptiert; Pilot nach
einem Versuch abgebrochen. 0.4.1 als Korrekturkandidat gebaut/gepackt und auf Nutzer-Go
installiert und anschließend begrenzt live abgenommen: belegte Lodge abgewiesen,
freier Path akzeptiert. Zwei Vorschauversuche, keine beobachtete Entity-/Bestandsänderung,
keine Platzierung. Kein Nachweis für beliebige Vorlagen oder Baugeometrien.
