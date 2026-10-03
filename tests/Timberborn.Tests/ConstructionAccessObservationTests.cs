using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class ConstructionAccessObservationTests
{
    private static LogisticsRequest Request() => LogisticsRequest.Parse("/agent-api/v1/building-access", new NameValueCollection
    {
        ["id"] = "11111111-1111-1111-1111-111111111111",
        ["session"] = "22222222-2222-2222-2222-222222222222"
    });
    private static BridgeEnvelope<NativeAccess> Envelope(bool finished, NativeConstructionAccess? accesses) =>
        new(1, Request().Session, DateTimeOffset.UtcNow, "0.31.4",
            new(Request().Id, finished, new(2, 3, 4), false, false, null,
                finished ? null : true, 10, 2, 1, ["synthetic_test"], accesses));

    [Theory]
    [InlineData("observed")] [InlineData("unavailable")] [InlineData("not_construction")]
    public void ActualAccessStatesAreDistinct(string state)
    {
        var e = Envelope(state == "not_construction", new(state,
            state == "observed" ? [new(3, 3, 4), new(2, 4, 3)] : [], false));
        NativeClient.ValidateBuildingAccess(e, Request());
    }
    [Fact]
    public void ExpandedSiteIsExplicitlyUnavailable() =>
        NativeClient.ValidateBuildingAccess(Envelope(false, new("unavailable", [], true)), Request());
    [Fact]
    public void LegacyBridgeDoesNotRequireNewField() =>
        NativeClient.ValidateBuildingAccess(Envelope(false, null) with { BridgeVersion = "0.31.3" }, Request());
    [Theory]
    [InlineData("missing")] [InlineData("empty_observed")] [InlineData("hidden_cells")]
    [InlineData("duplicate")] [InlineData("too_many")] [InlineData("finished")]
    [InlineData("expanded_observed")] [InlineData("unknown_state")]
    public void FalseOrIncompleteAccessEvidenceRejected(string defect)
    {
        NativeConstructionAccess? accesses = defect switch
        {
            "missing" => null,
            "empty_observed" => new("observed", [], false),
            "hidden_cells" => new("unavailable", [new(1, 2, 3)], false),
            "duplicate" => new("observed", [new(1, 2, 3), new(1, 2, 3)], false),
            "too_many" => new("observed", Enumerable.Range(0, 65).Select(i => new Position(i, 2, 3)).ToArray(), false),
            "expanded_observed" => new("observed", [new(1, 2, 3)], true),
            "unknown_state" => new("safe", [], false),
            _ => new("observed", [new(1, 2, 3)], false)
        };
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateBuildingAccess(Envelope(defect == "finished", accesses), Request()));
    }
}
