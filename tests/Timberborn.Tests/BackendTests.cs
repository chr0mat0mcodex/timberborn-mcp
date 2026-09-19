using System.Net;
using System.Text;
using Timberborn.Backend.Abstractions;
using Timberborn.Backend.MoreHttpApi;
using Xunit;

namespace Timberborn.Tests;

public sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
{
    public List<string> Routes { get; } = [];
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    { Routes.Add(request.RequestUri!.AbsolutePath); Assert.Equal(HttpMethod.Get, request.Method); return send(request, ct); }
    public static StubHandler Json(string body, HttpStatusCode code = HttpStatusCode.OK) => new((_, _) =>
        Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") }));
}

public sealed class BackendTests
{
    public const string Building = """
    {"Entity":{"EntityId":"00000000-0000-0000-0000-000000000001"},"Name":{"EntityName":"Test"},
    "TemplateName":"DistrictCenter.Folktails","Pausable":{"IsPaused":false,"Pausable":true}}
    """;
    public const string Live = """
    {"TopBar":{"Cycle":{"Cycle":1,"Day":2,"Hours":0.5},"Weather":{"Current":{"Id":"TemperateWeather"},
    "Next":{"Weather":{"Id":"DroughtWeather"},"ComingInDays":3.5},"ShouldShowNext":false},"Speed":{"Speed":0}}}
    """;
    [Fact]
    public async Task HiddenForecastIsNotLeaked()
    {
        using var backend = new MoreHttpApiBackend(new(), StubHandler.Json(Live));
        var live = await backend.GetLiveAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0.5, live.Time.DayProgress); Assert.Null(live.Weather.Next); Assert.Null(live.Weather.DaysUntilNext);
    }
    [Fact]
    public async Task VisibleForecastIsMapped()
    {
        using var backend = new MoreHttpApiBackend(new(), StubHandler.Json(Live.Replace("false", "true")));
        var live = await backend.GetLiveAsync(TestContext.Current.CancellationToken);
        Assert.Equal("drought", live.Weather.Next); Assert.Equal(3.5, live.Weather.DaysUntilNext);
    }
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"Adult\":[],\"Child\":[]}")]
    [InlineData("{\"Adult\":null,\"Child\":[],\"Bot\":[]}")]
    [InlineData("not-json")]
    public async Task MissingPopulationIsNotZero(string body)
    {
        using var backend = new MoreHttpApiBackend(new(), StubHandler.Json(body));
        var error = await Assert.ThrowsAsync<BackendException>(() => backend.GetPopulationAsync(TestContext.Current.CancellationToken));
        Assert.Equal("backend_incompatible", error.Fault.Code);
    }
    [Fact]
    public async Task EmptyPopulationIsValid()
    {
        using var backend = new MoreHttpApiBackend(new(), StubHandler.Json("{\"Adult\":[],\"Child\":[],\"Bot\":[],\"extra\":42}"));
        Assert.Empty(await backend.GetPopulationAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task MapsBuildingAndValidatesMembership()
    {
        var handler = new StubHandler((r, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        { Content = new StringContent(r.RequestUri!.AbsolutePath.EndsWith("/buildings", StringComparison.Ordinal)
            ? "{\"Groups\":[{\"Buildings\":[" + Building + "]}]}" : Building) }));
        using var backend = new MoreHttpApiBackend(new(), handler);
        var item = await backend.GetBuildingAsync(Guid.Parse("00000000-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);
        Assert.True(item.Pausable); Assert.False(item.Paused); Assert.Equal(2, handler.Routes.Count);
        var error = await Assert.ThrowsAsync<BackendException>(() => backend.GetBuildingAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
        Assert.Equal("entity_not_found", error.Fault.Code); Assert.Equal(3, handler.Routes.Count);
    }
    [Theory]
    [InlineData(401, "authentication_failed")]
    [InlineData(403, "authentication_failed")]
    [InlineData(404, "capability_unavailable")]
    [InlineData(500, "backend_error")]
    [InlineData(302, "backend_error")]
    public async Task HttpErrorsAreSanitized(int status, string expected)
    {
        using var backend = new MoreHttpApiBackend(new(), StubHandler.Json("SECRET PATH", (HttpStatusCode)status));
        var error = await Assert.ThrowsAsync<BackendException>(() => backend.GetGameInfoAsync(TestContext.Current.CancellationToken));
        Assert.Equal(expected, error.Fault.Code); Assert.DoesNotContain("SECRET", error.Message);
    }
    [Theory]
    [InlineData("http://example.org:8080/")]
    [InlineData("http://localhost:8080/MoreHttpApi/")]
    [InlineData("http://user:pass@localhost:8080/")]
    [InlineData("http://localhost:8080/?x=1")]
    [InlineData("https://localhost:8080/")]
    public void RejectsUnsafeBaseAddress(string url) =>
        Assert.Throws<ArgumentException>(() => new LoopbackHttpTransport(new() { BaseUrl = url }));
    [Theory]
    [InlineData("live-data/set-game-speed?speed=1")]
    [InlineData("buildings/00000000-0000-0000-0000-000000000001/toggle-pause")]
    [InlineData("../misc")]
    [InlineData("http://example.org/")]
    [InlineData("buildings/00000000-0000-0000-0000-000000000001?paused=true")]
    public async Task RejectsWriteAndArbitraryRoutesBeforeSending(string route)
    {
        var handler = StubHandler.Json("{}"); using var transport = new LoopbackHttpTransport(new(), handler);
        await Assert.ThrowsAsync<BackendException>(() => transport.GetAsync(route, TestContext.Current.CancellationToken));
        Assert.Empty(handler.Routes);
    }
    [Fact]
    public async Task BodySizeIsBounded()
    {
        using var backend = new MoreHttpApiBackend(new() { MaxBodyBytes = 10 }, StubHandler.Json(Live));
        var error = await Assert.ThrowsAsync<BackendException>(() => backend.GetLiveAsync(TestContext.Current.CancellationToken));
        Assert.Equal("response_too_large", error.Fault.Code);
    }
    [Fact]
    public async Task RequestTimeoutIsDistinctFromCallerCancellation()
    {
        using var backend = new MoreHttpApiBackend(new() { RequestTimeout = TimeSpan.FromMilliseconds(50) },
            new StubHandler(async (_, ct) => { await Task.Delay(10000, ct); return new(HttpStatusCode.OK); }));
        var error = await Assert.ThrowsAsync<BackendException>(() => backend.GetLiveAsync(TestContext.Current.CancellationToken));
        Assert.Equal("timeout", error.Fault.Code);
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => backend.GetLiveAsync(cancel.Token));
    }
    [Fact]
    public async Task ModPathsAreDiscardedAndActiveFlagMapped()
    {
        using var backend = new MoreHttpApiBackend(new(), StubHandler.Json("""
        {"GameVersion":{"Full":"1.1.2.4"},"Mods":[{"Id":"MoreHttpApi","Name":"Test",
        "Version":{"Full":"11.0.0"},"Active":true,"Directory":"PRIVATE-PATH"}]}
        """));
        var info = await backend.GetGameInfoAsync(TestContext.Current.CancellationToken);
        Assert.True(info.Mods[0].Enabled);
        Assert.DoesNotContain("PRIVATE", System.Text.Json.JsonSerializer.Serialize(info));
    }
}
