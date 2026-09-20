namespace Timberborn.Bridge.Core;

public static class BuildingPolicy
{
    public static bool ValidTemplate(string? value) => value is not null && value.Length is >= 1 and <= 160 &&
        value.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '_' or '-');

    public static string[] UnsupportedReasons(string layout, string shape, bool terrainSide, bool developerTool, int cells)
    {
        var reasons = new List<string>();
        // SideLine and TwoSegmentLine describe drag gestures, not a ban on one fixed object.
        // Placement still uses exactly one blueprint footprint and the normal game validators.
        if (layout is not ("Single" or "Rectangle" or "SideLine" or "TwoSegmentLine")) reasons.Add("special_layout");
        if (shape != "Square") reasons.Add("special_tool_shape");
        if (terrainSide) reasons.Add("terrain_side_attachment");
        if (developerTool) reasons.Add("developer_tool");
        if (cells is < 1 or > 64) reasons.Add("geometry_exceeds_64_cells");
        return reasons.ToArray();
    }
}

// One singleton per loaded game. Never evict IDs: uncertain actions must not execute twice.
public sealed class BuildingActionGate
{
    private readonly HashSet<Guid> attempted = new();
    public T Execute<T>(Guid actionId, Func<T> action)
    {
        if (actionId == Guid.Empty) throw new ArgumentException("invalid_action_id");
        if (attempted.Count >= 256 || !attempted.Add(actionId)) throw new InvalidOperationException("action_used_or_session_limit");
        return action();
    }
}
