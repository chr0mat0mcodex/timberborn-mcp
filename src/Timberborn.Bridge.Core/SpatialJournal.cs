namespace Timberborn.Bridge.Core;

public sealed class SpatialJournalState(string key,string kind,string? id,string? template,int x,int y,int z,string value)
{
    public string Key {get;}=key;
    public string Kind {get;}=kind;
    public string? Id {get;}=id;
    public string? Template {get;}=template;
    public int X {get;}=x; public int Y {get;}=y; public int Z {get;}=z;
    public string Value {get;}=value;
}

// A complete bounded snapshot is validated before baseline or history is changed.
// Missing observations are never interpreted as removals.
public sealed class SpatialJournal
{
    public const int MaxEntries=65536;
    private Dictionary<string,SpatialJournalState>? previous;
    public int Count=>previous?.Count??0;
    public static string PlantState(bool tree,bool? yieldRemoved)=>
        !tree?"plant":yieldRemoved==true?"stump":yieldRemoved==false?"tree":"tree_unknown";
    public void Capture(IEnumerable<SpatialJournalState> observations,RecentActionBuffer buffer,
        DateTimeOffset at,double hours,string source)
    {
        if(!RecentActionsRequest.ValidSource(source)||double.IsNaN(hours)||double.IsInfinity(hours)||hours<0)throw new ArgumentException();
        var next=new Dictionary<string,SpatialJournalState>();
        foreach(var s in observations) {
            if(next.Count>=MaxEntries||s.X<0||s.Y<0||s.Z<0||s.X>4095||s.Y>4095||s.Z>4095||
                s.Kind is not ("plant" or "planting_designation" or "cutting_designation")||
                !BuildingPolicy.ValidTemplate(s.Value)||
                (s.Kind=="plant"? !Guid.TryParseExact(s.Id,"D",out var id)||id==Guid.Empty||!BuildingPolicy.ValidTemplate(s.Template)||s.Value is not ("plant" or "tree" or "stump" or "tree_unknown"):
                    s.Id is not null||s.Template is not null||s.Kind=="cutting_designation"&&s.Value!="marked")||
                next.ContainsKey(s.Key))throw new ArgumentException("spatial_journal_incomplete");
            next.Add(s.Key,s);
        }
        if(previous is not null) {
            foreach(var s in next.Values.Where(s=>!previous.TryGetValue(s.Key,out var old)||s.Value!=old.Value).OrderBy(s=>s.Key,StringComparer.Ordinal)) {
                if(!previous.TryGetValue(s.Key,out var old))Emit(s,s.Kind=="plant"?"plant_added":s.Kind,null,s.Value);
                else if(s.Value!=old.Value)Emit(s,s.Kind=="plant"?"plant_state":s.Kind,old.Value,s.Value);
            }
            foreach(var s in previous.Values.Where(s=>!next.ContainsKey(s.Key)).OrderBy(s=>s.Key,StringComparer.Ordinal))
                Emit(s,s.Kind=="plant"?"plant_removed":s.Kind,s.Value,null);
        }
        previous=next;
        void Emit(SpatialJournalState s,string kind,string? before,string? after)=>
            buffer.Add(at,hours,kind,source,"state_difference",s.Id,s.Template,s.X,s.Y,s.Z,before,after);
    }
}
