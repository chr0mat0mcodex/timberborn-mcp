using System.IO;
namespace Timberborn.Bridge.Core;

public sealed class BuilderWorkplaceEvidence
{
    public string Id { get; set; } = "";
    public string Template { get; set; } = "";
    public string? DistrictId { get; set; }
    public int? AssignedWorkers { get; set; }
    public bool? Paused { get; set; }
    public bool? WorkingHours { get; set; }
    public bool? AnyJobRunning { get; set; }
}

public sealed class BuilderWorkforceEvidence
{
    public string DistrictId { get; set; } = "";
    public string Source { get; set; } = "";
    public bool Complete { get; set; }
    public bool? WorkingHours { get; set; }
    public BuilderWorkplaceEvidence[] Workplaces { get; set; } = Array.Empty<BuilderWorkplaceEvidence>();
}

public static class BuilderWorkforcePolicy
{
    public const int Limit = 64;
    public const string Source = "district_builder_workplaces";
    public static bool IsBuilder(string template) => template is "DistrictCenter.Folktails" or "DistrictCenter.IronTeeth" or
        "BuildersHut.Folktails" or "BuildersHut.IronTeeth";
    // true: observed available work window/activity; false: all eligible observed builders off duty;
    // null: coverage, membership, staffing or schedule insufficient (including no staffed builders).
    public static bool? Evaluate(BuilderWorkforceEvidence evidence)
    {
        bool Id(string? id) => Guid.TryParseExact(id,"D",out var g) && g!=Guid.Empty;
        var items=evidence.Workplaces;
        if(!Id(evidence.DistrictId)||evidence.Source!=Source||items is null||items.Length>Limit||
            items.Any(w=>w is null||!Id(w.Id)||!IsBuilder(w.Template)||w.DistrictId is not null&&!Id(w.DistrictId)||w.AssignedWorkers<0)||
            items.Select(w=>w.Id).Distinct().Count()!=items.Length)
            throw new InvalidDataException("invalid_builder_workforce_evidence");
        var local=items.Where(w=>w.DistrictId==evidence.DistrictId).ToArray();
        if(local.Any(w=>w.AnyJobRunning==true || w.AssignedWorkers>0 && w.Paused==false && w.WorkingHours==true)) return true;
        if(!evidence.Complete || !local.Any(w=>w.Id==evidence.DistrictId && w.Template.StartsWith("DistrictCenter.",StringComparison.Ordinal)) ||
            items.Any(w=>w.DistrictId is null)) return null;
        var eligible=local.Where(w=>w.Paused!=true && w.AssignedWorkers!=0).ToArray();
        if(eligible.Length==0 || eligible.Any(w=>w.AssignedWorkers is null||w.Paused is null||w.WorkingHours is null||w.AnyJobRunning is null)) return null;
        return false;
    }
}
