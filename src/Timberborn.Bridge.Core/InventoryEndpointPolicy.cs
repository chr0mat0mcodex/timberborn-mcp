namespace Timberborn.Bridge.Core;

public static class InventoryEndpointPolicy
{
    // Native direction-specific collections/services need not exist on other roles.
    public static bool Matches(bool receiving, bool isInput, bool isOutput,
        Func<bool> canReceive, Func<bool> canSupply) => receiving
        ? isInput && canReceive()
        : isOutput && canSupply();
}
