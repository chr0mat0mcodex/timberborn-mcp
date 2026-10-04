using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ResponsePresentationTests
{
    private static JsonObject Envelope(JsonObject data) => new() {
        ["schemaVersion"] = 1, ["status"] = "ok", ["data"] = data,
        ["meta"] = new JsonObject { ["backend"] = "native", ["simulated"] = false,
            ["sessionId"] = "11111111-1111-4111-8111-111111111111", ["observedAtUtc"] = "2026-01-01T00:00:00Z" },
        ["error"] = null
    };

    [Fact]
    public void RequestDetailIsStrictAndNeverReachesBackend()
    {
        var args = JsonSerializer.SerializeToElement(new { detail = "full", reasoning = "Check", id = "sample" });
        var parsed = ResponsePresentation.ReadRequest("set_storage_good", args);
        Assert.True(parsed.Full);
        Assert.False(parsed.Arguments.TryGetProperty("detail", out _));
        Assert.Equal("Check", parsed.Arguments.GetProperty("reasoning").GetString());
        foreach (var json in new[] { "{\"detail\":true}", "{\"detail\":null}", "{\"detail\":\"large\"}",
            "{\"detail\":\"full\",\"detail\":\"compact\"}" }) {
            using var document = JsonDocument.Parse(json);
            Assert.Throws<ArgumentException>(() => ResponsePresentation.ReadRequest("set_storage_good", document.RootElement));
        }
        Assert.Throws<ArgumentException>(() => ResponsePresentation.ReadRequest("place_building", args));
        Assert.False(ResponsePresentation.ReadRequest("timberborn_status", JsonSerializer.SerializeToElement(new { reasoning = "Read" })).Full);
        using var duplicates = JsonDocument.Parse("{\"id\":\"first\",\"id\":\"second\",\"detail\":\"compact\"}");
        Assert.Equal(2, ResponsePresentation.ReadRequest("set_storage_good", duplicates.RootElement)
            .Arguments.EnumerateObject().Count(p => p.Name == "id"));
    }

    [Fact]
    public void CompactSettingsKeepNullsStateAndStockWithoutMutatingFullResult()
    {
        var source = Envelope(new JsonObject {
            ["finished"] = false, ["storageComponentPresent"] = true, ["farmComponentPresent"] = false,
            ["farm"] = null, ["storage"] = new JsonObject {
                ["selectedGood"] = "Carrot", ["mode"] = "obtain", ["stock"] = new JsonArray(),
                ["allowedGoods"] = new JsonArray("Carrot", "Berries") },
            ["limitations"] = new JsonArray("storage_and_farm_settings_only_for_finished_buildings") });
        var before = source.ToJsonString();
        var compact = ResponsePresentation.Present("inspect_building_settings", source, false);
        Assert.False(compact["data"]!["finished"]!.GetValue<bool>());
        Assert.True(compact["data"]!.AsObject().ContainsKey("farm"));
        Assert.Null(compact["data"]!["farm"]);
        Assert.Equal("Carrot", compact["data"]!["storage"]!["selectedGood"]!.GetValue<string>());
        Assert.NotNull(compact["data"]!["storage"]!["stock"]);
        Assert.False(compact["data"]!["storage"]!.AsObject().ContainsKey("allowedGoods"));
        Assert.Equal(before, source.ToJsonString());
        Assert.Equal(before, ResponsePresentation.Present("inspect_building_settings", source, true).ToJsonString());
    }

    [Fact]
    public void AppliedSettingReceiptIsSmallerButUnconfirmedAndErrorEvidenceIsComplete()
    {
        var data = new JsonObject { ["id"] = "sample", ["setting"] = "storage_good", ["previousValue"] = "",
            ["requestedValue"] = "Carrot", ["observedValue"] = "Carrot", ["outcome"] = "applied",
            ["observation"] = new JsonObject { ["storage"] = new JsonObject {
                ["allowedGoods"] = new JsonArray(Enumerable.Range(0, 32).Select(i => (JsonNode?)JsonValue.Create("SyntheticGood" + i)).ToArray()) } },
            ["limitations"] = new JsonArray("no_automatic_retry") };
        var source = Envelope(data);
        var compact = ResponsePresentation.Present("set_storage_good", source, false);
        Assert.Equal("Carrot", compact["data"]!["observedValue"]!.GetValue<string>());
        Assert.Equal("", compact["data"]!["previousValue"]!.GetValue<string>());
        Assert.True(compact.ToJsonString().Length < source.ToJsonString().Length * 0.7);
        data["outcome"] = "unconfirmed";
        Assert.Equal(source.ToJsonString(), ResponsePresentation.Present("set_storage_good", source, false).ToJsonString());
        source["status"] = "error";
        source["error"] = new JsonObject { ["code"] = "state_conflict", ["retryable"] = false };
        Assert.Equal(source.ToJsonString(), ResponsePresentation.Present("set_storage_good", source, false).ToJsonString());
        Assert.True(ResponsePresentation.ToToolResult(source).IsError);
    }

    [Fact]
    public void ValidationKeepsUnknownAndAffectedAccessEvidence()
    {
        var source = Envelope(new JsonObject { ["valid"] = null, ["sessionLocked"] = true,
            ["gameValidated"] = false, ["noPersistentChangeObserved"] = false,
            ["roadProtection"] = new JsonObject { ["status"] = "unknown", ["restored"] = false,
                ["affected"] = new JsonArray(new JsonObject { ["id"] = "affected" }),
                ["reasons"] = new JsonArray("construction_access_unknown"),
                ["limitations"] = new JsonArray("construction_preflight_unproven"),
                ["candidateCells"] = new JsonArray(new JsonObject { ["x"] = 1, ["y"] = 2, ["z"] = 3 }) } });
        var compact = ResponsePresentation.Present("validate_building", source, false);
        Assert.True(compact["data"]!.AsObject().ContainsKey("valid"));
        Assert.Null(compact["data"]!["valid"]);
        Assert.True(compact["data"]!["sessionLocked"]!.GetValue<bool>());
        Assert.Equal(source["data"]!["roadProtection"]!["affected"]!.ToJsonString(), compact["data"]!["roadProtection"]!["affected"]!.ToJsonString());
        Assert.NotNull(compact["data"]!["roadProtection"]!["limitations"]);
        Assert.Null(compact["data"]!["roadProtection"]!["candidateCells"]);
    }

    [Fact]
    public void NestedValidationRetainsIndependentUnknownsAndLosses()
    {
        var source = Envelope(JsonNode.Parse("""
            {"roadProtection":{"restored":false,"lostConnections":1,"affected":[{"id":"sample"}],
              "constructionAccessPreview":{"status":"observed","lostSites":1,"restored":false,
                "items":[{"id":"site","before":true,"during":false,"after":false,"accessCells":[{"x":1}]}],
                "details":{"status":"unknown","reason":"diagnostic_limit","baselineMatches":null,
                  "restored":null,"originOnDistrictRoad":null,"limitations":["not_builder_proof"],
                  "origin":null,"rangeOrigin":null,"originSource":null,"accesses":[]}}},
             "assessment":{"checks":[{"name":"access","status":"unknown","phase":"construction",
               "reason":"unproven","source":"native_diagnostic"}]}}
            """)!.AsObject());
        var before = source.ToJsonString();
        var compact = ResponsePresentation.Present("validate_building", source, false);
        var road = compact["data"]!["roadProtection"]!;
        Assert.Equal(1, road["lostConnections"]!.GetValue<int>());
        Assert.Equal(source["data"]!["roadProtection"]!["affected"]!.ToJsonString(), road["affected"]!.ToJsonString());
        var preview = road["constructionAccessPreview"]!;
        Assert.Equal(1, preview["lostSites"]!.GetValue<int>());
        Assert.False(preview["items"]![0]!["during"]!.GetValue<bool>());
        Assert.Null(preview["items"]![0]!["accessCells"]);
        Assert.Equal("unknown", preview["details"]!["status"]!.GetValue<string>());
        Assert.True(preview["details"]!.AsObject().ContainsKey("restored"));
        Assert.Null(preview["details"]!["restored"]);
        Assert.NotNull(preview["details"]!["limitations"]);
        Assert.Null(preview["details"]!["accesses"]);
        Assert.Equal(before, source.ToJsonString());
        Assert.Equal(before, ResponsePresentation.Present("validate_building", source, true).ToJsonString());
    }

    [Fact]
    public void CatalogAdvertisesOptionalDetailAndOptionalOmittedFields()
    {
        var tool = ResponsePresentation.Describe(BuildingSettingsTools.Catalog(true).Single(t => t.Name == "set_storage_good"));
        Assert.Equal("compact", tool.InputSchema.GetProperty("properties").GetProperty("detail").GetProperty("default").GetString());
        Assert.DoesNotContain(tool.InputSchema.GetProperty("required").EnumerateArray(), n => n.GetString() == "detail");
        void Check(JsonElement schema) {
            if (schema.ValueKind == JsonValueKind.Object) {
                if (schema.TryGetProperty("required", out var required))
                    Assert.DoesNotContain(required.EnumerateArray(), n => n.GetString() is "observation" or "limitations");
                foreach (var property in schema.EnumerateObject()) Check(property.Value);
            } else if (schema.ValueKind == JsonValueKind.Array) foreach (var item in schema.EnumerateArray()) Check(item);
        }
        Check(tool.OutputSchema!.Value);
    }

    [Fact]
    public void TextAndStructuredContentRemainEquivalentAndOtherToolsUnchanged()
    {
        var source = Envelope(new JsonObject { ["connection"] = "reachable", ["bridgeVersion"] = "0.35.3" });
        Assert.Equal(source.ToJsonString(), ResponsePresentation.Present("timberborn_status", source, false).ToJsonString());
        Assert.Equal(source.ToJsonString(), ResponsePresentation.Present("inspect_colony", source, false).ToJsonString());
        var response = ResponsePresentation.ToToolResult(source);
        Assert.Equal(response.StructuredContent!.Value.GetRawText(), Assert.IsType<TextContentBlock>(Assert.Single(response.Content)).Text);
    }
}
