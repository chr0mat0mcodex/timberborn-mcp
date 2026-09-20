# Fachliche Ablehnungen ab 0.17.2

Feste Fehlercodes helfen dem Agenten, eine neue Entscheidung zu treffen, statt einen
Zustandskonflikt für einen Verbindungsfehler zu halten. Die MCP-Werkzeuge bleiben gleich.

| Code | Bedeutung / nächste Prüfung |
| --- | --- |
| `stale_session` | Spielstand/Sitzung neu lesen und Ziel erneut bestimmen. |
| `template_locked` | Forschung und Freischaltung prüfen. |
| `template_disabled` | Vorlage ist für die aktuellen Spiel-Features nicht verfügbar. |
| `state_conflict` | Gebäudeeinstellung stimmt nicht mehr mit dem erwarteten Wert überein; neu lesen. |
| `building_not_found` | Gebäude erneut suchen, keine alte ID blind wiederverwenden. |
| `finished_building_required` | Baufortschritt prüfen. |
| `not_pausable` | Pausierbarkeit prüfen. |
| `unsupported_storage_good`, `not_storage` | Lagerkomponente und erlaubte Güter prüfen. |
| `not_farm`, `not_crop_prioritizer`, `unavailable_crop` | Farmkomponente und erlaubte Pflanzen prüfen. |
| `unsupported_setting` | Unterstützte Gebäudeeinstellungen prüfen. |

Diese Codes entstehen ausschließlich an expliziten Vorbedingungen vor dem jeweiligen
Eingriff. Mod und MainThreadQueue geben nur Codes aus einer festen Positivliste weiter.
HTTP 409 enthält genau `error`; der Client akzeptiert ausschließlich bekannte Codes,
keine zusätzlichen Felder oder doppelten Schlüssel. MCP meldet `status=error`, den Code
und `retryable=false`. Das Ingame-Log zeigt `rejected`. Keine automatischen Wiederholungen.

HTTP 400 mit dem bisherigen `invalid_request` wird zu `invalid_argument`. Unbekannte
HTTP-409-Antworten bleiben `backend_incompatible`. Unbekannte Spielausnahmen werden
weiterhin maskiert; keine Exception-Texte, lokalen Pfade oder Spielinhalte im Fehlertext.
Transportfehler bleiben von fachlichen Ablehnungen getrennt. Ein Fehler nach begonnener
Mutation kann weiterhin `unconfirmed` bzw. ein unbestätigtes Ergebnis bedeuten.

Die Umstellung umfasst vorerst zentrale Session-Prüfung, Vorlagenverfügbarkeit in der
Bauvalidierung und Gebäudeeinstellungen. Andere Management-/Entfernungsfehler sind
noch nicht vollständig auf spezifische Codes umgestellt. Alte Bridge-Versionen können
keine nachträglich erfundenen spezifischen Fehler liefern.

Live-Abnahme: gesperrte Vorlage, absichtlich veraltete Session und abweichender erwarteter
Pausenwert; jeweils Ablehnung, fehlende Mutation und Logstatus separat bestätigen.

## Live-Abnahme 0.17.2

Bestätigt: template_locked, stale_session und state_conflict über echte MCP-Aufrufe,
jeweils retryable=false und rejected im Ingame-Log. Separate Rückabfragen zeigen
unveränderte Gebäudepause, Forschungspunkte und Simulation. Elf Aufrufe, keine Wiederholung.
