namespace Timberborn.McpServer;

public enum NativeToolKind { Read, Mutation, Status, Cancel }

// Pilot metadata for simulation runs, not a general authorization framework.
// Status stays readable with actions disabled; Cancel still requires the speed opt-in.
public sealed record NativeToolDescriptor(string Name, string Route, NativeToolKind Kind)
{
    public bool Writes => Kind is NativeToolKind.Mutation or NativeToolKind.Cancel;
    public bool IsEnabled(bool speedControlEnabled) => !Writes || speedControlEnabled;
}
