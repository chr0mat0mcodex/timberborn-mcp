using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record ForestryRequest(string Session, string WorkBuildingId, int X, int Y, int Z,
    int Width, int Height, int Depth = 1, string[]? ConsumerIds = null);
public sealed record ForestryTree(Guid Id, string Template, Position Position);
public sealed record ForestryConsumer(string Id, string Template, bool Finished, bool? Paused,
    OperationWorkplace? Workplace, int? StoredLogs, string[] Statuses);
public sealed record ForestryReport(ForestryRequest Region, ForestryTree[] Candidates,
    int TotalCandidates, bool CandidatesTruncated, Dictionary<string, int> Excluded, bool RangeSupported, int NativeReads,
    DateTimeOffset ObservedAtUtc, int AvailableLogs, int AllLogs, ForestryConsumer[] Consumers, string[] Limitations);

public interface IForestryPort
{
    Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct);
    Task<BridgeEnvelope<NativeAreaTypes>> Types(CancellationToken ct);
    Task<BridgeEnvelope<NativeRange>> Range(ForestryRequest r, int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeAreas>> Cutting(int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeRemovalTargets>> Trees(ForestryRequest r, int z, int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeGoods>> Goods(int offset, CancellationToken ct);
    Task<BridgeEnvelope<NativeOperation>> Operation(string session, string id, CancellationToken ct);
    Task<BridgeEnvelope<NativeAreaChange>> Mark(ForestryRequest r, Position p, CancellationToken ct);
}

public static class ForestrySurvey
{
    public static void Validate(ForestryRequest r)
    {
        BuildBatchState.Id(r.Session); BuildBatchState.Id(r.WorkBuildingId);
        if (r.ConsumerIds is { } ids) {
            if (ids.Length > 8 || ids.Distinct().Count() != ids.Length) throw new ArgumentException("invalid_consumer_ids");
            foreach (var id in ids) BuildBatchState.Id(id);
        }
        if (r.X < 0 || r.Y < 0 || r.Z < 0 || r.Width is < 1 or > 8 || r.Height is < 1 or > 8 ||
            r.Depth is < 1 or > 4 || (long)r.X + r.Width > 4096 || (long)r.Y + r.Height > 4096 ||
            (long)r.Z + r.Depth > 4096) throw new ArgumentException("forestry_region_out_of_bounds");
    }

    public static string? Exclusion(NativeRemovalTarget t, ISet<string> templates, bool supported,
        ISet<Position> range, ISet<Position> marked)
    {
        if (t.Kind is not ("vegetation" or "planted") || !templates.Contains(t.Template)) return "not_tree";
        // A mature Growable survives on leftovers. Read the actual cutting yield, not the tree model/life flags.
        var vegetation = t.Vegetation;
        if (vegetation?.CutYieldRemoved == true) return "harvested_leftover";
        if (vegetation is null || vegetation.CutYieldRemoved is null || vegetation.CutIsYielding is null ||
            vegetation.CutYieldGood is null || vegetation.CutYieldAmount is null) return "cut_yield_unknown";
        if (vegetation.CutYieldGood != "Log" || vegetation.CutYieldAmount <= 0) return "no_log_yield";
        if (vegetation.CutIsYielding != true) return "not_currently_cuttable";
        if (!supported) return "range_unknown";
        if (!range.Contains(t.Position)) return "outside_range";
        if (t.Marked) return "removal_pending";
        if (marked.Contains(t.Position)) return "already_marked";
        if (vegetation.LifeState != "alive") return "not_observed_alive";
        if (vegetation.IsGrown != true) return "not_observed_mature";
        return null;
    }

    public static async Task<ForestryReport> Observe(ForestryRequest r, IForestryPort port, CancellationToken ct)
    {
        Validate(r);
        int reads = 0; string? version = null; DateTimeOffset observed = default;
        T Check<T>(BridgeEnvelope<T> e) {
            ct.ThrowIfCancellationRequested(); reads++;
            if (e.SessionId != r.Session || version is not null && e.BridgeVersion != version)
                throw new BridgeRejectionException("stale_session");
            version = e.BridgeVersion; observed = e.ObservedAtUtc; return e.Data;
        }
        static void Page(int offset, int actualOffset, int limit, int total, int count, bool more, int? previous, int bound) {
            if (actualOffset != offset || limit != 32 || total < 0 || total > bound || previous is not null && previous != total ||
                count != Math.Min(32, Math.Max(0, total - offset)) || more != (offset + count < total))
                throw new InvalidDataException("forestry_incomplete_page");
        }
        if (Check(await port.Simulation(ct)).CurrentSpeed != 0) throw new BridgeRejectionException("state_conflict");
        var types = Check(await port.Types(ct));
        var templates = types.Plants.Where(p => p.Kind == "tree_planting").Select(p => p.Resource).ToHashSet(StringComparer.Ordinal);
        var range = new HashSet<Position>(); int? total = null; bool? supported = null; string? source = null;
        for (int offset = 0; ; offset += 32) {
            if (offset >= 2048) throw new InvalidDataException("forestry_range_bound");
            var p = Check(await port.Range(r, offset, ct));
            Page(offset, p.Offset, p.Limit, p.Total, p.Items.Length, p.HasMore, total, 2048);
            if (p.Id != r.WorkBuildingId || supported is not null && (supported != p.Supported || source != p.Source) ||
                !p.Supported && p.Total != 0) throw new InvalidDataException("forestry_range_changed");
            total = p.Total; supported = p.Supported; source = p.Source;
            foreach (var cell in p.Items) if (!range.Add(cell)) throw new InvalidDataException("forestry_duplicate_range");
            if (!p.HasMore) break;
        }
        var marked = new HashSet<Position>(); total = null;
        for (int offset = 0; ; offset += 32) {
            if (offset >= 4096) throw new InvalidDataException("forestry_marking_bound");
            var p = Check(await port.Cutting(offset, ct));
            if (!p.Supported || p.Kind != "tree_cutting" || p.Total is null) throw new InvalidDataException("forestry_marking_unknown");
            Page(offset, p.Offset, p.Limit, p.Total.Value, p.Items.Length, p.HasMore, total, 4096); total = p.Total;
            foreach (var cell in p.Items) if (cell.Resource != "marked" || !marked.Add(cell.Position))
                throw new InvalidDataException("forestry_marking_changed");
            if (!p.HasMore) break;
        }
        var trees = new Dictionary<Guid, NativeRemovalTarget>();
        for (int z = r.Z; z < r.Z + r.Depth; z++) {
            total = null; var seen = new HashSet<Guid>();
            for (int offset = 0; ; offset += 32) {
                if (offset >= 256) throw new InvalidDataException("forestry_tree_bound");
                var p = Check(await port.Trees(r, z, offset, ct));
                Page(offset, p.Offset, p.Limit, p.Total, p.Items.Length, p.HasMore, total, 256); total = p.Total;
                if (p.Kind != "all") throw new InvalidDataException("forestry_target_kind");
                foreach (var t in p.Items) {
                    if (!seen.Add(t.Id)) throw new InvalidDataException("forestry_duplicate_tree");
                    // Queries report overlapping objects. Only origins inside the requested volume are candidates.
                    if (t.Position.X < r.X || t.Position.X >= r.X + r.Width || t.Position.Y < r.Y ||
                        t.Position.Y >= r.Y + r.Height || t.Position.Z != z) continue;
                    trees.Add(t.Id, t);
                }
                if (!p.HasMore) break;
            }
        }
        GoodStock? logs = null; total = null; var goods = new HashSet<string>(StringComparer.Ordinal);
        for (int offset = 0; ; offset += 32) {
            if (offset >= 512) throw new InvalidDataException("forestry_goods_bound");
            var p = Check(await port.Goods(offset, ct));
            Page(offset, p.Offset, p.Limit, p.Total, p.Items.Length, p.HasMore, total, 512); total = p.Total;
            foreach (var g in p.Items) { if (!goods.Add(g.Id)) throw new InvalidDataException("forestry_duplicate_good"); if (g.Id == "Log") logs = g; }
            if (!p.HasMore) break;
        }
        if (logs is null) throw new InvalidDataException("forestry_logs_unknown");
        var consumers = new List<ForestryConsumer>();
        foreach (var id in r.ConsumerIds ?? []) {
            var o = Check(await port.Operation(r.Session, id, ct));
            if (o.Id != id) throw new InvalidDataException("forestry_consumer_mismatch");
            consumers.Add(new(id, o.Template, o.Finished, o.Paused, o.Workplace,
                o.Inventories is null ? null : o.Inventories.Sum(i => i.Goods.Where(g => g.Id == "Log").Sum(g => g.Stock)), o.Statuses));
        }
        if (Check(await port.Simulation(ct)).CurrentSpeed != 0) throw new BridgeRejectionException("state_conflict");
        var candidates = new List<ForestryTree>(); var excluded = new Dictionary<string, int>();
        foreach (var t in trees.Values.OrderBy(t => t.Position.Z).ThenBy(t => t.Position.Y).ThenBy(t => t.Position.X)) {
            var reason = Exclusion(t, templates, supported == true, range, marked);
            if (reason is null) candidates.Add(new(t.Id, t.Template, t.Position));
            else excluded[reason] = excluded.GetValueOrDefault(reason) + 1;
        }
        return new(r, candidates.Take(16).ToArray(), candidates.Count, candidates.Count > 16,
            excluded, supported == true, reads, observed, logs.AvailableStock, logs.AllStock, consumers.ToArray(),
            ["non_atomic_paused_observation", "native_work_range_not_worker_or_delivery_proof",
             "tree_catalog_membership_not_guaranteed_log_yield", "stocks_not_additive_no_rate_or_sustainable_yield_claim"]);
    }
}
