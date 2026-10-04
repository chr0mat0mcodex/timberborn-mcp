using System.Collections.Specialized;
namespace Timberborn.Bridge.Core;

public sealed class BuildingProjectExecutionRequest
{
    public string Session { get; private set; } = "";
    public Guid ActionId { get; private set; }
    public BuildingProjectValidationRequest? Validation { get; private set; }
    public string? InitialStorageGood { get; private set; }
    public string? InitialStorageMode { get; private set; }
    public bool HasInitialConfiguration => InitialStorageGood is not null || InitialStorageMode is not null;
    public static BuildingProjectExecutionRequest Parse(bool start, NameValueCollection query)
    {
        int optional = (query.GetValues("initialStorageGood") is null ? 0 : 1) +
            (query.GetValues("initialStorageMode") is null ? 0 : 1);
        if (query.Count != (start ? 13 + optional : 2) || !start && optional != 0) throw new ArgumentException();
        string One(string key) => query.GetValues(key) is { Length: 1 } values ? values[0] : throw new ArgumentException();
        if (!Guid.TryParse(One("session"), out var session) || session == Guid.Empty ||
            !Guid.TryParse(One("actionId"), out var action) || action == Guid.Empty) throw new ArgumentException();
        var result = new BuildingProjectExecutionRequest { Session = session.ToString("D"), ActionId = action };
        if (start)
        {
            if (One("mode") != "development_pilot") throw new ArgumentException();
            var copy = new NameValueCollection(query); copy.Remove("actionId"); copy.Remove("mode");
            if (query.GetValues("initialStorageGood") is not null) {
                result.InitialStorageGood = One("initialStorageGood");
                if (!BuildingPolicy.ValidTemplate(result.InitialStorageGood)) throw new ArgumentException();
                copy.Remove("initialStorageGood");
            }
            if (query.GetValues("initialStorageMode") is not null) {
                result.InitialStorageMode = One("initialStorageMode");
                if (!BuildingSettingsRequest.IsStorageMode(result.InitialStorageMode)) throw new ArgumentException();
                copy.Remove("initialStorageMode");
            }
            result.Validation = BuildingProjectValidationRequest.Parse(copy);
            if (!BuildingProjectPilotPolicy.SupportsTemplate(result.Validation.Plan.Template)) throw new ArgumentException();
        }
        return result;
    }
}
