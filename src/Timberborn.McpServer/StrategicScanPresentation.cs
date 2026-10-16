using System.Text.Json.Nodes;

namespace Timberborn.McpServer;

// Projection of an already validated report. No cache mutation or additional game reads.
public static class StrategicScanPresentation
{
    public static readonly string[] OptionalFields = ["tiles", "specialObjects", "corridors", "rivers", "sections", "links", "sources",
        "crossSection", "nearbyTerrainBanks", "requiredChecks", "outlets", "modifiers", "excavationCells"];

    public static void Compact(JsonObject data)
    {
        data.Remove("tiles"); data.Remove("specialObjects");
        if(data["analysis"] is JsonObject analysis)
        {
            analysis.Remove("corridors");
            if(analysis["terraforming"]?["options"] is JsonArray options)
                foreach(var option in options.OfType<JsonObject>()) option.Remove("excavationCells");
            if(analysis["hazards"] is JsonObject hazards && hazards["priorities"] is JsonArray priorities)
            {
                var entries=priorities.OfType<JsonObject>().ToArray();
                Factor("measures","commonMeasures"); Factor("unresolved","commonUnresolved");
                foreach(var entry in entries)
                    if(entry["candidate"] is JsonObject candidate)
                    {
                        candidate.Remove("crossSection"); candidate.Remove("nearbyTerrainBanks");
                        candidate.Remove("requiredChecks");
                    }
                void Factor(string field,string commonField)
                {
                    if(entries.Length==0) return;
                    var common=(entries[0][field] as JsonArray)?.Select(n=>n!.GetValue<string>()).ToHashSet() ?? [];
                    foreach(var entry in entries.Skip(1))
                        common.IntersectWith((entry[field] as JsonArray)?.Select(n=>n!.GetValue<string>()) ?? []);
                    hazards[commonField]=new JsonArray(common.Order(StringComparer.Ordinal).Select(s=>(JsonNode?)JsonValue.Create(s)).ToArray());
                    foreach(var entry in entries)
                        if(entry[field] is JsonArray values)
                            for(int i=values.Count-1;i>=0;i--)
                                if(common.Contains(values[i]!.GetValue<string>())) values.RemoveAt(i);
                }
            }
        }
        // River inventory is full detail. An overview-only request has no hazard report;
        // preserve its river summary and paging handles, but not the inventory pages.
        if(data["analysis"] is JsonObject) data.Remove("rivers");
        else if(data["rivers"] is JsonObject rivers)
        { rivers.Remove("sections"); rivers.Remove("links"); rivers.Remove("sources"); }
    }
}
