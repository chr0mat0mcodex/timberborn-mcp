using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeProjectExecution>> VerticalStair(VerticalStairRequest r, CancellationToken ct)
    {
        string q = $"session={r.Session}&actionId={r.ActionId:D}";
        if (r.Start) q += $"&mode={(r.WithWarehouse ? "stair_platform_warehouse_pilot" : r.WithPlatform ? "stair_platform_pilot" : r.UpperPathCount == 0 ? "single_stair_pilot" : "stair_with_upper_paths_pilot")}&districtId={r.DistrictId}&x={r.X}&y={r.Y}&z={r.Z}&rotation={r.Rotation}&upperPathCount={r.UpperPathCount}";
        var e = await Get<NativeProjectExecution>((r.Start ? "vertical-stair-execute" : "vertical-stair-status") + "?" + q, ct, r.Start ? HttpMethod.Post : HttpMethod.Get);
        var d=e.Data;
        ValidateVerticalReceipt(e, r);
        return e;
    }
    public static void ValidateVerticalReceipt(BridgeEnvelope<NativeProjectExecution> e, VerticalStairRequest r)
    {
        var d = e.Data;
        if (e.BridgeVersion is not ("0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2") || e.SessionId != r.Session || d is null || d.ActionId != r.ActionId.ToString("D") ||
            d.Steps is null || d.Steps.Length is < 1 or > 7 || d.Steps.Any(s => s is null) ||
            d.Steps[0].Template != "Stairs.Folktails" || d.Steps[0].EntityId != d.ActionId ||
            d.State is not ("running" or "waiting" or "completed" or "stopped" or "unconfirmed")) throw new InvalidDataException("Invalid vertical stair receipt");
        bool legacyPlatform = d.Steps.Length == 4;
        bool twoPlatforms = d.Steps.Length == 5;
        bool warehouse = d.Steps.Length == 7;
        bool platform = legacyPlatform || twoPlatforms || warehouse;
        if (legacyPlatform && (e.BridgeVersion != "0.29.0" || !d.Steps.Select(s => s.Template).SequenceEqual(new[] { "Stairs.Folktails", "Path", "Platform.Folktails", "Path" })) ||
            twoPlatforms && (e.BridgeVersion is not ("0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2") || !d.Steps.Select(s => s.Template).SequenceEqual(new[] { "Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path" })) ||
            warehouse && (e.BridgeVersion is not ("0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2") || !d.Steps.Select(s => s.Template).SequenceEqual(new[] { "Stairs.Folktails", "Platform.Folktails", "Path", "Platform.Folktails", "Path", "Platform.Folktails", "SmallWarehouse.Folktails" })) ||
            !platform && (d.Steps.Length > 3 || d.Steps.Skip(1).Any(s => s.Template != "Path")) ||
            r.Start && (warehouse != r.WithWarehouse || platform != r.WithPlatform || d.Steps.Length != (r.WithWarehouse ? 7 : 1 + r.UpperPathCount + (r.WithPlatform ? (twoPlatforms ? 2 : 1) : 0)))) throw new InvalidDataException("Invalid vertical project scope");
        var stair = d.Steps[0];
        if (stair.Rotation is < 0 or > 3 || r.Start && (stair.X != r.X || stair.Y != r.Y || stair.Z != r.Z || stair.Rotation != r.Rotation)) throw new InvalidDataException("Invalid stair geometry");
        var direction = stair.Rotation switch { 0 => (x:0,y:-1), 1 => (x:-1,y:0), 2 => (x:0,y:1), _ => (x:1,y:0) };
        for (int i = 1; i < d.Steps.Length; i++)
        {
            var s = d.Steps[i]; int distance = twoPlatforms || warehouse ? (i + 1) / 2 : legacyPlatform ? (i == 1 ? 1 : 2) : i;
            if (s.X != stair.X + direction.x * distance || s.Y != stair.Y + direction.y * distance ||
                s.Z != stair.Z + (s.Template == "Platform.Folktails" ? 0 : 1) || s.Rotation != (s.Template == "SmallWarehouse.Folktails" ? (stair.Rotation + 2) % 4 : 0)) throw new InvalidDataException("Invalid upper geometry");
        }
        if (d.Steps.Any(s => !Guid.TryParseExact(s.EntityId,"D",out var id) || id == Guid.Empty || s.State is not ("pending" or "unconfirmed" or "confirmed")) ||
            d.Steps.Select(s => s.EntityId).Distinct().Count() != d.Steps.Length ||
            d.State == "completed" && d.Steps.Any(s => s.State != "confirmed")) throw new InvalidDataException("Invalid vertical confirmation");
        if (d.State == "waiting" && (e.BridgeVersion is not ("0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2") || !(twoPlatforms || warehouse || e.BridgeVersion is ("0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2")) ||
            d.Reason is not ("awaiting_construction_finished" or "awaiting_pause") ||
            d.Steps.All(s => s.State == "confirmed") || d.Steps.Any(s => s.State == "unconfirmed") ||
            d.Steps.SkipWhile(s => s.State == "confirmed").Any(s => s.State != "pending")))
            throw new InvalidDataException("Invalid construction wait receipt");
    }
}
