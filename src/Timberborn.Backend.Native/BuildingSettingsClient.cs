using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativeStorageSettings(string? SelectedGood, bool CanChangeGood, string[] AllowedGoods,
    string? Mode, bool CanChangeMode, int Capacity, NativeGoodAmount[] Stock, bool HasUnwantedStock);
public sealed record NativeFarmPlant(string Resource, bool Unlocked);
public sealed record NativeFarmSettings(string Priority, string? PrioritizedResource, bool CanChangeCrop,
    bool CanClearCropPriority, NativeFarmPlant[]? AllowedPlants);
public sealed record NativeRecipeOption(string Id, float CycleHours, NativeGoodAmount[] Ingredients, NativeGoodAmount[] Products, int Science, string? Fuel);
public sealed record NativeRecipeSettings(string SelectedRecipe, bool CanChangeRecipe, NativeRecipeOption[] AllowedRecipes);
public sealed record NativeBuildingSettings(Guid Id, string Template, Position Position, bool Finished, NativePause? Pause,
    bool StorageComponentPresent, NativeStorageSettings? Storage, bool FarmComponentPresent, NativeFarmSettings? Farm, string[] Limitations, bool? RecipeComponentPresent=null, NativeRecipeSettings? Recipes=null);
public sealed record NativeSettingChange(Guid Id, string Template, string Setting, string PreviousValue, string RequestedValue,
    string ObservedValue, string Outcome, NativeBuildingSettings Observation, string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeBuildingSettings>> BuildingSettings(BuildingSettingsRequest r, CancellationToken ct)
    {
        if(r.Write) throw new ArgumentException();
        var result=await Get<NativeBuildingSettings>($"building-settings?id={r.Id}&session={r.Session}",ct);
        if(result.BridgeVersion is not ("0.15.0" or "0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0" or "0.19.0" or "0.19.1" or "0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or "0.21.1" or "0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0") || result.SessionId!=r.Session || result.Data.Id.ToString("D")!=r.Id)
            throw new InvalidDataException("Settings correlation failed");
        ValidateSettings(result.Data);
        if(result.BridgeVersion is ("0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0") && result.Data.RecipeComponentPresent is null) throw new InvalidDataException("Missing recipe availability"); return result;
    }
    public async Task<BridgeEnvelope<NativeSettingChange>> SetBuildingSetting(BuildingSettingsRequest r, CancellationToken ct)
    {
        if(!r.Write) throw new ArgumentException();
        string key=BuildingSettingsRequest.ValueKey(r.Route), expected=BuildingSettingsRequest.ExpectedKey(r.Route);
        var result=await Get<NativeSettingChange>($"{r.Route}?id={r.Id}&session={r.Session}&{key}={Uri.EscapeDataString(r.Value)}&{expected}={Uri.EscapeDataString(r.ExpectedValue)}",ct,HttpMethod.Post);
        var d=result.Data;
        if(result.BridgeVersion is not ("0.15.0" or "0.16.0" or "0.17.0" or "0.17.1" or "0.17.2" or "0.18.0" or "0.19.0" or "0.19.1" or "0.19.2" or "0.20.0" or "0.20.1" or "0.21.0" or "0.21.1" or "0.22.0" or "0.23.0" or "0.23.1" or "0.23.2" or "0.24.0" or "0.24.1" or "0.25.0" or "0.26.0" or "0.27.0" or "0.28.0" or "0.28.1" or "0.29.0" or "0.29.1" or "0.29.2" or "0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2" or "0.35.3" or "0.35.4" or "0.35.5" or "0.36.0" or "0.37.0" or "0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0") || result.SessionId!=r.Session || d.Id.ToString("D")!=r.Id ||
            d.Setting!=key || d.PreviousValue!=r.ExpectedValue || d.RequestedValue!=r.Value ||
            d.Outcome is not ("applied" or "unconfirmed") || d.Observation is null || d.Limitations is null)
            throw new InvalidDataException("Invalid settings receipt");
        ValidateSettings(d.Observation);
        if(result.BridgeVersion is ("0.38.0" or "0.39.0" or "0.40.0" or "0.41.0" or "0.42.0" or "0.43.0" or "0.44.0" or "0.45.0" or "0.46.0" or "0.47.0" or "0.48.0" or "0.49.0" or "0.50.0" or "0.51.0" or "0.52.0" or "0.53.0" or "0.54.0" or "0.55.0" or "0.56.0" or "0.57.0" or "0.58.0") && d.Observation.RecipeComponentPresent is null)
            throw new InvalidDataException("Missing recipe availability");
        if(d.Observation.Id!=d.Id || d.Observation.Template!=d.Template || Value(d.Observation,r.Route)!=d.ObservedValue ||
            d.Outcome=="applied" && d.ObservedValue!=d.RequestedValue)
            throw new InvalidDataException("Inconsistent settings receipt");
        return result;
    }
    private static string? Value(NativeBuildingSettings d,string route) => route switch {
        "set-building-paused" => d.Pause is { } p ? p.Paused?"true":"false" : null,
        "set-building-recipe" => d.Recipes?.SelectedRecipe,
        "set-storage-good" => d.Storage?.SelectedGood, "set-storage-mode" => d.Storage?.Mode,
        "set-farm-priority" => d.Farm?.Priority, "set-farm-crop" => d.Farm?.PrioritizedResource, _ => null };
    private static void ValidateSettings(NativeBuildingSettings d)
    {
        if(d.Id==Guid.Empty || !BuildingPolicy.ValidTemplate(d.Template) || d.Position is null || d.Limitations is null ||
            (d.Finished && d.StorageComponentPresent)!=(d.Storage is not null) ||
            (d.Finished && d.FarmComponentPresent)!=(d.Farm is not null)) throw new InvalidDataException("Invalid settings availability");
        if(d.RecipeComponentPresent is null && d.Recipes is not null ||
            d.RecipeComponentPresent is not null && (d.Finished && d.RecipeComponentPresent==true)!=(d.Recipes is not null))
            throw new InvalidDataException("Invalid recipe availability");
        if(d.Recipes is {} recipes) {
            bool Amounts(NativeGoodAmount[]? values)=>values is not null && values.Length<=64 &&
                values.All(g=>g is not null && BuildingPolicy.ValidTemplate(g.Id) && g.Amount>0) && values.Select(g=>g.Id).Distinct().Count()==values.Length;
            if(recipes.SelectedRecipe is null || recipes.AllowedRecipes is null || recipes.AllowedRecipes.Length>64 ||
                recipes.AllowedRecipes.Any(r=>r is null || !BuildingPolicy.ValidTemplate(r.Id) || !float.IsFinite(r.CycleHours) || r.CycleHours<=0 ||
                    !Amounts(r.Ingredients) || !Amounts(r.Products) || r.Science<0 || r.Fuel is not null && !BuildingPolicy.ValidTemplate(r.Fuel)) ||
                recipes.AllowedRecipes.Select(r=>r.Id).Distinct().Count()!=recipes.AllowedRecipes.Length ||
                recipes.CanChangeRecipe!=(recipes.AllowedRecipes.Length>0) ||
                recipes.SelectedRecipe.Length>0 && !recipes.AllowedRecipes.Any(r=>r.Id==recipes.SelectedRecipe))
                throw new InvalidDataException("Invalid recipe choices");
        }
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
