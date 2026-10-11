using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

// Bounded greedy selection. Every accepted addition replaces the entire joint preview.
public static class RegionGroupPlanning
{
    public static (SurveyRequest Survey, string[] Templates, int Budget) Parse(JsonElement args)
    {
        if(args.ValueKind!=JsonValueKind.Object)throw new ArgumentException();
        var clean=new JsonObject();var seen=new HashSet<string>();string[]? templates=null;
        foreach(var p in args.EnumerateObject()) {
            if(!seen.Add(p.Name))throw new ArgumentException("duplicate_argument");
            if(p.Name=="template")throw new ArgumentException("template_and_templates_exclusive");
            if(p.Name=="templates") {
                if(p.Value.ValueKind!=JsonValueKind.Array)throw new ArgumentException("templates_1_to_8");
                var values=p.Value.EnumerateArray().ToArray();
                if(values.Length is <1 or >8 || values.Any(v=>v.ValueKind!=JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(v.GetString()) || v.GetString()!.Length>160))throw new ArgumentException("templates_1_to_8");
                templates=values.Select(v=>v.GetString()!).ToArray();
            } else clean[p.Name]=JsonNode.Parse(p.Value.GetRawText());
        }
        if(templates is null)throw new ArgumentException("templates_required");
        clean["template"]=templates[0];
        var (survey,budget)=RegionBuildPlanning.Parse(JsonSerializer.SerializeToElement(clean));
        return(survey with{MaxResults=32},templates,budget);
    }

    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct)
    {
        var (request,templates,budget)=Parse(args);
        var start=await client.Simulation(ct);
        if(start.SessionId!=request.Session)throw new BridgeRejectionException("stale_session");
        if(start.BridgeVersion!="0.50.0")throw new InvalidDataException("shared_group_planning_requires_050");
        if(start.Data.CurrentSpeed!=0)throw new BridgeRejectionException("state_conflict");
        var surveys=new Dictionary<string,RegionSurveyReport>(StringComparer.Ordinal);
        var choices=new Dictionary<string,SurveyCandidate[]>(StringComparer.Ordinal);
        var shared=new SharedRegionSurveyPort(new NativeRegionSurveyPort(client));
        foreach(var template in templates.Distinct(StringComparer.Ordinal)) {
            var survey=await RegionSurvey.Observe(request with{Template=template},shared,ct);
            surveys.Add(template,survey);
            choices.Add(template,survey.BuildingCandidates.Concat(survey.AdditionalLevels.SelectMany(l=>l.BuildingCandidates)).ToArray());
        }
        var report=await Plan(request,templates,budget,choices,async (steps,token)=>{
            var q=new NameValueCollection{{"session",request.Session},{"districtId",request.DistrictId},
                {"steps",LargeProjectRequest.Encode(steps)}};
            return await client.LargeProject(LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-access",q),token);
        },ct);
        var end=await client.Simulation(ct);
        if(end.SessionId!=request.Session || end.BridgeVersion!=start.BridgeVersion || end.Data.CurrentSpeed!=0)
            throw new BridgeRejectionException("state_conflict");
        report["surveys"]=JsonSerializer.SerializeToNode(surveys.Select(p=>new {
            template=p.Key,p.Value.Region,p.Value.Coverage,
            additionalLevels=p.Value.AdditionalLevels.Select(l=>new{l.Region,l.Coverage}),
            observationRequests=p.Value.NativeReads,p.Value.PlanningCalls,p.Value.ObservationStartedAtUtc,p.Value.ObservationEndedAtUtc
        }),NativeJson.Options);
        report["boundaryReads"]=2;report["atomic"]=false;
        report["occupations"]=shared.Occupations();
        report["readMetrics"]=JsonSerializer.SerializeToNode(new {nativeReads=shared.NativeReads+2,shared.CacheHits,shared.PlanningCalls,
            nativePreviewCalls=report["nativeValidationCalls"]!.GetValue<int>(),scope="one_request_not_atomic"},NativeJson.Options);
        return JsonSerializer.SerializeToNode(new NativeResult<JsonObject>(1,"ok",report,
            new("native",false,request.Session,end.ObservedAtUtc),null),NativeJson.Options)!.AsObject();
    }

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
                var blockers=selectedGeometry.Select(g=>new{g.InputIndex,g.Template,g.Candidate,cells=ConflictingCells(c,g.Candidate)})
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
                if(e.BridgeVersion is not ("0.49.0" or "0.50.0"))throw new InvalidDataException("group_planning_requires_049");
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

    public static Position[] ConflictingCells(SurveyCandidate candidate,SurveyCandidate selected)
    {
        var conflicts=new HashSet<Position>();
        if(candidate.SolidCells is {} a && selected.SolidCells is {} b)conflicts.UnionWith(a.Intersect(b));
        foreach(var cell in candidate.CellOccupations ?? []) {
            if(cell.Mask is not int mask)continue;
            var profile=candidate.OccupationProfiles?.SingleOrDefault(p=>p.Mask==mask && p.Known);
            if(profile is null)continue;
            if((selected.CellOccupations ?? []).Any(other=>other.Position==cell.Position && other.Mask is int otherMask &&
                profile.ConflictsWith.Contains(otherMask)))conflicts.Add(cell.Position);
        }
        return conflicts.ToArray();
    }

    public static void Compact(JsonObject data)
    {
        // The one executable plan stays intact; earlier previews are not executable alternatives.
        if(data["attempts"] is JsonArray attempts)
        {
            var skipped=new JsonArray();
            foreach(var group in attempts.OfType<JsonObject>().Where(a=>a["state"]?.GetValue<string>()=="filtered")
                .GroupBy(a=>a["inputIndex"]!.GetValue<int>()))
                skipped.Add(new JsonObject{["inputIndex"]=group.Key,["count"]=group.Count(),["reason"]="overlap_selected_target_occupation",
                    ["occupations"]=new JsonArray(group.SelectMany(a=>a["occupations"]!.AsArray()).OfType<JsonObject>()
                        .GroupBy(o=>o["inputIndex"]!.GetValue<int>()).Select(g=>{
                            var first=g.First();return (JsonNode)new JsonObject{["kind"]="planned_building",["occupation"]="native_intersection_profile",
                                ["inputIndex"]=g.Key,["template"]=first["template"]!.DeepClone(),
                                ["candidateLayers"]=JsonSerializer.SerializeToNode(g.SelectMany(o=>o["candidateLayers"]!.AsArray()).Select(v=>v!.GetValue<string>()).Distinct()),
                                ["blockingLayers"]=JsonSerializer.SerializeToNode(g.SelectMany(o=>o["blockingLayers"]!.AsArray()).Select(v=>v!.GetValue<string>()).Distinct())};}).ToArray())});
            data["filterSummary"]=skipped;
            for(int i=attempts.Count-1;i>=0;i--) {
                if(attempts[i] is not JsonObject item)continue;
                item.Remove("evidence");
                if(item["state"]?.GetValue<string>() is "filtered" or "native_preview_passed")attempts.RemoveAt(i);
            }
        }
        if(data["jointPlan"] is JsonObject joint) {
            ProjectReportPresentation.Compact("plan_large_project_access",joint);
            joint.Remove("steps");joint.Remove("cleanupCandidateIndices");
            (joint["assessment"] as JsonObject)?.Remove("parts");
        }
        if(data["candidates"] is JsonArray candidates)
            foreach(var candidate in candidates.OfType<JsonObject>()) {
                var wrapper=new JsonObject{["plan"]=candidate.DeepClone()};
                ProjectReportPresentation.Compact("plan_large_project_access",wrapper);
                var plan=wrapper["plan"]!.AsObject();
                foreach(string key in new[]{"parts","roadProtection","summary"})
                    if(plan[key] is JsonNode value)candidate[key]=value.DeepClone();
            }
        // Retain coverage and unknown cells, but omit geometric rejection samples and raster rows.
        TrimCoverage(data);
        if(data["occupations"] is JsonObject occupations && occupations["items"] is JsonArray items) {
            occupations["byKind"]=JsonSerializer.SerializeToNode(items.OfType<JsonObject>()
                .GroupBy(i=>i["kind"]!.GetValue<string>()).ToDictionary(g=>g.Key,g=>g.Count()));
            occupations["truncated"]=items.Count>8;
            while(items.Count>8)items.RemoveAt(items.Count-1);
            foreach(var item in items.OfType<JsonObject>()) {
                item["cellCount"]=item["cells"]!.AsArray().Count;item.Remove("cells");
                if(item["vegetation"] is JsonObject vegetation) {
                    var brief=new JsonObject();foreach(string key in new[]{"lifeState","isTree","cutYieldRemoved","cutIsYielding"})brief[key]=vegetation[key]?.DeepClone();
                    item["vegetation"]=brief;
                }
            }
        }
        TrimNestedLimits(data,true);
        data["detailAvailable"]="full";
    }
    private static void TrimNestedLimits(JsonNode node,bool root=false)
    {
        if(node is JsonObject obj) {
            if(!root)obj.Remove("limitations");
            // Summary status, reasons, losses and restoration remain next to these verbose details.
            if(obj["constructionAccessPreview"] is JsonObject access)access.Remove("details");
            foreach(var p in obj.ToArray())if(p.Value is not null)TrimNestedLimits(p.Value);
        } else if(node is JsonArray array)foreach(var item in array)if(item is not null)TrimNestedLimits(item);
    }
    private static void TrimCoverage(JsonNode node)
    {
        if(node is JsonObject obj) {
            obj.Remove("rejectedSites");obj.Remove("planningRows");
            foreach(var p in obj.ToArray())if(p.Value is not null)TrimCoverage(p.Value);
        } else if(node is JsonArray array)foreach(var item in array)if(item is not null)TrimCoverage(item);
    }
}
