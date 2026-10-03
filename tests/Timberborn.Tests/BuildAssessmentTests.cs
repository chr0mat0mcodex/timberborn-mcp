using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;
namespace Timberborn.Tests;

public sealed class BuildAssessmentTests
{
    private const string Session = "11111111-1111-1111-1111-111111111111";
    private static NativeRoadProtection Roads() => new("unknown",
        ["construction_and_road_node_coverage_unproven"],4,0,true,false,[],false,[],[],2,1,0);
    private static NativeValidation Single() => new("Path",new(10,10,3),0,true,true,true,false,255,[],Roads());
    private static BuildCheck Check(BuildAssessment a, string name) => a.Checks.Single(c => c.Name == name);
    [Fact]
    public void NoLossIsScopedEvidenceNotRegularBuildPermission()
    {
        var a = BuildAssessment.FromValidation(Single());
        Assert.Equal("unknown",a.Decision); Assert.False(a.RegularExecutionAllowed);
        Assert.Equal("passed",Check(a,"placement").Status);
        Assert.Equal("passed",Check(a,"existingDistrictConnections").Status);
        Assert.Equal("passed",Check(a,"previewRestoration").Status);
        foreach (var name in new[] { "existingConstructionAccess","candidateEntrance","newConstructionAccess","newFinishedAccess" })
            Assert.Equal("unknown",Check(a,name).Status);
        Assert.Contains("not_complete_construction_safety",a.Scope);
    }
    [Fact]
    public void LossRetainsAffectedObjectsAndBlockedDecision()
    {
        var affected = new NativeRoadAffected(Guid.Parse("22222222-2222-2222-2222-222222222222"),new(1,2,3),new(4,5,3),"road_cell");
        var roads = Roads() with { Status="blocked",Reasons=["existing_district_access_lost"],LostConnections=1,Affected=[affected] };
        var a = BuildAssessment.FromValidation(Single() with { RoadProtection=roads });
        Assert.Equal("blocked",a.Decision); Assert.Equal("failed",Check(a,"existingDistrictConnections").Status);
        Assert.Equal(affected,Assert.Single(a.Affected)); Assert.False(a.RegularExecutionAllowed);
    }
    [Theory]
    [InlineData("missing")] [InlineData("baseline_mismatch")] [InlineData("empty_baseline")]
    public void MissingOrIncomparableBaselineIsNeverReportedPassed(string defect)
    {
        var roads = defect switch { "missing" => null, "empty_baseline" => Roads() with { ConnectedBefore=0 },
            _ => Roads() with { Reasons=["navigation_preview_baseline_mismatch"] } };
        var a = BuildAssessment.FromValidation(Single() with { RoadProtection=roads });
        Assert.Equal("unknown",Check(a,"existingDistrictConnections").Status); Assert.False(a.RegularExecutionAllowed);
    }
    [Fact]
    public void InvalidGeometryAndFailedCleanupAreBlocking()
    {
        Assert.Equal("blocked",BuildAssessment.FromValidation(Single() with { Valid=false }).Decision);
        var a = BuildAssessment.FromValidation(Single() with { Valid=null,NoPersistentChangeObserved=false,SessionLocked=true });
        Assert.Equal("blocked",a.Decision); Assert.Equal("failed",Check(a,"previewRestoration").Status);
    }
    private static NativeProjectValidation Project() => new("SmallWarehouse.Folktails",new string('a',64),0,
        new(new(0,1,1),0,new(0,0,1),new(1,0,1),[new(0,0,1)],[new(1,0,1),new(0,0,1)],false,[],new string('a',64)),
        true,[true],[0],true,Roads(),true,false,15,false,[]);
    [Fact]
    public void JointEntranceEvidenceDoesNotBecomeActualAccessEvidence()
    {
        var a = BuildAssessment.FromProject(Project());
        Assert.Equal("passed",Check(a,"candidateEntrance").Status);
        Assert.Equal("finished_preview",Check(a,"candidateEntrance").Phase);
        Assert.Equal("unknown",Check(a,"newConstructionAccess").Status);
        Assert.Equal("unknown",Check(a,"newFinishedAccess").Status);
        Assert.False(a.RegularExecutionAllowed);
        Assert.Equal("blocked",BuildAssessment.FromProject(Project() with { PreviewEntranceConnected=false }).Decision);
    }
    [Fact]
    public void PrefixLossOrInvalidRoadBlocksDespitePositiveFinalPreview()
    {
        var a = BuildAssessment.FromProject(Project() with { RoadStepLostConnections=[1] });
        Assert.Equal("blocked",a.Decision);
        Assert.Equal("existing_connection_lost_in_road_prefix",Check(a,"existingDistrictConnections").Reason);
        Assert.Empty(a.Affected); // The old bridge exposes no per-prefix object IDs.
        Assert.Equal("blocked",BuildAssessment.FromProject(Project() with { RoadValid=[false] }).Decision);
    }
    [Fact]
    public void EvenSafeRoadEvidenceDoesNotProveNewBuildingAccess()
    {
        var a = BuildAssessment.FromValidation(Single() with { RoadProtection=Roads() with { Status="safe",ConstructionCovered=true,Reasons=[] } });
        Assert.Equal("unknown",a.Decision); Assert.False(a.RegularExecutionAllowed);
    }
    [Fact]
    public void ExistingValidationCatalogExplainsAssessmentAndRetainsOptIn()
    {
        foreach (var name in new[] { "validate_building","validate_building_project" }) {
            var tool=Timberborn.McpServer.BuildingTools.Catalog(true).Single(t=>t.Name==name);
            Assert.Contains("assessment",tool.Description);
            Assert.Contains("regularExecutionAllowed=false",tool.Description);
            Assert.DoesNotContain(Timberborn.McpServer.BuildingTools.Catalog(false),t=>t.Name==name);
        }
    }
    [Fact]
    public async Task ClientDerivesReportOnlyAfterExistingValidation()
    {
        var r=BridgeRequest.Parse("/agent-api/v1/building-validation",new NameValueCollection {
            ["template"]="Path",["x"]="10",["y"]="10",["z"]="3",["rotation"]="0",["session"]=Session });
        var raw=Single() with { Assessment=new("safe",true,"invented",[],[],false,[]) };
        var e=new BridgeEnvelope<NativeValidation>(1,Session,DateTimeOffset.UtcNow,"0.30.0",raw);
        using var client=new NativeClient(new(8081,new string('a',64)),new Handler(JsonSerializer.Serialize(e,NativeJson.Options)));
        var result=await client.Validate(r,TestContext.Current.CancellationToken);
        Assert.Equal("unknown",result.Data.Assessment!.Decision);
        Assert.False(result.Data.Assessment.RegularExecutionAllowed);
        Assert.Equal(JsonSerializer.Serialize(raw.RoadProtection,NativeJson.Options),
            JsonSerializer.Serialize(result.Data.RoadProtection,NativeJson.Options));
        using var invalid=new NativeClient(new(8081,new string('a',64)),new Handler(JsonSerializer.Serialize(e with { SessionId="22222222-2222-2222-2222-222222222222" },NativeJson.Options)));
        await Assert.ThrowsAsync<InvalidDataException>(()=>invalid.Validate(r,TestContext.Current.CancellationToken));
    }
    private sealed class Handler(string json):HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json)});
    }
}
