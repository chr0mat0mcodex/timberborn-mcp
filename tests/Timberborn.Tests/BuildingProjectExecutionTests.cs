using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Timberborn.Backend.Native;
using Xunit;
namespace Timberborn.Tests;

public sealed class BuildingProjectExecutionTests
{
    private static readonly Guid Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static BuildingProjectController.Step[] Steps() => [
        new() {EntityId="11111111-1111-1111-1111-111111111111",Template="Path"},
        new() {EntityId=Id.ToString("D"),Template="SmallWarehouse.Folktails"}];
    private static NameValueCollection Query() => new() { ["template"]="SmallWarehouse.Folktails",
        ["session"]="11111111-1111-1111-1111-111111111111",["districtId"]="22222222-2222-2222-2222-222222222222",
        ["x"]="0",["y"]="0",["z"]="1",["width"]="3",["height"]="2",["rotation"]="0",
        ["optionIndex"]="0",["planKey"]=new string('a',64),["actionId"]=Id.ToString("D"),["mode"]="development_pilot" };
    [Theory]
    [InlineData("valid",true)] [InlineData("three_roads",false)] [InlineData("geometry",false)]
    [InlineData("entrance",false)] [InlineData("world_changed",false)] [InlineData("locked",false)]
    [InlineData("road_invalid",false)] [InlineData("prefix_loss",false)] [InlineData("blocked",false)]
    [InlineData("not_restored",false)] [InlineData("loss",false)] [InlineData("other_unknown",false)]
    [InlineData("missing_road_evidence",false)]
    public void PilotWaivesOnlyTheExplicitlyApprovedEvidenceGap(string defect,bool expected)
    {
        Assert.Equal(expected,BuildingProjectPilotPolicy.Allows(defect=="three_roads"?3:2,
            defect!="geometry",defect!="entrance",defect!="world_changed",defect=="locked",
            defect=="missing_road_evidence"?[]:[true,defect!="road_invalid"],[0,defect=="prefix_loss"?1:0],
            defect=="blocked"?"blocked":"unknown",defect!="not_restored",defect=="loss"?1:0,
            [defect=="other_unknown"?"navigation_coverage_unknown":"construction_and_road_node_coverage_unproven"]));
    }
    [Theory]
    [InlineData("mode","normal")] [InlineData("template","Lodge.Folktails")] [InlineData("actionId","")]
    [InlineData("optionIndex","5")] [InlineData("session","bad")]
    public void ScopeAndExplicitModeRequired(string key,string value)
    { var q=Query();q[key]=value;Assert.Throws<ArgumentException>(()=>BuildingProjectExecutionRequest.Parse(true,q)); }
    [Fact]
    public void StrictQueryAndSessionRouting()
    {
        var q=Query(); var r=BridgeRequest.Parse("/agent-api/v1/building-project-execute",q);
        Assert.Equal(q["session"],r.Session);Assert.NotNull(r.ProjectExecution);Assert.Equal(13,q.Count);
        q.Add("mode","development_pilot");Assert.Throws<ArgumentException>(()=>BuildingProjectExecutionRequest.Parse(true,q));
    }
    [Fact]
    public void HttpAndMcpGates()
    {
        const string route="/agent-api/v1/building-project-execute";
        Assert.False(BridgeHttpServer.MethodAllowed("GET",route,false,false,enableBuildingPlacement:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST",route,false,false));
        Assert.True(BridgeHttpServer.MethodAllowed("POST",route,false,false,enableBuildingPlacement:true));
        Assert.DoesNotContain(BuildingTools.Catalog(false),t=>t.Name=="execute_building_project_pilot");
        Assert.Contains(BuildingTools.Catalog(false),t=>t.Name=="inspect_building_project"&&t.Annotations!.ReadOnlyHint==true);
        Assert.Contains(BuildingTools.Catalog(true),t=>t.Name=="execute_building_project_pilot"&&t.Annotations!.ReadOnlyHint==false);
    }
    [Fact]
    public void DelayedNavigationAndDuplicateNeverRepeatPlacement()
    {
        var c=new BuildingProjectController();int writes=0;bool ready=false;
        var result=c.Start(Id,"fingerprint","key",Steps(),()=>true,_=>writes++,_=>ready?"confirmed":"pending");
        c.Tick(0);Assert.Equal(1,writes);c.Tick(1);Assert.Equal(1,writes);
        Assert.Same(result,c.Existing(Id,"fingerprint"));
        Assert.Throws<BridgeRejectionException>(()=>c.Existing(Id,"different"));
        Assert.Throws<BridgeRejectionException>(()=>c.Existing(Guid.NewGuid(),"fingerprint"));
        ready=true;c.Tick(2);Assert.Equal(1,writes);c.Tick(3);Assert.Equal(2,writes);
        c.Tick(4);Assert.Equal("completed",result.State);c.Tick(100);Assert.Equal(2,writes);
    }
    [Fact]
    public void TimeoutRetainsUnconfirmedPathAndNeverStartsBuilding()
    {
        var c=new BuildingProjectController();int writes=0;
        var r=c.Start(Id,"f","k",Steps(),()=>true,_=>writes++,_=>"pending");
        c.Tick(0);c.Tick(5);c.Tick(10);
        Assert.Equal("unconfirmed",r.State);Assert.Equal("confirmation_timeout",r.Reason);
        Assert.Equal("pending",r.Steps[1].State);Assert.Equal(1,writes);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PauseOrRoadGuardChangeStopsBeforeNextMutation(bool afterPath)
    {
        var c=new BuildingProjectController();bool guard=true;int writes=0;
        var r=c.Start(Id,"f","k",Steps(),()=>guard,_=>writes++,_=>"confirmed");
        if(afterPath) { c.Tick(0);c.Tick(1); }
        guard=false;c.Tick(2);c.Tick(3);
        Assert.Equal("stopped",r.State);Assert.Equal(afterPath?1:0,writes);
        Assert.Equal("guard_changed",r.Reason);
    }
    [Fact]
    public void ThrowingPlacementConsumesAttemptAndCannotReplay()
    {
        var c=new BuildingProjectController();int writes=0;
        var r=c.Start(Id,"f","k",Steps(),()=>true,_=>{writes++;throw new InvalidOperationException();},_=>"confirmed");
        c.Tick(0);c.Tick(1);
        Assert.Equal("unconfirmed",r.State);Assert.Equal(1,writes);
        Assert.Same(r,c.Start(Id,"f","k",Steps(),()=>true,_=>writes++,_=>"confirmed"));
        c.Tick(2);Assert.Equal(1,writes);
    }
    [Fact]
    public void WrongObjectStopsImmediately()
    {
        var c=new BuildingProjectController();int writes=0;
        var r=c.Start(Id,"f","k",Steps(),()=>true,_=>writes++,_=>"mismatch");
        c.Tick(0);c.Tick(0.1);Assert.Equal("object_mismatch",r.Reason);Assert.Equal(1,writes);
    }
    [Fact]
    public void LedgerIsSessionLocalAndAbsentStatusDoesNotCreateWork()
    {
        var c=new BuildingProjectController();Assert.Throws<BridgeRejectionException>(()=>c.Inspect(Id));
        c.Tick(0);Assert.Null(c.Existing(Id,"f"));
    }
    private static BridgeEnvelope<NativeProjectExecution> Receipt() => new(1,Query()["session"]!,DateTimeOffset.UtcNow,"0.25.0",
        new(Id.ToString("D"),new string('a',64),"completed","order_and_access_confirmed",
            [new(Id.ToString("D"),"SmallWarehouse.Folktails",0,1,1,0,"confirmed")],false,["synthetic_test"]));
    [Fact]
    public void CompletedReceiptIsAnOrderNotProvenConstructionPreflight() =>
        NativeClient.ValidateProjectExecution(Receipt(),BuildingProjectExecutionRequest.Parse(true,Query()));
    [Theory]
    [InlineData("session")] [InlineData("key")] [InlineData("completed")] [InlineData("proof")] [InlineData("id")] [InlineData("outside")]
    public void FalseSuccessAndMisboundReceiptsRejected(string defect)
    {
        var e=Receipt();e=defect switch {
            "session"=>e with{SessionId=Guid.NewGuid().ToString("D")},
            "key"=>e with{Data=e.Data with{PlanKey=new string('b',64)}},
            "completed"=>e with{Data=e.Data with{Steps=[e.Data.Steps[0] with{State="pending"}]}},
            "proof"=>e with{Data=e.Data with{ConstructionPreflightProven=true}},
            "outside"=>e with{Data=e.Data with{Steps=[e.Data.Steps[0] with{X=100}]}},
            _=>e with{Data=e.Data with{ActionId=Guid.NewGuid().ToString("D")}}
        };
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateProjectExecution(e,BuildingProjectExecutionRequest.Parse(true,Query())));
    }
}
