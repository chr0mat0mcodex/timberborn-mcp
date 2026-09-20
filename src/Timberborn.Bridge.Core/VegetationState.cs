namespace Timberborn.Bridge.Core;

// Missing component data stays unknown; absence is never evidence of health.
public sealed class VegetationState
{
    public string LifeState { get; }
    public bool? IsDying { get; }
    public bool? WaterStress { get; }
    public bool? IsGrown { get; }
    public float? GrowthProgress { get; }
    public VegetationState(string lifeState, bool? isDying, bool? waterStress, bool? isGrown, float? growthProgress)
    { LifeState=lifeState; IsDying=isDying; WaterStress=waterStress; IsGrown=isGrown; GrowthProgress=growthProgress; }
    public bool IsTappingCandidate() => LifeState=="alive" && IsDying==false && WaterStress==false && IsGrown==true;
    public bool IsValid() => (LifeState is "alive" or "dead" or "unknown") &&
        (!GrowthProgress.HasValue || (!float.IsNaN(GrowthProgress.Value) && !float.IsInfinity(GrowthProgress.Value) && GrowthProgress.Value>=0));
}
