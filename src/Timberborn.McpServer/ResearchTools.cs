using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class ResearchTools
{
    public static bool Handles(string name)=>name is "inspect_research" or "unlock_building";
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        foreach(bool write in enabled?new[]{false,true}:new[]{false}) {
            var props=new JsonObject();
            if(write) {
                props["template"]=new JsonObject { ["type"]="string",["minLength"]=1,["maxLength"]=160,["pattern"]="^[A-Za-z0-9._-]+$" };
                props["session"]=new JsonObject { ["type"]="string",["format"]="uuid" };
                props["expectedCost"]=new JsonObject { ["type"]="integer",["minimum"]=0,["maximum"]=int.MaxValue };
            } else {
                props["offset"]=new JsonObject { ["type"]="integer",["minimum"]=0,["maximum"]=65535 };
                props["limit"]=new JsonObject { ["type"]="integer",["minimum"]=1,["maximum"]=32 };
            }
            yield return new Tool { Name=write?"unlock_building":"inspect_research",
                Description=write?"Schaltet eine konkrete Gebäudevorlage regulär gegen Forschungspunkte frei. Aktuelle session und erwartete scienceCost als expectedCost erforderlich. Eigenes Mod-/MCP-Opt-in. applied bestätigt Freischaltung und exakten unmittelbaren Kostenabzug; already_unlocked verändert nichts. Ablehnungen ändern nichts. Bei Fehler/unconfirmed ausschließlich nachlesen, niemals automatisch wiederholen. Keine Bauplatzierung, keine künstlichen Punkte.":"Liest Forschungspunkte und seitenweise Gebäudevorlagen der aktiven Szene mit scienceCost, available, unlocked und regulärem unlockable. Alle Seiten lesen; keine atomare Gesamtsicht. Keine Liste einzelner Technologie-Voraussetzungen verfügbar. Ab Bridge 0.17.0.",
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject { ["type"]="object",["properties"]=props,["required"]=new JsonArray(props.Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()),["additionalProperties"]=false }),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(write?typeof(NativeResult<NativeUnlock>):typeof(NativeResult<NativeResearch>))),
                Annotations=new(){ReadOnlyHint=!write,DestructiveHint=write,IdempotentHint=!write,OpenWorldHint=false} };
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool enabled,CancellationToken ct)
    {
        bool write=name=="unlock_building";if(!Handles(name)||write&&!enabled)throw new ArgumentException();
        var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()) {
            if(p.Name is "template" or "session") { if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException();q.Add(p.Name,p.Value.GetString()); }
            else { if(p.Value.ValueKind!=JsonValueKind.Number||!p.Value.TryGetInt32(out int n))throw new ArgumentException();q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture)); }
        }
        var r=ResearchRequest.Parse(write,q);
        JsonObject Wrap<T>(BridgeEnvelope<T> e)=>(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        return write?Wrap(await client.UnlockBuilding(r,ct)):Wrap(await client.Research(r,ct));
    }
}
