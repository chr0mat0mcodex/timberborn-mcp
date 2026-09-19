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
public sealed record NativeMeta(string Backend, bool Simulated, string? SessionId, DateTimeOffset? ObservedAtUtc);
public sealed record NativeFault(string Code, string Message, bool Retryable);
public sealed record NativeResult<T>(int SchemaVersion, string Status, T? Data, NativeMeta Meta, NativeFault? Error);
