using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;
public static class BuildingSettingsTools
{
    private static readonly Dictionary<string,string> Routes=new() {
        ["inspect_building_settings"]="building-settings", ["set_building_paused"]="set-building-paused",
        ["set_storage_good"]="set-storage-good", ["set_storage_mode"]="set-storage-mode",
        ["set_farm_priority"]="set-farm-priority", ["set_farm_crop"]="set-farm-crop" };
    public static bool Handles(string name)=>Routes.ContainsKey(name);
    public static bool Writes(string name)=>Handles(name) && name!="inspect_building_settings";
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options=new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver=new DefaultJsonTypeInfoResolver() };
        foreach(var (name,route) in Routes) {
            bool write=Writes(name); if(write&&!enabled)continue;
            var props=new JsonObject {
                ["id"]=new JsonObject { ["type"]="string",["format"]="uuid" },
                ["session"]=new JsonObject { ["type"]="string",["format"]="uuid" } };
            if(write)foreach(var key in new[]{BuildingSettingsRequest.ValueKey(route),BuildingSettingsRequest.ExpectedKey(route)}) {
                bool pause=route=="set-building-paused";
                var p=new JsonObject { ["type"]=pause?"boolean":"string" };
                if(!pause)p["maxLength"]=160;
                if(route=="set-storage-mode")p["enum"]=new JsonArray("accept","empty","obtain","supply");
                if(route=="set-farm-priority")p["enum"]=new JsonArray("planting","harvesting");
                props[key]=p;
            }
            string description=name switch {
                "inspect_building_settings"=>"Liest Gebäudepause sowie bei fertigen Lagern/Farmen die Betriebsoptionen. Lager: ausgewähltes Gut (leer=keines), zulässige Güter, Modus, Kapazität und vorhandene Bestände. Farm: Pflanzen-/Erntenpriorität, bevorzugte Pflanze, erlaubte Pflanzen mit Unlockstatus. Fehlende Komponente/noch unfertig ausdrücklich unterscheiden. Anbauflächen separat über inspect_areas(kind=crops) lesen; keine feste Farm-Zuordnung behaupten.",
                "set_building_paused"=>"Pausiert oder aktiviert genau ein Gebäude über reguläres Pause/Resume. paused und expectedPaused sind boolesch; aktuelle Session nötig. IsPausable muss gelten. Betrifft auch geeignete Baustellen, nicht die globale Simulationsgeschwindigkeit. Fortsetzen garantiert keine Arbeiter oder Produktion.",
                "set_storage_good"=>"Wählt ein passendes Lagergut über die reguläre Lagerauswahl; good leer hebt die Auswahl auf. expectedGood ist die zuletzt gelesene Auswahl (leer=keine). Nur fertige, frei konfigurierbare Lager und Güter aus allowedGoods. Alte Bestände bleiben Spielzustand; kein Leeren durch Löschen und keine sofortige Umlagerung.",
                "set_storage_mode"=>"Setzt Lagermodus accept (Annehmen), empty (Leeren), obtain (Beschaffen) oder supply (Liefern) über die Spielmethoden. Fertiges Lager, zuletzt gelesener expectedMode und frische Session nötig. Modusänderung bewegt selbst keine Güter.",
                "set_farm_priority"=>"Setzt planting (Pflanzen zuerst) oder harvesting (Ernten zuerst) für ein fertiges Farmhaus. expectedPriority muss dem zuletzt gelesenen Wert entsprechen. Getrennt von Personal-/Baupriorität und Pflanzenwahl.",
                _=>"Wählt die bevorzugte Pflanze eines fertigen Farmhauses über PlantablePrioritizer. resource muss eine freigeschaltete allowedPlants-Vorlage sein; expectedResource ist die gelesene bisherige Vorlage (leer=keine). Rücksetzen auf keine Präferenz ist noch nicht freigegeben. Markiert keine Anbaufläche: dafür set_area(kind=crops, resource=Vorlage) nutzen. Kein erzwungener Job oder Ertrag." };
            if(write)description+=" Danach inspect_building_settings separat lesen. Bei Fehler/unconfirmed nicht automatisch wiederholen oder zurücksetzen.";
            yield return new Tool { Name=name,Description=description,
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject { ["type"]="object",["properties"]=props,
                    ["required"]=new JsonArray(props.Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()),["additionalProperties"]=false }),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(write?typeof(NativeResult<NativeSettingChange>):typeof(NativeResult<NativeBuildingSettings>))),
                Annotations=new() { ReadOnlyHint=!write,DestructiveHint=write,IdempotentHint=!write,OpenWorldHint=false } };
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client,string name,JsonElement args,bool enabled,CancellationToken ct)
    {
        bool write=Writes(name); if(write&&!enabled)throw new ArgumentException();
        var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()) {
            if(p.Name is "paused" or "expectedPaused") {
                if(p.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))throw new ArgumentException();
                q.Add(p.Name,p.Value.GetBoolean()?"true":"false");
            } else { if(p.Value.ValueKind!=JsonValueKind.String)throw new ArgumentException(); q.Add(p.Name,p.Value.GetString()); }
        }
        var r=BuildingSettingsRequest.Parse("/agent-api/v1/"+Routes[name],q);
        JsonObject Wrap<T>(BridgeEnvelope<T> e)=>(JsonObject)JsonSerializer.SerializeToNode(new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        return write?Wrap(await client.SetBuildingSetting(r,ct)):Wrap(await client.BuildingSettings(r,ct));
    }
}
