using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record ResearchOption(string Template,int ScienceCost,bool Available,bool Unlocked,bool Unlockable);
public sealed record NativeResearch(int SciencePoints,int Offset,int Limit,int Total,ResearchOption[] Items,bool HasMore,string[] Limitations);
public sealed record NativeUnlock(string Template,int ExpectedCost,int ScienceCost,int PointsBefore,int PointsAfter,bool PreviouslyUnlocked,bool Unlocked,string Outcome);
public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeResearch>> Research(ResearchRequest r,CancellationToken ct)
    {
        if(r.Write)throw new ArgumentException();
        var e=await Get<NativeResearch>($"research?offset={r.Offset}&limit={r.Limit}",ct);var d=e.Data;
        if(e.BridgeVersion!="0.17.0"||d.SciencePoints<0||d.Offset!=r.Offset||d.Limit!=r.Limit||d.Total<0||d.Items is null||
            d.Items.Length!=Math.Min(r.Limit,Math.Max(0,d.Total-r.Offset))||d.HasMore!=((long)d.Offset+d.Items.Length<d.Total)||d.Limitations is null||
            d.Items.Any(i=>i is null||!BuildingPolicy.ValidTemplate(i.Template)||i.ScienceCost<0)||d.Items.Select(i=>i.Template).Distinct().Count()!=d.Items.Length)
            throw new InvalidDataException("Invalid research observation");
        return e;
    }
    public async Task<BridgeEnvelope<NativeUnlock>> UnlockBuilding(ResearchRequest r,CancellationToken ct)
    {
        if(!r.Write)throw new ArgumentException();
        var e=await Get<NativeUnlock>($"unlock-building?template={Uri.EscapeDataString(r.Template)}&session={r.Session}&expectedCost={r.ExpectedCost}",ct,HttpMethod.Post);
        var d=e.Data;
        if(e.BridgeVersion!="0.17.0"||e.SessionId!=r.Session||d.Template!=r.Template||d.ExpectedCost!=r.ExpectedCost||d.ScienceCost<0||d.PointsBefore<0||d.PointsAfter<0||
            d.Outcome is not ("cost_changed" or "unavailable" or "already_unlocked" or "insufficient_points" or "not_unlockable" or "applied" or "unconfirmed")||
            d.Outcome=="applied" && (!d.Unlocked||d.PreviouslyUnlocked||d.ScienceCost!=r.ExpectedCost||(long)d.PointsBefore-d.ScienceCost!=d.PointsAfter)||
            d.Outcome=="already_unlocked" && (!d.PreviouslyUnlocked||!d.Unlocked)||
            d.Outcome is not ("applied" or "unconfirmed") && (d.PointsBefore!=d.PointsAfter||d.PreviouslyUnlocked!=d.Unlocked))
            throw new InvalidDataException("Invalid unlock receipt; do not retry");
        return e;
    }
}
