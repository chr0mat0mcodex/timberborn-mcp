using Timberborn.Bridge.Core;

namespace Timberborn.Backend.Native;

// The only exception to completed-with-pause is a confirmed pure-time handoff.
internal static class CompletionSpeedContract
{
    internal static bool ConfirmedContinuation(NativeSimulationRun d) =>
        d.State == "completed" && d.Terminal && d.Reason == "target_reached" && d.TargetReached &&
        d.CompletionSpeed == 1 && d.EffectiveCompletionSpeed == 1 && d.CompletionSpeedConfirmed &&
        d.CompletionPauseObserved && d.CompletionFallbackReason is null && d.ObservedSpeed == 1 && !d.PauseConfirmed &&
        d.ElapsedRealSeconds < d.MaxRealSeconds && PureTime(d);

    private static bool PureTime(NativeSimulationRun d) =>
        d.StopAtAvailableLogs is null && (d.GoodTargets?.Length ?? 0) == 0 && d.StopAtScience is null &&
        (d.BuildingIds?.Length ?? 0) == 0 && !d.MeasureProduction;

    internal static void Validate(NativeSimulationRun d, SimulationRunRequest request)
    {
        bool hasOption = d.CompletionSpeed == 1;
        bool completing = d.State == "completing";
        if (d.CompletionSpeed is not (0 or 1) || request.Starts && d.CompletionSpeed != request.CompletionSpeed ||
            hasOption && !PureTime(d) ||
            d.EffectiveCompletionSpeed is not (null or 0 or 1) ||
            d.CompletionSpeedConfirmed != d.EffectiveCompletionSpeed.HasValue ||
            d.CompletionSpeedConfirmed && (!d.Terminal || d.State is not ("completed" or "cancelled")) ||
            d.EffectiveCompletionSpeed == 0 && (!d.PauseConfirmed || d.ObservedSpeed != 0) ||
            d.EffectiveCompletionSpeed == 1 && !ConfirmedContinuation(d) ||
            !hasOption && (completing || d.CompletionPauseObserved || d.CompletionFallbackReason is not null) ||
            completing && (d.Terminal || !d.TargetReached || !d.CompletionPauseObserved || d.CompletionSpeedConfirmed ||
                d.CompletionFallbackReason is not null || d.Reason != "awaiting_completion_speed" || d.ObservedSpeed != 0) ||
            d.Reason == "awaiting_completion_speed" && !completing ||
            hasOption && d.State == "completed" && d.PauseConfirmed &&
                (d.CompletionFallbackReason is null || d.EffectiveCompletionSpeed != 0) ||
            d.CompletionFallbackReason is not (null or "game_speed_locked" or "cancel_requested" or "real_time_limit" or
                "completion_speed_unconfirmed" or "completion_observation_unavailable" or "clock_reversed" or
                "speed_command_failed" or "speed_changed" or "explicit_speed_change" or "pause_unconfirmed" or
                "speed_unconfirmed" or "simulation_stalled" or "session_ended_pause_unconfirmed"))
            throw new InvalidDataException("Invalid completion speed receipt");
    }
}
