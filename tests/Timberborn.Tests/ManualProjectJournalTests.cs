using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ManualProjectJournalTests
{
    private static string Id(int n) => new Guid(n, 0, 0, new byte[8]).ToString("D");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletedAndReleasedHistoryRetainsCapacityAndReplayProtection(bool release)
    {
        var ledger = new LargeProjectLedger();
        for (int i = 1; i <= 128; i++)
        {
            Assert.Equal(128 - (i - 1), ledger.Remaining);
            ledger.Start(Id(i), "key", "fingerprint", [0], [Id(i + 1000)]);
            ledger.Advance(Id(i), 0, () => true, _ => "finished", _ => true, _ => { });
            ledger.Advance(Id(i), 1, () => true, _ => "finished", _ => true, _ => { });
            Assert.Equal("completed", ledger.Inspect(Id(i)).State);
            if (release) ledger.Release(Id(i), ["finished"]);
            Assert.Equal(i, ledger.Count);
        }
        Assert.Equal(0, ledger.Remaining);
        Assert.False(ledger.BlocksNewProject);
        Assert.Equal("large_project_journal_limit", Assert.Throws<BridgeRejectionException>(() =>
            ledger.Start(Id(129), "key", "new", [0], [Id(1129)])).Code);
        Assert.Equal(128, ledger.Count);
        Assert.Same(ledger.Inspect(Id(1)), ledger.Start(Id(1), "key", "fingerprint", [0], [Id(1001)]));
        Assert.Equal("state_conflict", Assert.Throws<BridgeRejectionException>(() =>
            ledger.Start(Id(1), "key", "changed", [0], [Id(1001)])).Code);
    }

    [Fact]
    public void UnresolvedProjectStillReportsConflictBelowCapacity()
    {
        var ledger = new LargeProjectLedger();
        ledger.Start(Id(1), "key", "first", [0], [Id(1001)]);
        Assert.Equal("state_conflict", Assert.Throws<BridgeRejectionException>(() =>
            ledger.Start(Id(2), "key", "next", [0], [Id(1002)])).Code);
        Assert.Equal(127, ledger.Remaining);
    }

    [Theory]
    [InlineData(0, 128, false, true)]
    [InlineData(127, 1, false, true)]
    [InlineData(128, 0, false, true)]
    [InlineData(129, -1, false, false)]
    [InlineData(-1, 129, false, false)]
    [InlineData(128, 1, false, false)]
    [InlineData(128, 0, true, false)]
    public void DiagnosticCountsAreConsistent(int count, int remaining, bool release, bool valid)
    {
        var data = JsonSerializer.SerializeToElement(new { manualJournal = new {
            scope = "manual_large_projects", limit = 128, count, remaining, releaseFreesCapacity = release } });
        if (valid) ManualProjectJournalContract.Validate(data);
        else Assert.Throws<InvalidDataException>(() => ManualProjectJournalContract.Validate(data));
    }

    [Fact]
    public void OlderBridgeMayOmitDiagnostic() =>
        ManualProjectJournalContract.Validate(JsonSerializer.SerializeToElement(new { }));

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"scope\":3}")]
    [InlineData("{\"scope\":\"manual_large_projects\",\"limit\":\"128\"}")]
    public void MalformedDiagnosticIsRejected(string journal) =>
        Assert.Throws<InvalidDataException>(() => ManualProjectJournalContract.Validate(
            JsonSerializer.Deserialize<JsonElement>("{\"manualJournal\":" + journal + "}")));

    [Theory]
    [InlineData("plan_large_project")]
    [InlineData("inspect_large_project")]
    public void CompactRetainsCapacity(string tool)
    {
        var journal = new JsonObject { ["scope"] = "manual_large_projects", ["limit"] = 128,
            ["count"] = 127, ["remaining"] = 1, ["releaseFreesCapacity"] = false };
        var data = new JsonObject { ["manualJournal"] = journal.DeepClone(), ["parts"] = new JsonArray(),
            ["partStates"] = new JsonArray(), ["entityIds"] = new JsonArray() };
        ProjectReportPresentation.Compact(tool, data);
        Assert.True(JsonNode.DeepEquals(journal, data["manualJournal"]));
    }

    [Fact]
    public async Task LimitReachesMcpAsSpecificNonRetryablePreDispatchRejection()
    {
        using var handler = new LimitHandler();
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), enableBuildingPlacement: true);
        var result = await tools.Invoke("start_large_project", JsonSerializer.SerializeToElement(new {
            session = Id(1), districtId = Id(2), actionId = Id(3), planKey = new string('a', 64), mode = "development_pilot",
            steps = new[] { new { template = "Path", x = 1, y = 2, z = 3, rotation = 0, dependsOn = Array.Empty<int>() } }
        }), TestContext.Current.CancellationToken);
        Assert.Equal("large_project_journal_limit", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal("not_executed", result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Contains("128", result["error"]!["message"]!.GetValue<string>());
        Assert.Contains("128", ActivityOutcome.Describe("start_large_project", result));
        Assert.Equal(1, handler.Writes);
    }

    private sealed class LimitHandler : HttpMessageHandler
    {
        public int Writes;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (request.Method == HttpMethod.Get) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            Writes++;
            Assert.Equal("/agent-api/v1/large-project-start", request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict) {
                Content = new StringContent("{\"error\":\"large_project_journal_limit\"}") });
        }
    }
}
