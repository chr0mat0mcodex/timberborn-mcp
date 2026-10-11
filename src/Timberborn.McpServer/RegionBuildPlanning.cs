using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

// Search remains observational; native previews require the existing building gate.
public static class RegionBuildPlanning
{
    public const string Name = "plan_region_buildings";
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        if (!enabled) yield break;
        var schema = JsonNode.Parse(RegionSurveyTools.Reader().InputSchema.GetRawText())!.AsObject();
        schema["properties"]!["maxCandidates"] = new JsonObject {
            ["type"]="integer", ["minimum"]=1, ["maximum"]=4, ["default"]=3 };
        schema["properties"]!["templates"] = new JsonObject { ["type"]="array",["minItems"]=1,["maxItems"]=8,
            ["items"]=new JsonObject{["type"]="string",["minLength"]=1,["maxLength"]=160},
            ["description"]="0.49: gemeinsame Gebäudegruppe in dieser Reihenfolge; Wiederholungen erlaubt. Exklusiv zu template." };
        var required=schema["required"]!.AsArray();
        for(int i=required.Count-1;i>=0;i--)if(required[i]?.GetValue<string>()=="template")required.RemoveAt(i);
        schema["oneOf"]=new JsonArray(
            new JsonObject{["required"]=new JsonArray("template"),["not"]=new JsonObject{["required"]=new JsonArray("templates")}},
            new JsonObject{["required"]=new JsonArray("templates"),["not"]=new JsonObject{["required"]=new JsonArray("template")}});
        yield return new Tool { Name=Name, InputSchema=JsonSerializer.SerializeToElement(schema),
            OutputSchema=JsonSerializer.SerializeToElement(new {type="object"}),
            Description="0.49: Alternativ templates mit 1–8 Gebäuden als gemeinsamem Plan; maxCandidates gilt dann je Gebäude (insgesamt höchstens 32 Vorschauen). Gemeinsame Wege, greedy in Eingabereihenfolge ohne globale Optimierung. complete/partial/none und unplaced lesen; Teilplan enthält nur die ausgewiesenen Gebäude. Compact standardmäßig, detail=full mit allen Belegen. Mit template weiterhin Einzelalternativen. 0.48: Region wie survey_region durchsuchen, anschließend höchstens maxCandidates (1–4, Standard 3) eindeutige Kandidaten gemeinsam mit automatischen Zugangshilfen nativ prüfen. Nur pausiert und mit Bauschalter; temporäre native Vorschauen, kein Bauauftrag. Erst native_preview_passed liefert steps plus planKey für run_large_project; rejected/unchecked sind keine Freigabe. Gründe, Teilberichte, Höhen-/Suchabdeckung und Prüfanzahl bleiben sichtbar. Ein Kandidat ist eine Alternative: nie mehrere alternativ geprüfte Pläne ungeprüft zusammenbauen. Bestehende Grenzen 32 Teile/32×32/acht Höhen; keine Rodung, keine optimale oder vollständige Standortsuche. WorkBuildingId und planBuildings=false hier unzulässig. Start validiert den unveränderten Plan erneut; kein Auftrags-Replay.",
            Annotations=new(){ReadOnlyHint=false,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false} };
    }
    public static (SurveyRequest Survey,int MaxCandidates) Parse(JsonElement args)
    {
        if(args.ValueKind!=JsonValueKind.Object)throw new ArgumentException();
        var clean=new JsonObject();int max=3;var seen=new HashSet<string>();
        foreach(var p in args.EnumerateObject()) {
            if(!seen.Add(p.Name))throw new ArgumentException("duplicate_argument");
            if(p.Name=="maxCandidates") {
                if(p.Value.ValueKind!=JsonValueKind.Number || !p.Value.TryGetInt32(out max) || max is <1 or >4)
                    throw new ArgumentException("max_candidates_1_to_4");
            } else clean[p.Name]=JsonNode.Parse(p.Value.GetRawText());
        }
        var r=BuildBatchState.Parse<SurveyRequest>(JsonSerializer.SerializeToElement(clean));
        RegionSurvey.Validate(r);
        if(!r.PlanBuildings || r.WorkBuildingId is not null)throw new ArgumentException("building_search_required");
        return(r,max);
    }
    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,bool enabled,CancellationToken ct)
    {
        if(!enabled)throw new ArgumentException("building_actions_disabled");
        if(args.TryGetProperty("templates",out _))return await RegionGroupPlanning.Invoke(client,args,ct);
        var (request,max)=Parse(args);
        var initial=await client.Simulation(ct);
        if(initial.SessionId!=request.Session)throw new BridgeRejectionException("stale_session");
        if(initial.BridgeVersion is not ("0.48.0" or "0.49.0"))throw new InvalidDataException("regional_planning_requires_048");
        if(initial.Data.CurrentSpeed!=0)throw new BridgeRejectionException("state_conflict");
        var survey=await RegionSurvey.Observe(request,new NativeRegionSurveyPort(client),ct);
        var candidates=survey.BuildingCandidates.Concat(survey.AdditionalLevels.SelectMany(l=>l.BuildingCandidates));
        var checkedReport=await CheckCandidates(request,candidates,max,async (step,token)=> {
            var q=new NameValueCollection{{"session",request.Session},{"districtId",request.DistrictId},
                {"steps",LargeProjectRequest.Encode([step])}};
            return await client.LargeProject(LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-access",q),token);
        },ct);
        var end=await client.Simulation(ct);
        if(end.SessionId!=request.Session || end.BridgeVersion!=initial.BridgeVersion || end.Data.CurrentSpeed!=0)throw new BridgeRejectionException("state_conflict");
        checkedReport["survey"] = JsonSerializer.SerializeToNode(new {
            survey.Region,survey.Coverage, additionalLevels=survey.AdditionalLevels.Select(l=>new{l.Region,l.Coverage}),
            survey.NativeReads,survey.PlanningCalls,survey.ObservationStartedAtUtc,survey.ObservationEndedAtUtc,
            boundaryReads=2, atomic=false },NativeJson.Options);
        return JsonSerializer.SerializeToNode(new NativeResult<JsonObject>(1,"ok",checkedReport,
            new("native",false,request.Session,end.ObservedAtUtc),null),NativeJson.Options)!.AsObject();
    }
    public static async Task<JsonObject> CheckCandidates(SurveyRequest request,IEnumerable<SurveyCandidate> source,
        int max,Func<LargeProjectStep,CancellationToken,Task<BridgeEnvelope<JsonElement>>> check,CancellationToken ct)
    {
        if(max is <1 or >4)throw new ArgumentException();
        var choices=source.OrderBy(c=>c.NewPaths).ThenBy(c=>Math.Abs(c.Origin.Z-request.Z))
            .ThenBy(c=>c.Origin.Z).ThenBy(c=>c.Origin.Y).ThenBy(c=>c.Origin.X).ThenBy(c=>c.Rotation)
            .DistinctBy(c=>(c.Origin,c.Rotation)).ToArray();
        var results=new JsonArray();int attempts=0,passed=0;
        foreach(var c in choices) {
            ct.ThrowIfCancellationRequested();
            var item=new JsonObject { ["origin"]=JsonSerializer.SerializeToNode(c.Origin,NativeJson.Options),
                ["rotation"]=c.Rotation,["state"]="unchecked",["reason"]="native_candidate_budget" };
            if(attempts<max) {
                attempts++;
                var step=new LargeProjectStep {Template=request.Template,X=c.Origin.X,Y=c.Origin.Y,Z=c.Origin.Z,Rotation=c.Rotation};
                var e=await check(step,ct); // Transport/session failures abort; never turn them into a rejected site.
                if(e.SessionId!=request.Session)throw new BridgeRejectionException("stale_session");
                if(e.BridgeVersion is not ("0.48.0" or "0.49.0"))throw new InvalidDataException("regional_planning_requires_048");
                var data=JsonNode.Parse(e.Data.GetRawText())!.AsObject();
                bool ok=data["candidateFound"]!.GetValue<bool>();
                item["state"]=ok?"native_preview_passed":"rejected";
                item["reason"]=data["reason"]?.DeepClone();
                item["observedAtUtc"]=JsonValue.Create(e.ObservedAtUtc);
                foreach(string key in new[]{"blockedPart","phase","workCellCount","assessment"}) item[key]=data[key]?.DeepClone();
                if(data["plan"] is JsonObject plan) {
                    item["parts"]=plan["parts"]?.DeepClone();
                    item["restored"]=plan["restored"]?.DeepClone();
                    item["roadProtection"]=plan["roadProtection"]?.DeepClone();
                }
                if(ok) {
                    if(data["plan"]?["pilotEligible"]?.GetValue<bool>()!=true || data["plan"]?["restored"]?.GetValue<bool>()!=true)
                        throw new InvalidDataException("regional_preview_incomplete");
                    item["steps"]=data["steps"]?.DeepClone();
                    item["planKey"]=data["plan"]!["planKey"]?.DeepClone();
                    item["cleanupCandidateIndices"]=data["cleanupCandidateIndices"]?.DeepClone();passed++;
                }
            }
            results.Add(item);
        }
        return new JsonObject { ["session"]=request.Session,["districtId"]=request.DistrictId,["template"]=request.Template,
            ["candidateCount"]=choices.Length,["nativeValidationCalls"]=attempts,["passed"]=passed,
            ["unchecked"]=choices.Length-attempts,["candidates"]=results,["regularExecutionAllowed"]=false,
            ["limitations"]=new JsonArray("alternatives_not_jointly_validated","search_not_complete_map_coverage",
                "native_preview_not_actual_builder_or_production_proof","no_candidate_does_not_mean_unbuildable",
                "native_validation_errors_abort_without_retry","start_requires_fresh_native_revalidation") };
    }
}
