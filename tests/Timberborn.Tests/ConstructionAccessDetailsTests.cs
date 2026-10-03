using Timberborn.Backend.Native;
using Xunit;
namespace Timberborn.Tests;

public sealed class ConstructionAccessDetailsTests
{
    private static ConstructionAccessState State() => new(true,true,true,false,false,false,"None");
    private static NativeConstructionDetails Details() => new("observed","per_access_comparison_only",true,true,new(0,0,3),
        [new(new(4,5,3),State(),State(),State(),State(),State())],["occupation_not_navigation_blockage"]);
    private static NativeConstructionPreview Report(NativeConstructionDetails? details) => new("observed","range_comparison_only",true,true,0,
        [new(Guid.Parse("11111111-1111-1111-1111-111111111111"),[new(4,5,3)],true,true,true,true,true,true,true)],[],details);
    private static NativeConstructionDetails EndpointDetails(bool originValid=true, bool targetValid=true)
    {
        var s=State() with {OriginOnActualNavMesh=originValid,TargetOnActualNavMesh=targetValid};
        return Details() with {RangeOrigin=new(1,1,3),OriginSource="district_entrance",OriginOnDistrictRoad=true,
            Accesses=[new(new(4,5,3),s,s,s,s,s)]};
    }
    [Fact]
    public void NewVersionRequiresExplicitEndpointEvidence()
    {
        ConstructionPreviewContract.ValidateDetails(EndpointDetails(),true);
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(Details(),true));
        var d=EndpointDetails();
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(d with {RangeOrigin=null},true));
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(d with {OriginSource="district_center"},true));
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(d with {
            Accesses=[d.Accesses[0] with {During=d.Accesses[0].During with {TargetOnActualNavMesh=null}}]},true));
    }
    private static NativeConstructionDetails QueryDetails()
    {
        var d=EndpointDetails();
        var s=d.Accesses[0].Before with {Connected=false,RoadConnected=false,OriginNeighbourRoadEdges=1,
            ActualRoadPathFound=true,ActualDestinationReachable=true,ActualDistrictRoadSpill=true};
        return d with {Limitations=["actual_queries_not_preview_reachability"],
            Accesses=[new(new(4,5,3),s,s,s,s,s)]};
    }
    [Fact]
    public void NewQueryControlsMustBeCompleteBoundedAndLabelled()
    {
        ConstructionPreviewContract.ValidateDetails(QueryDetails(),true,true);
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(EndpointDetails(),true,true));
        var d=QueryDetails();
        foreach(var invalid in new[] {d with {Limitations=[]},
            d with {Accesses=[d.Accesses[0] with {During=d.Accesses[0].During with {ActualDistrictRoadSpill=null}}]},
            d with {Accesses=[d.Accesses[0] with {During=d.Accesses[0].During with {OriginNeighbourRoadEdges=-1}}]},
            d with {Accesses=[d.Accesses[0] with {During=d.Accesses[0].During with {OriginNeighbourRoadEdges=13}}]} })
            Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(invalid,true,true));
    }
    [Fact]
    public void PositiveActualPathWithNegativeRawEdgeDoesNotProvePreviewSafety()
    {
        var d=QueryDetails();
        // Actual queries remain actual even during preview; no equality with
        // preview range/edge values is asserted or manufactured.
        d=d with {Accesses=[d.Accesses[0] with {During=d.Accesses[0].During with {RangeContains=false}}]};
        ConstructionPreviewContract.ValidateDetails(d,true,true);
        var report=Report(d) with {LostSites=1,Items=[Report(d).Items[0] with {During=false}]};
        ConstructionPreviewContract.Validate(report);
        var roads=new NativeRoadProtection("unknown",["construction_and_road_node_coverage_unproven"],4,0,true,false,[],false,[],[],2,1,0,report);
        var assessment=BuildAssessment.FromValidation(new("Path",new(10,10,3),0,true,true,true,false,255,[],roads));
        Assert.Equal("blocked",assessment.Decision);
        Assert.False(assessment.RegularExecutionAllowed);
        Assert.True(d.Accesses[0].During.ActualRoadPathFound);
        Assert.False(d.Accesses[0].During.RoadConnected);
    }
    [Fact]
    public void QueryControlRestorationAndCommonOriginMustBeConsistent()
    {
        var d=QueryDetails();
        var changed=d.Accesses[0] with {After=d.Accesses[0].After with {ActualRoadPathFound=false}};
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(d with {Accesses=[changed]},true,true));
        ConstructionPreviewContract.ValidateDetails(d with {Status="unknown",Reason="detail_not_restored",Restored=false,Accesses=[changed]},true,true);
        var other=d.Accesses[0] with {Cell=new(5,5,3),During=d.Accesses[0].During with {OriginNeighbourRoadEdges=2}};
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(d with {Accesses=[d.Accesses[0],other]},true,true));
    }
    [Theory] [InlineData(false,true)] [InlineData(true,false)]
    public void InvalidOriginCannotPretendConnectionControlPassed(bool navMesh,bool districtRoad)
    {
        var d=EndpointDetails(navMesh) with {OriginOnDistrictRoad=districtRoad};
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(d,true));
        d=d with {Status="unknown",Reason="detail_origin_invalid"};
        ConstructionPreviewContract.ValidateDetails(d,true);
        ConstructionPreviewContract.Validate(Report(d));
    }
    [Fact]
    public void TargetValidityIsIndependentAndRestorationIncludesIt()
    {
        // A raw connection result is never upgraded to endpoint validity or builder access.
        ConstructionPreviewContract.ValidateDetails(EndpointDetails(targetValid:false),true);
        var d=EndpointDetails();
        d=d with {Status="unknown",Reason="detail_not_restored",Restored=false,
            Accesses=[d.Accesses[0] with {After=d.Accesses[0].After with {TargetOnActualNavMesh=false}}]};
        ConstructionPreviewContract.ValidateDetails(d,true);
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.Validate(Report(d)));
    }
    [Fact]
    public void OccupationAndConnectivityRemainIndependentOfRange()
    {
        var d = Details();
        d = d with { Accesses=[d.Accesses[0] with { During=State() with {
            Connected=false,RoadConnected=false,CandidateCoversCell=true,CandidateOccupied=true,CandidateOccupation="All" } }] };
        ConstructionPreviewContract.Validate(Report(d));
        var roads = new NativeRoadProtection("unknown",["construction_and_road_node_coverage_unproven"],4,0,true,false,[],false,[],[],2,1,0,Report(d));
        var assessment=BuildAssessment.FromValidation(new("Path",new(10,10,3),0,true,true,true,false,255,[],roads));
        Assert.Equal("unknown",assessment.Decision);Assert.False(assessment.RegularExecutionAllowed);
        Assert.True(d.Accesses[0].During.RangeContains);Assert.False(d.Accesses[0].During.Connected);
    }
    [Fact]
    public void CandidateCanCoverAnUnoccupiedTemplateCell()
    {
        var d=Details();
        d=d with { Accesses=[d.Accesses[0] with { During=State() with { CandidateCoversCell=true } }] };
        ConstructionPreviewContract.ValidateDetails(d);
    }
    [Fact]
    public void BaselineMismatchIsReportedUnknown()
    {
        var d=Details();var alternate=State() with { Connected=false };
        d=d with {Status="unknown",Reason="detail_baseline_mismatch",BaselineMatches=false,
            Accesses=[d.Accesses[0] with {PreviewBefore=alternate,PreviewAfter=alternate}]};
        ConstructionPreviewContract.Validate(Report(d));
    }
    [Fact]
    public void FailedDetailRestorationPropagatesToParent()
    {
        var d=Details() with {Status="unknown",Reason="detail_not_restored",Restored=false};
        d=d with {Accesses=[d.Accesses[0] with {After=State() with {Connected=false}}]};
        ConstructionPreviewContract.ValidateDetails(d);
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.Validate(Report(d)));
        ConstructionPreviewContract.Validate(Report(d) with {Status="unknown",Reason="construction_range_not_restored",Restored=false});
    }
    [Fact]
    public void InconsistentDetailSummariesAndCoverageAreRejected()
    {
        var d=Details();
        foreach(var invalid in new[] { d with {BaselineMatches=false},d with {Restored=false},d with {Origin=null},
            d with {Accesses=[d.Accesses[0],d.Accesses[0]]},
            d with {Accesses=[d.Accesses[0] with {During=State() with {CandidateOccupied=true}}]},
            d with {Accesses=Enumerable.Range(0,9).Select(i=>d.Accesses[0] with {Cell=new(i,5,3)}).ToArray()} })
            Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(invalid));
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.Validate(Report(d with {Accesses=[d.Accesses[0] with {Cell=new(6,5,3)}]})));
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.Validate(Report(d with {Accesses=[d.Accesses[0] with {During=State() with {RangeContains=false}}]})));
    }
    [Fact]
    public void MissingOrOutOfPilotDetailsCannotPretendSuccess()
    {
        var d=new NativeConstructionDetails("unknown","detail_site_limit",null,null,null,[],[]);
        ConstructionPreviewContract.ValidateDetails(d);
        Assert.Throws<InvalidDataException>(()=>ConstructionPreviewContract.ValidateDetails(d with {Status="observed"}));
        var roads = new NativeRoadProtection("unknown",["construction_and_road_node_coverage_unproven"],4,0,true,false,[],false,[],[],2,1,0,Report(null));
        Assert.Throws<InvalidDataException>(()=>RoadProtectionContract.Validate(roads,false,requireConstructionDetails:true));
    }
}
