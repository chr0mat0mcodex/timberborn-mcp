using System.Text.Json;
using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ManualProjectJournalContract
{
    // Additive diagnostic: older bridges may omit it; absence never means empty.
    public static void Validate(JsonElement data)
    {
        if (!data.TryGetProperty("manualJournal", out var journal)) return;
        if (journal.ValueKind != JsonValueKind.Object ||
            !journal.TryGetProperty("scope", out var scope) || scope.ValueKind != JsonValueKind.String || scope.GetString() != "manual_large_projects" ||
            !journal.TryGetProperty("limit", out var limit) || limit.ValueKind != JsonValueKind.Number || !limit.TryGetInt32(out var maximum) || maximum != LargeProjectLedger.Capacity ||
            !journal.TryGetProperty("count", out var count) || count.ValueKind != JsonValueKind.Number || !count.TryGetInt32(out var used) || used < 0 || used > maximum ||
            !journal.TryGetProperty("remaining", out var remaining) || remaining.ValueKind != JsonValueKind.Number || !remaining.TryGetInt32(out var free) || free != maximum - used ||
            !journal.TryGetProperty("releaseFreesCapacity", out var release) || release.ValueKind != JsonValueKind.False)
            throw new InvalidDataException("Invalid manual project journal observation");
    }
}
