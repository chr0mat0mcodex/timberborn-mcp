using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class VerticalSequentialTests
{
    private static readonly string[][] Modes = [
        ["Stairs.Folktails"],
        ["Stairs.Folktails", "Path", "Path"],
        ["Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path"],
        ["Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path", "Platform.Folktails", "SmallWarehouse.Folktails"]
    ];
    private static Guid Id(int i) => Guid.Parse($"00000000-0000-0000-0000-{i:D12}");
    private static BuildingProjectController.Step[] Steps(int mode) => Modes[mode]
        .Select(t => new BuildingProjectController.Step { Template = t }).ToArray();

    [Theory]
    [InlineData(4)]
    [InlineData(VerticalStairRequest.MaxProjectsPerSession)]
    public void FourVerticalModesShareCapacityAndKeepOldReceiptsDuringNextProject(int capacity)
    {
        var controller = new BuildingProjectController(capacity);
        int writes = 0, expectedWrites = 0;
        BuildingProjectController.Receipt Start(int i) => controller.Start(Id(i + 1), $"f{i}", "key",
            Steps(i % 4), () => true, _ => writes++, _ => "confirmed", _ => "ready");
        var receipts = new List<BuildingProjectController.Receipt>();
        double tick = 0;
        for (int i = 0; i < capacity; i++)
        {
            var receipt = Start(i); receipts.Add(receipt);
            Assert.Throws<BridgeRejectionException>(() => Start(i + 1));
            if (i > 0) Assert.Same(receipts[0], Start(0)); // Replay cannot replace the active project.
            for (int j = 0; j < Modes[i % 4].Length * 2; j++) controller.Tick(tick++);
            expectedWrites += Modes[i % 4].Length;
            Assert.Equal("completed", receipt.State);
            Assert.Equal(expectedWrites, writes);
            foreach (var previous in receipts) Assert.Same(previous, controller.Inspect(Guid.Parse(previous.ActionId)));
            Assert.Throws<BridgeRejectionException>(() => controller.Existing(Id(1), "changed"));
        }
        Assert.Throws<BridgeRejectionException>(() => Start(capacity));
        Assert.Same(receipts[0], Start(0)); // Replay remains available at capacity.
        controller.Tick(tick); Assert.Equal(expectedWrites, writes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void StoppedOrUnconfirmedVerticalProjectCannotBeBypassed(bool uncertain)
    {
        var controller = new BuildingProjectController(VerticalStairRequest.MaxProjectsPerSession);
        var receipt = controller.Start(Id(1), "first", "key", Steps(3), () => uncertain,
            _ => throw new InvalidOperationException(), _ => "confirmed", _ => "ready");
        controller.Tick(0);
        Assert.Equal(uncertain ? "unconfirmed" : "stopped", receipt.State);
        Assert.Throws<BridgeRejectionException>(() => controller.Existing(Id(2), "next"));
        Assert.Same(receipt, controller.Existing(Id(1), "first"));
    }

    [Fact]
    public void WaitingVerticalProjectBlocksNewIdAndFreshSessionHasNoOldReceipt()
    {
        var controller = new BuildingProjectController(VerticalStairRequest.MaxProjectsPerSession);
        var receipt = controller.Start(Id(1), "first", "key", Steps(3), () => true,
            _ => throw new InvalidOperationException(), _ => "confirmed", _ => "construction");
        controller.Tick(0);
        Assert.Equal("waiting", receipt.State);
        Assert.Throws<BridgeRejectionException>(() => controller.Existing(Id(2), "next"));
        var fresh = new BuildingProjectController(VerticalStairRequest.MaxProjectsPerSession);
        Assert.Throws<BridgeRejectionException>(() => fresh.Inspect(Id(1)));
        Assert.Null(fresh.Existing(Id(2), "next"));
    }

    [Fact]
    public void CompletedOrderDoesNotBypassConstructionIsolationBeforeNextProject()
    {
        var controller = new BuildingProjectController(VerticalStairRequest.MaxProjectsPerSession);
        var receipt = controller.Start(Id(1), "first", "key", Steps(0), () => true, _ => { }, _ => "confirmed");
        controller.Tick(0); controller.Tick(1);
        Assert.Equal("completed", receipt.State);
        Assert.Null(controller.Existing(Id(2), "next")); // Ledger readiness is not building readiness.
        Assert.False(ConstructionIsolationPolicy.Allows(true, [(true, false, true, false)]));
        Assert.True(ConstructionIsolationPolicy.Allows(true, [(true, true, false, false)]));
        Assert.False(ConstructionIsolationPolicy.Allows(false, []));
    }
}
