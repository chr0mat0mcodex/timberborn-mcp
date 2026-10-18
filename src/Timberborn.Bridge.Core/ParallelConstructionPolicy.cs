namespace Timberborn.Bridge.Core;

// Separation is only a geometry gate. Native access preservation is independently required.
public static class ParallelConstructionPolicy
{
    public readonly struct Cell
    {
        public readonly int X, Y;
        public Cell(int x, int y) { X = x; Y = y; }
    }

    public static bool Separated(Cell[] proposed, Cell[] existing) =>
        proposed.Length > 0 && existing.Length > 0 &&
        proposed.All(a => existing.All(b => Math.Abs((long)a.X - b.X) > 1 || Math.Abs((long)a.Y - b.Y) > 1));

    public static bool Preserved(bool[] before, bool[] previewBefore, bool[] during, bool repair = false) =>
        before.Length > 0 && before.Length == previewBefore.Length && before.Length == during.Length &&
        before.SequenceEqual(previewBefore) && (repair || before.Any(v => v)) &&
        !before.Where((v, i) => v && !during[i]).Any();

    public static bool KnownPart(string recorded, string observed) =>
        (recorded is "pending" or "rejected" or "construction" or "finished" or "awaiting_access") &&
        (LargeProjectRecoveryPolicy.Resolved(recorded, observed) ||
         (recorded is "construction" or "finished" or "awaiting_access") && observed == "construction");

    public static bool KnownRepairPart(string recorded, string observed) =>
        recorded is not ("unconfirmed" or "unknown") &&
        (KnownPart(recorded, observed) ||
         (recorded is "construction" or "finished" or "awaiting_access") &&
         observed is "construction_unreachable" or "finished_access_missing");

    public static bool RetainsReachability(bool repair, bool previouslyReachable, bool nowReachable) =>
        nowReachable || repair && !previouslyReachable;

    public static bool RepairAccessRestored(string recorded, string observed) =>
        KnownRepairPart(recorded, observed) &&
        (LargeProjectRecoveryPolicy.Resolved(recorded, observed) || observed == "construction");
}
