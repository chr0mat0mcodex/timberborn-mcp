using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
using Timberborn.McpServer;
using Xunit;

namespace Timberborn.Tests;

public sealed class RecipeSettingsTests
{
    private static readonly Guid Id=Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string Session="22222222-2222-2222-2222-222222222222";
    private static NativeBuildingSettings Settings()=>new(Id,"SyntheticWorkshop",new(1,2,3),true,new(false,true),
        false,null,false,null,[],true,new("SyntheticRecipe",true,[new("SyntheticRecipe",1,[new("Wheat",2)],[new("Flour",1)],0,null)]));
    private static NameValueCollection Query()=>new(){["id"]=Id.ToString(),["session"]=Session,["recipe"]="SyntheticRecipe",["expectedRecipe"]=""};

    [Fact]
    public void RecipeRequiresExplicitExpectedStateAndHasWriteGate()
    {
        var q=Query();var r=BuildingSettingsRequest.Parse("/agent-api/v1/set-building-recipe",q);
        Assert.True(r.Write);Assert.Equal("recipe",BuildingSettingsRequest.ValueKey(r.Route));
        Assert.Equal("",r.ExpectedValue);
        q.Remove("expectedRecipe");Assert.Throws<ArgumentException>(()=>BuildingSettingsRequest.Parse("/agent-api/v1/set-building-recipe",q));
        q=Query();q["recipe"]="";Assert.Throws<ArgumentException>(()=>BuildingSettingsRequest.Parse("/agent-api/v1/set-building-recipe",q));
        q=Query();q.Add("recipe","Other");Assert.Throws<ArgumentException>(()=>BuildingSettingsRequest.Parse("/agent-api/v1/set-building-recipe",q));
        Assert.DoesNotContain(BuildingSettingsTools.Catalog(false),t=>t.Name=="set_building_recipe");
        Assert.False(Assert.Single(BuildingSettingsTools.Catalog(true),t=>t.Name=="set_building_recipe").Annotations!.ReadOnlyHint);
    }

    [Theory]
    [InlineData("ok")][InlineData("duplicate")][InlineData("unknown_selected")][InlineData("negative_amount")]
    [InlineData("unfinished")][InlineData("missing_availability")][InlineData("zero_cycle")][InlineData("missing_recipes")]
    public async Task ValidatesOptionsAndAvailability(string defect)
    {
        var d=Settings(); var recipes=d.Recipes!;
        d=defect switch {
            "duplicate"=>d with{Recipes=recipes with{AllowedRecipes=[recipes.AllowedRecipes[0],recipes.AllowedRecipes[0]]}},
            "unknown_selected"=>d with{Recipes=recipes with{SelectedRecipe="Unknown"}},
            "negative_amount"=>d with{Recipes=recipes with{AllowedRecipes=[recipes.AllowedRecipes[0] with{Ingredients=[new("Wheat",-1)]}]}},
            "zero_cycle"=>d with{Recipes=recipes with{AllowedRecipes=[recipes.AllowedRecipes[0] with{CycleHours=0}]}},
            "unfinished"=>d with{Finished=false},
            "missing_availability"=>d with{RecipeComponentPresent=null,Recipes=null},
            "missing_recipes"=>d with{Recipes=null},
            _=>d };
        using var handler=new Reply(d);
        using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var q=Query();q.Remove("recipe");q.Remove("expectedRecipe");
        var request=BuildingSettingsRequest.Parse("/agent-api/v1/building-settings",q);
        if(defect=="ok") Assert.Single((await client.BuildingSettings(request,TestContext.Current.CancellationToken)).Data.Recipes!.AllowedRecipes);
        else await Assert.ThrowsAsync<InvalidDataException>(()=>client.BuildingSettings(request,TestContext.Current.CancellationToken));
        Assert.Equal(1,handler.Calls);
    }

    [Theory]
    [InlineData(false)][InlineData(true)]
    public async Task RecipeReceiptMustMatchReadbackWithoutRetry(bool wrong)
    {
        var d=Settings();
        var receipt=new NativeSettingChange(Id,d.Template,"recipe","","SyntheticRecipe",wrong?"Other":"SyntheticRecipe","applied",d,[]);
        using var handler=new Reply(receipt);using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var request=BuildingSettingsRequest.Parse("/agent-api/v1/set-building-recipe",Query());
        if(wrong) await Assert.ThrowsAsync<InvalidDataException>(()=>client.SetBuildingSetting(request,TestContext.Current.CancellationToken));
        else Assert.Equal("SyntheticRecipe",(await client.SetBuildingSetting(request,TestContext.Current.CancellationToken)).Data.ObservedValue);
        Assert.Equal(1,handler.Calls);Assert.Equal(HttpMethod.Post,handler.Method);
    }

    [Fact]
    public void ReadinessSeparatesWarningsUnknownsAndProductionEvidence()
    {
        var d=new NativeOperation(Id.ToString(),"SyntheticWorkshop",true,false,new(1,2,2,true,false,true),
            new(true,"SyntheticRecipe",true,true,true,false,true,0),[],[],[],new(true,true,true,true,100,200));
        var a=OperationAssessmentBuilder.Read(d);
        Assert.Empty(a.Blockers);Assert.Contains("understaffed",a.Warnings);Assert.Contains("network_power_deficit",a.Warnings);
        Assert.False(a.ProductionProven);
        a=OperationAssessmentBuilder.Read(d with{Manufacturing=null,Power=null});
        Assert.Contains("manufacturing_not_observed",a.Unknown);Assert.Contains("power_not_observed",a.Unknown);
        a=OperationAssessmentBuilder.Read(d with{Paused=true,Manufacturing=new(false,null,false,null,null,null,null,0),Power=new(true,true,false,true,0,100)});
        Assert.Contains("recipe_missing",a.Blockers);Assert.Contains("building_paused",a.Blockers);Assert.Contains("consumer_not_powered",a.Blockers);
        a=OperationAssessmentBuilder.Read(d with{Manufacturing=new(true,"SyntheticRecipe",false,false,false,true,false,0)});
        Assert.Contains("ingredients_not_ready",a.Blockers);Assert.Contains("fuel_not_ready",a.Blockers);Assert.Contains("output_capacity_unavailable",a.Blockers);
    }

    [Fact]
    public void NewVersionPreservesExistingConstructionModes()
    {
        var old=BuildingCapabilityTools.Describe("0.37.0","Folktails",true);
        var next=BuildingCapabilityTools.Describe("0.38.0","Folktails",true);
        Assert.Equal("known",next.ProfileState);Assert.Equal(old.Modes.Select(m=>m.Tool),next.Modes.Select(m=>m.Tool));
        Assert.Equal(old.MaxObjectFootprintCells,next.MaxObjectFootprintCells);
    }

    [Theory]
    [InlineData("ok")][InlineData("no_component")][InlineData("missing")][InlineData("contradiction")][InlineData("negative")]
    public async Task PowerAndAssessmentComeFromOneValidatedObservation(string variant)
    {
        OperationPower? power=variant switch {
            "missing"=>null,
            "no_component"=>new(false,null,null,null,null,null),
            "contradiction"=>new(false,true,true,true,100,50),
            "negative"=>new(true,true,true,true,-1,50),
            _=>new(true,true,false,true,0,50) };
        var d=new NativeOperation(Id.ToString(),"SyntheticWorkshop",true,false,null,
            new(true,"SyntheticRecipe",true,true,true,false,true,0),[],[],[],power,new([],[],[],true));
        using var handler=new Reply(d);using var client=new NativeClient(new(8081,new string('a',64)),handler);
        var request=DiagnosticsRequest.Parse("/agent-api/v1/building-operation",new(){["id"]=Id.ToString(),["session"]=Session});
        if(variant is "ok" or "no_component") {
            var result=await client.BuildingOperation(request,TestContext.Current.CancellationToken);
            Assert.False(result.Data.Assessment!.ProductionProven); // Never trust an upstream success claim.
            if(variant=="ok")Assert.Contains("consumer_not_powered",result.Data.Assessment.Blockers);
            else Assert.Empty(result.Data.Assessment.Blockers);
        } else await Assert.ThrowsAsync<InvalidDataException>(()=>client.BuildingOperation(request,TestContext.Current.CancellationToken));
        Assert.Equal(1,handler.Calls);
    }

    private sealed class Reply(object data):HttpMessageHandler
    {
        public int Calls; public HttpMethod? Method;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;Method=request.Method;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(
                JsonSerializer.Serialize(new BridgeEnvelope<object>(1,Session,DateTimeOffset.UnixEpoch,"0.38.0",data),NativeJson.Options))});
        }
    }
}
