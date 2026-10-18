namespace Timberborn.Backend.Native;

// Optional evidence: old packages remain readable, absence never means an empty cell.
public static class SiteCollisionContract
{
    public static void Validate(SiteCell cell)
    {
        if (cell.Collision is not { } evidence) return;
        bool valid = evidence.ObjectState switch {
            "outside_map" => !cell.InsideMap && evidence.IntersectsTerrain is null &&
                evidence.Objects is null && !evidence.Truncated && !cell.IntersectsObject,
            "observed" => cell.InsideMap && evidence.IntersectsTerrain is not null &&
                evidence.Objects is { Length: <= 32 } objects &&
                cell.IntersectsObject == (objects.Length > 0) &&
                (!evidence.Truncated || objects.Length == 32) &&
                objects.All(o => o is not null && o.Id != Guid.Empty &&
                    (o.Template is null || !string.IsNullOrWhiteSpace(o.Template) && o.Template.Length <= 160)) &&
                objects.Where(o => o.Id is not null).Select(o => o.Id).Distinct().Count() == objects.Count(o => o.Id is not null),
            _ => false
        };
        if (!valid) throw new InvalidDataException("Invalid site collision evidence");
    }
}
