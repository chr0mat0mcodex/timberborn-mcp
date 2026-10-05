using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ValidationBudgetContract
{
    // The expanded test budget ships under 0.35.1. Older 0.35.1 packages may
    // still report their smaller budget, so both remain valid observations.
    public static bool Accepts(string bridgeVersion, int remaining, int legacyMaximum)
    {
        int maximum = bridgeVersion is ("0.35.1" or "0.35.2" or "0.35.3" or "0.35.4")
            ? BuildingActionGate.MaxActionsPerSession - 1
            : legacyMaximum;
        return remaining >= 0 && remaining <= maximum;
    }
}
