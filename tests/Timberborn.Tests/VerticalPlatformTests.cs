using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.Backend.Native;
using Xunit;

namespace Timberborn.Tests;

public sealed class VerticalPlatformTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void WholeVerticalStructureSubmitsBeforeAnyObservation(bool rejectMiddle)
    {
        var controller=new BuildingProjectController();
        string[] templates=["Stairs.Folktails","Platform.Folktails","Path","Platform.Folktails","Path"];
        var steps=templates.Select(t=>new BuildingProjectController.Step {Template=t}).ToArray();
        int writes=0, reads=0;bool finished=false;
        var receipt=controller.Start(Guid.NewGuid(),"synthetic","key",steps,()=>true,
            _=>{writes++;Assert.Equal(0,reads);if(rejectMiddle && writes==2)throw new BridgeRejectionException("template_locked");},
            _=>{reads++;return finished?"finished":"confirmed";},
            _=>throw new InvalidOperationException("Per-part readiness must not run"));
        controller.Tick(0);Assert.Equal(5,writes);Assert.Equal(0,reads);
        controller.Tick(1);Assert.Equal("ordered",receipt.State);
        finished=true;controller.Tick(2);
        Assert.Equal(rejectMiddle?"stopped":"completed",receipt.State);
        if(rejectMiddle)Assert.Equal("template_locked",steps[1].Reason);
        controller.Tick(3);Assert.Equal(5,writes);
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
