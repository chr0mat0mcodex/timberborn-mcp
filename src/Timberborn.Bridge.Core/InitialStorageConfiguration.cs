namespace Timberborn.Bridge.Core;

// Configure the initialized construction site; do not wait for construction to finish.
public sealed class InitialStorageConfiguration
{
    public string? Good { get; set; }
    public string? Mode { get; set; }
    public string State { get; set; } = "waiting_order";
    public string Reason { get; set; } = "";
    public bool FinishedObserved { get; set; }
    public string? ObservedGood { get; set; }
    public string? ObservedMode { get; set; }

    public bool Terminal => State is "confirmed" or "conflict" or "unconfirmed" or "stopped";

    public void Tick(string constructionState, Func<(bool Finished, string Good, string Mode)> read,
        Action<string, string, string> set)
    {
        if (Terminal) return;
        if (constructionState is "stopped" or "unconfirmed") { State = "stopped"; Reason = "construction_not_confirmed"; return; }
        if (constructionState != "completed") return;
        try
        {
            var current = read();
            FinishedObserved = current.Finished;
            ObservedGood = current.Good; ObservedMode = current.Mode;
            if (State == "applying")
            {
                State = (Good is null || Good == current.Good) && (Mode is null || Mode == current.Mode)
                    ? "confirmed" : "unconfirmed";
                Reason = State == "confirmed" ? "settings_read_back" : "settings_mismatch";
                return;
            }
            // A fresh storage starts empty/accept. Do not overwrite intervening user choices.
            if (Good is not null && current.Good != "" && current.Good != Good ||
                Mode is not null && current.Mode != "accept" && current.Mode != Mode)
            { State = "conflict"; Reason = "initial_setting_changed"; return; }
            State = "applying"; // Consume the mutation attempt before calling the game.
            if (Good is not null && Good != current.Good) set("good", current.Good, Good);
            if (Mode is not null && Mode != current.Mode) set("mode", current.Mode, Mode);
            // Confirm only on a later frame; never repeat a mutation after uncertainty.
        }
        catch { State = "unconfirmed"; Reason = "configuration_operation_unconfirmed"; }
    }
}
