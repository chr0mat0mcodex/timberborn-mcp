using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class BuilderWorkforceTests
{
    private const string District="11111111-1111-4111-8111-111111111111";
    private const string Hut="22222222-2222-4222-8222-222222222222";
    private const string Other="33333333-3333-4333-8333-333333333333";
    private static BuilderWorkplaceEvidence Workplace(string id,bool? hours,int? assigned=4,bool? paused=false,bool? running=false,string? district=District) => new() {
        Id=id,Template=id==District?"DistrictCenter.Folktails":"BuildersHut.Folktails",DistrictId=district,
        AssignedWorkers=assigned,Paused=paused,WorkingHours=hours,AnyJobRunning=running
    };
    private static BuilderWorkforceEvidence Evidence(params BuilderWorkplaceEvidence[] items) => new() {
        DistrictId=District,Source=BuilderWorkforcePolicy.Source,Complete=true,Workplaces=items
    };

    [Theory]
    [InlineData(true,false,true)]
    [InlineData(false,true,true)]
    [InlineData(false,false,false)]
    [InlineData(null,false,null)]
    public void HutCanOverrideCenterOffHours(bool? hours,bool running,bool? expected)
        => Assert.Equal(expected,BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,hours,running:running))));

    [Fact]
    public void ForeignInactiveAndUnknownWorkplacesDoNotInventActivityOrOffHours()
    {
        Assert.False(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,true,district:Other))));
        Assert.False(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,true,paused:true))));
        Assert.False(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,true,assigned:0))));
        Assert.Null(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false,assigned:0),Workplace(Hut,false,paused:true))));
        Assert.Null(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,false,district:null))));
        Assert.Null(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,false,assigned:null))));
        Assert.Null(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,false,paused:null))));
        Assert.Null(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(Hut,false,running:null))));
        Assert.Null(BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(Hut,false))));
        var partial=Evidence(Workplace(District,false));partial.Complete=false;
        Assert.Null(BuilderWorkforcePolicy.Evaluate(partial));
        partial.Workplaces=[Workplace(District,false),Workplace(Hut,true)];
        Assert.True(BuilderWorkforcePolicy.Evaluate(partial));
    }

    [Fact]
    public void DuplicateOrInvalidEvidenceIsRejected()
    {
        Assert.Throws<InvalidDataException>(()=>BuilderWorkforcePolicy.Evaluate(Evidence(Workplace(District,false),Workplace(District,false))));
        var e=Evidence(Workplace(District,false));e.Workplaces[0].AssignedWorkers=-1;
        Assert.Throws<InvalidDataException>(()=>BuilderWorkforcePolicy.Evaluate(e));
    }

    [Theory]
    [InlineData(true,"awaiting_builders_or_work")]
    [InlineData(false,"builders_outside_working_hours")]
    [InlineData(null,"awaiting_builders_or_work")]
    public void ContractChecksAggregateAndCompactKeepsIndividualSources(bool? hutHours,string reason)
    {
        var e=Evidence(Workplace(District,false),Workplace(Hut,hutHours));
        e.WorkingHours=BuilderWorkforcePolicy.Evaluate(e);
        Assert.Equal(reason,LargeProjectRecoveryPolicy.ConstructionReason(false,true,true,false,e.WorkingHours,true));
        var part=JsonSerializer.SerializeToNode(new {index=0,observed="construction",recorded="construction",reason,
            manuallyPaused=false,unfinishedSupportIndices=Array.Empty<int>(),constructionCenterId=District,
            constructionCenterWorkingHours=false,workingHoursSource="district_center_workplace_component",
            builderWorkforce=e,construction=new{isOn=true,hasMaterialsToResumeBuilding=true}},NativeJson.Options)!.AsObject();
        RuntimeObservationContract.Construction(JsonSerializer.SerializeToElement(part),1);
        var data=new JsonObject{["parts"]=new JsonArray(part),["currentSpeed"]=0,["lostConnections"]=0,
            ["closeEligible"]=false,["repairEligible"]=true,["released"]=false,["state"]="blocked"};
        ProjectReportPresentation.ObserveCurrent(data);
        ProjectReportPresentation.Compact("diagnose_large_project",data);
        Assert.Equal(2,part["builderWorkforce"]!["workplaces"]!.AsArray().Count);
        Assert.Equal(hutHours==false?"wait_for_construction_working_hours":"check_workers_and_work_progress",
            part["nextStep"]!.GetValue<string>());
        part["builderWorkforce"]!["workingHours"]=e.WorkingHours==false?true:false;
        Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Construction(JsonSerializer.SerializeToElement(part),1));
    }
}
