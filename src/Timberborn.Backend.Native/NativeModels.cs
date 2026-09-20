using System.Text.Json;

namespace Timberborn.Backend.Native;

public static class NativeJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    { PropertyNameCaseInsensitive = false, RespectRequiredConstructorParameters = true };
}
public sealed record NativeConfiguration(int Port, string Token)
{
    public static NativeConfiguration Load(string path)
    {
        if (!Path.IsPathFullyQualified(path) || new FileInfo(path).Length > 4096)
            throw new InvalidOperationException("Ungültige lokale Bridge-Konfiguration.");
        var config = JsonSerializer.Deserialize<NativeConfiguration>(File.ReadAllText(path), NativeJson.Options)
            ?? throw new InvalidOperationException("Bridge-Konfiguration fehlt.");
        config.Validate(); return config;
    }
    public void Validate()
    {
        if (Port is < 1024 or > 65535 || Token is null || Token.Length != 64 || Token.Any(c => !Uri.IsHexDigit(c)))
            throw new InvalidOperationException("Ungültige lokale Bridge-Konfiguration.");
    }
}
public sealed record BridgeEnvelope<T>(int SchemaVersion, string SessionId, DateTimeOffset ObservedAtUtc, string BridgeVersion, T Data);
public sealed record Position(int X, int Y, int Z);
public sealed record NativeResource(string Id, bool Available, int? AvailableStock, int? AllStock, int? StockpiledStock,
    int? BufferedOutputStock, int? InputOutputCapacity, int? TotalCapacity);
public sealed record Housing(int OccupiedBeds, int FreeBeds, int Homeless, int TotalBeds);
public sealed record NativePopulation(int Adults, int Children, int Bots);
public sealed record WorkerGroup(int Employable, int Unemployable, int Unemployed, int OccupiedWorkslots, int FreeWorkslots);
public sealed record Workforce(WorkerGroup Beavers, WorkerGroup Bots);
public sealed record NativeObject(Guid Id, string ObjectName, Position Position, string Orientation, string FlipMode,
    bool Finished, bool Unfinished, Position? Entrance);
public sealed record NativeSnapshot(string Scope, NativeResource[] Resources, Housing Housing, NativePopulation Population,
    Workforce Workforce, Position MapSize, int ObjectCount, NativeObject[] ObjectSample, bool ObjectsTruncated, string[] Limitations);
public sealed record MapCell(int X, int Y, int Z, bool Underground, bool OnGround, int TerrainHeight,
    float WaterDepth, float Contamination, bool Underwater);
public sealed record NativeMap(Position Origin, int Width, int Height, int Depth, MapCell[] Cells, string[] Limitations);
public sealed record NativeStatus(string Connection, string BridgeVersion, bool WritesEnabled);
public sealed record BuildingPosition(Guid Id, string Template, Position Position, string Orientation, bool Finished,
    Position? Entrance, Position[] OccupiedCells, bool CellsTruncated);
public sealed record NativeObjects(string Scope, int Offset, int Limit, int Total, BuildingPosition[] Items, bool HasMore, string[] Limitations);
public sealed record NativeCost(string Id, int Required, int AvailableGlobally);
public sealed record CatalogEntry(string Template, bool Available, bool FactionCompatible, bool? Unlocked, Position? Size,
    Position? Entrance, NativeCost[] Costs);
public sealed record NativeCatalog(string Faction, CatalogEntry[] Items, string[] Limitations);
public sealed record SiteCell(Position Position, bool InsideMap, bool Underground, bool OnGround, bool IntersectsObject, string SupportRule);
public sealed record NativeSite(string Template, Position Origin, int Rotation, string Assessment, bool GameValidated,
    string[] Reasons, SiteCell[] Cells, Position? Entrance, bool? PathAtEntrance, NativeCost[] Costs, string[] Limitations);
public sealed record NativeValidation(string Template, Position Origin, int Rotation, bool GameValidated, bool? Valid,
    bool NoPersistentChangeObserved, bool SessionLocked, int AttemptsRemaining, string[] Limitations);
public sealed record NativePlacement(string Template, Position Origin, int Rotation, Guid EntityId,
    string Outcome, bool? Finished, bool SessionLocked, string[] Limitations);
public sealed record NativeGoodAmount(string Id, int Amount);
public sealed record NativeConstructionMaterials(NativeGoodAmount[] BuildingCosts, bool InventoryAvailable, NativeGoodAmount[]? SiteStock);
public sealed record NativeConstruction(bool WasStarted, bool IsOn, bool ReadyToBuild, float MaterialProgress,
    float BuildTimeProgress, float BuildTimeProgressInHours, bool HasMaterialsToResumeBuilding, bool ReadyToFinish,
    NativeConstructionMaterials Materials);
public sealed record NativeDistrict(bool ComponentPresent, Guid? AssignedDistrictId, Guid? InstantDistrictId, Guid? ConstructionDistrictId);
public sealed record NativeBuildingDetails(string Template, Position Position, bool Finished, bool Unfinished,
    bool ConstructionComponentPresent, NativeConstruction? Construction, NativeDistrict District, NativeOperations? Operations = null);
public sealed record NativePause(bool Paused, bool CanPause);
public sealed record NativeWorkplace(int DesiredWorkers, int AssignedWorkers, int MaxWorkers,
    bool Understaffed, bool Overstaffed, bool AnyWorkerHasJobRunning);
public sealed record NativeOperations(bool PauseComponentPresent, NativePause? Pause,
    bool WorkplaceComponentPresent, NativeWorkplace? Workplace);
public sealed record NativeBuilding(Guid Id, bool Found, NativeBuildingDetails? Details, string[] Limitations);
public sealed record NativeMeta(string Backend, bool Simulated, string? SessionId, DateTimeOffset? ObservedAtUtc);
public sealed record NativeFault(string Code, string Message, bool Retryable);
public sealed record NativeResult<T>(int SchemaVersion, string Status, T? Data, NativeMeta Meta, NativeFault? Error);
public sealed record NativeSimulation(float CurrentSpeed, int DayNumber, float DayProgress, float HoursPassedToday);
public sealed record NativeSpeedResult(int RequestedSpeed, float PreviousSpeed, bool MatchedImmediately,
    NativeSimulation Observation, string[] Limitations);
public sealed record NativeWorkplaceAssignment(Guid Id, string Template, Position Position);
public sealed record NativeWorker(Guid Id, string WorkerType, bool Employed, bool JobRunning,
    string AssignmentStatus, NativeWorkplaceAssignment? Workplace);
public sealed record NativeWorkforce(string Scope, int Offset, int Limit, int Total, int Employed, int Unemployed,
    NativeWorker[] Items, bool HasMore, string[] Limitations);
public sealed record NativeStaffingResult(Guid Id, string Template, int PreviousDesiredWorkers,
    int RequestedDesiredWorkers, int ObservedDesiredWorkers, int AssignedWorkers, int MaxWorkers,
    string Outcome, string[] Limitations);
public sealed record NativePriority(Guid Id, string Kind, string PreviousPriority, string Priority, string Outcome, string[] Limitations);
public sealed record NativeConstructionEntry(NativeBuilding Building, string? Priority);
public sealed record NativeConstructionList(int Offset, int Limit, int Total, NativeConstructionEntry[] Items, bool HasMore, string[] Limitations);
public sealed record NativeAreaKind(string Kind, bool CanRead, bool CanMark, bool CanRemove, string Reason);
public sealed record NativePlantOption(string Resource, string Kind, string ResourceGroup, bool Unlocked);
public sealed record NativeAreaTypes(NativeAreaKind[] Kinds, NativePlantOption[] Plants, string[] Limitations);
public sealed record NativeAreaCell(Position Position, string Resource);
public sealed record NativeAreas(string Kind, bool Supported, int Offset, int Limit, int? Total, NativeAreaCell[] Items, bool HasMore, string[] Limitations);
public sealed record NativeAreaChange(string Kind, string Operation, string Outcome, NativeAreaCell[] Items, string[] Limitations);
