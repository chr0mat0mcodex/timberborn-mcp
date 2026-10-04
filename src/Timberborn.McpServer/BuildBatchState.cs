using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record BatchBuilding(string Template, string? InitialStorageGood = null, string? InitialStorageMode = null);
public sealed record BuildBatchSpec(string Session, string BatchId, string DistrictId, int X, int Y, int Z,
    int Width, int Height, int[] Rotations, BatchBuilding[] Items, string Clearing, int MaxClearTargets,
    int ClearanceHours, int ConstructionHours, int Speed, int MaxRealSeconds);
public sealed record BatchStartRequest(BuildBatchSpec Batch, int WaitSeconds);
public sealed record BatchHandle(string Session, string BatchId, int WaitSeconds = 0);
public sealed record BatchSelection(int Rotation, int OptionIndex, string PlanKey, Position Origin, int NewPaths);
public sealed record BatchSearch(int Rotation, int Checked, int Rejected, string StopReason);
public sealed record BatchFinished(int Index, string ActionId, string Template, Position Origin, string Proof);

public sealed class BuildBatchJob
{
    public int SchemaVersion { get; set; } = 1;
    public required BuildBatchSpec Spec { get; init; }
    public required string Fingerprint { get; init; }
    public string State { get; set; } = "planning";
    public string Reason { get; set; } = "";
    public int Index { get; set; }
    public bool ClearanceUsed { get; set; }
    public NativeRemovalTarget[] Targets { get; set; } = [];
    public int RemovalCursor { get; set; }
    public BatchSelection? Selection { get; set; }
    public BatchSearch[] Search { get; set; } = [];
    public List<BatchFinished> Finished { get; set; } = [];
    public CompletionReport? Completion { get; set; }
    public NativeSimulationRun? ClearanceRun { get; set; }
    public bool DispatchStopped { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public bool Terminal => State is "completed" or "stopped" or "cancelled";
    public string ActionId => BuildBatchState.DerivedId(Spec, "building:" + Index);
    public string ClearanceRunId => BuildBatchState.DerivedId(Spec, "clearance");
}

public static class BuildBatchState
{
    public static readonly JsonSerializerOptions Json = new(NativeJson.Options) {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
    public static string DerivedId(BuildBatchSpec spec, string suffix) => new Guid(SHA256.HashData(
        Encoding.UTF8.GetBytes("timberborn-batch-v1:" + spec.Session + ":" + spec.BatchId + ":" + suffix)).AsSpan(0, 16)).ToString("D");
    public static string Fingerprint(BuildBatchSpec spec) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(spec, NativeJson.Options)));
    public static void Id(string value) {
        if (!Guid.TryParseExact(value, "D", out var id) || id == Guid.Empty || value != id.ToString("D")) throw new ArgumentException("canonical_uuid_required");
    }
    public static void Validate(BuildBatchSpec s, bool settingsEnabled)
    {
        if (s is null) throw new ArgumentException("batch_required");
        Id(s.Session); Id(s.BatchId); Id(s.DistrictId);
        if (s.X is < 0 or > 4095 || s.Y is < 0 or > 4095 || s.Z is < 0 or > 4095 ||
            s.Width is < 1 or > 8 || s.Height is < 1 or > 8 || s.X + s.Width > 4096 || s.Y + s.Height > 4096 ||
            s.Rotations is not { Length: >= 1 and <= 4 } || s.Rotations.Any(r => r is < 0 or > 3) || s.Rotations.Distinct().Count() != s.Rotations.Length ||
            s.Items is not { Length: >= 1 and <= 8 } || s.Clearing is not ("none" or "dead_vegetation" or "all_vegetation") ||
            s.MaxClearTargets is < 0 or > 64 || s.Clearing != "none" && s.MaxClearTargets == 0 ||
            s.ClearanceHours is < 1 or > 168 || s.ConstructionHours is < 1 or > 168 ||
            s.ClearanceHours + s.ConstructionHours * s.Items.Length > 672 || s.Speed is not (1 or 3 or 7) || s.MaxRealSeconds is < 30 or > 3600)
            throw new ArgumentException("invalid_batch_bounds");
        foreach (var item in s.Items) {
            if (item is null || !BuildingProjectPilotPolicy.SupportsTemplate(item.Template) ||
                item.InitialStorageGood is not null && !BuildingPolicy.ValidTemplate(item.InitialStorageGood) ||
                item.InitialStorageMode is not null && !BuildingSettingsRequest.IsStorageMode(item.InitialStorageMode) ||
                !settingsEnabled && (item.InitialStorageGood is not null || item.InitialStorageMode is not null)) throw new ArgumentException("invalid_batch_item");
        }
    }
    public static T Parse<T>(JsonElement args) {
        RejectDuplicates(args);
        return args.Deserialize<T>(Json) ?? throw new ArgumentException();
    }
    private static void RejectDuplicates(JsonElement node) {
        if (node.ValueKind == JsonValueKind.Object) {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in node.EnumerateObject()) { if (!keys.Add(p.Name)) throw new ArgumentException(); RejectDuplicates(p.Value); }
        } else if (node.ValueKind == JsonValueKind.Array) foreach (var item in node.EnumerateArray()) RejectDuplicates(item);
    }
}

// Private runtime journal, never source-controlled. Lock serializes all batches across MCP processes.
public sealed class BuildBatchStore(string directory)
{
    private string PathFor(string session, string id) {
        BuildBatchState.Id(session); BuildBatchState.Id(id);
        return Path.Combine(directory, session + "-" + id + ".json");
    }
    public IDisposable Acquire() {
        Directory.CreateDirectory(directory);
        return new FileStream(Path.Combine(directory, "dispatch.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }
    public BuildBatchJob? Load(string session, string id) {
        var path = PathFor(session, id);
        if (!File.Exists(path)) return null;
        if (new FileInfo(path).Length > 2_000_000) throw new InvalidDataException("batch_journal_too_large");
        var job = JsonSerializer.Deserialize<BuildBatchJob>(File.ReadAllText(path), NativeJson.Options) ?? throw new InvalidDataException();
        BuildBatchState.Validate(job.Spec, true);
        if (job.SchemaVersion != 1 || job.Spec.Session != session || job.Spec.BatchId != id || job.Fingerprint != BuildBatchState.Fingerprint(job.Spec) ||
            job.Index < 0 || job.Index > job.Spec.Items.Length || job.Finished is null || job.Finished.Count != job.Index ||
            job.Targets is null || job.Targets.Length > job.Spec.MaxClearTargets || job.RemovalCursor < 0 || job.RemovalCursor > job.Targets.Length ||
            job.State is not ("planning" or "discovering" or "marking" or "pending_mark" or "verify_clearance" or "pending_clearance_run" or "clearance_wait" or "pending_build" or "building" or "pending_build_run" or "completed" or "stopped" or "cancelled"))
            throw new InvalidDataException("invalid_batch_journal");
        if (job.Search is null || job.State == "pending_mark" && job.RemovalCursor >= job.Targets.Length ||
            job.State == "completed" && job.Index != job.Spec.Items.Length ||
            job.State is "pending_build" or "pending_build_run" or "building" && (job.Selection is null || job.Index >= job.Spec.Items.Length) ||
            job.Targets.Select(t => t.Id).Distinct().Count() != job.Targets.Length ||
            job.Targets.Any(t => t.Kind != "vegetation" || !t.CanDelete || t.Mode != "demolition_mark" ||
                t.Position.X < job.Spec.X || t.Position.X >= job.Spec.X + job.Spec.Width ||
                t.Position.Y < job.Spec.Y || t.Position.Y >= job.Spec.Y + job.Spec.Height || t.Position.Z != job.Spec.Z ||
                job.Spec.Clearing == "none" || job.Spec.Clearing == "dead_vegetation" && t.Vegetation?.LifeState != "dead"))
            throw new InvalidDataException("invalid_batch_checkpoint");
        if (job.Selection is { } selection && (!job.Spec.Rotations.Contains(selection.Rotation) || selection.OptionIndex < 0 ||
            string.IsNullOrEmpty(selection.PlanKey) || selection.Origin is null ||
            selection.Origin.X < job.Spec.X || selection.Origin.X >= job.Spec.X + job.Spec.Width ||
            selection.Origin.Y < job.Spec.Y || selection.Origin.Y >= job.Spec.Y + job.Spec.Height || selection.Origin.Z != job.Spec.Z))
            throw new InvalidDataException("invalid_batch_selection");
        return job;
    }
    public bool HasActive(string session) => Directory.EnumerateFiles(directory, session + "-*.json")
        .Any(path => Load(session, Path.GetFileNameWithoutExtension(path)[(session.Length + 1)..]) is { Terminal: false });
    public void Save(BuildBatchJob job) {
        job.UpdatedAtUtc = DateTimeOffset.UtcNow;
        var path = PathFor(job.Spec.Session, job.Spec.BatchId);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                JsonSerializer.Serialize(stream, job, NativeJson.Options); stream.Flush(true);
            }
            File.Move(temp, path, true);
        } finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
