# Kiefernschutz, Abriss und Hindernisse (0.13.0)

Stand: implementiert; Kiefernschutz, Abriss/Wiederaufbau und Vegetationsentfernung gezielt live bestätigt.
Lebenszustände werden seit 0.13.2 berücksichtigt. [Aktueller Projektstand](../PROJECT_STATE.md).

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
erlauben mark/unmark. Der reguläre Mark-Aufruf kann einen Auftrag setzen oder geeignete Ressourcen unmittelbar entfernen.
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

Live-Fortschreibung: tapping und tree_cutting an einer Kiefer mit Rückweg erfolgreich
(191 -> 190 -> 191 Fällzellen). Carrot- und Pine-Pflanzmarkierungen auf freier Zelle
jeweils 0 -> 1 -> 0 geprüft. Abrissversuche jetzt ausdrücklich für den Entwicklungs-
spielstand freigegeben; die frühere Vorgabe zur vorherigen Nutzerwahl eines Testziels
ist damit überholt. Ziele weiterhin konkret lesen und reguläre Löschsperren beachten.

## Live-Abnahme und Korrektur 0.13.1

Weg entfernt, alte Entity-ID anschließend nicht gefunden, freier Bauplatz geprüft und
Weg regulär neu gebaut. Vegetation sowie Pflanzen auf passender Pflanzmarkierung:
Aufträge mark/unmark mit separaten Rücklesungen bestanden. Farmhausbaustelle mit
14 Holz im Baustellenbestand entfernt; dabei sechs neue Schuttstapel beobachtet.
Ein neuer Stapel entfernt und Abwesenheit bestätigt, fünf bleiben zunächst liegen.

Ein weiterer Vegetations-Mark-Aufruf entfernte eine Kiefer bereits bei pausierter
Simulation. Separate Abfrage bestätigte die Abwesenheit und leere Zelle; keine
Spielzeitfortschaltung und kein forcierter Delete-Aufruf durch unsere Bridge.
Warum diese konkrete Ressource sofort entfernt wurde, ist damit nicht abschließend
geklärt. Die Antwort von 0.13.0 war unnötig unconfirmed, weil nur IsMarked erwartet wurde.

0.13.1 akzeptiert beim regulären mark sowohl einen gesetzten Auftrag als auch eine
bereits entfernte Entity als applied. removed unterscheidet beide Fälle. unmark darf
weiterhin keine Entfernung als Erfolg melden. Sechs Regressionstestfälle sichern den
Clientvertrag. Korrektur gebaut/automatisiert geprüft; Installation und erneuter
Live-Nachweis der korrigierten Antwort noch offen. Biberarbeit über Zeit weiterhin
nicht nachgewiesen; sofortige reguläre Entfernung ersetzt diesen Nachweis nicht.

### Live-Abnahme 0.13.1 abgeschlossen

Wartender Auftrag und Sofortentfernung liefern beide passende applied-Antworten;
removed unterscheidet sie korrekt. Separate Zustandsabfragen stimmen überein.
Ein weiterer zunächst wartender Auftrag wurde nach kurzer 7×-Simulation erledigt
(Entity abwesend, Fällmarkierung zuvor entfernt). Individueller Worker nicht verfolgt.
Simulation abschließend pausiert, keine offenen Testaufträge. Damit ist der oben noch
offen genannte Patch-Livenachweis abgeschlossen.

## Änderung 0.13.2: Baumgesundheit berücksichtigen

Die bisherigen ungefilterten unmarkierten Kiefern sind keine verlässlichen gesunden
Zapfkandidaten. Ab 0.13.2 Lebenszustand, Sterben, Wasserstress und Wachstum ausgeben;
tapping nur bei nachweislich geeigneten Lebens-/Wachstumszuständen. Tote Vegetation
bleibt für Abriss sichtbar. [Aktueller Vertrag](vegetation-state.md).
