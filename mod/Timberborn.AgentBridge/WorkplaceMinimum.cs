using System.Runtime.CompilerServices;
using Timberborn.Bridge.Core;
using Timberborn.WorkSystem;
namespace Timberborn.AgentBridge;

internal static class WorkplaceMinimum
{
    // Weak keys expire with scene entities. No saved state and no probing writes.
    private static readonly ConditionalWeakTable<Workplace,StaffingMinimumObservation> observations = new();
    public static StaffingMinimumObservation Read(Workplace workplace)
    {
        var value=observations.GetValue(workplace,_=>new StaffingMinimumObservation());
        value.Observe(workplace.DesiredWorkers);
        return value;
    }
}
