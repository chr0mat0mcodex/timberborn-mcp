using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class ConstructionIsolationTests
{
    [Fact]
    public void DenseFinishedMapDoesNotHideLateForeignOrUnknownObjects()
    {
        var finished = Enumerable.Repeat((true,true,false,false), 10000);
        Assert.Null(ConstructionIsolationPolicy.Blocker(true,finished));
        Assert.Equal("foreign_construction",ConstructionIsolationPolicy.Blocker(true,finished.Append((true,false,true,false))));
        Assert.Null(ConstructionIsolationPolicy.Blocker(true,finished.Append((true,false,true,true))));
        Assert.Equal("construction_initialization_pending",ConstructionIsolationPolicy.Blocker(true,finished.Append((false,false,false,false))));
        Assert.Equal("construction_state_unknown",ConstructionIsolationPolicy.Blocker(true,finished.Append((true,false,false,false))));
        Assert.Equal("construction_inventory_incomplete",ConstructionIsolationPolicy.Blocker(false,finished));
    }

    [Theory]
    [InlineData("foreign_construction","project_plan_foreign_construction")]
    [InlineData("construction_initialization_pending","project_plan_construction_initialization_pending")]
    [InlineData("construction_inventory_incomplete","project_plan_construction_inventory_incomplete")]
    [InlineData("construction_state_unknown","project_plan_construction_state_unknown")]
    [InlineData("private detail","project_plan_construction_state_unknown")]
    public void IsolationReasonsReachPlanningWithoutInventingForeignSites(string reason,string expected) =>
        Assert.Equal(expected,Assert.Throws<BridgeRejectionException>(() => LargeProjectPlanningPolicy.RequireIsolation(reason)).Code);

    [Theory]
    [InlineData(true, true, false, false, true)]
    [InlineData(true, false, true, false, false)]
    [InlineData(true, false, true, true, true)]
    [InlineData(true, false, false, true, false)]
    [InlineData(true, true, true, true, false)]
    [InlineData(false, true, false, true, false)]
    public void FinishedUnfinishedAndUnknownAreDistinct(bool initialized, bool finished, bool unfinished, bool owned, bool allowed)
        => Assert.Equal(allowed, ConstructionIsolationPolicy.Allows(true, [(initialized, finished, unfinished, owned)]));

    [Fact]
    public void IncompleteInventoryBlocksEvenWithoutVisibleSites()
    {
        Assert.True(ConstructionIsolationPolicy.Allows(true, []));
        Assert.False(ConstructionIsolationPolicy.Allows(false, []));
        // Reachability and pause deliberately have no exemption in this policy.
        Assert.False(ConstructionIsolationPolicy.Allows(true, [(true, true, false, false), (true, false, true, false)]));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OwnSiteCanWaitButCannotAuthorizePlacement(bool independentAppears)
    {
        bool previousFinished = false; bool independent = false; int writes = 0;
        var controller = new BuildingProjectController();
        var steps = new[] { new BuildingProjectController.Step { Template = "Stairs.Folktails" }, new BuildingProjectController.Step { Template = "Path" } };
        var receipt = controller.Start(Guid.NewGuid(), "synthetic", "key", steps,
            () => ConstructionIsolationPolicy.Allows(true, independent ? [(true, false, true, false)] : []),
            _ => writes++, _ => "confirmed", s => s == steps[0] || previousFinished ? "ready" : "construction");
        controller.Tick(0); controller.Tick(1); controller.Tick(2);
        Assert.Equal("waiting", receipt.State); Assert.Equal(1, writes);
        Assert.True(ConstructionIsolationPolicy.Allows(true, [(true, false, true, true)]));
        Assert.False(ConstructionIsolationPolicy.Allows(true, [(true, false, true, false)]));
        independent = independentAppears; previousFinished = true;
        controller.Tick(3); controller.Tick(4);
        Assert.Equal(independentAppears ? "stopped" : "completed", receipt.State);
        Assert.Equal(independentAppears ? 1 : 2, writes);
        controller.Tick(5); Assert.Equal(independentAppears ? 1 : 2, writes);
    }

    [Fact]
    public void NonPlatformStairWaitRequiresNewBridgeVersion()
    {
        var r = VerticalStairRequest.Parse(false, new NameValueCollection {
            ["session"] = "11111111-1111-1111-1111-111111111111", ["actionId"] = "22222222-2222-2222-2222-222222222222" });
        var e = new BridgeEnvelope<NativeProjectExecution>(1, r.Session, DateTimeOffset.UtcNow, "0.32.1",
            new(r.ActionId.ToString("D"), new string('a', 64), "waiting", "awaiting_construction_finished", [
                new(r.ActionId.ToString("D"), "Stairs.Folktails", 10, 10, 3, 0, "confirmed"),
                new("33333333-3333-3333-3333-333333333333", "Path", 10, 9, 4, 0, "pending")], false, ["synthetic_test"]));
        NativeClient.ValidateVerticalReceipt(e, r);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateVerticalReceipt(e with { BridgeVersion = "0.32.0" }, r));
    }
}
