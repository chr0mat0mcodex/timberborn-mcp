using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;
namespace Timberborn.Tests;

public sealed class PathDistrictTests
{
    private static NameValueCollection Query() => new() {
        ["id"]="11111111-1111-1111-1111-111111111111", ["districtId"]="22222222-2222-2222-2222-222222222222",
        ["session"]="33333333-3333-3333-3333-333333333333" };
    [Fact]
    public void RequestRequiresExactDistrictAndSession()
    {
        Assert.True(LogisticsRequest.Handles("/agent-api/v1/path-district"));
        var q = Query();
        Assert.Equal(q["districtId"], LogisticsRequest.Parse("/agent-api/v1/path-district", q).DistrictId);
        q.Remove("districtId");
        Assert.Throws<ArgumentException>(() => LogisticsRequest.Parse("/agent-api/v1/path-district", q));
        q = Query(); q.Add("districtId", q["districtId"]);
        Assert.Throws<ArgumentException>(() => LogisticsRequest.Parse("/agent-api/v1/path-district", q));
    }
    [Theory]
    [InlineData(true)] [InlineData(false)] [InlineData(null)]
    public void ObservationDistinguishesFalseFromUnknown(bool? connected)
    {
        var r = LogisticsRequest.Parse("/agent-api/v1/path-district", Query());
        bool supported = connected.HasValue;
        var e = new BridgeEnvelope<NativePathDistrict>(1, r.Session, DateTimeOffset.UtcNow, "0.29.3",
            new(r.Id, r.DistrictId, "Path", supported, supported, supported ? new Position(10,10,5) : null,
                connected, supported ? "observed" : "unfinished_path", ["synthetic"]));
        NativeClient.ValidatePathDistrict(e, r);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidatePathDistrict(e with {
            Data = e.Data with { DistrictId = r.Id } }, r));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidatePathDistrict(e with {
            Data = e.Data with { Supported = !supported } }, r));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidatePathDistrict(e with {
            SessionId = r.Id }, r));
    }
    [Fact]
    public void CatalogExposesReadOnlyTool()
    {
        var tool = Timberborn.McpServer.LogisticsTools.Catalog().Single(t => t.Name == "inspect_path_district");
        Assert.True(tool.Annotations!.ReadOnlyHint);
        Assert.False(tool.Annotations.DestructiveHint);
        Assert.Contains("districtId", tool.InputSchema.GetProperty("required").EnumerateArray().Select(p => p.GetString()));
    }
}
