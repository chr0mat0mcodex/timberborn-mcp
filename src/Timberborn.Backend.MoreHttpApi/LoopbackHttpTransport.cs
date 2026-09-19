using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Timberborn.Backend.Abstractions;

namespace Timberborn.Backend.MoreHttpApi;

public sealed record MoreHttpApiOptions
{
    public string BaseUrl { get; init; } = "http://localhost:8080/";
    public string? Authorization { get; init; }
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(5);
    public int MaxBodyBytes { get; init; } = 16 * 1024 * 1024;
    public int MaxEntities { get; init; } = 50_000;
}

public sealed class LoopbackHttpTransport : IDisposable
{
    private readonly HttpClient client;
    private readonly MoreHttpApiOptions options;
    private readonly SemaphoreSlim gate = new(1, 1);

    public LoopbackHttpTransport(MoreHttpApiOptions options, HttpMessageHandler? testHandler = null)
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) || uri.Scheme != "http" ||
            uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.AbsolutePath != "/" ||
            !(uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
              IPAddress.TryParse(uri.Host.Trim('[', ']'), out var ip) && IPAddress.IsLoopback(ip)) ||
            options.RequestTimeout <= TimeSpan.Zero || options.MaxBodyBytes < 1 || options.MaxEntities < 1)
            throw new ArgumentException("Nur eine HTTP-Loopback-Basisadresse und positive Limits sind erlaubt.");
        this.options = options;
        var handler = testHandler ?? new SocketsHttpHandler
        {
            AllowAutoRedirect = false, UseProxy = false, UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ConnectCallback = async (context, ct) =>
            {
                // Preserve the localhost Host header while never resolving to a non-loopback address.
                var addresses = context.DnsEndPoint.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    ? new[] { IPAddress.IPv6Loopback, IPAddress.Loopback }
                    : new[] { IPAddress.Parse(context.DnsEndPoint.Host.Trim('[', ']')) };
                foreach (var address in addresses)
                {
                    if (!IPAddress.IsLoopback(address)) throw new HttpRequestException("Non-loopback target rejected");
                    var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), ct);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch (SocketException) { socket.Dispose(); }
                    catch { socket.Dispose(); throw; }
                }
                throw new HttpRequestException("Loopback connection failed");
            }
        };
        client = new HttpClient(handler) { BaseAddress = uri, Timeout = Timeout.InfiniteTimeSpan };
    }

    public static bool IsAllowed(string route) => route is "ping" or "misc" or "live-data" or "buildings" or "characters" ||
        (route.StartsWith("buildings/", StringComparison.Ordinal) &&
         Guid.TryParseExact(route[10..], "D", out var id) && id != Guid.Empty);

    public async Task<JsonElement?> GetAsync(string route, CancellationToken ct)
    {
        if (!IsAllowed(route)) throw Faults.Exception("invalid_argument");
        await gate.WaitAsync(ct);
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(options.RequestTimeout);
            using var request = new HttpRequestMessage(HttpMethod.Get, "MoreHttpApi/" + route);
            if (!string.IsNullOrEmpty(options.Authorization)) request.Headers.TryAddWithoutValidation("Authorization", options.Authorization);
            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    throw Faults.Exception("authentication_failed");
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw Faults.Exception(route.StartsWith("buildings/", StringComparison.Ordinal) ? "entity_not_found" : "capability_unavailable");
                if (!response.IsSuccessStatusCode) throw Faults.Exception("backend_error");
                if (route == "ping")
                {
                    if (response.StatusCode != HttpStatusCode.NoContent) throw Faults.Exception("backend_incompatible");
                    return null;
                }
                await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
                using var buffer = new MemoryStream();
                var bytes = new byte[8192];
                int count;
                while ((count = await stream.ReadAsync(bytes, timeout.Token)) != 0)
                {
                    if (buffer.Length + count > options.MaxBodyBytes) throw Faults.Exception("response_too_large");
                    buffer.Write(bytes, 0, count);
                }
                using var doc = JsonDocument.Parse(buffer.ToArray(), new() { MaxDepth = 64 });
                return doc.RootElement.Clone();
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw Faults.Exception("timeout"); }
            catch (HttpRequestException) { throw Faults.Exception("backend_unavailable"); }
            catch (IOException) { throw Faults.Exception("backend_unavailable"); }
            catch (JsonException) { throw Faults.Exception("backend_incompatible"); }
        }
        finally { gate.Release(); }
    }
    public void Dispose() { client.Dispose(); gate.Dispose(); }
}
