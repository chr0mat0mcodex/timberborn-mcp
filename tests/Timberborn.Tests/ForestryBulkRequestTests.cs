using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class ForestryBulkRequestTests
{
    [Fact]
    public async Task Native_port_requests_and_validates_complete_bulk_pages()
    {
        const string session = "22222222-2222-4222-8222-222222222222";
        var cells = Enumerable.Range(0, 256).Select(i => new Position(i, 0, 0)).ToArray();
        var range = new NativeRange(session, true, [], 0, 256, 300, cells, true, [], "building_terrain_range");
        var areas = new NativeAreas("tree_cutting", true, 0, 256, 300, cells.Select(p => new NativeAreaCell(p, "marked")).ToArray(), true, []);
        using var client = new NativeClient(new(8081, new string('a', 64)), new Handler(session, range, areas));
        var port = new NativeForestryPort(client);
        Assert.Equal(256, (await port.Range(new(session, session, FullRange: true), 0, TestContext.Current.CancellationToken)).Data.Items.Length);
        Assert.Equal(256, (await port.Cutting(0, TestContext.Current.CancellationToken)).Data.Items.Length);
    }

    private sealed class Handler(string session, NativeRange range, NativeAreas areas) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var uri = request.RequestUri ?? throw new InvalidDataException("missing_uri");
            Assert.Contains("limit=256", uri.Query);
            object data = uri.AbsolutePath.EndsWith("work-range", StringComparison.Ordinal) ? range : areas;
            string json = JsonSerializer.Serialize(new BridgeEnvelope<object>(1, session, DateTimeOffset.UnixEpoch, "0.37.0", data), NativeJson.Options);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        }
    }

    [Theory]
    [InlineData(32, true)]
    [InlineData(256, true)]
    [InlineData(257, false)]
    public void Bulk_reader_boundaries(int limit, bool accepted)
    {
        var range = new NameValueCollection { { "id", "11111111-1111-4111-8111-111111111111" },
            { "session", "22222222-2222-4222-8222-222222222222" }, { "offset", "0" }, { "limit", limit.ToString(System.Globalization.CultureInfo.InvariantCulture) } };
        var areas = new NameValueCollection { { "kind", "tree_cutting" }, { "offset", "0" }, { "limit", range["limit"] } };
        if (accepted) {
            Assert.Equal(limit, LogisticsRequest.Parse("/agent-api/v1/work-range", range).Limit);
            Assert.Equal(limit, ManagementRequest.Parse("/agent-api/v1/areas", areas).Limit);
        } else {
            Assert.Throws<ArgumentException>(() => LogisticsRequest.Parse("/agent-api/v1/work-range", range));
            Assert.Throws<ArgumentException>(() => ManagementRequest.Parse("/agent-api/v1/areas", areas));
        }
        var construction = new NameValueCollection { { "offset", "0" }, { "limit", "256" } };
        Assert.Throws<ArgumentException>(() => ManagementRequest.Parse("/agent-api/v1/construction", construction));
        var history = new NameValueCollection { { "good", "Log" }, { "offset", "0" }, { "limit", "256" } };
        Assert.Throws<ArgumentException>(() => LogisticsRequest.Parse("/agent-api/v1/good-history", history));
    }
}
