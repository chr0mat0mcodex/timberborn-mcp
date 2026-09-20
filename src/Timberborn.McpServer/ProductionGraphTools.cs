using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol.Protocol;
using Timberborn.Backend.Native;
namespace Timberborn.McpServer;

public static class ProductionGraphTools
{
    public static Tool Reader()
    {
        var options=new JsonSerializerOptions(NativeJson.Options){TypeInfoResolver=new DefaultJsonTypeInfoResolver()};
        return new(){Name="inspect_production_graph",Description="Liefert mit einem Aufruf den vollständigen kompakten Definitionsgraphen: Güter, Rezepte mit Mengen/Dauer/Brennstoff, zugeordnete Gebäude samt Bau-/Forschungskosten sowie natürliche Schnitt-/Sammelquellen mit Ertrag, Wachstumszeit und Ernte-/Pflanzgebäuden. Arrays und ID-Verweise bilden die Kanten: inputs -> Rezept -> outputs, buildings und harvesters/planters. Alternative Rezepte erhalten; keine einzige globale Reihenfolge erzwingen. completeWithinScope gilt nur für registrierte Definitionen und aktive Szenen-Gebäude, nicht alle Fraktionen oder sämtliche Betriebsbedingungen. coverage nennt Güter ohne bekannte Quelle und fehlende Gebäudezuordnungen. Baukosten sind keine Rezeptzutaten; nominale Zeit ist kein realer Durchsatz. Brennstoff eine Einheit je fuelCycles. Wasser-/Energie-Sonderbedingungen nicht vollständig; aktuelle Freischaltung, Bestände und Erreichbarkeit separat lesen. Einmal pro geladener Szene extrahiert; graphRevision prüft identische Definitionen. Rein lesend, keine Seiten oder stille Kürzung, keine Namen/Spiel-IDs. Ab Bridge 0.21.0.",
            InputSchema=JsonSerializer.SerializeToElement(new{type="object",properties=new{},additionalProperties=false}),
            OutputSchema=JsonSerializer.SerializeToElement(options.GetJsonSchemaAsNode(typeof(NativeResult<NativeProductionGraph>))),
            Annotations=new(){ReadOnlyHint=true,DestructiveHint=false,IdempotentHint=true,OpenWorldHint=false}};
    }
    public static async Task<JsonObject> Invoke(NativeClient client,JsonElement args,CancellationToken ct)
    {
        if(args.ValueKind!=JsonValueKind.Object||args.EnumerateObject().Any())throw new ArgumentException();
        var e=await client.ProductionGraph(ct);
        return JsonSerializer.SerializeToNode(new NativeResult<NativeProductionGraph>(1,"ok",e.Data,new("native",false,e.SessionId,e.ObservedAtUtc),null),NativeJson.Options)!.AsObject();
    }
}
