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

public sealed record OperatingDemand(string Good, int Amount);
public sealed record OperatingRequest(string Session, string ConstructionCenterId, string[] ProducerIds, OperatingDemand[] Materials);
public sealed record OperatingStep(int Priority, string Target, string Code, string Action);
public sealed record OperatingBuilding(string Id, string Role, string State, OperationSummary? Operation, NativeAccess? Access);
public sealed record OperatingMaterial(string Good, int Required, int? GlobalAvailableStock, double? CoveragePercent,
    string[] SelectedRecipeProducerIds, string[] SelectedHarvesterIds, string SupplyState, bool ProductionProven);
public sealed record OperatingObservation(string Source, DateTimeOffset ObservedAtUtc);
public sealed record OperatingReport(Workforce Workforce, OperatingBuilding[] Buildings, OperatingMaterial[] Materials,
    OperatingStep[] NextSteps, bool GoodsComplete, int? GoodsNextOffset, bool Atomic, bool ProductionProven,
    OperatingObservation[] Observations, string[] Limitations);

public static class OperatingReadinessTools
{
    public const string Name = "inspect_operating_readiness";
    public const int MaxGoodsPages = 4;
    public static Tool Reader()
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        return new() {
            Name = Name,
            Description = "Gemeinsame lesende Betriebsdiagnose vor längeren Bau-/Simulationsläufen: ausgewähltes Bauzentrum, native Überflutung, Personal, Zugang und bis sieben ausgewählte Materialproduzenten. IDs vorher über find_buildings bestimmen. materials enthält 1–8 Güter und vollständigen geplanten Bedarf einschließlich Zugangshilfen; amount=0 prüft nur Versorgung ohne Deckungsquote. Priorisierte nächste Schritte, keine Startsperre und keine automatische Aktion. Producer-Auswahl ist keine vollständige Koloniesuche: kein Treffer bedeutet unbekannte Versorgung, nicht fehlende Produktion. Globale Bestände sind kein Liefernachweis. Höchstens 128 Güter, 15 native Leseaufrufe, kein Warten/Retry; dieselbe Session, keine atomare Gesamtsicht oder Produktionsgarantie.",
            InputSchema = JsonSerializer.SerializeToElement(new {
                type = "object", properties = new {
                    session = new { type = "string", format = "uuid" },
                    constructionCenterId = new { type = "string", format = "uuid" },
                    producerIds = new { type = "array", maxItems = 7, uniqueItems = true, items = new { type = "string", format = "uuid" } },
                    materials = new { type = "array", minItems = 1, maxItems = 8, items = new {
                        type = "object", properties = new { good = new { type = "string" }, amount = new { type = "integer", minimum = 0, maximum = 1000000 } },
                        required = new[] { "good", "amount" }, additionalProperties = false } }
                }, required = new[] { "session", "constructionCenterId", "producerIds", "materials" }, additionalProperties = false }),
            OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<OperatingReport>))),
            Annotations = new() { ReadOnlyHint = true, DestructiveHint = false, IdempotentHint = true, OpenWorldHint = false }
        };
    }

    public static OperatingRequest Parse(JsonElement args)
    {
        static void Keys(JsonElement value, params string[] names) {
            if (value.ValueKind != JsonValueKind.Object) throw new ArgumentException();
            var keys = value.EnumerateObject().Select(p => p.Name).ToArray();
            if (keys.Length != names.Length || keys.Distinct().Count() != keys.Length || keys.Except(names).Any()) throw new ArgumentException();
        }
        static string Id(JsonElement value) {
            if (value.ValueKind != JsonValueKind.String || !Guid.TryParseExact(value.GetString(), "D", out var id) || id == Guid.Empty) throw new ArgumentException();
            return id.ToString("D");
        }
        Keys(args, "session", "constructionCenterId", "producerIds", "materials");
        var session = Id(args.GetProperty("session")); var center = Id(args.GetProperty("constructionCenterId"));
        var producers = args.GetProperty("producerIds"); var materials = args.GetProperty("materials");
        if (producers.ValueKind != JsonValueKind.Array || producers.GetArrayLength() > 7 ||
            materials.ValueKind != JsonValueKind.Array || materials.GetArrayLength() is < 1 or > 8) throw new ArgumentException();
        var ids = producers.EnumerateArray().Select(Id).ToArray();
        if (ids.Contains(center) || ids.Distinct().Count() != ids.Length) throw new ArgumentException();
        var demands = materials.EnumerateArray().Select(m => {
            Keys(m, "good", "amount");
            var g = m.GetProperty("good"); var a = m.GetProperty("amount");
            if (g.ValueKind != JsonValueKind.String || !BuildingPolicy.ValidTemplate(g.GetString()!) ||
                a.ValueKind != JsonValueKind.Number || !a.TryGetInt32(out int amount) || amount is < 0 or > 1000000) throw new ArgumentException();
            return new OperatingDemand(g.GetString()!, amount);
        }).ToArray();
        if (demands.Select(d => d.Good).Distinct().Count() != demands.Length) throw new ArgumentException();
        return new(session, center, ids, demands);
    }

    public static async Task<JsonObject> Invoke(NativeClient client, JsonElement args, CancellationToken ct)
    {
        var r = Parse(args);
        var e = await Observe(r, client.Snapshot, client.ProductionGraph,
            token => client.BuildingOperations(BuildingQueryRequest.Parse("/agent-api/v1/building-operations",
                new NameValueCollection { ["session"] = r.Session, ["ids"] = string.Join(",", new[] { r.ConstructionCenterId }.Concat(r.ProducerIds)) }), token),
            (offset, token) => client.Goods(EconomyRequest.Parse("/agent-api/v1/goods", new NameValueCollection {
                ["offset"] = offset.ToString(CultureInfo.InvariantCulture), ["limit"] = "32" }), token),
            (id, token) => client.BuildingAccess(LogisticsRequest.Parse("/agent-api/v1/building-access",
                new NameValueCollection { ["id"] = id, ["session"] = r.Session }), token), ct);
        return JsonSerializer.SerializeToNode(new NativeResult<OperatingReport>(1, "ok", e.Data,
            new("native", false, e.SessionId, e.ObservedAtUtc), null), NativeJson.Options)!.AsObject();
    }

    public static async Task<BridgeEnvelope<OperatingReport>> Observe(OperatingRequest r,
        Func<CancellationToken, Task<BridgeEnvelope<NativeSnapshot>>> snapshot,
        Func<CancellationToken, Task<BridgeEnvelope<NativeProductionGraph>>> graph,
        Func<CancellationToken, Task<BridgeEnvelope<OperationBatch>>> operations,
        Func<int, CancellationToken, Task<BridgeEnvelope<NativeGoods>>> goods,
        Func<string, CancellationToken, Task<BridgeEnvelope<NativeAccess>>> access, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var start = await snapshot(ct);
        var observations = new List<OperatingObservation>();
        T Check<T>(string source, BridgeEnvelope<T> e) {
            ct.ThrowIfCancellationRequested();
            if (e.SessionId != r.Session || e.BridgeVersion != start.BridgeVersion) throw new BridgeRejectionException("state_conflict");
            observations.Add(new(source, e.ObservedAtUtc)); return e.Data;
        }
        var colony = Check("workforce", start);
        var definitions = Check("production_graph", await graph(ct));
        var batch = Check("operations", await operations(ct));
        var stock = new List<GoodStock>();
        int? total = null; bool complete = false;
        for (int page = 0; page < MaxGoodsPages; page++) {
            ct.ThrowIfCancellationRequested();
            var d = Check("goods:" + page, await goods(page * 32, ct));
            if (d.Offset != stock.Count || total is {} n && n != d.Total) throw new BridgeRejectionException("state_conflict");
            total = d.Total; stock.AddRange(d.Items); complete = !d.HasMore;
            if (complete) break;
        }
        if (!stock.Select(s => s.Id).SequenceEqual(stock.Select(s => s.Id).Distinct().Order(StringComparer.Ordinal)))
            throw new BridgeRejectionException("state_conflict");
        var steps = new List<OperatingStep>();
        var buildings = new List<OperatingBuilding>();
        foreach (var item in batch.Items) {
            ct.ThrowIfCancellationRequested();
            var role = item.Id == r.ConstructionCenterId ? "construction_center" : "material_producer";
            NativeAccess? a = null; OperationSummary? summary = null;
            if (item.Operation is {} o) {
                a = Check("access:" + item.Id, await access(item.Id, ct));
                var assessment = OperationAssessmentBuilder.Read(o);
                summary = new(o.Template, o.Finished, o.Paused, o.Workplace, o.Manufacturing, o.Power, assessment, null, o.Water);
                foreach (var code in assessment.Blockers.Concat(assessment.Warnings).Concat(assessment.Unknown).Distinct())
                    if (role != "construction_center" || code != "manufacturing_not_observed")
                        steps.Add(Step(item.Id, code, o.Water?.Flooded == true));
                if (o.Water?.Flooded is null) steps.Add(Step(item.Id, "flood_state_unknown", false));
                if (o.Workplace is null) steps.Add(Step(item.Id, "workplace_unknown", false));
                if (o.Paused is null) steps.Add(Step(item.Id, "pause_state_unknown", false));
                if (a.EntranceBlocked == true || a.EntranceInaccessible == true || a.UnconnectedBlocked == true)
                    steps.Add(new(1, item.Id, "access_blocked", "Eingang und Bezirksanschluss prüfen und reparieren; Verbindung allein beweist keinen Baubetrieb."));
                // A zero component count explicitly explains the absent optional blocker value.
                // Unknown count or a present component without a value remains unknown.
                if (a.EntranceBlocked is null || a.EntranceInaccessible is null ||
                    a.UnconnectedBlocked is null && a.UnconnectedBlockerCount != 0)
                    steps.Add(new(2, item.Id, "access_unknown", "Zugangsbelege fehlen teilweise; inspect_building_access gezielt auswerten."));
            } else steps.Add(new(1, item.Id, "building_not_found", "Gebäude erneut suchen und aktuelle ID auswählen; fehlend ist kein gesunder Betrieb."));
            buildings.Add(new(item.Id, role, item.State, summary, a));
        }
        var materials = r.Materials.Select(d => {
            var s = stock.SingleOrDefault(g => g.Id == d.Good);
            var producers = batch.Items.Where(i => i.Id != r.ConstructionCenterId && i.Operation is not null).ToArray();
            var recipeIds = producers.Where(i => definitions.Definitions.Recipes.Any(recipe =>
                recipe.Id == i.Operation!.Manufacturing?.Recipe && recipe.Buildings.Contains(i.Operation.Template) &&
                recipe.Outputs.Any(g => g.Good == d.Good))).Select(i => i.Id).ToArray();
            var harvesters = producers.Where(i => definitions.Definitions.Sources.Any(source =>
                source.Good == d.Good && source.Harvesters.Contains(i.Operation!.Template))).Select(i => i.Id).ToArray();
            var candidates = recipeIds.Concat(harvesters).Distinct().ToArray();
            string state = candidates.Length == 0 ? "not_observed_in_selection" : candidates.All(id =>
                OperationAssessmentBuilder.Read(producers.Single(i => i.Id == id).Operation!).Blockers.Length > 0 ||
                buildings.Single(b => b.Id == id).Access is { } route &&
                (route.EntranceBlocked == true || route.EntranceInaccessible == true || route.UnconnectedBlocked == true))
                ? "selected_producers_blocked" : "potential_supply_unproven";
            if (s is null) steps.Add(new(2, d.Good, "stock_unknown", complete ?
                "Gut nicht im gelesenen Katalog: Güter-ID prüfen; Bestand nicht als null annehmen." : "Weitere Güterseiten mit inspect_goods lesen; der begrenzte Scan enthält dieses Gut noch nicht."));
            else if (s.AvailableStock < d.Amount) steps.Add(new(3, d.Good, "material_shortfall",
                $"Global fehlen {d.Amount - s.AvailableStock} Einheiten zum angegebenen Bedarf. Nachlieferung und Produzenten prüfen; Bau darf auf Material warten, keine pauschale Startsperre."));
            steps.Add(new(3, d.Good, state, state == "not_observed_in_selection" ?
                "Passende Rezept-/Erntegebäude mit Produktionsgraph und find_buildings suchen und in die Auswahl aufnehmen; außerhalb der Auswahl ist Versorgung unbekannt." :
                state == "selected_producers_blocked" ? "Belegte Blockaden der ausgewählten Produzenten zuerst bearbeiten; alternative Versorgung prüfen." :
                "Lieferweg und bei Erntern markierte erreichbare Ressourcen prüfen; danach kurzen beobachteten Produktionslauf statt langer Blind-Simulation verwenden."));
            return new OperatingMaterial(d.Good, d.Amount, s?.AvailableStock,
                d.Amount > 0 && s is not null ? Math.Round(100d * s.AvailableStock / d.Amount, 1) : null,
                recipeIds, harvesters, state, false);
        }).ToArray();
        steps.Add(new(4, "colony", "verify_short_run", "Nach den priorisierten Prüfungen kurz simulieren und reale Lieferung/Produktion nachlesen; bei Verschlechterung stoppen. Globale freie Arbeiter sind keine lokale Zuweisungszusage."));
        var report = new OperatingReport(colony.Workforce, buildings.ToArray(), materials,
            steps.Distinct().OrderBy(s => s.Priority).ThenBy(s => s.Target, StringComparer.Ordinal).ThenBy(s => s.Code, StringComparer.Ordinal).ToArray(),
            complete, complete ? null : stock.Count, false, false, observations.ToArray(),
            ["selected_buildings_only_no_colony_wide_producer_absence_claim", "construction_center_role_supplied_by_caller_not_capability_verified",
             "global_workforce_not_district_staffing", "global_stock_not_delivery_or_reservation", "non_atomic_same_session_reads",
             "no_throughput_or_production_proof", "no_automatic_start_gate", "building_access_not_project_site_access", "demand_supplied_by_caller_including_access_helpers"]);
        return new(1, start.SessionId, observations.Max(o => o.ObservedAtUtc), start.BridgeVersion, report);
    }

    private static OperatingStep Step(string id, string code, bool flooded) => new(
        code == "building_flooded" ? 0 : (code is "no_assigned_workers" or "understaffed") && flooded ? 3 : 2, id, code, code switch {
            "building_flooded" => "Überflutung zuerst beheben oder trockenen Ersatzstandort prüfen; mehr Personal löst diese Blockade nicht. Vor Wasserumbau Rückstau und Versorgungsreserve prüfen.",
            "no_assigned_workers" or "understaffed" => flooded ? "Erst nach geklärter Überflutung Personal zuweisen; globale Arbeitslosenreserve und lokale Erreichbarkeit prüfen." : "Soll-/Ist-Besetzung, globale Arbeitslosenreserve und lokale Erreichbarkeit prüfen; Teilbesetzung ist keine vollständige Blockade.",
            "building_paused" => "Pause nach Prüfung von Überflutung und Versorgung gezielt aufheben.",
            "outside_working_hours" => "Arbeitszeit beachten; Leerlauf außerhalb der Schicht ist kein dauerhafter Produktionsfehler.",
            "ingredients_not_ready" => "Zutatenbestand und erreichbare Zulieferer prüfen; Baukosten sind keine Rezeptzutaten.",
            "fuel_not_ready" => "Brennstoffbestand und Belieferung des ausgewählten Rezepts prüfen.",
            "output_capacity_unavailable" => "Produktlager, freien Platz und Abtransport prüfen.",
            "recipe_missing" => "Passendes freigeschaltetes Rezept für das benötigte Material auswählen.",
            "consumer_not_powered" or "network_power_deficit" => "Aktives Kraftnetz, Erzeugung und Verbrauch prüfen.",
            "construction_unfinished" => "Baufortschritt und Baustellenzugang prüfen; unfertiges Gebäude liefert keinen Betriebsnachweis.",
            "water_intake_dry" or "water_intake_blocked" or "water_intake_contaminated" => "Ansaugpunkt, Wasserstand, Kontamination und Hindernisse prüfen; Personal ist dafür kein Ersatz.",
            _ => "Offenen Prüfpunkt mit inspect_building_operation bzw. inspect_building_operations(diagnose=true) klären; unbekannt nicht als betriebsbereit werten."
        });
}
