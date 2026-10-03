using System.Collections.Specialized;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class SelectionTests
{
    private const string Session = "11111111-1111-1111-1111-111111111111";
    private static BridgeEnvelope<NativeSelection> Envelope(string state, SelectionTarget? target = null) =>
        new(1, Session, DateTimeOffset.UtcNow, "0.30.0", new(state, target, ["synthetic"]));
    private static SelectionTarget Target() => new(Guid.Parse("22222222-2222-2222-2222-222222222222"),
        "Path", "block_object", new(10, 11, 4), new(10, 4, 11));

    [Theory]
    [InlineData("none")] [InlineData("unsupported")] [InlineData("selected")]
    public void DistinctStatesHaveConsistentTarget(string state)
    {
        var e = Envelope(state, state == "selected" ? Target() : null);
        NativeClient.ValidateSelection(e, Session);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateSelection(e with {
            Data = e.Data with { Target = state == "selected" ? null : Target() } }, Session));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateSelection(e, Target().Id.ToString("D")));
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateSelection(e with { BridgeVersion = "0.29.3" }, Session));
    }
    [Fact]
    public void EntityPositionIsNotInventedGridPosition()
    {
        var target = Target() with { Kind = "entity", GridPosition = null, Template = "BeaverAdult.Folktails" };
        NativeClient.ValidateSelection(Envelope("selected", target), Session);
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateSelection(Envelope("selected",
            target with { GridPosition = new(10, 11, 4) }), Session));
    }
    [Fact]
    public void RejectsInvalidIdentityStateAndGeometry()
    {
        Assert.Throws<InvalidDataException>(() => NativeClient.ValidateSelection(Envelope("hover"), Session));
        var invalid = new[] { Target() with { Id = Guid.Empty }, Target() with { Template = "" },
            Target() with { Kind = "unknown" }, Target() with { GridPosition = null },
            Target() with { GridPosition = new(-1, 0, 0) },
            Target() with { WorldPosition = new(float.NaN, 0, 0) }, Target() with { WorldPosition = null! } };
        foreach (var target in invalid)
            Assert.Throws<InvalidDataException>(() => NativeClient.ValidateSelection(Envelope("selected", target), Session));
    }
    [Fact]
    public void RequestAcceptsOnlyOneNonemptySession()
    {
        var q = new NameValueCollection { ["session"] = Session };
        Assert.Equal("selection", BridgeRequest.Parse("/agent-api/v1/selection", q).Route);
        Assert.Equal(Session, BridgeRequest.Parse("/agent-api/v1/selection", q).Session);
        q.Add("session", Session);
        Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/selection", q));
        foreach (var value in new[] { "", "wrong", Guid.Empty.ToString("D") })
            Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/selection", new() { ["session"] = value }));
        Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/selection", new()));
        Assert.Throws<ArgumentException>(() => BridgeRequest.Parse("/agent-api/v1/selection", new() { ["session"] = Session, ["id"] = Session }));
    }
    [Fact]
    public void ReaderIsAvailableWithoutWriteOptIns()
    {
        var tool = SelectionTools.Reader();
        Assert.Equal("inspect_selection", tool.Name);
        Assert.True(tool.Annotations!.ReadOnlyHint);
        Assert.False(tool.Annotations.DestructiveHint);
        Assert.Equal(new[] { "session" }, tool.InputSchema.GetProperty("required").EnumerateArray().Select(p => p.GetString()));
        Assert.False(tool.InputSchema.GetProperty("additionalProperties").GetBoolean());
    }
}
