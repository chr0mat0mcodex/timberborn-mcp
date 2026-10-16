using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ActivityOutcomeTests
{
    private static JsonObject Report(string data)=>JsonNode.Parse("{\"status\":\"ok\",\"data\":"+data+"}")!.AsObject();
    [Theory]
    [InlineData("set_building_paused","true","false","Gebäude pausiert")]
    [InlineData("set_building_paused","false","true","Gebäude freigegeben")]
    [InlineData("set_storage_good","Water","Log","Lagergut geändert")]
    [InlineData("set_storage_mode","obtain","accept","Lagermodus geändert")]
    [InlineData("set_farm_priority","planting","harvesting","Arbeitspriorität geändert")]
    [InlineData("set_farm_crop","Carrot","Potato","Pflanzenpriorität geändert")]
    [InlineData("set_building_recipe","RecipeA","RecipeB","Rezept geändert")]
    public void ConfirmedSettingsDescribeObservedChange(string tool,string value,string previous,string expected)
    {
        var report=Report("{}");report["data"]=JsonSerializer.SerializeToNode(new{outcome="applied",observedValue=value,requestedValue=value,previousValue=previous});
        Assert.StartsWith(expected,ActivityOutcome.Describe(tool,report));
        report["data"]!["previousValue"]=value;
        Assert.Contains("bereits",ActivityOutcome.Describe(tool,report));
        report["data"]!["outcome"]="unconfirmed";
        Assert.Contains("unbestätigt",ActivityOutcome.Describe(tool,report));
        report["data"]!["outcome"]="applied";report["data"]!["observedValue"]="different";
        Assert.Equal("Änderung nicht bestätigt.",ActivityOutcome.Describe(tool,report));
    }
    [Theory]
    [InlineData("running",false,false,"advancing","Simulation läuft", "Abschluss offen")]
    [InlineData("pausing",false,false,"target_reached","Simulationsauftrag", "Abschluss offen")]
    [InlineData("completed",true,true,"target_reached","Simulation beendet", "Pause bestätigt")]
    [InlineData("completed",true,true,"production_time_limit","Simulation beendet", "Produktionsziel nicht bestätigt")]
    [InlineData("completed",true,true,"production_measurement_complete","Simulation beendet", "kein Produktionszielnachweis")]
    [InlineData("cancelled",true,true,"cancelled","Simulation abgebrochen", "Pause bestätigt")]
    [InlineData("failed",true,false,"pause_unconfirmed","Simulation mit Fehler beendet", "Pause nicht bestätigt")]
    [InlineData("interrupted",true,false,"speed_changed","Simulation unterbrochen", "Pause nicht bestätigt")]
    public void SimulationKeepsCompletionPauseAndGoalsSeparate(string state,bool terminal,bool pause,string reason,string start,string evidence)
    {
        var result=Report("{}");result["data"]=JsonSerializer.SerializeToNode(new{state,terminal,pauseConfirmed=pause,reason,elapsedGameHours=1.25,targetReached=true});
        foreach(var tool in new[]{"run_simulation_for","run_simulation_until","inspect_simulation_run","cancel_simulation_run"}) {
            var text=ActivityOutcome.Describe(tool,result);Assert.StartsWith(start,text);Assert.Contains(evidence,text);Assert.Contains("1,25 Spielstunden",text);
        }
    }
    [Fact]
    public void SimulationReasonAloneDoesNotProveProductionTarget()
    {
        var result=Report("{\"terminal\":true,\"state\":\"completed\",\"reason\":\"production_targets_reached\",\"pauseConfirmed\":true,\"elapsedGameHours\":1.0}");
        Assert.Contains("Kein weiterer Zielnachweis",ActivityOutcome.Describe("inspect_simulation_run",result));
        result["data"]!["productionTargetsReached"]=true;
        Assert.Contains("Bestands-/Forschungsziele bestätigt",ActivityOutcome.Describe("inspect_simulation_run",result));
    }
    [Fact]
    public void PlantingPreviewPartialAndHistoricalReceiptsNeverClaimGrowth()
    {
        var result=Report("""
        {"outcome":"preview","summary":{"marked":0,"alreadyMarked":3,"excluded":2,"eligible":12,"unconfirmed":0,"notAttempted":0}}
        """);
        var preview=ActivityOutcome.Describe(PlantingBatchTools.Preview,result);
        Assert.Contains("Nur Vorschau: 12 Zellen geeignet",preview);Assert.Contains("Keine Markierungen gesetzt",preview);
        result["data"]!["outcome"]="unconfirmed";result["data"]!["summary"]!["marked"]=8;result["data"]!["summary"]!["unconfirmed"]=1;result["data"]!["summary"]!["notAttempted"]=3;
        var applied=ActivityOutcome.Describe(PlantingBatchTools.Apply,result);
        Assert.Contains("8 Markierungen gesetzt",applied);Assert.Contains("1 unbestätigt, 3 nicht versucht",applied);Assert.Contains("Wachstum nicht nachgewiesen",applied);
        Assert.StartsWith("Gespeicherter Pflanzbeleg",ActivityOutcome.Describe(PlantingBatchTools.Status,result));
        result["data"]!["outcome"]="applied";result["data"]!["summary"]!["marked"]=12;result["data"]!["summary"]!["unconfirmed"]=0;result["data"]!["summary"]!["notAttempted"]=0;
        Assert.Contains("12 Markierungen gesetzt, 3 bereits vorhanden, 2 ausgeschlossen",ActivityOutcome.Describe(PlantingBatchTools.Apply,result));
        result["data"]!["summary"]!.AsObject().Remove("marked");
        Assert.Equal("Pflanzergebnis ohne vollständige Zählnachweise.",ActivityOutcome.Describe(PlantingBatchTools.Apply,result));
    }
    [Theory]
    [InlineData("not_executed","Nicht ausgeführt")]
    [InlineData("unconfirmed","Ausführung unbestätigt")]
    [InlineData("not_applicable","Kein bestätigtes Ergebnis")]
    public void FailuresNeverClaimAnEffect(string execution,string expected)
    {
        var report=JsonSerializer.SerializeToNode(new{status="error",error=new{execution,message="Untrusted raw message"}})!.AsObject();
        Assert.StartsWith(expected,ActivityOutcome.Describe("set_building_paused",report));
        Assert.Equal("",ActivityOutcome.Describe("inspect_colony",report));
    }
    [Fact]
    public void MissingOrMalformedReportCannotTurnLoggingIntoAnActionFailure()
    {
        Assert.Equal("Kein bestätigtes Ergebnis verfügbar.",ActivityOutcome.Describe("inspect_simulation_run",new JsonObject()));
        Assert.Equal("Kein bestätigtes Ergebnis verfügbar.",ActivityOutcome.Describe("inspect_building_settings",Report("{\"pause\":42}")));
    }
    private static ActivityRequest Request(string state,string? outcome=null)
    {
        var query=new NameValueCollection{{"id","11111111-1111-1111-1111-111111111111"},{"tool","set_building_paused"},{"state",state},
            {"session",state=="running"?"":"22222222-2222-2222-2222-222222222222"},{"reasoning","Synthetischer Zweck"},{"summary","paused=true"}};
        if(outcome is not null)query.Add("outcomeSummary",outcome);
        return ActivityRequest.Parse(query);
    }
    [Fact]
    public void OutcomeOnlyArrivesOnCompletionAndSurvivesPayloadWithoutReplacingIntent()
    {
        var log=new ActivityLog();log.Record(Request("running"),DateTimeOffset.UnixEpoch);
        Assert.Empty(Assert.Single(log.Snapshot()).OutcomeSummary);
        log.Record(Request("applied","Gebäude pausiert – Zustand bestätigt."),DateTimeOffset.UnixEpoch.AddSeconds(1));
        var entry=Assert.Single(log.Snapshot());Assert.Equal("Synthetischer Zweck",entry.Reasoning);Assert.Equal("paused=true",entry.Summary);Assert.Equal(1,entry.Sequence);
        var wire=JsonSerializer.Serialize(ActivityLog.Payload(entry));
        var read=JsonSerializer.Deserialize<ActivityEntry>(wire,Timberborn.Backend.Native.NativeJson.Options)!;
        Assert.Equal("Gebäude pausiert – Zustand bestätigt.",read.OutcomeSummary);
        Assert.False(log.Record(Request("ok","Späte fremde Meldung"),DateTimeOffset.UnixEpoch.AddSeconds(2)));
        Assert.Equal(entry.OutcomeSummary,Assert.Single(log.Snapshot()).OutcomeSummary);
        log.Clear();Assert.False(log.Record(Request("ok","Zu spät"),DateTimeOffset.UnixEpoch));Assert.Empty(log.Snapshot());
    }
    [Fact]
    public void OutcomeTextHasBoundedOptionalWireContract()
    {
        Assert.Empty(Request("ok").OutcomeSummary);
        Assert.Throws<ArgumentException>(()=>Request("running","Schon fertig"));
        Assert.Throws<ArgumentException>(()=>Request("ok",new string('x',401)));
        Assert.Equal("[redacted]",Request("ok","Bearer synthetic-secret").OutcomeSummary);
    }
}
