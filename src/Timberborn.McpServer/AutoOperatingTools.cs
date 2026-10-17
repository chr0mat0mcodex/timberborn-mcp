using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record AutoOperatingSelection(string State, string? DistrictId, string? CenterId,
    string[] ProducerIds, bool SearchComplete, int? NextOffset, int CandidatesChecked,
    bool CandidateLimitReached, string[] UnresolvedBuildingIds, string[] OmittedGoods, string[] UnknownCostTemplates);
public sealed record AutoOperatingReport(string State, string DemandBasis, AutoOperatingSelection? Selection,
    OperatingDemand[] Demands, OperatingReport? Diagnosis, string[] NextSteps, string[] Limitations);

public static class AutoOperatingTools
{
    public const string Name = "inspect_operating_readiness_auto";
    public const int MaxSearchPages = 4, MaxCandidates = 16;
    public static Tool Reader()
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        return new() { Name = Name,
            Description = "Automatische lesende Betriebsdiagnose: Bauzentrum über native Bezirkszuordnung und Produzenten über Rezept-/Erntegraph auswählen. Optional districtId, bis acht buildingIds als Bezirks-/Produktbezug und materials. Ohne Materialvorgabe aktuelle Produkte der Bezugsgebäude oder Log/Plank/Gear mit Nullbedarf. Maximal 128 Gebäude, 16 Produzentenkandidaten; Auswahl/Bezirk/Abdeckung und fehlende Kostendefinitionen explizit. Keine automatische Material-Startsperre; keine Vollständigkeits- oder Produktionsgarantie.",
            InputSchema = JsonSerializer.SerializeToElement(new {
                type = "object", properties = new {
                    session = new { type = "string", format = "uuid" }, districtId = new { type = "string", format = "uuid" },
                    buildingIds = new { type = "array", maxItems = 8, uniqueItems = true, items = new { type = "string", format = "uuid" } },
                    materials = new { type = "array", minItems = 1, maxItems = 8, items = new {
                        type = "object", properties = new { good = new { type = "string" }, amount = new { type = "integer", minimum = 0, maximum = 1000000 } },
                        required = new[] { "good", "amount" }, additionalProperties = false } }
                }, required = new[] { "session" }, additionalProperties = false }),
            OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<AutoOperatingReport>))),
            Annotations = new() { ReadOnlyHint = true, DestructiveHint = false, IdempotentHint = true, OpenWorldHint = false } };
    }

    public static async Task<JsonObject> Invoke(NativeClient client, JsonElement args, CancellationToken ct)
    {
        if (args.ValueKind != JsonValueKind.Object || args.EnumerateObject().Any(p => p.Name is not ("session" or "districtId" or "buildingIds" or "materials")) ||
            args.EnumerateObject().Select(p => p.Name).Distinct().Count() != args.EnumerateObject().Count()) throw new ArgumentException();
        string Id(JsonElement e) => e.ValueKind == JsonValueKind.String && Guid.TryParseExact(e.GetString(), "D", out var id) && id != Guid.Empty ? id.ToString("D") : throw new ArgumentException();
        if (!args.TryGetProperty("session", out var sessionValue)) throw new ArgumentException();
        string session = Id(sessionValue);
        string? district = args.TryGetProperty("districtId", out var dv) ? Id(dv) : null;
        string[] ids = [];
        if (args.TryGetProperty("buildingIds", out var iv)) {
            if (iv.ValueKind != JsonValueKind.Array || iv.GetArrayLength() > 8) throw new ArgumentException();
            ids = iv.EnumerateArray().Select(Id).ToArray();
            if (ids.Distinct().Count() != ids.Length) throw new ArgumentException();
        }
        OperatingDemand[]? demands = null;
        if (args.TryGetProperty("materials", out var mv)) {
            // Reuse the manual reader's strict material contract without inventing a real center.
            var parsed = OperatingReadinessTools.Parse(JsonSerializer.SerializeToElement(new {
                session, constructionCenterId = session, producerIds = Array.Empty<string>(), materials = mv }));
            demands = parsed.Materials;
        }
        var e = await Observe(client, session, district, ids, demands, [], demands is null ? "operating_supply" : "caller_demand", ct);
        return JsonSerializer.SerializeToNode(new NativeResult<AutoOperatingReport>(1, "ok", e.Data,
            new("native", false, e.SessionId, e.ObservedAtUtc), null), NativeJson.Options)!.AsObject();
    }

    public static bool IsCenter(string template) => template.Split('.')[0] is "DistrictCenter" or "BuildersHut";
    public static string? District(NativeBuilding building)
    {
        if (!building.Found || building.Details is not { } d) return null;
        if (d.Template.Split('.')[0] == "DistrictCenter") return building.Id.ToString("D");
        var ids = new[] { d.District.AssignedDistrictId, d.District.InstantDistrictId, d.District.ConstructionDistrictId }
            .Where(id => id is not null).Distinct().ToArray();
        return ids.Length == 1 ? ids[0]!.Value.ToString("D") : null;
    }
    public static (OperatingDemand[] Demands, string[] Unknown) Costs(GraphDefinitions definitions, LargeProjectStep[] steps)
    {
        var amounts = new Dictionary<string, int>(StringComparer.Ordinal); var unknown = new HashSet<string>();
        foreach (var step in steps) {
            var building = definitions.Buildings.SingleOrDefault(b => b.Id == step.Template);
            if (building is null) { unknown.Add(step.Template); continue; }
            foreach (var cost in building.Costs) amounts[cost.Good] = checked(amounts.GetValueOrDefault(cost.Good) + cost.Amount);
        }
        return (amounts.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new OperatingDemand(p.Key, p.Value)).ToArray(), unknown.Order(StringComparer.Ordinal).ToArray());
    }
    public static string[] ProducerTemplates(GraphDefinitions definitions, IEnumerable<string> goods) =>
        definitions.Recipes.Where(r => r.Outputs.Any(o => goods.Contains(o.Good))).SelectMany(r => r.Buildings)
        .Concat(definitions.Sources.Where(s => goods.Contains(s.Good)).SelectMany(s => s.Harvesters)).Distinct().ToArray();

    public static async Task<BridgeEnvelope<AutoOperatingReport>> Observe(NativeClient client, string session, string? district,
        string[] focusIds, OperatingDemand[]? demands, LargeProjectStep[] steps, string basis, CancellationToken ct)
    {
        var graph = await client.ProductionGraph(ct);
        T Check<T>(BridgeEnvelope<T> e) {
            ct.ThrowIfCancellationRequested();
            if (e.SessionId != session || e.BridgeVersion != graph.BridgeVersion) throw new BridgeRejectionException("state_conflict");
            return e.Data;
        }
        var defs = Check(graph).Definitions;
        var details = new Dictionary<string, NativeBuilding>();
        async Task<NativeBuilding> Building(string id) {
            if (!details.TryGetValue(id, out var b)) {
                b = Check(await client.Building(BridgeRequest.Parse("/agent-api/v1/building", new NameValueCollection { ["id"] = id, ["session"] = session }), ct));
                details[id] = b;
            }
            return b;
        }
        var unresolved = new List<string>();
        var focusDistricts = new HashSet<string>();
        foreach (var id in focusIds) {
            var d = District(await Building(id));
            if (d is null) unresolved.Add(id); else focusDistricts.Add(d);
        }
        if (district is null && focusDistricts.Count == 1 && unresolved.Count == 0) district = focusDistricts.Single();
        bool focusAmbiguous = district is null && focusIds.Length > 0;
        string[] unknownCosts = [];
        if (steps.Length > 0) (demands, unknownCosts) = Costs(defs, steps);
        if (demands is null && focusIds.Length > 0) {
            var ops = Check(await client.BuildingOperations(BuildingQueryRequest.Parse("/agent-api/v1/building-operations", new NameValueCollection {
                ["session"] = session, ["ids"] = string.Join(",", focusIds) }), ct));
            var recipes = ops.Items.Select(i => i.Operation?.Manufacturing?.Recipe).Where(id => id is not null).ToArray();
            demands = defs.Recipes.Where(r => recipes.Contains(r.Id)).SelectMany(r => r.Outputs).Select(o => o.Good).Distinct()
                .Order(StringComparer.Ordinal).Select(g => new OperatingDemand(g, 0)).ToArray();
        }
        demands ??= [new("Log", 0), new("Plank", 0), new("Gear", 0)];
        var omitted = demands.Skip(8).Select(d => d.Good).ToArray();
        var checkedDemands = demands.Take(8).ToArray();
        var matches = new List<BuildingMatch>(); int? total = null; bool complete = false;
        for (int page = 0; page < MaxSearchPages; page++) {
            var d = Check(await client.SearchBuildings(BuildingQueryRequest.Parse("/agent-api/v1/building-search", new NameValueCollection {
                ["offset"] = (page * 32).ToString(CultureInfo.InvariantCulture), ["limit"] = "32" }), ct));
            if (total is {} n && n != d.Total) throw new BridgeRejectionException("state_conflict");
            total = d.Total; matches.AddRange(d.Items); complete = !d.HasMore;
            if (complete) break;
        }
        if (matches.Select(b => b.Id).Distinct().Count() != matches.Count) throw new BridgeRejectionException("state_conflict");
        var centers = matches.Where(b => b.Finished && IsCenter(b.Template)).OrderBy(b => b.Id).Take(8).ToArray();
        if (district is null && !focusAmbiguous) {
            var found = new HashSet<string>();
            foreach (var center in centers) {
                string id = center.Id.ToString("D"); var d = District(await Building(id));
                if (d is null) unresolved.Add(id); else found.Add(d);
            }
            if (complete && matches.Count(b => b.Finished && IsCenter(b.Template)) <= 8 && found.Count == 1 && unresolved.Count == 0) district = found.Single();
        }
        string? centerId = null;
        if (district is not null) {
            var actualCenter = await Building(district);
            if (actualCenter is { Found: true, Details: { Finished: true } } && IsCenter(actualCenter.Details.Template) && District(actualCenter) == district)
                centerId = district;
            if (centerId is null) foreach (var center in centers) {
                var id = center.Id.ToString("D");
                if (District(await Building(id)) == district) { centerId = id; break; }
            }
        }
        var templates = ProducerTemplates(defs, checkedDemands.Select(d => d.Good));
        var candidates = matches.Where(b => b.Finished && templates.Contains(b.Template)).OrderByDescending(b => focusIds.Contains(b.Id.ToString("D"))).ThenBy(b => b.Id).ToArray();
        var producers = new List<string>(); int inspected = 0;
        foreach (var candidate in candidates.Take(MaxCandidates)) {
            string id = candidate.Id.ToString("D"); inspected++;
            var d = District(await Building(id));
            if (d is null) unresolved.Add(id);
            if (id != centerId && (district is null || d == district) && producers.Count < 7) producers.Add(id);
        }
        var selection = new AutoOperatingSelection(centerId is null ? "center_unknown" : "selected", district, centerId, producers.ToArray(), complete,
            complete ? null : matches.Count, inspected, candidates.Length > MaxCandidates || candidates.Length > 7,
            unresolved.Distinct().ToArray(), omitted, unknownCosts);
        var selectedIds = (centerId is null ? Array.Empty<string>() : new[] { centerId }).Concat(producers).ToArray();
        OperatingReport? diagnosis = null;
        DateTimeOffset ended = graph.ObservedAtUtc;
        if (selectedIds.Length > 0) {
            var r = new OperatingRequest(session, centerId ?? "", producers.ToArray(), checkedDemands);
            var e = await OperatingReadinessTools.Observe(r,
                async token => { var e = await client.Snapshot(token); Check(e); return e; },
                token => Task.FromResult(graph),
                async token => { var e = await client.BuildingOperations(BuildingQueryRequest.Parse("/agent-api/v1/building-operations", new NameValueCollection {
                    ["session"] = session, ["ids"] = string.Join(",", selectedIds) }), token); Check(e); return e; },
                async (offset, token) => { var e = await client.Goods(EconomyRequest.Parse("/agent-api/v1/goods", new NameValueCollection {
                    ["offset"] = offset.ToString(CultureInfo.InvariantCulture), ["limit"] = "32" }), token); Check(e); return e; },
                async (id, token) => { var e = await client.BuildingAccess(LogisticsRequest.Parse("/agent-api/v1/building-access", new NameValueCollection {
                    ["id"] = id, ["session"] = session }), token); Check(e); return e; }, ct);
            diagnosis = e.Data; ended = e.ObservedAtUtc;
        }
        var next = new List<string>();
        if (centerId is null) next.Add("Bauzentrum/Bezirk nicht eindeutig belegt: districtId oder Bezugsgebäude nachlesen und gezielt erneut prüfen; keine Zuständigkeit aus Nähe ableiten.");
        if (!complete || selection.CandidateLimitReached) next.Add("Auswahl begrenzt: weitere Gebäudeseiten bzw. gezielte Produzenten prüfen; keine globale Abwesenheit behaupten.");
        if (omitted.Length > 0 || unknownCosts.Length > 0) next.Add("Nicht abgedeckte Güter/Kostendefinitionen separat prüfen; ausgewiesene Teilkosten sind kein vollständiger Baubedarf.");
        if (diagnosis is null) next.Add("Keine Gebäude zur Betriebsbeobachtung ausgewählt; Einzelabfragen zu Personal, Beständen und Gebäuden erforderlich.");
        return new(1, session, ended, graph.BridgeVersion, new("observed", basis, selection, demands, diagnosis, next.ToArray(),
            ["advisory_not_start_permission", "no_material_start_gate", "bounded_non_atomic_selection", "district_membership_not_delivery_proof",
             "no_geometric_proximity_as_district_proof", "recipe_capability_not_current_production", "no_transitive_supply_chain_scan"]));
    }
}

// One invocation owns its report collection, including internal batch/completion starts.
// AsyncLocal prevents concurrent cancellation/status calls from inheriting another call's results.
public sealed class OperatingPreflight
{
    private readonly NativeClient client;
    private readonly AsyncLocal<Scope?> current = new();
    public OperatingPreflight(NativeClient client) { this.client = client; client.BeforeOperatingRun = Before; }
    public sealed class Scope : IDisposable
    {
        private readonly OperatingPreflight owner;
        private readonly Scope? previous;
        public List<AutoOperatingReport> Reports { get; } = new();
        internal Scope(OperatingPreflight owner) { this.owner = owner; previous = owner.current.Value; owner.current.Value = this; }
        public void Dispose() => owner.current.Value = previous;
    }
    public Scope Begin() => new(this);
    public static bool NeedsSimulation(SimulationRunRequest r) => r.Starts && (r.Route == "simulation-run-until" || r.DurationHours >= 6);
    private async Task Before(SimulationRunRequest? simulation, LargeProjectRequest? project, CancellationToken ct)
    {
        if (current.Value is not { } scope) return;
        // Stop/status are never routed here; no retries and no automatic decisions from findings.
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(5));
        try {
            OperatingDemand[]? demands = simulation?.GoodTargets.Length > 0 ? simulation.GoodTargets.Select(t => new OperatingDemand(t.Good, t.Amount)).ToArray() :
                simulation?.StopAtAvailableLogs is { } logs ? [new("Log", logs)] : null;
            var e = await AutoOperatingTools.Observe(client, project?.Session ?? simulation!.Session, project?.DistrictId,
                simulation?.BuildingIds ?? [], demands, project?.Steps ?? [], project is not null ? "full_planned_costs_including_helpers" :
                demands is not null ? "stock_targets_not_construction_costs" : "operating_supply", budget.Token);
            scope.Reports.Add(e.Data);
        } catch (OperationCanceledException) when (!ct.IsCancellationRequested) {
            scope.Reports.Add(new("budget_exhausted", "unknown", null, [], null,
                ["Zeitbudget der Betriebsdiagnose erreicht; gezielt lesend nachprüfen. Keine Wiederholung des Startauftrags."], ["five_second_budget", "no_material_start_gate"]));
        } catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidDataException or JsonException) {
            scope.Reports.Add(new("unavailable", "unknown", null, [], null,
                ["Betriebsdiagnose nicht verfügbar; Einzelabfragen nachholen. Native Startprüfungen bleiben maßgeblich."], ["no_material_start_gate", "no_retry"]));
        }
    }
}
