# Ingame-MCP-Log und Aktionsbegründungen (0.16.0)

Implementiert, mit 362 regulären Tests geprüft und gesichert installiert. Fenster vom Nutzer bestätigt; Leser und kontrollierte Pumpenaktionen live im Log nachgewiesen.

## Bedienung

Rechts unten erscheint **MCP-Log** mit der Anzahl gespeicherter Aufrufe. Der Schalter
öffnet/schließt ein nichtmodales Fenster. Es zeigt die neuesten Aufrufe zuerst:
Uhrzeit, Werkzeugname, ausgewählte Parameter, Status und optionale Begründung.
**Alle Aufrufe / Nur Aktionen** filtert die Ansicht. **Leeren** entfernt die Historie.
Das Fenster verändert die Simulationsgeschwindigkeit nicht. Schließen erhält das Log.

Maximal 128 Aufrufe bleiben im Arbeitsspeicher der geladenen Spielsitzung; die ältesten
werden verdrängt. Beim Szenenwechsel/Entladen verschwinden Fenster und Historie.
Keine Speicherung im Spielstand, keine Logdatei und keine zusätzliche Fremdmod.
Öffentliche UILayout.AddBottomRight-API aus dem offiziellen HelloWorld-Beispiel,
Unity UI Toolkit mit eigenen VisualElements; keine private Reflection.

## reasoning für jeden nativen MCP-Aufruf

Jedes native Werkzeug akzeptiert zusätzlich ein **optionales** Stringfeld `reasoning`
mit maximal 600 Zeichen. Es ist eine kurze, für den Spieler bestimmte Begründung:
Ziel und relevanter Zustand. Keine internen Gedankengänge, Zugangsdaten oder privaten
Gesprächsauszüge. Das Feld ändert die Aktion und deren Freigaben nicht.

Beispiel (IDs aus frischer Beobachtung einsetzen):

```json
{
  "id": "<Gebäude-ID>",
  "session": "<aktuelle Sitzung>",
  "good": "Water",
  "expectedGood": "",
  "reasoning": "Der neue Tank hat noch keine Flüssigkeitsauswahl. Wasser auswählen, damit die Pumpen ihren Vorrat einlagern können."
}
```

Dieser Aufruf gehört zu `set_storage_good`. Auch Leser können eine Begründung
mitgeben, beispielsweise `inspect_colony` mit `reasoning: "Versorgung vor dem nächsten Bau prüfen."`.
Ohne Feld zeigt das Fenster ausdrücklich „Keine Begründung mitgegeben“.
Ungültige Typen, Steuerzeichen, doppelte Felder und überlange Texte werden abgewiesen.

## Protokollierung und Grenzen

Der native MCP-Server meldet Beginn und Abschluss unter einer eigenen Aufruf-ID.
Gleichzeitige Aufrufe bleiben getrennt. Der Abschluss enthält ok, applied, rejected,
unconfirmed oder error. Ein fehlender Abschluss bleibt sichtbar unbestätigt.
Auch lokal abgewiesene/ungekannte Werkzeugaufrufe werden protokolliert, solange die
Mod erreichbar ist. HTTP-Transportanfragen, tools/list, die Logmeldungen selbst und
Aufrufe gegen das alte More-HTTP-API-/Fake-Backend sind keine solchen nativen Toolcalls.

Die Metadaten laufen über einen authentifizierten, bodylosen POST auf `activity`;
Begründung und begrenzte Parameterzusammenfassung sind UTF-8/Base64-Header, nicht URL-Text.
Session- und Auth-Parameter werden nicht in die Zusammenfassung aufgenommen.
Bearer-Tokens und 64-stellige Hexschlüssel werden zusätzlich maskiert; das ist keine
allgemeine Erkennung aller vertraulichen Inhalte. UI-Texte werden ohne Rich Text angezeigt.
Keine vollständigen Ergebnisdaten, Arbeiterlisten oder Gesprächsverläufe im Log.

Ein Telemetriefehler löst **keine Aktionswiederholung** aus und ersetzt kein erfolgreiches
Aktionsergebnis. Je Logmeldung höchstens 1,5 Sekunden, kein Retry. Bei nicht erreichbarer
Mod kann ein Aufruf nicht im Spiel angezeigt werden; stderr meldet die Loglücke.
Ein Szenenwechsel verhindert den Abschluss in der falschen Spielsitzung. Gelöschte oder
verdrängte Einträge werden durch verspätete Abschlüsse nicht wiederhergestellt.
Das Log ist eine Anzeigehilfe, kein verbindliches Auditprotokoll und keine Autorisierung.

`inspect_agent_log` liest die neuesten bis zu 32 Einträge sowie Kapazität, Revision
und UI-Anbindungs-/Sichtbarkeitsstatus. Es ist selbst ein protokollierter Aufruf und
kann deshalb seinen eigenen laufenden Eintrag sehen. Keine zusätzliche Schreibfreigabe
für Telemetrie; sämtliche Spielaktionen behalten ihre bisherigen separaten Freigaben.

## Abnahme

Automatisch: Größen-/Zeichen-/Metadatenprüfung, Ringpuffer, Korrelation, verspäteter
Abschluss, fehlende Abschlussverbindung ohne doppelte Aktion, optionale Begründung
und echte MCP-stdio/HTTP-Verbindung einschließlich Unicode-Begründung.
Live nach Installation: Benutzer öffnet MCP-Log; Leser und kontrollierte Aktion mit
Begründung ausführen, Einträge separat lesen, Filter/Leeren/Schließen prüfen und
bestätigen, dass das Fenster die Spielgeschwindigkeit nicht verändert.

## 2026-09-20 — Ingame-Log 0.16.0 live bestätigt

Nutzer hat Schalter rechts unten gefunden und sichtbares Fenster bestätigt.
MCP meldet uiAttached=true und nach Öffnen windowVisible=true. Abfragen samt
Unicode-Begründungen und Abschlussstatus aus dem laufenden Spiel zurückgelesen.
Kontrollierter Pumpentest: Pause false -> true -> false, beide Zustände separat
bestätigt; zwei zugehörige Logeinträge mit reasoning und applied gelesen.
Simulation durchgehend pausiert, Tag 10 etwa 04:07 Uhr; Pumpe abschließend aktiv.
Keine Anzeigeänderung erforderlich, keine neue Modversion. Filter/Leeren und
Szenenwechselverhalten noch nicht manuell abgenommen; automatisierte Logtests bestehen.

Zusätzlich vom Nutzer bestätigt: Scrollen mit der Maus funktioniert. Anzeige auf ausdrücklichen Wunsch unverändert belassen.
