using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Xunit;

namespace Timberborn.Tests;

public sealed class VerticalPlatformTests
{
    [Fact]
    public void ConstructionWaitResumesOnlyAfterReadinessWithoutReplay()
    {
        var controller = new BuildingProjectController();
        var steps = new[] { new BuildingProjectController.Step { Template = "Stairs.Folktails" },
            new BuildingProjectController.Step { Template = "Path" } };
        int writes = 0;
        string ready = "ready";
        var receipt = controller.Start(Guid.NewGuid(), "synthetic", "key", steps, () => true,
            _ => writes++, _ => "confirmed", _ => ready);
        controller.Tick(0); controller.Tick(1);
        ready = "construction"; controller.Tick(2); controller.Tick(100);
        Assert.Equal("waiting", receipt.State);
        Assert.Equal("awaiting_construction_finished", receipt.Reason);
        Assert.Equal("pending", steps[1].State);
        Assert.Equal(1, writes);
        ready = "pause"; controller.Tick(101);
        Assert.Equal("awaiting_pause", receipt.Reason);
        Assert.Equal(1, writes);
        ready = "ready"; controller.Tick(102); controller.Tick(103);
        Assert.Equal("completed", receipt.State);
        Assert.Equal(2, writes);
    }

    [Theory]
    [InlineData("construction", false, "construction_phase_timeout")]
    [InlineData("ready", false, "construction_phase_timeout")]
    [InlineData("mismatch", false, "dependency_mismatch")]
    [InlineData("ready", true, "guard_changed")]
    public void WaitingStopsWithoutPlacement(string readiness, bool loseGuard, string reason)
    {
        var controller = new BuildingProjectController();
        int writes = 0; bool guard = true; string ready = "construction";
        var receipt = controller.Start(Guid.NewGuid(), "synthetic", "key",
            [new BuildingProjectController.Step { Template = "Stairs.Folktails" }],
            () => guard, _ => writes++, _ => "confirmed", _ => ready);
        controller.Tick(0);
        ready = readiness; guard = !loseGuard;
        controller.Tick(readiness == "mismatch" || loseGuard ? 1 : 300);
        Assert.Equal("stopped", receipt.State);
        Assert.Equal(reason, receipt.Reason);
        Assert.Equal(0, writes);
        controller.Tick(301);
        Assert.Equal(0, writes);
    }

    [Fact]
    public void ControllerConfirmsFiveStepPlatformSequenceWithoutReplay()
    {
        var controller = new BuildingProjectController();
        var templates = new[] { "Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path" };
        var steps = templates.Select((template, index) => new BuildingProjectController.Step {
            EntityId = Guid.NewGuid().ToString("D"), Template = template, X = index, Y = 10, Z = 3 }).ToArray();
        var placed = new List<string>();
        var id = Guid.NewGuid();
        var receipt = controller.Start(id, "synthetic", new string('a', 64), steps,
            () => true, step => placed.Add(step.EntityId), _ => "confirmed");
        for (int tick = 0; tick < 10; tick++) controller.Tick(tick);
        Assert.Equal("completed", receipt.State);
        Assert.All(receipt.Steps, step => Assert.Equal("confirmed", step.State));
        Assert.Equal(steps.Select(step => step.EntityId), placed);
        Assert.Same(receipt, controller.Existing(id, "synthetic"));
        controller.Tick(11);
        Assert.Equal(5, placed.Count);
    }

    private static NameValueCollection Query() => new() {
        ["session"]="11111111-1111-1111-1111-111111111111",
        ["districtId"]="22222222-2222-2222-2222-222222222222",
        ["actionId"]="33333333-3333-3333-3333-333333333333",
        ["mode"]="stair_platform_pilot",["upperPathCount"]="2",
        ["x"]="10",["y"]="10",["z"]="3",["rotation"]="1" };

    [Theory]
    [InlineData("0")] [InlineData("1")] [InlineData("3")]
    public void PlatformModeRejectsOtherPathCounts(string count)
    {
        var q=Query();q["upperPathCount"]=count;
        Assert.Throws<ArgumentException>(()=>VerticalStairRequest.Parse(true,q));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ReceiptBindsSupportAndUpperPathGeometry(bool wrongSupportHeight)
    {
        var r=VerticalStairRequest.Parse(true,Query());
        Assert.True(r.WithPlatform);
        var e=new BridgeEnvelope<NativeProjectExecution>(1,r.Session,DateTimeOffset.UtcNow,"0.29.2",
            new(r.ActionId.ToString("D"),new string('a',64),"completed","order_and_access_confirmed",[
                new(r.ActionId.ToString("D"),"Stairs.Folktails",10,10,3,1,"confirmed"),
                new("77777777-7777-7777-7777-777777777777","Platform.Folktails",9,10,3,0,"confirmed"),
                new("44444444-4444-4444-4444-444444444444","Path",9,10,4,0,"confirmed"),
                new("55555555-5555-5555-5555-555555555555","Platform.Folktails",8,10,wrongSupportHeight?4:3,0,"confirmed"),
                new("66666666-6666-6666-6666-666666666666","Path",8,10,4,0,"confirmed")],false,["synthetic_test"]));
        if(wrongSupportHeight) Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateVerticalReceipt(e,r));
        else {
            NativeClient.ValidateVerticalReceipt(e,r);
            var status=VerticalStairRequest.Parse(false,new NameValueCollection { ["session"]=r.Session,["actionId"]=r.ActionId.ToString("D") });
            NativeClient.ValidateVerticalReceipt(e,status);
            var waitingSteps = e.Data.Steps.Select((step, index) => step with { State = index == 0 ? "confirmed" : "pending" }).ToArray();
            var waiting = e with { Data = e.Data with { State = "waiting", Reason = "awaiting_construction_finished", Steps = waitingSteps } };
            NativeClient.ValidateVerticalReceipt(waiting, status);
            Assert.Throws<InvalidDataException>(() => NativeClient.ValidateVerticalReceipt(
                waiting with { BridgeVersion = "0.29.1" }, status));
            Assert.Throws<InvalidDataException>(() => NativeClient.ValidateVerticalReceipt(
                waiting with { Data = waiting.Data with { Reason = "unknown_wait" } }, status));
        }
    }
}
