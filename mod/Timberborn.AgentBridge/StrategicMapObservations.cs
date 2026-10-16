using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.NaturalResources;
using Timberborn.Ruins;
using Timberborn.TemplateSystem;
using Timberborn.TerrainSystem;
using Timberborn.WaterSystem;
using Timberborn.WaterSourceSystem;
using Timberborn.GameWaterSourceSystem;
using Timberborn.WeatherSystem;
using Timberborn.HazardousWeatherSystem;
using Timberborn.GameCycleSystem;
using UnityEngine;
using Timberborn.Explosions;

namespace Timberborn.AgentBridge;

// Bounded read on the game thread. Empty vertical intervals preserve caves and overhangs.
public sealed class StrategicMapObservations(ITerrainService terrain, IThreadSafeWaterMap water, EntityRegistry entities,
    WeatherService weather, HazardousWeatherService hazardousWeather, GameCycleService cycle,
    ExplosionOutcomeGatherer explosionOutcomes)
{
    public object Read(StrategicMapRequest r)
    {
        var size = terrain.Size;
        if (size.x < 1 || size.y < 1 || size.z < 1 || size.x > 512 || size.y > 512 || size.z > 64)
            throw new ArgumentException("invalid_request");
        int across = (size.x + 31) / 32, total = across * ((size.y + 31) / 32);
        if (r.Tile >= total) throw new ArgumentException("invalid_request");
        int x0 = r.Tile % across * 32, y0 = r.Tile / across * 32;
        int width = Math.Min(32, size.x - x0), height = Math.Min(32, size.y - y0);
        var columns = new List<object>();
        for (int y = y0; y < y0 + height; y++) for (int x = x0; x < x0 + width; x++)
        {
            var spans = new List<object>(); int z = 0;
            while (z < size.z)
            {
                if (terrain.Underground(new Vector3Int(x, y, z))) { z++; continue; }
                int bottom = z;
                while (z < size.z && !terrain.Underground(new Vector3Int(x, y, z))) z++;
                var cell = new Vector3Int(x, y, bottom);
                spans.Add(new { bottom, top = z, waterDepth = water.WaterDepth(cell), contamination = water.ColumnContamination(cell) });
            }
            columns.Add(new { x, y, spans });
        }
        var objects = new List<object>(); var sources = new List<object>();
        int scanned = 0, matched = 0, sourceMatches = 0, explosionReads = 0; bool registryComplete = true, sourceAnchorsComplete = true;
        foreach (var entity in entities.Entities)
        {
            if (++scanned > 100000) { registryComplete = false; break; }
            if (!entity.Initialized || entity.Deleted) continue;
            if (!entity.TryGetComponent<BlockObject>(out var block))
            { if(entity.HasComponent<WaterSource>())sourceAnchorsComplete=false; continue; }
            if(block.IsPreview)continue;
            var p = block.Coordinates;
            if (p.x < x0 || p.x >= x0 + width || p.y < y0 || p.y >= y0 + height) continue;
            if (entity.TryGetComponent<WaterSource>(out var source))
            {
                sourceMatches++;
                if (sources.Count < 512) sources.Add(new {
                    id = entity.EntityId, anchor = new { x=p.x,y=p.y,z=p.z },
                    outlets = source.Coordinates.Take(64).Select(c=>new { x=c.x,y=c.y,z=c.z }).ToArray(),
                    outletsTruncated = source.Coordinates.Length > 64,
                    specifiedStrength = source.SpecifiedStrength, currentStrength = source.CurrentStrength,
                    contamination = source.Contamination, underground = entity.HasComponent<UndergroundWaterSource>(),
                    activeInHazardousWeather = entity.TryGetComponent<HazardousWeatherWaterSourceSpec>(out var rule)
                        ? rule.ActiveInHazardousWeather.ToArray() : null,
                    modifiersTruncated = source.WaterStrengthModifiers.Count() > 32,
                    modifiers = source.WaterStrengthModifiers.Take(32).Select(m=>new {
                        type=m.GetType().Name, currentMultiplier=m.GetStrengthModifier() }).ToArray()
                });
            }
            if (entity.HasComponent<NaturalResource>() && !entity.HasComponent<Ruin>()) continue;
            matched++;
            if (objects.Count >= 512) continue;
            string template = entity.TryGetComponent<TemplateSpec>(out var spec) ? spec.TemplateName : "unknown";
            if (template.Length > 160) template = template.Substring(0, 160);
            objects.Add(new { id = entity.EntityId, template, x = p.x, y = p.y, z = p.z,
                kind = entity.HasComponent<Building>() ? "building" : template == "Path" ? "path" : "special_or_unknown",
                finished = block.IsFinished, explosion = Explosion(entity, ref explosionReads) });
        }
        return new { tile = r.Tile, totalTiles = total, mapSize = new { x = size.x, y = size.y, z = size.z },
            x = x0, y = y0, width, height, columns, objects,
            objectsComplete = registryComplete && matched <= 512, registryComplete,
            sources, sourcesComplete = registryComplete && sourceAnchorsComplete && sourceMatches <= 512,
            weather = new { cycle=cycle.Cycle, cycleDay=cycle.CycleDay, partialCycleDay=cycle.PartialCycleDay,
                hazardous=weather.IsHazardousWeather, selectedHazard=hazardousWeather.CurrentCycleHazardousWeather?.Id ?? "unknown",
                temperateDays=weather.TemperateWeatherDuration, hazardousDays=weather.HazardousWeatherDuration,
                hazardousStartCycleDay=weather.HazardousWeatherStartCycleDay, cycleLengthDays=weather.CycleLengthInDays },
            limitations = new[] { "terrain_voids_not_native_water_flow_or_navigation", "objects_by_anchor_not_footprint",
                "vegetation_excluded", "water_sample_at_void_bottom_semantics_need_live_validation", "water_control_objects_not_modelled" } };
    }

    private object? Explosion(EntityComponent entity, ref int reads)
    {
        if (!entity.TryGetComponent<UnstableCore>(out var core)) return null;
        // Native radius geometry, filtered against observed solid terrain. This is a
        // hypothetical removal scenario, not an invocation of ExplosionService or a
        // claim about collapse, object destruction, activation time or hydraulic arrival.
        int radius = core.ExplosionRadius;
        bool bounded = radius >= 0 && radius <= 32 && ++reads <= 4;
        var cells = new HashSet<Vector3Int>();
        if (bounded) foreach (var shell in explosionOutcomes.GetAffectedTilesPerRadius(core.ExplosionCenter, radius))
            foreach (var cell in shell.Value)
                if (cell.x >= 0 && cell.y >= 0 && cell.z >= 0 && cell.x < terrain.Size.x && cell.y < terrain.Size.y &&
                    cell.z < terrain.Size.z && terrain.Underground(cell)) cells.Add(cell);
        return new {
            radius, innerRadius = core.InnerRadius,
            blocked = entity.TryGetComponent<UnstableCoreExplosionBlocker>(out var blocker) && blocker.ExplosionBlocked,
            evidence = "native_radius_terrain_scenario", timing = "unknown",
            terrainCells = cells.OrderBy(p=>p.x).ThenBy(p=>p.y).ThenBy(p=>p.z).Take(8192).Select(p=>new { x=p.x,y=p.y,z=p.z }).ToArray(),
            complete = bounded && cells.Count <= 8192
        };
    }
}
