namespace Timberborn.Backend.Native;

// Wire-contract knowledge only: no faction, geometry, execution or live-test approval.
public sealed record BridgeVersionProfile(bool ExpandedValidationBudget);

public static class BridgeVersionProfiles
{
    private static readonly BridgeVersionProfile LegacyBudget = new(false);
    private static readonly BridgeVersionProfile ExpandedBudget = new(true);

    // Exact, case-sensitive allowlist. Unknown patches/future versions stay unknown.
    // Keep each known version in exactly one profile; never infer support from ordering.
    public static BridgeVersionProfile? Find(string? version) => version switch
    {
        "0.2.0" or "0.3.0" or "0.4.0" or "0.4.1" or "0.5.0" or "0.6.0" or
        "0.6.1" or "0.7.0" or "0.8.0" or "0.9.0" or "0.10.0" or "0.11.0" or
        "0.12.0" or "0.13.0" or "0.13.1" or "0.13.2" or "0.14.0" or "0.14.1" or
        "0.15.0" or "0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0" or
        "0.19.0" or "0.19.1" or "0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or
        "0.21.1" or "0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or
        "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or
        "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or
        "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or
        "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" => LegacyBudget,
        "0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or
        "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or
        "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or
        "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or
        "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or
        "0.61.0" or "0.62.0" or "0.63.0" or "0.64.0" or "0.65.0" or "0.66.0" or
        "0.67.0" or "0.68.0" or "0.69.0" or "0.70.0" or "0.71.0" or "0.72.0" or
        "0.73.0" or "0.74.0" => ExpandedBudget,
        _ => null
    };
}
