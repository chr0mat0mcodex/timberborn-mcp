using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record BuildingOption(string Template, bool Available, bool Unlocked, bool Supported,
    string[] UnsupportedReasons, string Layout, string ToolShape, string ToolGroup, Position Size, Position? Entrance,
    bool PlaceFinished, NativeCost[] Costs);
public sealed record NativeBuildingCatalog(string Faction, int Offset, int Limit, int Total, BuildingOption[] Items,
    bool HasMore, string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeBuildingCatalog>> BuildingCatalog(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "building-catalog") throw new ArgumentException();
        var result = await Get<NativeBuildingCatalog>($"building-catalog?offset={r.Offset}&limit={r.Limit}", ct);
        var d = result.Data;
        if (result.BridgeVersion is not ("0.14.0" or "0.14.1" or "0.15.0" or "0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0" or "0.19.0") || string.IsNullOrEmpty(d.Faction) || d.Offset != r.Offset || d.Limit != r.Limit ||
            d.Total < 0 || d.Items is null || d.Items.Length != Math.Min(r.Limit, Math.Max(0, d.Total-r.Offset)) ||
            d.HasMore != ((long)r.Offset+d.Items.Length < d.Total) || d.Limitations is null ||
            d.Items.Any(i => i is null || !BuildingPolicy.ValidTemplate(i.Template) || i.Size is null ||
                i.Size.X <= 0 || i.Size.Y <= 0 || i.Size.Z <= 0 || i.UnsupportedReasons is null ||
                i.Supported != (i.UnsupportedReasons.Length == 0) || string.IsNullOrEmpty(i.Layout) ||
                string.IsNullOrEmpty(i.ToolShape) || i.ToolGroup is null || !ValidCosts(i.Costs)) ||
            d.Items.Select(i => i.Template).Distinct().Count() != d.Items.Length)
            throw new InvalidDataException("Invalid building catalog");
        return result;
    }

    public Task<BridgeEnvelope<NativePlacement>> PlaceBuilding(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "building-placement") throw new ArgumentException();
        return Place(r, ct);
    }
}
