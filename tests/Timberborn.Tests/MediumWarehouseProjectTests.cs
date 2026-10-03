using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class MediumWarehouseProjectTests
{
    [Theory]
    [InlineData("0.33.1")] [InlineData("0.34.0")]
    public void NewerVersionsKeepExistingMediumWarehouseContract(string version) =>
        NativeClient.ValidateProjectExecution(Receipt("MediumWarehouse.Folktails") with { BridgeVersion = version }, Request());
    private const string Action = "33333333-3333-3333-3333-333333333333";
    private static BuildingProjectExecutionRequest Request(string template = "MediumWarehouse.Folktails", int rotation = 0) =>
        BuildingProjectExecutionRequest.Parse(true, new NameValueCollection {
            ["session"] = "11111111-1111-1111-1111-111111111111",
            ["districtId"] = "22222222-2222-2222-2222-222222222222",
            ["actionId"] = Action, ["template"] = template, ["mode"] = "development_pilot",
            ["x"] = "0", ["y"] = "0", ["z"] = "1", ["width"] = "4", ["height"] = "4",
            ["rotation"] = rotation.ToString(), ["optionIndex"] = "0", ["planKey"] = new string('a', 64) });

    private static BridgeEnvelope<NativeProjectExecution> Receipt(string template, int rotation = 0) =>
        new(1, Request().Session, DateTimeOffset.UtcNow, "0.33.0",
            new(Action, new string('a', 64), "completed", "order_and_access_confirmed",
                [new(Action, template, 1, 1, 1, rotation, "confirmed")], false, ["synthetic_test"]));

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void MediumReceiptBindsToRequestedTemplateAndRotation(int rotation) =>
        NativeClient.ValidateProjectExecution(Receipt("MediumWarehouse.Folktails", rotation), Request(rotation: rotation));

    [Theory]
    [InlineData("SmallWarehouse.Folktails", "MediumWarehouse.Folktails")]
    [InlineData("MediumWarehouse.Folktails", "SmallWarehouse.Folktails")]
    public void AnotherSupportedTemplateCannotMasqueradeAsRequestedBuilding(string requested, string observed) =>
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(Receipt(observed), Request(requested)));

    [Theory]
    [InlineData("0.32.1")] [InlineData("0.32.0")] [InlineData("0.26.0")]
    public void OldBridgeCannotClaimMediumProject(string version) =>
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(
            Receipt("MediumWarehouse.Folktails") with { BridgeVersion = version }, Request()));

    [Fact]
    public void ExistingSmallWarehouseReceiptRemainsCompatible() =>
        NativeClient.ValidateProjectExecution(Receipt("SmallWarehouse.Folktails") with { BridgeVersion = "0.32.1" },
            Request("SmallWarehouse.Folktails"));

    [Fact]
    public void MediumWarehouseDoesNotIncreaseRoadLimit()
    {
        var steps = Enumerable.Range(0, 5).Select(_ => new BuildingProjectController.Step { Template = "Path" })
            .Append(new BuildingProjectController.Step { Template = "MediumWarehouse.Folktails" }).ToArray();
        Assert.Throws<ArgumentException>(() => new BuildingProjectController().Start(Guid.Parse(Action), "f", "k",
            steps, () => true, _ => { }, _ => "confirmed"));
    }

    [Fact]
    public void StatusReadAlsoRejectsUnsupportedAndLegacyMediumReceipts()
    {
        var status = BuildingProjectExecutionRequest.Parse(false,
            new NameValueCollection { ["session"] = Request().Session, ["actionId"] = Action });
        NativeClient.ValidateProjectExecution(Receipt("MediumWarehouse.Folktails"), status);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(Receipt("Lodge.Folktails"), status));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(
            Receipt("MediumWarehouse.Folktails") with { BridgeVersion = "0.32.1" }, status));
    }

    [Theory]
    [InlineData("LargeWarehouse.Folktails")] [InlineData("DoubleLodge.Folktails")]
    [InlineData("MediumWarehouse.IronTeeth")] [InlineData("Path")]
    public void OtherTemplatesRemainOutsideFlatExecutionScope(string template) =>
        Assert.Throws<ArgumentException>(() => Request(template));

    [Fact]
    public void MediumProjectKeepsOrderedPathsAndIdempotentLedger()
    {
        var id = Guid.Parse(Action);
        var controller = new BuildingProjectController();
        int writes = 0;
        var steps = new BuildingProjectController.Step[] {
            new() { EntityId = "44444444-4444-4444-4444-444444444444", Template = "Path" },
            new() { EntityId = Action, Template = "MediumWarehouse.Folktails" } };
        var receipt = controller.Start(id, "synthetic", "key", steps, () => true, _ => writes++, _ => "confirmed");
        for (int tick = 0; tick < 4; tick++) controller.Tick(tick);
        Assert.Equal("completed", receipt.State);
        Assert.Equal(2, writes);
        Assert.Same(receipt, controller.Existing(id, "synthetic"));
        controller.Tick(100);
        Assert.Equal(2, writes);
        Assert.False(receipt.ConstructionPreflightProven);
    }

    [Fact]
    public void LargerWarehouseCannotEnterFixedVerticalSequence()
    {
        string[] templates = ["Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path",
            "Platform.Folktails", "MediumWarehouse.Folktails"];
        var steps = templates.Select(t => new BuildingProjectController.Step { Template = t }).ToArray();
        Assert.Throws<ArgumentException>(() => new BuildingProjectController().Start(Guid.Parse(Action), "f", "k",
            steps, () => true, _ => { }, _ => "confirmed"));
    }
}
