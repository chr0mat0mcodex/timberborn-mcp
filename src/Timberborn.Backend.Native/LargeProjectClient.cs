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
        else if(r.Operation is "plan" or "start")
        {
            query+="&districtId="+r.DistrictId+"&steps="+Uri.EscapeDataString(LargeProjectRequest.Encode(r.Steps));
            if(r.Operation=="start")query+="&actionId="+r.ActionId+"&planKey="+r.PlanKey+"&mode=development_pilot";
        }
        else query+="&actionId="+r.ActionId;
        var e=await Get<JsonElement>(route+"?"+query,ct,LargeProjectRequest.Writes(r.Operation)?HttpMethod.Post:HttpMethod.Get,maxResponseBytes:LargeProjectRequest.MaxResponseBytes);
        ValidateLargeProject(e,r);return e;
    }
    public static void ValidateLargeProject(BridgeEnvelope<JsonElement> e,LargeProjectRequest r)
    {
        try
        {
            if(e.BridgeVersion!="0.37.0"||e.SessionId!=r.Session||e.Data.ValueKind!=JsonValueKind.Object)throw new InvalidDataException();
            var d=e.Data;
            var limits=d.GetProperty("limitations");
            if(limits.ValueKind!=JsonValueKind.Array||limits.GetArrayLength()>16||limits.EnumerateArray().Any(v=>v.ValueKind!=JsonValueKind.String||v.GetString() is not {Length:>0 and <=160}))throw new InvalidDataException();
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
            if(d.GetProperty("regularExecutionAllowed").GetBoolean())throw new InvalidDataException();
            var order=d.GetProperty("order").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
            if(order.Length is <1 or >32||!order.Order().SequenceEqual(Enumerable.Range(0,order.Length)))throw new InvalidDataException();
            if(r.Operation=="plan")
            {
                if(!ValidPlanKey(d.GetProperty("planKey").GetString()??"")||!order.SequenceEqual(LargeProjectRequest.Order(r.Steps)))throw new InvalidDataException();
                var parts=d.GetProperty("parts").EnumerateArray().ToArray();
                if(parts.Length!=r.Steps.Length||d.GetProperty("powerLinks").GetArrayLength()>512)throw new InvalidDataException();
                var safety=d.GetProperty("roadProtection").Deserialize<NativeRoadProtection>(NativeJson.Options);
                RoadProtectionContract.Validate(safety,false,requireBaseline:true,requireProbeKinds:true,requireConstructionPreview:true,maxCandidateCells:2048);
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
