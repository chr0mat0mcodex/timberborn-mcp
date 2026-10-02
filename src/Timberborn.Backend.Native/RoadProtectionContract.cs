namespace Timberborn.Backend.Native;

public static class RoadProtectionContract
{
    public static void Validate(NativeRoadProtection? d, bool applied, bool requireBaseline = false)
    {
        if (d is null || d.Status is not ("safe" or "blocked" or "unknown") || d.Reasons is null || d.Reasons.Length > 8 ||
            d.Reasons.Any(s => string.IsNullOrEmpty(s) || s.Length > 100) || d.Limitations is null || d.Limitations.Length > 16 ||
            d.Limitations.Any(s => string.IsNullOrEmpty(s) || s.Length > 160) || d.CheckedConnections is < 0 or > 16384 ||
            d.LostConnections < 0 || d.LostConnections > d.CheckedConnections || d.Affected is null || d.Affected.Length > 32 ||
            requireBaseline && d.ConnectedBefore is null || d.ConnectedBefore is < 0 ||
            d.ConnectedBefore > d.CheckedConnections || d.LostConnections > d.ConnectedBefore ||
            d.Status == "safe" && d.ConnectedBefore == 0 ||
            d.Affected.Any(a => a is null || a.Id == Guid.Empty || a.Entrance is null || a.DistrictCenter is null) ||
            d.Affected.Length != Math.Min(d.LostConnections, 32) || d.AffectedTruncated != (d.LostConnections > d.Affected.Length) ||
            d.CandidateCells is null || d.CandidateCells.Length > 64 || d.CandidateCells.Any(c => c is null) ||
            d.Status == "safe" && (!d.Restored || !d.ConstructionCovered || d.LostConnections != 0 || d.CheckedConnections == 0) ||
            d.Status == "blocked" && (!d.Restored || d.LostConnections == 0) || applied && d.Status != "safe")
            throw new InvalidDataException("Invalid road protection evidence");
    }
}
