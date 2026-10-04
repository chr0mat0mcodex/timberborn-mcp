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
        public bool NoPlan, LoseBuildAck, LoseMarkAck, KeepMarked, ClearanceDone, LoseRunAck;
        public int ClearanceStarts;
        public string? Failure;
        public string? CompletionFailure;
        public string SavedState = "";
        public NativeRemovalTarget[] Objects = [];
        public Task<NativeSimulation> Current(BuildBatchSpec s, CancellationToken ct) => Task.FromResult(new NativeSimulation(0, 1, 0, 0));
        public Task<string?> Prerequisite(BuildBatchSpec s, int i, CancellationToken ct) => Task.FromResult(Failure);
        public Task<NativeBuildingPlan> Plan(BuildBatchSpec s, int i, int r, CancellationToken ct) => Task.FromResult(new NativeBuildingPlan(
            s.Items[i].Template, s.DistrictId, new(s.X, s.Y, s.Z), s.Width, s.Height, r, 1, 0, 1, true, "complete",
            NoPlan ? [] : [new(new(2, 2, 3), r, new(2, 3, 3), new(2, 4, 3), r == 0 ? [new(2, 3, 3)] : [], [], false, [], new string('a', 64))], []));
        public Task<NativeRemovalTarget[]> Targets(BuildBatchSpec s, CancellationToken ct) => Task.FromResult(Objects);
        public Task<NativeRemoval> Mark(BuildBatchSpec s, NativeRemovalTarget t, CancellationToken ct) {
            Assert.Equal("pending_mark", SavedState); Marks++;
            Objects = KeepMarked ? [t with { Marked = true }] : []; NoPlan = KeepMarked;
            if (LoseMarkAck) throw new IOException();
            return Task.FromResult(new NativeRemoval(t.Id, t.Kind, t.Template, t.Position, "mark", "applied", true, null, []));
        }
        public Task<NativeSimulationRun> ClearanceRun(BuildBatchJob j, bool start, CancellationToken ct) {
            if (start) { Assert.Equal("pending_clearance_run", SavedState); ClearanceStarts++; if (LoseRunAck) throw new IOException(); }
            return Task.FromResult(new NativeSimulationRun(j.ClearanceRunId, ClearanceDone ? "completed" : "running", "",
                0, j.Spec.ClearanceHours, ClearanceDone ? j.Spec.ClearanceHours : 0, ClearanceDone ? j.Spec.ClearanceHours : 0,
                0, 1, j.Spec.MaxRealSeconds, j.Spec.Speed, ClearanceDone ? 0 : j.Spec.Speed, ClearanceDone, ClearanceDone, ClearanceDone));
        }
        public Task<NativeProjectExecution> Submit(BuildBatchJob j, CancellationToken ct) {
            Assert.Equal("pending_build", SavedState); Assert.Equal(1, j.Selection!.Rotation); Builds++;
            if (LoseBuildAck) throw new IOException();
            return Task.FromResult(new NativeProjectExecution(j.ActionId, new string('a', 64), "completed", "", [], false, []));
        }
        public Task<CompletionReport> Completion(BuildBatchJob j, bool advance, CancellationToken ct) {
            if (advance) { Assert.Equal("pending_build_run", SavedState); Runs++; }
            bool finished = Runs > j.Index;
            return Task.FromResult(new CompletionReport(j.ActionId, "run", CompletionFailure ?? (finished ? "finished_accessible" : "awaiting_simulation"), "",
                "completed", "", [new(j.ActionId, j.Spec.Items[j.Index].Template, true, finished, null, null)], null, null, 0, null, []));
        }
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
            await BuildBatchTools.Execute(BuildBatchTools.Stop, handle, false, false, false, store, p, TestContext.Current.CancellationToken);
            await BuildBatchTools.Execute(BuildBatchTools.Advance, handle, true, true, true, store, p, TestContext.Current.CancellationToken);
            Assert.Equal("cancelled", store.Load(s.Session, s.BatchId)!.State); Assert.Equal(0, p.Builds);
        } finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
}
