using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Abstractions;
using Timberborn.Contracts;

namespace Timberborn.Application;

public sealed class ObservationService(ITimberbornReadBackend backend)
{
    private readonly ConcurrentDictionary<string, Capability> checkedCapabilities = new();
    private string? gameVersion;
    public string BackendId => backend.Id;
    public bool Simulated => backend.Simulated;
    public static readonly IReadOnlyDictionary<string, Type> ResultTypes = new Dictionary<string, Type>
    {
        ["timberborn_status"] = typeof(ToolResult<StatusData>),
        ["inspect_colony"] = typeof(ToolResult<ColonySnapshot>),
        ["inspect_population"] = typeof(ToolResult<PopulationSnapshot>),
        ["find_buildings"] = typeof(ToolResult<BuildingPage>),
        ["inspect_building"] = typeof(ToolResult<BuildingSummary>)
    };

    public async Task<JsonObject> InvokeAsync(string name, JsonElement arguments, CancellationToken ct)
    {
        var start = DateTimeOffset.UtcNow;
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(20));
        var warnings = new List<Warning>();
        try
        {
            var args = new Arguments(arguments);
            object data = name switch
            {
                "timberborn_status" => await StatusAsync(args, warnings, budget.Token),
                "inspect_colony" => await ColonyAsync(args, warnings, budget.Token),
                "inspect_population" => await PopulationAsync(args, warnings, budget.Token),
                "find_buildings" => await BuildingsAsync(args, warnings, budget.Token),
                "inspect_building" => await BuildingAsync(args, warnings, budget.Token),
                _ => throw Faults.Exception("invalid_argument")
            };
            budget.Token.ThrowIfCancellationRequested();
            var result = Envelope(data, null, warnings, start);
            FitPage(result);
            return result;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { return Envelope(null, Faults.Create("timeout"), [], start); }
        catch (BackendException e)
        { ct.ThrowIfCancellationRequested(); return Envelope(null, e.Fault, [], start); }
        catch (Exception) when (!ct.IsCancellationRequested)
        { return Envelope(null, Faults.Create("internal_error"), [], start); }
    }

    private JsonObject Envelope(object? data, Fault? error, List<Warning> warnings, DateTimeOffset start)
    {
        bool partial = warnings.Any(w => w.Code is not ("name_truncated" or "page_size_reduced"));
        if (warnings.Count > 10)
            warnings = [.. warnings.Take(9), new("warnings_omitted", "Weitere Warnungen zusammengefasst.", "result")];
        return JsonSerializer.SerializeToNode(new ToolResult<object>(1,
            error is not null ? "error" : partial ? "partial" : "ok", data,
            new(backend.Id, backend.Simulated, gameVersion, start, DateTimeOffset.UtcNow, partial), warnings, error),
            ContractJson.Options)!.AsObject();
    }

    // Budget applies to serialized semantic JSON, before the MCP text mirror.
    private static void FitPage(JsonObject result)
    {
        bool reduced = false;
        while (JsonSerializer.SerializeToUtf8Bytes(result, ContractJson.Options).Length > 32768)
        {
            if (result["data"] is not JsonObject data || data["items"] is not JsonArray items ||
                data["page"] is not JsonObject page || items.Count <= 1)
                throw Faults.Exception("response_too_large");
            items.RemoveAt(items.Count - 1);
            page["returned"] = items.Count;
            page["nextOffset"] = page["offset"]!.GetValue<int>() + items.Count;
            if (!reduced)
            {
                var warnings = result["warnings"]!.AsArray();
                if (warnings.Count >= 10) warnings.RemoveAt(warnings.Count - 1);
                warnings.Add(JsonSerializer.SerializeToNode(new Warning("page_size_reduced",
                    "Seite wegen Ausgabegröße verkleinert.", "page"), ContractJson.Options));
                reduced = true;
            }
        }
    }

    private async Task<T> Read<T>(string capability, Func<CancellationToken, Task<T>> read, CancellationToken ct)
    {
        try
        {
            var value = await read(ct);
            checkedCapabilities[capability] = new("available", DateTimeOffset.UtcNow);
            return value;
        }
        catch (BackendException e)
        {
            checkedCapabilities.Clear();
            gameVersion = null;
            if (e.Fault.Code == "capability_unavailable")
                checkedCapabilities[capability] = new("unavailable", DateTimeOffset.UtcNow);
            throw;
        }
        catch
        {
            checkedCapabilities.Clear(); gameVersion = null;
            throw;
        }
    }

    private BackendCapabilities Capabilities()
    {
        Capability One(string name) => checkedCapabilities.TryGetValue(name, out var capability) &&
            DateTimeOffset.UtcNow - capability.CheckedAtUtc < TimeSpan.FromSeconds(30)
            ? capability : new("unknown", null);
        return new(One("gameInfo"), One("liveData"), One("population"), One("buildings"), One("buildingDetails"));
    }

    private async Task<StatusData> StatusAsync(Arguments a, List<Warning> w, CancellationToken ct)
    {
        a.Allow("includeMods");
        bool include = a.Bool("includeMods") ?? false;
        try { await backend.PingAsync(ct); }
        catch (BackendException e)
        {
            checkedCapabilities.Clear(); gameVersion = null;
            return new(e.Fault.Code == "authentication_failed" ? "unauthorized" :
                e.Fault.Code == "backend_unavailable" ? "unreachable" : "unknown",
                null, null, Capabilities(), null, e.Fault);
        }
        try
        {
            var info = await Read("gameInfo", backend.GetGameInfoAsync, ct);
            gameVersion = info.GameVersion;
            var mods = info.Mods.Select(m => m with { Name = Short(m.Name, w, "mods")! }).ToArray();
            return new("reachable", null, mods.FirstOrDefault(m => m.Id == "MoreHttpApi" && m.Enabled)?.Version,
                Capabilities(), include ? mods : null, null);
        }
        catch (BackendException e)
        {
            w.Add(new(e.Fault.Code, e.Fault.Message, "gameInfo"));
            return new("reachable", null, null, Capabilities(), null, e.Fault);
        }
    }

    private async Task<ColonySnapshot> ColonyAsync(Arguments a, List<Warning> w, CancellationToken ct)
    {
        a.Allow("detail");
        var detail = a.Choice("detail", "summary", "summary", "standard");
        async Task<T?> Part<T>(string section, Func<CancellationToken, Task<T>> get) where T : class
        {
            try { return await Read(section, get, ct); }
            catch (BackendException e) { w.Add(new(e.Fault.Code, e.Fault.Message, section)); return null; }
        }
        var live = await Part("liveData", backend.GetLiveAsync);
        var pop = await Part("population", backend.GetPopulationAsync);
        var buildings = await Part("buildings", backend.GetBuildingsAsync);
        if (live is null && pop is null && buildings is null) throw Faults.Exception(w[0].Code);
        var weather = live?.Weather;
        if (weather?.Current == "unknown" || weather?.Next == "unknown")
            w.Add(new("unknown_weather", "Unbekannte Wetter-ID.", "weather"));
        return new(live?.Time, weather, pop is null ? null : Counts(pop), buildings is null ? null : Aggregate(buildings),
            detail == "standard" ? live?.GameSpeed : null, "unsupported");
    }

    public static PopulationCounts Counts(IReadOnlyList<CharacterSummary> items)
    {
        int adults = items.Count(c => c.Kind == "adult"), children = items.Count(c => c.Kind == "child"),
            bots = items.Count(c => c.Kind == "bot");
        return new(adults, children, adults + children, bots, items.Count);
    }
    public static BuildingAggregate Aggregate(IReadOnlyList<BuildingSummary> items)
    {
        int paused = items.Count(b => b.Pausable == true && b.Paused == true),
            active = items.Count(b => b.Pausable == true && b.Paused == false),
            notPausable = items.Count(b => b.Pausable == false);
        return new(items.Count, paused, active, notPausable, items.Count - paused - active - notPausable);
    }
    private async Task<PopulationSnapshot> PopulationAsync(Arguments a, List<Warning> w, CancellationToken ct)
    {
        a.Allow("detail", "kind", "offset", "limit");
        var detail = a.Choice("detail", "summary", "summary", "list");
        var kind = a.Choice("kind", "all", "all", "adult", "child", "bot");
        if (detail == "summary" && (a.Has("offset") || a.Has("limit"))) throw Faults.Exception("invalid_argument");
        int offset = a.Int("offset", 0, 0, int.MaxValue), limit = a.Int("limit", 25, 1, 100);
        var all = await Read("population", backend.GetPopulationAsync, ct);
        var filtered = all.Where(c => kind == "all" || c.Kind == kind).OrderBy(c => c.Id.ToString(), StringComparer.Ordinal).ToArray();
        if (detail == "summary") return new(Counts(all), filtered.Length, null, null);
        var page = filtered.Skip(offset).Take(limit).Select(c => c with { Name = Short(c.Name, w, "population") }).ToArray();
        return new(Counts(all), filtered.Length, page, Paging(offset, limit, page.Length, filtered.Length));
    }

    private async Task<BuildingPage> BuildingsAsync(Arguments a, List<Warning> w, CancellationToken ct)
    {
        a.Allow("nameContains", "template", "paused", "id", "offset", "limit");
        var name = a.Text("nameContains", 120); var template = a.Text("template", 200);
        var paused = a.Bool("paused"); var id = a.Id("id");
        int offset = a.Int("offset", 0, 0, int.MaxValue), limit = a.Int("limit", 25, 1, 100);
        var all = await Read("buildings", backend.GetBuildingsAsync, ct);
        var filtered = all.Where(b => (name is null || b.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
            (template is null || b.Template == template) && (id is null || b.Id == id) &&
            (paused is null || (b.Pausable == true && b.Paused == paused)))
            .OrderBy(b => b.Template, StringComparer.Ordinal).ThenBy(b => b.Id.ToString(), StringComparer.Ordinal).ToArray();
        var items = filtered.Skip(offset).Take(limit).Select(b => b with { Name = Short(b.Name, w, "buildings")! }).ToArray();
        return new(items, Paging(offset, limit, items.Length, filtered.Length));
    }
    private async Task<BuildingSummary> BuildingAsync(Arguments a, List<Warning> w, CancellationToken ct)
    {
        a.Allow("id"); var id = a.Id("id") ?? throw Faults.Exception("invalid_argument");
        var b = await Read("buildingDetails", token => backend.GetBuildingAsync(id, token), ct);
        return b with { Name = Short(b.Name, w, "building")! };
    }
    private static Page Paging(int offset, int limit, int returned, int total) =>
        new(offset, limit, returned, total, (long)offset + returned < total ? offset + returned : null);
    private static string? Short(string? text, List<Warning> warnings, string section)
    {
        if (text is null || text.EnumerateRunes().Count() <= 200) return text;
        if (!warnings.Any(w => w.Code == "name_truncated" && w.Section == section))
            warnings.Add(new("name_truncated", "Langer Anzeigename gekürzt.", section));
        return string.Concat(text.EnumerateRunes().Take(200).Select(r => r.ToString()));
    }
}

internal sealed class Arguments(JsonElement value)
{
    public bool Has(string key) => value.ValueKind == JsonValueKind.Object && value.TryGetProperty(key, out _);
    public void Allow(params string[] keys)
    {
        if (value.ValueKind != JsonValueKind.Object) throw Faults.Exception("invalid_argument");
        if (value.EnumerateObject().Any(p => !keys.Contains(p.Name) || p.Value.ValueKind == JsonValueKind.Null))
            throw Faults.Exception("invalid_argument");
    }
    public string? Text(string key, int max)
    {
        if (!value.TryGetProperty(key, out var v)) return null;
        if (v.ValueKind != JsonValueKind.String) throw Faults.Exception("invalid_argument");
        var text = v.GetString()!.Trim();
        if (text.Length == 0 || text.Length > max) throw Faults.Exception("invalid_argument");
        return text;
    }
    public string Choice(string key, string fallback, params string[] choices)
    { var s = Text(key, 30) ?? fallback; return choices.Contains(s) ? s : throw Faults.Exception("invalid_argument"); }
    public bool? Bool(string key)
    {
        if (!value.TryGetProperty(key, out var v)) return null;
        return v.ValueKind is JsonValueKind.True or JsonValueKind.False ? v.GetBoolean() : throw Faults.Exception("invalid_argument");
    }
    public int Int(string key, int fallback, int min, int max)
    {
        if (!value.TryGetProperty(key, out var v)) return fallback;
        return v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i) && i >= min && i <= max
            ? i : throw Faults.Exception("invalid_argument");
    }
    public Guid? Id(string key)
    {
        var s = Text(key, 36); if (s is null) return null;
        return Guid.TryParseExact(s, "D", out var id) && id != Guid.Empty ? id : throw Faults.Exception("invalid_argument");
    }
}
