using Timberborn.BlockSystem;
using Timberborn.Bridge.Core;
using Timberborn.Buildings;
using Timberborn.ScienceSystem;
using Timberborn.TemplateSystem;
namespace Timberborn.AgentBridge;

public sealed class Research(TemplateService templates, ScienceService science, BuildingUnlockingService unlocking)
{
    private TemplateSpec[] Templates() => templates.GetAll<TemplateSpec>()
        .Where(t=>t.HasSpec<BuildingSpec>() && t.HasSpec<PlaceableBlockObjectSpec>())
        .GroupBy(t=>t.TemplateName).Select(g=>g.First()).OrderBy(t=>t.TemplateName,StringComparer.Ordinal).ToArray();
    private static bool Available(TemplateSpec t) => t.UsableWithCurrentFeatureToggles &&
        t.GetSpec<PlaceableBlockObjectSpec>().UsableWithCurrentFeatureToggles && !t.GetSpec<PlaceableBlockObjectSpec>().DevModeTool;
    public object Read(ResearchRequest r)
    {
        var all=Templates();
        var items=all.Skip(r.Offset).Take(r.Limit).Select(t=> {
            var b=t.GetSpec<BuildingSpec>();
            return new { template=t.TemplateName, scienceCost=b.ScienceCost, available=Available(t),
                unlocked=unlocking.Unlocked(b), unlockable=unlocking.Unlockable(b) };
        }).ToArray();
        return new { sciencePoints=science.SciencePoints, offset=r.Offset, limit=r.Limit, total=all.Length, items,
            hasMore=r.Offset+items.Length<all.Length, limitations=new[]{"active_scene_templates","unlockable_is_game_predicate","prerequisite_graph_not_exposed","pages_are_separate_observations"} };
    }
    public object Unlock(ResearchRequest r)
    {
        var t=Templates().SingleOrDefault(t=>t.TemplateName==r.Template) ?? throw new ArgumentException("unknown_template");
        var b=t.GetSpec<BuildingSpec>();int before=science.SciencePoints;bool wasUnlocked=unlocking.Unlocked(b);
        string outcome=ResearchTransaction.Execute(r.ExpectedCost,b.ScienceCost,Available(t),()=>science.SciencePoints,
            ()=>unlocking.Unlocked(b),()=>unlocking.Unlockable(b),()=>unlocking.Unlock(b));
        return new { template=r.Template, expectedCost=r.ExpectedCost, scienceCost=b.ScienceCost,
            pointsBefore=before, pointsAfter=science.SciencePoints, previouslyUnlocked=wasUnlocked,
            unlocked=unlocking.Unlocked(b), outcome };
    }
}
