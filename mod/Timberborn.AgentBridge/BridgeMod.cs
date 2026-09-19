using Bindito.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.Population;
using Timberborn.ResourceCountingSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.WaterSystem;
using UnityEngine;

namespace Timberborn.AgentBridge;

[Context("Game")]
public sealed class BridgeConfigurator : Configurator
{
    protected override void Configure() => Bind<BridgeMod>().AsSingleton();
}

public sealed class BridgeMod(ResourceCountingService resources, PopulationService population,
    EntityRegistry entities, ITerrainService terrain, IThreadSafeWaterMap water, IGoodService goods)
    : ILoadableSingleton, IUnloadableSingleton, IUpdatableSingleton
{
    private readonly MainThreadQueue queue = new();
    private readonly string sessionId = Guid.NewGuid().ToString("D");
    private BridgeHttpServer? server;
    public void Load()
    {
        try
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(BridgeMod).Assembly.Location)!, "bridge.local.json");
            if (new FileInfo(path).Length > 4096) throw new InvalidOperationException();
            var config = JObject.Parse(File.ReadAllText(path));
            server = new BridgeHttpServer((int?)config["port"] ?? 8081, (string?)config["token"] ?? "", queue);
            server.Start();
            Debug.Log("[Timberborn Agent Bridge] Read-only endpoint ready on loopback.");
        }
        catch (Exception)
        {
            server?.Dispose(); server = null;
            Debug.LogError("[Timberborn Agent Bridge] Start failed. Check private configuration and port. No game changes performed.");
        }
    }
    public void UpdateSingleton() => queue.Pump(Observe);
    public void Unload() { server?.Dispose(); queue.Dispose(); }
    private string Observe(BridgeRequest request)
    {
        object data = request.Route == "snapshot" ? Snapshot() : Map(request);
        return JsonConvert.SerializeObject(new { schemaVersion = 1, sessionId, observedAtUtc = DateTimeOffset.UtcNow,
            bridgeVersion = "0.2.0", data });
    }
    private object Snapshot()
    {
        var p = population.GlobalPopulationData;
        var beds = p.BedData;
        var beavers = p.BeaverWorkforceData;
        var bots = p.BotWorkforceData;
        var jobs = p.BeaverWorkplaceData;
        var botJobs = p.BotWorkplaceData;
        var stocks = new[] { "Water", "Berries", "Log" }.Select(id =>
        {
            bool available = goods.HasGood(id);
            ResourceCount? value = available ? resources.GetGlobalResourceCount(id) : null;
            return new { id, available, availableStock = value?.AvailableStock, allStock = value?.AllStock,
                stockpiledStock = value?.StockpiledStock, bufferedOutputStock = value?.BufferedOutputStock,
                inputOutputCapacity = value?.InputOutputCapacity, totalCapacity = value?.TotalCapacity };
        }).ToArray();
        // Bound output while walking the live registry. Names are labels, never instructions or template IDs.
        var objects = new List<object>(); int count = 0;
        foreach (var entity in entities.Entities)
        {
            if (!entity.Initialized || entity.Deleted || !entity.TryGetComponent<BlockObject>(out var block) || block.IsPreview) continue;
            count++;
            if (objects.Count == 16) continue;
            string name = entity.Name ?? "";
            if (name.Length > 120) name = name.Substring(0, 120);
            objects.Add(new { id = entity.EntityId, objectName = name, position = Vec(block.Coordinates),
                orientation = block.Orientation.ToString(), flipMode = block.FlipMode.ToString(),
                finished = block.IsFinished, unfinished = block.IsUnfinished,
                entrance = block.HasEntrance ? Vec(block.PositionedEntrance.Coordinates) : null });
        }
        return new {
            scope = "global", resources = stocks,
            housing = new { occupiedBeds = beds.OccupiedBeds, freeBeds = beds.FreeBeds, homeless = beds.Homeless,
                totalBeds = beds.OccupiedBeds + beds.FreeBeds },
            population = new { adults = p.NumberOfAdults, children = p.NumberOfChildren, bots = p.NumberOfBots },
            workforce = new { beavers = new { employable = beavers.Employable, unemployable = beavers.Unemployable,
                unemployed = jobs.Unemployed, occupiedWorkslots = jobs.OccupiedWorkslots, freeWorkslots = jobs.FreeWorkslots },
                bots = new { employable = bots.Employable, unemployable = bots.Unemployable,
                    unemployed = botJobs.Unemployed, occupiedWorkslots = botJobs.OccupiedWorkslots, freeWorkslots = botJobs.FreeWorkslots } },
            mapSize = Vec(terrain.Size), objectCount = count, objectSample = objects, objectsTruncated = count > objects.Count,
            limitations = new[] { "resources_are_three_example_goods_not_total_food", "object_sample_includes_natural_objects",
                "no_path_connectivity_or_build_validation", "main_thread_observation_not_atomic_simulation_snapshot" }
        };
    }
    private object Map(BridgeRequest r)
    {
        var start = new Vector3Int(r.X, r.Y, r.Z);
        var end = new Vector3Int(r.X + r.Width - 1, r.Y + r.Height - 1, r.Z + r.Depth - 1);
        if (!terrain.Contains(start) || !terrain.Contains(end)) throw new ArgumentException("invalid_region");
        var cells = new List<object>();
        for (int z = r.Z; z < r.Z + r.Depth; z++)
            for (int y = r.Y; y < r.Y + r.Height; y++)
                for (int x = r.X; x < r.X + r.Width; x++)
                {
                    var cell = new Vector3Int(x, y, z);
                    cells.Add(new { x, y, z, underground = terrain.Underground(cell), onGround = terrain.OnGround(cell),
                        terrainHeight = terrain.GetTerrainHeight(cell), waterDepth = water.WaterDepth(cell),
                        contamination = water.ColumnContamination(cell), underwater = water.CellIsUnderwater(cell) });
                }
        return new { origin = Vec(start), width = r.Width, height = r.Height, depth = r.Depth, cells,
            limitations = new[] { "raw_game_coordinates_semantics_need_live_validation", "no_buildability_or_reachability_claim" } };
    }
    private static object Vec(Vector3Int value) => new { x = value.x, y = value.y, z = value.z };
}
