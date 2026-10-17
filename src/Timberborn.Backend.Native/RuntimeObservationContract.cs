using System.Text.Json;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public static class RuntimeObservationContract
{
    public static void Minimum(int? minimum,string? source,int desired,int maximum,bool required)
    {
        if (!required && minimum is null && source is null) return;
        if (source == "unknown_public_api" && minimum is null) return;
        if (source != "observed_decrease_boundary" || minimum is not > 0 || minimum > desired || minimum > maximum)
            throw new InvalidDataException("invalid_staffing_minimum_evidence");
    }
    public static void Simulation(NativeSimulation d,bool required)
    {
        if (!required && d.SpeedLockSource is null && d.SpeedLocked is null && d.PauseSettled is null && d.SpeedLockReason is null) return;
        if (d.SpeedLockReason != "not_exposed_by_public_api") throw new InvalidDataException("invalid_speed_lock_reason");
        if (d.SpeedLockSource == "not_observed" && d.SpeedLocked is null && d.PauseSettled is null) return;
        if (d.SpeedLockSource != "speed_lock_changed_event" || d.SpeedLocked is null || d.PauseSettled is null ||
            d.PauseSettled != (d.CurrentSpeed == 0 && d.SpeedLocked == false && d.PendingSpeed is null))
            throw new InvalidDataException("invalid_speed_lock_evidence");
    }
    public static void Construction(JsonElement part,int partCount)
    {
        var pause=part.GetProperty("manuallyPaused");
        if (pause.ValueKind is not (JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null)) throw new InvalidDataException();
        var supports=part.GetProperty("unfinishedSupportIndices").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
        int index=part.GetProperty("index").GetInt32();
        if (supports.Length>31 || supports.Distinct().Count()!=supports.Length || supports.Any(i=>i<0||i>=partCount||i==index)) throw new InvalidDataException();
        if (part.GetProperty("observed").GetString()!="construction") return;
        var c=part.GetProperty("construction");
        bool? Flag(string key)=>c.ValueKind==JsonValueKind.Null?null:c.GetProperty(key).GetBoolean();
        bool? workingHours=null;
        if(part.TryGetProperty("constructionCenterWorkingHours",out var hours)) {
            if(hours.ValueKind is not (JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null) ||
                !part.TryGetProperty("workingHoursSource",out var source) || source.GetString()!="district_center_workplace_component" ||
                !part.TryGetProperty("constructionCenterId",out var center) ||
                !Guid.TryParseExact(center.GetString(),"D",out var centerId) || centerId==Guid.Empty)
                throw new InvalidDataException("invalid_construction_working_hours_evidence");
            workingHours=hours.ValueKind==JsonValueKind.Null?null:hours.GetBoolean();
        }
        var reason=LargeProjectRecoveryPolicy.ConstructionReason(pause.ValueKind==JsonValueKind.Null?null:pause.GetBoolean(),
            Flag("isOn"),Flag("hasMaterialsToResumeBuilding"),supports.Length>0,workingHours);
        if (part.GetProperty("reason").GetString()!=reason) throw new InvalidDataException("inconsistent_construction_reason");
    }
}
