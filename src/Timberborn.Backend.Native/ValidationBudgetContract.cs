using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ValidationBudgetContract
{
    // The expanded test budget ships under 0.35.1. Older 0.35.1 packages may
    // still report their smaller budget, so both remain valid observations.
    // Preserve the standalone legacy fallback for unknown versions. The envelope
    // rejects those versions separately; a fitting counter is not version support.
    public static bool Accepts(string bridgeVersion, int remaining, int legacyMaximum)
    {
        int maximum = BridgeVersionProfiles.Find(bridgeVersion)?.ExpandedValidationBudget == true
            ? BuildingActionGate.MaxActionsPerSession - 1
            : legacyMaximum;
        return remaining >= 0 && remaining <= maximum;
    }
}
