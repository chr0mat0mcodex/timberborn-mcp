# Budgetgerechte ebene Routensuche — 0.35.1

Status: 0.35.1 nach menschlichem Gate einschließlich isolierter
Überbudget-Kandidatenkontrolle live bestanden.

## Abschluss der Grenzkontrolle

Kontrollierter ebener 1x7-Streifen: sechs exakt gelesene Wegobjekte regulär
entfernt, einziges verbleibendes Wegziel am Ende nativ als distriktverbunden
bestätigt. Gelände aller Zellen eben, trocken und oberirdisch; Rücklesung zeigt
keine weiteren Objekte im Streifen. Beide Vergleichsgrundrisse ohne Kollision
oder Geometriefehler, Eingang jeweils eine Zelle in Richtung Anschluss versetzt.

Erster Bauplatz benötigt fünf neue Wegfelder und fehlt korrekt im Ergebnis.
Unmittelbar folgender Bauplatz benötigt vier und erscheint als erste Option.
Ergebnis: fünf Kandidaten geprüft, einer verworfen, vier Vorschläge mit 4/3/2/1
neuen Wegen. Der erste Kandidat wurde damit vor Erreichen des Vorschlagslimits
geprüft, nicht durch dieses verdeckt. Wiederholung liefert identische Daten und
Planschlüssel. Kein Bauauftrag aus dieser reinen Grenzkontrolle; der vollständige
Bau-/Zugangsablauf ist separat unten belegt. Spiel abschließend pausiert.

Die sechs entfernten Wege bleiben als offener Teststreifen bestehen. Dadurch
getrennte Bestandsanschlüsse sind eine beabsichtigte Testvorbereitung, kein
Nachweis einer verlustfreien Abrissaktion. Kein automatischer Wiederaufbau.

## Live-Teilergebnis am 2026-10-03

- Profil einschließlich Bank-/LargePile-Historie korrekt; reguläre Ausführung
  und allgemeiner Bauphasen-Vorabnachweis bleiben ausdrücklich false.
- Wiederholte Suche bei pausiertem Spiel: identische vier Optionen und Schlüssel,
  null bis drei neue Wege. Weitere kleine Suchfläche liefert genau vier neue
  Wege als Kandidat. Ohne erreichbaren Bestandsanschluss keine Vorschläge.
- Bank mit drei neuen Wegen um einen unveränderten Baum: native gemeinsame
  Vorschau mit 566 Anschlussprüfungen, null Verlusten und drei verlustfreien
  Wegpräfixen. Realer Bauzugang bestätigt; nach exakt vier Spielstunden fertig,
  Eingang frei und angeschlossen. Wohnhaus und höheres Lager weiterhin erreichbar.
- Replay liefert denselben bestätigten Auftrag. Gefährliche Hauptweg-Überbauung
  nur in Vorschau: 28 verlorene Anschlüsse erkannt, abgelehnt und restauriert.
- Zwei selbst angelegte Testwege für eine isolierte Budgetkontrolle gelöscht.
  Der gedachte Fünf-Wege-Standort scheitert jedoch bereits an Geländeüberlagerung;
  das ist KEIN Nachweis der Budgetablehnung. Kleine angrenzende Suchausschnitte
  ebenfalls durch Gelände beziehungsweise Vorschlagslimit ungeeignet.
- Testzweig anschließend über einen zweiten regulären Drei-Wege-Baupilot wieder
  angeschlossen; erster Bankzugang wieder mit unveränderter Distriktdistanz.
  Zweite Bank nach weiteren exakt vier Spielstunden ebenfalls fertig und
  erreichbar. Beide Eingänge abschließend frei, Simulation pausiert.

Die zunächst offene Fünf-/Vier-Wege-Kontrolle wurde anschließend im oben
beschriebenen kontrollierten Streifen bestanden. Geländesperre, fehlendes Ziel
oder vorzeitiges Optionslimit dürfen nicht als Budgetablehnung gezählt werden.

## Änderung

Die bisherige Suche konnte ihre vier Vorschlagsplätze mit Projekten belegen,
deren neue Wege das Ausführungslimit überschritten. Jetzt sucht sie innerhalb
derselben maximalen 8x8-Fläche nach der Route mit den wenigsten neuen Wegfeldern;
bei Gleichstand gewinnt die kürzere Route. Bereits vorhandene fertige Wege
kosten kein Baubudget. Der Eingang zählt, wenn dort noch ein Weg gebaut wird.
Mehr als vier neue Wege verwerfen den Standort, nicht den restlichen Suchlauf.
Ausführung und Suche verwenden dieselbe Grenzkonstante.

Vollständige Grundfläche bleibt für Routen gesperrt. Ein Bestandsweg ist nicht
automatisch ein Distriktanschluss: Zielpunkte müssen im beobachteten Distriktnetz
liegen. Die Suche bleibt Kandidatensuche; native gemeinsame Vorschau,
Bestandsverbindungsschutz, tatsächlicher Bauzugang und fertiger Zugang bleiben
unverändert erforderlich. Keine erhöhte Aktionsgrenze und keine freie 3D-Planung.

## Gezielte Abnahme nach menschlichem Gate

1. Bridge 0.35.1 und Profil lesen; Bank und LargePile jeweils nur mit historischer
   Drehung 1, keine pauschale neue Sicherheitsbehauptung.
2. Kleine Suchfläche mit nahen und über dem Vier-Wege-Budget liegenden Bauplätzen
   prüfen: alle Vorschläge maximal vier neue Wege, wiederholte unveränderte Suche
   identische Optionen/Schlüssel. Abgelehnte entfernte Kandidaten dürfen die Suche
   nach näheren Standorten nicht vorzeitig beenden.
3. Einen passenden ebenen Anschluss um eine belegte Zelle oder an einer
   vorhandenen Wegstrecke gemeinsam nativ validieren und ausführen; reale
   Erreichbarkeit, fertigen Zugang und betroffene Bestandsanschlüsse zurücklesen.
   Einen ungeeigneten Fall ohne baubare Route als Negativkontrolle prüfen.
4. Keine zufällige breite Koloniesuche: fehlt ein geeigneter kontrollierbarer
   Testausschnitt, gezielt eine kleine Testfläche vorbereiten. Bei widersprüchlichen
   Ergebnissen diagnostizieren, nicht weitere positive Fälle anhängen.

Synthetische Tests decken Vier-/Drei-Felder-Grenze, längere Bestandswegroute mit
geringerem Neubedarf, Längen-Gleichstand, Grundrisssperre, vorhandenen Eingang,
Eingabeprüfung und neue Versionsbindung ab. Sie ersetzen keinen Spielnachweis.
Ein Neubau-Umweg als Schutz für eine spätere Bestandsänderung ist damit weiterhin
nicht automatisch bewiesen; dafür muss der Umweg zuerst tatsächlich nutzbar sein.
