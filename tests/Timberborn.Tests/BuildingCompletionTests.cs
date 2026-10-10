using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuildingCompletionTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string Action = "22222222-2222-4222-8222-222222222222";
    private static CompletionRequest Request(bool advance = true) => new(Session, Action, 24, 7, 300, 0, advance);
    private static BridgeEnvelope<T> Envelope<T>(T data) => new(1, Session, DateTimeOffset.UnixEpoch, "0.35.3", data);

    private sealed class Fixture
    {
        public NativeSimulationRun? Run;
        public int Starts, Reads, Cancels;
        public bool TimeoutAfterCancel, DeferredPause, FinishDuringDelay;
        public bool Finished, Missing, TimeoutAfterStart, FailRead, ChangedSession, PathUnfinished;
        public bool? Builders = true, EntranceBlocked = false;
        public bool? UnconnectedBlocked = false;
        public int? UnconnectedBlockerCount;
        public float Speed;
        public string ProjectState = "completed";
        public bool FinishOnPoll, Configured, ChangedConfiguration;
        public Task<JsonObject> Invoke(CompletionRequest r) => BuildingCompletionTools.Execute(r,
            (q, ct) => Task.FromResult(Envelope(new NativeProjectExecution(Action, new string('a', 64), ProjectState, "test",
                PathUnfinished
                    ? [new("33333333-3333-4333-8333-333333333333", "Path", 1, 0, 3, 0, "confirmed"), new(Action, "SmallWarehouse.Folktails", 1, 1, 3, 0, "confirmed")]
                    : [new(Action, "SmallWarehouse.Folktails", 1, 1, 3, 0, "confirmed")], false, []) {
                        InitialConfiguration = Configured ? new() { Good = "Carrot", Mode = "obtain", State = "confirmed" } : null })),
            (q, ct) => {
                bool path = q.EntityId != Action;
                var e = Envelope(new NativeBuilding(Guid.Parse(q.EntityId), !Missing, Missing ? null :
                    new(path ? "Path" : "SmallWarehouse.Folktails", path ? new(1, 0, 3) : new(1, 1, 3),
                        path ? false : Finished, path || !Finished, true, null, new(false, null, null, null)), []));
                return Task.FromResult(ChangedSession ? e with { SessionId = "44444444-4444-4444-8444-444444444444" } : e);
            },
            (q, ct) => Task.FromResult(Envelope(new NativeAccess(Action, Finished, new(1, 1, 3), EntranceBlocked,
                false, UnconnectedBlocked, Finished ? null : Builders, 1, 1, 1, [], UnconnectedBlockerCount: UnconnectedBlockerCount))),
            (q, ct) => Task.FromResult(Envelope(new NativeBuildingSettings(Guid.Parse(Action), "SmallWarehouse.Folktails", new(1, 1, 3), Finished,
                null, true, new(ChangedConfiguration ? "Berries" : "Carrot", true, [], "obtain", true, 30, [], false), false, null, []))),
            (q, values, ct) => {
                if (q.Route == "simulation-run-cancel") {
                    Cancels++;
                    Assert.Equal(r.RunId, q.RunId);
                    Run = Running(r) with { State = DeferredPause ? "pausing" : "cancelled", Reason = "cancel_requested",
                        ObservedSpeed = DeferredPause ? 7 : 0, PauseConfirmed = !DeferredPause, Terminal = !DeferredPause };
                    Speed = DeferredPause ? 7 : 0;
                    if (TimeoutAfterCancel) throw new HttpRequestException();
                    return Task.FromResult(Envelope(Run));
                }
                if (!q.Starts) {
                    Reads++;
                    if (FailRead) throw new HttpRequestException();
                    if (Run is null) throw new BridgeRejectionException("run_not_found");
                    if (FinishOnPoll) { Finished = true; Speed = 0; Run = Completed(r); }
                    return Task.FromResult(Envelope(Run));
                }
                Starts++;
                Assert.Equal(r.RunId, q.RunId);
                Assert.Equal(0, q.ExpectedSpeed);
                Run = new(r.RunId, "running", "advancing", 0, 24, 1, 1, 0, 1, 300, 7, 7, false, false, false);
                Speed = 7;
                if (TimeoutAfterStart) throw new HttpRequestException();
                return Task.FromResult(Envelope(Run));
            },
            ct => Task.FromResult(Envelope(new NativeSimulation(Speed, 0, 0, 0))),
            (t, ct) => { if (FinishDuringDelay) Finished = true; return Task.CompletedTask; }, TestContext.Current.CancellationToken);
    }

    private static NativeSimulationRun Completed(CompletionRequest r) => new(r.RunId, "completed", "target_reached",
        0, 24, 24, 24, 0, 12, 300, 7, 0, true, true, true);
    private static NativeSimulationRun Running(CompletionRequest r) => new(r.RunId, "running", "advancing",
        0, 24, 1, 1, 0, 1, 300, 7, 7, false, false, false);
    private static string Outcome(JsonObject result) => result["data"]!["outcome"]!.GetValue<string>();

    [Fact]
    public async Task FinishedProjectPausesItsRunBeforeTimeCeilingButInspectDoesNotMutate()
    {
        var f = new Fixture { Finished = true, Run = Running(Request()), Speed = 7 };
        Assert.Equal("running", Outcome(await f.Invoke(Request(false))));
        Assert.Equal(0, f.Cancels);
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request())));
        Assert.Equal(1, f.Cancels); Assert.Equal(0, f.Starts);
        Assert.False(f.Run!.TargetReached);
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request())));
        Assert.Equal(1, f.Cancels);
    }

    [Fact]
    public async Task CompletionDuringPollingStopsEarlyWithoutWaitingForTimeTarget()
    {
        var f = new Fixture { FinishDuringDelay = true };
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request() with { WaitSeconds = 2 })));
        Assert.Equal(1, f.Starts); Assert.Equal(1, f.Cancels);
        Assert.Equal(1d, f.Run!.ElapsedGameHours);
    }

    [Fact]
    public async Task EarlyPauseNeedsConfirmationAndLostAcknowledgementRecoversByReading()
    {
        var f = new Fixture { Finished = true, Run = Running(Request()), Speed = 7, DeferredPause = true };
        Assert.Equal("running", Outcome(await f.Invoke(Request())));
        Assert.Equal("running", Outcome(await f.Invoke(Request())));
        Assert.Equal(1, f.Cancels);
        f.Run = f.Run! with { State = "cancelled", ObservedSpeed = 0, PauseConfirmed = true, Terminal = true }; f.Speed = 0;
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request(false))));

        f = new Fixture { Finished = true, Run = Running(Request()), Speed = 7, TimeoutAfterCancel = true };
        Assert.Equal("simulation_unconfirmed", Outcome(await f.Invoke(Request())));
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request(false))));
        Assert.Equal(1, f.Cancels); Assert.Equal(0, f.Starts);
    }

    [Fact]
    public async Task EarlyPauseDoesNotBypassAccessOrFinishUnfinishedPaths()
    {
        var f = new Fixture { Finished = true, Run = Running(Request()), Speed = 7, EntranceBlocked = true };
        Assert.Equal("access_unproven", Outcome(await f.Invoke(Request())));
        Assert.Equal(1, f.Cancels);
        f = new Fixture { Finished = true, PathUnfinished = true, Run = Running(Request()), Speed = 7 };
        Assert.Equal("running", Outcome(await f.Invoke(Request())));
        Assert.Equal(0, f.Cancels);
        f = new Fixture { Run = Running(Request()) with { State = "cancelled", Reason = "cancel_requested", Terminal = true,
            PauseConfirmed = true, ObservedSpeed = 0 }, Speed = 0 };
        Assert.Equal("simulation_stopped", Outcome(await f.Invoke(Request())));
        Assert.Equal(0, f.Starts);
    }

    [Fact]
    public async Task DisconnectResumesSameRunWithoutAnotherStart()
    {
        var f = new Fixture { TimeoutAfterStart = true };
        Assert.Equal("simulation_unconfirmed", Outcome(await f.Invoke(Request())));
        Assert.Equal("running", Outcome(await f.Invoke(Request(false))));
        Assert.Equal("running", Outcome(await f.Invoke(Request())));
        Assert.Equal(1, f.Starts);
        f.Finished = true; f.Speed = 0; f.Run = Completed(Request());
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request(false))));
        Assert.Equal(1, f.Starts);
    }

    [Fact]
    public async Task BudgetNeverExtendsAndChangedParametersAreRejected()
    {
        var f = new Fixture { Run = Completed(Request()) };
        Assert.Equal("budget_exhausted", Outcome(await f.Invoke(Request())));
        Assert.Equal("budget_conflict", Outcome(await f.Invoke(Request() with { DurationHours = 48 })));
        Assert.Equal(0, f.Starts);
    }

    [Fact]
    public async Task BoundedPollReturnsActualFinishedAccessProof()
    {
        var f = new Fixture { FinishOnPoll = true };
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request() with { WaitSeconds = 2 })));
        Assert.Equal(1, f.Starts);
        Assert.Equal(2, f.Reads);
    }

    [Fact]
    public async Task InspectionDoesNotStartTimeAndFinishedProjectNeedsNoTime()
    {
        var f = new Fixture();
        Assert.Equal("awaiting_simulation", Outcome(await f.Invoke(Request(false))));
        f.Finished = true;
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request())));
        Assert.Equal(0, f.Starts);
    }

    [Theory]
    [InlineData("unknown", "access_unproven")]
    [InlineData("blocked", "access_unproven")]
    [InlineData("missing", "entity_missing")]
    [InlineData("path", "access_unproven")]
    [InlineData("speed", "not_paused")]
    [InlineData("order", "order_stopped")]
    public async Task PreconditionsBlockBeforeSimulation(string failure, string outcome)
    {
        var f = new Fixture();
        if (failure == "unknown") f.Builders = null;
        if (failure == "blocked") f.Builders = false;
        if (failure == "missing") f.Missing = true;
        if (failure == "path") f.PathUnfinished = true;
        if (failure == "speed") f.Speed = 3;
        if (failure == "order") f.ProjectState = "unconfirmed";
        Assert.Equal(outcome, Outcome(await f.Invoke(Request())));
        Assert.Equal(0, f.Starts);
    }

    [Fact]
    public async Task TransportFailureAndSessionChangeAreNotAbsence()
    {
        var f = new Fixture { FailRead = true };
        await Assert.ThrowsAsync<HttpRequestException>(() => f.Invoke(Request()));
        f.FailRead = false; f.ChangedSession = true;
        await Assert.ThrowsAsync<InvalidDataException>(() => f.Invoke(Request()));
        Assert.Equal(0, f.Starts);
    }

    [Fact]
    public async Task FinishedButBlockedIsNotSuccessAndStoppedRunDoesNotRestart()
    {
        var f = new Fixture { Finished = true, EntranceBlocked = true };
        Assert.Equal("access_unproven", Outcome(await f.Invoke(Request())));
        f.Run = Completed(Request()) with { State = "interrupted" };
        Assert.Equal("simulation_stopped", Outcome(await f.Invoke(Request())));
        Assert.Equal(0, f.Starts);
    }

    [Fact]
    public async Task StrictInputsAndIndependentGates()
    {
        var args = JsonSerializer.SerializeToElement(new { session = Session, actionId = Action,
            durationHours = 24, speed = 7, maxRealSeconds = 300, waitSeconds = 20 });
        Assert.Equal(Request().RunId, BuildingCompletionTools.Parse(BuildingCompletionTools.Advance, args).RunId);
        Assert.NotEqual(Request().RunId, (Request() with { ActionId = "55555555-5555-4555-8555-555555555555" }).RunId);
        var invalid = JsonNode.Parse(args.GetRawText())!; invalid["durationHours"] = 169;
        Assert.Throws<ArgumentException>(() => BuildingCompletionTools.Parse(BuildingCompletionTools.Advance, JsonSerializer.SerializeToElement(invalid)));
        Assert.Throws<ArgumentException>(() => BuildingCompletionTools.Parse(BuildingCompletionTools.Inspect, args));
        Assert.Single(BuildingCompletionTools.Catalog(false));
        Assert.True(BuildingCompletionTools.Catalog(false).Single().Annotations!.ReadOnlyHint);
        await Assert.ThrowsAsync<ArgumentException>(() => BuildingCompletionTools.Invoke(null!, BuildingCompletionTools.Advance, args, false, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task InitialConfigurationMustStillMatchAtCompletion()
    {
        var f = new Fixture { Configured = true, Finished = true };
        Assert.Equal("finished_accessible", Outcome(await f.Invoke(Request(false))));
        f.ChangedConfiguration = true;
        Assert.NotEqual("finished_accessible", Outcome(await f.Invoke(Request(false))));
        Assert.Equal(0, f.Starts);
    }

    [Theory]
    [InlineData(null, null, "access_unproven")]
    [InlineData(null, 0, "finished_accessible")]
    [InlineData(false, 1, "finished_accessible")]
    [InlineData(true, 1, "access_unproven")]
    public async Task AbsentOptionalBlockerRequiresExplicitComponentEvidence(bool? blocked, int? count, string outcome)
    {
        var f = new Fixture { Finished = true, Run = Completed(Request()), UnconnectedBlocked = blocked, UnconnectedBlockerCount = count };
        Assert.Equal(outcome, Outcome(await f.Invoke(Request(false))));
        Assert.Equal(0, f.Starts);
    }

    [Fact]
    public void DevelopmentValidatesBothPhasesAndSettingsGateBeforeStart()
    {
        var args = JsonSerializer.SerializeToElement(new { session = Session, actionId = Action, mode = "development_pilot",
            selection = "first_candidate", template = "SmallWarehouse.Folktails", districtId = "33333333-3333-4333-8333-333333333333",
            x = 1, y = 1, z = 3, width = 2, height = 2, rotation = 0, initialStorageGood = "Carrot",
            durationHours = 24, speed = 7, maxRealSeconds = 300, waitSeconds = 0 });
        var parsed = BuildingCompletionTools.ParseDevelopment(args, true);
        Assert.Equal("Carrot", parsed.Build["initialStorageGood"]);
        Assert.Equal(Request().RunId, parsed.Completion.RunId);
        Assert.Throws<ArgumentException>(() => BuildingCompletionTools.ParseDevelopment(args, false));
        var invalid = JsonNode.Parse(args.GetRawText())!; invalid["speed"] = 2;
        Assert.Throws<ArgumentException>(() => BuildingCompletionTools.ParseDevelopment(JsonSerializer.SerializeToElement(invalid), true));
        Assert.Equal(3, BuildingCompletionTools.Catalog(true).Count());
    }

    [Theory]
    [InlineData(BuildingCompletionTools.Advance, "waitSeconds", 21)]
    [InlineData(BuildingCompletionTools.Develop, "waitSeconds", 21)]
    [InlineData(BuildingCompletionTools.Advance, "speed", 2)]
    [InlineData(BuildingCompletionTools.Develop, "speed", 2)]
    public async Task InvalidInputReportsFieldWithoutTouchingClient(string tool, string field, int value)
    {
        var args = new JsonObject {
            ["session"] = Session, ["actionId"] = Action, ["durationHours"] = 24,
            ["speed"] = 7, ["maxRealSeconds"] = 300, ["waitSeconds"] = 0
        };
        if (tool == BuildingCompletionTools.Develop) {
            args["mode"] = "development_pilot"; args["selection"] = "first_candidate";
            args["template"] = "SmallWarehouse.Folktails";
            args["districtId"] = "33333333-3333-4333-8333-333333333333";
            args["x"] = 1; args["y"] = 1; args["z"] = 3;
            args["width"] = 2; args["height"] = 2; args["rotation"] = 0;
        }
        args[field] = value;
        // A null client makes any accidental access fail instead of hiding a request in a mock.
        var result = await BuildingCompletionTools.Invoke(null!, tool,
            JsonSerializer.SerializeToElement(args), true, TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        var message = result["error"]!["message"]!.GetValue<string>();
        Assert.Contains(field, message);
        Assert.Contains("kein Bau-/Simulationsauftrag gesendet", message);
        Assert.DoesNotContain("unbestätigt", message);
    }
}
