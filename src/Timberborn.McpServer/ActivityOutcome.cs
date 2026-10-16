using System.Globalization;
using System.Text.Json.Nodes;

namespace Timberborn.McpServer;

// Only validated native reports; never infer effects from request parameters.
public static class ActivityOutcome
{
    private static string? Text(JsonNode? node,string key)=>node?[key] is JsonValue v&&v.TryGetValue<string>(out var text)?text:null;
    private static bool? Flag(JsonNode? node,string key)=>node?[key] is JsonValue v&&v.TryGetValue<bool>(out var flag)?flag:null;
    private static int? Count(JsonNode? node,string key)=>node?[key] is JsonValue v&&v.TryGetValue<int>(out var n)&&n>=0?n:null;
    public static bool Handles(string name)=>name is "inspect_building_settings" or "set_building_paused" or "set_storage_good" or "set_storage_mode" or "set_farm_priority" or "set_farm_crop" or "set_building_recipe" or "run_simulation_for" or "run_simulation_until" or "inspect_simulation_run" or "cancel_simulation_run" || PlantingBatchTools.Handles(name) || BuildingActivityOutcome.Handles(name);
    public static string Describe(string name,JsonObject result)
    {
        try {return DescribeCore(name,result);}
        catch(Exception ex) when(ex is InvalidOperationException or ArgumentException) {
            return Handles(name)?"Kein bestätigtes Ergebnis verfügbar.":"";
        }
    }
    private static string DescribeCore(string name,JsonObject result)
    {
        if(!Handles(name))return "";
        if(Text(result,"status")=="error")return Text(result["error"],"execution") switch {
            "not_executed"=>"Nicht ausgeführt. Voraussetzung oder Anfrage prüfen.",
            "unconfirmed"=>"Ausführung unbestätigt. Zustand lesend prüfen; nicht erneut ausführen.",
            _=>"Kein bestätigtes Ergebnis verfügbar."
        };
        if(Text(result,"status")!="ok" || result["data"] is not JsonObject d)return "Kein bestätigtes Ergebnis verfügbar.";
        if(BuildingActivityOutcome.Handles(name))return BuildingActivityOutcome.Describe(name,d);
        if(PlantingBatchTools.Handles(name))return Planting(name,d);
        if(name is "run_simulation_for" or "run_simulation_until" or "inspect_simulation_run" or "cancel_simulation_run")return Simulation(d);
        if(name=="inspect_building_settings")return Flag(d["pause"],"paused") switch {
            true=>"Gebäude ist pausiert – Zustand gelesen.", false=>"Gebäude ist nicht pausiert – Betrieb damit nicht nachgewiesen.",
            _=>"Gebäudeeinstellungen gelesen; Pausenzustand nicht verfügbar."
        };
        if(Text(d,"outcome")=="unconfirmed")return "Einstellung unbestätigt. Zustand lesend prüfen; nicht erneut ausführen.";
        var observed=Text(d,"observedValue");
        if(Text(d,"outcome")!="applied" || observed is null || observed!=Text(d,"requestedValue"))return "Änderung nicht bestätigt.";
        if(name=="set_building_paused" && observed is not ("true" or "false"))return "Pausenzustand nicht bestätigt.";
        string setting=name switch {
            "set_storage_good"=>"Lagergut", "set_storage_mode"=>"Lagermodus", "set_farm_priority"=>"Arbeitspriorität", "set_farm_crop"=>"Pflanzenpriorität", "set_building_recipe"=>"Rezept", _=>"Einstellung"
        };
        if(Text(d,"previousValue")==observed)return name=="set_building_paused" ? observed=="true"?"Gebäude war bereits pausiert – Zustand bestätigt.":"Gebäude war bereits freigegeben – Zustand bestätigt; Betrieb nicht nachgewiesen.":$"{setting} bereits wie angefragt – Zustand bestätigt.";
        if(name=="set_building_paused")return observed=="true"?"Gebäude pausiert – Zustand bestätigt.":observed=="false"?"Gebäude freigegeben – Pausenzustand bestätigt; Betrieb nicht nachgewiesen.":"Pausenzustand nicht bestätigt.";
        return $"{setting} geändert – Einstellung bestätigt; Betrieb nicht nachgewiesen.";
    }
    private static string Simulation(JsonObject d)
    {
        string hours="Dauer unbekannt";
        if(d["elapsedGameHours"] is JsonValue v&&v.TryGetValue<double>(out var h)&&!double.IsNaN(h)&&!double.IsInfinity(h)&&h>=0)
            hours=h.ToString("0.###",CultureInfo.GetCultureInfo("de-DE"))+" Spielstunden";
        string pause=Flag(d,"pauseConfirmed")==true?"Pause bestätigt.":"Pause nicht bestätigt.";
        if(Flag(d,"terminal")!=true)return Text(d,"state")=="running"?$"Simulation läuft: {hours}; Abschluss offen.":$"Simulationsauftrag läuft noch an oder wird gestoppt: {hours}; Abschluss offen.";
        string state=Text(d,"state") switch {"completed"=>"Simulation beendet", "cancelled"=>"Simulation abgebrochen", "interrupted"=>"Simulation unterbrochen", "failed"=>"Simulation mit Fehler beendet", _=>"Simulationslauf beendet"};
        string goal=Text(d,"reason") switch {
            "production_time_limit" or "building_production_time_limit"=>"Zeitlimit erreicht; Produktionsziel nicht bestätigt.",
            "production_measurement_complete"=>"Messzeitraum beendet; kein Produktionszielnachweis.",
            "production_targets_reached" when Flag(d,"productionTargetsReached")==true=>"Bestands-/Forschungsziele bestätigt.",
            "building_production_proven" when Flag(d,"buildingProductionReached")==true=>"Produktionszyklen bestätigt; Lieferung nicht nachgewiesen.",
            "stock_target_reached" when Flag(d,"stockTargetReached")==true=>"Bestandsziel bestätigt.",
            "target_reached" when Flag(d,"targetReached")==true=>"Zeitziel erreicht.",
            _=>"Kein weiterer Zielnachweis."
        };
        return $"{state}: {hours}. {pause} {goal}";
    }
    private static string Planting(string name,JsonObject d)
    {
        var s=d["summary"];
        var marked=Count(s,"marked");var already=Count(s,"alreadyMarked");var excluded=Count(s,"excluded");var eligible=Count(s,"eligible");
        var uncertain=Count(s,"unconfirmed");var pending=Count(s,"notAttempted");
        if(marked is null||already is null||excluded is null||eligible is null||uncertain is null||pending is null)return "Pflanzergebnis ohne vollständige Zählnachweise.";
        if(Text(d,"outcome")=="preview")return $"Nur Vorschau: {eligible} Zellen geeignet, {already} bereits markiert, {excluded} ausgeschlossen. Keine Markierungen gesetzt.";
        if(Text(d,"outcome") is not ("applied" or "unconfirmed"))return "Pflanzauftrag ohne bestätigten Abschluss.";
        string prefix=name==PlantingBatchTools.Status?"Gespeicherter Pflanzbeleg: ":"Pflanzbeleg: ";
        return $"{prefix}{marked} Markierungen gesetzt, {already} bereits vorhanden, {excluded} ausgeschlossen, {uncertain} unbestätigt, {pending} nicht versucht. Wachstum nicht nachgewiesen.";
    }
}
