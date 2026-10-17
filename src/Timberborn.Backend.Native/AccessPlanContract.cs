using System.Collections.Specialized;
using System.Text.Json;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed partial class NativeClient
{
    private static void ValidateAccessPlan(BridgeEnvelope<JsonElement> envelope,LargeProjectRequest request)
    {
        var d=envelope.Data; bool found=d.GetProperty("candidateFound").GetBoolean();
        if(request.ReservedAreas.Length>0 && (envelope.BridgeVersion is not ("0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0") ||
            d.GetProperty("reservedAreasKey").GetString()!=ReservedArea.Encode(request.ReservedAreas) ||
            found && !d.GetProperty("reservedAreasClear").GetBoolean()))throw new InvalidDataException("reserved_area_evidence_missing");
        string? reason=d.GetProperty("reason").GetString();
        if(reason is not {Length:>0 and <=160} || found!=(reason=="candidate"))throw new InvalidDataException();
        var steps=d.GetProperty("steps").Deserialize<LargeProjectStep[]>(NativeJson.Options)??throw new InvalidDataException();
        if(steps.Length<request.Steps.Length || steps.Length>32)throw new InvalidDataException();
        if(steps.Any(s=>s is null||!BuildingPolicy.ValidTemplate(s.Template)||s.X is <0 or >4095||s.Y is <0 or >4095||s.Z is <0 or >4095||s.Rotation is <0 or >3||s.DependsOn is null||s.DependsOn.Length>31||s.DependsOn.Any(i=>i<0||i>=steps.Length)))throw new InvalidDataException();
        if(steps.Skip(request.Steps.Length).Any(s=>s.Template is not ("Path" or "Stairs.Folktails" or "Platform.Folktails")))throw new InvalidDataException();
        for(int i=0;i<request.Steps.Length;i++)
        {
            var a=request.Steps[i];var b=steps[i];
            if(b is null||a.Template!=b.Template||a.X!=b.X||a.Y!=b.Y||a.Z!=b.Z||a.Rotation!=b.Rotation||b.DependsOn is null||a.DependsOn.Except(b.DependsOn).Any())throw new InvalidDataException();
        }
        var assessment=d.GetProperty("assessment");
        if(envelope.BridgeVersion is "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0") ValidateAccessSearchDiagnostics(d.GetProperty("searchDiagnostics"),assessment,envelope.BridgeVersion);
        if(assessment.GetProperty("requestedParts").GetInt32()!=request.Steps.Length ||
            assessment.GetProperty("generatedParts").GetInt32()!=steps.Length-request.Steps.Length ||
            assessment.GetProperty("visitedNodes").GetInt32() is <0 or >4096 ||
            assessment.GetProperty("regularExecutionAllowed").GetBoolean() ||
            assessment.GetProperty("constructionAccess").GetString()!="unproven_until_live_construction")throw new InvalidDataException();
        var cleanup=d.GetProperty("cleanupCandidateIndices").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
        if(cleanup.Length!=cleanup.Distinct().Count()||cleanup.Any(i=>i<request.Steps.Length||i>=steps.Length))throw new InvalidDataException();
        var plan=d.GetProperty("plan");
        if(d.TryGetProperty("blockedPart",out var blocked))
        {
            if(blocked.ValueKind!=JsonValueKind.Null && (found||blocked.GetInt32()<0||blocked.GetInt32()>=steps.Length))throw new InvalidDataException();
            if(d.GetProperty("phase").GetString() is not ("targets" or "target_geometry" or "finished_entrance" or "construction_work_cell" or "native_joint_preview"))throw new InvalidDataException();
            if(found && (!plan.TryGetProperty("accessPolicyVersion",out var version)||version.GetInt32()!=1))throw new InvalidDataException();
        }
        if(found || plan.ValueKind!=JsonValueKind.Null)
        {
            LargeProjectRequest generated;
            try { generated=LargeProjectRequest.Parse("/agent-api/v1/large-project-plan",new NameValueCollection{
                {"session",request.Session},{"districtId",request.DistrictId},{"steps",LargeProjectRequest.Encode(steps)}}); }
            catch(ArgumentException ex){throw new InvalidDataException("invalid_generated_plan",ex);}
            ValidateLargeProject(envelope with{Data=plan},generated);
            if(found!=plan.GetProperty("pilotEligible").GetBoolean())throw new InvalidDataException();
        }
    }

    private static void ValidateAccessSearchDiagnostics(JsonElement d,JsonElement assessment,string bridgeVersion)
    {
        bool landingPreview=bridgeVersion is "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0" or "0.74.0",roadGraph=bridgeVersion!="0.68.0";
        int Count(JsonElement value,string name,int maximum=65536)
        {
            int count=value.GetProperty(name).GetInt32();
            if(count<0||count>maximum)throw new InvalidDataException("invalid_access_search_count");
            return count;
        }
        void Position(JsonElement p)
        {
            // Rejected neighbours can be one cell outside map bounds.
            foreach(string axis in new[]{"x","y","z"})
                if(p.GetProperty(axis).GetInt32() is <-1 or >4096)throw new InvalidDataException("invalid_access_search_position");
        }
        if(d.GetProperty("version").GetInt32()!=(landingPreview?3:roadGraph?2:1)||d.GetProperty("strategy").GetString()!="a_star_geometry"||
            d.GetProperty("evidence").GetString()!=(landingPreview?"native_edges_or_registered_landing_not_build_permission":"native_existing_edges_and_geometric_candidates_not_build_permission"))
            throw new InvalidDataException("invalid_access_search_evidence");
        if(roadGraph&&d.GetProperty("connectionGraph").GetString()!=(landingPreview?"instant_road_or_registered_landing":"instant_road"))throw new InvalidDataException("invalid_access_connection_graph");
        if(landingPreview)
        {
            Count(d,"landingPreviewCount",8);
            if(d.GetProperty("landingPreviewLimit").GetInt32()!=8)throw new InvalidDataException("invalid_landing_preview_limit");
        }
        int observed=Count(d,"observedPathCount"),sources=Count(d,"existingSourceCount"),examined=Count(d,"examinedHeightSources");
        int exits=Count(d,"heightExitCount",524288),connected=Count(d,"nativeConnectedExits",524288);
        if(sources>observed||examined>sources||exits!=examined*8||connected>exits)throw new InvalidDataException("inconsistent_access_search_counts");
        var paths=d.GetProperty("sources"); var edges=d.GetProperty("heightExits");var routes=d.GetProperty("routes");
        if(paths.GetArrayLength()!=Math.Min(32,observed)||edges.GetArrayLength()!=Math.Min(64,exits)||routes.GetArrayLength()>64||
            d.GetProperty("sourcesTruncated").GetBoolean()!=(observed>32)||d.GetProperty("heightExitsTruncated").GetBoolean()!=(exits>64))
            throw new InvalidDataException("invalid_access_search_coverage");
        foreach(var path in paths.EnumerateArray())
        {
            Position(path.GetProperty("position"));
            if(!BuildingPolicy.ValidTemplate(path.GetProperty("template").GetString()??""))throw new InvalidDataException();
            bool district=path.GetProperty("districtConnected").GetBoolean();
            if(path.GetProperty("heightExitsExamined").GetBoolean()&&!district)throw new InvalidDataException();
        }
        foreach(var edge in edges.EnumerateArray())
        {
            Position(edge.GetProperty("from"));Position(edge.GetProperty("to"));
            bool inside=edge.GetProperty("insideRegion").GetBoolean();var native=edge.GetProperty("nativeConnected");
            if(inside ? native.ValueKind is not (JsonValueKind.True or JsonValueKind.False) : native.ValueKind!=JsonValueKind.Null)
                throw new InvalidDataException("invalid_access_edge_observation");
            if(roadGraph)
            {
                var general=edge.GetProperty("generalConnected");
                if(inside ? general.ValueKind is not (JsonValueKind.True or JsonValueKind.False) : general.ValueKind!=JsonValueKind.Null)
                    throw new InvalidDataException("invalid_access_general_edge_observation");
            }
            int attempts=Count(edge,"candidateAttempts",4096)+Count(edge,"geometryBlockedAttempts",4096)+Count(edge,"unsupportedAttempts",4096);
            bool landingConnected=false;
            if(landingPreview)
            {
                bool candidate=edge.GetProperty("landingCandidate").GetBoolean();
                var preview=edge.GetProperty("previewLandingConnected");
                if(preview.ValueKind is not (JsonValueKind.Null or JsonValueKind.True or JsonValueKind.False)||
                    !inside&&candidate||(!inside||!candidate)&&preview.ValueKind!=JsonValueKind.Null)
                    throw new InvalidDataException("invalid_landing_preview_evidence");
                landingConnected=preview.ValueKind==JsonValueKind.True;
                if(landingConnected&&d.GetProperty("landingPreviewCount").GetInt32()==0)throw new InvalidDataException("missing_landing_preview_probe");
            }
            if(attempts>4096||attempts>0&&!AccessRouteSearch.ConfirmedHeightExit(native.ValueKind==JsonValueKind.True,landingConnected))throw new InvalidDataException("unconfirmed_access_edge_attempt");
        }
        int visited=0;
        foreach(var route in routes.EnumerateArray())
        {
            visited+=Count(route,"visitedStates",2048);
            Count(route,"sourceCount");Count(route,"targetCount");Count(route,"generatedStates");Count(route,"peakOpenStates");Count(route,"pathParts",32);
            if(route.GetProperty("closestRemainingCost").GetInt32() is <-1 or >16384)throw new InvalidDataException();
            if(route.GetProperty("phase").GetString() is not ("finished_entrance" or "construction_work_cell"))throw new InvalidDataException();
            bool found=route.GetProperty("reason").GetString()=="candidate";
            if(found)Position(route.GetProperty("selectedSource"));
            else if(route.GetProperty("selectedSource").ValueKind!=JsonValueKind.Null)throw new InvalidDataException();
        }
        if(visited!=assessment.GetProperty("visitedNodes").GetInt32())throw new InvalidDataException("inconsistent_access_search_visits");
    }
}
