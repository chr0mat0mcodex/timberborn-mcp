namespace Timberborn.Backend.Native;

public static class RoadProtectionContract
{
    public static void Validate(NativeRoadProtection? d, bool applied, bool requireBaseline = false, bool requireProbeKinds = false, bool requireConstructionPreview = false, bool requireConstructionDetails = false, bool requireConstructionEndpoints = false, bool requireConstructionQueryControls = false, int maxCandidateCells = 64)
    {
        if (d?.ConstructionAccessPreview is { } construction) ConstructionPreviewContract.Validate(construction);
        if (requireConstructionEndpoints || requireConstructionQueryControls)
        {
            if (d?.ConstructionAccessPreview?.Details is not { } detail) throw new InvalidDataException("Missing endpoint diagnostic");
            ConstructionPreviewContract.ValidateDetails(detail,true,requireConstructionQueryControls);
        }
        if (requireConstructionPreview && d?.ConstructionAccessPreview is null ||
            requireConstructionDetails && d?.ConstructionAccessPreview?.Details is null ||
            d is { Restored: true, ConstructionAccessPreview.Restored: false } ||
            d is { ConstructionAccessPreview: { Status: "observed", LostSites: > 0 } } && (applied || d.Status == "safe"))
            throw new InvalidDataException("Inconsistent construction preview evidence");
        if (d is null || d.Status is not ("safe" or "blocked" or "unknown") || d.Reasons is null || d.Reasons.Length > 8 ||
            d.Reasons.Any(s => string.IsNullOrEmpty(s) || s.Length > 100) || d.Limitations is null || d.Limitations.Length > 16 ||
            d.Limitations.Any(s => string.IsNullOrEmpty(s) || s.Length > 160) || d.CheckedConnections is < 0 or > 16384 ||
            d.LostConnections < 0 || d.LostConnections > d.CheckedConnections || d.Affected is null || d.Affected.Length > 32 ||
            requireBaseline && d.ConnectedBefore is null || d.ConnectedBefore is < 0 ||
            d.ConnectedBefore > d.CheckedConnections || d.LostConnections > d.ConnectedBefore ||
            d.Status == "safe" && d.ConnectedBefore == 0 ||
            requireProbeKinds && (d.RoadProbeCount is null || d.ConstructionProbeCount is null) ||
            d.RoadProbeCount is < 0 or > 4096 || d.ConstructionProbeCount is < 0 or > 4096 ||
            d.RoadProbeCount + d.ConstructionProbeCount > 4096 ||
            d.Affected.Any(a => a is null || a.Id == Guid.Empty || a.Entrance is null || a.DistrictCenter is null) ||
            d.Affected.Any(a => requireProbeKinds && a.Kind is null || a.Kind is not (null or "building_access" or "construction_access" or "road_cell")) ||
            d.Affected.Length != Math.Min(d.LostConnections, 32) || d.AffectedTruncated != (d.LostConnections > d.Affected.Length) ||
            d.CandidateCells is null || d.CandidateCells.Length > maxCandidateCells || d.CandidateCells.Any(c => c is null) ||
            d.Status == "safe" && (!d.Restored || !d.ConstructionCovered || d.LostConnections != 0 || d.CheckedConnections == 0) ||
            d.Status == "blocked" && (!d.Restored || d.LostConnections == 0) || applied && d.Status != "safe")
            throw new InvalidDataException("Invalid road protection evidence");
    }
}
