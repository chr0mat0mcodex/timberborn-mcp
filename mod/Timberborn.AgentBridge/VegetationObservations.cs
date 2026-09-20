using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.Growing;
using Timberborn.NaturalResources;
using Timberborn.NaturalResourcesLifecycle;
using Timberborn.NaturalResourcesMoisture;
namespace Timberborn.AgentBridge;

internal static class VegetationObservations
{
    // Explicit wire names: the game uses Newtonsoft defaults, while MCP uses camelCase.
    public static object? ToPayload(VegetationState? s) => s is null ? null : new { lifeState=s.LifeState, isDying=s.IsDying, waterStress=s.WaterStress, isGrown=s.IsGrown, growthProgress=s.GrowthProgress };
    public static VegetationState? Observe(EntityComponent e)
    {
        if(!e.HasComponent<NaturalResource>())return null;
        string life=e.TryGetComponent<LivingNaturalResource>(out var living)?(living.IsDead?"dead":"alive"):"unknown";
        bool? dying=e.TryGetComponent<DyingNaturalResource>(out var d)?d.IsDying:(bool?)null;
        bool? waterStress=e.TryGetComponent<WateredNaturalResource>(out var water)?water.DyingProgress.IsDying:(bool?)null;
        bool grows=e.TryGetComponent<Growable>(out var growth);
        return new VegetationState(life,dying,waterStress,grows?growth.IsGrown:(bool?)null,grows?growth.GrowthProgress:(float?)null);
    }
}
