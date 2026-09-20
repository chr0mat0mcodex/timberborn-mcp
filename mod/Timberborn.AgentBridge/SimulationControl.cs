using Timberborn.Bridge.Core;
using Timberborn.TimeSystem;

namespace Timberborn.AgentBridge;

public sealed class SimulationControl(SpeedManager speed, IDayNightCycle time)
{
    public object Observe() => new { currentSpeed = speed.CurrentSpeed, dayNumber = time.DayNumber,
        dayProgress = time.DayProgress, hoursPassedToday = time.HoursPassedToday };

    public object Set(BridgeRequest request)
    {
        // Compare on the game thread. Never override a newer user speed or unlock a game lock.
        float previous = speed.CurrentSpeed;
        if (previous != request.ExpectedSpeed) throw new ArgumentException("speed_changed");
        speed.ChangeSpeed(request.Speed);
        return new { requestedSpeed = request.Speed, previousSpeed = previous,
            matchedImmediately = speed.CurrentSpeed == request.Speed, observation = Observe(),
            limitations = new[] { "read_simulation_to_confirm", "no_automatic_retry", "game_locks_not_overridden",
                "pause_and_normal_speed_only", "speed_is_not_measured_tick_throughput" } };
    }
}
