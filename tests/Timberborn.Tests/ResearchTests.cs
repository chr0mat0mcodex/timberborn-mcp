using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class ResearchTests
{
    [Theory]
    [InlineData(5,10,true,false,true,"cost_changed")]
    [InlineData(10,9,true,false,true,"insufficient_points")]
    [InlineData(10,20,false,false,true,"unavailable")]
    [InlineData(10,20,true,true,true,"already_unlocked")]
    [InlineData(10,20,true,false,false,"not_unlockable")]
    public void RejectionAndNoopNeverInvokeUnlock(int expected,int points,bool available,bool unlocked,bool unlockable,string outcome)
    {
        int calls=0;
        Assert.Equal(outcome,ResearchTransaction.Execute(expected,10,available,()=>points,()=>unlocked,()=>unlockable,()=>calls++));
        Assert.Equal(0,calls);
    }
    [Fact]
    public void SuccessfulUnlockDebitsExactlyOnceAndRepeatedCallIsNoop()
    {
        int points=25,calls=0;bool unlocked=false;
        string Act()=>ResearchTransaction.Execute(10,10,true,()=>points,()=>unlocked,()=>true,()=>{calls++;points-=10;unlocked=true;});
        Assert.Equal("applied",Act());Assert.Equal(15,points);
        Assert.Equal("already_unlocked",Act());Assert.Equal(15,points);Assert.Equal(1,calls);
    }
    [Theory]
    [InlineData(false,10)]
    [InlineData(true,0)]
    [InlineData(true,11)]
    public void UnexpectedPostStateIsUnconfirmedWithoutCompensation(bool unlocked,int debit)
    {
        int points=20,calls=0;bool state=false;
        Assert.Equal("unconfirmed",ResearchTransaction.Execute(10,10,true,()=>points,()=>state,()=>true,()=>{calls++;points-=debit;state=unlocked;}));
        Assert.Equal(1,calls);Assert.Equal(20-debit,points);
    }
    [Fact]
    public void ExceptionNeverTriggersRetry()
    {
        int calls=0;
        Assert.Throws<InvalidOperationException>(()=>ResearchTransaction.Execute(10,10,true,()=>20,()=>false,()=>true,()=>{calls++;throw new InvalidOperationException();}));
        Assert.Equal(1,calls);
    }
    [Theory]
    [InlineData("GET",true,false,false)]
    [InlineData("POST",false,false,false)]
    [InlineData("POST",true,true,false)]
    [InlineData("POST",true,false,true)]
    public void ResearchRequiresOwnGateAndBodylessPost(string method,bool enabled,bool body,bool expected)=>
        Assert.Equal(expected,BridgeHttpServer.MethodAllowed(method,"/agent-api/v1/unlock-building",body,true,true,true,true,true,true,true,true,true,true,enabled));
    [Fact]
    public void CatalogHasReaderAndOptInActionWithReasoning()
    {
        Assert.Contains(NativeTools.Catalog(),t=>t.Name=="inspect_research");
        Assert.DoesNotContain(NativeTools.Catalog(),t=>t.Name=="unlock_building");
        var tool=Assert.Single(NativeTools.Catalog(enableResearch:true),t=>t.Name=="unlock_building");
        Assert.True(tool.InputSchema.GetProperty("properties").TryGetProperty("reasoning",out _));
    }
    [Fact]
    public void ParserRejectsDuplicatesUnknownKeysAndStaleFormat()
    {
        var q=new NameValueCollection{{"template","Inventor.Folktails"},{"session",Guid.NewGuid().ToString("D")},{"expectedCost","10"}};
        Assert.True(BridgeRequest.Parse("/agent-api/v1/unlock-building",q).Research!.Write);
        q.Add("expectedCost","10");Assert.Throws<ArgumentException>(()=>ResearchRequest.Parse(true,q));
        q.Set("expectedCost","-1");Assert.Throws<ArgumentException>(()=>ResearchRequest.Parse(true,q));
        q.Set("expectedCost","10");q.Set("session",Guid.Empty.ToString());Assert.Throws<ArgumentException>(()=>ResearchRequest.Parse(true,q));
        var read=new NameValueCollection{{"offset","0"},{"limit","32"}};
        Assert.Equal(32,ResearchRequest.Parse(false,read).Limit);
        read.Add("cheat","true");Assert.Throws<ArgumentException>(()=>ResearchRequest.Parse(false,read));
    }
}
