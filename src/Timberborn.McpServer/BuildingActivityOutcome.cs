using System.Text.Json.Nodes;

namespace Timberborn.McpServer;

// Presentation only: consume the existing report, never dispatch or refresh a project.
public static class BuildingActivityOutcome
{
    private static string? Text(JsonNode? n,string key)=>n is JsonObject o && o[key] is JsonValue v && v.TryGetValue<string>(out var s)?s:null;
    private static bool IsTrue(JsonNode? n,string key)=>n is JsonObject o && o[key] is JsonValue v && v.TryGetValue<bool>(out var b) && b;
    private static int? Count(JsonNode? n,string key)=>n is JsonObject o && o[key] is JsonValue v && v.TryGetValue<int>(out var i) && i>=0?i:null;
    public static bool Handles(string name)=>name is
        "execute_building_project_pilot" or "inspect_building_project" or "start_building_project" or
        "advance_building_project" or "inspect_building_completion" or "develop_building_project" ||
        BuildBatchTools.Handles(name) || ClearedGroupTools.Handles(name) || ProjectReportPresentation.Supports(name);

    public static string Describe(string name,JsonObject d)
    {
        if(BuildBatchTools.Handles(name))return Batch(d);
        if(ClearedGroupTools.Handles(name)) {
            if(name==ClearedGroupTools.Plan)return "Nur Räumvorschau. Keine Räum- oder Bauaufträge erteilt; Bauplanung folgt erst nach der Räumung.";
            string stop=IsTrue(d,"stopRequested") && !IsTrue(d,"terminal")?" Stopp angefragt; Abschluss noch offen.":"";
            return "Gespeicherter Räum-/Baustand: "+State(d)+stop+" "+
                (d["construction"] is JsonObject construction?Large(construction,true):"Noch kein Baunachweis verfügbar.");
        }
        if(ProjectReportPresentation.Supports(name)) {
            if(name is "plan_large_project" or "plan_large_project_access" or "plan_large_project_repair")
                return "Nur Bauvorschau. Keine Bauaufträge erteilt; Freigabe und Zugangsprüfungen im Fachbericht beachten.";
            return Large(d);
        }
        if(name=="develop_building_project")return d["completion"] is JsonObject completion?Completion(completion):Start(d["start"] as JsonObject);
        if(name is "advance_building_project" or "inspect_building_completion")return Completion(d);
        if(name=="start_building_project")return Start(d);
        return Orders(d);
    }

    private static string Start(JsonObject? d)
    {
        if(d is null)return "Baustart ohne bestätigten Ergebnisbericht.";
        if(d["execution"] is JsonObject e)return Orders(e);
        if(Text(d,"outcome")=="unconfirmed")return "Baustart unbestätigt. Zustand lesend prüfen; nicht erneut ausführen.";
        return IsTrue(d,"requestSubmitted")?"Bauanfrage übermittelt. Auftrag und Fertigbau separat prüfen.":"Kein Baustart bestätigt. Planung und Voraussetzungen im Fachbericht prüfen.";
    }
    private static string Orders(JsonObject d)
    {
        if(d["steps"] is not JsonArray steps)return "Bauaufträge ohne vollständigen Zählnachweis. Fertigbau nicht nachgewiesen.";
        int confirmed=steps.Count(s=>Text(s,"state")=="confirmed");
        return $"Gespeicherter Auftragsstand: {confirmed} von {steps.Count} Bauaufträgen bestätigt. {State(d)} Fertigbau nicht nachgewiesen.";
    }
    private static string Completion(JsonObject d)
    {
        string count=d["steps"] is JsonArray steps?$"{steps.Count(s=>IsTrue(s,"found") && IsTrue(s,"finished"))} von {steps.Count} Bauteilen fertig. ":"Fertigbauzahlen unbekannt. ";
        string outcome=Text(d,"outcome") switch {
            "finished_accessible"=>"Fertigbau und Gebäudezugang bestätigt; Spiel pausiert. Betrieb nicht nachgewiesen.",
            "budget_exhausted"=>"Zeitbudget ausgeschöpft; Bauabschluss offen.",
            "access_unproven"=>"Gebäudezugang nicht nachgewiesen.",
            "entity_missing"=>"Erwartetes Bauobjekt fehlt.",
            "configuration_changed"=>"Gebäudeeinstellung weicht ab.",
            "not_paused"=>"Spielpause nicht bestätigt.",
            "simulation_unconfirmed"=>"Simulationsstart unbestätigt; lesend prüfen, nicht erneut starten.",
            "running"=>"Bausimulation läuft; Abschluss offen.",
            "awaiting_simulation"=>"Wartet auf Bausimulation.",
            "awaiting_order"=>"Bauaufträge noch nicht vollständig bestätigt.",
            "order_stopped"=>"Bauauftrag gestoppt; Fachbericht prüfen.",
            "simulation_stopped"=>"Bausimulation gestoppt; Abschluss offen.",
            _=>"Bauabschluss und Zugang nicht bestätigt; Fachbericht prüfen."
        };
        return count+outcome;
    }
    private static string Batch(JsonObject d)
    {
        var finished=Count(d,"finishedCount");var total=Count(d,"total");
        string count=finished is not null && total is not null && finished<=total?$"{finished} von {total} Gebäuden fertig und zugänglich belegt. ":"Fertigbauzahlen unbekannt. ";
        return "Gespeicherter Chargenstand: "+count+(Text(d,"state")=="cancelled"
            ?"Weitere Aufträge gestoppt; ein bestehender Simulationslauf behält sein Budget."
            :State(d));
    }
    private static string Large(JsonObject d,bool checkpoint=false)
    {
        // Reuse the existing projection on a clone: compact and full reports share evidence semantics.
        var report=d.DeepClone().AsObject();
        if(report["summary"] is not JsonObject)
            ProjectReportPresentation.Compact(report["parts"] is JsonArray?"diagnose_large_project":"inspect_large_project",report);
        var s=report["summary"];var total=Count(s,"planned");
        bool fresh=Text(s,"evidence")=="fresh_recovery_observation";
        var finished=Count(s,fresh?"finishedAccessible":"finishedAccessRecorded");
        string count=total is not null && finished is not null && finished<=total
            ?$"{finished} von {total} Bauteilen fertig und zugänglich belegt. ":"Fertigbauzahlen unbekannt. ";
        string orders="";
        if(!checkpoint && !fresh && Count(s,"confirmedPlaced") is int placed && Count(s,"unconfirmed") is int uncertain && Count(s,"pending") is int pending)
            orders=$"{placed} Platzierungen bestätigt, {uncertain} unbestätigt, {pending} noch offen. ";
        if(checkpoint)return count+"Keine neue Bau- oder Zugangsprüfung durch diese Abfrage.";
        return (fresh?"Bauprüfung: ":"Gespeicherter Projektstand: ")+count+orders+
            (IsTrue(d,"released")?"Projekt freigegeben; offene Bauteile können aufgegeben sein.":State(d));
    }
    private static string State(JsonObject d)=>Text(d,"state") switch {
        "completed"=>"Ablauf abgeschlossen.",
        "running" or "building" or "clearance_wait"=>"Bearbeitung läuft; Abschluss offen.",
        "ready"=>"Bereit für den nächsten Bauschritt.",
        "pausing"=>"Pause wird angefordert; noch nicht bestätigt.",
        "stopped" or "blocked" or "failed"=>"Bearbeitung angehalten. "+Reason(d),
        "unconfirmed" or "interrupted"=>"Ausführung unbestätigt oder unterbrochen; lesend prüfen, nicht wiederholen.",
        _=>"Zwischenstand; nächsten Schritt im Fachbericht prüfen."
    };
    private static string Reason(JsonObject d)=>Text(d,"reason") switch {
        "materials_missing"=>"Baumaterial fehlt.",
        "budget_exhausted" or "clearance_budget_exhausted"=>"Zeitbudget ausgeschöpft.",
        "access_unproven"=>"Zugang nicht nachgewiesen.",
        "no_candidate_in_authorized_regions" or "no_candidate_in_authorized_region"=>"Kein geeigneter Bauplatz gefunden.",
        _=>"Stopgrund im Fachbericht prüfen."
    };
}
