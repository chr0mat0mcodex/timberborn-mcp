using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.Demolishing;
using Timberborn.EntitySystem;
using Timberborn.InventorySystem;
using Timberborn.PathSystem;
using Timberborn.SingletonSystem;
using Timberborn.StockpilePrioritySystem;
using Timberborn.TemplateSystem;
using Timberborn.TimeSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
namespace Timberborn.AgentBridge;

// Public events plus bounded reconciliation. No UI hooks, private reflection or save data.
public sealed class RecentActionsJournal
{
    private readonly EntityRegistry entities;
    private readonly EventBus events;
    private readonly SpeedManager speed;
    private readonly IDayNightCycle time;
    private readonly RecentActionBuffer buffer=new();
    private readonly Dictionary<Guid,State> states=new();
    private readonly Stopwatch clock=Stopwatch.StartNew();
    private double nextScan;
    private bool started;
    private bool complete;
    private long failures;
    private int mcpDepth;
    private float lastSpeed;
    private DateTimeOffset? startedAt;
    private DateTimeOffset? lastScanAt;
    private const int MaxObjects=4096,MaxRegistryEntries=20000;
    private string Source=>mcpDepth>0?"mcp":"unknown";
    private double Hours=>time.DayNumber*24d+time.HoursPassedToday;
    private sealed class State(string id,string template,int x,int y,int z,Dictionary<string,string> values)
    {
        public string Id=id,Template=template;
        public int X=x,Y=y,Z=z;
        public Dictionary<string,string> Values=values;
    }
    public RecentActionsJournal(EntityRegistry entities,EventBus events,SpeedManager speed,IDayNightCycle time)
    {
        this.entities=entities;this.events=events;this.speed=speed;this.time=time;
        events.Register(this);
    }
    public long Latest => buffer.Latest;
    public long ObservationFailures => failures;
    public bool TrackingComplete => complete;
    public RecentActionPage Window(long after)=>buffer.Window(after);
    public void Unload()=>events.Unregister(this);
    public void Update() { if(clock.Elapsed.TotalSeconds<nextScan)return;nextScan=clock.Elapsed.TotalSeconds+1;Capture(); }
    public void BeginMcp(bool reconcile) { if(reconcile)Capture();mcpDepth++; }
    public void EndMcp(bool reconcile) { try { if(reconcile)Capture(); } finally {mcpDepth--;} }
    public IDisposable Scope(string route)=>new McpScope(this,route is
        "planting-apply" or "configure-buildings" or "large-project" or "building-project-execution" or "vertical-stair-execution" or
        "building-placement" or "path-placement" or "lodge-placement" or "remove-object" or
        "set-building-paused" or "set-storage-good" or "set-storage-mode" or "set-building-recipe" or
        "workplace-staffing" or "simulation-speed" or "simulation-run-for" or "simulation-run-until" or "simulation-run-cancel");
    private sealed class McpScope : IDisposable
    {
        private readonly RecentActionsJournal owner;private readonly bool enabled;
        public McpScope(RecentActionsJournal owner,bool enabled) {this.owner=owner;this.enabled=enabled;if(enabled)owner.BeginMcp(true);}
        public void Dispose() {if(enabled)owner.EndMcp(true);}
    }
    private void Guard(Action action) { try { action(); } catch { failures++;complete=false; } }
    private static State? Observe(EntityComponent e)
    {
        if(!e.Initialized||e.Deleted||!e.TryGetComponent<BlockObject>(out var b)||b.IsPreview||
            !(e.HasComponent<Building>()||e.HasComponent<PathSpec>())||!e.TryGetComponent<TemplateSpec>(out var t))return null;
        var v=new Dictionary<string,string>{{"finished",b.IsFinished?"true":"false"}};
        if(e.TryGetComponent<PausableBuilding>(out var p))v["paused"]=p.Paused?"true":"false";
        if(e.TryGetComponent<Demolishable>(out var d))v["demolition_marked"]=d.IsMarked?"true":"false";
        if(b.IsFinished) {
            if(e.TryGetComponent<Manufactory>(out var m))v["recipe"]=m.HasCurrentRecipe?m.CurrentRecipe.Id:"";
            if(e.TryGetComponent<SingleGoodAllower>(out var g))v["storage_good"]=g.HasAllowedGood?g.AllowedGood:"";
            if(e.TryGetComponent<StockpilePriority>(out var sp))v["storage_mode"]=sp.IsAcceptActive?"accept":sp.IsEmptyActive?"empty":sp.IsObtainActive?"obtain":sp.IsSupplyActive?"supply":"unknown";
            if(e.TryGetComponent<Workplace>(out var w))v["workers"]=w.DesiredWorkers.ToString(CultureInfo.InvariantCulture);
        }
        return new State(e.EntityId.ToString("D"),t.TemplateName,b.Coordinates.x,b.Coordinates.y,b.Coordinates.z,v);
    }
    private void Add(string kind,string evidence,State? s,string? before,string? after)=>
        buffer.Add(DateTimeOffset.UtcNow,Hours,kind,Source,evidence,s?.Id,s?.Template,s?.X,s?.Y,s?.Z,before,after);
    private void Track(Guid id,State current,string evidence)
    {
        if(states.TryGetValue(id,out var previous)) {
            foreach(var pair in current.Values)
                if(previous.Values.TryGetValue(pair.Key,out var before)&&before!=pair.Value)Add(pair.Key,"state_difference",current,before,pair.Value);
        } else Add("object_added",evidence,current,null,current.Values["finished"]=="true"?"finished":"construction");
        states[id]=current;
    }
    public void Capture()=>Guard(()=>{
        var snapshot=new Dictionary<Guid,State>();int examined=0;bool full=true;
        foreach(var e in entities.Entities) {
            if(++examined>MaxRegistryEntries) {full=false;break;}
            var state=Observe(e);if(state is null)continue;
            if(snapshot.Count>=MaxObjects) {full=false;break;}
            snapshot[e.EntityId]=state;
        }
        if(!full) { complete=false;failures++;lastScanAt=DateTimeOffset.UtcNow;return; }
        if(!started) {
            foreach(var pair in snapshot)states[pair.Key]=pair.Value;
            lastSpeed=speed.CurrentSpeed;startedAt=DateTimeOffset.UtcNow;started=true;
        } else {
            foreach(var pair in snapshot)Track(pair.Key,pair.Value,"state_difference");
            // Partial coverage must never manufacture removals.
            if(full)foreach(var id in states.Keys.Where(id=>!snapshot.ContainsKey(id)).ToArray()) {
                Add("object_removed","state_difference",states[id],"present",null);states.Remove(id);
            }
            TrackSpeed(speed.CurrentSpeed,"state_difference");
        }
        complete=full;lastScanAt=DateTimeOffset.UtcNow;
    });
    private void TrackSpeed(float value,string evidence)
    {
        if(lastSpeed!=value)Add("speed",evidence,null,lastSpeed.ToString(CultureInfo.InvariantCulture),value.ToString(CultureInfo.InvariantCulture));
        lastSpeed=value;
    }
    [OnEvent] public void Initialized(EntityInitializedEvent change)=>Guard(()=>{
        if(!started)return;var s=Observe(change.Entity);if(s is null)return;
        if(states.Count>=MaxObjects&&!states.ContainsKey(change.Entity.EntityId)) {complete=false;failures++;return;}
        Track(change.Entity.EntityId,s,"event");
    });
    [OnEvent] public void Deleted(EntityDeletedEvent change)=>Guard(()=>{
        if(!started||!states.TryGetValue(change.Entity.EntityId,out var s))return;
        Add("object_removed","event",s,"present",null);states.Remove(change.Entity.EntityId);
    });
    [OnEvent] public void SpeedChanged(CurrentSpeedChangedEvent change)=>Guard(()=>{if(started)TrackSpeed(change.CurrentSpeed,"event");});
    public object Read(RecentActionsRequest r)
    {
        Capture();
        var page=buffer.Read(r);
        var payload=new {capacity=RecentActionBuffer.Capacity,startedAtUtc=startedAt,lastScanAtUtc=lastScanAt,
            trackingComplete=complete,observationFailures=failures,trackedObjects=states.Count,
            maxObjects=MaxObjects,scanIntervalSeconds=1,page,
            supportedKinds=RecentActionsRequest.Kinds,
            limitations=new[]{"sources_mcp_or_unknown_only_no_human_attribution","settings_sampled_intermediate_changes_may_be_missed","registry_scan_capped_at_20000_entries",
                "buildings_and_paths_only_no_plants_area_marks_or_research","object_removed_is_observed_disappearance_not_demolition_order",
                "object_added_may_be_finished_or_construction_not_command_receipt","no_history_before_startedAt_or_across_session",
                "delayed_mcp_effects_may_have_unknown_source","finished_change_does_not_prove_operating_or_access",
                "trackingComplete_means_last_scan_within_limits_not_complete_action_history","failure_count_is_cumulative_read_gap_and_failures_together"}};
        return JObject.FromObject(payload,JsonSerializer.Create(new JsonSerializerSettings{ContractResolver=new CamelCasePropertyNamesContractResolver()}));
    }
}
