using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public static class ActivityCorrelationTools
{
    private static bool ProjectTool(string name) =>
        LargeProjectTools.Handles(name) && name!="inspect_power_network" || BuildingCompletionTools.Handles(name) ||
        name is "execute_building_project_pilot" or "inspect_building_project";

    public static ActivityCorrelation[] Request(string name,JsonElement args)
    {
        var references=new List<ActivityCorrelation>();
        if(args.ValueKind!=JsonValueKind.Object)return references.ToArray();
        void Add(string key,string kind) {
            var values=args.EnumerateObject().Where(p=>p.Name==key).ToArray();
            if(values.Length==1 && values[0].Value.ValueKind==JsonValueKind.String)
                AddValid(references,kind,values[0].Value.GetString(),"request");
        }
        if(ProjectTool(name))Add("actionId","project");
        if(name is "plan_large_project_repair" or "run_large_project_repair")Add("parentActionId","parent_project");
        if(SimulationRunTools.Handles(name))Add("runId","run");
        // Log filters, entity IDs and job IDs are not project/run references.
        return references.ToArray();
    }

    public static ActivityCorrelation[] Result(string name,JsonObject result)
    {
        var references=new List<ActivityCorrelation>();
        if(result["data"] is not JsonObject data)return references.ToArray();
        void Add(JsonObject? node,string key,string kind) {
            if(node?[key] is JsonValue value && value.TryGetValue<string>(out var id))AddValid(references,kind,id,"result");
        }
        if(ProjectTool(name)) {
            Add(data,"actionId","project");
            Add(data,"parentActionId","parent_project");
            Add(data,"runId","run");
            Add(data["simulation"] as JsonObject,"runId","run");
            Add(data["start"] as JsonObject,"actionId","project");
            Add(data["completion"] as JsonObject,"actionId","project");
            Add(data["completion"] as JsonObject,"runId","run");
        }
        if(SimulationRunTools.Handles(name) || BuildingConfigurationTools.Handles(name))Add(data,"runId","run");
        if(ClearedGroupTools.Handles(name)) {
            // Only reported native children, never preallocated dispatcher IDs.
            Add(data["clearanceRun"] as JsonObject,"runId","run");
            var construction=data["construction"] as JsonObject;
            Add(construction,"actionId","project");
            Add(construction?["simulation"] as JsonObject,"runId","run");
        }
        return references.ToArray();
    }
    private static void AddValid(List<ActivityCorrelation> values,string kind,string? text,string source)
    {
        if(!Guid.TryParseExact(text,"D",out var id)||id==Guid.Empty)return;
        string canonical=id.ToString("D");
        if(!values.Any(v=>v.Kind==kind&&v.Id==canonical&&v.Source==source))values.Add(new(kind,canonical,source));
    }
}
