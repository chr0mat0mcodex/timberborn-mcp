using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public interface IStructureBatchPort : IBuildBatchPort
{
    Task<JsonObject> PlanStructure(BuildBatchSpec spec, CancellationToken ct);
    Task<JsonObject> Structure(BuildBatchJob job, string operation, CancellationToken ct);
    Task ConfigureStructure(BuildBatchJob job, Action<BuildBatchJob> save, CancellationToken ct);
}

public sealed partial class NativeBuildBatchPort
{
    public async Task ConfigureStructure(BuildBatchJob j, Action<BuildBatchJob> save, CancellationToken ct)
    {
        foreach (var selection in j.StructurePlan!["selected"]!.AsArray()) {
            int input=selection!["inputIndex"]!.GetValue<int>(), part=selection["stepIndex"]!.GetValue<int>();
            var item=j.Spec.Items[input];
            if (item.InitialStorageGood is null && item.InitialStorageMode is null) continue;
            string id=j.StructureReceipt!["entityIds"]![part]!.GetValue<string>();
            foreach (var field in new[] { "good", "mode" }) {
                string? desired=field=="good"?item.InitialStorageGood:item.InitialStorageMode;
                if(desired is null) continue;
                var q=Query(("session",j.Spec.Session),("id",id));
                var read=Check(j.Spec,await client.BuildingSettings(BuildingSettingsRequest.Parse("/agent-api/v1/building-settings",q),ct));
                var step=j.StructurePlan["steps"]![part]!;
                if(read.Template!=item.Template || read.Position!=new Position(step["x"]!.GetValue<int>(),step["y"]!.GetValue<int>(),step["z"]!.GetValue<int>()) || read.Storage is null)
                    throw new InvalidDataException("structure_configuration_identity_mismatch");
                string? current=field=="good"?read.Storage.SelectedGood:read.Storage.Mode;
                if(current==desired) continue;
                string key=input+":"+field;
                if(j.ConfigurationAttempts.Contains(key)) throw new InvalidDataException("structure_configuration_unconfirmed_no_retry");
                if(current!=(field=="good"?"":"accept")) throw new InvalidDataException("structure_initial_configuration_changed");
                j.ConfigurationAttempts.Add(key); save(j);
                q[field]=desired; q[field=="good"?"expectedGood":"expectedMode"]=current;
                var result=Check(j.Spec,await client.SetBuildingSetting(BuildingSettingsRequest.Parse("/agent-api/v1/set-storage-"+field,q),ct));
                if(result.Outcome!="applied") throw new IOException("structure_configuration_unconfirmed");
            }
        }
    }
    public async Task<JsonObject> PlanStructure(BuildBatchSpec s, CancellationToken ct)
    {
        var args = JsonSerializer.SerializeToElement(new {
            session=s.Session, districtId=s.DistrictId, x=s.X, y=s.Y, z=s.Z,
            width=s.Width, height=s.Height, templates=s.Items.Select(i=>i.Template).ToArray(),
            reservedAreas=Array.Empty<ReservedArea>(), searchBudget=32, maxCandidates=4,
            scanOtherHeights=false, maxWindows=1, planBuildings=true }, NativeJson.Options);
        var result = await RegionGroupPlanning.Invoke(client, args, ct, allowedRotations:s.Rotations);
        var data=result["data"]!.AsObject();
        // Persist the executable plan and coverage, not every discarded preview.
        var plan=new JsonObject();
        foreach(string key in new[]{"state","steps","planKey","selected","unplaced","requested","planned","search","surveys"})
            if(data[key] is JsonNode value)plan[key]=value.DeepClone();
        return plan;
    }

    public async Task<JsonObject> Structure(BuildBatchJob j, string operation, CancellationToken ct)
    {
        var s=j.Spec;
        if (operation == "run") {
            var plan=j.StructurePlan ?? throw new InvalidDataException("structure_plan_missing");
            var args=new JsonObject {
                ["session"]=s.Session, ["actionId"]=j.StructureActionId, ["districtId"]=s.DistrictId,
                ["steps"]=plan["steps"]!.DeepClone(), ["planKey"]=plan["planKey"]!.DeepClone(),
                ["mode"]="development_pilot", ["speed"]=s.Speed,
                ["maxGameHours"]=s.ConstructionHours, ["maxRealSeconds"]=s.MaxRealSeconds };
            return JsonNode.Parse(Check(s, await client.LargeProject(LargeProjectTools.Parse("run_large_project",JsonSerializer.SerializeToElement(args)),ct)).GetRawText())!.AsObject();
        }
        var q=Query(("session",s.Session),("actionId",j.StructureActionId));
        return JsonNode.Parse(Check(s,await client.LargeProject(LargeProjectRequest.Parse("/agent-api/v1/large-project-"+operation,q),ct)).GetRawText())!.AsObject();
    }
}
