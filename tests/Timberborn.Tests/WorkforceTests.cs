using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;
public sealed class WorkforceTests
{
    [Theory]
    [InlineData("valid", true)]
    [InlineData("unresolved", true)]
    [InlineData("duplicate", false)]
    [InlineData("bad_total", false)]
    [InlineData("bad_pagination", false)]
    [InlineData("missing_assignment", false)]
    [InlineData("unexpected_assignment", false)]
    [InlineData("empty_worker_id", false)]
    public async Task RosterDistinguishesAssignmentAndEmployment(string scenario, bool valid)
    {
        var id = Guid.NewGuid();
        var data = new NativeWorkforce("entities_with_worker_component", 0, 2, 3, 1, 2,
            [new(id, "Beaver", true, false, "assigned", new(Guid.NewGuid(), "LumberjackFlag.Folktails", new(1, 2, 3))),
             new(Guid.NewGuid(), "Beaver", false, false, "unassigned", null)], true, ["synthetic_test"]);
        var payload = JsonSerializer.SerializeToNode(new BridgeEnvelope<NativeWorkforce>(1, Guid.NewGuid().ToString("D"), DateTimeOffset.UtcNow, "0.9.0", data), NativeJson.Options)!;
        var d = payload["data"]!;
        switch (scenario)
        {
            case "unresolved": d["items"]![0]!["assignmentStatus"] = "unresolved"; d["items"]![0]!["workplace"] = null; break;
            case "duplicate": d["items"]![1]!["id"] = id.ToString("D"); break;
            case "bad_total": d["unemployed"] = 3; break;
            case "bad_pagination": d["hasMore"] = false; break;
            case "missing_assignment": d["items"]![0]!["workplace"] = null; break;
            case "unexpected_assignment": d["items"]![0]!["assignmentStatus"] = "unassigned"; break;
            case "empty_worker_id": d["items"]![0]!["id"] = Guid.Empty.ToString("D"); break;
        }
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), new Handler(payload.ToJsonString())));
        var result = await tools.Invoke("inspect_workforce", JsonSerializer.SerializeToElement(new { offset = 0, limit = 2 }), TestContext.Current.CancellationToken);
        Assert.Equal(valid ? "ok" : "error", result["status"]!.GetValue<string>());
        if (scenario == "unresolved") Assert.True(result["data"]!["items"]![0]!["employed"]!.GetValue<bool>());
        if (!valid) Assert.Equal("backend_incompatible", result["error"]!["code"]!.GetValue<string>());
    }
    [Theory]
    [InlineData(-1, 2)]
    [InlineData(0, 33)]
    [InlineData(0, 0)]
    public async Task InvalidPageNeverCallsTransport(int offset, int limit)
    {
        using var tools = new NativeTools(new NativeClient(new(8081, new string('a', 64)), new Handler(null)));
        var result = await tools.Invoke("inspect_workforce", JsonSerializer.SerializeToElement(new { offset, limit }), TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument", result["error"]!["code"]!.GetValue<string>());
    }
    private sealed class Handler(string? json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            Assert.NotNull(json); Assert.Equal(HttpMethod.Get, r.Method);
            Assert.Equal("/agent-api/v1/workforce?offset=0&limit=2", r.RequestUri!.PathAndQuery);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json!) });
        }
    }
}
