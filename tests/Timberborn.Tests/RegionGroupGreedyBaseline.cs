using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
namespace Timberborn.Tests;

// Frozen 0.50 policy for comparison tests, not part of the production MCP.
internal static class RegionGroupGreedyBaseline
{
    // Retained 0.50 greedy baseline for regression comparisons; the live entry point uses RegionGroupSearch.
    public static async Task<JsonObject> Plan(SurveyRequest request,string[] templates,int budget,
        IReadOnlyDictionary<string,SurveyCandidate[]> choices,
        Func<LargeProjectStep[],CancellationToken,Task<BridgeEnvelope<JsonElement>>> check,CancellationToken ct)
    {
        if(templates.Length is <1 or >8 || budget is <1 or >4)throw new ArgumentException();
        var targets=new List<LargeProjectStep>();var selected=new JsonArray();var unplaced=new JsonArray();
        var attempts=new JsonArray();JsonObject? final=null;int calls=0,filtered=0;
        var selectedGeometry=new List<(int InputIndex,string Template,SurveyCandidate Candidate)>();
        for(int index=0;index<templates.Length;index++) {
            ct.ThrowIfCancellationRequested();
            string template=templates[index];
            // Exact target duplicates cannot succeed; do not spend native budget on them.
            var candidates=choices[template].OrderBy(c=>c.NewPaths).ThenBy(c=>Math.Abs(c.Origin.Z-request.Z))
                .ThenBy(c=>c.Origin.Z).ThenBy(c=>c.Origin.Y).ThenBy(c=>c.Origin.X).ThenBy(c=>c.Rotation)
                .DistinctBy(c=>(c.Origin,c.Rotation)).Where(c=>!targets.Any(t=>t.Template==template &&
                    t.X==c.Origin.X && t.Y==c.Origin.Y && t.Z==c.Origin.Z && t.Rotation==c.Rotation)).ToArray();
            bool accepted=false;int tried=0,skipped=0;string reason="no_search_candidate";
            foreach(var c in candidates) {
                ct.ThrowIfCancellationRequested();
                var blockers=selectedGeometry.Select(g=>new{g.InputIndex,g.Template,g.Candidate,cells=RegionGroupPlanning.ConflictingCells(c,g.Candidate)})
                    .Where(g=>g.cells.Length>0).ToArray();
                if(blockers.Length>0) {
                    skipped++;filtered++;reason="overlap_selected_target_occupation";
                    attempts.Add(new JsonObject{["inputIndex"]=index,["origin"]=JsonSerializer.SerializeToNode(c.Origin,NativeJson.Options),
                        ["rotation"]=c.Rotation,["state"]="filtered",["reason"]=reason,["phase"]="native_occupation_overlap",
                        ["occupations"]=JsonSerializer.SerializeToNode(blockers.Select(g=>new{kind="planned_building",
                            occupation="native_intersection_profile",inputIndex=g.InputIndex,template=g.Template,cells=g.cells,
                            candidateLayers=(c.CellOccupations ?? []).Where(v=>g.cells.Contains(v.Position)).Select(v=>v.Occupation).Distinct().ToArray(),
                            blockingLayers=(g.Candidate.CellOccupations ?? []).Where(v=>g.cells.Contains(v.Position)).Select(v=>v.Occupation).Distinct().ToArray()}),NativeJson.Options)});
                    continue;
                }
                if(tried>=budget)break;
                tried++;
                var target=new LargeProjectStep{Template=template,X=c.Origin.X,Y=c.Origin.Y,Z=c.Origin.Z,Rotation=c.Rotation};
                var proposed=targets.Append(target).ToArray();
                try { LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-access",new NameValueCollection{
                    {"session",request.Session},{"districtId",request.DistrictId},{"steps",LargeProjectRequest.Encode(proposed)}}); }
                catch(ArgumentException ex) {
                    reason=ex.Message;
                    attempts.Add(new JsonObject{["inputIndex"]=index,["origin"]=JsonSerializer.SerializeToNode(c.Origin,NativeJson.Options),
                        ["rotation"]=c.Rotation,["state"]="rejected",["reason"]=reason,["phase"]="request_limits"});
                    continue;
                }
                calls++;
                var e=await check(proposed,ct);
                if(e.SessionId!=request.Session)throw new BridgeRejectionException("stale_session");
                if(e.BridgeVersion is not ("0.49.0" or "0.50.0" or "0.51.0"))throw new InvalidDataException("group_planning_requires_049");
                var data=JsonNode.Parse(e.Data.GetRawText())!.AsObject();
                bool ok=data["candidateFound"]!.GetValue<bool>();reason=data["reason"]!.GetValue<string>();
                attempts.Add(new JsonObject{["inputIndex"]=index,["origin"]=JsonSerializer.SerializeToNode(c.Origin,NativeJson.Options),
                    ["rotation"]=c.Rotation,["state"]=ok?"native_preview_passed":"rejected",["reason"]=reason,
                    ["phase"]=data["phase"]?.DeepClone(),["blockedPart"]=data["blockedPart"]?.DeepClone(),
                    ["observedAtUtc"]=JsonValue.Create(e.ObservedAtUtc),["evidence"]=data.DeepClone()});
                if(!ok)continue;
                if(data["plan"]?["pilotEligible"]?.GetValue<bool>()!=true || data["plan"]?["restored"]?.GetValue<bool>()!=true ||
                    data["steps"] is not JsonArray || data["plan"]?["planKey"] is null)
                    throw new InvalidDataException("regional_preview_incomplete");
                selected.Add(new JsonObject{["inputIndex"]=index,["template"]=template,["stepIndex"]=targets.Count});
                targets.Add(target);selectedGeometry.Add((index,template,c));
                final=data;accepted=true;break;
            }
            if(!accepted)unplaced.Add(new JsonObject{["inputIndex"]=index,["template"]=template,["reason"]=reason,
                ["attempted"]=tried,["filtered"]=skipped,["unchecked"]=candidates.Length-tried-skipped,["budgetExhausted"]=candidates.Length>tried+skipped});
        }
        return new JsonObject{["session"]=request.Session,["districtId"]=request.DistrictId,["mode"]="group",
            ["state"]=targets.Count==templates.Length?"complete":targets.Count==0?"none":"partial",
            ["requested"]=templates.Length,["planned"]=targets.Count,["selected"]=selected,["unplaced"]=unplaced,
            ["nativeValidationCalls"]=calls,["filteredCandidates"]=filtered,["attempts"]=attempts,["jointPlan"]=final,
            ["steps"]=final?["steps"]?.DeepClone(),["planKey"]=final?["plan"]?["planKey"]?.DeepClone(),
            ["cleanupCandidateIndices"]=final?["cleanupCandidateIndices"]?.DeepClone(),["regularExecutionAllowed"]=false,
            ["limitations"]=new JsonArray("greedy_input_order_no_backtracking","search_not_complete_map_coverage",
                "partial_plan_excludes_unplaced_buildings","native_preview_not_actual_builder_or_production_proof",
                "no_candidate_does_not_mean_unbuildable","native_validation_errors_abort_without_retry",
                "start_requires_fresh_native_revalidation")};
    }

}
