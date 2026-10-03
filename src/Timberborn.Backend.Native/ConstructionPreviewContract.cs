namespace Timberborn.Backend.Native;

public sealed record ConstructionPreviewItem(Guid Id, Position[] AccessCells, bool ObservedBuildersReachable,
    bool Before, bool PreviewBefore, bool During, bool After, bool PreviewAfter, bool ObservedBuildersReachableAfter);
public sealed record NativeConstructionPreview(string Status, string Reason, bool? BaselineMatches,
    bool? Restored, int LostSites, ConstructionPreviewItem[] Items, string[] Limitations, NativeConstructionDetails? Details = null);
public sealed record ConstructionAccessState(bool RangeContains, bool Connected, bool RoadConnected, bool ExistingOccupied,
    bool CandidateCoversCell, bool CandidateOccupied, string CandidateOccupation,
    bool? OriginOnActualNavMesh = null, bool? TargetOnActualNavMesh = null,
    int? OriginNeighbourRoadEdges = null, bool? ActualRoadPathFound = null,
    bool? ActualDestinationReachable = null, bool? ActualDistrictRoadSpill = null);
public sealed record ConstructionAccessDetail(Position Cell, ConstructionAccessState Before, ConstructionAccessState PreviewBefore,
    ConstructionAccessState During, ConstructionAccessState After, ConstructionAccessState PreviewAfter);
public sealed record NativeConstructionDetails(string Status, string Reason, bool? BaselineMatches, bool? Restored,
    Position? Origin, ConstructionAccessDetail[] Accesses, string[] Limitations,
    Position? RangeOrigin = null, string? OriginSource = null, bool? OriginOnDistrictRoad = null);
public static class ConstructionPreviewContract
{
    public static void Validate(NativeConstructionPreview? d)
    {
        if (d?.Details is { } details) ValidateDetails(details);
        if (d is null || d.Status is not ("unknown" or "observed") ||
            d.Reason is not ("no_existing_construction_sites" or "construction_site_limit" or "requires_single_finished_district" or
                "unsupported_construction_access" or "construction_access_limit" or "construction_range_unavailable" or
                "captured" or "construction_range_not_restored" or "construction_range_baseline_mismatch" or "range_comparison_only") ||
            d.Items is null || d.Items.Length > 32 || d.Items.Any(i => i is null || i.Id == Guid.Empty || i.AccessCells is null ||
                i.AccessCells.Length is < 1 or > 64 || i.AccessCells.Any(p => p is null)) ||
            d.Items.Select(i => i.Id).Distinct().Count() != d.Items.Length ||
            d.LostSites != d.Items.Count(i => i.Before && !i.During) ||
            d.Status == "observed" && (d.Items.Length == 0 || d.BaselineMatches != true || d.Restored != true || d.Reason != "range_comparison_only") ||
            d.Items.Length > 0 && (d.BaselineMatches != d.Items.All(i => i.Before == i.PreviewBefore && i.Before == i.ObservedBuildersReachable) ||
                d.Restored != (d.Items.All(i => i.Before == i.After && i.PreviewBefore == i.PreviewAfter && i.ObservedBuildersReachable == i.ObservedBuildersReachableAfter) && d.Details?.Restored != false)) ||
            d.Items.Length == 0 && (d.BaselineMatches is not null || d.Restored is not null) ||
            d.Items.Length > 0 && (d.Status == "observed") != (d.BaselineMatches == true && d.Restored == true) ||
            d.Limitations is null || d.Limitations.Length > 16 || d.Limitations.Any(s => string.IsNullOrEmpty(s) || s.Length > 160))
            throw new InvalidDataException("Invalid construction preview comparison");
        if (d.Details is { Accesses.Length: > 0 } detail && (d.Items.Length != 1 ||
            !detail.Accesses.Select(a => a.Cell).SequenceEqual(d.Items[0].AccessCells) ||
            detail.Accesses.Any(a => a.Before.RangeContains) != d.Items[0].Before ||
            detail.Accesses.Any(a => a.PreviewBefore.RangeContains) != d.Items[0].PreviewBefore ||
            detail.Accesses.Any(a => a.During.RangeContains) != d.Items[0].During ||
            detail.Accesses.Any(a => a.After.RangeContains) != d.Items[0].After ||
            detail.Accesses.Any(a => a.PreviewAfter.RangeContains) != d.Items[0].PreviewAfter))
            throw new InvalidDataException("Inconsistent per-access range evidence");
    }
    public static void ValidateDetails(NativeConstructionDetails d, bool requireEndpoints = false, bool requireQueryControls = false)
    {
        static bool Valid(ConstructionAccessState s) => s is not null && s.CandidateOccupation is { Length: > 0 and <= 160 } &&
            !s.CandidateOccupation.Any(char.IsControl) && (!s.CandidateOccupied || s.CandidateCoversCell) &&
            (s.CandidateCoversCell || !s.CandidateOccupied && s.CandidateOccupation == "None");
        static bool EmptyCandidate(ConstructionAccessState s) => !s.CandidateCoversCell && !s.CandidateOccupied && s.CandidateOccupation == "None";
        if (d.Status is not ("unknown" or "observed") || d.Reason is not ("no_existing_construction_sites" or "detail_not_sampled" or
            "detail_site_limit" or "detail_access_limit" or "captured" or "construction_range_unavailable" or "detail_unavailable" or
            "detail_not_restored" or "detail_baseline_mismatch" or "detail_origin_unavailable" or "detail_origin_invalid" or "per_access_comparison_only") ||
            d.Accesses is null || d.Accesses.Length > 8 || d.Accesses.Any(a => a is null || a.Cell is null ||
                !Valid(a.Before) || !Valid(a.PreviewBefore) || !Valid(a.During) || !Valid(a.After) || !Valid(a.PreviewAfter)) ||
            d.Accesses.Select(a => a.Cell).Distinct().Count() != d.Accesses.Length ||
            d.Limitations is null || d.Limitations.Length > 16 || d.Limitations.Any(s => string.IsNullOrEmpty(s) || s.Length > 160))
            throw new InvalidDataException("Invalid per-access construction evidence");
        if (d.Accesses.Length == 0)
        {
            if (d.Status != "unknown" || d.BaselineMatches is not null || d.Restored is not null || d.Origin is not null)
                throw new InvalidDataException("Empty per-access evidence must remain unknown");
            return;
        }
        bool baseline = d.Accesses.All(a => a.Before == a.PreviewBefore);
        bool restored = d.Accesses.All(a => a.Before == a.After && a.PreviewBefore == a.PreviewAfter);
        bool endpoints = requireEndpoints || requireQueryControls || d.OriginSource is not null;
        if (endpoints && (d.RangeOrigin is null || d.OriginSource != "district_entrance" || d.OriginOnDistrictRoad is null ||
            d.Accesses.SelectMany(a => new[] {a.Before,a.PreviewBefore,a.During,a.After,a.PreviewAfter})
                .Any(s => s.OriginOnActualNavMesh is null || s.TargetOnActualNavMesh is null)))
            throw new InvalidDataException("Missing native endpoint evidence");
        var states = d.Accesses.SelectMany(a => new[] {a.Before,a.PreviewBefore,a.During,a.After,a.PreviewAfter}).ToArray();
        bool controls = requireQueryControls || states.Any(s => s.OriginNeighbourRoadEdges is not null ||
            s.ActualRoadPathFound is not null || s.ActualDestinationReachable is not null || s.ActualDistrictRoadSpill is not null);
        if (controls && (states.Any(s => s.OriginNeighbourRoadEdges is null or < 0 or > 12 ||
                s.ActualRoadPathFound is null || s.ActualDestinationReachable is null || s.ActualDistrictRoadSpill is null) ||
            !d.Limitations.Contains("actual_queries_not_preview_reachability") ||
            Enumerable.Range(0,5).Any(i => d.Accesses.Select(a => new[] {a.Before,a.PreviewBefore,a.During,a.After,a.PreviewAfter}[i].OriginNeighbourRoadEdges).Distinct().Count() != 1)))
            throw new InvalidDataException("Invalid actual query or neighbour control evidence");
        bool originValid = !endpoints || d.OriginOnDistrictRoad == true && d.Accesses.All(a => a.Before.OriginOnActualNavMesh == true);
        string reason = !restored ? "detail_not_restored" : !baseline ? "detail_baseline_mismatch" : !originValid ? "detail_origin_invalid" : "per_access_comparison_only";
        if (d.Origin is null || d.BaselineMatches != baseline || d.Restored != restored || d.Reason != reason ||
            (d.Status == "observed") != (baseline && restored && originValid) || d.Accesses.Any(a => !EmptyCandidate(a.Before) ||
                !EmptyCandidate(a.PreviewBefore) || !EmptyCandidate(a.After) || !EmptyCandidate(a.PreviewAfter)))
            throw new InvalidDataException("Inconsistent per-access construction comparison");
    }
}
