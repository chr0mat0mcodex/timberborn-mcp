namespace Timberborn.Bridge.Core;

// Fresh identity observations, never a retry authorization or inferred placement success.
public static class LargeProjectRecoveryPolicy
{
    public static bool Resolved(string recorded, string observed) =>
        recorded == "pending" ? observed == "missing" :
        observed == "finished" || observed == "missing" && recorded is "construction" or "finished";

    public static bool Repairable(string recorded, string observed) =>
        Resolved(recorded, observed) || recorded != "pending" &&
        observed is "construction" or "construction_unreachable";

    public static bool AccessHelper(string template) => template is "Path" or
        "Stairs.Folktails" or "Platform.Folktails" or "DoublePlatform.Folktails" or "TriplePlatform.Folktails" or
        "Stairs.IronTeeth" or "Platform.IronTeeth" or "DoublePlatform.IronTeeth" or "TriplePlatform.IronTeeth";
}
