# 0.61 — Navigationsauswahl und konkrete Planungsfehler

Stand 16. Oktober 2026: nach menschlichem Gate begrenzt live abgenommen.

## Änderung

Zwei pauschale Grenzen auf 4.096 Kartenobjekte blockierten kleine Baupläne:
in RoadProtection vor der Auswahl der Messpunkte und in ConstructionIsolation
vor der Auswertung der Baustellen. Beide sind entfernt. Navigationsziele bleiben
alle unfertigen Objekte, Distrikte, Wege und Objekte mit Zugangskomponenten.
Accessible wird über die Mehrfachkomponenten-API gelesen. Die Isolationsprüfung
durchläuft den Bestand als Stream und unterscheidet fremde Baustellen,
Initialisierung und unbekannten Zustand. Native Kollisionsdaten bleiben vollständig.

Arbeitsgrenzen bleiben: 4.096 Zugangsmesspunkte, 16.384 Distriktvergleiche,
1–16 Distrikte und separate Baustellenprüfungen. Registerdurchläufe bleiben
linear zur Kartengröße; keine Zusage konstanter Laufzeit.

Explizite project_plan_*-Vorabweisungen werden bei genau einem direkten
Schreibversuch als not_executed und mit deutscher Erklärung ausgegeben.
Der Zugangsplaner verschluckt diese Ablehnungen nicht mehr als Geometriefehler.
Generische Konflikte, fehlerhafte Antworten und Transportfehler bleiben
unconfirmed. Keine automatischen Wiederholungen. Frühere Aufträge bleiben bestehen.

## Nachweise

Der Mensch hat das Gate nach den Korrekturen erneut ausgeführt und live bereit
gemeldet; installierte Bridge 0.61 und neue Sitzung wurden bestätigt.

- Große Karte: zuvor 4.141 Objekte; kleine Lagervorschau mit 3.404 Vergleichen,
  35 verbundenen Ausgangspunkten und 33 Wegmesspunkten erfolgreich.
  Geometrie, Eingang und geometrischer Arbeitszugang positiv; pilotEligible=true.
- Höhenkontrolle: derselbe Platz eine Ebene höher ohne Träger korrekt
  native_invalid, Zugang negativ, pilotEligible=false.
- Bestandskontrolle: Lager auf einer bestehenden Wegzelle erkennt sieben
  verlorene Anschlüsse und existing_district_access_lost.
- Umweg: drei gültige westliche Vorschauwege erhalten die Anschlüsse um eine
  belegte Wegzelle; lostConnections=0. Die Grundrisskollision bleibt korrekt
  native_invalid/pilotEligible=false, also keine ungeprüfte Baufreigabe.
- Alle Vorschauen: restored=true, sessionLocked=false. Frische
  Baustellenübersicht nach den ersten Kontrollen leer; keine Bauaufträge erteilt.
- Bei bestätigtem Tempo 7: project_plan_requires_pause, not_executed,
  retryable=false. Deutsche Ergebniszeile im MCP-Dateilog als rejected bestätigt.
  Keine gesonderte menschliche Abnahme der visuellen Ingame-Darstellung.
- Nach Abschluss Tempo 7 wiederhergestellt und gelesen.

Vorbereitete synthetische Tests decken 10.000 irrelevante/fertige Objekte,
späte Zugangsziele und Baustellen, unbekannte Zustände, konkrete Vorbedingungen,
Whitelist/Fallback, alle sechs betroffenen MCP-Operationen sowie mehrdeutige
HTTP-Antworten ab. Ausführung gehört zum menschlichen Gate; kein Agent-Testlauf.

## Korrekturen während der Abnahme

Erster Gate-Lauf scheiterte an verlorenen Tupelfeldnamen im neuen Test;
expliziter benannter Sequenztyp korrigiert. Erster Livepilot entdeckte die zweite
Objektgrenze in ConstructionIsolation. Zweiter Pilot lokalisierte eine
InvalidOperationException aus ComponentCache.GetCachedComponent; nach Ersatz
der Einzelabfrage für Accessible durch die Mehrfachabfrage lief die Planung durch.
Lokale Diagnose schreibt nur Ausnahmetyp und Methodenherkunft, keine Texte,
Argumente, Pfade oder Spielobjektdaten.

Ein erster Umwegentwurf war geometrisch ungeeignet: sein Startpunkt lag im
Erfindergrundriss. Das wurde lesend anhand occupiedCells geklärt. Anschließend
wurde genau ein kürzerer, freier Kontrollumweg geprüft; kein Serienbauversuch.

## Grenzen

Kein Lasttest nahe 4.096 Messpunkten oder 16.384 Vergleichen, keine neue
reguläre Baufreigabe. roadProtection bleibt bei fehlendem Baustellen-/Wegknoten-
Nachweis unknown. Auftrag, Fertigstellung, Bauarbeiterzugang, Lieferung und
Produktion bleiben getrennte Belege. Kein Gebäude wurde für diese Abnahme gebaut.
