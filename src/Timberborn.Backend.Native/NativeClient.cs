using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public sealed partial class NativeClient : IDisposable
{
    private readonly HttpClient client;
    private readonly string token;
    public NativeClient(NativeConfiguration config, HttpMessageHandler? handler = null)
    {
        config.Validate(); token = config.Token;
        client = new HttpClient(handler ?? new SocketsHttpHandler
        {
            AllowAutoRedirect = false, UseCookies = false, UseProxy = false,
            ConnectCallback = async (context, ct) =>
            {
                foreach (var ip in new[] { IPAddress.IPv6Loopback, IPAddress.Loopback })
                {
                    var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try { await socket.ConnectAsync(new IPEndPoint(ip, context.DnsEndPoint.Port), ct); return new NetworkStream(socket, true); }
                    catch (SocketException) { socket.Dispose(); }
                    catch { socket.Dispose(); throw; }
                }
                throw new HttpRequestException("Loopback unavailable");
            }
        }) { BaseAddress = new Uri($"http://localhost:{config.Port}/agent-api/v1/"), Timeout = Timeout.InfiniteTimeSpan };
    }
    public async Task<BridgeEnvelope<NativeSnapshot>> Snapshot(CancellationToken ct)
    {
        var result = await Get<NativeSnapshot>("snapshot", ct);
        var data = result.Data;
        if (data.Scope != "global" || data.Resources is null || data.Resources.Length != 3 ||
            data.Resources.Any(r => r is null) ||
            !data.Resources.Select(r => r.Id).Order().SequenceEqual(new[] { "Berries", "Log", "Water" }) ||
            data.Housing is null || data.Population is null || data.Workforce?.Beavers is null || data.Workforce.Bots is null ||
            data.MapSize is null || data.MapSize.X <= 0 || data.MapSize.Y <= 0 || data.MapSize.Z <= 0 ||
            data.ObjectSample is null || data.ObjectSample.Length > 16 || data.ObjectCount < data.ObjectSample.Length ||
            data.ObjectsTruncated != (data.ObjectCount > data.ObjectSample.Length) || data.Limitations is null ||
            data.ObjectSample.Any(o => o is null || o.Id == Guid.Empty || o.ObjectName is null || o.ObjectName.Length > 120 || o.Position is null) ||
            data.Housing.OccupiedBeds < 0 || data.Housing.FreeBeds < 0 || data.Housing.Homeless < 0 ||
            (long)data.Housing.TotalBeds != (long)data.Housing.OccupiedBeds + data.Housing.FreeBeds)
            throw new InvalidDataException("Invalid snapshot");
        return result;
    }
    public async Task<BridgeEnvelope<NativeMap>> Map(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "map") throw new ArgumentException("Invalid map request");
        var result = await Get<NativeMap>($"map?x={r.X}&y={r.Y}&z={r.Z}&width={r.Width}&height={r.Height}&depth={r.Depth}", ct);
        var data = result.Data;
        if (data.Origin != new Position(r.X, r.Y, r.Z) || data.Width != r.Width || data.Height != r.Height ||
            data.Depth != r.Depth || data.Cells is null || data.Cells.Length != r.Width * r.Height * r.Depth ||
            data.Limitations is null || data.Cells.Any(c => c is null) ||
            data.Cells.Select(c => (c.X, c.Y, c.Z)).Distinct().Count() != data.Cells.Length ||
            data.Cells.Any(c => c.X < r.X || c.X >= r.X + r.Width || c.Y < r.Y || c.Y >= r.Y + r.Height ||
                c.Z < r.Z || c.Z >= r.Z + r.Depth || !float.IsFinite(c.WaterDepth) || !float.IsFinite(c.Contamination)))
            throw new InvalidDataException("Invalid map");
        return result;
    }
    public async Task<BridgeEnvelope<NativeObjects>> Objects(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "objects") throw new ArgumentException();
        var result = await Get<NativeObjects>($"objects?offset={r.Offset}&limit={r.Limit}", ct);
        var d = result.Data;
        if (d.Scope != "buildings_and_paths" || d.Offset != r.Offset || d.Limit != r.Limit || d.Total < 0 ||
            d.Items is null || d.Items.Length != Math.Min(r.Limit, Math.Max(0, d.Total - r.Offset)) ||
            d.HasMore != ((long)r.Offset + d.Items.Length < d.Total) || d.Limitations is null ||
            d.Items.Any(o => o is null || o.Id == Guid.Empty || string.IsNullOrEmpty(o.Template) || o.Template.Length > 160 ||
                o.Position is null || o.OccupiedCells is null || o.OccupiedCells.Length > 64 || o.OccupiedCells.Any(p => p is null)) ||
            d.Items.Select(o => o.Id).Distinct().Count() != d.Items.Length)
            throw new InvalidDataException("Invalid objects");
        return result;
    }
    public async Task<BridgeEnvelope<NativeWorkforce>> Workforce(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "workforce") throw new ArgumentException();
        var result = await Get<NativeWorkforce>($"workforce?offset={r.Offset}&limit={r.Limit}", ct);
        var d = result.Data;
        if (d.Scope != "entities_with_worker_component" || d.Offset != r.Offset || d.Limit != r.Limit || d.Total < 0 ||
            d.Employed < 0 || d.Unemployed < 0 || (long)d.Employed + d.Unemployed != d.Total ||
            d.Items is null || d.Items.Length != Math.Min(r.Limit, Math.Max(0, d.Total - r.Offset)) ||
            d.HasMore != ((long)r.Offset + d.Items.Length < d.Total) || d.Limitations is null ||
            d.Items.Any(w => w is null || w.Id == Guid.Empty || string.IsNullOrEmpty(w.WorkerType) || w.WorkerType.Length > 160 ||
                w.AssignmentStatus is not ("assigned" or "unassigned" or "unresolved") ||
                (w.AssignmentStatus == "assigned") != (w.Workplace is not null) ||
                (w.Workplace is { } a && (a.Id == Guid.Empty || string.IsNullOrEmpty(a.Template) || a.Template.Length > 160 || a.Position is null))) ||
            d.Items.Select(w => w.Id).Distinct().Count() != d.Items.Length ||
            d.Items.Count(w => w.Employed) > d.Employed || d.Items.Count(w => !w.Employed) > d.Unemployed)
            throw new InvalidDataException("Invalid workforce roster");
        return result;
    }
    public async Task<BridgeEnvelope<NativeCatalog>> Catalog(CancellationToken ct)
    {
        var result = await Get<NativeCatalog>("catalog", ct);
        var d = result.Data;
        if (string.IsNullOrEmpty(d.Faction) || d.Items is null || d.Items.Length != 2 || d.Limitations is null ||
            d.Items.Any(i => i is null || !ValidCosts(i.Costs) || (i.Available && (i.Size is null || i.Unlocked is null))) ||
            !d.Items.Select(i => i.Template).Order().SequenceEqual(new[] { "Lodge.Folktails", "Path" }))
            throw new InvalidDataException("Invalid catalog");
        return result;
    }
    public async Task<BridgeEnvelope<NativeSite>> Precheck(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route is not ("site-precheck" or "building-precheck")) throw new ArgumentException();
        var result = await Get<NativeSite>($"{r.Route}?template={Uri.EscapeDataString(r.Template)}&x={r.X}&y={r.Y}&z={r.Z}&rotation={r.Rotation}", ct);
        if (r.GenericBuilding && result.BridgeVersion is not ("0.14.0" or "0.14.1" or "0.15.0" or "0.16.0" or "0.17.0")) throw new InvalidDataException("Generic building contract unavailable");
        var d = result.Data;
        if (d.Template != r.Template || d.Origin != new Position(r.X, r.Y, r.Z) || d.Rotation != r.Rotation ||
            d.GameValidated || d.Assessment is not ("blocked" or "requires_game_validation") || d.Reasons is null ||
            (d.Assessment == "blocked") != (d.Reasons.Length > 0) || d.Cells is null || d.Cells.Length is < 1 or > 64 ||
            d.Cells.Any(c => c is null || c.Position is null || c.SupportRule is null) || !ValidCosts(d.Costs) || d.Limitations is null)
            throw new InvalidDataException("Invalid site precheck");
        return result;
    }
    private static bool ValidCosts(NativeCost[]? costs) => costs is not null && costs.Length <= 32 &&
        costs.All(c => c is not null && !string.IsNullOrEmpty(c.Id) && c.Required >= 0 && c.AvailableGlobally >= 0);
    public async Task<BridgeEnvelope<NativeValidation>> Validate(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route is not ("site-validation" or "building-validation")) throw new ArgumentException();
        var result = await Get<NativeValidation>($"{r.Route}?template={Uri.EscapeDataString(r.Template)}&x={r.X}&y={r.Y}&z={r.Z}&rotation={r.Rotation}&session={r.Session}", ct, HttpMethod.Post);
        if (r.GenericBuilding && result.BridgeVersion is not ("0.14.0" or "0.14.1" or "0.15.0" or "0.16.0" or "0.17.0")) throw new InvalidDataException("Generic building contract unavailable");
        var d = result.Data;
        if (result.SessionId != r.Session || d.Template != r.Template || d.Origin != new Position(r.X, r.Y, r.Z) ||
            d.Rotation != r.Rotation || !d.GameValidated || (d.AttemptsRemaining < 0 || d.AttemptsRemaining > (r.GenericBuilding ? 255 : 7)) || d.Limitations is null ||
            (d.NoPersistentChangeObserved ? d.Valid is null || d.SessionLocked : d.Valid is not null || !d.SessionLocked))
            throw new InvalidDataException("Invalid validation result");
        return result;
    }
    public Task<BridgeEnvelope<NativePlacement>> PlacePath(BridgeRequest r, CancellationToken ct) { if (r.Route != "path-placement") throw new ArgumentException(); return Place(r, ct); }
    public Task<BridgeEnvelope<NativePlacement>> PlaceLodge(BridgeRequest r, CancellationToken ct) { if (r.Route != "lodge-placement") throw new ArgumentException(); return Place(r, ct); }
    private async Task<BridgeEnvelope<NativePlacement>> Place(BridgeRequest r, CancellationToken ct)
    {
        if (!((r.Route == "path-placement" && r.Template == "Path") || (r.Route == "lodge-placement" && r.Template == "Lodge.Folktails") || r.Route == "building-placement")) throw new ArgumentException();
        var result = await Get<NativePlacement>($"{r.Route}?template={Uri.EscapeDataString(r.Template)}&x={r.X}&y={r.Y}&z={r.Z}&rotation={r.Rotation}&session={r.Session}" + (r.Route == "building-placement" ? $"&actionId={r.EntityId}" : ""), ct, HttpMethod.Post);
        if (r.GenericBuilding && result.BridgeVersion is not ("0.14.0" or "0.14.1" or "0.15.0" or "0.16.0" or "0.17.0")) throw new InvalidDataException("Generic building contract unavailable");
        var d = result.Data;
        if (result.SessionId != r.Session || d.Template != r.Template || d.Origin != new Position(r.X, r.Y, r.Z) ||
            d.Rotation != r.Rotation || d.EntityId == Guid.Empty || (r.GenericBuilding ? d.EntityId.ToString("D") != r.EntityId || d.SessionLocked : !d.SessionLocked) || d.Limitations is null ||
            d.Outcome is not ("applied" or "rejected" or "unconfirmed") ||
            (d.Outcome == "applied" ? d.Finished is null : d.Finished is not null))
            throw new InvalidDataException("Invalid placement result");
        return result;
    }
    public async Task<BridgeEnvelope<NativeBuilding>> Building(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "building") throw new ArgumentException();
        var result = await Get<NativeBuilding>($"building?id={r.EntityId}&session={r.Session}", ct);
        var d = result.Data;
        if (result.SessionId != r.Session || d.Id.ToString("D") != r.EntityId || d.Limitations is null || d.Found != (d.Details is not null))
            throw new InvalidDataException("Invalid building result");
        if (d.Details is { } b)
        {
            if (result.BridgeVersion is "0.9.0" or "0.10.0" or "0.11.0" or "0.12.0" or "0.13.0" or "0.13.1" or "0.13.2" or "0.14.0" or "0.14.1" or "0.15.0" or "0.16.0" or "0.17.0" && b.Operations is null)
                throw new InvalidDataException("Missing operations contract");
            if (b.Operations is { } o)
            {
                if (o.PauseComponentPresent != (o.Pause is not null) ||
                    (o.WorkplaceComponentPresent && b.Finished) != (o.Workplace is not null))
                    throw new InvalidDataException("Invalid operation availability");
                if (o.Workplace is { } w && (w.DesiredWorkers < 0 || w.AssignedWorkers < 0 || w.MaxWorkers < 0 || w.DesiredWorkers > w.MaxWorkers))
                    throw new InvalidDataException("Invalid workplace counts");
                // Assigned workers may temporarily exceed desired/max. Keep game flags as observations.
            }
            if (string.IsNullOrEmpty(b.Template) || b.Template.Length > 160 || b.Position is null || b.District is null ||
                (b.Finished && b.Unfinished) || (b.Construction is not null && (!b.Unfinished || !b.ConstructionComponentPresent)) ||
                (b.Unfinished && b.ConstructionComponentPresent && b.Construction is null))
                throw new InvalidDataException("Invalid building details");
            var ids = new[] { b.District.AssignedDistrictId, b.District.InstantDistrictId, b.District.ConstructionDistrictId };
            if (ids.Any(id => id == Guid.Empty) || (!b.District.ComponentPresent && ids.Any(id => id is not null)))
                throw new InvalidDataException("Invalid district assignments");
            if (b.Construction is { } c && (!float.IsFinite(c.MaterialProgress) || c.MaterialProgress < 0 ||
                !float.IsFinite(c.BuildTimeProgress) || c.BuildTimeProgress < 0 || !float.IsFinite(c.BuildTimeProgressInHours) ||
                c.BuildTimeProgressInHours < 0 || c.Materials is null || !ValidGoods(c.Materials.BuildingCosts) ||
                c.Materials.InventoryAvailable != (c.Materials.SiteStock is not null) ||
                (c.Materials.SiteStock is not null && !ValidGoods(c.Materials.SiteStock))))
                throw new InvalidDataException("Invalid construction state");
        }
        return result;
    }
    public async Task<BridgeEnvelope<NativeStaffingResult>> SetStaffing(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "workplace-staffing") throw new ArgumentException();
        var result = await Get<NativeStaffingResult>($"workplace-staffing?id={r.EntityId}&session={r.Session}&desiredWorkers={r.DesiredWorkers}&expectedDesiredWorkers={r.ExpectedDesiredWorkers}", ct, HttpMethod.Post);
        var d = result.Data;
        if (result.SessionId != r.Session || d.Id.ToString("D") != r.EntityId || string.IsNullOrEmpty(d.Template) || d.Template.Length > 160 ||
            d.PreviousDesiredWorkers != r.ExpectedDesiredWorkers || d.RequestedDesiredWorkers != r.DesiredWorkers ||
            d.ObservedDesiredWorkers < 0 || d.MaxWorkers < d.ObservedDesiredWorkers || d.AssignedWorkers < 0 ||
            d.MaxWorkers < d.RequestedDesiredWorkers || d.Outcome is not ("applied" or "unconfirmed") ||
            (d.Outcome == "applied" && d.ObservedDesiredWorkers != d.RequestedDesiredWorkers) || d.Limitations is null)
            throw new InvalidDataException("Invalid staffing receipt");
        return result;
    }
    public async Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct)
    {
        var result = await Get<NativeSimulation>("simulation", ct);
        ValidateSimulation(result.Data);
        return result;
    }
    public async Task<BridgeEnvelope<NativeSpeedResult>> SetSpeed(BridgeRequest r, CancellationToken ct)
    {
        if (r.Route != "simulation-speed") throw new ArgumentException();
        var result = await Get<NativeSpeedResult>($"simulation-speed?speed={r.Speed}&expectedSpeed={r.ExpectedSpeed}&session={r.Session}", ct, HttpMethod.Post);
        var d = result.Data;
        if (result.SessionId != r.Session || d.RequestedSpeed != r.Speed || d.PreviousSpeed != r.ExpectedSpeed ||
            d.Observation is null || d.Limitations is null || d.MatchedImmediately != (d.Observation.CurrentSpeed == r.Speed))
            throw new InvalidDataException("Invalid speed receipt");
        ValidateSimulation(d.Observation);
        return result;
    }
    private static void ValidateSimulation(NativeSimulation d)
    {
        if (!float.IsFinite(d.CurrentSpeed) || d.CurrentSpeed < 0 || d.DayNumber < 0 ||
            !float.IsFinite(d.DayProgress) || d.DayProgress < 0 || d.DayProgress > 1 ||
            !float.IsFinite(d.HoursPassedToday) || d.HoursPassedToday < 0 || d.HoursPassedToday > 24)
            throw new InvalidDataException("Invalid simulation observation");
    }
    private static bool ValidGoods(NativeGoodAmount[]? goods) => goods is not null && goods.Length <= 32 &&
        goods.All(g => g is not null && !string.IsNullOrEmpty(g.Id) && g.Id.Length <= 160 && g.Amount >= 0) &&
        goods.Select(g => g.Id).Distinct().Count() == goods.Length;
    private async Task<BridgeEnvelope<T>> Get<T>(string route, CancellationToken ct, HttpMethod? method = null, Dictionary<string,string>? headers = null)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(7));
        using var request = new HttpRequestMessage(method ?? HttpMethod.Get, route);
        request.Headers.Authorization = new("Bearer", token);
        if(headers is not null) foreach(var pair in headers) request.Headers.Add(pair.Key,pair.Value);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, budget.Token);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Bridge authorization failed");
        if (response.StatusCode != HttpStatusCode.OK) throw new HttpRequestException("Bridge unavailable");
        await using var source = await response.Content.ReadAsStreamAsync(budget.Token);
        using var buffer = new MemoryStream(); var bytes = new byte[8192]; int count;
        while ((count = await source.ReadAsync(bytes, budget.Token)) > 0)
        {
            if (buffer.Length + count > 128 * 1024) throw new InvalidDataException("Bridge response too large");
            await buffer.WriteAsync(bytes.AsMemory(0, count), budget.Token);
        }
        var result = JsonSerializer.Deserialize<BridgeEnvelope<T>>(buffer.ToArray(), NativeJson.Options);
        if (result is null || result.SchemaVersion != 1 || result.BridgeVersion is not ("0.2.0" or "0.3.0" or "0.4.0" or "0.4.1" or "0.5.0" or "0.6.0" or "0.6.1" or "0.7.0" or "0.8.0" or "0.9.0" or "0.10.0" or "0.11.0" or "0.12.0" or "0.13.0" or "0.13.1" or "0.13.2" or "0.14.0" or "0.14.1" or "0.15.0" or "0.16.0" or "0.17.0") ||
            !Guid.TryParseExact(result.SessionId, "D", out var session) || session == Guid.Empty ||
            result.ObservedAtUtc == default || result.Data is null) throw new InvalidDataException("Invalid bridge envelope");
        return result;
    }
    public void Dispose() => client.Dispose();
}
