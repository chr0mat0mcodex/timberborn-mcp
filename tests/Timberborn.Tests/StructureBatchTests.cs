using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class StructureBatchTests
{
    private sealed class Port : IStructureBatchPort
    {
        public int Submissions, Inspections;
        public bool Finished, LoseResponse;
        public Task<JsonObject> PlanStructure(BuildBatchSpec s, CancellationToken ct) => Task.FromResult(new JsonObject {
            ["state"]="complete", ["planKey"]=new string('a',64),
            ["steps"]=new JsonArray(s.Items.Select((item,i)=>(JsonNode)new JsonObject {
                ["template"]=item.Template,["x"]=s.X+i,["y"]=s.Y,["z"]=s.Z,["rotation"]=0,["dependsOn"]=new JsonArray() }).ToArray()),
            ["selected"]=new JsonArray(s.Items.Select((_,i)=>(JsonNode)new JsonObject { ["inputIndex"]=i,["stepIndex"]=i }).ToArray()) });
        public Task<JsonObject> Structure(BuildBatchJob j,string operation,CancellationToken ct)
        {
            if(operation=="run") { Submissions++;Assert.Equal("pending_build",j.State);if(LoseResponse)throw new IOException("synthetic lost acknowledgement"); }
            else Inspections++;
            return Task.FromResult(new JsonObject { ["state"]=Finished?"completed":"running",["reason"]="synthetic",
                ["actionId"]=j.StructureActionId });
        }
        public Task ConfigureStructure(BuildBatchJob j,Action<BuildBatchJob> save,CancellationToken ct)=>Task.CompletedTask;
        public Task<NativeSimulation> Current(BuildBatchSpec s,CancellationToken ct)=>Task.FromResult(new NativeSimulation(0,1,0,0));
        public Task<string?> Prerequisite(BuildBatchSpec s,int index,CancellationToken ct)=>throw new InvalidOperationException("No per-building material gate");
        public Task<NativeBuildingPlan> Plan(BuildBatchSpec s,int index,int rotation,CancellationToken ct)=>throw new InvalidOperationException("No sequential planning");
        public Task<NativeRemovalTarget[]> Targets(BuildBatchSpec s,CancellationToken ct)=>throw new NotSupportedException();
        public Task<NativeRemoval> Mark(BuildBatchSpec s,NativeRemovalTarget t,CancellationToken ct)=>throw new NotSupportedException();
        public Task<NativeSimulationRun> ClearanceRun(BuildBatchJob j,bool start,CancellationToken ct)=>throw new NotSupportedException();
        public Task<NativeProjectExecution> Submit(BuildBatchJob j,CancellationToken ct)=>throw new InvalidOperationException("No per-building submission");
        public Task<CompletionReport> Completion(BuildBatchJob j,bool advance,CancellationToken ct)=>throw new InvalidOperationException("No per-building finish barrier");
    }
    private static BuildBatchJob Job()
    {
        var spec=new BuildBatchSpec("11111111-1111-1111-1111-111111111111","22222222-2222-2222-2222-222222222222",
            "33333333-3333-3333-3333-333333333333",1,1,3,8,8,[0],
            [new("Lodge.Folktails"),new("SmallWarehouse.Folktails")],"none",0,24,24,7,300);
        return new() {Spec=spec,Fingerprint=BuildBatchState.Fingerprint(spec),SchemaVersion=2};
    }
    [Fact]
    public async Task WholeGroupSubmittedOnceAndCompletedOnlyAfterSharedFinish()
    {
        var port=new Port();var job=Job();var engine=new BuildBatchEngine(port,_=>{});
        Assert.True(await engine.Step(job,TestContext.Current.CancellationToken));
        Assert.Equal(1,port.Submissions);Assert.Equal(0,job.Index);
        Assert.True(await engine.Step(job,TestContext.Current.CancellationToken));
        port.Finished=true;
        Assert.False(await engine.Step(job,TestContext.Current.CancellationToken));
        Assert.Equal("completed",job.State);Assert.Equal(2,job.Finished.Count);Assert.Equal(1,port.Submissions);
    }
    [Fact]
    public void SchemaTwoCheckpointKeepsWholePlanAndStableActionAcrossReload()
    {
        string directory=Path.Combine(Path.GetTempPath(),"structure-fixture-"+Guid.NewGuid().ToString("N"));
        try {
            var store=new BuildBatchStore(directory);var job=Job();
            using var lease=store.Acquire();
            job.State="pending_build";
            job.StructurePlan=new JsonObject { ["state"]="complete", ["steps"]=new JsonArray(), ["selected"]=new JsonArray() };
            store.Save(job);
            var read=store.Load(job.Spec.Session,job.Spec.BatchId)!;
            Assert.Equal(2,read.SchemaVersion);Assert.Equal(job.StructureActionId,read.StructureActionId);
            Assert.Equal("pending_build",read.State);Assert.NotNull(read.StructurePlan);
        } finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }

    [Fact]
    public async Task LostAcknowledgementRecoversOnlyByInspectingSameStructure()
    {
        var port=new Port {LoseResponse=true};var job=Job();var engine=new BuildBatchEngine(port,_=>{});
        await Assert.ThrowsAsync<IOException>(()=>engine.Step(job,TestContext.Current.CancellationToken));
        Assert.Equal("pending_build",job.State);
        await engine.Step(job,TestContext.Current.CancellationToken);
        Assert.Equal(1,port.Submissions);Assert.Equal(1,port.Inspections);
    }
}
