# 0.38.0 — Rezeptwahl und Betriebsdiagnose

Nach menschlichem Gate und begrenztem Livetest am 2026-10-11 abgenommen.
Vorgängerabschluss: [0.37.0](releases/0.37.0.md).

## Live-Nachweis

- Native 0.38.0 nach menschlicher Vorbereitung erreichbar. Veraltete Negativfixture
  korrigiert; der nachfolgende Paketstand und die geladene Bridge sind 0.38.0.
- Grill: drei native Rezeptoptionen; GrilledSpadderdock → GrilledPotato gesetzt
  und separat rückgelesen. Veralteter Erwartungswert: `state_conflict`;
  unbekanntes Rezept: `unavailable_recipe`. Auswahl nach beiden Ablehnungen
  unverändert. Ursprüngliches GrilledSpadderdock wiederhergestellt.
- Papierfabrik: 1 Holz → 2 Papier, 1,56 Stunden korrekt gelesen. Pausekontrolle
  liefert `building_paused`; Pause zurückgenommen und rückgelesen.
- Bäckerei: Bread → MaplePastry → Bread, Rücklesung bestätigt. Begrenzter Lauf
  über 16,9375 Spielstunden in etwa 50 Sekunden; automatische Pause an Tag 629,
  Stunde 10. Brot im nativen Ausgangsinventar 180 → 190, Mehl 38 → 37 bei
  möglicher gleichzeitiger Belieferung. Der Outputanstieg ist der Produktionsbeleg;
  keine exakte Produktionsrate oder reine Verbrauchsbilanz daraus ableiten.
- Nach dem Lauf belegt die Diagnose fehlenden Produktplatz, nicht mehr fehlende
  Arbeitszeit. `productionProven=false` bleibt absichtlich eine Grenze der
  Momentaufnahme; der getrennte Vorher-/Nachhervergleich belegt Output.

Keine Testbauten, Abrisse oder Rohstofflöschung. Ursprüngliche Rezepte und
Gebäudeaktivierung wiederhergestellt; globale Simulation am Ende pausiert.
Kraftbeleg `powered=false` bei Netzbedarf null belegt keine Anschlussursache;
Diagnosegründe sind aktuelle Zustände, keine unabhängigen Ursachenbeweise.

Der neue Rezeptaufruf erschien erst nach Clientneustart, obwohl der Supervisor
seinen Katalog bereits aktualisiert hatte. Kein zusätzlicher Build nötig.
Verbesserungsvorschläge: sichtbarer Hinweis auf veralteten Clientkatalog,
Vorlagenfilter für Gebäudesuche und Einordnung von Kraftzustand bei inaktivem
Verbraucher. Nur notiert, nicht Bestandteil dieses Nachweises.

## Schnittstellenvertrag

Fraktionsunabhängig: Quelle ist ausschließlich die `ProductionRecipes`-Liste
der konkreten nativen Manufactory, keine Folktails-/Gebäude-/Rezept-Whitelist.
Damit werden auch andere Rezeptlisten der Iron Teeth sowie weitere native
Manufactory-Gebäude über denselben Pfad gelesen und gesetzt. Agenten müssen IDs
aus der aktuellen Gebäudeantwort verwenden, niemals Folktails-Rezepte übertragen.
Live geprüft sind bisher Folktails-Grill und -Bäckerei; Iron Teeth sind noch kein
eigener Livenachweis. Komponenten außerhalb von Manufactory werden nicht als
unterstützt ausgegeben. Vertragsgrenzen: fertige Gebäude, bis 64 Rezepte und
je 64 Zutaten/Produkte. Eine neue Rezeptart innerhalb dieses Vertrags benötigt
keine separate Freischaltung im MCP-Code.

`inspect_building_settings(id, session)` ergänzt `recipeComponentPresent` und
`recipes`: aktuelle `selectedRecipe` (leer = keine), `canChangeRecipe`,
`allowedRecipes` mit ID, Zutaten/Produkten und Mengen, Zyklusdauer in Stunden,
Forschungspunkten und optionalem Brennstoff. Maximal 64 Rezepte und je 64 Güter.
Ältere Bridge: Komponentenbeleg null; keine Manufactory: false; unfertiges Gebäude:
Komponentenbeleg vorhanden, aber noch keine Betriebsoptionen. Keine globalen
Rezeptkataloge pro Gebäude nachladen.

`set_building_recipe(id, session, recipe, expectedRecipe)` wählt ausschließlich
ein Rezept aus der nativen `Manufactory.ProductionRecipes` des fertigen Gebäudes.
Ausführung über öffentliche `Manufactory.SetRecipe(RecipeSpec)` im Spielthread,
mit bestehendem `enableBuildingSettings`-Gate, Sessionprüfung und bodylosem POST.
Die öffentlichen Signaturen wurden lesend in den installierten Spielassemblies
geprüft; keine private Reflection im Laufzeitcode und keine neue Abhängigkeit.

`expectedRecipe` muss der aktuellen Auswahl entsprechen; leer bedeutet keine
Auswahl. Leere Zielauswahl, unbekannte Rezepte, andere Gebäudearten und Baustellen
werden abgelehnt. Passende Auswahl ist ein No-op. Rücklesung bestätigt nur die
Auswahl. Der reguläre Spielservice kann laufenden Fortschritt beim Wechsel
zurücksetzen; die Bridge manipuliert keine Inventare oder Produktionsfortschritte.
Bei `unconfirmed` oder Transportabbruch erst lesen, niemals automatisch wiederholen.

`inspect_building_operation` ergänzt `power` (Komponente, Verbraucher,
versorgt, gültiges Netz, Angebot/Nachfrage) und `assessment`:
belegte Blockaden, Warnungen und unbekannte Prüfpunkte. Rezept, Pause, Personal,
Arbeitszeit, Zutaten, Brennstoff, Produktplatz und Kraft kommen aus einer nativen
Beobachtung; keine zusätzlichen internen Requests. Teilbesetzung und Netzdefizit
sind Warnungen, fehlende Daten bleiben unbekannt. Unfertige Gebäude liefern keine
Betriebsdiagnose. Kein laufender Job allein ist keine Blockade. Lokalisierte
Statusmeldungen werden nicht als maschinenlesbare Gründe interpretiert.
`productionProven` bleibt false: aktuelle Bereitschaft ist kein gemessener Output.
Keine vollständige Wasser-, Rohstoffzugangs- oder Lieferkettenanalyse.

## Gate und begrenzter Pilot

Vor Übergabe Backend stoppen und Starts sperren. Mensch führt das reguläre
Vorbereitungsskript aus und meldet nach Spielstart `live bereit`.

1. Ein bestehendes fertiges Produktionsgebäude mit mehreren Rezepten auswählen.
   Native Auswahl und Zutaten-/Produktdaten lesen; keine neue Fabrik bauen.
2. Auf ein erlaubtes Rezept wechseln und separat rücklesen. Einen veralteten
   `expectedRecipe` verwenden: `state_conflict`, Auswahl bleibt unverändert.
   Unbekannte Rezept-ID ablehnen. Bei fehlerhaftem Kontrollfall stoppen.
3. Betriebsdiagnose mit gezielter Pause/Resume-Kontrolle prüfen; Personal,
   Zutaten und Kraft gegen dieselbe Rohbeobachtung abgleichen.
4. Für tatsächlichen Output einen begrenzten Produktionslauf nutzen und Output
   separat belegen. Kein unbegrenzter Kolonieausbau bei fehlenden Voraussetzungen.
   Danach Pausenzustand und eventuell offene Teständerungen dokumentieren.

Quelltests decken Vertrag, Schreibgate, Zustandskonflikt, No-op, Rücklesekorrelation,
ungültige Optionen, Kraftdaten und unbekannte Beobachtungen ab. Integration über
echtes stdio prüft Rezeptwahl und veralteten Erwartungswert. Tests laufen im menschlichen Gate; Commit/Push dieses Features folgen dem
bestandenen Livetest.
