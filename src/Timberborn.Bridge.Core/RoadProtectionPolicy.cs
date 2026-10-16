namespace Timberborn.Bridge.Core;

/// <summary>Compare native district membership without approximating the game's road graph.</summary>
public static class RoadProtectionPolicy
{
    // These are observation targets, not a replacement for the game's collision world.
    public static bool NeedsObservation(bool unfinished, bool district, bool path, bool accessible) =>
        unfinished || district || path || accessible;

    public static T[] CaptureCandidates<T>(IEnumerable<T> objects, Func<T, bool> needsObservation) =>
        objects.Where(needsObservation).ToArray();

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
