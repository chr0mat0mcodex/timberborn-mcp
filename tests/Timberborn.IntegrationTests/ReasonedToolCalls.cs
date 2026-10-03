using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
namespace Timberborn.IntegrationTests;

internal static class ReasonedToolCalls
{
    // Existing positive/functional cases must exercise the mandatory metadata too.
    public static ValueTask<CallToolResult> CallToolWithReasonAsync(this McpClient client, string name,
        IDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        var args = arguments is null ? new Dictionary<string,object?>() : new Dictionary<string,object?>(arguments);
        args.TryAdd("reasoning", "Integrationstest: " + name + " mit dem vorbereiteten Testzustand prüfen.");
        return client.CallToolAsync(name, args, cancellationToken: cancellationToken);
    }
}
