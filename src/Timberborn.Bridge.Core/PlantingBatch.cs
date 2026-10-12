using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
namespace Timberborn.Bridge.Core;

public sealed class PlantingRectangle
{
    public int X {get;set;} public int Y {get;set;} public int Z {get;set;}
    public int Width {get;set;} public int Height {get;set;}
    public string Resource {get;set;}="";
    public string WorkBuildingId {get;set;}="";
    public bool RequireMoist {get;set;}=true;
}
public sealed class PlantingBatchRequest
{
    public string Session {get;set;}=""; public string JobId {get;set;}="";
    public string Key {get;set;}="";
    public string Operation {get;set;}="preview";
    public PlantingRectangle[] Rectangles {get;set;}=Array.Empty<PlantingRectangle>();
    public const int MaxResponseBytes=2*1024*1024;
    public static bool Handles(string path)=>path is "/agent-api/v1/planting-preview" or "/agent-api/v1/planting-apply" or "/agent-api/v1/planting-status";
    public static void Id(string value,string field) {if(!Guid.TryParseExact(value,"D",out var id)||id==Guid.Empty||value!=id.ToString("D"))throw new ArgumentException("planting_"+field+"_must_be_uuid");}
    public void Validate()
    {
        Id(Session,"session");Id(JobId,"jobId");
        if(Operation is not ("preview" or "apply" or "status"))throw new ArgumentException("planting_invalid_operation");
        if(Operation=="status") {if(Rectangles is null||Rectangles.Length!=0||Key!="")throw new ArgumentException("planting_status_has_no_plan");return;}
        if(Rectangles is null||Rectangles.Length is <1 or >8)throw new ArgumentException("planting_rectangles_1_to_8");
        var cells=new HashSet<string>();
        foreach(var r in Rectangles) {
            if(r is null||r.X<0||r.Y<0||r.Z<0||r.Z>4095||r.Width is <1 or >8||r.Height is <1 or >8||r.X>4096-r.Width||r.Y>4096-r.Height)
                throw new ArgumentException("planting_rectangle_bounds_8x8");
            Id(r.WorkBuildingId,"workBuildingId");
            if(!BuildingPolicy.ValidTemplate(r.Resource))throw new ArgumentException("planting_invalid_resource");
            for(int y=0;y<r.Height;y++)for(int x=0;x<r.Width;x++)
                if(!cells.Add($"{r.X+x},{r.Y+y},{r.Z}"))throw new ArgumentException("planting_rectangles_overlap");
        }
        if(Operation=="apply"&&(Key is null||Key.Length!=64||Key.Any(c=>!Uri.IsHexDigit(c))))throw new ArgumentException("planting_invalid_preview_key");
        if(Operation=="preview"&&Key!="")throw new ArgumentException("planting_preview_has_no_key");
    }
    public string Rows()=>string.Join(";",Rectangles.Select(r=>string.Join("|",r.X,r.Y,r.Z,r.Width,r.Height,r.Resource,r.WorkBuildingId,r.RequireMoist?"true":"false")));
    public static PlantingBatchRequest Parse(string path,NameValueCollection q)
    {
        string Read(string k)=>q.GetValues(k) is {Length:1} v?v[0]:throw new ArgumentException("planting_missing_"+k);
        if(!Handles(path))throw new ArgumentException("planting_invalid_route");
        var r=new PlantingBatchRequest{Session=Read("session"),JobId=Read("jobId"),Operation=path.Substring(path.LastIndexOf('-')+1)};
        var keys=new List<string>{"session","jobId"};
        if(r.Operation!="status") {
            keys.Add("rectangles");var raw=Read("rectangles");if(raw.Length>4000)throw new ArgumentException("planting_request_too_large");
            int N(string s)=>int.TryParse(s,NumberStyles.None,CultureInfo.InvariantCulture,out var n)?n:throw new ArgumentException("planting_invalid_integer");
            r.Rectangles=raw.Split(';').Select(row=>{var v=row.Split('|');if(v.Length!=8||v[7] is not ("true" or "false"))throw new ArgumentException("planting_invalid_rectangle");
                return new PlantingRectangle{X=N(v[0]),Y=N(v[1]),Z=N(v[2]),Width=N(v[3]),Height=N(v[4]),Resource=v[5],WorkBuildingId=v[6],RequireMoist=v[7]=="true"};}).ToArray();
        }
        if(r.Operation=="apply"){keys.Add("key");r.Key=Read("key");}
        if(q.Count!=keys.Count||q.AllKeys.Any(k=>!keys.Contains(k!)))throw new ArgumentException("planting_unexpected_parameter");
        r.Validate();return r;
    }
}
public sealed class PlantingOccupant
{
    public string Id {get;set;}=""; public string Kind {get;set;}=""; public string Template {get;set;}="";
}
public sealed class PlantingCell
{
    public int Rectangle {get;set;} public int X {get;set;} public int Y {get;set;} public int Z {get;set;}
    public string Resource {get;set;}=""; public string Existing {get;set;}="unmarked";
    public bool? NativeValid {get;set;} public bool? Reachable {get;set;} public bool? SoilIsMoist {get;set;}
    public string[] Reasons {get;set;}=Array.Empty<string>();
    public PlantingOccupant[] Occupants {get;set;}=Array.Empty<PlantingOccupant>();
    public bool Eligible {get;set;}
    public string Result {get;set;}="excluded";
}
public static class PlantingEligibility
{
    public static void Evaluate(PlantingCell c,bool requireMoist,IEnumerable<string> preconditions)
    {
        var reasons=new List<string>(preconditions);
        if(c.Reachable!=true)reasons.Add(c.Reachable==false?"outside_work_range":"work_range_unknown");
        if(requireMoist&&c.SoilIsMoist!=true)reasons.Add(c.SoilIsMoist==false?"soil_dry":"soil_moisture_unknown");
        c.Result="excluded";
        if(c.Existing==c.Resource){c.Result="already_marked";reasons.Add("already_marked");}
        else if(c.Existing!="unmarked")reasons.Add("other_planting_designation");
        if(c.NativeValid!=true)reasons.Add(c.NativeValid==false?"native_planting_blocked":"native_planting_unknown");
        c.Reasons=reasons.Distinct().ToArray();c.Eligible=c.Reasons.Length==0;
        if(c.Eligible)c.Result="not_attempted";
    }
}
public sealed class PlantingBatchReport
{
    public string JobId {get;set;}=""; public string Key {get;set;}="";
    public string Outcome {get;set;}="preview"; public string Code {get;set;}="";
    public PlantingCell[] Cells {get;set;}=Array.Empty<PlantingCell>();
}
public interface IPlantingBatchPort
{
    void RequirePaused();
    PlantingCell[] Observe(PlantingBatchRequest request);
    void Mark(PlantingCell cell);
    string ReadDesignation(PlantingCell cell);
}
public sealed class PlantingBatchController
{
    private readonly Dictionary<string,(string Fingerprint,PlantingBatchReport Report)> jobs=new();
    public static string Key(PlantingBatchRequest r,PlantingCell[] cells)
    {
        var text=r.Session+"/"+r.JobId+"/"+r.Rows()+"/"+string.Join(";",cells.Select(c=>
            $"{c.Rectangle},{c.X},{c.Y},{c.Z},{c.Resource},{c.Existing},{c.NativeValid},{c.Reachable},{c.SoilIsMoist},{c.Eligible}:"+
            string.Join(",",c.Reasons)+":"+string.Join(",",c.Occupants.Select(o=>o.Id+"/"+o.Kind+"/"+o.Template))));
        using var hash=SHA256.Create();return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","").ToLowerInvariant();
    }
    public PlantingBatchReport Handle(PlantingBatchRequest r,IPlantingBatchPort port)
    {
        r.Validate();string id=r.Session+"/"+r.JobId,fingerprint=r.Rows()+"/"+r.Key;
        if(r.Operation=="status")return jobs.TryGetValue(id,out var prior)?prior.Report:throw new BridgeRejectionException("planting_job_not_found");
        if(r.Operation=="apply"&&jobs.TryGetValue(id,out var old)) {
            if(old.Fingerprint!=fingerprint)throw new BridgeRejectionException("planting_job_parameters_changed");return old.Report;
        }
        port.RequirePaused();var cells=port.Observe(r);port.RequirePaused();
        var report=new PlantingBatchReport{JobId=r.JobId,Cells=cells,Key=Key(r,cells)};
        if(r.Operation=="preview")return report;
        if(report.Key!=r.Key)throw new BridgeRejectionException("planting_preview_changed");
        if(jobs.Count>=128)throw new BridgeRejectionException("planting_job_limit");
        jobs.Add(id,(fingerprint,report)); // Record before first mutation; no replay even after partial failure.
        report.Outcome="applied";
        foreach(var c in cells.Where(c=>c.Eligible)) {
            c.Result="unconfirmed";
            try {port.Mark(c);c.Existing=port.ReadDesignation(c);if(c.Existing!=c.Resource)throw new InvalidOperationException();c.Result="marked";}
            catch {report.Outcome="unconfirmed";report.Code="planting_partial_change_no_retry";break;}
        }
        return report;
    }
}
