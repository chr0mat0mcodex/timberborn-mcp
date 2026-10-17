using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class ActionExecutionTests
{
    private const string Session="11111111-1111-4111-8111-111111111111";
    private static JsonElement Speed(bool valid=true)=>JsonSerializer.SerializeToElement(new {session=Session,speed=valid?3:8,expectedSpeed=0,reasoning="Synthetic action check"});
    private static NativeTools Tools(HttpMessageHandler h)=>new(new NativeClient(new(8081,new string('a',64)),h),enableSpeedControl:true);
    [Fact]
    public async Task InvalidArgumentsAndCancelledBeforeTransportAreNotExecuted()
    {
        using var h=new Reply();using var tools=Tools(h);
        var bad=await tools.Invoke("set_simulation_speed",Speed(false),TestContext.Current.CancellationToken);
        Assert.Equal("not_executed",bad["error"]!["execution"]!.GetValue<string>());Assert.Equal(0,h.Calls);
        using var cancel=new CancellationTokenSource();cancel.Cancel();
        var cancelled=await tools.Invoke("set_simulation_speed",Speed(),cancel.Token);
        Assert.Equal("not_executed",cancelled["error"]!["execution"]!.GetValue<string>());Assert.Equal(0,h.Calls);
        Assert.False(cancelled["error"]!["retryable"]!.GetValue<bool>());
    }
    [Theory]
    [InlineData("lost")]
    [InlineData("malformed")]
    [InlineData("bad_request")]
    [InlineData("conflict")]
    [InlineData("cancelled")]
    public async Task AnyFailureAfterWriteDispatchRemainsUnconfirmed(string response)
    {
        using var h=new Reply{Mode=response};using var tools=Tools(h);
        var result=await tools.Invoke("set_simulation_speed",Speed(),TestContext.Current.CancellationToken);
        Assert.Equal("unconfirmed",result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());Assert.Equal(1,h.Calls);
        if(response=="bad_request")Assert.Equal("invalid_argument",result["error"]!["code"]!.GetValue<string>());
    }
    [Fact]
    public async Task ActivityPostsDoNotCountAsGameplayDispatch()
    {
        using var h=new Reply();using var tools=Tools(h);
        var result=await tools.InvokeLogged("set_simulation_speed",Speed(false),TestContext.Current.CancellationToken);
        Assert.Equal("not_executed",result["error"]!["execution"]!.GetValue<string>());
        Assert.Equal(2,h.Logs);Assert.Equal(0,h.Calls);
    }
    [Fact]
    public async Task ConcurrentAndSubsequentCallsHaveIndependentDispatchState()
    {
        using var h=new Reply{Mode="waiting"};using var tools=Tools(h);
        var pending=tools.Invoke("set_simulation_speed",Speed(),TestContext.Current.CancellationToken);
        await h.Entered.Task.WaitAsync(TestContext.Current.CancellationToken);
        var rejected=await tools.Invoke("set_simulation_speed",Speed(false),TestContext.Current.CancellationToken);
        Assert.Equal("not_executed",rejected["error"]!["execution"]!.GetValue<string>());
        h.Release.SetResult(true);
        Assert.Equal("unconfirmed",(await pending)["error"]!["execution"]!.GetValue<string>());
        var next=await tools.Invoke("set_simulation_speed",Speed(false),TestContext.Current.CancellationToken);
        Assert.Equal("not_executed",next["error"]!["execution"]!.GetValue<string>());
    }
    [Fact]
    public async Task ReadTransportFailureDoesNotSuggestUnknownMutation()
    {
        using var h=new Reply{Mode="lost"};using var tools=Tools(h);
        var result=await tools.Invoke("inspect_simulation",JsonSerializer.SerializeToElement(new{}),TestContext.Current.CancellationToken);
        Assert.Equal("not_applicable",result["error"]!["execution"]!.GetValue<string>());
        Assert.True(result["error"]!["retryable"]!.GetValue<bool>());
    }
    [Fact]
    public async Task ContendingActionIsNotQueuedAndCancellationDoesNotReleaseAnotherCallsGate()
    {
        using var h=new Reply{Mode="waiting"};using var tools=Tools(h);
        var pending=tools.Invoke("set_simulation_speed",Speed(),TestContext.Current.CancellationToken);
        try {
            await h.Entered.Task.WaitAsync(TestContext.Current.CancellationToken);
            using var cancel=new CancellationTokenSource();cancel.Cancel();
            var cancelled=await tools.Invoke("set_simulation_speed",Speed(),cancel.Token);
            Assert.Equal("not_executed",cancelled["error"]!["execution"]!.GetValue<string>());
            var contending=await tools.Invoke("set_simulation_speed",Speed(),TestContext.Current.CancellationToken);
            Assert.Equal("not_executed",contending["error"]!["execution"]!.GetValue<string>());
            Assert.False(contending["error"]!["retryable"]!.GetValue<bool>());
            var stop=await tools.Invoke("cancel_simulation_run",JsonSerializer.SerializeToElement(new {
                session=Session,runId="22222222-2222-4222-8222-222222222222"
            }),TestContext.Current.CancellationToken);
            Assert.Equal("not_executed",stop["error"]!["execution"]!.GetValue<string>());
            Assert.Equal(1,h.Calls);
        } finally {h.Release.TrySetResult(true);await pending;}
        Assert.Equal("unconfirmed",(await pending)["error"]!["execution"]!.GetValue<string>());
        h.Mode="malformed";
        var next=await tools.Invoke("set_simulation_speed",Speed(),TestContext.Current.CancellationToken);
        Assert.Equal("unconfirmed",next["error"]!["execution"]!.GetValue<string>());
        Assert.Equal(2,h.Calls);
    }
    private sealed class Reply:HttpMessageHandler
    {
        public string Mode="malformed";
        public int Calls,Logs;
        public TaskCompletionSource<bool> Entered=new(TaskCreationOptions.RunContinuationsAsynchronously),Release=new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            if(request.RequestUri!.AbsolutePath.EndsWith("/activity")) {
                Logs++;
                return new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new BridgeEnvelope<NativeActivityAck>(1,Session,DateTimeOffset.UnixEpoch,"0.45.0",new(true)),NativeJson.Options))};
            }
            Calls++;
            if(Mode=="waiting") {Entered.SetResult(true);await Release.Task.WaitAsync(ct);throw new HttpRequestException();}
            if(Mode=="lost")throw new HttpRequestException();
            if(Mode=="cancelled")throw new OperationCanceledException();
            return Mode switch {
                "bad_request"=>new(HttpStatusCode.BadRequest){Content=new StringContent("{\"error\":\"invalid_request\"}")},
                "conflict"=>new(HttpStatusCode.Conflict){Content=new StringContent("{\"error\":\"state_conflict\"}")},
                _=>new(HttpStatusCode.OK){Content=new StringContent("invalid json")}
            };
        }
    }
}
