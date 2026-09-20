using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NeedSummary(string Id,string DisplayName,int Observed,int Enabled,int Active,int Warning,int Critical,int Unfavorable,float? MinimumPoints,float? MaximumPoints,double? AveragePoints);
public sealed record NativeNeeds(string Scope,int Beavers,int ObservedBeavers,int MissingNeedManagers,int Offset,int Limit,int Total,NeedSummary[] Items,bool HasMore,string[] Limitations);
public sealed record NeedDetail(string Id,string DisplayName,float Points,float Minimum,float Maximum,bool Enabled,bool Active,bool CriticalNeed,bool Critical,bool Warning,bool Favorable);
public sealed record NeedWorldPosition(float X,float Y,float Z);
public sealed record NativeBeaverNeeds(string Id,bool Supported,NeedWorldPosition WorldPosition,int Offset,int Limit,int Total,NeedDetail[] Items,bool HasMore,string[] Limitations);
public sealed record OperationWorkplace(int Assigned,int Desired,int Maximum,bool Understaffed,bool AnyJobRunning,bool? WorkingHours);
public sealed record OperationManufacturing(bool HasRecipe,string? Recipe,bool Ready,bool? HasIngredients,bool? HasFuel,bool? ConsumesFuel,bool? OutputSpace,float ProductionProgress);
public sealed record NativeOperation(string Id,string Template,bool Finished,bool? Paused,OperationWorkplace? Workplace,OperationManufacturing? Manufacturing,string[] Statuses,string[] Limitations);

public sealed partial class NativeClient
{
    private static void DiagnosticsEnvelope<T>(BridgeEnvelope<T> e,DiagnosticsRequest r,string[]? limits)
    {
        if(e.BridgeVersion!="0.20.0"||r.Session.Length>0&&e.SessionId!=r.Session||limits is null||limits.Any(s=>s is null))throw new InvalidDataException("Invalid diagnostics envelope");
    }
    private static bool DiagnosticPage(DiagnosticsRequest r,int offset,int limit,int total,int count,bool more)=>offset==r.Offset&&limit==r.Limit&&total>=0&&count==Math.Min(limit,Math.Max(0,total-offset))&&more==((long)offset+count<total);
    private static bool SortedNeedIds(IEnumerable<string> ids)=>ids.All(BuildingPolicy.ValidTemplate)&&ids.SequenceEqual(ids.Distinct().Order(StringComparer.Ordinal));
    public async Task<BridgeEnvelope<NativeNeeds>> Needs(DiagnosticsRequest r,CancellationToken ct)
    {
        if(r.Route!="needs")throw new ArgumentException();
        var e=await Get<NativeNeeds>($"needs?offset={r.Offset}&limit={r.Limit}",ct);var d=e.Data;DiagnosticsEnvelope(e,r,d.Limitations);
        if(d.Scope!="beavers_with_need_manager"||d.Beavers<0||d.ObservedBeavers<0||d.MissingNeedManagers<0||(long)d.ObservedBeavers+d.MissingNeedManagers!=d.Beavers||d.Items is null||
            !DiagnosticPage(r,d.Offset,d.Limit,d.Total,d.Items.Length,d.HasMore)||d.Items.Any(n=>n is null)||!SortedNeedIds(d.Items.Select(n=>n.Id)))throw new InvalidDataException("Invalid need overview");
        foreach(var n in d.Items) {
            if(n.DisplayName is null||n.DisplayName.Length>512||n.Observed<1||n.Observed>d.ObservedBeavers||n.Enabled<0||n.Enabled>n.Observed||new[]{n.Active,n.Warning,n.Critical,n.Unfavorable}.Any(v=>v<0||v>n.Enabled)||
                (n.Enabled>0)!=(n.MinimumPoints is not null)||(n.Enabled>0)!=(n.MaximumPoints is not null)||(n.Enabled>0)!=(n.AveragePoints is not null)||
                n.Enabled>0&&(!float.IsFinite(n.MinimumPoints!.Value)||!float.IsFinite(n.MaximumPoints!.Value)||!double.IsFinite(n.AveragePoints!.Value)||n.MinimumPoints>n.MaximumPoints||n.AveragePoints<n.MinimumPoints||n.AveragePoints>n.MaximumPoints))throw new InvalidDataException("Invalid need counts");
        }
        return e;
    }
    public async Task<BridgeEnvelope<NativeBeaverNeeds>> BeaverNeeds(DiagnosticsRequest r,CancellationToken ct)
    {
        if(r.Route!="beaver-needs")throw new ArgumentException();
        var e=await Get<NativeBeaverNeeds>($"beaver-needs?id={r.Id}&session={r.Session}&offset={r.Offset}&limit={r.Limit}",ct);var d=e.Data;DiagnosticsEnvelope(e,r,d.Limitations);
        if(d.Id!=r.Id||d.WorldPosition is null||!float.IsFinite(d.WorldPosition.X)||!float.IsFinite(d.WorldPosition.Y)||!float.IsFinite(d.WorldPosition.Z)||d.Items is null||
            !DiagnosticPage(r,d.Offset,d.Limit,d.Total,d.Items.Length,d.HasMore)||!d.Supported&&d.Total!=0||d.Items.Any(n=>n is null)||!SortedNeedIds(d.Items.Select(n=>n.Id))||
            d.Items.Any(n=>n.DisplayName is null||n.DisplayName.Length>512||!float.IsFinite(n.Points)||!float.IsFinite(n.Minimum)||!float.IsFinite(n.Maximum)||n.Minimum>n.Maximum||n.Points<n.Minimum||n.Points>n.Maximum))throw new InvalidDataException("Invalid beaver needs");
        return e;
    }
    public async Task<BridgeEnvelope<NativeOperation>> BuildingOperation(DiagnosticsRequest r,CancellationToken ct)
    {
        if(r.Route!="building-operation")throw new ArgumentException();
        var e=await Get<NativeOperation>($"building-operation?id={r.Id}&session={r.Session}",ct);var d=e.Data;DiagnosticsEnvelope(e,r,d.Limitations);
        if(d.Id!=r.Id||!BuildingPolicy.ValidTemplate(d.Template)||!d.Finished&&(d.Workplace is not null||d.Manufacturing is not null)||d.Statuses is null||d.Statuses.Length>32||d.Statuses.Any(s=>s is null||s.Length>512))throw new InvalidDataException("Invalid operation");
        if(d.Workplace is {} w&&(w.Assigned<0||w.Desired<0||w.Maximum<0||w.Desired>w.Maximum||w.Assigned==0&&w.AnyJobRunning))throw new InvalidDataException("Invalid workplace observation");
        if(d.Manufacturing is {} m&&(!float.IsFinite(m.ProductionProgress)||m.ProductionProgress<0||m.HasRecipe!=(m.Recipe is not null)||m.HasRecipe&&!BuildingPolicy.ValidTemplate(m.Recipe!)||
            m.HasRecipe!=(m.HasIngredients is not null)||m.HasRecipe!=(m.HasFuel is not null)||m.HasRecipe!=(m.ConsumesFuel is not null)||m.HasRecipe!=(m.OutputSpace is not null)))throw new InvalidDataException("Invalid manufacturing observation");
        return e;
    }
}
