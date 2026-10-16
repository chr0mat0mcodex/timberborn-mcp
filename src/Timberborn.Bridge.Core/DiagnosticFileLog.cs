using System.Text;

namespace Timberborn.Bridge.Core;

// One append-only file per observed game session and logger instance. No rotation or deletion.
public sealed class DiagnosticFileLog
{
    private readonly string directory,component;
    private readonly Func<object,string> serialize;
    private readonly Action? warning;
    private readonly object sync=new();
    private readonly string instanceId=Guid.NewGuid().ToString("D");
    private readonly Dictionary<string,(string Path,string Name)> files=new();
    private long sequence;
    private bool warned;
    public long FailedWrites { get; private set; }
    public static string DefaultDirectory(string component) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"TimberbornMcp","Logs",component);
    public DiagnosticFileLog(string directory,string component,Func<object,string> serialize,Action? warning=null)
    {
        if(component is not ("mcp" or "bridge"))throw new ArgumentException();
        this.directory=directory;this.component=component;this.serialize=serialize;this.warning=warning;
    }
    private static string Label(string? value) => value is {Length:>0 and <=80} &&
        value.All(c=>c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-') &&
        !(value.Length==64&&value.All(c=>c is >= 'a' and <= 'f' or >= '0' and <= '9')) ? value : "unknown";
    private static string? Identity(string? value)=>Guid.TryParseExact(value,"D",out var id)&&id!=Guid.Empty?id.ToString("D"):null;
    public static string FileNamePart(string? value)
    {
        var safe=new string((value??"").Select(c=>char.IsControl(c)||"<>:\"/\\|?*".Contains(c)?'_':c).Take(80).ToArray()).Trim().Trim('.');
        return safe.Length==0?"unbekannter-spielstand":safe;
    }
    public void Write(string eventName,string? operation=null,string? state=null,string? code=null,string? callId=null,string? session=null,
        IEnumerable<ActivityCorrelation>? correlations=null,string? reasoning=null,string? summary=null,string? saveName=null)
    {
        lock(sync) {
            sequence++;
            try {
                string? identity=Identity(session);
                string key=identity??"process";
                if(!files.TryGetValue(key,out var file)) {
                    string name=identity is null?"prozess":FileNamePart(saveName);
                    string stamp=DateTimeOffset.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture);
                    file=(Path.Combine(directory,$"{component}_{name}_{stamp}_{Guid.NewGuid():N}.log"),name);
                    files.Add(key,file);
                }
                var refs=(correlations??Array.Empty<ActivityCorrelation>()).Where(ActivityCorrelation.Valid).Take(8).Select(ActivityCorrelation.Payload).ToArray();
                var record=new {schemaVersion=1,component,instanceId,sequence,atUtc=DateTimeOffset.UtcNow,
                    @event=Label(eventName),operation=operation is null?null:Label(operation),state=state is null?null:Label(state),
                    code=code is null?null:Label(code),callId=Identity(callId),session=identity,saveName=file.Name,
                    reasoning=reasoning is null?null:ActivityRequest.Clean(reasoning,600),summary=summary is null?null:ActivityRequest.Clean(summary,400),
                    correlations=refs,failedWrites=FailedWrites};
                string line=serialize(record);
                if(line.IndexOfAny(new[]{'\r','\n'})>=0)throw new InvalidDataException("multiline_log_record");
                byte[] bytes=new UTF8Encoding(false).GetBytes(line+"\n");
                Directory.CreateDirectory(directory);
                using var output=new FileStream(file.Path,FileMode.Append,FileAccess.Write,FileShare.Read);
                output.Write(bytes,0,bytes.Length);
                output.Flush();
            } catch(Exception) {
                FailedWrites++;
                if(!warned) {
                    warned=true;
                    try {warning?.Invoke();}catch(Exception) { /* Diagnostics must never fail an action. */ }
                }
            }
        }
    }
}
