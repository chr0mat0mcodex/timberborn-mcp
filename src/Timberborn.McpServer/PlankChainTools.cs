using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record PlankChainFinding(int Priority,string Stage,string Target,string Code,string Severity,string NextStep);
public sealed record PlankChainReport(string Product,string DistrictId,GraphRecipe[] Recipes,
    AutoOperatingReport Supply,OperationBatch? Sawmills,PlankChainFinding[] Findings,
    OperatingObservation[] Observations,bool ProductionProven,string[] Limitations);

public static class PlankChainTools
{
    public const string Name="inspect_plank_chain";
    public static Tool Reader()
    {
        var options=new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver=new DefaultJsonTypeInfoResolver() };
        return new() { Name=Name,
            Description="Lesende Bretterketten-Vorprüfung im ausdrücklich gewählten Bezirk: begrenzte automatische Auswahl von Holzlieferanten und Sägewerken; Holz-/Bretterbestand, Rezept, Pause, Überflutung, Personal, Arbeitszeit, Kraft sowie Zutaten-/Produktlager und Abtransport gemeinsam prüfen. Priorisierte findings mit konkreten nextStep. Keine automatische Aktion, Simulation, Budgetverlängerung oder Material-Startsperre. Begrenzte Suchabdeckung bleibt sichtbar; kein Liefer-/Produktionsnachweis. Bestehende Bridge 0.74.0 genügt.",
            InputSchema=JsonSerializer.SerializeToElement(new { type="object",properties=new {
                session=new { type="string",format="uuid" },districtId=new { type="string",format="uuid" }
            },required=new[]{"session","districtId"},additionalProperties=false }),
            OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<PlankChainReport>))),
            Annotations=new() { ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false } };
    }

    public static (string Session,string District) Parse(JsonElement args)
    {
        if(args.ValueKind!=JsonValueKind.Object)throw new ArgumentException("invalid_plank_chain_request");
        var keys=args.EnumerateObject().Select(p=>p.Name).ToArray();
        if(keys.Length!=2 || keys.Distinct().Count()!=2 || keys.Except(new[]{"session","districtId"}).Any())throw new ArgumentException("invalid_plank_chain_request");
        string Id(string key)=>args.GetProperty(key).ValueKind==JsonValueKind.String &&
            Guid.TryParseExact(args.GetProperty(key).GetString(),"D",out var id) && id!=Guid.Empty?id.ToString("D"):throw new ArgumentException("invalid_plank_chain_id");
        return (Id("session"),Id("districtId"));
    }

    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct)
    {
        var (session,district)=Parse(args);
        var e=await Observe(session,district,client.ProductionGraph,
            token=>AutoOperatingTools.Observe(client,session,district,[],[new("Log",0),new("Plank",0)],[],"plank_chain_supply",token),
            (ids,token)=>client.BuildingOperations(BuildingQueryRequest.Parse("/agent-api/v1/building-operations",
                new NameValueCollection { ["session"]=session,["ids"]=string.Join(",",ids),["diagnose"]="true" }),token),ct);
        return JsonSerializer.SerializeToNode(new NativeResult<PlankChainReport>(1,"ok",e.Data,
            new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!.AsObject();
    }

    public static async Task<BridgeEnvelope<PlankChainReport>> Observe(string session,string district,
        Func<CancellationToken,Task<BridgeEnvelope<NativeProductionGraph>>> graph,
        Func<CancellationToken,Task<BridgeEnvelope<AutoOperatingReport>>> supply,
        Func<string[],CancellationToken,Task<BridgeEnvelope<OperationBatch>>> operations,CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var g=await graph(ct);
        void Check<T>(BridgeEnvelope<T> e) {
            ct.ThrowIfCancellationRequested();
            if(e.SessionId!=session || e.BridgeVersion!=g.BridgeVersion)throw new BridgeRejectionException("state_conflict");
        }
        Check(g);var a=await supply(ct);Check(a);
        if(a.Data.Selection?.DistrictId is {} selected && selected!=district)throw new BridgeRejectionException("state_conflict");
        var recipes=g.Data.Definitions.Recipes.Where(x=>x.Outputs.Any(o=>o.Good=="Plank")).ToArray();
        var templates=recipes.SelectMany(x=>x.Buildings).ToHashSet(StringComparer.Ordinal);
        var ids=a.Data.Diagnosis?.Buildings.Where(b=>b.Role=="material_producer" && b.Operation is {} o && templates.Contains(o.Template))
            .Select(b=>b.Id).Distinct().Take(7).ToArray() ?? [];
        BridgeEnvelope<OperationBatch>? batch=null;
        if(ids.Length>0) { batch=await operations(ids,ct);Check(batch);
            if(!batch.Data.Items.Select(i=>i.Id).Order(StringComparer.Ordinal).SequenceEqual(ids.Order(StringComparer.Ordinal)))throw new BridgeRejectionException("state_conflict"); }
        var observations=new List<OperatingObservation> { new("chain_definitions",g.ObservedAtUtc),new("supply",a.ObservedAtUtc) };
        if(batch is not null)observations.Add(new("sawmill_flows",batch.ObservedAtUtc));
        return new(1,session,observations.Max(o=>o.ObservedAtUtc),g.BridgeVersion,
            Build(district,recipes,a.Data,batch?.Data,observations.ToArray()));
    }

    public static PlankChainReport Build(string district,GraphRecipe[] recipes,AutoOperatingReport supply,OperationBatch? sawmills,OperatingObservation[] observations)
    {
        var findings=new List<PlankChainFinding>();
        void Add(int priority,string stage,string target,string code,string severity,string step)
            =>findings.Add(new(priority,stage,target,code,severity,step));
        foreach(var step in supply.Diagnosis?.NextSteps ?? []) {
            if(step.Target==supply.Selection?.CenterId || step.Code=="verify_short_run")continue;
            Add(step.Priority,step.Target is "Log" or "Plank"?"supply":"operation",step.Target,step.Code,"advisory",step.Action);
        }
        if(supply.Selection is not { SearchComplete:true,CandidateLimitReached:false } || supply.Selection.UnresolvedBuildingIds.Length>0)
            Add(1,"coverage",district,"selection_incomplete","unknown","Weitere Gebäudeseiten und ungeklärte Bezirkszuordnungen prüfen; fehlende Treffer sind kein Abwesenheitsnachweis.");
        foreach(var next in supply.NextSteps)Add(2,"coverage",district,"supply_follow_up","unknown",next);
        var logs=supply.Diagnosis?.Materials.SingleOrDefault(m=>m.Good=="Log");
        if(logs?.GlobalAvailableStock==0)
            Add(2,"supply","Log","no_global_available_logs","warning","Global kein frei verfügbarer Holzbestand beobachtet. Lokales Sägewerksinventar, Reservierungen und Holzlieferanten prüfen; laufende Zyklen können Zutaten bereits verbraucht haben. Kein pauschales Startverbot.");
        if(recipes.Length==0)Add(1,"recipe","Plank","plank_recipe_unknown","unknown","Produktionsgraph und Fraktionsrezept für Bretter prüfen; keine Zutatenmengen erfinden.");
        if(sawmills is null || sawmills.Items.Length==0)
            Add(1,"manufacturing","Plank","sawmill_not_observed","unknown","Sägewerk gezielt mit find_buildings suchen und Betriebsdaten lesen; Auswahl kann unvollständig sein. Kein automatischer Neubau.");
        foreach(var item in sawmills?.Items ?? []) {
            if(item.Operation is not {} o) { Add(1,"manufacturing",item.Id,"sawmill_not_found","unknown","Aktuelle Gebäude-ID nachlesen; fehlend bedeutet nicht betriebsbereit.");continue; }
            if(!recipes.Any(r=>r.Id==o.Manufacturing?.Recipe && r.Buildings.Contains(o.Template)))
                Add(1,"recipe",item.Id,"plank_recipe_not_selected","warning","Erlaubte Rezepte und aktuelle Auswahl prüfen; Bretterproduktion ist nicht belegt. Keine automatische Rezeptänderung.");
            foreach(var finding in BottleneckAssessment.Read(o))
                Add(finding.Code=="building_flooded"?0:finding.Severity=="blocker"?1:2,finding.Area,item.Id,finding.Code,finding.Severity,finding.NextStep);
            if(o.Bottleneck is { State:"observed" } evidence) {
                foreach(var flow in evidence.Goods.Where(g=>g.Good is "Log" or "Plank")) {
                    if(flow.ReachableEndpoints>0)
                        Add(3,flow.Role=="product"?"storage":"transport",item.Id,"reachable_"+flow.Role+"_endpoint","info",
                            flow.Role=="product"?"Passendes erreichbares Produktziel beobachtet. Annahmemodus, unreservierten Platz und tatsächlichen Abtransport kontrollieren; Erreichbarkeit beweist keine Lieferung.":"Passende erreichbare Holzquelle beobachtet. Freien Bestand und tatsächliche Belieferung prüfen; globale Vorräte sind keine lokale Zusage.");
                    else if(flow.Role=="product")
                        Add(2,"storage",item.Id,"product_destination_unproven","unknown","Bretterlager, Annahmemodus, freien Platz und Weg gezielt prüfen; begrenzter Scan beweist keine kolonieweite Lagerlücke.");
                }
            }
        }
        Add(4,"verification","Plank","verify_bounded_production","advisory","Nach priorisierten Prüfungen einen ausdrücklich begrenzten Produktionslauf mit Sägewerk-IDs messen; native Zyklen, Holzverbrauch und Bretterausgabe vergleichen. Keine automatische Budgetverlängerung.");
        return new("Plank",district,recipes,supply,sawmills,
            findings.Distinct().OrderBy(f=>f.Priority).ThenBy(f=>f.Stage,StringComparer.Ordinal).ThenBy(f=>f.Target,StringComparer.Ordinal).ThenBy(f=>f.Code,StringComparer.Ordinal).ToArray(),
            observations,false,["read_only_no_automatic_expansion","no_material_start_gate","bounded_non_atomic_selection",
                "selected_district_not_delivery_guarantee","global_stock_not_local_supply","endpoint_reachability_not_transport_proof",
                "no_resource_designation_or_regrowth_forecast","no_throughput_or_production_proof","no_automatic_budget_extension"]);
    }
}
