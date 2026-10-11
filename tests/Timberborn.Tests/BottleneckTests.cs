using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class BottleneckTests
{
    private const string Session="11111111-1111-1111-1111-111111111111";
    private const string Id="22222222-2222-2222-2222-222222222222";
    private const string Endpoint="33333333-3333-3333-3333-333333333333";
    private static GoodFlowEvidence Flow()=>new(){Good="SyntheticInput",Role="ingredient",Amount=2,GlobalAll=10,GlobalAvailable=10,
        MatchingEndpoints=1,ReachableEndpoints=1,ReachableExamples=[Endpoint]};
    [Fact]
    public void InventoryDirectionIsCheckedBeforeCallingNativeRoleServices()
    {
        bool Absent()=>throw new InvalidOperationException("Native role service absent");
        Assert.False(InventoryEndpointPolicy.Matches(true,false,true,Absent,Absent));
        Assert.False(InventoryEndpointPolicy.Matches(false,true,false,Absent,Absent));
        Assert.False(InventoryEndpointPolicy.Matches(false,false,false,Absent,Absent));
        Assert.True(InventoryEndpointPolicy.Matches(true,true,false,()=>true,Absent));
        Assert.True(InventoryEndpointPolicy.Matches(false,false,true,Absent,()=>true));
        Assert.False(InventoryEndpointPolicy.Matches(true,true,true,()=>false,Absent));
        Assert.False(InventoryEndpointPolicy.Matches(false,true,true,Absent,()=>false));
    }
    private static NativeOperation Operation(GoodFlowEvidence? flow=null)=>new(Id,"SyntheticFactory.IronTeeth",true,false,
        new(1,2,2,true,false,true),new(true,"SyntheticRecipe",false,false,true,false,true,0),[],[],[],
        new(true,true,true,true,50,100,true),Bottleneck:new(){State="observed",DistrictConnected=true,EntranceBlocked=false,
            Complete=true,CandidateBuildings=1,InspectedBuildings=1,Goods=[flow??Flow()]});
    private static BuildingQueryRequest Request(int count)=>BuildingQueryRequest.Parse("/agent-api/v1/building-operations",new(){
        ["session"]=Session,["ids"]=string.Join(",",Enumerable.Range(1,count).Select(n=>$"{n:D8}-0000-0000-0000-000000000001")),["diagnose"]="true"});

    [Fact]
    public void ExplicitWireNamesSurviveDefaultSerializerAndPreserveNestedEvidence()
    {
        var source=Operation().Bottleneck!;
        // Native envelope uses default naming, unlike our former web-only fixtures.
        string json=JsonSerializer.Serialize(source.ToWire());
        var decoded=JsonSerializer.Deserialize<BottleneckEvidence>(json,NativeJson.Options)!;
        BottleneckContract.Validate(decoded);
        Assert.Equal("observed",decoded.State);
        Assert.Equal(source.CandidateBuildings,decoded.CandidateBuildings);
        Assert.Equal("SyntheticInput",Assert.Single(decoded.Goods).Good);
        Assert.Equal(new[]{Endpoint},decoded.Goods[0].ReachableExamples);
        var wrongCase=JsonSerializer.Deserialize<BottleneckEvidence>(JsonSerializer.Serialize(source),NativeJson.Options)!;
        Assert.Throws<InvalidDataException>(()=>BottleneckContract.Validate(wrongCase));
    }

    [Fact]
    public void FailedObservationNamesStageWithoutClaimingAnEconomicBottleneck()
    {
        var evidence=new BottleneckEvidence {State="unavailable",FailureStage="local_inventory"};
        BottleneckContract.Validate(evidence);
        var findings=BottleneckAssessment.Read(Operation() with{Bottleneck=evidence});
        Assert.Equal("local_inventory",Assert.Single(findings).Code);
        Assert.Equal("unknown",findings[0].Severity);
        evidence.State="observed";
        Assert.Throws<InvalidDataException>(()=>BottleneckContract.Validate(evidence));
    }

    [Fact]
    public void GermanAdviceIsNotDoubleEncoded()
    {
        var findings=BottleneckAssessment.Read(Operation() with{Bottleneck=new(){State="recipe_missing"}});
        Assert.Contains("ausgew\u00e4hltem",Assert.Single(findings).NextStep);
        Assert.DoesNotContain("\u00c3",findings[0].NextStep);
    }

    [Fact]
    public void EightBuildingLimitIsIndependentOfCheapThirtyTwoBuildingOverview()
    {
        Assert.True(Request(8).Diagnose);
        Assert.Throws<ArgumentException>(()=>Request(9));
        var tool=BuildingQueryTools.Catalog().Single(t=>t.Name=="inspect_building_operations");
        Assert.True(tool.Annotations!.ReadOnlyHint);
        Assert.True(tool.InputSchema.GetProperty("properties").TryGetProperty("diagnose",out _));
    }
    [Fact]
    public void FindingsCombineDeliveryStaffAndPowerWithoutClaimingProduction()
    {
        var operation=Operation();BottleneckContract.Validate(operation.Bottleneck!);
        var assessment=OperationAssessmentBuilder.Read(operation);
        Assert.False(assessment.ProductionProven);
        Assert.Contains(assessment.Findings!,f=>f.Code=="input_missing_source_available"&&f.Good=="SyntheticInput");
        Assert.Contains(assessment.Findings!,f=>f.Code=="understaffed");
        Assert.Contains(assessment.Findings!,f=>f.Code=="network_power_deficit");
        Assert.DoesNotContain(assessment.Findings!,f=>f.Code=="global_available_shortage");
    }
    [Fact]
    public void GlobalShortageAndGoodsElsewhereAreDifferentEvidence()
    {
        var flow=Flow();flow.GlobalAvailable=0;flow.GlobalAll=10;flow.ReachableEndpoints=0;flow.ReachableExamples=[];flow.DisconnectedEndpoints=1;
        var result=BottleneckAssessment.Read(Operation(flow));
        Assert.Contains(result,f=>f.Code=="global_available_shortage"&&f.NextStep.Contains("Reservierungen"));
        Assert.Contains(result,f=>f.Code=="matching_endpoints_disconnected");
        flow.GlobalAll=0;
        Assert.Contains(BottleneckAssessment.Read(Operation(flow)),f=>f.Code=="global_available_shortage"&&f.NextStep.Contains("nachproduzieren"));
    }
    [Fact]
    public void IncompleteScanOrAmbiguousAccessNeverClaimsNoRoute()
    {
        var flow=Flow();flow.ReachableEndpoints=0;flow.ReachableExamples=[];flow.UnknownEndpoints=1;
        var operation=Operation(flow);
        Assert.Contains(BottleneckAssessment.Read(operation),f=>f.Code=="route_coverage_unknown");
        Assert.DoesNotContain(BottleneckAssessment.Read(operation),f=>f.Code=="matching_endpoints_disconnected");
        operation.Bottleneck!.CandidateBuildings=129;operation.Bottleneck.InspectedBuildings=128;operation.Bottleneck.Complete=false;
        BottleneckContract.Validate(operation.Bottleneck);
        Assert.Contains(BottleneckAssessment.Read(operation),f=>f.Code=="endpoint_scan_incomplete");
    }
    [Fact]
    public void FullOutputRecommendsExistingStorageBeforeConstruction()
    {
        var flow=Flow();flow.Role="product";
        var operation=Operation(flow);operation=operation with{Manufacturing=operation.Manufacturing! with{OutputSpace=false}};
        Assert.Contains(BottleneckAssessment.Read(operation),f=>f.Code=="output_full_destination_available");
        flow.ReachableEndpoints=0;flow.ReachableExamples=[];flow.MatchingEndpoints=0;
        Assert.Contains(BottleneckAssessment.Read(operation),f=>f.Code=="no_eligible_storage");
    }
    [Fact]
    public void FuelIsDiagnosedSeparatelyAndInactivePowerIsNotBrokenConnection()
    {
        var flow=Flow();flow.Role="fuel";
        var operation=Operation(flow);operation=operation with{
            Manufacturing=operation.Manufacturing! with{HasIngredients=true,ConsumesFuel=true,HasFuel=false},
            Power=operation.Power! with{Active=false,Powered=false}};
        var findings=BottleneckAssessment.Read(operation);
        Assert.Contains(findings,f=>f.Good==flow.Good&&f.Area=="transport");
        Assert.Contains(findings,f=>f.Code=="consumer_inactive");
        Assert.DoesNotContain(findings,f=>f.Code=="consumer_not_powered"||f.Code=="network_power_deficit");
    }
    [Fact]
    public void ReadyFactoryWithConsumedInputsDoesNotInventShortage()
    {
        var flow=Flow();flow.GlobalAvailable=0;flow.GlobalAll=0;
        var operation=Operation(flow);operation=operation with{Manufacturing=operation.Manufacturing! with{HasIngredients=true,Ready=true}};
        Assert.DoesNotContain(BottleneckAssessment.Read(operation),f=>f.Good is not null);
    }
    [Theory]
    [InlineData("counts")][InlineData("examples")][InlineData("complete")][InlineData("null")]
    public void InvalidEvidenceRejected(string defect)
    {
        var e=Operation().Bottleneck!;
        if(defect=="counts")e.Goods[0].ReachableEndpoints=2;
        if(defect=="examples")e.Goods[0].ReachableExamples=["not-an-id"];
        if(defect=="complete")e.Complete=false;
        if(defect=="null")e.Goods=null!;
        Assert.Throws<InvalidDataException>(()=>BottleneckContract.Validate(e));
    }
    [Theory]
    [InlineData(false)][InlineData(true)]
    public async Task OneNativeReadReturnsCompactEvidenceOrRejectsMissingEvidence(bool missing)
    {
        var operation=Operation();if(missing)operation=operation with{Bottleneck=null};
        using var handler=new Reply(new OperationBatch([new(Id,"observed",operation)],1,[]));
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var args=JsonSerializer.SerializeToElement(new{ids=new[]{Id},session=Session,diagnose=true});
        if(missing)await Assert.ThrowsAsync<InvalidDataException>(()=>BuildingQueryTools.Invoke(client,"inspect_building_operations",args,TestContext.Current.CancellationToken));
        else {
            var result=await BuildingQueryTools.Invoke(client,"inspect_building_operations",args,TestContext.Current.CancellationToken);
            Assert.NotNull(result["data"]!["items"]![0]!["summary"]!["bottleneck"]);
            Assert.NotNull(result["data"]!["items"]![0]!["summary"]!["assessment"]!["findings"]);
        }
        Assert.Equal(1,handler.Calls);Assert.Contains("diagnose=true",handler.Url);
    }
    [Theory]
    [InlineData("ok")][InlineData("missing")][InlineData("wrong_id")][InlineData("unavailable")]
    public async Task RunInspectionKeepsHistoricalProofSeparateFromCurrentDiagnosis(string mode)
    {
        var proof=new BuildingProductionState {Id=Id,Template="SyntheticFactory.IronTeeth",Recipe="SyntheticRecipe",
            CompletedCycles=1,ProvenGameHours=25,ObservedGameHours=25,Blockers=[],SeenBlockers=["ingredients_not_ready"]};
        var batch=new OperationBatch([new(mode=="wrong_id"?Endpoint:Id,"observed",Operation())],1,[]);
        var receipt=new NativeSimulationRun(Endpoint,"completed","building_production_proven",24,26,25,1,0,3,60,3,0,true,false,true,
            BuildingIds:[Id],BuildingProduction:[proof],BuildingProductionReached:true,
            CurrentBuildingOperations:mode is "missing" or "unavailable"?null:batch,
            BottleneckObservedGameHours:mode=="missing"?null:30,
            BottleneckState:mode=="missing"?null:mode=="unavailable"?"unavailable":"observed");
        using var handler=new Reply(receipt);using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var q=new NameValueCollection{["session"]=Session,["runId"]=Endpoint};
        var r=SimulationRunRequest.Parse("/agent-api/v1/simulation-run",q);
        if(mode is "missing" or "wrong_id")
            await Assert.ThrowsAsync<InvalidDataException>(()=>client.SimulationRun(r,q,TestContext.Current.CancellationToken));
        else {
            var result=(await client.SimulationRun(r,q,TestContext.Current.CancellationToken)).Data;
            Assert.True(result.BuildingProductionReached);
            Assert.Equal(25d,result.BuildingProduction![0].ProvenGameHours);
            Assert.Equal(30d,result.BottleneckObservedGameHours);
            Assert.Equal(new[]{"ingredients_not_ready"},result.BuildingProduction[0].SeenBlockers);
            if(mode=="ok")Assert.NotNull(result.CurrentBuildingOperations!.Items[0].Operation!.Assessment!.Findings);
            else Assert.Null(result.CurrentBuildingOperations);
        }
    }
    [Theory]
    [InlineData(8,true)][InlineData(32,false)]
    public async Task FullBatchLimitsPreserveOrderAndMissingBuildingState(int count,bool diagnose)
    {
        var ids=Enumerable.Range(1,count).Select(n=>$"{n:D8}-0000-0000-0000-000000000001").ToArray();
        var items=ids.Select((id,index)=>index==count-1?new OperationBatchItem(id,"not_found",null):
            new OperationBatchItem(id,"observed",Operation() with{Id=id,Bottleneck=diagnose?Operation().Bottleneck:null})).ToArray();
        using var handler=new Reply(new OperationBatch(items,1,[]));
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var args=JsonSerializer.SerializeToElement(new{session=Session,ids,diagnose});
        var result=await BuildingQueryTools.Invoke(client,"inspect_building_operations",args,TestContext.Current.CancellationToken);
        var returned=result["data"]!["items"]!.AsArray();
        Assert.Equal(count,returned.Count);
        Assert.Equal(ids,returned.Select(x=>x!["id"]!.GetValue<string>()));
        Assert.Equal("not_found",returned[count-1]!["state"]!.GetValue<string>());
        Assert.Null(returned[count-1]!["summary"]);
        Assert.Equal(diagnose,returned[0]!["summary"]!["bottleneck"] is not null);
        Assert.Equal(1,handler.Calls);
    }

    [Theory]
    [InlineData("ninth")][InlineData("wrong_type")][InlineData("duplicate")]
    public async Task InvalidDiagnosisArgumentsNeverReachTheGame(string defect)
    {
        var ids=defect=="ninth"?Enumerable.Range(1,9).Select(n=>$"{n:D8}-0000-0000-0000-000000000001").ToArray():
            defect=="duplicate"?new[]{Id,Id}:new[]{Id};
        var args=JsonSerializer.SerializeToElement(new{session=Session,ids,diagnose=defect=="wrong_type"?(object)"true":true});
        using var handler=new Reply(new{});using var client=new NativeClient(new(8081,new string('a',64)),handler);
        await Assert.ThrowsAsync<ArgumentException>(()=>BuildingQueryTools.Invoke(client,"inspect_building_operations",args,TestContext.Current.CancellationToken));
        Assert.Equal(0,handler.Calls);
    }

    [Theory]
    [InlineData(0,false,"no_assigned_workers")]
    [InlineData(1,false,"outside_working_hours")]
    [InlineData(1,null,"working_hours_unknown")]
    public void PersonnelAndHoursDoNotInventDeliveryOrProduction(int assigned,bool? hours,string code)
    {
        var operation=Operation() with{Paused=true,Workplace=new(assigned,1,1,false,false,hours)};
        var findings=BottleneckAssessment.Read(operation);
        Assert.Contains(findings,f=>f.Code==code);
        Assert.Contains(findings,f=>f.Code=="building_paused");
        Assert.False(OperationAssessmentBuilder.Read(operation).ProductionProven);
    }

    [Fact]
    public void UnknownPowerAndAccessStayUnknownInsteadOfClaimingDisconnection()
    {
        var operation=Operation() with{Power=new(true,true,false,false,null,null,true)};
        operation.Bottleneck!.DistrictConnected=null;
        var findings=BottleneckAssessment.Read(operation);
        Assert.Contains(findings,f=>f.Code=="network_unknown"&&f.Severity=="unknown");
        Assert.Contains(findings,f=>f.Code=="district_access_unknown"&&f.Severity=="unknown");
        Assert.DoesNotContain(findings,f=>f.Code=="building_access_blocked"||f.Code=="consumer_not_powered");
    }

    [Theory]
    [InlineData("unfinished")][InlineData("not_manufactory")][InlineData("recipe_missing")]
    public void UnsupportedOperationIsExplicitAndHasNoInventedEconomicCause(string state)
    {
        var evidence=new BottleneckEvidence{State=state};
        BottleneckContract.Validate(evidence);
        var finding=Assert.Single(BottleneckAssessment.Read(Operation() with{Bottleneck=evidence}));
        Assert.Equal(state,finding.Code);Assert.Equal("unknown",finding.Severity);Assert.Null(finding.Good);
    }

    private sealed class Reply(object data):HttpMessageHandler
    {
        public int Calls;public string Url="";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Calls++;Url=request.RequestUri!.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(
                new BridgeEnvelope<object>(1,Session,DateTimeOffset.UnixEpoch,"0.43.0",data),NativeJson.Options))});
        }
    }
}
