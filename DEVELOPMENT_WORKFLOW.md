# Entwicklung und Verifikation

Aktuelle Architektur und Nachweise: [Projektstand](PROJECT_STATE.md).
Der Entwicklungsweg verwendet die eigene Agent Bridge. Frühere Read-only-POC-Stände
stehen im Journal und sind keine aktuelle Beschreibung des Funktionsumfangs.

## Arbeitsprinzip

Das Spiel ist ein austauschbares Testsystem. Schnelle, effiziente Entwicklung hat
Vorrang vor Koloniepflege. Testbauten, Abriss und Simulationsläufe sind normale
Testmittel; kein Save-Schutz oder Wiederherstellen des Ausgangszustands erforderlich.
Zusammenhängende Änderungen bündeln, zuerst einen aussagekräftigen Fall prüfen,
bei Bedarf erweitern. Gesicherte Ergebnisse nur bei relevanten Änderungen erneut prüfen.

## Build und Tests

Vor Änderungen `git status --short --branch` prüfen und vorhandene Nutzeränderungen erhalten.
Für Codeänderungen im Repository:

```powershell
pwsh -NoProfile -File ./scripts/verify.ps1
```

Das Skript setzt projektlokale Caches, verwendet Locked-Mode-Restore, baut Release und
führt die regulären Tests mit synthetischen Daten aus. Der erste Restore benötigt
Netzwerkzugriff; Abhängigkeiten sind in Directory.Packages.props und Lockfiles gepinnt.
Keine globalen Tools werden installiert. Aktueller Nachweis: 648 Tests.

Bei reinen Dokumentations-/Beschreibungsänderungen genügen Inhalts-, Link-, JSON- und
Diff-Prüfung; keine unnötige Wiederholung der Spieltests.

## Eigene Mod bauen und paketieren

```powershell
pwsh -NoProfile -File ./scripts/build-native-bridge.ps1 `
  -TimberbornManagedDir '<Spielverzeichnis>/Timberborn_Data/Managed'
```

Der Mod-Build ist separat vom Solution-Build: proprietäre Spielbibliotheken liegen nicht
im Repository. Ziel netstandard2.1; Server/Tests gemäß .NET-SDK-Version in global.json.
Jeder Aufruf erzeugt ein neues Paket unter `.local/packages/`; das verteilbare ZIP enthält
nur eigene DLLs, Manifest, Lizenz und Installationshinweise. Die zusätzlich lokal erzeugte
private Konfiguration gehört nicht ins ZIP oder Git.

Für den Dateiaustausch muss das Spiel beendet sein; Speichern ist nicht nötig. Installationsziel und Mod-ID prüfen,
vorhandenen Modordner vollständig sichern, fünf Paketdateien vergleichen und private
Konfiguration/Identitätsdateien erhalten. Keine Spielbibliotheken oder Fremdmods mitkopieren.
[Installation](docs/native-bridge-install.md).

## Live-Prüfung

```powershell
# Eigene Mod, geladenes Testspiel, ausschließlich lesender MCP-Abnahmetest:
pwsh -NoProfile -File ./scripts/verify.ps1 `
  -NativeConfig '<installierte Mod>/bridge.local.json'
```

Dieser Aufruf führt zuerst die normalen Prüfungen und danach den nativen Lesetest aus.
Dessen Werkzeugkatalog und tatsächlich abgefragte Fälle stehen in `LiveNativeTests.cs`;
Katalogprüfung nicht mit vollständiger funktionaler Abnahme jedes Werkzeugs gleichsetzen.
Schreibtests im Testsystem sind grundsätzlich autorisiert; technische Aktionsschalter
explizit setzen und frische Sessions verwenden. Das Leseskript aktiviert sie nicht.
`-Live` bezeichnet aus Kompatibilitätsgründen weiterhin den **historischen More-HTTP-API-Lesetest**,
nicht den nativen Test. [Legacy-Hinweise](docs/legacy-backend.md).

Vorprüfung, Spielvalidierung, Auftrag, Fertigstellung und Wirkung getrennt nachweisen.
Bei Fehlern oder unconfirmed tatsächliche Wirkung klären und Ursache bearbeiten,
nicht blind wiederholen. Erforderliches Spielende/Neustart mit dem Nutzer koordinieren; die Kontrolle erfolgt über strukturierte MCP-/API-Abfragen, nicht über UI-Eingaben.

## Dokumentation

README beschreibt das Produkt, PROJECT_STATE den aktuellen belegten Stand, BACKLOG offene
Arbeiten, missionsplan die Ziele/Abnahmeschritte. Fachdokumente enthalten Verträge und Grenzen.
Datierte Ergebnisse gehören ins Projektjournal; historische Befunde nicht als neuen aktuellen
Status an bestehende Einführungen anhängen. Versions-/Installations-/Live-Stand unterscheiden.

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
