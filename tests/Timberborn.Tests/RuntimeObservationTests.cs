using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class RuntimeObservationTests
{
    [Theory]
    [InlineData(true,false,false,true,"construction_paused")]
    [InlineData(false,false,false,true,"waiting_for_support_completion")]
    [InlineData(null,false,false,true,"waiting_for_support_completion")]
    [InlineData(false,false,false,false,"construction_inactive_unknown")]
    [InlineData(null,false,true,false,"construction_inactive_unknown")]
    [InlineData(false,true,false,true,"materials_not_ready")]
    [InlineData(false,true,true,false,"awaiting_builders_or_work")]
    public void NativeInactivityIsNotManualPause(bool? paused,bool? active,bool? materials,bool support,string expected)
        => Assert.Equal(expected,LargeProjectRecoveryPolicy.ConstructionReason(paused,active,materials,support));

    [Theory]
    [InlineData(false,true,true,false,"construction_center_outside_working_hours")]
    [InlineData(false,true,true,true,"awaiting_builders_or_work")]
    [InlineData(false,true,true,null,"awaiting_builders_or_work")]
    [InlineData(true,true,true,false,"construction_paused")]
    [InlineData(null,true,true,false,"awaiting_builders_or_work")]
    [InlineData(false,true,false,false,"materials_not_ready")]
    [InlineData(false,false,true,false,"construction_inactive_unknown")]
    public void WorkingHoursDoNotOverrideOtherCauses(bool? paused,bool? active,bool? materials,bool? hours,string reason)
        => Assert.Equal(reason,LargeProjectRecoveryPolicy.ConstructionReason(paused,active,materials,false,hours));

    [Fact]
    public void WorkingHoursEvidenceSurvivesCompactAndContradictionsAreRejected()
    {
        var part=JsonSerializer.SerializeToNode(new {index=0,observed="construction",recorded="construction",
            manuallyPaused=false,unfinishedSupportIndices=Array.Empty<int>(),reason="construction_center_outside_working_hours",
            constructionCenterId="11111111-1111-4111-8111-111111111111",constructionCenterWorkingHours=false,
            workingHoursSource="district_center_workplace_component",construction=new{isOn=true,hasMaterialsToResumeBuilding=true}})!.AsObject();
        RuntimeObservationContract.Construction(JsonSerializer.SerializeToElement(part),1);
        var data=new JsonObject{["parts"]=new JsonArray(part),["state"]="blocked",["currentSpeed"]=0,
            ["released"]=false,["closeEligible"]=false,["repairEligible"]=true,["lostConnections"]=0};
        ProjectReportPresentation.ObserveCurrent(data);
        ProjectReportPresentation.Compact("diagnose_large_project",data);
        Assert.Equal("wait_for_construction_working_hours",data["recoveryAdvice"]!["nextStep"]!.GetValue<string>());
        var shown=data["parts"]![0]!;
        Assert.False(shown["constructionCenterWorkingHours"]!.GetValue<bool>());
        Assert.Contains("Feierabend",shown["nextStepExplanation"]!.GetValue<string>());
        shown["constructionCenterWorkingHours"]=true;
        Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Construction(JsonSerializer.SerializeToElement(shown),1));
        shown["constructionCenterWorkingHours"]=false;
        shown["workingHoursSource"]="guessed_from_clock";
        Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Construction(JsonSerializer.SerializeToElement(shown),1));
    }

    [Fact]
    public void MinimumIsUnknownUntilReturnedDecreaseRefusesAndCanBeInvalidated()
    {
        var evidence=new StaffingMinimumObservation();
        evidence.Observe(1);Assert.Null(evidence.MinimumWorkers);
        evidence.DecreaseReturned(2,1);Assert.Null(evidence.MinimumWorkers);
        evidence.DecreaseReturned(1,1);Assert.Equal(1,evidence.MinimumWorkers);
        Assert.Equal("observed_decrease_boundary",evidence.Source);
        evidence.Observe(3);Assert.Equal(1,evidence.MinimumWorkers);
        evidence.Observe(0);Assert.Null(evidence.MinimumWorkers);
        Assert.Equal("unknown_public_api",evidence.Source);
    }

    [Theory]
    [InlineData(null,"unknown_public_api",true)]
    [InlineData(1,"observed_decrease_boundary",true)]
    [InlineData(2,"observed_decrease_boundary",false)]
    [InlineData(0,"observed_decrease_boundary",false)]
    [InlineData(1,"unknown_public_api",false)]
    [InlineData(null,null,false)]
    public void MinimumEvidenceDoesNotInventLimits(int? value,string? source,bool valid)
    {
        if(valid)RuntimeObservationContract.Minimum(value,source,1,4,true);
        else Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Minimum(value,source,1,4,true));
    }

    [Theory]
    [InlineData(true,0,null,false)] [InlineData(false,0,null,true)]
    [InlineData(false,7,null,false)] [InlineData(false,0,7,false)]
    public void PauseSettledRequiresUnlockedObservedPause(bool locked,float speed,int? pending,bool settled)
    {
        var d=new NativeSimulation(speed,1,0,0,PendingSpeed:pending,SpeedLocked:locked,
            SpeedLockSource:"speed_lock_changed_event",SpeedLockReason:"not_exposed_by_public_api",PauseSettled:settled);
        RuntimeObservationContract.Simulation(d,true);
        Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Simulation(d with{PauseSettled=!settled},true));
    }

    [Fact]
    public void MissingInitialEventAndLegacyFieldsRemainUnknown()
    {
        var legacy=new NativeSimulation(0,1,0,0);
        RuntimeObservationContract.Simulation(legacy,false);
        Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Simulation(legacy,true));
        var unknown=legacy with{SpeedLockSource="not_observed",SpeedLockReason="not_exposed_by_public_api"};
        RuntimeObservationContract.Simulation(unknown,true);
        Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Simulation(unknown with{SpeedLocked=false},true));
    }

    [Fact]
    public void RecoveryRejectsFalsePauseClaimAndCompactKeepsItsEvidence()
    {
        var part=JsonSerializer.SerializeToNode(new {index=1,observed="construction",recorded="construction",
            manuallyPaused=false,unfinishedSupportIndices=new[]{0},reason="waiting_for_support_completion",
            construction=new{isOn=false,hasMaterialsToResumeBuilding=false}})!;
        RuntimeObservationContract.Construction(JsonSerializer.SerializeToElement(part),2);
        part["reason"]="construction_paused";
        Assert.Throws<InvalidDataException>(()=>RuntimeObservationContract.Construction(JsonSerializer.SerializeToElement(part),2));
        part["reason"]="waiting_for_support_completion";
        var data=new JsonObject{["parts"]=new JsonArray(part),["state"]="blocked",["released"]=false,["closeEligible"]=false,["repairEligible"]=true};
        ProjectReportPresentation.Compact("diagnose_large_project",data);
        Assert.Equal(1,data["summary"]!["confirmedPresent"]!.GetValue<int>());
        Assert.False(data["parts"]![0]!["manuallyPaused"]!.GetValue<bool>());
        Assert.Equal(0,data["parts"]![0]!["unfinishedSupportIndices"]![0]!.GetValue<int>());
    }
}
