using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed class NativeClearedGroupPort(NativeClient client) : IClearedGroupPort
{
    private static NameValueCollection Q(params (string Key,object Value)[] values) {
        var q=new NameValueCollection();foreach(var (k,v) in values)q[k]=Convert.ToString(v,CultureInfo.InvariantCulture);return q;
    }
    private static T Check<T>(ClearedGroupSpec s,BridgeEnvelope<T> e) {
        if(e.SessionId!=s.Session)throw new BridgeRejectionException("stale_session");
        if(e.BridgeVersion is not ("0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0"))throw new InvalidDataException("cleared_groups_require_052");return e.Data;
    }
    public async Task RequirePaused(ClearedGroupSpec s,CancellationToken ct) {
        if(Check(s,await client.Simulation(ct)).CurrentSpeed!=0)throw new BridgeRejectionException("state_conflict");
    }
    public async Task<ClearanceObservation> Observe(ClearedGroupSpec s,bool areas,CancellationToken ct) {
        var query=Q(("kind","all"),("x",s.X),("y",s.Y),("z",s.Z),("width",s.Width),("height",s.Height),("depth",4),("offset",0),("limit",256));
        var observed=Check(s,await client.RemovalTargets(RemovalRequest.Parse("/agent-api/v1/removal-targets",query),ct));
        if(observed.HasMore || observed.Total!=observed.Items.Length)throw new InvalidDataException("clearance_observation_incomplete");
        var objects=observed.Items;
        var cells=new List<ClearanceDesignation>();
        if(areas)foreach(string kind in new[]{"crops","tree_planting"}) {
            var q=Q(("kind",kind),("offset",0),("limit",256),("x",s.X),("y",s.Y),("z",s.Z),("width",s.Width),("height",s.Height));
            var page=Check(s,await client.Areas(ManagementRequest.Parse("/agent-api/v1/areas",q),ct));
            if(!page.Supported || page.HasMore || page.Total!=page.Items.Length)throw new InvalidDataException("clearance_designations_incomplete");
            cells.AddRange(page.Items.Select(c=>new ClearanceDesignation(kind,c.Position,c.Resource)));
        }
        return new(objects,cells.ToArray());
    }
    public async Task RemoveDesignation(ClearedGroupSpec s,ClearanceDesignation d,CancellationToken ct) {
        if(d.Width is <1 or >4 || d.Height is <1 or >4 || d.Kind is not ("crops" or "tree_planting") ||
            Enumerable.Range(0,d.Width).Any(x=>Enumerable.Range(0,d.Height).Any(y=>!ClearedGroupPolicy.InClearance(s,new(d.Position.X+x,d.Position.Y+y,d.Position.Z)))))throw new ArgumentException();
        await RequirePaused(s,ct);
        var q=Q(("kind",d.Kind),("operation","remove"),("resource",""),("expectedResource",d.Resource),("session",s.Session),
            ("x",d.Position.X),("y",d.Position.Y),("z",d.Position.Z),("width",d.Width),("height",d.Height));
        var receipt=Check(s,await client.SetArea(ManagementRequest.Parse("/agent-api/v1/set-area",q),ct));
        if(receipt.Outcome!="applied")throw new InvalidDataException("designation_unconfirmed");
    }
    public async Task Mark(ClearedGroupSpec s,NativeRemovalTarget t,CancellationToken ct) {
        // Intentionally no route for debris/buildings/unknown objects, even on a malformed checkpoint.
        if(t.Kind is not ("planted" or "vegetation") || !ClearedGroupPolicy.InClearance(s,t.Position) || !t.CanDelete || t.Mode!="demolition_mark")throw new ArgumentException();
        await RequirePaused(s,ct);
        var q=Q(("kind",t.Kind),("id",t.Id),("template",t.Template),("session",s.Session),("operation","mark"),
            ("expectedMarked",t.Marked?"true":"false"),("x",t.Position.X),("y",t.Position.Y),("z",t.Position.Z));
        var receipt=Check(s,await client.RemoveObject(RemovalRequest.Parse("/agent-api/v1/remove-object",q),ct));
        if(receipt.Outcome!="applied")throw new InvalidDataException("vegetation_mark_unconfirmed");
    }
    public async Task<NativeSimulationRun> Clearance(ClearedGroupJob j,string operation,int seconds,CancellationToken ct) {
        var s=j.Spec;var q=Q(("session",s.Session),("runId",j.RunId));
        string route=operation switch{"start"=>"simulation-run-for","cancel"=>"simulation-run-cancel","inspect"=>"simulation-run",_=>throw new ArgumentException()};
        if(operation=="start") {
            q["duration"]=s.ClearanceHours.ToString(CultureInfo.InvariantCulture);q["unit"]="hours";q["speed"]=s.Speed.ToString(CultureInfo.InvariantCulture);
            q["expectedSpeed"]="0";q["maxRealSeconds"]=seconds.ToString(CultureInfo.InvariantCulture);
        } else if(operation=="inspect")q["diagnose"]="false";
        return Check(s,await client.SimulationRun(SimulationRunRequest.Parse("/agent-api/v1/"+route,q),q,ct));
    }
    public async Task<JsonObject> Plan(ClearedGroupSpec s,CancellationToken ct) {
        var result=await RegionGroupPlanning.Invoke(client,ClearedGroupPolicy.PlanningArgs(s),ct, new NativeRegionSurveyPort(client,new SurveyRect(s.X,s.Y,s.Z,s.Width,s.Height)));
        return result["data"]!.AsObject().DeepClone().AsObject();
    }
    public async Task<JsonObject> Build(ClearedGroupJob j,string operation,int seconds,CancellationToken ct) {
        var s=j.Spec;var q=Q(("session",s.Session),("actionId",j.ActionId));
        if(operation=="run") {
            var plan=j.Plan ?? throw new InvalidDataException();
            if(plan["state"]?.GetValue<string>()!="complete")throw new InvalidDataException();
            var args=new JsonObject{["session"]=s.Session,["actionId"]=j.ActionId,["districtId"]=s.DistrictId,
                ["steps"]=plan["steps"]!.DeepClone(),["planKey"]=plan["planKey"]!.DeepClone(),["mode"]="development_pilot",
                ["speed"]=s.Speed,["maxGameHours"]=s.ConstructionHours,["maxRealSeconds"]=seconds};
            var e=await client.LargeProject(LargeProjectTools.Parse("run_large_project",JsonSerializer.SerializeToElement(args)),ct);
            return JsonNode.Parse(Check(s,e).GetRawText())!.AsObject();
        }
        if(operation is not ("inspect" or "close" or "stop"))throw new ArgumentException();
        var envelope=await client.LargeProject(LargeProjectRequest.Parse("/agent-api/v1/large-project-"+operation,q),ct);
        return JsonNode.Parse(Check(s,envelope).GetRawText())!.AsObject();
    }
}
