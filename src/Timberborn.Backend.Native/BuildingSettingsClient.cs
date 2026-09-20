using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeStorageSettings(string? SelectedGood, bool CanChangeGood, string[] AllowedGoods,
    string? Mode, bool CanChangeMode, int Capacity, NativeGoodAmount[] Stock, bool HasUnwantedStock);
public sealed record NativeFarmPlant(string Resource, bool Unlocked);
public sealed record NativeFarmSettings(string Priority, string? PrioritizedResource, bool CanChangeCrop,
    bool CanClearCropPriority, NativeFarmPlant[]? AllowedPlants);
public sealed record NativeBuildingSettings(Guid Id, string Template, Position Position, bool Finished, NativePause? Pause,
    bool StorageComponentPresent, NativeStorageSettings? Storage, bool FarmComponentPresent, NativeFarmSettings? Farm, string[] Limitations);
public sealed record NativeSettingChange(Guid Id, string Template, string Setting, string PreviousValue, string RequestedValue,
    string ObservedValue, string Outcome, NativeBuildingSettings Observation, string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeBuildingSettings>> BuildingSettings(BuildingSettingsRequest r, CancellationToken ct)
    {
        if(r.Write) throw new ArgumentException();
        var result=await Get<NativeBuildingSettings>($"building-settings?id={r.Id}&session={r.Session}",ct);
        if(result.BridgeVersion is not ("0.15.0" or "0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0") || result.SessionId!=r.Session || result.Data.Id.ToString("D")!=r.Id)
            throw new InvalidDataException("Settings correlation failed");
        ValidateSettings(result.Data); return result;
    }
    public async Task<BridgeEnvelope<NativeSettingChange>> SetBuildingSetting(BuildingSettingsRequest r, CancellationToken ct)
    {
        if(!r.Write) throw new ArgumentException();
        string key=BuildingSettingsRequest.ValueKey(r.Route), expected=BuildingSettingsRequest.ExpectedKey(r.Route);
        var result=await Get<NativeSettingChange>($"{r.Route}?id={r.Id}&session={r.Session}&{key}={Uri.EscapeDataString(r.Value)}&{expected}={Uri.EscapeDataString(r.ExpectedValue)}",ct,HttpMethod.Post);
        var d=result.Data;
        if(result.BridgeVersion is not ("0.15.0" or "0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0") || result.SessionId!=r.Session || d.Id.ToString("D")!=r.Id ||
            d.Setting!=key || d.PreviousValue!=r.ExpectedValue || d.RequestedValue!=r.Value ||
            d.Outcome is not ("applied" or "unconfirmed") || d.Observation is null || d.Limitations is null)
            throw new InvalidDataException("Invalid settings receipt");
        ValidateSettings(d.Observation);
        if(d.Observation.Id!=d.Id || d.Observation.Template!=d.Template || Value(d.Observation,r.Route)!=d.ObservedValue ||
            d.Outcome=="applied" && d.ObservedValue!=d.RequestedValue)
            throw new InvalidDataException("Inconsistent settings receipt");
        return result;
    }
    private static string? Value(NativeBuildingSettings d,string route) => route switch {
        "set-building-paused" => d.Pause is { } p ? p.Paused?"true":"false" : null,
        "set-storage-good" => d.Storage?.SelectedGood, "set-storage-mode" => d.Storage?.Mode,
        "set-farm-priority" => d.Farm?.Priority, "set-farm-crop" => d.Farm?.PrioritizedResource, _ => null };
    private static void ValidateSettings(NativeBuildingSettings d)
    {
        if(d.Id==Guid.Empty || !BuildingPolicy.ValidTemplate(d.Template) || d.Position is null || d.Limitations is null ||
            (d.Finished && d.StorageComponentPresent)!=(d.Storage is not null) ||
            (d.Finished && d.FarmComponentPresent)!=(d.Farm is not null)) throw new InvalidDataException("Invalid settings availability");
        if(d.Storage is { } s) {
            if(s.Capacity<0 || s.AllowedGoods is null || s.AllowedGoods.Length>128 || s.AllowedGoods.Any(g=>!BuildingPolicy.ValidTemplate(g)) ||
                s.AllowedGoods.Distinct().Count()!=s.AllowedGoods.Length || s.CanChangeGood && s.SelectedGood is null ||
                s.SelectedGood is { Length:>0 } && !BuildingPolicy.ValidTemplate(s.SelectedGood) ||
                s.CanChangeMode!=(s.Mode is not null) || s.Mode is not null && !BuildingSettingsRequest.IsStorageMode(s.Mode) ||
                s.Stock is null || s.Stock.Length>128 || s.Stock.Any(g=>g is null || !BuildingPolicy.ValidTemplate(g.Id) || g.Amount<0) ||
                s.Stock.Select(g=>g.Id).Distinct().Count()!=s.Stock.Length)
                throw new InvalidDataException("Invalid storage settings");
        }
        if(d.Farm is { } f) {
            if(!BuildingSettingsRequest.IsFarmPriority(f.Priority) || f.CanClearCropPriority ||
                f.CanChangeCrop && (f.PrioritizedResource is null || f.AllowedPlants is null) ||
                f.PrioritizedResource is { Length:>0 } && !BuildingPolicy.ValidTemplate(f.PrioritizedResource) ||
                f.AllowedPlants is { } plants && (plants.Length>64 || plants.Any(p=>p is null || !BuildingPolicy.ValidTemplate(p.Resource)) ||
                    plants.Select(p=>p.Resource).Distinct().Count()!=plants.Length)) throw new InvalidDataException("Invalid farm settings");
        }
    }
}
