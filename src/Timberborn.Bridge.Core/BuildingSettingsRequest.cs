using System.Collections.Specialized;
namespace Timberborn.Bridge.Core;

public sealed class BuildingSettingsRequest
{
    public string Route { get; private set; } = "";
    public string Id { get; private set; } = "";
    public string Session { get; private set; } = "";
    public string Value { get; private set; } = "";
    public string ExpectedValue { get; private set; } = "";
    public bool Write => Route != "building-settings";
    public static bool Handles(string path) => path is "/agent-api/v1/building-settings" or
        "/agent-api/v1/set-building-paused" or "/agent-api/v1/set-storage-good" or "/agent-api/v1/set-storage-mode" or
        "/agent-api/v1/set-farm-priority" or "/agent-api/v1/set-farm-crop";
    public static bool IsStorageMode(string value) => value is "accept" or "empty" or "obtain" or "supply";
    public static bool IsFarmPriority(string value) => value is "planting" or "harvesting";
    public static string ValueKey(string route) => route switch {
        "set-building-paused" => "paused", "set-storage-good" => "good", "set-storage-mode" => "mode",
        "set-farm-priority" => "priority", "set-farm-crop" => "resource", _ => throw new ArgumentException() };
    public static string ExpectedKey(string route) => "expected" + char.ToUpperInvariant(ValueKey(route)[0]) + ValueKey(route).Substring(1);
    public static BuildingSettingsRequest Parse(string path, NameValueCollection q)
    {
        if (!Handles(path)) throw new ArgumentException("invalid_settings_route");
        string Text(string key) {
            var v=q.GetValues(key);
            if(v is null || v.Length!=1 || v[0] is null || v[0]!.Length>160) throw new ArgumentException("invalid_parameter");
            return v[0]!;
        }
        string Id(string key) {
            if(!Guid.TryParseExact(Text(key),"D",out var id) || id==Guid.Empty) throw new ArgumentException("invalid_id");
            return id.ToString("D");
        }
        var r=new BuildingSettingsRequest { Route=path.Substring("/agent-api/v1/".Length), Id=Id("id"), Session=Id("session") };
        if(q.Count!=(r.Write?4:2)) throw new ArgumentException("invalid_parameter_count");
        if(!r.Write) return r;
        r.Value=Text(ValueKey(r.Route)); r.ExpectedValue=Text(ExpectedKey(r.Route));
        bool Identifier(string s) => s=="" || BuildingPolicy.ValidTemplate(s);
        bool valid=r.Route switch {
            "set-building-paused" => (r.Value is "true" or "false") && (r.ExpectedValue is "true" or "false"),
            "set-storage-mode" => IsStorageMode(r.Value) && IsStorageMode(r.ExpectedValue),
            "set-farm-priority" => IsFarmPriority(r.Value) && IsFarmPriority(r.ExpectedValue),
            "set-storage-good" => Identifier(r.Value) && Identifier(r.ExpectedValue),
            "set-farm-crop" => BuildingPolicy.ValidTemplate(r.Value) && Identifier(r.ExpectedValue),
            _ => false };
        if(!valid) throw new ArgumentException("invalid_setting_value");
        return r;
    }
}

public sealed class SettingChangeResult(string previous, string requested, string observed, string outcome)
{
    public string Previous { get; } = previous;
    public string Requested { get; } = requested;
    public string Observed { get; } = observed;
    public string Outcome { get; } = outcome;
}

public static class SettingChange
{
    // Invoked synchronously on the game thread. A stale observation must never overwrite a user's setting.
    public static SettingChangeResult Execute(string expected, string requested, Func<string> read, Action set)
    {
        var previous=read();
        if(previous!=expected) throw new ArgumentException("setting_changed");
        var outcome="applied";
        if(previous!=requested) { try { set(); } catch { outcome="unconfirmed"; } }
        var observed=read();
        if(observed!=requested) outcome="unconfirmed";
        return new(previous,requested,observed,outcome);
    }
}
