# Generischer ebener Gebäudeprojektpilot — 0.35.0

Stand: 2026-10-03, 0.35.0 nach menschlichem Gate geladen und begrenzten
MCP-Livetest bestanden. Neue Vorlagen Bench.Folktails (ein Feld, zwei neue Wege)
und LargePile.Folktails (3x3 Grundfläche, ein neuer Weg), beide in Drehung 1,
regulär gebaut; Bau- und Fertigzugang getrennt bestätigt.

## Änderung

Keine Namensliste mehr im ebenen Projektpilot. Der aktive Spielkatalog löst die
Vorlage auf; vorhandene technische Kataloggrenzen, Freischaltung und Verfügbarkeit
bleiben maßgeblich. Planung verwendet alle tatsächlich belegten Geometriezellen
in der gewählten Drehung, nicht bloß Breite/Höhe eines Rechtecks. Auf der Wegebene
werden sämtliche Grundrisszellen vor der Routenwahl ausgeschlossen. Der gewählte
Eingang muss innerhalb der Suchfläche auf dieser Ebene per Bodenweg an einen
tatsächlich verbundenen Distriktweg anschließbar sein.

Der Routenbaustein akzeptiert mehrere Zugangskandidaten und wählt eine kürzeste
gefundene Route; ein brauchbarer Zugang genügt. Bei Gleichstand gilt Kandidaten-
reihenfolge. Kein globales Optimum über Grundstücke oder Baukosten.

**Spielseitige Grenze:** Öffentliche Metadaten der installierten Spielbibliotheken
liefern `BlockObjectSpec.Entrance`/`EntranceBlockSpec.HasEntrance` und
`PositionedEntrance.From(...)`, genau einen Wegzugang. Auch `BlockObject` hat
einen `PositionedEntrance`; `BuildingAccessible` liefert keine Eingangsliste.
Keine zusätzliche öffentliche Eingangsquelle nachgewiesen. Die Bridge liefert
deshalb nur diesen einen Zugang, keine erfundenen weiteren Türen. Mehrere
Zugangskandidaten sind synthetisch testbar, nicht als Spiel-Multi-Eingang belegt.
Fehlender Zugang erzeugt keinen Bauplan. `entranceModel` und Limitations nennen
diese Grenze ausdrücklich.

## Unveränderte Schutzprüfungen

- Frische Sitzung, gebundener PlanKey und Replay-Ledger.
- Gemeinsame native Vorschau für Gebäude und sämtliche neuen Wege; Vorschau
  rückgängig und ohne persistente Veränderung, keine bekannten Anschlussverluste.
- Neue Projekte nur ohne unabhängige offene Baustellen; pausierte Platzierung,
  fertige Vorgänger und Rücklesen jeder tatsächlichen Entity.
- Erfasste Bestandsanschlüsse vor und während der Auftragsschritte erhalten.
- Tatsächlicher Distriktanschluss am Eingang und bei unfertigem Gebäude realer
  Bauarbeiterzugang. Regulär direkt fertig platzierte Objekte brauchen keine
  Baustellenkomponente, aber denselben Eingang-/Distriktnachweis.

Grenzen: maximal 64 Objektzellen, 8x8 Suchfläche, eine Wegeebene und eine Drehung
pro Anfrage, vier neue Bodenwege und vier sequenzielle Projekte. Keine neue
freie Vertikalplanung; feste Vertikalpiloten bleiben unverändert. Allgemeiner
Bauphasen-Vorabnachweis weiterhin `unknown`, reguläre Ausführung weiterhin
gesperrt. Auftragserfolg nicht mit Fertigbau, Lieferung oder Betrieb verwechseln.

## MCP-Profil und Kompatibilität

0.35.0 verwendet `templateSelection=native_catalog_supported_geometry_with_road_entrance`;
`objectTemplates=[]` bedeutet hier dynamischen Katalogumfang, nicht keine Vorlagen.
Historische `templateEvidence` begrenzen nicht die Vorlagewahl und behaupten
keine Prüfung beliebiger Gebäude. Lodge-Drehung 3 aus der 0.34.0-Abnahme aufgenommen.
Alte Bridges behalten ihre festen Vorlagen in der Receipt-Prüfung. 0.35.0 kennt
Folktails und IronTeeth; IronTeeth bekommt nur den generischen ebenen Modus und
keine übertragenen Folktails-Livenachweise oder festen Vertikalmodi.

## Bestandener Live-Nachweis

Bereitmeldung nach dem menschlichen Skript-Gate; laufende Bridge-Version 0.35.0
und neues Fähigkeitsprofil strukturiert bestätigt. Keine neue automatische
Testzählung übermittelt. Der gesamte Katalog mit 162 Vorlagen wurde seitenweise
gelesen. Im gewählten dicht bebauten Bereich fanden Lagerfeuer/Bauhütte keinen
passenden Grundriss; die Bank bot einen geeigneten begrenzten Abnahmefall.

- Bank, Drehung 1, ein Grundrissfeld, versetzter Eingang und zwei neue Wege:
  gemeinsame Vorschau für alle drei Objekte gültig, Eingang verbunden,
  562 geprüfte Verbindungen ohne Verlust, beide Wegpräfixe ohne Verlust,
  Vorschau restauriert und keine persistente Änderung beobachtet.
- Negativkontrolle auf einem bestehenden Hauptweg: native Platzierung ungültig,
  `roadProtection=blocked`, 24 verlorene Vorschauverbindungen erkannt, darunter
  der Zugang eines erhöhten Lagers. Vorschau restauriert, kein Bauauftrag.
- Identischer Suchbereich eine Ebene höher: keine Bodenwegoption. Separater
  Neun-Zellen-Grundriss mit Belegung/Kartenrand ebenfalls als blockiert erkannt.
- Echter Projektauftrag mit drei bestätigten Schritten; Baustelle meldete
  `buildersReachable=true` und drei tatsächliche Bauzugangszellen. Diese Zellen
  sind Baustellenzugänge, keine drei Gebäude-Eingänge.
- Ein begrenzter Lauf über zwölf Spielstunden, Ziel exakt erreicht, Überschuss
  null, automatische Pause. Bank danach fertig, Eingang frei und zugänglich,
  Distriktdistanz 47. Beide neuen Wege fertig und nativ distriktverbunden.
- Zwei bestehende Wohnhauszugänge und ein erhöhtes Lager blieben frei und
  zugänglich; Distanzen 45/50/53 unverändert. Unterer und oberer Vergleichsweg
  weiterhin nativ mit dem Distrikt verbunden.
- Exaktes Replay nach Fertigbau lieferte dieselben drei bestätigten Schritte;
  Objektzahl unverändert bei 190 (vor Neubau 187). Abschluss pausiert.

## Zusatzabnahme: großes Gebäude

Auf Nutzerwunsch anschließend LargePile.Folktails mit neun Grundrissfeldern
getestet, in derselben Sitzung und auf unveränderter Bridge 0.35.0.

- Exakt zehn identifizierte Beerenbüsche für Grundriss und Eingang regulär zur
  Entfernung markiert und durch Biber entfernen lassen. Benachbarter Busch und
  bestehender Weg erhalten. Keine Sofortlöschung oder Spielstandmanipulation.
- Zunächst acht Grundrissfelder freigeräumt und ein Busch am gegenüberliegenden
  äußeren Eckfeld belassen: Ursprung frei, aber Vorprüfung meldete genau diese
  eine Kollision. Native Validierung ebenfalls `valid=false`, keine dauerhafte
  Änderung. Nach Entfernung des letzten Buschs genau eine passende Planoption.
- Gemeinsame Vorschau von großem Lager und neuem Weg gültig; Eingang verbunden,
  alle neun Kandidatenzellen enthalten, 564 Vergleichsverbindungen ohne Verlust,
  Wegpräfix ohne Verlust, restaurierter Ausgangszustand.
- Beide Auftragsobjekte bestätigt; tatsächliche Baustelle mit
  `buildersReachable=true` und 15 beobachteten Bauzugangszellen. Fertiger Weg
  nativ mit dem vorhandenen Distrikt verbunden.
- Nach begrenzten Läufen von vier und zwölf Spielstunden Lager fertig,
  Eingang frei/zugänglich, Distriktdistanz 5. Nachbarwohnhaus und Sammelstelle
  weiter frei/zugänglich, Distanzen 4/3 unverändert; erhöhtes Vergleichslager
  weiterhin Distanz 53. Neuer und ursprünglicher Anschlussweg verbunden.
- Replay des großen Projekts und des vorherigen Bankprojekts unverändert;
  zwei bzw. drei bestätigte Schritte, insgesamt 192 Gebäude/Wege vor und nach
  dem Replay. Alle Simulationsziele exakt erreicht, Schlusszustand pausiert.

Das belegt auch die vollständige Grundrissprüfung außerhalb der Ursprungszelle
und zwei unterschiedliche generische Vorlagen nacheinander. Lagerkonfiguration,
Einlagerung oder Betrieb waren nicht Gegenstand dieses Zugangstests.

## Verbleibende Abdeckung

Kein Spiel-Multi-Eingang und kein vollständiger Vorschau-Bauphasenschutz belegt.
Beide neuen Vorlagen live nur in Drehung 1, keine pauschale Abnahme aller Katalogobjekte.
Umweg-Routing in diesem Lauf nicht zusätzlich live geprüft. Einziger im Katalog
gefundener unterstützter/freigeschalteter Direkt-fertig-Typ mit Eingang war das
Distriktzentrum; dessen zusätzlicher Distrikt-Lebenszyklus wurde hier nicht
getestet. Dieser Sonderfall bleibt ausdrücklich ohne Live-Nachweis.

Das ausgelieferte MCP-Profil enthält den Historienstand vor dieser Abnahme
(`generic_geometry_scope_not_live_proven`, noch keine Bank-/LargePile-Nachweise). Beim
nächsten ohnehin nötigen MCP-Gate aktualisieren; keine Neuinstallation allein
für die nachträgliche Historie.
