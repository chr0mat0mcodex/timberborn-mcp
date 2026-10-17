using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class SimulationStartRejectionTests
{
    private static SimulationRunRequest Request(string? id=null) => SimulationRunRequest.Parse(
        "/agent-api/v1/simulation-run-for", new NameValueCollection {
            {"session","11111111-1111-4111-8111-111111111111"},
            {"runId",id??Guid.NewGuid().ToString("D")},{"duration","1"},{"unit","hours"},
            {"expectedSpeed","0"},{"speed","7"},{"maxRealSeconds","60"}});

    [Theory]
    [InlineData(true,false,false,"run_start_speed_locked")]
    [InlineData(false,true,false,"run_start_batch_busy")]
    [InlineData(false,false,true,"run_start_speed_pending")]
    public void NativeOwnerChecksHaveDistinctReasons(bool locked,bool owner,bool pending,string code)
    {
        Assert.Equal(code,Assert.Throws<BridgeRejectionException>(()=>
            SimulationStartPolicy.RequireAvailable(locked,owner,pending)).Code);
        SimulationStartPolicy.RequireAvailable(false,false,false);
    }

    [Fact]
    public void ChangedSpeedRejectsBeforeRegistrationOrCommandAndFreshRequestCanStart()
    {
        var controller=new SimulationRunController();var commands=new List<int>();var r=Request();
        Assert.Equal("run_start_speed_changed",Assert.Throws<BridgeRejectionException>(()=>
            controller.Start(r,24,7,0,commands.Add)).Code);
        Assert.Empty(commands);Assert.Null(controller.ActiveRunId);
        Assert.Equal("run_not_found",Assert.Throws<BridgeRejectionException>(()=>controller.Inspect(r.RunId)).Code);
        controller.Start(Request(),24,0,1,commands.Add);
        Assert.Single(commands);
    }

    [Fact]
    public void FullJournalRejectsWithoutAnotherCommand()
    {
        var controller=new SimulationRunController();var commands=new List<int>();
        for(int i=0;i<SimulationRunController.Capacity;i++) {
            controller.Start(Request(),24,0,i,commands.Add);controller.Interrupt();
        }
        var r=Request();
        Assert.Equal("run_start_capacity",Assert.Throws<BridgeRejectionException>(()=>controller.Start(r,24,0,200,commands.Add)).Code);
        Assert.Equal(SimulationRunController.Capacity,commands.Count);
        Assert.Equal("run_not_found",Assert.Throws<BridgeRejectionException>(()=>controller.Inspect(r.RunId)).Code);
    }

    [Fact]
    public async Task UntilStartHasSameSpecificOutcomeButCancelDoesNotClaimNoPriorRun()
    {
        foreach(bool cancel in new[]{false,true}) {
            using var handler=new Handler("run_start_speed_changed",cancel?"simulation-run-cancel":"simulation-run-until");
            using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableSpeedControl:true);
            var fields=new Dictionary<string,object> {
                ["session"]="11111111-1111-4111-8111-111111111111",
                ["runId"]="22222222-2222-4222-8222-222222222222"};
            if(!cancel) {
                fields["dayNumber"]=2;fields["hour"]=12;fields["expectedSpeed"]=0;
                fields["speed"]=7;fields["maxRealSeconds"]=60;
            }
            var result=await tools.Invoke(cancel?"cancel_simulation_run":"run_simulation_until",
                JsonSerializer.SerializeToElement(fields),TestContext.Current.CancellationToken);
            Assert.Equal(cancel?"unconfirmed":"not_executed",result["error"]!["execution"]!.GetValue<string>());
            Assert.Equal(1,handler.Calls);
            Assert.Equal(cancel?0:1,handler.PreflightReads);
            if(cancel) Assert.Null(result["operatingPreflight"]);
            else Assert.Equal("unavailable",result["operatingPreflight"]![0]!["state"]!.GetValue<string>());
        }
    }

    [Theory]
    [InlineData("run_start_speed_locked",true)]
    [InlineData("run_start_speed_pending",true)]
    [InlineData("run_start_batch_busy",true)]
    [InlineData("run_start_active",true)]
    [InlineData("run_start_capacity",true)]
    [InlineData("run_start_speed_changed",true)]
    [InlineData("run_start_stock_unavailable",true)]
    [InlineData("state_conflict",false)]
    [InlineData("run_id_used",false)]
    [InlineData("lost",false)]
    [InlineData("malformed",false)]
    public async Task OnlyNativePreStartRejectionProvesNonExecution(string code,bool rejected)
    {
        using var handler=new Handler(code);
        using var tools=new NativeTools(new NativeClient(new(8081,new string('a',64)),handler),enableSpeedControl:true);
        var args=JsonSerializer.SerializeToElement(new {
            session="11111111-1111-4111-8111-111111111111",runId="22222222-2222-4222-8222-222222222222",
            duration=1,unit="hours",expectedSpeed=0,speed=7,maxRealSeconds=60});
        var result=await tools.Invoke("run_simulation_for",args,TestContext.Current.CancellationToken);
        Assert.Equal(rejected?"not_executed":"unconfirmed",result["error"]!["execution"]!.GetValue<string>());
        Assert.False(result["error"]!["retryable"]!.GetValue<bool>());
        Assert.Equal(1,handler.Calls);
        Assert.Equal(0,handler.PreflightReads);
        Assert.Null(result["operatingPreflight"]);
        if(rejected) {
            Assert.Equal(code,result["error"]!["code"]!.GetValue<string>());
            Assert.Equal(SimulationStartOutcome.Describe(code),result["error"]!["message"]!.GetValue<string>());
        }
    }

    private sealed class Handler(string code,string route="simulation-run-for"):HttpMessageHandler
    {
        public int Calls;
        public int PreflightReads;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            if (request.Method == HttpMethod.Get) {
                Assert.Equal("/agent-api/v1/production-graph",request.RequestUri!.AbsolutePath);
                Assert.Equal("simulation-run-until",route);
                Assert.Equal(0,Calls);
                Assert.Equal(0,PreflightReads);
                PreflightReads++;
                // Synthetic unavailable diagnosis: the original start rejection must still survive.
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            }
            Calls++;Assert.Equal(HttpMethod.Post,request.Method);
            Assert.Equal("/agent-api/v1/"+route,request.RequestUri!.AbsolutePath);
            if(code=="lost")throw new HttpRequestException();
            var body=code=="malformed"?"{\"error\":\"run_start_speed_changed\",\"extra\":true}":JsonSerializer.Serialize(new{error=code});
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict){Content=new StringContent(body)});
        }
    }
}
