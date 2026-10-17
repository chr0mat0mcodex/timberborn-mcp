using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class ParameterDiagnosticsTests
{
    [Theory]
    [InlineData("inspect_goods", "{\"offset\":0,\"limit\":100}", "limit", "1..32")]
    [InlineData("inspect_map_region", "{\"x\":0,\"y\":0,\"z\":0,\"width\":5,\"height\":6,\"depth\":5}", "depth", "1..4")]
    [InlineData("inspect_goods", "{\"offset\":0,\"limit\":\"secret-value\"}", "limit", "integer")]
    [InlineData("inspect_goods", "{\"offset\":0}", "limit", "fehlt")]
    [InlineData("inspect_map_region", "{\"x\":0,\"y\":0,\"z\":0,\"width\":5,\"height\":6,\"depth\":5,\"reasoning\":\"synthetic range check\"}", "depth", "1..4")]
    public async Task Read_argument_failures_explain_parameter_without_write_claim(string name,string json,string field,string expected)
    {
        var handler=new NoTransport();
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler));
        using var args=JsonDocument.Parse(json);
        var result=await tools.Invoke(name,args.RootElement,TestContext.Current.CancellationToken);
        Assert.Equal("invalid_argument",result["error"]!["code"]!.GetValue<string>());
        Assert.Equal("not_applicable",result["error"]!["execution"]!.GetValue<string>());
        string message=result["error"]!["message"]!.GetValue<string>();
        Assert.Contains(field,message); Assert.Contains(expected,message);
        Assert.DoesNotContain("reasoning",message);
        Assert.DoesNotContain("schreibender",message); Assert.DoesNotContain("secret-value",message);
        Assert.Equal(0,handler.Calls);
    }
    private sealed class NoTransport:HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        { Calls++; throw new InvalidOperationException("unexpected_transport"); }
    }
}
