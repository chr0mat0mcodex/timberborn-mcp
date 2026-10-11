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
        public bool Found => Reason == "candidate";
    }
    public static Result Find(IEnumerable<Cell> sources, ISet<Cell> targets,
        Func<Cell,IEnumerable<Edge>> expand, int partBudget, int nodeBudget=2048)
    {
        if(partBudget<0 || partBudget>32 || nodeBudget is <1 or >8192) throw new ArgumentException("route_budget");
        var distance=new Dictionary<Cell,int>();
        var previous=new Dictionary<Cell,(Cell From,Edge Edge)>();
        var open=new List<Cell>(); var done=new HashSet<Cell>();
        foreach(var source in sources.Distinct().OrderBy(c=>c.Z).ThenBy(c=>c.X).ThenBy(c=>c.Y))
        { distance[source]=0; open.Add(source); }
        var result=new Result();
        while(open.Count>0)
        {
            if(result.Visited>=nodeBudget) { result.Reason="search_budget_exhausted"; return result; }
            int best=0;
            for(int i=1;i<open.Count;i++) if(distance[open[i]]<distance[open[best]]) best=i;
            var cell=open[best]; open.RemoveAt(best);
            if(!done.Add(cell)) continue;
            result.Visited++;
            if(targets.Contains(cell))
            {
                var route=new List<Edge>();
                while(previous.TryGetValue(cell,out var p)) { route.Add(p.Edge); cell=p.From; }
                route.Reverse(); result.Source=cell; result.Edges=route.ToArray(); result.Reason="candidate"; return result;
            }
            foreach(var edge in expand(cell))
            {
                if(edge.Parts.Length<1 || edge.Parts.Length>32 || done.Contains(edge.To)) continue;
                int next=distance[cell]+edge.Parts.Length;
                if(next>partBudget || distance.TryGetValue(edge.To,out int old)&&old<=next) continue;
                if(!distance.ContainsKey(edge.To)) open.Add(edge.To);
                distance[edge.To]=next; previous[edge.To]=(cell,edge);
            }
        }
        return result;
    }
}
