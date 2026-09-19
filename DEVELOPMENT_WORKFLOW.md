# Entwicklungsablauf

## Prüfen

Vor Änderungen `git status --short --branch` prüfen und vorhandene Nutzeränderungen erhalten.
Für Codeänderungen im Repository ausführen:

```powershell
pwsh -NoProfile -File ./scripts/verify.ps1
```

Das Skript verwendet projektlokale Caches, Locked-Mode-Restore, Release-Build und gewöhnliche Tests.
`-Live` greift zusätzlich auf die vom Nutzer vorbereitete Testkolonie zu und bleibt ausdrücklich opt-in.
Bei reinen Dokumentationsänderungen genügen Inhalts-, Link- und Diff-Prüfung; keine unnötige Wiederholung der Spieltests.

## Checkpoint und Push

GitHub-Ziel: https://github.com/chr0mat0mcodex/timberborn-mcp
Standardbranch bei Projektanlage: `master`. Neue Arbeitsbranches erhalten das Präfix `codex/`.

1. Nach einem sinnvoll abgeschlossenen Arbeitsstand relevante Prüfungen abschließen.
2. Diff und Dateiliste auf unbeabsichtigte Änderungen, Geheimnisse und private bzw. generierte Daten prüfen. Nur ausdrücklich ausgewählte Projektdateien stagen.
3. Einen aussagekräftigen Commit erstellen. Freigegebene Agentenidentität pro Befehl verwenden: `git -c user.name=Codex -c user.email=codex@openai.com commit ...`. Keine dauerhafte Git-Identität konfigurieren.
4. Den Remote-Stand mit `git fetch origin` prüfen. Bei Divergenz keine fremden Commits überschreiben; Ursache klären und Änderungen nachvollziehbar integrieren.
5. Geprüfte Checkpoints auf den vorgesehenen origin-Branch pushen. Nur bewusst erstellte Projekt-Tags gezielt übertragen, kein pauschales `--mirror` oder Force-Push.
6. Remote-Commit mit lokalem HEAD vergleichen und Push-Erfolg berichten. Bei fehlendem Zugang oder Netzwerkfehler bleibt der lokale Checkpoint erhalten; fehlende Synchronisierung ausdrücklich melden.

Der Nutzer hat diese fortlaufende Sicherung während der Projektarbeit autorisiert. Dafür ist nicht bei jedem normalen Push eine neue Bestätigung erforderlich. Eine neue Veröffentlichung außerhalb dieses Repositorys oder eine Erweiterung des freigegebenen Funktionsumfangs ist davon nicht gedeckt.

## Aktueller Ausgangspunkt

Read-only-POC mit fünf MCP-Werkzeugen implementiert. Zuletzt geprüft: 47 gewöhnliche Tests erfolgreich,
zusätzlicher expliziter Live-Test erfolgreich, UI-Werte vom Nutzer bestätigt. Meilenstein: `poc-readonly-v0.1`.
Der lokale Codex-Client wurde am 2026-09-19 auf ausdrücklichen Nutzerauftrag als `timberborn` eingerichtet.
Registrierung geprüft; separater stdio-Livetest aller fünf Werkzeuge erneut erfolgreich.
Die Werkzeugverfügbarkeit im bereits laufenden Chat ist damit noch nicht bestätigt.
Weitere Funktionen benötigen eine neue Umfangsfreigabe.

Freigegebene Erweiterung vom 2026-09-19: set_building_paused als einzelnes Schreibwerkzeug hinter
TIMBERBORN_ENABLE_WRITES=1 implementiert. Aktuell 74 reguläre Tests bestanden; gesonderter Live-Schreibpilot
an genau einer Holzfällerflagge erfolgreich einschließlich Rückweg und Schlussabfrage.
UI-Abgleich vom Nutzer bestätigt: nach dem erfolgreichen Hin-/Rücktest auf gesonderten Auftrag erneut
pausiert und sichtbar bestätigt. Letzter bestätigter Zustand absichtlich pausiert; nicht automatisch reaktivieren.
Standardkonfiguration weiterhin lesend. Details: missionsplan.md Abschnitt 6.
