using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record BatchBuilding(string Template, string? InitialStorageGood = null, string? InitialStorageMode = null);
public sealed record BatchRegion(int X, int Y, int Z, int Width, int Height);
public sealed record BuildBatchSpec(string Session, string BatchId, string DistrictId, int X, int Y, int Z,
    int Width, int Height, int[] Rotations, BatchBuilding[] Items, string Clearing, int MaxClearTargets,
    int ClearanceHours, int ConstructionHours, int Speed, int MaxRealSeconds,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] BatchRegion[]? AdditionalRegions = null);
public sealed record BatchStartRequest(BuildBatchSpec Batch, int WaitSeconds);
public sealed record BatchHandle(string Session, string BatchId, int WaitSeconds = 0);
public sealed record BatchSelection(int Rotation, int OptionIndex, string PlanKey, Position Origin, int NewPaths);
public sealed record BatchSearch(int Rotation, int Checked, int Rejected, string StopReason);
public sealed record BatchFinished(int Index, string ActionId, string Template, Position Origin, string Proof, int RegionIndex = 0);
public sealed record BatchRegionExit(int RegionIndex, string Reason, bool ClearanceUsed, int Targets);

public sealed class BuildBatchJob
{
    public int SchemaVersion { get; set; } = 1;
    public required BuildBatchSpec Spec { get; init; }
    public required string Fingerprint { get; init; }
    public string State { get; set; } = "planning";
    public string Reason { get; set; } = "";
    public int Index { get; set; }
    public int RegionIndex { get; set; }
    public List<BatchRegionExit> PreviousRegions { get; set; } = [];
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
    public string ClearanceRunId => BuildBatchState.DerivedId(Spec, RegionIndex == 0 ? "clearance" : "clearance:region:" + RegionIndex);
    [JsonIgnore] public BatchRegion Region => BuildBatchState.Regions(Spec)[RegionIndex];
    [JsonIgnore] public BuildBatchSpec ActiveSpec => Spec with {
        X = Region.X, Y = Region.Y, Z = Region.Z, Width = Region.Width, Height = Region.Height };
    [JsonIgnore] public int PreviousClearTargets => PreviousRegions.Sum(r => r.Targets);
}

public static class BuildBatchState
{
    public static BatchRegion[] Regions(BuildBatchSpec s) => [new(s.X, s.Y, s.Z, s.Width, s.Height), ..(s.AdditionalRegions ?? [])];
    private static bool ValidRegion(BatchRegion? r) => r is not null &&
        r.X is >= 0 and <= 4095 && r.Y is >= 0 and <= 4095 && r.Z is >= 0 and <= 4095 &&
        r.Width is >= 1 and <= 8 && r.Height is >= 1 and <= 8 && r.X + r.Width <= 4096 && r.Y + r.Height <= 4096;
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
        if (s.AdditionalRegions is { Length: > 3 } || Regions(s).Any(r => !ValidRegion(r)) ||
            Regions(s).Distinct().Count() != Regions(s).Length) throw new ArgumentException("invalid_batch_regions");
        if (s.X is < 0 or > 4095 || s.Y is < 0 or > 4095 || s.Z is < 0 or > 4095 ||
            s.Width is < 1 or > 8 || s.Height is < 1 or > 8 || s.X + s.Width > 4096 || s.Y + s.Height > 4096 ||
            s.Rotations is not { Length: >= 1 and <= 4 } || s.Rotations.Any(r => r is < 0 or > 3) || s.Rotations.Distinct().Count() != s.Rotations.Length ||
            s.Items is not { Length: >= 1 and <= 8 } || s.Clearing is not ("none" or "dead_vegetation" or "all_vegetation") ||
            s.MaxClearTargets is < 0 or > 64 || s.Clearing != "none" && s.MaxClearTargets == 0 ||
            s.ClearanceHours is < 1 or > 168 || s.ConstructionHours is < 1 or > 168 ||
            s.ClearanceHours * Regions(s).Length + s.ConstructionHours * s.Items.Length > 672 || s.Speed is not (1 or 3 or 7) || s.MaxRealSeconds is < 30 or > 3600)
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
        if (job.RegionIndex < 0 || job.RegionIndex >= BuildBatchState.Regions(job.Spec).Length ||
            job.PreviousRegions is null || job.PreviousRegions.Count != job.RegionIndex ||
            job.PreviousRegions.Where((r, i) => r is null || r.RegionIndex != i || r.Targets < 0 || r.Targets > job.Spec.MaxClearTargets ||
                !r.ClearanceUsed && r.Targets != 0 || r.Reason is not ("no_candidate_in_authorized_region" or "no_removable_vegetation")).Any() ||
            job.PreviousClearTargets > job.Spec.MaxClearTargets)
            throw new InvalidDataException("invalid_batch_regions_checkpoint");
        var active = job.ActiveSpec;
        if (job.SchemaVersion != 1 || job.Spec.Session != session || job.Spec.BatchId != id || job.Fingerprint != BuildBatchState.Fingerprint(job.Spec) ||
            job.Index < 0 || job.Index > job.Spec.Items.Length || job.Finished is null || job.Finished.Count != job.Index ||
            job.Targets is null || job.Targets.Length + job.PreviousClearTargets > job.Spec.MaxClearTargets || job.RemovalCursor < 0 || job.RemovalCursor > job.Targets.Length ||
            job.State is not ("planning" or "discovering" or "marking" or "pending_mark" or "verify_clearance" or "pending_clearance_run" or "clearance_wait" or "pending_build" or "building" or "pending_build_run" or "completed" or "stopped" or "cancelled"))
            throw new InvalidDataException("invalid_batch_journal");
        if (job.Search is null || job.State == "pending_mark" && job.RemovalCursor >= job.Targets.Length ||
            job.State == "completed" && job.Index != job.Spec.Items.Length ||
            job.State is "pending_build" or "pending_build_run" or "building" && (job.Selection is null || job.Index >= job.Spec.Items.Length) ||
            job.Targets.Select(t => t.Id).Distinct().Count() != job.Targets.Length ||
            job.Targets.Any(t => t.Kind != "vegetation" || !t.CanDelete || t.Mode != "demolition_mark" ||
                t.Position.X < active.X || t.Position.X >= active.X + active.Width ||
                t.Position.Y < active.Y || t.Position.Y >= active.Y + active.Height || t.Position.Z != active.Z ||
                job.Spec.Clearing == "none" || job.Spec.Clearing == "dead_vegetation" && t.Vegetation?.LifeState != "dead"))
            throw new InvalidDataException("invalid_batch_checkpoint");
        if (job.Selection is { } selection && (!job.Spec.Rotations.Contains(selection.Rotation) || selection.OptionIndex < 0 ||
            string.IsNullOrEmpty(selection.PlanKey) || selection.Origin is null ||
            selection.Origin.X < active.X || selection.Origin.X >= active.X + active.Width ||
            selection.Origin.Y < active.Y || selection.Origin.Y >= active.Y + active.Height || selection.Origin.Z != active.Z))
            throw new InvalidDataException("invalid_batch_selection");
        foreach (var finished in job.Finished) {
            if (finished is null || finished.RegionIndex < 0 || finished.RegionIndex > job.RegionIndex || finished.Origin is null)
                throw new InvalidDataException("invalid_batch_finished_region");
            var region = BuildBatchState.Regions(job.Spec)[finished.RegionIndex];
            if (finished.Origin.X < region.X || finished.Origin.X >= region.X + region.Width ||
                finished.Origin.Y < region.Y || finished.Origin.Y >= region.Y + region.Height || finished.Origin.Z != region.Z)
                throw new InvalidDataException("invalid_batch_finished_region");
        }
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
