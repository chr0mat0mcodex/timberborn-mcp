# Kiefernschutz, Abriss und Hindernisse (0.13.0)

Stand: implementiert, Live-Abnahme offen. Ergänzt 0.12.0 ohne Fremdmod-Abhängigkeit.

## Zapffläche als Kiefernschutz

set_area mit kind=tapping und operation=mark entfernt Baumfällmarkierungen im
gewählten Rechteck (höchstens 4×4). resource bleibt leer; expectedResource ist marked
oder unmarked und muss für jede Zelle stimmen. Gemischte Bereiche vorher mit
inspect_areas(kind=tree_cutting) lesen und passend aufteilen. Es werden weder Bäume
entfernt noch Zapfer gebaut oder Arbeiter zugeordnet.

inspect_areas(kind=tapping) liefert die Positionen aller vorhandenen Kiefern ohne
Fällmarkierung, einschließlich ursprünglich unmarkierter Bäume. Das ist ein aus dem
Spielzustand abgeleiteter Kandidatenstatus, keine gespeicherte benannte Zapfzone. Auch
andere Bäume im gewählten Rechteck verlieren ihre Fällmarkierung. Kein Nachweis für
Reichweite, Reife, Harzertrag oder bereits reservierte Holzfälleraufträge.

operation=remove für tapping ist gesperrt: erneutes Fällen muss ausdrücklich über
kind=tree_cutting angeordnet werden. Es gibt keine separate Save-Erweiterung; die
veränderten regulären Fällmarkierungen speichert das Spiel.

## Getrennte Entfernung

inspect_removal_targets liest Blockobjekte, die einen Bereich bis 8×8×4 überlappen.
kind=all/buildings/planted/vegetation/debris; offset/limit bis 32 pro Seite. Auch
nicht unterstützte Objekte erscheinen bei all als other. Jede Antwort nennt ID,
Vorlage, Ankerposition, Kategorie, Modus, CanDelete und Entfernungsmarkierung.

| Schreibwerkzeug | Ziel und reguläre Aktion |
| --- | --- |
| demolish_building | Genau ein Gebäude/einen Weg über EntityService.Delete entfernen |
| remove_planted | Pflanze auf passender aktueller Pflanzmarkierung: Demolishable.Mark/Unmark |
| remove_vegetation | Übriger Bewuchs: Demolishable.Mark/Unmark |
| remove_debris | Genau einen RecoveredGoodStack über dessen Delete entfernen |

Für alle Aktionen erforderlich: id, session, template, x/y/z (Ankerposition), operation,
expectedMarked. Gebäude/Schutt erlauben nur delete und expectedMarked=false. Pflanzen
erlauben mark/unmark; sie werden durch reguläre Biberarbeit entfernt, nicht sofort.
Nicht jede Vegetation muss Demolishable unterstützen; die Abfrage meldet das.
Ruinen zählen ausdrücklich nicht als Schuttstapel und werden nicht entfernt.

Die öffentliche Plantable/NaturalResource-API belegt keine historische Herkunft.
planted bedeutet deshalb ausschließlich: aktueller Vorlagenname stimmt mit der
Pflanzmarkierung an dieser Stelle überein. plantOrigin bleibt unknown. Eine natürliche
Pflanze auf passender Markierung kann damit ebenfalls planted sein; eine früher
gepflanzte Pflanze ohne Markierung vegetation. Diese Kategorien sind keine Besitzdaten.
Pflanzmarkierungen bleiben beim Entfernungsauftrag erhalten; für dauerhafte Räumung
mit set_area separat entfernen, sonst kann neu gepflanzt werden.

## Schutz und Grenzen

Eigenes Opt-in auf beiden Seiten: enableRemoval in privater Mod-Konfiguration und
TIMBERBORN_ENABLE_REMOVAL=1 im MCP-Prozess. Default aus. Feste POST-Route ohne Body,
keine generische Entity-/Konsolensteuerung. Session, ID, Kategorie, Vorlage, Position und
bisherige Markierung werden unmittelbar auf dem Spielhauptthread geprüft. CanDelete
muss Entfernung erlauben; kein Force-Delete oder Auflösen gestapelter Abhängigkeiten.
Unmark ist auch bei später entstandenen Löschhindernissen erlaubt.

Gebäude-/Schuttlöschung ist irreversibel und kann Güter verlieren. Der eigene Code
ruft keine Terrainzerstörung, Stapellöschung, Dev-Sofortfertigstellung oder manuelle
Gütererstattung auf. Die normalen Lebenszyklus-Ereignisse des Spiels dürfen wirken;
Rückgewinnung und abhängige Effekte sind vor Live-Abnahme nicht garantiert.

Antwort applied bedeutet beobachtete Löschung bzw. gesetzten/zurückgenommenen Auftrag,
nicht erledigte Biberarbeit. Bei unconfirmed, Timeout oder Fehler nur nachlesen, niemals
automatisch wiederholen. Es gibt keinen automatischen Undo oder Rollback.

## Vor Bauvorhaben

Bereits vorhanden: precheck_build_site mit outside_map, object_intersection,
terrain_intersection, required_ground_missing sowie Fraktion/Freischaltung.
Es meldet betroffene Zellen, aber keine vollständige Baufreigabe. Der bestehende
Pilotkatalog bleibt Lodge.Folktails/Path. validate_build_site nutzt getrennt den
Spielvalidator. inspect_removal_targets(kind=all) identifiziert jetzt Objekte im
betroffenen Bereich, einschließlich Vegetation und Schutt. Nicht jedes Objekt im
Bereich ist tatsächlich ein Kollisionshindernis; mit der Vorprüfung abgleichen.
Nach Entfernung erneut prüfen, vor vollständiger Räumung nicht einfach bauen.

## Belege und nächste Prüfung

Öffentliche Signaturen der installierten Spielversion: BlockObject.CanDelete,
EntityService.Delete, Demolishable.Mark/Unmark/IsMarked, RecoveredGoodStack.Delete,
NaturalResource, Plantable, TreeCuttingArea. Offizielle Blueprints belegen Kiefer/Pine
und Schuttstapel. Keine privaten Felder, IL-Decompilation oder übernommenen Fremd-DLLs.
[Hersteller-Moddingwerkzeuge](https://github.com/mechanistry/timberborn-modding) und
[vorhandene Referenzen](references/README.md) bleiben Dokumentationsgrundlage;
der öffentliche API-Pfad benötigt keine neue Abhängigkeit.

Nach Installation zuerst reine Abfragen und ausstehende Prioritätsabnahme. Kiefernschutz
an begrenztem Testbereich nachlesen. Für irreversiblen Abriss zunächst ein entbehrliches
Testobjekt bestimmen; kein bestehendes Versorgungsgebäude oder Bewuchs als stilles Testziel.
Pflanzenauftrag und tatsächliche Räumung getrennt beobachten. Mod-Komponentenanbindung,
CanDelete-Semantik und normale Folgeereignisse sind live noch zu prüfen.

Live-Klarstellung: Die Abfrage ist kartenweit. Unmarkierte Kiefern sind nur mögliche
Zapfkandidaten, keine vom Agenten eingerichtete Schutzfläche. Im geprüften markierten
Bereich wurden 32 Kiefern korrekt als fällmarkiert erkannt und nicht als tapping
ausgegeben. Die Flächenmutation wurde weiterhin nicht live ausgeführt.
