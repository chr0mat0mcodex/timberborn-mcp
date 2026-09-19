using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public sealed class NativeClient : IDisposable
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
    private async Task<BridgeEnvelope<T>> Get<T>(string route, CancellationToken ct)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(7));
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.Authorization = new("Bearer", token);
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
        if (result is null || result.SchemaVersion != 1 || result.BridgeVersion != "0.2.0" ||
            !Guid.TryParseExact(result.SessionId, "D", out var session) || session == Guid.Empty ||
            result.ObservedAtUtc == default || result.Data is null) throw new InvalidDataException("Invalid bridge envelope");
        return result;
    }
    public void Dispose() => client.Dispose();
}
