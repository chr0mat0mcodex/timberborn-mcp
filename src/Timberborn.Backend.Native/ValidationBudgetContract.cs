using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ValidationBudgetContract
{
    // The expanded test budget ships under 0.35.1. Older 0.35.1 packages may
    // still report their smaller budget, so both remain valid observations.
    public static bool Accepts(string bridgeVersion, int remaining, int legacyMaximum)
    {
        int maximum = bridgeVersion is ("0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or "0.73.0")
            ? BuildingActionGate.MaxActionsPerSession - 1
            : legacyMaximum;
        return remaining >= 0 && remaining <= maximum;
    }
}
