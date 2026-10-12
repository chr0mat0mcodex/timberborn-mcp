using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class ClearedGroupTests
{
    private static ClearedGroupSpec Spec => new("11111111-1111-4111-8111-111111111111","22222222-2222-4222-8222-222222222222",
        "33333333-3333-4333-8333-333333333333",0,0,3,8,8,["Bench.Folktails","LumberjackFlag.Folktails"],[]);
    private static NativeRemovalTarget Tree(string kind="vegetation")=>new(Guid.Parse("44444444-4444-4444-8444-444444444444"),kind,"Oak",new(1,1,3),"demolition_mark",true,false,"unknown","current_components",OverlapCells:[new(1,1,3)]);
    private static ClearedGroupJob Job(Fake port,ClearedGroupSpec? spec=null) {
        var s=spec??Spec;return new(){Spec=s,ClearanceKey=ClearedGroupPolicy.Key(s,port.Observation())};
    }
    private static Task Run(Fake p,ClearedGroupJob j,List<string>? pending=null,Func<bool>? stop=null)=>
        new ClearedGroupEngine(p,x=>{if(x.PendingAction is {} action)pending?.Add(action);},stop??(()=>false),(_,_)=>Task.CompletedTask)
        .Run(j,TestContext.Current.CancellationToken);
    private sealed class Fake : IClearedGroupPort
    {
        public List<NativeRemovalTarget> Objects=[Tree("planted"),Tree("debris") with{Id=Guid.Parse("55555555-5555-4555-8555-555555555555"),Template="RecoveredGoodStack",Mode="delete"}];
        public bool Designation=true,Running,Cancelled,Finished,Clear=true,Partial,FailMark,Changed,Interrupted;
        public int Marks,Builds,Closes,Pauses,Scans;
        public string BuildState="completed";
        public List<string> Calls=[];
        public ClearanceObservation Observation()=>new(Objects.ToArray(),Designation?[new("tree_planting",new(1,1,3),"Oak")]:[]);
        public Task RequirePaused(ClearedGroupSpec s,CancellationToken ct) {Pauses++;if(Running)throw new InvalidDataException();return Task.CompletedTask;}
        public Task<ClearanceObservation> Observe(ClearedGroupSpec s,bool areas,CancellationToken ct) {Scans++;return Task.FromResult(Observation());}
        public Task RemoveDesignation(ClearedGroupSpec s,ClearanceDesignation d,CancellationToken ct) {
            Calls.Add("designation");Designation=false;Objects=Objects.Select(t=>t.Kind=="planted"?t with{Kind="vegetation"}:t).ToList();return Task.CompletedTask;
        }
        public Task Mark(ClearedGroupSpec s,NativeRemovalTarget t,CancellationToken ct) {
            Assert.Contains(t.Kind,new[]{"planted","vegetation"});Assert.False(Designation);Marks++;Calls.Add("mark");
            if(FailMark)throw new HttpRequestException();
            Objects=Objects.Select(o=>o.Id==t.Id?o with{Marked=true}:o).ToList();return Task.CompletedTask;
        }
        public Task<NativeSimulationRun> Clearance(ClearedGroupJob j,string operation,int seconds,CancellationToken ct) {
            Calls.Add(operation);
            if(operation=="start")Running=true;
            if(operation=="inspect" && Clear)Objects.RemoveAll(t=>t.Kind is "vegetation" or "planted");
            if(operation=="inspect" && Changed)Objects=Objects.Select(t=>t with{Marked=false}).ToList();
            if(operation=="cancel") {Running=false;Cancelled=true;}
            if(operation=="inspect" && !Clear && !Changed) {Running=false;Finished=true;}
            if(Interrupted)Running=false;
            return Task.FromResult(new NativeSimulationRun(j.RunId,Interrupted?"interrupted":Cancelled?"cancelled":Finished?"completed":"running",
                Interrupted?"speed_changed":"target_reached",0,48,Finished?48:1,Finished?48:1,0,1,60,7,Running?7:0,!Running,Finished,Interrupted||Cancelled||Finished));
        }
        public Task<JsonObject> Plan(ClearedGroupSpec s,CancellationToken ct) {
            Calls.Add("plan");Assert.False(Running);Assert.False(Designation);
            return Task.FromResult(new JsonObject{["state"]=Partial?"partial":"complete",["steps"]=new JsonArray(),["planKey"]=new string('a',64)});
        }
        public Task<JsonObject> Build(ClearedGroupJob j,string operation,int seconds,CancellationToken ct) {
            Calls.Add("build_"+operation);
            if(operation=="run")Builds++;if(operation=="close")Closes++;
            return Task.FromResult(new JsonObject{["state"]=BuildState,["reason"]="native_blocker",["released"]=operation=="close",["simulation"]=new JsonObject{["pauseConfirmed"]=true}});
        }
    }
    [Fact] public async Task ClearsThenPausesReplansAndRunsOneWholeGroupWithoutDeletingDebris() {
        var p=new Fake();var j=Job(p);var writes=new List<string>();await Run(p,j,writes);
        Assert.Equal("completed",j.State);Assert.True(j.PauseConfirmed);Assert.Equal(1,p.Marks);Assert.Equal(1,p.Builds);Assert.Equal(1,p.Closes);
        Assert.Single(p.Objects);Assert.Equal("debris",p.Objects[0].Kind);
        Assert.True(p.Calls.IndexOf("cancel")<p.Calls.IndexOf("plan"));Assert.Contains("mark_vegetation",writes);Assert.Contains("start_group_build",writes);
    }
    [Fact] public async Task ClearanceBudgetEndsWithRemainingTargetsAndNoBuild() {
        var p=new Fake{Clear=false};var j=Job(p);await Run(p,j);
        Assert.Equal("blocked",j.State);Assert.Contains("clearance_budget",j.Reason);Assert.Equal(1,j.RemainingTargets);Assert.Equal(0,p.Builds);
    }
    [Fact] public async Task UnconfirmedMarkIsNeverReplayedAndNeverStartsSimulation() {
        var p=new Fake{FailMark=true};var j=Job(p);await Run(p,j);
        Assert.Equal("unconfirmed",j.State);Assert.Equal("mark_vegetation",j.PendingAction);Assert.Equal(1,p.Marks);Assert.DoesNotContain("start",p.Calls);
    }
    [Fact] public async Task ChangedConfirmationKeyDoesNotMutate() {
        var p=new Fake();var j=Job(p);p.Objects[0]=p.Objects[0] with{Marked=true};await Run(p,j);
        Assert.Equal("clearance_changed_replan",j.Reason);Assert.Empty(p.Calls);
    }
    [Fact] public async Task PartialGroupPlanDoesNotDispatchPartialConstruction() {
        var p=new Fake{Partial=true};var j=Job(p);await Run(p,j);Assert.Equal(0,p.Builds);Assert.Equal("incomplete_group_plan_no_build_orders",j.Reason);
    }
    [Theory] [InlineData(true)] [InlineData(false)] public async Task ExternalChangeStopsWithoutNewGroup(bool speedChange) {
        var p=new Fake{Interrupted=speedChange,Clear=false,Changed=!speedChange};var j=Job(p);await Run(p,j);
        Assert.Equal("blocked",j.State);Assert.Equal(0,p.Builds);
    }
    [Fact] public async Task StopBeforeFirstMutationLeavesEverythingUntouched() {
        var p=new Fake();var j=Job(p);await Run(p,j,stop:()=>true);Assert.Equal("stopped",j.State);Assert.Empty(p.Calls);
    }
    [Fact] public void ReservedColumnsExcludeClearingEvenOnAnotherHeight() {
        var s=Spec with{ReservedAreas=[new(){X=1,Y=1,Z=7,Width=1,Height=1,Depth=1}]};var p=new Fake();
        Assert.Empty(ClearedGroupPolicy.Targets(s,p.Observation()));Assert.Empty(ClearedGroupPolicy.Designations(s,p.Observation()));
    }
    [Fact] public void DebrisIsNotPartOfConfirmationKeyOrClearance() {
        var p=new Fake();string key=ClearedGroupPolicy.Key(Spec,p.Observation());p.Objects.RemoveAll(t=>t.Kind=="debris");
        Assert.Equal(key,ClearedGroupPolicy.Key(Spec,p.Observation()));Assert.Single(ClearedGroupPolicy.Targets(Spec,p.Observation()));
    }
    [Fact] public void InvalidBudgetsAndVolumesFailBeforeDispatch() {
        Assert.Throws<ArgumentException>(()=>ClearedGroupPolicy.Validate(Spec with{Width=9}));
        Assert.Throws<ArgumentException>(()=>ClearedGroupPolicy.Validate(Spec with{ClearanceHours=600,ConstructionHours=100}));
        Assert.Throws<ArgumentException>(()=>ClearedGroupPolicy.Validate(Spec with{Templates=[]}));
    }
    [Fact] public void BoundedAreaQueryAcceptsExactlyOneSmallRegion() {
        var q=new NameValueCollection{{"kind","crops"},{"offset","0"},{"limit","256"},{"x","1"},{"y","2"},{"z","3"},{"width","8"},{"height","8"}};
        var r=ManagementRequest.Parse("/agent-api/v1/areas",q);Assert.True(r.BoundedArea);Assert.Equal(8,r.Width);
        q["width"]="9";Assert.Throws<ArgumentException>(()=>ManagementRequest.Parse("/agent-api/v1/areas",q));
    }
    [Fact] public void CatalogRequiresAllActionGatesAndMarksInspectReadOnly() {
        Assert.Equal(new[]{ClearedGroupTools.Inspect,ClearedGroupTools.Stop},ClearedGroupTools.Catalog(false).Select(t=>t.Name));
        Assert.Equal(4,ClearedGroupTools.Catalog(true).Count());
        Assert.True(ClearedGroupTools.Catalog(true).Single(t=>t.Name==ClearedGroupTools.Inspect).Annotations!.ReadOnlyHint);
        Assert.False(ClearedGroupTools.Catalog(true).Single(t=>t.Name==ClearedGroupTools.Start).Annotations!.ReadOnlyHint);
    }
    [Fact] public void ResourcePileOnlyCellsRemainCandidateWindows() {
        var r=new SurveyRequest(Spec.Session,Spec.DistrictId,"Bench.Folktails",0,0,3,2,1);
        var windows=RegionSurvey.Windows(r,["pd"]);Assert.Single(windows);Assert.Equal(1,windows[0].FreeCells);
    }
    [Fact] public void CompactProjectionPreservesLifecycleAndNativeIds() {
        var full=JsonSerializer.SerializeToNode(Job(new Fake()),NativeJson.Options)!.AsObject();
        full["observation"]=JsonSerializer.SerializeToNode(new Fake().Observation(),NativeJson.Options);
        var compact=full.DeepClone().AsObject();ClearedGroupTools.Compact(compact);
        foreach(string key in new[]{"state","runId","actionId","pendingAction","clearanceKey"})Assert.True(JsonNode.DeepEquals(full[key],compact[key]));
        Assert.NotNull(full["observation"]);Assert.Null(compact["observation"]);
    }
    [Fact] public async Task PersistedInterruptedJobIsReadWithoutReplay() {
        string directory=Path.Combine(Path.GetTempPath(),"cleared-group-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try {
            var fake=new Fake();var job=Job(fake);job.PendingAction="start_group_build";
            File.WriteAllText(Path.Combine(directory,Spec.Session+"-"+Spec.JobId+".json"),JsonSerializer.Serialize(job,NativeJson.Options));
            using var tools=new ClearedGroupTools(fake,directory);
            var handle=JsonSerializer.SerializeToElement(new ClearedGroupHandle(Spec.Session,Spec.JobId),NativeJson.Options);
            var result=await tools.Invoke(ClearedGroupTools.Inspect,handle,false,TestContext.Current.CancellationToken);
            Assert.Equal("interrupted",result["data"]!["state"]!.GetValue<string>());Assert.Equal("start_group_build",result["data"]!["pendingAction"]!.GetValue<string>());
            var start=JsonSerializer.SerializeToElement(new ClearedGroupStart(Spec,job.ClearanceKey),NativeJson.Options);
            await tools.Invoke(ClearedGroupTools.Start,start,true,TestContext.Current.CancellationToken);
            Assert.Empty(fake.Calls);Assert.Equal(0,fake.Scans);
        } finally {Directory.Delete(directory,true);}
    }

    [Fact] public void FullDetailReachesWorkflowInsteadOfBeingStripped() {
        var args=JsonSerializer.SerializeToElement(new ClearedGroupHandle(Spec.Session,Spec.JobId,"full"),NativeJson.Options);
        var parsed=ResponsePresentation.ReadRequest(ClearedGroupTools.Inspect,args);
        Assert.Equal("full",parsed.Arguments.GetProperty("detail").GetString());
    }
    [Fact] public void ClearanceObservationSupportsOneBoundedPageInsteadOfMovingPagination() {
        var q=new NameValueCollection{{"kind","all"},{"x","0"},{"y","0"},{"z","3"},{"width","8"},{"height","8"},{"depth","4"},{"offset","0"},{"limit","256"}};
        Assert.Equal(256,RemovalRequest.Parse("/agent-api/v1/removal-targets",q).Limit);
        q["limit"]="257";Assert.Throws<ArgumentException>(()=>RemovalRequest.Parse("/agent-api/v1/removal-targets",q));
    }

    [Fact] public void HomogeneousDesignationsAreBatchedWithoutCrossingReservedColumns() {
        var cells=Enumerable.Range(0,8).SelectMany(y=>Enumerable.Range(0,8).Select(x=>new ClearanceDesignation("crops",new(x,y,3),"Carrot"))).ToArray();
        Assert.Equal(4,ClearedGroupPolicy.DesignationBatches(Spec,new([],cells)).Length);
        var spec=Spec with{ReservedAreas=[new(){X=2,Y=2,Z=3,Width=1,Height=1,Depth=1}]};
        var batches=ClearedGroupPolicy.DesignationBatches(spec,new([],cells));
        Assert.Equal(63,batches.Sum(b=>b.Width*b.Height));
        Assert.All(batches,b=>Assert.False(2>=b.Position.X && 2<b.Position.X+b.Width && 2>=b.Position.Y && 2<b.Position.Y+b.Height));
    }

    [Fact] public async Task EmptyClearanceStartsBuildingWithoutAdvancingClearanceTime() {
        var fake=new Fake{Designation=false};fake.Objects.RemoveAll(t=>t.Kind!="debris");var job=Job(fake);
        await Run(fake,job);Assert.Equal("completed",job.State);Assert.DoesNotContain("start",fake.Calls);Assert.Equal(1,fake.Builds);
    }
    [Fact] public async Task StopDuringClearanceCancelsOwnRunWithoutBuilding() {
        var fake=new Fake{Clear=false,Changed=true};var job=Job(fake);bool requested=false;
        await Run(fake,job,stop:()=>requested=requested||fake.Running);
        Assert.Equal("stopped",job.State);Assert.Equal(0,fake.Builds);Assert.Contains("cancel",fake.Calls);
    }
    [Fact] public void EveryMissingWriteGateHidesGroupStart() {
        foreach(int disabled in Enumerable.Range(0,4)) {
            var names=NativeTools.Catalog(enableBuildingPlacement:disabled!=0,enableSpeedControl:disabled!=1,
                enableRemoval:disabled!=2,enableAreas:disabled!=3).Select(t=>t.Name);
            Assert.DoesNotContain(ClearedGroupTools.Start,names);Assert.Contains(ClearedGroupTools.Inspect,names);
        }
    }

    [Theory] [InlineData("blocked")] [InlineData("interrupted")] [InlineData("unconfirmed")] public async Task NativeTerminalBlockadeReturnsImmediately(string state) {
        var fake=new Fake{BuildState=state};var job=Job(fake);await Run(fake,job);
        Assert.Equal(state,job.State);Assert.Equal("native_blocker",job.Reason);Assert.Equal(0,fake.Closes);Assert.Single(fake.Calls,c=>c=="build_inspect");
    }

}
