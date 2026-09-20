using Bindito.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.Goods;
using Timberborn.Modding;
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
    protected override void Configure()
    {
        Bind<SpatialObservations>().AsSingleton();
        Bind<BuildingObservations>().AsSingleton();
        Bind<WorkforceObservations>().AsSingleton();
        Bind<WorkplaceStaffing>().AsSingleton();
        Bind<PriorityAndConstruction>().AsSingleton(); Bind<AreaManagement>().AsSingleton();
        Bind<SiteValidation>().AsSingleton();
        Bind<PilotPlacement>().AsSingleton();
        Bind<SimulationControl>().AsSingleton();
        Bind<BridgeMod>().AsSingleton();
    }
}

public sealed class BridgeMod(ResourceCountingService resources, PopulationService population,
    EntityRegistry entities, ITerrainService terrain, IThreadSafeWaterMap water, IGoodService goods,
    ModRepository mods, SpatialObservations spatial, SiteValidation validation, PilotPlacement placement, BuildingObservations buildings, SimulationControl simulation, WorkforceObservations workforce, WorkplaceStaffing staffing, PriorityAndConstruction management, AreaManagement areas)
    : ILoadableSingleton, IUnloadableSingleton, IUpdatableSingleton
{
    private readonly MainThreadQueue queue = new();
    private readonly string sessionId = Guid.NewGuid().ToString("D");
    private BridgeHttpServer? server;
    public void Load()
    {
        string stage = "locate_configuration";
        try
        {
            // The game can load assemblies from bytes, leaving Assembly.Location empty.
            // Resolve the installed mod by manifest identity, not by the loaded DLL location.
            var ownMod = mods.EnabledMods.Single(m => m.Manifest.Id == "chr0mat0mcodex.TimberbornAgentBridge");
            var path = Path.Combine(ownMod.ModDirectory.Path, "bridge.local.json");
            stage = "read_configuration";
            if (new FileInfo(path).Length > 4096) throw new InvalidOperationException();
            var config = JObject.Parse(File.ReadAllText(path));
            stage = "create_listener";
            server = new BridgeHttpServer((int?)config["port"] ?? 8081, (string?)config["token"] ?? "", queue,
                (bool?)config["enableValidation"] == true, (bool?)config["enablePlacement"] == true, (bool?)config["enableLodgePlacement"] == true, (bool?)config["enableSpeedControl"] == true, (bool?)config["enableStaffing"] == true, (bool?)config["enablePriorities"] == true, (bool?)config["enableAreas"] == true);
            stage = "start_listener";
            server.Start();
            Debug.Log("[Timberborn Agent Bridge] Endpoint ready on loopback; actions require explicit opt-in.");
        }
        catch (Exception ex)
        {
            server?.Dispose(); server = null;
            Debug.LogError($"[Timberborn Agent Bridge] Start failed at {stage}: {ex.GetType().Name}. No game changes performed.");
        }
    }
    public void UpdateSingleton() => queue.Pump(Observe);
    public void Unload() { server?.Dispose(); queue.Dispose(); }
    private string Observe(BridgeRequest request)
    {
        if ((request.Route is "site-validation" or "path-placement" or "lodge-placement" or "building" or "simulation-speed" or "workplace-staffing" or "priority" or "set-priority" or "set-area") && request.Session != sessionId) throw new ArgumentException("stale_session");
        object data = request.Route switch
        {
            "simulation" => simulation.Observe(), "simulation-speed" => simulation.Set(request),
            "snapshot" => Snapshot(), "map" => Map(request), "objects" => spatial.Objects(request),
            "catalog" => spatial.Catalog(), "site-precheck" => spatial.Precheck(request),
            "site-validation" => validation.Validate(request),
            "priority" or "set-priority" => management.Priority(request.Management!),
            "construction" => management.Construction(request.Management!),
            "area-types" => areas.Types(), "areas" => areas.Areas(request.Management!), "set-area" => areas.Set(request.Management!),
            "workplace-staffing" => staffing.Set(request),
            "building" => buildings.Observe(request), "workforce" => workforce.Observe(request),
            "path-placement" or "lodge-placement" => placement.Place(request),
            _ => throw new ArgumentException("invalid_request")
        };
        return JsonConvert.SerializeObject(new { schemaVersion = 1, sessionId, observedAtUtc = DateTimeOffset.UtcNow,
            bridgeVersion = "0.12.0", data });
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
