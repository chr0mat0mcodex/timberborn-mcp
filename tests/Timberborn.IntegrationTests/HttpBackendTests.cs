using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Timberborn.Backend.Abstractions;
using Timberborn.Backend.MoreHttpApi;
using Xunit;

namespace Timberborn.IntegrationTests;

public sealed class LocalHttpStub : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource stop = new();
    private readonly Task worker;
    public ConcurrentQueue<string> Requests { get; } = new();
    public string Url { get; }
    public LocalHttpStub(Func<string, CancellationToken, Task<byte[]>> respond)
    {
        listener.Start(); Url = $"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/";
        worker = Task.Run(async () =>
        {
            try
            {
                while (!stop.IsCancellationRequested)
                {
                    using var socket = await listener.AcceptTcpClientAsync(stop.Token);
                    await using var stream = socket.GetStream();
                    using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
                    var first = await reader.ReadLineAsync(stop.Token) ?? "";
                    Requests.Enqueue(first);
                    while (!string.IsNullOrEmpty(await reader.ReadLineAsync(stop.Token))) { }
                    var bytes = await respond(first, stop.Token);
                    await stream.WriteAsync(bytes, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (IOException) when (stop.IsCancellationRequested) { }
            catch (SocketException) when (stop.IsCancellationRequested) { }
        });
    }
    public static byte[] Response(byte[] body, string status = "200 OK", string extra = "") =>
        [.. Encoding.ASCII.GetBytes($"HTTP/1.1 {status}\r\nContent-Type: application/json\r\nContent-Length: {body.Length}\r\nConnection: close\r\n{extra}\r\n"), .. body];
    public async ValueTask DisposeAsync()
    { stop.Cancel(); listener.Stop(); await worker; stop.Dispose(); }
}

public sealed class HttpBackendTests
{
    [Fact]
    public async Task ActualHttpMapsGameInfoAndRefusesRedirects()
    {
        await using var stub = new LocalHttpStub((_, _) => Task.FromResult(LocalHttpStub.Response(
            Encoding.UTF8.GetBytes("{\"GameVersion\":{\"Full\":\"test\"},\"Mods\":[]}"))));
        using var backend = new MoreHttpApiBackend(new() { BaseUrl = stub.Url });
        Assert.Equal("test", (await backend.GetGameInfoAsync(TestContext.Current.CancellationToken)).GameVersion);
        Assert.Single(stub.Requests);
        await using var redirect = new LocalHttpStub((_, _) => Task.FromResult(LocalHttpStub.Response([], "302 Found", "Location: " + stub.Url + "\r\n")));
        using var other = new MoreHttpApiBackend(new() { BaseUrl = redirect.Url });
        var error = await Assert.ThrowsAsync<BackendException>(() => other.GetGameInfoAsync(TestContext.Current.CancellationToken));
        Assert.Equal("backend_error", error.Fault.Code); Assert.Single(stub.Requests);
    }
    [Fact]
    public async Task DecompressedSizeIsBounded()
    {
        using var compressed = new MemoryStream();
        using (var gzip = new GZipStream(compressed, CompressionMode.Compress, true))
            gzip.Write(Encoding.UTF8.GetBytes(new string('x', 20000)));
        await using var stub = new LocalHttpStub((_, _) => Task.FromResult(LocalHttpStub.Response(compressed.ToArray(), extra: "Content-Encoding: gzip\r\n")));
        using var backend = new MoreHttpApiBackend(new() { BaseUrl = stub.Url, MaxBodyBytes = 1024 });
        var error = await Assert.ThrowsAsync<BackendException>(() => backend.GetGameInfoAsync(TestContext.Current.CancellationToken));
        Assert.Equal("response_too_large", error.Fault.Code);
    }
    [Fact]
    public async Task RealNetworkTimeoutIsBounded()
    {
        await using var stub = new LocalHttpStub(async (_, ct) => { await Task.Delay(10000, ct); return []; });
        using var backend = new MoreHttpApiBackend(new() { BaseUrl = stub.Url, RequestTimeout = TimeSpan.FromMilliseconds(150) });
        var error = await Assert.ThrowsAsync<BackendException>(() => backend.GetGameInfoAsync(TestContext.Current.CancellationToken));
        Assert.Equal("timeout", error.Fault.Code);
    }
}
