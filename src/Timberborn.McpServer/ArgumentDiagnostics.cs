namespace Timberborn.McpServer;

// Fixed messages only. Never expose arbitrary exception text, paths or request values.
public static class ArgumentDiagnostics
{
    // Advisory diagnosis of backend arguments after rejection; not a replacement validator.
    // reasoning is validated by the activity boundary and stripped before backend dispatch.
    public static string? SchemaHint(System.Text.Json.JsonElement args, System.Text.Json.JsonElement schema)
    {
        if (args.ValueKind != System.Text.Json.JsonValueKind.Object) return "Argumente muessen ein Objekt sein.";
        if (!schema.TryGetProperty("properties", out var properties)) return null;
        if (schema.TryGetProperty("required", out var required))
            foreach (var field in required.EnumerateArray())
                if (field.GetString() is { } key && key != "reasoning" && !args.TryGetProperty(key, out _))
                    return $"Pflichtparameter {key} fehlt.";
        foreach (var field in properties.EnumerateObject()) {
            if (field.Name == "reasoning" || !args.TryGetProperty(field.Name, out var value)) continue;
            var rule = field.Value;
            if (!rule.TryGetProperty("type", out var type) || type.ValueKind != System.Text.Json.JsonValueKind.String) continue;
            var kind = type.GetString();
            bool valid = kind switch {
                "integer" => value.ValueKind == System.Text.Json.JsonValueKind.Number && value.TryGetInt64(out _),
                "string" => value.ValueKind == System.Text.Json.JsonValueKind.String,
                "boolean" => value.ValueKind is System.Text.Json.JsonValueKind.True or System.Text.Json.JsonValueKind.False,
                "array" => value.ValueKind == System.Text.Json.JsonValueKind.Array,
                "object" => value.ValueKind == System.Text.Json.JsonValueKind.Object,
                _ => true
            };
            if (!valid) return $"Parameter {field.Name}: Typ {kind} erforderlich.";
            if (kind == "integer" && value.TryGetInt64(out var number) &&
                rule.TryGetProperty("minimum", out var min) && rule.TryGetProperty("maximum", out var max) &&
                (number < min.GetInt64() || number > max.GetInt64()))
                return $"Parameter {field.Name}: gueltiger Bereich {min.GetInt64()}..{max.GetInt64()} (Ganzzahl).";
        }
        if (schema.TryGetProperty("additionalProperties", out var additional) && additional.ValueKind == System.Text.Json.JsonValueKind.False &&
            args.EnumerateObject().Any(p => !properties.TryGetProperty(p.Name, out _)))
            return "Unbekannter Parameter; nur Felder des Werkzeugschemas verwenden.";
        return null;
    }
    public static T Parse<T>(System.Text.Json.JsonElement args) {
        try {return BuildBatchState.Parse<T>(args);}
        catch(System.Text.Json.JsonException){throw new ArgumentException("invalid_argument_structure");}
    }
    public static string? Hint(Exception ex)=>ex is ArgumentException ? ex.Message switch {
        "canonical_uuid_required"=>"Kennungen muessen nichtleere UUIDs im kanonischen D-Format sein (kleingeschrieben, mit Bindestrichen).",
        "invalid_argument_structure"=>"Argumentstruktur passt nicht zum Werkzeugschema: Pflichtfelder, Typen und unbekannte Felder pruefen.",
        "planting_missing_rectangle_field"=>"Jedes Rechteck braucht x, y, z, width, height, resource und workBuildingId.",
        "invalid_cleared_group_spec"=>"Raeumgruppe: 1-8 Vorlagen, Flaeche 1-8x1-8, Stunden je 1-672 und zusammen maximal 672, Echtzeit 60-3600 Sekunden, Tempo 1/3/7, Suchbudget 1-32, Kandidaten 1-4.",
        "invalid_clearance_key"=>"clearanceKey muss der unveraenderte 64-stellige Hex-Schluessel der Vorschau sein.",
        "job_parameters_changed"=>"Vorhandene jobId gehoert zu anderen Parametern; zuerst den vorhandenen Auftrag lesen.",
        "invalid_detail"=>"detail muss compact oder full sein.",
        "another_call_in_progress_not_queued"=>"Ein anderer MCP-Aufruf laeuft noch; dieser Auftrag wurde nicht eingereiht.",
        "planting_session_must_be_uuid"=>"session muss eine kanonische UUID der aktuellen Spielsitzung sein.",
        "planting_jobId_must_be_uuid"=>"jobId muss eine nichtleere kanonische UUID sein.",
        "planting_workBuildingId_must_be_uuid"=>"workBuildingId muss eine kanonische UUID eines passenden Pflanzgebaeudes sein.",
        "planting_rectangles_1_to_8"=>"rectangles muss ein bis acht Rechtecke enthalten.",
        "planting_rectangle_bounds_8x8"=>"Jedes Rechteck braucht x/y/z innerhalb 0-4095 und width/height innerhalb 1-8; das Ende muss innerhalb 4096 liegen.",
        "planting_rectangles_overlap"=>"Rechtecke duerfen dieselbe Zelle auf derselben Hoehe nicht mehrfach enthalten.",
        "planting_invalid_resource"=>"resource muss ein gueltiger Vorlagenname aus inspect_area_types sein.",
        "planting_invalid_preview_key"=>"key muss der unveraenderte 64-stellige Hex-Schluessel der Pflanzvorschau sein.",
        "planting_job_parameters_changed"=>"jobId hat bereits einen anderen Pflanzauftrag; inspect_planting_batch lesen.",
        "planting_job_not_found"=>"In dieser Sitzung ist kein Beleg fuer diese jobId vorhanden; nicht automatisch erneut ausfuehren.",
        "planting_actions_disabled"=>"Der technische Flaechen-Aktionsschalter ist deaktiviert.",
        _=>null
    } : null;
}
