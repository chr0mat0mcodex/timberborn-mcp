using Timberborn.Backend.Native;
using Xunit;
namespace Timberborn.Tests;

public sealed class ConstructionPreviewContractTests
{
    private static NativeConstructionPreview Observed(bool during = true) => new("observed", "range_comparison_only",
        true, true, during ? 0 : 1,
        [new(Guid.Parse("11111111-1111-1111-1111-111111111111"), [new(4,5,3)], true,true,true,during,true,true,true)],
        ["diagnostic_range_comparison_not_build_permission"]);
    private static NativeRoadProtection Roads(NativeConstructionPreview? d) => new("unknown",
        ["construction_and_road_node_coverage_unproven"],4,0,true,false,[],false,[],[],2,1,0,d);
    [Fact]
    public void NoSitesIsUnknownNotAProof()
    {
        var d = new NativeConstructionPreview("unknown","no_existing_construction_sites",null,null,0,[],[]);
        ConstructionPreviewContract.Validate(d);
        var a = BuildAssessment.FromValidation(new("Path",new(10,10,3),0,true,true,true,false,255,[],Roads(d)));
        Assert.Equal("unknown",a.Checks.Single(c => c.Name == "existingConstructionAccess").Status);
        Assert.False(a.RegularExecutionAllowed);
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public void MatchingBaselineReportsScopedComparison(bool during)
    {
        var d = Observed(during); ConstructionPreviewContract.Validate(d);
        var a = BuildAssessment.FromValidation(new("Path",new(10,10,3),0,true,true,true,false,255,[],Roads(d)));
        Assert.Equal(during ? "unknown" : "blocked",a.Decision);
        Assert.False(a.RegularExecutionAllowed);
    }
    [Theory] [InlineData(true)] [InlineData(false)]
    public void MismatchAndFailedRestorationRemainUnknown(bool baselineMismatch)
    {
        var item = Observed().Items[0];
        var d = Observed() with { Status="unknown", BaselineMatches=!baselineMismatch, Restored=baselineMismatch,
            Reason=baselineMismatch ? "construction_range_baseline_mismatch" : "construction_range_not_restored",
            Items=[baselineMismatch ? item with { ObservedBuildersReachable=false,ObservedBuildersReachableAfter=false } : item with { After=false }] };
        ConstructionPreviewContract.Validate(d);
        if (!baselineMismatch) Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(Roads(d),false));
    }
    [Fact]
    public void ForgedOrIncompleteDiagnosticIsRejected()
    {
        var d = Observed();
        foreach (var invalid in new[] { d with { Status="safe" },d with { LostSites=1 },
            d with { Items=[d.Items[0],d.Items[0]] },d with { BaselineMatches=false },d with { Restored=false } })
            Assert.Throws<InvalidDataException>(() => ConstructionPreviewContract.Validate(invalid));
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(Roads(null),false,requireConstructionPreview:true));
    }
    [Fact]
    public void KnownLossCannotBeSafeOrApplied()
    {
        var d = Roads(Observed(false));
        RoadProtectionContract.Validate(d,false);
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(d with { Status="safe",ConstructionCovered=true },false));
        Assert.Throws<InvalidDataException>(() => RoadProtectionContract.Validate(d,true));
    }
}
