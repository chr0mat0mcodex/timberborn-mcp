# Strukturierter räumlicher Pilot — Agent Bridge 0.3.0

> Historischer Pilot-/Nachweisbericht. Alte Versionsstände und Grenzen gelten für den damaligen Test. Aktueller Einstieg: [native Installation](native-bridge-install.md), [generischer Bau](generic-building.md) und [Projektstand](../PROJECT_STATE.md).

Nutzer-Go: autonom weiterarbeiten, bis tatsächliche Hilfe nötig ist. Zustände direkt
im Spiel abfragen, Interaktionen programmieren und kontrollieren; keine Screenshots.
Aktueller Schritt erweitert ausschließlich Leserouten. Noch keine Vorschauobjekte,
Bauaufträge, Freischaltungen, Ressourcenänderungen oder Spielzeitsteuerung.

## Sechs native MCP-Werkzeuge

| Werkzeug | Beitrag zum Spielen | Grenze |
|---|---|---|
| timberborn_status | Erreichbarkeit, Modversion und Sitzung | keine Schreibfähigkeit |
| inspect_colony | Bedarf: Bestände, Personal, Betten | drei Beispielgüter, globale Werte |
| inspect_map_region | Terrain/Wasser pro Zelle | höchstens 8×8×4 Zellen |
| find_buildings | Gebäude und Wege, TemplateSpec-ID, Eingang, belegte Zellen | 32 Objekte je Seite, 64 Zellen je Objekt, natürliche Objekte ausgeschlossen |
| inspect_build_catalog | Lodge.Folktails/Path, Fraktion, Freischaltung, Größe, Eingang, Kosten | Pilotkatalog, keine gesamte Baupalette; Bettenskapazität noch nicht öffentlich erschlossen |
| precheck_build_site | geometrische Hindernisse und Materialinformation für genau eine Position | ausdrücklich keine vollständige Baufreigabe |

Die Eingabe-Rotation 0/1/2/3 wird explizit auf die Spielwerte Cw0/Cw90/Cw180/Cw270
abgebildet; FlipMode bleibt Unflipped. PositionedBlocks und PositionedEntrance
berechnen die transformierte Geometrie. Keine nachgebaute Rotationsmathematik.

Die Vorprüfung meldet outside_map, object_intersection, terrain_intersection,
required_ground_missing, wrong_faction, template_disabled bzw. template_locked,
wenn die jeweiligen lesenden Abfragen ein Hindernis zeigen. Für andere Stützregeln
(z.B. GroundOrStackable) wird kein vollständiges Urteil erfunden. Ein freier Eingang
oder PathAtEntrance belegt noch keine Verbindung zum Distrikt oder Erreichbarkeit.

Ergebnis ist **blocked** bei einem erkannten Hindernis, sonst
**requires_game_validation**. **gameValidated bleibt immer false** und wird im
MCP-Adapter darauf geprüft. Materialvorräte sind globale Informationen; sie werden
nicht mit reservierten, erreichbaren oder rechtzeitig lieferbaren Materialien gleichgesetzt.
Wasser-/Gefahrensicherheit sowie volle Spielvalidatoren bleiben getrennt zu prüfen.

## Nach Neustart: begrenzter Live-Ablauf

1. Vorherige lokal gespeicherte Session-ID mit frischem Snapshot vergleichen.
2. Sechs Werkzeuge einmal über echtes MCP testen, Objektliste und zwei Vorlagen lesen.
3. Maximal eine weitere Gebäudeseite, sofern die erste keine geeignete Struktur liefert.
4. Für ein bekanntes Gebäude Eingang, belegte Zellen und passenden kleinen Kartenausschnitt
   anhand der direkten Spielabfragen vergleichen. Keine visuelle Interpretation.
5. Vorprüfung für Lodge auf belegter Stelle, am Kartenrand und an höchstens zwei
   plausibel freien Bodenpositionen. Gründe und transformierte Zellen auswerten.

Budget: höchstens zwölf fachliche Live-Abfragen im ersten Pilot; Testaufrufe mitzählen.
Stoppen bei inkonsistenten Koordinaten, inkompatiblen Daten oder unerwarteten Nebenwirkungen.
Keine ganze Karte blind abscannen. Der Pilot ist erfolgreich, wenn stabile Vorlagen,
Geometrie und erklärbare Hindernisse vorliegen. Er ist **keine** Bauabnahme.
Erst danach gezielte vollständige Spielvalidierung und ein regulärer Bauauftrag.

## Quellenentscheidung

Öffentliche Spielsignaturen: TemplateNameMapper, TemplateSpec, IBlockService,
PositionedBlocks, PositionedEntrance, BuildingUnlockingService und FactionService
an der lokalen Version 1.1.2.4 geprüft. Bauprüfung berührt keine privaten Felder.
Hersteller beschreibt Validatoren in der
[offiziellen Architektur](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture).

Die ergänzend geprüften [Community-Referenzen](references/sources.json) zeigen
PreviewFactory/PreviewShower und TemplateNameMapper. BuildingBlueprints' Validator
platziert teils reale Objekte und löscht sie zur Prüfung wieder. Dies ist für unseren
kontrollierten Vorprüfungsschritt ungeeignet. Deshalb pure Geometrie-/Zustandsabfragen,
keine Übernahme der Spawn-/Delete-Strategie und keine neue Fremdmod-Abhängigkeit.

Die allgemeinen Fähigkeiten von Mod 0.2.0 sind live nachgewiesen; 0.3.0 muss nach
Installation neu geladen werden. Tests gegen synthetischen HTTP-/MCP-Zustand können
diese neue Spielsemantik nicht allein beweisen.
