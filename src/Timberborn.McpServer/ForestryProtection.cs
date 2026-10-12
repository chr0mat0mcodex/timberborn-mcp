using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record ForestryProtectionRequest(string Session, string WorkBuildingId, string Template,
    string ActionId, int MaxTrees = 256);
public sealed record ForestryProtectionReport(string ActionId, string Fingerprint, string State,
    int Selected, int Confirmed, ForestryTree? Unconfirmed, string Reason, bool ReadbackConfirmed = false, int NativeReads = 0);

public static class ForestryProtection
{
    public static async Task<JsonObject> Execute(JsonElement args, IForestryPort port, string directory, CancellationToken ct)
    {
        var r = BuildBatchState.Parse<ForestryProtectionRequest>(args);
        var region = new ForestryRequest(r.Session, r.WorkBuildingId, FullRange: true);
        ForestrySurvey.Validate(region); BuildBatchState.Id(r.ActionId);
        if (!BuildingPolicy.ValidTemplate(r.Template) || r.MaxTrees is < 1 or > 512) throw new ArgumentException("invalid_forestry_protection");
        string fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(r, NativeJson.Options)));
        Directory.CreateDirectory(directory);
        using var lease = new FileStream(Path.Combine(directory, "dispatch.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        string path = Path.Combine(directory, r.Session + "-protect-" + r.ActionId + ".json");
        if (File.Exists(path)) {
            if (new FileInfo(path).Length > 65536) throw new InvalidDataException("forestry_journal_too_large");
            var old = JsonSerializer.Deserialize<ForestryProtectionReport>(File.ReadAllText(path), NativeJson.Options) ?? throw new InvalidDataException();
            if (old.ActionId != r.ActionId || old.Fingerprint != fingerprint) throw new ArgumentException("forestry_parameters_changed");
            return ForestryTools.Wrap(old, r.Session, null);
        }
        var before = await ForestrySurvey.Observe(region, port, ct);
        if (!before.KnownTemplates.Contains(r.Template)) throw new BridgeRejectionException("unknown_tree_template");
        var selected = before.ReachableTrees.Where(t => t.Template == r.Template && !t.Marked &&
                t.Vegetation is { IsTree: true, LifeState: "alive", CutYieldRemoved: false } && before.MarkedCells.Contains(t.Position))
            .OrderBy(t => t.Position.Z).ThenBy(t => t.Position.Y).ThenBy(t => t.Position.X).ToArray();
        if (selected.Length > r.MaxTrees) throw new BridgeRejectionException("forestry_budget_exceeded");
        if (selected.Select(t => t.Position).Distinct().Count() != selected.Length ||
            selected.Any(t => before.ReachableTrees.Any(other => other.Position == t.Position && other.Id != t.Id)))
            throw new InvalidDataException("overlapping_tree_origins");
        var receipt = new ForestryProtectionReport(r.ActionId, fingerprint, "protecting", selected.Length, 0, null, "",
            NativeReads: before.NativeReads);
        void Save() {
            string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try {
                using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                    JsonSerializer.Serialize(stream, receipt, NativeJson.Options); stream.Flush(true);
                }
                // Match BuildBatchStore: lease serializes writers; preserve the
                // committed receipt until replacement succeeds. No action retry.
                if (File.Exists(path)) File.Replace(temp, path, null);
                else File.Move(temp, path);
            } finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        Save();
        string stage = "precondition";
        try {
            foreach (var tree in selected) {
                stage = "precondition";
                receipt = receipt with { NativeReads = receipt.NativeReads + 1 };
                var sim = await port.Simulation(ct);
                if (sim.SessionId != r.Session || sim.Data.CurrentSpeed != 0) throw new BridgeRejectionException("state_conflict");
                // Recheck identity/life immediately before removing only this cell's cutting designation.
                var cell = region with { X = tree.Position.X, Y = tree.Position.Y, Z = tree.Position.Z, Width = 1, Height = 1 };
                receipt = receipt with { NativeReads = receipt.NativeReads + 1 };
                var fresh = await port.Trees(cell, cell.Z, 0, ct);
                var data = fresh.Data;
                if (fresh.SessionId != r.Session || data.Kind != "all" || data.Offset != 0 || data.Limit != 32 ||
                    data.HasMore || data.Total != data.Items.Length || !data.Items.Any(t => t.Id == tree.Id &&
                        t.Template == r.Template && t.Position == tree.Position && !t.Marked &&
                        t.Vegetation is { IsTree: true, LifeState: "alive", CutYieldRemoved: false }))
                    throw new BridgeRejectionException("forestry_tree_changed");
                receipt = receipt with { State = "unconfirmed", Unconfirmed = new(tree.Id, tree.Template, tree.Position), Reason = "protection_acknowledgement_pending_no_retry" };
                stage = "save_pending";
                Save();
                stage = "protect_acknowledgement";
                var e = await port.Protect(region, tree.Position, ct);
                if (e.SessionId != r.Session || e.Data.Kind != "tree_cutting" || e.Data.Operation != "remove" ||
                    e.Data.Outcome != "applied" || e.Data.Items.Length != 1 || e.Data.Items[0].Position != tree.Position ||
                    e.Data.Items[0].Resource != "unmarked") throw new InvalidDataException("forestry_protection_unconfirmed");
                receipt = receipt with { State = "protecting", Confirmed = receipt.Confirmed + 1, Unconfirmed = null, Reason = "" };
                stage = "save_confirmed";
                Save();
            }
            // Confirm all changed cells and detect intervening changes elsewhere; never silently undo human input.
            stage = "readback";
            var after = await ForestrySurvey.ReadMarkings(r.Session, port, ct);
            receipt = receipt with { NativeReads = receipt.NativeReads + after.NativeReads };
            var expected = new HashSet<Position>(before.MarkedCells);
            expected.ExceptWith(selected.Select(t => t.Position));
            bool matches = expected.SetEquals(after.MarkedCells);
            receipt = receipt with { State = matches ? "completed" : "stopped", ReadbackConfirmed = matches,
                Reason = matches ? "cutting_removed_other_markings_unchanged_not_persistent_protection" : "cutting_changed_during_protection_read_again" };
            stage = "save_readback";
            Save();
        } catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or HttpRequestException or OperationCanceledException or JsonException) {
            if (receipt.Unconfirmed is null) receipt = receipt with { State = "stopped",
                // No exception message: it can contain private paths or transport data.
                Reason = ex is BridgeRejectionException rejection ? rejection.Code :
                    $"{stage}_{FailureKind(ex)}_{ex.HResult:X8}_no_retry" };
            Save();
        }
        return ForestryTools.Wrap(receipt, r.Session, null);
    }

    private static string FailureKind(Exception ex) => ex switch {
        InvalidDataException => "invalid_data",
        IOException => "io",
        OperationCanceledException => "cancelled",
        HttpRequestException => "transport",
        JsonException => "json",
        _ => "argument"
    };
}
