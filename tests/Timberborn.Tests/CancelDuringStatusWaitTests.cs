using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class CancelDuringStatusWaitTests
{
    private const string Session="11111111-1111-4111-8111-111111111111";
    private const string Id="22222222-2222-4222-8222-222222222222";
    private static JsonElement Cancel()=>JsonSerializer.SerializeToElement(new {session=Session,runId=Id});
    private static JsonElement Inspect(string name,int wait=30)=>name=="inspect_large_project"
        ? JsonSerializer.SerializeToElement(new {session=Session,actionId=Id,waitSeconds=wait})
        : JsonSerializer.SerializeToElement(new {session=Session,runId=Id,waitSeconds=wait});
    private static JsonElement Speed()=>JsonSerializer.SerializeToElement(new {session=Session,speed=3,expectedSpeed=0});
    private static async Task Rejected(Task<JsonObject> task) {
        var result=await task;
        Assert.Equal("not_executed",result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("inspect_simulation_run")]
    [InlineData("inspect_large_project")]
    public async Task CancelReachesTransportAndConfirmsPauseBeforeStatusReturns(string name)
    {
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var handler=new Reply{HoldOnRead=name=="inspect_simulation_run"?2:1};
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableSpeedControl:true);
        var read=tools.Invoke(name,Inspect(name),timeout.Token);
        try {
            await handler.ReadEntered.Task.WaitAsync(timeout.Token);
            var cancel=tools.Invoke("cancel_simulation_run",Cancel(),timeout.Token);
            await handler.CancelEntered.Task.WaitAsync(timeout.Token);
            await Rejected(tools.Invoke("set_simulation_speed",Speed(),timeout.Token));
            await Rejected(tools.Invoke("cancel_simulation_run",Cancel(),timeout.Token));
            var reader=await tools.Invoke(name,Inspect(name),timeout.Token);
            Assert.Equal("error",reader["status"]!.GetValue<string>());
            Assert.Equal(handler.HoldOnRead+1,handler.Calls);
            handler.ReleaseCancel.TrySetResult(true);
            var result=await cancel;
            Assert.Equal("ok",result["status"]!.GetValue<string>());
            Assert.True(result["data"]!["pauseConfirmed"]!.GetValue<bool>());
            Assert.Equal(0,result["data"]!["observedSpeed"]!.GetValue<float>());
            Assert.False(read.IsCompleted);
            handler.ReleaseRead.TrySetResult(true);
            var observed=await read;
            if(name=="inspect_simulation_run") {
                Assert.Equal("terminal",observed["watch"]!["reason"]!.GetValue<string>());
                Assert.True(observed["data"]!["pauseConfirmed"]!.GetValue<bool>());
            }
            Assert.Equal(1,handler.CancelCalls);
        } finally {handler.ReleaseRead.TrySetResult(true);handler.ReleaseCancel.TrySetResult(true);await read;}
    }

    [Fact]
    public async Task EndedReaderDoesNotAdmitNewWritesWhileCancelIsPendingAndFailureDoesNotRetry()
    {
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var readCancellation=CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
        using var handler=new Reply{LoseCancel=true};
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableSpeedControl:true);
        var read=tools.Invoke("inspect_simulation_run",Inspect("inspect_simulation_run"),readCancellation.Token);
        try {
            await handler.ReadEntered.Task.WaitAsync(timeout.Token);
            var cancel=tools.Invoke("cancel_simulation_run",Cancel(),timeout.Token);
            await handler.CancelEntered.Task.WaitAsync(timeout.Token);
            readCancellation.Cancel();
            await read;
            await Rejected(tools.Invoke("set_simulation_speed",Speed(),timeout.Token));
            await Rejected(tools.Invoke("cancel_simulation_run",Cancel(),timeout.Token));
            handler.ReleaseCancel.TrySetResult(true);
            var failed=await cancel;
            Assert.Equal("unconfirmed",failed["error"]!["execution"]!.GetValue<string>());
            Assert.False(failed["error"]!["retryable"]!.GetValue<bool>());
            Assert.Equal(1,handler.CancelCalls);
            handler.ReleaseRead.TrySetResult(true);
            var next=await tools.Invoke("inspect_simulation_run",Inspect("inspect_simulation_run",0),timeout.Token);
            Assert.Equal("ok",next["status"]!.GetValue<string>());
        } finally {handler.ReleaseRead.TrySetResult(true);handler.ReleaseCancel.TrySetResult(true);await read;}
    }

    [Theory]
    [InlineData(false,30)]
    [InlineData(true,0)]
    public async Task DisabledActionsAndNonWaitingReadsDoNotGainCancelAdmission(bool enabled,int wait)
    {
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var handler=new Reply();
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableSpeedControl:enabled);
        var read=tools.Invoke("inspect_simulation_run",Inspect("inspect_simulation_run",wait),timeout.Token);
        try {
            await handler.ReadEntered.Task.WaitAsync(timeout.Token);
            await Rejected(tools.Invoke("cancel_simulation_run",Cancel(),timeout.Token));
            Assert.Equal(0,handler.CancelCalls);
        } finally {handler.ReleaseRead.TrySetResult(true);timeout.Cancel();await read;}
    }

    private sealed class Reply:HttpMessageHandler
    {
        public int Calls,CancelCalls,ReadCalls;
        public int HoldOnRead=1;
        public bool LoseCancel;
        private int cancelled;
        public TaskCompletionSource<bool> ReadEntered=new(TaskCreationOptions.RunContinuationsAsynchronously),
            CancelEntered=new(TaskCreationOptions.RunContinuationsAsynchronously),
            ReleaseRead=new(TaskCreationOptions.RunContinuationsAsynchronously),
            ReleaseCancel=new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Interlocked.Increment(ref Calls);
            bool cancel=request.RequestUri!.AbsolutePath.EndsWith("/simulation-run-cancel",StringComparison.Ordinal);
            if(cancel) {
                Assert.Equal(HttpMethod.Post,request.Method);
                Interlocked.Increment(ref CancelCalls);CancelEntered.TrySetResult(true);
                await ReleaseCancel.Task.WaitAsync(ct);
                if(LoseCancel)throw new HttpRequestException("Synthetic lost cancel response");
                Volatile.Write(ref cancelled,1);
            } else {
                Assert.Equal(HttpMethod.Get,request.Method);
                if(Interlocked.Increment(ref ReadCalls)>=HoldOnRead) {
                    ReadEntered.TrySetResult(true);await ReleaseRead.Task.WaitAsync(ct);
                }
                if(!request.RequestUri.AbsolutePath.EndsWith("/simulation-run",StringComparison.Ordinal))
                    return new(HttpStatusCode.OK){Content=new StringContent("invalid synthetic project response")};
            }
            bool stopped=Volatile.Read(ref cancelled)==1;
            var data=new NativeSimulationRun(Id,stopped?"cancelled":"running",stopped?"cancel_requested":"advancing",
                24,26,25,1,0,1,60,3,stopped?0:3,stopped,false,stopped,BuildingIds:[],BuildingProduction:[]);
            return new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(
                new BridgeEnvelope<NativeSimulationRun>(1,Session,DateTimeOffset.UnixEpoch,"0.74.0",data),NativeJson.Options))};
        }
    }
}
