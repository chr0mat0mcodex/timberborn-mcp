using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

// Synthetic contracts/grid cases, not claims about an installed game blueprint.
public sealed class GenericBuildingProjectTests
{
    private const string Session = "11111111-1111-1111-1111-111111111111";
    private const string Action = "33333333-3333-3333-3333-333333333333";
    private static BuildingProjectExecutionRequest Request(string template, int rotation) =>
        BuildingProjectExecutionRequest.Parse(true, new NameValueCollection {
            ["session"] = Session, ["actionId"] = Action,
            ["districtId"] = "22222222-2222-2222-2222-222222222222",
            ["template"] = template, ["mode"] = "development_pilot",
            ["x"] = "0", ["y"] = "0", ["z"] = "1", ["width"] = "4", ["height"] = "4",
            ["rotation"] = rotation.ToString(), ["optionIndex"] = "0", ["planKey"] = new string('a', 64) });

    [Theory]
    [InlineData("SyntheticWorkshop.Folktails", 0)]
    [InlineData("LargeWarehouse.Folktails", 1)]
    [InlineData("DoubleLodge.Folktails", 2)]
    [InlineData("SyntheticWorkshop.IronTeeth", 3)]
    public void GenericReceiptsBindTemplateRotationSessionAndRegion(string template, int rotation)
    {
        var r = Request(template, rotation);
        var data = new NativeProjectExecution(Action, new string('a', 64), "completed", "order_and_access_confirmed",
            [new(Action, template, 1, 1, 1, rotation, "confirmed")], false, ["synthetic_test"]);
        var e = new BridgeEnvelope<NativeProjectExecution>(1, Session, DateTimeOffset.UtcNow, "0.35.0", data);
        NativeClient.ValidateProjectExecution(e, r);
        NativeClient.ValidateProjectExecution(e with { BridgeVersion = "0.35.1" }, r);
        NativeClient.ValidateProjectExecution(e with { BridgeVersion = "0.36.0" }, r);
        var status = BuildingProjectExecutionRequest.Parse(false, new() { ["session"] = Session, ["actionId"] = Action });
        NativeClient.ValidateProjectExecution(e, status);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e with { BridgeVersion = "0.34.0" }, status));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e with { BridgeVersion = "99.0.0" }, r));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e with { SessionId = Action }, r));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e,
            Request("DifferentSyntheticBuilding", rotation)));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e with {
            Data = data with { Steps = [data.Steps[0] with { X = 4 }] } }, r));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e with {
            Data = data with { Steps = [data.Steps[0] with { Rotation = (rotation + 1) % 4 }] } }, r));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e with {
            Data = data with { ConstructionPreflightProven = true } }, r));
        var controller = new BuildingProjectController();
        int writes = 0;
        var receipt = controller.Start(Guid.Parse(Action), "synthetic", "key",
            [new() { EntityId = Action, Template = template }], () => true, _ => writes++, _ => "confirmed");
        controller.Tick(0); controller.Tick(1);
        Assert.Equal("completed", receipt.State);
        Assert.Same(receipt, controller.Existing(Guid.Parse(Action), "synthetic"));
        controller.Tick(2); Assert.Equal(1, writes);
    }

    [Theory]
    [InlineData("Path")] [InlineData("")] [InlineData("../Building")] [InlineData("Building with spaces")]
    public void SyntaxAndInfrastructureAreNotTerminalBuildingTemplates(string template) =>
        Assert.Throws<ArgumentException>(() => Request(template, 0));

    [Fact]
    public void WholeFootprintCannotBeUsedEvenWhenCellsPreviouslyHadPaths()
    {
        bool[] passable = [true, true, true, true, true, true];
        bool[] goals = [false, false, true, false, false, false];
        Assert.Equal(new[] { 2, 5, 4, 3, 0 },
            FlatRoadSearch.FindForBuilding(3, 2, [0], [1], passable, goals));
        Assert.Null(FlatRoadSearch.FindForBuilding(3, 2, [0], [1, 3], passable, goals));
        Assert.All(passable, cell => Assert.True(cell)); // Never corrupt the shared search grid.
    }

    [Fact]
    public void OneConnectableEntranceIsEnoughAndShortestCandidateWins()
    {
        bool[] passable = [true, false, true, true, true, true];
        bool[] goals = [false, false, true, false, false, false];
        Assert.Equal(new[] { 2, 5 }, FlatRoadSearch.FindForBuilding(3, 2, [1, 0, 5], [], passable, goals));
        Assert.Null(FlatRoadSearch.FindForBuilding(3, 2, [], [], passable, goals));
        Assert.Null(FlatRoadSearch.FindForBuilding(3, 2, [1], [], passable, goals));
        Assert.Throws<ArgumentException>(() => FlatRoadSearch.FindForBuilding(3, 2, [6], [], passable, goals));
    }

    [Fact]
    public void EntranceInsideFootprintNeverBecomesAConnection()
    {
        Assert.Null(FlatRoadSearch.FindForBuilding(2, 1, [0], [0], [true, true], [false, true]));
        Assert.Equal(new[] { 1 }, FlatRoadSearch.FindForBuilding(2, 1, [0, 1], [0], [true, true], [false, true]));
    }

    [Theory]
    [InlineData("0.35.0", "Folktails", 5)] [InlineData("0.35.0", "IronTeeth", 1)]
    [InlineData("0.35.1", "Folktails", 5)] [InlineData("0.35.1", "IronTeeth", 1)]
    [InlineData("0.36.0", "Folktails", 5)] [InlineData("0.36.0", "IronTeeth", 1)]
    public void ProfileUsesCatalogueScopeNotAnExhaustiveOrProvenTemplateList(string version, string faction, int modes)
    {
        var report = BuildingCapabilityTools.Describe(version, faction, true);
        Assert.Equal("known", report.ProfileState);
        Assert.Equal(modes, report.Modes.Length);
        var flat = Assert.Single(report.Modes, m => m.Mode == "development_pilot");
        Assert.Empty(flat.ObjectTemplates);
        Assert.Empty(flat.LiveRotations);
        Assert.Equal("native_catalog_supported_geometry_with_road_entrance", flat.TemplateSelection);
        Assert.Equal("public_block_object_spec_single_entrance", flat.EntranceModel);
        Assert.Equal(4, flat.MaxNewGroundRoads);
        Assert.False(report.RegularExecutionAllowed);
        Assert.False(report.ConstructionPreflightProven);
        if (faction == "Folktails") {
            Assert.Equal(new[] { 3 }, Assert.Single(flat.TemplateEvidence, e => e.Template == "Lodge.Folktails").LiveRotations);
            Assert.DoesNotContain(flat.TemplateEvidence, e => e.Template == "LargeWarehouse.Folktails");
            Assert.Equal(new[] { 1 }, Assert.Single(flat.TemplateEvidence, e => e.Template == "Bench.Folktails").LiveRotations);
            Assert.Equal(new[] { 1 }, Assert.Single(flat.TemplateEvidence, e => e.Template == "LargePile.Folktails").LiveRotations);
        } else Assert.Empty(flat.TemplateEvidence);
    }
}
