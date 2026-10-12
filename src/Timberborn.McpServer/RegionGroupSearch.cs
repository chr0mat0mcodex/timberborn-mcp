using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

// Deterministic bounded depth-first alternatives; only native-confirmed prefixes become results.
public static class RegionGroupSearch
{
    public const int MaxStates=256;
    private sealed record Pick(int InputIndex,string Template,SurveyCandidate Candidate);
    public static async Task<JsonObject> Plan(SurveyRequest request,string[] templates,int maxCandidates,int searchBudget,
        ReservedArea[] reserved,IReadOnlyDictionary<string,SurveyCandidate[]> choices,
        Func<LargeProjectStep[],CancellationToken,Task<BridgeEnvelope<JsonElement>>> check,CancellationToken ct)
    {
        if(templates.Length is <1 or >8 || maxCandidates is <1 or >4 || searchBudget is <1 or >32)throw new ArgumentException();
        ReservedArea.Validate(reserved);
        var options=templates.Select(t=>choices[t].OrderBy(c=>c.NewPaths).ThenBy(c=>Math.Abs(c.Origin.Z-request.Z))
            .ThenBy(c=>c.Origin.Z).ThenBy(c=>c.Origin.Y).ThenBy(c=>c.Origin.X).ThenBy(c=>c.Rotation)
            .DistinctBy(c=>(c.Origin,c.Rotation)).ToArray()).ToArray();
        var attempts=new JsonArray();var picked=new List<Pick>();Pick[] best=[];JsonObject? bestPlan=null;
        int calls=0,states=0,filtered=0,cacheHits=0,backtracks=0,bestPaths=int.MaxValue,bestHelpers=int.MaxValue;
        bool stateLimit=false,nativeLimit=false,candidateLimit=false;
        var cache=new Dictionary<string,JsonObject>(StringComparer.Ordinal);
        var reasons=Enumerable.Range(0,templates.Length).Select(_=>new HashSet<string>(StringComparer.Ordinal)).ToArray();
        var tried=new int[templates.Length];var skipped=new int[templates.Length];
        LargeProjectStep[] Steps()=>picked.Select(p=>new LargeProjectStep{Template=p.Template,
            X=p.Candidate.Origin.X,Y=p.Candidate.Origin.Y,Z=p.Candidate.Origin.Z,Rotation=p.Candidate.Rotation}).ToArray();
        void Save(JsonObject plan)
        {
            var parts=plan["steps"]!.AsArray();int paths=parts.Count(p=>p?["template"]?.GetValue<string>()=="Path");
            int helpers=parts.Count-picked.Count;
            if(picked.Count>best.Length || picked.Count==best.Length && (paths<bestPaths || paths==bestPaths && helpers<bestHelpers)) {
                best=picked.ToArray();bestPlan=(JsonObject)plan.DeepClone();bestPaths=paths;bestHelpers=helpers;
            }
        }
        async Task Visit(int index)
        {
            ct.ThrowIfCancellationRequested();
            if(states>=MaxStates){stateLimit=true;return;}states++;
            if(index==templates.Length || picked.Count+templates.Length-index<best.Length)return;
            int considered=0;
            foreach(var c in options[index]) {
                ct.ThrowIfCancellationRequested();
                if(states>=MaxStates){stateLimit=true;break;}states++;
                // Symmetric duplicates are not new alternatives. Other templates still need native geometry checks.
                if(picked.Any(p=>p.Template==templates[index] && p.Candidate.Origin==c.Origin && p.Candidate.Rotation==c.Rotation))continue;
                string? filter=null;
                var blockers=picked.Where(p=>RegionGroupPlanning.ConflictingCells(c,p.Candidate).Length>0).ToArray();
                if(blockers.Length>0)filter="overlap_selected_target_occupation";
                if((c.OccupiedCells ?? []).Any(p=>reserved.Any(a=>a.Contains(p.X,p.Y,p.Z))))filter="reserved_area";
                if(filter is not null) {
                    filtered++;skipped[index]++;reasons[index].Add(filter);
                    attempts.Add(new JsonObject{["inputIndex"]=index,["state"]="filtered",["reason"]=filter,
                        ["origin"]=JsonSerializer.SerializeToNode(c.Origin,NativeJson.Options),["rotation"]=c.Rotation,
                        ["blockers"]=JsonSerializer.SerializeToNode(blockers.Select(b=>new{kind="planned_building",b.InputIndex,b.Template,
                            candidateLayers=(c.CellOccupations??[]).Where(v=>RegionGroupPlanning.ConflictingCells(c,b.Candidate).Contains(v.Position)).Select(v=>v.Occupation).Distinct(),
                            blockingLayers=(b.Candidate.CellOccupations??[]).Where(v=>RegionGroupPlanning.ConflictingCells(c,b.Candidate).Contains(v.Position)).Select(v=>v.Occupation).Distinct()}),NativeJson.Options),
                        ["reservedAreaIndices"]=JsonSerializer.SerializeToNode(Enumerable.Range(0,reserved.Length)
                            .Where(i=>(c.OccupiedCells??[]).Any(p=>reserved[i].Contains(p.X,p.Y,p.Z))))});
                    continue;
                }
                if(considered>=maxCandidates){candidateLimit=true;break;}considered++;
                picked.Add(new(index,templates[index],c));var steps=Steps();string key;
                try {key=LargeProjectRequest.Encode(steps);}
                catch(ArgumentException ex) {
                    reasons[index].Add(ex.Message);attempts.Add(new JsonObject{["inputIndex"]=index,["state"]="rejected",["reason"]=ex.Message,["phase"]="request_limits"});
                    picked.RemoveAt(picked.Count-1);continue;
                }
                JsonObject data;
                if(cache.TryGetValue(key,out var cached)){data=cached;cacheHits++;}
                else {
                    if(calls>=searchBudget){nativeLimit=true;picked.RemoveAt(picked.Count-1);break;}
                    calls++;tried[index]++;
                    var e=await check(steps,ct);
                    if(e.SessionId!=request.Session)throw new BridgeRejectionException("stale_session");
                    if(e.BridgeVersion!="0.51.0")throw new InvalidDataException("group_search_requires_051");
                    data=JsonNode.Parse(e.Data.GetRawText())!.AsObject();
                    bool ok=data["candidateFound"]!.GetValue<bool>();
                    if(ok && (data["plan"]?["pilotEligible"]?.GetValue<bool>()!=true || data["plan"]?["restored"]?.GetValue<bool>()!=true ||
                        data["steps"] is not JsonArray || data["plan"]?["planKey"] is null))throw new InvalidDataException("regional_preview_incomplete");
                    if(reserved.Length>0 && (data["reservedAreasKey"]?.GetValue<string>()!=ReservedArea.Encode(reserved) ||
                        ok && data["reservedAreasClear"]?.GetValue<bool>()!=true))throw new InvalidDataException("reserved_area_evidence_missing");
                    cache.Add(key,data);
                    attempts.Add(new JsonObject{["inputIndex"]=index,["state"]=ok?"native_preview_passed":"rejected",
                        ["reason"]=data["reason"]?.DeepClone(),["phase"]=data["phase"]?.DeepClone(),["blockedPart"]=data["blockedPart"]?.DeepClone(),["origin"]=JsonSerializer.SerializeToNode(c.Origin,NativeJson.Options),
                        ["rotation"]=c.Rotation,["selectedInputIndices"]=JsonSerializer.SerializeToNode(picked.Select(p=>p.InputIndex)),
                        ["evidence"]=data.DeepClone(),["observedAtUtc"]=JsonValue.Create(e.ObservedAtUtc)});
                }
                if(data["candidateFound"]!.GetValue<bool>()) {Save(data);await Visit(index+1);}
                else reasons[index].Add(data["reason"]!.GetValue<string>());
                picked.RemoveAt(picked.Count-1);backtracks++;
                if(nativeLimit || stateLimit)break;
            }
            // Skipping an unplaceable/expensive early target allows later requested buildings to compete.
            if(!nativeLimit && !stateLimit)await Visit(index+1);
        }
        await Visit(0);
        var selected=new JsonArray(best.Select((p,i)=>(JsonNode)new JsonObject{
            ["inputIndex"]=p.InputIndex,["template"]=p.Template,["stepIndex"]=i}).ToArray());
        var unplaced=new JsonArray(Enumerable.Range(0,templates.Length).Where(i=>best.All(p=>p.InputIndex!=i)).Select(i=>(JsonNode)new JsonObject{
            ["inputIndex"]=i,["template"]=templates[i],["reason"]=options[i].Length==0?"no_search_candidate":"not_in_best_confirmed_plan",
            ["observedReasons"]=JsonSerializer.SerializeToNode(reasons[i].Order()),["nativeAttempts"]=tried[i],["filteredAttempts"]=skipped[i],
            ["candidateCount"]=options[i].Length}).ToArray());
        return new JsonObject{["session"]=request.Session,["districtId"]=request.DistrictId,["mode"]="group",
            ["state"]=best.Length==templates.Length?"complete":best.Length==0?"none":"partial",
            ["requested"]=templates.Length,["planned"]=best.Length,["selected"]=selected,["unplaced"]=unplaced,
            ["nativeValidationCalls"]=calls,["filteredCandidates"]=filtered,["attempts"]=attempts,["jointPlan"]=bestPlan,
            ["steps"]=bestPlan?["steps"]?.DeepClone(),["planKey"]=bestPlan?["plan"]?["planKey"]?.DeepClone(),
            ["cleanupCandidateIndices"]=bestPlan?["cleanupCandidateIndices"]?.DeepClone(),["regularExecutionAllowed"]=false,
            ["reservedAreas"]=JsonSerializer.SerializeToNode(reserved,NativeJson.Options),
            ["search"]=JsonSerializer.SerializeToNode(new {strategy="bounded_backtracking",nativeBudget=searchBudget,nativeCalls=calls,
                visitedStates=states,maxStates=MaxStates,cacheHits,backtracks,candidateLimitReached=candidateLimit,
                stopReason=nativeLimit?"native_budget":stateLimit?"state_budget":candidateLimit?"candidate_budget":"candidates_exhausted",
                globalOptimalityProven=false},NativeJson.Options),
            ["score"]=bestPlan is null?null:JsonSerializer.SerializeToNode(new{placed=best.Length,newPaths=bestPaths,helpers=bestHelpers},NativeJson.Options),
            ["limitations"]=new JsonArray("best_confirmed_plan_within_bounded_search","search_not_complete_map_coverage",
                "reservations_for_returned_plan_only_not_persistent","existing_roads_may_cross_reserved_areas",
                "partial_plan_excludes_unplaced_buildings","native_preview_not_actual_builder_or_production_proof",
                "native_validation_errors_abort_without_retry","start_requires_fresh_native_revalidation")};
    }
    public static void Compact(JsonObject data)
    {
        if(data["attempts"] is JsonArray attempts) {
            data["searchSummary"]=JsonSerializer.SerializeToNode(attempts.OfType<JsonObject>()
                .GroupBy(a=>new{index=a["inputIndex"]?.GetValue<int>(),state=a["state"]?.GetValue<string>(),reason=a["reason"]?.GetValue<string>(),phase=a["phase"]?.GetValue<string>(),blockedPart=a["blockedPart"]?.GetValue<int>()})
                .Select(g=>new{inputIndex=g.Key.index,g.Key.state,g.Key.reason,g.Key.phase,g.Key.blockedPart,count=g.Count(),
                    blockers=g.SelectMany(a=>(a["blockers"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                        .DistinctBy(b=>b.ToJsonString()).Select(b=>b.DeepClone()).ToArray(),
                    reservedAreaIndices=g.SelectMany(a=>(a["reservedAreaIndices"] as JsonArray ?? new JsonArray()))
                        .Select(n=>n!.GetValue<int>()).Distinct().Order().ToArray()}),NativeJson.Options);
            data["attempts"]=new JsonArray();
        }
    }
}
