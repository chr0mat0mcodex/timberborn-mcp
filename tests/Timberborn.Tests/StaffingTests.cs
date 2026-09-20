using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;
public sealed class StaffingTests
{
    [Theory]
    [InlineData("GET", true, false, false)]
    [InlineData("POST", false, false, false)]
    [InlineData("POST", true, true, false)]
    [InlineData("POST", true, false, true)]
    public void OwnOptIn(string method, bool enabled, bool body, bool expected) =>
        Assert.Equal(expected, BridgeHttpServer.MethodAllowed(method, "/agent-api/v1/workplace-staffing", body, true, true, true, true, enabled));
    [Theory]
    [InlineData(false, 2, true)]
    [InlineData(true, -1, true)]
    [InlineData(true, 65, true)]
    [InlineData(true, 2, false)]
    public async Task RejectsBeforeTransport(bool enabled, int desired, bool validId)
    {
        var handler = new Handler(null);
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), handler), enableStaffing: enabled);
        var args = JsonSerializer.SerializeToElement(new { id = validId ? Guid.NewGuid().ToString("D") : "bad", session = Guid.NewGuid().ToString("D"), desiredWorkers = desired, expectedDesiredWorkers = 2 });
        var result = await tools.Invoke("set_workplace_staffing", args, TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>()); Assert.Equal(0, handler.Calls);
        Assert.DoesNotContain(NativeTools.Catalog(true, true, true, true), t => t.Name == "set_workplace_staffing");
    }
    [Theory]
    [InlineData("applied", 3, false, true)]
    [InlineData("unconfirmed", 2, false, true)]
    [InlineData("applied", 2, false, false)]
    [InlineData("applied", 3, true, false)]
    public async Task ReceiptCorrelatesAndPreservesPartialOutcome(string outcome, int observed, bool mismatch, bool valid)
    {
        var id=Guid.NewGuid(); var session=Guid.NewGuid().ToString("D");
        var data = new NativeStaffingResult(id,"DistrictCenter.Folktails",2,3,observed,2,4,outcome,["synthetic_test"]);
        var handler = new Handler(JsonSerializer.Serialize(new BridgeEnvelope<NativeStaffingResult>(1,mismatch ? Guid.NewGuid().ToString("D") : session,DateTimeOffset.UtcNow,"0.10.0",data),NativeJson.Options));
        using var tools = new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableStaffing:true);
        var result=await tools.Invoke("set_workplace_staffing",JsonSerializer.SerializeToElement(new{id,session,desiredWorkers=3,expectedDesiredWorkers=2}),TestContext.Current.CancellationToken);
        Assert.Equal(valid ? "ok":"error",result["status"]!.GetValue<string>());Assert.Equal(1,handler.Calls);
        if(!valid)Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
    }
    private sealed class Handler(string? json):HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r,CancellationToken ct){Calls++;Assert.NotNull(json);Assert.Equal(HttpMethod.Post,r.Method);Assert.Null(r.Content);return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(json!)});}
    }
}
