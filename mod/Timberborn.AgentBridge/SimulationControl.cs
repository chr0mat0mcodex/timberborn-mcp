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
    private bool speedLockObserved;
    private readonly IGoodService goods;
    private readonly ResourceCountingService resources;
    private readonly ScienceService science;
    private readonly EntityRegistry entities;
    private readonly RecentActionsJournal journal;
    private BuildingProductionObserver? buildingProof;
    public SimulationControl(SpeedManager speed, IDayNightCycle time, EventBus events, IGoodService goods, ResourceCountingService resources, ScienceService science, EntityRegistry entities, RecentActionsJournal journal) {
        this.speed = speed; this.time = time; this.events = events;
        this.goods = goods; this.resources = resources; this.science = science; this.entities = entities;this.journal=journal;
        events.Register(this);
    }
    [OnEvent]
    public void OnSpeedLockChanged(SpeedLockChangedEvent change) {
        speedLocked = change.IsLocked; speedLockObserved = true;
        if (speedLocked) {
            runs.CompletionLocked(wall.Elapsed.TotalSeconds, speedCommand.Request);
            if (runs.IsPausing && speedCommand.PendingSpeed is > 0) speedCommand.Clear();
        }
    }
    private readonly SimulationRunController runs = new();
    private readonly DeferredSpeedCommand speedCommand = new();
    private string? batchOwner;
    public bool SpeedLocked => speedLocked;
    public bool PauseSettled => speed.CurrentSpeed == 0 && !speedLocked && speedCommand.PendingSpeed is null;
    public void RequireBatchAvailable()
    {
        if (batchOwner is not null || runs.HasActiveRun || !PauseSettled)
            throw new BridgeRejectionException("state_conflict");
    }
    public void RequireConfigurationAvailable(BuildingConfigurationRequest r) {
        RequireBatchAvailable();
        if(r.ProofHours is not null)runs.RequireUnusedRun(r.JobId);
    }
    public SimulationRunStatus StartBatch(LargeProjectRequest r)
    {
        RequireBatchAvailable();
        var request = SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for", new System.Collections.Specialized.NameValueCollection {
            {"session", r.Session}, {"runId", r.ActionId}, {"duration", r.MaxGameHours.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {"unit", "hours"},
            {"speed", r.Speed.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {"expectedSpeed", "0"}, {"maxRealSeconds", r.MaxRealSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} });
        var result = runs.Start(request, Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, speedCommand.Request, deferSpeedStart: true);
        batchOwner = r.ActionId;
        return result;
    }
    public SimulationRunStatus InspectBatch(string id) => runs.Inspect(id);
    public void BeginBatch(string id) => runs.BeginDeferred(id, wall.Elapsed.TotalSeconds, speedCommand.Request);
    public void CancelBatch(string id) => runs.Cancel(id, Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, speedCommand.Request, speedLocked);
    public void ReleaseBatch(string id) { if (batchOwner == id && runs.Inspect(id).Terminal) batchOwner = null; }
    private readonly System.Diagnostics.Stopwatch wall = System.Diagnostics.Stopwatch.StartNew();
    public double ObservedGameHours => Hours;
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
    public void Update() {
        speedCommand.ObserveNextUpdate();
        // Cancellation under a game lock defers its pause command. Discard a
        // queued acceleration before the lock lifts; never dispatch it afterwards.
        if (runs.IsPausing && speedCommand.PendingSpeed is > 0) speedCommand.Clear();
        // Do not let the old CurrentSpeed acknowledge a newly queued pause/start.
        // The following bridge update occurs after SpeedManager's LateUpdate.
        if (speedCommand.HasPending && !speedLocked) {
            if (runs.IsCompleting && !runs.PrepareCompletionDispatch(Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, speedCommand.Request)) {
                if (!runs.HasActiveRun) speedCommand.Clear();
                return;
            }
            try { speedCommand.Apply(s => speed.ChangeSpeed(s)); }
            catch { runs.SpeedCommandFailed(wall.Elapsed.TotalSeconds, speedCommand.Request); }
            CleanupBuildingProof();
            return;
        }
        runs.Tick(Hours, speed.CurrentSpeed, wall.Elapsed.TotalSeconds, speedCommand.Request, speedLocked,
            runs.StockConditionActive ? AvailableLogs() : null,
            runs.ActiveProductionRun is { } active ? Sample(active.GoodTargets.Select(t=>t.Good),active.StopAtScience is not null) : null,
            runs.ActiveBuildingRun is not null ? buildingProof?.Sample(runs.BuildingObservationNeedsReconcile(Hours,wall.Elapsed.TotalSeconds)) : null);
        CleanupBuildingProof();
    }
    private void CleanupBuildingProof() {
        if(runs.ActiveBuildingRun is not {} active||active.State=="pausing") {buildingProof?.Dispose();buildingProof=null;}
    }
    public void Unload() { buildingProof?.Dispose();buildingProof=null;events.Unregister(this); speedCommand.Clear(); runs.Unload(s => speed.ChangeSpeed(s)); }
    public object Run(SimulationRunRequest r)
    {
        if (r.Starts) SimulationStartPolicy.RequireAvailable(speedLocked,batchOwner is not null,speedCommand.PendingSpeed is not null);
        SimulationRunStatus result;
        if(r.Starts) {
            BuildingProductionObserver? candidate=null;
            try {
                if(r.BuildingIds.Length>0)candidate=new BuildingProductionObserver(entities,journal,time,r.BuildingIds,r.MeasureProduction);
                result=runs.Start(r,Hours,speed.CurrentSpeed,wall.Elapsed.TotalSeconds,speedCommand.Request,
                    r.StopAtAvailableLogs is null?null:AvailableLogs(),production:Sample(r.GoodTargets.Select(t=>t.Good),r.StopAtScience is not null),
                    buildingProduction:candidate?.Sample());
                buildingProof=candidate;
            } catch {candidate?.Dispose();throw;}
        } else result=r.Route=="simulation-run-cancel"?runs.Cancel(r.RunId,Hours,speed.CurrentSpeed,wall.Elapsed.TotalSeconds,speedCommand.Request,speedLocked):runs.Inspect(r.RunId);
        CleanupBuildingProof();
        var payload = Newtonsoft.Json.Linq.JObject.FromObject(result, Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() }));
        if (result.HasProductionTargets && r.Diagnose) {
            try { payload["productionDiagnostics"] = Newtonsoft.Json.Linq.JArray.FromObject(ProductionRunObservations.Read(entities,resources,result), Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() })); }
            catch { payload["productionDiagnostics"] = null; }
            payload["diagnosticsObservedGameHours"] = Hours;
        }
        // Enrich only explicit inspection: never a global endpoint scan on ticks/start/cancel.
        if(r.Route=="simulation-run"&&r.Diagnose&&result.BuildingIds.Length>0) {
            payload["bottleneckObservedGameHours"]=Hours;
            try {
                var query=BuildingQueryRequest.Parse("/agent-api/v1/building-operations",new System.Collections.Specialized.NameValueCollection {
                    {"session",r.Session},{"ids",string.Join(",",result.BuildingIds)},{"diagnose","true"}});
                payload["currentBuildingOperations"]=Newtonsoft.Json.Linq.JObject.FromObject(new DiagnosticsObservations(entities,resources).Operations(query),
                    Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings { ContractResolver=new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() }));
                payload["bottleneckState"]="observed";
            } catch {payload["bottleneckState"]="unavailable";}
        }
        return payload;
    }
    public object Observe() => new { currentSpeed = speed.CurrentSpeed, dayNumber = time.DayNumber,
        dayProgress = time.DayProgress, hoursPassedToday = time.HoursPassedToday,
        activeRunId = runs.ActiveRunId, pendingSpeed = speedCommand.PendingSpeed,
        speedLocked = speedLockObserved ? (bool?)speedLocked : null,
        speedLockSource = speedLockObserved ? "speed_lock_changed_event" : "not_observed",
        speedLockReason = "not_exposed_by_public_api",
        pauseSettled = speedLockObserved ? (bool?)PauseSettled : null };

    public object Set(BridgeRequest request)
    {
        // Compare on the game thread. Never override a newer user speed or unlock a game lock.
        float previous = speed.CurrentSpeed;
        if (speedLocked || previous != request.ExpectedSpeed) throw new ArgumentException("speed_changed");
        runs.Interrupt();CleanupBuildingProof();
        speedCommand.Request(request.Speed);
        return new { requestedSpeed = request.Speed, previousSpeed = previous,
            matchedImmediately = speed.CurrentSpeed == request.Speed, observation = Observe(),
            limitations = new[] { "read_simulation_to_confirm", "no_automatic_retry", "game_locks_not_overridden",
                "standard_speeds_only_0_1_3_7", "speed_is_not_measured_tick_throughput" } };
    }
}
