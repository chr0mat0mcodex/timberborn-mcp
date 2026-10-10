using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuildBatchTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Catalog_has_MCP_object_roots_with_and_without_write_gates(bool enabled) {
        var tools = BuildBatchTools.Catalog(enabled).Select(ActivityTools.WithReasoning).ToArray();
        Assert.Equal(enabled ? 4 : 2, tools.Length);
        Assert.All(tools, tool => {
            Assert.Equal("object", tool.InputSchema.GetProperty("type").GetString());
            Assert.False(tool.InputSchema.GetProperty("additionalProperties").GetBoolean());
            Assert.True(tool.InputSchema.GetProperty("properties").TryGetProperty("reasoning", out _));
            Assert.Contains(tool.InputSchema.GetProperty("required").EnumerateArray(), p => p.GetString() == "reasoning");
        });
    }
    private static BuildBatchSpec Spec() => new("11111111-1111-4111-8111-111111111111", "22222222-2222-4222-8222-222222222222",
        "33333333-3333-4333-8333-333333333333", 1, 1, 3, 8, 8, [0, 1],
        [new("SmallWarehouse.Folktails"), new("Lodge.Folktails")], "none", 0, 24, 24, 7, 300);
    private static BuildBatchJob Job(BuildBatchSpec? spec = null) {
        var s = spec ?? Spec(); return new() { Spec = s, Fingerprint = BuildBatchState.Fingerprint(s) };
    }
    private sealed class Port : IBuildBatchPort
    {
        public int Builds, Runs, Marks;
        public bool NoPlan, LoseBuildAck, LoseMarkAck, KeepMarked, ClearanceDone, LoseRunAck, RejectBuild;
        public int ClearanceStarts;
        public Func<BuildBatchSpec, int, bool>? MissingPlan;
        public List<int> SubmittedRegions = [];
        public bool ClearEachRegion;
        public HashSet<int> ClearedRegions = [];
        public string? Failure;
        public string? CompletionFailure;
        public string SavedState = "";
        public Func<string>? ReadCheckpoint;
        private string CheckpointState => ReadCheckpoint?.Invoke() ?? SavedState;
        public bool FinishedWhileRunning, PauseUnconfirmed, LosePauseAck;
        public int EarlyPauses;
        public NativeRemovalTarget[] Objects = [];
        public Task<NativeSimulation> Current(BuildBatchSpec s, CancellationToken ct) => Task.FromResult(new NativeSimulation(0, 1, 0, 0));
        public Task<string?> Prerequisite(BuildBatchSpec s, int i, CancellationToken ct) => Task.FromResult(Failure);
        public Task<NativeBuildingPlan> Plan(BuildBatchSpec s, int i, int r, CancellationToken ct) => Task.FromResult(new NativeBuildingPlan(
            s.Items[i].Template, s.DistrictId, new(s.X, s.Y, s.Z), s.Width, s.Height, r, 1, 0, 1, true, "complete",
            NoPlan || ClearEachRegion && !ClearedRegions.Contains(s.X) || MissingPlan?.Invoke(s, i) == true ? [] : [new(new(s.X + 1, s.Y + 1, s.Z), r,
                new(s.X + 1, s.Y + 2, s.Z), new(s.X + 1, s.Y + 3, s.Z),
                r == 0 ? [new(s.X + 1, s.Y + 2, s.Z)] : [], [], false, [], new string('a', 64))], []));
        public Task<NativeRemovalTarget[]> Targets(BuildBatchSpec s, CancellationToken ct) => Task.FromResult(ClearEachRegion ?
            ClearedRegions.Contains(s.X) ? [] : new[] { new NativeRemovalTarget(
                Guid.Parse(BuildBatchState.DerivedId(s, "tree:" + s.X)), "vegetation", "Pine",
                new(s.X + 1, s.Y + 1, s.Z), "demolition_mark", true, false, "natural", "tree") } : Objects);
        public Task<NativeRemoval> Mark(BuildBatchSpec s, NativeRemovalTarget t, CancellationToken ct) {
            Assert.Equal("pending_mark", CheckpointState); Marks++;
            if (ClearEachRegion) ClearedRegions.Add(s.X);
            Objects = KeepMarked ? [t with { Marked = true }] : []; NoPlan = KeepMarked;
            if (LoseMarkAck) throw new IOException();
            return Task.FromResult(new NativeRemoval(t.Id, t.Kind, t.Template, t.Position, "mark", "applied", true, null, []));
        }
        public Task<NativeSimulationRun> ClearanceRun(BuildBatchJob j, bool start, CancellationToken ct) {
            if (start) { Assert.Equal("pending_clearance_run", CheckpointState); ClearanceStarts++; if (LoseRunAck) throw new IOException(); }
            return Task.FromResult(new NativeSimulationRun(j.ClearanceRunId, ClearanceDone ? "completed" : "running", "",
                0, j.Spec.ClearanceHours, ClearanceDone ? j.Spec.ClearanceHours : 0, ClearanceDone ? j.Spec.ClearanceHours : 0,
                0, 1, j.Spec.MaxRealSeconds, j.Spec.Speed, ClearanceDone ? 0 : j.Spec.Speed, ClearanceDone, ClearanceDone, ClearanceDone));
        }
        public Task<NativeProjectExecution> Submit(BuildBatchJob j, CancellationToken ct) {
            Assert.Equal("pending_build", CheckpointState); Assert.Equal(1, j.Selection!.Rotation); Builds++;
            SubmittedRegions.Add(j.RegionIndex);
            if (RejectBuild) throw new Timberborn.Bridge.Core.BridgeRejectionException("state_conflict");
            if (LoseBuildAck) throw new IOException();
            return Task.FromResult(new NativeProjectExecution(j.ActionId, new string('a', 64), "completed", "", [], false, []));
        }
        public Task<CompletionReport> Completion(BuildBatchJob j, bool advance, CancellationToken ct) {
            if (FinishedWhileRunning) {
                if (advance) {
                    Assert.Equal("pending_build_pause", CheckpointState);
                    EarlyPauses++;
                    if (LosePauseAck) throw new IOException();
                }
                bool paused = advance && !PauseUnconfirmed;
                return Task.FromResult(new CompletionReport(j.ActionId, "run", paused ? "finished_accessible" : "running", "",
                    "completed", "", [new(j.ActionId, j.Spec.Items[j.Index].Template, true, true, null, null)], null,
                    new("run", paused ? "cancelled" : "running", paused ? "cancel_requested" : "advancing",
                        0, 24, 1, 1, 0, 1, 300, 7, paused ? 0 : 7, paused, false, paused), paused ? 0 : null, null, []));
            }
            if (advance) { Assert.Equal("pending_build_run", CheckpointState); Runs++; }
            bool finished = Runs > j.Index;
            return Task.FromResult(new CompletionReport(j.ActionId, "run", CompletionFailure ?? (finished ? "finished_accessible" : "awaiting_simulation"), "",
                "completed", "", [new(j.ActionId, j.Spec.Items[j.Index].Template, true, finished, null, null)], null, null, 0, null, []));
        }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Finished_objects_request_early_pause_but_next_build_requires_confirmed_pause(bool unconfirmed) {
        var p = new Port { FinishedWhileRunning = true, PauseUnconfirmed = unconfirmed };
        var j = Job(); var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        await e.Step(j, TestContext.Current.CancellationToken); // Submit first building.
        await e.Step(j, TestContext.Current.CancellationToken); // Observe and request pause.
        Assert.Equal(1, p.EarlyPauses); Assert.Equal(1, p.Builds);
        Assert.Equal(unconfirmed ? "building" : "planning", j.State);
        Assert.Equal(unconfirmed ? 0 : 1, j.Finished.Count);
        Assert.Equal(0, p.Runs);
    }

    [Fact]
    public async Task Lost_early_pause_ack_keeps_checkpoint_and_does_not_repeat_command() {
        string dir = Path.Combine(Path.GetTempPath(), "timberborn-pause-test-" + Guid.NewGuid().ToString("N"));
        try {
            var p = new Port { FinishedWhileRunning = true, LosePauseAck = true };
            var j = Job(); var store = new BuildBatchStore(dir);
            using var lease = store.Acquire();
            var e = new BuildBatchEngine(p, x => { p.SavedState = x.State; store.Save(x); });
            await e.Step(j, TestContext.Current.CancellationToken);
            await Assert.ThrowsAsync<IOException>(() => e.Step(j, TestContext.Current.CancellationToken));
            j = new BuildBatchStore(dir).Load(j.Spec.Session, j.Spec.BatchId)!;
            Assert.Equal("pending_build_pause", j.State);
            await e.Step(j, TestContext.Current.CancellationToken);
            Assert.Equal("stopped", j.State);
            Assert.Equal("simulation_pause_unconfirmed_no_retry", j.Reason);
            Assert.Equal(1, p.EarlyPauses); Assert.Equal(1, p.Builds); Assert.Empty(j.Finished);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Zero_wait_dispatches_ready_transitions_but_stops_for_unconfirmed_pause(bool unconfirmed) {
        string dir = Path.Combine(Path.GetTempPath(), "timberborn-immediate-test-" + Guid.NewGuid().ToString("N"));
        try {
            var s = Spec(); var store = new BuildBatchStore(dir);
            var p = new Port { FinishedWhileRunning = unconfirmed, PauseUnconfirmed = unconfirmed };
            p.ReadCheckpoint = () => store.Load(s.Session, s.BatchId)!.State;
            var input = JsonSerializer.SerializeToElement(new BatchStartRequest(s, 0), NativeJson.Options);
            var result = await BuildBatchTools.Execute(BuildBatchTools.Start, input, true, true, true, store, p, TestContext.Current.CancellationToken);
            Assert.Equal(unconfirmed ? "building" : "completed", result["data"]!["state"]!.GetValue<string>());
            Assert.Equal(unconfirmed ? 1 : 2, p.Builds); Assert.Equal(unconfirmed ? 0 : 2, p.Runs);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public void Checkpoint_replacement_keeps_open_snapshot_and_publishes_complete_new_state() {
        string dir = Path.Combine(Path.GetTempPath(), "timberborn-replace-test-" + Guid.NewGuid().ToString("N"));
        try {
            var j = Job(); var store = new BuildBatchStore(dir);
            using var lease = store.Acquire();
            store.Save(j);
            var path = Path.Combine(dir, j.Spec.Session + "-" + j.Spec.BatchId + ".json");
            using var snapshot = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            j.State = "stopped"; j.Reason = "materials_missing";
            store.Save(j);
            var current = store.Load(j.Spec.Session, j.Spec.BatchId)!;
            Assert.Equal("stopped", current.State); Assert.Equal("materials_missing", current.Reason);
            var previous = JsonSerializer.Deserialize<BuildBatchJob>(snapshot, NativeJson.Options)!;
            Assert.Equal("planning", previous.State);
            Assert.Empty(Directory.EnumerateFiles(dir, "*.tmp"));
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
    [Fact]
    public async Task Regions_switch_after_finished_build_and_survive_each_checkpoint_reload() {
        string dir = Path.Combine(Path.GetTempPath(), "timberborn-regions-test-" + Guid.NewGuid().ToString("N"));
        try {
            var s = Spec() with { AdditionalRegions = [new(20, 1, 3, 8, 8)] };
            var j = Job(s); var p = new Port { MissingPlan = (region, index) => index == 1 && region.X == 1 };
            var store = new BuildBatchStore(dir);
            using (store.Acquire()) {
                store.Save(j);
                for (int i = 0; i < 16 && !j.Terminal; i++) {
                    var e = new BuildBatchEngine(p, x => { p.SavedState = x.State; store.Save(x); });
                    await e.Step(j, TestContext.Current.CancellationToken);
                    j = store.Load(s.Session, s.BatchId)!;
                }
            }
            Assert.Equal("completed", j.State); Assert.Equal(new[] { 0, 1 }, p.SubmittedRegions);
            Assert.Equal(2, j.Finished.Count); Assert.Equal(1, j.Finished[1].RegionIndex);
            Assert.Equal(21, j.Finished[1].Origin.X); Assert.Single(j.PreviousRegions);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task Empty_regions_exhaust_once_and_pending_build_never_switches_or_resubmits() {
        var s = Spec() with { AdditionalRegions = [new(20, 1, 3, 8, 8)] };
        var j = Job(s); var p = new Port { NoPlan = true };
        var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        for (int i = 0; i < 4; i++) await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("no_candidate_in_authorized_regions", j.Reason); Assert.Equal(0, p.Builds);
        Assert.Equal(1, j.RegionIndex);
        j = Job(s); p = new Port { MissingPlan = (region, _) => region.X == 1, LoseBuildAck = true };
        e = new(p, x => p.SavedState = x.State);
        await e.Step(j, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<IOException>(() => e.Step(j, TestContext.Current.CancellationToken));
        string id = j.ActionId;
        await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal(1, p.Builds); Assert.Equal(id, Assert.Single(j.Finished).ActionId);
        Assert.Equal(1, j.RegionIndex);
    }

    [Fact]
    public async Task Each_region_clears_once_with_separate_receipts_and_shared_budget() {
        var j = Job(Spec() with { Clearing = "all_vegetation", MaxClearTargets = 2,
            AdditionalRegions = [new(20, 1, 3, 8, 8)] });
        var p = new Port { ClearEachRegion = true, MissingPlan = (s, i) => i == 1 && s.X == 1 };
        var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        for (int i = 0; i < 32 && !j.Terminal; i++) await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("completed", j.State); Assert.Equal(2, p.Marks); Assert.Equal(2, p.Builds);
        Assert.Equal(1, Assert.Single(j.PreviousRegions).Targets);
        Assert.Single(j.Targets); Assert.Equal(2, j.PreviousClearTargets + j.Targets.Length);
    }

    [Fact]
    public async Task Clearance_cap_is_shared_and_run_ids_are_distinct_per_region() {
        var s = Spec() with { Clearing = "all_vegetation", MaxClearTargets = 1,
            AdditionalRegions = [new(20, 1, 3, 8, 8)] };
        var j = Job(s); string firstRun = j.ClearanceRunId;
        j.RegionIndex = 1; j.PreviousRegions.Add(new(0, "no_candidate_in_authorized_region", true, 1));
        j.State = "discovering";
        Assert.NotEqual(firstRun, j.ClearanceRunId);
        var p = new Port { Objects = [new(Guid.NewGuid(), "vegetation", "Pine", new(21, 2, 3),
            "demolition_mark", true, false, "natural", "tree")] };
        await new BuildBatchEngine(p, _ => { }).Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("clearance_limit_or_unsupported_target", j.Reason); Assert.Equal(0, p.Marks);
    }

    [Fact]
    public void Region_bounds_total_time_and_legacy_fingerprints_are_preserved() {
        Assert.Throws<ArgumentException>(() => BuildBatchState.Validate(Spec() with { AdditionalRegions =
            [new(20, 1, 3, 8, 8), new(30, 1, 3, 8, 8), new(40, 1, 3, 8, 8), new(50, 1, 3, 8, 8)] }, true));
        Assert.Throws<ArgumentException>(() => BuildBatchState.Validate(Spec() with { AdditionalRegions = [new(20, 1, 3, 9, 8)] }, true));
        Assert.Throws<ArgumentException>(() => BuildBatchState.Validate(Spec() with { AdditionalRegions = [new(1, 1, 3, 8, 8)] }, true));
        Assert.Throws<ArgumentException>(() => BuildBatchState.Validate(Spec() with { ClearanceHours = 168,
            AdditionalRegions = [new(20, 1, 3, 8, 8), new(30, 1, 3, 8, 8), new(40, 1, 3, 8, 8)] }, true));
        var json = JsonSerializer.SerializeToElement(Spec(), NativeJson.Options);
        Assert.False(json.TryGetProperty("additionalRegions", out _));
        var legacy = json.Deserialize<BuildBatchSpec>(BuildBatchState.Json)!;
        Assert.Equal(BuildBatchState.Fingerprint(Spec()), BuildBatchState.Fingerprint(legacy));
        Assert.NotEqual(BuildBatchState.Fingerprint(Spec()), BuildBatchState.Fingerprint(Spec() with { AdditionalRegions = [new(20, 1, 3, 8, 8)] }));
        Assert.Equal(BuildBatchState.DerivedId(Spec(), "clearance"), Job().ClearanceRunId);
    }

    [Fact]
    public async Task Sequential_builds_choose_fewer_paths_and_stop_after_completion() {
        var p = new Port(); var j = Job(); var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        for (int i = 0; i < 8; i++) await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("completed", j.State); Assert.Equal(2, p.Builds); Assert.Equal(2, p.Runs);
        Assert.Equal(2, j.Finished.Count); Assert.NotEqual(j.Finished[0].ActionId, j.Finished[1].ActionId);
    }
    [Fact]
    public async Task Lost_build_ack_recovers_without_second_submission() {
        var p = new Port { LoseBuildAck = true }; var j = Job();
        var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        await Assert.ThrowsAsync<IOException>(() => e.Step(j, TestContext.Current.CancellationToken));
        Assert.Equal("pending_build", j.State);
        e = new(p, x => p.SavedState = x.State);
        await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal(1, p.Builds); Assert.Single(j.Finished);
    }
    [Fact]
    public async Task Explicit_rejection_is_terminal_and_never_retried() {
        var p = new Port { RejectBuild = true }; var j = Job();
        var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        await e.Step(j, TestContext.Current.CancellationToken);
        await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("stopped", j.State); Assert.Equal("build_rejected:state_conflict", j.Reason);
        Assert.Equal(1, p.Builds); Assert.Equal(0, p.Runs);
    }
    [Fact]
    public async Task Lost_clearance_ack_reads_absence_without_repeating_mark() {
        var p = new Port { NoPlan = true, LoseMarkAck = true, Objects = [new(Guid.Parse("44444444-4444-4444-8444-444444444444"),
            "vegetation", "Pine", new(2, 2, 3), "demolition_mark", true, false, "natural", "tree")] };
        var j = Job(Spec() with { Clearing = "all_vegetation", MaxClearTargets = 4 });
        var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        await e.Step(j, TestContext.Current.CancellationToken); await e.Step(j, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<IOException>(() => e.Step(j, TestContext.Current.CancellationToken));
        await e.Step(j, TestContext.Current.CancellationToken); await e.Step(j, TestContext.Current.CancellationToken); await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("planning", j.State); Assert.Equal(1, p.Marks); Assert.Equal(0, p.Runs);
    }
    [Fact]
    public async Task Missing_materials_stops_before_any_write() {
        var p = new Port { Failure = "materials_missing" }; var j = Job();
        var e = new BuildBatchEngine(p, _ => { }); await e.Step(j, TestContext.Current.CancellationToken); await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("stopped", j.State); Assert.Equal(0, p.Builds); Assert.Equal(0, p.Marks);
    }
    [Fact]
    public async Task Unknown_access_stops_before_next_building() {
        var p = new Port { CompletionFailure = "access_unproven" }; var j = Job();
        var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        await e.Step(j, TestContext.Current.CancellationToken); await e.Step(j, TestContext.Current.CancellationToken); await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal("stopped", j.State); Assert.Equal("access_unproven", j.Reason);
        Assert.Equal(1, p.Builds); Assert.Equal(0, p.Runs); Assert.Empty(j.Finished);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Clearance_budget_never_restarts_after_lost_ack_or_exhaustion(bool removed) {
        var p = new Port { KeepMarked = true, LoseRunAck = true, Objects = [new(Guid.Parse("44444444-4444-4444-8444-444444444444"),
            "vegetation", "Pine", new(2, 2, 3), "demolition_mark", true, true, "natural", "tree")] };
        var j = Job(Spec() with { Clearing = "all_vegetation", MaxClearTargets = 4 });
        j.Targets = p.Objects; j.ClearanceUsed = true; j.RemovalCursor = 1; j.State = "verify_clearance";
        var e = new BuildBatchEngine(p, x => p.SavedState = x.State);
        await Assert.ThrowsAsync<IOException>(() => e.Step(j, TestContext.Current.CancellationToken));
        Assert.True(await e.Step(j, TestContext.Current.CancellationToken));
        Assert.Equal(1, p.ClearanceStarts); Assert.Equal(0, p.Builds);
        p.ClearanceDone = true; if (removed) p.Objects = [];
        await e.Step(j, TestContext.Current.CancellationToken);
        Assert.Equal(removed ? "planning" : "stopped", j.State);
        if (!removed) { await e.Step(j, TestContext.Current.CancellationToken); Assert.Equal("clearance_budget_exhausted", j.Reason); }
        Assert.Equal(1, p.ClearanceStarts);
    }
    [Fact]
    public void Bounds_and_duplicate_json_rejected() {
        Assert.Throws<ArgumentException>(() => BuildBatchState.Validate(Spec() with { ConstructionHours = 168, Items = Enumerable.Repeat(new BatchBuilding("Lodge.Folktails"), 8).ToArray() }, true));
        Assert.Throws<ArgumentException>(() => BuildBatchState.Validate(Spec() with { Rotations = [0, 0] }, true));
        using var doc = JsonDocument.Parse("{\"session\":\"a\",\"session\":\"b\"}");
        Assert.Throws<ArgumentException>(() => BuildBatchState.Parse<BatchHandle>(doc.RootElement));
    }
    [Fact]
    public async Task Durable_replay_conflict_and_stop_have_no_additional_writes() {
        string dir = Path.Combine(Path.GetTempPath(), "timberborn-batch-test-" + Guid.NewGuid().ToString("N"));
        try {
            var s = Spec(); var p = new Port(); var store = new BuildBatchStore(dir);
            // A saved pre-dispatch checkpoint survives a new store/engine instance.
            using (store.Acquire()) store.Save(Job(s));
            var input = JsonSerializer.SerializeToElement(new BatchStartRequest(s, 0), NativeJson.Options);
            await BuildBatchTools.Execute(BuildBatchTools.Start, input, true, true, true, new(dir), p, TestContext.Current.CancellationToken);
            Assert.Equal(0, p.Builds);
            var changed = JsonSerializer.SerializeToElement(new BatchStartRequest(s with { ConstructionHours = 48 }, 0), NativeJson.Options);
            await Assert.ThrowsAsync<ArgumentException>(() => BuildBatchTools.Execute(BuildBatchTools.Start, changed, true, true, true, store, p, TestContext.Current.CancellationToken));
            var handle = JsonSerializer.SerializeToElement(new BatchHandle(s.Session, s.BatchId), NativeJson.Options);
            var compact = await BuildBatchTools.Execute(BuildBatchTools.Inspect, handle, false, false, false, store, p, TestContext.Current.CancellationToken);
            Assert.Null(compact["data"]!["search"]);
            Assert.True(compact["data"]!["detailsAvailable"]!.GetValue<bool>());
            var detailedHandle = JsonSerializer.SerializeToElement(new BatchHandle(s.Session, s.BatchId, Details: true), NativeJson.Options);
            var detailed = await BuildBatchTools.Execute(BuildBatchTools.Inspect, detailedHandle, false, false, false, store, p, TestContext.Current.CancellationToken);
            Assert.NotNull(detailed["data"]!["search"]);
            await BuildBatchTools.Execute(BuildBatchTools.Stop, handle, false, false, false, store, p, TestContext.Current.CancellationToken);
            await BuildBatchTools.Execute(BuildBatchTools.Advance, handle, true, true, true, store, p, TestContext.Current.CancellationToken);
            Assert.Equal("cancelled", store.Load(s.Session, s.BatchId)!.State); Assert.Equal(0, p.Builds);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
}
