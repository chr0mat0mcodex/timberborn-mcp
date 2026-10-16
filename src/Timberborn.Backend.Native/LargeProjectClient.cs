using System.Text.Json;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<JsonElement>> LargeProject(LargeProjectRequest r,CancellationToken ct)
    {
        string route=r.Operation=="power-network"?r.Operation:"large-project-"+r.Operation;
        string query="session="+r.Session;
        if(r.Operation=="power-network")query+="&id="+r.EntityId;
        else if(r.Operation is "plan-access" or "plan" or "start" or "run" or "plan-repair" or "run-repair")
        {
            query+="&districtId="+r.DistrictId+"&steps="+Uri.EscapeDataString(LargeProjectRequest.Encode(r.Steps));
            if(r.Operation is "start" or "run" or "run-repair")query+="&actionId="+r.ActionId+"&planKey="+r.PlanKey+"&mode=development_pilot";
            if(r.Operation is "run" or "run-repair")query+="&speed="+r.Speed+"&maxGameHours="+r.MaxGameHours+"&maxRealSeconds="+r.MaxRealSeconds;
        }
        else query+="&actionId="+r.ActionId;
        if(r.ReservedAreas.Length>0)query+="&reservedAreas="+Uri.EscapeDataString(ReservedArea.Encode(r.ReservedAreas));
        if(r.Operation is "plan-repair" or "run-repair")query+="&parentActionId="+r.ParentActionId;
        var e=await Get<JsonElement>(route+"?"+query,ct,LargeProjectRequest.Writes(r.Operation)?HttpMethod.Post:HttpMethod.Get,maxResponseBytes:LargeProjectRequest.MaxResponseBytes);
        ValidateLargeProject(e,r);return e;
    }
    public static void ValidateLargeProject(BridgeEnvelope<JsonElement> e,LargeProjectRequest r)
    {
        try
        {
            if(e.BridgeVersion is not ("0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0" or "0.59.0" or "0.60.0" or "0.61.0")||e.SessionId!=r.Session||e.Data.ValueKind!=JsonValueKind.Object)throw new InvalidDataException();
            var d=e.Data;
            var limits=d.GetProperty("limitations");
            if(limits.ValueKind!=JsonValueKind.Array||limits.GetArrayLength()>16||limits.EnumerateArray().Any(v=>v.ValueKind!=JsonValueKind.String||v.GetString() is not {Length:>0 and <=160}))throw new InvalidDataException();
            if(r.Operation=="plan-access") { ValidateAccessPlan(e,r); return; }
            if(r.Operation is "plan-repair" or "run-repair") { if(d.GetProperty("parentActionId").GetString()!=r.ParentActionId)throw new InvalidDataException(); }
            if(r.Operation=="power-network")
            {
                if(d.GetProperty("id").GetString()!=r.EntityId||!BuildingPolicy.ValidTemplate(d.GetProperty("template").GetString()))throw new InvalidDataException();
                CheckPorts(d.GetProperty("ports"));
                CheckVector(d.GetProperty("position"));
                _=d.GetProperty("finished").GetBoolean();
                bool supported=d.GetProperty("supported").GetBoolean();
                foreach(var key in new[]{"active","powered"}) {var value=d.GetProperty(key);if(supported)_=value.GetBoolean();else if(value.ValueKind!=JsonValueKind.Null)throw new InvalidDataException();}
                foreach(var key in new[]{"actualInput","actualOutput"}) {var value=d.GetProperty(key);if(value.ValueKind!=JsonValueKind.Null&&(!supported||value.GetInt32()<0))throw new InvalidDataException();}
                var network=d.GetProperty("network");
                if(!supported&&(d.GetProperty("ports").GetArrayLength()!=0||network.ValueKind!=JsonValueKind.Null))throw new InvalidDataException();
                if(network.ValueKind!=JsonValueKind.Null)
                {
                    var members=network.GetProperty("members").EnumerateArray().Select(v=>v.GetString()).ToArray();
                    if(members.Length is <1 or >512||members.Distinct().Count()!=members.Length||members.Any(s=>!Guid.TryParseExact(s,"D",out var id)||id==Guid.Empty)||!members.Contains(r.EntityId)||!members.Contains(network.GetProperty("anchorId").GetString()))throw new InvalidDataException();
                    foreach(var key in new[]{"powerSupply","powerDemand","batteryCharge","batteryCapacity"})if(network.GetProperty(key).GetInt32()<0)throw new InvalidDataException();
                    _=network.GetProperty("powerSurplus").GetInt32();_=network.GetProperty("powered").GetBoolean();
                }
                return;
            }
            if(r.Operation is "diagnose" or "close") { ValidateRecovery(d,r); return; }
            if(d.GetProperty("regularExecutionAllowed").GetBoolean())throw new InvalidDataException();
            var order=d.GetProperty("order").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
            if(order.Length is <1 or >32||!order.Order().SequenceEqual(Enumerable.Range(0,order.Length)))throw new InvalidDataException();
            if(r.Operation is "plan" or "plan-repair")
            {
                if(!ValidPlanKey(d.GetProperty("planKey").GetString()??"")||!order.SequenceEqual(LargeProjectRequest.Order(r.Steps)))throw new InvalidDataException();
                var parts=d.GetProperty("parts").EnumerateArray().ToArray();
                if(parts.Length!=r.Steps.Length||d.GetProperty("powerLinks").GetArrayLength()>512)throw new InvalidDataException();
                // Same release can reconnect to an older installed package. Its absent policy is
                // legacy evidence, never upgraded to a construction-work-cell proof.
                if(d.TryGetProperty("accessPolicyVersion",out var policy))
                {
                    if(policy.GetInt32()!=1)throw new InvalidDataException();
                    foreach(var part in parts) ConstructionWorkAccessContract.Validate(part.GetProperty("constructionWorkAccess"),d.GetProperty("pilotEligible").GetBoolean());
                }
                var safety=d.GetProperty("roadProtection").Deserialize<NativeRoadProtection>(NativeJson.Options);
                RoadProtectionContract.Validate(safety,false,requireBaseline:true,requireProbeKinds:true,requireConstructionPreview:true,maxCandidateCells:BuildingPolicy.MaxProjectGeometryCells);
                foreach(var link in d.GetProperty("powerLinks").EnumerateArray()) {
                    int from=link.GetProperty("fromStep").GetInt32();if(from<0||from>=parts.Length)throw new InvalidDataException();
                    CheckVector(link.GetProperty("from"));CheckVector(link.GetProperty("to"));_=link.GetProperty("rotationMatches").GetBoolean();
                    string? kind=link.GetProperty("kind").GetString();
                    if(kind=="planned_geometry_not_live_network") {int to=link.GetProperty("toStep").GetInt32();if(to<0||to>=parts.Length||to==from)throw new InvalidDataException();}
                    else if(kind!="existing_geometry_not_live_connection"||!Guid.TryParseExact(link.GetProperty("toEntityId").GetString(),"D",out var id)||id==Guid.Empty)throw new InvalidDataException();
                }
                for(int i=0;i<parts.Length;i++)
                {
                    var p=parts[i];var expected=r.Steps[i];var pos=p.GetProperty("position");
                    if(p.GetProperty("index").GetInt32()!=i||p.GetProperty("template").GetString()!=expected.Template||p.GetProperty("rotation").GetInt32()!=expected.Rotation||pos.GetProperty("x").GetInt32()!=expected.X||pos.GetProperty("y").GetInt32()!=expected.Y||pos.GetProperty("z").GetInt32()!=expected.Z||!p.GetProperty("dependsOn").EnumerateArray().Select(v=>v.GetInt32()).SequenceEqual(expected.DependsOn)||p.GetProperty("constructionAccess").GetString()!="unknown")throw new InvalidDataException();
                    CheckPorts(p.GetProperty("powerPorts"));
                    bool valid=p.GetProperty("placementValid").GetBoolean();
                    string? validation=p.GetProperty("placementState").GetString();
                    if(validation is not ("native_valid" or "native_invalid" or "deferred_native_validation")||valid!=(validation=="native_valid")||validation=="deferred_native_validation"&&expected.DependsOn.Length==0)throw new InvalidDataException();
                }
                if(d.GetProperty("pilotEligible").GetBoolean()&&(!d.GetProperty("restored").GetBoolean()||d.GetProperty("sessionLocked").GetBoolean()||safety is null||!safety.Restored||safety.LostConnections!=0||safety.Status=="blocked"||parts.Any(p=>p.GetProperty("placementState").GetString()=="native_invalid"||p.GetProperty("lostConnections").GetInt32()!=0||p.GetProperty("previewEntranceConnected").ValueKind==JsonValueKind.False)))throw new InvalidDataException();
            }
            else
            {
                if(d.TryGetProperty("executionMode",out var mode)&&mode.GetString()=="batch") { ValidateBatchReceipt(d,r,order); return; }
                if(r.Operation is "run" or "run-repair")throw new InvalidDataException();
                if(d.GetProperty("actionId").GetString()!=r.ActionId||!ValidPlanKey(d.GetProperty("planKey").GetString()??"")||r.Operation=="start"&&d.GetProperty("planKey").GetString()!=r.PlanKey)throw new InvalidDataException();
                var state=d.GetProperty("state").GetString();var ids=d.GetProperty("entityIds").EnumerateArray().Select(v=>v.GetString()).ToArray();var states=d.GetProperty("partStates").EnumerateArray().Select(v=>v.GetString()).ToArray();
                if(state is not ("ready" or "waiting" or "completed" or "stopped" or "unconfirmed")||ids.Length!=order.Length||states.Length!=order.Length||ids.Distinct().Count()!=ids.Length||ids.Any(s=>!Guid.TryParseExact(s,"D",out var id)||id==Guid.Empty)||states.Any(s=>s is not ("pending" or "unconfirmed" or "construction" or "finished")))throw new InvalidDataException();
                int current=d.GetProperty("current").GetInt32();
                if(current<0||current>order.Length||order.Take(current).Any(i=>states[i]!="finished")||order.Skip(current+1).Any(i=>states[i]!="pending")||state=="completed"&&(current!=order.Length||states.Any(s=>s!="finished")))throw new InvalidDataException();
                if(current==order.Length&&state!="completed"||state=="ready"&&(current!=0||states.Any(s=>s!="pending"))||state=="waiting"&&states[order[current]] is not ("unconfirmed" or "construction"))throw new InvalidDataException();
                if(r.Operation=="start"&&!order.SequenceEqual(LargeProjectRequest.Order(r.Steps)))throw new InvalidDataException();
            }
        }
        catch(Exception ex) when(ex is KeyNotFoundException or InvalidOperationException or FormatException or OverflowException or InvalidDataException or JsonException)
        {throw new InvalidDataException("Invalid 0.37 project/power response",ex);}
    }
    private static void ValidateBatchReceipt(JsonElement d,LargeProjectRequest r,int[] order)
    {
        if(r.Operation is not ("run" or "run-repair" or "inspect" or "stop")||d.GetProperty("actionId").GetString()!=r.ActionId||!ValidPlanKey(d.GetProperty("planKey").GetString()??""))throw new InvalidDataException();
        var state=d.GetProperty("state").GetString();
        if(state is not ("running" or "pausing" or "completed" or "blocked" or "stopped" or "unconfirmed" or "interrupted")||d.GetProperty("reason").GetString() is not {Length:>0 and <=160})throw new InvalidDataException();
        var ids=d.GetProperty("entityIds").EnumerateArray().Select(v=>v.GetString()).ToArray();
        var states=d.GetProperty("partStates").EnumerateArray().Select(v=>v.GetString()).ToArray();
        if(ids.Length!=order.Length||states.Length!=order.Length||ids.Distinct().Count()!=ids.Length||ids.Any(s=>!Guid.TryParseExact(s,"D",out var id)||id==Guid.Empty)||states.Any(s=>s is not ("pending" or "unconfirmed" or "construction" or "finished")))throw new InvalidDataException();
        if(d.GetProperty("ordered").GetInt32()!=states.Count(s=>s!="pending")||d.GetProperty("finished").GetInt32()!=states.Count(s=>s=="finished"))throw new InvalidDataException();
        var deferred=d.GetProperty("deferredParts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
        if(deferred.Length>order.Length||deferred.Distinct().Count()!=deferred.Length||deferred.Any(i=>i<0||i>=order.Length||states[i]!="pending"))throw new InvalidDataException();
        var blocked=d.GetProperty("blockedPart");
        if(blocked.ValueKind!=JsonValueKind.Null&&(blocked.GetInt32()<0||blocked.GetInt32()>=order.Length))throw new InvalidDataException();
        var sim=d.GetProperty("simulation").Deserialize<SimulationRunStatus>(NativeJson.Options)??throw new InvalidDataException();
        if(sim.RunId!=r.ActionId||sim.RequestedSpeed is not (1 or 3 or 7)||sim.MaxRealSeconds is <30 or >86400||sim.State is not ("starting" or "running" or "pausing" or "completed" or "cancelled" or "interrupted" or "failed"))throw new InvalidDataException();
        foreach(double value in new[]{sim.StartGameHours,sim.TargetGameHours,sim.ObservedGameHours,sim.ElapsedGameHours,sim.ElapsedRealSeconds,(double)sim.ObservedSpeed})if(!double.IsFinite(value)||value<0)throw new InvalidDataException();
        if(sim.TargetGameHours<=sim.StartGameHours||sim.TargetGameHours-sim.StartGameHours>672||sim.PauseConfirmed!=(sim.ObservedSpeed==0)||sim.Terminal!=(sim.State is "completed" or "cancelled" or "interrupted" or "failed"))throw new InvalidDataException();
        if(state is "completed" or "blocked" or "stopped" or "unconfirmed" or "interrupted") { if(!sim.Terminal)throw new InvalidDataException(); }
        if(state=="completed"&&(states.Any(s=>s!="finished")||!sim.PauseConfirmed||sim.State!="cancelled"||blocked.ValueKind!=JsonValueKind.Null))throw new InvalidDataException();
        if(r.Operation is ("run" or "run-repair")&&(d.GetProperty("planKey").GetString()!=r.PlanKey||!order.SequenceEqual(LargeProjectRequest.Order(r.Steps))||sim.RequestedSpeed!=r.Speed||sim.MaxRealSeconds!=r.MaxRealSeconds||sim.TargetGameHours-sim.StartGameHours!=r.MaxGameHours))throw new InvalidDataException();
    }
    private static void ValidateRecovery(JsonElement d, LargeProjectRequest r)
    {
        if(d.GetProperty("actionId").GetString()!=r.ActionId)throw new InvalidDataException();
        string? state=d.GetProperty("state").GetString();
        if(state is not ("ready" or "waiting" or "running" or "pausing" or "completed" or "blocked" or "stopped" or "unconfirmed" or "interrupted"))throw new InvalidDataException();
        bool released=d.GetProperty("released").GetBoolean(), close=d.GetProperty("closeEligible").GetBoolean(), repair=d.GetProperty("repairEligible").GetBoolean();
        if(r.Operation=="close")
        {
            string? outcome=d.GetProperty("closeOutcome").GetString();
            if(outcome is not ("released" or "already_released" or "rejected") || released!=(outcome!="rejected") || outcome=="released"&&!close)throw new InvalidDataException();
        }
        bool busy=d.GetProperty("busy").GetBoolean(), faulted=d.GetProperty("faulted").GetBoolean();
        double speed=d.GetProperty("currentSpeed").GetDouble();
        if(!double.IsFinite(speed)||speed<0 || (close||repair)&&(speed!=0||busy||faulted||state is "ready" or "waiting" or "running" or "pausing") || released&&repair)throw new InvalidDataException();
        var parts=d.GetProperty("parts").EnumerateArray().ToArray();
        if(parts.Length is <1 or >32)throw new InvalidDataException();
        var ids=new HashSet<string>();
        for(int i=0;i<parts.Length;i++)
        {
            var p=parts[i];var id=p.GetProperty("id").GetString();
            if(p.GetProperty("index").GetInt32()!=i||!Guid.TryParseExact(id,"D",out var parsed)||parsed==Guid.Empty||!ids.Add(id!)||!BuildingPolicy.ValidTemplate(p.GetProperty("template").GetString()))throw new InvalidDataException();
            CheckVector(p.GetProperty("position"));
            string recorded=p.GetProperty("recorded").GetString()??"", observed=p.GetProperty("observed").GetString()??"";
            if(recorded is not ("pending" or "unconfirmed" or "construction" or "finished")||observed is not ("missing" or "initializing" or "identity_mismatch" or "construction" or "construction_unreachable" or "construction_access_unknown" or "finished" or "finished_access_missing" or "district_unavailable"))throw new InvalidDataException();
            if(p.GetProperty("reason").GetString() is not {Length:>0 and <=160}||close&&!LargeProjectRecoveryPolicy.Resolved(recorded,observed)||repair&&!LargeProjectRecoveryPolicy.Repairable(recorded,observed))throw new InvalidDataException();
        }
        if(d.GetProperty("unissuedParts").GetInt32()!=parts.Count(p=>p.GetProperty("recorded").GetString()=="pending"))throw new InvalidDataException();
        int lost=d.GetProperty("lostConnections").GetInt32();
        var affected=d.GetProperty("affected").EnumerateArray().ToArray();
        if(lost<0||affected.Length!=Math.Min(lost,32)||d.GetProperty("affectedTruncated").GetBoolean()!=(lost>32))throw new InvalidDataException();
        foreach(var a in affected) {CheckVector(a.GetProperty("position"));if(!a.GetProperty("baselineConnected").GetBoolean()||a.GetProperty("currentConnected").GetBoolean())throw new InvalidDataException();}
        var foreign=d.GetProperty("foreignSites").EnumerateArray().ToArray();
        if(foreign.Length>32||foreign.Any(v=>!Guid.TryParseExact(v.GetString(),"D",out _))||repair&&foreign.Length!=0)throw new InvalidDataException();
        _=d.GetProperty("foreignSitesTruncated").GetBoolean();
    }
    private static void CheckPorts(JsonElement ports)
    {
        if(ports.ValueKind!=JsonValueKind.Array||ports.GetArrayLength()>64)throw new InvalidDataException();
        foreach(var port in ports.EnumerateArray())
        {
            foreach(var field in new[]{"position","target"})CheckVector(port.GetProperty(field));
            _=port.GetProperty("reversed").GetBoolean();_=port.GetProperty("finished").GetBoolean();
            if(port.GetProperty("direction").GetString() is not {Length:>0 and <=32})throw new InvalidDataException();
            bool connected=port.GetProperty("connected").GetBoolean();var id=port.GetProperty("connectedEntityId");
            if(connected!=(id.ValueKind==JsonValueKind.String)||connected&&(!Guid.TryParseExact(id.GetString(),"D",out var value)||value==Guid.Empty))throw new InvalidDataException();
        }
    }
    private static void CheckVector(JsonElement value) {foreach(var axis in new[]{"x","y","z"})if(value.GetProperty(axis).GetInt32() is <-1 or >4096)throw new InvalidDataException();}
}
