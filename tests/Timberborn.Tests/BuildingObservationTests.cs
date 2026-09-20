using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuildingObservationTests
{
    [Theory]
    [InlineData("bad")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task InvalidIdsNeverReachTransport(string id)
    {
        var handler = new Handler("{}");
        using var tools = Tools(handler);
        var result = await tools.Invoke("inspect_building", Args(id, Guid.NewGuid().ToString("D")), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NotFoundAndUnfinishedBuildingRemainDistinct(bool found)
    {
        var id = Guid.NewGuid(); var session = Guid.NewGuid().ToString("D");
        var handler = new Handler(Payload(id, session, found).ToJsonString());
        using var tools = Tools(handler);
        var result = await tools.Invoke("inspect_building", Args(id.ToString("D"), session), TestContext.Current.CancellationToken);
        Assert.Equal("ok", result["status"]!.GetValue<string>());
        Assert.Equal(found, result["data"]!["found"]!.GetValue<bool>());
        if (found)
        {
            Assert.False(result["data"]!["details"]!["finished"]!.GetValue<bool>());
            Assert.Equal(12, result["data"]!["details"]!["construction"]!["materials"]!["buildingCosts"]![0]!["amount"]!.GetValue<int>());
            Assert.Empty(result["data"]!["details"]!["construction"]!["materials"]!["siteStock"]!.AsArray());
            Assert.Null(result["data"]!["details"]!["construction"]!["remainingRequiredGoods"]);
        }
        else Assert.Null(result["data"]!["details"]);
    }

    [Theory]
    [InlineData("session")]
    [InlineData("id")]
    [InlineData("negative_material")]
    [InlineData("district_without_component")]
    [InlineData("finished_with_construction")]
    [InlineData("stock_without_inventory")]
    [InlineData("negative_stock")]
    [InlineData("legacy_material_contract")]
    public async Task RejectsMismatchedOrContradictoryObservations(string fault)
    {
        var id = Guid.NewGuid(); var session = Guid.NewGuid().ToString("D");
        var payload = Payload(id, session, true);
        var details = payload["data"]!["details"]!;
        switch (fault)
        {
            case "session": payload["sessionId"] = Guid.NewGuid().ToString("D"); break;
            case "id": payload["data"]!["id"] = Guid.NewGuid().ToString("D"); break;
            case "negative_material": details["construction"]!["materials"]!["buildingCosts"]![0]!["amount"] = -1; break;
            case "stock_without_inventory": details["construction"]!["materials"]!["inventoryAvailable"] = false; break;
            case "negative_stock": details["construction"]!["materials"]!["siteStock"] = new JsonArray(new JsonObject { ["id"] = "Log", ["amount"] = -1 }); break;
            case "legacy_material_contract": details["construction"]!.AsObject().Remove("materials"); details["construction"]!["remainingRequiredGoods"] = new JsonArray(); break;
            case "district_without_component": details["district"]!["componentPresent"] = false; break;
            case "finished_with_construction": details["finished"] = true; details["unfinished"] = false; break;
        }
        var handler = new Handler(payload.ToJsonString());
        using var tools = Tools(handler);
        var result = await tools.Invoke("inspect_building", Args(id.ToString("D"), session), TestContext.Current.CancellationToken);
        Assert.Equal("backend_incompatible", result["error"]!["code"]!.GetValue<string>());
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task UnavailableInventoryStaysUnknownInsteadOfEmpty()
    {
        var id = Guid.NewGuid(); var session = Guid.NewGuid().ToString("D");
        var payload = Payload(id, session, true);
        var materials = payload["data"]!["details"]!["construction"]!["materials"]!;
        materials["inventoryAvailable"] = false;
        materials["siteStock"] = null;
        using var tools = Tools(new Handler(payload.ToJsonString()));
        var result = await tools.Invoke("inspect_building", Args(id.ToString("D"), session), TestContext.Current.CancellationToken);
        Assert.Equal("ok", result["status"]!.GetValue<string>());
        Assert.Null(result["data"]!["details"]!["construction"]!["materials"]!["siteStock"]);
    }

    private static NativeTools Tools(Handler handler) => new(new NativeClient(new(8081, new string('a', 64)), handler));
    private static JsonElement Args(string id, string session) => JsonSerializer.SerializeToElement(new { id, session });
    private static JsonObject Payload(Guid id, string session, bool found) => (JsonObject)JsonSerializer.SerializeToNode(
        new BridgeEnvelope<NativeBuilding>(1, session, DateTimeOffset.UtcNow, "0.6.1", new(id, found,
            found ? new("Lodge.Folktails", new(1, 2, 3), false, true, true,
                new(false, true, false, 0, 0, 0, false, false, new([new("Log", 12)], true, [])),
                new(true, null, null, Guid.NewGuid())) : null, ["synthetic_test"])), NativeJson.Options)!;
    private sealed class Handler(string json) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++; Assert.Equal(HttpMethod.Get, request.Method); Assert.Null(request.Content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        }
    }
}
