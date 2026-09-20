using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;
namespace Timberborn.Tests;

public sealed class RejectionTests
{
    [Fact]
    public async Task QueuePreservesOnlyExplicitRejections()
    {
        using var queue=new MainThreadQueue();
        var request=BridgeRequest.Parse("/agent-api/v1/snapshot",new());
        var known=queue.Enqueue(request,TestContext.Current.CancellationToken);
        queue.Pump(_=>throw new BridgeRejectionException("template_locked"));
        Assert.Equal("template_locked",(await Assert.ThrowsAsync<BridgeRejectionException>(()=>known)).Code);
        var unknown=queue.Enqueue(request,TestContext.Current.CancellationToken);
        queue.Pump(_=>throw new ArgumentException("private data template_locked"));
        Assert.Equal("invalid_region",(await Assert.ThrowsAsync<ArgumentException>(()=>unknown)).Message);
        Assert.Throws<ArgumentException>(()=>new BridgeRejectionException("private data"));
    }

    [Theory]
    [InlineData(409,"{\"error\":\"template_locked\"}","template_locked")]
    [InlineData(409,"{\"error\":\"stale_session\"}","stale_session")]
    [InlineData(409,"{\"error\":\"state_conflict\"}","state_conflict")]
    [InlineData(400,"{\"error\":\"invalid_request\"}","invalid_argument")]
    [InlineData(409,"{\"error\":\"private data\"}","backend_incompatible")]
    [InlineData(409,"{\"error\":\"template_locked\",\"details\":\"private data\"}","backend_incompatible")]
    [InlineData(409,"{\"error\":\"template_locked\",\"error\":\"state_conflict\"}","backend_incompatible")]
    [InlineData(409,"null","backend_incompatible")]
    [InlineData(503,"{\"error\":\"private data\"}","backend_unavailable")]
    public async Task TypedFailureIsSafeAndNeverRetried(int status,string body,string code)
    {
        using var handler=new Reply((HttpStatusCode)status,body);
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableBuildingPlacement:true);
        var args=JsonSerializer.SerializeToElement(new { template="SyntheticBuilding",x=1,y=2,z=3,rotation=0,session=Guid.NewGuid().ToString("D") });
        var result=await tools.Invoke("validate_building",args,TestContext.Current.CancellationToken);
        Assert.Equal(code,result["error"]!["code"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.DoesNotContain("private data",result.ToJsonString());Assert.Equal(1,handler.Calls);
    }
    private sealed class Reply(HttpStatusCode status,string body):HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {Calls++;return Task.FromResult(new HttpResponseMessage(status){Content=new StringContent(body)});}
    }
}
