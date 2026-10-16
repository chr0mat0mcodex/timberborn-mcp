namespace Timberborn.Bridge.Core;

// References are telemetry, never proof that the referenced project/run exists.
public sealed class ActivityCorrelation(string kind,string id,string source)
{
    public string Kind { get; } = kind;
    public string Id { get; } = id;
    public string Source { get; } = source;
    public static bool Valid(ActivityCorrelation? value) => value is not null &&
        value.Kind is ("project" or "parent_project" or "run") && value.Source is ("request" or "result") &&
        Guid.TryParseExact(value.Id,"D",out var id) && id!=Guid.Empty && value.Id==id.ToString("D");
    public static string Encode(IEnumerable<ActivityCorrelation> values)
    {
        var items=values.ToArray();
        if(items.Length>8 || items.Any(i=>!Valid(i)))throw new ArgumentException("invalid_activity_correlation");
        return string.Join(";",items.Select(i=>i.Kind+":"+i.Id+":"+i.Source));
    }
    public static ActivityCorrelation[] Decode(string text)
    {
        if(text.Length==0)return Array.Empty<ActivityCorrelation>();
        if(text.Length>600)throw new ArgumentException("invalid_activity_correlation");
        var items=text.Split(';').Select(item=> {
            var parts=item.Split(':');
            if(parts.Length!=3)throw new ArgumentException("invalid_activity_correlation");
            return new ActivityCorrelation(parts[0],parts[1],parts[2]);
        }).ToArray();
        Encode(items);
        if(items.Select(i=>i.Kind+":"+i.Id+":"+i.Source).Distinct().Count()!=items.Length)
            throw new ArgumentException("duplicate_activity_correlation");
        return items;
    }
    public static ActivityCorrelation[] Merge(IEnumerable<ActivityCorrelation> first,IEnumerable<ActivityCorrelation> second) =>
        Decode(string.Join(";",first.Concat(second).Select(i=>Encode(new[]{i})).Distinct()));
    public static object Payload(ActivityCorrelation value)=>new {kind=value.Kind,id=value.Id,source=value.Source};
    public static string Display(ActivityCorrelation value)=>
        (value.Kind=="run"?"Lauf":value.Kind=="parent_project"?"Elternprojekt":"Projekt")+": "+value.Id+
        (value.Source=="request"?" (angefragt)":" (im Ergebnis gemeldet)");
}
