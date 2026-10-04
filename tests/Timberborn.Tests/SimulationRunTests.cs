using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class SimulationRunTests
{
    [Fact]
    public void GameLockDoesNotAbandonRunOrRestartSpeed()
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(24.1, 3, 1, commands.Add);
        c.Tick(24.2, 0, 2, commands.Add, true);
        Assert.False(r.Terminal); Assert.Equal("game_speed_locked", r.Reason);
        c.Tick(24.2, 0, 140, commands.Add, true);
        c.Tick(24.2, 0, 141, commands.Add); // unlocked event precedes LateUpdate speed restoration
        Assert.False(r.Terminal);
        c.Tick(24.3, 3, 142, commands.Add);
        Assert.Equal("running", r.State); Assert.Single(commands); Assert.Equal(26, r.TargetGameHours);
        c.Tick(26, 3, 150, commands.Add); c.Tick(26, 0, 151, commands.Add);
        Assert.Equal("completed", r.State); Assert.Equal(new[] { 3, 0 }, commands);
    }

    [Fact]
    public void GameLockDoesNotExtendRealTimeBudgetOrFalselyConfirmFinalPause()
    {
        var q = Query(); q["maxRealSeconds"] = "30";
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(q), 24, 0, 0, commands.Add);
        c.Tick(24.1, 3, 1, commands.Add);
        c.Tick(24.2, 0, 31, commands.Add, true);
        Assert.Equal("pausing", r.State); Assert.False(r.Terminal); Assert.Single(commands);
        c.Tick(24.2, 0, 40, commands.Add, true);
        c.Tick(24.2, 3, 41, commands.Add);
        Assert.False(r.Terminal); Assert.Equal(new[] { 3, 0 }, commands);
        c.Tick(24.2, 0, 42, commands.Add);
        Assert.Equal("failed", r.State); Assert.Equal("real_time_limit", r.Reason);
    }

    [Fact]
    public void CancelDuringLockWaitsForUnlockWithoutRestartingSimulation()
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(24.1, 3, 1, commands.Add); c.Tick(24.2, 0, 2, commands.Add, true);
        c.Cancel(r.RunId, 24.2, 0, 3, commands.Add, true);
        c.Tick(24.2, 0, 4, commands.Add, true);
        Assert.Single(commands); Assert.False(r.Terminal);
        c.Tick(24.2, 3, 5, commands.Add); c.Tick(24.2, 0, 6, commands.Add);
        Assert.Equal("cancelled", r.State); Assert.Equal(new[] { 3, 0 }, commands);
    }

    [Fact]
    public void UnlockedManualPauseStillInterruptsWithoutResumeCommand()
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(24.1, 3, 1, commands.Add); c.Tick(24.2, 0, 2, commands.Add);
        Assert.True(r.Terminal); Assert.Equal("speed_changed", r.Reason); Assert.Single(commands);
    }
    private static NameValueCollection Query(string duration = "2", string unit = "hours", int speed = 3) => new() {
        ["session"] = "11111111-1111-4111-8111-111111111111", ["runId"] = Guid.NewGuid().ToString("D"),
        ["duration"] = duration, ["unit"] = unit, ["speed"] = speed.ToString(CultureInfo.InvariantCulture), ["expectedSpeed"] = "0", ["maxRealSeconds"] = "7200" };
    private static SimulationRunRequest Parse(NameValueCollection q, string route = "simulation-run-for") => SimulationRunRequest.Parse("/agent-api/v1/" + route, q);

    [Theory]
    [InlineData(1, "hours", 2)] [InlineData(3, "hours", 2)] [InlineData(7, "hours", 2)]
    [InlineData(7, "days", 48)] [InlineData(7, "weeks", 336)]
    public void TargetAndDelayedPauseAreBasedOnGameClock(int speed, string unit, double elapsed)
    {
        var c = new SimulationRunController(); var commands = new List<int>();
        var r = c.Start(Parse(Query("2", unit, speed)), 239, 0, 0, commands.Add);
        c.Tick(239.1, speed, 1, commands.Add); Assert.Equal("running", r.State);
        c.Tick(239 + elapsed + 0.01, speed, 10, commands.Add);
        Assert.Equal("pausing", r.State); Assert.False(r.Terminal); Assert.False(r.PauseConfirmed);
        c.Tick(239 + elapsed + 0.02, 0, 11, commands.Add);
        Assert.Equal("completed", r.State); Assert.True(r.Terminal); Assert.True(r.PauseConfirmed);
        Assert.Equal(0.02, r.OvershootHours, 8); Assert.Equal(new[] { speed, 0 }, commands);
    }

    [Fact]
    public void AbsoluteTargetUsesMonotonicDayAndRejectsPastAndTooDistantTargets()
    {
        var q = Query(); q.Remove("duration"); q.Remove("unit"); q.Add("dayNumber", "11"); q.Add("hour", "1.5");
        var r = Parse(q, "simulation-run-until");
        var c = new SimulationRunController(); var s = c.Start(r, 263, 0, 0, _ => { });
        Assert.Equal(265.5, s.TargetGameHours);
        Assert.Throws<BridgeRejectionException>(() => new SimulationRunController().Start(r, 266, 0, 0, _ => { }));
        q["dayNumber"] = "100";
        Assert.Throws<BridgeRejectionException>(() => new SimulationRunController().Start(Parse(q, "simulation-run-until"), 1, 0, 0, _ => { }));
    }

    [Theory]
    [InlineData("duration", "0")] [InlineData("duration", "NaN")] [InlineData("duration", "Infinity")]
    [InlineData("duration", "1,5")] [InlineData("duration", "-1")] [InlineData("duration", "673")]
    [InlineData("speed", "0")] [InlineData("speed", "2")] [InlineData("expectedSpeed", "2")]
    [InlineData("maxRealSeconds", "29")] [InlineData("maxRealSeconds", "86401")]
    [InlineData("unit", "minutes")] [InlineData("runId", "invalid")]
    public void InvalidRequestsAreRejected(string key, string value) { var q = Query(); q[key] = value; Assert.Throws<ArgumentException>(() => Parse(q)); }

    [Fact]
    public void DuplicateUnknownParametersAndTooManyWeeksRejected()
    {
        var q = Query(); q.Add("speed", "3"); Assert.Throws<ArgumentException>(() => Parse(q));
        q = Query(); q.Add("other", "1"); Assert.Throws<ArgumentException>(() => Parse(q));
        Assert.Throws<ArgumentException>(() => Parse(Query("5", "weeks")));
    }

    [Fact]
    public void ConcurrentAndReusedIdsNeverIssueAnotherCommand()
    {
        var c = new SimulationRunController(); var commands = new List<int>(); var r = Parse(Query());
        c.Start(r, 24, 0, 0, commands.Add);
        Assert.Equal("run_id_used", Assert.Throws<BridgeRejectionException>(() => c.Start(r, 24, 0, 1, commands.Add)).Code);
        Assert.Equal("state_conflict", Assert.Throws<BridgeRejectionException>(() => c.Start(Parse(Query()), 24, 0, 1, commands.Add)).Code);
        c.Interrupt();
        Assert.Throws<BridgeRejectionException>(() => c.Start(r, 24, 0, 2, commands.Add)); Assert.Single(commands);
    }

    [Fact]
    public void ManualSpeedChangeIsPreservedAndTerminalStatusFrozen()
    {
        var c = new SimulationRunController(); var commands = new List<int>(); var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(24.1, 3, 1, commands.Add); c.Tick(24.2, 1, 2, commands.Add);
        Assert.Equal("interrupted", r.State); Assert.Single(commands);
        c.Tick(25, 7, 30, commands.Add); Assert.Equal(24.2, r.ObservedGameHours);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CancellationWaitsForPauseAndDoesNotRepeatCommand(bool pauseApplies)
    {
        var c = new SimulationRunController(); var commands = new List<int>(); var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(24.1, 3, 1, commands.Add);
        c.Cancel(r.RunId, 24.1, 3, 2, commands.Add); c.Cancel(r.RunId, 24.2, 3, 3, commands.Add);
        Assert.Equal("pausing", r.State); Assert.Equal(2, commands.Count);
        c.Tick(24.3, pauseApplies ? 0 : 3, 7, commands.Add);
        Assert.Equal(pauseApplies ? "cancelled" : "failed", r.State);
        Assert.Equal(pauseApplies ? "cancel_requested" : "pause_unconfirmed", r.Reason);
    }

    [Theory]
    [InlineData("deadline")] [InlineData("stalled")] [InlineData("reversed")] [InlineData("blocked")]
    public void FailureRequestsPauseAndDoesNotClaimCompletion(string scenario)
    {
        var q = Query(); if (scenario == "deadline") q["maxRealSeconds"] = "30";
        var c = new SimulationRunController(); var commands = new List<int>(); var r = c.Start(Parse(q), 24, 0, 0, commands.Add);
        if (scenario != "blocked") c.Tick(24.1, 3, 1, commands.Add);
        double h = scenario == "reversed" ? 24 : scenario == "blocked" ? 24 : 24.1;
        double t = scenario == "stalled" ? 122 : scenario == "deadline" ? 31 : 6;
        c.Tick(h, scenario == "blocked" ? 0 : 3, t, commands.Add);
        Assert.Equal("pausing", r.State); c.Tick(h, 0, t + 1, commands.Add);
        Assert.Equal("failed", r.State); Assert.True(r.PauseConfirmed); Assert.False(r.TargetReached);
        Assert.Equal(new[] { 3, 0 }, commands);
    }

    [Fact]
    public void AlreadyReachedTargetStillNeedsPauseConfirmation()
    {
        var q = Query(); q.Remove("duration"); q.Remove("unit"); q.Add("dayNumber", "1"); q.Add("hour", "0");
        var c = new SimulationRunController(); var commands = new List<int>(); var r = c.Start(Parse(q, "simulation-run-until"), 24, 0, 0, commands.Add);
        Assert.Equal("pausing", r.State); c.Tick(24, 0, 1, commands.Add); Assert.Equal("completed", r.State); Assert.Equal(new[] { 0 }, commands);
    }

    [Fact]
    public void UnloadRequestsPauseWithoutClaimingConfirmation()
    {
        var c = new SimulationRunController(); var commands = new List<int>(); var r = c.Start(Parse(Query()), 24, 0, 0, commands.Add);
        c.Tick(24.1, 3, 1, commands.Add); c.Unload(commands.Add);
        Assert.Equal("session_ended_pause_unconfirmed", r.Reason); Assert.False(r.PauseConfirmed); Assert.Equal(new[] { 3, 0 }, commands);
    }

    [Theory]
    [InlineData("simulation-run-for")] [InlineData("simulation-run-until")] [InlineData("simulation-run-cancel")]
    public void WritesNeedIndependentOptInAndPost(string route)
    {
        string path = "/agent-api/v1/" + route;
        Assert.False(BridgeHttpServer.MethodAllowed("GET", path, false, true, enableSpeedControl:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST", path, false, true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST", path, true, true, enableSpeedControl:true));
        Assert.True(BridgeHttpServer.MethodAllowed("POST", path, false, true, enableSpeedControl:true));
    }

    [Fact]
    public async Task TransportFailureOnStartIsNotRetryable()
    {
        var handler = new BrokenHandler();
        using var client = new NativeClient(new NativeConfiguration(8081, new string('a', 64)), handler);
        using var tools = new NativeTools(client, enableSpeedControl:true);
        var q = Query(); var args = JsonSerializer.SerializeToElement(q.AllKeys.ToDictionary(k => k!, k => k is "session" or "runId" or "unit" ? (object)q[k]! : decimal.Parse(q[k]!, CultureInfo.InvariantCulture)));
        var result = await tools.Invoke("run_simulation_for", args, TestContext.Current.CancellationToken);
        Assert.Equal("backend_unavailable", result["error"]!["code"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>()); Assert.Equal(1, handler.Calls);
    }
    private sealed class BrokenHandler : HttpMessageHandler {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) { Calls++; throw new HttpRequestException(); }
    }

    [Theory]
    [InlineData("valid")] [InlineData("session")] [InlineData("id")] [InlineData("false_completion")]
    [InlineData("elapsed")] [InlineData("target")] [InlineData("missing")]
    public async Task ReceiptValidationRejectsInconsistentEvidence(string defect)
    {
        var q = Query(); var r = Parse(q);
        var status = new NativeSimulationRun(r.RunId, "starting", "awaiting_speed", 24, 26, 24, 0, 0, 0, 7200, 3, 0, true, false, false);
        var node = JsonSerializer.SerializeToNode(new BridgeEnvelope<NativeSimulationRun>(1,r.Session,DateTimeOffset.UtcNow,"0.22.0",status),NativeJson.Options)!;
        switch (defect) {
            case "session": node["sessionId"]=Guid.NewGuid().ToString("D"); break;
            case "id": node["data"]!["runId"]=Guid.NewGuid().ToString("D"); break;
            case "false_completion": node["data"]!["state"]="completed"; node["data"]!["terminal"]=true; break;
            case "elapsed": node["data"]!["elapsedGameHours"]=2; break;
            case "target": node["data"]!["targetGameHours"]=27; break;
            case "missing": node["data"]!.AsObject().Remove("pauseConfirmed"); break;
        }
        using var client = new NativeClient(new NativeConfiguration(8081,new string('a',64)),new ResponseHandler(node.ToJsonString()));
        if (defect=="valid") Assert.Equal("starting",(await client.SimulationRun(r,q,TestContext.Current.CancellationToken)).Data.State);
        else if (defect=="missing") await Assert.ThrowsAsync<JsonException>(()=>client.SimulationRun(r,q,TestContext.Current.CancellationToken));
        else await Assert.ThrowsAsync<InvalidDataException>(()=>client.SimulationRun(r,q,TestContext.Current.CancellationToken));
    }
    private sealed class ResponseHandler(string body) : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Assert.Equal(HttpMethod.Post,request.Method); Assert.Null(request.Content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content=new StringContent(body) });
        }
    }
}
