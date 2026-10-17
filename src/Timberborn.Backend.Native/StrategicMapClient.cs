using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public sealed record StrategicSpan(int Bottom, int Top, float WaterDepth, float Contamination);
public sealed record StrategicColumn(int X, int Y, StrategicSpan[] Spans);
public sealed record StrategicObject(Guid Id, string Template, int X, int Y, int Z, string Kind, bool Finished,
    StrategicExplosion? Explosion = null);
public sealed record StrategicExplosion(float Radius, float InnerRadius, bool Blocked, string Evidence,
    Position[] TerrainCells, bool Complete, string Timing);
public sealed record StrategicWeather(int Cycle, int CycleDay, float PartialCycleDay, bool Hazardous,
    string SelectedHazard, int TemperateDays, int HazardousDays, int HazardousStartCycleDay, int CycleLengthDays);
public sealed record StrategicSourceModifier(string Type, float CurrentMultiplier);
public sealed record StrategicSource(Guid Id, Position Anchor, Position[] Outlets, bool OutletsTruncated,
    float SpecifiedStrength, float CurrentStrength, float Contamination, bool Underground,
    string[]? ActiveInHazardousWeather, StrategicSourceModifier[] Modifiers, bool ModifiersTruncated = false);
public sealed record StrategicTile(int Tile, int TotalTiles, Position MapSize, int X, int Y, int Width, int Height,
    StrategicColumn[] Columns, StrategicObject[] Objects, bool ObjectsComplete, bool RegistryComplete, string[] Limitations,
    StrategicWeather? Weather = null, StrategicSource[]? Sources = null, bool SourcesComplete = false);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<StrategicTile>> StrategicMap(string session, int tile, CancellationToken ct)
    {
        if (!Guid.TryParseExact(session, "D", out var id) || id == Guid.Empty || tile is < 0 or >= 256)
            throw new ArgumentException("invalid_strategic_map_request");
        var result = await Get<StrategicTile>($"strategic-map?session={id:D}&tile={tile}", ct,
            maxResponseBytes: StrategicMapRequest.MaxResponseBytes);
        if (result.BridgeVersion is not ("0.62.0" or "0.63.0" or "0.64.0" or "0.65.0")) throw new InvalidDataException("strategic_map_requires_062");
        if (result.SessionId != session) throw new BridgeRejectionException("stale_session");
        ValidateStrategicTile(result.Data, tile);
        return result;
    }

    public static void ValidateStrategicTile(StrategicTile d, int tile)
    {
        if (d.MapSize is null || d.MapSize.X is < 1 or > 512 || d.MapSize.Y is < 1 or > 512 || d.MapSize.Z is < 1 or > 64)
            throw new InvalidDataException("strategic_map_invalid_dimensions");
        int across = (d.MapSize.X + 31) / 32;
        if (d.Tile != tile || d.TotalTiles != across * ((d.MapSize.Y + 31) / 32) || tile >= d.TotalTiles ||
            d.X != tile % across * 32 || d.Y != tile / across * 32 ||
            d.Width != Math.Min(32, d.MapSize.X - d.X) || d.Height != Math.Min(32, d.MapSize.Y - d.Y) ||
            d.Columns is null || d.Columns.Length != d.Width * d.Height || d.Limitations is null ||
            d.Objects is null || d.Objects.Length > 512 || d.ObjectsComplete && !d.RegistryComplete)
            throw new InvalidDataException("strategic_map_incomplete_tile");
        var seen = new HashSet<(int, int)>();
        foreach (var c in d.Columns)
        {
            if (c is null || c.X < d.X || c.X >= d.X + d.Width || c.Y < d.Y || c.Y >= d.Y + d.Height ||
                !seen.Add((c.X, c.Y)) || c.Spans is null || c.Spans.Length > 32)
                throw new InvalidDataException("strategic_map_invalid_column");
            int end = -1;
            foreach (var s in c.Spans)
            {
                if (s is null || s.Bottom < 0 || s.Top <= s.Bottom || s.Top > d.MapSize.Z || s.Bottom <= end ||
                    !float.IsFinite(s.WaterDepth) || s.WaterDepth < 0 || s.WaterDepth > d.MapSize.Z ||
                    !float.IsFinite(s.Contamination) || s.Contamination is < 0 or > 1)
                    throw new InvalidDataException("strategic_map_invalid_span");
                end = s.Top;
            }
        }
        if (d.Objects.Any(o => o is null || o.Id == Guid.Empty || string.IsNullOrEmpty(o.Template) || o.Template.Length > 160 ||
            o.X < d.X || o.X >= d.X + d.Width || o.Y < d.Y || o.Y >= d.Y + d.Height || o.Z < 0 || o.Z >= d.MapSize.Z ||
            o.Kind is not ("building" or "path" or "special_or_unknown")) || d.Objects.Select(o => o.Id).Distinct().Count() != d.Objects.Length)
            throw new InvalidDataException("strategic_map_invalid_objects");
        if(d.SourcesComplete && (!d.RegistryComplete || d.Sources is null))
            throw new InvalidDataException("strategic_sources_incomplete");
        bool InvalidPoint(Position? p)=>p is null || p.X<0 || p.Y<0 || p.Z<0 || p.X>=d.MapSize.X || p.Y>=d.MapSize.Y || p.Z>=d.MapSize.Z;
        foreach (var o in d.Objects.Where(o => o.Explosion is not null)) {
            var e = o.Explosion!;
            if (!float.IsFinite(e.Radius) || e.Radius is < 0 or > 128 || !float.IsFinite(e.InnerRadius) ||
                e.InnerRadius < 0 || e.InnerRadius > e.Radius || e.TerrainCells is null || e.TerrainCells.Length > 8192 ||
                e.TerrainCells.Any(InvalidPoint) || e.TerrainCells.Distinct().Count() != e.TerrainCells.Length ||
                e.Evidence != "native_radius_terrain_scenario" || e.Timing != "unknown")
                throw new InvalidDataException("strategic_invalid_explosion");
        }
        if(d.Sources is {} sources && (sources.Length>512 || sources.Select(s=>s?.Id).Distinct().Count()!=sources.Length || sources.Any(s=>
            s is null || s.Id==Guid.Empty || InvalidPoint(s.Anchor) || s.Anchor.X<d.X || s.Anchor.X>=d.X+d.Width || s.Anchor.Y<d.Y || s.Anchor.Y>=d.Y+d.Height ||
            s.Outlets is null || s.Outlets.Length>64 || s.Outlets.Any(InvalidPoint) ||
            !float.IsFinite(s.SpecifiedStrength) || s.SpecifiedStrength<0 || !float.IsFinite(s.CurrentStrength) || s.CurrentStrength<0 ||
            !float.IsFinite(s.Contamination) || s.Contamination is <0 or >1 ||
            s.Modifiers is null || s.Modifiers.Length>32 || s.Modifiers.Any(m=>m is null || string.IsNullOrEmpty(m.Type) || m.Type.Length>160 || !float.IsFinite(m.CurrentMultiplier) || m.CurrentMultiplier<0) ||
            s.ActiveInHazardousWeather is {} rules && (rules.Length>32 || rules.Any(r=>string.IsNullOrEmpty(r)||r.Length>160)))))
            throw new InvalidDataException("strategic_invalid_sources");
        if(d.Weather is {} w && (!float.IsFinite(w.PartialCycleDay) || w.PartialCycleDay<0 || w.Cycle<0 || w.CycleDay<0 ||
            string.IsNullOrEmpty(w.SelectedHazard) || w.SelectedHazard.Length>160 || w.TemperateDays<0 || w.HazardousDays<0 || w.HazardousStartCycleDay<0 || w.CycleLengthDays<0))
            throw new InvalidDataException("strategic_invalid_weather");
    }
}
