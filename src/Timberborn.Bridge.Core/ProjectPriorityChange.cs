namespace Timberborn.Bridge.Core;

public static class ProjectPriorityChange
{
    // Call only after whole-project identity/revision preflight. Null marks a skipped part.
    public static string[] Apply(string?[] before, string target, Func<int,string> read, Action<int,string> write)
    {
        if(before.Length is <1 or >32 || !ManagementRequest.IsPriority(target) ||
            before.Any(p=>p is not null && !ManagementRequest.IsPriority(p))) throw new ArgumentException();
        var outcomes=new string[before.Length];bool stopped=false;
        for(int i=0;i<before.Length;i++)
        {
            if(before[i] is null) {outcomes[i]="skipped";continue;}
            if(stopped) {outcomes[i]="not_attempted";continue;}
            if(before[i]==target) {outcomes[i]="unchanged";continue;}
            try {write(i,target);outcomes[i]=read(i)==target?"applied":"unconfirmed";}
            catch {outcomes[i]="unconfirmed";}
            stopped=outcomes[i]=="unconfirmed";
        }
        return outcomes;
    }
}
