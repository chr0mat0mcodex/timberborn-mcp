using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public sealed record ForestryMarkRequest(ForestryRequest Region, string ActionId, string[] TreeIds);
public sealed record ForestryMarkReport(string ActionId, string Fingerprint, string State, int Requested,
    int Confirmed, ForestryTree? Unconfirmed, string Reason);

public static class ForestryTools
{
    public const string Inspect = "inspect_forestry", Mark = "mark_forestry";
    public static bool Handles(string name) => name is Inspect or Mark;
    public static IEnumerable<Tool> Catalog(bool areasEnabled)
    {
        var json = new JsonSerializerOptions(BuildBatchState.Json) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        foreach (var name in new[] { Inspect, Mark }) {
            if (name == Mark && !areasEnabled) continue;
            var schema = json.GetJsonSchemaAsNode(name == Inspect ? typeof(ForestryRequest) : typeof(ForestryMarkRequest)).AsObject();
            schema["type"] = "object";
            yield return new() {
                Name = name,
                Description = name == Inspect
                    ? "Kompakte Holzdiagnose, pausiert, bis 8×8×4: Baumkatalog, Reife, tatsächlichen Fällertrag, Fäll-/Abrissmarkierung und native Arbeitsreichweite zusammenführen. Abgeerntete Reste und unbekannter Fällertrag sind ausgeschlossen; alte 0.35.5-Pakete ohne Ertragsfelder liefern keine Kandidaten. Liefert die ersten 16 Kandidaten mit IDs, Gesamtzahl/Trunkierung, Ausschlusszähler und freie/gesamte Holzbestände. Optional consumerIds (maximal 8) für Personal, Pause und Holz im Gebäudeinventar; keine Verbrauchsrate. Alle internen Seiten begrenzt und vollständig geprüft, nativeReads sichtbar. Kein Ertrags-/Transportnachweis; unbekannte Reichweite liefert keine Kandidaten. Keine Spieländerung."
                    : "Markiert 1–16 ausdrücklich gewählte Baum-IDs aus inspect_forestry nach frischer regionaler Prüfung für reguläres Fällen. Gleiche actionId liest nur den gespeicherten Beleg, geänderte Parameter abgelehnt. Kein Retry nach unbestätigtem Teilauftrag. Ausführung sequenziell, Teilfortschritt bleibt erhalten; completed bedeutet Markierungen bestätigt, nicht Ernte. Kein Simulationsstart.",
                InputSchema = JsonSerializer.SerializeToElement(schema),
                OutputSchema = JsonSerializer.SerializeToElement(json.GetJsonSchemaAsNode(typeof(NativeResult<object>))),
                Annotations = new() { ReadOnlyHint = name == Inspect, DestructiveHint = name == Mark,
                    IdempotentHint = true, OpenWorldHint = false }
            };
        }
    }

    public static Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled, CancellationToken ct) =>
        Execute(name, args, enabled, new NativeForestryPort(client), Path.Combine(AppContext.BaseDirectory, ".local", "forestry"), ct);

    public static async Task<JsonObject> Execute(string name, JsonElement args, bool enabled, IForestryPort port, string directory, CancellationToken ct)
    {
        if (!Handles(name) || name == Mark && !enabled) throw new ArgumentException();
        if (name == Inspect) {
            var r = BuildBatchState.Parse<ForestryRequest>(args);
            var report = await ForestrySurvey.Observe(r, port, ct);
            return Wrap(report, r.Session, report.ObservedAtUtc);
        }
        var request = BuildBatchState.Parse<ForestryMarkRequest>(args);
        if (request.Region is null || request.TreeIds is not { Length: >= 1 and <= 16 }) throw new ArgumentException();
        ForestrySurvey.Validate(request.Region); BuildBatchState.Id(request.ActionId);
        foreach (var id in request.TreeIds) BuildBatchState.Id(id);
        if (request.TreeIds.Distinct().Count() != request.TreeIds.Length) throw new ArgumentException("duplicate_tree_ids");
        var fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(request, NativeJson.Options)));
        Directory.CreateDirectory(directory);
        using var lease = new FileStream(Path.Combine(directory, "dispatch.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        string path = Path.Combine(directory, request.Region.Session + "-" + request.ActionId + ".json");
        if (File.Exists(path)) {
            if (new FileInfo(path).Length > 65536) throw new InvalidDataException("forestry_journal_too_large");
            var old = JsonSerializer.Deserialize<ForestryMarkReport>(File.ReadAllText(path), NativeJson.Options) ?? throw new InvalidDataException();
            if (old.ActionId != request.ActionId || old.Fingerprint != fingerprint) throw new ArgumentException("forestry_parameters_changed");
            return Wrap(old, request.Region.Session, null); // Replay is always read-only, even after a lost acknowledgement.
        }
        var observation = await ForestrySurvey.Observe(request.Region, port, ct);
        var selected = request.TreeIds.Select(id => observation.Candidates.SingleOrDefault(t => t.Id.ToString("D") == id)
            ?? throw new ArgumentException("tree_no_longer_eligible")).ToArray();
        if (selected.Select(t => t.Position).Distinct().Count() != selected.Length) throw new InvalidDataException("overlapping_tree_origins");
        var receipt = new ForestryMarkReport(request.ActionId, fingerprint, "marking", selected.Length, 0, null, "");
        void Save() {
            string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try {
                using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                    JsonSerializer.Serialize(stream, receipt, NativeJson.Options); stream.Flush(true);
                }
                File.Move(temp, path, true);
            } finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        Save();
        try {
            foreach (var tree in selected) {
                var simulation = await port.Simulation(ct);
                if (simulation.SessionId != request.Region.Session || simulation.Data.CurrentSpeed != 0)
                    throw new BridgeRejectionException("state_conflict");
                receipt = receipt with { State = "unconfirmed", Unconfirmed = tree, Reason = "mark_acknowledgement_pending_no_retry" }; Save();
                var e = await port.Mark(request.Region, tree.Position, ct);
                if (e.SessionId != request.Region.Session || e.Data.Kind != "tree_cutting" || e.Data.Operation != "mark" ||
                    e.Data.Outcome != "applied" || e.Data.Items.Length != 1 || e.Data.Items[0].Position != tree.Position ||
                    e.Data.Items[0].Resource != "marked") throw new InvalidDataException("forestry_mark_unconfirmed");
                receipt = receipt with { State = "marking", Confirmed = receipt.Confirmed + 1, Unconfirmed = null, Reason = "" }; Save();
            }
            receipt = receipt with { State = "completed", Reason = "markings_confirmed_not_harvested" }; Save();
        } catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or HttpRequestException or OperationCanceledException or JsonException) {
            if (receipt.Unconfirmed is null) receipt = receipt with { State = "stopped", Reason = "precondition_or_transport_failure_no_retry" };
            Save();
        }
        return Wrap(receipt, request.Region.Session, null);
    }

    private static JsonObject Wrap(object report, string session, DateTimeOffset? observed) =>
        JsonSerializer.SerializeToNode(new NativeResult<object>(1, "ok", report,
            new("native", false, session, observed), null), NativeJson.Options)!.AsObject();
}

public sealed class NativeForestryPort(NativeClient client) : IForestryPort
{
    private static NameValueCollection Query(params (string Key, object Value)[] fields) {
        var q = new NameValueCollection();
        foreach (var (key, value) in fields) q[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
        return q;
    }
    public Task<BridgeEnvelope<NativeSimulation>> Simulation(CancellationToken ct) => client.Simulation(ct);
    public Task<BridgeEnvelope<NativeAreaTypes>> Types(CancellationToken ct) => client.AreaTypes(ct);
    public Task<BridgeEnvelope<NativeRange>> Range(ForestryRequest r, int offset, CancellationToken ct) =>
        client.WorkRange(LogisticsRequest.Parse("/agent-api/v1/work-range", Query(("session", r.Session), ("id", r.WorkBuildingId), ("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeAreas>> Cutting(int offset, CancellationToken ct) =>
        client.Areas(ManagementRequest.Parse("/agent-api/v1/areas", Query(("kind", "tree_cutting"), ("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeRemovalTargets>> Trees(ForestryRequest r, int z, int offset, CancellationToken ct) =>
        client.RemovalTargets(RemovalRequest.Parse("/agent-api/v1/removal-targets", Query(("kind", "all"), ("x", r.X), ("y", r.Y),
            ("z", z), ("width", r.Width), ("height", r.Height), ("depth", 1), ("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeGoods>> Goods(int offset, CancellationToken ct) =>
        client.Goods(EconomyRequest.Parse("/agent-api/v1/goods", Query(("offset", offset), ("limit", 32))), ct);
    public Task<BridgeEnvelope<NativeOperation>> Operation(string session, string id, CancellationToken ct) =>
        client.BuildingOperation(DiagnosticsRequest.Parse("/agent-api/v1/building-operation", Query(("session", session), ("id", id))), ct);
    public Task<BridgeEnvelope<NativeAreaChange>> Mark(ForestryRequest r, Position p, CancellationToken ct) =>
        client.SetArea(ManagementRequest.Parse("/agent-api/v1/set-area", Query(("kind", "tree_cutting"), ("operation", "mark"),
            ("resource", ""), ("expectedResource", "unmarked"), ("session", r.Session), ("x", p.X), ("y", p.Y), ("z", p.Z), ("width", 1), ("height", 1))), ct);
}
