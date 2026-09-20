using System.Net;
using System.Text;

namespace Timberborn.Bridge.Core;

public sealed class BridgeHttpServer : IDisposable
{
    private readonly HttpListener listener = new();
    private readonly MainThreadQueue queue;
    private readonly string token;
    private readonly bool enableValidation;
    private readonly bool enableSpeedControl;
    private readonly bool enableStaffing;
    private readonly bool enablePriorities, enableAreas, enableRemoval, enableBuildingPlacement, enableBuildingSettings;
    private readonly bool enablePlacement; private readonly bool enableLodgePlacement;
    private readonly CancellationTokenSource shutdown = new();
    private Task? worker;
    public BridgeHttpServer(int port, string token, MainThreadQueue queue, bool enableValidation = false, bool enablePlacement = false, bool enableLodgePlacement = false, bool enableSpeedControl = false, bool enableStaffing = false, bool enablePriorities = false, bool enableAreas = false, bool enableRemoval = false, bool enableBuildingPlacement = false, bool enableBuildingSettings = false)
    {
        if (port < 1024 || port > 65535 || token.Length != 64 || token.Any(c => !Uri.IsHexDigit(c)))
            throw new ArgumentException("invalid_configuration");
        this.token = token; this.queue = queue; this.enableValidation = enableValidation;
        this.enableBuildingSettings=enableBuildingSettings; this.enableBuildingPlacement=enableBuildingPlacement; this.enableRemoval=enableRemoval; this.enablePriorities=enablePriorities; this.enableAreas=enableAreas; this.enableStaffing = enableStaffing; this.enableSpeedControl = enableSpeedControl; this.enablePlacement = enablePlacement; this.enableLodgePlacement = enableLodgePlacement;
        listener.Prefixes.Add($"http://localhost:{port}/agent-api/v1/");
    }
    public void Start() { listener.Start(); worker = Task.Run(Serve); }
    private async Task Serve()
    {
        while (!shutdown.IsCancellationRequested)
        {
            HttpListenerContext context;
            try { context = await listener.GetContextAsync().ConfigureAwait(false); }
            catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException) { break; }
            // Serial requests limit work and response memory; every accepted request has a deadline.
            try { await Handle(context).ConfigureAwait(false); }
            catch (Exception) { try { context.Response.Abort(); } catch { } }
        }
    }
    private async Task Handle(HttpListenerContext context)
    {
        int status = 200;
        string json;
        var request = context.Request;
        if (request.RemoteEndPoint is null || !IPAddress.IsLoopback(request.RemoteEndPoint.Address) ||
            request.Url?.Host != "localhost" || !string.IsNullOrEmpty(request.Headers["Origin"]))
        { status = 403; json = "{\"error\":\"forbidden\"}"; }
        else if (!Matches(request.Headers["Authorization"]))
        { status = 401; json = "{\"error\":\"unauthorized\"}"; }
        else if (!MethodAllowed(request.HttpMethod, request.Url!.AbsolutePath, request.HasEntityBody, enableValidation, enablePlacement, enableLodgePlacement, enableSpeedControl, enableStaffing, enablePriorities, enableAreas, enableRemoval, enableBuildingPlacement, enableBuildingSettings))
        { status = 405; json = "{\"error\":\"read_only\"}"; }
        else
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(shutdown.Token);
            deadline.CancelAfter(TimeSpan.FromSeconds(5));
            try { json = await queue.Enqueue(BridgeRequest.Parse(request.Url!.AbsolutePath, request.QueryString), deadline.Token).ConfigureAwait(false); }
            catch (ArgumentException) { status = 400; json = "{\"error\":\"invalid_request\"}"; }
            catch (OperationCanceledException) { status = 503; json = "{\"error\":\"session_unavailable\"}"; }
            catch (InvalidOperationException) { status = 503; json = "{\"error\":\"observation_unavailable\"}"; }
        }
        var bytes = Encoding.UTF8.GetBytes(json);
        if (bytes.Length > 128 * 1024) { status = 507; bytes = Encoding.UTF8.GetBytes("{\"error\":\"response_too_large\"}"); }
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers["Cache-Control"] = "no-store";
        context.Response.ContentLength64 = bytes.Length;
        using var responseDeadline = CancellationTokenSource.CreateLinkedTokenSource(shutdown.Token);
        responseDeadline.CancelAfter(TimeSpan.FromSeconds(5));
        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length, responseDeadline.Token).ConfigureAwait(false);
        context.Response.Close();
    }
    public static bool MethodAllowed(string method, string path, bool hasBody, bool enableValidation, bool enablePlacement = false, bool enableLodgePlacement = false, bool enableSpeedControl = false, bool enableStaffing = false, bool enablePriorities = false, bool enableAreas = false, bool enableRemoval = false, bool enableBuildingPlacement = false, bool enableBuildingSettings = false) =>
        !hasBody && (BuildingSettingsRequest.Handles(path) && path != "/agent-api/v1/building-settings" ? enableBuildingSettings && method == "POST" : path is "/agent-api/v1/building-validation" or "/agent-api/v1/building-placement" ? enableBuildingPlacement && method == "POST" : path == "/agent-api/v1/remove-object" ? enableRemoval && method == "POST" : path == "/agent-api/v1/set-priority" ? enablePriorities && method == "POST" : path == "/agent-api/v1/set-area" ? enableAreas && method == "POST" : path == "/agent-api/v1/workplace-staffing" ? enableStaffing && method == "POST" : path == "/agent-api/v1/simulation-speed" ? enableSpeedControl && method == "POST" : path == "/agent-api/v1/lodge-placement" ? enableLodgePlacement && method == "POST" : path == "/agent-api/v1/path-placement" ? enablePlacement && method == "POST"
            : path == "/agent-api/v1/site-validation" ? enableValidation && method == "POST" : method == "GET");
    private bool Matches(string? supplied)
    {
        var expected = "Bearer " + token;
        if (supplied is null || supplied.Length != expected.Length) return false;
        int difference = 0;
        for (int i = 0; i < expected.Length; i++) difference |= supplied[i] ^ expected[i];
        return difference == 0;
    }
    public void Dispose()
    {
        shutdown.Cancel(); queue.Dispose(); listener.Close();
        // Do not block Unity on a pending network response; the worker observes cancellation.
    }
}
