using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Timberborn.Backend.Native;
namespace Timberborn.TestFixtures;

// Deliberately synthetic: alternatives, a cycle, a natural source, and an unexplained good.
internal static class ProductionGraphFixture
{
    public static NativeProductionGraph Create()
    {
        var d=new GraphDefinitions(
            [new("A","Rohstoff",true),new("B","Produkt",true),new("Unknown","Unbekannt",false)],
            [new("Alternative",[new("A",2)],[new("B",3)],1,null,0,0,["Workshop"]),
             new("Forward",[new("A",1)],[new("B",1)],0.5f,"A",2,1,["Workshop"]),
             new("Reverse",[new("B",1)],[new("A",1)],1,null,0,0,[])],
            [new("Workshop",[new("A",5)],10,true,"","",false,50,0)],
            [new("Plant.gather","Plant","gather","A",2,0.5f,4,2,false,true,["Workshop"],["Workshop"],0.25f)]);
        string revision=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("synthetic-1\nSyntheticFaction\n"+JsonSerializer.Serialize(d,NativeJson.Options)))).ToLowerInvariant();
        return new("registered_definitions_and_active_scene_buildings","synthetic-1","SyntheticFaction",revision,true,d,
            new(["Unknown"],["Reverse"],[]),["synthetic_fixture"]);
    }
}
