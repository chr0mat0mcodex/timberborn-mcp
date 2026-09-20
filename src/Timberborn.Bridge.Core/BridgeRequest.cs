using System.Collections.Specialized;
using System.Globalization;

namespace Timberborn.Bridge.Core;

public sealed class BridgeRequest
{
    public string Route { get; }
    public DiagnosticsRequest? Diagnostics { get; private set; }
    public LogisticsRequest? Logistics { get; private set; }
    public EconomyRequest? Economy { get; private set; }
    public ResearchRequest? Research { get; private set; }
    public ActivityRequest? Activity { get; private set; }
    public BuildingSettingsRequest? Settings { get; private set; }
    public bool GenericBuilding => Route is "building-precheck" or "building-validation" or "building-placement";
    public RemovalRequest? Removal { get; private set; }
    public ManagementRequest? Management { get; private set; }
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }
    public int Offset { get; }
    public int Limit { get; }
    public string Template { get; }
    public int Rotation { get; }
    public string Session { get; }
    public string EntityId { get; }
    public int DesiredWorkers { get; }
    public int ExpectedDesiredWorkers { get; }
    public int Speed { get; }
    public int ExpectedSpeed { get; }
    private BridgeRequest(string route, int x = 0, int y = 0, int z = 0, int width = 0, int height = 0, int depth = 0,
        int offset = 0, int limit = 0, string template = "", int rotation = 0, string session = "", string entityId = "", int speed = 0, int expectedSpeed = 0, int desiredWorkers = 0, int expectedDesiredWorkers = 0)
    { Route = route; X = x; Y = y; Z = z; Width = width; Height = height; Depth = depth;
        Offset = offset; Limit = limit; Template = template; Rotation = rotation; Session = session; EntityId = entityId; Speed = speed; ExpectedSpeed = expectedSpeed; DesiredWorkers = desiredWorkers; ExpectedDesiredWorkers = expectedDesiredWorkers; }

    public static BridgeRequest Parse(string path, NameValueCollection query)
    {
        if (DiagnosticsRequest.Handles(path)) { var d=DiagnosticsRequest.Parse(path,query); return new BridgeRequest(d.Route,session:d.Session) { Diagnostics=d }; }
        if (LogisticsRequest.Handles(path)) { var l=LogisticsRequest.Parse(path,query); return new BridgeRequest(l.Route,session:l.Session) { Logistics=l }; }
        if (EconomyRequest.Handles(path)) { var e=EconomyRequest.Parse(path,query); return new BridgeRequest(e.Route,session:e.Session) { Economy=e }; }
        if (path is "/agent-api/v1/research" or "/agent-api/v1/unlock-building") { var r=ResearchRequest.Parse(path.EndsWith("unlock-building"),query); return new BridgeRequest(r.Write?"unlock-building":"research",session:r.Session) { Research=r }; }
        if (path == "/agent-api/v1/activity") return new BridgeRequest("activity") { Activity=ActivityRequest.Parse(query) };
        if (path == "/agent-api/v1/production-graph" && query.Count==0) return new BridgeRequest("production-graph");
        if (path == "/agent-api/v1/activity-log" && query.Count==0) return new BridgeRequest("activity-log");
        if (BuildingSettingsRequest.Handles(path)) { var r=BuildingSettingsRequest.Parse(path,query); return new BridgeRequest(r.Route, session:r.Session) { Settings=r }; }
        if (RemovalRequest.Handles(path)) { var r=RemovalRequest.Parse(path,query); return new BridgeRequest(r.Route, session:r.Session) { Removal=r }; }
        if (ManagementRequest.Handles(path)) { var m=ManagementRequest.Parse(path,query); return new BridgeRequest(m.Route, session: m.Session) { Management=m }; }
        if (path == "/agent-api/v1/simulation" && query.Count == 0) return new("simulation");
        if (path == "/agent-api/v1/snapshot" && query.Count == 0) return new("snapshot");
        if (path == "/agent-api/v1/catalog" && query.Count == 0) return new("catalog");
        if (path == "/agent-api/v1/building" && query.Count == 2)
        {
            string Identifier(string key)
            {
                var values = query.GetValues(key);
                if (values is null || values.Length != 1 || !Guid.TryParseExact(values[0], "D", out var id) || id == Guid.Empty)
                    throw new ArgumentException("invalid_identifier");
                return id.ToString("D");
            }
            return new("building", session: Identifier("session"), entityId: Identifier("id"));
        }
        int Read(string key, int min, int max)
        {
            var values = query.GetValues(key);
            if (values is null || values.Length != 1 || !int.TryParse(values[0], NumberStyles.None,
                    CultureInfo.InvariantCulture, out var value) || value < min || value > max)
                throw new ArgumentException("invalid_request");
            return value;
        }
        if (path == "/agent-api/v1/workplace-staffing" && query.Count == 4)
        {
            string Id(string key) {
                var values = query.GetValues(key);
                if (values is null || values.Length != 1 || !Guid.TryParseExact(values[0], "D", out var id) || id == Guid.Empty) throw new ArgumentException("invalid_identifier");
                return id.ToString("D");
            }
            return new("workplace-staffing", session: Id("session"), entityId: Id("id"), desiredWorkers: Read("desiredWorkers", 0, 64), expectedDesiredWorkers: Read("expectedDesiredWorkers", 0, 64));
        }
        if (path == "/agent-api/v1/simulation-speed" && query.Count == 3)
        {
            var sessions = query.GetValues("session");
            if (sessions is null || sessions.Length != 1 || !Guid.TryParseExact(sessions[0], "D", out var id) || id == Guid.Empty)
                throw new ArgumentException("invalid_session");
            int Speed(string key) { int value = Read(key, 0, 7); if (value is not (0 or 1 or 3 or 7)) throw new ArgumentException("invalid_speed"); return value; }
            return new("simulation-speed", session: id.ToString("D"), speed: Speed("speed"), expectedSpeed: Speed("expectedSpeed"));
        }
        if (path == "/agent-api/v1/workforce" && query.Count == 2)
            return new("workforce", offset: Read("offset", 0, 65535), limit: Read("limit", 1, 32));
        if (path == "/agent-api/v1/objects" && query.Count == 2)
            return new("objects", offset: Read("offset", 0, 65535), limit: Read("limit", 1, 32));
        if (path == "/agent-api/v1/building-catalog" && query.Count == 2)
            return new("building-catalog", offset: Read("offset", 0, 65535), limit: Read("limit", 1, 32));
        if (path is "/agent-api/v1/building-precheck" or "/agent-api/v1/building-validation" or "/agent-api/v1/building-placement")
        {
            bool write = path != "/agent-api/v1/building-precheck";
            bool place = path == "/agent-api/v1/building-placement";
            if (query.Count != (place ? 7 : write ? 6 : 5)) throw new ArgumentException("invalid_request");
            var names = query.GetValues("template");
            if (names is null || names.Length != 1 || !BuildingPolicy.ValidTemplate(names[0])) throw new ArgumentException("invalid_template");
            string Id(string key) {
                var values = query.GetValues(key);
                if (values is null || values.Length != 1 || !Guid.TryParseExact(values[0], "D", out var id) || id == Guid.Empty) throw new ArgumentException("invalid_identifier");
                return id.ToString("D");
            }
            return new(place ? "building-placement" : write ? "building-validation" : "building-precheck",
                Read("x", 0, 4095), Read("y", 0, 4095), Read("z", 0, 4095), template: names[0],
                rotation: Read("rotation", 0, 3), session: write ? Id("session") : "", entityId: place ? Id("actionId") : "");
        }
        bool lodge = path == "/agent-api/v1/lodge-placement"; bool placement = path == "/agent-api/v1/path-placement" || lodge;
        bool validation = path == "/agent-api/v1/site-validation" || placement;
        if ((path == "/agent-api/v1/site-precheck" && query.Count == 5) || (validation && query.Count == 6))
        {
            var values = query.GetValues("template");
            if (values is null || values.Length != 1 || values[0] is not ("Lodge.Folktails" or "Path"))
                throw new ArgumentException("invalid_request");
            if (placement && values[0] != (lodge ? "Lodge.Folktails" : "Path")) throw new ArgumentException("invalid_template");
            var session = "";
            if (validation)
            {
                var sessions = query.GetValues("session");
                if (sessions is null || sessions.Length != 1 || !Guid.TryParseExact(sessions[0], "D", out var id) || id == Guid.Empty)
                    throw new ArgumentException("invalid_session");
                session = id.ToString("D");
            }
            return new(placement ? (lodge ? "lodge-placement" : "path-placement") : validation ? "site-validation" : "site-precheck", Read("x", 0, 4095), Read("y", 0, 4095), Read("z", 0, 4095),
                template: values[0], rotation: Read("rotation", 0, 3), session: session);
        }
        if (path != "/agent-api/v1/map" || query.Count != 6) throw new ArgumentException("invalid_request");
        return new("map", Read("x", 0, 4095), Read("y", 0, 4095), Read("z", 0, 4095),
            Read("width", 1, 8), Read("height", 1, 8), Read("depth", 1, 4));
    }
}
