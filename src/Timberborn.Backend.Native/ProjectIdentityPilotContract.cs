using System.Text.Json;
using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

public static class ProjectIdentityPilotContract
{
    public static int Validate(JsonElement evidence, string session, string actionId, bool active)
    {
        if (evidence.GetProperty("source").GetString() != "development_identity_injection" ||
            evidence.GetProperty("injected").GetBoolean() != active || evidence.GetProperty("active").GetBoolean() != active ||
            evidence.GetProperty("session").GetString() != session || evidence.GetProperty("actionId").GetString() != actionId ||
            evidence.GetProperty("injectedObservation").GetString() != "identity_mismatch" ||
            evidence.GetProperty("scope").GetString() != "recovery_observation_only_no_entity_or_ledger_mutation")
            throw new InvalidDataException("Invalid identity injection evidence");
        int index = evidence.GetProperty("partIndex").GetInt32();
        string? real = evidence.GetProperty("realObserved").GetString();
        if (index is < 0 or >= 32 || real is null ||
            !(ProjectIdentityPilot.MatchingIdentity(real) || real is "missing" or "initializing" or "identity_mismatch"))
            throw new InvalidDataException("Invalid identity injection observation");
        return index;
    }
}
