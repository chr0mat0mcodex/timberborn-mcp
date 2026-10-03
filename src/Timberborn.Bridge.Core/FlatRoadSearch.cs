namespace Timberborn.Bridge.Core;

// Candidate routing only. This grid never claims native navigability or build safety.
public static class FlatRoadSearch
{
    // Prefer fewer new roads, then fewer total cells. Existing roads cost no build
    // budget, but remain candidates only: native joint validation is still required.
    public static int[]? FindWithinRoadBudget(int width, int height, int start, int[] footprint,
        bool[] passable, bool[] goals, bool[] existingRoads, int maxNewRoads)
    {
        int count = width * height;
        if (width is < 1 or > 8 || height is < 1 or > 8 || start < 0 || start >= count ||
            passable.Length != count || goals.Length != count || existingRoads.Length != count ||
            maxNewRoads < 0 || footprint.Length > 64 || footprint.Any(i => i < 0 || i >= count))
            throw new ArgumentException();
        var allowed = (bool[])passable.Clone();
        foreach (int cell in footprint) allowed[cell] = false;
        if (!allowed[start]) return null;
        var costs = Enumerable.Repeat(int.MaxValue, count).ToArray();
        var parents = Enumerable.Repeat(-1, count).ToArray();
        var visited = new bool[count];
        // Every simple route has at most count cells, so this encoding is
        // lexicographic: one new road always costs more than the length tie-break.
        int scale = count + 1;
        costs[start] = (existingRoads[start] ? 0 : scale) + 1;
        for (int iteration = 0; iteration < count; iteration++) {
            int current = -1;
            for (int i = 0; i < count; i++)
                if (!visited[i] && costs[i] != int.MaxValue &&
                    (current < 0 || costs[i] < costs[current])) current = i;
            if (current < 0 || costs[current] / scale > maxNewRoads) return null;
            if (goals[current]) {
                var route = new List<int>();
                for (int i = current; i >= 0; i = parents[i]) route.Add(i);
                return route.ToArray();
            }
            visited[current] = true;
            foreach (int next in new[] { current % width > 0 ? current - 1 : -1,
                current % width + 1 < width ? current + 1 : -1,
                current >= width ? current - width : -1, current + width < count ? current + width : -1 }) {
                if (next < 0 || !allowed[next] || visited[next]) continue;
                int cost = costs[current] + (existingRoads[next] ? 0 : scale) + 1;
                if (cost >= costs[next]) continue;
                costs[next] = cost;
                parents[next] = current;
            }
        }
        return null;
    }

    // All supplied footprint cells are excluded before considering any entrance.
    // Candidate order breaks equal-length ties. No native reachability claim.
    public static int[]? FindForBuilding(int width, int height, int[] entrances, int[] footprint,
        bool[] passable, bool[] goals)
    {
        int count = width * height;
        if (width is < 1 or > 8 || height is < 1 or > 8 || passable.Length != count || goals.Length != count ||
            entrances.Length > 8 || footprint.Length > 64 ||
            entrances.Concat(footprint).Any(i => i < 0 || i >= count)) throw new ArgumentException();
        var allowed = (bool[])passable.Clone();
        foreach (int cell in footprint) allowed[cell] = false;
        int[]? best = null;
        foreach (int entry in entrances.Distinct())
        {
            var route = Find(width, height, entry, allowed, goals);
            if (route is not null && (best is null || route.Length < best.Length)) best = route;
        }
        return best;
    }

    public static int[]? Find(int width,int height,int start,bool[] passable,bool[] goals)
    {
        int count=width*height;
        if(width<1||height<1||width>8||height>8||passable.Length!=count||goals.Length!=count||start<0||start>=count)throw new ArgumentException();
        if(!passable[start])return null;
        var parent=Enumerable.Repeat(-2,count).ToArray();var queue=new Queue<int>();parent[start]=-1;queue.Enqueue(start);
        while(queue.Count>0) {
            int p=queue.Dequeue();
            if(goals[p]) { var path=new List<int>();for(int i=p;i>=0;i=parent[i])path.Add(i);return path.ToArray(); }
            foreach(int n in new[]{p%width>0?p-1:-1,p%width+1<width?p+1:-1,p>=width?p-width:-1,p+width<count?p+width:-1})
                if(n>=0&&passable[n]&&parent[n]==-2){parent[n]=p;queue.Enqueue(n);}
        }
        return null;
    }
}
