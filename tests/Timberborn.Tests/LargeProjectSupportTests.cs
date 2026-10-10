using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class LargeProjectSupportTests
{
    private static LargeProjectSupportPolicy.Cell Foundation(int x=3)=>new(){X=x,Y=4,Z=6,MatterBelow="GroundOrStackable"};
    private static LargeProjectSupportPolicy.Cell Support(int step=0,int z=5,bool stackable=true)=>new(){X=3,Y=4,Z=z,Step=step,Stackable=stackable};
    [Fact]
    public void FutureSupportDefersOnlyItsDependentCheck()
    {
        Assert.True(LargeProjectSupportPolicy.CanDefer(false,false,[0],[Foundation()],[Support()]));
        Assert.False(LargeProjectSupportPolicy.CanDefer(true,false,[0],[Foundation()],[Support()]));
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,true,[0],[Foundation()],[Support()]));
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,false,[],[Foundation()],[Support()]));
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,false,[1],[Foundation()],[Support()]));
    }
    [Fact]
    public void EveryFoundationNeedsAnExplicitStackableSupportDirectlyBelow()
    {
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,false,[0],[Foundation(),Foundation(4)],[Support()]));
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,false,[0],[Foundation()],[Support(z:4)]));
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,false,[0],[Foundation()],[Support(stackable:false)]));
        var groundOnly=Foundation();groundOnly.MatterBelow="Ground";
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,false,[0],[groundOnly],[Support()]));
        Assert.False(LargeProjectSupportPolicy.CanDefer(false,false,[0],[],[Support()]));
    }
    [Fact]
    public void DeferredPlanNeverBypassesFreshExecutionValidation()
    {
        var ledger=new LargeProjectLedger();const string id="11111111-1111-1111-1111-111111111111";
        ledger.Start(id,new string('a',64),"synthetic",[0,1],["22222222-2222-2222-2222-222222222222","33333333-3333-3333-3333-333333333333"]);
        var placed=new List<int>();
        ledger.Advance(id,0,()=>true,_=>"finished",_=>true,placed.Add);
        var result=ledger.Advance(id,1,()=>true,_=>"finished",_=>false,placed.Add);
        Assert.Equal("stopped",result.State);
        Assert.Equal("native_step_validation_failed",result.Reason);
        Assert.Equal(new[]{0},placed);
    }
}
