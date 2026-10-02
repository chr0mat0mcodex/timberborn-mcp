namespace Timberborn.Bridge.Core;

/// <summary>Compare native district membership without approximating the game's road graph.</summary>
public static class RoadProtectionPolicy
{
    public static int[] LostConnections(bool[] before, bool[] preview)
    {
        if (before.Length != preview.Length) throw new ArgumentException("incomplete_navigation_sample");
        return Enumerable.Range(0, before.Length).Where(i => before[i] && !preview[i]).ToArray();
    }

    public static string Decision(bool baselineMatches, bool restored, bool constructionCovered, int lost)
    {
        if (lost < 0) throw new ArgumentException();
        if (!baselineMatches || !restored) return "unknown";
        if (lost > 0) return "blocked";
        return constructionCovered ? "safe" : "unknown";
    }
}
