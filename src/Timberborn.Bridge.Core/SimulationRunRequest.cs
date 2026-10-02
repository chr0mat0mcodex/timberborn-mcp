using System.Collections.Specialized;
using System.Globalization;

namespace Timberborn.Bridge.Core;

public sealed class SimulationRunRequest
{
    public string Route { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string RunId { get; private set; } = "";
    public double DurationHours { get; private set; }
    public double TargetHours { get; private set; }
    public int Speed { get; private set; }
    public int ExpectedSpeed { get; private set; }
    public int MaxRealSeconds { get; private set; }
    public bool Starts => Route is "simulation-run-for" or "simulation-run-until";
    public bool Writes => Route != "simulation-run";
    public static bool Handles(string path) => path is "/agent-api/v1/simulation-run-for" or "/agent-api/v1/simulation-run-until" or "/agent-api/v1/simulation-run" or "/agent-api/v1/simulation-run-cancel";

    public static SimulationRunRequest Parse(string path, NameValueCollection q)
    {
        if (!Handles(path)) throw new ArgumentException();
        var r = new SimulationRunRequest { Route = path.Substring("/agent-api/v1/".Length) };
        var keys = new List<string> { "session", "runId" };
        if (r.Starts) keys.AddRange(new[] { "speed", "expectedSpeed", "maxRealSeconds" });
        if (r.Route == "simulation-run-for") keys.AddRange(new[] { "duration", "unit" });
        if (r.Route == "simulation-run-until") keys.AddRange(new[] { "dayNumber", "hour" });
        if (q.Count != keys.Count || q.AllKeys.Any(k => k is null || !keys.Contains(k))) throw new ArgumentException();
        string Read(string key) { var v = q.GetValues(key); if (v is null || v.Length != 1) throw new ArgumentException(); return v[0]; }
        string Id(string key) { if (!Guid.TryParseExact(Read(key), "D", out var id) || id == Guid.Empty) throw new ArgumentException(); return id.ToString("D"); }
        double Number(string key, double min, double max) {
            if (!double.TryParse(Read(key), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double n) || double.IsNaN(n) || double.IsInfinity(n) || n < min || n > max) throw new ArgumentException(); return n;
        }
        int Integer(string key, int min, int max) { double n = Number(key, min, max); if (n != Math.Floor(n)) throw new ArgumentException(); return (int)n; }
        r.Session = Id("session"); r.RunId = Id("runId");
        if (r.Starts) {
            r.Speed = Integer("speed", 1, 7); r.ExpectedSpeed = Integer("expectedSpeed", 0, 7);
            if (r.Speed is not (1 or 3 or 7) || r.ExpectedSpeed is not (0 or 1 or 3 or 7)) throw new ArgumentException();
            r.MaxRealSeconds = Integer("maxRealSeconds", 30, 86400);
        }
        if (r.Route == "simulation-run-for") {
            double factor = Read("unit") switch { "hours" => 1, "days" => 24, "weeks" => 168, _ => throw new ArgumentException() };
            r.DurationHours = Number("duration", 0.000001, 672) * factor;
            if (r.DurationHours > 672) throw new ArgumentException();
        }
        if (r.Route == "simulation-run-until") {
            int day = Integer("dayNumber", 0, 1000000); double hour = Number("hour", 0, 24);
            if (hour >= 24) throw new ArgumentException(); r.TargetHours = day * 24d + hour;
        }
        return r;
    }
}
