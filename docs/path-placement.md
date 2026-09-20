# Einzelner Wegauftrag — 0.5.0

Auf Nutzer-Go implementierter Pilot, noch nicht im Spiel abgenommen. Ziel ist der
Nachweis des regulären Platzierungspfads und einer überprüfbaren Ergebniszuordnung.
0.5.0 ist installiert; letzter Live-Nachweis bleibt 0.4.1. Kein autonomer Kolonieaufbau.

## Vertrag

`place_path(template="Path", x, y, z, rotation, session)` sendet ausschließlich
POST ohne Body an `/agent-api/v1/path-placement`. Erforderlich sind
`enablePlacement: true` in der privaten Mod-Konfiguration und
`TIMBERBORN_ENABLE_PLACEMENT=1` im MCP-Prozess. Beide sind standardmäßig aus.
Das bisherige `TIMBERBORN_ENABLE_WRITES` schaltet diesen nativen Pilot nicht ein.
Der Standardstarter und sämtliche lesenden Live-Tests deaktivieren das neue Opt-in.
Die separate Vorschau-Route muss dafür nicht freigeschaltet sein; der Auftrag selbst
führt die geprüfte Vorschauvalidierung intern aus.

Auf dem Spielhauptthread: aktuelle Sitzungs-ID vergleichen, einmaliges Versuchslimit
verbrauchen, ausschließlich einen einfeldrigen Path auf leerem Standort zulassen,
Fraktion/Freischaltung/Grenzen und Vorschau über SiteValidation frisch prüfen.
Bei Vorschau-Zustandsabweichung oder ungültigem Platz erfolgt kein Auftrag.
Anschließend `BlockObjectPlacerService.GetMatchingPlacer(spec).Place(builder, placement)`.
Builder stammt aus `TemplateSpec.Blueprint` und erhält vorab eine neue Entity-ID.
Keine direkte EntityService-Instanziierung, Fertigbau-Abkürzung, Freischaltung oder Löschung.
Öffentliche Signaturen wurden gegen lokale Spielbibliotheken geprüft; keine neue Abhängigkeit.

Ergebnisse:

- `applied`: genau die vorgesehene Entity-ID ist initialisiert, kein Preview und hat
  Vorlage Path, Position und Orientierung wie angefordert. `finished` gibt separat
  den beobachteten Fertigstatus an; keine Aussage zur Distriktanbindung.
- `rejected`: vor dem Platzierer abgewiesen, kein Bauauftrag erteilt.
- `unconfirmed`: Platzierer wurde aufgerufen, aber die Zuordnung ist unklar oder
  eine Ausnahme trat auf. Eine Änderung kann bereits erfolgt sein.

Die zurückgegebene Entity-ID ist bei rejected/unconfirmed nur die vorgesehene ID,
kein Existenznachweis. Zur Klärung `find_buildings` lesen und ID, Vorlage, Position
und Fertigstatus vergleichen. Falls der Platzierer SetId nicht übernimmt, wird der
Auftrag ausdrücklich unconfirmed; kein heuristischer Erfolg aus irgendeinem neuen Objekt.

Nach jedem angenommenen Versuch bleibt der Pilot bis zum Szenenwechsel gesperrt,
auch nach Ablehnung oder Ausnahme. Keine Wiederholung bei Timeout oder unbekanntem
Ergebnis, kein automatisches Zurücksetzen. Ein Szenenwechsel ist kein Retry-Verfahren.
Sitzungs-ID ist eine Szenenkennung, kein Revisionszähler; der Platz wird deshalb erneut
im selben Hauptthread-Aufruf geprüft. Preview-Budget und -Fehlersperre gelten weiterhin.
Transportfehler liefern keine sichere Aussage, ob der Auftrag angekommen ist.

## Begrenzte Live-Abnahme nach Installation

1. Version 0.5.0 und frische Sitzung lesen, Gebäude/Wege und kleinen Kartenausschnitt
   abfragen. Einen leeren, ebenerdigen Path-Standort möglichst neben bestehendem Weg
   auswählen und rein lesend vorprüfen; keine Screenshots oder Eingabesimulation.
2. Genau ein `place_path`, keine Wiederholung in diesem Pilot. Bei Fehler/unconfirmed
   ausschließlich lesend klären und weitere Bauaktionen stoppen.
3. Über `find_buildings` genau die zurückgegebene ID mit Path/Position und Fertigstatus
   nachweisen. Vorher-/Nachher-Objektzahl und Ressourcen als Zusatzbeobachtung erfassen.
4. Der Testweg bleibt bestehen; keine automatische Löschung und kein automatisches Save.

Lokale Tests können HTTP/MCP-Vertrag, Opt-ins, Eingabegrenzen, Antwortprüfung und
Einmaligkeit nachweisen. Ob der echte Spielplatzierer die ID übernimmt und einen
regulären Weg erzeugt, muss der oben begrenzte Live-Pilot belegen.
