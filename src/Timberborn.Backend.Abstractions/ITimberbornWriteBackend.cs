namespace Timberborn.Backend.Abstractions;

public interface ITimberbornWriteBackend
{
    bool WritesEnabled { get; }
    Task SetBuildingPausedAsync(Guid id, bool paused, CancellationToken ct);
}
