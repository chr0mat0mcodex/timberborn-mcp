# Zusatzprotokoll 0.59 — 16. Oktober 2026

Nachtrag: Ein späterer unabhängiger Reparaturkontrollfall mit derselben installierten
0.59 gelang bis Abschluss und Elternfreigabe. Der unten dokumentierte Fehler bleibt
unverändert belegt und ursächlich ungeklärt. [Nachweis und 0.60-Diagnoseänderung](repair-start-0.60.md).

Native MCP-Liveproben auf dem Westufer des freigegebenen Testspielstands.
Bridge 0.59, dieselbe Sitzung wie beim vorherigen Großprojekt-/Stoppnachweis.
Kein neuer Build, keine Installation, keine Speicherung durch den Agenten.
Je ein kleiner repräsentativer Fall; keine vollständige Fehlerkombinatorik.

| Fall | Durchführung und tatsächlicher Nachweis | Ergebnis |
| --- | --- | --- |
| Zeitbudget | Ein Lager mit einer Spielstunde Budget. Projekt endet `blocked/game_time_limit`, 0/1 fertig, 1 bestätigte Baustelle. Lauf endet nach genau einer Stunde mit bestätigter Pause. | Fachlich bestanden; Log meldet korrekt angehalten/unfertig. Ursache im Kurztext nur als Verweis auf Fachbericht, nicht ausdrücklich als Zeitlimit. |
| Reparatur | Zulässige Weg-Ergänzung für das budgetgestoppte Elternprojekt geplant. `run_large_project_repair` liefert `state_conflict`, Ausführung als `unconfirmed` klassifiziert. | Start fehlgeschlagen. Lesende Kontrolle findet weder Reparaturprojekt noch neuen Weg; Tempo bleibt 0. Kein Retry. Reparaturerfolg nicht abgenommen. Fehlertext fordert korrekt lesende Klärung. |
| Räum-/Baugruppe | Kleine Fläche mit sechs toten Birkenstümpfen. Alle sechs entfernt, anschließend ein Lager und ein Anschlussweg gebaut. `completed`, `remainingTargets=0`, 2/2 Bauteile fertig/zugänglich, Projekt freigegeben, Pause bestätigt. Unabhängige Vegetationsabfrage findet keine Ziele mehr. | Bestanden. Anfang ohne Baunachweis, Zwischenstand 1/2 und Abschluss 2/2 als gespeicherte Belege korrekt. Kein eigener länger laufender Räumzeitlauf nötig; Stümpfe wurden direkt entfernt. |
| Separater Bauchargenstopp | Zwei Lager vorgesehen. Nach Start des ersten zwölfstündigen Baulaufs `stop_building_batch`: Charge cancelled, tatsächlich weiterhin Tempo 7. Vorhandener Lauf beendet sein ursprüngliches Budget nach zwölf Stunden und pausiert. | Bestanden. Kein zweiter Auftrag. Historischer Chargenstand bleibt 0/2 und meldet ausdrücklich weiterbestehendes Laufbudget; frische Einzelbauabfrage bestätigt später 1/1 fertig/zugänglich und Pause. |

## Reparaturfehler und Diagnosegrenze

Planung meldete `pilotEligible=true`; der Elternbericht war `repairEligible=true`.
Start wurde mit `state_conflict` abgewiesen. Derselbe Reparaturhandle war danach
nicht lesbar; am vorgesehenen Weganker existierte kein Weg. Kein neuer Lauf
beobachtet. Die Quelle der Ablehnung ist mit den vorhandenen Belegen nicht eindeutig
bestimmt; keine erfundene Ursachenfestlegung. Vor erneutem Start den konkreten
Ablehnungszweig diagnostizieren. Keine Umgehung durch andere ID oder Neustart.
Das unfertige Elterngebäude wurde anschließend durch einen separaten begrenzten
Zeitlauf fertiggestellt und das Elternprojekt frisch geprüft/freigegeben. Das ist
kein erfolgreicher Reparaturaufruf und erweitert dessen Abnahme nicht.

## Logtransport und Anzeige

19 zu diesen Zusatzproben gehörende Ergebnisbelege in MCP- und Bridge-Dateilog
je Aufrufkennung/Text identisch; beide melden `failedWrites=0`. Agentenlog-Zeilen
gegen Fachberichte geprüft. Mensch bestätigt Räumabschluss, Chargenstopp und
abgewiesene Reparatur vollständig und verständlich lesbar.
Rohlogs, konkrete Spielkennungen und persönliche Dateinamen bleiben lokal.

## Offene Punkte

- Reparaturstart mit `state_conflict` gezielt diagnostizieren und danach separat
  erneut abnehmen. Kein erfolgreicher Reparaturabschluss nachgewiesen.
- `game_time_limit` und angeforderten Stopp im Kurztext verständlich benennen;
  derzeit korrekter, aber wenig hilfreicher Verweis auf den Fachbericht.
- Keine Abdeckung beliebiger Teilfehler, Netzwerkabbrüche, realer Zeitlimits,
  langer Räumläufe, lebender Vegetation oder unterbrochener Zugänge durch diese
  kleinen Proben. Keine pauschale Sicherheit, Produktion oder Belieferung ableiten.

Abschlusszustand: Spiel pausiert; letzte Einzelbaustelle fertig und zugänglich.
