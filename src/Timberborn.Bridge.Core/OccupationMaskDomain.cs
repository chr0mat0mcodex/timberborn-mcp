namespace Timberborn.Bridge.Core;

// Enumerate combinations of actual flag bits, not the numeric range of an All sentinel.
public static class OccupationMaskDomain
{
    public static int[] Create(IEnumerable<int> declaredValues,int all)
    {
        var values=declaredValues.Distinct().ToArray();
        var bits=values.Where(v=>v!=all && v!=0 &&
            (unchecked((uint)v) & (unchecked((uint)v)-1))==0).ToArray();
        if(bits.Length is <1 or >8)return Array.Empty<int>();
        var masks=new List<int>{0};
        foreach(int bit in bits) {
            int count=masks.Count;
            for(int i=0;i<count;i++)masks.Add(masks[i]|bit);
        }
        // A newly introduced composite with unknown bits must not produce a partial known domain.
        if(values.Any(v=>v!=all && !masks.Contains(v)))return Array.Empty<int>();
        if(!masks.Contains(all))masks.Add(all);
        return masks.ToArray();
    }
}
