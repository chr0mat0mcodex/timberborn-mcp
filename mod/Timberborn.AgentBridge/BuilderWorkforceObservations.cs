using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.TemplateSystem;
using Timberborn.WorkSystem;

namespace Timberborn.AgentBridge;

public sealed partial class LargeProjectService
{
    private BuilderWorkforceEvidence ObserveBuilderWorkforce(string districtId)
    {
        var candidates=new List<BuilderWorkplaceEvidence>();
        string? Id(DistrictCenter? center) => center?.GetComponent<EntityComponent>().EntityId.ToString("D");
        foreach(var e in entities.Entities) {
            if(e.Deleted || !e.TryGetComponent<TemplateSpec>(out var template) || !BuilderWorkforcePolicy.IsBuilder(template.TemplateName) ||
                !e.TryGetComponent<BlockObject>(out var block) || block.IsPreview || !block.IsFinished) continue;
            string id=e.EntityId.ToString("D");
            string? membership=null;
            if(template.TemplateName.StartsWith("DistrictCenter.",StringComparison.Ordinal)) membership=id;
            else if(e.TryGetComponent<DistrictBuilding>(out var district)) {
                var assignments=new[]{Id(district.District),Id(district.InstantDistrict)}.Where(x=>x is not null).Distinct().ToArray();
                if(assignments.Length==1) membership=assignments[0];
            }
            if(membership is not null && membership!=districtId) continue;
            bool workplace=e.Initialized && e.TryGetComponent<Workplace>(out _);
            var w=workplace?e.GetComponent<Workplace>():null;
            candidates.Add(new BuilderWorkplaceEvidence {
                Id=id,Template=template.TemplateName,DistrictId=membership,
                AssignedWorkers=w?.NumberOfAssignedWorkers,AnyJobRunning=w?.AnyWorkerHasJobRunning(),
                Paused=e.Initialized && e.TryGetComponent<PausableBuilding>(out var pause)?(bool?)pause.Paused:null,
                WorkingHours=workplace && e.TryGetComponent<WorkplaceWorkingHours>(out var hours)?(bool?)hours.AreWorkingHours:null
            });
        }
        var result=new BuilderWorkforceEvidence {DistrictId=districtId,Source=BuilderWorkforcePolicy.Source,
            Complete=candidates.Count<=BuilderWorkforcePolicy.Limit,
            Workplaces=candidates.OrderBy(w=>w.Id==districtId?0:1).ThenBy(w=>w.Id,StringComparer.Ordinal).Take(BuilderWorkforcePolicy.Limit).ToArray()};
        result.WorkingHours=BuilderWorkforcePolicy.Evaluate(result);
        return result;
    }
}
