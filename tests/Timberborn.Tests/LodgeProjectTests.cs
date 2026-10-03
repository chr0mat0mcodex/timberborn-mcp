using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class LodgeProjectTests
{
    private const string Session = "11111111-1111-1111-1111-111111111111";
    private const string Action = "33333333-3333-3333-3333-333333333333";
    private static BuildingProjectExecutionRequest Request(int rotation = 0) =>
        BuildingProjectExecutionRequest.Parse(true, new NameValueCollection {
            ["template"] = "Lodge.Folktails", ["session"] = Session,
            ["districtId"] = "22222222-2222-2222-2222-222222222222", ["actionId"] = Action,
            ["mode"] = "development_pilot", ["x"] = "0", ["y"] = "0", ["z"] = "1",
            ["width"] = "4", ["height"] = "4", ["rotation"] = rotation.ToString(),
            ["optionIndex"] = "0", ["planKey"] = new string('a', 64) });
    private static BridgeEnvelope<NativeProjectExecution> Receipt(int rotation = 0) =>
        new(1, Session, DateTimeOffset.UtcNow, "0.34.0",
            new(Action, new string('a', 64), "completed", "order_and_access_confirmed",
                [new(Action, "Lodge.Folktails", 1, 1, 1, rotation, "confirmed")], false, ["synthetic_test"]));

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void LodgeReceiptBindsTemplateAndRotation(int rotation)
    {
        NativeClient.ValidateProjectExecution(Receipt(rotation), Request(rotation));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(
            Receipt((rotation + 1) % 4), Request(rotation)));
        var e = Receipt(rotation);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(
            e with { Data = e.Data with { Steps = [e.Data.Steps[0] with { Template = "SmallWarehouse.Folktails" }] } }, Request(rotation)));
    }

    [Theory]
    [InlineData("0.33.1")] [InlineData("0.33.0")] [InlineData("0.32.1")]
    public void OldBridgeCannotClaimLodgeProjectEvenOnStatusRead(string version)
    {
        var e = Receipt() with { BridgeVersion = version };
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e, Request()));
        var status = BuildingProjectExecutionRequest.Parse(false, new NameValueCollection { ["session"] = Session, ["actionId"] = Action });
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(e, status));
        NativeClient.ValidateProjectExecution(Receipt(), status);
    }

    [Fact]
    public void LodgeReceiptCannotClaimOtherSessionRegionOrPreflightSafety()
    {
        var e = Receipt();
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(
            e with { SessionId = "44444444-4444-4444-4444-444444444444" }, Request()));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(
            e with { Data = e.Data with { ConstructionPreflightProven = true } }, Request()));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateProjectExecution(
            e with { Data = e.Data with { Steps = [e.Data.Steps[0] with { X = 4 }] } }, Request()));
    }

    [Fact]
    public void LodgeUsesSameOrderedFourRoadLimitAndReplayProtection()
    {
        var controller = new BuildingProjectController(4);
        var steps = Enumerable.Range(0, 4).Select(_ => new BuildingProjectController.Step { Template = "Path" })
            .Append(new BuildingProjectController.Step { Template = "Lodge.Folktails" }).ToArray();
        int writes = 0;
        var receipt = controller.Start(Guid.Parse(Action), "f", "k", steps, () => true, _ => writes++, _ => "confirmed");
        for (int tick = 0; tick < 10; tick++) controller.Tick(tick);
        Assert.Equal("completed", receipt.State);
        Assert.Equal(5, writes);
        Assert.Same(receipt, controller.Start(Guid.Parse(Action), "f", "k", steps, () => true, _ => writes++, _ => "confirmed"));
        controller.Tick(100); Assert.Equal(5, writes);
        Assert.Throws<BridgeRejectionException>(() => controller.Existing(Guid.Parse(Action), "changed"));
        var tooMany = steps.Prepend(new BuildingProjectController.Step { Template = "Path" }).ToArray();
        Assert.Throws<ArgumentException>(() => new BuildingProjectController().Start(Guid.Parse(Action), "f", "k", tooMany, () => true, _ => { }, _ => "confirmed"));
    }

    [Fact]
    public void LodgeCannotEnterFixedVerticalWarehouseSequence()
    {
        string[] templates = ["Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path", "Platform.Folktails", "Lodge.Folktails"];
        var steps = templates.Select(t => new BuildingProjectController.Step { Template = t }).ToArray();
        Assert.Throws<ArgumentException>(() => new BuildingProjectController().Start(Guid.Parse(Action), "f", "k", steps, () => true, _ => { }, _ => "confirmed"));
    }
}
