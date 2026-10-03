using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuildingCapabilityTests
{
    private const string Session = "11111111-1111-1111-1111-111111111111";

    [Theory]
    [InlineData("0.33.0", 1)]
    [InlineData("0.33.1", 4)]
    public void KnownProfileSeparatesFlatAndFixedVerticalScope(string version, int verticalCapacity)
    {
        var report = BuildingCapabilityTools.Describe(version, "Folktails", true);
        Assert.Equal("known", report.ProfileState);
        Assert.Equal(64, report.MaxObjectFootprintCells);
        Assert.Equal(5, report.Modes.Length);
        var flat = Assert.Single(report.Modes, m => m.Mode == "development_pilot");
        Assert.Equal(4, flat.MaxNewGroundRoads);
        Assert.Equal(8, flat.MaxSearchWidth);
        Assert.Equal(8, flat.MaxSearchHeight);
        Assert.Equal(4, flat.MaxProjectsPerSession);
        Assert.Equal(new[] { 0, 1, 2, 3 }, flat.LiveRotations);
        Assert.All(flat.ObjectTemplates.Where(t => t != "Path"), t => Assert.True(BuildingProjectPilotPolicy.SupportsTemplate(t)));
        Assert.DoesNotContain("LargeWarehouse.Folktails", flat.ObjectTemplates);
        var vertical = Assert.Single(report.Modes, m => m.Mode == "stair_platform_warehouse_pilot");
        Assert.Equal(7, vertical.MaxSteps);
        Assert.Equal(7, vertical.MinSteps);
        Assert.Equal(verticalCapacity, vertical.MaxProjectsPerSession);
        Assert.All(report.Modes.Where(m => m.Tool == "execute_vertical_stair_pilot"),
            m => Assert.Equal(verticalCapacity, m.MaxProjectsPerSession));
        Assert.Equal(4, VerticalStairRequest.MaxProjectsPerSession);
        Assert.Equal(300, vertical.MaxWaitRealSeconds);
        Assert.Equal(new[] { 2 }, vertical.UpperPathCounts);
        Assert.Equal(new[] { 0, 1, 2, 3 }, vertical.LiveRotations);
        Assert.Equal(new[] { 3 }, Assert.Single(report.Modes, m => m.Mode == "stair_platform_pilot").LiveRotations);
        Assert.Contains("max_projects_shared_across_vertical_modes_not_per_mode", report.Limitations);
        Assert.DoesNotContain("MediumWarehouse.Folktails", vertical.ObjectTemplates);
    }

    [Theory]
    [InlineData("0.32.1", "Folktails")]
    [InlineData("0.35.0", "Folktails")]
    [InlineData("0.33.0", "IronTeeth")]
    public void UnknownProfileNeverInventsCapabilities(string version, string faction)
    {
        var report = BuildingCapabilityTools.Describe(version, faction, true);
        Assert.Equal("unknown", report.ProfileState);
        Assert.Empty(report.Modes);
        Assert.Null(report.MaxObjectFootprintCells);
        Assert.False(report.RegularExecutionAllowed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ServerSwitchAndHistoricalEvidenceNeverGrantSiteSafety(bool enabled)
    {
        var report = BuildingCapabilityTools.Describe("0.33.0", "Folktails", enabled);
        Assert.Equal(enabled, report.ServerBuildingPlacementEnabled);
        Assert.Equal("not_observed", report.BridgePlacementGate);
        Assert.False(report.SiteValidationPerformed);
        Assert.False(report.RegularExecutionAllowed);
        Assert.False(report.ConstructionPreflightProven);
        Assert.All(report.Modes, m => Assert.All(m.LiveRotations, r => Assert.Contains(r, m.Rotations)));
        var stair = Assert.Single(report.Modes, m => m.Mode == "single_stair_pilot");
        Assert.Empty(stair.LiveRotations);
        Assert.Equal("live_rotation_coverage_not_catalogued", stair.EvidenceSource);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"session\":\"bad\"}")]
    [InlineData("{\"session\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("{\"session\":1}")]
    [InlineData("{\"session\":\"11111111-1111-1111-1111-111111111111\",\"execute\":true}")]
    [InlineData("{\"session\":\"11111111-1111-1111-1111-111111111111\",\"session\":\"11111111-1111-1111-1111-111111111111\"}")]
    public void StrictSessionArguments(string json) =>
        Assert.Throws<ArgumentException>(() => BuildingCapabilityTools.ParseSession(JsonSerializer.Deserialize<JsonElement>(json)));

    [Fact]
    public void ToolIsReadOnlyAvailableWithoutWritesAndRequiresReasoning()
    {
        var tool = Assert.Single(NativeTools.Catalog(), t => t.Name == "inspect_building_capabilities");
        Assert.True(tool.Annotations!.ReadOnlyHint);
        Assert.False(tool.Annotations.DestructiveHint);
        Assert.False(tool.InputSchema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(new[] { "session", "reasoning" },
            tool.InputSchema.GetProperty("required").EnumerateArray().Select(x => x.GetString()));
        Assert.NotNull(tool.OutputSchema);
    }

    [Theory]
    [InlineData("0.33.0", false)] [InlineData("0.33.0", true)]
    [InlineData("0.33.1", false)] [InlineData("0.33.1", true)]
    [InlineData("0.34.0", false)] [InlineData("0.34.0", true)]
    public async Task NativeRoutingUsesOnlyReadOnlyCatalogHeaderAndBindsSession(string version, bool stale)
    {
        var handler = new CatalogHeaderHandler(version);
        using var native = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler));
        var args = JsonSerializer.SerializeToElement(new { session = stale ? "22222222-2222-2222-2222-222222222222" : Session,
            reasoning = "Synthetic capability query" });
        var result = await native.Invoke("inspect_building_capabilities", args, TestContext.Current.CancellationToken);
        Assert.Equal(1, handler.Reads);
        Assert.Equal(stale ? "error" : "ok", result["status"]!.GetValue<string>());
        if (stale) Assert.Equal("state_conflict", result["error"]!["code"]!.GetValue<string>());
        else {
            Assert.Equal(Session, result["meta"]!["sessionId"]!.GetValue<string>());
            Assert.False(result["data"]!["serverBuildingPlacementEnabled"]!.GetValue<bool>());
            Assert.Equal("known", result["data"]!["profileState"]!.GetValue<string>());
        }
    }

    [Fact]
    public void LodgeProfileDoesNotInheritWarehouseLiveEvidence()
    {
        var report = BuildingCapabilityTools.Describe("0.34.0", "Folktails", true);
        Assert.Equal("known", report.ProfileState);
        Assert.Equal(5, report.Modes.Length);
        var flat = Assert.Single(report.Modes, m => m.Mode == "development_pilot");
        Assert.Contains("Lodge.Folktails", flat.ObjectTemplates);
        Assert.Empty(flat.LiveRotations);
        var lodge = Assert.Single(flat.TemplateEvidence, e => e.Template == "Lodge.Folktails");
        Assert.Empty(lodge.LiveRotations);
        Assert.Equal("not_live_proven", lodge.EvidenceSource);
        Assert.All(flat.TemplateEvidence.Where(e => e.Template != "Lodge.Folktails"),
            e => Assert.Equal(new[] { 0, 1, 2, 3 }, e.LiveRotations));
        Assert.All(report.Modes, m => Assert.Equal(4, m.MaxProjectsPerSession));
        Assert.All(report.Modes.Where(m => m.Tool == "execute_vertical_stair_pilot"),
            m => Assert.DoesNotContain("Lodge.Folktails", m.ObjectTemplates));
        Assert.False(report.RegularExecutionAllowed);
        Assert.False(report.ConstructionPreflightProven);
        Assert.DoesNotContain("Lodge.Folktails", BuildingCapabilityTools.Describe("0.33.1", "Folktails", true).Modes[0].ObjectTemplates);
        Assert.Empty(BuildingCapabilityTools.Describe("0.34.0", "IronTeeth", true).Modes);
    }

    private sealed class CatalogHeaderHandler(string version) : HttpMessageHandler
    {
        public int Reads { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Reads++;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/agent-api/v1/building-catalog", request.RequestUri!.AbsolutePath);
            Assert.Equal("?offset=0&limit=1", request.RequestUri.Query);
            var data = new NativeBuildingCatalog("Folktails", 0, 1, 1,
                [new("SyntheticBuilding", true, true, true, [], "Single", "Square", "Synthetic", new(1, 1, 1), null, false, [])],
                false, ["synthetic_test"]);
            var envelope = new BridgeEnvelope<NativeBuildingCatalog>(1, Session, DateTimeOffset.UtcNow, version, data);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(JsonSerializer.Serialize(envelope, NativeJson.Options)) });
        }
    }
}
