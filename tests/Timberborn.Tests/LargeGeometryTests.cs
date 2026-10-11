using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class LargeGeometryTests
{
    private const string Session = "11111111-1111-4111-8111-111111111111";

    [Fact]
    public void SparseGeometryKeepsOuterCellsAndRejectsOverflowWithoutTruncation()
    {
        var cells = Enumerable.Range(0, 512).Select(i => new Position(i % 16, i / 16, i % 8)).ToArray();
        Assert.Equal(cells, BuildingPolicy.Geometry(cells));
        Assert.Throws<ArgumentException>(() => BuildingPolicy.Geometry(cells.Append(new Position(31, 31, 7))));
        Assert.Throws<ArgumentException>(() => BuildingPolicy.Geometry(Array.Empty<Position>()));
        // Do not fill the holes of a sparse native shape with a bounding cuboid.
        Assert.Equal(2, BuildingPolicy.Geometry(new[] { cells[0], cells[511] }).Length);
    }

    [Theory]
    [InlineData(65, true)]
    [InlineData(512, true)]
    [InlineData(513, false)]
    public async Task NativePrecheckTransfersEntireGeometry(int count, bool accepted)
    {
        var cells = Enumerable.Range(0, count).Select(i => new SiteCell(new(i % 16, i / 16, 0), true, false, true, false, "Ground")).ToArray();
        var data = new NativeSite("SyntheticGarden", new(0, 0, 0), 0, "requires_game_validation", false, [], cells, null, null, [], []);
        using var client = Client(data);
        var request = BridgeRequest.Parse("/agent-api/v1/building-precheck", new() { ["template"] = "SyntheticGarden", ["x"] = "0", ["y"] = "0", ["z"] = "0", ["rotation"] = "0" });
        if (accepted) Assert.Equal(count, (await client.Precheck(request, TestContext.Current.CancellationToken)).Data.Cells.Length);
        else await Assert.ThrowsAsync<InvalidDataException>(() => client.Precheck(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ObjectPageKeepsAllCellsBeyondOldTransportBudget()
    {
        var cells = Enumerable.Range(0, 512).Select(i => new Position(i % 8, i / 8 % 8, i / 64)).ToArray();
        var items = Enumerable.Range(1, 32).Select(i => new BuildingPosition(new Guid(i, 0, 0, new byte[8]),
            "SyntheticGarden", new(0, 0, 0), "Cw0", true, null, cells, false)).ToArray();
        var data = new NativeObjects("buildings_and_paths", 0, 32, 32, items, false, []);
        Assert.True(JsonSerializer.SerializeToUtf8Bytes(data, NativeJson.Options).Length > 128 * 1024);
        using var client = Client(data);
        var request = BridgeRequest.Parse("/agent-api/v1/objects", new() { ["offset"] = "0", ["limit"] = "32" });
        var result = await client.Objects(request, TestContext.Current.CancellationToken);
        Assert.All(result.Data.Items, item => { Assert.Equal(512, item.OccupiedCells.Length); Assert.False(item.CellsTruncated); });
    }

    [Fact]
    public void RoadEvidenceAllowsWholeObjectAndExplicitWholePlanButNotOverflow()
    {
        NativeRoadProtection Evidence(int count) => new("unknown", [], 0, 0, true, false, [], false,
            Enumerable.Range(0, count).Select(i => new Position(i % 32, i / 32 % 32, i / 1024)).ToArray(), []);
        RoadProtectionContract.Validate(Evidence(512), false);
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(Evidence(513), false));
        RoadProtectionContract.Validate(Evidence(16384), false, maxCandidateCells: BuildingPolicy.MaxProjectGeometryCells);
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(Evidence(16385), false, maxCandidateCells: BuildingPolicy.MaxProjectGeometryCells));
    }

    [Theory]
    [InlineData(null, null, 64)]
    [InlineData(125, 512, 512)]
    public async Task CapabilityUsesObservedBudgetNotSameVersionGuess(int? cells, int? limit, int expected)
    {
        var option = new BuildingOption("SyntheticGarden", true, true, true, [], "Single", "Hex", "Wellbeing", new(5, 5, 5), null, false, [], cells, limit);
        using var client = Client(new NativeBuildingCatalog("Folktails", 0, 1, 1, [option], false, []));
        var result = await BuildingCapabilityTools.Invoke(client, JsonSerializer.SerializeToElement(new { session = Session }), true, TestContext.Current.CancellationToken);
        Assert.Equal(expected, result["data"]!["maxObjectFootprintCells"]!.GetValue<int>());
    }

    [Theory]
    [InlineData(0, 512)]
    [InlineData(513, 512)]
    [InlineData(1, 513)]
    [InlineData(null, 512)]
    public async Task CatalogRejectsUnsupportedGeometryClaim(int? cells, int limit)
    {
        var option = new BuildingOption("SyntheticGarden", true, true, true, [], "Single", "Hex", "Wellbeing", new(5, 5, 5), null, false, [], cells, limit);
        using var client = Client(new NativeBuildingCatalog("Folktails", 0, 1, 1, [option], false, []));
        var request = BridgeRequest.Parse("/agent-api/v1/building-catalog", new() { ["offset"] = "0", ["limit"] = "1" });
        await Assert.ThrowsAsync<InvalidDataException>(() => client.BuildingCatalog(request, TestContext.Current.CancellationToken));
    }

    private static NativeClient Client<T>(T data) => new(new(8081, new string('a', 64)),
        new Handler(JsonSerializer.Serialize(new BridgeEnvelope<T>(1, Session, DateTimeOffset.UnixEpoch, "0.37.0", data), NativeJson.Options)));
    private sealed class Handler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
    }
}
