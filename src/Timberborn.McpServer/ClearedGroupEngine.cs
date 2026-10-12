using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record ClearedGroupSpec(string Session, string JobId, string DistrictId, int X, int Y, int Z,
    int Width, int Height, string[] Templates, ReservedArea[] ReservedAreas,
    int ClearanceHours = 48, int ConstructionHours = 120, int MaxRealSeconds = 600,
    int Speed = 7, int SearchBudget = 16, int MaxCandidates = 3);
public sealed record ClearanceDesignation(string Kind, Position Position, string Resource, int Width=1, int Height=1);
public sealed record ClearanceObservation(NativeRemovalTarget[] Objects, ClearanceDesignation[] Designations);
public sealed class ClearedGroupJob
{
    public required ClearedGroupSpec Spec { get; init; }
    public required string ClearanceKey { get; init; }
    public string State { get; set; } = "accepted";
    public string Reason { get; set; } = "starting";
    public bool Terminal { get; set; }
    public string? PendingAction { get; set; }
    public string RunId { get; set; } = Guid.NewGuid().ToString("D");
    public string ActionId { get; set; } = Guid.NewGuid().ToString("D");
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public int RemovedDesignations { get; set; }
    public int MarkedTargets { get; set; }
    public int RemainingTargets { get; set; }
    public bool PauseConfirmed { get; set; }
    public bool StopRequested { get; set; }
    public ClearanceObservation? Observation { get; set; }
    public JsonObject? Plan { get; set; }
    public JsonObject? Construction { get; set; }
    public NativeSimulationRun? ClearanceRun { get; set; }
}

public static class ClearedGroupPolicy
{
    public static void Validate(ClearedGroupSpec s)
    {
        BuildBatchState.Id(s.Session); BuildBatchState.Id(s.JobId); BuildBatchState.Id(s.DistrictId);
        if(s.X is <0 or >4095 || s.Y is <0 or >4095 || s.Z is <0 or >4092 || s.Width is <1 or >8 || s.Height is <1 or >8 ||
            s.X+s.Width>4096 || s.Y+s.Height>4096 || s.Templates is null || s.Templates.Length is <1 or >8 ||
            s.Templates.Any(t=>!BuildingPolicy.ValidTemplate(t)) || s.ReservedAreas is null ||
            s.ClearanceHours is <1 or >672 || s.ConstructionHours is <1 or >672 || s.ClearanceHours+s.ConstructionHours>672 ||
            s.MaxRealSeconds is <60 or >3600 || s.Speed is not (1 or 3 or 7) || s.SearchBudget is <1 or >32 || s.MaxCandidates is <1 or >4)
            throw new ArgumentException("invalid_cleared_group_spec");
        ReservedArea.Validate(s.ReservedAreas);
    }
    // Clearing conservatively protects the entire XY column of every reserved volume.
    public static bool InClearance(ClearedGroupSpec s, Position p) => p.X>=s.X && p.X<s.X+s.Width &&
        p.Y>=s.Y && p.Y<s.Y+s.Height && p.Z==s.Z && !s.ReservedAreas.Any(r=>p.X>=r.X && p.X<r.X+r.Width && p.Y>=r.Y && p.Y<r.Y+r.Height);
    public static NativeRemovalTarget[] Targets(ClearedGroupSpec s, ClearanceObservation o) => o.Objects
        .Where(t=>t.Kind is "planted" or "vegetation")
        .Where(t=>InClearance(s,t.Position) && !(t.OverlapCells ?? []).Any(p=>s.ReservedAreas.Any(r=>p.X>=r.X && p.X<r.X+r.Width && p.Y>=r.Y && p.Y<r.Y+r.Height)))
        .OrderBy(t=>t.Id).ToArray();
    public static ClearanceDesignation[] Designations(ClearedGroupSpec s, ClearanceObservation o) =>
        o.Designations.Where(d=>InClearance(s,d.Position)).OrderBy(d=>d.Kind).ThenBy(d=>d.Position.Y).ThenBy(d=>d.Position.X).ToArray();
    public static ClearanceDesignation[] DesignationBatches(ClearedGroupSpec s, ClearanceObservation o) {
        var remaining=Designations(s,o).ToDictionary(d=>d.Position);var result=new List<ClearanceDesignation>();
        while(remaining.Count>0) {
            var first=remaining.Values.OrderBy(d=>d.Position.Y).ThenBy(d=>d.Position.X).First();var p=first.Position;
            bool Same(int x,int y)=>remaining.TryGetValue(new(x,y,p.Z),out var d) && d.Kind==first.Kind && d.Resource==first.Resource;
            int width=1,height=1;
            while(width<4 && Same(p.X+width,p.Y))width++;
            while(height<4 && Enumerable.Range(0,width).All(x=>Same(p.X+x,p.Y+height)))height++;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)remaining.Remove(new(p.X+x,p.Y+y,p.Z));
            result.Add(first with{Width=width,Height=height});
        }
        return result.ToArray();
    }
    public static string Key(ClearedGroupSpec s, ClearanceObservation o) => Convert.ToHexString(SHA256.HashData(
        JsonSerializer.SerializeToUtf8Bytes(new {spec=s, targets=Targets(s,o).Select(t=>new{t.Id,t.Kind,t.Template,t.Position,t.Mode,t.CanDelete,t.Marked}),designations=Designations(s,o)},NativeJson.Options))).ToLowerInvariant();
    public static string? Blocker(ClearedGroupSpec s, ClearanceObservation o) => Targets(s,o).Length>64 ? "clearance_target_limit" :
        Targets(s,o).Any(t=>!t.CanDelete || t.Mode!="demolition_mark") ? "vegetation_not_removable" : null;
    public static JsonElement PlanningArgs(ClearedGroupSpec s) => JsonSerializer.SerializeToElement(new {
        session=s.Session,districtId=s.DistrictId,x=s.X,y=s.Y,z=s.Z,width=s.Width,height=s.Height,
        templates=s.Templates,reservedAreas=s.ReservedAreas,searchBudget=s.SearchBudget,maxCandidates=s.MaxCandidates,
        scanOtherHeights=false,maxWindows=1,planBuildings=true},NativeJson.Options);
}

public interface IClearedGroupPort
{
    Task RequirePaused(ClearedGroupSpec s, CancellationToken ct);
    Task<ClearanceObservation> Observe(ClearedGroupSpec s, bool areas, CancellationToken ct);
    Task RemoveDesignation(ClearedGroupSpec s, ClearanceDesignation d, CancellationToken ct);
    Task Mark(ClearedGroupSpec s, NativeRemovalTarget t, CancellationToken ct);
    Task<NativeSimulationRun> Clearance(ClearedGroupJob j, string operation, int seconds, CancellationToken ct);
    Task<JsonObject> Plan(ClearedGroupSpec s, CancellationToken ct);
    Task<JsonObject> Build(ClearedGroupJob j, string operation, int seconds, CancellationToken ct);
}

// One async dispatcher per MCP backend. Native simulation/build runners retain their own hard budgets.
public sealed class ClearedGroupEngine(IClearedGroupPort port, Action<ClearedGroupJob> save,
    Func<bool> stopping, Func<TimeSpan,CancellationToken,Task>? delay = null)
{
    public async Task Run(ClearedGroupJob j, CancellationToken ct)
    {
        var watch=System.Diagnostics.Stopwatch.StartNew();
        int Remaining() => Math.Max(0,j.Spec.MaxRealSeconds-(int)Math.Ceiling(watch.Elapsed.TotalSeconds));
        async Task Wait() { if(delay is null)await Task.Delay(TimeSpan.FromSeconds(2),ct);else await delay(TimeSpan.FromSeconds(2),ct); }
        void State(string state,string reason) { j.State=state;j.Reason=reason;save(j); }
        void Pending(string action) { ct.ThrowIfCancellationRequested(); if(stopping())throw new OperationCanceledException(); j.PendingAction=action;save(j); }
        void Applied() { j.PendingAction=null;save(j); }
        void End(string state,string reason) { j.State=state;j.Reason=reason;j.Terminal=true;save(j); }
        try {
            await port.RequirePaused(j.Spec,ct);j.PauseConfirmed=true;
            var first=await port.Observe(j.Spec,true,ct);j.Observation=first;
            if(ClearedGroupPolicy.Key(j.Spec,first)!=j.ClearanceKey) {End("blocked","clearance_changed_replan");return;}
            if(ClearedGroupPolicy.Blocker(j.Spec,first) is {} blocker) {End("blocked",blocker);return;}
            State("clearing","remove_planting_designations");
            foreach(var d in ClearedGroupPolicy.DesignationBatches(j.Spec,first)) {
                Pending("remove_designation");await port.RemoveDesignation(j.Spec,d,ct);j.RemovedDesignations+=d.Width*d.Height;Applied();
            }
            // Classification changes from planted to vegetation after designation removal.
            var fresh=await port.Observe(j.Spec,true,ct);j.Observation=fresh;
            var expected=ClearedGroupPolicy.Targets(j.Spec,first);var targets=ClearedGroupPolicy.Targets(j.Spec,fresh);
            if(ClearedGroupPolicy.Designations(j.Spec,fresh).Length!=0 || targets.Length!=expected.Length ||
                targets.Any(t=>!expected.Any(e=>e.Id==t.Id && e.Position==t.Position && e.Template==t.Template)) ||
                ClearedGroupPolicy.Blocker(j.Spec,fresh) is not null) {End("blocked","area_changed_before_marking");return;}
            foreach(var target in targets) {
                if(target.Marked)continue;
                Pending("mark_vegetation");await port.Mark(j.Spec,target,ct);j.MarkedTargets++;Applied();
            }
            var observed=await port.Observe(j.Spec,false,ct);
            j.RemainingTargets=ClearedGroupPolicy.Targets(j.Spec,observed).Length;
            if(j.RemainingTargets>0) {
                if(Remaining()<30) {End("blocked","real_time_limit_before_clearance");return;}
                j.PauseConfirmed=false;Pending("start_clearance_run");j.ClearanceRun=await port.Clearance(j,"start",Remaining(),ct);Applied();
                State("clearing","waiting_for_workers_with_work_schedule");j.PauseConfirmed=false;
                bool cancelSent=false;
                while(true) {
                    ct.ThrowIfCancellationRequested();
                    j.ClearanceRun=await port.Clearance(j,"inspect",0,ct);j.PauseConfirmed=j.ClearanceRun.PauseConfirmed;
                    if(j.ClearanceRun.Terminal && (j.ClearanceRun.State is not ("completed" or "cancelled") || j.ClearanceRun.State=="cancelled" && !cancelSent)) {End("blocked",j.ClearanceRun.Reason);return;}
                    observed=await port.Observe(j.Spec,false,ct);j.Observation=observed;
                    var remaining=ClearedGroupPolicy.Targets(j.Spec,observed);j.RemainingTargets=remaining.Length;
                    bool changed=remaining.Any(t=>!targets.Any(e=>e.Id==t.Id && e.Template==t.Template && e.Position==t.Position) || !t.Marked);
                    bool stop=stopping() || Remaining()==0 || changed;
                    if(!j.ClearanceRun.Terminal && !cancelSent && (remaining.Length==0 || stop)) {
                        // This is a distinct cancellation, never a replay of the start/mark requests.
                        j.PendingAction="pause_clearance_run";save(j);
                        j.ClearanceRun=await port.Clearance(j,"cancel",0,ct);cancelSent=true;Applied();
                    }
                    save(j);
                    if(j.ClearanceRun.Terminal) {
                        j.PauseConfirmed=j.ClearanceRun.PauseConfirmed;
                        if(!j.PauseConfirmed || stop || remaining.Length!=0) {
                            End(stopping()?"stopped":"blocked",changed?"clearance_intervened":remaining.Length!=0?"clearance_budget_exhausted_check_workers_access_and_work_hours":"clearance_stopped");return;
                        }
                        break;
                    }
                    if(Remaining()==0 && cancelSent) {End("unconfirmed","pause_pending_inspect_native_run");return;}
                    await Wait();
                }
            }
            if(stopping()) {End("stopped","requested_before_group_plan");return;}
            await port.RequirePaused(j.Spec,ct);j.PauseConfirmed=true;
            fresh=await port.Observe(j.Spec,true,ct);j.Observation=fresh;
            if(ClearedGroupPolicy.Targets(j.Spec,fresh).Length!=0 || ClearedGroupPolicy.Designations(j.Spec,fresh).Length!=0) {End("blocked","clearance_changed_before_plan");return;}
            State("planning","fresh_joint_native_preview");j.Plan=await port.Plan(j.Spec,ct);save(j);
            if(j.Plan["state"]?.GetValue<string>()!="complete") {End("blocked","incomplete_group_plan_no_build_orders");return;}
            if(Remaining()<30) {End("blocked","real_time_limit_before_build");return;}
            j.PauseConfirmed=false;Pending("start_group_build");j.Construction=await port.Build(j,"run",Remaining(),ct);Applied();
            State("building","native_autonomous_batch");j.PauseConfirmed=false;
            bool buildStopSent=false;
            while(true) {
                ct.ThrowIfCancellationRequested();
                j.Construction=await port.Build(j,"inspect",0,ct);
                var state=j.Construction["state"]?.GetValue<string>();
                j.PauseConfirmed=j.Construction["simulation"]?["pauseConfirmed"]?.GetValue<bool>()==true;save(j);
                if(state=="completed") {
                    await port.RequirePaused(j.Spec,ct);j.PauseConfirmed=true;
                    Pending("close_group_build");j.Construction=await port.Build(j,"close",0,ct);Applied();
                    if(j.Construction["released"]?.GetValue<bool>()!=true) {End("blocked","project_close_not_confirmed");return;}
                    End("completed","group_finished_access_checked_and_released");return;
                }
                if(state is "stopped" or "unconfirmed" or "blocked" or "interrupted") {End(state,j.Construction["reason"]?.GetValue<string>()??"native_project_stopped_inspect_diagnosis");return;}
                if(!buildStopSent && (stopping() || Remaining()==0)) {
                    j.PendingAction="stop_group_build";save(j);j.Construction=await port.Build(j,"stop",0,ct);buildStopSent=true;Applied();
                }
                if(Remaining()==0 && buildStopSent) {End("unconfirmed","build_stop_pending_inspect_native_project");return;}
                await Wait();
            }
        } catch(Exception ex) when(ex is not OutOfMemoryException) {
            // Preserve the write-ahead action and native IDs. Never resend an uncertain action.
            j.PauseConfirmed=false;
            End(j.PendingAction is not null?"unconfirmed":stopping()?"stopped":"blocked",
                ex is BridgeRejectionException b?b.Code:ex is OperationCanceledException?"dispatcher_stopped_native_budget_remains":"dispatch_failed_inspect_native_ids");
        }
    }
}
