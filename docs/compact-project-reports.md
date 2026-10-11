# Kompakte Bauprojektberichte — 0.46.0

Status: nach Größenkorrektur menschliches Gate und begrenzter Abschluss-Livepilot
am 2026-10-12 bestanden.

## Umfang

Die elf großen Projektwerkzeuge für Plan/Zugang/Start/Lauf/Advance/Inspect/Stopp/
Diagnose/Abschluss/Reparatur unterstützen detail=compact|full, Standard compact.
inspect_power_network bleibt unverändert. Die älteren Standortchargen behalten
ihren eigenen details=true-Vertrag; keine neue Ausführungslogik oder Baugrenze.

Die Darstellung erfolgt erst nach vollständiger nativer Antwortvalidierung auf
einer Kopie. Full bleibt der vollständige unveränderte native Bericht. Fehler
werden nicht gekürzt. Text und StructuredContent enthalten dieselbe Ansicht.
Keine zusätzlichen Spielabfragen, kein Wegfall nativer Prüfungen oder interner
Lesearbeit. Angaben zu Unknowns, Bestandsverlusten, Wiederherstellung, Pause und
abgeschnittenen Ergebnissen bleiben erhalten.

## Fortschritt richtig lesen

Bei Lauf-/Ledgerantworten liefert summary:

- planned: gesamte Anzahl Bauteile; pending: noch ohne Versuch.
- attempted: alle begonnenen Bauversuche, einschließlich unconfirmed.
- confirmedPlaced: construction plus finished; unconfirmed bleibt separat.
- finishedAccessRecorded: im Projekt als fertig und zugänglich protokolliert.
  Das ist keine frische Zugangsprüfung und kein Produktionsnachweis.
- knownBlockedParts: nur ausdrücklich mit blockedPart belegte Indizes, IDs und
  Gründe. Leere Liste heißt kein indizierter Blocker gemeldet, nicht freie Bahn.
- nextStep: Handlungshinweis, kein automatisch ausgeführter Auftrag.

attentionParts enthält nicht fertige und ausdrücklich blockierte Teile mit ihren
ursprünglichen Indizes, IDs und Zuständen. Vollständige entityIds/partStates der
fertigen Teile stehen in Full. planKey und redundante ordered/finished-Zähler
bleiben ebenfalls in Full, actionId identifiziert das Projekt. order bleibt im
manuellen Modus, entfällt beim autonomen Lauf. current, deferredParts, reason,
Freigaben und Zeit-/Pausebelege bleiben unverändert. ordered im nativen Bericht zählt Versuche;
für bestätigte Platzierungen confirmedPlaced verwenden.

Frische diagnose/close-Berichte unterscheiden confirmedPresent von
finishedAccessible. Ein fertiges Gebäude ohne Zugang zählt nicht als zugänglich.
attentionPartIndices in summary verweist auf Teile mit Handlungs-/Klärungsbedarf, auch
normales Warten; das ist keine pauschale Zahl gesicherter Blockaden. Material- und
Baufortschritt sowie Bereitschaft bleiben sichtbar. Detaillierte Kosten,
Baustellenbestände und globale Bestände stehen in Full; Kosten sind kein Restbedarf.
Frisch als finished beobachtete Teile werden in Compact ausschließlich in den
Zählern dargestellt; ihre Einzelbelege stehen in Full. Alle fehlenden, unbekannten
oder unzugänglichen Teile bleiben mit IDs und Gründen in parts. Historische
lastGuardFailure/lastValidationFailure bleiben als solche erkennbar.

Planberichte erhalten Schritte, planKey, Abhängigkeiten, Platzierungs-/Zugangs-
zustände, Wiederherstellung und Nachweisgrenzen. summary benennt bekannte
blockierte sowie erst später nativ prüfbare Teile. Kein Bauversuch wird aus einer
Vorschau abgeleitet. Kraftports werden auf ihre Anzahl gekürzt, PowerLinks bleiben
mit ihrer ausdrücklich geometrischen Bedeutung erhalten. Zellbelege stehen in
Full. Zugangsvorschläge behalten die vollständigen ausführbaren steps und
cleanupCandidateIndices; diese sind keine pauschale Abrissfreigabe.

## Details und Fortsetzung

Für bestehende Projekte inspect_large_project(actionId, detail=full) zum Ledger
oder diagnose_large_project(actionId, detail=full) für frische Beobachtung nutzen.
Keinen Start/Advance/Stopp/Abschluss nur für Details wiederholen. Planberichte
haben keinen gespeicherten Full-Abruf: bei Bedarf detail=full bereits bei der
Planung setzen; eine neue Planung ist eine neue Vorschau mit ggf. anderem planKey.

Hinweise für manuelle oder unbestätigte Zustände führen zur Diagnose; unbestätigte
Pause muss zuerst lesend geklärt werden. closeEligible empfiehlt nur die bewusste
Prüfung des Abschlusses, der unbeauftragte Teile aufgibt. repairEligible empfiehlt
nur Reparaturplanung. Kein Hinweis hebt Opt-ins, Sessionbindung oder native
Prüfungen auf. Produzieren und Kraftversorgung bleiben getrennte Nachweise.

## Tests und Abnahme

Vorbereitet: Versuch/Bestätigung/Fertigzugang getrennt, ausstehende Pause,
unbestätigte/gestoppte Projekte, manueller Ledger ohne erfundene Pause,
fehlender Fertigzugang und unbekannter Bauzugang, Bestandsverlust/Trunkierung,
Zugangsplan mit unveränderten Schritten und planKey, Vollbeleg unverändert,
Fehler unverändert, Text/StructuredContent gleich, alle elf Katalogeinträge mit
optionalem detail. Versionsprofile erkennen 0.46, negative Fixture bleibt 99.0.0.

Nach menschlichem Gate: kleine Zwei-/Dreiteilcharge bis zum bestätigten Abschluss;
Compact/Full desselben pausierten Projekts vergleichen. Dazu ein geometrisch oder
zugangsbedingt blockierter Plan ohne Wiederholung unbestätigter Schreibaufträge.
Stopp bei widersprüchlichem Zustand, verlorener Blockade oder falscher Gewissheit.
Antwortgröße in UTF-8-JSON messen: Ziel mindestens 25 % Ersparnis für einen
repräsentativen detailreichen Bericht. Sehr kleine Antworten können durch die
zusätzliche Zusammenfassung größer werden; keine pauschale Größenersparnis.
Testbauten und Hilfswege nach Zugangsprüfung aufräumen, Rückgewinnungsrohstoffe
nicht löschen. Danach Nachweise dokumentieren und gezielt committen/pushen.

## Erster Livepilot 2026-10-12 — Größenkorrektur, noch keine Abnahme

Installierte 0.46 bestätigt. Zweiteilcharge mit zwei seitlichen Testwegen:
Start korrekt attempted=2, confirmedPlaced=0, unconfirmed=2; anschließend
completed, confirmedPlaced=2, finishedAccessRecorded=2 und bestätigte Pause.
Frische Diagnose meldet zwei zugängliche fertige Teile. Blockierter Weg unter
Gelände in Compact/Full native_invalid, pilotEligible=false, restored=true;
keine Ausführung der Negativkontrolle. Nach Abschluss beide Testwege entfernt,
Fläche ohne verbleibende Gebäude/Wege bestätigt, keine Rohstofflöschung.

Erste Größenmessung (StructuredContent UTF-8): Ledger 1.530/1.856 Bytes
(17,6 % kleiner), fertige Diagnose 1.438/1.278 Bytes (12,5 % größer).
Deshalb keine Abnahme/kein Commit: redundante Ledgerfelder und erfolgreiche
Einzelbelege aus Compact entfernt, Zusammenfassung bleibt; Diagnose verweist
per attentionPartIndices auf die ohnehin erhaltenen Problemteile. Regression
für die konkrete Zweiteil-Diagnose ergänzt. Full bleibt unverändert.
Korrektur statisch geprüft; neues menschliches Gate und erneuter kurzer
Ansichtenvergleich erforderlich. Kein großer zusätzlicher Baupilot nötig.

## Abschluss-Livepilot 2026-10-12

Korrigierte 0.46 erneut nach menschlichem Gate gestartet. Zweiteilcharge aus
zwei freien seitlichen Wegen: Start attempted=2, confirmedPlaced=0, unconfirmed=2;
Abschluss completed, confirmedPlaced=2, finishedAccessRecorded=2, Pause bestätigt.
Frische Diagnose finishedAccessible=2; Full zeigt beide Einzelbelege. Compact
enthält keine erfolgreichen Einzelteile, alle gemeinsamen Felder stimmen überein.
Zeit-/Pausebelege unverändert, Text und StructuredContent je Ansicht identisch.

UTF-8-JSON des StructuredContent: Ledger 1.384 statt 1.856 Bytes (25,4 % weniger),
Diagnose 1.065 statt 1.278 Bytes (16,7 % weniger). Ziel im Ledger erreicht;
keine pauschale Größenersparnis für jeden Plan oder jede Diagnose behauptet.
Negativkontrolle unter Gelände: native_invalid, bekannter Blocker Teil 0,
pilotEligible=false, restored=true. Keine Ausführung des blockierten Plans.

Projekt regulär abgeschlossen, beide Testwege entfernt; Teststreifen ohne
verbleibende Gebäude/Wege und bestehender Distriktweg connected=true bestätigt.
Keine Rückgewinnungsrohstoffe gelöscht. Simulation pausiert. Liveumfang: zwei
sofort fertige Wege und ungültige Vorschau; längere Baustellen-/Reparaturverläufe
bleiben durch gezielte synthetische Regressionen, nicht neue Livebreite abgedeckt.
