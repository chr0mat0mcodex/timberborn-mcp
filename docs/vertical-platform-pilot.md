# Plattformpilot — Testübergabe

Bridge 0.29.2 erweitert `execute_vertical_stair_pilot` um
`mode=stair_platform_pilot`, ausschließlich mit `upperPathCount=2`.
Die bisherigen Modi bleiben verfügbar.

Schritte: Treppe, erste Plattform, erster oberer Weg, zweite Plattform,
zweiter oberer Weg. Jede Plattform ist eine Ebene niedriger als ihr Weg.
Alle Koordinaten werden aus Ursprung und Rotation berechnet; pro Objekt eigene ID.
Jede Platzierung prüft frische Spielvorschau und Wegschutz-Wiederherstellung.

## Baustellenphase 0.29.2 — live bestanden

Vor jedem weiteren Schritt müssen alle zuvor platzierten Objekte exakt vorhanden
und fertig sein. Solange Träger unfertig sind, liefert der Pilot `state=waiting`
mit `reason=awaiting_construction_finished`; der folgende Schritt bleibt `pending`.
Simulation über bestehende begrenzte MCP-Läufe ist in dieser Phase erlaubt.
Der Pilot steuert die Geschwindigkeit nicht selbst. Sind die Träger fertig,
wartet er mit `awaiting_pause`, bis das Spiel pausiert ist. Erst dann läuft die
frische Spiel-/Wegschutzprüfung und genau ein neuer Platzierungsaufruf.

Fehlende/veränderte Objekte oder verlorener unterer Distriktanschluss stoppen den
Pilot. Eine Wartephase endet nach maximal 300 realen Sekunden, inklusive Pausewartezeit.
Keine Wiederholung unbestätigter Platzierungen, kein Wiederanlauf terminaler Aufträge,
kein pauschales Lockern des Navigationsabgleichs. Andere Pilotmodi bleiben unverändert.
Wartezustand und Zustandsübergänge sind synthetisch abgedeckt; Tests/Build/Installation
werden ausschließlich durch das menschliche Skript durchgeführt.

Erwarteter neuer Live-Nachweis: Treppe platzieren, Wartezustand lesen, begrenzte
Bauphase bis automatischer Pause laufen lassen; entsprechend beide Plattformen
fertigstellen, beide Wege setzen und alle fünf Objekte samt `completed` zurücklesen.
Falls Baustellen außerhalb des Pilots den Abgleich stören, bleibt der Wegschutz
geschlossen und die Diagnose wird gelesen.

Abschlussnachweis am 2026-10-03 nach menschlichem Skriptlauf: Vorprüfung lehnte
Abrissschutt vor Annahme des Auftrags ab; nach gezielter Entfernung neuer Auftrag.
Treppe, erste Plattform, erster Weg, zweite Plattform und zweiter Weg wurden genau
einmal gesetzt und bestätigt. Zwischen Trägern funktionierten `waiting` und drei
separate begrenzte Simulationsläufe von jeweils 0,6 Spieltagen mit automatischer
Pause. Folgeschritte erfolgten erst nach Fertigstellung und Pause. Endstatus
`completed`/`order_and_access_confirmed`; alle fünf Objekte unabhängig vorhanden
und mit `finished=true` zurückgelesen. Spiel abschließend pausiert.
Kein pauschales Lockern des Wegschutzes, kein Nachweis vollständiger vertikaler
Distriktanbindung. Timeout und Abhängigkeitverlust sind synthetisch abgedeckt,
aber nicht zusätzlich als destruktiver Livefall getestet.

Liveversuch mit 0.29.0: Treppe bestätigt, erster oberer Weg nicht platziert;
eine tote Kiefer belegte dessen Zelle. Plattform und zweiter Weg wurden nicht
versucht. Die anschließende Spielvorschau meldete `valid=false`, der Vorcheck
`object_intersection`. Kein Nachweis für oder gegen Plattformunterstützung.
Die Vorprüfung in 0.29.1 kontrolliert deshalb alle Zielgrundrisse gegen reale Objekte,
bevor ein Pilot angenommen wird; abhängige Stützen bleiben pro Schritt zu prüfen.

Vor dem Versuch verursachten alte Treppe und oberer Weg einen
`navigation_preview_baseline_mismatch`. Nach gezieltem Entfernen beider Objekte
war der Abgleich konsistent, ohne verlorene Verbindungen und mit bestätigter
Wiederherstellung. Eine unfertige Testtreppe kann damit spätere Vorschauen stören;
kein Grund für blindes Wiederholen oder eine vollständige Neuinstallation.

Der vorherige Zwei-Weg-Fall bestätigte Treppe und ersten
Weg, platzierte aber den zweiten Weg nicht. Eine fehlende Stütze ist eine Hypothese,
keine aus diesem Fehler bewiesene Spielregel. Der Plattformpilot prüft diese Hypothese.
Eine unfertige Plattform kann den folgenden Weg verhindern; dann bleibt der
bestätigte Teilstand erhalten und die Ursache wird gelesen. Kein automatischer Retry.

Ein weiterer lesender Versuch fand keinen passenden vorhandenen ersten Unterbau
im untersuchten Bereich; der einzige kollisionsfreie Kandidat über einer bestehenden
Treppe wurde vom Spiel abgelehnt. Die Annahme eines vorhandenen ersten Auflagers
entfällt deshalb in 0.29.1: Beide oberen Wege erhalten eine eigene Plattform.
0.29.1 wurde menschlich installiert und gezielt live geprüft. Ein Neuladen kann
die Freischaltung zurücksetzen; Forschung vor dem Pilot erneut lesen.

## Livebefund 0.29.1

Nach regulärer Freischaltung und Entfernung einer markierten toten Kiefer durch
Bauarbeiter waren alle fünf Zielgrundrisse frei. Der Pilot bestätigte Treppe und
erste Plattform mit Bauarbeiterzugang. Der erste obere Weg wurde nicht platziert;
zweite Plattform und zweiter Weg wurden nicht versucht.

Die direkte Diagnosevorschau bestätigte den ersten oberen Weg bereits auf der
unfertigen Plattform mit `valid=true`. Der Abbruch entstand stattdessen durch
`navigation_preview_baseline_mismatch` bei zwölf Baustellen-Zugangspunkten;
die tatsächlichen abweichenden Punkte sind noch nicht einzeln ausgewiesen.
Das ist keine bewiesene Ablehnung unfertiger Plattformen durch den Spielvalidator.

Ein einmaliger begrenzter Simulationslauf von 0,6 Spieltagen stellte Treppe und
erste Plattform fertig und pausierte automatisch. Danach blieb der Weg gültig,
der Navigationsabgleich war wieder konsistent (`restored=true`, null verlorene
Verbindungen, null Baustellen-Zugangspunkte). Der terminale Pilot wurde nicht
wiederholt oder fortgesetzt. Der fünfteilige Abschlussnachweis ist nicht bestanden;
Änderungen bleiben uncommitted.

In 0.29.2 implementierter, noch ungeprüfter Entwicklungsschritt: Baustellenphase explizit behandeln, statt Gleichheit
von Echt- und Vorschau-Navigation während des Baus vorauszusetzen. Konservative
Alternative: Träger zunächst fertigstellen und den Wegteil separat bestätigen.
Wegschutz nicht pauschal umgehen; generische Bauaufträge bleiben gesperrt.

Erwarteter Nachweis: alle fünf Schritte und Objekte zurückgelesen. `completed`
bedeutet bestätigte Aufträge/Objekte, nicht fertig gebaute Treppe oder nachgewiesene
vertikale Navigation. Fertige Wege allein belegen keine Distriktanbindung.

Menschlicher Testlauf über `scripts/prepare-human-live-test.ps1`, danach
Spiel laden und `live bereit` melden. Plattform muss im geladenen Stand
freigeschaltet sein; erneute Freischaltung nur bei tatsächlichem Bedarf.
