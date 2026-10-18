using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class CompletionSpeedTests
{
    private static NameValueCollection Query(string? completion = "1")
    {
        var q = new NameValueCollection {
            ["session"]="11111111-1111-4111-8111-111111111111", ["runId"]=Guid.NewGuid().ToString("D"),
            ["duration"]="2", ["unit"]="hours", ["speed"]="3", ["expectedSpeed"]="0", ["maxRealSeconds"]="30"
        };
        if (completion is not null) q["completionSpeed"] = completion;
        return q;
    }
    private static SimulationRunRequest Parse(NameValueCollection q) => SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for", q);

    [Theory]
    [InlineData(null)] [InlineData("0")] [InlineData("1")]
    public void PauseThenOptionalHandoffRetainsOwnerUntilObserved(string? completion)
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query(completion)), 24, 0, 0, commands.Add);
        c.Tick(26, 3, 1, commands.Add);
        Assert.Equal("pausing", r.State); Assert.False(r.Terminal);
        c.Tick(26, 0, 2, commands.Add);
        if (completion == "1") {
            Assert.Equal("completing", r.State); Assert.True(r.CompletionPauseObserved);
            Assert.True(r.PauseConfirmed); Assert.False(r.CompletionSpeedConfirmed);
            Assert.Equal(r.RunId, c.ActiveRunId);
            Assert.Throws<BridgeRejectionException>(() => c.Start(Parse(Query()), 26, 0, 2, commands.Add));
            c.Tick(26.1, 1, 3, commands.Add);
            Assert.False(r.PauseConfirmed); Assert.Equal(1, r.EffectiveCompletionSpeed);
            Assert.Equal(new[] { 3, 0, 1 }, commands);
        } else {
            Assert.True(r.PauseConfirmed); Assert.Equal(0, r.EffectiveCompletionSpeed);
            Assert.Equal(new[] { 3, 0 }, commands);
        }
        Assert.Equal("completed", r.State); Assert.True(r.CompletionSpeedConfirmed); Assert.Null(c.ActiveRunId);
        int count = commands.Count;
        c.Cancel(r.RunId, 29, 7, 9, commands.Add); // Historical terminal cancel never pauses current gameplay.
        Assert.Equal(count, commands.Count);
    }

    [Theory]
    [InlineData("-1")] [InlineData("2")] [InlineData("3")] [InlineData("7")] [InlineData("0.5")] [InlineData("true")]
    public void InvalidOptionRejected(string value) => Assert.Throws<ArgumentException>(() => Parse(Query(value)));

    [Theory]
    [InlineData("stopAtAvailableLogs", "1")] [InlineData("goodTargets", "Log:1")]
    [InlineData("stopAtScience", "1")] [InlineData("buildingIds", "22222222-2222-4222-8222-222222222222")]
    [InlineData("measureProduction", "false")]
    public void ExplicitOptionIncludingZeroCannotEnterOtherFamilies(string key, string value)
    {
        foreach (string option in new[] { "0", "1" }) {
            var q = Query(option); q[key] = value;
            Assert.Throws<ArgumentException>(() => Parse(q));
        }
    }

    [Fact]
    public void UntilStatusAndDuplicateOptionRejected()
    {
        var q = Query(); q.Remove("duration"); q.Remove("unit"); q["dayNumber"]="2"; q["hour"]="0";
        Assert.Throws<ArgumentException>(() => SimulationRunRequest.Parse("/agent-api/v1/simulation-run-until", q));
        foreach (string route in new[] { "simulation-run", "simulation-run-cancel" }) {
            var read = new NameValueCollection { ["session"]=q["session"], ["runId"]=q["runId"], ["completionSpeed"]="1" };
            Assert.Throws<ArgumentException>(() => SimulationRunRequest.Parse("/agent-api/v1/" + route, read));
        }
        q = Query(); q.Add("completionSpeed", "1"); Assert.Throws<ArgumentException>(() => Parse(q));
    }

    [Theory]
    [InlineData("running")] [InlineData("pausing")] [InlineData("completing")]
    public void CancelReplacesSuccessAndQueuedAcceleration(string phase)
    {
        var c = new SimulationRunController(); var queue = new DeferredSpeedCommand();
        var r = c.Start(Parse(Query()), 24, 0, 0, queue.Request);
        queue.Apply(_ => { }); queue.ObserveNextUpdate();
        c.Tick(24.1, 3, 1, queue.Request);
        if (phase != "running") c.Tick(26, 3, 2, queue.Request);
        if (phase == "completing") {
            queue.Apply(_ => { }); queue.ObserveNextUpdate(); c.Tick(26, 0, 3, queue.Request);
            Assert.Equal(1, queue.PendingSpeed);
        }
        c.Cancel(r.RunId, 26, phase == "completing" ? 0 : 3, 4, queue.Request);
        Assert.Equal(0, queue.PendingSpeed);
        var dispatch = new List<int>(); queue.Apply(dispatch.Add); queue.ObserveNextUpdate();
        c.Tick(26, 0, 5, queue.Request);
        Assert.Equal(new[] { 0 }, dispatch); Assert.Equal("cancelled", r.State);
        Assert.True(r.PauseConfirmed); Assert.Equal(0, r.EffectiveCompletionSpeed); Assert.Null(c.ActiveRunId);
    }

    [Fact]
    public void CancelAfterHandoffDispatchWaitsForLateUpdateThenPauses()
    {
        var c = new SimulationRunController(); var queue = new DeferredSpeedCommand();
        var r = c.Start(Parse(Query()), 24, 0, 0, queue.Request);
        queue.Apply(_ => { }); c.Tick(26, 3, 1, queue.Request);
        queue.Apply(_ => { }); c.Tick(26, 0, 2, queue.Request);
        int? late = null; queue.Apply(s => late = s);
        c.Cancel(r.RunId, 26, 0, 2.1, queue.Request);
        Assert.False(r.Terminal); Assert.Equal(1, late);
        // SpeedManager applies the submitted 1 before the queued cancellation 0.
        float speed = late!.Value; queue.ObserveNextUpdate(); queue.Apply(s => speed = s);
        queue.ObserveNextUpdate(); c.Tick(26.1, speed, 3, queue.Request);
        Assert.Equal("cancelled", r.State); Assert.True(r.PauseConfirmed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LockPermanentlyDiscardsOptionEvenBetweenUpdates(bool completing)
    {
        var c = new SimulationRunController(); var queue = new DeferredSpeedCommand();
        var r = c.Start(Parse(Query()), 24, 0, 0, queue.Request);
        queue.Apply(_ => { }); c.Tick(completing ? 26 : 24.1, 3, 1, queue.Request);
        if (completing) { queue.Apply(_ => { }); c.Tick(26, 0, 2, queue.Request); }
        c.CompletionLocked(3, queue.Request); // Event can occur without a locked Tick.
        if (c.IsPausing && queue.PendingSpeed is > 0) queue.Clear();
        c.Tick(26, completing ? 1 : 3, 4, queue.Request);
        Assert.Equal(0, queue.PendingSpeed);
        queue.Apply(_ => { }); queue.ObserveNextUpdate(); c.Tick(26, 0, 5, queue.Request);
        Assert.Equal("completed", r.State); Assert.True(r.PauseConfirmed);
        Assert.Equal("game_speed_locked", r.CompletionFallbackReason); Assert.Equal(0, r.EffectiveCompletionSpeed);
    }

    [Theory]
    [InlineData("budget")] [InlineData("timeout")] [InlineData("dispatch_delay")]
    [InlineData("command_failure")] [InlineData("observation")]
    public void FailedHandoffNeverRetriesPositiveCommand(string failure)
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(26, 3, 1, commands.Add);
        c.Tick(26, 0, failure == "budget" ? 26 : 2, commands.Add);
        if (failure == "timeout") c.Tick(26, 0, 7, commands.Add);
        if (failure == "dispatch_delay") Assert.False(c.PrepareCompletionDispatch(26, 0, 7, commands.Add));
        if (failure == "command_failure") c.SpeedCommandFailed(3, commands.Add);
        if (failure == "observation") c.Tick(double.NaN, 0, 3, commands.Add);
        if (!r.Terminal) c.Tick(26, 0, 8, commands.Add);
        Assert.Equal("failed", r.State); Assert.True(r.PauseConfirmed); Assert.NotNull(r.CompletionFallbackReason);
        Assert.Equal(failure == "budget" ? 0 : 1, commands.Count(s => s == 1));
        Assert.Null(c.ActiveRunId);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void InvalidOrReversedPauseObservationCannotResume(bool reversed)
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(26, 3, 1, commands.Add);
        c.Tick(reversed ? 25 : double.NaN, 0, 2, commands.Add);
        if (!r.Terminal) c.Tick(26, 0, 3, commands.Add);
        Assert.Equal("failed", r.State); Assert.DoesNotContain(1, commands);
    }

    [Fact]
    public void CompletionFallbackChangesStatusTokenWithoutTimeProgress()
    {
        var report = new JsonObject { ["data"] = new JsonObject { ["state"]="running", ["completionSpeed"]=1 } };
        string before = StatusWaitTools.ChangeToken("inspect_simulation_run", report);
        report["data"]!["completionFallbackReason"]="game_speed_locked";
        Assert.NotEqual(before, StatusWaitTools.ChangeToken("inspect_simulation_run", report));
    }

    [Fact]
    public void DeadlineWinsOverLateConfirmationAndFallbackCanPauseSpeedOne()
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(26, 3, 24, commands.Add); c.Tick(26, 0, 25, commands.Add);
        c.Tick(26.1, 1, 30, commands.Add);
        Assert.Equal("pausing", r.State); Assert.False(r.CompletionSpeedConfirmed);
        c.Tick(26.1, 1, 30.1, commands.Add); Assert.False(r.Terminal);
        c.Tick(26.1, 0, 31, commands.Add); Assert.Equal("failed", r.State);
    }

    [Theory]
    [InlineData("explicit")] [InlineData("observed")] [InlineData("unload")]
    public void SupersededCompletionCannotLeaveQueuedResume(string cause)
    {
        var c = new SimulationRunController(); var queue = new DeferredSpeedCommand();
        var r = c.Start(Parse(Query()), 24, 0, 0, queue.Request);
        queue.Apply(_ => { }); c.Tick(26, 3, 1, queue.Request);
        queue.Apply(_ => { }); c.Tick(26, 0, 2, queue.Request);
        if (cause == "explicit") { c.Interrupt(); queue.Request(0); } // Same-value explicit MCP command also supersedes.
        if (cause == "observed") {
            Assert.False(c.PrepareCompletionDispatch(26, 7, 3, queue.Request));
            if (!c.HasActiveRun) queue.Clear();
        }
        if (cause == "unload") { queue.Clear(); c.Unload(queue.Request); }
        Assert.Equal("interrupted", r.State); Assert.NotEqual(1, queue.PendingSpeed);
    }

    [Fact]
    public void CatalogAndResultTextDescribeUnmonitoredContinuation()
    {
        var catalog = SimulationRunTools.Catalog(true).ToArray();
        foreach (var tool in catalog) {
            bool exists = tool.InputSchema.GetProperty("properties").TryGetProperty("completionSpeed", out var property);
            Assert.Equal(tool.Name == "run_simulation_for", exists);
            if (exists) {
                Assert.Equal(new[] { 0, 1 }, property.GetProperty("enum").EnumerateArray().Select(x => x.GetInt32()));
                Assert.Equal(0, property.GetProperty("default").GetInt32());
                Assert.DoesNotContain(tool.InputSchema.GetProperty("required").EnumerateArray(), x => x.GetString() == "completionSpeed");
            }
        }
        var report = new JsonObject { ["status"]="ok", ["data"]=JsonSerializer.SerializeToNode(Receipt(Parse(Query())), NativeJson.Options) };
        string text = ActivityOutcome.Describe("inspect_simulation_run", report);
        Assert.Contains("Tempo 1 bestätigt", text); Assert.Contains("ohne Laufbudget und Überwachung", text);
        Assert.DoesNotContain("Pause bestätigt", text);
    }

    private static NativeSimulationRun Receipt(SimulationRunRequest r) => new(r.RunId, "completed", "target_reached",
        24, 26, 26, 2, 0, 3, 30, 3, 1, false, true, true,
        BuildingIds: [], BuildingProduction: [], CompletionSpeed: 1, EffectiveCompletionSpeed: 1,
        CompletionSpeedConfirmed: true, CompletionPauseObserved: true);

    [Theory]
    [InlineData("valid")] [InlineData("missing_option")] [InlineData("missing_pause")]
    [InlineData("missing_confirmation")] [InlineData("fallback")]
    [InlineData("wrong_speed")] [InlineData("budget")] [InlineData("target")]
    public async Task NativeReceiptRequiresCompleteHandoffEvidence(string defect)
    {
        var q = Query(); var request = Parse(q); var status = Receipt(request);
        status = defect switch {
            "missing_option" => status with { CompletionSpeed = 0 },
            "missing_pause" => status with { CompletionPauseObserved = false },
            "missing_confirmation" => status with { CompletionSpeedConfirmed = false },
            "fallback" => status with { CompletionFallbackReason = "game_speed_locked" },
            "wrong_speed" => status with { ObservedSpeed = 3 },
            "budget" => status with { ElapsedRealSeconds = 30 },
            "target" => status with { TargetReached = false },
            _ => status
        };
        var payload = JsonSerializer.Serialize(new BridgeEnvelope<NativeSimulationRun>(1, request.Session,
            DateTimeOffset.UnixEpoch, "0.74.0", status), NativeJson.Options);
        using var client = new NativeClient(new NativeConfiguration(8081, new string('a', 64)), new Response(payload));
        if (defect == "valid") Assert.Equal(1, (await client.SimulationRun(request, q, TestContext.Current.CancellationToken)).Data.EffectiveCompletionSpeed);
        else await Assert.ThrowsAsync<InvalidDataException>(() => client.SimulationRun(request, q, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LostResponseIsNotRetriedAndSameRunCanBeInspected()
    {
        var c = new SimulationRunController(); var request = Parse(Query());
        var r = c.Start(request, 24, 0, 0, _ => { });
        c.Tick(26, 3, 1, _ => { }); c.Tick(26, 0, 2, _ => { }); c.Tick(26, 1, 3, _ => { });
        Assert.Same(r, c.Inspect(request.RunId));
        Assert.Throws<BridgeRejectionException>(() => c.Start(request, 26, 1, 4, _ => { }));
        var handler = new BrokenResponse();
        using var client = new NativeClient(new NativeConfiguration(8081, new string('a', 64)), handler);
        using var tools = new NativeTools(client, enableSpeedControl: true);
        var q = Query(); var args = JsonSerializer.SerializeToElement(q.AllKeys.ToDictionary(k => k!, k =>
            k is "session" or "runId" or "unit" ? (object)q[k]! : int.Parse(q[k]!)));
        var result = await tools.Invoke("run_simulation_for", args, TestContext.Current.CancellationToken);
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>()); Assert.Equal(1, handler.Calls);
    }
    [Theory]
    [InlineData("starting")] [InlineData("pausing")] [InlineData("completing")]
    [InlineData("completed")] [InlineData("cancelled")] [InlineData("locked")]
    public async Task ControllerStatesRoundTripThroughNativeContract(string phase)
    {
        var q = Query(); var request = Parse(q); var c = new SimulationRunController();
        var r = c.Start(request, 24, 0, 0, _ => { });
        if (phase != "starting") c.Tick(26, 3, 1, _ => { });
        if (phase is "completing" or "completed" or "cancelled") c.Tick(26, 0, 2, _ => { });
        if (phase == "completed") c.Tick(26, 1, 3, _ => { });
        if (phase == "cancelled") { c.Cancel(r.RunId, 26, 0, 3, _ => { }); c.Tick(26, 0, 4, _ => { }); }
        if (phase == "locked") { c.CompletionLocked(2, _ => { }); c.Tick(26, 0, 3, _ => { }); }
        var payload = JsonSerializer.Serialize(new BridgeEnvelope<SimulationRunStatus>(1, request.Session,
            DateTimeOffset.UnixEpoch, "0.74.0", r), NativeJson.Options);
        using var client = new NativeClient(new NativeConfiguration(8081, new string('a', 64)), new Response(payload));
        var read = new NameValueCollection { ["session"] = request.Session, ["runId"] = request.RunId };
        var inspect = SimulationRunRequest.Parse("/agent-api/v1/simulation-run", read);
        Assert.Equal(r.State, (await client.SimulationRun(inspect, read, TestContext.Current.CancellationToken)).Data.State);
    }

    private sealed class Response(string payload) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload) });
    }
    private sealed class BrokenResponse : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) {
            Calls++; throw new HttpRequestException();
        }
    }
}
