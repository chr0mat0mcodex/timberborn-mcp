using System.Collections.Specialized;
using System.Globalization;

namespace Timberborn.Bridge.Core;

public sealed class ActivityLogQuery
{
    public int Limit { get; private set; } = 32;
    public string Tool { get; private set; } = "";
    public string State { get; private set; } = "";
    public string Cursor { get; private set; } = "";
    public string ProjectId { get; private set; } = "";
    public string RunId { get; private set; } = "";
    public bool Matches(ActivityEntry entry) =>
        (ProjectId.Length==0 || entry.Correlations?.Any(c=>c.Kind is ("project" or "parent_project") && c.Id==ProjectId)==true) &&
        (RunId.Length==0 || entry.Correlations?.Any(c=>c.Kind=="run" && c.Id==RunId)==true);
    public static ActivityLogQuery Parse(NameValueCollection values)
    {
        var result = new ActivityLogQuery();
        foreach (string? key in values.AllKeys)
        {
            var items = values.GetValues(key);
            if (items is null || items.Length != 1 || string.IsNullOrEmpty(items[0]))
                throw new ArgumentException("invalid_activity_query");
            string value = items[0];
            switch (key)
            {
                case "limit":
                    if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int limit) || limit < 1 || limit > 32)
                        throw new ArgumentException("invalid_activity_limit");
                    result.Limit = limit; break;
                case "tool":
                    if (value.Length > 80 || value.Any(c => !(c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_')))
                        throw new ArgumentException("invalid_activity_tool");
                    result.Tool = value; break;
                case "state":
                    if (!ActivityRequest.IsState(value)) throw new ArgumentException("invalid_activity_state");
                    result.State = value; break;
                case "cursor":
                    if (value.Length > 260) throw new ArgumentException("invalid_activity_cursor");
                    result.Cursor = value; break;
                case "projectId": case "runId":
                    if(!Guid.TryParseExact(value,"D",out var id)||id==Guid.Empty)throw new ArgumentException("invalid_activity_identity");
                    if(key=="projectId")result.ProjectId=id.ToString("D");else result.RunId=id.ToString("D");
                    break;
                default: throw new ArgumentException("invalid_activity_query");
            }
        }
        if (result.Cursor.Length > 0) result.DecodeCursor();
        return result;
    }

    public (string Epoch, long Before) DecodeCursor()
    {
        var parts = Cursor.Split(':');
        if ((parts.Length != 4 && parts.Length != 6) || !Guid.TryParseExact(parts[0], "N", out _) ||
            !long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out long before) || before < 1 ||
            parts[2] != Tool || parts[3] != State ||
            (parts.Length==4 ? ProjectId.Length>0 || RunId.Length>0 : parts[4]!=ProjectId || parts[5]!=RunId))
            throw new ArgumentException("invalid_activity_cursor_or_filters");
        return (parts[0], before);
    }

    public string EncodeCursor(string epoch, long before) =>
        epoch + ":" + before.ToString(CultureInfo.InvariantCulture) + ":" + Tool + ":" + State +
        (ProjectId.Length==0&&RunId.Length==0?"":":"+ProjectId+":"+RunId);
}
