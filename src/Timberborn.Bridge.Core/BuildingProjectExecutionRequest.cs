using System.Collections.Specialized;
namespace Timberborn.Bridge.Core;

public sealed class BuildingProjectExecutionRequest
{
    public string Session { get; private set; } = "";
    public Guid ActionId { get; private set; }
    public BuildingProjectValidationRequest? Validation { get; private set; }
    public static BuildingProjectExecutionRequest Parse(bool start, NameValueCollection query)
    {
        if (query.Count != (start ? 13 : 2)) throw new ArgumentException();
        string One(string key) => query.GetValues(key) is { Length: 1 } values ? values[0] : throw new ArgumentException();
        if (!Guid.TryParse(One("session"), out var session) || session == Guid.Empty ||
            !Guid.TryParse(One("actionId"), out var action) || action == Guid.Empty) throw new ArgumentException();
        var result = new BuildingProjectExecutionRequest { Session = session.ToString("D"), ActionId = action };
        if (start)
        {
            if (One("mode") != "development_pilot") throw new ArgumentException();
            var copy = new NameValueCollection(query); copy.Remove("actionId"); copy.Remove("mode");
            result.Validation = BuildingProjectValidationRequest.Parse(copy);
            if (!BuildingProjectPilotPolicy.SupportsTemplate(result.Validation.Plan.Template)) throw new ArgumentException();
        }
        return result;
    }
}
