namespace Timberborn.Bridge.Core;

// This permits deferring a native check, never authorizes placement or proves support.
public static class LargeProjectSupportPolicy
{
    public sealed class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Step { get; set; }
        public bool Stackable { get; set; }
        public string MatterBelow { get; set; } = "";
    }
    public static bool CanDefer(bool nativeValid,bool intersectsExistingOrPlanned,int[] dependencies,Cell[] foundations,Cell[] supports)
    {
        if(nativeValid||intersectsExistingOrPlanned||dependencies.Length==0||foundations.Length==0)return false;
        return foundations.All(c=>c.MatterBelow is "GroundOrStackable" or "Stackable" &&
            supports.Any(s=>dependencies.Contains(s.Step)&&s.Stackable&&s.X==c.X&&s.Y==c.Y&&s.Z==c.Z-1));
    }
}
