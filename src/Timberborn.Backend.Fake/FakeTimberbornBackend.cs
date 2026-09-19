using Timberborn.Backend.Abstractions;
using Timberborn.Contracts;

namespace Timberborn.Backend.Fake;

public sealed class FakeTimberbornBackend(string scenario = "healthy", bool enableWrites = false) : ITimberbornReadBackend, ITimberbornWriteBackend
{
    private readonly Dictionary<Guid, bool> pauseStates = [];
    public bool WritesEnabled => enableWrites;
    public async Task SetBuildingPausedAsync(Guid id, bool paused, CancellationToken ct)
    {
        if (!WritesEnabled) throw Faults.Exception("writes_disabled");
        var building = await GetBuildingAsync(id, ct);
        if (building.Pausable != true) throw Faults.Exception("not_pausable");
        lock (pauseStates) pauseStates[id] = paused;
    }
    private bool State(Guid id, bool fallback)
    { lock (pauseStates) return pauseStates.GetValueOrDefault(id, fallback); }
    public string Id => "fake";
    public bool Simulated => true;
    public static readonly Guid BuildingId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private void Check(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (scenario == "offline") throw Faults.Exception("backend_unavailable");
    }
    public Task PingAsync(CancellationToken ct) { Check(ct); return Task.CompletedTask; }
    public Task<GameInfo> GetGameInfoAsync(CancellationToken ct)
    { Check(ct); return Task.FromResult(new GameInfo("simulated", [])); }
    public Task<LiveSnapshot> GetLiveAsync(CancellationToken ct)
    {
        Check(ct);
        return Task.FromResult(new LiveSnapshot(new(1, 1, 0.25), new("temperate", null, null, false), 0));
    }
    public Task<IReadOnlyList<CharacterSummary>> GetPopulationAsync(CancellationToken ct)
    {
        Check(ct);
        IReadOnlyList<CharacterSummary> items = Enumerable.Range(1, 17).Select(i => new CharacterSummary(
            Guid.Parse($"10000000-0000-0000-0000-{i:000000000000}"),
            i <= 12 ? "adult" : i <= 15 ? "child" : "bot", $"Test {i}", 10, 5, null, BuildingId, null)).ToArray();
        return Task.FromResult(items);
    }
    public Task<IReadOnlyList<BuildingSummary>> GetBuildingsAsync(CancellationToken ct)
    {
        Check(ct);
        if (scenario == "partial") throw Faults.Exception("backend_error");
        return Task.FromResult<IReadOnlyList<BuildingSummary>>([
            new(BuildingId, "Testzentrum", "DistrictCenter.Folktails", true, State(BuildingId, false)),
            new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Testlager", "Warehouse.Folktails", false, null),
            new(Guid.Parse("00000000-0000-0000-0000-000000000003"), "Testmühle", "Mill.Folktails", true,
                State(Guid.Parse("00000000-0000-0000-0000-000000000003"), true))]);
    }
    public async Task<BuildingSummary> GetBuildingAsync(Guid id, CancellationToken ct) =>
        (await GetBuildingsAsync(ct)).FirstOrDefault(b => b.Id == id) ?? throw Faults.Exception("entity_not_found");
}
