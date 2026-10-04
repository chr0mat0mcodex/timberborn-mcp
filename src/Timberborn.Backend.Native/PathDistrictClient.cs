using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record NativePathDistrict(string Id, string DistrictId, string Template, bool Finished,
    bool Supported, Position? PathCell, bool? Connected, string Reason, string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativePathDistrict>> PathDistrict(LogisticsRequest r, CancellationToken ct)
    {
        if (r.Route != "path-district") throw new ArgumentException();
        var e = await Get<NativePathDistrict>($"path-district?id={r.Id}&districtId={r.DistrictId}&session={r.Session}", ct);
        ValidatePathDistrict(e, r);
        return e;
    }
    public static void ValidatePathDistrict(BridgeEnvelope<NativePathDistrict> e, LogisticsRequest r)
    {
        var d = e.Data;
        if (e.BridgeVersion is not ("0.29.3" or "0.30.0" or "0.31.0" or "0.31.1" or "0.31.2" or "0.31.3" or "0.31.4" or "0.32.0" or "0.32.1" or "0.33.0" or "0.33.1" or "0.34.0" or "0.35.0" or "0.35.1" or "0.35.2") || e.SessionId != r.Session || d is null ||
            d.Id != r.Id || d.DistrictId != r.DistrictId || !BuildingPolicy.ValidTemplate(d.Template) ||
            d.Supported != (d.Connected is not null) || d.Supported != (d.PathCell is not null) ||
            d.Supported && (!d.Finished || d.Reason != "observed") ||
            !d.Supported && (d.Reason is not ("not_path_object" or "unfinished_path") || d.Reason == "unfinished_path" && d.Finished) ||
            d.PathCell is { } p && (p.X is < 0 or > 4095 || p.Y is < 0 or > 4095 || p.Z is < 0 or > 4095) ||
            d.Limitations is null || d.Limitations.Length > 16 || d.Limitations.Any(s => string.IsNullOrEmpty(s) || s.Length > 160))
            throw new InvalidDataException("Invalid path district observation");
    }
}
