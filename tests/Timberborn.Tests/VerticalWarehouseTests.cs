using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Xunit;

namespace Timberborn.Tests;

public sealed class VerticalWarehouseTests
{
    private static VerticalStairRequest Request(int rotation = 0, string count = "2") => VerticalStairRequest.Parse(true, new NameValueCollection {
        ["session"] = "11111111-1111-1111-1111-111111111111", ["districtId"] = "22222222-2222-2222-2222-222222222222",
        ["actionId"] = "33333333-3333-3333-3333-333333333333", ["mode"] = "stair_platform_warehouse_pilot",
        ["upperPathCount"] = count, ["x"] = "10", ["y"] = "10", ["z"] = "3", ["rotation"] = rotation.ToString() });

    private static readonly string[] Templates = ["Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path", "Platform.Folktails", "SmallWarehouse.Folktails"];

    [Theory]
    [InlineData("0")] [InlineData("1")] [InlineData("3")]
    public void RequiresExactlyTwoUpperPaths(string count) => Assert.Throws<ArgumentException>(() => Request(count: count));

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void ReceiptBindsSevenStepsAndInwardEntrance(int rotation)
    {
        var r = Request(rotation);
        Assert.True(r.WithWarehouse); Assert.True(r.WithPlatform);
        var direction = rotation switch { 0 => (x: 0, y: -1), 1 => (x: -1, y: 0), 2 => (x: 0, y: 1), _ => (x: 1, y: 0) };
        var steps = Templates.Select((template, i) => new NativeProjectStep(
            i == 0 ? r.ActionId.ToString("D") : Guid.NewGuid().ToString("D"), template,
            10 + direction.x * ((i + 1) / 2), 10 + direction.y * ((i + 1) / 2),
            i == 0 || template == "Platform.Folktails" ? 3 : 4,
            i == 0 ? rotation : i == 6 ? (rotation + 2) % 4 : 0, "confirmed")).ToArray();
        var e = new BridgeEnvelope<NativeProjectExecution>(1, r.Session, DateTimeOffset.UtcNow, "0.32.0",
            new(r.ActionId.ToString("D"), new string('a', 64), "completed", "order_and_access_confirmed", steps, false, ["synthetic_test"]));
        NativeClient.ValidateVerticalReceipt(e, r);
        var status = VerticalStairRequest.Parse(false, new NameValueCollection { ["session"] = r.Session, ["actionId"] = r.ActionId.ToString("D") });
        NativeClient.ValidateVerticalReceipt(e, status);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateVerticalReceipt(e with { BridgeVersion = "0.31.4" }, status));
        foreach (var bad in new[] {
            steps.Take(6).ToArray(),
            steps.Select((s,i) => i == 5 ? s with { Z = 4 } : s).ToArray(),
            steps.Select((s,i) => i == 6 ? s with { Rotation = (s.Rotation + 1) % 4 } : s).ToArray(),
            steps.Select((s,i) => i == 5 ? s with { Template = "Path" } : s).ToArray() })
            Assert.Throws<InvalidDataException>(() => NativeClient.ValidateVerticalReceipt(e with { Data = e.Data with { Steps = bad } }, status));
        NativeClient.ValidateVerticalReceipt(e with { Data = e.Data with { State = "waiting", Reason = "awaiting_construction_finished",
            Steps = steps.Select((s,i) => s with { State = i < 5 ? "confirmed" : "pending" }).ToArray() } }, status);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SevenStepWorkflowWaitsAndDoesNotReplay(bool loseConnection)
    {
        var controller = new BuildingProjectController();
        var id = Guid.NewGuid(); int writes = 0; bool connected = true; string ready = "ready";
        var steps = Templates.Select(t => new BuildingProjectController.Step { Template = t }).ToArray();
        var receipt = controller.Start(id, "synthetic", "key", steps, () => connected, _ => writes++, _ => "confirmed", _ => ready);
        for (int tick = 0; tick < 10; tick++) controller.Tick(tick);
        ready = "construction"; controller.Tick(10); controller.Tick(11);
        Assert.Equal("waiting", receipt.State); Assert.Equal(5, writes);
        connected = !loseConnection; ready = "ready";
        for (int tick = 12; tick < 16; tick++) controller.Tick(tick);
        Assert.Equal(loseConnection ? "stopped" : "completed", receipt.State);
        Assert.Equal(loseConnection ? 5 : 7, writes);
        Assert.Same(receipt, controller.Existing(id, "synthetic")); controller.Tick(17);
        Assert.Equal(loseConnection ? 5 : 7, writes);
    }

    [Fact]
    public void FlatWarehouseScopeIsNotWidened()
    {
        var controller = new BuildingProjectController();
        var steps = Enumerable.Repeat("Path", 5).Append("SmallWarehouse.Folktails")
            .Select(t => new BuildingProjectController.Step { Template = t }).ToArray();
        Assert.Throws<ArgumentException>(() => controller.Start(Guid.NewGuid(), "synthetic", "key", steps, () => true, _ => { }, _ => "confirmed"));
    }
}
