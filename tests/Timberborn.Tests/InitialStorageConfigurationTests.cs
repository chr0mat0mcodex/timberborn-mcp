using System.Collections.Specialized;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Timberborn.Backend.Native;
using System.Text.Json;
using Xunit;

namespace Timberborn.Tests;

public sealed class InitialStorageConfigurationTests
{
    [Fact]
    public void ConfiguresUnfinishedSiteAndConfirmsOnLaterTickWithoutReplay()
    {
        var config = new InitialStorageConfiguration { Good = "Carrot", Mode = "obtain" };
        string good = "", mode = "accept"; int writes = 0;
        (bool, string, string) Read() => (false, good, mode);
        void Set(string key, string expected, string value) {
            Assert.Equal(expected, key == "good" ? good : mode);
            if (key == "good") good = value; else mode = value;
            writes++;
        }
        config.Tick("running", Read, Set); Assert.Equal(0, writes);
        config.Tick("completed", Read, Set);
        Assert.Equal("applying", config.State); Assert.Equal(2, writes);
        config.Tick("completed", Read, Set);
        Assert.Equal("confirmed", config.State); Assert.False(config.FinishedObserved);
        config.Tick("completed", Read, Set); Assert.Equal(2, writes);
    }

    [Fact]
    public void ManualChoiceIsNotOverwritten()
    {
        var config = new InitialStorageConfiguration { Good = "Carrot", Mode = "obtain" };
        int writes = 0;
        config.Tick("completed", () => (false, "Berries", "accept"), (_, _, _) => writes++);
        Assert.Equal("conflict", config.State); Assert.Equal(0, writes);
    }

    [Fact]
    public void PartialMutationFailureNeverRetriesOrClaimsSuccess()
    {
        var config = new InitialStorageConfiguration { Good = "Water", Mode = "obtain" };
        int writes = 0;
        void Set(string key, string expected, string value) { writes++; if (key == "mode") throw new InvalidOperationException(); }
        config.Tick("completed", () => (false, "", "accept"), Set);
        config.Tick("completed", () => (false, "Water", "accept"), Set);
        Assert.Equal("unconfirmed", config.State); Assert.Equal(2, writes);
    }

    [Fact]
    public void FailedOrderNeverAppliesConfiguration()
    {
        var config = new InitialStorageConfiguration { Good = "Water" };
        config.Tick("unconfirmed", () => throw new Exception(), (_, _, _) => throw new Exception());
        Assert.Equal("stopped", config.State);
    }

    private static NameValueCollection Query() => new() {
        ["session"]="11111111-1111-1111-1111-111111111111",
        ["actionId"]="22222222-2222-2222-2222-222222222222",
        ["districtId"]="33333333-3333-3333-3333-333333333333",
        ["mode"]="development_pilot", ["template"]="SmallWarehouse.Folktails",
        ["x"]="10", ["y"]="10", ["z"]="2", ["width"]="8", ["height"]="8",
        ["rotation"]="0", ["optionIndex"]="0", ["planKey"]=new string('a',64) };

    [Fact]
    public void ConfigurationIsOptionalStrictAndAbsentFromPlanQuery()
    {
        var q = Query(); Assert.False(BuildingProjectExecutionRequest.Parse(true,q).HasInitialConfiguration);
        q.Add("initialStorageGood","Carrot"); q.Add("initialStorageMode","obtain");
        var r=BuildingProjectExecutionRequest.Parse(true,q);
        Assert.Equal("Carrot",r.InitialStorageGood); Assert.Equal("obtain",r.InitialStorageMode);
        q.Add("initialStorageGood","Water");
        Assert.Throws<ArgumentException>(()=>BuildingProjectExecutionRequest.Parse(true,q));
        q=Query(); q.Add("initialStorageMode","invalid");
        Assert.Throws<ArgumentException>(()=>BuildingProjectExecutionRequest.Parse(true,q));
        q=Query(); q.Add("initialStorageGood","");
        Assert.Throws<ArgumentException>(()=>BuildingProjectExecutionRequest.Parse(true,q));
    }

    [Fact]
    public void ToolSchemaKeepsConfigurationOptional()
    {
        var tool=Assert.Single(BuildingTools.Catalog(true),t=>t.Name=="execute_building_project_pilot");
        Assert.True(tool.InputSchema.GetProperty("properties").TryGetProperty("initialStorageGood",out _));
        Assert.DoesNotContain(tool.InputSchema.GetProperty("required").EnumerateArray(),p=>p.GetString()=="initialStorageGood");
    }

    [Fact]
    public void ReceiptRequiresRequestedConfigurationAndAcceptsUnfinishedConfirmation()
    {
        var q=Query(); q.Add("initialStorageGood","Carrot");
        var request=BuildingProjectExecutionRequest.Parse(true,q);
        var data=new NativeProjectExecution(q["actionId"]!,q["planKey"]!,"completed","order_and_access_confirmed",
            [new(q["actionId"]!,"SmallWarehouse.Folktails",10,11,2,0,"confirmed")],false,[]);
        var envelope=new BridgeEnvelope<NativeProjectExecution>(1,q["session"]!,DateTimeOffset.UtcNow,"0.35.3",data);
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateProjectExecution(envelope,request));
        var config=new InitialStorageConfiguration { Good="Carrot", State="confirmed", ObservedGood="Carrot", ObservedMode="accept", FinishedObserved=false };
        envelope=envelope with { Data=data with { InitialConfiguration=config } };
        NativeClient.ValidateProjectExecution(envelope,request);
        config.ObservedGood="Berries";
        Assert.Throws<InvalidDataException>(()=>NativeClient.ValidateProjectExecution(envelope,request));
    }

    [Fact]
    public async Task ConfigurationCannotBypassMcpSettingsOptIn()
    {
        using var client=new NativeClient(new(8081,new string('a',64)));
        using var args=JsonDocument.Parse("{\"initialStorageGood\":\"Carrot\"}");
        await Assert.ThrowsAsync<ArgumentException>(()=>BuildingTools.Invoke(client,"execute_building_project_pilot",
            args.RootElement,true,CancellationToken.None,false));
    }
}
