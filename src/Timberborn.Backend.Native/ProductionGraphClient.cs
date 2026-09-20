using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Timberborn.Bridge.Core;
namespace Timberborn.Backend.Native;

public sealed record GraphGood(string Id,string Name,bool Active);
public sealed record GraphAmount(string Good,int Amount);
public sealed record GraphRecipe(string Id,GraphAmount[] Inputs,GraphAmount[] Outputs,float Hours,string? Fuel,int FuelCycles,int ScienceOutput,string[] Buildings);
public sealed record GraphBuilding(string Id,GraphAmount[] Costs,int ScienceCost,bool Available,string RequiredFeature,string DisablingFeature,bool WaterInput,int? PowerInput,int? PowerOutput);
public sealed record GraphSource(string Id,string Resource,string Kind,string Good,int Amount,float HarvestHours,float? GrowthDays,float? RegrowthDays,bool RemovesPlant,bool Available,string[] Harvesters,string[] Planters,float? PlantHours);
public sealed record GraphDefinitions(GraphGood[] Goods,GraphRecipe[] Recipes,GraphBuilding[] Buildings,GraphSource[] Sources);
public sealed record GraphCoverage(string[] GoodsWithoutKnownSource,string[] RecipesWithoutSceneBuilding,string[] SourcesWithoutSceneHarvester);
public sealed record NativeProductionGraph(string Scope,string GameVersion,string Faction,string GraphRevision,bool CompleteWithinScope,GraphDefinitions Definitions,GraphCoverage Coverage,string[] Limitations);

public sealed partial class NativeClient
{
    public async Task<BridgeEnvelope<NativeProductionGraph>> ProductionGraph(CancellationToken ct)
    {
        var raw=await Get<JsonElement>("production-graph",ct);
        if(raw.BridgeVersion!="0.21.0")throw new InvalidDataException("Production graph unavailable");
        var graph=raw.Data.Deserialize<NativeProductionGraph>(NativeJson.Options)??throw new InvalidDataException();
        ValidateGraph(graph);
        string context=graph.GameVersion+"\n"+graph.Faction+"\n"+raw.Data.GetProperty("definitions").GetRawText();
        var revision=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(context))).ToLowerInvariant();
        if(graph.GraphRevision!=revision)throw new InvalidDataException("Graph revision mismatch");
        return new(raw.SchemaVersion,raw.SessionId,raw.ObservedAtUtc,raw.BridgeVersion,graph);
    }
    private static void ValidateGraph(NativeProductionGraph g)
    {
        static void Require([System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)] bool ok){if(!ok)throw new InvalidDataException("Invalid production graph");}
        static bool Id(string? s)=>s is not null&&BuildingPolicy.ValidTemplate(s);
        static bool Text(string? s,int max)=>s is not null&&s.Length<=max;
        static bool Number(float? n)=>n is null||float.IsFinite(n.Value)&&n>=0;
        static bool Ordered(string[]? ids)=>ids is not null&&ids.All(Id)&&ids.SequenceEqual(ids.Distinct().Order(StringComparer.Ordinal));
        Require(g.Scope=="registered_definitions_and_active_scene_buildings"&&g.CompleteWithinScope&&Text(g.GameVersion,80)&&g.GameVersion.Length>0&&Id(g.Faction)&&g.Definitions is not null&&g.Coverage is not null&&g.Limitations is not null&&g.Limitations.All(s=>Text(s,180)));
        var d=g.Definitions;
        Require(d.Goods is {Length:>0 and <=512}&&d.Recipes is {Length:<=512}&&d.Buildings is {Length:<=1024}&&d.Sources is {Length:<=1024});
        Require(d.Goods.All(x=>x is not null&&Text(x.Name,160))&&d.Recipes.All(x=>x is not null)&&d.Buildings.All(x=>x is not null)&&d.Sources.All(x=>x is not null));
        Require(Ordered(d.Goods.Select(x=>x.Id).ToArray())&&Ordered(d.Recipes.Select(x=>x.Id).ToArray())&&Ordered(d.Buildings.Select(x=>x.Id).ToArray())&&Ordered(d.Sources.Select(x=>x.Id).ToArray()));
        var goods=d.Goods.Select(x=>x.Id).ToHashSet();var buildings=d.Buildings.Select(x=>x.Id).ToHashSet();
        bool Links(string[]? links)=>Ordered(links)&&links!.All(buildings.Contains);
        bool Amounts(GraphAmount[]? a)=>a is {Length:<=128}&&a.All(x=>x is not null&&goods.Contains(x.Good)&&x.Amount>0)&&Ordered(a.Select(x=>x.Good).ToArray());
        foreach(var r in d.Recipes)Require(Amounts(r.Inputs)&&Amounts(r.Outputs)&&Number(r.Hours)&&r.Hours>0&&r.ScienceOutput>=0&&Links(r.Buildings)&&
            (r.Fuel is null?r.FuelCycles==0:goods.Contains(r.Fuel)&&r.FuelCycles>0));
        foreach(var b in d.Buildings)Require(Amounts(b.Costs)&&b.ScienceCost>=0&&Text(b.RequiredFeature,160)&&Text(b.DisablingFeature,160)&&b.PowerInput is null or >=0&&b.PowerOutput is null or >=0);
        foreach(var s in d.Sources)Require(Id(s.Resource)&&s.Kind is "cut" or "gather"&&goods.Contains(s.Good)&&s.Amount>0&&Number(s.HarvestHours)&&Number(s.GrowthDays)&&Number(s.RegrowthDays)&&Number(s.PlantHours)&&Links(s.Harvesters)&&Links(s.Planters));
        var produced=d.Recipes.SelectMany(r=>r.Outputs.Select(o=>o.Good)).Concat(d.Sources.Select(s=>s.Good)).ToHashSet();
        Require(g.Coverage.GoodsWithoutKnownSource is not null&&g.Coverage.RecipesWithoutSceneBuilding is not null&&g.Coverage.SourcesWithoutSceneHarvester is not null&&
            g.Coverage.GoodsWithoutKnownSource.SequenceEqual(d.Goods.Where(x=>!produced.Contains(x.Id)).Select(x=>x.Id))&&
            g.Coverage.RecipesWithoutSceneBuilding.SequenceEqual(d.Recipes.Where(x=>x.Buildings.Length==0).Select(x=>x.Id))&&
            g.Coverage.SourcesWithoutSceneHarvester.SequenceEqual(d.Sources.Where(x=>x.Harvesters.Length==0).Select(x=>x.Id)));
    }
}
