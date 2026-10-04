using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class WorkflowTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";
    private const string Action = "22222222-2222-4222-8222-222222222222";
    private static BridgeEnvelope<T> Envelope<T>(T data) => new(1, Session, DateTimeOffset.UnixEpoch, "0.35.3", data);
    private static JsonElement Batch(int count = 3) => JsonSerializer.SerializeToElement(new {
        session = Session, operation = "mark", targets = Enumerable.Range(1, count).Select(i => new {
            id = $"00000000-0000-4000-8000-{i:D12}", template = "Pine", x = i, y = 2, z = 3, expectedMarked = false }) });
    private static JsonElement Build() => JsonSerializer.SerializeToElement(new {
        session = Session, actionId = Action, mode = "development_pilot", selection = "first_candidate",
        template = "SmallWarehouse.Folktails", districtId = "33333333-3333-4333-8333-333333333333",
        x = 0, y = 0, z = 3, width = 4, height = 4, rotation = 0,
        initialStorageGood = "Carrot", initialStorageMode = "obtain" });
    private static BridgeEnvelope<NativeRemoval> Removed(RemovalRequest r, bool pending = false, string outcome = "applied") => Envelope(
        new NativeRemoval(Guid.Parse(r.Id), "vegetation", r.Template, new(r.X, r.Y, r.Z), r.Operation, outcome, !pending, pending ? true : null, []));
    private static BridgeEnvelope<NativeBuildingPlan> Plan(BuildingPlanRequest p, bool empty = false) => Envelope(new NativeBuildingPlan(
        p.Template, p.DistrictId, new(p.X, p.Y, p.Z), p.Width, p.Height, p.Rotation, 16, empty ? 16 : 15, 16, true, "area_exhausted",
        empty ? [] : [new(new(1, 1, 3), 0, new(1, 0, 3), new(1, 0, 3), [], [new(1, 0, 3)], false, [], new string('a', 64))], []));

    [Fact]
    public void EveryTargetIsParsedBeforeExecutionAndDuplicateIdsAreRejected()
    {
        Assert.Equal(16, WorkflowTools.ParseRemoval(Batch(16)).Length);
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseRemoval(Batch(0)));
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseRemoval(Batch(17)));
        var invalid = JsonNode.Parse(Batch().GetRawText())!;
        invalid["targets"]![2]!["expectedMarked"] = "false";
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseRemoval(JsonSerializer.SerializeToElement(invalid)));
        var duplicate = JsonNode.Parse(Batch().GetRawText())!;
        duplicate["targets"]![1]!["id"] = duplicate["targets"]![0]!["id"]!.GetValue<string>();
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseRemoval(JsonSerializer.SerializeToElement(duplicate)));
        using var repeatedProperty = JsonDocument.Parse(Batch(1).GetRawText().Replace("\"x\":1", "\"x\":1,\"x\":1"));
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseRemoval(repeatedProperty.RootElement));
    }

    [Fact]
    public async Task BatchReportsRemovedAndStillPendingSeparately()
    {
        int calls = 0;
        var result = await WorkflowTools.Remove(WorkflowTools.ParseRemoval(Batch()), (r, ct) => {
            calls++; return Task.FromResult(Removed(r, calls == 2));
        }, TestContext.Current.CancellationToken);
        Assert.Equal(3, calls);
        Assert.Equal("ok", result["status"]!.GetValue<string>());
        Assert.Equal(1, result["data"]!["pendingWorkerOrders"]!.GetValue<int>());
        Assert.Equal(3, result["data"]!["applied"]!.GetValue<int>());
        Assert.True(result["data"]!["items"]![0]!["removed"]!.GetValue<bool>());
        Assert.False(result["data"]!["items"]![1]!["removed"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("conflict", "rejected")]
    [InlineData("timeout", "unconfirmed")]
    [InlineData("receipt", "unconfirmed")]
    [InlineData("malformed", "unconfirmed")]
    public async Task PartialBatchStopsOnFirstFailureAndKeepsEarlierEvidence(string failure, string outcome)
    {
        int calls = 0;
        var result = await WorkflowTools.Remove(WorkflowTools.ParseRemoval(Batch()), (r, ct) => {
            calls++;
            if (calls == 2) {
                if (failure == "conflict") throw new BridgeRejectionException("state_conflict");
                if (failure == "timeout") throw new HttpRequestException("private message");
                if (failure == "malformed") throw new InvalidDataException("private message");
                return Task.FromResult(Removed(r, true, "unconfirmed"));
            }
            return Task.FromResult(Removed(r));
        }, TestContext.Current.CancellationToken);
        Assert.Equal(2, calls);
        Assert.Equal("error", result["status"]!.GetValue<string>());
        Assert.Equal("applied", result["data"]!["items"]![0]!["outcome"]!.GetValue<string>());
        Assert.Equal(outcome, result["data"]!["items"]![1]!["outcome"]!.GetValue<string>());
        Assert.Equal("not_attempted", result["data"]!["items"]![2]!["outcome"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.DoesNotContain("private message", result.ToJsonString());
    }

    [Fact]
    public async Task CancellationBetweenItemsDoesNotSendAnotherMutation()
    {
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        int calls = 0;
        var result = await WorkflowTools.Remove(WorkflowTools.ParseRemoval(Batch()), (r, ct) => {
            calls++; cancel.Cancel(); return Task.FromResult(Removed(r));
        }, cancel.Token);
        Assert.Equal(1, calls);
        Assert.Equal("not_attempted", result["data"]!["items"]![1]!["outcome"]!.GetValue<string>());
        Assert.Equal(1, result["data"]!["applied"]!.GetValue<int>());
    }

    [Fact]
    public void BuildRequiresExplicitSelectionAndRetainsSettingsGate()
    {
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseBuild(Build(), false));
        var args = JsonNode.Parse(Build().GetRawText())!;
        args["selection"] = "try_every_candidate";
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseBuild(JsonSerializer.SerializeToElement(args), true));
        args["selection"] = "first_candidate";
        args["width"] = "4";
        Assert.Throws<ArgumentException>(() => WorkflowTools.ParseBuild(JsonSerializer.SerializeToElement(args), true));
        var q = WorkflowTools.ParseBuild(Build(), true);
        Assert.Equal("Carrot", q["initialStorageGood"]);
        Assert.Equal("obtain", q["initialStorageMode"]);
    }

    [Fact]
    public async Task NoCandidateNeverDispatchesExecution()
    {
        int writes = 0;
        var result = await WorkflowTools.Start(WorkflowTools.ParseBuild(Build(), true),
            (r, ct) => Task.FromResult(Plan(r, true)), (r, ct) => {
                writes++; throw new InvalidOperationException("must not run");
            }, TestContext.Current.CancellationToken);
        Assert.Equal(0, writes);
        Assert.Equal("not_started", result["data"]!["outcome"]!.GetValue<string>());
        Assert.False(result["data"]!["requestSubmitted"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FirstCandidateUsesExistingNativeExecutionAndNeverFallsBack(bool conflict)
    {
        int writes = 0;
        var result = await WorkflowTools.Start(WorkflowTools.ParseBuild(Build(), true),
            (r, ct) => Task.FromResult(Plan(r)), (r, ct) => {
                writes++;
                Assert.Equal(new string('a', 64), r.Validation!.PlanKey);
                Assert.Equal(0, r.Validation.OptionIndex);
                Assert.Equal("Carrot", r.InitialStorageGood);
                Assert.Equal("obtain", r.InitialStorageMode);
                Assert.Equal(Action, r.ActionId.ToString("D"));
                if (conflict) throw new BridgeRejectionException("state_conflict");
                return Task.FromResult(Envelope(new NativeProjectExecution(Action, r.Validation.PlanKey,
                    "running", "", [new(Action, r.Validation.Plan.Template, 1, 1, 3, 0, "pending")], false, [])));
            }, TestContext.Current.CancellationToken);
        Assert.Equal(1, writes);
        Assert.Equal(conflict ? "error" : "ok", result["status"]!.GetValue<string>());
        if (!conflict) {
            Assert.Equal("running", result["data"]!["execution"]!["state"]!.GetValue<string>());
            Assert.False(result["data"]!["execution"]!["constructionPreflightProven"]!.GetValue<bool>());
        }
    }

    [Fact]
    public async Task BuildTimeoutPreservesActionAndSelectedCandidateWithoutRetry()
    {
        int writes = 0;
        var result = await WorkflowTools.Start(WorkflowTools.ParseBuild(Build(), true),
            (r, ct) => Task.FromResult(Plan(r)), (r, ct) => {
                writes++; throw new HttpRequestException("private message");
            }, TestContext.Current.CancellationToken);
        Assert.Equal(1, writes);
        Assert.Equal(Action, result["data"]!["actionId"]!.GetValue<string>());
        Assert.Equal("unconfirmed", result["data"]!["outcome"]!.GetValue<string>());
        Assert.NotNull(result["data"]!["selectedOption"]);
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
    }

    [Fact]
    public async Task UnconfirmedNativeProjectIsAnErrorWithOriginalEvidence()
    {
        var result = await WorkflowTools.Start(WorkflowTools.ParseBuild(Build(), true),
            (r, ct) => Task.FromResult(Plan(r)), (r, ct) => Task.FromResult(Envelope(
                new NativeProjectExecution(Action, r.Validation!.PlanKey, "unconfirmed", "access_unknown",
                    [new(Action, r.Validation.Plan.Template, 1, 1, 3, 0, "unconfirmed")], false, []))),
            TestContext.Current.CancellationToken);
        Assert.Equal("error", result["status"]!.GetValue<string>());
        Assert.Equal("access_unknown", result["data"]!["execution"]!["reason"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
    }

    [Fact]
    public async Task DisabledToolsRejectBeforeAccessAndCatalogUsesIndependentGates()
    {
        Assert.Empty(WorkflowTools.Catalog(false, false));
        Assert.Equal(WorkflowTools.Removal, Assert.Single(WorkflowTools.Catalog(true, false)).Name);
        Assert.Equal(WorkflowTools.Build, Assert.Single(WorkflowTools.Catalog(false, true)).Name);
        foreach (var tool in WorkflowTools.Catalog(true, true)) {
            Assert.False(tool.Annotations!.ReadOnlyHint);
            Assert.True(tool.Annotations.DestructiveHint);
            Assert.False(tool.Annotations.IdempotentHint);
        }
        await Assert.ThrowsAsync<ArgumentException>(() => WorkflowTools.Invoke(null!, WorkflowTools.Removal,
            Batch(), false, false, false, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => WorkflowTools.Invoke(null!, WorkflowTools.Build,
            Build(), false, false, true, TestContext.Current.CancellationToken));
    }
}
