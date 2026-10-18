using System.Text.Json;
using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ProjectPriorityContract
{
    public static void Validate(JsonElement d, LargeProjectRequest r)
    {
        static bool Revision(string? s)=>s is {Length:64} && s.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');
        string? Priority(JsonElement item,string key) {
            var p=item.GetProperty(key);if(p.ValueKind==JsonValueKind.Null)return null;
            var value=p.GetString();if(value is null || !ManagementRequest.IsPriority(value))throw new InvalidDataException();return value;
        }
        var outcome=d.GetProperty("outcome").GetString();
        bool write=r.Operation=="set-priority";
        if(d.GetProperty("actionId").GetString()!=r.ActionId || !Revision(d.GetProperty("revision").GetString()) ||
            !Revision(d.GetProperty("previousRevision").GetString()) ||
            (write ? outcome is not ("applied" or "partial_unconfirmed") : outcome!="observed") ||
            write && d.GetProperty("previousRevision").GetString()!=r.ExpectedRevision)throw new InvalidDataException();
        var items=d.GetProperty("items").EnumerateArray().ToArray();
        if(items.Length is <1 or >32)throw new InvalidDataException();
        var ids=new HashSet<string>();bool stopped=false,unconfirmed=false;
        for(int i=0;i<items.Length;i++) {
            var item=items[i];var id=item.GetProperty("id").GetString();
            if(!Guid.TryParseExact(id,"D",out var guid)||guid==Guid.Empty||!ids.Add(id!)||item.GetProperty("index").GetInt32()!=i)throw new InvalidDataException();
            var state=item.GetProperty("state").GetString();
            if(state is not ("missing" or "initializing" or "identity_mismatch" or "construction" or "construction_unreachable" or
                "construction_access_unknown" or "finished" or "finished_access_missing" or "district_unavailable"))throw new InvalidDataException();
            var previous=Priority(item,"previousPriority");var current=Priority(item,"priority");var result=item.GetProperty("outcome").GetString();
            if(!write) {if(result!="observed"||current!=previous)throw new InvalidDataException();continue;}
            if(result is not ("applied" or "unchanged" or "unconfirmed" or "not_attempted" or "skipped"))throw new InvalidDataException();
            if(result is "applied" or "unchanged") {
                if(stopped||state!="construction"||current!=r.Priority||previous is null||result=="unchanged"&&previous!=current)throw new InvalidDataException();
            }
            if(result=="skipped" && (current is not null || previous is not null || state is not ("finished" or "missing")))throw new InvalidDataException();
            if(result=="not_attempted" && (!stopped||current!=previous||previous is null))throw new InvalidDataException();
            if(result=="unconfirmed") {if(stopped||previous is null)throw new InvalidDataException();stopped=true;unconfirmed=true;}
        }
        if(write && (outcome=="partial_unconfirmed")!=unconfirmed)throw new InvalidDataException();
        if(!write && d.GetProperty("previousRevision").GetString()!=d.GetProperty("revision").GetString())throw new InvalidDataException();
    }
}
