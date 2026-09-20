# Zukunftsfeature: Agentenfrage im Spiel

## Kurzrecherche 2026-09-20

Theoretisch über öffentliche Timberborn-1.1.2.4-APIs möglich, noch nicht implementiert
oder live geprüft. Keine zusätzliche UI-Mod erscheint für den Basisdialog erforderlich.
Öffentliche Metadaten der lokalen Timberborn.CoreUI-Bibliothek belegen:

- InputBoxShower.Create() -> Builder mit SetDefaultValue(string),
  SetLocalizedMessage(string), SetConfirmButton(Action<string>) und Show().
  Der Callback erhält die eingegebene Antwort.
- DialogBoxShower.Create() -> Builder mit SetMessage(string),
  AddContent(VisualElement), SetConfirmButton(Action), SetCancelButton(Action)
  und Show() -> DialogBox. DialogBox.Close() ist öffentlich.
- PanelStack.PushDialog(IPanelController); IPanelController liefert GetPanel(),
  OnUIConfirmed() und OnUICancelled().

Ein vorhandener InputBox-Dialog ist die einfachste Referenz. Für eine beliebige
Agentenfrage mit sauber beobachtbarem Abbruch und expliziter Dialogreferenz bietet
sich DialogBoxShower mit TextField als eigenem Inhalt an. Genaue Gestaltung,
Lokalisierung und Fokusverhalten müssen in einem kleinen UI-Prototyp geprüft werden.

## Vorgeschlagener späterer MCP-Vertrag

1. ask_player(question, session) stellt genau eine Frage auf dem Spielthread dar
   und gibt sofort questionId/status=pending zurück. Begrenzte Textlänge, maximal
   ein offener Dialog; keine blockierende HTTP-Anfrage während menschlicher Antwort.
2. get_player_answer(questionId, session) liest pending/answered/cancelled/expired
   und gegebenenfalls Antworttext. Optional spätere MCP-Benachrichtigung, keine
   Voraussetzung für den ersten Prototyp.
3. Antwort/Abbruch nur der passenden Frage zuordnen; bei Szenenwechsel Dialog
   schließen und Frage ungültig machen. Keine Speicherung im Save oder Git.

Spielpause durch Dialogstack, Eingabefokus, Escape, Ladewechsel und Weiterlaufen
nach Schließen ausdrücklich testen. Keine automatische Antwort, keine versteckte
Freigabe weiterer Aktionen und kein pauschales UnlockSpeed. Antworttext ist
Nutzereingabe, keine ausführbare Konfiguration. Frage-ID verhindert Vermischung
von Antworten mit späteren Fragen. Inhalte weder roh protokollieren noch committen.

Abnahme später: Agent stellt eine Frage; Nutzer beantwortet sie im Spiel; dieselbe
Antwort wird über MCP zurückgelesen. Zusätzlich Abbruch und Szenenwechsel testen.
Aktuell nur Zukunftsplanung; laufender Ausbau bleibt die Arbeitsplatzsteuerung.
