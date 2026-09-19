using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Application;
using Timberborn.Backend.Abstractions;
using Timberborn.Backend.Fake;
using Timberborn.Backend.MoreHttpApi;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuildingActionTests
{
    private static readonly Guid Id = FakeTimberbornBackend.BuildingId;
    private static JsonElement Args(bool target = true, bool expected = false) =>
        JsonSerializer.SerializeToElement(new { id = Id, paused = target, expectedPaused = expected });
    private static void Outcome(JsonObject result, string outcome) => Assert.Equal(outcome, result["data"]!["outcome"]!.GetValue<string>());

    // Stateful HTTP stub: exercises real mapping, membership validation and transport with no game.
    private sealed class Game
    {
        public bool Paused;
        public bool Pausable = true;
        public bool Missing;
        public bool Unknown;
        public bool IgnoreWrite;
        public int Writes;
        public int Reads;
        public int? WriteStatus;
        public bool LoseResponse;
        public bool FailReadback;
        public bool DelayWrite;
        public bool CancelWrite;
        public StubHandler Handler() => new(async (r, ct) =>
        {
            if (r.RequestUri!.AbsolutePath.EndsWith("/toggle-pause", StringComparison.Ordinal))
            {
                Assert.Equal($"/MoreHttpApi/buildings/{Id:D}/toggle-pause", r.RequestUri.AbsolutePath);
                Assert.Contains(r.RequestUri.Query, new[] { "?paused=true", "?paused=false" });
                Writes++;
                if (DelayWrite) await Task.Delay(10000, ct);
                if (CancelWrite) throw new OperationCanceledException(ct);
                if (WriteStatus is int status) return new((HttpStatusCode)status);
                if (!IgnoreWrite) Paused = r.RequestUri.Query == "?paused=true";
                if (LoseResponse) throw new HttpRequestException("PRIVATE");
                return new(HttpStatusCode.NoContent);
            }
            Reads++;
            if (Writes > 0 && FailReadback) return new(HttpStatusCode.Unauthorized);
            var building = JsonNode.Parse(BackendTests.Building)!;
            building["Pausable"]!["Pausable"] = Pausable;
            building["Pausable"]!["IsPaused"] = Paused;
            if (Unknown) building["Pausable"] = null;
            string body = r.RequestUri.AbsolutePath.EndsWith("/buildings", StringComparison.Ordinal)
                ? "{\"Groups\":[{\"Buildings\":[" + (Missing ? "" : building.ToJsonString()) + "]}]}"
                : building.ToJsonString();
            return new(HttpStatusCode.OK) { Content = new StringContent(body) };
        });
    }

    [Fact]
    public async Task DisabledAtServiceAndTransportNeverSends()
    {
        var game = new Game(); using var handler = game.Handler();
        using var backend = new MoreHttpApiBackend(new(), handler);
        Outcome(await new BuildingActionService(backend, backend).InvokeAsync(Args(), TestContext.Current.CancellationToken), "rejected");
        await Assert.ThrowsAsync<BackendException>(() => backend.SetBuildingPausedAsync(Id, true, TestContext.Current.CancellationToken));
        Assert.Empty(handler.Routes);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{\"id\":\"bad\",\"paused\":true,\"expectedPaused\":false}")]
    [InlineData("{\"id\":\"00000000-0000-0000-0000-000000000001\",\"paused\":\"true\",\"expectedPaused\":false}")]
    [InlineData("{\"id\":\"00000000-0000-0000-0000-000000000001\",\"paused\":true,\"expectedPaused\":false,\"url\":\"/other\"}")]
    public async Task InvalidArgumentsDoNotReadOrWrite(string json)
    {
        var game = new Game(); using var backend = new MoreHttpApiBackend(new() { EnableWrites = true }, game.Handler());
        var result = await new BuildingActionService(backend, backend).InvokeAsync(JsonDocument.Parse(json).RootElement, TestContext.Current.CancellationToken);
        Outcome(result, "rejected"); Assert.Equal(0, game.Reads); Assert.Equal(0, game.Writes);
    }

    [Theory]
    [InlineData("missing", "rejected")]
    [InlineData("not-pausable", "rejected")]
    [InlineData("unknown", "rejected")]
    [InlineData("conflict", "rejected")]
    [InlineData("noop", "unchanged")]
    public async Task PreconditionsPreventMutation(string scenario, string outcome)
    {
        var game = new Game { Missing = scenario == "missing", Pausable = scenario != "not-pausable",
            Unknown = scenario == "unknown", Paused = scenario == "conflict" };
        using var backend = new MoreHttpApiBackend(new() { EnableWrites = true }, game.Handler());
        Outcome(await new BuildingActionService(backend, backend).InvokeAsync(Args(scenario != "noop"), TestContext.Current.CancellationToken), outcome);
        Assert.Equal(0, game.Writes);
    }

    [Fact]
    public async Task BothTransitionsAreVerifiedWithExactlyOneRequestEach()
    {
        var game = new Game(); using var backend = new MoreHttpApiBackend(new() { EnableWrites = true }, game.Handler());
        var service = new BuildingActionService(backend, backend);
        Outcome(await service.InvokeAsync(Args(), TestContext.Current.CancellationToken), "applied");
        Assert.True(game.Paused); Assert.Equal(1, game.Writes); Assert.Equal(4, game.Reads);
        Outcome(await service.InvokeAsync(Args(false, true), TestContext.Current.CancellationToken), "applied");
        Assert.False(game.Paused); Assert.Equal(2, game.Writes); Assert.Equal(8, game.Reads);
    }

    [Theory]
    [InlineData("lost")]
    [InlineData("readback")]
    [InlineData("ignored")]
    [InlineData("timeout")]
    [InlineData("cancel")]
    public async Task UncertainOutcomesNeverRetryOrRestore(string failure)
    {
        var game = new Game { LoseResponse = failure == "lost", FailReadback = failure == "readback",
            IgnoreWrite = failure == "ignored", DelayWrite = failure == "timeout", CancelWrite = failure == "cancel" };
        using var backend = new MoreHttpApiBackend(new() { EnableWrites = true, RequestTimeout = TimeSpan.FromMilliseconds(100) }, game.Handler());
        var result = await new BuildingActionService(backend, backend).InvokeAsync(Args(), TestContext.Current.CancellationToken);
        Outcome(result, "unconfirmed"); Assert.Equal(1, game.Writes);
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.DoesNotContain("PRIVATE", result.ToJsonString());
    }

    [Theory]
    [InlineData(401, "rejected")]
    [InlineData(403, "rejected")]
    [InlineData(404, "unconfirmed")]
    [InlineData(500, "unconfirmed")]
    [InlineData(302, "unconfirmed")]
    [InlineData(200, "unconfirmed")]
    public async Task WriteHttpErrorsAreConservative(int status, string outcome)
    {
        var game = new Game { WriteStatus = status };
        using var backend = new MoreHttpApiBackend(new() { EnableWrites = true }, game.Handler());
        Outcome(await new BuildingActionService(backend, backend).InvokeAsync(Args(), TestContext.Current.CancellationToken), outcome);
        Assert.Equal(1, game.Writes);
    }

    [Fact]
    public async Task ConcurrentConflictingRequestsCannotBothWrite()
    {
        var backend = new FakeTimberbornBackend(enableWrites: true);
        var first = new BuildingActionService(backend, backend);
        var second = new BuildingActionService(backend, backend);
        var results = await Task.WhenAll(Task.Run(() => first.InvokeAsync(Args(), TestContext.Current.CancellationToken)),
            Task.Run(() => second.InvokeAsync(Args(), TestContext.Current.CancellationToken)));
        Assert.Single(results, r => r["data"]!["outcome"]!.GetValue<string>() == "applied");
        Assert.Single(results, r => r["data"]!["outcome"]!.GetValue<string>() == "rejected");
    }

    [Fact]
    public async Task CancellationBeforeSendDoesNotMutate()
    {
        var game = new Game(); using var backend = new MoreHttpApiBackend(new() { EnableWrites = true }, game.Handler());
        using var cts = new CancellationTokenSource(); cts.Cancel();
        Outcome(await new BuildingActionService(backend, backend).InvokeAsync(Args(), cts.Token), "rejected");
        Assert.Equal(0, game.Writes); Assert.Equal(0, game.Reads);
    }
}
