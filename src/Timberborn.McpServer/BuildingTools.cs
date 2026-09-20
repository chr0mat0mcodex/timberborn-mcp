using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;

namespace Timberborn.McpServer;

public static class BuildingTools
{
    private static readonly Dictionary<string,string> Routes = new() {
        ["inspect_build_options"]="building-catalog", ["precheck_building"]="building-precheck",
        ["validate_building"]="building-validation", ["place_building"]="building-placement" };
    public static bool Handles(string name) => Routes.ContainsKey(name);
    public static bool Writes(string name) => name is "validate_building" or "place_building";
    public static IEnumerable<Tool> Catalog(bool enabled)
    {
        var options = new JsonSerializerOptions(NativeJson.Options) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        foreach (var name in Routes.Keys.Where(n => enabled || !Writes(n)))
        {
            bool catalog = name == "inspect_build_options", write = Writes(name), place = name == "place_building";
            var props = new JsonObject();
            void Num(string key, int min, int max) => props[key] = new JsonObject { ["type"]="integer", ["minimum"]=min, ["maximum"]=max };
            if (catalog) { Num("offset",0,65535); Num("limit",1,32); }
            else {
                props["template"] = new JsonObject { ["type"]="string", ["minLength"]=1, ["maxLength"]=160, ["pattern"]="^[A-Za-z0-9._-]+$" };
                foreach (var k in new[] { "x","y","z" }) Num(k,0,4095);
                Num("rotation",0,3);
                if (write) props["session"] = new JsonObject { ["type"]="string", ["format"]="uuid" };
                if (place) props["actionId"] = new JsonObject { ["type"]="string", ["format"]="uuid" };
            }
            string description = catalog ? "Vollständiger seitenweiser Gebäudekatalog der aktiven Spiel-Template-Sammlungen: IDs, Baukosten, Freischaltung, Geometrie, supported und unsupportedReasons. Alle Seiten lesen. supported beschreibt den Baupfad, keinen Live-Nachweis oder funktionierenden Betrieb. Gebäudekonfiguration nach Bau separat."
                : place ? "Erteilt einen regulären Bauauftrag für eine unterstützte Katalogvorlage. Frische Spielvalidierung inklusive. x/y/z ist BlockObject-Ursprung, rotation 0/1/2/3=Cw0/90/180/270, ungespiegelt. Frische session und NEUE actionId (UUID) nötig; diese wird Entity-ID. Jede ID ist pro Sitzung nur einmal ausführbar, auch bei Fehlern. Maximal 256 Versuche/Sitzung. Keine automatische Wiederholung; bei Unsicherheit inspect_building mit actionId lesen. applied bedeutet platziert, finished bedeutet fertig; Materialien, Bauzeit und Betrieb bleiben Spielaufgabe. Keine Überschreibung vorhandener Objekte."
                : write ? "Prüft eine unterstützte Katalogvorlage über eigene Spielvorschau und beide Spielvalidatoren, ohne Bauauftrag. Frische session erforderlich. Maximal 256 generische Prüfungen (inklusive Platzierungen) und 64 gecachte Vorlagen je Sitzung. Keine Erreichbarkeits-/Liefergarantie. Fehler nicht automatisch wiederholen."
                : "Liest Hindernisse, Gelände, Eingang und Kosten für eine unterstützte Katalogvorlage. x/y/z ist BlockObject-Ursprung; rotation 0/1/2/3=Cw0/90/180/270, ungespiegelt. Keine vollständige Spielvalidierung oder Erreichbarkeitsgarantie. Sonderlayouts laut Katalog nicht unterstützt.";
            Type result = catalog ? typeof(NativeResult<NativeBuildingCatalog>) : place ? typeof(NativeResult<NativePlacement>)
                : write ? typeof(NativeResult<NativeValidation>) : typeof(NativeResult<NativeSite>);
            yield return new Tool { Name=name, Description=description,
                InputSchema=JsonSerializer.SerializeToElement(new JsonObject { ["type"]="object", ["properties"]=props,
                    ["required"]=new JsonArray(props.Select(p=>(JsonNode?)JsonValue.Create(p.Key)).ToArray()), ["additionalProperties"]=false }),
                OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(result)),
                Annotations=new() { ReadOnlyHint=!write, DestructiveHint=write, IdempotentHint=!write, OpenWorldHint=false } };
        }
    }
    public static async Task<JsonObject> Invoke(NativeClient client, string name, JsonElement args, bool enabled, CancellationToken ct)
    {
        if (Writes(name) && !enabled) throw new ArgumentException();
        var q = new NameValueCollection();
        foreach (var p in args.EnumerateObject()) {
            if (p.Name is "template" or "session" or "actionId") {
                if (p.Value.ValueKind != JsonValueKind.String) throw new ArgumentException();
                q.Add(p.Name,p.Value.GetString());
            } else {
                if (p.Value.ValueKind != JsonValueKind.Number || !p.Value.TryGetInt32(out int n)) throw new ArgumentException();
                q.Add(p.Name,n.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        var r = BridgeRequest.Parse("/agent-api/v1/"+Routes[name],q);
        JsonObject Wrap<T>(BridgeEnvelope<T> e) => (JsonObject)JsonSerializer.SerializeToNode(
            new NativeResult<T>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!;
        return name switch {
            "inspect_build_options" => Wrap(await client.BuildingCatalog(r,ct)),
            "precheck_building" => Wrap(await client.Precheck(r,ct)),
            "validate_building" => Wrap(await client.Validate(r,ct)),
            "place_building" => Wrap(await client.PlaceBuilding(r,ct)),
            _ => throw new ArgumentException() };
    }
}
