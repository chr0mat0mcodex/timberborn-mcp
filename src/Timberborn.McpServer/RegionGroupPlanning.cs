using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

// Shared observation and presentation for bounded group planning.
public static class RegionGroupPlanning
{
    public static (SurveyRequest Survey, string[] Templates, int Budget, int SearchBudget, ReservedArea[] Reserved) Parse(JsonElement args)
    {
        if(args.ValueKind!=JsonValueKind.Object)throw new ArgumentException();
        var clean=new JsonObject();var seen=new HashSet<string>();string[]? templates=null;int searchBudget=32;ReservedArea[] reserved=[];
        foreach(var p in args.EnumerateObject()) {
            if(!seen.Add(p.Name))throw new ArgumentException("duplicate_argument");
            if(p.Name=="template")throw new ArgumentException("template_and_templates_exclusive");
            if(p.Name=="searchBudget") {
                if(p.Value.ValueKind!=JsonValueKind.Number || !p.Value.TryGetInt32(out searchBudget) || searchBudget is <1 or >32)throw new ArgumentException("search_budget_1_to_32");
            } else if(p.Name=="reservedAreas") {
                if(p.Value.ValueKind!=JsonValueKind.Array || p.Value.GetArrayLength()>8)throw new ArgumentException("invalid_reserved_areas");
                foreach(var area in p.Value.EnumerateArray()) {
                    string[] keys=["x","y","z","width","height","depth"];
                    if(area.ValueKind!=JsonValueKind.Object || area.EnumerateObject().Count()!=6 ||
                        keys.Any(k=>!area.TryGetProperty(k,out var v) || v.ValueKind!=JsonValueKind.Number || !v.TryGetInt32(out _)))
                        throw new ArgumentException("invalid_reserved_areas");
                }
                reserved=BuildBatchState.Parse<ReservedArea[]>(p.Value);ReservedArea.Validate(reserved);
            } else if(p.Name=="templates") {
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
        return(survey with{MaxResults=32},templates,budget,searchBudget,reserved);
    }

    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct,IRegionSurveyPort? surveyPort=null)
    {
        var (request,templates,budget,searchBudget,reserved)=Parse(args);
        var start=await client.Simulation(ct);
        if(start.SessionId!=request.Session)throw new BridgeRejectionException("stale_session");
        if(start.BridgeVersion is not ("0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0"))throw new InvalidDataException("group_search_requires_051");
        if(start.Data.CurrentSpeed!=0)throw new BridgeRejectionException("state_conflict");
        var surveys=new Dictionary<string,RegionSurveyReport>(StringComparer.Ordinal);
        var choices=new Dictionary<string,SurveyCandidate[]>(StringComparer.Ordinal);
        var shared=new SharedRegionSurveyPort(surveyPort ?? new NativeRegionSurveyPort(client));
        foreach(var template in templates.Distinct(StringComparer.Ordinal)) {
            var survey=await RegionSurvey.Observe(request with{Template=template},shared,ct);
            surveys.Add(template,survey);
            choices.Add(template,survey.BuildingCandidates.Concat(survey.AdditionalLevels.SelectMany(l=>l.BuildingCandidates)).ToArray());
        }
        var report=await RegionGroupSearch.Plan(request,templates,budget,searchBudget,reserved,choices,async (steps,token)=>{
            var q=new NameValueCollection{{"session",request.Session},{"districtId",request.DistrictId},
                {"steps",LargeProjectRequest.Encode(steps)}};
            if(reserved.Length>0)q.Add("reservedAreas",ReservedArea.Encode(reserved));
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
        if(data["search"] is not null)RegionGroupSearch.Compact(data);
        // The one executable plan stays intact; earlier previews are not executable alternatives.
        if(data["attempts"] is JsonArray attempts && data["search"] is null)
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
