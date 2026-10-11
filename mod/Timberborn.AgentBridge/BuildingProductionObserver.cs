using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.MechanicalSystem;
using Timberborn.TemplateSystem;
using Timberborn.TimeSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
namespace Timberborn.AgentBridge;

// Only native ProductionFinished is proof. Inventory movement and progress are never counted.
public sealed class BuildingProductionObserver : IDisposable
{
    private sealed class Watch(EntityComponent entity,Manufactory factory,string recipe,string template)
    {
        public EntityComponent Entity=entity;
        public Manufactory Factory=factory;
        public string Recipe=recipe,Template=template;
        public EventHandler Finished=null!,Changed=null!;
        public double? Proven;
        public HashSet<string> Seen=new(StringComparer.Ordinal);
    }
    private readonly List<Watch> watches=new();
    private readonly RecentActionsJournal journal;
    private readonly IDayNightCycle time;
    private long cursor,initialFailures;
    private string? failure;
    private bool disposed;
    private double Hours=>time.DayNumber*24d+time.HoursPassedToday;
    public BuildingProductionObserver(EntityRegistry entities,RecentActionsJournal journal,IDayNightCycle time,string[] ids)
    {
        this.journal=journal;this.time=time;
        journal.Capture();
        if(!journal.TrackingComplete)throw new BridgeRejectionException("building_proof_journal_gap");
        cursor=journal.Latest;initialFailures=journal.ObservationFailures;
        foreach(string id in ids) {
            var e=entities.Entities.SingleOrDefault(e=>e.EntityId.ToString("D")==id&&e.Initialized&&!e.Deleted);
            if(e is null||!e.TryGetComponent<BlockObject>(out var b)||b.IsPreview||!b.IsFinished||!e.TryGetComponent<Manufactory>(out var m))
                throw new BridgeRejectionException("building_unavailable");
            if(!m.HasCurrentRecipe)throw new BridgeRejectionException("unavailable_recipe");
            watches.Add(new Watch(e,m,m.CurrentRecipe.Id,e.GetComponent<TemplateSpec>().TemplateName));
        }
        try {
            foreach(var w in watches) {
                w.Finished=(_,_)=>Finished(w);
                w.Changed=(_,_)=>{if(!disposed)failure="building_configuration_changed";};
                w.Factory.ProductionFinished+=w.Finished;w.Factory.RecipeChanged+=w.Changed;
            }
        } catch { Dispose();throw; }
    }
    private bool Present(Watch w)=>!w.Entity.Deleted&&w.Entity.Initialized&&w.Entity.GetComponent<BlockObject>().IsFinished;
    private void Finished(Watch w)
    {
        if(disposed||failure is not null)return;
        try {
            if(!Present(w)) {failure="building_unavailable";return;}
            if(!w.Factory.HasCurrentRecipe||w.Factory.CurrentRecipe.Id!=w.Recipe) {failure="building_configuration_changed";return;}
            w.Proven ??= Hours;
        } catch {failure="building_proof_observation_unavailable";}
    }
    public BuildingProductionSample Sample()
    {
        try {
        // Reconcile once before declaring success; normal samples only read new journal entries.
        if(watches.All(w=>w.Proven is not null))journal.Capture();
        if(journal.Latest!=cursor || !journal.TrackingComplete || journal.ObservationFailures!=initialFailures) {
        var changes=journal.Window(cursor);
        if(changes.Gap||!journal.TrackingComplete||journal.ObservationFailures!=initialFailures)failure="building_proof_journal_gap";
        if(changes.Items.Any(e=>watches.Any(w=>w.Entity.EntityId.ToString("D")==e.Id)&&
            e.Kind is "paused" or "recipe" or "storage_good" or "storage_mode" or "workers"))failure="building_configuration_changed";
        cursor=changes.NextCursor;
        }
        } catch {failure="building_proof_observation_unavailable";}
        var states=new List<BuildingProductionState>();
        foreach(var w in watches) {
            var blockers=new List<string>();
            try {
                if(!Present(w)) {failure="building_unavailable";blockers.Add("observation_unavailable");}
                else {
                    var e=w.Entity;var m=w.Factory;
                    if(!m.HasCurrentRecipe||m.CurrentRecipe.Id!=w.Recipe)failure="building_configuration_changed";
                    if(e.TryGetComponent<PausableBuilding>(out var pause)&&pause.Paused)blockers.Add("building_paused");
                    if(e.TryGetComponent<Workplace>(out var workplace)&&workplace.NumberOfAssignedWorkers==0)blockers.Add("no_assigned_workers");
                    if(e.TryGetComponent<WorkplaceWorkingHours>(out var workHours)&&!workHours.AreWorkingHours)blockers.Add("outside_working_hours");
                    if(!m.HasAllIngredients)blockers.Add("ingredients_not_ready");
                    if(m.HasCurrentRecipe&&m.CurrentRecipe.ConsumesFuel&&!m.HasFuel)blockers.Add("fuel_not_ready");
                    if(m.HasCurrentRecipe&&!m.HasUnreservedCapacityForCurrentProducts())blockers.Add("output_capacity_unavailable");
                    if(e.TryGetComponent<MechanicalNode>(out var node)&&node.IsConsumer) {
                        if(!node.Active)blockers.Add("consumer_inactive");
                        else if(node.Graph is null||!node.Graph.Valid)blockers.Add("power_network_unknown");
                        else if(!node.Powered)blockers.Add("consumer_not_powered");
                    }
                }
            } catch {failure="building_proof_observation_unavailable";blockers.Add("observation_unavailable");}
            foreach(string blocker in blockers)w.Seen.Add(blocker);
            states.Add(new BuildingProductionState {Id=w.Entity.EntityId.ToString("D"),Template=w.Template,Recipe=w.Recipe,
                CompletedCycles=w.Proven is null?0:1,ProvenGameHours=w.Proven,ObservedGameHours=Hours,
                Blockers=blockers.Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToArray(),SeenBlockers=w.Seen.OrderBy(x=>x,StringComparer.Ordinal).ToArray()});
        }
        return new BuildingProductionSample {Buildings=states.ToArray(),Failure=failure,JournalCursor=cursor};
    }
    public void Dispose()
    {
        if(disposed)return;disposed=true;
        foreach(var w in watches) {w.Factory.ProductionFinished-=w.Finished;w.Factory.RecipeChanged-=w.Changed;}
    }
}
