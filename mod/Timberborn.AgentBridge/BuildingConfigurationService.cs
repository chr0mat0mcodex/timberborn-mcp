using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.TemplateSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
namespace Timberborn.AgentBridge;

public sealed class BuildingConfigurationService(EntityRegistry entities,SimulationControl simulation) : IBuildingConfigurationPort
{
    private readonly BuildingConfigurationController controller=new();
    private Dictionary<string,EntityComponent>? batchEntities;
    private EntityComponent Find(string id)
    {
        EntityComponent? e;
        if(batchEntities is not null)batchEntities.TryGetValue(id,out e);
        else e=entities.Entities.SingleOrDefault(e=>e.EntityId.ToString("D")==id);

        if(e is null||!e.Initialized||e.Deleted||!e.TryGetComponent<BlockObject>(out var b)||b.IsPreview||!b.IsFinished||
            !e.HasComponent<Manufactory>()||!e.HasComponent<Workplace>()||!e.HasComponent<PausableBuilding>())
            throw new BridgeRejectionException("building_unavailable");
        return e;
    }
    public ConfigurationCheck Read(string id)
    {
        var e=Find(id);var m=e.GetComponent<Manufactory>();var w=e.GetComponent<Workplace>();var p=e.GetComponent<PausableBuilding>();
        return new ConfigurationCheck {State=new ConfigurationObservation {Template=e.GetComponent<TemplateSpec>().TemplateName,
            Recipe=m.HasCurrentRecipe?m.CurrentRecipe.Id:"",DesiredWorkers=w.DesiredWorkers,Paused=p.Paused},
            Recipes=m.ProductionRecipes.Select(r=>r.Id).ToArray(),MaxWorkers=w.MaxWorkers,CanPause=p.IsPausable()};
    }
    public void SetRecipe(string id,string recipe)
    {
        var m=Find(id).GetComponent<Manufactory>();
        var choice=m.ProductionRecipes.SingleOrDefault(r=>r.Id==recipe)??throw new BridgeRejectionException("unavailable_recipe");
        m.SetRecipe(choice);
    }
    public void SetWorkers(string id,int workers)
    {
        var w=Find(id).GetComponent<Workplace>();
        if(workers<1||workers>w.MaxWorkers)throw new ArgumentException();
        int steps=Math.Abs(workers-w.DesiredWorkers);
        for(int i=0;i<steps;i++) {
            int before=w.DesiredWorkers;int delta=workers>before?1:-1;
            if(delta>0)w.IncreaseDesiredWorkers();else w.DecreaseDesiredWorkers();
            if(w.DesiredWorkers!=before+delta)throw new InvalidOperationException();
        }
    }
    public void SetPaused(string id,bool paused)
    {
        var p=Find(id).GetComponent<PausableBuilding>();
        if(!p.IsPausable())throw new BridgeRejectionException("not_pausable");
        if(paused)p.Pause();else p.Resume();
    }
    public object Handle(BuildingConfigurationRequest r)
    {
        try {
            if(r.Write) {
                var ids=new HashSet<string>(r.Items.Select(i=>i.Id),StringComparer.Ordinal);
                batchEntities=entities.Entities.Where(e=>ids.Contains(e.EntityId.ToString("D"))).ToDictionary(e=>e.EntityId.ToString("D"));
            }
            var result=r.Write?controller.Execute(r,this,()=>simulation.RequireConfigurationAvailable(r),()=>simulation.Run(r.ProofRequest())):controller.Inspect(r.JobId);
            return Newtonsoft.Json.Linq.JObject.FromObject(result,Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings {
                ContractResolver=new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() }));
        } finally {batchEntities=null;}
    }
}
