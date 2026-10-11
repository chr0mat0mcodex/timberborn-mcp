using Timberborn.Bridge.Core;
using Timberborn.TimeSystem;
using Timberborn.SingletonSystem;
using Timberborn.Goods;
using Timberborn.ResourceCountingSystem;
using Timberborn.ScienceSystem;
using Timberborn.EntitySystem;

namespace Timberborn.AgentBridge;

public sealed class SimulationControl
{
    private readonly SpeedManager speed;
    private readonly IDayNightCycle time;
    private readonly EventBus events;
    private bool speedLocked;
    private readonly IGoodService goods;
    private readonly ResourceCountingService resources;
    private readonly ScienceService science;
    private readonly EntityRegistry entities;
    public SimulationControl(SpeedManager speed, IDayNightCycle time, EventBus events, IGoodService goods, ResourceCountingService resources, ScienceService science, EntityRegistry entities) {
        this.speed = speed; this.time = time; this.events = events;
        this.goods = goods; this.resources = resources; this.science = science; this.entities = entities;
        events.Register(this);
    }
    [OnEvent]
    public void OnSpeedLockChanged(SpeedLockChangedEvent change) => speedLocked = change.IsLocked;
    private readonly SimulationRunController runs = new();
    private string? batchOwner;
    public bool SpeedLocked => speedLocked;
    public void RequireBatchAvailable()
    {
        if (batchOwner is not null || runs.HasActiveRun || speedLocked || speed.CurrentSpeed != 0)
            throw new BridgeRejectionException("state_conflict");
    }
    public SimulationRunStatus StartBatch(LargeProjectRequest r)
    {
        RequireBatchAvailable();
        var request = SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for", new System.Collections.Specialized.NameValueCollection {
            {"session", r.Session}, {"runId", r.ActionId}, {"duration", r.MaxGameHours.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {"unit", "hours"},
            {"speed", r.Speed.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {"expectedSpeed", "0"}, {"maxRealSeconds", r.MaxRealSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} });
        var result = runs.Start(request, Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, s => speed.ChangeSpeed(s), deferSpeedStart: true);
        batchOwner = r.ActionId;
        return result;
    }
    public SimulationRunStatus InspectBatch(string id) => runs.Inspect(id);
    public void BeginBatch(string id) => runs.BeginDeferred(id, wall.Elapsed.TotalSeconds, s => speed.ChangeSpeed(s));
    public void CancelBatch(string id) => runs.Cancel(id, Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, s => speed.ChangeSpeed(s), speedLocked);
    public void ReleaseBatch(string id) { if (batchOwner == id && runs.Inspect(id).Terminal) batchOwner = null; }
    private readonly System.Diagnostics.Stopwatch wall = System.Diagnostics.Stopwatch.StartNew();
    private double Hours => time.DayNumber * 24d + time.HoursPassedToday;
    private int? AvailableLogs() {
        try { return goods.Goods.Contains("Log") ? resources.GetGlobalResourceCount("Log").AvailableStock : null; }
        catch { return null; } // Unknown stock stops a conditional run; never assume zero or met.
    }
    private ProductionSample Sample(IEnumerable<string> ids, bool withScience) {
        var sample = new ProductionSample();
        foreach (string id in ids) {
            try { sample.Goods[id] = goods.HasGood(id) ? resources.GetGlobalResourceCount(id).AvailableStock : null; }
            catch { sample.Goods[id] = null; }
        }
        if (withScience) { try { sample.Science = science.SciencePoints; } catch { sample.Science = null; } }
        return sample;
    }
    public void Update() => runs.Tick(Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, s => speed.ChangeSpeed(s), speedLocked,
        runs.StockConditionActive ? AvailableLogs() : null,
        runs.ActiveProductionRun is { } active ? Sample(active.GoodTargets.Select(t=>t.Good),active.StopAtScience is not null) : null);
    public void Unload() { events.Unregister(this); runs.Unload(s => speed.ChangeSpeed(s)); }
    public object Run(SimulationRunRequest r)
    {
        if (r.Starts && (speedLocked || batchOwner is not null)) throw new BridgeRejectionException("state_conflict");
        var result = r.Starts ? runs.Start(r, Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, s => speed.ChangeSpeed(s),
            r.StopAtAvailableLogs is null ? null : AvailableLogs(), production: Sample(r.GoodTargets.Select(t=>t.Good),r.StopAtScience is not null))
            : r.Route == "simulation-run-cancel" ? runs.Cancel(r.RunId, Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, s => speed.ChangeSpeed(s), speedLocked) : runs.Inspect(r.RunId);
        var payload = Newtonsoft.Json.Linq.JObject.FromObject(result, Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() }));
        if (result.HasProductionTargets) {
            try { payload["productionDiagnostics"] = Newtonsoft.Json.Linq.JArray.FromObject(ProductionRunObservations.Read(entities,resources,result), Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() })); }
            catch { payload["productionDiagnostics"] = null; }
            payload["diagnosticsObservedGameHours"] = Hours;
        }
        return payload;
    }
    public object Observe() => new { currentSpeed = speed.CurrentSpeed, dayNumber = time.DayNumber,
        dayProgress = time.DayProgress, hoursPassedToday = time.HoursPassedToday };

    public object Set(BridgeRequest request)
    {
        // Compare on the game thread. Never override a newer user speed or unlock a game lock.
        float previous = speed.CurrentSpeed;
        if (speedLocked || previous != request.ExpectedSpeed) throw new ArgumentException("speed_changed");
        runs.Interrupt();
        speed.ChangeSpeed(request.Speed);
        return new { requestedSpeed = request.Speed, previousSpeed = previous,
            matchedImmediately = speed.CurrentSpeed == request.Speed, observation = Observe(),
            limitations = new[] { "read_simulation_to_confirm", "no_automatic_retry", "game_locks_not_overridden",
                "standard_speeds_only_0_1_3_7", "speed_is_not_measured_tick_throughput" } };
    }
}
