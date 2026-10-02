using System.Collections.Specialized;
namespace Timberborn.Bridge.Core;

public sealed class BuildingProjectValidationRequest
{
    public BuildingPlanRequest Plan { get; }
    public int OptionIndex { get; }
    public string PlanKey { get; }
    private BuildingProjectValidationRequest(BuildingPlanRequest plan,int index,string key)
    { Plan=plan;OptionIndex=index;PlanKey=key; }
    public static BuildingProjectValidationRequest Parse(NameValueCollection q)
    {
        if(q.Count!=11)throw new ArgumentException();
        var indices=q.GetValues("optionIndex");var keys=q.GetValues("planKey");
        if(indices is not {Length:1}||indices[0].Length!=1||indices[0][0]<'0'||indices[0][0]>'3'||
            keys is not {Length:1}||keys[0].Length!=64||keys[0].Any(c=>!(c>='0'&&c<='9'||c>='a'&&c<='f')))throw new ArgumentException();
        var copy=new NameValueCollection(q);copy.Remove("optionIndex");copy.Remove("planKey");
        return new(BuildingPlanRequest.Parse(copy),indices[0][0]-'0',keys[0]);
    }
}
