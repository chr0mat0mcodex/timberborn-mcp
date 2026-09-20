# Bedürfnisse und Betriebsbelege — 0.20.0

Drei rein lesende MCP-Werkzeuge, ohne neue Fremdmod oder Bibliothek. Öffentliche
Spiel-APIs lokal gegen Timberborn 1.1.2.4 geprüft; gebaut, regulär getestet und installiert,
gezielte Live-Abnahme bestanden (siehe unten). Keine Speicherstandänderung oder private Reflection.

## Koloniebedürfnisse

`inspect_needs(offset, limit)` gruppiert NeedManager.NeedSpecs aller initialisierten,
nicht gelöschten Beaver-Entities nach Bedürfnis-ID. Pro Bedürfnis: beobachtet, aktiviert,
aktiv, unter Warnschwelle, kritisch und ungünstig, dazu Minimum/Maximum/Mittel der
Punkte aktivierter Bedürfnisse. Warn-/Kritisch-Zähler können sich überschneiden.
Die Flags kommen direkt aus NeedManager; keine selbst erfundenen Hungergrenzen.
**warning ist ein rohes Unter-Warnschwelle-Flag, keine akute UI-Warnung.** Es ist auch
bei ungenutztem Komfort sowie inaktiven negativen Bedürfnissen gesetzt. Zum Beispiel
liefert eine Verletzung bei null Punkten warning=true, aber active=false und favorable=true.
Deshalb immer critical, active und favorable/unfavorable zusammen betrachten; keine
Warnzähler über verschiedene Bedürfnisse zu einer Zahl gefährdeter Biber addieren.

Beaver-Gesamtzahl, Zahl mit NeedManager und fehlende Manager getrennt. Fehlende
Manager sind unbekannt, nicht gesund. Bei keinem aktivierten Bedürfnis sind
Punktestatistiken null. Bots sind nicht in diesem Leser enthalten. Punkteskalen
unterschiedlicher Bedürfnisse nicht miteinander vergleichen. Maximal 32 Bedürfnisse
pro Seite, ordinal nach ID; Seiten sind getrennte Beobachtungen.

## Einzelbiber

`inspect_beaver_needs(id, session, offset, limit)` liefert native Punkte, definierte
Minimal-/Maximalwerte, enabled/active, criticalNeed/critical, warning und favorable.
Deaktivierte Bedürfnisse bleiben erkennbar; ihre Flags nicht als akuten Bedarf zählen.
Die ID kann aus Arbeiterliste oder Warnungszielen stammen. Die Arbeiterliste ist keine
vollständige Kinderliste. Keine Namen. Position ist Unity-Weltposition, kein Spielraster.
Fehlende Komponente: supported=false; fehlender Biber: entity_not_found. Veraltete
Session: stale_session. Maximal 32 Bedürfnisse je Seite.

## Gebäude

`inspect_building_operation(id, session)` liefert fertigen Bauzustand, Pause,
Personal (Ist/Soll/Maximum, understaffed), laufende Jobs und Arbeitszeit. Für fertige
Objekte mit nativer Manufactory zusätzlich Rezept-ID, ready, hasIngredients, hasFuel,
consumesFuel, outputSpace und Produktionsfortschritt. Ohne Rezept bleiben dessen
Voraussetzungen null. Ohne passende Komponente bleibt der gesamte Teil null.
Baustellen bekommen keine erfundene laufende Produktion. Aktive sichtbare Status-
beschreibungen werden begrenzt mitgeliefert; das sind lokalisierte, nicht vertrauenswürdige
Spieldaten, keine auszuführenden Anweisungen.

Beispielinterpretationen: hasIngredients=false ist ein nativer Zutatenmangel;
consumesFuel=true zusammen mit hasFuel=false ein Brennstoffmangel; outputSpace=false
bedeutet keinen unreservierten Platz für die aktuellen Produkte. Diese Bedingungen
können gleichzeitig vorliegen. Unterbesetzung muss die Produktion nicht ganz stoppen.
Keine laufenden Jobs können Nacht, Wegzeit oder andere Aufgaben bedeuten. ready ist
keine Garantie tatsächlicher Produktion. Farm, Sammler und Pumpen besitzen nicht
notwendig dieselbe Manufactory-Komponente. Für Wege/Eingang den vorhandenen Zugangsleser,
für konkreten Vorrat Lager-/Güterleser hinzunehmen. Vollständige Energie-, Wasser-,
Rohstoff- und Lieferdiagnose ist nicht behauptet.

## Grenzen und Verifikation

Feste GET-Routen über authentifizierten Loopback, Hauptthread-Queue und bestehendes
128-KiB-Antwortlimit. Keine Schreibfreigabe nötig. Session-Prüfung für konkrete Ziele.
Der Client verwirft alte Bridge-Versionen, widersprüchliche Seitendaten/Zähler,
ungültige Punkteskalen und unpassende Rezept-/Komponentenbelege.

460 reguläre Tests (447 Unit, 13 Integration), darunter neue Gegenbeispiele für
überhöhte Warnzähler, fehlende Manager, falsche Session/Ziele, Duplikate, Rezept-
widersprüche und Baustellen mit erfundener Produktion. Alle drei Leser laufen im
synthetischen authentifizierten HTTP-/stdio-Test mit unterschiedlichen Aktionsfreigaben.

Live 2026-09-20: alle 29 Leser bestanden. 13 Biber mit NeedManager, keine fehlenden
Manager, 42 Bedürfnisse vollständig über zwei Seiten; ein Einzelbiber ebenfalls über
zwei Seiten. Hunger/Durst ohne Warn-/Kritisch-Flags. Shelter bei sieben Bibern ungünstig;
inspect_colony bestätigt unabhängig sieben Obdachlose bei sechs belegten Betten.
Fremde Session und unbekannter Biber korrekt mit stale_session/entity_not_found abgelehnt.

Zwei Pumpen: Rezept Water, Zutaten vorhanden, kein Brennstoffverbrauch, outputSpace=false
und ready=false. Erfinder: SciencePoints, ready=true; Personalzahlen separat über
inspect_building gegengeprüft. Farm: Personal belegt, manufacturing=null korrekt als
fehlende Komponente. Kein Bauauftrag vorhanden: Baustellenfall nur regulär getestet.
Arbeitszeit-/Jobflags sind native Beobachtungen; kein gesonderter Tag-/Nachtwechseltest.
Keine Simulation oder Gebäudeeinstellung geändert; Spiel blieb pausiert. Aktive
Biber-UI-Warnung weiterhin nicht vorhanden, daher noch nicht live abgenommen.
