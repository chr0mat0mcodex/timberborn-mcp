namespace Timberborn.Bridge.Core;

// Endpoint evidence only: no claim about uninterrupted work or gross deliveries.
public sealed class ConstructionProgressSample
{
    public string Id { get; set; } = "";
    public string Observed { get; set; } = "";
    public string Reason { get; set; } = "";
    public double GameHours { get; set; }
    public double? MaterialProgress { get; set; }
    public double? BuildProgress { get; set; }
    public bool? BuildersAvailable { get; set; }
    public Dictionary<string,int>? SiteStock { get; set; }
}

public sealed class ConstructionProgressComparison
{
    public ConstructionProgressSample? Before { get; set; }
    public ConstructionProgressSample After { get; set; } = new();
    public double? ElapsedGameHours { get; set; }
    public double? MaterialProgressDelta { get; set; }
    public double? BuildProgressDelta { get; set; }
    public Dictionary<string,int>? SiteStockDelta { get; set; }
    public string State { get; set; } = "baseline_recorded";
    public string NextStep { get; set; } = "observe_bounded_window";
    public string Explanation { get; set; } = "Ausgangswert erfasst. Nach einem ausdrücklich begrenzten Zeitfenster erneut diagnostizieren; kein automatischer Lauf.";
    public string Scope { get; set; } = "endpoints_only_not_continuous_work_or_gross_deliveries";
}

public static class ConstructionProgressPolicy
{
    public static ConstructionProgressComparison Compare(ConstructionProgressSample? before, ConstructionProgressSample after)
    {
        var result=new ConstructionProgressComparison { Before=before, After=after };
        void Set(string state,string next,string explanation) { result.State=state;result.NextStep=next;result.Explanation=explanation; }
        if(before is null) return result;
        if(before.Id!=after.Id || after.GameHours<before.GameHours) {
            Set("not_comparable","refresh_progress_baseline","Identität oder Spielzeit passt nicht zum Ausgangswert. Neuen Ausgangswert erfassen."); return result;
        }
        result.ElapsedGameHours=after.GameHours-before.GameHours;
        if(result.ElapsedGameHours==0) {
            Set("no_game_time_elapsed","observe_bounded_window","Keine Spielzeit vergangen; kein Stillstandsnachweis. Zeitfenster ausdrücklich wählen."); return result;
        }
        bool Known(string s)=>s is "construction" or "construction_unreachable" or "construction_access_unknown" or "finished" or "finished_access_missing";
        if(!Known(before.Observed)||!Known(after.Observed)) {
            Set("observation_unknown","inspect_unknown_access_or_identity","Objektidentität oder Bauzustand ungeklärt; keine Fortschrittsbehauptung."); return result;
        }
        result.MaterialProgressDelta=after.MaterialProgress-before.MaterialProgress;
        result.BuildProgressDelta=after.BuildProgress-before.BuildProgress;
        if(before.SiteStock is not null && after.SiteStock is not null)
            result.SiteStockDelta=before.SiteStock.Keys.Union(after.SiteStock.Keys).ToDictionary(k=>k,k=>
                (after.SiteStock.TryGetValue(k,out var a)?a:0)-(before.SiteStock.TryGetValue(k,out var b)?b:0));
        if(after.Observed is "finished" or "finished_access_missing") {
            Set(before.Observed is "finished" or "finished_access_missing"?"already_finished":"finished_since_baseline",
                after.Observed=="finished"?"inspect_release_conditions":"inspect_missing_access","Fertigstellung beobachtet; Zugang und Projektabschluss separat prüfen."); return result;
        }
        if(result.BuildProgressDelta>0) {
            Set("construction_progress","observe_within_existing_budget","Baufortschritt gestiegen. Bestehendes Zeitbudget beibehalten; aktuelle Blockaden zusätzlich beachten."); return result;
        }
        if(result.MaterialProgressDelta>0 || result.SiteStockDelta?.Values.Any(v=>v>0)==true) {
            Set("material_progress","observe_delivery_within_existing_budget","Materialfortschritt oder Baustellenvorrat gestiegen. Nettozugang ist keine Bruttoliefermenge; Versorgung im bestehenden Budget weiter beobachten."); return result;
        }
        if(after.Reason=="builders_outside_working_hours") {
            Set("currently_outside_working_hours","wait_for_construction_working_hours","Aktuell gemeinsamer Feierabend. Das gesamte Zeitfenster ist dadurch nicht erklärt; nächste Arbeitsphase ausdrücklich wählen."); return result;
        }
        if(after.Reason=="materials_not_ready") {
            Set("waiting_for_materials","check_material_supply","Kein positiver Materialfortschritt beobachtet. Erzeuger, Lagerbestand und Lieferweg prüfen; keine pauschale Startsperre."); return result;
        }
        if(after.Reason is "construction_paused" or "builders_unreachable" or "waiting_for_support_completion") {
            Set("current_blocker","inspect_current_construction_blocker","Aktuelle Pause, Zugang oder unfertige Stütze gezielt anhand der frischen Baudiagnose prüfen."); return result;
        }
        if(result.BuildProgressDelta<0 || result.MaterialProgressDelta<0) {
            Set("progress_regressed","inspect_progress_regression","Fortschrittswert gesunken; Bauzustand und Materialverbrauch prüfen, keinen Stillstand ableiten."); return result;
        }
        if(result.BuildProgressDelta is null || result.MaterialProgressDelta is null || after.Observed!="construction" || after.Reason!="awaiting_builders_or_work") {
            Set("observation_unknown","inspect_construction_inactivity","Messwerte oder Aktivität ungeklärt; Baustelle und Zugang nachlesen."); return result;
        }
        if(before.BuildersAvailable==true && after.BuildersAvailable==true) {
            Set("no_progress_builders_available_at_endpoints","check_workers_access_and_competing_sites","Kein Baufortschritt zwischen den Messungen; Bauarbeiter an beiden Endpunkten verfügbar. Besetzung, laufende Aufträge, Baustellenzugang, Anmarsch und konkurrierende Baustellen prüfen. Keine durchgehende Arbeit oder Zuweisung zu dieser Baustelle bewiesen."); return result;
        }
        Set("no_progress_observed","check_workers_and_work_progress","Kein Baufortschritt beobachtet. Arbeitszeit und Besetzung prüfen; fehlende oder wechselnde Arbeitszeit erklärt nicht automatisch das ganze Intervall.");
        return result;
    }
}

// One bounded pair per part. Same-time reads retain the comparison window.
public sealed class ConstructionProgressWindow
{
    private ConstructionProgressSample? previous;
    private ConstructionProgressSample? latest;
    public ConstructionProgressComparison Observe(ConstructionProgressSample sample)
    {
        if(latest is not null && (latest.Id!=sample.Id || sample.GameHours<latest.GameHours)) {
            var invalid=ConstructionProgressPolicy.Compare(latest,sample);
            previous=null;latest=sample;return invalid;
        }
        if(latest is not null && sample.GameHours>latest.GameHours) previous=latest;
        var result=ConstructionProgressPolicy.Compare(previous ?? latest,sample);
        latest=sample;
        return result;
    }
}
