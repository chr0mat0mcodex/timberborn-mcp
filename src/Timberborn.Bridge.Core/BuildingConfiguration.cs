using System.Collections.Specialized;
using System.Globalization;
namespace Timberborn.Bridge.Core;

public sealed class BuildingConfigurationItem
{
    public string Id { get; set; } = "";
    public string Recipe { get; set; } = "";
    public string ExpectedRecipe { get; set; } = "";
    public int DesiredWorkers { get; set; }
    public int ExpectedDesiredWorkers { get; set; }
    public bool Paused { get; set; }
    public bool ExpectedPaused { get; set; }
}
public sealed class BuildingConfigurationRequest
{
    public string Session { get; private set; } = "";
    public string JobId { get; private set; } = "";
    public bool Write { get; private set; }
    public BuildingConfigurationItem[] Items { get; private set; } = Array.Empty<BuildingConfigurationItem>();
    public double? ProofHours { get; private set; }
    public int ProofSpeed { get; private set; }
    public int ProofMaxRealSeconds { get; private set; }
    public static bool Handles(string path)=>path is "/agent-api/v1/configure-buildings" or "/agent-api/v1/building-configuration";
    public static BuildingConfigurationRequest Parse(string path,NameValueCollection q)
    {
        if(!Handles(path))throw new ArgumentException();
        var r=new BuildingConfigurationRequest {Write=path.EndsWith("configure-buildings",StringComparison.Ordinal)};
        string Read(string k)=>q.GetValues(k) is {Length:1} values ? values[0] : throw new ArgumentException();
        string Id(string k)=>Guid.TryParseExact(Read(k),"D",out var id)&&id!=Guid.Empty?id.ToString("D"):throw new ArgumentException();
        r.Session=Id("session");r.JobId=Id("jobId");
        var keys=new List<string>{"session","jobId"};
        if(r.Write) {
            keys.Add("items");string raw=Read("items");if(raw.Length>4000)throw new ArgumentException();
            int Workers(string value)=>int.TryParse(value,NumberStyles.None,CultureInfo.InvariantCulture,out int n)&&n>=0&&n<=100?n:throw new ArgumentException();
            bool Bool(string value)=>value=="true"?true:value=="false"?false:throw new ArgumentException();
            r.Items=raw.Split(';').Select(row=>{
                var v=row.Split('|');if(v.Length!=7||!Guid.TryParseExact(v[0],"D",out var id)||id==Guid.Empty||
                    !BuildingPolicy.ValidTemplate(v[1])||v[2]!=""&&!BuildingPolicy.ValidTemplate(v[2]))throw new ArgumentException();
                return new BuildingConfigurationItem {Id=id.ToString("D"),Recipe=v[1],ExpectedRecipe=v[2],
                    DesiredWorkers=Workers(v[3]),ExpectedDesiredWorkers=Workers(v[4]),Paused=Bool(v[5]),ExpectedPaused=Bool(v[6])};
            }).ToArray();
            if(r.Items.Length is <1 or >8||r.Items.Select(i=>i.Id).Distinct().Count()!=r.Items.Length)throw new ArgumentException();
            if(q["proofHours"] is not null) {
                keys.AddRange(new[]{"proofHours","proofSpeed","proofMaxRealSeconds"});
                if(!double.TryParse(Read("proofHours"),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out double h)||double.IsNaN(h)||double.IsInfinity(h)||h<0.000001||h>672)throw new ArgumentException();
                if(!int.TryParse(Read("proofSpeed"),out int speed)||speed is not (1 or 3 or 7)||
                    !int.TryParse(Read("proofMaxRealSeconds"),out int seconds)||seconds<30||seconds>86400||r.Items.Any(i=>i.Paused||i.DesiredWorkers==0))throw new ArgumentException();
                r.ProofHours=h;r.ProofSpeed=speed;r.ProofMaxRealSeconds=seconds;
            }
        }
        if(q.Count!=keys.Count||q.AllKeys.Any(k=>k is null||!keys.Contains(k)))throw new ArgumentException();
        return r;
    }
    public SimulationRunRequest ProofRequest()=>SimulationRunRequest.Parse("/agent-api/v1/simulation-run-for",new NameValueCollection {
        {"session",Session},{"runId",JobId},{"duration",ProofHours!.Value.ToString(CultureInfo.InvariantCulture)},
        {"unit","hours"},{"speed",ProofSpeed.ToString(CultureInfo.InvariantCulture)},{"expectedSpeed","0"},
        {"maxRealSeconds",ProofMaxRealSeconds.ToString(CultureInfo.InvariantCulture)}, {"buildingIds",string.Join(",",Items.Select(i=>i.Id))} });
}
public sealed class ConfigurationObservation
{
    public string Template {get;set;}="";
    public string Recipe {get;set;}="";
    public int DesiredWorkers {get;set;}
    public bool Paused {get;set;}
}
public sealed class ConfigurationCheck
{
    public ConfigurationObservation State {get;set;}=new();
    public string[] Recipes {get;set;}=Array.Empty<string>();
    public int MaxWorkers {get;set;}
    public bool CanPause {get;set;}
}
public sealed class ConfigurationItemResult
{
    public BuildingConfigurationItem Requested {get;set;}=new();
    public string Outcome {get;set;}="not_attempted";
    public string Code {get;set;}="";
    public string[] AppliedFields {get;set;}=Array.Empty<string>();
    public string? AttemptedField {get;set;}
    public ConfigurationObservation? Observed {get;set;}
}
public sealed class BuildingConfigurationResult
{
    public string JobId {get;set;}="";
    public string Outcome {get;set;}="rejected";
    public ConfigurationItemResult[] Items {get;set;}=Array.Empty<ConfigurationItemResult>();
    public string ProofState {get;set;}="not_requested";
    public string ProofCode {get;set;}="";
    public string? RunId {get;set;}
}
public interface IBuildingConfigurationPort
{
    ConfigurationCheck Read(string id);
    void SetRecipe(string id,string recipe);
    void SetWorkers(string id,int workers);
    void SetPaused(string id,bool paused);
}
// One synchronous game-thread transaction with explicit partial results, never rollback/replay.
public sealed class BuildingConfigurationController
{
    private readonly Dictionary<string,BuildingConfigurationResult> history=new();
    public BuildingConfigurationResult Inspect(string id)=>history.TryGetValue(id,out var r)?r:throw new BridgeRejectionException("configuration_not_found");
    public BuildingConfigurationResult Execute(BuildingConfigurationRequest request,IBuildingConfigurationPort port,Action requireAvailable,Action startProof)
    {
        if(!request.Write)throw new ArgumentException();
        if(history.ContainsKey(request.JobId))throw new BridgeRejectionException("configuration_id_used");
        if(history.Count>=128)throw new BridgeRejectionException("state_conflict");
        requireAvailable();
        var result=new BuildingConfigurationResult {JobId=request.JobId,ProofState=request.ProofHours is null?"not_requested":"not_started",
            Items=request.Items.Select(i=>new ConfigurationItemResult{Requested=i}).ToArray()};
        history.Add(request.JobId,result);
        bool invalid=false;
        foreach(var item in result.Items) {
            try {
                var check=port.Read(item.Requested.Id);item.Observed=check.State;
                string? code=Validate(item.Requested,check);
                if(code is not null){item.Outcome="rejected";item.Code=code;invalid=true;}
            } catch(BridgeRejectionException e) {item.Outcome="rejected";item.Code=e.Code;invalid=true;}
              catch {item.Outcome="rejected";item.Code="observation_unavailable";invalid=true;}
        }
        if(invalid)return result; // ALL preflight checks precede the first setter.
        foreach(var item in result.Items) {
            var target=item.Requested;var expected=item.Observed!;var applied=new List<string>();
            try {
                foreach(string field in new[]{"recipe","workers","paused"}) {
                    var now=port.Read(target.Id).State;
                    if(!Same(now,expected)) {item.Observed=now;item.Outcome="rejected";item.Code="state_conflict";result.Outcome="partial";return result;}
                    bool change=field=="recipe"?now.Recipe!=target.Recipe:field=="workers"?now.DesiredWorkers!=target.DesiredWorkers:now.Paused!=target.Paused;
                    if(!change)continue;
                    item.AttemptedField=field;
                    if(field=="recipe")port.SetRecipe(target.Id,target.Recipe);
                    else if(field=="workers")port.SetWorkers(target.Id,target.DesiredWorkers);
                    else port.SetPaused(target.Id,target.Paused);
                    expected=new ConfigurationObservation {Template=now.Template,Recipe=field=="recipe"?target.Recipe:now.Recipe,
                        DesiredWorkers=field=="workers"?target.DesiredWorkers:now.DesiredWorkers,Paused=field=="paused"?target.Paused:now.Paused};
                    var after=port.Read(target.Id).State;item.Observed=after;
                    if(!Same(after,expected))throw new InvalidOperationException();
                    applied.Add(field);item.AppliedFields=applied.ToArray();item.AttemptedField=null;
                }
                item.Outcome="applied";
            } catch {
                item.Outcome=item.AttemptedField is null?"rejected":"unconfirmed";
                item.Code=item.AttemptedField is null?"observation_unavailable":"setting_unconfirmed";
                try {item.Observed=port.Read(target.Id).State;}catch {item.Observed=null;}
                result.Outcome="partial";return result;
            }
        }
        result.Outcome="applied";
        if(request.ProofHours is not null) {
            result.RunId=request.JobId;
            try {startProof();result.ProofState="started";}
            catch(BridgeRejectionException e) {result.ProofState="unconfirmed";result.ProofCode=e.Code;}
            catch {result.ProofState="unconfirmed";result.ProofCode="proof_start_unconfirmed";} // Inspect runId; never repeat configuration.
        }
        return result;
    }
    private static bool Same(ConfigurationObservation a,ConfigurationObservation b)=>a.Template==b.Template&&a.Recipe==b.Recipe&&a.DesiredWorkers==b.DesiredWorkers&&a.Paused==b.Paused;
    private static string? Validate(BuildingConfigurationItem i,ConfigurationCheck c)=>
        c.State.Recipe!=i.ExpectedRecipe||c.State.DesiredWorkers!=i.ExpectedDesiredWorkers||c.State.Paused!=i.ExpectedPaused?"state_conflict":
        !c.Recipes.Contains(i.Recipe)?"unavailable_recipe":i.DesiredWorkers<1||i.DesiredWorkers>c.MaxWorkers?"staffing_out_of_range":
        i.Paused!=c.State.Paused&&!c.CanPause?"not_pausable":null;
}
