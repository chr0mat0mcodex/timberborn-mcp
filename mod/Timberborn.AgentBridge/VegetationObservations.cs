using Timberborn.Bridge.Core;
using Timberborn.EntitySystem;
using Timberborn.Growing;
using Timberborn.NaturalResources;
using Timberborn.NaturalResourcesLifecycle;
using Timberborn.NaturalResourcesMoisture;
using Timberborn.Cutting;
namespace Timberborn.AgentBridge;

internal static class VegetationObservations
{
    // Explicit wire names: the game uses Newtonsoft defaults, while MCP uses camelCase.
    public static object? ToPayload(VegetationState? s) => s is null ? null : new { lifeState=s.LifeState, isDying=s.IsDying, waterStress=s.WaterStress, isGrown=s.IsGrown, growthProgress=s.GrowthProgress,
        cutYieldRemoved=s.CutYieldRemoved, cutIsYielding=s.CutIsYielding, cutYieldGood=s.CutYieldGood, cutYieldAmount=s.CutYieldAmount };
    public static VegetationState? Observe(EntityComponent e)
    {
        if(!e.HasComponent<NaturalResource>())return null;
        string life=e.TryGetComponent<LivingNaturalResource>(out var living)?(living.IsDead?"dead":"alive"):"unknown";
        bool? dying=e.TryGetComponent<DyingNaturalResource>(out var d)?d.IsDying:(bool?)null;
        bool? waterStress=e.TryGetComponent<WateredNaturalResource>(out var water)?water.DyingProgress.IsDying:(bool?)null;
        bool grows=e.TryGetComponent<Growable>(out var growth);
        var yield = e.TryGetComponent<Cuttable>(out var cuttable) ? cuttable.Yielder : null;
        return new VegetationState(life,dying,waterStress,grows?growth.IsGrown:(bool?)null,grows?growth.GrowthProgress:(float?)null,
            yield?.IsYieldRemoved, yield?.IsYielding, yield is null ? null : yield.Yield.GoodId, yield is null ? (int?)null : yield.Yield.Amount);
    }
}
