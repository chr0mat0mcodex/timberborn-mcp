using Timberborn.Backend.Native;

namespace Timberborn.McpServer;

public sealed record RiverSection(int Id, Position Sample, Position BoundsMin, Position BoundsMax, int Cells, bool Covered, string Liquid,
    float? MinLevel, float? MaxLevel, float? MinContamination, float? MaxContamination, int? BoundaryPass);
public sealed record RiverLink(int A, int B, int TerrainPass, Position MeetingAt, string Direction,
    bool ALevelAbovePass, bool BLevelAbovePass);
public sealed record RiverSource(StrategicSource Native, int[] Sections, string Liquid,
    string WeatherExpectation, string FutureContamination, float? StrengthChange = null, float? ContaminationChange = null);
public sealed record RiverOverview(string State, int TotalSections, int TotalLinks, int TotalSources,
    RiverSection[] Sections, RiverLink[] Links, RiverSource[] Sources, bool SourcesComplete,
    StrategicWeather? FirstWeather, StrategicWeather? LastWeather, bool WeatherChanged,
    int SectionOffset, int LinkOffset, int SourceOffset, bool MoreSections, bool MoreLinks, bool MoreSources,
    string[] Limitations, string ComparisonState = "no_previous_complete_scan", bool? ComparisonWeatherChanged = null,
    DateTimeOffset? PreviousScanObservedAtUtc = null);

public static class RiverOverviewAnalysis
{
    private sealed record Cell(int X,int Y,StrategicSpan Span);
    private static readonly (int X,int Y)[] Steps=[(1,0),(-1,0),(0,1),(0,-1)];
    public static string Liquid(float min,float max) => max<=0.00001f ? "water" : min>=0.99999f ? "badwater" : "mixed_or_variable";
    public static RiverOverview Analyze(Position size, IReadOnlyList<StrategicColumn> columns,
        IReadOnlyList<StrategicSource> sources, bool sourcesComplete, StrategicWeather? firstWeather,
        StrategicWeather? lastWeather, CancellationToken ct)
    {
        string[] limits=["geometric_connectivity_not_simulated_flow", "waterworks_not_modelled",
            "sections_split_at_8x8_boundaries_and_disconnected_voids", "levels_and_contamination_are_local_samples",
            "dry_connections_are_possible_paths_not_current_rivers", "no_arrival_time_or_future_mixing_prediction",
            "strength_is_native_strength_not_measured_discharge", "weather_durations_not_verified_countdown",
            "contamination_label_tolerance_0.00001", "map_observations_not_atomic"];
        bool changed=firstWeather is not null && lastWeather is not null &&
            (firstWeather.Cycle!=lastWeather.Cycle || firstWeather.Hazardous!=lastWeather.Hazardous || firstWeather.SelectedHazard!=lastWeather.SelectedHazard);
        RiverOverview Empty(string state)=>new(state,0,0,sources.Count,[],[],[],sourcesComplete,firstWeather,lastWeather,changed,0,0,0,false,false,false,limits);
        if(columns.Count!=size.X*size.Y)return Empty("incomplete_terrain");
        var cells=new List<Cell>(); var byColumn=new Dictionary<(int,int),List<int>>();
        foreach(var c in columns)
        {
            ct.ThrowIfCancellationRequested(); var ids=new List<int>();
            foreach(var s in c.Spans){ids.Add(cells.Count);cells.Add(new(c.X,c.Y,s));}
            if(!byColumn.TryAdd((c.X,c.Y),ids))throw new InvalidDataException("strategic_duplicate_column");
            if(cells.Count>524288)return Empty("river_node_budget_exceeded");
        }
        IEnumerable<int> Neighbors(int i)
        {
            var a=cells[i];
            foreach(var d in Steps)if(byColumn.TryGetValue((a.X+d.X,a.Y+d.Y),out var ids))
                foreach(int j in ids)if(Math.Max(a.Span.Bottom,cells[j].Span.Bottom)<Math.Min(a.Span.Top,cells[j].Span.Top))yield return j;
        }
        Position Point(int i)=>new(cells[i].X,cells[i].Y,cells[i].Span.Bottom);
        bool Wet(int i)=>cells[i].Span.WaterDepth>0.01f;
        var sourceNodes=new Dictionary<Guid,int[]>(); var sourceSeeds=new HashSet<int>();
        foreach(var s in sources)
        {
            var ids=s.Outlets.SelectMany(p=>byColumn.TryGetValue((p.X,p.Y),out var at)
                ? at.Where(i=>cells[i].Span.Bottom<=p.Z && p.Z<cells[i].Span.Top):Enumerable.Empty<int>()).Distinct().ToArray();
            sourceNodes[s.Id]=ids;sourceSeeds.UnionWith(ids);
        }
        var owner=Enumerable.Repeat(-1,cells.Count).ToArray(); var groups=new List<List<int>>();
        for(int i=0;i<cells.Count;i++)
        {
            ct.ThrowIfCancellationRequested();if(owner[i]>=0 || !Wet(i)&&!sourceSeeds.Contains(i))continue;
            if(groups.Count>=4096)return Empty("river_section_budget_exceeded");
            int id=groups.Count;var group=new List<int>();groups.Add(group);var queue=new Queue<int>();queue.Enqueue(i);owner[i]=id;
            while(queue.TryDequeue(out int n))
            {
                group.Add(n);var a=cells[n];
                foreach(int j in Neighbors(n))
                {
                    var b=cells[j];
                    if(owner[j]>=0 || !Wet(n)||!Wet(j) || a.X/8!=b.X/8 || a.Y/8!=b.Y/8 ||
                        Math.Min(a.Span.Bottom+a.Span.WaterDepth,b.Span.Bottom+b.Span.WaterDepth)<=Math.Max(a.Span.Bottom,b.Span.Bottom))continue;
                    owner[j]=id;queue.Enqueue(j);
                }
            }
        }
        // Freeze source association before dry-terrain expansion assigns catchments.
        var associations=sourceNodes.ToDictionary(p=>p.Key,p=>p.Value.Select(i=>owner[i]).Where(i=>i>=0).Distinct().Order().ToArray());
        var seeded=owner.Select(o=>o>=0).ToArray();
        var distance=Enumerable.Repeat(int.MaxValue,cells.Count).ToArray();var flood=new PriorityQueue<int,int>();
        foreach(var g in groups)foreach(int i in g){distance[i]=cells[i].Span.Bottom;flood.Enqueue(i,distance[i]);}
        var links=new Dictionary<(int,int),(int Pass,int At)>();var boundary=Enumerable.Repeat(int.MaxValue,groups.Count).ToArray();
        while(flood.TryDequeue(out int i,out int score))
        {
            ct.ThrowIfCancellationRequested();if(distance[i]!=score)continue;var a=cells[i];
            if(a.X==0||a.Y==0||a.X==size.X-1||a.Y==size.Y-1)boundary[owner[i]]=Math.Min(boundary[owner[i]],score);
            foreach(int j in Neighbors(i))
            {
                int pass=Math.Max(score,cells[j].Span.Bottom);
                if(owner[j]>=0 && owner[i]!=owner[j])
                {
                    var key=(Math.Min(owner[i],owner[j]),Math.Max(owner[i],owner[j]));int saddle=Math.Max(pass,distance[j]);
                    if(!links.TryGetValue(key,out var old)||saddle<old.Pass)links[key]=(saddle,i);
                    if(links.Count>32768)return Empty("river_link_budget_exceeded");
                }
                // Wet sections and dry source seeds stay fixed; dry terrain follows the lowest discovered pass.
                if(seeded[j] || pass>=distance[j])continue;
                owner[j]=owner[i];distance[j]=pass;flood.Enqueue(j,pass);
            }
        }
        var sections=groups.Select((g,id)=>
        {
            var wet=g.Where(Wet).Select(i=>cells[i].Span).ToArray();
            return new RiverSection(id,Point(g[0]),new(g.Min(i=>cells[i].X),g.Min(i=>cells[i].Y),g.Min(i=>cells[i].Span.Bottom)),
                new(g.Max(i=>cells[i].X),g.Max(i=>cells[i].Y),g.Max(i=>cells[i].Span.Top)-1),g.Count,g.Any(i=>cells[i].Span.Top<size.Z),wet.Length==0?"dry":Liquid(wet.Min(s=>s.Contamination),wet.Max(s=>s.Contamination)),
                wet.Length==0?null:wet.Min(s=>s.Bottom+s.WaterDepth),wet.Length==0?null:wet.Max(s=>s.Bottom+s.WaterDepth),
                wet.Length==0?null:wet.Min(s=>s.Contamination),wet.Length==0?null:wet.Max(s=>s.Contamination),boundary[id]==int.MaxValue?null:boundary[id]);
        }).ToArray();
        var edges=links.OrderBy(p=>p.Key.Item1).ThenBy(p=>p.Key.Item2).Select(p=>new RiverLink(p.Key.Item1,p.Key.Item2,p.Value.Pass,Point(p.Value.At),
            "unknown_geometric_connection",sections[p.Key.Item1].MaxLevel>p.Value.Pass,sections[p.Key.Item2].MaxLevel>p.Value.Pass)).ToArray();
        var sourceReport=sources.OrderBy(s=>s.Id).Select(s=>new RiverSource(s,associations[s.Id],Liquid(s.Contamination,s.Contamination),
            s.ActiveInHazardousWeather is null ? "native_hazard_activation_rule_unavailable" :
                lastWeather is null || lastWeather.SelectedHazard=="unknown" ? "selected_weather_unknown" :
                s.ActiveInHazardousWeather.Contains(lastWeather.SelectedHazard) ? "selected_hazard_allowed_by_activation_rule_other_modifiers_may_block" :
                "selected_hazard_excluded_by_activation_rule",
            "unknown_reobserve_at_weather_transition")).ToArray();
        return new("geometric_river_overview",sections.Length,edges.Length,sourceReport.Length,sections,edges,sourceReport,sourcesComplete,
            firstWeather,lastWeather,changed,0,0,0,false,false,false,limits);
    }
    public static RiverOverview Page(RiverOverview report,int sectionOffset,int linkOffset,int sourceOffset)=>report with {
        Sections=report.Sections.Skip(sectionOffset).Take(32).ToArray(),Links=report.Links.Skip(linkOffset).Take(64).ToArray(),Sources=report.Sources.Skip(sourceOffset).Take(16).ToArray(),
        SectionOffset=sectionOffset,LinkOffset=linkOffset,SourceOffset=sourceOffset,
        MoreSections=sectionOffset+32<report.TotalSections,MoreLinks=linkOffset+64<report.TotalLinks,MoreSources=sourceOffset+16<report.TotalSources};
    public static RiverOverview Compare(RiverOverview current,RiverOverview previous)
    {
        var old=previous.Sources.ToDictionary(s=>s.Native.Id);
        bool? changed=current.LastWeather is {} a && previous.LastWeather is {} b ?
            a.Cycle!=b.Cycle || a.Hazardous!=b.Hazardous || a.SelectedHazard!=b.SelectedHazard : null;
        return current with {ComparisonState="observed_change_not_causal_weather_proof",ComparisonWeatherChanged=changed,
            Sources=current.Sources.Select(s=>old.TryGetValue(s.Native.Id,out var before) ? s with {
                StrengthChange=s.Native.CurrentStrength-before.Native.CurrentStrength,
                ContaminationChange=s.Native.Contamination-before.Native.Contamination}:s).ToArray()};
    }
}
