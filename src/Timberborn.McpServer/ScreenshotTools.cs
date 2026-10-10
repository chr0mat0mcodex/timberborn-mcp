using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
using Timberborn.Bridge.Core;
namespace Timberborn.McpServer;

public static class ScreenshotTools
{
    public const string Name="capture_screenshot";
    public static Tool Reader() => new() {
        Name=Name,
        Description="Explizites Spielbild inklusive UI aus Unity, keine Desktopaufnahme. Ab Bridge 0.36.0. Frische session; maxWidth 64–1600 (Standard 1280), maxHeight 64–900 (Standard 720). Seitenverhältnis bleibt, kein Hochskalieren. JPEG höchstens 768 KiB als MCP-Bildblock, Metadaten separat: Zeitpunkt, Größe, Kameraposition/Höhe, Blickrichtung, Ziel, Bildwinkel und Bildweite auf Zielebene in Unity-Welteinheiten (Y=Höhe). Keine Höhe über Gelände oder Bodenflächenabdeckung behauptet. Keine Kamera-/Spieländerung, kein Dateispeichern. Maximal eine Aufnahme alle zwei Sekunden. Nicht gerenderte/minimierte Ansicht kann screenshot_unavailable liefern; kein automatischer Retry. Sichtbarer UI-Text kann Spielnamen enthalten. Bild ergänzt strukturierte Daten.",
        InputSchema=JsonSerializer.SerializeToElement(new JsonObject { ["type"]="object",["properties"]=new JsonObject {
            ["session"]=new JsonObject { ["type"]="string",["format"]="uuid" },
            ["maxWidth"]=new JsonObject { ["type"]="integer",["minimum"]=64,["maximum"]=1600,["default"]=1280 },
            ["maxHeight"]=new JsonObject { ["type"]="integer",["minimum"]=64,["maximum"]=900,["default"]=720 } },
            ["required"]=new JsonArray("session"),["additionalProperties"]=false }),
        OutputSchema=JsonSerializer.SerializeToElement(new { type="object" }),
        Annotations=new() { ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false }
    };
    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct) {
        var q=new NameValueCollection();
        foreach(var p in args.EnumerateObject()) {
            if(p.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Number))throw new ArgumentException();
            q.Add(p.Name,p.Value.ToString());
        }
        var e=await client.Screenshot(ScreenshotRequest.Parse(q),ct);
        return JsonSerializer.SerializeToNode(new NativeResult<NativeScreenshot>(1,"ok",e.Data,
            new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!.AsObject();
    }
    public static CallToolResult ToToolResult(JsonObject result) {
        if(result["status"]?.GetValue<string>()!="ok")return ResponsePresentation.ToToolResult(result);
        var copy=result.DeepClone().AsObject();
        var data=copy["data"]!.AsObject();
        var bytes=Convert.FromBase64String(data["imageBase64"]!.GetValue<string>());
        data.Remove("imageBase64");
        var response=ResponsePresentation.ToToolResult(copy);
        response.Content.Add(ImageContentBlock.FromBytes(bytes,"image/jpeg"));
        return response;
    }
}
