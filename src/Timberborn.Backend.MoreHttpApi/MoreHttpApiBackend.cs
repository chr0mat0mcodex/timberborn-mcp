using System.Text.Json;
using Timberborn.Backend.Abstractions;
using Timberborn.Contracts;

namespace Timberborn.Backend.MoreHttpApi;

public sealed class MoreHttpApiBackend(MoreHttpApiOptions options, HttpMessageHandler? testHandler = null)
    : ITimberbornReadBackend, ITimberbornWriteBackend, IDisposable
{
    private readonly LoopbackHttpTransport transport = new(options, testHandler);
    public string Id => "more-http-api";
    public bool Simulated => false;
    public bool WritesEnabled => options.EnableWrites;
    public Task SetBuildingPausedAsync(Guid id, bool paused, CancellationToken ct) => transport.SetBuildingPausedAsync(id, paused, ct);
    public async Task PingAsync(CancellationToken ct) => await transport.GetAsync("ping", ct);
    private async Task<JsonElement> Fetch(string route, CancellationToken ct) =>
        await transport.GetAsync(route, ct) ?? throw Faults.Exception("backend_incompatible");
    public async Task<GameInfo> GetGameInfoAsync(CancellationToken ct)
    {
        var json = await Fetch("misc", ct);
        var version = String(Prop(Prop(json, "GameVersion"), "Full"));
        var mods = Array(Prop(json, "Mods"));
        Limit(mods.Length);
        return new(version, mods.Select(m => new ModInfo(String(Prop(m, "Id")), String(Prop(m, "Name")),
            String(Prop(Prop(m, "Version"), "Full")), Bool(Prop(m, "Active")))).ToArray());
    }
    public async Task<LiveSnapshot> GetLiveAsync(CancellationToken ct)
    {
        var top = Prop(await Fetch("live-data", ct), "TopBar");
        var cycle = Prop(top, "Cycle"); var weather = Prop(top, "Weather");
        double progress = Number(Prop(cycle, "Hours"));
        if (progress < 0 || progress >= 1) throw Faults.Exception("backend_incompatible");
        bool show = Bool(Prop(weather, "ShouldShowNext"));
        string current = Weather(String(Prop(Prop(weather, "Current"), "Id")));
        string? next = null; double? days = null;
        if (show)
        {
            var upcoming = Prop(weather, "Next");
            next = Weather(String(Prop(Prop(upcoming, "Weather"), "Id")));
            days = Number(Prop(upcoming, "ComingInDays"));
            if (days < 0) throw Faults.Exception("backend_incompatible");
        }
        return new(new(Integer(Prop(cycle, "Cycle")), Integer(Prop(cycle, "Day")), progress),
            new(current, next, days, show), Number(Prop(Prop(top, "Speed"), "Speed")));
    }
    public async Task<IReadOnlyList<CharacterSummary>> GetPopulationAsync(CancellationToken ct)
    {
        var json = await Fetch("characters", ct);
        var result = new List<CharacterSummary>();
        var seen = new HashSet<Guid>();
        foreach (var (field, kind) in new[] { ("Adult", "adult"), ("Child", "child"), ("Bot", "bot") })
        {
            foreach (var c in Array(Prop(json, field)))
            {
                ct.ThrowIfCancellationRequested(); Limit(result.Count + 1);
                var id = GuidValue(Prop(Prop(c, "Entity"), "EntityId"));
                if (!seen.Add(id)) throw Faults.Exception("backend_incompatible");
                result.Add(new(id, kind, OptionalString(c, "Name"), OptionalNumber(c, "Age"),
                    OptionalNumber(c, "Wellbeing"), OptionalGuid(c, "Dwelling"), OptionalGuid(c, "Workplace"), OptionalGuid(c, "District")));
            }
        }
        return result;
    }
    public async Task<IReadOnlyList<BuildingSummary>> GetBuildingsAsync(CancellationToken ct)
    {
        var json = await Fetch("buildings", ct);
        var result = new List<BuildingSummary>(); var seen = new HashSet<Guid>();
        foreach (var group in Array(Prop(json, "Groups")))
            foreach (var b in Array(Prop(group, "Buildings")))
            {
                ct.ThrowIfCancellationRequested(); Limit(result.Count + 1);
                var mapped = MapBuilding(b);
                if (!seen.Add(mapped.Id)) throw Faults.Exception("backend_incompatible");
                result.Add(mapped);
            }
        return result;
    }
    public async Task<BuildingSummary> GetBuildingAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) throw Faults.Exception("invalid_argument");
        // The upstream single-entity route does not itself prove the entity is a building.
        var buildings = await GetBuildingsAsync(ct);
        if (!buildings.Any(b => b.Id == id)) throw Faults.Exception("entity_not_found");
        var mapped = MapBuilding(await Fetch("buildings/" + id.ToString("D"), ct));
        if (mapped.Id != id) throw Faults.Exception("backend_incompatible");
        return mapped;
    }
    private static BuildingSummary MapBuilding(JsonElement b)
    {
        var id = GuidValue(Prop(Prop(b, "Entity"), "EntityId"));
        var template = String(Prop(b, "TemplateName"));
        var name = b.TryGetProperty("Name", out var n) && n.ValueKind == JsonValueKind.Object ? OptionalString(n, "EntityName") : null;
        if (string.IsNullOrWhiteSpace(name)) name = OptionalString(b, "LabelName");
        if (string.IsNullOrWhiteSpace(name)) name = template;
        bool? pausable = null, paused = null;
        if (b.TryGetProperty("Pausable", out var p) && p.ValueKind != JsonValueKind.Null)
        {
            pausable = Bool(Prop(p, "Pausable"));
            paused = pausable == true ? Bool(Prop(p, "IsPaused")) : null;
        }
        return new(id, name, template, pausable, paused);
    }
    private void Limit(int count) { if (count > options.MaxEntities) throw Faults.Exception("response_too_large"); }
    private static string Weather(string id) => id switch
    { "TemperateWeather" => "temperate", "DroughtWeather" => "drought", "BadtideWeather" => "badtide", _ => "unknown" };
    private static JsonElement Prop(JsonElement e, string key) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v)
        ? v : throw Faults.Exception("backend_incompatible");
    private static JsonElement[] Array(JsonElement e) => e.ValueKind == JsonValueKind.Array
        ? e.EnumerateArray().ToArray() : throw Faults.Exception("backend_incompatible");
    private static string String(JsonElement e) => e.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(e.GetString())
        ? e.GetString()! : throw Faults.Exception("backend_incompatible");
    private static bool Bool(JsonElement e) => e.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? e.GetBoolean() : throw Faults.Exception("backend_incompatible");
    private static double Number(JsonElement e) => e.ValueKind == JsonValueKind.Number && e.TryGetDouble(out var d) && double.IsFinite(d)
        ? d : throw Faults.Exception("backend_incompatible");
    private static int Integer(JsonElement e) => e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var i) && i >= 0
        ? i : throw Faults.Exception("backend_incompatible");
    private static Guid GuidValue(JsonElement e) => Guid.TryParse(String(e), out var id) && id != Guid.Empty
        ? id : throw Faults.Exception("backend_incompatible");
    private static string? OptionalString(JsonElement e, string key)
    {
        if (!e.TryGetProperty(key, out var v) || v.ValueKind == JsonValueKind.Null) return null;
        if (v.ValueKind != JsonValueKind.String) throw Faults.Exception("backend_incompatible");
        return v.GetString();
    }
    private static double? OptionalNumber(JsonElement e, string key) => !e.TryGetProperty(key, out var v) || v.ValueKind == JsonValueKind.Null ? null : Number(v);
    private static Guid? OptionalGuid(JsonElement e, string key) => !e.TryGetProperty(key, out var v) || v.ValueKind == JsonValueKind.Null ? null : GuidValue(v);
    public void Dispose() => transport.Dispose();
}
