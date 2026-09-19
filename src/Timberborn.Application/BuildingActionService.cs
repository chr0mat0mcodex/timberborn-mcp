using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Abstractions;
using Timberborn.Contracts;

namespace Timberborn.Application;

public sealed class BuildingActionService(ITimberbornReadBackend reader, ITimberbornWriteBackend writer)
{
    // Serialize the entire transaction across service instances in this process.
    // This cannot lock the game UI or other API clients.
    private static readonly SemaphoreSlim Gate = new(1, 1);
    public async Task<JsonObject> InvokeAsync(JsonElement args, CancellationToken ct)
    {
        var started = DateTimeOffset.UtcNow;
        Guid id = default;
        bool target = false;
        bool? before = null, observed = null;
        bool entered = false, attempted = false, acknowledged = false;
        JsonObject Result(string outcome, Fault? fault = null) =>
            (JsonObject)JsonSerializer.SerializeToNode(new ToolResult<PauseActionResult>(1,
                fault is null ? "ok" : "error", new(id, target, before, observed, outcome),
                new(reader.Id, reader.Simulated, null, started, DateTimeOffset.UtcNow, false), [], fault), ContractJson.Options)!;
        try
        {
            if (!writer.WritesEnabled) throw Faults.Exception("writes_disabled");
            if (args.ValueKind != JsonValueKind.Object || args.EnumerateObject().Count() != 3 ||
                !args.TryGetProperty("id", out var idValue) || idValue.ValueKind != JsonValueKind.String ||
                !Guid.TryParseExact(idValue.GetString(), "D", out id) || id == Guid.Empty ||
                !args.TryGetProperty("paused", out var desired) || desired.ValueKind is not (JsonValueKind.True or JsonValueKind.False) ||
                !args.TryGetProperty("expectedPaused", out var expected) || expected.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                throw Faults.Exception("invalid_argument");
            target = desired.GetBoolean();
            await Gate.WaitAsync(ct);
            entered = true;
            var building = await reader.GetBuildingAsync(id, ct);
            if (building.Id != id) throw Faults.Exception("backend_incompatible");
            before = observed = building.Paused;
            if (building.Pausable != true || before is null) throw Faults.Exception("not_pausable");
            if (before != expected.GetBoolean()) throw Faults.Exception("state_conflict");
            if (before == target) return Result("unchanged");
            ct.ThrowIfCancellationRequested();
            attempted = true;
            observed = null;
            await writer.SetBuildingPausedAsync(id, target, ct);
            acknowledged = true;
            var after = await reader.GetBuildingAsync(id, ct);
            if (after.Id == id && after.Pausable == true) observed = after.Paused;
            return observed == target ? Result("applied") : Result("unconfirmed", Faults.Create("action_unconfirmed"));
        }
        catch (BackendException ex)
        {
            // Only explicit transport rejections are known not to have performed a mutation.
            bool rejected = !attempted || (!acknowledged && ex.Fault.Code is "writes_disabled" or "invalid_argument" or "authentication_failed");
            // An entity_not_found during readback is uncertain, so keep it conservative here.
            return Result(rejected ? "rejected" : "unconfirmed", rejected ? ex.Fault : Faults.Create("action_unconfirmed"));
        }
        catch (OperationCanceledException)
        { return Result(attempted ? "unconfirmed" : "rejected", Faults.Create(attempted ? "action_unconfirmed" : "cancelled")); }
        catch (Exception)
        { return Result(attempted ? "unconfirmed" : "rejected", Faults.Create(attempted ? "action_unconfirmed" : "internal_error")); }
        finally { if (entered) Gate.Release(); }
    }
}
