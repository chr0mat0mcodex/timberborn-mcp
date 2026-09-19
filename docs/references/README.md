# Good references — keine Fremdmod-Pflichtbasis

Stand: 2026-09-20. Nutzerziel: Timberborn aktiv per MCP spielen; für Phase 2 Wasser,
Nahrung, Holz, Wege und Wohnraum aufbauen. Die eigene Spielmod verwendet öffentliche
Spielservices direkt. Community-Mods dienen als Anschauungsmaterial und Vergleich.

## Gesicherte Referenzen

[sources.json](sources.json) bindet 17 benötigte Text-/Quelldateien an feste Git-Commits.
`scripts/fetch-references.ps1` legt sie unter `.local/references/` ab, einschließlich
der jeweiligen MIT-Lizenz und eines Inventars mit Quell-URL, Größe und SHA256.
Erster Abruf: 17/17 Dateien, 40.818 Bytes. Keine Quellskripte ausgeführt oder importiert.
Die Kopien werden weder kompiliert noch ausgeliefert oder ins Projekt-Git kopiert;
Katalog, Abrufskript und diese extrahierten Erkenntnisse bleiben im Repository.

| Quelle | Status | Was wir daraus benötigen |
|---|---|---|
| [Mechanistry Modding Tools](https://github.com/mechanistry/timberborn-modding/tree/976487702864412bbcd2dbd6abaf1058465e1ea4) | **official reference** | Game-Kontext, Configurator, Lebenszyklus, Manifest und Paketstruktur; ausgewählte offizielle Beispiele und Buildquellen lokal gesichert |
| [ModdableTimberborn](https://github.com/datvm/TimberbornMods/tree/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn) | **good reference** | Güter-/Betten-/Personalbegriffe, Workplace-Sollbesetzung, Grenzen von Areas; keine Abhängigkeit unserer Mod |
| [More HTTP API](https://github.com/datvm/TimberbornMods/tree/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi) | **good reference / Legacy-Vergleichsbackend** | Blueprint-Katalog, Building-Handler, HTTP-Fehler-/Antwortkonventionen; native Mod benötigt weder Router noch DLL |
| [TimberUi](https://github.com/datvm/TimberbornMods/tree/1c057bda82ec955d1712937da916cddf0f382f24/TimberUi) | **good reference** | mögliche spätere Ingame-UI; aktuell kein UI-Bedarf und daher keine Quelldateien importiert |
| [Mod Settings](https://github.com/eMkaQQ/timberborn-modding) | **good reference** | mögliche Einstellungsoberflächen; private Bridge-Konfiguration benötigt diese Mod nicht; keine Quellübernahme |
| [Harmony](https://github.com/pardeike/Harmony) | **good reference** | Patch-Technik nur für belegte API-Lücken; aktuell keine Patches, keine DLL-Referenz |

Die beiden gesicherten Quellpakete enthalten MIT-Lizenzen (Mechanistry 2024 bzw.
Luke Vo 2024). Die Originalhinweise bleiben bei den lokalen Kopien. Andere oben
verlinkte Projekte wurden nicht übernommen; deren konkrete Datei-/Versionslizenz
wäre erst bei einer tatsächlichen Übernahme zu prüfen. Die Spielbibliotheken sind
nicht durch diese MIT-Lizenzen abgedeckt und werden nicht weiterverteilt.

## Extrahierte technische Erkenntnisse

1. **Start/Lebenszyklus:** offizielle HelloWorld-Registrierung zeigt Game-Kontext und
   Singleton-Bindung. Unsere Mod bindet nur ihren eigenen Dienst. Load/Update/Unload
   begrenzen HTTP-Queue und Spielsession; Spielzugriffe erfolgen im Update-Hook.
2. **Bestände:** GoodStatsProvider verweist auf ResourceCountingService und unterscheidet
   AvailableStock von InputOutputCapacity. Wir fragen den Spielservice direkt ab;
   wir übernehmen weder GameStatService noch dessen dynamische Stat-ID-Infrastruktur.
3. **Wohnraum/Personal:** GlobalPopulationStatsProvider verweist auf PopulationService.
   Betten, Obdachlose und Arbeitsfähigkeit sind getrennte Messwerte. Eine fehlende
   Dwelling-Zuordnung ersetzt keine offizielle Homeless-Zählung.
4. **Arbeitsplatz:** WorkplaceSettings verwendet DesiredWorkers als Sollwert. Daraus
   keine tatsächliche Besetzung oder verfügbare Arbeitskraft ableiten.
5. **Raum:** MapStatsProvider und Areas liefern keine vollständige 3D-Karte/Bauprüfung.
   Unsere Mod nutzt BlockObject, ITerrainService und IThreadSafeWaterMap direkt;
   dafür sind öffentliche Signaturen lokal geprüft, die Semantik braucht Live-Abgleich.
6. **Katalog:** BlueprintHandler ist Referenz für Vorlagen-/Spec-Abfragen. Für spätere
   Bauaktionen brauchen wir aktive Fraktion, Freischaltung, Kosten, Ausrichtung und
   Eingang. Export-/Dateirouten sind dafür nicht erforderlich.
7. **Bauen:** öffentliche Preview-/Validation-/Placer-Signaturen sind Kandidaten,
   noch kein abgenommener Bauablauf. Gültigen und ungültigen Wohnbauplatz prüfen,
   Vorschauen aufräumen, danach reguläre Kosten/Platzierung und Ergebnis-ID nachweisen.
   Keine Fertigbau-/Spawn-Abkürzung aus fremden Mods übernehmen.

Weitere öffentliche Signaturen und Herstellerlinks:
[API-Machbarkeitsbericht](../architecture/native-game-api.md).

## Welche Abhängigkeiten bleiben?

Die native Mod hat **null erforderliche Fremdmods** (`RequiredMods: []`) und keine
NuGet-Pakete. Sie nutzt unsere Bridge.Core-DLL sowie von Timberborn bereitgestellte
Spiel-/Unity-/Bindito-/Newtonsoft-/Frameworkbibliotheken. Diese werden nur aus der
Installation referenziert (`Private=false`), nicht kopiert oder neu heruntergeladen.
Das ist eine Spiel-/Plattformabhängigkeit und kein fremdes Modpaket.

Der externe MCP-Server behält .NET, das offizielle MCP-SDK und dessen reguläre
Bibliotheken. Das MCP-Protokoll selbst neu zu implementieren wäre ein eigener,
hier nicht beauftragter Umbau. MoreHttpApi-Adapter bleibt als vorhandener
Vergleichspfad im Repository; ausschließlich bei expliziter Backendwahl wird er
verwendet. Kein stiller Fallback und keine Vermischung von Sitzungen.

Die vollständige Unity-Werkzeugumgebung, Grafiken, AssetBundles und DLL-Publicizer
werden für unsere reine C#-Brücke nicht gebraucht. Die benötigten Teile des
offiziellen Modding-Projekts sind gesichert; Unity oder Fremdmods wurden nicht installiert.
Ob weitere Spielfunktionen zusätzliche öffentliche APIs benötigen, wird pro Funktion
geprüft. Die Referenzsammlung ist kein Nachweis, dass bereits das ganze Spiel steuerbar ist.
