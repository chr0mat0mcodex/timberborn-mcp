using System.Text.Json;

namespace Timberborn.Contracts;

public static class ContractJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    { PropertyNameCaseInsensitive = false, WriteIndented = false };
}
public sealed record Fault(string Code, string Message, bool Retryable, string SuggestedAction);
public sealed record Warning(string Code, string Message, string Section);
public sealed record SnapshotMetadata(string Backend, bool Simulated, string? GameVersion,
    DateTimeOffset ObservationStartedAtUtc, DateTimeOffset ObservationCompletedAtUtc, bool Partial);
public sealed record ToolResult<T>(int SchemaVersion, string Status, T? Data, SnapshotMetadata Meta,
    IReadOnlyList<Warning> Warnings, Fault? Error);
public sealed record Capability(string State, DateTimeOffset? CheckedAtUtc);
public sealed record BackendCapabilities(Capability GameInfo, Capability LiveData, Capability Population,
    Capability Buildings, Capability BuildingDetails);
public sealed record ModInfo(string Id, string Name, string Version, bool Enabled);
public sealed record StatusData(string Connection, bool? GameLoaded, string? BackendVersion,
    BackendCapabilities Capabilities, IReadOnlyList<ModInfo>? Mods, Fault? Diagnostic);
public sealed record GameInfo(string GameVersion, IReadOnlyList<ModInfo> Mods);
public sealed record GameTimeSnapshot(int Cycle, int CycleDay, double DayProgress);
public sealed record WeatherSnapshot(string Current, string? Next, double? DaysUntilNext, bool? ForecastVisible);
public sealed record LiveSnapshot(GameTimeSnapshot Time, WeatherSnapshot Weather, double GameSpeed);
public sealed record PopulationCounts(int Adults, int Children, int Beavers, int Bots, int TotalEntities);
public sealed record CharacterSummary(Guid Id, string Kind, string? Name, double? AgeDays,
    double? Wellbeing, Guid? HomeId, Guid? WorkplaceId, Guid? DistrictId);
public sealed record BuildingSummary(Guid Id, string Name, string Template, bool? Pausable, bool? Paused);
public sealed record PauseActionResult(Guid Id, bool RequestedPaused, bool? BeforePaused,
    bool? ObservedPaused, string Outcome);
public sealed record BuildingAggregate(int Total, int Paused, int Active, int NotPausable, int PauseStateUnknown);
public sealed record ColonySnapshot(GameTimeSnapshot? Time, WeatherSnapshot? Weather,
    PopulationCounts? Population, BuildingAggregate? Buildings, double? GameSpeed, string ResourceAvailability);
public sealed record Page(int Offset, int Limit, int Returned, int MatchedTotal, int? NextOffset);
public sealed record BuildingPage(IReadOnlyList<BuildingSummary> Items, Page Page);
public sealed record PopulationSnapshot(PopulationCounts Counts, int MatchedTotal,
    IReadOnlyList<CharacterSummary>? Items, Page? Page);
