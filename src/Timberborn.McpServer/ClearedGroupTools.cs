using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;

namespace Timberborn.McpServer;

public sealed record ClearedGroupStart(ClearedGroupSpec Spec, string ClearanceKey, string Detail="compact");
public sealed record ClearedGroupPreview(ClearedGroupSpec Spec, string Detail="compact");
public sealed record ClearedGroupHandle(string Session,string JobId,string Detail="compact");

public sealed class ClearedGroupTools(IClearedGroupPort port,string directory) : IDisposable
{
    public const string Plan="plan_cleared_building_group",Start="start_cleared_building_group",
        Inspect="inspect_cleared_building_group",Stop="stop_cleared_building_group";
    private readonly object sync=new();
    private readonly CancellationTokenSource lifetime=new();
    private ClearedGroupJob? running;
    private Task? worker;
    private JsonObject? snapshot;
    private volatile bool stopRequested;
    public bool Active {get {lock(sync)return worker is {IsCompleted:false};}}
    public static bool Handles(string name)=>name is Plan or Start or Inspect or Stop;
    public static bool Writes(string name)=>name is Start or Stop;
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options=new JsonSerializerOptions(BuildBatchState.Json){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(var name in new[]{Plan,Start,Inspect,Stop}) {
            if(!enabled && name is Plan or Start)continue;
            var schema=options.GetJsonSchemaAsNode(name==Plan?typeof(ClearedGroupPreview):name==Start?typeof(ClearedGroupStart):typeof(ClearedGroupHandle)).AsObject();
            schema["type"]="object";
            schema["properties"]!["detail"]=new JsonObject{["type"]="string",["enum"]=new JsonArray("compact","full"),["default"]="compact"};
            yield return new Tool{Name=name,InputSchema=JsonSerializer.SerializeToElement(schema),OutputSchema=JsonSerializer.SerializeToElement(new{type="object"}),
                Description=name switch {
                    Plan=>"0.52: Räumvorschau für eine 1–8 × 1–8 Fläche auf einer Höhe und 1–8 Wunschgebäude. Liest Pflanzmarkierungen/Vegetation, gibt clearanceKey aus; keine Bauplatzgarantie vor Räumung. ReservedAreas schützen beim Räumen ganze XY-Spalten. Rohstoffstapel ignoriert und nie gelöscht. Gleiche spec plus Schlüssel an start. Alle vier Aktionsschalter erforderlich: Bau, Flächen, Entfernung, Tempo. detail=full für Einzelziele.",
                    Start=>"0.52: Bestätigte Räumvorschau einmal starten. Entfernt Pflanzmarkierungen, markiert natürliche/gepflanzte Vegetation (max64), wartet auf echte Räumung, plant dann frisch und baut nur eine vollständige gemeinsame Gruppe mit Wegen. MCP-Dispatcher arbeitet im Hintergrund, Mod simuliert begrenzt; kein Agentenaufruf pro Teil. clearanceHours+constructionHours höchstens672; maxRealSeconds 60–3600 gilt zusammen. Arbeitsschichten werden nicht verändert. Gleiche jobId liest nur; kein Retry/Replay nach Neustart. Rohstoffstapel ignoriert, Gebäude nicht abgerissen. Compact standardmäßig, detail=full Belege.",
                    Inspect=>"Liest lokalen Ablaufcheckpoint ohne Spielaufrufe/Dispatch. state, remainingTargets, pauseConfirmed, native runId/actionId und updatedAtUtc beachten. Nach MCP-Neustart interrupted ohne Wiederaufnahme: native IDs lesend abgleichen. Full enthält Räumziele, frischen Gruppenplan und Bau-/Abschlussbericht.",
                    _=>"Fordert dauerhaften Dispatcher-Stopp an; laufender Einzelaufruf kann noch abschließen. Eigener Simulations-/Baulauf wird einmal zum Stoppen aufgefordert; Status nachlesen. Bei Backendende laufen native Zeitbudgets weiter. Keine Rücknahme, Wiederholung oder Entfernung vorhandener Objekte."},
                Annotations=new(){ReadOnlyHint=!Writes(name),DestructiveHint=Writes(name),IdempotentHint=name!=Start,OpenWorldHint=false}};
        }
    }
    private static void Detail(string detail) {if(detail is not ("compact" or "full"))throw new ArgumentException("invalid_detail");}
    private string FileFor(string session,string id) {BuildBatchState.Id(session);BuildBatchState.Id(id);return Path.Combine(directory,session+"-"+id+".json");}
    private JsonObject? Read(string session,string id) {
        string path=FileFor(session,id);if(!File.Exists(path))return null;
        using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
        if(stream.Length>8_000_000)throw new InvalidDataException("cleared_group_journal_limit");
        var node=JsonNode.Parse(stream)!.AsObject();
        if(node["spec"]?["session"]?.GetValue<string>()!=session || node["spec"]?["jobId"]?.GetValue<string>()!=id)throw new InvalidDataException("invalid_cleared_group_checkpoint");
        return node;
    }
    private void Save(ClearedGroupJob j) {
        j.UpdatedAtUtc=DateTimeOffset.UtcNow;j.StopRequested=stopRequested;
        var node=JsonSerializer.SerializeToNode(j,NativeJson.Options)!.AsObject();
        string path=FileFor(j.Spec.Session,j.Spec.JobId),temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        Directory.CreateDirectory(directory);
        try {
            using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)) {JsonSerializer.Serialize(stream,node,NativeJson.Options);stream.Flush(true);}
            if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
        } finally {if(File.Exists(temp))File.Delete(temp);}
        lock(sync)snapshot=node;
    }
    public async Task<JsonObject> Invoke(string name,JsonElement args,bool enabled,CancellationToken ct)
    {
        if(!Handles(name) || !enabled && name is Plan or Start)throw new ArgumentException("cleared_group_actions_disabled");
        if(name==Plan) {
            var request=BuildBatchState.Parse<ClearedGroupPreview>(args);if(request.Spec is null)throw new ArgumentException();
            ClearedGroupPolicy.Validate(request.Spec);Detail(request.Detail);
            if(Active)throw new ArgumentException("cleared_group_active");
            await port.RequirePaused(request.Spec,ct);var observation=await port.Observe(request.Spec,true,ct);await port.RequirePaused(request.Spec,ct);
            var targets=ClearedGroupPolicy.Targets(request.Spec,observation);var designations=ClearedGroupPolicy.Designations(request.Spec,observation);
            var report=JsonSerializer.SerializeToNode(new{clearanceKey=ClearedGroupPolicy.Key(request.Spec,observation),
                state=ClearedGroupPolicy.Blocker(request.Spec,observation) is null?"clearance_ready":"blocked",
                reason=ClearedGroupPolicy.Blocker(request.Spec,observation),targetCount=targets.Length,designationCount=designations.Length,
                targets,designations,objects=observation.Objects,resourcePiles="ignored_never_removed",buildingPlan="only_after_clearance"},NativeJson.Options)!.AsObject();
            if(request.Detail=="compact") {report.Remove("targets");report.Remove("designations");report.Remove("objects");}
            return Wrap(request.Spec.Session,report);
        }
        string session,id,detail;
        ClearedGroupStart? start=null;
        if(name==Start) {
            start=BuildBatchState.Parse<ClearedGroupStart>(args);if(start.Spec is null)throw new ArgumentException();
            ClearedGroupPolicy.Validate(start.Spec);session=start.Spec.Session;id=start.Spec.JobId;detail=start.Detail;
            if(start.ClearanceKey is null || start.ClearanceKey.Length!=64 || start.ClearanceKey.Any(c=>!char.IsAsciiHexDigit(c)))throw new ArgumentException("invalid_clearance_key");
        } else {var h=BuildBatchState.Parse<ClearedGroupHandle>(args);session=h.Session;id=h.JobId;detail=h.Detail;}
        Detail(detail);BuildBatchState.Id(session);BuildBatchState.Id(id);
        JsonObject? node;lock(sync)node=running?.Spec.Session==session && running.Spec.JobId==id?snapshot?.DeepClone().AsObject():null;
        node??=Read(session,id);
        if(start is not null && node is not null && (node["clearanceKey"]?.GetValue<string>()!=start.ClearanceKey ||
            node["spec"]!.ToJsonString()!=JsonSerializer.SerializeToNode(start.Spec,NativeJson.Options)!.ToJsonString()))throw new ArgumentException("job_parameters_changed");
        if(start is not null && node is null) {
            if(Active)throw new ArgumentException("cleared_group_active");
            await port.RequirePaused(start.Spec,ct);
            Directory.CreateDirectory(directory);
            // Lifetime lease prevents two backend instances dispatching reclaim jobs simultaneously.
            var lease=new FileStream(Path.Combine(directory,"dispatcher.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            try {
                if(Directory.EnumerateFiles(directory,start.Spec.Session+"-*.json").Take(129).Count()>=128)throw new ArgumentException("cleared_group_journal_limit");
                var fresh=await port.Observe(start.Spec,true,ct);
                if(ClearedGroupPolicy.Key(start.Spec,fresh)!=start.ClearanceKey)throw new ArgumentException("clearance_changed_replan");
                if(ClearedGroupPolicy.Blocker(start.Spec,fresh) is {} blocker)throw new ArgumentException(blocker);
                stopRequested=false;var job=new ClearedGroupJob{Spec=start.Spec,ClearanceKey=start.ClearanceKey};Save(job);
                lock(sync) {
                    running=job;stopRequested=false;
                    using var noInheritedDispatch=ExecutionContext.SuppressFlow();
                    worker=Task.Run(async ()=> {
                        using(lease)using(var bound=CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token)) {
                            bound.CancelAfter(TimeSpan.FromSeconds(job.Spec.MaxRealSeconds+15));
                            try {await new ClearedGroupEngine(port,Save,()=>stopRequested).Run(job,bound.Token);}
                            catch(Exception ex) when(ex is not OutOfMemoryException) { Console.Error.WriteLine("Cleared group dispatcher checkpoint unavailable; no replay. Inspect native IDs."); }
                        }
                    },CancellationToken.None);
                    node=snapshot!.DeepClone().AsObject();
                }
            } catch {lease.Dispose();throw;}
        }
        if(node is null)throw new ArgumentException("cleared_group_not_found");
        lock(sync) {
            bool owned=running?.Spec.Session==session && running.Spec.JobId==id;
            if(owned)node["stopRequested"]=stopRequested;
            if(name==Stop && owned && worker is {IsCompleted:false}) {stopRequested=true;node["stopRequested"]=true;}
            if(node["terminal"]?.GetValue<bool>()!=true && (!owned || worker is {IsCompleted:true})) {
                node["checkpointState"]=node["state"]?.DeepClone();node["state"]="interrupted";
                node["reason"]="dispatcher_absent_no_replay_inspect_native_ids";node["terminal"]=true;
            }
        }
        node["checkpointOnly"]=true;node["detailAvailable"]="full";
        node["nextAction"]=node["state"]?.GetValue<string>() switch {"completed"=>"done","unconfirmed" or "interrupted"=>"inspect_native_run_and_project_ids_no_replay","blocked" or "stopped"=>"inspect_full_report_and_current_game_before_new_plan",_=>"wait_then_inspect_existing_job"};
        node["suggestedPollSeconds"]=node["terminal"]?.GetValue<bool>()==true?0:10;
        if(detail=="compact")Compact(node);
        return Wrap(session,node);
    }
    public static void Compact(JsonObject node) {
        if(node["observation"] is JsonObject o) {
            node["observedObjects"]=o["objects"] is JsonArray objects?objects.Count:0;node.Remove("observation");
        }
        if(node["plan"] is JsonObject p) {RegionGroupPlanning.Compact(p);p.Remove("steps");p.Remove("jointPlan");p.Remove("surveys");p.Remove("occupations");}
        if(node["construction"] is JsonObject c)ProjectReportPresentation.Compact(node["state"]?.GetValue<string>()=="completed"?"close_large_project":"inspect_large_project",c);
    }
    private static JsonObject Wrap(string session,JsonObject data)=>JsonSerializer.SerializeToNode(new NativeResult<JsonObject>(1,"ok",data,new("native",false,session,null),null),NativeJson.Options)!.AsObject();
    public void Dispose() {stopRequested=true;lifetime.Cancel();}
}
