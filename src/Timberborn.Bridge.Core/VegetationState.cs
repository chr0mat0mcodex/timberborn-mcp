namespace Timberborn.Bridge.Core;

// Missing component data stays unknown; absence is never evidence of health.
public sealed class VegetationState
{
    public string LifeState { get; }
    public bool? IsDying { get; }
    public bool? WaterStress { get; }
    public bool? IsGrown { get; }
    public float? GrowthProgress { get; }
    public bool? CutYieldRemoved { get; }
    public bool? CutIsYielding { get; }
    public string? CutYieldGood { get; }
    public int? CutYieldAmount { get; }
    public VegetationState(string lifeState, bool? isDying, bool? waterStress, bool? isGrown, float? growthProgress,
        bool? cutYieldRemoved = null, bool? cutIsYielding = null, string? cutYieldGood = null, int? cutYieldAmount = null)
    { LifeState=lifeState; IsDying=isDying; WaterStress=waterStress; IsGrown=isGrown; GrowthProgress=growthProgress;
      CutYieldRemoved=cutYieldRemoved; CutIsYielding=cutIsYielding; CutYieldGood=cutYieldGood; CutYieldAmount=cutYieldAmount; }
    public bool IsTappingCandidate() => LifeState=="alive" && IsDying==false && WaterStress==false && IsGrown==true;
    public bool IsValid() => (LifeState is "alive" or "dead" or "unknown") &&
        (!CutYieldAmount.HasValue || CutYieldAmount.Value >= 0) &&
        (CutYieldGood is null || CutYieldGood.Length is > 0 and <= 160) &&
        (!GrowthProgress.HasValue || (!float.IsNaN(GrowthProgress.Value) && !float.IsInfinity(GrowthProgress.Value) && GrowthProgress.Value>=0));
}
