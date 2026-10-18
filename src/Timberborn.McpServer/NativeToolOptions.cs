namespace Timberborn.McpServer;

// MCP opt-ins only. The bridge enforces its own independently configured permissions.
public sealed record NativeToolOptions
{
    public bool EnableValidation { get; init; }
    public bool EnablePlacement { get; init; }
    public bool EnableLodgePlacement { get; init; }
    public bool EnableSpeedControl { get; init; }
    public bool EnableStaffing { get; init; }
    public bool EnablePriorities { get; init; }
    public bool EnableAreas { get; init; }
    public bool EnableRemoval { get; init; }
    public bool EnableBuildingPlacement { get; init; }
    public bool EnableBuildingSettings { get; init; }
    public bool EnableResearch { get; init; }

    public static NativeToolOptions AllEnabled { get; } = new()
    {
        EnableValidation = true,
        EnablePlacement = true,
        EnableLodgePlacement = true,
        EnableSpeedControl = true,
        EnableStaffing = true,
        EnablePriorities = true,
        EnableAreas = true,
        EnableRemoval = true,
        EnableBuildingPlacement = true,
        EnableBuildingSettings = true,
        EnableResearch = true,
    };
}
