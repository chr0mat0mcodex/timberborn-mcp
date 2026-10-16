using System.Text.Json.Nodes;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class StructureBatchTests
{
    private sealed class Port : IStructureBatchPort
    {
        public int Submissions, Inspections;
        public bool Finished, LoseResponse;
        public Exception? PlanningFailure, SubmissionFailure;
        public int PlanningCalls;
        public Task<JsonObject> PlanStructure(BuildBatchSpec s, CancellationToken ct) {
            PlanningCalls++;
            if(PlanningFailure is not null) throw PlanningFailure;
            return Task.FromResult(new JsonObject {
            ["state"]="complete", ["planKey"]=new string('a',64),
            ["steps"]=new JsonArray(s.Items.Select((item,i)=>(JsonNode)new JsonObject {
                ["template"]=item.Template,["x"]=s.X+i,["y"]=s.Y,["z"]=s.Z,["rotation"]=0,["dependsOn"]=new JsonArray() }).ToArray()),
            ["selected"]=new JsonArray(s.Items.Select((_,i)=>(JsonNode)new JsonObject { ["inputIndex"]=i,["stepIndex"]=i }).ToArray()) });
        }
        public Task<JsonObject> Structure(BuildBatchJob j,string operation,CancellationToken ct)
        {
            if(operation=="run") { Submissions++;Assert.Equal("pending_build",j.State);if(SubmissionFailure is not null)throw SubmissionFailure;if(LoseResponse)throw new IOException("synthetic lost acknowledgement"); }
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

    [Theory]
    [InlineData("project_plan_faulted", false, "not_executed", "planning")]
    [InlineData("project_preview_failed", false, "unconfirmed", "planning")]
    [InlineData("project_plan_faulted", true, "not_executed", "pending_build")]
    [InlineData("project_preview_failed", true, "ledger_required", "pending_build")]
    public async Task Native_failure_survives_checkpoint_inspect_and_advance_without_replanning(string code,bool submitting,string execution,string stage)
    {
        string directory=Path.Combine(Path.GetTempPath(),"batch-failure-fixture-"+Guid.NewGuid().ToString("N"));
        try {
            var job=Job();var store=new BuildBatchStore(directory);var port=new Port();
            if(submitting)port.SubmissionFailure=new BridgeRejectionException(code);
            else port.PlanningFailure=new BridgeRejectionException(code);
            var result=await BuildBatchTools.Execute(BuildBatchTools.Start,
                JsonSerializer.SerializeToElement(new BatchStartRequest(job.Spec,0),NativeJson.Options),true,true,true,store,port,TestContext.Current.CancellationToken);
            Assert.Equal(code,result["error"]!["code"]!.GetValue<string>());
            Assert.Equal(execution,result["error"]!["execution"]!.GetValue<string>());
            Assert.Equal("stopped",result["data"]!["state"]!.GetValue<string>());
            Assert.Equal(stage,result["data"]!["failure"]!["stage"]!.GetValue<string>());
            Assert.Equal("reload_game_and_reinspect",result["data"]!["nextAction"]!.GetValue<string>());
            Assert.Contains(code,ActivityOutcome.Describe(BuildBatchTools.Start,result));
            Assert.Contains("Phase "+stage,ActivityOutcome.Describe(BuildBatchTools.Start,result));
            Assert.Contains("reload_game_and_reinspect",ActivityOutcome.Describe(BuildBatchTools.Start,result));
            foreach(var tool in new[]{BuildBatchTools.Inspect,BuildBatchTools.Advance}) {
                var read=await BuildBatchTools.Execute(tool,
                    JsonSerializer.SerializeToElement(new BatchHandle(job.Spec.Session,job.Spec.BatchId,Details:true),NativeJson.Options),
                    true,true,true,new BuildBatchStore(directory),port,TestContext.Current.CancellationToken);
                Assert.Equal(code,read["error"]!["code"]!.GetValue<string>());
                Assert.Equal(stage,read["data"]!["failure"]!["stage"]!.GetValue<string>());
                Assert.Contains(code,ActivityOutcome.Describe(tool,read));
                Assert.Contains("Phase "+stage,ActivityOutcome.Describe(tool,read));
                Assert.Contains("reload_game_and_reinspect",ActivityOutcome.Describe(tool,read));
            }
            Assert.Equal(1,port.PlanningCalls);Assert.Equal(submitting?1:0,port.Submissions);Assert.Equal(0,port.Inspections);
        } finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }

    [Fact]
    public async Task Lost_submission_keeps_recovery_checkpoint_and_never_resubmits()
    {
        string directory=Path.Combine(Path.GetTempPath(),"batch-lost-fixture-"+Guid.NewGuid().ToString("N"));
        try {
            var job=Job();var store=new BuildBatchStore(directory);var port=new Port {LoseResponse=true};
            var result=await BuildBatchTools.Execute(BuildBatchTools.Start,
                JsonSerializer.SerializeToElement(new BatchStartRequest(job.Spec,0),NativeJson.Options),true,true,true,store,port,TestContext.Current.CancellationToken);
            Assert.Equal("pending_build",result["data"]!["state"]!.GetValue<string>());
            Assert.Equal("ledger_required",result["error"]!["execution"]!.GetValue<string>());
            Assert.DoesNotContain("synthetic lost acknowledgement",result.ToJsonString());
            var read=await BuildBatchTools.Execute(BuildBatchTools.Advance,
                JsonSerializer.SerializeToElement(new BatchHandle(job.Spec.Session,job.Spec.BatchId),NativeJson.Options),true,true,true,store,port,TestContext.Current.CancellationToken);
            Assert.Equal("ok",read["status"]!.GetValue<string>());
            Assert.Null(read["data"]!["failure"]);
            Assert.Equal(1,port.Submissions);Assert.Equal(1,port.Inspections);
        } finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }

    [Fact]
    public async Task Unexpected_planning_failure_does_not_leak_private_text_or_repeat_search()
    {
        string directory=Path.Combine(Path.GetTempPath(),"batch-plan-fixture-"+Guid.NewGuid().ToString("N"));
        try {
            var job=Job();var store=new BuildBatchStore(directory);var port=new Port {PlanningFailure=new ArgumentException("synthetic private diagnostic")};
            var result=await BuildBatchTools.Execute(BuildBatchTools.Start,
                JsonSerializer.SerializeToElement(new BatchStartRequest(job.Spec,0),NativeJson.Options),true,true,true,store,port,TestContext.Current.CancellationToken);
            Assert.Equal("stopped",result["data"]!["state"]!.GetValue<string>());
            Assert.Equal("batch_interrupted",result["error"]!["code"]!.GetValue<string>());
            Assert.DoesNotContain("synthetic private diagnostic",result.ToJsonString());
            await BuildBatchTools.Execute(BuildBatchTools.Advance,
                JsonSerializer.SerializeToElement(new BatchHandle(job.Spec.Session,job.Spec.BatchId),NativeJson.Options),true,true,true,store,port,TestContext.Current.CancellationToken);
            Assert.Equal(1,port.PlanningCalls);Assert.Equal(0,port.Submissions);
        } finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
    }
}
