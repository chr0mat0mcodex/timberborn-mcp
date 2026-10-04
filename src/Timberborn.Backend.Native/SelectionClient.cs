using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record SelectionTarget(Guid Id, string Template, string Kind, Position? GridPosition, WorldPosition WorldPosition);
public sealed record NativeSelection(string State, SelectionTarget? Target, string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeSelection>> Selection(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "selection") throw new ArgumentException();
        var e = await Get<NativeSelection>($"selection?session={r.Session}", ct);
        ValidateSelection(e, r.Session);
        return e;
    }
    public static void ValidateSelection(BridgeEnvelope<NativeSelection> e, string session)
    {
        var d = e.Data;
        if (e.BridgeVersion is not ("0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2" or "0.35.3") || e.SessionId != session || d is null ||
            d.State is not ("none" or "selected" or "unsupported") ||
            (d.State == "selected") != (d.Target is not null) ||
            d.Limitations is null || d.Limitations.Length > 16 || d.Limitations.Any(s => string.IsNullOrEmpty(s) || s.Length > 160))
            throw new InvalidDataException("Invalid selection observation");
        if (d.Target is { } t && (t.Id == Guid.Empty || !BuildingPolicy.ValidTemplate(t.Template) ||
            t.Kind is not ("block_object" or "entity") || (t.Kind == "block_object") != (t.GridPosition is not null) ||
            t.GridPosition is { } p && (p.X is < 0 or > 4095 || p.Y is < 0 or > 4095 || p.Z is < 0 or > 4095) ||
            t.WorldPosition is not { } w || !float.IsFinite(w.X) || !float.IsFinite(w.Y) || !float.IsFinite(w.Z)))
            throw new InvalidDataException("Invalid selection target");
    }
}
