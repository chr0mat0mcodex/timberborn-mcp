using System.Text.Json.Nodes;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuildingActivityOutcomeTests
{
    private static string Describe(string tool,string data)=>ActivityOutcome.Describe(tool,
        new JsonObject { ["status"]="ok",["data"]=JsonNode.Parse(data) });
    [Fact]
    public void CompletedOrdersAreNotFinishedBuildings()
    {
        var text=Describe("inspect_building_project","""
            {"state":"completed","steps":[{"state":"confirmed"},{"state":"unconfirmed"}]}
            """);
        Assert.Contains("1 von 2 Bauaufträgen bestätigt",text);
        Assert.Contains("Fertigbau nicht nachgewiesen",text);
        Assert.Contains("Bauanfrage übermittelt",Describe("start_building_project","{\"requestSubmitted\":true}"));
    }
    [Theory]
    [InlineData("finished_accessible","Gebäudezugang bestätigt")]
    [InlineData("access_unproven","Gebäudezugang nicht nachgewiesen")]
    [InlineData("budget_exhausted","Zeitbudget ausgeschöpft")]
    [InlineData("running","Bausimulation läuft")]
    public void CompletionKeepsObjectCompletionAndAccessSeparate(string outcome,string expected)
    {
        var d=new JsonObject { ["outcome"]=outcome,["steps"]=JsonNode.Parse("[{\"found\":true,\"finished\":true},{\"found\":true,\"finished\":false}]") };
        if(outcome=="finished_accessible")d["steps"]![1]!["finished"]=true;
        var text=Describe("inspect_building_completion",d.ToJsonString());
        Assert.Contains(outcome=="finished_accessible"?"2 von 2 Bauteilen fertig":"1 von 2 Bauteilen fertig",text);Assert.Contains(expected,text);
        Assert.Equal(text,Describe("develop_building_project",new JsonObject { ["completion"]=d }.ToJsonString()));
    }
    [Fact]
    public void BatchStopDoesNotClaimPauseOrFreshObservation()
    {
        var text=Describe("stop_building_batch","{\"state\":\"cancelled\",\"finishedCount\":2,\"total\":5}");
        Assert.StartsWith("Gespeicherter Chargenstand",text);Assert.Contains("2 von 5 Gebäuden",text);
        Assert.Contains("bestehender Simulationslauf behält sein Budget",text);
        Assert.Contains("Fertigbauzahlen unbekannt",Describe("inspect_building_batch","{}"));
        Assert.Contains("Stopp angefragt; Abschluss noch offen",Describe("stop_cleared_building_group","{\"stopRequested\":true,\"terminal\":false}"));
    }
    [Fact]
    public void LargeProjectFullAndCompactKeepRecordedEvidenceAndPartialCounts()
    {
        var d=JsonNode.Parse("""
            {"state":"stopped","reason":"raw-private-reason","partStates":["finished","construction","unconfirmed","pending"],"entityIds":["a","b","c","d"]}
            """)!.AsObject();
        var full=Describe("inspect_large_project",d.ToJsonString());
        Assert.StartsWith("Gespeicherter Projektstand",full);Assert.Contains("1 von 4 Bauteilen",full);
        Assert.Contains("2 Platzierungen bestätigt, 1 unbestätigt, 1 noch offen",full);
        Assert.DoesNotContain("raw-private-reason",full);
        ProjectReportPresentation.Compact("inspect_large_project",d);
        Assert.Equal(full,Describe("inspect_large_project",d.ToJsonString()));
        Assert.InRange(full.Length,1,400);
    }
    [Fact]
    public void ReleaseIsNotFullCompletionAndCompactDiagnosisKeepsTotal()
    {
        var d=JsonNode.Parse("""
            {"released":true,"parts":[{"observed":"finished","recorded":"finished","reason":"finished"},
            {"observed":"finished_access_missing","recorded":"construction","reason":"blocked"},
            {"observed":"pending","recorded":"pending","reason":"not_ordered"}]}
            """)!.AsObject();
        var full=Describe("close_large_project",d.ToJsonString());
        Assert.StartsWith("Bauprüfung",full);Assert.Contains("1 von 3 Bauteilen",full);
        Assert.Contains("offene Bauteile können aufgegeben sein",full);
        ProjectReportPresentation.Compact("close_large_project",d);
        Assert.Equal(full,Describe("close_large_project",d.ToJsonString()));
        var group=Describe("inspect_cleared_building_group",new JsonObject { ["state"]="completed",["construction"]=d }.ToJsonString());
        Assert.StartsWith("Gespeicherter Räum-/Baustand",group);Assert.Contains("1 von 3 Bauteilen",group);
        Assert.InRange(group.Length,1,400);
    }
    [Theory]
    [InlineData("run_large_project","running",14)]
    [InlineData("inspect_large_project","running",14)]
    [InlineData("inspect_large_project","blocked",14)]
    [InlineData("inspect_large_project","completed",15)]
    [InlineData("stop_large_project","stopped",14)]
    [InlineData("run_large_project_repair","running",14)]
    public void BatchPartsDoNotTurnRecordedProgressIntoFreshDiagnosis(string tool,string state,int finished)
    {
        // Native BatchReceipt includes both partStates and descriptive parts (state, not observed).
        var states=new JsonArray();var ids=new JsonArray();var parts=new JsonArray();
        for(int i=0;i<15;i++) {
            string partState=i<finished?"finished":"construction";
            states.Add(partState);ids.Add("synthetic-"+i);
            parts.Add(new JsonObject { ["index"]=i,["state"]=partState });
        }
        var data=new JsonObject { ["executionMode"]="batch",["state"]=state,
            ["partStates"]=states,["entityIds"]=ids,["parts"]=parts };
        string original=data.ToJsonString();
        string full=ActivityOutcome.Describe(tool,new JsonObject { ["status"]="ok",["data"]=data });
        Assert.StartsWith("Gespeicherter Projektstand",full);
        Assert.Contains($"{finished} von 15 Bauteilen",full);
        Assert.DoesNotContain("Bauprüfung:",full);
        Assert.Equal(original,data.ToJsonString());
        ProjectReportPresentation.Compact(tool,data);
        Assert.Equal(full,Describe(tool,data.ToJsonString()));
        Assert.InRange(full.Length,1,400);
    }
    [Theory]
    [InlineData("diagnose_large_project")]
    [InlineData("close_large_project")]
    public void FreshDiagnosisCountsObservedCompletionDespiteOlderLedger(string tool)
    {
        var data=JsonNode.Parse("""
            {"state":"blocked","reason":"game_time_limit","closeEligible":true,
             "lostConnections":0,"parts":[
               {"index":0,"recorded":"construction","observed":"finished","reason":"finished"},
               {"index":1,"recorded":"finished","observed":"finished","reason":"finished"}]}
            """)!.AsObject();
        string original=data.ToJsonString();
        string full=ActivityOutcome.Describe(tool,new JsonObject { ["status"]="ok",["data"]=data });
        Assert.StartsWith("Bauprüfung:",full);
        Assert.Contains("2 von 2 Bauteilen",full);
        Assert.Equal(original,data.ToJsonString());
        ProjectReportPresentation.Compact(tool,data);
        Assert.Equal(full,Describe(tool,data.ToJsonString()));
    }
    [Theory]
    [InlineData("plan_large_project")]
    [InlineData("plan_large_project_access")]
    [InlineData("plan_large_project_repair")]
    [InlineData("plan_cleared_building_group")]
    public void PlansNeverClaimOrders(string tool)=>Assert.StartsWith("Nur",Describe(tool,"{}"));
    [Fact]
    public void ErrorAndMalformedDataNeverBecomeSuccess()
    {
        Assert.Equal("Kein bestätigtes Ergebnis verfügbar.",ActivityOutcome.Describe("start_building_batch",
            JsonNode.Parse("{\"status\":\"error\",\"data\":{\"state\":\"completed\"}}")!.AsObject()));
        Assert.Equal("Kein bestätigtes Ergebnis verfügbar.",Describe("inspect_large_project","{\"partStates\":[42],\"entityIds\":[\"a\"]}"));
        Assert.Contains("Fertigbauzahlen unbekannt",Describe("inspect_building_batch","{\"finishedCount\":9,\"total\":2}"));
        Assert.Equal("",Describe("inspect_power_network","{}"));
    }
}
