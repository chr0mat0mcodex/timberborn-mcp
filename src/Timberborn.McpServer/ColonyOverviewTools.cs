using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record OverviewNeed(string Id, int Observed, int Enabled, int Active, int Warning, int Critical, int Unfavorable);
public sealed record OverviewNeeds(int Beavers, int ObservedBeavers, int MissingNeedManagers,
    int DeadExcluded, int UnknownLifeStateExcluded, int Total, int ReadCount, bool Complete,
    int? NextOffset, bool CountsStable, string[] DisabledIds, OverviewNeed[] Items);
public sealed record ColonyOverview(NativePopulation Population, Housing Housing,
    NativeResource[] Resources, Workforce Workforce, OverviewNeeds Needs,
    DateTimeOffset ObservationStartedAtUtc, DateTimeOffset ObservationEndedAtUtc, bool Atomic, string[] Limitations);

public static class ColonyOverviewTools
{
    public const string Name = "inspect_colony_overview";
    public const int MaxPages = 4;
    public static Tool Reader()
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        return new Tool {
            Name = Name,
            Description = "Gebündelte lesende Übersicht: Bevölkerung, Betten, drei Beispielgüter (kein Gesamtnahrungsbestand), Personal und alle Bedürfniszähler bis 128 Einträge. Ersetzt inspect_colony plus Bedürfnis-Seiten, ohne Objektstichprobe/Punktestatistik. complete, nextOffset und countsStable beachten. Nur dieselbe Spielsitzung; keine atomare Beobachtung. warning allein ist keine akute Notlage; critical, active und unfavorable mitlesen. Kein Gesamt-Wohlbefindenswert und kein Zufriedenheitsnachweis. Details über inspect_needs/inspect_goods.",
            InputSchema = JsonSerializer.SerializeToElement(new { type = "object", properties = new { }, additionalProperties = false }),
            OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<ColonyOverview>))),
            Annotations = new() { ReadOnlyHint = true, DestructiveHint = false, IdempotentHint = true, OpenWorldHint = false }
        };
    }

    public static async Task<JsonObject> Invoke(NativeClient client, JsonElement args, CancellationToken ct)
    {
        if (args.EnumerateObject().Any()) throw new ArgumentException();
        var result = await Observe(client.Snapshot, (offset, token) => client.Needs(
            DiagnosticsRequest.Parse("/agent-api/v1/needs", new NameValueCollection {
                ["offset"] = offset.ToString(System.Globalization.CultureInfo.InvariantCulture), ["limit"] = "32" }), token), ct);
        return (JsonObject)JsonSerializer.SerializeToNode(new NativeResult<ColonyOverview>(1, "ok", result.Data,
            new("native", false, result.SessionId, result.ObservedAtUtc), null), NativeJson.Options)!;
    }

    // Bounded read aggregation, no automatic retries. Each native response was validated
    // by NativeClient; a session/catalog change must never become a mixed overview.
    public static async Task<BridgeEnvelope<ColonyOverview>> Observe(
        Func<CancellationToken, Task<BridgeEnvelope<NativeSnapshot>>> readSnapshot,
        Func<int, CancellationToken, Task<BridgeEnvelope<NativeNeeds>>> readNeeds, CancellationToken ct)
    {
        var snapshot = await readSnapshot(ct);
        var pages = new List<NativeNeeds>();
        var items = new List<NeedSummary>();
        var ended = snapshot.ObservedAtUtc;
        for (int page = 0; page < MaxPages; page++) {
            ct.ThrowIfCancellationRequested();
            var envelope = await readNeeds(page * 32, ct);
            var data = envelope.Data;
            if (envelope.SessionId != snapshot.SessionId || envelope.BridgeVersion != snapshot.BridgeVersion ||
                data.Offset != items.Count || data.Limit != 32 ||
                pages.Count > 0 && data.Total != pages[0].Total)
                throw new BridgeRejectionException("state_conflict");
            pages.Add(data); items.AddRange(data.Items); ended = envelope.ObservedAtUtc;
            if (!data.HasMore) break;
        }
        if (!items.Select(n => n.Id).SequenceEqual(items.Select(n => n.Id).Distinct().Order(StringComparer.Ordinal)))
            throw new BridgeRejectionException("state_conflict");
        var first = pages[0];
        bool stable = snapshot.Data.Population.Adults + snapshot.Data.Population.Children == first.Beavers &&
            pages.All(p => p.Beavers == first.Beavers && p.ObservedBeavers == first.ObservedBeavers &&
                p.MissingNeedManagers == first.MissingNeedManagers && p.DeadExcluded == first.DeadExcluded &&
                p.UnknownLifeStateExcluded == first.UnknownLifeStateExcluded);
        bool complete = !pages[^1].HasMore && items.Count == first.Total;
        var needs = new OverviewNeeds(first.Beavers, first.ObservedBeavers, first.MissingNeedManagers,
            first.DeadExcluded, first.UnknownLifeStateExcluded, first.Total, items.Count, complete,
            complete ? null : items.Count, stable, items.Where(n => n.Enabled == 0).Select(n => n.Id).ToArray(),
            items.Where(n => n.Enabled > 0).Select(n => new OverviewNeed(n.Id, n.Observed, n.Enabled, n.Active,
                n.Warning, n.Critical, n.Unfavorable)).ToArray());
        var overview = new ColonyOverview(snapshot.Data.Population, snapshot.Data.Housing,
            snapshot.Data.Resources, snapshot.Data.Workforce, needs, snapshot.ObservedAtUtc, ended, false,
            ["non_atomic_bounded_reads", "three_example_goods_not_total_food", "warning_alone_not_emergency",
             "counts_stable_does_not_mean_atomic", "no_overall_wellbeing_score", "points_and_details_via_inspect_needs"]);
        return new(1, snapshot.SessionId, ended, snapshot.BridgeVersion, overview);
    }
}
