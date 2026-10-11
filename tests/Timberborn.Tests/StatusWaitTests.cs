using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class StatusWaitTests
{
    private const string Session="11111111-1111-4111-8111-111111111111", Id="22222222-2222-4222-8222-222222222222";
    private static JsonObject Snapshot(string state="running") => JsonNode.Parse("""
        {"status":"ok","meta":{"sessionId":"synthetic-session","observedAtUtc":"2026-01-01T00:00:00Z"},
         "data":{"runId":"synthetic-run","state":"running","reason":"advancing","terminal":false,
          "pauseConfirmed":false,"observedSpeed":3,"elapsedGameHours":1,"buildingProduction":[]}}
        """)!.AsObject().WithState(state);
    private static StatusWaitTools.Options Options(int wait=30,string? after=null) => new(JsonSerializer.SerializeToElement(new{}),wait,after,false);
    private sealed class Clock : StatusWaitTools.IClock {
        public double Seconds { get; private set; }
        public Task Delay(TimeSpan duration,CancellationToken ct) {ct.ThrowIfCancellationRequested();Seconds+=duration.TotalSeconds;return Task.CompletedTask;}
    }

    [Fact]
    public async Task TimeOnlyProgressDoesNotWakeAndWaitIsBoundedWithoutDiagnostics()
    {
        var clock=new Clock();int calls=0;
        Task<JsonObject> Read(CancellationToken _) {calls++;var r=Snapshot();r["data"]!["elapsedGameHours"]=calls;return Task.FromResult(r);}
        var result=await StatusWaitTools.Wait("inspect_simulation_run",Options(),Read,TestContext.Current.CancellationToken,clock);
        Assert.Equal("wait_limit",result["watch"]!["reason"]!.GetValue<string>());
        Assert.Equal(30,clock.Seconds);Assert.Equal(30,calls);Assert.Equal(0,result["watch"]!["diagnosticRequests"]!.GetValue<int>());
        Assert.False(result["watch"]!["changed"]!.GetValue<bool>());
    }
    [Fact]
    public async Task CompletionAndBlockersReturnEarly()
    {
        int calls=0;var clock=new Clock();
        var result=await StatusWaitTools.Wait("inspect_simulation_run",Options(),_=>Task.FromResult(Snapshot(++calls==1?"running":"completed")),TestContext.Current.CancellationToken,clock);
        Assert.Equal(2,calls);Assert.Equal("terminal",result["watch"]!["reason"]!.GetValue<string>());Assert.Equal(1,clock.Seconds);
        var blocked=Snapshot();blocked["data"]!["buildingProduction"]=JsonNode.Parse("[{\"id\":\"synthetic\",\"blockers\":[\"output_capacity_unavailable\"]}]");
        result=await StatusWaitTools.Wait("inspect_simulation_run",Options(),_=>Task.FromResult(blocked),TestContext.Current.CancellationToken,new Clock());
        Assert.Equal("blocked",result["watch"]!["reason"]!.GetValue<string>());Assert.Equal(1,result["watch"]!["statusReads"]!.GetValue<int>());
    }
    [Fact]
    public async Task ExplicitCursorDetectsNewPartsAndIsScopedToSessionAndRun()
    {
        var r=Snapshot();r["data"]!["partStates"]=JsonNode.Parse("[\"pending\",\"pending\"]");
        string before=StatusWaitTools.ChangeToken("inspect_large_project",r);
        r["data"]!["partStates"]![0]="finished";
        var result=await StatusWaitTools.Wait("inspect_large_project",Options(after:before),_=>Task.FromResult(r),TestContext.Current.CancellationToken,new Clock());
        Assert.Equal("changed",result["watch"]!["reason"]!.GetValue<string>());Assert.True(result["watch"]!["changed"]!.GetValue<bool>());
        string token=StatusWaitTools.ChangeToken("inspect_large_project",r);r["meta"]!["sessionId"]="new-session";
        Assert.NotEqual(token,StatusWaitTools.ChangeToken("inspect_large_project",r));
        token=StatusWaitTools.ChangeToken("inspect_large_project",r);r["data"]!["runId"]="other-run";
        Assert.NotEqual(token,StatusWaitTools.ChangeToken("inspect_large_project",r));
    }
    [Fact]
    public async Task FailuresAndCallerCancellationDoNotRetryOrHideLastState()
    {
        int calls=0;var clock=new Clock();
        await Assert.ThrowsAsync<HttpRequestException>(()=>StatusWaitTools.Wait("inspect_simulation_run",Options(),_=>{
            if(++calls>1)throw new HttpRequestException();return Task.FromResult(Snapshot());
        },TestContext.Current.CancellationToken,clock));
        Assert.Equal(2,calls);
        using var stop=new CancellationTokenSource();stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>StatusWaitTools.Wait("inspect_simulation_run",Options(),_=>Task.FromResult(Snapshot()),stop.Token,new Clock()));
    }
    [Theory]
    [InlineData("-1")][InlineData("31")][InlineData("0.5")][InlineData("\"2\"")]
    public void InvalidWaitIsRejectedBeforeRead(string value)
    {
        using var d=JsonDocument.Parse($"{{\"session\":\"{Session}\",\"runId\":\"{Id}\",\"waitSeconds\":{value}}}");
        Assert.Throws<ArgumentException>(()=>StatusWaitTools.Parse("inspect_simulation_run",d.RootElement));
    }
    [Fact]
    public void DiagnosisOnlyAllowedOnNativeInspectionAndDuplicateArgumentsFail()
    {
        var q=new NameValueCollection{{"session",Session},{"runId",Id},{"diagnose","false"}};
        Assert.False(SimulationRunRequest.Parse("/agent-api/v1/simulation-run",q).Diagnose);
        Assert.Throws<ArgumentException>(()=>SimulationRunRequest.Parse("/agent-api/v1/simulation-run-cancel",q));
        q.Remove("diagnose");Assert.True(SimulationRunRequest.Parse("/agent-api/v1/simulation-run",q).Diagnose);
        using var duplicate=JsonDocument.Parse($"{{\"session\":\"{Session}\",\"runId\":\"{Id}\",\"waitSeconds\":0,\"waitSeconds\":1}}");
        Assert.Throws<ArgumentException>(()=>StatusWaitTools.Parse("inspect_simulation_run",duplicate.RootElement));
    }
    [Fact]
    public void PublicWaitOptionsAreOptionalAndLimitedToReadTools()
    {
        var catalog=SimulationRunTools.Catalog(true).Concat(LargeProjectTools.Catalog(true,true));
        foreach(var tool in catalog) {
            var described=ResponsePresentation.Describe(tool);
            bool read=StatusWaitTools.Handles(tool.Name);
            Assert.Equal(read,described.InputSchema.GetProperty("properties").TryGetProperty("waitSeconds",out var wait));
            if(!read)continue;
            Assert.Equal(30,wait.GetProperty("maximum").GetInt32());
            Assert.False(described.InputSchema.GetProperty("properties").GetProperty("diagnose").GetProperty("default").GetBoolean());
            Assert.DoesNotContain(described.InputSchema.GetProperty("required").EnumerateArray(),p=>p.GetString() is "waitSeconds" or "after" or "diagnose");
        }
    }
    [Fact]
    public async Task CheapReadRejectsAnOldBackendInsteadOfSilentlyScanning()
    {
        using var h=new Handler{Version="0.46.0"};using var client=new NativeClient(new(8081,new string('a',64)),h);
        await Assert.ThrowsAsync<InvalidDataException>(()=>StatusWaitTools.Invoke(client,"inspect_simulation_run",JsonSerializer.SerializeToElement(new{session=Session,runId=Id}),TestContext.Current.CancellationToken));
        Assert.Equal(1,h.Calls);Assert.Equal(0,h.Diagnostics);
    }
    [Theory]
    [InlineData(false,1,0)][InlineData(true,2,1)]
    public async Task StatusUsesGetWithoutDiagnosticsAndOptInEnrichesExactlyOnce(bool diagnose,int expectedCalls,int expectedDiagnostics)
    {
        using var h=new Handler();using var client=new NativeClient(new(8081,new string('a',64)),h);
        var result=await StatusWaitTools.Invoke(client,"inspect_simulation_run",JsonSerializer.SerializeToElement(new{session=Session,runId=Id,diagnose}),TestContext.Current.CancellationToken);
        Assert.Equal(expectedCalls,h.Calls);Assert.Equal(expectedDiagnostics,h.Diagnostics);
        Assert.Equal(expectedDiagnostics,result["watch"]!["diagnosticRequests"]!.GetValue<int>());
        Assert.Equal("ok",result["status"]!.GetValue<string>());
    }
    private sealed class Handler:HttpMessageHandler {
        public int Calls,Diagnostics;
        public string Version="0.47.0";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
            Assert.Equal(HttpMethod.Get,request.Method);Calls++;
            if(request.RequestUri!.Query.Contains("diagnose=true",StringComparison.Ordinal))Diagnostics++;
            else Assert.Contains("diagnose=false",request.RequestUri.Query);
            var data=new NativeSimulationRun(Id,"running","advancing",24,26,25,1,0,1,60,3,3,false,false,false,BuildingIds:[],BuildingProduction:[]);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(new BridgeEnvelope<NativeSimulationRun>(1,Session,DateTimeOffset.UnixEpoch,Version,data),NativeJson.Options))});
        }
    }
}

internal static class StatusWaitTestExtensions
{
    public static JsonObject WithState(this JsonObject node,string state) {
        node["data"]!["state"]=state;
        if(state=="completed") {node["data"]!["terminal"]=true;node["data"]!["pauseConfirmed"]=true;node["data"]!["observedSpeed"]=0;}
        return node;
    }
}
