using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Timberborn.McpServer;

// Lifetime: one group planning call. Never cache simulation boundaries or native plans.
public sealed class SharedRegionSurveyPort(IRegionSurveyPort inner) : IRegionSurveyPort
{
    private readonly Dictionary<object,object> observations=[];
    private string? session,version;
    public int NativeReads { get; private set; }
    public int CacheHits { get; private set; }
    public int PlanningCalls { get; private set; }
    public JsonObject Occupations()
    {
        var targets=observations.Values.OfType<BridgeEnvelope<NativeRemovalTargets>>()
            .SelectMany(e=>e.Data.Items).GroupBy(t=>t.Id);
        var buildings=observations.Values.OfType<BridgeEnvelope<NativeObjects>>()
            .SelectMany(e=>e.Data.Items).GroupBy(b=>b.Id).ToDictionary(g=>g.Key,g=>g.First());
        var entries=new JsonArray();
        foreach(var group in targets) {
            var item=group.First();
            string kind=item.Kind;
            if(item.Kind=="buildings")kind=item.Template=="Path"?"path":"building";
            if(item.Kind=="other")kind="unknown_object";
            var cells=group.Where(t=>t.OverlapCells is not null).SelectMany(t=>t.OverlapCells!).Distinct().ToArray();
            entries.Add(JsonSerializer.SerializeToNode(new {item.Id,kind,item.Template,item.Position,item.Marked,
                finished=buildings.TryGetValue(item.Id,out var building)?(bool?)building.Finished:null,
                vegetation=item.Vegetation,cells,cellsKnown=group.All(t=>t.OverlapCells is not null)},NativeJson.Options));
        }
        return new JsonObject{["scope"]="observed_region_context_not_proven_rejection_cause",["items"]=entries,
            ["total"]=entries.Count,["truncated"]=false};
    }
    private BridgeEnvelope<T> Check<T>(BridgeEnvelope<T> result)
    {
        if(session is not null && (result.SessionId!=session || result.BridgeVersion!=version))
            throw new BridgeRejectionException("stale_session");
        session=result.SessionId;version=result.BridgeVersion;
        return result;
    }
    private async Task<BridgeEnvelope<T>> Read<T>(object key,Func<Task<BridgeEnvelope<T>>> read,CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if(observations.TryGetValue(key,out var value)) { CacheHits++;return (BridgeEnvelope<T>)value; }
        NativeReads++;var result=Check(await read());observations.Add(key,result);return result;
    }
    public async Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();NativeReads++;
        var result=Check(await inner.Simulation(ct));
        if(result.Data.CurrentSpeed!=0)throw new BridgeRejectionException("state_conflict");
        return result;
    }
    public Task<BridgeEnvelope<NativeMap>> Map(SurveyRect r,CancellationToken ct)=>Read(("map",r),()=>inner.Map(r,ct),ct);
    public Task<BridgeEnvelope<NativeObjects>> Objects(int offset,CancellationToken ct)=>Read(("objects",offset),()=>inner.Objects(offset,ct),ct);
    public Task<BridgeEnvelope<NativeAreas>> Areas(string kind,int offset,CancellationToken ct)=>Read(("areas",kind,offset),()=>inner.Areas(kind,offset,ct),ct);
    public Task<BridgeEnvelope<NativeRemovalTargets>> Targets(SurveyRect r,int offset,CancellationToken ct)=>Read(("targets",r,offset),()=>inner.Targets(r,offset,ct),ct);
    public Task<BridgeEnvelope<NativeRange>> WorkRange(SurveyRequest r,int offset,CancellationToken ct)=>Read(("range",r,offset),()=>inner.WorkRange(r,offset,ct),ct);
    public async Task<BridgeEnvelope<NativeBuildingPlan>> Plan(SurveyRequest r,SurveyRect area,int rotation,CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();NativeReads++;PlanningCalls++;
        return Check(await inner.Plan(r,area,rotation,ct));
    }
}
