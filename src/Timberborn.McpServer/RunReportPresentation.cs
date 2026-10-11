using System.Text.Json.Nodes;

namespace Timberborn.McpServer;

// Project an already validated receipt. Full data and the run ledger remain untouched.
public static class RunReportPresentation
{
    public static readonly string[] OptionalFields = ["buildingIds","buildingJournalCursor","goodTargets","stopAtScience","observedScience",
        "productionTargetsReached","productionDiagnostics","diagnosticsObservedGameHours","stopAtAvailableLogs","observedAvailableLogs","stockTargetReached",
        "buildingProduction","buildingProductionReached","measureProduction","products","sciencePerCycle","recipeDerivedScience","recipeDerivedSciencePerHour",
        "currentBuildingOperations","bottleneckObservedGameHours","bottleneckState","limitations","workplace","manufacturing","statuses","inventories","power","bottleneck"];
    public static void Compact(JsonObject data)
    {
        data.Remove("buildingIds");data.Remove("buildingJournalCursor");
        bool stock=data["goodTargets"] is JsonArray {Count:>0}||data["stopAtScience"] is not null;
        if(!stock)Remove(data,"goodTargets","stopAtScience","observedScience","productionTargetsReached","productionDiagnostics","diagnosticsObservedGameHours");
        if(data["stopAtAvailableLogs"] is null)Remove(data,"stopAtAvailableLogs","observedAvailableLogs","stockTargetReached");
        if(data["buildingProduction"] is JsonArray {Count:>0} buildings) {
            foreach(var building in buildings.OfType<JsonObject>()) {
                if(building["measurement"] is not JsonObject m)continue;
                bool science=m["sciencePerCycle"]?.GetValue<double>()>0;
                Remove(m,"products","sciencePerCycle");
                if(!science)Remove(m,"recipeDerivedScience","recipeDerivedSciencePerHour");
                // Only omit known-zero categories; unknown time remains explicit when positive.
                if(m["times"] is JsonArray times)
                    for(int i=times.Count-1;i>=0;i--)
                        if(times[i]?["hours"]?.GetValue<double>()==0)times.RemoveAt(i);
            }
        } else Remove(data,"buildingProduction","buildingProductionReached","measureProduction");
        if(data["currentBuildingOperations"] is JsonObject operations) {
            operations.Remove("limitations");
            if(operations["items"] is JsonArray items)
                foreach(var item in items.OfType<JsonObject>())
                    if(item["operation"] is JsonObject operation)
                        Remove(operation,"workplace","manufacturing","statuses","limitations","inventories","power","bottleneck");
        } else if(data["bottleneckState"] is null)Remove(data,"currentBuildingOperations","bottleneckObservedGameHours","bottleneckState");
        // unavailable retains its explicit state/time, rather than looking like a missing scan.
    }
    private static void Remove(JsonObject node,params string[] keys) {foreach(string key in keys)node.Remove(key);}
}
