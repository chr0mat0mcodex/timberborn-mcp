# Siedlungsumbau: Flussland frei, Wohnen auf Z4

Laufender Spieltest vom 2026-10-10/11; kein Versionsabschluss.

## Größere Baugruppe: Kräuterkundler, Mühle und Windturbine

- Sechs tote Kiefern beziehungsweise abgeerntete Reste geräumt; zwei echte
  Arbeiteraufträge erst nach Rücklesung als entfernt behandelt. Drei neue Typen
  für zusammen 600 Forschungspunkte freigeschaltet.
- Gemeinsamer Achtteilplan mit Kräuterkundler (43,25,6), fünf Wegen über
  (45,21,6)/(45..48,20,6), Mühle (46,21,6) und Windturbine (45,28,8).
  Windturbine zunächst tiefer native_invalid, vor Start aufs höhere ebene Feld
  verlegt; dort native_valid. Keine ungeprüfte Kraftverbindung behauptet.
- Alle Teile separat fertig nachgewiesen, Gesamtprojekt completed. Fünf Wege
  und nächster Mühlenauftrag in begrenzter sequenzieller Agentenfolge verarbeitet;
  unveränderte native Prüfungen, keine erneute Sendung eines unbestätigten Auftrags.
- Kräuterkundler besetzt: Berries/Paper vorhanden, Dandelion fehlt. Mühle ohne
  ausgewähltes Rezept und ohne Kraft. Windturbine einzelnes Netz ohne Verbraucher;
  beobachtete Leistung 0. Bauabdeckung steigt auf 65/157, Betrieb separat offen.
- Anschließend begrenzter Dreitageslauf für fehlende Forschung; 206 Punkte
  erreicht, Bevölkerungszähler für 200 freigeschaltet. Einzelplan (46,19,6),
  Drehung 2, nativ gültig, Zugang verbunden. Nach zwölf Spielstunden separat
  fertig nachgewiesen und Projekt completed. Keine neue Rückfrage zwischen Bauten.
- Vier neue Typen in dieser Etappe, insgesamt 66/157 reguläre Typen, 91 offen.
  Kräuterkundlerzugang Distanz 74, Mühlenzugang 83. Abschluss Tag 537,
  14,125 Uhr pausiert. Unveränderte Spieleransicht ergänzend angesehen;
  keine Aufnahme gespeichert. Windleistung bleibt Momentaufnahme und ohne
  angeschlossenen Verbraucher kein produktiver Kraftnachweis.

## Fortsetzung: Papierlager, zweites Laufrad und Bienenstock

- Bienenstock für 400 Forschungspunkte freigeschaltet. Für obere Zufahrt zwei
  tote Kiefern geräumt: Jungbaum unmittelbar entfernt, Vollbaum erst nach
  Arbeiterauftrag und 16 Spielstunden separat als entfernt bestätigt.
- Gemeinsamer Achtteilplan: kleines Lager (41,25,6), fünf Wege über (43..45,24,6)
  bis (45,22,6), Laufrad (44,21,6), Drehung 3, Bienenstock (27,26,3).
  Erste Laufraddrehung 1 hatte falschen Port und keinen verbundenen Eingang;
  Plan vor Start korrigiert. Korrigierte Vorschau ohne verlorene Verbindungen.
- Lager fertig, Paper/accept konfiguriert und separat rückgelesen; später
  30 Papier im Lager. Fertiger Zugang Distanz 72. Zusatzlaufrad fertig/besetzt,
  Zugang Distanz 78; reales gemeinsames Fabriknetz mit beiden Rädern bestätigt.
  Tagsüber 106 Leistung bei 80 Bedarf, Überschuss 26; keine Nachtgarantie.
- Bienenstock fertig und separat gelesen. Gesamtprojekt completed. Wirkung
  auf Pflanzen nicht beobachtbar; entsprechende Diagnose im Backlog ergänzt.
  Abdeckung 62/157 reguläre Typen, 95 offen, fünf Entwicklervorlagen ausgeschlossen.
- Abschluss Tag 526, 12,46875 Uhr pausiert. Keine neuen offenen Projektteile;
  keine Codeänderung, kein Build und kein Versionsabschluss in diesem Abschnitt.

## Fortsetzung 2026-10-11: Papierfabrik und Vogelscheuche

- Obere trockene Baufläche geräumt: vier tote Kiefern im Fabrik-/Laufradgrundriss,
  danach ein abgeernteter Stumpf und ein toter Jungbaum für den Westzugang.
  Arbeiteraufträge erst nach tatsächlichem Entfernen als geräumt behandelt.
- Erster Achtteilplan hatte keinen verbundenen südlichen Laufradzugang und wurde
  nicht gestartet. Korrigierter Elfteilplan mit neun zusammenhängenden Wegfeldern
  nativ gültig; keine verlorenen Bestandsverbindungen in der Vorschau.
- Papierfabrik (41,21,6), Drehung 0, fertig/besetzt. Laufrad (43,23,6), Drehung 2,
  anschließend fertig/besetzt. Projekt completed, Fabrikzugang Distanz 69.
  Reale Verbindung Fabrik (42,22,6) zu Laufrad (42,23,6) bestätigt.
- Tagsüber Netz 53 Leistung bei 80 Bedarf. Zehn Papier im Fabrikbestand waren
  erster Produktionsbeleg; volle Leistung ausdrücklich nicht nachgewiesen.
  Das Vorschaufeld rotationMatches=false verhinderte reale Verbindung nicht.
- Vogelscheuche für 200 Forschungspunkte freigeschaltet, (43,20,6) nativ geprüft,
  gebaut und separat finished=true gelesen; Einzelprojekt completed.
  Insgesamt 61/157 reguläre Typen nachgewiesen, 96 offen, fünf Entwicklervorlagen
  weiterhin ausgenommen. Keine allgemeine Katalog- oder Versionsabnahme.
- Abschluss Tag 523, 18,46875 Uhr pausiert. Fabrikpuffer 18 Papier/10 Stämme,
  workingHours=false, kein laufender Auftrag; Nachtmomentaufnahme Kraft 0/0.
  Unveränderte Spieleransicht ergänzend betrachtet, Bild nicht gespeichert.
  Verbesserungen nur dokumentiert: gebündelte Sofortwege, Anschlussrotation,
  klare Semantik von actualInput bei unterversorgtem Netz.
Nutzer hat Fortsetzung trotz dokumentiertem Aufwand ausdrücklich beauftragt.

## Ziel und Plan

Flussebene für Landwirtschaft/Wald freimachen. Wohnen und übrige Betriebe auf
Z4 verlagern. Wasserbau, Pumpen und benötigte Zugänge funktional berücksichtigen.
Erste Wohnstufe: zwölf Häuser mit 36 Betten, danach Bedarf prüfen. Vorhandene
obere Straße als Hauptachse nach Norden verlängern; kurze Abzweige statt
verstreuter Einzelerschließung. Lager/Produktion neben dem Wohnviertel vorsehen.
Dies ist ein Zielplan, noch kein vollständig geometrisch geprüfter Gesamtplan.

## Nachgewiesener Zwischenstand

- Zwölf neue Lodges auf Z4 gebaut, jeweils `finished_accessible` bestätigt und
  als zwölf fertige Wohngebäude im Bestand rückgelesen. Keine Wohngebäude mehr
  auf Z3. Ein straßenblockierender Fehlbau wurde entfernt und zählt nicht mit.
- Durchgehende Hauptstraße mit seitlich ausgerichteten Wohnzeilen. Die erste
  freie Flächensuche verbaute die Straßenfortsetzung; schmale Suchregionen mit
  fester Drehung korrigierten dies. Noch kein Nachweis global optimaler Wege.
- Neues Krankenbett, kleines Holzlager und großes Holzlager auf Z4 fertig und
  erreichbar. Altes Krankenbett nach Ersatz entfernt. Altes großes Holzlager
  vollständig geleert und entfernt; neues auf Beschaffen gestellt.
- Sechs leere kleine Lager unten entfernt. Weitere frühere Freizeit-/Nebenbauten
  entfernt: Hammock, TeethGrindstone, Inventor, Sauna, Bench, ContemplationSpot.
- Spieler ergänzte oberen Treppenanschluss und eine Flussbrücke. Fremdbaustellen
  blockierten neue MCP-Bauprojekte global bis zur tatsächlichen Fertigstellung.
- 40 zusätzliche Karottenzellen auf ehemaligen Hausflächen markiert und
  Gesamtbestand von 89 Markierungen über drei Seiten rückgelesen. Markierung ist
  kein Nachweis bereits wachsender Pflanzen oder gesicherter Ernte.
- Westliche Lagercharge für Karotten, Bretter, Beeren und zweimal Wasser fertig
  und erreichbar. Zusätzlich Bauhütte, Transportposten, Erfinder, Grill,
  Bäckerei und Sägewerk oben fertig und erreichbar. Das neue Sägewerk hat noch
  keine nachgewiesene Kraftversorgung. Untere Kraft-/Industrieanlage verbleibt.
- Ein altes Karotten-Kleinlager sowie weitere Freizeit-/Nebenbauten verbleiben.
  Umzug als Gesamtauftrag nicht abgeschlossen.
- Nutzer priorisiert anschließend Hochhaus und alle Gebäudetypen, mit regelmäßiger
  Aufnahme seiner unveränderten Spielansicht. Frühere Sitzungssperre nach
  wirkungslosem Stopptest inzwischen korrigiert und live abgenommen.

## Höhenwohnblock am 2026-10-11

- Zusätzliche Lodge auf Z5, dann Doppelwohnhaus auf Z5 über bestehender Lodge
  auf Z4: dreigeschossiger Abschnitt, oberer Hauseingang auf Z6. Plattformen,
  Treppen und Wege sequenziell fertiggestellt. Doppelwohnhaus-Eingang frei,
  native Distriktdistanz 15. Keine achtgeschossige Gesamtanlage behauptet.
- Drei Doppelplattformen und obere Querwege fertig; anschließend Dachterrasse
  auf Z6 fertig und zugänglich. Beide Projektfolgen completed; Baustellenzugänge
  und fertige Gebäudezugänge getrennt beobachtet. Doppelplattform regulär für
  150 Forschungspunkte freigeschaltet.
- Terrasse zunächst native_invalid in gemeinsamer Vorschau, auf real fertigen
  Trägern bei gleicher Position/Drehung native_valid. Keine Validierung umgangen.
  Zugang über höheren Spielerweg hat native Distriktdistanz 72: erreichbar,
  jedoch kein optimaler Weg. Verbesserungsvorschlag in BACKLOG.
- Drei neue Typen gebaut: DoubleLodge, DoublePlatform, RooftopTerrace. Frühere
  Platform und PowerShaft waren noch nicht in der CSV; nach Nachweis ergänzt.
  Abdeckung jetzt 38 built_finished, 119 not_yet_verified, 5 developer_excluded
  bei 162 Katalogzeilen. Historischer Bau mindestens einmal, nicht Betrieb aller
  Gebäude oder gleichzeitiger Bestand sämtlicher früherer Typen.
- Ende dieser Etappe: Tag 478, 5,21875 Uhr, Pause bestätigt. Keine offene eigene
  Projektfolge; Screenshots aus der jeweils aktuellen Spielerkamera betrachtet,
  nicht gespeichert. Gesamtziel weiterhin offen.

## MCP-Befunde

Konkrete Verbesserungspunkte stehen im BACKLOG unter „Umzug auf Z4“:
wirkungsabhängiges Räumende, früh verständliche Fremdbaustellen-Diagnose,
gemeinsame Straßen-/Viertelplanung, Änderungsabruf seit Cursor und aktueller
Simulationszustand neben eingefrorenem Laufabschluss. Keine Umsetzung während
dieses Spielauftrags; menschliches Test-Gate bleibt unverändert.

## Terrassenabkürzung und Gebäudefolge 2026-10-11

Neun Bauteile mit zwei Treppenflügen fertiggestellt. Terrassenzugang nach einer
Spielstunde Navigationsaktualisierung mit nativer Distriktdistanz 24 statt 72,
Eingang frei; Doppelwohnhaus unverändert Distanz 15. Direkt nach Fertigstellung
zeigte die Abfrage noch den alten Distanzwert. Keine gemessene Gehzeit.

13 weitere Typen fertig: Roof1x1, Roof1x2, Roof2x2, Roof2x3, Roof3x2,
Weathervane, Lantern, Hedge, Shrub, WoodFence, Chronometer, Lever, Relay.
Gesamte Projektfolge completed; tatsächliche Baustellenzugänge und Fertigstatus
je Teil kontrolliert. Automationsbetrieb nicht nachgewiesen. CSV jetzt 51/157
reguläre Typen, 106 noch nicht nachgewiesen, fünf Entwicklervorlagen separat.

Danach frei verfügbares Rundholz erschöpft, 33 Bretter und 11 Zahnräder vorhanden.
Altes Sägewerk vorübergehend pausiert. Westlicher Brückenkopf hatte abgeerntete
Stümpfe. Erste kostenlose Holzfällerfahne fertig und zugänglich, jedoch keine
westlichen Ertragsbäume in Arbeitsreichweite. Fahne deshalb eine Zelle landeinwärts
versetzt und bisherigen Platz als Wegverlängerung genutzt; Erntenachweis offen.

## Holzversorgung, Eichen und nächste Vierergruppe

- Abgebrochenen Abschluss zuerst lesend geprüft: nicht ausgeführt. Nach expliziter
  Freigabe fertigen Einzelbau abgeschlossen. Nutzer erlaubt in diesem Spieltest
  generell Fortsetzung im Gesamtzustand waiting nach separatem Objektnachweis;
  terminale stopped/unconfirmed bleiben ausgeschlossen. Diese Aussage dokumentiert
  die Freigabe des konkreten Tests und ändert keine dauerhafte Agentenregel.
- Landseitiger Weg am westlichen Brückenkopf plus versetzte Holzfällerfahne fertig.
  Arbeitsplatz auf VeryHigh, ein Arbeiter tatsächlich zugewiesen. Fünf zuvor
  erreichbare markierte Bäume später als abgeerntete Reste nachgewiesen.
  Frei verfügbare Stämme von 3 auf 18 gestiegen, anschließend trotz Bauverbrauch
  wieder 20. Kein allein diesem Arbeitsplatz zugerechneter Gesamtertrag behauptet.
- 54 neue Eichenmarkierungen auf feuchtem, förstererreichbarem Boden gesetzt und
  vollständig rückgelesen. Anschließend alle 54 als lebende Pflanzen nachgewiesen,
  kein Wasserstress, Wachstum etwa 3 bis 12 Prozent. Alle stehen innerhalb bereits
  vorhandener Fällmarkierungen. Noch keine reife nachhaltige Ernte nachgewiesen.
- Siebenteilige obere Baugruppe vollständig completed: Treppe, Plattform, Weg,
  TripleLodge, TriplePlatform, FlowSensor, Levee. Vier weitere fehlende Typen fertig.
  Hauseingang auf Z7 frei, native Distriktdistanz 32; Haus auf trockenem Z6-Gelände.
  FlowSensor/Levee hier Bau-Test auf dem Plateau, keine Strömungs-/Stauwirkung.
- Abdeckung jetzt 55/157 reguläre Typen, 102 offen, fünf Entwicklervorlagen separat.
  Kolonie zuletzt 47 Biber bei 51 Betten. Sägewerk nach Holzaufbau wieder aktiviert.
  Screenshots aus unveränderter Spieleransicht angesehen, nicht gespeichert.
- Sechs tote, zuvor ertragreiche Birken zusätzlich regulär markiert; anschließend
  alle sechs als abgeerntet (cutYieldRemoved=true, Ertrag 0) nachgewiesen.
  Wieder aktiviertes Sägewerk: freie Bretter von 5 auf 13 gestiegen; Stämme teils
  im Produktionspuffer gebunden. Freie/gesamte Bestände nicht addieren.

## Warnungsbereinigung: Korrektur des früheren Terrassennachweises

Spätere Warnungsprüfung zeigte zwei verlorene Bestandszugänge: Lodge und SmallTank
teilten das Eingangsfeld der neuen Abkürzungstreppe. Kürzung auf Distanz 24 war
somit kein erfolgreicher Gesamtabschluss. Treppe entfernt und flachen Weg
wiederhergestellt; beide Bestandsgebäude wieder frei zugänglich, Distanz 15.
Terrasse weiterhin erreichbar, jetzt wieder Distanz 72. Neue konfliktfreie
Abkürzung bleibt offen; ursprüngliche Behauptung optimaler Endlösung zurückgenommen.

Zwei leere Lager auf Gear beziehungsweise Carrot eingestellt und rückgelesen.
Grill/Bäckerei mangels MCP-Rezeptwahl und Schrottfahne ohne erreichbaren Schrott
vorläufig pausiert; kein behobener Produktionsbetrieb behauptet. Wasserbauer
bekam neun native gültige und erreichbare Spadderdock-Markierungen auf Z2.
Oberes Sägewerk erhält eigenes Laufrad; Bau-/Leistungsnachweis noch ausstehend.
- Zwischenprüfung nach Warnungsreparaturen: 51 Biber, 54 zugängliche Betten,
  keine Obdachlosen. Damit reduziertes Bevölkerungsziel 50 erstmals aktuell belegt.
  Wasserfarm tatsächlich mit laufendem Arbeitsauftrag und ohne Statuswarnung.
- Laufrad mit drei Zuwegen fertig, Projekt completed. Reale Netzverbindung zum
  oberen Sägewerk bestätigt. Erste Abschlussabfrage abends ohne momentane Leistung;
  Arbeitsplatz besetzt. Alle showAlert=true-Gruppen verschwunden; normale Status
  Paused/Empty/Drying verbleiben. Tagesbetriebsprüfung folgt.

### Abschlusskontrolle und Eingriff des Spielers

- Morgens native Leistung 53 bei Bedarf 50, Sägewerk powered und actualInput 50.
  Ein Arbeiter aktiv, Zutaten vorhanden, 13 Bretter im Ausgangspuffer. Dieser ist
  aktuell voll (outputSpace=false); keine dauerhafte ungehinderte Produktion behauptet.
- Spieler ergänzte 100 Eichen-Pflanzplätze. Alle sechs Flächenseiten gelesen:
  154 Oak und 18 Pine, insgesamt 172. Die bisherigen 54 Eichen waren als lebende
  Pflanzen belegt; für die neuen 100 ist hier nur der Pflanzauftrag vollständig belegt.
  Aktuelle Spieleransicht unverändert aufgenommen und angesehen, nicht gespeichert.
- Keine showAlert=true-Gruppe. Drei bewusst pausierte Betriebe bleiben eingeschränkt;
  Rezeptwahl für Grill/Bäckerei und Schrottversorgung sind weiter offen.
  Zusätzlich gewöhnliche Status Emptying, Drying sowie eine kontaminierte Birke
  am Kartenrand (9,0,2); kein kontaminierter Biber in dieser Statusgruppe.
- 53 Biber, 54 Betten, keine Obdachlosen. Simulation an Tag 497, 9,3125 Uhr pausiert.
  Bevölkerungsteilziel erreicht, Gebäudeziel 55/157; konfliktfreie Terrassenkürzung offen.

## Fortsetzung: fertiger Pegel, undiagnostischer Schutzstopp

- StreamGauge, VerticalPowerShaft und PaperMill regulär freigeschaltet. Beide
  Sägewerke wegen voller Bretterpuffer vorläufig pausiert und Pause rückgelesen.
  Begrenzter Holzlauf brachte zunächst neun freie Stämme; für kleinen Bau unterbrochen.
- Zweierplan auf trockenem Z6-Plateau nativ gültig, keine verlorenen Verbindungen
  in Vorschau. Pegel bei (44,13,6) beauftragt, Baustellenzugang buildersReachable=true;
  nach 20 Spielstunden separat found=true/finished=true. Bau-, kein Wasserfunktionsnachweis.
- Folgender Advance bei bestätigter Pause stoppte vor weiterer Platzierung mit
  guard_changed. Kraftwelle separat nicht gefunden, Projekt terminal stopped;
  keine Wiederholung oder Ausweichprojekt gestartet. Ledger sperrt Folgeprojekte.
- Keine offenen Baustellen, keine neuen HUD-Alerts. Benachbarte TripleLodge weiter
  zugänglich, jetzt native Distanz 62. Guard prüft interne Fehler, Bauisolation,
  Bestandsverbindungen und Vorgänger, nennt aber weder Teilursache noch Zielobjekte.
  Ursache deshalb nicht behauptet. Gegenüber älterem Bestandsinventar fehlen sechs
  Wege bei x21..26/y2/z3; Zeitpunkt und Ursachenzusammenhang unbekannt.
- Abdeckung 56/157, 101 offen. Zuletzt 6 freie Stämme, 38 freie Bretter, 20 Zahnräder.
  54 belegte Betten, keine freien Betten oder Obdachlosen. Tag 499, 10,84375 Uhr pausiert.
  Zum Weiterbauen neue Spielsitzung durch Speichern/Neuladen erforderlich; kein Build-Gate.

## Neue Sitzung: Kraftwelle, Tank und obere Holzernte

- Nach menschlichem Neuladen neue Session bestätigt. Pegel vorhanden/fertig,
  alte unbeauftragte Kraftwellen-ID nicht vorhanden. Neu geplante Kraftwelle
  (44,12,6) fertig, Einzelprojekt completed; kein erneuter Schutzstopp.
  Nur Bau nachgewiesen, keine produktive Kraftverbindung dieser Welle.
- Mittlerer Tank (41,18,6) nach Räumung zweier toter Kiefern fertig; ein neues
  Wegfeld (41,17,6), Projekt completed. Water/accept rückgelesen, anschließend
  23 Wasser eingelagert. Nachbarhaus weiterhin zugänglich, Distriktdistanz 62.
- Vorherigen Tankentwurf über Bestandswegen verworfen (24 verlorene Verbindungen
  in Vorschau). Regionalsuche lieferte anschließend gedrehten Kandidaten über
  Geländekante; native Prüfung wies ihn zurück. Konkreter Befund im Backlog.
- Zusätzlicher kostenloser Holzfäller (40,19,6), Drehung 1, fertig und besetzt,
  Einzelprojekt completed. 16 neue Fällmarkierungen auf oberem trockenen Bestand
  rückgelesen. Fünf tote ertragreiche Vollbäume danach cutYieldRemoved=true;
  freie Stämme 4 auf 14. Bestandsziel pausierte nach 12,3125 Spielstunden.
  Westlicher Holzplatz dagegen 27 abgeerntete Reste, neun Objekte ohne Ertrag,
  keine erntbaren Kandidaten in untersuchter 8×8-Fläche.
- Kurze Hängebrücke (42,13,6), Drehung 2, nativ gültig und beauftragt,
  Bauarbeiterzugang bestätigt. Fertigstellung noch ausstehend.
- Brücke anschließend fertig und Projekt completed. Erst Stämme geliefert,
  Bretter fehlten; oberes Sägewerk aktiviert, konkurrierende Zahnradproduktion
  vorläufig pausiert, beide Änderungen rückgelesen. Nach 18 Spielstunden fertig.
  Entranceloser Brückenbauteil: native Eingangswarnungen false, keine erfundene
  Gebäudetür; keine neue durchgehende Verkehrsroute behauptet.
- Abschließend 59/157 reguläre Typen, 98 offen. Tankbestand 175 Wasser.
  54 Biber, 54 belegte Betten, keine Obdachlosen. Freie Baustoffe 8 Log, 15 Plank,
  7 Gear. Tag 503, 14,71875 Uhr pausiert, keine offenen bekannten Bauprojekte.
- Oberer Holzfäller nach erster Ernte mit Nothing to do in range. Zwei benachbarte
  tote Vollkiefern mit je zwei Stämmen zusätzlich markiert und Markierungen
  rückgelesen; deren Erntewirkung noch nicht bestätigt. Unteres Sägewerk und
  Zahnradwerkstatt vorläufig pausiert, oberes Sägewerk läuft.
  Screenshot der unveränderten Spieleransicht betrachtet, nicht gespeichert.

## 15-Tage-Wachstumslauf auf ausdrücklichen Nutzerwunsch

- Dusche für 50 Forschungspunkte freigeschaltet. Zwei abgeerntete Kiefernstümpfe
  und eine tote Kiefer ohne Ertrag bei (41..43,20,6) regulär entfernt/rückgelesen.
- Nutzer wünschte 15 Tage Wachstum statt weiterer kurzer Bauläufe. Exakt
  360 Spielstunden bei Tempo 7, ohne Bestands-Frühstopp; completed/target_reached,
  overshoot 0, Pause bestätigt. Tag 504, 6,71875 Uhr bis Tag 519, 6,71875 Uhr.
  Laufzeit etwa 17 Minuten 40 Sekunden real. Keine Unterbrechung des Laufs.
- Vollständiger Markierungsabgleich: 159 Eichen- und 18 Kiefern-Pflanzplätze.
  Vier Eichenplätze ohne Fällmarkierung ergänzt und separat rückgelesen.
  Förster besetzt. Nach knapp fünf Tagen 41 freie Stämme; oberes Sägewerk und
  Zahnradwerkstatt wieder aktiviert und rückgelesen, unteres weiterhin pausiert.
- Repräsentative 4×4-Überlappungsstichprobe am Ende: zwei lebende erntereife
  Eichen mit je acht Stämmen, ein nachweislich abgeernteter Rest, mehrere neue
  Jungpflanzen und weitere unreife Bäume. Kein Wasserstress in der Stichprobe.
  Keine Hochrechnung auf vollständige Waldabdeckung. Frei danach 144 Stämme,
  33 Bretter, 29 Zahnräder; gebundene Bestände getrennt betrachtet.
- Nachlaufende Duschplanung auf Z6 mit Treppe, Podest und Zugang auf Z7:
  native_invalid für Dusche, daher kein Start/Auftrag. Podestweg wäre erst nach
  realer Stütze validierbar; das hebt die unabhängige Duschablehnung nicht auf.
  Platzierungsursache noch offen, geräumte Fläche bleibt nutzbar. Abdeckung 59/157.
