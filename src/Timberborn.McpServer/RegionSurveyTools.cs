using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public static class RegionSurveyTools
{
    public const string Name = "survey_region";
    public static Tool Reader()
    {
        var options = new JsonSerializerOptions(BuildBatchState.Json) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var input = options.GetJsonSchemaAsNode(typeof(SurveyRequest)).AsObject(); input["type"] = "object";
        var props = input["properties"]!.AsObject();
        foreach (string key in new[] { "width", "height" }) { props[key]!["minimum"] = 1; props[key]!["maximum"] = 16; }
        foreach (string key in new[] { "x", "y", "z" }) { props[key]!["minimum"] = 0; props[key]!["maximum"] = 4095; }
        props["maxHeights"]!["minimum"] = 1; props["maxHeights"]!["maximum"] = 8;
        props["maxWindows"]!["minimum"] = 1; props["maxWindows"]!["maximum"] = 9;
        return new() {
            Name = Name,
            Description = "Lesende Regionalsuche bis 16×16, nur pausiert. Native Überlappungszellen lokalisieren Rampen/Hindernisse; ältere Bridge: höchstens 64 zusätzliche Verfeinerungsreads je Höhe, Rest unbekannt. Standard scanOtherHeights=true: relevante Gelände-/Weghöhen automatisch prüfen, maxHeights=4 (1–8, inklusive z), Wege bevorzugt. maxWindows=3 (1–9) je Höhe, vier Drehungen, bevorzugt neue Flächenabdeckung. Höchstens maxHeights×maxWindows×4 Planungen; Kandidaten pro Höhe auf vier begrenzt. coverage.heights meldet ungeprüfte Ebenen; planningRows u=ungeprüft/p=teilweise/c=vier Drehungen vollständig gesucht, keine Bauvalidierung. additionalLevels enthält obere/untere Ergebnisse. Freie Rechtecke bis 4×4 und Feuchte. planBuildings=false oder workBuildingId: nur angefragte Höhe, keine automatische Höhenfolge; workBuildingId liefert native Reichweite bis 2048 Zellen. Keine Pflanz-, Bau-, Distrikt- oder Ertragsgarantie. Kandidaten frisch validieren; unvollständige/wechselnde Daten brechen ab. Keine Spieländerung.",
            InputSchema = JsonSerializer.SerializeToElement(input),
            OutputSchema = JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<RegionSurveyReport>))),
            Annotations = new() { ReadOnlyHint = true, DestructiveHint = false, IdempotentHint = true, OpenWorldHint = false }
        };
    }
    public static async Task<JsonObject> Invoke(NativeClient client, JsonElement args, CancellationToken ct)
    {
        var request = BuildBatchState.Parse<SurveyRequest>(args);
        RegionSurveyReport report;
        try { report = await RegionSurvey.Observe(request, new NativeRegionSurveyPort(client), ct); }
        catch (InvalidDataException ex) when (ex.Message.StartsWith("survey_", StringComparison.Ordinal)) {
            return JsonSerializer.SerializeToNode(new NativeResult<RegionSurveyReport>(1, "error", null,
                new("native", false, request.Session, null), new("survey_incomplete", ex.Message, false)), NativeJson.Options)!.AsObject();
        }
        return JsonSerializer.SerializeToNode(new NativeResult<RegionSurveyReport>(1, "ok", report,
            new("native", false, request.Session, report.ObservationEndedAtUtc), null), NativeJson.Options)!.AsObject();
    }
}

public sealed class NativeRegionSurveyPort(NativeClient client) : IRegionSurveyPort
{
    private static NameValueCollection Query(params (string Key, object Value)[] fields)
    {
        var q = new NameValueCollection();
        foreach (var (key, value) in fields) q[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
        return q;
    }
    public Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct) => client.Simulation(ct);
    public Task<BridgeEnvelope<NativeRange>> WorkRange(SurveyRequest r, int offset, CancellationToken ct) =>
        client.WorkRange(LogisticsRequest.Parse("/agent-api/v1/work-range", Query(("session", r.Session),
            ("id", r.WorkBuildingId!), ("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeMap>> Map(SurveyRect r, CancellationToken ct) =>
        client.Map(BridgeRequest.Parse("/agent-api/v1/map", Query(("x", r.X), ("y", r.Y), ("z", r.Z),
            ("width", r.Width), ("height", r.Height), ("depth", 1))), ct);
    public Task<BridgeEnvelope<NativeObjects>> Objects(int offset, CancellationToken ct) =>
        client.Objects(BridgeRequest.Parse("/agent-api/v1/objects", Query(("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeAreas>> Areas(string kind, int offset, CancellationToken ct) =>
        client.Areas(ManagementRequest.Parse("/agent-api/v1/areas", Query(("kind", kind), ("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeRemovalTargets>> Targets(SurveyRect r, int offset, CancellationToken ct) =>
        client.RemovalTargets(RemovalRequest.Parse("/agent-api/v1/removal-targets", Query(("kind", "all"),
            ("x", r.X), ("y", r.Y), ("z", r.Z), ("width", r.Width), ("height", r.Height), ("depth", 1),
            ("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeBuildingPlan>> Plan(SurveyRequest s, SurveyRect r, int rotation, CancellationToken ct) =>
        client.BuildingPlan(BuildingPlanRequest.Parse(Query(("session", s.Session), ("districtId", s.DistrictId),
            ("template", s.Template), ("x", r.X), ("y", r.Y), ("z", r.Z), ("width", r.Width), ("height", r.Height), ("rotation", rotation))), ct);
}
