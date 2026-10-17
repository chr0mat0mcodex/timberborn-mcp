using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
using Cell=Timberborn.Bridge.Core.AccessRouteSearch.Cell;

namespace Timberborn.Tests;

public sealed class AccessPlanningTests
{
    private const string Session="11111111-1111-1111-1111-111111111111";
    private const string District="22222222-2222-2222-2222-222222222222";
    private static LargeProjectStep Part(int x=1,int z=1)=>new(){Template="Path",X=x,Y=1,Z=z};
    private static AccessRouteSearch.Edge Edge(Cell to,int cost=1)=>new(){To=to,Parts=Enumerable.Range(0,cost).Select(i=>Part(i)).ToArray()};

    [Fact]
    public void FlatRouteDetoursAroundBlockedCellWithoutInventingAnEdge()
    {
        var blocked=new Cell(1,0,1);
        IEnumerable<AccessRouteSearch.Edge> Expand(Cell c)
        {
            foreach(var d in new[]{(1,0),(-1,0),(0,1),(0,-1)})
            {
                var next=new Cell(c.X+d.Item1,c.Y+d.Item2,c.Z);
                if(next.X>=0&&next.X<=2&&next.Y>=0&&next.Y<=1&&!next.Equals(blocked))yield return Edge(next);
            }
        }
        var result=AccessRouteSearch.Find([new(0,0,1)],new HashSet<Cell>{new(2,0,1)},Expand,4);
        Assert.True(result.Found);Assert.Equal(4,result.Edges.Length);
        Assert.DoesNotContain(result.Edges,e=>e.To.Equals(blocked));
        Assert.False(AccessRouteSearch.Find([new(0,0,1)],new HashSet<Cell>{new(2,0,1)},Expand,3).Found);
    }

    [Fact]
    public void ElevatedRouteAccountsForStairAndSupportCosts()
    {
        var start=new Cell(0,0,1);var end=new Cell(2,0,2);
        var parts=new[]{new LargeProjectStep{Template="Stairs.Folktails",X=1,Y=0,Z=1,Rotation=3},
            new LargeProjectStep{Template="Platform.Folktails",X=2,Y=0,Z=1},Part(2,2)};
        IEnumerable<AccessRouteSearch.Edge> Expand(Cell c)=>c.Equals(start)?[new(){To=end,Parts=parts}]:[];
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{end},Expand,3);
        Assert.True(result.Found);Assert.Equal(parts,Assert.Single(result.Edges).Parts);
        Assert.False(AccessRouteSearch.Find([start],new HashSet<Cell>{end},Expand,2).Found);
    }

    [Fact]
    public void SearchStopsAtBudgetAndAlreadyConnectedNeedsNoParts()
    {
        IEnumerable<AccessRouteSearch.Edge> Expand(Cell c)=>[Edge(new(c.X+1,0,1))];
        var result=AccessRouteSearch.Find([new(0,0,1)],new HashSet<Cell>{new(5,0,1)},Expand,32,2);
        Assert.Equal("search_budget_exhausted",result.Reason);Assert.Equal(2,result.Visited);
        var same=AccessRouteSearch.Find([new(0,0,1)],new HashSet<Cell>{new(0,0,1)},Expand,0);
        Assert.True(same.Found);Assert.Empty(same.Edges);
    }

    [Fact]
    public void WeightedSearchChoosesFewerPartsAndKeepsSourceIdentity()
    {
        var start=new Cell(0,0,1);var middle=new Cell(1,0,1);var end=new Cell(2,0,1);
        IEnumerable<AccessRouteSearch.Edge> Expand(Cell c)=>c.Equals(start)?[Edge(end,5),Edge(middle)]:c.Equals(middle)?[Edge(end)]:[];
        var result=AccessRouteSearch.Find([start],new HashSet<Cell>{end},Expand,5);
        Assert.Equal(2,result.Edges.Length);Assert.Equal(start,result.Source);
    }

    private static LargeProjectRequest Request()=>LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-access",
        new NameValueCollection{{"session",Session},{"districtId",District},{"steps",LargeProjectRequest.Encode([Part()])}});

    [Fact]
    public void ReusedTemporarySupportBecomesPermanentWithItsWholeChain()
    {
        var steps=new[]{Part(0),Part(1),Part(2),Part(3)};
        steps[1].DependsOn=[0];steps[2].DependsOn=[1];
        Assert.Equal(new[]{0,1,2},AccessRouteSearch.RequiredParts(steps,[2]));
        Assert.DoesNotContain(3,AccessRouteSearch.RequiredParts(steps,[2]));
        Assert.Throws<ArgumentException>(()=>AccessRouteSearch.RequiredParts(steps,[4]));
    }

    [Fact]
    public void PlanningRequiresBuildingGateButNeverSpeedOrActionId()
    {
        var request=Request();Assert.Equal("plan-access",request.Operation);Assert.Empty(request.ActionId);
        Assert.DoesNotContain(LargeProjectTools.Catalog(false),t=>t.Name=="plan_large_project_access");
        var tool=Assert.Single(LargeProjectTools.Catalog(true,false),t=>t.Name=="plan_large_project_access");
        Assert.False(tool.Annotations!.ReadOnlyHint);
        Assert.True(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/large-project-plan-access",false,false,enableBuildingPlacement:true));
        Assert.False(BridgeHttpServer.MethodAllowed("GET","/agent-api/v1/large-project-plan-access",false,false,enableBuildingPlacement:true));
        Assert.False(BridgeHttpServer.MethodAllowed("POST","/agent-api/v1/large-project-plan-access",false,false));
        var args=JsonSerializer.SerializeToElement(new{session=Session,districtId=District,steps=new[]{new{template="Path",x=1,y=1,z=1,rotation=0,dependsOn=Array.Empty<int>()}}});
        Assert.Equal(LargeProjectRequest.Encode(request.Steps),LargeProjectRequest.Encode(LargeProjectTools.Parse(tool.Name,args).Steps));
    }

    [Fact]
    public void FailedSearchMustNotInventEligibilityOrChangeOriginalTarget()
    {
        var payload=JsonSerializer.SerializeToNode(new{candidateFound=false,reason="no_route_in_region",
            steps=new[]{new{template="Path",x=1,y=1,z=1,rotation=0,dependsOn=Array.Empty<int>()}},plan=(object?)null,
            assessment=new{requestedParts=1,generatedParts=0,visitedNodes=12,regularExecutionAllowed=false,constructionAccess="unproven_until_live_construction"},
            cleanupCandidateIndices=Array.Empty<int>(),limitations=new[]{"bounded_geometric_search_not_complete_route_coverage"}})!.AsObject();
        BridgeEnvelope<JsonElement> Envelope()=>new(1,Session,DateTimeOffset.UtcNow,"0.37.0",JsonSerializer.SerializeToElement(payload));
        NativeClient.ValidateLargeProject(Envelope(),Request());
        payload["candidateFound"]=true;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
        payload["candidateFound"]=false;payload["steps"]![0]!["x"]=8;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
    }
    [Fact]
    public void ReservationEvidenceMustMatchRequestEvenForFailedNativeSearch()
    {
        var q=new NameValueCollection{{"session",Session},{"districtId",District},{"steps",LargeProjectRequest.Encode([Part()])},
            {"reservedAreas","1,1,1,1,1,1"}};
        var request=LargeProjectRequest.Parse("/agent-api/v1/large-project-plan-access",q);
        var payload=JsonSerializer.SerializeToNode(new{candidateFound=false,reason="reserved_area",steps=new[]{Part()},plan=(object?)null,
            assessment=new{requestedParts=1,generatedParts=0,visitedNodes=0,regularExecutionAllowed=false,constructionAccess="unproven_until_live_construction"},
            cleanupCandidateIndices=Array.Empty<int>(),limitations=new[]{"bounded_search"},reservedAreasKey="1,1,1,1,1,1",reservedAreasClear=false},NativeJson.Options)!.AsObject();
        BridgeEnvelope<JsonElement> Envelope()=>new(1,Session,DateTimeOffset.UnixEpoch,"0.51.0",JsonSerializer.SerializeToElement(payload,NativeJson.Options));
        NativeClient.ValidateLargeProject(Envelope(),request);
        payload["reservedAreasKey"]="";
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),request));
        payload.Remove("reservedAreasKey");
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),request));
    }

    [Theory]
    [InlineData("0.68.0")]
    [InlineData("0.69.0")]
    [InlineData("0.70.0")]
    [InlineData("0.71.0")]
    [InlineData("0.72.0")]
    public void SearchDiagnosticsSurviveCompactAndRejectInventedNativeEvidence(string bridgeVersion)
    {
        var point=new{x=2,y=2,z=1};
        var payload=JsonSerializer.SerializeToNode(new{candidateFound=false,reason="search_budget_exhausted",
            steps=new[]{new{template="Path",x=1,y=1,z=1,rotation=0,dependsOn=Array.Empty<int>()}},plan=(object?)null,
            assessment=new{requestedParts=1,generatedParts=0,visitedNodes=2,regularExecutionAllowed=false,constructionAccess="unproven_until_live_construction"},
            cleanupCandidateIndices=Array.Empty<int>(),limitations=new[]{"bounded_search"},
            searchDiagnostics=new{version=1,strategy="a_star_geometry",existingSourceCount=1,observedPathCount=1,
                examinedHeightSources=1,heightExitCount=8,nativeConnectedExits=0,
                sources=new[]{new{position=point,template="Stairs.Folktails",districtConnected=true,heightExitsExamined=true}},sourcesTruncated=false,
                heightExits=Enumerable.Range(0,8).Select(_=>new{from=point,to=new{x=3,y=2,z=2},insideRegion=true,nativeConnected=false,
                    candidateAttempts=0,geometryBlockedAttempts=0,unsupportedAttempts=0}).ToArray(),heightExitsTruncated=false,
                routes=new[]{new{part=0,phase="finished_entrance",reason="search_budget_exhausted",sourceCount=1,targetCount=1,
                    visitedStates=2,generatedStates=3,peakOpenStates=2,closestRemainingCost=1,selectedSource=(object?)null,pathParts=0}},
                evidence="native_existing_edges_and_geometric_candidates_not_build_permission"}})!.AsObject();
        var diagnostics=payload["searchDiagnostics"]!;
        if(bridgeVersion!="0.68.0")
        {
            diagnostics["version"]=2;diagnostics["connectionGraph"]="instant_road";
            foreach(var edge in diagnostics["heightExits"]!.AsArray())edge!["generalConnected"]=false;
        }
        string graph=bridgeVersion is "0.70.0" or "0.71.0" or "0.72.0"?"instant_road_or_registered_landing":"instant_road";
        if(bridgeVersion is "0.70.0" or "0.71.0" or "0.72.0")
        {
            diagnostics["version"]=3;diagnostics["connectionGraph"]=graph;
            diagnostics["evidence"]="native_edges_or_registered_landing_not_build_permission";
            diagnostics["landingPreviewCount"]=0;diagnostics["landingPreviewLimit"]=8;
            foreach(var edge in diagnostics["heightExits"]!.AsArray())
            {edge!["landingCandidate"]=false;edge!["previewLandingConnected"]=null;}
        }
        BridgeEnvelope<JsonElement> Envelope()=>new(1,Session,DateTimeOffset.UnixEpoch,bridgeVersion,JsonSerializer.SerializeToElement(payload));
        NativeClient.ValidateLargeProject(Envelope(),Request());
        if(bridgeVersion!="0.68.0")
        {
            var edge=diagnostics["heightExits"]![0]!;
            // Road-only stair connections are valid even when the general graph
            // reports false; the reverse must never authorize a route candidate.
            edge["nativeConnected"]=true;edge["candidateAttempts"]=1;
            diagnostics["nativeConnectedExits"]=1;
            NativeClient.ValidateLargeProject(Envelope(),Request());
            edge["nativeConnected"]=false;edge["generalConnected"]=true;
            diagnostics["nativeConnectedExits"]=0;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            edge["candidateAttempts"]=0;edge["generalConnected"]=false;
            diagnostics["connectionGraph"]="instant_general";
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            diagnostics["connectionGraph"]=graph;
            edge["generalConnected"]=null;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            edge["generalConnected"]=false;
        }
        if(bridgeVersion is "0.70.0" or "0.71.0" or "0.72.0")
        {
            var edge=diagnostics["heightExits"]![0]!;
            edge["landingCandidate"]=true;edge["previewLandingConnected"]=true;
            edge["candidateAttempts"]=1;diagnostics["landingPreviewCount"]=1;
            NativeClient.ValidateLargeProject(Envelope(),Request());
            diagnostics["landingPreviewCount"]=0;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            diagnostics["landingPreviewCount"]=1;edge["previewLandingConnected"]=null;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            edge["previewLandingConnected"]=false;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            edge["previewLandingConnected"]=true;edge["landingCandidate"]=false;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            edge["previewLandingConnected"]=null;edge["candidateAttempts"]=0;
            diagnostics["landingPreviewCount"]=9;
            Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
            diagnostics["landingPreviewCount"]=1;
        }
        var compact=(JsonObject)payload.DeepClone();
        ProjectReportPresentation.Compact("plan_large_project_access",compact);
        Assert.True(JsonNode.DeepEquals(payload["searchDiagnostics"],compact["searchDiagnostics"]));
        payload["searchDiagnostics"]!["heightExits"]![0]!["candidateAttempts"]=1;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
        payload["searchDiagnostics"]!["heightExits"]![0]!["candidateAttempts"]=0;
        payload["searchDiagnostics"]!["heightExitsTruncated"]=true;
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
        payload.Remove("searchDiagnostics");
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateLargeProject(Envelope(),Request()));
    }

}
