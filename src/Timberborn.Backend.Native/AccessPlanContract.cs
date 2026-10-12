using System.Collections.Specialized;
using System.Text.Json;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed partial class NativeClient
{
    private static void ValidateAccessPlan(BridgeEnvelope<JsonElement> envelope,LargeProjectRequest request)
    {
        var d=envelope.Data; bool found=d.GetProperty("candidateFound").GetBoolean();
        if(request.ReservedAreas.Length>0 && (envelope.BridgeVersion is not ("0.51.0" or "0.52.0" or "0.53.0") ||
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
}
