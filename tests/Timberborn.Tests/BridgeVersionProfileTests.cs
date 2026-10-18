using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Xunit;

namespace Timberborn.Tests;

public sealed class BridgeVersionProfileTests
{
    // Frozen pre-pilot compatibility baseline. Do not derive expected support from the implementation.
    public static TheoryData<string, bool> KnownVersions
    {
        get
        {
            var cases = new TheoryData<string, bool>();
            foreach (var version in new[]
            {
                "0.2.0", "0.3.0", "0.4.0", "0.4.1", "0.5.0", "0.6.0",
                "0.6.1", "0.7.0", "0.8.0", "0.9.0", "0.10.0", "0.11.0",
                "0.12.0", "0.13.0", "0.13.1", "0.13.2", "0.14.0", "0.14.1",
                "0.15.0", "0.16.0", "0.17.0", "0.17.1", "0.17.2", "0.18.0",
                "0.19.0", "0.19.1", "0.19.2", "0.20.0", "0.20.1", "0.21.0",
                "0.21.1", "0.22.0", "0.23.0", "0.23.1", "0.23.2", "0.24.0",
                "0.24.1", "0.25.0", "0.26.0", "0.27.0", "0.28.0", "0.28.1",
                "0.29.0", "0.29.1", "0.29.2", "0.29.3", "0.30.0", "0.31.0",
                "0.31.1", "0.31.2", "0.31.3", "0.31.4", "0.32.0", "0.32.1",
                "0.33.0", "0.33.1", "0.34.0", "0.35.0",
            }) cases.Add(version, false);
            foreach (var version in new[]
            {
                "0.35.1", "0.35.2", "0.35.3", "0.35.4", "0.35.5", "0.36.0",
                "0.37.0", "0.38.0", "0.39.0", "0.40.0", "0.41.0", "0.42.0",
                "0.43.0", "0.44.0", "0.45.0", "0.46.0", "0.47.0", "0.48.0",
                "0.49.0", "0.50.0", "0.51.0", "0.52.0", "0.53.0", "0.54.0",
                "0.55.0", "0.56.0", "0.57.0", "0.58.0", "0.59.0", "0.60.0",
                "0.61.0", "0.62.0", "0.63.0", "0.64.0", "0.65.0", "0.66.0",
                "0.67.0", "0.68.0", "0.69.0", "0.70.0", "0.71.0", "0.72.0",
                "0.73.0", "0.74.0",
            }) cases.Add(version, true);
            return cases;
        }
    }

    private const string Session = "11111111-1111-4111-8111-111111111111";
    private static JsonObject Envelope(string? version) => JsonSerializer.SerializeToNode(
        new BridgeEnvelope<NativeMap>(1, Session, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), version!,
            new(new(0, 0, 0), 1, 1, 1, [new(0, 0, 0, false, true, 0, 0, 0, false)], [])), NativeJson.Options)!.AsObject();

    private static async Task Read(JsonObject envelope)
    {
        using var client = new NativeClient(new(8081, new string('a', 64)), new MapHandler(envelope.ToJsonString()));
        var request = BridgeRequest.Parse("/agent-api/v1/map", new NameValueCollection {
            ["x"] = "0", ["y"] = "0", ["z"] = "0", ["width"] = "1", ["height"] = "1", ["depth"] = "1" });
        var response = await client.Map(request, TestContext.Current.CancellationToken);
        Assert.Equal(Session, response.SessionId);
        Assert.Single(response.Data.Cells);
    }

    [Theory]
    [MemberData(nameof(KnownVersions))]
    public async Task ExistingVersionsRetainEnvelopeAcceptanceAndBudgetLimits(string version, bool expanded)
    {
        await Read(Envelope(version));
        Assert.Equal(expanded, Assert.IsType<BridgeVersionProfile>(BridgeVersionProfiles.Find(version)).ExpandedValidationBudget);
        foreach (int legacyMaximum in new[] { 7, 15, 255 })
        {
            int maximum = expanded ? 999_999 : legacyMaximum;
            Assert.True(ValidationBudgetContract.Accepts(version, 0, legacyMaximum));
            Assert.True(ValidationBudgetContract.Accepts(version, legacyMaximum, legacyMaximum));
            Assert.True(ValidationBudgetContract.Accepts(version, maximum, legacyMaximum));
            Assert.False(ValidationBudgetContract.Accepts(version, -1, legacyMaximum));
            Assert.False(ValidationBudgetContract.Accepts(version, maximum + 1, legacyMaximum));
        }
    }

    [Theory]
    [InlineData("99.0.0")]
    [InlineData("0.75.0")]
    [InlineData("0.35.6")]
    [InlineData("0.74.1")]
    [InlineData("0.74")]
    [InlineData(" 0.74.0")]
    [InlineData("0.74.0-preview")]
    [InlineData("")]
    [InlineData(null)]
    public async Task UnknownVersionsAreRejectedEvenWhenTheStandaloneBudgetFits(string? version)
    {
        Assert.Null(BridgeVersionProfiles.Find(version));
        // Preserve the helper's existing fallback; it is not an envelope admission check.
        Assert.True(ValidationBudgetContract.Accepts(version!, 7, 7));
        Assert.False(ValidationBudgetContract.Accepts(version!, 8, 7));
        await Assert.ThrowsAsync<InvalidDataException>(() => Read(Envelope(version)));
    }

    [Theory]
    [InlineData("schemaVersion")]
    [InlineData("sessionId")]
    [InlineData("observedAtUtc")]
    [InlineData("bridgeVersion")]
    [InlineData("data")]
    public async Task KnownVersionDoesNotRelaxRequiredEnvelopeFields(string field)
    {
        var envelope = Envelope("0.74.0");
        envelope.Remove(field);
        // Required constructor fields are rejected by deserialization, before profile lookup.
        await Assert.ThrowsAsync<JsonException>(() => Read(envelope));
    }

    [Theory]
    [InlineData("schemaVersion", "2")]
    [InlineData("sessionId", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("sessionId", "\"not-a-session\"")]
    [InlineData("observedAtUtc", "\"0001-01-01T00:00:00+00:00\"")]
    [InlineData("data", "null")]
    public async Task KnownVersionDoesNotRelaxInvalidEnvelopeValues(string field, string json)
    {
        var envelope = Envelope("0.74.0");
        envelope[field] = JsonNode.Parse(json);
        await Assert.ThrowsAsync<InvalidDataException>(() => Read(envelope));
    }

    private sealed class MapHandler(string payload) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/agent-api/v1/map", request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload) });
        }
    }
}
