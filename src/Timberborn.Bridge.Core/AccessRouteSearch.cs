namespace Timberborn.Bridge.Core;

// Candidate geometry only. Native joint validation remains the authority.
public static class AccessRouteSearch
{
    public static int[] RequiredParts(IReadOnlyList<LargeProjectStep> steps,IEnumerable<int> roots)
    {
        var result=new HashSet<int>();var pending=new Stack<int>(roots);
        while(pending.Count>0)
        {
            int i=pending.Pop();if(i<0||i>=steps.Count)throw new ArgumentException("dependency_index");
            if(result.Add(i))foreach(int dependency in steps[i].DependsOn)pending.Push(dependency);
        }
        return result.OrderBy(i=>i).ToArray();
    }
    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly int X, Y, Z;
        public Cell(int x, int y, int z) { X=x; Y=y; Z=z; }
        public bool Equals(Cell other) => X==other.X && Y==other.Y && Z==other.Z;
        public override bool Equals(object? other) => other is Cell c && Equals(c);
        public override int GetHashCode() => HashCode.Combine(X,Y,Z);
    }
    public sealed class Edge
    {
        public Cell To { get; set; }
        public LargeProjectStep[] Parts { get; set; } = Array.Empty<LargeProjectStep>();
    }
    public sealed class Result
    {
        public string Reason { get; set; } = "no_route_in_region";
        public Cell Source { get; set; }
        public Edge[] Edges { get; set; } = Array.Empty<Edge>();
        public int Visited { get; set; }
        public int GeneratedStates { get; set; }
        public int PeakOpenStates { get; set; }
        public int ClosestRemainingCost { get; set; } = -1;
        public bool Found => Reason == "candidate";
    }
    public static Result Find(IEnumerable<Cell> sources, ISet<Cell> targets,
        Func<Cell,IEnumerable<Edge>> expand, int partBudget, int nodeBudget=2048)
        => Find(sources, targets, (cell, _) => expand(cell), partBudget, nodeBudget);

    public static bool ConfirmedHeightExit(bool nativeRoadConnected,bool? registeredLandingConnected)
        => nativeRoadConnected || registeredLandingConnected==true;

    public static Cell StraightStairLandingCandidate(Cell anchor,int rotation)
    {
        var direction=rotation switch {0=>(X:0,Y:-1),1=>(X:-1,Y:0),2=>(X:0,Y:1),3=>(X:1,Y:0),_=>throw new ArgumentException("stair_rotation")};
        return new Cell(anchor.X+direction.X,anchor.Y+direction.Y,anchor.Z+1);
    }

    // The lower exit is opposite the rise, at the anchor's height. Neither
    // candidate proves connectivity: both still require a native road check.
    public static Cell StraightStairLowerCandidate(Cell anchor,int rotation)
    {
        var upper=StraightStairLandingCandidate(anchor,rotation);
        return new Cell(2*anchor.X-upper.X,2*anchor.Y-upper.Y,anchor.Z);
    }

    public static bool AllowsGenericStairExit(Cell lowerCandidate,Cell destination,int heightChange)
        => heightChange==0 && lowerCandidate.Equals(destination);

    // The caller confirms a native edge or a restored registered landing preview.
    // The destination remains a normal, validated path rather than a free source.
    public static IEnumerable<Cell> HeightExits(Cell from, Func<Cell,bool> connected)
    {
        foreach(var d in new[]{(X:0,Y:-1),(X:-1,Y:0),(X:0,Y:1),(X:1,Y:0)})
            foreach(int dz in new[]{-1,1})
            {
                var to=new Cell(from.X+d.X,from.Y+d.Y,from.Z+dz);
                if(connected(to)) yield return to;
            }
    }

    private sealed class State
    {
        public Cell Cell, Source;
        public int RemainingCost;
        public LargeProjectStep[] Parts=Array.Empty<LargeProjectStep>();
        public Edge[] Edges=Array.Empty<Edge>();
    }

    private static string GeometryKey(IEnumerable<LargeProjectStep> parts) => string.Join(";",parts
        .Select(p=>$"{p.Template.Length}:{p.Template}:{p.X}:{p.Y}:{p.Z}:{p.Rotation}")
        .OrderBy(p=>p,StringComparer.Ordinal));

    public static Result Find(IEnumerable<Cell> sources, ISet<Cell> targets,
        Func<Cell,IReadOnlyList<LargeProjectStep>,IEnumerable<Edge>> expand, int partBudget, int nodeBudget=2048,
        Func<Cell,int>? remainingCost=null)
    {
        if(partBudget<0 || partBudget>32 || nodeBudget is <1 or >8192) throw new ArgumentException("route_budget");
        // Equal endpoints with different built geometry are different search states.
        // Merging them by distance alone can discard the only non-colliding detour.
        var seen=new HashSet<(Cell,string)>();
        var open=new List<State>();
        var estimates=new Dictionary<Cell,int>();
        int Estimate(Cell cell)
        {
            if(!estimates.TryGetValue(cell,out int value))
            {
                value=remainingCost?.Invoke(cell)??0;
                if(value<0 || value>16384) throw new ArgumentException("route_estimate");
                estimates.Add(cell,value);
            }
            return value;
        }
        foreach(var source in sources.Distinct().OrderBy(c=>c.Z).ThenBy(c=>c.X).ThenBy(c=>c.Y))
        { seen.Add((source,"")); open.Add(new State{Cell=source,Source=source,RemainingCost=Estimate(source)}); }
        var result=new Result{GeneratedStates=seen.Count,PeakOpenStates=open.Count};
        while(open.Count>0)
        {
            if(result.Visited>=nodeBudget) { result.Reason="search_budget_exhausted"; return result; }
            int best=0;
            for(int i=1;i<open.Count;i++)
            {
                int score=open[i].Parts.Length+open[i].RemainingCost;
                int bestScore=open[best].Parts.Length+open[best].RemainingCost;
                if(score<bestScore || score==bestScore&&open[i].RemainingCost<open[best].RemainingCost) best=i;
            }
            var current=open[best]; open.RemoveAt(best);
            var cell=current.Cell;
            result.Visited++;
            result.ClosestRemainingCost=result.ClosestRemainingCost<0?current.RemainingCost:
                Math.Min(result.ClosestRemainingCost,current.RemainingCost);
            if(targets.Contains(cell))
            {
                result.Source=current.Source; result.Edges=current.Edges; result.Reason="candidate"; return result;
            }
            foreach(var edge in expand(cell,current.Parts))
            {
                if(edge.Parts.Length<1 || edge.Parts.Length>32) continue;
                // A route is a simple path; loops only spend the bounded build budget.
                if(edge.To.Equals(current.Source)||current.Edges.Any(e=>e.To.Equals(edge.To))) continue;
                if(current.Parts.Length+edge.Parts.Length>partBudget) continue;
                int estimate=Estimate(edge.To);
                // Caller supplies an admissible lower bound; zero preserves generic search.
                if(current.Parts.Length+edge.Parts.Length+estimate>partBudget) continue;
                var parts=current.Parts.Concat(edge.Parts).ToArray();
                if(!seen.Add((edge.To,GeometryKey(parts)))) continue;
                result.GeneratedStates=seen.Count;
                // Bound queued geometry variants as well as expanded states.
                if(seen.Count>nodeBudget*16) { result.Reason="search_budget_exhausted"; return result; }
                open.Add(new State{Cell=edge.To,Source=current.Source,Parts=parts,RemainingCost=estimate,
                    Edges=current.Edges.Concat(new[]{edge}).ToArray()});
                result.PeakOpenStates=Math.Max(result.PeakOpenStates,open.Count);
            }
        }
        return result;
    }
}
