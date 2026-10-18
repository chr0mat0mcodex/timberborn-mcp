# A12/N09 — begrenzter Lesemesspilot

Stand 2026-10-18: Messpilot abgeschlossen, keine Laufzeitänderung.
Backend-Parallelfreigabe zurückgestellt; zusätzlicher Nutzen nicht nachgewiesen.

## Fragestellung und Grenze

Bringt das gleichzeitige Anfordern unabhängiger Leser im bestehenden Codex-Anschluss
einen messbaren Vorteil gegenüber seriellen Aufrufen? Ist dafür eine Lockerung
der sofort ablehnenden Backend-Sperre erforderlich?

Unveränderter, zuvor durch menschliches Gate und N08-Liveabnahme bestätigter Stand
268375f, Bridge 0.74.0. Kein separater stdio-Prozess, kein direkter HTTP-Ersatz.
NativeTools behält callGate.Wait(0): konkurrierende Fachaufrufe werden weiterhin
mit another_call_in_progress_not_queued abgewiesen. Die einzige bestehende
Cancel-Ausnahme während Statuswarten bleibt erhalten. InvokeLogged zeichnet
Aktivität vor und nach dem gesperrten Fachaufruf auf; die gesamte Antwortdauer
entspricht daher nicht der Dauer der Fachsperre.

## Vorgehen und vorab gesetzte Kriterien

Drei abwechselnde serielle und gleichzeitig angeforderte Paare, insgesamt zwölf
lesende Aufrufe: inspect_colony und inspect_workforce(offset=0, limit=20).
Der zweite Leser deckt ausdrücklich nur die erste Seite ab, nicht alle Arbeitsplätze.
Gleiche Sitzung, pausiertes Spiel, identische fachliche Parameter, unveränderte
Standarddarstellung. Gleichzeitige Anforderungen mit Promise.allSettled; jedes
Einzelergebnis auf Erfolg und Sitzung geprüft. Keine automatische Wiederholung.

Erfolg: nachweisbare Requestüberlappung und mindestens 20 Prozent kürzere mittlere
Paarlaufzeit ohne Ablehnung. Bei serieller Zustellung, Fehler oder unklarem Nutzen
keine Skalierung und kein spekulativer Umbau. Drei Paare sind eine Richtungsprüfung,
kein belastbarer Lasttest. Keine Aussagen zu teuren Planern, großen Antworten,
Mutationen, anderen Clients oder einem gemeinsamen atomaren Spielsnapshot.

## Beobachtungen

Paarlaufzeiten umfassen den Toolaufruf bis zur empfangenen Antwort, gemessen beim
Aufrufer. Supervisor-Zeitmarken wurden zusätzlich lokal gelesen; keine Rohlogs,
Sitzungs-/Objektkennungen oder Spielinhalte übernommen.

| Paar | Seriell | Gleichzeitig angefordert | Verkürzung |
| --- | ---: | ---: | ---: |
| 1 | 412 ms | 237 ms | 42,5 % |
| 2 | 353 ms | 259 ms | 26,6 % |
| 3 | 367 ms | 354 ms | 3,5 % |
| Mittel | 377,3 ms | 283,3 ms | 24,9 % |

Alle zwölf Antworten status=ok, gleiche Sitzung, keine Busy-Ablehnung und kein
fehlendes Ergebnis. In allen drei gleichzeitigen Paaren leitete der Supervisor
den zweiten Request vor der ersten Backendantwort weiter: etwa 40, 49 und 68 ms
Überlappung. Anders als beim historischen Cancel-Vorfall war die Zustellung dieser
Lesepaare somit nicht vollständig seriell. Antwortzeitpunkte sind getrennte
Beobachtungen, kein gemeinsamer Snapshot.

Diese Messung lokalisiert keine überlappenden Fachhandler innerhalb von NativeTools.
Aktivitätslogging, Transport und Antwortaufbereitung können sich überlappen,
während die Fachsperre intakt bleibt. Das ist eine Erklärung aus der Quellstruktur,
kein separat instrumentierter Nachweis ihrer jeweiligen Zeitanteile.

## Entscheidung, Nutzen und Aufwand

Begrenztes gleichzeitiges Anfordern der zwei unabhängigen Leser ist im geprüften
Anschluss bereits nützlich: im Mittel rund 94 ms pro Paar eingespart, bei deutlicher
Streuung. Kein zusätzlicher Backend-Parallelpfad für diesen Pilot. Dessen Nutzen
gegenüber sofortiger Ablehnung konnte nicht gemessen werden, weil keine Ablehnung
auftrat. Keine pauschale Freigabe weiterer Leser oder größerer Parallelchargen.

Aufwand des Messpiloten klein: zwölf Nutzaufrufe, vorhandene lokale Zeitmarken und
gezielte Quellprüfung. Ein echter Mehrleserpfad hätte mindestens mittleren Aufwand:
zulässige Werkzeugfamilien, Kapazität, Fehlerfreigabe und Cancel-Zulassung müssten
zusammen modelliert und durch Gate/Livetest bewiesen werden. Dieser Aufwand ist
mit den vorliegenden Fällen nicht gerechtfertigt. Alternative ist die bestehende
begrenzte Paaranforderung oder serielle Abfrage bei fachlichen Abhängigkeiten.

Keine Mutationen, Mutationsqueue, Replay oder Änderungen der Cancel-Zulassung.
Cancel unter Leserlast wurde in diesem Pilot nicht neu getestet; seine bestehenden
Regressionen aus dem N08-Gate ersetzen keinen neuen Parallelitäts-Livebeleg.
Keine neue Installation, kein neues Gate erforderlich. Anleitung/Werkzeugübersicht
und Projektstand aktualisiert, keine projektbezogenen Spielskills vorhanden.

## Wiedervorlage

Backend-Parallelität erst wieder aufnehmen, wenn ein konkreter Nutzerablauf
wiederholt another_call_in_progress_not_queued bei notwendigen unabhängigen Lesern
zeigt oder diese Paarlatenz tatsächlich relevant wird. Dann höchstens eine kleine
Leserfamilie mit Kapazität zwei pilotieren; keine Mutation warten lassen und Cancel
explizit unter Leserlast abnehmen. Keine automatische Wiederaufnahme oder Ausweitung.
