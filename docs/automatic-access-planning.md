# Automatische Zugangsplanung für Gesamtpläne

## Ergänzende Liveprüfung: Rotorüberhang und Bau unter Überhang

Am 2026-10-11 mit unverändertem 0.37.0-Build geprüft:

- WindTurbine (47,7,6), Fundament/Mast (48,8,6): vorhandener Weg (47,7,6)
  unter dem Rotor bleibt gültige Arbeitszelle. Acht freie Kandidaten, 69 Suchknoten,
  keine zusätzlichen Hilfen. Tatsächliche Baustelle `buildersReachable=true`;
  autonom fertig nach 10,26 Sekunden / 3,5 Spielstunden, Pause bestätigt.
- Gegenprobe mit Mast auf Bestandsweg: `target_intersects_existing_or_planned_object`,
  Zielindex 0, Phase `target_geometry`, null Aufträge.
- Separates SmallPile (47,8,6) unter dem fertigen Rotor: native Geometrie gültig,
  sieben freie Arbeitszellen, zwei davon am Distriktweg, 124 Suchknoten.
  Echte Baustelle erreichbar; fertig nach 11,92 Sekunden / 1,75 Spielstunden.
  Fertiger Eingang weder blockiert noch unerreichbar, Distriktdistanz 44.
- Beide Ledger nach frischem Abgleich freigegeben; keine verlorenen Bestandsanschlüsse.
  Keine Kraftproduktion behauptet: das Testwindrad war nicht ans Produktionsnetz gekoppelt.

Das zuvor als leer bestätigte Lager und das Testwindrad wurden gezielt entfernt.
Nach regulärer Sammlung ihrer Rückgewinnungsstapel beide Testwege und die
Testtreppe zurückgebaut; auch die Treppenrohstoffe anschließend eingesammelt.
Abschlussabfrage der Testfläche (45,7,5), 5×3×4: null Objekte, keine Reststapel.
Bestandswege daneben erhalten; Holzwerkstatt weiterhin zugänglich, Distriktdistanz 47.
Kein `remove_debris`, kein Löschen gelagerter Güter. Alle Testledger freigegeben.
Die Simulation lief beim abschließenden Lesen wieder mit Tempo 7; keine eigene
aktive Bau- oder Simulationscharge zurückgelassen. Zwei Sammelstarts wurden wegen
geänderter Geschwindigkeit abgewiesen; vor weiterem Vorgehen jeweils frisch gelesen.
Dies belegt Rotorfreiraum und einen niedrigen Bau darunter,
keine allgemeine Freigabe sämtlicher Überhang-/Spezialvorlagen oder späterer Überbauung.

## Liveprüfung 2026-10-11: begrenzter Höhenzugangspilot bestanden

Nach menschlichem Gate und Bereitmeldung ist Policy 1 live vorhanden; installierte
Bridge- und Core-Dateien stimmen mit dem neuen lokalen Paket überein.
Wetterstationsplatz (47,8,6): `placementValid=true`, aber ohne Arbeitsweg
`pilotEligible=false`, keine Aufträge und keine verlorenen Bestandsanschlüsse.
Automatische Ergänzung am selben Ziel: eine Treppe von Ebene 5 nach 6 und zwei
Wege, 174 Suchknoten, vier Teile insgesamt. Die Vorschau meldet `road_reachable`.
Ein Run beauftragt alle Teile in ihren Abhängigkeiten; alle drei Zugangshilfen
sind fertig. Am realen Ziel sind `buildersReachable=true` und die tatsächlichen
Baustellenzugangszellen separat beobachtet. Damit ist der Höhenzugang praktisch belegt.

Das 48-Spielstunden-Budget beendet die Charge korrekt mit bestätigter Pause und
`game_time_limit`: veredelte Planken fehlen noch, kein Zugangsfehler. Bestehende
Produktion und Lieferung sind beobachtet. Ein separater, einmaliger Vier-Tage-Lauf
wartet auf den bereits erteilten Bau; kein Wiederanlauf des terminalen Projektledgers.
Der Testbau wurde im Zusatzlauf fertig; danach Lauf vorzeitig beendet, Pause und
alle vier fertigen Teile frisch bestätigt. Das terminale Ledger blieb historisch
`blocked/game_time_limit`, wurde jedoch nach frischem Abgleich freigegeben.
Keine verlorenen Bestandsanschlüsse. Belegter Weg als neues Gebäudeziel wird mit
`target_intersects_existing_or_planned_object`, `blockedPart=0`, `phase=target_geometry`
ohne Auftrag abgewiesen. WeatherStation hat keinen regulären Eingang; keine
Aussage über einen Arbeitseingang oder Distriktdistanz aus null-Werten ableiten.

Zusätzliche Test-Wetterstation entfernt. Ihr Rückgewinnungsstapel bei (48,8,6)
bleibt erhalten, ebenso die Treppe (45,7,5) und Wege (46,7,6)/(47,7,6) für Sammlung.
Dies war der Zwischenstand nach dem ersten Pilot. Die ergänzende Liveprüfung oben
schließt Sammlung und Rückbau aller drei Hilfen ab. Keine Rohstofflöschung.

Begrenzte Abnahme: fehlender Arbeitsweg, automatische Höhenroute, echter
Baustellenzugang, tatsächliche Fertigstellung und belegter Bestandsweg. Überhang-/
Windradfall waren hier zunächst nicht separat geprüft; die ergänzende Abnahme
oben liefert diese Nachweise. Spätere Überbauung einer zunächst erreichbaren
Arbeitszelle bleibt ungeprüft; dafür keine allgemeine Freigabe behaupten.

## Erweiterung: Bauarbeitszugang — begrenzt live abgenommen

Die Quellfassung ergänzt `accessPolicyVersion=1`. Jeder Teil enthält
`constructionWorkAccess`: Pflichtprüfung, Status, Anzahl freier Kandidaten und
höchstens 16 nativ an den Distriktweg angeschlossene Arbeitszellen. Kandidaten
liegen an allen acht Nachbarpositionen der Fundamentzellen auf deren Höhe.
Native Blockkollisionen ersetzen den Ausschluss ganzer Bauvolumen: freie Bereiche
unter überhängenden Bauteilen werden nicht allein wegen ihrer Koordinate verworfen.

Gebäude benötigen `road_reachable` sowohl in der topologisch aufgebauten Vorschau
als auch unmittelbar vor dem tatsächlichen Auftrag. Der Gesamtplan wird zusätzlich
auf später überbaute oder abgeschnittene Arbeitszellen geprüft. Fehlende Zellen,
fehlender Weganschluss und spätere Blockaden verhindern die Pilotfreigabe.
Ein zur Laufzeit verlorener Zugang stoppt vor der Platzierung mit
`construction_work_access_rejected`; die Diagnose enthält den Prüfbericht.

`plan_large_project_access` ergänzt die dafür benötigten Wege, Treppen und Stützen
einschließlich Abhängigkeiten. Suchrand jetzt zwölf Zellen; Teile-, Höhen-, Knoten-
und Zeitbudgets unverändert. `blockedPart` und `phase` ordnen Ablehnungen einem
Ziel und Prüfschritt zu; `workCellCount` ist die Kandidatenzahl der letzten
Routensuche, nicht die Zahl sicher erreichbarer Baustellen. Teilpläne nie ausführen.

**Nachweisgrenze:** Dies ist eine native Wegprüfung zu geometrischen Arbeitszellen,
keine hypothetische Abfrage von `ReachableConstructionSite`. Deshalb bleiben
`nativeConstructionProven=false`, `constructionAccess=unknown` und
`regularExecutionAllowed=false`. Wege und zugelassene Zugangshilfen melden
`helper_runtime_check`; deren Bauzugang wird weiterhin erst am realen Objekt
kontrolliert. Spezialbau-Reichweiten, automatische Reparatur bestehender Baustellen
und vollständige Routenabdeckung sind damit nicht gelöst. Alte Antworten ohne
Policy-Version bleiben lesbar, gelten aber nicht als Nachweis dieser Erweiterung.

Vorbereitete Tests decken Fundamentrand, Ecken, Kartenrand, überdeckte Zellen,
Auftragsstopp bei fehlendem Nachweis und widersprüchliche Antwortverträge ab.
Menschliches Gate und obiger begrenzter Livetest sind erfolgt. Der ursprüngliche
Abnahmeplan bleibt zur Einordnung der noch ungeprüften Sonderfälle dokumentiert:

1. Gegenprobe: gültiger freier Bauplatz ohne angeschlossene Arbeitszelle wird
   ohne Auftrag abgewiesen; Bestandszugänge bleiben unverändert.
2. Dasselbe Ziel mit automatischer Ergänzung: benötigte Hilfen vor dem Gebäude,
   ein autonomer Lauf bis fertig, echter Baustellenzugang und fertiger Eingang belegt.
3. Höhen-/Überhangfall mit Treppe: freie native Geometrie wird berücksichtigt;
   notwendige Stützung, Bauzugang und benachbarte Zugänge separat bestätigen.
4. Gefährdeter Bestandszugang oder später überbaute Arbeitszelle: Ablehnung ohne Bau.

Bei falscher Gegenprobe sofort stoppen und diagnostizieren. Kein Skalieren auf
weitere Gebäude. Hilfen nur nach frischer Zugangsprüfung rückbauen, Rohstoffe erhalten.

## Bisherige Abnahme vor dieser Erweiterung

Stand 2026-10-11: nach menschlichem Gate und erneuter Bereitmeldung begrenzt
live abgenommen, gemeinsamer 0.37.0-Build. Compilerkorrektur (`Vector3Int`) und
Isolation des Vorschaltprozess-Dateitests enthalten. Neuer Werkzeugkatalog nach
Codex-Neustart übernommen.

## Live-Ergebnis

- Ebener Zweiteiler: Lager plus automatisch ergänzter Weg. Ein Run, beide Teile
  nach 4,11 Sekunden / 1,40625 Spielstunden fertig; Pause bestätigt.
- Höhen-Sechsteiler: Lager, zwei Plattformen, Treppe und zwei Wege automatisch
  geplant. Ein Run, alle sechs Teile nach 11,83 Sekunden / 4,0625 Spielstunden
  fertig; keine manuelle Zugangsreparatur oder Advance-Aufrufe pro Teil.
- Beide Lagereingänge separat frei und erreichbar, Distriktdistanz 35. Benachbarter
  Trägerposten und Bauhütte weiterhin erreichbar. Beide Projektledger freigegeben.
- Ziel auf Bestandsweg: `candidateFound=false`,
  `target_intersects_existing_or_planned_object`, null Plan, keine Bauaufträge.
- 48 bzw. 114 Suchknoten. Alle erzeugten Hilfen dienen hier den dauerhaften
  Eingängen/Trägern; leere Rückbaukandidatenlisten sind in diesen Fällen korrekt.

Rückbau: sieben der acht Testobjekte entfernt, ohne `remove_debris`. Die oberen
Rohstoffstapel wurden während begrenzter Sammelläufe abtransportiert. Ein Stapel
der Treppe bei (33,4,4) bleibt erhalten; die letzte Testwegzelle (33,5,4) bleibt
für dessen Sammlung stehen. Nach insgesamt 36 Spielstunden keine weitere
Warteschleife. Bestandswege unverändert, Spiel pausiert. Diese zwei Restobjekte
bei nächster Arbeit frisch prüfen, nur den Weg nach Sammlung entfernen.

Grenzen: kein separater Live-Umwegfall, keine positive temporäre Rückbauliste,
keine 32-Teile-/Acht-Ebenen-Vollabdeckung. Native hypothetische Baustellenreichweite
bleibt unbekannt; erfolgreiche tatsächliche Fertigstellung belegt diese zwei Fälle.

## Ablauf

`plan_large_project_access(session, districtId, steps)` erhält die gewünschten
Gebäude mit Position, Drehung und vorhandenen Abhängigkeiten. Es verschiebt keine
Gebäude und erteilt keine Aufträge. Der Planer ergänzt fehlende Plattformstützen,
Wege und Folktails-Treppen für den dauerhaften Eingang sowie eine angrenzende
Arbeitszelle auf Höhe des Gebäudefundaments. Vorhandene angeschlossene Wege und
gemeinsam benötigte Hilfen werden wiederverwendet. Eigene Wegziele gehören weiter
in den expliziten manuellen Plan.

Die Suche arbeitet gewichtet nach zusätzlichen Teilen im lokalen Graphen; sie
garantiert weder die global kürzeste Gesamtlösung noch vollständige Abdeckung.
Alle erzeugten Teile laufen anschließend durch die vorhandene gemeinsame native
Vorschau einschließlich Bestandszugängen, Stützen und Eingängen. Teilbudgets,
Abhängigkeiten und belegtes Volumen gelten für den erweiterten Gesamtplan.

- `candidateFound=true`: Vorschau hat den begrenzten Entwicklungspilot akzeptiert.
  Zur Ausführung die zurückgegebenen **`steps` und `plan.planKey`** gemeinsam an
  `run_large_project` übergeben. Der Start prüft denselben Plan erneut frisch.
- `candidateFound=false`: `reason` unterscheidet Hindernis, fehlende Stützung,
  fehlende Route im Suchraum, Suchbudget und native Ablehnung. Teilweise erzeugte
  Schritte nicht ausführen; `plan` kann null sein. Keine zweite Suche versteckt
  nachschieben, nachdem ein Budget verbraucht ist.
- `assessment` bündelt angefragte/ergänzte Teile, besuchte Knoten, Suchregion und
  vorgeschlagene Zugänge je Ziel. `plan.roadProtection` enthält den nativen
  Bestandsprüfbericht. Ungeprüfte Ziele fehlen im Teilbericht und sind nicht sicher.
- `cleanupCandidateIndices` benennt nur zusätzlich erzeugte Hilfen, die nicht
  für einen geplanten Eingang oder Gebäudeträger benötigt werden. Nach tatsächlicher
  Fertigstellung Bestandszugänge erneut lesen, dann gegebenenfalls rückbauen.
  Keine automatische Abrissfreigabe; Rohstoffstapel erhalten und einsammeln lassen.

## Grenzen und ehrliche Aussagen

- Höchstens 32 Gesamtteile, 32×32 belegte Grundfläche, acht belegte Höhen.
- Suchregion der neuen Quellfassung: Zielvolumen plus zwölf Zellen Rand (bisher sechs), höchstens sieben Ebenen unter
  der höchsten belegten Zielzelle. Vorhandener angeschlossener Distriktweg darin
  erforderlich. Maximal 2048 Knoten je Route, 4096 insgesamt, fünf Sekunden Suche.
- Die Ziele werden in Abhängigkeitsreihenfolge ergänzt. Eine ungünstige erste Route
  kann spätere Ziele blockieren; kein kombinatorischer Optimierer oder vollständiger
  Umplanungsalgorithmus. Ein nativer Fehlkandidat wird als solcher zurückgegeben.
- Unterstützung nur über öffentliche Vorlagengeometrie, Terrain und vorhandene
  fertige stapelbare Objekte; kein Raten bei erforderlichem Boden oder Spezialformen.
  Zusätzliche Teile nur Path, Stairs.Folktails und Platform.Folktails; gesperrte
  Hilfsvorlagen werden nicht freigeschaltet. Kein Räumen oder Abreißen im Aufruf.
- **Bauarbeiterzugang bleibt bis zur realen Baustelle ungeprüft.** Die angrenzende
  Arbeitszelle ist eine geometrische Planungshilfe. Die native fertige Vorschau
  liefert weiterhin keinen allgemeinen Nachweis des hypothetischen Bauzustands.
  `regularExecutionAllowed=false`; Laufzeitkontrollen und Reparaturverfahren bleiben.
- Gemeinsame Materiallieferung, Besetzung, Kraftbetrieb und Produktion separat prüfen.

## Testgate und Abnahme

Vorbereitete automatisierte Fälle: Umweg um gesperrte Zelle, gewichtete Route,
Höhenwechsel mit Treppen-/Stützenkosten, Teil-/Knotenbudget, bereits angeschlossener
Eingang ohne neue Teile, Schreibgate/HTTP-Methode/Schema, widersprüchliche Antwort
und unerlaubte Veränderung eines Zielgebäudes. Keine Agent-Testläufe.

Nach menschlichem Skriptlauf und `live bereit` drei begrenzte Kontrollen:

1. Ebenes kleines Gebäude mit nötigem Weg, einschließlich Hindernis/Umweg:
   Plan erzeugt erforderliche Wege; ein Run stellt alles fertig, Eingang separat
   zugänglich. Keine Agentenaufrufe je Bauteil.
2. Erhöhtes kleines Gebäude: benötigte Plattformen, Treppe und obere Wege werden
   ergänzt; Bauabhängigkeiten, realer Baustellenzugang, fertiger Eingang und
   benachbarte Zugänge nachweisen. Rückbaukandidaten dürfen keinen dauerhaften
   Eingang oder Gebäudeträger als entbehrlich ausgeben.
3. Ziel über belegtem Bestandsweg: Planung abweisen, keine Aufträge/Verluste,
   ursprüngliche Wege und Zugänge erhalten. Bei fehlerhafter Kontrolle stoppen.

Erfolg: beide positiven Fälle ohne manuelle Reparatur und die negative Kontrolle
korrekt. Bei fehlendem Kandidaten/Blockade gezielt Ursache prüfen, keine breite
Koloniesuche. Danach Testbauten/temporäre Straßen gezielt aufräumen, alle
Rückgewinnungsrohstoffe erhalten. Erst nach erfolgreichem Gate und Livetest committen.
