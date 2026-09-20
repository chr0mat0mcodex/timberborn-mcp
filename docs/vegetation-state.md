# Vegetationszustand und Zapfkandidaten (0.13.2)

Fehler bis 0.13.1: Die Zapfkandidaten-Abfrage prüfte nur Pine und fehlende Fällmarkierung.
Damit konnten tote, sterbende oder junge Kiefern wie gesunde ausgewachsene erscheinen.
Auch die Objekt-/Abrissabfrage enthielt keinen Lebenszustand. Der Nutzerhinweis war berechtigt.

## Strukturierte Zustände

inspect_removal_targets liefert für natürliche Ressourcen vegetation:

- lifeState: alive, dead oder unknown aus LivingNaturalResource.IsDead.
- isDying: aktueller Sterbezustand aus DyingNaturalResource.IsDying; null ohne Komponente.
- waterStress: WateredNaturalResource.DyingProgress.IsDying; null ohne Komponente.
- isGrown und growthProgress: direkte Growable-Beobachtung, sonst null.

Nicht vorhandene Komponenten bedeuten unbekannt, niemals gesund. WaterStress bezeichnet
aktuellen gemeldeten Stress des Wasserbedarfs, nicht zwingend eine bewiesene frühere
Todesursache. Eine tote Ressource wird anhand lifeState erkannt, auch wenn aktuell kein
Sterbe-/Wasserstress mehr gemeldet wird. Keine Todesursache aus der Vorlage Pine ableiten.

## Zapfkandidaten

inspect_areas(kind=tapping) liefert ab 0.13.2 nur Kiefern ohne Fällmarkierung, für die
lifeState=alive, isDying=false, waterStress=false und isGrown=true tatsächlich beobachtet
wurden. Jede Kandidatenzelle enthält vegetation. Fehlende Daten schließen die Zelle aus.
Der MCP-Client verwirft tote/ungeeignete Einträge und alte Bridge-Antworten für diese
Abfrage, statt die alte Bedeutung unbemerkt weiterzugeben.

Das bleibt ein Kandidatenfilter, kein Nachweis von Harzvorrat, Zapferreichweite oder
Arbeitskräften. set_area(kind=tapping) entfernt weiterhin reguläre Fällmarkierungen im
gewählten Rechteck; es heilt, belebt oder pflanzt keine Bäume. Baumfällmarkierungen und
Abrissziele werden nicht pauschal um tote Pflanzen bereinigt: tote Bäume können weiterhin
Hindernisse sein oder Holz enthalten. Der Agent bekommt dafür den Lebenszustand.

## Belege und Abnahme

Öffentliche Signaturen der installierten Spielversion geprüft: LivingNaturalResource,
DyingNaturalResource, WateredNaturalResource und Growable. Keine neue Fremdmod oder
private Reflexion. Kompilation erfolgreich; Live-Abnahme nach Installation offen.

Die zuvor genannten 124 Kandidaten waren ungefilterte, unmarkierte Kiefern. Sie sind
kein belastbarer Zähler gesunder Harzbäume. Ob bereits entfernte Testkiefern tot waren,
ist nachträglich nicht belegt. Ihr unmittelbares Entfernen allein beweist keinen Tod.
Nach Installation lebende/tote/sterbende Ressourcen rein lesend vergleichen und prüfen,
dass jeder ausgegebene tapping-Kandidat die Zustandsbedingungen erfüllt. Keine weiteren
Abrissversuche sind für diese Korrektur notwendig.
