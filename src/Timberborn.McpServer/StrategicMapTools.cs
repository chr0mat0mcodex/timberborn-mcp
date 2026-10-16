using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record StrategicScanRequest(string Session, int X = 0, int Y = 0, int Z = 0, int Width = 1, int Height = 1,
    string? ScanId = null, int TilesPerCall = 4, float? ScenarioWaterLevel = null, int ObjectOffset = 0,
    bool OverviewOnly = false, int SectionOffset = 0, int LinkOffset = 0, int SourceOffset = 0);
public sealed record StrategicTileSummary(int Tile, int X, int Y, int Width, int Height, int VoidCount,
    int WetColumns, int CaveColumns, int? LowestFloor, int? HighestFloor, DateTimeOffset ObservedAtUtc);
public sealed record StrategicScanCoverage(int CompletedTiles, int TotalTiles, int ObservedColumns, int TotalColumns,
    bool TerrainComplete, bool ObjectsComplete, bool Atomic, int? NextTile, DateTimeOffset FirstObservedAtUtc,
    DateTimeOffset LastObservedAtUtc, double ObservationSpreadSeconds, string TerrainChangeDetection);
public sealed record StrategicScanReport(string ScanId, string State, Position MapSize, ProtectedZone ProtectedZone,
    float? ScenarioWaterLevel, StrategicScanCoverage Coverage, int NativeReadsThisCall, StrategicTileSummary[] Tiles,
    StrategicAnalysis? Analysis, int ObservedSpecialObjectCount, int ObjectOffset, StrategicObject[] SpecialObjects,
    bool HasMoreObjects, double AgeSeconds, string NextStep, string[] Limitations, RiverOverview? Rivers = null);

public sealed class StrategicScanStore
{
    private sealed class Scan(StrategicScanRequest request, DateTimeOffset created)
    {
        public readonly string Id = Guid.NewGuid().ToString("D");
        public readonly StrategicScanRequest Request = request;
        public DateTimeOffset Touched = created;
        public readonly List<BridgeEnvelope<StrategicTile>> Tiles = [];
        public StrategicAnalysis? Analysis;
        public RiverOverview? Rivers;
    }
    private readonly Dictionary<string,Scan> scans = new();
    private readonly SemaphoreSlim gate = new(1,1);

    public async Task<StrategicScanReport> Observe(StrategicScanRequest request,
        Func<string,int,CancellationToken,Task<BridgeEnvelope<StrategicTile>>> read, CancellationToken ct)
    {
        Validate(request);
        await gate.WaitAsync(ct);
        try
        {
            var now = DateTimeOffset.UtcNow;
            foreach (string key in scans.Where(p => now-p.Value.Touched > TimeSpan.FromMinutes(15)).Select(p=>p.Key).ToArray()) scans.Remove(key);
            Scan scan;
            if (request.ScanId is null)
            {
                if (scans.Count >= 2) scans.Remove(scans.OrderBy(p=>p.Value.Touched).First().Key);
                scan = new(request,now); scans.Add(scan.Id,scan);
            }
            else
            {
                if (!scans.TryGetValue(request.ScanId,out var found)) throw new ArgumentException("strategic_scan_missing_or_expired_start_new_scan");
                scan = found;
                var expected = scan.Request with { ScanId=request.ScanId,TilesPerCall=request.TilesPerCall,ObjectOffset=request.ObjectOffset,
                    SectionOffset=request.SectionOffset,LinkOffset=request.LinkOffset,SourceOffset=request.SourceOffset };
                if (expected != request) throw new ArgumentException("strategic_scan_parameters_changed");
            }
            int reads=0;
            for (int count=0;count<request.TilesPerCall;count++)
            {
                if (scan.Tiles.Count>0 && scan.Tiles.Count==scan.Tiles[0].Data.TotalTiles) break;
                var page=await read(request.Session,scan.Tiles.Count,ct); reads++;
                if (page.SessionId!=request.Session) throw new BridgeRejectionException("stale_session");
                NativeClient.ValidateStrategicTile(page.Data,scan.Tiles.Count);
                if (scan.Tiles.Count>0 && (page.Data.MapSize!=scan.Tiles[0].Data.MapSize || page.BridgeVersion!=scan.Tiles[0].BridgeVersion ||
                    page.ObservedAtUtc<scan.Tiles[^1].ObservedAtUtc)) throw new InvalidDataException("strategic_map_changed_between_tiles");
                var size=page.Data.MapSize;
                if ((long)request.X+request.Width>size.X || (long)request.Y+request.Height>size.Y || request.Z>=size.Z)
                    throw new ArgumentException("strategic_target_outside_map");
                if(request.ScenarioWaterLevel >= size.Z)throw new ArgumentException("strategic_scenario_outside_map_height");
                // Commit only complete validated pages. Cancellation/failure never advances coverage.
                scan.Tiles.Add(page); scan.Touched=DateTimeOffset.UtcNow;
            }
            var first=scan.Tiles[0]; var last=scan.Tiles[^1];
            bool complete=scan.Tiles.Count==first.Data.TotalTiles;
            var zone=new ProtectedZone(request.X,request.Y,request.Z,request.Width,request.Height);
            var allObjects=scan.Tiles.SelectMany(t=>t.Data.Objects).GroupBy(o=>o.Id).Select(g=>g.Last()).ToArray();
            if (complete && scan.Analysis is null && !request.OverviewOnly)
                scan.Analysis=StrategicMapAnalysis.Analyze(first.Data.MapSize,scan.Tiles.SelectMany(t=>t.Data.Columns).ToArray(),
                    zone,request.ScenarioWaterLevel,ct,allObjects,scan.Tiles.All(t=>t.Data.ObjectsComplete));
            if(complete && scan.Rivers is null)
            {
                scan.Rivers=RiverOverviewAnalysis.Analyze(first.Data.MapSize,scan.Tiles.SelectMany(t=>t.Data.Columns).ToArray(),
                    scan.Tiles.SelectMany(t=>t.Data.Sources ?? []).GroupBy(s=>s.Id).Select(g=>g.Last()).ToArray(),
                    scan.Tiles.All(t=>t.Data.SourcesComplete),first.Data.Weather,last.Data.Weather,ct);
                var previous=scans.Values.Where(s=>s!=scan && s.Request.Session==request.Session && s.Rivers?.State=="geometric_river_overview" &&
                    s.Tiles[^1].ObservedAtUtc<first.ObservedAtUtc).OrderByDescending(s=>s.Tiles[^1].ObservedAtUtc).FirstOrDefault();
                if(previous?.Rivers is {} old && scan.Rivers.State=="geometric_river_overview")
                    scan.Rivers=RiverOverviewAnalysis.Compare(scan.Rivers,old) with {PreviousScanObservedAtUtc=previous.Tiles[^1].ObservedAtUtc};
            }
            var specials=allObjects.Where(o=>o.Kind=="special_or_unknown").OrderBy(o=>o.Template,StringComparer.Ordinal).ThenBy(o=>o.X).ThenBy(o=>o.Y).ThenBy(o=>o.Id).ToArray();
            var items=specials.Skip(request.ObjectOffset).Take(32).ToArray();
            int columnCount=scan.Tiles.Sum(t=>t.Data.Columns.Length);
            scan.Touched=DateTimeOffset.UtcNow;
            return new(scan.Id,complete?"complete_non_atomic":"scanning",first.Data.MapSize,zone,request.ScenarioWaterLevel,
                new(scan.Tiles.Count,first.Data.TotalTiles,columnCount,first.Data.MapSize.X*first.Data.MapSize.Y,complete,
                    complete&&scan.Tiles.All(t=>t.Data.ObjectsComplete),false,complete?null:scan.Tiles.Count,
                    first.ObservedAtUtc,last.ObservedAtUtc,(last.ObservedAtUtc-first.ObservedAtUtc).TotalSeconds,"not_available_revalidate_candidates"),reads,
                scan.Tiles.Select(t=>Summary(t)).ToArray(),scan.Analysis,specials.Length,request.ObjectOffset,items,
                request.ObjectOffset+items.Length<specials.Length,(DateTimeOffset.UtcNow-last.ObservedAtUtc).TotalSeconds,
                !complete?"continue_same_scanId_and_target":request.OverviewOnly?"page_river_sections_links_sources_reobserve_before_forecasting":scan.Analysis?.State!="terrain_analysis_complete"?
                    "analysis_unavailable_review_analysis_state":scan.Analysis?.Hazards?.NextAction ?? "reobserve_candidate_regions_then_use_native_build_planning",
                ["bounded_tiles_observed_while_simulation_runs", "no_atomic_map_snapshot_or_change_revision",
                    "partial_scan_never_produces_protection_recommendations", "cache_two_scans_per_client_expires_after_15_minutes_idle",
                    "completed_scan_is_historical_start_new_scan_to_refresh", "source_template_hints_not_native_source_classification",
                    "special_objects_exclude_vegetation_buildings_paths", "corridors_are_up_to_eight_representatives_not_all_hazards",
                    "route_path_contains_turning_points_not_every_cell", "no_placement_or_simulation_speed_change"],
                scan.Rivers is null ? null : RiverOverviewAnalysis.Page(scan.Rivers,request.SectionOffset,request.LinkOffset,request.SourceOffset));
        }
        finally { gate.Release(); }
    }
    private static StrategicTileSummary Summary(BridgeEnvelope<StrategicTile> t)
    {
        var d=t.Data; var spans=d.Columns.SelectMany(c=>c.Spans).ToArray();
        return new(d.Tile,d.X,d.Y,d.Width,d.Height,spans.Length,d.Columns.Count(c=>c.Spans.Any(s=>s.WaterDepth>0.01f)),
            d.Columns.Count(c=>c.Spans.Length>1||c.Spans.Any(s=>s.Top<d.MapSize.Z)),
            spans.Length==0?null:spans.Min(s=>s.Bottom),spans.Length==0?null:spans.Max(s=>s.Bottom),t.ObservedAtUtc);
    }
    public static void Validate(StrategicScanRequest r)
    {
        if (!Guid.TryParseExact(r.Session,"D",out var session)||session==Guid.Empty ||
            r.ScanId is not null && (!Guid.TryParseExact(r.ScanId,"D",out var scan)||scan==Guid.Empty) ||
            r.X is <0 or >511 || r.Y is <0 or >511 || r.Z is <0 or >63 || r.Width is <1 or >64 || r.Height is <1 or >64 ||
            r.TilesPerCall is <1 or >8 || r.ObjectOffset is <0 or >131072 ||
            r.SectionOffset is <0 or >4096 || r.LinkOffset is <0 or >32768 || r.SourceOffset is <0 or >131072 ||
            r.ScenarioWaterLevel is {} level && (!float.IsFinite(level)||level<0||level>64))
            throw new ArgumentException("invalid_strategic_scan_request");
    }
}

public static class StrategicMapTools
{
    public const string Name="scan_strategic_map";
    private static readonly ConditionalWeakTable<NativeClient,StrategicScanStore> Stores=new();
    public static Tool Reader()
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        var schema=options.GetJsonSchemaAsNode(typeof(StrategicScanRequest)).AsObject();
        schema["type"]="object";
        schema["additionalProperties"]=false;
        var props=schema["properties"]!.AsObject();
        foreach(var (key,min,max) in new[]{("x",0,511),("y",0,511),("z",0,63),("width",1,64),("height",1,64),("tilesPerCall",1,8),("objectOffset",0,131072),("sectionOffset",0,4096),("linkOffset",0,32768),("sourceOffset",0,131072)})
        {props[key]!["minimum"]=min;props[key]!["maximum"]=max;}
        return new(){Name=Name,Description="Strategischer, lesender Gesamtkartenscan ab 0.62. Mit Zielzone liefert analysis.terraforming zusätzlich bis zu drei bedingte 3x3-Reservoirkandidaten mit zwei Ebenen Absenkung, geometrischem Zusatzvolumen, Zuflussbeleg und Blockaden; kein garantierter Wasservorrat oder Grabauftrag. Zuerst analysis.hazards lesen: alle erreichbaren nassen Hohlräume bewertet, Restwasser über Zielhöhe und Geländeschwelle priorisiert, unabhängig von Quellenaktivität. Priorities enthalten Schutzquerschnitt, Kronenhöhe, Höhlendecken-/Umwegprüfung und nächste Maßnahmen; keine Bau- oder Flutgarantie. Zähler sind Hohlräume, keine unabhängigen Flutereignisse; höchstens acht priorisierte Beispiele. overviewOnly=true erlaubt den Flussüberblick ohne Siedlungsziel. rivers liefert Abschnitte (sectionOffset, 32), geometrische Verbindungen (linkOffset, 64), native Quellen (sourceOffset, 16) und Wetterzustände. Kontamination gilt lokal, Richtung ist unbekannt; Schwellen sind keine Durchflussprognose. Quellen nennen native Stärke, Kontamination, Auslässe, Modifikatoren und belegte Aktivierungsregeln. Künftige Kontamination bleibt unbekannt; Änderungen gegenüber einem früheren fertigen Scan derselben Sitzung sind Beobachtungen, kein Kausalbeweis. Wetterzeiten sind Rohwerte, kein abgenommener Countdown. Zielzone x/y/z/width/height angeben, NICHT den Scanbereich: immer ganze Karte bis 512×512×64. Pro Aufruf 1–8 native 32×32-Kacheln (Standard vier), bei laufendem Spiel ohne Pause. Mit zurückgegebener scanId und identischer Zielzone fortsetzen. Keine Schutzvorschläge vor vollständiger Terrainabdeckung. Hohlräume/Überhänge, Spezialobjekte, mögliche Wasser-/Randverbindungen und niedrigste Geländeschwellen; geometrische Routen sind keine Flutprognose. Optional scenarioWaterLevel: absolute Rasterhöhe, kein gemessener Zukunftspegel. Kandidaten mit Koordinaten, Querschnitt und angenommener Sperrhöhe; keine Bauvalidierung oder vollständige Abschottung. Bauwerke/Wasserdynamik nicht modelliert, Rückstau/Umwege frisch prüfen. objectOffset blättert Spezialobjekte (32). Zwei Scans je Client, 15 Minuten Leerlauf; fertiger Scan historisch, neuer Aufruf ohne scanId aktualisiert. Abdeckung und Beobachtungszeitraum beachten.",
            InputSchema=JsonSerializer.SerializeToElement(schema),
            OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<StrategicScanReport>))),
            Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=false,OpenWorldHint=false}};
    }
    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct)
    {
        var request=BuildBatchState.Parse<StrategicScanRequest>(args);
        try
        {
            StrategicScanStore.Validate(request);
            var current=await client.Simulation(ct);
            if(current.SessionId!=request.Session)throw new BridgeRejectionException("stale_session");
            if(current.BridgeVersion!="0.62.0")throw new InvalidDataException("strategic_map_requires_062");
            var report=await Stores.GetValue(client,_=>new()).Observe(request,client.StrategicMap,ct);
            report=report with {NativeReadsThisCall=report.NativeReadsThisCall+1};
            return JsonSerializer.SerializeToNode(new NativeResult<StrategicScanReport>(1,"ok",report,
                new("native",false,request.Session,report.Coverage.LastObservedAtUtc),null),NativeJson.Options)!.AsObject();
        }
        catch(Exception ex) when ((ex is ArgumentException || ex is InvalidDataException) &&
            (ex.Message.StartsWith("strategic_",StringComparison.Ordinal)||ex.Message=="invalid_strategic_scan_request"))
        {
            return JsonSerializer.SerializeToNode(new NativeResult<StrategicScanReport>(1,"error",null,
                new("native",false,request.Session,null),new(ex.Message,"Strategischer Scan abgelehnt: "+ex.Message+". Keine Spieländerung; gültige scanId bleibt für bereits bestätigte Kacheln erhalten.",false,"not_applicable")),NativeJson.Options)!.AsObject();
        }
    }
}
