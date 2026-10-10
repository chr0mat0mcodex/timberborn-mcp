using System.Collections.Specialized;
using System.Globalization;

namespace Timberborn.Bridge.Core;

// Bounded explicit 3D blueprint. Coordinates are proposals, never placement authority.
public sealed class LargeProjectStep
{
    public string Template { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Rotation { get; set; }
    public int[] DependsOn { get; set; } = Array.Empty<int>();
}

public sealed class LargeProjectRequest
{
    public const int MaxParts = 32;
    public const int MaxHeightSpan = 8;
    public const int MaxResponseBytes = 1024 * 1024;
    public string Operation { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string DistrictId { get; private set; } = "";
    public string ActionId { get; private set; } = "";
    public string PlanKey { get; private set; } = "";
    public string EntityId { get; private set; } = "";
    public LargeProjectStep[] Steps { get; private set; } = Array.Empty<LargeProjectStep>();
    public static bool Handles(string path) => path is "/agent-api/v1/large-project-plan" or "/agent-api/v1/large-project-start" or "/agent-api/v1/large-project-advance" or "/agent-api/v1/large-project-inspect" or "/agent-api/v1/large-project-stop" or "/agent-api/v1/power-network";
    public static bool Writes(string operation) => operation is "plan" or "start" or "advance" or "stop"; // plan uses temporary native previews

    public static LargeProjectRequest Parse(string path, NameValueCollection q)
    {
        if (!Handles(path)) throw new ArgumentException("unknown_large_project_operation");
        string operation = path.Substring(path.LastIndexOf('/') + 1).Replace("large-project-", "");
        string One(string key) => q.GetValues(key) is { Length: 1 } values ? values[0] : throw new ArgumentException("missing_or_duplicate_" + key);
        string Id(string key) => Guid.TryParseExact(One(key), "D", out var id) && id != Guid.Empty ? id.ToString("D") : throw new ArgumentException("invalid_" + key);
        var r = new LargeProjectRequest { Operation = operation, Session = Id("session") };
        string[] keys;
        if (operation == "power-network") { r.EntityId = Id("id"); keys = new[] { "session", "id" }; }
        else if (operation is "plan" or "start")
        {
            r.DistrictId = Id("districtId"); r.Steps = Decode(One("steps"));
            keys = operation == "plan" ? new[] { "session", "districtId", "steps" } : new[] { "session", "districtId", "steps", "actionId", "planKey", "mode" };
            if (operation == "start")
            {
                r.ActionId = Id("actionId"); r.PlanKey = One("planKey");
                if (One("mode") != "development_pilot" || r.PlanKey.Length != 64 || r.PlanKey.Any(c => !(c is >= '0' and <= '9' or >= 'a' and <= 'f'))) throw new ArgumentException("invalid_plan_or_mode");
            }
        }
        else { r.ActionId = Id("actionId"); keys = new[] { "session", "actionId" }; }
        if (q.Count != keys.Length || q.AllKeys.Any(k => !keys.Contains(k))) throw new ArgumentException("unexpected_project_argument");
        return r;
    }

    public static string Encode(LargeProjectStep[] steps)
    {
        Validate(steps);
        var encoded = string.Join(";", steps.Select(s => string.Join(",", s.Template, s.X.ToString(CultureInfo.InvariantCulture), s.Y.ToString(CultureInfo.InvariantCulture), s.Z.ToString(CultureInfo.InvariantCulture), s.Rotation.ToString(CultureInfo.InvariantCulture), string.Join(".", s.DependsOn.OrderBy(x => x)))));
        if(encoded.Length>8000)throw new ArgumentException("project_size");
        return encoded;
    }
    public static LargeProjectStep[] Decode(string encoded)
    {
        if (encoded.Length is < 1 or > 8000) throw new ArgumentException("project_size");
        int Number(string value) => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int n) ? n : throw new ArgumentException("invalid_project_number");
        var items = encoded.Split(';');
        if (items.Length > MaxParts) throw new ArgumentException("project_parts_limit");
        var steps = items.Select(item => {
            var p = item.Split(','); if (p.Length != 6) throw new ArgumentException("invalid_project_step");
            return new LargeProjectStep { Template = p[0], X = Number(p[1]), Y = Number(p[2]), Z = Number(p[3]), Rotation = Number(p[4]), DependsOn = p[5].Length == 0 ? Array.Empty<int>() : p[5].Split('.').Select(Number).ToArray() };
        }).ToArray();
        Validate(steps); return steps;
    }
    public static void Validate(LargeProjectStep[] steps)
    {
        if (steps is null || steps.Length is < 1 or > MaxParts || steps.Any(s => s is null || !BuildingPolicy.ValidTemplate(s.Template) || s.X is < 0 or > 4095 || s.Y is < 0 or > 4095 || s.Z is < 0 or > 4095 || s.Rotation is < 0 or > 3 || s.DependsOn is null || s.DependsOn.Length > MaxParts)) throw new ArgumentException("invalid_project_steps");
        if (steps.Max(s => s.Z) - steps.Min(s => s.Z) >= MaxHeightSpan || steps.Max(s => s.X) - steps.Min(s => s.X) >= 32 || steps.Max(s => s.Y) - steps.Min(s => s.Y) >= 32) throw new ArgumentException("project_extent_limit");
        if (steps.Select(s => (s.Template,s.X,s.Y,s.Z,s.Rotation)).Distinct().Count() != steps.Length) throw new ArgumentException("duplicate_project_step");
        for (int i = 0; i < steps.Length; i++) if (steps[i].DependsOn.Any(d => d < 0 || d >= steps.Length || d == i) || steps[i].DependsOn.Distinct().Count() != steps[i].DependsOn.Length) throw new ArgumentException("invalid_project_dependency");
        Order(steps);
    }
    public static int[] Order(LargeProjectStep[] steps)
    {
        var ordered = new List<int>();
        while (ordered.Count < steps.Length)
        {
            int next = Enumerable.Range(0, steps.Length).Where(i => !ordered.Contains(i) && steps[i].DependsOn.All(ordered.Contains)).DefaultIfEmpty(-1).First();
            if (next < 0) throw new ArgumentException("cyclic_project_dependency");
            ordered.Add(next);
        }
        return ordered.ToArray();
    }
}
