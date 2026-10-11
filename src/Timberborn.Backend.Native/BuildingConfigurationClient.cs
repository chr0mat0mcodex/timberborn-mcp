using System.Collections.Specialized;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<BuildingConfigurationResult>> ConfigureBuildings(BuildingConfigurationRequest r,NameValueCollection query,CancellationToken ct)
    {
        string route=r.Write?"configure-buildings":"building-configuration";
        var checkedRequest=BuildingConfigurationRequest.Parse("/agent-api/v1/"+route,query);
        if(checkedRequest.JobId!=r.JobId||checkedRequest.Session!=r.Session)throw new ArgumentException();
        string suffix=string.Join("&",query.AllKeys.Select(k=>Uri.EscapeDataString(k!)+"="+Uri.EscapeDataString(query[k]!)));
        var e=await Get<BuildingConfigurationResult>(route+"?"+suffix,ct,r.Write?HttpMethod.Post:HttpMethod.Get);
        if(e.BridgeVersion is not ("0.42.0" or "0.43.0" or "0.44.0" or "0.45.0")||e.SessionId!=r.Session)throw new InvalidDataException();
        BuildingConfigurationContract.Validate(e.Data,checkedRequest);
        return e;
    }
}
public static class BuildingConfigurationContract
{
    public static void Validate(BuildingConfigurationResult d,BuildingConfigurationRequest r)
    {
        if(d.JobId!=r.JobId||d.Outcome is not ("applied" or "partial" or "rejected")||d.Items is null||d.Items.Length is <1 or >8||
            d.Items.Any(i=>i is null||i.Requested is null)||d.Items.Select(i=>i.Requested.Id).Distinct().Count()!=d.Items.Length||
            (d.ProofState=="unconfirmed" ? d.ProofCode!="proof_start_unconfirmed"&&!BridgeRejectionException.IsCode(d.ProofCode) : d.ProofCode!="")||
            d.ProofState=="not_started"&&d.Outcome=="applied"||
            d.ProofState is not ("not_requested" or "not_started" or "started" or "unconfirmed")||
            (d.ProofState is "started" or "unconfirmed" ? d.RunId!=d.JobId||d.Outcome!="applied" : d.RunId is not null))throw new InvalidDataException("invalid_configuration_result");
        foreach(var i in d.Items) {
            var t=i.Requested;
            if(!Guid.TryParseExact(t.Id,"D",out var id)||id==Guid.Empty||!BuildingPolicy.ValidTemplate(t.Recipe)||
                t.ExpectedRecipe!=""&&!BuildingPolicy.ValidTemplate(t.ExpectedRecipe)||t.DesiredWorkers is <0 or >100||t.ExpectedDesiredWorkers is <0 or >100||
                i.Outcome is not ("applied" or "rejected" or "unconfirmed" or "not_attempted")||
                i.Code is not ("" or "state_conflict" or "unavailable_recipe" or "staffing_out_of_range" or "not_pausable" or "building_unavailable" or "observation_unavailable" or "setting_unconfirmed")||
                i.Outcome=="rejected"&&i.Code==""||i.Outcome=="unconfirmed"&&i.Code!="setting_unconfirmed"||
                i.AppliedFields is null||i.AppliedFields.Length>3||i.AppliedFields.Distinct().Count()!=i.AppliedFields.Length||
                i.AppliedFields.Any(f=>f is not ("recipe" or "workers" or "paused"))||i.AttemptedField is not (null or "recipe" or "workers" or "paused")||
                (i.Outcome=="unconfirmed")!=(i.AttemptedField is not null)||
                i.Observed is {} o&&(!BuildingPolicy.ValidTemplate(o.Template)||o.Recipe!=""&&!BuildingPolicy.ValidTemplate(o.Recipe)||o.DesiredWorkers is <0 or >100)||
                i.Outcome=="applied"&&(i.Observed is null||i.Code!=""||i.Observed.Recipe!=t.Recipe||i.Observed.DesiredWorkers!=t.DesiredWorkers||i.Observed.Paused!=t.Paused)||
                i.Outcome=="not_attempted"&&(i.AppliedFields.Length!=0||i.Code!=""))throw new InvalidDataException("invalid_configuration_item");
        }
        if(d.Outcome=="applied"&&d.Items.Any(i=>i.Outcome!="applied")||
            d.Outcome=="rejected"&&d.Items.Any(i=>i.Outcome is "applied" or "unconfirmed"||i.AppliedFields.Length!=0)||
            d.Outcome!="applied"&&!d.Items.Any(i=>i.Outcome is "rejected" or "unconfirmed"))throw new InvalidDataException();
        if(r.Write) {
            string Key(BuildingConfigurationItem i)=>$"{i.Id}|{i.Recipe}|{i.ExpectedRecipe}|{i.DesiredWorkers}|{i.ExpectedDesiredWorkers}|{i.Paused}|{i.ExpectedPaused}";
            if(!r.Items.Select(Key).SequenceEqual(d.Items.Select(i=>Key(i.Requested)))||
                (r.ProofHours is null)!=(d.ProofState=="not_requested"))throw new InvalidDataException("configuration_request_mismatch");
        }
    }
}
